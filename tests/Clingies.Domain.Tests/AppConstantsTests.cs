namespace Clingies.Domain.Tests;

public class AppConstantsTests
{
    [Fact]
    public void Dimensions_AreStableForDefaultUiLayout()
    {
        Assert.Equal(380, AppConstants.Dimensions.DefaultClingyWidth);
        Assert.Equal(80, AppConstants.Dimensions.DefaultClingyHeight);
        Assert.Equal(380, AppConstants.Dimensions.DefaultDialogWidth);
        Assert.Equal(60, AppConstants.Dimensions.DefaultDialogHeight);
        Assert.Equal(20, AppConstants.Dimensions.TitleHeight);
    }

    [Fact]
    public void SystemStyle_ConstantsMatchExpectedDefaults()
    {
        Assert.Equal("System", AppConstants.SystemStyle.Name);
        Assert.Equal("#FFFFB8", AppConstants.SystemStyle.BodyColor);
        Assert.Equal("monospace", AppConstants.SystemStyle.BodyFontName);
        Assert.Equal("#000000", AppConstants.SystemStyle.BodyFontColor);
        Assert.Equal(14, AppConstants.SystemStyle.BodyFontSize);
        Assert.Equal(0, AppConstants.SystemStyle.BodyFontDecorations);
    }

    [Fact]
    public void TrayMenuCommands_AreStableIdentifiers()
    {
        Assert.Equal("new", AppConstants.TrayMenuCommands.New);
        Assert.Equal("show", AppConstants.TrayMenuCommands.Show);
        Assert.Equal("hide", AppConstants.TrayMenuCommands.Hide);
        Assert.Equal("style_manager", AppConstants.TrayMenuCommands.StyleManager);
        Assert.Equal("exit", AppConstants.TrayMenuCommands.Exit);
    }

    [Fact]
    public void ContextMenuCommands_AreStableIdentifiers()
    {
        Assert.Equal("sleep", AppConstants.ContextMenuCommands.Sleep);
        Assert.Equal("alarm", AppConstants.ContextMenuCommands.Alarm);
        Assert.Equal("title", AppConstants.ContextMenuCommands.Title);
        Assert.Equal("lock", AppConstants.ContextMenuCommands.Lock);
        Assert.Equal("unlock", AppConstants.ContextMenuCommands.Unlock);
        Assert.Equal("rollup", AppConstants.ContextMenuCommands.RollUp);
        Assert.Equal("rolldown", AppConstants.ContextMenuCommands.RollDown);
        Assert.Equal("set_style", AppConstants.ContextMenuCommands.SetStyle);
    }

    [Fact]
    public void CssSectionNames_AreStableSelectors()
    {
        Assert.Equal("clingy-window", AppConstants.CssSections.ClingyWindow);
        Assert.Equal("clingy-title", AppConstants.CssSections.ClingyTitle);
        Assert.Equal("clingy-content", AppConstants.CssSections.ClingyContent);
        Assert.Equal("clingy-title-label", AppConstants.CssSections.ClingyTitleLabel);
        Assert.Equal("clingy-content-view", AppConstants.CssSections.ClingyContentView);
        Assert.Equal("focused", AppConstants.CssSections.Focused);
    }

    [Fact]
    public void IconNames_AndMenuOptions_AreStableKeys()
    {
        Assert.Equal("clingy_pinned", AppConstants.IconNames.ClingyPinned);
        Assert.Equal("clingy_unpinned", AppConstants.IconNames.ClingyUnpinned);
        Assert.Equal("clingy_locked", AppConstants.IconNames.ClingyLocked);
        Assert.Equal("clingy_close", AppConstants.IconNames.ClingyClose);
        Assert.Equal("clingy_icon", AppConstants.IconNames.Application);
        Assert.Equal("clingy", AppConstants.MenuOptions.ClingyMenuType);
        Assert.Equal("tray", AppConstants.MenuOptions.TrayMenuType);
    }
}
