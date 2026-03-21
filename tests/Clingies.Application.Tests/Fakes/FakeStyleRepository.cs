namespace Clingies.Application.Tests.Fakes;

internal sealed class FakeStyleRepository : IStyleRepository
{
    public Func<List<StyleModel>>? OnGetAll { get; set; }
    public Func<List<StyleModel>>? OnGetAllActive { get; set; }
    public Func<int, StyleModel?>? OnGet { get; set; }
    public Func<StyleModel?>? OnGetDefault { get; set; }
    public Action<StyleModel>? OnCreate { get; set; }
    public Action<StyleModel>? OnUpdate { get; set; }
    public Action<int>? OnDelete { get; set; }
    public Action<int, bool>? OnMarkActive { get; set; }
    public Action<int, bool>? OnMarkDefault { get; set; }
    public Func<int>? OnGetSystemStyleId { get; set; }

    public int? LastId { get; private set; }
    public bool? LastBool { get; private set; }
    public StyleModel? LastStyle { get; private set; }

    public List<StyleModel> GetAll() =>
        OnGetAll?.Invoke() ?? [];

    public List<StyleModel> GetAllActive() =>
        OnGetAllActive?.Invoke() ?? [];

    public StyleModel? Get(int id)
    {
        LastId = id;
        return OnGet?.Invoke(id);
    }

    public StyleModel? GetDefault() =>
        OnGetDefault?.Invoke();

    public void Create(StyleModel style)
    {
        LastStyle = style;
        OnCreate?.Invoke(style);
    }

    public void Update(StyleModel style)
    {
        LastStyle = style;
        OnUpdate?.Invoke(style);
    }

    public void Delete(int id)
    {
        LastId = id;
        OnDelete?.Invoke(id);
    }

    public void MarkActive(int id, bool isActive)
    {
        LastId = id;
        LastBool = isActive;
        OnMarkActive?.Invoke(id, isActive);
    }

    public void MarkDefault(int id, bool isDefault)
    {
        LastId = id;
        LastBool = isDefault;
        OnMarkDefault?.Invoke(id, isDefault);
    }

    public int GetSystemStyleId() =>
        OnGetSystemStyleId?.Invoke() ?? 0;
}
