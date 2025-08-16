namespace Clingies.Domain.Models;

public class Clingy
{
    public int Id { get; set; }

    private string? _title;
    public string? Title
    {
        get => _title;
        set
        {
            _title = value;
            ModifiedAt = DateTime.UtcNow;
        }
    }

    private string _content = string.Empty;
    public string Content
    {
        get => _content;
        set
        {
            _content = value;
            ModifiedAt = DateTime.UtcNow;
        }
    }

    private double _positionX;
    public double PositionX
    {
        get => _positionX;
        set
        {
            _positionX = value;
            ModifiedAt = DateTime.UtcNow;
        }
    }

    private double _positionY;
    public double PositionY
    {
        get => _positionY;
        set
        {
            _positionY = value;
            ModifiedAt = DateTime.UtcNow;
        }
    }

    private double _width;
    public double Width
    {
        get => _width;
        set
        {
            _width = value;
            ModifiedAt = DateTime.UtcNow;
        }
    }

    private double _height;
    public double Height
    {
        get => _height;
        set
        {
            _height = value;
            ModifiedAt = DateTime.UtcNow;
        }
    }

    private bool _isPinned;
    public bool IsPinned
    {
        get => _isPinned;
        set
        {
            _isPinned = value;
            ModifiedAt = DateTime.UtcNow;
        }
    }

    private bool _isRolled;
    public bool IsRolled
    {
        get => _isRolled;
        set
        {
            _isRolled = value;
            ModifiedAt = DateTime.UtcNow;
        }
    }

    private bool _isLocked;
    public bool IsLocked
    {
        get => _isLocked;
        set
        {
            _isLocked = value;
            ModifiedAt = DateTime.UtcNow;
        }
    }

    private bool _isStanding;
    public bool IsStanding
    {
        get => _isStanding;
        set
        {
            _isStanding = value;
            ModifiedAt = DateTime.UtcNow;
        }
    }

    private bool _isDeleted;
    public bool IsDeleted
    {
        get => _isDeleted;
        set
        {
            _isDeleted = value;
            ModifiedAt = DateTime.UtcNow;
        }
    }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; private set; }

    public Clingy() { }


    public ClingyDto ToDto()
    {
        return new ClingyDto()
        {
            Id = Id,
            Title = Title,
            Content = Content,
            PositionX = PositionX,
            PositionY = PositionY,
            Width = Width,
            Height = Height,
            IsPinned = IsPinned,
            IsLocked = IsLocked,
            IsRolled = IsRolled,
            IsStanding = IsStanding,
            IsDeleted = IsDeleted
        };
    }
}
