using System;
using Gdk;
using Gtk;
using Clingies.Gtk.Windows;
using Clingies.Gtk.Factories;
using Clingies.Domain.Interfaces;
using Clingies.Gtk.Utils;

namespace Clingies.Gtk
{
    public class GtkFrontendHost
    {
        private readonly IClingiesLogger _logger;
        private readonly IIconPathRepository _iconRepo;
        private readonly ClingyWindowFactory _windowFactory;
        private readonly MenuFactory _menuFactory;

        public GtkFrontendHost(IClingiesLogger logger, IIconPathRepository iconRepo,
                        ClingyWindowFactory windowFactory, MenuFactory menuFactory)
        {
            _logger = logger;
            _iconRepo = iconRepo;
            _windowFactory = windowFactory;
            _menuFactory = menuFactory;
        }

        public int Run()
        {
            var provider = new ClingyCssProvider();
            StyleContext.AddProviderForScreen(Screen.Default, provider, StyleProviderPriority.Application);

            DrawTrayIconAndMenu();

            _windowFactory.InitActiveWindows();
            return 0;
        }

        private void DrawTrayIconAndMenu()
        {
            var iconPath = _iconRepo.GetLightPath("clingy_icon");
            if (string.IsNullOrEmpty(iconPath))
            {
                _logger.Error(new Exception(), "Could not find main tray icon in DB");
                return;
            }

            var trayMenu = _menuFactory.BuildTrayMenu();

            try
            {
                // If you provide a *file path*, some shells expect a theme name; set a simple icon name first,
                // then immediately set the full path:
                using var ind = new AppIndicator("clingies", iconPath,
                    AppIndicatorNative.AppIndicatorCategory.ApplicationStatus);

                ind.SetIcon(iconPath);
                ind.SetTitle("Clingies");
                ind.SetMenu(trayMenu);
                ind.SetStatus(AppIndicatorNative.AppIndicatorStatus.Active);

                // AppIndicator doesn’t raise a “clicked” event; mimic Avalonia’s behavior by
                // putting a top menu item like “Show All” OR add a global accelerator.
                // If you want a left-click action specifically, keep a fallback StatusIcon for Cinnamon:

                AttachOptionalStatusIconFallback(iconPath, trayMenu);
            }
            catch (DllNotFoundException)
            {
                _logger.Warning("libayatana-appindicator3 is missing; falling back to Gtk.StatusIcon.");
                AttachOptionalStatusIconFallback(iconPath, trayMenu);
            }
        }

        void AttachOptionalStatusIconFallback(string iconPath, Menu trayMenu)
        {
            #pragma warning disable CS0612
            var si = new StatusIcon(new Gdk.Pixbuf(iconPath, 24, 24))
            {
                Visible = true,

                //Tooltip = "Clingies"
            };
            si.Activate += (_, __) => _windowFactory.RenderAllWindows();
            si.PopupMenu += (_, __) => { trayMenu.ShowAll(); trayMenu.Popup(); };
            #pragma warning restore CS0618
        }

        // ITrayCommandController methods (direct port from Avalonia App.axaml.cs)
        // public void CreateNewClingy()
        // {
        //     var defW = 300; var defH = 100;
        //     var (x, y, w, h) = GetDesktopWorkingArea();
        //     var cx = x + (w - defW) / 2;
        //     var cy = y + (h - defH) / 2;
        //     windowFactory.CreateNewWindow(cx, cy);
        // }

        public void ShowSettings() => Console.WriteLine("SETTINGS NOT IMPLEMENTED");
        public void ExitApp() => Application.Quit();
        public void CreateNewStack() => Console.WriteLine("NEW STACK NOT IMPLEMENTED");
        public void RollUpAllClingies() => Console.WriteLine("ROLL UP ALL NOT IMPLEMENTED");
        public void RollDownAllClingies() => Console.WriteLine("ROLL DOWN ALL NOT IMPLEMENTED");
        public void PinAllClingies() => Console.WriteLine("PIN ALL NOT IMPLEMENTED");
        public void UnpinAllClingies() => Console.WriteLine("UNPIN ALL NOT IMPLEMENTED");
        public void LockAllClingies() => Console.WriteLine("LOCK ALL NOT IMPLEMENTED");
        public void UnlockAllClingies() => Console.WriteLine("UNLOCK ALL NOT IMPLEMENTED");
        public void ShowAllClingies() => Console.WriteLine("SHOW ALL NOT IMPLEMENTED");
        public void HideAllClingies() => Console.WriteLine("HIDE ALL NOT IMPLEMENTED");
        public void ShowManageClingiesWindow() => Console.WriteLine("MANAGE CLINGIES NOT IMPLEMENTED");
        public void ShowHelpWindow() => Console.WriteLine("HELP WINDOW NOT IMPLEMENTED");
        public void ShowAboutWindow() => Console.WriteLine("ABOUT WINDOW NOT IMPLEMENTED");

        // private static (int X, int Y, int Width, int Height) GetDesktopWorkingArea()
        // {
        //     var display = Display.Default;
        //     var monitor = display?.GetPrimaryMonitor() ?? display?.GetMonitor(0);
        //     monitor.GetGeometry(out var rect);
        //     return (rect.X, rect.Y, rect.Width, rect.Height);
        // }
    }
}