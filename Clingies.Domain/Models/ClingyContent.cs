
namespace Clingies.Domain.Models;

public sealed class ClingyContent
{
    public int Id { get; init; }
    public string? Text { get; set; }
    public byte[]? Png { get; set; }

    public bool HasText => !string.IsNullOrWhiteSpace(Text);
    public bool HasImage => Png is { Length: > 0 };    
}
