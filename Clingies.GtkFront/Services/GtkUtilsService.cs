using System;
using System.IO;
using Clingies.Application.Interfaces;
using Gdk;
using Gtk;

namespace Clingies.GtkFront.Services
{
    public class GtkUtilsService
    {
        private readonly IClingiesLogger _logger;
        private readonly IIconPathRepository _iconRepo;
        public GtkUtilsService(IClingiesLogger logger, IIconPathRepository iconRepo)
        {
            _logger = logger;
            _iconRepo = iconRepo;
        }

        public string? LoadPathFromDb(string iconId, bool isDarkPath = false)
        {
            string? assetPath = isDarkPath ? _iconRepo.GetDarkPath(iconId) : _iconRepo.GetLightPath(iconId);
            if (string.IsNullOrEmpty(assetPath))
            {
                _logger.Warning($"{iconId} not found in database");
                return null;
            }

            var path = Path.Combine(AppContext.BaseDirectory, assetPath!);
            if (!File.Exists(path))
            {
                _logger.Warning($"Image asset not found: {path}");
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

        public Image? CreateImageFromDb(string iconId, int size, bool isDarkPath = false)
        {
            string? path = LoadPathFromDb(iconId, isDarkPath);
            if (string.IsNullOrEmpty(path)) return null;
            var pixbuf = new Pixbuf(path, size, size, true); // consider caching or scaling if you ship multiple DPI sizes
            return new Image(pixbuf);
        }

        // Map your menu item id -> pixbuf (file path, resource, or theme icon)
        public Pixbuf? LoadPixbuf(string iconId, int size, bool isDarkPath = false)
        {
            string? path = LoadPathFromDb(iconId, isDarkPath);
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

        public Button MakeImgButton(string name, string asset, EventHandler? onClick = null)
        {
            string path = Path.Combine(AppContext.BaseDirectory, $"Assets/{asset}");
            var btn = new Button
            {
                Name = name,
                Relief = ReliefStyle.None,
                CanFocus = false,
                Image = CreateImageFromPath(path, 12)
            };
            btn.SetSizeRequest(16, 16);
            if (onClick != null) btn.Clicked += onClick;
            return btn;
        }

        public void SetButtonIcon(Button btn, string assetName, int size = 12)
        {
            string assetPath = Path.Combine(AppContext.BaseDirectory, $"Assets/{assetName}");
            btn.Image = CreateImageFromPath(assetPath, size);
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