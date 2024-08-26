using Azure;
using Microsoft.AspNetCore.Mvc;
using OpenAI;
using OpenAI.Chat;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

builder.AddAzureOpenAIClient("AZURE-OPENAI-CONNSTR");

builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddProblemDetails();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.MapPost("/chat", async ([FromServices] OpenAIClient client, [FromBody] string prompt) =>
{
    var chat = client.GetChatClient("chat");

    var clientResult = await chat.CompleteChatAsync(prompt);

    return clientResult.Value;
});

app.UseSwagger();
app.UseSwaggerUI();

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
