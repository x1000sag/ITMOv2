using System;
using System.Globalization;
using Kanban.Domain;
using Kanban.Infrastructure;

namespace Kanban.Cli;

class Program
{
    static int Main(string[] args)
    {
        try
        {
            return Run(args);
        }
        catch (ValidationException ve)
        {
            Console.Error.WriteLine(ve.Message);
            return 2; // ошибка валидации ввода
        }
        catch (NotFoundException nfe)
        {
            Console.Error.WriteLine(nfe.Message);
            return 3; // сущность не найдена или отсутствует доска
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Ошибка: {ex.Message}");
            return 1; // прочие ошибки выполнения
        }
    }

    private static int Run(string[] args)
    {
        if (args.Length == 0)
            throw new ValidationException("Ожидалась команда. См. ADR-0002: CLI UI");

        var cmd = args[0];
        var repo = new BoardRepository(Path.Combine("data", "board.json"));

        switch (cmd)
        {
            case "board":
                return BoardCmd(args, repo);
            case "column":
                return ColumnCmd(args, repo);
            case "card":
                return CardCmd(args, repo);
            default:
                throw new ValidationException($"Неизвестная команда: {cmd}");
        }
    }

    // board init [--name <name>]
    private static int BoardCmd(string[] args, BoardRepository repo)
    {
        if (args.Length < 2)
            throw new ValidationException("Ожидалась подкоманда для 'board'");

        var sub = args[1];
        if (sub == "init")
        {
            // Извлечь имя, если задано
            var name = GetOptionValue(args, "--name") ?? "My Board";

            // Если файл существует — ошибка (2)
            if (File.Exists(Path.Combine("data", "board.json")))
                throw new ValidationException("Файл data/board.json уже существует. Используйте другой путь или удалите файл.");

            var board = repo.LoadOrCreate(name);
            // LoadOrCreate создаст board.json с дефолтными колонками и именем
            Console.WriteLine($"Инициализирована доска: {board.Name}");
            return 0;
        }

        throw new ValidationException($"Неизвестная подкоманда 'board {sub}'");
    }

    // column add <name> | column list
    private static int ColumnCmd(string[] args, BoardRepository repo)
    {
        if (args.Length < 2)
            throw new ValidationException("Ожидалась подкоманда для 'column'");

        var sub = args[1];
        if (sub == "add")
        {
            EnsureBoardExists();
            if (args.Length < 3)
                throw new ValidationException("Укажите имя колонки: column add <name>");
            var name = args[2];

            var board = repo.LoadOrCreate("ignored");
            if (board.Columns.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
                throw new ValidationException($"Колонка '{name}' уже существует");

            board.Columns.Add(new Column { Name = name });
            repo.Save(board);
            return 0;
        }
        else if (sub == "list")
        {
            EnsureBoardExists();
            var board = repo.LoadOrCreate("ignored");
            foreach (var c in board.Columns)
                Console.WriteLine(c.Name);
            return 0;
        }

        throw new ValidationException($"Неизвестная подкоманда 'column {sub}'");
    }

    // card ...
    private static int CardCmd(string[] args, BoardRepository repo)
    {
        if (args.Length < 2)
            throw new ValidationException("Ожидалась подкоманда для 'card'");

        var sub = args[1];
        switch (sub)
        {
            case "add":
                return CardAdd(args, repo);
            case "list":
                return CardList(args, repo);
            case "move":
                return CardMove(args, repo);
            case "tag":
                return CardTag(args, repo);
            case "deadline":
                return CardDeadline(args, repo);
            default:
                throw new ValidationException($"Неизвестная подкоманда 'card {sub}'");
        }
    }

    // card add <column> --title "..." [--desc "..."]
    private static int CardAdd(string[] args, BoardRepository repo)
    {
        EnsureBoardExists();
        if (args.Length < 3)
            throw new ValidationException("Укажите колонку: card add <column>");
        var column = args[2];
        var title = GetOptionValue(args, "--title");
        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException("--title обязателен и не может быть пустым");
        var desc = GetOptionValue(args, "--desc");

        var board = repo.LoadOrCreate("ignored");
        var col = board.Columns.FirstOrDefault(c => string.Equals(c.Name, column, StringComparison.OrdinalIgnoreCase));
        if (col == null)
            throw new NotFoundException($"Колонка '{column}' не найдена");

        var card = new Card
        {
            Id = Guid.NewGuid().ToString(),
            Title = title!,
            Description = string.IsNullOrWhiteSpace(desc) ? null : desc,
            CreatedAt = DateTime.UtcNow
        };
        col.Cards.Add(card);
        repo.Save(board);
        Console.WriteLine(card.Id);
        return 0;
    }

    // card list [--column <name>]
    private static int CardList(string[] args, BoardRepository repo)
    {
        EnsureBoardExists();
        var board = repo.LoadOrCreate("ignored");
        var filter = GetOptionValue(args, "--column");
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var col = board.Columns.FirstOrDefault(c => string.Equals(c.Name, filter, StringComparison.OrdinalIgnoreCase));
            if (col == null)
                throw new NotFoundException($"Колонка '{filter}' не найдена");
            PrintCardsFlat(col);
            return 0;
        }

        foreach (var c in board.Columns)
        {
            Console.WriteLine($"# {c.Name}");
            PrintCardsFlat(c);
        }
        return 0;
    }

    private static void PrintCardsFlat(Column col)
    {
        // Заголовок, затем строки: ID  Title  [deadline] [tags]
        foreach (var card in col.Cards)
        {
            var deadline = card.Deadline.HasValue ? $"[{card.Deadline.Value:yyyy-MM-dd}]" : string.Empty;
            var tags = (card.Tags != null && card.Tags.Count > 0) ? $"[{string.Join(',', card.Tags)}]" : string.Empty;
            var parts = new[] { card.Id, card.Title, deadline, tags };
            Console.WriteLine(string.Join("  ", parts.Where(p => !string.IsNullOrEmpty(p))));
        }
    }

    // card move <cardId> --to <column>
    private static int CardMove(string[] args, BoardRepository repo)
    {
        EnsureBoardExists();
        if (args.Length < 3)
            throw new ValidationException("Укажите cardId: card move <cardId> --to <column>");
        var cardId = args[2];
        var to = GetOptionValue(args, "--to");
        if (string.IsNullOrWhiteSpace(to))
            throw new ValidationException("--to обязателен");

        var board = repo.LoadOrCreate("ignored");
        var (card, fromCol) = FindCardWithColumn(board, cardId) ?? throw new NotFoundException($"Карточка '{cardId}' не найдена");
        var toCol = board.Columns.FirstOrDefault(c => string.Equals(c.Name, to, StringComparison.OrdinalIgnoreCase))
                   ?? throw new NotFoundException($"Колонка '{to}' не найдена");
        if (!ReferenceEquals(fromCol, toCol))
        {
            fromCol.Cards.Remove(card);
            toCol.Cards.Add(card);
            repo.Save(board);
        }
        return 0;
    }

    // card tag add <cardId> <tag> | rm
    private static int CardTag(string[] args, BoardRepository repo)
    {
        EnsureBoardExists();
        if (args.Length < 3)
            throw new ValidationException("Ожидалась подкоманда: card tag add|rm");
        var action = args[2];
        if (args.Length < 5)
            throw new ValidationException("Использование: card tag add <cardId> <tag> | card tag rm <cardId> <tag>");
        var cardId = args[3];
        var tag = args[4];

        var board = repo.LoadOrCreate("ignored");
        var (card, _) = FindCardWithColumn(board, cardId) ?? throw new NotFoundException($"Карточка '{cardId}' не найдена");

        switch (action)
        {
            case "add":
                if (!card.Tags.Contains(tag))
                {
                    card.Tags.Add(tag);
                    repo.Save(board);
                }
                return 0;
            case "rm":
                if (card.Tags.Contains(tag))
                {
                    card.Tags.Remove(tag);
                    repo.Save(board);
                }
                return 0;
            default:
                throw new ValidationException($"Неизвестная подкоманда 'card tag {action}'");
        }
    }

    // card deadline set <cardId> <YYYY-MM-DD> | clear <cardId>
    private static int CardDeadline(string[] args, BoardRepository repo)
    {
        EnsureBoardExists();
        if (args.Length < 3)
            throw new ValidationException("Ожидалась подкоманда: card deadline set|clear");
        var action = args[2];
        if (action == "set")
        {
            if (args.Length < 5)
                throw new ValidationException("Использование: card deadline set <cardId> <YYYY-MM-DD>");
            var cardId = args[3];
            var dateStr = args[4];
            if (!TryParseDate(dateStr, out var date))
                throw new ValidationException($"Неверный формат даты: {dateStr}. Ожидается YYYY-MM-DD");

            var board = repo.LoadOrCreate("ignored");
            var (card, _) = FindCardWithColumn(board, cardId) ?? throw new NotFoundException($"Карточка '{cardId}' не найдена");
            card.Deadline = date;
            repo.Save(board);
            return 0;
        }
        else if (action == "clear")
        {
            if (args.Length < 4)
                throw new ValidationException("Использование: card deadline clear <cardId>");
            var cardId = args[3];
            var board = repo.LoadOrCreate("ignored");
            var (card, _) = FindCardWithColumn(board, cardId) ?? throw new NotFoundException($"Карточка '{cardId}' не найдена");
            card.Deadline = null;
            repo.Save(board);
            return 0;
        }

        throw new ValidationException($"Неизвестная подкоманда 'card deadline {action}'");
    }

    private static (Card card, Column column)? FindCardWithColumn(Board board, string cardId)
    {
        foreach (var col in board.Columns)
        {
            var found = col.Cards.FirstOrDefault(c => string.Equals(c.Id, cardId, StringComparison.OrdinalIgnoreCase));
            if (found != null) return (found, col);
        }
        return null;
    }

    private static bool TryParseDate(string s, out DateOnly date)
    {
        return DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    private static string? GetOptionValue(string[] args, string option)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == option)
            {
                if (i + 1 >= args.Length)
                    throw new ValidationException($"Для опции {option} ожидается значение");
                return args[i + 1];
            }
            // Также поддержим форму --opt=value
            if (args[i].StartsWith(option + "=", StringComparison.Ordinal))
            {
                return args[i].Substring(option.Length + 1);
            }
        }
        return null;
    }

    private static void EnsureBoardExists()
    {
        var path = Path.Combine("data", "board.json");
        if (!File.Exists(path))
            throw new NotFoundException("Доска не найдена (data/board.json). Выполните 'board init'.");
    }
}

// Специальные типы исключений для маппинга на коды возврата
file class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

file class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
}
