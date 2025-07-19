using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;

namespace Clingies.Factories;

public class TrayMenuFactory(IMenuRepository repo, IIconPathRepository iconRepo,
                            IClingiesLogger logger, ITrayCommandProvider commandProvider)

{
    public NativeMenu BuildTrayMenu()
    {
        var menu = new NativeMenu();

        var topLevelItems = repo.GetAllParents()
            .OrderBy(x => x.SortOrder)
            .ToList();

        foreach (var item in topLevelItems)
            menu.Items.Add(BuildMenuItemRecursive(item, repo));

        return menu;
    }

    private NativeMenuItemBase BuildMenuItemRecursive(TrayMenuItem item, IMenuRepository repo)
    {
        if (item.Separator)
            return new NativeMenuItemSeparator();

        var children = repo.GetChildrenByParentId(item.Id)
                        .OrderBy(c => c.SortOrder)
                        .ToList();

        if (children.Count > 0)
        {
            var parent = new NativeMenuItem(item.Label ?? item.Id)
            {
                Menu = new NativeMenu(),
                IsEnabled = item.Enabled
            };

            parent.Icon = LoadIcon(item.Id);

            foreach (var child in children)
                parent.Menu.Items.Add(BuildMenuItemRecursive(child, repo));

            return parent;
        }
        else
        {
            var menuItem = new NativeMenuItem(item.Label ?? item.Id)
            {
                IsEnabled = item.Enabled,
            };

            menuItem.Icon = LoadIcon(item.Id);

            // Use closure to bind the item's ID to the click event
            var id = item.Id;
            var command = item.Id switch
            {
                "new" => commandProvider.NewCommand,
                "settings" => commandProvider.SettingsCommand,
                "exit" => commandProvider.ExitCommand,
                _ => null
            };
            if (command is not null)
            {
                menuItem.Click += (_, _) =>
                {
                    if (command.CanExecute(null))
                    {
                        command.Execute(null);
                    }
                };
            }
            else
            {
                logger.Warning($"[TrayMenu] No command defined for item id '{item.Id}'");
            }
            return menuItem;
        }
    }

    private Bitmap? LoadIcon(string iconId)
    {
        try
        {
            string? iconPath = iconRepo.GetLightPath(iconId);
            if (!string.IsNullOrEmpty(iconPath))
            {
                var uri = new Uri(iconPath, UriKind.Absolute);
                using var stream = AssetLoader.Open(uri);
                return new Bitmap(stream);
            }

            return null;
        }
        catch (Exception ex)
        {
            logger.Warning($"[TrayMenu] Could not load icon '{iconId}': {ex.Message}");
            return null;
        }
    }
}
