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
            var sql = """
                          SELECT id, style_name, body_color, body_font_name, body_font_color, 
                              body_font_size, body_font_decorations, is_system, is_default, is_active
                          FROM styles
                      """;
            var styles = Conn.Query<StyleEntity>(sql).Select(entity => entity.ToModel()).ToList();
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
            var sql = """
                SELECT id, style_name, body_color, body_font_name, body_font_color, 
                    body_font_size, body_font_decorations, is_system, is_default, is_active
                FROM styles
                WHERE is_active = 1
            """;
            return Conn.Query<StyleEntity>(sql).Select(entity => entity.ToModel()).ToList();
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
            const string sql = """
                SELECT COUNT(*)
                FROM styles
                WHERE is_active = 1
            """;
            return Conn.ExecuteScalar<int>(sql);
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
                INSERT INTO styles (style_name, body_color, body_font_name, body_font_color, 
                    body_font_size, body_font_decorations, is_system, is_default, is_active)
                VALUES (@StyleName, @BodyColor, @BodyFontName, @BodyFontColor, 
                    @BodyFontSize, @BodyFontDecorations, 0, @IsDefault, @IsActive);
                SELECT last_insert_rowid();
                """;

            int newStyleId = Conn.ExecuteScalar<int>(sql, style.ToEntity());
            if (style.IsDefault) MarkDefault(newStyleId, true);
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
            var current = Get(style.Id) ?? throw new KeyNotFoundException($"Style {style.Id} not found.");

            if (style.IsSystem)
                throw new CannotUpdateSystemStyleException("The system style cannot be modified.");
            if (style.StyleName == AppConstants.SystemStyle.Name)
                throw new ReservedStyleNameException("You cannot use the 'System' name for a style. Please choose another.");
            ValidateActiveTransition(current, style.IsActive);

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

            if (style.IsDefault) MarkDefault(style.Id, true);
            else if (current.IsDefault) CheckOneAndOnlyOneDefault();
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
            if (CheckStyleIsInUse(id)) throw new CannotDeleteStyleInUse("Cannot delete the selected style, is being used by at least one active Clingy");

            SetStyleToSystemForNotesUsingStyle(id);

            var parms = new Dictionary<string, object> { { "@Id", id } };
            var sql = """
                DELETE FROM styles 
                WHERE id = @Id
                """;

            Conn.Execute(sql, parms);

            // We make SURE there's at least one style marked as default, in case we've just deleted the default
            CheckOneAndOnlyOneDefault();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error deleting style");
            throw;
        }
    }

    /// <summary>
    /// Cambiamos el estilo por System para cualquier clingy que aún use el estilo que vamos a borrar,
    /// aunque no esté activo en escritorio, para evitar errores de FK.
    /// </summary>
    /// <param name="styleId"></param>
    /// <returns>True si con éxito</returns>
    private bool SetStyleToSystemForNotesUsingStyle(int styleId)
    {
        try
        {
            var systemStyleId = GetSystemStyleId();

            var parms = new Dictionary<string, object> { { "@OldStyleId", styleId }, { "@SystemStyleId", systemStyleId } };
            var sql = """
                UPDATE clingy_properties SET 
                    style_id = @SystemStyleId
                WHERE style_id = @OldStyleId
                """;

            Conn.Execute(sql, parms);
            return true;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error setting default style for notes using deleted style");
            throw;
        }
    }

    public void MarkActive(int id, bool isActive)
    {
        try
        {
            var current = Get(id) ?? throw new KeyNotFoundException($"Style {id} not found.");
            ValidateActiveTransition(current, isActive);

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
                        is_default = @IsDefault
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

    public StyleModel? GetDefault()
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
                .Select(entity => entity.ToModel()).FirstOrDefault();
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

    private void CheckOneAndOnlyOneDefault()
    {
        if (GetDefault() == null)
        {
            MarkDefault(GetSystemStyleId(), true);
        }
    }

    private void ValidateActiveTransition(StyleModel currentStyle, bool targetIsActive)
    {
        if (currentStyle.IsActive == targetIsActive)
            return;

        int currentActives = CountAllActive();
        if (targetIsActive && currentActives >= 10)
            throw new TooManyActiveStylesException("You cannot have more than 10 active styles at a time");
        if (!targetIsActive && currentActives <= 1)
            throw new AtLeastOneActiveStyleException("You must have at least one active style");
    }

    private bool CheckStyleIsInUse(int styleId)
    {
        var parms = new Dictionary<string, object>
        {
            { "@StyleId", styleId },
            { "@TypeId", (int)Clingies.Domain.Common.Enums.ClingyType.Desktop }
        };
        var notesUsingStyles = Conn.ExecuteScalar<int>(
            """
                SELECT COUNT(*)
                FROM clingies AS c 
                INNER JOIN clingy_properties AS p ON p.id = c.id
                WHERE p.style_id = @StyleId AND c.type_id = @TypeId
            """, parms);

        return notesUsingStyles > 0;
    }
}
