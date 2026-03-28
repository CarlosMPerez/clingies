using System;
using Clingies.Domain.Models;
using Gtk;

namespace Clingies.Windows.Parts;

public sealed class ClingyBody : Overlay
{
    private TextView _content;
    private ScrolledWindow _scroller;
    private EventBox _leftGrip;
    private EventBox _rightGrip;
    private bool _isLocked;

    private bool _autoSizeEnabled = true;
    private uint _autosizeId;
    private int _lastAutoHeight = -1;
    private bool _scrollNormalizationPending;
    private uint _scrollNormalizationId;
    private bool _contentFitsWithoutScrolling = true;
    private bool _pasteScrollRecoveryPending;

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

        _scroller = new ScrolledWindow();
        _leftGrip = new EventBox();
        _rightGrip = new EventBox();
    }

    public static ClingyBody Build(ClingyModel model,
                                    Gtk.Window owner,
                                    ClingyWindowCallbacks cb)
    {
        var overlay = new ClingyBody();
        overlay._scroller = new ScrolledWindow
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
            overlay.DebounceAutosize(owner);
        };
        overlay._content.PasteClipboard += (_, __) => overlay.HandlePaste(owner);

        overlay._content.SizeAllocated += (_, __) =>
        {
            overlay.DebounceAutosize(owner);
            overlay.FlushPendingScrollNormalization();
        };
        overlay._scroller.SizeAllocated += (_, __) => overlay.FlushPendingScrollNormalization();
        overlay.MapEvent += (_, __) => overlay.DebounceAutosize(owner);

        overlay._scroller.Add(overlay._content);
        overlay.Add(overlay._scroller);

        // grips: 2 thin overlays that start resize drags, and on release report size+pos
        overlay._leftGrip = MakeGrip(owner, cb, true, () => overlay._isLocked, () => overlay.DebounceAutosize(owner));
        overlay._rightGrip = MakeGrip(owner, cb, false, () => overlay._isLocked, () => overlay.DebounceAutosize(owner));
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
            UnclampHeight(owner);
            DebounceAutosize(owner);
        }

        parent.QueueResize();
    }

    public void SetAutoSizeEnabled(bool enabled)
    {
        _autoSizeEnabled = enabled;

        if (!enabled && _autosizeId != 0)
        {
            GLib.Source.Remove(_autosizeId);
            _autosizeId = 0;
        }
    }

    private static EventBox MakeGrip(Window owner, ClingyWindowCallbacks cb, bool isLeft, Func<bool> isLocked,
        System.Action autosizeAfterResize)
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
            autosizeAfterResize();
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

    private void DebounceAutosize(Gtk.Window owner)
    {
        if (!_autoSizeEnabled) return;

        if (_autosizeId != 0) GLib.Source.Remove(_autosizeId);
        _autosizeId = GLib.Timeout.Add(16, () =>
        {
            _autosizeId = 0;
            AutoHeightToContent(owner);
            return false;
        });
    }

    private void AutoHeightToContent(Gtk.Window owner)
    {
        int availableTextWidth = _content.Allocation.Width - _content.LeftMargin - _content.RightMargin;
        if (availableTextWidth <= 0) return;

        var text = string.IsNullOrEmpty(_content.Buffer.Text) ? " " : _content.Buffer.Text;
        using var layout = _content.CreatePangoLayout(text);
        layout.Wrap = Pango.WrapMode.WordChar;
        layout.Width = (int)(availableTextWidth * Pango.Scale.PangoScale);
        layout.GetPixelSize(out _, out int textH);

        const int MinWindowHeight = AppConstants.Dimensions.DefaultClingyHeight;
        const int MaxWindowHeight = 1500;
        const int TextVerticalPad = 16;

        int chromeHeight = owner.Allocation.Height > 0 && _content.Allocation.Height > 0
            ? Math.Max(AppConstants.Dimensions.TitleHeight, owner.Allocation.Height - _content.Allocation.Height)
            : AppConstants.Dimensions.TitleHeight;

        int desiredWindowHeight = chromeHeight + textH + TextVerticalPad;
        _contentFitsWithoutScrolling = desiredWindowHeight <= MaxWindowHeight;
        int targetH = Math.Max(MinWindowHeight, Math.Min(MaxWindowHeight, desiredWindowHeight));
        int currentW = owner.Allocation.Width > 0 ? owner.Allocation.Width : AppConstants.Dimensions.DefaultClingyWidth;

        if (_lastAutoHeight == targetH && owner.Allocation.Height == targetH)
        {
            QueueScrollNormalization();
            return;
        }

        if (Math.Abs(owner.Allocation.Height - targetH) <= 1)
        {
            _lastAutoHeight = targetH;
            QueueScrollNormalization();
            return;
        }

        _lastAutoHeight = targetH;
        _scrollNormalizationPending = true;
        ResizeKeepingPosition(owner, currentW, targetH);
    }

    private void QueueScrollNormalization()
    {
        if (_scrollNormalizationId != 0)
            GLib.Source.Remove(_scrollNormalizationId);

        GLib.Idle.Add(() =>
        {
            NormalizeScrollPosition();
            return false;
        });

        // Gtk may update adjustment/page size one or two frames after the resize.
        // Retry shortly after the idle pass so a first Ctrl+V lands in the final settled state.
        _scrollNormalizationId = GLib.Timeout.Add(48, () =>
        {
            NormalizeScrollPosition();
            _scrollNormalizationId = GLib.Timeout.Add(120, () =>
            {
                _scrollNormalizationId = 0;
                NormalizeScrollPosition();
                _pasteScrollRecoveryPending = false;
                return false;
            });
            return false;
        });
    }

    private void NormalizeScrollPosition()
    {
        var vadj = _scroller.Vadjustment;
        if (vadj == null) return;

        if (_pasteScrollRecoveryPending || _contentFitsWithoutScrolling)
        {
            if (Math.Abs(vadj.Value - vadj.Lower) > 1)
                vadj.Value = vadj.Lower;
            return;
        }

        var insertMark = _content.Buffer.InsertMark;
        _content.ScrollMarkOnscreen(insertMark);
    }

    private void FlushPendingScrollNormalization()
    {
        if (!_scrollNormalizationPending) return;

        _scrollNormalizationPending = false;
        QueueScrollNormalization();
    }

    private void HandlePaste(Gtk.Window owner)
    {
        _pasteScrollRecoveryPending = true;
        DebounceAutosize(owner);
        QueueScrollNormalization();
    }

    private static void ResizeKeepingPosition(Gtk.Window owner, int width, int height)
    {
        owner.GetPosition(out var x, out var y);
        owner.Resize(width, height);

        GLib.Idle.Add(() =>
        {
            owner.Move(x, y);
            return false;
        });

        GLib.Timeout.Add(48, () =>
        {
            owner.Move(x, y);
            return false;
        });
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

    private void UnclampHeight(Gtk.Window owner)
    {
        var geom = new Gdk.Geometry
        {
            MinHeight = AppConstants.Dimensions.TitleHeight,
            MaxHeight = int.MaxValue,
            MinWidth = AppConstants.Dimensions.MinimumClingyWidth,
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
