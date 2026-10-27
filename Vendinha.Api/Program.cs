using Vendinha.Api.Endpoints;
using Vendinha.Data;
using Vendinha.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Vendinha") ?? "Data Source=vendinha.db";
var database = new Database(connectionString);
database.CriarTabelas(Path.Combine(AppContext.BaseDirectory, "schema.sql"));
builder.Services.AddSingleton(database);
builder.Services.AddScoped<ClienteService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Vendinha API");
app.MapClienteEndpoints();

app.Run();
