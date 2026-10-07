namespace Kanban.Domain;

public class Board
{
    public required string Name { get; init; }
    public List<Column> Columns { get; init; } = new();
}

public class Column
{
    public required string Name { get; init; }
    public List<Card> Cards { get; init; } = new();
}

public class Card
{
    public required string Id { get; init; } // UUID как строка
    public required string Title { get; set; }
    public string? Description { get; set; }
    public List<string> Tags { get; } = new();
    public DateOnly? Deadline { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
