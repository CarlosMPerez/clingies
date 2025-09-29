
namespace Clingies.Utils;

/// <summary>
/// Application-wide constants to avoid magic numbers and strings.
/// </summary>
public static class AppConstants
{
    public static class IconNames
    {
        public const string ClingyPinned = "clingy_pinned.png";
        public const string ClingyUnpinned = "clingy_unpinned.png";
        public const string ClingyLocked = "clingy_locked.png";
        public const string ClingyUnlocked = "clingy_unlocked.png";
        public const string ClingyClose = "clingy_close.png";
        public const string Application = "clingy_icon";
    }

    public static class MenuOptions
    {
        public const string ClingyMenuType = "clingy";
        public const string TrayMenuType = "tray";
    }

    public static class TrayMenuCommands
    {
        public const string New = "new";
        public const string RollUp = "rolled_up";
        public const string RollDown = "rolled_down";
        public const string Pin = "pinned";
        public const string UnPin = "unpinned";
        public const string Lock = "locked";
        public const string Unlock = "unlocked";
        public const string Show = "show";
        public const string Hide = "hide";
        public const string ManageClingies = "manage_clingies";
        public const string Settings = "settings";
        public const string Help = "help";
        public const string About = "about";
        public const string Exit = "exit";
        public const string StyleManager = "style_manager";
    }

    public static class ContextMenuCommands
    {
        public const string Sleep = "sleep";
        public const string Alarm = "alarm";
        public const string Title = "title";
        public const string Color = "color";
        public const string Lock = "lock";
        public const string Unlock = "unlock";
        public const string RollUp = "rollup";
        public const string RollDown = "rolldown";
        public const string Properties = "properties";
        public const string StyleManager = "style_manager";
    }

    public static class CssSections
    {
        public const string ClingyWindow = "clingy-window";
        public const string ClingyTitle = "clingy-title";
        public const string ClingyContent = "clingy-content";
        public const string ClingyTitleLabel = "clingy-title-label";
        public const string ClingyContentView = "clingy-content-view";
        public const string ButtonPin = "btn-pin";
        public const string ButtonLock = "btn-lock";
        public const string ButtonClose = "btn-close";
        public const string Focused = "focused";
    }

    public static class Colors
    {
        public const string TitleBarFocused = "#10cce0";
        public const string TitleBarUnfocused = "#b1b4b5";
    }

    public static class Dimensions
    {
        public const int DefaultClingyWidth = 380;
        public const int DefaultClingyHeight = 80;

        public const int DefaultDialogWidth = 380;
        public const int DefaultDialogHeight = 60;

        public const int TitleHeight = 20;
    }

    public static class SystemStyle
    {
        public const string Name = "System";
        public const string BodyColor = "#FFFFB8";
        public const string BodyFontName = "monospace";
        public const string BodyFontColor = "#000000";
        public const int BodyFontSize = 14;
        public const string BodyFontDecorations = "0000";
    }
}
