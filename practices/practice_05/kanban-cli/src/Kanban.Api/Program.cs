using Kanban.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // оставляем PascalCase как в доменной модели
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(new Kanban.Api.Controllers.DateOnlyJsonConverter());
    });
builder.Services.AddOpenApi();

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure BoardRepository to use data/board.json relative to solution root
var solutionRoot = Directory.GetCurrentDirectory();
while (solutionRoot != null && !Directory.Exists(Path.Combine(solutionRoot, "data")) && !File.Exists(Path.Combine(solutionRoot, "kanban-cli.sln")))
{
    var parent = Directory.GetParent(solutionRoot);
    if (parent == null) break;
    solutionRoot = parent.FullName;
}
// Try to find the data directory
string dataPath;
if (Directory.Exists(Path.Combine(solutionRoot ?? Directory.GetCurrentDirectory(), "data")))
{
    dataPath = Path.Combine(solutionRoot ?? Directory.GetCurrentDirectory(), "data", "board.json");
}
else
{
    // Fallback to relative path
    dataPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "data", "board.json");
    if (!File.Exists(dataPath))
    {
        dataPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "board.json");
    }
}

builder.Services.AddSingleton(new BoardRepository(Path.GetFullPath(dataPath)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

app.MapControllers();

app.Run();
