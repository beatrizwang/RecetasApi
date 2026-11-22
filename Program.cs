using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RecetasApi.Data;
using RecetasApi.Errors;
using RecetasApi.Interfaces;
using RecetasApi.Repositories;
using RecetasApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Base de datos: SQLite para que el proyecto se ejecute sin instalar nada.
// Para producción bastaría cambiar el proveedor (SQL Server / PostgreSQL) y la cadena de conexión.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Servicios con vida Scoped: una instancia por petición, igual que el DbContext.
builder.Services.AddScoped<IAlimentoService, AlimentoService>();
builder.Services.AddScoped<IRecetaService, RecetaService>();
// El DbContext hace de unidad de trabajo: se expone con la interfaz, pero es la misma instancia Scoped.
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IAlimentoRepository, AlimentoRepository>();
builder.Services.AddScoped<IRecetaRepository, RecetaRepository>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Los enums viajan como texto ("Carne") y no se aceptan números sueltos.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
    });

// Errores homogéneos en formato ProblemDetails.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Documento OpenAPI en /openapi/v1.json
}

app.MapControllers();

app.Run();
