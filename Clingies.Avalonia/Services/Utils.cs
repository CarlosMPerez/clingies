// using System;
// using Avalonia.Controls;
// using Avalonia.Media.Imaging;
// using Avalonia.Platform;
// using Clingies.Domain.Interfaces;

// namespace Clingies.Avalonia.Services;

// public class UtilsService
// {
//     private readonly IClingiesLogger _logger;
//     private readonly IIconPathRepository _iconRepo;
//     public UtilsService(IClingiesLogger logger, IIconPathRepository iconRepo)
//     {
//         _logger = logger;
//         _iconRepo = iconRepo;
//     }

//     public Bitmap? LoadBitmap(string iconId)
//     {
//         try
//         {
//             string? iconPath = _iconRepo.GetLightPath(iconId);
//             if (!string.IsNullOrEmpty(iconPath))
//             {
//                 var uri = new Uri(iconPath, UriKind.Absolute);
//                 using var stream = AssetLoader.Open(uri);
//                 return new Bitmap(stream);
//             }

//             return null;
//         }
//         catch (Exception ex)
//         {
//             _logger.Warning($"[LoadIcon] Could not load icon '{iconId}': {ex.Message}");
//             return null;
//         }
//     }

//     public Image? LoadImage(string iconId)
//     {
//         try
//         {
//             var icon = LoadBitmap(iconId);
//             if (icon != null)
//             {
//                 return new Image
//                 {
//                     Source = icon,
//                     Width = 32,
//                     Height = 32
//                 };
//             }

//             return null;
//         }
//         catch (Exception ex)
//         {
//             _logger.Warning($"[LoadImage] Could not load icon '{iconId}': {ex.Message}");
//             return null;
//         }
//     }
// }
