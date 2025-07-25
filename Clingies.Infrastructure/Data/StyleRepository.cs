using System.Data;
using Clingies.Domain.Dtos;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;
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
                    title_font_color, title_font_decorations
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

    public Style? Get(string id)
    {
        try
        {
            var parms = new Dictionary<string, object> { { "@Id", id } };
            var sql = """
                SELECT id, body_color, title_color, body_font, body_font_color, 
                    body_font_size, body_font_decorations, title_font, title_font_size, 
                    title_font_color, title_font_decorations
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
            var dto = style.ToDto();
            var sql = """
                INSERT INTO styles (id, body_color, title_color, body_font, body_font_color, 
                    body_font_size, body_font_decorations, title_font, title_font_size, 
                    title_font_color, title_font_decorations)
                VALUES (@id, @body_color, @title_color, @body_font, @body_font_color, 
                    @body_font_size, @body_font_decorations, @title_font, @title_font_size, 
                    @title_font_color, @title_font_decorations)
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
                    title_font_decorations = @title_font_decorations
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
}
