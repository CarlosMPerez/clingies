namespace Clingies.Application.Tests.Fakes;

internal sealed class FakeClingyRepository : IClingyRepository
{
    public Func<List<ClingyModel>>? OnGetAllActive { get; set; }
    public Func<int, ClingyModel?>? OnGet { get; set; }
    public Func<ClingyModel, int>? OnCreate { get; set; }
    public Action<ClingyModel>? OnUpdate { get; set; }
    public Action<int>? OnClose { get; set; }
    public Action<int>? OnHardDelete { get; set; }

    public int? LastId { get; private set; }
    public ClingyModel? LastModel { get; private set; }

    public List<ClingyModel> GetAllActive() =>
        OnGetAllActive?.Invoke() ?? [];

    public ClingyModel? Get(int id)
    {
        LastId = id;
        return OnGet?.Invoke(id);
    }

    public int Create(ClingyModel clingy)
    {
        LastModel = clingy;
        return OnCreate?.Invoke(clingy) ?? 0;
    }

    public void Update(ClingyModel clingy)
    {
        LastModel = clingy;
        OnUpdate?.Invoke(clingy);
    }

    public void Close(int id)
    {
        LastId = id;
        OnClose?.Invoke(id);
    }

    public void HardDelete(int id)
    {
        LastId = id;
        OnHardDelete?.Invoke(id);
    }
}
