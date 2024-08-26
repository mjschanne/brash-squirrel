using Azure.Monitor.OpenTelemetry.AspNetCore;
using BrashSquirrel.ApiService.Services;
using BrashSquirrel.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

var appInsightsConnStr = builder.Configuration.GetValue<string>(ResourceKeys.APPLICATIONINSIGHTS_CONNECTION_STRING);
var openaiConnStr = builder.Configuration.GetConnectionString(ResourceKeys.AZURE_OPENAI_CONN_STR);

ArgumentException.ThrowIfNullOrWhiteSpace(appInsightsConnStr, ResourceKeys.APPLICATIONINSIGHTS_CONNECTION_STRING);
ArgumentException.ThrowIfNullOrWhiteSpace(openaiConnStr, ResourceKeys.AZURE_OPENAI_CONN_STR);

builder.Services.AddOpenTelemetry().UseAzureMonitor();

builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();

var openaiConnStrDict = openaiConnStr!.ParseAsConnectionString();
var chatKernel = new MainChatKernelService(ResourceKeys.AZURE_OPENAI_CHAT_DEPLOYMENT_NAME, openaiConnStrDict["Endpoint"], appInsightsConnStr);
builder.Services.AddSingleton(_ => chatKernel);

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

app.MapPost("/chat", async ([FromServices] MainChatKernelService kernelService, [FromBody] string prompt) =>
{

    return await kernelService.ChatAsync(prompt);
});

app.UseSwagger();
app.UseSwaggerUI();

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public static class Extensions
{
    public static Dictionary<string, string> ParseAsConnectionString(this string connectionString)
    {
        var result = new Dictionary<string, string>();
        var pairs = connectionString.Split(';');

        foreach (var pair in pairs)
        {
            if (!string.IsNullOrWhiteSpace(pair))
            {
                var keyValue = pair.Split(new[] { '=' }, 2);
                if (keyValue.Length == 2)
                {
                    result[keyValue[0].Trim()] = keyValue[1].Trim();
                }
            }
        }

        return result;
    }
}
