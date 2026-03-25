using System;
using System.IO;
using Clingies.Application.Interfaces;
using Clingies.Resources;
using Gdk;
using Gtk;

namespace Clingies.Services
{
    public class GtkUtilsService(IClingiesLogger logger)
    {
        public string? LoadIconPath(string iconId, bool isDarkPath = false)
        {
            string? assetPath = IconPathResources.GetPath(iconId, isDarkPath);
            if (string.IsNullOrEmpty(assetPath))
            {
                logger.Warning($"{iconId} not found in resources");
                return null;
            }

            var path = Path.Combine(AppContext.BaseDirectory, assetPath!);
            if (!File.Exists(path))
            {
                logger.Warning($"Image asset not found: {path}");
                return null;
            }

            return path;
        }

        public Image? CreateImageFromPath(string fullPath, int size)
        {
            if (string.IsNullOrEmpty(fullPath)) return null;
            var pixbuf = new Pixbuf(fullPath, size, size, true); // consider caching or scaling if you ship multiple DPI sizes
            return new Image(pixbuf);
        }

        public Pixbuf? LoadPixbuf(string iconId, int size, bool isDarkPath = false)
        {
            string? path = LoadIconPath(iconId, isDarkPath);
            if (string.IsNullOrEmpty(path)) return null;
            return TryLoad(path!, size, size);
        }

        private static Pixbuf? TryLoad(string path, int w, int h)
        {
            try { return new Pixbuf(path, w, h); }
            catch { return null; }
        }

        public bool IsX11()
        {
            return Display.Default?
                    .GetType().FullName?
                    .Contains("X11", StringComparison.OrdinalIgnoreCase) == true;
        }

        public Button MakeImgButton(string name, string iconId, EventHandler? onClick = null)
        {
            var btn = new Button
            {
                Name = name,
                Relief = ReliefStyle.None,
                CanFocus = false,
                Image = CreateImageFromPath(LoadIconPath(iconId) ?? string.Empty, 12)
            };
            btn.SetSizeRequest(16, 16);
            if (onClick != null) btn.Clicked += onClick;
            return btn;
        }

        public void SetButtonIcon(Button btn, string iconId, int size = 12)
        {
            btn.Image = CreateImageFromPath(LoadIconPath(iconId) ?? string.Empty, size);
            btn.ShowAll();
        }

        public Point GetCenterPointDefaultMonitor(int windowWidth, int windowHeight)
        {
            // center in primary monitor
            var monitor = Display.Default.GetMonitor(0);
            var geom = monitor?.Geometry ?? new Rectangle { X = 0, Y = 0, Width = 800, Height = 600 };

            int centerX = geom.X + (geom.Width - windowWidth) / 2;
            int centerY = geom.Y + (geom.Height - windowHeight) / 2;
            return new Point() { X = centerX, Y = centerY };
        }
    }
}
