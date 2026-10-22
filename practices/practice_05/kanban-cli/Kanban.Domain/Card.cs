using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kanban.Domain;

public class Card
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateOnly? Deadline { get; set; }

    [JsonConstructor]
    public Card(Guid id, string title, string? description = null)
    {
        Id = id;
        Title = title;
        Description = description;
    }

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Тег не может быть пустым", nameof(tag));

        if (!Tags.Contains(tag))
            Tags.Add(tag);
    }

    public void RemoveTag(string tag)
    {
        Tags.Remove(tag);
    }
}
