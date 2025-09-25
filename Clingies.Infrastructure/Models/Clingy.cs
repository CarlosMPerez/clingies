namespace Clingies.Infrastructure.Models;

public sealed class Clingy
{
    public int Id { get; set; }
    public Enums.ClingyType Type { get; set; }
    public string? Title { get; set; }
    public bool IsDeleted { get; set; }

    [IgnoreComparisonField]
    public DateTime CreatedAt { get; set; }
    [IgnoreComparisonField]
    public ClingyProperties Properties { get; set; }
    [IgnoreComparisonField]
    public ClingyContent Content { get; set; }

    public Clingy()
    {
        Properties = new ClingyProperties();
        Content = new ClingyContent();    
    }
}
