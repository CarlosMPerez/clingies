using System;
using Clingies.Application.CustomEventArgs;
using Clingies.Application.Interfaces;
using Clingies.Domain.Models;
using Clingies.GtkFront.Services;
using Clingies.GtkFront.Windows.Parts;
using Gtk;

namespace Clingies.GtkFront.Windows
{
    public sealed class ClingyWindow : Window
    {
        private readonly ClingyModel model;
        public int ClingyId => model.Id;

        public event EventHandler<int>? CloseRequested;
        public event EventHandler<PinRequestedEventArgs>? PinRequested;
        public event EventHandler<TitleChangeRequestedEventArgs>? TitleChangeRequested;
        public event EventHandler<PositionChangeRequestedEventArgs>? PositionChangeRequested;
        public event EventHandler<ContentChangeRequestedEventArgs>? ContentChangeRequested;
        public event EventHandler<UpdateWindowSizeRequestedEventArgs>? UpdateWindowSizeRequested;
        public event EventHandler<RollRequestedEventArgs>? RollRequested;
        public event EventHandler<StyleChangeRequestedEventArgs>? StyleChangeRequested;

        public IContextCommandProvider? CommandProvider { get; private set; }

        private readonly GtkUtilsService _srvUtils;
        private readonly MenuFactory _menuFactory;
        private readonly ClingyContextController _contextController;
        private int _lastX = int.MinValue;
        private int _lastY = int.MinValue;
        private ClingyTitleBar _titleBar;
        private ClingyBody _body;
        private Box _root;

        public ClingyWindow(ClingyModel clingyModel, GtkUtilsService utils,
                            MenuFactory menuFactory, ClingyContextController contextController)
                            : base(clingyModel.Title ?? string.Empty)
        {
            model = clingyModel;
            _srvUtils = utils;
            _menuFactory = menuFactory;
            _contextController = contextController;

            Decorated = false;
            SkipTaskbarHint = true;
            SkipPagerHint = true;

            DefaultWidth = Math.Max(AppConstants.Dimensions.DefaultClingyWidth, (int)model.Width);
            DefaultHeight = Math.Max(AppConstants.Dimensions.DefaultClingyHeight, (int)model.Height);
            Move((int)model.PositionX, (int)model.PositionY);
            _lastX = (int)model.PositionX;
            _lastY = (int)model.PositionY;

            // Build callbacks that *raise the same events* the Manager already listens to
            var cb = new ClingyWindowCallbacks(
                model.Id,
                closeRequested: () => CloseRequested?.Invoke(this, model.Id),
                positionChanged: (x, y) => PositionChangeRequested?.Invoke(this, new PositionChangeRequestedEventArgs(model.Id, x, y)),
                sizeChanged: (w, h) => UpdateWindowSizeRequested?.Invoke(this, new UpdateWindowSizeRequestedEventArgs(model.Id, w, h)),
                contentChanged: text => ContentChangeRequested?.Invoke(this, new ContentChangeRequestedEventArgs(model.Id, text)),
                titleChanged: title => TitleChangeRequested?.Invoke(this, new TitleChangeRequestedEventArgs(model.Id, title)),
                pinChanged: isPinned => PinRequested?.Invoke(this, new PinRequestedEventArgs(model.Id, isPinned)),
                rollChanged: isRolled => RollRequested?.Invoke(this, new RollRequestedEventArgs(model.Id, isRolled))
            );

            // Compose UI
            _root = new Box(Orientation.Vertical, 0)
            {
                Name = AppConstants.CssSections.ClingyWindow,
                BorderWidth = 0
            };
            _titleBar = ClingyTitleBar.Build(model, this, _srvUtils, cb);
            _body = ClingyBody.Build(model, this, cb);

            _root.PackStart(_titleBar, false, false, 0);
            _root.PackStart(_body, true, true, 0);

            // Wrap everything in an EventBox so we can catch right-clicks anywhere
            var clickCatcher = new EventBox { VisibleWindow = false }; // transparent, but receives events
            clickCatcher.Add(_root);

            // Listen for button presses
            clickCatcher.AddEvents((int)Gdk.EventMask.ButtonPressMask);
            clickCatcher.ButtonPressEvent += OnRightClick;

            // Optional: keyboard menu (Shift+F10 / Menu key) for accessibility
            clickCatcher.AddEvents((int)Gdk.EventMask.KeyPressMask);
            clickCatcher.KeyPressEvent += OnKeyPressForContextMenu;

            Add(clickCatcher);

            clickCatcher.ShowAll();

            // Persist *size* continuously in a WM-agnostic way
            this.SizeAllocated += (_, a) =>
            {
                UpdateWindowSizeRequested?.Invoke(this,
                    new UpdateWindowSizeRequestedEventArgs(model.Id, a.Allocation.Width, a.Allocation.Height));
            };

            // Add on focus title bar color change
            FocusInEvent += (_, __) => _titleBar.StyleContext.AddClass(AppConstants.CssSections.Focused);
            FocusOutEvent += (_, __) => _titleBar.StyleContext.RemoveClass(AppConstants.CssSections.Focused);

            AddEvents((int)Gdk.EventMask.StructureMask);
            ConfigureEvent += OnConfigureEvent;
            ApplyLockState(model.IsLocked);
            ApplyRollState(model.IsRolled);
        }

        [GLib.ConnectBefore]
        private void OnConfigureEvent(object? sender, ConfigureEventArgs e)
        {
            // Ask after the event has been applied
            int x, y;
            GetPosition(out x, out y);

            if (x != _lastX || y != _lastY)
            {
                _lastX = x;
                _lastY = y;
                // for now both the if and the else do the same, placeholder for when we adapt the app to Wayland
                if (_srvUtils.IsX11())
                    PositionChangeRequested?.Invoke(this,
                        new PositionChangeRequestedEventArgs(model.Id, x, y));
                else
                    PositionChangeRequested?.Invoke(this,
                        new PositionChangeRequestedEventArgs(model.Id, x, y));
            }

            // Don't swallow the event
            e.RetVal = false;
        }

        private void OnRightClick(object? sender, ButtonPressEventArgs e)
        {
            if (e.Event.Button != 3) return; // only right button
            var menu = _menuFactory.BuildClingyMenu(CommandProvider!, model.IsLocked, model.IsRolled);
            menu.ShowAll();
            menu.PopupAtPointer(e.Event);
            e.RetVal = true; // stop further handling
        }

        private void OnKeyPressForContextMenu(object? sender, KeyPressEventArgs e)
        {
            // Show menu on Shift+F10 or the Menu key
            var isShiftF10 = (e.Event.Key == Gdk.Key.F10) && ((e.Event.State & Gdk.ModifierType.ShiftMask) != 0);
            // Show title dialog on Shft+Ctrl+T
            var isShiftCtrlT = (e.Event.Key == Gdk.Key.T) &&
                                ((e.Event.State & Gdk.ModifierType.ShiftMask) != 0) &&
                                ((e.Event.State & Gdk.ModifierType.ControlMask) != 0);
            var isMenuKey = e.Event.Key == Gdk.Key.Menu;

            if (!isShiftF10 && !isMenuKey && !isShiftCtrlT) return;

            if (isShiftF10 || isMenuKey)
            {
                var menu = _menuFactory.BuildClingyMenu(CommandProvider!, model.IsLocked, model.IsRolled);
                menu.ShowAll();
                menu.PopupAtWidget(this, Gdk.Gravity.SouthWest, Gdk.Gravity.NorthWest, null);
            }

            if (isShiftCtrlT)
            {
                _contextController.ShowChangeTitleDialog();
            }

            e.RetVal = true;
        }
        public void SetPinIcon(bool pinned) => _titleBar.SetPinIcon(pinned);

        public void ChangeTitleBarText(string newTitle) => _titleBar.ChangeTitle(newTitle);

        public void SetContextCommandProvider(IContextCommandProvider provider) =>
            CommandProvider = provider ?? throw new ArgumentNullException(nameof(provider));

        public void ApplyLockState(bool isLocked)
        {
            _titleBar.SetLockIcon(isLocked);
            _body.ApplyLock(isLocked, this);
        }

        // public accessor for window manager (only knows isRolled)
        public void ApplyRollState(bool isRolled) =>
            _body.ApplyRollState(this, _root, isRolled, (int)model.Width);
    }
}
