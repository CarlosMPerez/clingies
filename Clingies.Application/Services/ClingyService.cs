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
            logger.Error(ex, "Error retrieving Clingie Id {0}", id);
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
            logger.Error(ex, "Error creating a Clingie");
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
            logger.Error(ex, "Error updating a Clingie");
            throw;
        }
    }

    public void SoftDelete(int id)
    {
        try
        {
            repo.SoftDelete(id);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error soft deleting a Clingie");
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
