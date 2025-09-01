// src/Clingies.Gtk/Windows/ClingyWindow.cs  (edited)
using System;
using Clingies.ApplicationLogic.CustomEventArgs;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.Domain.Models;
using Clingies.GtkFront.Factories;
using Clingies.GtkFront.Utils;
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

        private readonly UtilsService _srvUtils;
        private readonly MenuFactory _menuFactory;
        private readonly ClingyContextController _contextController;
        private int _lastX = int.MinValue;
        private int _lastY = int.MinValue;

        public ClingyWindow(ClingyDto clingyDto, UtilsService utils,
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

            DefaultWidth = Math.Max(200, (int)dto.Width);
            DefaultHeight = Math.Max(120, (int)dto.Height);
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
                pinChanged: isPinned => PinRequested?.Invoke(this, new PinRequestedEventArgs(dto.Id, isPinned)),
                lockChanged: isLocked => LockRequested?.Invoke(this, new LockRequestedEventArgs(dto.Id, isLocked))
            );

            // Compose UI
            var root = new Box(Orientation.Vertical, 0) { Name = "clingy-window", BorderWidth = 0 };
            var title = ClingyTitleBarBuilder.Build(dto, this, _srvUtils, cb);
            var body = ClingyBodyBuilder.Build(dto, this, _srvUtils, cb);

            root.PackStart(title, false, false, 0);
            root.PackStart(body, true, true, 0);

            // Wrap everything in an EventBox so we can catch right-clicks anywhere
            var clickCatcher = new EventBox { VisibleWindow = false }; // transparent, but receives events
            clickCatcher.Add(root);

            // Listen for button presses
            clickCatcher.AddEvents((int)Gdk.EventMask.ButtonPressMask);
            clickCatcher.ButtonPressEvent += OnAnyRightClick;

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
            FocusInEvent += (_, __) => title.StyleContext.AddClass("focused");
            FocusOutEvent += (_, __) => title.StyleContext.RemoveClass("focused");

            AddEvents((int)Gdk.EventMask.StructureMask);
            ConfigureEvent += OnConfigureEvent;
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

        private void OnAnyRightClick(object? sender, ButtonPressEventArgs e)
        {
            if (e.Event.Button != 3) return; // only right button
            var menu = _menuFactory.BuildClingyMenu(CommandProvider!);
            menu.ShowAll();
            menu.PopupAtPointer(e.Event);
            e.RetVal = true; // stop further handling
        }

        private void OnKeyPressForContextMenu(object? sender, KeyPressEventArgs e)
        {
            // Show menu on Shift+F10 or the Menu key
            var isShiftF10 = (e.Event.Key == Gdk.Key.F10) && (e.Event.State & Gdk.ModifierType.ShiftMask) != 0;
            var isMenuKey  = e.Event.Key == Gdk.Key.Menu;

            if (!isShiftF10 && !isMenuKey) return;

            var menu = _menuFactory.BuildClingyMenu(CommandProvider!);
            menu.ShowAll();
            menu.PopupAtWidget(this, Gdk.Gravity.SouthWest, Gdk.Gravity.NorthWest, null);
            e.RetVal = true;
        }

        public void SetContextCommandProvider(IContextCommandProvider provider) =>
            CommandProvider = provider ?? throw new ArgumentNullException(nameof(provider));
    }
}
