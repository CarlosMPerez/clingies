using System;
using Clingies.Domain.DTOs;
using Clingies.GtkFront.Services;
using Gtk;

namespace Clingies.GtkFront.Windows.Parts;

public sealed class ClingyBody : Overlay
{
    private bool _isLocked;
    private TextView _view;

    private ClingyBody() : base()
    {
    }

    public static ClingyBody Build(ClingyDto dto, Window owner, GtkUtilsService utils, ClingyWindowCallbacks cb)
    {
        var overlay = new ClingyBody();

        var scroller = new ScrolledWindow
        {
            Name = AppConstants.CssSections.ClingyContent,
            ShadowType = ShadowType.None,
            HscrollbarPolicy = PolicyType.Never,
            VscrollbarPolicy = PolicyType.Automatic
        };

        overlay._view = new TextView
        {
            Name = AppConstants.CssSections.ClingyContentView,
            CursorVisible = true,
            Editable = true,
            CanFocus = true,
            WrapMode = Gtk.WrapMode.WordChar,
            LeftMargin = 6,
            RightMargin = 6
        };

        // init + caret behavior
        overlay._view.Buffer.Text = dto.Text ?? string.Empty;
        overlay._view.MapEvent += (_, __) => GLib.Idle.Add(() => { overlay._view.GrabFocus(); return false; });
        overlay._view.EnterNotifyEvent += (_, e) =>
        {
            if(overlay._isLocked) { e.RetVal = true;  return; }
            try
            {
                var win = e.Event.Window ?? overlay._view.GetWindow(TextWindowType.Text);
                using var cursor = new Gdk.Cursor(owner.Display, Gdk.CursorType.Xterm);
                if (win != null) win.Cursor = cursor;
            }
            catch { }
        };

        overlay._view.LeaveNotifyEvent += (_, e) =>
        {
            if(overlay._isLocked) { e.RetVal = true;  return; }
            try { (e.Event.Window ?? overlay._view.GetWindow(TextWindowType.Text))!.Cursor = null; } catch { }
        };

        // content changes bubble up
        overlay._view.Buffer.Changed += (_, e) =>
        {
            if(overlay._isLocked) { return; }
            cb.ContentChanged(overlay._view.Buffer.Text);
        };

        // optional: auto-height (same logic, local to body)
            uint autosizeId = 0;
        overlay._view.SizeAllocated += (_, __) => DebounceAutosize();
        overlay._view.Buffer.Changed += (_, __) => DebounceAutosize();

        void DebounceAutosize()
        {
            if (overlay._isLocked) { return; }
            if (autosizeId != 0) GLib.Source.Remove(autosizeId);
            autosizeId = GLib.Timeout.Add(16, () =>
            {
                autosizeId = 0;
                AutoHeightToContent(owner, overlay._view);
                return false;
            });
        }

        scroller.Add(overlay._view);
        overlay.Add(scroller);

        // grips: 2 thin overlays that start resize drags, and on release report size+pos
        overlay.AddOverlay(MakeGrip(owner, cb, isLeft: true, isLocked:overlay. _isLocked));
        overlay.AddOverlay(MakeGrip(owner, cb, isLeft: false, isLocked: overlay._isLocked));

        return overlay;
    }

    private static Widget MakeGrip(Window owner, ClingyWindowCallbacks cb, bool isLeft, bool isLocked)
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
            if(isLocked) { e.RetVal = true;  return; }
            if (e.Event.Button == 1)
                owner.BeginResizeDrag(isLeft ? Gdk.WindowEdge.West : Gdk.WindowEdge.East,
                                      (int)e.Event.Button, (int)e.Event.XRoot, (int)e.Event.YRoot, e.Event.Time);
        };
        grip.ButtonReleaseEvent += (_, e) =>
        {
            if(isLocked) { e.RetVal = true;  return; }
            // end-of-resize certainty
            cb.SizeChanged(owner.Allocation.Width, owner.Allocation.Height);
            owner.GetPosition(out var x, out var y);
            cb.PositionChanged(x, y);
        };
        grip.EnterNotifyEvent += (_, e) =>
        {
            if(isLocked) { e.RetVal = true;  return; }
            try
            {
                using var c = new Gdk.Cursor(owner.Display, isLeft ? Gdk.CursorType.LeftSide : Gdk.CursorType.RightSide);
                (e.Event.Window ?? grip.Window)!.Cursor = c;
            }
            catch { }
        };
        grip.LeaveNotifyEvent += (_, e) =>
        {
            if(isLocked) { e.RetVal = true;  return; }
            try { (e.Event.Window ?? grip.Window)!.Cursor = null; } catch { }
        };

        return grip;
    }

    private static void AutoHeightToContent(Window owner, TextView tv)
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
        owner.Resize(currentW, targetH);
    }

    public void SetLocked(bool isLocked)
    {
        _isLocked = isLocked;
        _view.Editable = !isLocked;
        _view.CursorVisible = !isLocked;
        _view.CanFocus = !isLocked;
    } 
}
