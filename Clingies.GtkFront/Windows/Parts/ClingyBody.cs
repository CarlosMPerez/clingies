using System;
using Clingies.Domain.Models;
using Gtk;

namespace Clingies.GtkFront.Windows.Parts;

public sealed class ClingyBody : Overlay
{
    private TextView _content;
    private EventBox _leftGrip;
    private EventBox _rightGrip;
    private bool _isLocked;

    private bool _autoSizeEnabled = true;

    private ClingyBody() : base()
    {
        _content = new TextView
        {
            Name = AppConstants.CssSections.ClingyContentView,
            CursorVisible = true,
            Editable = true,
            CanFocus = true,
            WrapMode = Gtk.WrapMode.WordChar,
            LeftMargin = 6,
            RightMargin = 6
        };

        _leftGrip = new EventBox();
        _rightGrip = new EventBox();
    }

    public static ClingyBody Build(ClingyModel model,
                                    Gtk.Window owner,
                                    ClingyWindowCallbacks cb)
    {
        var overlay = new ClingyBody();
        var scroller = new ScrolledWindow
        {
            Name = AppConstants.CssSections.ClingyContent,
            ShadowType = ShadowType.None,
            HscrollbarPolicy = PolicyType.Never,
            VscrollbarPolicy = PolicyType.Automatic
        };


        // init + caret behavior
        overlay._content.Buffer.Text = model.Text ?? string.Empty;
        overlay._content.MapEvent += (_, __) => GLib.Idle.Add(() => { overlay._content.GrabFocus(); return false; });
        overlay._content.EnterNotifyEvent += (_, e) =>
        {
            try
            {
                var win = e.Event.Window ?? overlay._content.GetWindow(TextWindowType.Text);
                using var cursor = new Gdk.Cursor(owner.Display, Gdk.CursorType.Xterm);
                if (win != null) win.Cursor = cursor;
            }
            catch { }
        };

        overlay._content.LeaveNotifyEvent += (_, e) =>
        {
            try { (e.Event.Window ?? overlay._content.GetWindow(TextWindowType.Text))!.Cursor = null; } catch { }
        };

        // content changes bubble up
        overlay._content.Buffer.Changed += (_, e) =>
        {
            cb.ContentChanged(overlay._content.Buffer.Text);
        };

        // optional: auto-height (same logic, local to body)
        uint autosizeId = 0;
        overlay._content.SizeAllocated += (_, __) => DebounceAutosize();
        overlay._content.Buffer.Changed += (_, __) => DebounceAutosize();

        void DebounceAutosize()
        {
            if (!overlay._autoSizeEnabled) return;

            if (autosizeId != 0) GLib.Source.Remove(autosizeId);
            autosizeId = GLib.Timeout.Add(16, () =>
            {
                autosizeId = 0;
                AutoHeightToContent(owner, overlay._content);
                return false;
            });
        }

        scroller.Add(overlay._content);
        overlay.Add(scroller);

        // grips: 2 thin overlays that start resize drags, and on release report size+pos
        overlay._leftGrip = MakeGrip(owner, cb, true, () => overlay._isLocked);
        overlay._rightGrip = MakeGrip(owner, cb, false, () => overlay._isLocked);
        overlay.AddOverlay(overlay._leftGrip);
        overlay.AddOverlay(overlay._rightGrip);

        return overlay;
    }

    public void ApplyLock(bool isLocked, Gtk.Window owner)
    {
        _isLocked = isLocked;

        // 1) WM-level
        owner.Resizable = !isLocked;

        // 2) Custom grips
        _leftGrip.Sensitive = !isLocked;
        _rightGrip.Sensitive = !isLocked;

        _content.Editable = !isLocked;
        _content.CursorVisible = !isLocked;
        _content.CanFocus = !isLocked;

        if (isLocked)
        {
            _content.KeyPressEvent += BlockKey;
            _content.ButtonPressEvent += BlockMouse;
            _content.PasteClipboard += BlockClipboard;
            _content.CutClipboard += BlockClipboard;
            _content.Buffer.InsertText += BlockInsert;
            _content.Buffer.DeleteRange += BlockDelete;
        }
        else
        {
            _content.KeyPressEvent -= BlockKey;
            _content.ButtonPressEvent -= BlockMouse;
            _content.PasteClipboard -= BlockClipboard;
            _content.CutClipboard -= BlockClipboard;
            _content.Buffer.InsertText -= BlockInsert;
            _content.Buffer.DeleteRange -= BlockDelete;
        }

        // Optional: neutral cursor when locked
        if (isLocked)
        {
            _leftGrip.Window?.SetDeviceCursor(owner.Display.DefaultSeat.Pointer, null);
            _rightGrip.Window?.SetDeviceCursor(owner.Display.DefaultSeat.Pointer, null);
        }
    }

    public void ApplyRollState(Gtk.Window owner, Box parent, bool isRolled, int width)
    {
        if (isRolled)
        {
            SetAutoSizeEnabled(false);
            Hide();
            parent.SetChildPacking(this, expand: false, fill: false, padding: 0, PackType.Start);
            ClampToTitleBarHeight(owner, width);
        }
        else
        {
            SetAutoSizeEnabled(true);
            parent.SetChildPacking(this, expand: true, fill: true, padding: 0, PackType.Start);
            ShowAll();
            UnclampHeight(owner, width);
        }

        parent.QueueResize();
    }

    public void SetAutoSizeEnabled(bool enabled) => _autoSizeEnabled = enabled;

    private static EventBox MakeGrip(Gtk.Window owner, ClingyWindowCallbacks cb, bool isLeft, Func<bool> isLocked)
    {
        var grip = new EventBox
        {
            VisibleWindow = false,
            WidthRequest = 6,
            Halign = isLeft ? Align.Start : Align.End,
            Valign = Align.Fill
        };
        grip.Events |= Gdk.EventMask.ButtonPressMask |
                        Gdk.EventMask.ButtonReleaseMask |
                        Gdk.EventMask.EnterNotifyMask |
                        Gdk.EventMask.LeaveNotifyMask;

        grip.ButtonPressEvent += (_, e) =>
        {
            if (isLocked()) { e.RetVal = true; return; }
            if (e.Event.Button == 1)
                owner.BeginResizeDrag(isLeft ? Gdk.WindowEdge.West : Gdk.WindowEdge.East,
                                      (int)e.Event.Button, (int)e.Event.XRoot, (int)e.Event.YRoot, e.Event.Time);
        };
        grip.ButtonReleaseEvent += (_, e) =>
        {
            if (isLocked()) { e.RetVal = true; return; }
            // end-of-resize certainty
            cb.SizeChanged(owner.Allocation.Width, owner.Allocation.Height);
            owner.GetPosition(out var x, out var y);
            cb.PositionChanged(x, y);
        };
        grip.EnterNotifyEvent += (_, e) =>
        {
            if (isLocked()) { e.RetVal = true; return; }
            try
            {
                using var c = new Gdk.Cursor(owner.Display, isLeft ? Gdk.CursorType.LeftSide : Gdk.CursorType.RightSide);
                (e.Event.Window ?? grip.Window)!.Cursor = c;
            }
            catch { }
        };
        grip.LeaveNotifyEvent += (_, e) =>
        {
            try { (e.Event.Window ?? grip.Window)!.Cursor = null; } catch { }
        };

        return grip;
    }

    private static void AutoHeightToContent(Gtk.Window owner, TextView tv)
    {
        int w = tv.Allocation.Width;
        if (w <= 0) return;

        using var layout = tv.CreatePangoLayout(tv.Buffer.Text);
        layout.Wrap = Pango.WrapMode.WordChar;
        layout.Width = (int)(w * Pango.Scale.PangoScale);
        layout.GetPixelSize(out _, out int textH);

        const int MinH = AppConstants.Dimensions.DefaultClingyHeight, MaxH = 1500, Pad = 16;
        int targetH = Math.Max(MinH, Math.Min(MaxH, textH + Pad));
        int currentW = owner.Allocation.Width > 0 ? owner.Allocation.Width : AppConstants.Dimensions.DefaultClingyWidth;
        //Console.WriteLine($"W:{currentW} - H:{targetH}");
        owner.Resize(currentW, targetH);
    }

    private void ClampToTitleBarHeight(Gtk.Window owner, int width)
    {
        var geom = new Gdk.Geometry
        {
            MinHeight = AppConstants.Dimensions.TitleHeight,
            MaxHeight = AppConstants.Dimensions.TitleHeight,
            MinWidth = width,
            MaxWidth = width
        };
        owner.SetGeometryHints(this, geom, Gdk.WindowHints.MinSize | Gdk.WindowHints.MaxSize);
    }

    private void UnclampHeight(Gtk.Window owner, int width)
    {
        var geom = new Gdk.Geometry
        {
            MinHeight = AppConstants.Dimensions.TitleHeight,
            MaxHeight = int.MaxValue,
            MinWidth = width,
            MaxWidth = int.MaxValue
        };

        owner.SetGeometryHints(this, geom, Gdk.WindowHints.MinSize | Gdk.WindowHints.MaxSize);
    }

    private void BlockKey(object? o, Gtk.KeyPressEventArgs ev) => ev.RetVal = true;
    private void BlockMouse(object? o, Gtk.ButtonPressEventArgs ev) => ev.RetVal = true;
    private void BlockClipboard(object? o, EventArgs e) { }
    private void BlockInsert(object? o, Gtk.InsertTextArgs args) => args.RetVal = true;
    private void BlockDelete(object? o, Gtk.DeleteRangeArgs args) => args.RetVal = true;

}
