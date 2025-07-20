using System;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;

namespace Clingies.Factories;

public class MenuFactory(IMenuRepository repo, IIconPathRepository iconRepo,
                            IClingiesLogger logger,
                            ITrayCommandProvider trayCommandProvider, 
                            Func<IContextCommandController, IContextCommandProvider> contextProviderFactory)

{
    IContextCommandProvider _contextCommandProvider;
    public NativeMenu BuildTrayMenu()
    {
        return BuildNativeTrayMenu();
    }

    public ContextMenu BuildClingyMenu(IContextCommandController controller)
    {
        _contextCommandProvider = contextProviderFactory(controller);
        return BuildContextMenu();
    }

    private ContextMenu BuildContextMenu()
    {
        var menu = new ContextMenu();

        var topLevelItems = repo.GetAllParents("clingy")
            .OrderBy(x => x.SortOrder)
            .ToList();

        foreach (var item in topLevelItems)
            if (item.Separator) menu.Items.Add(new Separator());
            else menu.Items.Add(BuildContextMenuItemRecursive(item, repo));

        return menu;
    }

    private MenuItem BuildContextMenuItemRecursive(TrayMenuItem item, IMenuRepository repo)
    {
        var children = repo.GetChildrenByParentId(item.Id)
                        .OrderBy(c => c.SortOrder)
                        .ToList();

        if (children.Count > 0)
        {
            var parent = new MenuItem
            {
                Header = item.Label ?? item.Id,
                IsEnabled = item.Enabled
            };

            var icon = LoadIcon(item.Id);
            if (icon != null)
            {
                parent.Icon = new Image
                {
                    Source = icon,
                    Width = 32,
                    Height = 32
                };
            }

            foreach (var child in children)
                parent.Items.Add(BuildContextMenuItemRecursive(child, repo));

            return parent;
        }
        else
        {
            var menuItem = new MenuItem
            {
                Header = item.Label ?? item.Id,
                IsEnabled = item.Enabled
            };

            var icon = LoadIcon(item.Id);
            if (icon != null)
            {
                menuItem.Icon = new Image
                {
                    Source = icon,
                    Width = 32,
                    Height = 32
                };
            }

            var command = ResolveContextCommand(item.Id);
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
                logger.Warning($"[ClingyMenu] No command defined for item id '{item.Id}'");
            }

            return menuItem;
        }
    }

    private NativeMenu BuildNativeTrayMenu()
    {
        var menu = new NativeMenu();

        var topLevelItems = repo.GetAllParents("tray")
            .OrderBy(x => x.SortOrder)
            .ToList();

        foreach (var item in topLevelItems)
            if (item.Separator) menu.Items.Add(new NativeMenuItemSeparator());
            else menu.Items.Add(BuildNativeTrayMenuItemRecursive(item, repo));

        return menu;

    }

    private NativeMenuItemBase BuildNativeTrayMenuItemRecursive(TrayMenuItem item, IMenuRepository repo)
    {
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
                parent.Menu.Items.Add(BuildNativeTrayMenuItemRecursive(child, repo));

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
            var command = ResolveTrayCommand(item.Id);
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

    private ICommand? ResolveTrayCommand(string itemId) => itemId switch
    {
        "new" => trayCommandProvider.NewCommand,
        "new_stack" => trayCommandProvider.NewStackCommand,
        "rolled_up" => trayCommandProvider.RolledUpCommand,
        "rolled_down" => trayCommandProvider.RolledDownCommand,
        "pinned" => trayCommandProvider.PinnedCommand,
        "unpinned" => trayCommandProvider.UnpinnedCommand,
        "locked" => trayCommandProvider.LockedCommand,
        "unlocked" => trayCommandProvider.UnlockedCommand,
        "show" => trayCommandProvider.ShowCommand,
        "hide" => trayCommandProvider.HideCommand,
        "manage_clingies" => trayCommandProvider.ManageClingiesCommand,
        "settings" => trayCommandProvider.SettingsCommand,
        "help" => trayCommandProvider.HelpCommand,
        "about" => trayCommandProvider.AboutCommand,
        "exit" => trayCommandProvider.ExitCommand,
        _ => null
    };

    private ICommand? ResolveContextCommand(string itemId) => itemId switch
    {
        "sleep" => _contextCommandProvider.SleepCommand,
        "attach" => _contextCommandProvider.AttachCommand,
        "add_to_stack" => _contextCommandProvider.BuildStackMenuCommand,
        "alarm" => _contextCommandProvider.ShowAlarmWindowCommand,
        "title" => _contextCommandProvider.ShowChangeTitleDialogCommand,
        "color" => _contextCommandProvider.ShowColorWindowCommand,
        "lock" => _contextCommandProvider.LockCommand,
        "unlock" => _contextCommandProvider.UnlockCommand,
        "properties" => _contextCommandProvider.ShowPropertiesWindowCommand,
        _ => null
    };

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
