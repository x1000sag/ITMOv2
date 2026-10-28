using System.Text.Json;
using System.Text.Json.Serialization;
using Kanban.Domain;
using Kanban.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Kanban.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BoardController : ControllerBase
{
    private readonly BoardRepository _repository;
    private readonly JsonSerializerOptions _jsonOptions;

    public BoardController(BoardRepository repository)
    {
        _repository = repository;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Converters = { new DateOnlyJsonConverter() }
        };
    }

    [HttpGet]
    public ActionResult<Board> GetBoard()
    {
        try
        {
            // Try to load existing board, create default if not exists
            if (!System.IO.File.Exists(GetBoardPath()))
            {
                var defaultBoard = new Board
                {
                    Name = "My Board",
                    Columns = new List<Column>
                    {
                        new() { Name = "ToDo" },
                        new() { Name = "InProgress" },
                        new() { Name = "Done" }
                    }
                };
                _repository.Save(defaultBoard);
                return Ok(defaultBoard);
            }
            var board = _repository.LoadOrCreate("My Board");
            return Ok(board);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut]
    public IActionResult SaveBoard([FromBody] Board board)
    {
        try
        {
            if (board == null)
            {
                return BadRequest(new { error = "Board cannot be null" });
            }
            // Validate basic structure
            if (string.IsNullOrWhiteSpace(board.Name))
            {
                return BadRequest(new { error = "Board name is required" });
            }
            if (board.Columns == null)
            {
                return BadRequest(new { error = "Columns are required" });
            }
            // Save atomically using repository
            _repository.Save(board);
            return Ok(new { message = "Board saved successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private string GetBoardPath()
    {
        // Try to get path from repository - but it's private; just check common locations
        var currentDir = Directory.GetCurrentDirectory();
        var paths = new[]
        {
            Path.Combine(currentDir, "data", "board.json"),
            Path.Combine(currentDir, "..", "..", "..", "data", "board.json"),
            Path.Combine(currentDir, "..", "..", "data", "board.json"),
        };
        foreach (var p in paths)
        {
            if (System.IO.File.Exists(Path.GetFullPath(p))) return Path.GetFullPath(p);
        }
        return Path.Combine(currentDir, "data", "board.json");
    }
}

// Copy of DateOnly converter for API serialization
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString();
        if (s is null) throw new JsonException("Ожидалась строка даты");
        if (DateOnly.TryParseExact(s, Format, out var d)) return d;
        throw new JsonException($"Неверный формат даты: {s}");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}
