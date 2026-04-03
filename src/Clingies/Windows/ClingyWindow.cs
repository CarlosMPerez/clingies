using System;
using Clingies.Application.Interfaces;
using Clingies.Domain.Models;
using Clingies.Services;
using Clingies.Windows.Parts;
using Gtk;

namespace Clingies.Windows
{
    public sealed class ClingyWindow : Window
    {
        private readonly ClingyModel model;
        public int ClingyId => model.Id;

        public event global::System.Action<int>? CloseRequested;
        public event global::System.Action<int, bool>? PinRequested;
        public event global::System.Action<int, string>? TitleChangeRequested;
        public event global::System.Action<int, int, int>? PositionChangeRequested;
        public event global::System.Action<int, string?>? ContentChangeRequested;
        public event global::System.Action<int, double, double>? UpdateWindowSizeRequested;
        public event global::System.Action<int, bool>? RollRequested;

        private readonly GtkUtilsService _srvUtils;
        private readonly AppMenuFactory _menuFactory;
        private readonly ClingyContextController _contextController;
        private int _lastX = int.MinValue;
        private int _lastY = int.MinValue;
        private int _lastWidth = int.MinValue;
        private int _lastHeight = int.MinValue;
        private bool _isClosing;
        private ClingyTitleBar _titleBar;
        private ClingyBody _body;
        private Box _root;

        public ClingyWindow(ClingyModel clingyModel, GtkUtilsService utils,
                            AppMenuFactory menuFactory, ClingyContextController contextController)
                            : base(clingyModel.Title ?? string.Empty)
        {
            model = clingyModel;
            _srvUtils = utils;
            _menuFactory = menuFactory;
            _contextController = contextController;

            Decorated = false;
            SkipTaskbarHint = true;
            SkipPagerHint = true;

            var initialWidth = model.Width > 0 ? (int)model.Width : AppConstants.Dimensions.DefaultClingyWidth;
            var initialHeight = model.Height > 0 ? (int)model.Height : AppConstants.Dimensions.DefaultClingyHeight;

            DefaultWidth = Math.Max(AppConstants.Dimensions.MinimumClingyWidth, initialWidth);
            DefaultHeight = Math.Max(AppConstants.Dimensions.TitleHeight, initialHeight);
            Move((int)model.PositionX, (int)model.PositionY);
            _lastX = (int)model.PositionX;
            _lastY = (int)model.PositionY;
            _lastWidth = DefaultWidth;
            _lastHeight = DefaultHeight;

            // Build callbacks that *raise the same events* the Manager already listens to
            var cb = new ClingyWindowCallbacks(
                model.Id,
                closeRequested: () => CloseRequested?.Invoke(model.Id),
                positionChanged: (x, y) => PositionChangeRequested?.Invoke(model.Id, x, y),
                sizeChanged: (w, h) => UpdateWindowSizeRequested?.Invoke(model.Id, w, h),
                contentChanged: text => ContentChangeRequested?.Invoke(model.Id, text),
                titleChanged: title => TitleChangeRequested?.Invoke(model.Id, title),
                pinChanged: isPinned => PinRequested?.Invoke(model.Id, isPinned),
                rollChanged: isRolled => RollRequested?.Invoke(model.Id, isRolled)
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

            AddEvents((int)(Gdk.EventMask.KeyPressMask | Gdk.EventMask.StructureMask));
            KeyPressEvent += OnWindowKeyPress;

            Add(clickCatcher);

            clickCatcher.ShowAll();

            DeleteEvent += (_, __) =>
            {
                if (_isClosing)
                    return;

                GetPosition(out var x, out var y);
                GetSize(out var w, out var h);

                PositionChangeRequested?.Invoke(model.Id, x, y);
                UpdateWindowSizeRequested?.Invoke(model.Id, w, h);
            };

            // Add on focus title bar color change
            FocusInEvent += (_, __) => _titleBar.StyleContext.AddClass(AppConstants.CssSections.Focused);
            FocusOutEvent += (_, __) => _titleBar.StyleContext.RemoveClass(AppConstants.CssSections.Focused);

            ConfigureEvent += OnConfigureEvent;
            ApplyLockState(model.IsLocked);
            ApplyRollState(model.IsRolled);
        }

        [GLib.ConnectBefore]
        private void OnConfigureEvent(object? sender, ConfigureEventArgs e)
        {
            if (_isClosing)
            {
                e.RetVal = false;
                return;
            }

            var x = e.Event.X;
            var y = e.Event.Y;
            var width = e.Event.Width;
            var height = e.Event.Height;

            if (x != _lastX || y != _lastY)
            {
                _lastX = x;
                _lastY = y;
                // for now both the if and the else do the same, placeholder for when we adapt the app to Wayland
                if (_srvUtils.IsX11())
                    PositionChangeRequested?.Invoke(model.Id, x, y);
                else
                    PositionChangeRequested?.Invoke(model.Id, x, y);
            }

            if (width != _lastWidth || height != _lastHeight)
            {
                _lastWidth = width;
                _lastHeight = height;
                UpdateWindowSizeRequested?.Invoke(model.Id, width, height);
            }

            // Don't swallow the event
            e.RetVal = false;
        }

        [GLib.ConnectBefore]
        private void OnWindowKeyPress(object? sender, KeyPressEventArgs e)
        {
            var isCtrlV = (e.Event.State & Gdk.ModifierType.ControlMask) != 0 &&
                          (e.Event.Key == Gdk.Key.v || e.Event.Key == Gdk.Key.V);

            if (!isCtrlV)
                return;

            if (model.IsLocked)
            {
                e.RetVal = true;
                return;
            }

            if (_body.TryPasteImageFromClipboard(this))
                e.RetVal = true;
        }

        private void OnRightClick(object? sender, ButtonPressEventArgs e)
        {
            if (e.Event.Button != 3) return; // only right button
            var menu = _menuFactory.BuildClingyMenu(_contextController, model.IsLocked, model.IsRolled);
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
                var menu = _menuFactory.BuildClingyMenu(_contextController, model.IsLocked, model.IsRolled);
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

        public void ApplyLockState(bool isLocked)
        {
            _titleBar.SetLockIcon(isLocked);
            _body.ApplyLock(isLocked, this);
        }

        public void BeginClose() => _isClosing = true;

        // public accessor for window manager (only knows isRolled)
        public void ApplyRollState(bool isRolled) =>
            _body.ApplyRollState(this, _root, isRolled, (int)model.Width);
    }
}
