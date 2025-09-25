using Clingies.Infrastructure.Interfaces;
using Clingies.Domain.Interfaces;
using Clingies.Domain.DTOs;

namespace Clingies.Application.Services;

public class ClingyService(IClingyRepository repo, IClingiesLogger logger)
{
    public List<ClingyDto> GetAllActive()
    {
        try
        {
            return repo.GetAllActive().Select(dto => dto.ToDto()).ToList();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving all active Clingies");
            throw;
        }
    }

    public ClingyDto? Get(int id)
    {
        try
        {
            return repo.Get(id)?.ToDto();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving Clingie Id {0}", id);
            throw;
        }

    }

    public int Create(ClingyDto dto)
    {
        try
        {
            var entity = dto.ToEntity();
            entity.CreatedAt = DateTime.UtcNow;
            return repo.Create(entity);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error creating a Clingie");
            throw;
        }
    }

    public void Update(ClingyDto clingy)
    {
        try
        {
            repo.Update(clingy.ToEntity());
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
