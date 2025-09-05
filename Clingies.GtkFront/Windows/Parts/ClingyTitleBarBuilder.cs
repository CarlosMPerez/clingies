using Clingies.Domain.Models;
using Clingies.GtkFront.Services;
using Clingies.GtkFront.Utils;
using Gtk;

namespace Clingies.GtkFront.Windows.Parts;

internal static class ClingyTitleBarBuilder
{
    public static Widget Build(ClingyDto dto, Window owner,
            UtilsService utils, ClingyWindowCallbacks callbacks)
    {
        // Inner layout (no-window)
        var row = new Box(Orientation.Horizontal, 0)
        {
            Name = AppConstants.CssSections.ClingyTitle, 
            HeightRequest = 22
        };

        // Left ---------------------------------------------------------------------------------
        var leftSide = new Box(Orientation.Horizontal, 0) { Halign = Align.Start, Valign = Align.Center };

        var pinBtn = utils.MakeImgButton(AppConstants.CssSections.ButtonPin, dto.IsPinned ?
                AppConstants.IconNames.ClingyPinned :
                AppConstants.IconNames.ClingyUnpinned);
        pinBtn.MarginStart = 2; pinBtn.MarginEnd = 4;
        pinBtn.Clicked += (_, __) => TogglePin(pinBtn, !dto.IsPinned, callbacks, utils);

        var lockBtn = utils.MakeImgButton(AppConstants.CssSections.ButtonLock, AppConstants.IconNames.ClingyLocked, (_, __) =>
                                    callbacks.LockChanged(!dto.IsLocked));
        lockBtn.MarginEnd = 4; lockBtn.NoShowAll = true; lockBtn.Visible = dto.IsLocked;
        leftSide.PackStart(pinBtn, false, false, 0);
        leftSide.PackStart(lockBtn, false, false, 0);

        // Center title and drag area -----------------------------------------------------------
        var titleLabel = new Label(dto.Title ?? string.Empty)
        {
            Name = AppConstants.CssSections.ClingyTitleLabel,
            Xalign = 0.5f, Yalign = 0.5f,
            Halign = Align.Fill, Valign = Align.Center,
            Ellipsize = Pango.EllipsizeMode.End
        };

        var dragArea = new EventBox { VisibleWindow = false, AboveChild = false };
        dragArea.Add(titleLabel);

        dragArea.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask;
        dragArea.ButtonPressEvent += (_, e) =>
        {
            if (e.Event.Button == 1)
                owner.BeginMoveDrag((int)e.Event.Button, (int)e.Event.XRoot, (int)e.Event.YRoot, e.Event.Time);
        };
        dragArea.ButtonReleaseEvent += (_, __) =>
        {
            owner.GetPosition(out var x, out var y);
            callbacks.PositionChanged(x, y);
        };

        // Right ---------------------------------------------------------------------------------
        var rightSide = new Box(Orientation.Horizontal, 0) { Halign = Align.End, Valign = Align.Center };
        var closeBtn = utils.MakeImgButton(AppConstants.CssSections.ButtonClose,
                    AppConstants.IconNames.ClingyClose, (_, __) =>
                    callbacks.CloseRequested());
        closeBtn.MarginStart = 4; closeBtn.MarginEnd = 2;
        rightSide.PackEnd(closeBtn, false, false, 0);

        // Pack
        row.PackStart(leftSide, false, false, 0);
        row.PackStart(dragArea, true, true, 0);
        row.PackEnd(rightSide, false, false, 0);

        return row;
    }

    private static void TogglePin(Button pinBtn, bool isPinned, ClingyWindowCallbacks callbacks, UtilsService utils)
    {
        var assetName = isPinned ?
            AppConstants.IconNames.ClingyPinned :
            AppConstants.IconNames.ClingyUnpinned;
        utils.SetButtonIcon(pinBtn, assetName);
        callbacks.PinChanged(isPinned);
    }
}
