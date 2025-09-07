using Clingies.Domain.Models;
using Clingies.GtkFront.Services;
using Clingies.GtkFront.Utils;
using Gtk;
using Pango;

namespace Clingies.GtkFront.Windows.Parts;

public sealed class ClingyTitleBar : Box
{
    private readonly Label _titleLabel;
    private readonly EventBox _dragArea;
    private readonly Box _leftSide;
    private readonly Box _rightSide;

    private bool _isPinned;
    private bool _isLocked;

    // Keep ctor non-public to enforce factory usage
    private ClingyTitleBar() : base(Orientation.Horizontal, 4)
    {
        Name = AppConstants.CssSections.ClingyTitle;
        HeightRequest = 22;

        _leftSide  = new Box(Orientation.Horizontal, 0) { Halign = Align.Start, Valign = Align.Center };
        _dragArea  = new EventBox { VisibleWindow = false, AboveChild = false };
        _rightSide = new Box(Orientation.Horizontal, 0) { Halign = Align.End,   Valign = Align.Center };

        _titleLabel = new Label
        {
            Name = AppConstants.CssSections.ClingyTitleLabel,
            Xalign = 0.5f, Yalign = 0.5f,
            Halign = Align.Fill, Valign = Align.Center,
            Ellipsize = EllipsizeMode.End
        };
        _dragArea.Add(_titleLabel);

        PackStart(_leftSide,  false, false, 0);
        PackStart(_dragArea,  true,  true,  0);
        PackEnd  (_rightSide, false, false, 0);
    }

    public static ClingyTitleBar Build(ClingyDto dto, Window owner,
        UtilsService utils, ClingyWindowCallbacks callbacks)
    {
        var bar = new ClingyTitleBar();

        // initialize state & title
        bar._isPinned = dto.IsPinned;
        bar._isLocked = dto.IsLocked;
        bar._titleLabel.Text = dto.Title ?? string.Empty;

        // left: pin, lock ----------------------------------------------------
        var pinBtn = utils.MakeImgButton(
            AppConstants.CssSections.ButtonPin,
            bar._isPinned ? AppConstants.IconNames.ClingyPinned : AppConstants.IconNames.ClingyUnpinned
        );
        pinBtn.MarginStart = 2; pinBtn.MarginEnd = 4;
        pinBtn.Clicked += (_, __) => bar.TogglePin(pinBtn, callbacks, utils);
        bar._leftSide.PackStart(pinBtn, false, false, 0);

        var lockBtn = utils.MakeImgButton(
            AppConstants.CssSections.ButtonLock,
            AppConstants.IconNames.ClingyLocked,
            (_, __) => { bar._isLocked = !bar._isLocked; callbacks.LockChanged(bar._isLocked); }
        );
        lockBtn.MarginEnd = 4;
        lockBtn.NoShowAll = true;
        lockBtn.Visible = bar._isLocked;
        bar._leftSide.PackStart(lockBtn, false, false, 0);

        // drag area events ---------------------------------------------------
        bar._dragArea.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask;
        bar._dragArea.ButtonPressEvent += (_, e) =>
        {
            if (e.Event.Button == 1)
                owner.BeginMoveDrag((int)e.Event.Button, (int)e.Event.XRoot, (int)e.Event.YRoot, e.Event.Time);
        };
        bar._dragArea.ButtonReleaseEvent += (_, __) =>
        {
            owner.GetPosition(out var x, out var y);
            callbacks.PositionChanged(x, y);
        };

        // right: close -------------------------------------------------------
        var closeBtn = utils.MakeImgButton(
            AppConstants.CssSections.ButtonClose,
            AppConstants.IconNames.ClingyClose,
            (_, __) => callbacks.CloseRequested()
        );
        closeBtn.MarginStart = 4; closeBtn.MarginEnd = 2;
        bar._rightSide.PackEnd(closeBtn, false, false, 0);

        bar.ShowAll();
        return bar;
    }

    private void TogglePin(Button pinBtn, ClingyWindowCallbacks callbacks, UtilsService utils)
    {
        _isPinned = !_isPinned;
        var assetName = _isPinned ? AppConstants.IconNames.ClingyPinned : AppConstants.IconNames.ClingyUnpinned;
        utils.SetButtonIcon(pinBtn, assetName);
        callbacks.PinChanged(_isPinned);
    }

    public void ChangeTitle(string newTitle) => _titleLabel.Text = newTitle ?? string.Empty;
}
