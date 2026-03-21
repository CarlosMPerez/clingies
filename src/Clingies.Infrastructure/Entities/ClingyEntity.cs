namespace Clingies.Infrastructure.Entities;

public sealed class ClingyEntity
{
    public int Id { get; set; }
    public Enums.ClingyType Type { get; set; }
    public string? Title { get; set; }
    public bool IsDeleted { get; set; }

    [IgnoreComparisonField]
    public DateTime CreatedAt { get; set; }
    [IgnoreComparisonField]
    public ClingyPropertiesEntity Properties { get; set; }
    [IgnoreComparisonField]
    public ClingyContentEntity Content { get; set; }

    public ClingyEntity()
    {
        Properties = new ClingyPropertiesEntity();
        Content = new ClingyContentEntity();    
    }
}
