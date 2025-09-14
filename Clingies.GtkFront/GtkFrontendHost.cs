using System;
using Gdk;
using Gtk;
using Clingies.GtkFront.Windows;
using Clingies.GtkFront.Services;
using Clingies.Domain.Interfaces;
using Clingies.GtkFront.Utils;

namespace Clingies.GtkFront
{
    public class GtkFrontendHost
    {
        private readonly IClingiesLogger _logger;
        private readonly IIconPathRepository _iconRepo;
        private readonly ClingyWindowManager _windowFactory;
        private readonly MenuFactory _menuFactory;

        private readonly GtkUtilsService _srvUtils;

        public GtkFrontendHost(IClingiesLogger logger, IIconPathRepository iconRepo,
                        ClingyWindowManager windowFactory, MenuFactory menuFactory,
                        GtkUtilsService utilsService)
        {
            _logger = logger;
            _iconRepo = iconRepo;
            _windowFactory = windowFactory;
            _menuFactory = menuFactory;
            _srvUtils = utilsService;
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
            var iconPath = _srvUtils.LoadPathFromDb(AppConstants.IconNames.Application, false);
            if (string.IsNullOrEmpty(iconPath))
            {
                _logger.Error(new Exception(), "Could not find main tray icon in DB");
                return;
            }

            var trayMenu = _menuFactory.BuildTrayMenu();
            trayMenu.ShowAll();

            try
            {
                // If you provide a *file path*, some shells expect a theme name; set a simple icon name first,
                // then immediately set the full path:
                using var ind = new AppIndicator("clingies", iconPath,
                    Enums.AppIndicatorCategory.ApplicationStatus);

                ind.SetIcon(iconPath);
                ind.SetTitle("Clingies");
                ind.SetMenu(trayMenu);
                ind.SetStatus(Enums.AppIndicatorStatus.Active);

                // AppIndicator doesn’t raise a “clicked” event; mimic Avalonia’s behavior by
                // putting a top menu item like “Show All” OR add a global accelerator.
                // If you want a left-click action specifically, keep a fallback StatusIcon for Cinnamon:

                //AttachOptionalStatusIconFallback(iconPath, trayMenu);
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
        }

    }
}