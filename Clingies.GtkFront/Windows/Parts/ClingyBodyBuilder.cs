using System;
using Clingies.Domain.Models;
using Clingies.GtkFront.Utils;
using Gtk;

namespace Clingies.GtkFront.Windows.Parts;

internal static class ClingyBodyBuilder
{
    public static Overlay Build(ClingyDto dto, Window owner, UtilsService utils, ClingyWindowCallbacks cb)
    {
        var overlay = new Overlay();

        var scroller = new ScrolledWindow
        {
            Name = "clingy-content",
            ShadowType = ShadowType.None,
            HscrollbarPolicy = PolicyType.Never,
            VscrollbarPolicy = PolicyType.Automatic
        };

        var view = new TextView
        {
            Name = "clingy-content-view",
            CursorVisible = true,
            Editable = true,
            CanFocus = true,
            WrapMode = Gtk.WrapMode.WordChar,
            LeftMargin = 6,
            RightMargin = 6
        };

        // init + caret behavior
        view.Buffer.Text = dto.Content ?? string.Empty;
        view.MapEvent += (_, __) => GLib.Idle.Add(() => { view.GrabFocus(); return false; });
        view.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.EnterNotifyMask | Gdk.EventMask.LeaveNotifyMask;

        view.ButtonPressEvent += (_, e) =>
        {
            view.GrabFocus();
            int bx, by;
            view.WindowToBufferCoords(TextWindowType.Text, (int)e.Event.X, (int)e.Event.Y, out bx, out by);
            view.GetIterAtLocation(out var iter, bx, by);
            view.Buffer.PlaceCursor(iter);
            if (e.Event.Button == 3) Console.WriteLine("BDR!");
        };

        view.EnterNotifyEvent += (_, e) =>
        {
            try
            {
                var win = e.Event.Window ?? view.GetWindow(TextWindowType.Text);
                using var cursor = new Gdk.Cursor(owner.Display, Gdk.CursorType.Xterm);
                if (win != null) win.Cursor = cursor;
            } catch { }
        };

        view.LeaveNotifyEvent += (_, e) =>
        {
            try { (e.Event.Window ?? view.GetWindow(TextWindowType.Text))!.Cursor = null; } catch { }
        };

        // content changes bubble up
        view.Buffer.Changed += (_, __) => cb.ContentChanged(view.Buffer.Text);

        // optional: auto-height (same logic, local to body)
        uint autosizeId = 0;
        view.SizeAllocated += (_, __) => DebounceAutosize();
        view.Buffer.Changed += (_, __) => DebounceAutosize();

        void DebounceAutosize()
        {
            if (autosizeId != 0) GLib.Source.Remove(autosizeId);
            autosizeId = GLib.Timeout.Add(16, () =>
            {
                autosizeId = 0;
                AutoHeightToContent(owner, view);
                return false;
            });
        }

        scroller.Add(view);
        overlay.Add(scroller);

        // grips: 2 thin overlays that start resize drags, and on release report size+pos
        overlay.AddOverlay(MakeGrip(owner, cb, isLeft: true));
        overlay.AddOverlay(MakeGrip(owner, cb, isLeft: false));

        return overlay;
    }

    private static Widget MakeGrip(Window owner, ClingyWindowCallbacks cb, bool isLeft)
    {
        var grip = new EventBox { VisibleWindow = false, WidthRequest = 6, Halign = isLeft ? Align.Start : Align.End, Valign = Align.Fill };
        grip.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask | Gdk.EventMask.EnterNotifyMask | Gdk.EventMask.LeaveNotifyMask;

        grip.ButtonPressEvent += (_, e) =>
        {
            if (e.Event.Button == 1)
                owner.BeginResizeDrag(isLeft ? Gdk.WindowEdge.West : Gdk.WindowEdge.East,
                                      (int)e.Event.Button, (int)e.Event.XRoot, (int)e.Event.YRoot, e.Event.Time);
        };
        grip.ButtonReleaseEvent += (_, __) =>
        {
            // end-of-resize certainty
            cb.SizeChanged(owner.Allocation.Width, owner.Allocation.Height);
            owner.GetPosition(out var x, out var y);
            cb.PositionChanged(x, y);
        };
        grip.EnterNotifyEvent += (_, e) =>
        {
            try
            {
                using var c = new Gdk.Cursor(owner.Display, isLeft ? Gdk.CursorType.LeftSide : Gdk.CursorType.RightSide);
                (e.Event.Window ?? grip.Window)!.Cursor = c;
            } catch { }
        };
        grip.LeaveNotifyEvent += (_, e) =>
        {
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

        const int MinH = 100, MaxH = 1500, Pad = 16;
        int targetH = Math.Max(MinH, Math.Min(MaxH, textH + Pad));
        int currentW = owner.Allocation.Width > 0 ? owner.Allocation.Width : 300;
        owner.Resize(currentW, targetH);
    }
}
