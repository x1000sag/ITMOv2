using System;
using System.IO;
using Kanban.Domain;
using Kanban.Infrastructure;
using Xunit;

namespace Kanban.Tests;

public class BoardTests
{
    [Fact]
    public void AddColumn_UniqueNames()
    {
        var board = new Board();

        board.AddColumn("Todo");
        board.AddColumn("InProgress");

        Assert.Equal(2, board.Columns.Count);

        Assert.Throws<InvalidOperationException>(() => board.AddColumn("Todo"));
    }

    [Fact]
    public void AddCard_ToColumn()
    {
        var board = new Board();
        board.AddColumn("Todo");

        var card = board.AddCard("Todo", "Task 1", "Desc");

        Assert.NotEqual(Guid.Empty, card.Id);
        Assert.Single(board.Columns[0].Cards);
        Assert.Equal("Task 1", board.Columns[0].Cards[0].Title);
    }

    [Fact]
    public void MoveCard_BetweenColumns()
    {
        var board = new Board();
        board.AddColumn("Todo");
        board.AddColumn("Done");

        var card = board.AddCard("Todo", "Task 1");
        board.MoveCard(card.Id, "Done");

        Assert.Empty(board.Columns[0].Cards);
        Assert.Single(board.Columns[1].Cards);
        Assert.Equal(card.Id, board.Columns[1].Cards[0].Id);
    }

    [Fact]
    public void Tags_Add_Remove_Unique()
    {
        var board = new Board();
        board.AddColumn("Todo");
        var card = board.AddCard("Todo", "Task 1");

        board.AddTag(card.Id, "bug");
        board.AddTag(card.Id, "bug"); // не должен дублироваться
        board.AddTag(card.Id, "ui");

        Assert.Equal(2, board.FindCard(card.Id)!.Tags.Count);

        board.RemoveTag(card.Id, "bug");
        Assert.Single(board.FindCard(card.Id)!.Tags);
    }

    [Fact]
    public void Deadline_Set_Clear()
    {
        var board = new Board();
        board.AddColumn("Todo");
        var card = board.AddCard("Todo", "Task 1");

        var date = new DateOnly(2026, 1, 2);
        board.SetCardDeadline(card.Id, date);
        Assert.Equal(date, board.FindCard(card.Id)!.Deadline);

        board.ClearCardDeadline(card.Id);
        Assert.Null(board.FindCard(card.Id)!.Deadline);
    }

    [Fact]
    public void Repository_Save_Load()
    {
        var repo = new BoardRepository();
        var board = new Board();
        board.AddColumn("Todo");
        var card = board.AddCard("Todo", "Task 1");
        board.AddTag(card.Id, "tag1");
        var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "board.json");

        repo.Save(board, tmp);

        var loaded = repo.Load(tmp);
        Assert.Single(loaded.Columns);
        Assert.Single(loaded.Columns[0].Cards);
        Assert.Single(loaded.Columns[0].Cards[0].Tags);
    }
}
