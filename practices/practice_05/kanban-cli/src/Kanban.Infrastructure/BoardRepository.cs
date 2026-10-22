using System.Text.Json;
using System.Text.Json.Serialization;
using Kanban.Domain;

namespace Kanban.Infrastructure;

public class BoardRepository
{
    private readonly string _path;
    private readonly JsonSerializerOptions _jsonOptions;

    public BoardRepository(string path)
    {
        _path = path;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new DateOnlyJsonConverter() }
        };
    }

    public Board LoadOrCreate(string name)
    {
        if (!File.Exists(_path))
        {
            var board = new Board
            {
                Name = name,
                Columns = new List<Column>
                {
                    new() { Name = "ToDo" },
                    new() { Name = "InProgress" },
                    new() { Name = "Done" }
                }
            };
            Save(board);
            return board;
        }

        var json = File.ReadAllText(_path);
        var loaded = JsonSerializer.Deserialize<Board>(json, _jsonOptions) ?? throw new InvalidOperationException("Не удалось прочитать board.json");
        return loaded;
    }

    public void Save(Board board)
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(board, _jsonOptions);
        File.WriteAllText(_path, json);
    }
}

// Простой конвертер DateOnly для System.Text.Json
file sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
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
