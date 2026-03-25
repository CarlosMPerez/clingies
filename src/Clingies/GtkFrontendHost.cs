using System;
using Gdk;
using Gtk;
using Clingies.Windows;
using Clingies.Services;
using Clingies.Application.Interfaces;
using Clingies.Utils;

namespace Clingies
{
    public class GtkFrontendHost(IClingiesLogger logger,
                    ClingyWindowManager windowFactory,
                    TrayCommandController trayCommandController,
                    AppMenuFactory menuFactory,
                    GtkUtilsService utilsService)
    {
        public int Run()
        {
            var baseProvider = new GlobalClingyBaseCssProvider();
            StyleContext.AddProviderForScreen(Screen.Default, baseProvider, StyleProviderPriority.Application);

            DrawTrayIconAndMenu();

            windowFactory.InitActiveWindows();
            return 0;
        }

        private void DrawTrayIconAndMenu()
        {
            var iconPath = utilsService.LoadIconPath(AppConstants.IconNames.Application);
            if (string.IsNullOrEmpty(iconPath))
            {
                logger.Error(new Exception(), "Could not find main tray icon in resources");
                return;
            }

            var trayMenu = menuFactory.BuildTrayMenu(trayCommandController);
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
            }
            catch (DllNotFoundException)
            {
                logger.Warning("libayatana-appindicator3 is missing; falling back to Gtk.StatusIcon.");
                AttachOptionalStatusIconFallback(iconPath, trayMenu);
            }
        }

        void AttachOptionalStatusIconFallback(string iconPath, Menu trayMenu)
        {
#pragma warning disable CS0612
            var si = new StatusIcon(new Gdk.Pixbuf(iconPath, 24, 24))
            {
                Visible = true,
                TooltipText = "Clingies"
            };
            si.Activate += (_, __) => windowFactory.RenderAllWindows();
            si.PopupMenu += (_, __) => { trayMenu.ShowAll(); trayMenu.Popup(); };
        }
#pragma warning restore CS0612
    }
}
