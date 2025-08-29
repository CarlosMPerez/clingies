using System;
using Clingies.ApplicationLogic.CustomEventArgs;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.ApplicationLogic.Services;
using Clingies.Domain.Models;
using Clingies.Gtk.Factories;
using Clingies.Gtk.Utils;
using Gtk;

namespace Clingies.Gtk.Windows
{
    /// <summary>
    /// GTK sticky-note window (frameless). Composes a custom header (title bar) + body.
    /// - No taskbar/pager presence
    /// - Short header height
    /// - Centered title text (editable)
    /// - Image-based Close button (from Assets/clingy_close.png)
    /// - Persists geometry and content via injected service
    /// </summary>
    public sealed class ClingyWindow : Window, IContextCommandController
    {
        // --- Dependencies / state ---
        private readonly ClingyDto dto;
        public int ClingyId => dto.Id;
        private MenuFactory menuFactory;
        public event EventHandler<int>? CloseRequested;
        public event EventHandler<PinRequestedEventArgs>? PinRequested;
        public event EventHandler<TitleChangeRequestedEventArgs>? TitleChangeRequested;
        public event EventHandler<PositionChangeRequestedEventArgs>? PositionChangeRequested;
        public event EventHandler<RollRequestedEventArgs>? RollRequested;
        public event EventHandler<ContentChangeRequestedEventArgs>? ContentChangeRequested;
        public event EventHandler<UpdateWindowSizeRequestedEventArgs>? UpdateWindowSizeRequested;
        public event EventHandler<LockRequestedEventArgs>? LockRequested;
        public IContextCommandProvider? CommandProvider { get; private set; }
        private readonly ClingyService _srvClingy;
        private readonly UtilsService _srvUtils;
        // --- UI elements we need to access after build ---
        private Label titleEntry = default!;
        private TextView contentView = default!;
        private Overlay body = default!;
        private Box titleBar = default!;
        private EventBox _rightGrip;
        private EventBox _leftGrip;

        public ClingyWindow(ClingyDto clingyDto,
                            ClingyService srvClingy,
                            UtilsService srvUtils) : base(clingyDto.Title ?? string.Empty)
        {

            dto = clingyDto;
            _srvClingy = srvClingy;
            _srvUtils = srvUtils;

            ApplyWindowChrome();
            InitializeGeometryFromDto(dto);

            // Root container (vertical stack: header + body)
            var root = new Box(Orientation.Vertical, 0)
            {
                Name = "clingy-window",
                BorderWidth = 0
            };

            // Build and compose
            titleBar = BuildTitleBar();
            body = BuildBody();
            var rightGrip = ConfigureGrip(false);
            var leftGrip = ConfigureGrip(true);
            body.AddOverlay(rightGrip);
            body.AddOverlay(leftGrip);

            titleBar.Events |= Gdk.EventMask.ButtonPressMask;
            //Wiring drag and drop
            titleBar.ButtonPressEvent += (o, args) =>
            {
                if (args.Event.Button == 1) // left button
                {
                    Console.WriteLine("Button pressed");
                    BeginMoveDrag((int)args.Event.Button,
                            (int)args.Event.XRoot,
                            (int)args.Event.YRoot,
                            args.Event.Time);
                }
            };

            root.PackStart(titleBar, false, false, 0);
            root.PackStart(body, true, true, 0);
            root.ShowAll();
            Add(root);

            WirePersistenceEvents();
        }

        // ---------------------------
        // Window basics / chrome
        // ---------------------------
        private void ApplyWindowChrome()
        {
            // Remove system decorations (no title bar / min / max / close)
            Decorated = false;

            // Don’t show in taskbar or workspace pager
            SkipTaskbarHint = true;
            SkipPagerHint = true;

            // Future Pin/Unpin uses KeepAbove toggle:
            // KeepAbove = true;
        }

        private void InitializeGeometryFromDto(ClingyDto dto)
        {
            DefaultWidth = (int)Math.Max(200, dto.Width);
            DefaultHeight = (int)Math.Max(120, dto.Height);
            Move((int)dto.PositionX, (int)dto.PositionY);
        }

        public void SetContextCommandProvider(IContextCommandProvider provider)
        {
            CommandProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        // ---------------------------
        // Title Bar (fake title bar)
        // ---------------------------
        private Box BuildTitleBar()
        {
            // Shorter title bar; actual draw height is CSS + HeightRequest
            var header = new Box(Orientation.Horizontal, 0)
            {
                Name = "clingy-title",
                HeightRequest = 22, // shorter header; tweak with CSS padding if needed
                BorderWidth = 0
            };

            // --- LEFT SIDE GROUP ----------------------------------------------------
            var leftBox = new Box(Orientation.Horizontal, 0)
            {
                Halign = Align.Start,
                Valign = Align.Center
            };

            // PIN BUTTON
            var pinBtn = CreateImageButton(
                name: "btn-pin",
                assetName: "clingy_unpinned.png",
                onClick: (_, __) => TogglePin() //TODO 
            );
            pinBtn.MarginStart = 2;   // near the left edge
            pinBtn.MarginEnd = 4;

            // LOCK BUTTON
            var lockBtn = CreateImageButton(
                name: "btn-lock",
                assetName: "clingy_locked.png",
                onClick: (_, __) => ToggleLock() //TODO
            );
            lockBtn.MarginEnd = 4;

            // Only show when locked
            lockBtn.NoShowAll = true;
            lockBtn.Visible = dto.IsLocked;

            // Add buttons to leftBox
            leftBox.PackStart(pinBtn, false, false, 0);
            leftBox.PackStart(lockBtn, false, false, 0);

            // --- CENTER: TITLE ------------------------------------------------------
            var titleLabel = new Label(dto.Title ?? string.Empty)
            {
                Name = "clingy-title-label",
                Xalign = 0.5f,  // center inside its own allocation
                Yalign = 0.5f,
                Halign = Align.Fill,
                Valign = Align.Center
            };
            // Keep a reference if you want to update it from a rename dialog
            titleEntry = titleLabel;

            // --- RIGHT SIDE GROUP ---------------------------------------------------
            var rightBox = new Box(Orientation.Horizontal, 0)
            {
                Halign = Align.End,
                Valign = Align.Center
            };

            var closeBtn = CreateImageButton(
                name: "btn-close",
                assetName: "clingy_close.png",
                onClick: (_, __) => this.Close()
            );
            closeBtn.MarginEnd = 2;   // near the right edge
            closeBtn.MarginStart = 4;

            rightBox.PackEnd(closeBtn, false, false, 0);

            // --- COMPOSE ROW --------------------------------------------------------
            // Layout trick: pack left group, then title (expands), then right group.
            // Title will appear centered-ish; for pixel-perfect centering independent
            // of left/right widths, you'd need a more involved spacer-balancing routine.
            header.PackStart(leftBox, false, false, 0);
            header.PackStart(titleLabel, true, true, 0);
            header.PackEnd(rightBox, false, false, 0);

            return header;
        }

        /// <summary>
        /// Creates a button that shows an image loaded from the given relative asset path.
        /// The image is not scaled here—ship the PNG at the desired size (e.g., 12–14px).
        /// </summary>
        private Button CreateImageButton(string name, string assetName, EventHandler onClick)
        {
            string assetPath = System.IO.Path.Combine(AppContext.BaseDirectory, $"Assets/{assetName}");
            var btn = new Button
            {
                Name = name,
                Relief = ReliefStyle.None,
                CanFocus = false,
                // In GTK3, Button.Content = img is not a thing; use the Image property
                Image = _srvUtils.CreateImageFromPath(assetPath, 12)
            };

            // Tighten intrinsic size (prevents extra padding from some themes)
            btn.SetSizeRequest(16, 16);
            btn.Clicked += onClick;
            return btn;
        }

        // ---------------------------
        // Body (content area)
        // ---------------------------
        private Overlay BuildBody()
        {
            var overlay = new Overlay();
            var scroller = new ScrolledWindow
            {
                Name = "clingy-content",
                ShadowType = ShadowType.None
            };

            contentView = new TextView
            {
                Name = "clingy-content-view"
            };

            contentView.Buffer.Text = dto.Content ?? string.Empty;
            contentView.Buffer.Changed += (_, __) =>
            {
                dto.Content = contentView.Buffer.Text;
            };

            scroller.Add(contentView);
            overlay.Add(scroller);
            return overlay;
        }

        // ---------------------------
        // Persistence wiring
        // ---------------------------
        private void WirePersistenceEvents()
        {
            // Track geometry changes (move/resize). We only update the DTO fields here.
            // Actual DB write happens on window close (and you may add a debounce autosave later).
            ConfigureEvent += (_, e) =>
            {
                dto.PositionX = e.Event.X;
                dto.PositionY = e.Event.Y;
                dto.Width = e.Event.Width;
                dto.Height = e.Event.Height;
            };

            // Persist on close
            DeleteEvent += (_, __) => _srvClingy.Update(dto);
        }

        private EventBox ConfigureGrip(bool isLeftGrip)
        {
            EventBox grip = new EventBox { VisibleWindow = false, WidthRequest = 6 };
            grip.VisibleWindow = false;
            grip.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.EnterNotifyMask | Gdk.EventMask.LeaveNotifyMask;
            grip.Halign = isLeftGrip ? Align.Start : Align.End;
            grip.Valign = Align.Fill;

            grip.ButtonPressEvent += (o, args) =>
            {
                if (args.Event.Button == 1)
                {
                    BeginResizeDrag(isLeftGrip ? Gdk.WindowEdge.West : Gdk.WindowEdge.East,
                    (int)args.Event.Button, (int)args.Event.XRoot, (int)args.Event.YRoot, args.Event.Time);
                }
            };

            grip.EnterNotifyEvent += (o, args) =>
            {
                var display = Display;
                using (var cursor = new Gdk.Cursor(display, isLeftGrip ? Gdk.CursorType.LeftSide : Gdk.CursorType.RightSide))
                    grip.Window.Cursor = cursor;
            };

            grip.LeaveNotifyEvent += (o, args) =>
            {
                grip.Window.Cursor = null;
            };

            return grip;
        }

        // ---------------------------
        // Events
        // ---------------------------
        private void TogglePin()
        {
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }

        private void ToggleLock()
        {
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }

        public void SleepClingy()
        {
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }

        public void ShowAlarmWindow()
        {
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }

        public void ShowChangeTitleDialog()
        {
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }

        public void ShowColorWindow()
        {
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }

        public void LockClingy()
        {
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }

        public void UnlockClingy()
        {
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }

        public void ShowPropertiesWindow()
        {
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }
    }
}