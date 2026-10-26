using Vendinha.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Vendinha") ?? "Data Source=vendinha.db";
var database = new Database(connectionString);
database.CriarTabelas(Path.Combine(AppContext.BaseDirectory, "schema.sql"));
builder.Services.AddSingleton(database);

var app = builder.Build();

app.MapGet("/", () => "Vendinha API");

app.Run();
