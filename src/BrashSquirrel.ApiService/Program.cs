using Azure.Monitor.OpenTelemetry.AspNetCore;
using BrashSquirrel.ApiService;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

//builder.AddAzureOpenAIClient("AZURE-OPENAI-CONNSTR");
//builder.AddAzureOpenAIClient("openAi");


if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

var appInsightsConnStr = builder.Configuration.GetValue<string>("APPLICATIONINSIGHTS_CONNECTION_STRING");

var openaiConnStr = builder.Configuration.GetConnectionString("AZURE-OPENAI-CONNSTR");
var openaiConnStrDict = openaiConnStr.ParseAsConnectionString();
// todo: clean this up
// todo: add magic strings tying all this together to a shared library with a static class of constant strings that we use for type safety

builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<MyKernelService>(_ => new MyKernelService("chat", openaiConnStrDict["Endpoint"], appInsightsConnStr)); // serviceProvider => new MyKernelService(serviceProvider));

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

app.MapPost("/chat", async ([FromServices] MyKernelService kernelService, [FromBody] string prompt) => // [FromServices] OpenAIClient client, 
{

    return await kernelService.ChatAsync(prompt);
    //var chat = client.GetChatClient("chat");

    //var clientResult = await chat.CompleteChatAsync(prompt);

    //return clientResult.Value;
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
