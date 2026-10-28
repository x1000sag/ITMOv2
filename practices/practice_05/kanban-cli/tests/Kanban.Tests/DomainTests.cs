using Kanban.Domain;
using Kanban.Infrastructure;
using Xunit;

namespace Kanban.Tests;

public class DomainTests
{
    [Fact]
    public void CreateAndPersistBoard()
    {
        var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "board.json");
        var repo = new BoardRepository(tmp);
        var board = repo.LoadOrCreate("MyBoard");
        Assert.Equal("MyBoard", board.Name);
        Assert.Equal(3, board.Columns.Count);

        board.Columns[0].Cards.Add(new Card { Id = Guid.NewGuid().ToString(), Title = "Task A" });
        repo.Save(board);

        var reloaded = repo.LoadOrCreate("ignored");
        Assert.Equal("MyBoard", reloaded.Name);
        Assert.Equal(3, reloaded.Columns.Count);
        Assert.Single(reloaded.Columns[0].Cards);
        Assert.Equal("Task A", reloaded.Columns[0].Cards[0].Title);
    }
}
