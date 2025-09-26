using Clingies.Domain.Models;
using Clingies.GtkFront.Services;
using Gtk;
using Pango;

namespace Clingies.GtkFront.Windows.Parts;

public sealed class ClingyTitleBar : Box
{
    private readonly Label _titleLabel;
    private readonly EventBox _dragArea;
    private readonly Box _leftSide;
    private readonly Box _rightSide;

    private Button _pinButton;
    private Button _lockButton;
    private Button _closeButton;

    private bool _isPinned;
    private bool _isLocked;

    // Keep ctor non-public to enforce factory usage
    private ClingyTitleBar() : base(Orientation.Horizontal, 4)
    {
        Name = AppConstants.CssSections.ClingyTitle;
        HeightRequest = AppConstants.Dimensions.TitleHeight;

        _leftSide = new Box(Orientation.Horizontal, 0) { Halign = Align.Start, Valign = Align.Center };
        _dragArea = new EventBox { VisibleWindow = false, AboveChild = false };
        _rightSide = new Box(Orientation.Horizontal, 0) { Halign = Align.End, Valign = Align.Center };

        _pinButton = new Button();
        _lockButton = new Button();
        _closeButton = new Button();

        _titleLabel = new Label
        {
            Name = AppConstants.CssSections.ClingyTitleLabel,
            Xalign = 0.5f,
            Yalign = 0.5f,
            Halign = Align.Fill,
            Valign = Align.Center,
            Ellipsize = EllipsizeMode.End
        };
        _dragArea.Add(_titleLabel);

        PackStart(_leftSide, false, false, 0);
        PackStart(_dragArea, true, true, 0);
        PackEnd(_rightSide, false, false, 0);
    }

    public static ClingyTitleBar Build(ClingyModel model, Window owner,
        GtkUtilsService utils, ClingyWindowCallbacks callbacks)
    {
        var bar = new ClingyTitleBar();

        // initialize state & title
        bar._isPinned = model.IsPinned;
        bar._isLocked = model.IsLocked;
        bar._titleLabel.Text = model.Title ?? string.Empty;

        // left: pin, lock ----------------------------------------------------
        bar._pinButton = utils.MakeImgButton(
            AppConstants.CssSections.ButtonPin,
            bar._isPinned ? AppConstants.IconNames.ClingyPinned : AppConstants.IconNames.ClingyUnpinned
        );
        bar._pinButton.MarginStart = 2; bar._pinButton.MarginEnd = 4;
        bar._pinButton.Clicked += (_, __) => bar.InternalTogglePin(callbacks, utils);
        bar._leftSide.PackStart(bar._pinButton, false, false, 0);

        bar._lockButton = utils.MakeImgButton(
            AppConstants.CssSections.ButtonLock,
            AppConstants.IconNames.ClingyLocked,
            (_, __) => { bar._isLocked = !bar._isLocked; }
        );
        bar._lockButton.MarginEnd = 4;
        bar._lockButton.NoShowAll = true;
        bar._lockButton.Visible = bar._isLocked;
        bar._leftSide.PackStart(bar._lockButton, false, false, 0);

        // drag area events ---------------------------------------------------
        bar._dragArea.Events |= Gdk.EventMask.ButtonPressMask |
                                Gdk.EventMask.ButtonReleaseMask |
                                Gdk.EventMask.PointerMotionMask;
        bar._dragArea.ButtonPressEvent += (_, e) =>
        {
            // do NOT check for islocked for all actions of mouse or right click menu will be blocked
            // ask individually
            if (e.Event.Button == 1 && !model.IsLocked && e.Event.Type == Gdk.EventType.TwoButtonPress)
                callbacks.RollChanged(!model.IsRolled);

            if (e.Event.Button == 1 && !model.IsLocked && e.Event.Type == Gdk.EventType.ButtonPress)
                owner.BeginMoveDrag((int)e.Event.Button, (int)e.Event.XRoot, (int)e.Event.YRoot, e.Event.Time);
        };
        bar._dragArea.ButtonReleaseEvent += (_, __) =>
        {
            owner.GetPosition(out var x, out var y);
            callbacks.PositionChanged(x, y);
        };

        // right: close -------------------------------------------------------
        bar._closeButton = utils.MakeImgButton(
            AppConstants.CssSections.ButtonClose,
            AppConstants.IconNames.ClingyClose,
            (_, __) => callbacks.CloseRequested()
        );
        bar._closeButton.MarginStart = 4; bar._closeButton.MarginEnd = 2;
        bar._rightSide.PackEnd(bar._closeButton, false, false, 0);

        bar.ShowAll();
        return bar;
    }

    // Internal Pin/Unpin command (by button clicking)
    // can ONLY be called by command OnClick
    private void InternalTogglePin(ClingyWindowCallbacks callbacks, GtkUtilsService utils)
    {
        if (_isLocked) return;
        _isPinned = !_isPinned;
        var assetName = _isPinned ? AppConstants.IconNames.ClingyPinned : AppConstants.IconNames.ClingyUnpinned;
        utils.SetButtonIcon(_pinButton, assetName);
        // notify the window manager that pin has changed
        callbacks.PinChanged(_isPinned);
    }

    /// <summary>
    /// for when pin ios called EXTERNALLY, from menu for example
    /// </summary>
    /// <param name="isPinned"></param>
    private void TogglePin(bool isPinned)
    {
        _isPinned = isPinned;
        // SOLVE THIS
        //var assetName = _isPinned ? AppConstants.IconNames.ClingyPinned : AppConstants.IconNames.ClingyUnpinned;
        //_utils.SetButtonIcon(_pinButton, assetName);
        _pinButton.Show();
    }

    private void ToggleLock(bool isLocked)
    {
        _isLocked = isLocked;
        _lockButton.Visible = _isLocked;
        if (isLocked) _lockButton.Show();
        else _lockButton.Hide();
    }

    // This calls come from WindowsManager and CANNOT return to it
    public void ChangeTitle(string newTitle) => _titleLabel.Text = newTitle ?? string.Empty;

    public void SetPinIcon(bool isPinned) => TogglePin(isPinned);

    public void SetLockIcon(bool isLocked) => ToggleLock(isLocked);
}
