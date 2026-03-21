using System.Collections.Generic;

namespace Clingies.AppMenu;

internal static class AppMenuDefinitions
{
    public static readonly IReadOnlyList<AppMenuDefinition> TrayMenu =
    [
        new(AppConstants.TrayMenuCommands.New, "New", "Create a new Clingy", AppConstants.TrayMenuCommands.New),
        new(AppConstants.TrayMenuCommands.Show, "Show all", "Show all Clingies", AppConstants.TrayMenuCommands.Show),
        new("sep001", Separator: true),
        new(
            "set_all",
            "Set all",
            "Change state to all Clingies",
            Children:
            [
                new(AppConstants.TrayMenuCommands.RollUp, "Rolled Up", "Roll up all Clingies", AppConstants.TrayMenuCommands.RollUp),
                new(AppConstants.TrayMenuCommands.RollDown, "Rolled Down", "Roll down all Clingies", AppConstants.TrayMenuCommands.RollDown),
                new(AppConstants.TrayMenuCommands.Pin, "Pinned", "Pin all Clingies", AppConstants.TrayMenuCommands.Pin),
                new(AppConstants.TrayMenuCommands.UnPin, "Unpinned", "Unpin all Clingies", AppConstants.TrayMenuCommands.UnPin),
                new(AppConstants.TrayMenuCommands.Lock, "Locked", "Lock all Clingies", AppConstants.TrayMenuCommands.Lock),
                new(AppConstants.TrayMenuCommands.Unlock, "Unlocked", "Unlock all Clingies", AppConstants.TrayMenuCommands.Unlock)
            ]),
        new("sep002", Separator: true),
        new(AppConstants.TrayMenuCommands.ManageClingies, "Manage Clingies...", "Manage Clingies", AppConstants.TrayMenuCommands.ManageClingies),
        new(AppConstants.TrayMenuCommands.Settings, "Settings...", "Access the Settings window", AppConstants.TrayMenuCommands.Settings),
        new(AppConstants.TrayMenuCommands.StyleManager, "Style Manager...", "Manage Clingy styles", AppConstants.TrayMenuCommands.StyleManager),
        new("sep003", Separator: true),
        new(AppConstants.TrayMenuCommands.Help, "Clingies help...", "Clingies Help", AppConstants.TrayMenuCommands.Help),
        new(AppConstants.TrayMenuCommands.About, "About Clingies...", "About the Application", AppConstants.TrayMenuCommands.About),
        new("sep004", Separator: true),
        new(AppConstants.TrayMenuCommands.Exit, "Exit", "Close the application", AppConstants.TrayMenuCommands.Exit)
    ];

    public static readonly IReadOnlyList<AppMenuDefinition> ClingyMenu =
    [
        new(AppConstants.ContextMenuCommands.Sleep, "Sleep...", "Sleep the Clingy for a determined interval", AppConstants.ContextMenuCommands.Sleep),
        new(AppConstants.ContextMenuCommands.Alarm, "Alarm...", "Set an Alarm on the Clingy", AppConstants.ContextMenuCommands.Alarm),
        new(AppConstants.ContextMenuCommands.Title, "Set Title...\t\tShift+Ctrl+T", "Changes the Title on the Clingy", AppConstants.ContextMenuCommands.Title),
        new(AppConstants.ContextMenuCommands.Lock, "Lock", "Locks the Clingy contents", AppConstants.ContextMenuCommands.Lock),
        new(AppConstants.ContextMenuCommands.Unlock, "Unlock", "Unlocks the Clingy contents", AppConstants.ContextMenuCommands.Unlock),
        new(AppConstants.ContextMenuCommands.RollUp, "Roll Up", "Rolls up the Clingy content", AppConstants.ContextMenuCommands.RollUp),
        new(AppConstants.ContextMenuCommands.RollDown, "Roll Down", "Rolls down the Clingy content", AppConstants.ContextMenuCommands.RollDown),
        new(AppConstants.ContextMenuCommands.SetStyle, "Set to Style", "Sets the Clingy Style", AppConstants.ContextMenuCommands.SetStyle)
    ];
}
