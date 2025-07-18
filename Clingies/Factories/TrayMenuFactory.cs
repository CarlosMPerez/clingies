using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;

namespace Clingies.Factories;

public class TrayMenuFactory(IMenuRepository repo, IClingiesLogger logger, ITrayCommandProvider commandProvider)

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

            if (!string.IsNullOrWhiteSpace(item.Icon))
                parent.Icon = LoadIcon(item.Icon);

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

            if (!string.IsNullOrWhiteSpace(item.Icon))
                menuItem.Icon = LoadIcon(item.Icon);

            // Use closure to bind the item's ID to the click event
            var id = item.Id;
            menuItem.Command = item.Id switch
            {
                "new" => commandProvider.NewCommand,
                "settings" => commandProvider.SettingsCommand,
                "exit" => commandProvider.ExitCommand,
                _ => null
            };
            if (menuItem.Command is not null)
            {
                menuItem.Click += (_, _) =>
                {
                    if (menuItem.Command.CanExecute(null))
                        menuItem.Command.Execute(null);
                };
            }
            else
            {
                logger.Warning($"[TrayMenu] No command defined for item id '{item.Id}'");
            }            
            return menuItem;
        }
    }

    private Bitmap? LoadIcon(string iconPath)
    {
        try
        {
            var uri = new Uri(iconPath, UriKind.Absolute);
            using var stream = AssetLoader.Open(uri);
            return new Bitmap(stream);
        }
        catch (Exception ex)
        {
            logger.Warning($"[TrayMenu] Could not load icon '{iconPath}': {ex.Message}");
            return null;
        }
    }
}
