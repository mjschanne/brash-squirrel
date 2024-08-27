using Azure.Monitor.OpenTelemetry.AspNetCore;
using BrashSquirrel.ApiService.Services;
using BrashSquirrel.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

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

builder.AddAzureCosmosClient("cosmos");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/old", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)],
            Guid.NewGuid().ToString()
        ))
        .ToArray();
    return forecast;
});

app.MapGet("/weatherforecast", async ([FromServices] CosmosClient client) =>
{
    var db = client.GetDatabase(ResourceKeys.COSMOS_DB_NAME);
    var container = db.GetContainer(ResourceKeys.COSMOS_CONTAINER_NAME);

    using FeedIterator<WeatherForecast> feed = container.GetItemLinqQueryable<WeatherForecast>()
         .ToFeedIterator();

    var results = new List<WeatherForecast>();

    while (feed.HasMoreResults)
    {
        foreach (var session in await feed.ReadNextAsync())
        {
            results.Add(session);
        }
    }

    return results;
});

app.MapPost("/weatherforecast", async ([FromServices] CosmosClient client, [FromBody] WeatherForecast forecast) =>
{
    var db = client.GetDatabase(ResourceKeys.COSMOS_DB_NAME);
    var container = db.GetContainer(ResourceKeys.COSMOS_CONTAINER_NAME);

    var result = await container.UpsertItemAsync(forecast);

    return result.StatusCode == System.Net.HttpStatusCode.Created || result.StatusCode == System.Net.HttpStatusCode.OK
    ? Results.Ok(forecast)
    : Results.Problem("Failed to create item", statusCode: (int)result.StatusCode);
});


app.MapPost("/chat", async ([FromServices] MainChatKernelService kernelService, [FromBody] string prompt) => await kernelService.ChatAsync(prompt));

app.UseSwagger();
app.UseSwaggerUI();

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary, string Id)
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
