namespace Clingies.Domain.Tests;

public class EnumsTests
{
    [Fact]
    public void ClingyType_ValuesMatchPersistenceContract()
    {
        Assert.Equal(0, (int)Enums.ClingyType.Unknown);
        Assert.Equal(1, (int)Enums.ClingyType.Desktop);
        Assert.Equal(2, (int)Enums.ClingyType.Sleeping);
        Assert.Equal(3, (int)Enums.ClingyType.Recurring);
        Assert.Equal(4, (int)Enums.ClingyType.Closed);
        Assert.Equal(5, (int)Enums.ClingyType.Stored);
    }

    [Fact]
    public void FontDecorations_CanBeCombinedAsFlags()
    {
        var combined = Enums.FontDecorations.Bold | Enums.FontDecorations.Italic | Enums.FontDecorations.Underline;

        Assert.True(combined.HasFlag(Enums.FontDecorations.Bold));
        Assert.True(combined.HasFlag(Enums.FontDecorations.Italic));
        Assert.True(combined.HasFlag(Enums.FontDecorations.Underline));
        Assert.False(combined.HasFlag(Enums.FontDecorations.Strikethrough));
    }

    [Fact]
    public void AppIndicatorEnums_HaveStableValues()
    {
        Assert.Equal(0, (int)Enums.AppIndicatorCategory.ApplicationStatus);
        Assert.Equal(4, (int)Enums.AppIndicatorCategory.Other);
        Assert.Equal(0, (int)Enums.AppIndicatorStatus.Passive);
        Assert.Equal(1, (int)Enums.AppIndicatorStatus.Active);
        Assert.Equal(2, (int)Enums.AppIndicatorStatus.Attention);
    }
}
