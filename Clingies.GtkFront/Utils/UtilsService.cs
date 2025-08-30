using System;
using System.IO;
using Clingies.Domain.Interfaces;
using Gdk;
using Gtk;
using Microsoft.IdentityModel.Tokens;

namespace Clingies.GtkFront.Utils
{
    public class UtilsService
    {
        private readonly IClingiesLogger _logger;
        private readonly IIconPathRepository _iconRepo;
        public UtilsService(IClingiesLogger logger, IIconPathRepository iconRepo)
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
            if (fullPath.IsNullOrEmpty()) return null;
            var pixbuf = new Pixbuf(fullPath, size, size, true); // consider caching or scaling if you ship multiple DPI sizes
            return new Image(pixbuf);
        }

        public Image? CreateImageFromDb(string iconId, int size, bool isDarkPath = false)
        {
            string? path = LoadPathFromDb(iconId, isDarkPath);
            if (path.IsNullOrEmpty()) return null;
            var pixbuf = new Pixbuf(path, size, size, true); // consider caching or scaling if you ship multiple DPI sizes
            return new Image(pixbuf);
        }

        // Map your menu item id -> pixbuf (file path, resource, or theme icon)
        public Pixbuf? LoadPixbuf(string iconId, int size, bool isDarkPath = false)
        {
            string? path = LoadPathFromDb(iconId, isDarkPath);
            if (path.IsNullOrEmpty()) return null;
            return TryLoad(path!, size, size);
        }

        private static Pixbuf? TryLoad(string path, int w, int h)
        {
            try { return new Pixbuf(path, w, h); }
            catch { return null; }
        }
    }
}