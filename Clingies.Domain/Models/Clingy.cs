namespace Clingies.Domain.Models;

public class Clingy
{
    public Guid Id { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string? Title { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public bool IsPinned { get; private set; }

    public double PositionX { get; private set; }
    public double PositionY { get; private set; }

    public double Width { get; private set; }
    public double Height { get; private set; }

    private Clingy() { }

    internal Clingy(Guid id, string title, string content)
    {
        Id = id;
        Title = title;
        Content = content;
        PositionX = 100;
        PositionY = 100;
        Width = 300;
        Height = 100;
        IsDeleted = false;
        IsPinned = false;
        CreatedAt = DateTime.UtcNow;
    }

    internal Clingy(Guid id, string title, string content,
                    double posX, double posY, double width, double height,
                    bool isDeleted = false, bool isPinned = false)
    {
        Id = id;
        Title = title;
        Content = content;
        PositionX = posX;
        PositionY = posY;
        Width = width;
        Height = height;
        IsDeleted = isDeleted;
        IsPinned = isPinned;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateTitle(string newTitle)
    {
        Title = newTitle;
        ModifiedAt = DateTime.UtcNow;
    }

    public void UpdateContent(string newContent)
    {
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
