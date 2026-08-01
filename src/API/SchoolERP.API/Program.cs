using FluentValidation;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Behaviors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// 1. MediatR Register karo (Application assembly scan karega)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ICommand<>).Assembly);

    // 🔥 Behaviors ka ORDER MATTERS!
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));          // Pehle Log
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));       // Phir Validate
    cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));      // Phir Performance Check
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));      // Last mein Transaction (SaveChanges)
});

// 2. FluentValidation Scan karo (Saare Validators auto-detect honge)
builder.Services.AddValidatorsFromAssembly(typeof(ICommand<>).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
