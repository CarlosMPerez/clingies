using System.Collections.Generic;
using System.Linq;
using Clingies.AppMenu;
using Clingies.Application.Interfaces;
using Clingies.Application.Services;
using Gtk;
using GtkMenu = Gtk.Menu;
using SysAction = System.Action;

namespace Clingies.Services;

public class AppMenuFactory(StyleService styleService,
                            IClingiesLogger logger,
                            GtkUtilsService utils)

{
    public GtkMenu BuildTrayMenu(TrayCommandController trayController)
    {
        return BuildGtkTrayMenu(trayController);
    }

    public GtkMenu BuildClingyMenu(ClingyContextController controller, bool isLocked, bool isRolled)
    {
        return BuildContextMenu(controller, isLocked, isRolled);
    }

    private GtkMenu BuildContextMenu(ClingyContextController controller, bool isLocked, bool isRolled)
    {
        var menu = new GtkMenu();

        var topLevelItems = AppMenuDefinitions.ClingyMenu;

        foreach (var item in topLevelItems)
        {
            var enabled = item.Id switch
            {
                AppConstants.ContextMenuCommands.Lock => !isLocked && !isRolled,
                AppConstants.ContextMenuCommands.Unlock => isLocked && !isRolled,
                AppConstants.ContextMenuCommands.RollUp => !isLocked && !isRolled,
                AppConstants.ContextMenuCommands.RollDown => isRolled,
                _ => !isLocked
            };

            if (item.Separator) menu.Append(new SeparatorMenuItem());
            else 
            {
                if (item.Id == AppConstants.ContextMenuCommands.SetStyle)
                    menu.Append(BuildAvailableStylesMenu(item, controller, enabled));
                else
                    menu.Append(BuildContextMenuItemRecursive(item, controller, enabled));
            }
        }

        return menu;
    }

    private MenuItem BuildContextMenuItemRecursive(AppMenuDefinition model, ClingyContextController controller, bool enabledOverride = true)
    {
        MenuItem ret;
        var children = model.Children ?? [];

        if (children.Count > 0) ret = BuildParentContextMenuItem(model, children, controller);
        else ret = BuildNonParentContextMenuItem(model, controller, enabledOverride);

        return ret;
    }

    private MenuItem BuildParentContextMenuItem(AppMenuDefinition model, IReadOnlyList<AppMenuDefinition> children, ClingyContextController controller)
    {
        var parent = NewMenuItemWithOptionalIcon(model.Label ?? model.Id, model.IconId, utils);
        parent.Sensitive = model.Enabled;

        var sub = new GtkMenu();
        foreach (var child in children)
            sub.Append(BuildContextMenuItemRecursive(child, controller));

        parent.Submenu = sub;
        return parent;
    }

    private MenuItem BuildNonParentContextMenuItem(AppMenuDefinition model, ClingyContextController controller, bool enabledOverride = true)
    {
        var menuItem = NewMenuItemWithOptionalIcon(model.Label ?? model.Id, model.IconId, utils);
        menuItem.Sensitive = model.Enabled && enabledOverride;

        var action = ResolveContextAction(model.Id, controller);
        if (action is not null)
        {
            menuItem.Activated += (_, __) => action();
        }
        else
        {
            logger.Warning($"[ClingyMenu] No command defined for item id '{model.Id}'");
        }

        return menuItem;
    }

    private MenuItem BuildAvailableStylesMenu(AppMenuDefinition model, ClingyContextController controller, bool enabledOverride = true)
    {
        var styles = styleService.GetAllActive().OrderBy(c => c.StyleName).ToList();
        var parent = NewMenuItemWithOptionalIcon(model.Label ?? model.Id, model.IconId, utils);
        parent.Sensitive = model.Enabled && enabledOverride;

        if (styles.Count == 0)
            return parent;

        var sub = new GtkMenu();
        foreach (var style in styles)
        {
            var menuItem = NewMenuItemWithOptionalIcon(style.StyleName, null, utils);
            menuItem.Activated += (_, __) => controller.ApplyStyle(style.Id);
            sub.Append(menuItem);
        }

        parent.Submenu = sub;
        return parent;
    }

    private GtkMenu BuildGtkTrayMenu(TrayCommandController trayController)
    {
        var menu = new GtkMenu();

        var topLevelItems = AppMenuDefinitions.TrayMenu;

        foreach (var item in topLevelItems)
        {
            if (item.Separator) menu.Append(new SeparatorMenuItem());
            else menu.Append(BuildGtkTrayMenuItemRecursive(item, trayController));
        }

        return menu;
    }

    private MenuItem BuildGtkTrayMenuItemRecursive(AppMenuDefinition item, TrayCommandController trayController)
    {
        var children = item.Children ?? [];

        if (children.Count > 0)
        {
            var parent = NewMenuItemWithOptionalIcon(item.Label ?? item.Id, item.IconId, utils);
            parent.Sensitive = item.Enabled;

            var sub = new GtkMenu();
            foreach (var child in children)
                sub.Append(BuildGtkTrayMenuItemRecursive(child, trayController));

            parent.Submenu = sub;
            return parent;
        }
        else
        {
            var menuItem = NewMenuItemWithOptionalIcon(item.Label ?? item.Id, item.IconId, utils);
            menuItem.Sensitive = item.Enabled;

            var action = ResolveTrayAction(item.Id, trayController);
            if (action is not null)
            {
                menuItem.Activated += (_, __) => action();
            }
            else
            {
                logger.Warning($"[TrayMenu] No command defined for item id '{item.Id}'");
            }

            return menuItem;
        }
    }

    private SysAction? ResolveTrayAction(string itemId, TrayCommandController trayController)
    {
        return itemId switch
        {
            AppConstants.TrayMenuCommands.New => trayController.CreateNewClingy,
            AppConstants.TrayMenuCommands.RollUp => trayController.RollUpAllClingies,
            AppConstants.TrayMenuCommands.RollDown => trayController.RollDownAllClingies,
            AppConstants.TrayMenuCommands.Pin => trayController.PinAllClingies,
            AppConstants.TrayMenuCommands.UnPin => trayController.UnpinAllClingies,
            AppConstants.TrayMenuCommands.Lock => trayController.LockAllClingies,
            AppConstants.TrayMenuCommands.Unlock => trayController.UnlockAllClingies,
            AppConstants.TrayMenuCommands.Show => trayController.ShowAllClingies,
            AppConstants.TrayMenuCommands.Hide => trayController.HideAllClingies,
            AppConstants.TrayMenuCommands.ManageClingies => trayController.ShowManageClingiesWindow,
            AppConstants.TrayMenuCommands.Settings => trayController.ShowSettings,
            AppConstants.TrayMenuCommands.Help => trayController.ShowHelpWindow,
            AppConstants.TrayMenuCommands.About => trayController.ShowAboutWindow,
            AppConstants.TrayMenuCommands.Exit => trayController.ExitApp,
            AppConstants.TrayMenuCommands.StyleManager => trayController.ShowStyleManager,
            _ => null
        };
    }

    private static SysAction? ResolveContextAction(string itemId, ClingyContextController controller) => itemId switch
    {
        AppConstants.ContextMenuCommands.Sleep => controller.SleepClingy,
        AppConstants.ContextMenuCommands.Alarm => controller.ShowAlarmWindow,
        AppConstants.ContextMenuCommands.Title => controller.ShowChangeTitleDialog,
        AppConstants.ContextMenuCommands.Color => controller.ShowColorWindow,
        AppConstants.ContextMenuCommands.Lock => controller.LockClingy,
        AppConstants.ContextMenuCommands.Unlock => controller.UnlockClingy,
        AppConstants.ContextMenuCommands.RollUp => controller.RollUpClingy,
        AppConstants.ContextMenuCommands.RollDown => controller.RollDownClingy,
        AppConstants.ContextMenuCommands.Properties => controller.ShowPropertiesWindow,
        _ => null
    };

    private static MenuItem NewMenuItemWithOptionalIcon(string text, string? iconId, GtkUtilsService utils)
    {
        var label = new Label(text) { Xalign = 0f, UseUnderline = true };

        if (string.IsNullOrWhiteSpace(iconId))
            return new MenuItem { Child = label };

        var pixbuf = utils.LoadPixbuf(iconId, 16);
        if (pixbuf is null)
            return new MenuItem { Child = label };

        var hbox = new Box(Orientation.Horizontal, 6);
        var img = new Image(pixbuf);
        hbox.PackStart(img, false, false, 0);
        hbox.PackStart(label, true, true, 0);

        return new MenuItem { Child = hbox };
    }
}
