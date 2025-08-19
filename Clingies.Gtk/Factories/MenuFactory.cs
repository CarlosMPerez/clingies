using System;
using System.Linq;
using System.Windows.Input;
using Gtk;
using Clingies.ApplicationLogic.Interfaces;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;
using Clingies.Gtk.Utils;

namespace Clingies.Gtk.Factories;
// TODO : Implement MenuService and MenuDtos : FRONT END SHOULD NOT TALK TO DB!!!!
public class MenuFactory(IMenuRepository repo,
                            IClingiesLogger logger,
                            ITrayCommandProvider trayCommandProvider,
                            Func<IContextCommandController, IContextCommandProvider> contextProviderFactory,
                            UtilsService utils)

{
    private IContextCommandProvider? _contextCommandProvider;
    public Menu BuildTrayMenu()
    {
        return BuildGtkTrayMenu();
    }

    public Menu BuildClingyMenu(IContextCommandController controller)
    {
        _contextCommandProvider = contextProviderFactory(controller);
        return BuildContextMenu();
    }

    private Menu BuildContextMenu()
    {
        var menu = new Menu();

        var topLevelItems = repo.GetAllParents("clingy")
            .OrderBy(x => x.SortOrder)
            .ToList();

        foreach (var item in topLevelItems)
            if (item.Separator) menu.Append(new SeparatorMenuItem());
            else menu.Append(BuildContextMenuItemRecursive(item, repo));

        return menu;
    }

    private MenuItem BuildContextMenuItemRecursive(TrayMenuItem item, IMenuRepository repo)
    {
        var children = repo.GetChildrenByParentId(item.Id)
                        .OrderBy(c => c.SortOrder)
                        .ToList();

        if (children.Count > 0)
        {
            var parent = NewMenuItemWithOptionalIcon(item.Label ?? item.Id, item.Id, utils);
            parent.Sensitive = item.Enabled;

            var sub = new Menu();
            foreach (var child in children)
                sub.Append(BuildContextMenuItemRecursive(child, repo));

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

        var topLevelItems = repo.GetAllParents("tray")
            .OrderBy(x => x.SortOrder)
            .ToList();

        foreach (var item in topLevelItems)
        {
            if (item.Separator) menu.Append(new SeparatorMenuItem());
            else menu.Append(BuildGtkTrayMenuItemRecursive(item, repo));
        }

        return menu;
    }

    private MenuItem BuildGtkTrayMenuItemRecursive(TrayMenuItem item, IMenuRepository repo)
    {
        var children = repo.GetChildrenByParentId(item.Id)
                        .OrderBy(c => c.SortOrder)
                        .ToList();

        if (children.Count > 0)
        {
            var parent = NewMenuItemWithOptionalIcon(item.Label ?? item.Id, item.Id, utils);
            parent.Sensitive = item.Enabled;

            var sub = new Menu();
            foreach (var child in children)
                sub.Append(BuildGtkTrayMenuItemRecursive(child, repo));

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

    private ICommand? ResolveTrayCommand(string itemId) => itemId switch
    {
        "new" => trayCommandProvider.NewCommand,
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
        "sleep" => _contextCommandProvider!.SleepCommand,
        "alarm" => _contextCommandProvider!.ShowAlarmWindowCommand,
        "title" => _contextCommandProvider!.ShowChangeTitleDialogCommand,
        "color" => _contextCommandProvider!.ShowColorWindowCommand,
        "lock" => _contextCommandProvider!.LockCommand,
        "unlock" => _contextCommandProvider!.UnlockCommand,
        "properties" => _contextCommandProvider!.ShowPropertiesWindowCommand,
        _ => null
    };
    
    private static MenuItem NewMenuItemWithOptionalIcon(string text, string id, UtilsService utils)
    {
        // Plain label first (keeps mnemonics/ellipsizing sane)
        var label = new Label(text) { Xalign = 0f, UseUnderline = true };

        // If you have an icon for this id, compose a row: [img][label]
        var pixbuf = utils.LoadPixbuf(id, 16, true); // <- implement in your GTK UtilsService (returns Gdk.Pixbuf? or null)
        if (pixbuf is null)
            return new MenuItem { Child = label };

        var hbox = new Box(Orientation.Horizontal, 6);
        var img = new Image(pixbuf);
        hbox.PackStart(img, false, false, 0);
        hbox.PackStart(label, true, true, 0);

        return new MenuItem { Child = hbox };
    }    
}