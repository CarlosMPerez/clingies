namespace Clingies.Domain.Models;

public class Clingy
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }

    public static Clingy Create(string content, string? title = null)
    {
        return new Clingy
        {
            Id = Guid.NewGuid(),
            Content = content,
            Title = title,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = null,
            IsDeleted = false
        };
    }

    public void UpdateContent(string newContent)
    {
        Content = newContent;
        ModifiedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }    
}
