using System;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;

namespace Clingies.ApplicationLogic.Services;

public class StyleService(IStyleRepository repo, IClingiesLogger logger)
{
    public List<Style> GetAllActive()
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

    public List<Style> GetAll()
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

    public Style Get(string id)
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

    public Style GetDefault()
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

    public void Create(Style style)
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

    public void Update(Style style)
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

    public void Delete(string id)
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

    public void MarkActive(string id, bool isActive)
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

    public void MarkDefault(string id, bool isDefault)
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
