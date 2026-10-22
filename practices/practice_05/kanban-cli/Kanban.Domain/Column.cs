using System.Collections.Generic;

namespace Kanban.Domain;

public class Column
{
    public string Name { get; set; }
    public List<Card> Cards { get; set; } = new();

    public Column(string name)
    {
        Name = name;
    }
}
