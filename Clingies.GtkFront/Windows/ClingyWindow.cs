using System;
using Clingies.ApplicationLogic.CustomEventArgs;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.Domain.DTOs;
using Clingies.GtkFront.Services;
using Clingies.GtkFront.Windows.Parts;
using Gtk;

namespace Clingies.GtkFront.Windows
{
    public sealed class ClingyWindow : Window
    {
        private readonly ClingyDto dto;
        public int ClingyId => dto.Id;

        public event EventHandler<int>? CloseRequested;
        public event EventHandler<PinRequestedEventArgs>? PinRequested;
        public event EventHandler<TitleChangeRequestedEventArgs>? TitleChangeRequested;
        public event EventHandler<PositionChangeRequestedEventArgs>? PositionChangeRequested;
        public event EventHandler<RollRequestedEventArgs>? RollRequested;
        public event EventHandler<ContentChangeRequestedEventArgs>? ContentChangeRequested;
        public event EventHandler<UpdateWindowSizeRequestedEventArgs>? UpdateWindowSizeRequested;
        public event EventHandler<LockRequestedEventArgs>? LockRequested;

        public IContextCommandProvider? CommandProvider { get; private set; }

        private readonly GtkUtilsService _srvUtils;
        private readonly MenuFactory _menuFactory;
        private readonly ClingyContextController _contextController;
        private int _lastX = int.MinValue;
        private int _lastY = int.MinValue;
        private ClingyTitleBar _titleBar;
        private ClingyBody _body;
        private Revealer _bodyRevealer;
        private bool _isRolled;


        public ClingyWindow(ClingyDto clingyDto, GtkUtilsService utils,
                            MenuFactory menuFactory, ClingyContextController contextController)
                            : base(clingyDto.Title ?? string.Empty)
        {
            dto = clingyDto;
            _srvUtils = utils;
            _menuFactory = menuFactory;
            _contextController = contextController;

            Decorated = false;
            SkipTaskbarHint = true;
            SkipPagerHint = true;

            DefaultWidth = Math.Max(AppConstants.Dimensions.DefaultClingyWidth, (int)dto.Width);
            DefaultHeight = Math.Max(AppConstants.Dimensions.DefaultClingyHeight, (int)dto.Height);
            Move((int)dto.PositionX, (int)dto.PositionY);
            _lastX = (int)dto.PositionX;
            _lastY = (int)dto.PositionY;

            // Build callbacks that *raise the same events* the Manager already listens to
            var cb = new ClingyWindowCallbacks(
                dto.Id,
                closeRequested: () => CloseRequested?.Invoke(this, dto.Id),
                positionChanged: (x, y) => PositionChangeRequested?.Invoke(this, new PositionChangeRequestedEventArgs(dto.Id, x, y)),
                sizeChanged: (w, h) => UpdateWindowSizeRequested?.Invoke(this, new UpdateWindowSizeRequestedEventArgs(dto.Id, w, h)),
                contentChanged: text => ContentChangeRequested?.Invoke(this, new ContentChangeRequestedEventArgs(dto.Id, text)),
                titleChanged: title => TitleChangeRequested?.Invoke(this, new TitleChangeRequestedEventArgs(dto.Id, title)),
                pinChanged: isPinned => PinRequested?.Invoke(this, new PinRequestedEventArgs(dto.Id, isPinned))
            );

            // Compose UI
            var root = new Box(Orientation.Vertical, 0)
            {
                Name = AppConstants.CssSections.ClingyWindow,
                BorderWidth = 0
            };
            _titleBar = ClingyTitleBar.Build(dto, this, _srvUtils, cb);
            _body = ClingyBody.Build(dto, this, cb);

            _bodyRevealer = new Revealer
            {
                RevealChild = !dto.IsRolled,
                TransitionType = dto.IsRolled ?
                        RevealerTransitionType.SlideDown :
                        RevealerTransitionType.SlideDown,
                TransitionDuration = 150
            };

            _bodyRevealer.Add(_body);

            root.PackStart(_titleBar, false, false, 0);
            root.PackStart(_bodyRevealer, true, true, 0);

            // Wrap everything in an EventBox so we can catch right-clicks anywhere
            var clickCatcher = new EventBox { VisibleWindow = false }; // transparent, but receives events
            clickCatcher.Add(root);

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
                    new UpdateWindowSizeRequestedEventArgs(dto.Id, a.Allocation.Width, a.Allocation.Height));
            };

            // Add on focus title bar color change
            FocusInEvent += (_, __) => _titleBar.StyleContext.AddClass(AppConstants.CssSections.Focused);
            FocusOutEvent += (_, __) => _titleBar.StyleContext.RemoveClass(AppConstants.CssSections.Focused);

            AddEvents((int)Gdk.EventMask.StructureMask);
            ConfigureEvent += OnConfigureEvent;
            ApplyContentLock(dto.IsLocked);
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
                        new PositionChangeRequestedEventArgs(dto.Id, x, y));
                else
                    PositionChangeRequested?.Invoke(this,
                        new PositionChangeRequestedEventArgs(dto.Id, x, y));
            }

            // Don't swallow the event
            e.RetVal = false;
        }

        private void OnRightClick(object? sender, ButtonPressEventArgs e)
        {
            if (e.Event.Button != 3) return; // only right button
            var menu = _menuFactory.BuildClingyMenu(CommandProvider!, dto.IsLocked);
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
                var menu = _menuFactory.BuildClingyMenu(CommandProvider!, dto.IsLocked);
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

        public void SetLockIcon(bool locked)
        {
            _titleBar.SetLockIcon(locked);
            ApplyContentLock(locked);
        }

        public void SetRolled(bool isRolled)
        {
            _isRolled = isRolled;
            _bodyRevealer.TransitionType = isRolled ?
                     RevealerTransitionType.SlideUp :
                     RevealerTransitionType.SlideDown;
            _bodyRevealer.RevealChild = !isRolled;

            ApplyRollGeometry(isRolled);
            GLib.Idle.Add(() => { ApplyRollGeometry(_isRolled); return false; });            
        }

        private void ApplyRollGeometry(bool isRolled)
        {
            // Measure title bar height
            _titleBar.GetPreferredHeight(out int minH, out int natH);
            int titleH = Math.Max(minH, natH);

            var geom = new Gdk.Geometry();

            if (isRolled)
            {
                // Fix height to title bar only
                geom.MinHeight = titleH;
                geom.MaxHeight = titleH;
                // keep width flexible
                this.SetGeometryHints(this, geom,
                    Gdk.WindowHints.MinSize | Gdk.WindowHints.MaxSize);
            }
            else
            {
                // Remove the tight clamp: allow normal growth again
                geom.MinHeight = titleH;          // still can’t be smaller than title
                geom.MaxHeight = int.MaxValue;    // effectively “no cap”
                this.SetGeometryHints(this, geom,
                    Gdk.WindowHints.MinSize | Gdk.WindowHints.MaxSize);
            }

            // Optional: nudge size so WM applies constraints immediately
            this.QueueResize();
        }
        private void ApplyContentLock(bool isLocked) => _body.ApplyLock(isLocked, this);
    }
}
