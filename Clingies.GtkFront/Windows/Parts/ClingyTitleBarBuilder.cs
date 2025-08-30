using System;
using Clingies.Domain.Models;
using Clingies.GtkFront.Utils;
using Gtk;

namespace Clingies.GtkFront.Windows.Parts;

internal static class ClingyTitleBarBuilder
{
    public static Widget Build(ClingyDto dto, Window owner, UtilsService utils, ClingyWindowCallbacks cb)
    {
        // Inner layout (no-window)
        var row = new Box(Orientation.Horizontal, 0)
        {
            Name = "clingy-title",
            HeightRequest = 22
        };

        // Left
        var left = new Box(Orientation.Horizontal, 0) { Halign = Align.Start, Valign = Align.Center };
        var pinBtn = MakeImgButton(utils, "btn-pin", "clingy_unpinned.png", (_,__) => cb.PinChanged(true/*toggle in manager*/));
        pinBtn.MarginStart = 2; pinBtn.MarginEnd = 4;
        var lockBtn = MakeImgButton(utils, "btn-lock", "clingy_locked.png", (_,__) => cb.LockChanged(true/*toggle in manager*/));
        lockBtn.MarginEnd = 4; lockBtn.NoShowAll = true; lockBtn.Visible = dto.IsLocked;
        left.PackStart(pinBtn, false, false, 0);
        left.PackStart(lockBtn, false, false, 0);

        // Center title
        var titleLabel = new Label(dto.Title ?? string.Empty)
        {
            Name = "clingy-title-label",
            Xalign = 0.5f, Yalign = 0.5f,
            Halign = Align.Fill, Valign = Align.Center
        };

        // Right
        var right = new Box(Orientation.Horizontal, 0) { Halign = Align.End, Valign = Align.Center };
        var closeBtn = MakeImgButton(utils, "btn-close", "clingy_close.png", (_,__) => cb.CloseRequested());
        closeBtn.MarginStart = 4; closeBtn.MarginEnd = 2;
        right.PackEnd(closeBtn, false, false, 0);

        // Pack
        row.PackStart(left,  false, false, 0);
        row.PackStart(titleLabel, true,  true,  0);
        row.PackEnd  (right, false, false, 0);

        // EventBox wrapper (has GdkWindow) to capture press/release
        var hit = new EventBox { VisibleWindow = false, AboveChild = true };
        hit.Add(row);

        hit.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask;
        hit.ButtonPressEvent += (_, e) =>
        {
            if (e.Event.Button == 1)
                owner.BeginMoveDrag((int)e.Event.Button, (int)e.Event.XRoot, (int)e.Event.YRoot, e.Event.Time);
        };
        hit.ButtonReleaseEvent += (_, __) =>
        {
            // Wayland may not give true coords; manager decides what to persist
            owner.GetPosition(out var x, out var y);
            cb.PositionChanged(x, y);
        };

        return hit;
    }

    private static Button MakeImgButton(UtilsService utils, string name, string asset, EventHandler onClick)
    {
        string path = System.IO.Path.Combine(AppContext.BaseDirectory, $"Assets/{asset}");
        var btn = new Button
        {
            Name = name,
            Relief = ReliefStyle.None,
            CanFocus = false,
            Image = utils.CreateImageFromPath(path, 12)
        };
        btn.SetSizeRequest(16, 16);
        btn.Clicked += onClick;
        return btn;
    }
}
