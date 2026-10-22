using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kanban.Domain;

namespace Kanban.Infrastructure;

public class BoardRepository
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new DateOnlyJsonConverter() }
    };

    public Board Load(string path = "data/board.json")
    {
        if (!File.Exists(path))
        {
            // Возвращаем пустую доску
            return new Board();
        }

        var json = File.ReadAllText(path);
        var board = JsonSerializer.Deserialize<Board>(json, Options);
        return board ?? new Board();
    }

    public void Save(Board board, string path = "data/board.json")
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(board, Options);
        File.WriteAllText(path, json);
    }

    private sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string Format = "yyyy-MM-dd";
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (s is null) throw new JsonException("Дата не может быть null");
            return DateOnly.ParseExact(s, Format);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format));
        }
    }
}
