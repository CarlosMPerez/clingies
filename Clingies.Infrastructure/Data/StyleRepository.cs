using Dapper;
using System.Data;
using Clingies.Application.Interfaces;
using Clingies.Domain.Models;
using Clingies.Infrastructure.Entities;
using Clingies.Infrastructure.Mapper;
using Clingies.Infrastructure.CustomExceptions;

namespace Clingies.Infrastructure.Data;

public class StyleRepository(IConnectionFactory connectionFactory, IClingiesLogger logger) : IStyleRepository
{
    private IDbConnection Conn => connectionFactory.GetConnection();
    public List<StyleModel> GetAll()
    {
        try
        {
            List<StyleModel> styles = new List<StyleModel>();
            var sql = """
                SELECT id, style_name, body_color, body_font_name, body_font_color, 
                    body_font_size, body_font_decorations, is_system, is_default, is_active
                FROM styles
            """;
            styles = Conn.Query<StyleEntity>(sql).Select(entity => entity.ToModel()).ToList();
            return styles;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at StyleRepository.GetAll");
            throw;
        }
    }

    public List<StyleModel> GetAllActive()
    {
        try
        {
            List<StyleModel> styles = new List<StyleModel>();
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
            List<StyleModel> styles = new List<StyleModel>();
            return GetAll().Where(x => x.IsActive).ToList().Count;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at StyleRepository.CountAllActive");
            throw;
        }
    }

    public StyleModel? Get(int id)
    {
        try
        {
            var parms = new Dictionary<string, object> { { "@Id", id } };
            var sql = """
                SELECT id, style_name, body_color, body_font_name, body_font_color, 
                    body_font_size, body_font_decorations, is_system, is_default, is_active
                FROM styles
                WHERE id = @Id
            """;

            var style = Conn.Query<StyleEntity>(sql, parms)
                            .Select(entity => entity.ToModel()).FirstOrDefault();
            return style;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at StyleRepository.Get {0}", id);
            throw;
        }
    }

    public void Create(StyleModel style)
    {
        try
        {
            if (style.IsSystem)
                throw new CannotUpdateSystemStyleException("You cannot insert an Style marked as System.");
            if (style.StyleName == AppConstants.SystemStyle.Name)
                throw new ReservedStyleNameException("You cannot use the 'System' name for a style. Please choose another.");
            if (CountAllActive() >= 10 && style.IsActive)
                throw new TooManyActiveStylesException("Cannot create a new Active style, there are already 10 Active styles");
            var sql = """
                INSERT INTO styles (id, style_name, body_color, body_font_name, body_font_color, 
                    body_font_size, body_font_decorations, is_system, is_default, is_active)
                VALUES (@Id, @StyleName, @BodyColor, @BodyFontName, @BodyFontColor, 
                    @BodyFontSize, @BodyFontDecorations, 0, @IsDefault, @IsActive)
                """;

            Conn.Execute(sql, style.ToEntity());
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
            if (style.IsSystem)
                throw new CannotUpdateSystemStyleException("The system style cannot be modified.");
            if (style.StyleName == AppConstants.SystemStyle.Name)
                throw new ReservedStyleNameException("You cannot use the 'System' name for a style. Please choose another.");
            var styles = GetAllActive();
            if (styles.Count >= 10 && styles.Any(x => x.Id == style.Id) && style.IsActive)
                throw new TooManyActiveStylesException("Cannot create a new Active style, there are already 10 Active styles");

            var sql = """
                UPDATE styles SET 
                    style_name = @StyleName,
                    body_color = @BodyColor, 
                    body_font_name = @BodyFontName, 
                    body_font_color = @BodyFontColor, 
                    body_font_size = @BodyFontSize, 
                    body_font_decorations = @BodyFontDecorations,
                    is_system = 0,
                    is_default = @IsDefault,
                    is_active = @IsActive
                WHERE id = @Id
                """;
            Conn.Execute(sql, style.ToEntity());
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
            var style = Get(id);
            if (style == null) throw new KeyNotFoundException();
            if (style.IsSystem) throw new CannotDeleteSystemStyleException("The system style cannot be deleted");
            if (style.IsActive) throw new CannotDeleteActiveStyle("The active style cannot be deleted");

            var parms = new Dictionary<string, object> { { "@id", id } };
            var sql = """
                DELETE FROM styles 
                WHERE id = @Id
                """;

            Conn.Execute(sql, parms);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error deleting style");
            throw;
        }
    }

    public void MarkActive(int id, bool isActive)
    {
        try
        {
            int currentActives = CountAllActive();
            int futureActives = isActive ? currentActives + 1 : currentActives - 1;
            if (isActive && futureActives > 10)
                throw new TooManyActiveStylesException("You cannot have more than 10 active styles at a time");
            if (!isActive && futureActives < 1)
                throw new AtLeastOneActiveStyleException("You must have at least one active style");

            var parms = new Dictionary<string, object> { { "@Id", id }, { "@IsActive", isActive } };
            var sql = """
                UPDATE styles SET 
                    is_active = @IsActive
                WHERE id = @Id
                """;
            Conn.Execute(sql, parms);
        }
        catch (Exception ex)
        {
            logger.Error(ex, isActive ? "Error marking style as active" : "Error marking style as inactive");
            throw;
        }
    }

    public void MarkDefault(int id, bool isDefault)
    {
        try
        {
            var parms = new Dictionary<string, object> { { "@Id", id }, { "@IsDefault", isDefault } };
            var sql = "";
            if (isDefault)
            {
                // SET ALL STYLES TO NON-DEFAULT AND THEN SET ONLY THE CHOSEN ONE
                sql = """
                    UPDATE styles SET 
                        is_default = 0;
                    UPDATE styles SET 
                        is_default = 1
                    WHERE id = @Id;
                """;
            }
            else
            {
                sql = """
                    UPDATE styles SET 
                        is_default = @IsDefault;
                    WHERE id = @Id;
                """;
            }
            Conn.Execute(sql, parms);
        }
        catch (Exception ex)
        {
            logger.Error(ex, isDefault ? "Error marking style as default" : "Error unmarking style as default");
            throw;
        }
    }

    public StyleModel GetDefault()
    {
        try
        {
            var sql = """
                SELECT id, style_name, body_color, body_font_name, body_font_color, 
                    body_font_size, body_font_decorations, is_system, is_default, is_active
                FROM styles
                WHERE is_default = 1
            """;

            var style = Conn.Query<StyleEntity>(sql)
                .Select(entity => entity.ToModel()).FirstOrDefault() ?? throw new KeyNotFoundException();
            return style;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at StyleRepository.GetDefault");
            throw;
        }
    }

    public int GetSystemStyleId()
    {
        return GetSystemStyle().Id;
    }

    public StyleModel GetSystemStyle()
    {
        try
        {
            var sql = """
                SELECT id, style_name, body_color, body_font_name, body_font_color, 
                    body_font_size, body_font_decorations, is_system, is_default, is_active
                FROM styles
                WHERE is_system = 1
            """;

            var style = Conn.Query<StyleEntity>(sql)
                .Select(entity => entity.ToModel()).FirstOrDefault() ?? throw new KeyNotFoundException();
            return style;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error at StyleRepository.GetDefault");
            throw;
        }
    }
}
