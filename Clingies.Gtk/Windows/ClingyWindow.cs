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
        private uint _autosizeDebounceId;
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
        private const int MinHeight = 100;
        private const int MaxHeight = 1500;
        private const int VerticalPadding = 16;
        private const uint AUTOSAVE_DELAY_MS = 250;
        private uint _autosaveId;
        private bool _dirtyPending;

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
                onClick: (_, __) => CloseClingy()
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

            // Wire move drag
            header.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask;

            header.ButtonPressEvent += (_, e) =>
            {
                if (e.Event.Button == 1)
                    BeginMoveDrag((int)e.Event.Button, (int)e.Event.XRoot, (int)e.Event.YRoot, e.Event.Time);
            };

            header.ButtonReleaseEvent += (_, __) =>
            {
                GetPosition(out var x, out var y);
                dto.PositionX = x;
                dto.PositionY = y;
                FlushAutoSave();
                SaveNow();
            };

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
                ShadowType = ShadowType.None,
                // hard-disable horizontal scroll; we’re wrapping instead
                HscrollbarPolicy = PolicyType.Never,
                VscrollbarPolicy = PolicyType.Automatic                
            };

            contentView = new TextView
            {
                Name = "clingy-content-view",
                CursorVisible = true,
                Editable = true,
                CanFocus = true,
                WrapMode = WrapMode.WordChar,
                LeftMargin = 6,
                RightMargin = 6
            };

            contentView.Buffer.Text = dto.Content ?? string.Empty;
            // ensure it takes focus on first map (window show)
            contentView.MapEvent += (_, __) => GLib.Idle.Add(() => { contentView.GrabFocus(); return false; });            
            // ensure focus on click (some themes/overlays can be finnicky)
            contentView.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.EnterNotifyMask | Gdk.EventMask.LeaveNotifyMask;
            contentView.ButtonPressEvent += (_, e) =>
            {
                contentView.GrabFocus();
                // Translate click to buffer iter and place the cursor explicitly
                int bx, by;
                contentView.WindowToBufferCoords(TextWindowType.Text, (int)e.Event.X, (int)e.Event.Y, out bx, out by);
                contentView.GetIterAtLocation(out var iter, bx, by);
                contentView.Buffer.PlaceCursor(iter);
            };

            // belt-and-suspenders: if focus arrives via keyboard, make sure the caret is shown
            contentView.FocusInEvent += (_, __) =>
            {
                TextIter x; TextIter y;
                if (!contentView.CursorVisible) contentView.CursorVisible = true;
                if (contentView.Buffer != null && contentView.Buffer.GetSelectionBounds(out x, out y) == false)
                    contentView.Buffer.PlaceCursor(contentView.Buffer.EndIter);
            };

            // force I-beam when hovering the editable area (defensive against custom cursors elsewhere)
            contentView.EnterNotifyEvent += (o, e) =>
            {
                try
                {
                    // TextView has multiple GdkWindows; use the event window when available,
                    // or the dedicated text window as a fallback.
                    var win = e.Event.Window ?? contentView.GetWindow(TextWindowType.Text);
                    using var cursor = new Gdk.Cursor(Display, Gdk.CursorType.Xterm);
                    if (win != null) win.Cursor = cursor;
                }
                catch { /* harmless if WM/theme ignores */ }
            };

            contentView.LeaveNotifyEvent += (o, e) =>
            {
                try
                {
                    var win = e.Event.Window ?? contentView.GetWindow(TextWindowType.Text);
                    if (win != null) win.Cursor = null;
                }
                catch { }
            };            

            contentView.Buffer.Changed += (s, e) =>
            {
                dto.Content = contentView.Buffer.Text;
                DebounceAutosize();
                MarkDirtyAndDebounceSave();
            };
            contentView.SizeAllocated += (s, e) => DebounceAutosize();

            scroller.Add(contentView);
            overlay.Add(scroller);
            // keep your edge grips (thin overlay children) — they won’t interfere with the center
            overlay.AddOverlay(ConfigureGrip(isLeftGrip: true));
            overlay.AddOverlay(ConfigureGrip(isLeftGrip: false));            

            return overlay;
        }

        // ---------------------------
        // Auto-height (content)
        // ---------------------------
        private void DebounceAutosize()
        {
            if (_autosizeDebounceId != 0) GLib.Source.Remove(_autosizeDebounceId);

            _autosizeDebounceId = GLib.Timeout.Add(16, () =>
            {
                _autosizeDebounceId = 0;
                AutoHeightToContent();
                return false;
            });
        }

        private void AutoHeightToContent()
        {
            // current content width in pixels
            int contentWidth = contentView.Allocation.Width;
            if (contentWidth <= 0) return;
            // Measure text height using Pango layout
            int textHeight = MeasureTextHeight(contentView, contentWidth);
            int targetHeight = Clamp(textHeight + VerticalPadding, MinHeight, MaxHeight);
            // keep current width, only adjust height
            int currentWidth = Allocation.Width > 0 ? Allocation.Width : 300;
            Resize(currentWidth, targetHeight);
        }

        private static int MeasureTextHeight(TextView tv, int widthPx)
        {
            using (var layout = tv.CreatePangoLayout(tv.Buffer.Text))
            {
                layout.Wrap = Pango.WrapMode.WordChar;
                layout.Width = (int)(widthPx * Pango.Scale.PangoScale);
                layout.GetPixelSize(out _, out int heightPx);
                return heightPx;
            }
        }

        private static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);


        // ---------------------------
        // Centralized persistence wiring (+ debounce)
        // ---------------------------
        private void WirePersistenceEvents()
        {
            // Track geometry changes (move/resize). We only update the DTO fields here.
            this.SizeAllocated += (_, alloc) =>
            {
                dto.Width  = alloc.Allocation.Width;
                dto.Height = alloc.Allocation.Height;
                Console.WriteLine($"ALLOC X:{dto.PositionX} - Y:{dto.PositionY} - W:{dto.Width} - H:{dto.Height}");
                MarkDirtyAndDebounceSave();
            };            

            AddEvents((int)Gdk.EventMask.StructureMask);
            ConfigureEvent += (_, e) =>
            {
                GetPosition(out var x, out var y);
                dto.PositionX = x;
                dto.PositionY = y;

                dto.Width = e.Event.Width;
                dto.Height = e.Event.Height;

                Console.WriteLine($"CONFIG X:{dto.PositionX} - Y:{dto.PositionY} - W:{dto.Width} - H:{dto.Height}");
                MarkDirtyAndDebounceSave();
            };

            DeleteEvent += (_, __) =>
            {
                FlushAutoSave();
                SaveNow();
            };
        }

        private void MarkDirtyAndDebounceSave()
        {
            _dirtyPending = true;
            if (_autosaveId != 0)
            {
                GLib.Source.Remove(_autosaveId);
                _autosaveId = 0;
            }

            _autosaveId = GLib.Timeout.Add(AUTOSAVE_DELAY_MS, () =>
            {
                _autosaveId = 0;
                if (_dirtyPending)
                {
                    _dirtyPending = false;
                    SaveNow();
                }
                return false;
            });
        }

        private void FlushAutoSave()
        {
            if (_autosaveId != 0)
            {
                GLib.Source.Remove(_autosaveId);
                _autosaveId = 0;
            }
            if (_dirtyPending)
            {
                _dirtyPending = false;
                SaveNow();
            }
        }

        private void SaveNow()
        {
            try
            {
                _srvClingy.Update(dto);
            }
            catch (Exception ex)
            {
                //TODO Serilogger
                Console.Error.WriteLine($"[ClingyWindow] Persist failed (Id={dto.Id}): {ex}");
            }
        }

        private EventBox ConfigureGrip(bool isLeftGrip)
        {
            EventBox grip = new EventBox { VisibleWindow = false, WidthRequest = 6 };
            grip.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.EnterNotifyMask | Gdk.EventMask.LeaveNotifyMask;
            grip.Halign = isLeftGrip ? Align.Start : Align.End;
            grip.Valign = Align.Fill;

            grip.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask
                 | Gdk.EventMask.EnterNotifyMask | Gdk.EventMask.LeaveNotifyMask;

            grip.ButtonPressEvent += (o, args) =>
            {
                if (args.Event.Button == 1)
                {
                    BeginResizeDrag(isLeftGrip ? Gdk.WindowEdge.West : Gdk.WindowEdge.East,
                    (int)args.Event.Button, (int)args.Event.XRoot, (int)args.Event.YRoot, args.Event.Time);
                }
            };

            // THIS is the end-of-resize certainty
            grip.ButtonReleaseEvent += (_, __) =>
            {
                // Snapshot current geometry and save immediately (no debounce)
                dto.Width  = Allocation.Width;
                dto.Height = Allocation.Height;
                GetPosition(out var x, out var y);
                dto.PositionX = x;
                dto.PositionY = y;

                FlushAutoSave();
                SaveNow();
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
        private void CloseClingy()
        {
            // Ensure last changes are persisted even if DeleteEvent order differs
            dto.IsDeleted = true;
            FlushAutoSave();
            SaveNow();
            this.Close();
        }

        private void TogglePin()
        {
            // Make changes to DTO PRIOR TO SAVE
            // MarkDirtyAndDebounceSave();
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }

        private void ToggleLock()
        {
            // MarkDirtyAndDebounceSave();
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }

        public void SleepClingy()
        {
            // MarkDirtyAndDebounceSave();
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }

        public void ShowAlarmWindow()
        {
            // MarkDirtyAndDebounceSave();
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }

        public void ShowChangeTitleDialog()
        {
            // MarkDirtyAndDebounceSave();
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }

        public void ShowColorWindow()
        {
            // MarkDirtyAndDebounceSave();
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }

        public void LockClingy()
        {
            // MarkDirtyAndDebounceSave();
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }

        public void UnlockClingy()
        {
            // MarkDirtyAndDebounceSave();
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }

        public void ShowPropertiesWindow()
        {
            // MarkDirtyAndDebounceSave();
            Console.WriteLine("METHOD NOT IMPLEMENTED");
        }
    }
}