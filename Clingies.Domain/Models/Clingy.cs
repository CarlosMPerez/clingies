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
    public bool IsRolled { get; private set; }
    public bool IsLocked { get; private set; }
    public double PositionX { get; private set; }
    public double PositionY { get; private set; }
    public double Width { get; private set; }
    public double Height { get; private set; }

    private Clingy() { }

    internal Clingy(Guid id, string title, string content, double posX = 100, double posY = 100)
    {
        Id = id;
        Title = title;
        Content = content;
        PositionX = posX;
        PositionY = posY;
        Width = 300;
        Height = 100;
        IsDeleted = false;
        IsPinned = false;
        IsRolled = false;
        IsLocked = false;
        CreatedAt = DateTime.UtcNow;
    }

    internal Clingy(Guid id, string title, string content,
                    double posX, double posY, double width, double height,
                    bool isDeleted = false, bool isPinned = false,
                    bool isRolled = false, bool isLocked = false)
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
        IsRolled = isRolled;
        IsLocked = isLocked;
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

    public void SetPinState(bool pinned)
    {
        IsPinned = pinned;
        ModifiedAt = DateTime.UtcNow;
    }

    public void SetRolledState(bool rolled)
    {
        IsRolled = rolled;
        ModifiedAt = DateTime.UtcNow;
    }

    public void SetLockState(bool locked)
    {
        IsLocked = locked;
        ModifiedAt = DateTime.UtcNow;
    }
}
