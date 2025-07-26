using System.Data;
using Clingies.Domain.Dtos;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;
using Clingies.Infrastructure.CustomExceptions;
using Dapper;

namespace Clingies.Infrastructure.Data;

public class StyleRepository(IConnectionFactory connectionFactory, IClingiesLogger logger) : IStyleRepository
{

    private IDbConnection Conn => connectionFactory.GetConnection();
    public List<Style> GetAll()
    {
        try
        {
            List<Style> styles = new List<Style>();
            var sql = """
                SELECT id, body_color, title_color, body_font, body_font_color, 
                    body_font_size, body_font_decorations, title_font, title_font_size, 
                    title_font_color, title_font_decorations, is_default, is_active
                FROM styles
            """;
            var dtos = Conn.Query<StyleDto>(sql);
            styles = dtos.Select(dto => dto.ToEntity()).ToList();

            return styles;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at StyleRepository.GetAll");
            throw;
        }
    }

    public List<Style> GetAllActive()
    {
        try
        {
            List<Style> styles = new List<Style>();
            styles = GetAll().Where(x => x.IsActive).ToList();

            return styles;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at StyleRepository.GetAllActive");
            throw;
        }
    }

    private int CountAllActive()
    {
        try
        {
            List<Style> styles = new List<Style>();
            return GetAll().Where(x => x.IsActive).ToList().Count;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at StyleRepository.CountAllActive");
            throw;
        }
    }

    public Style? Get(string id)
    {
        try
        {
            var parms = new Dictionary<string, object> { { "@Id", id } };
            var sql = """
                SELECT id, body_color, title_color, body_font, body_font_color, 
                    body_font_size, body_font_decorations, title_font, title_font_size, 
                    title_font_color, title_font_decorations, is_default, is_active
                FROM styles
                WHERE id = @Id
            """;
            var dtos = Conn.Query<StyleDto>(sql, parms);
            var style = dtos.Select(dto => dto.ToEntity()).FirstOrDefault();

            return style;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at StyleRepository.Get {0}", id);
            throw;
        }
    }

    public void Create(Style style)
    {
        try
        {
            if (CountAllActive() >= 10 && style.IsActive) throw new TooManyActiveStylesException("Cannot create a new Active style, there are already 10 Active styles");
            var dto = style.ToDto();
            var sql = """
                INSERT INTO styles (id, body_color, title_color, body_font, body_font_color, 
                    body_font_size, body_font_decorations, title_font, title_font_size, 
                    title_font_color, title_font_decorations, is_default, is_active)
                VALUES (@id, @body_color, @title_color, @body_font, @body_font_color, 
                    @body_font_size, @body_font_decorations, @title_font, @title_font_size, 
                    @title_font_color, @title_font_decorations, @is_default, @is_active)
                """;

            Conn.Execute(sql, dto);
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
            if (style.Id == "system") throw new CannotUpdateSystemStyleException("The system style cannot be modified");
            var styles = GetAllActive();
            if (styles.Count >= 10 && styles.Any(x => x.Id == style.Id) && style.IsActive)
                throw new TooManyActiveStylesException("Cannot create a new Active style, tehre are already 10 Active styles");

            var dto = style.ToDto();
            var sql = """
                UPDATE styles SET 
                    body_color = @body_color, 
                    title_color = @title_color, 
                    body_font = @body_font, 
                    body_font_color = @body_font_color, 
                    body_font_size = @body_font_size, 
                    body_font_decorations = @body_font_decorations,
                    title_font = @title_font,
                    title_font_size = @title_font_size,
                    title_font_color = @title_font_color,
                    title_font_decorations = @title_font_decorations,
                    is_default = @is_default,
                    is_active = @is_active
                WHERE id = @id
                """;
            Conn.Execute(sql, dto);
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
            var style = Get(id);
            if (style == null) throw new KeyNotFoundException();
            if (style.Id == "system") throw new CannotDeleteSystemStyleException("The system style cannot be deleted");
            if (style.IsActive) throw new CannotDeleteActiveStyle("The active style cannot be deleted");

            var parms = new Dictionary<string, object> { { "@id", id } };
            var sql = """
                DELETE FROM styles 
                WHERE id = @id
                """;

            Conn.Execute(sql, parms);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error deleting style");
            throw;
        }
    }

    public void MarkActive(string id, bool isActive)
    {
        try
        {
            int currentActives = CountAllActive();
            int futureActives = isActive ? currentActives + 1 : currentActives - 1;
            if (isActive && futureActives > 10)
                throw new TooManyActiveStylesException("You cannot have more than 10 active styles at a time");
            if (!isActive && futureActives < 1)
                throw new AtLeastOneActiveStyleException("You must have at least one active style");

            var parms = new Dictionary<string, object> { { "@id", id }, { "@is_active", isActive } };
            var sql = """
                UPDATE styles SET 
                    is_active = @is_active
                WHERE id = @id
                """;
            Conn.Execute(sql, parms);
        }
        catch (Exception ex)
        {
            logger.Error(ex, isActive ? "Error marking style as active" : "Error marking style as inactive");
            throw;
        }
    }

    public void MarkDefault(string id, bool isDefault)
    {
        try
        {
            var parms = new Dictionary<string, object> { { "@id", id }, { "@is_default", isDefault } };
            // SET ALL STYLES TO NON-DEFAULT AND THEN SET ONLY THE CHOSEN ONE
            var sql = """
                UPDATE styles SET 
                    is_default = 0;
                UPDATE styles SET 
                    is_default = 1
                WHERE id = @id;
                """;
            Conn.Execute(sql, parms);
        }
        catch (Exception ex)
        {
            logger.Error(ex, isDefault ? "Error marking style as default" : "Error unmarking style as default");
            throw;
        }
    }

    public Style GetDefault()
    {
        try
        {
            var sql = """
                SELECT id, body_color, title_color, body_font, body_font_color, 
                    body_font_size, body_font_decorations, title_font, title_font_size, 
                    title_font_color, title_font_decorations, is_default, is_active
                FROM styles
                WHERE is_default = 1
            """;
            var dtos = Conn.Query<StyleDto>(sql);
            var style = dtos.Select(dto => dto.ToEntity()).FirstOrDefault() ?? throw new KeyNotFoundException();
            return style;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at StyleRepository.GetDefault");
            throw;
        }
    }
}
