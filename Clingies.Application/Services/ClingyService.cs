using Clingies.Common;
using Clingies.Domain.Factories;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;

namespace Clingies.Application.Services;

public class ClingyService(IClingyRepository repo, IClingiesLogger logger)
{
    public List<Clingy> GetAllActive()
    {
        try
        {
            return repo.GetAllActive();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving all active Clingies");
            throw;
        }
    }

    public Clingy? Get(Guid id)
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

    public Clingy Create(string title = "", string content = "", double posX = 0, double posY = 0)
    {
        try
        {
            var clingy = ClingyEntityFactory.CreateNew(title, content, posX, posY);
            repo.Create(clingy);
            return clingy;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error creating a Clingie");
            throw;
        }
    }

    public void Update(Clingy clingy)
    {
        try
        {
            repo.Update(clingy);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating a Clingie");
            throw;
        }
    }

    public void SoftDelete(Guid id)
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

    public void HardDelete(Guid id)
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
