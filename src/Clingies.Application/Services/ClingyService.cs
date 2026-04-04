using Clingies.Application.Interfaces;
using Clingies.Domain.Models;

namespace Clingies.Application.Services;

public class ClingyService(IClingyRepository repo, IClingiesLogger logger)
{
    public List<ClingyModel> GetAllActive()
    {
        try
        {
            return repo.GetAllActive().ToList();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving all active Clingies");
            throw;
        }
    }

    public ClingyModel? Get(int id)
    {
        try
        {
            return repo.Get(id);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving Clingy Id {0}", id);
            throw;
        }

    }

    public int Create(ClingyModel model)
    {
        try
        {
            model.CreatedAt = DateTime.UtcNow;
            return repo.Create(model);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error creating a Clingy");
            throw;
        }
    }

    public void Update(ClingyModel model)
    {
        try
        {
            repo.Update(model);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating a Clingy");
            throw;
        }
    }

    public void Close(int id)
    {
        try
        {
            repo.Close(id);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error closing a Clingy");
            throw;
        }
    }

    public void HardDelete(int id)
    {
        try
        {
            repo.HardDelete(id);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error hard deleting a Clingie");
            throw;
        }
    }
}
