using System;
using System.Collections.Generic;
using System.Linq;

namespace Kanban.Domain;

public class Board
{
    public List<Column> Columns { get; set; } = new();

    public void AddColumn(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя колонки не может быть пустым", nameof(name));

        if (Columns.Any(c => string.Equals(c.Name, name, StringComparison.Ordinal)))
            throw new InvalidOperationException($"Колонка с именем '{name}' уже существует");

        Columns.Add(new Column(name));
    }

    public Card AddCard(string columnName, string title, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException("Имя колонки не может быть пустым", nameof(columnName));

        var column = Columns.FirstOrDefault(c => string.Equals(c.Name, columnName, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Колонка '{columnName}' не найдена");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Заголовок карточки не может быть пустым", nameof(title));

        // Гарантия уникальности Guid в рамках доски (очень маловероятны коллизии, но проверим)
        Guid id;
        do { id = Guid.NewGuid(); } while (FindCard(id) != null);

        var card = new Card(id, title, description);
        column.Cards.Add(card);
        return card;
    }

    public void MoveCard(Guid cardId, string toColumnName)
    {
        var found = FindCardWithColumn(cardId) ?? throw new InvalidOperationException($"Карточка '{cardId}' не найдена");
        var (card, fromColumn) = found.Value;

        var toColumn = Columns.FirstOrDefault(c => string.Equals(c.Name, toColumnName, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Колонка '{toColumnName}' не найдена");

        if (ReferenceEquals(fromColumn, toColumn))
            return; // уже в этой колонке

        fromColumn.Cards.Remove(card);
        toColumn.Cards.Add(card);
    }

    public void SetCardDeadline(Guid cardId, DateOnly? date)
    {
        var card = FindCard(cardId) ?? throw new InvalidOperationException($"Карточка '{cardId}' не найдена");
        card.Deadline = date;
    }

    public void ClearCardDeadline(Guid cardId)
    {
        var card = FindCard(cardId) ?? throw new InvalidOperationException($"Карточка '{cardId}' не найдена");
        card.Deadline = null;
    }

    public void AddTag(Guid cardId, string tag)
    {
        var card = FindCard(cardId) ?? throw new InvalidOperationException($"Карточка '{cardId}' не найдена");
        card.AddTag(tag);
    }

    public void RemoveTag(Guid cardId, string tag)
    {
        var card = FindCard(cardId) ?? throw new InvalidOperationException($"Карточка '{cardId}' не найдена");
        card.RemoveTag(tag);
    }

    public Card? FindCard(Guid cardId)
    {
        foreach (var col in Columns)
        {
            var card = col.Cards.FirstOrDefault(c => c.Id == cardId);
            if (card != null)
                return card;
        }
        return null;
    }

    private (Card card, Column column)? FindCardWithColumn(Guid cardId)
    {
        foreach (var col in Columns)
        {
            var card = col.Cards.FirstOrDefault(c => c.Id == cardId);
            if (card != null)
                return (card, col);
        }
        return null;
    }
}
