using System;
using System.Linq;
using System.Windows.Input;
using Gtk;
using Clingies.Application.Interfaces;
using Clingies.Domain.Interfaces;
using Clingies.Domain.DTOs;
using Clingies.Application.Services;

namespace Clingies.GtkFront.Services;

public class MenuFactory(MenuService menuService,
                        IClingiesLogger logger,
                        Func<ITrayCommandProvider> trayCommandProviderFactory,
                        GtkUtilsService utils)

{
    private IContextCommandProvider? _contextCommandProvider;
    private readonly Func<ITrayCommandProvider> _trayCommandProviderFactory = trayCommandProviderFactory;

    public Menu BuildTrayMenu()
    {
        return BuildGtkTrayMenu();
    }

    public Menu BuildClingyMenu(IContextCommandProvider provider, bool isLocked, bool isRolled)
    {
        _contextCommandProvider = provider;
        return BuildContextMenu(isLocked, isRolled);
    }

    private Menu BuildContextMenu(bool isLocked, bool isRolled)
    {
        var menu = new Menu();

        var topLevelItems = menuService.GetAllParents(AppConstants.MenuOptions.ClingyMenuType)
            .OrderBy(x => x.SortOrder)
            .ToList();

        foreach (var item in topLevelItems)
        {
            item.Enabled = item.Id switch
            {
                AppConstants.ContextMenuCommands.Lock => !isLocked && !isRolled,
                AppConstants.ContextMenuCommands.Unlock => isLocked && !isRolled,
                AppConstants.ContextMenuCommands.RollUp => !isLocked && !isRolled,
                AppConstants.ContextMenuCommands.RollDown => isRolled,
                _ => !isLocked
            };

            if (item.Separator) menu.Append(new SeparatorMenuItem());
            else menu.Append(BuildContextMenuItemRecursive(item));
        }

        return menu;
    }

    private MenuItem BuildContextMenuItemRecursive(MenuItemDto item)
    {
        var children = menuService.GetChildren(item.Id)
                        .OrderBy(c => c.SortOrder)
                        .ToList();

        if (children.Count > 0)
        {
            var parent = NewMenuItemWithOptionalIcon(item.Label ?? item.Id, item.Id, utils);
            parent.Sensitive = item.Enabled;

            var sub = new Menu();
            foreach (var child in children)
                sub.Append(BuildContextMenuItemRecursive(child));

            parent.Submenu = sub;
            return parent;
        }
        else
        {
            var menuItem = NewMenuItemWithOptionalIcon(item.Label ?? item.Id, item.Id, utils);
            menuItem.Sensitive = item.Enabled;

            var command = ResolveContextCommand(item.Id);
            if (command is not null)
            {
                menuItem.Activated += (_, __) =>
                {
                    if (command.CanExecute(null))
                        command.Execute(null);
                };
            }
            else
            {
                logger.Warning($"[ClingyMenu] No command defined for item id '{item.Id}'");
            }

            return menuItem;
        }
    }

    private Menu BuildGtkTrayMenu()
    {
        var menu = new Menu();

        var topLevelItems = menuService.GetAllParents(AppConstants.MenuOptions.TrayMenuType)
            .OrderBy(x => x.SortOrder)
            .ToList();

        foreach (var item in topLevelItems)
        {
            if (item.Separator) menu.Append(new SeparatorMenuItem());
            else menu.Append(BuildGtkTrayMenuItemRecursive(item));
        }

        return menu;
    }

    private MenuItem BuildGtkTrayMenuItemRecursive(MenuItemDto item)
    {
        var children = menuService.GetChildren(item.Id)
                        .OrderBy(c => c.SortOrder)
                        .ToList();

        if (children.Count > 0)
        {
            var parent = NewMenuItemWithOptionalIcon(item.Label ?? item.Id, item.Id, utils);
            parent.Sensitive = item.Enabled;

            var sub = new Menu();
            foreach (var child in children)
                sub.Append(BuildGtkTrayMenuItemRecursive(child));

            parent.Submenu = sub;
            return parent;
        }
        else
        {
            var menuItem = NewMenuItemWithOptionalIcon(item.Label ?? item.Id, item.Id, utils);
            menuItem.Sensitive = item.Enabled;

            var command = ResolveTrayCommand(item.Id);
            if (command is not null)
            {
                menuItem.Activated += (_, __) =>
                {
                    if (command.CanExecute(null))
                        command.Execute(null);
                };
            }
            else
            {
                logger.Warning($"[TrayMenu] No command defined for item id '{item.Id}'");
            }

            return menuItem;
        }
    }

    private ICommand? ResolveTrayCommand(string itemId)
    {
        var prov = _trayCommandProviderFactory();
        return itemId switch
        {
            AppConstants.TrayMenuCommands.New => prov.NewCommand,
            AppConstants.TrayMenuCommands.RollUp => prov.RolledUpCommand,
            AppConstants.TrayMenuCommands.RollDown => prov.RolledDownCommand,
            AppConstants.TrayMenuCommands.Pin => prov.PinnedCommand,
            AppConstants.TrayMenuCommands.UnPin => prov.UnpinnedCommand,
            AppConstants.TrayMenuCommands.Lock => prov.LockedCommand,
            AppConstants.TrayMenuCommands.Unlock => prov.UnlockedCommand,
            AppConstants.TrayMenuCommands.Show => prov.ShowCommand,
            AppConstants.TrayMenuCommands.Hide => prov.HideCommand,
            AppConstants.TrayMenuCommands.ManageClingies => prov.ManageClingiesCommand,
            AppConstants.TrayMenuCommands.Settings => prov.SettingsCommand,
            AppConstants.TrayMenuCommands.Help => prov.HelpCommand,
            AppConstants.TrayMenuCommands.About => prov.AboutCommand,
            AppConstants.TrayMenuCommands.Exit => prov.ExitCommand,
            _ => null
        };
    }

    private ICommand? ResolveContextCommand(string itemId) => itemId switch
    {
        AppConstants.ContextMenuCommands.Sleep => _contextCommandProvider!.SleepCommand,
        AppConstants.ContextMenuCommands.Alarm => _contextCommandProvider!.ShowAlarmWindowCommand,
        AppConstants.ContextMenuCommands.Title => _contextCommandProvider!.ShowChangeTitleDialogCommand,
        AppConstants.ContextMenuCommands.Color => _contextCommandProvider!.ShowColorWindowCommand,
        AppConstants.ContextMenuCommands.Lock => _contextCommandProvider!.LockCommand,
        AppConstants.ContextMenuCommands.Unlock => _contextCommandProvider!.UnlockCommand,
        AppConstants.ContextMenuCommands.RollUp => _contextCommandProvider!.RollUpCommand,
        AppConstants.ContextMenuCommands.RollDown => _contextCommandProvider!.RollDownCommand,
        AppConstants.ContextMenuCommands.Properties => _contextCommandProvider!.ShowPropertiesWindowCommand,
        _ => null
    };

    private static MenuItem NewMenuItemWithOptionalIcon(string text, string id, GtkUtilsService utils)
    {
        var label = new Label(text) { Xalign = 0f, UseUnderline = true };

        var pixbuf = utils.LoadPixbuf(id, 16);
        if (pixbuf is null)
            return new MenuItem { Child = label };

        var hbox = new Box(Orientation.Horizontal, 6);
        var img = new Image(pixbuf);
        hbox.PackStart(img, false, false, 0);
        hbox.PackStart(label, true, true, 0);

        return new MenuItem { Child = hbox };
    }
}