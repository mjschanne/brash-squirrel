using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BrashSquirrel.ApiService.Services;

public class MainChatKernelService
{
    private Kernel _kernel;

    public MainChatKernelService(string modelId, string endpoint, string appInsightsConnStr)
    {
        var credential = new DefaultAzureCredential();

        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(modelId, endpoint, credential);

        builder.Services.AddLogging(services => services.AddOpenTelemetry(options =>
        {
            options.AddAzureMonitorLogExporter(options => options.ConnectionString = appInsightsConnStr);
            options.IncludeFormattedMessage = true;
        }));

        _kernel = builder.Build();
    }


    public async Task<string> ChatAsync(string prompt)
    {
        var chatClient = _kernel.GetRequiredService<IChatCompletionService>();

        var result = await chatClient.GetChatMessageContentAsync(prompt);

        return result.ToString();
    }
}
