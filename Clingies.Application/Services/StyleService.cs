using System;
using Clingies.Application.Interfaces;
using Clingies.Domain.Models;

namespace Clingies.Application.Services;

public class StyleService(IStyleRepository repo, IClingiesLogger logger)
{
    public List<StyleModel> GetAllActive()
    {
        try
        {
            return repo.GetAllActive();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving all active styles");
            throw;
        }
    }

    public List<StyleModel> GetAll()
    {
        try
        {
            return repo.GetAll();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving all styles");
            throw;
        }
    }

    public StyleModel Get(int id)
    {
        try
        {
            var style = repo.Get(id);
            if (style == null) throw new KeyNotFoundException();
            else return style;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving style {0}", id);
            throw;
        }
    }

    public StyleModel GetDefault()
    {
        try
        {
            return repo.GetDefault();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving default style");
            throw;
        }
    }

    public int GetSystemStyleId()
    {
        try
        {
            return repo.GetSystemStyleId();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving System style");
            throw;
        }
    }

    public void Create(StyleModel style)
    {
        try
        {
            repo.Create(style);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error creating new style");
            throw;
        }
    }

    public void Update(StyleModel style)
    {
        try
        {
            repo.Update(style);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating style");
            throw;
        }
    }

    public void Delete(int id)
    {
        try
        {
            repo.Delete(id);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error deleting style {0}", id);
            throw;
        }
    }

    public void MarkActive(int id, bool isActive)
    {
        try
        {
            repo.MarkActive(id, isActive);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error marking style {0} as {1}", id, isActive ? "active" : "inactive");
            throw;
        }
    }

    public void MarkDefault(int id, bool isDefault)
    {
        try
        {
            repo.MarkDefault(id, isDefault);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error marking style {0} as {1}", id, isDefault ? "default" : "non default");
            throw;
        }
    }
}
