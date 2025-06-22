namespace Clingies.Domain.Models;

public class Clingy
{
    public Guid Id { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string? Title { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    public double PositionX { get; private set; }
    public double PositionY { get; private set; }

    public double Width { get; private set; }
    public double Height { get; private set; }

    private Clingy() { }

    internal Clingy(Guid id, string title, string content, double x, double y, double width, double height)
    {
        Id = id;
        Title = title;
        Content = content;
        PositionX = x;
        PositionY = y;
        Width = width;
        Height = height;
        IsDeleted = false;
    }

    public void UpdateContent(string newTitle, string newContent)
    {
        Title = newTitle;
        Content = newContent;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Move(double x, double y)
    {
        PositionX = x;
        PositionY = y;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Resize(double width, double height)
    {
        Width = width;
        Height = height;
        ModifiedAt = DateTime.UtcNow;
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        ModifiedAt = DateTime.UtcNow;
    }
}
