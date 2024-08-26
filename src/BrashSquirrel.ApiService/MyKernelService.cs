using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace BrashSquirrel.ApiService
{
    public class MyKernelService
    {
        private Kernel _kernel;
        //private readonly OpenAIPromptExecutionSettings _promptSettings;

        public MyKernelService(string modelId, string endpoint, string appInsightsConnStr)// IServiceProvider serviceProvider)
        {
            var credential = new DefaultAzureCredential();

            var builder = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(modelId, endpoint, credential);

            // Add enterprise components
            builder.Services.AddLogging(services => services.AddOpenTelemetry(options => {
                options.AddAzureMonitorLogExporter(options => options.ConnectionString = appInsightsConnStr);
                // Format log messages. This defaults to false.
                options.IncludeFormattedMessage = true;
            }));

            // Build the kernel
            _kernel = builder.Build();

            // todo: add memory and plugins and all that good stuff
        }


        public async Task<string> ChatAsync(string prompt)
        {
            var chatClient = _kernel.GetRequiredService<IChatCompletionService>();

            var result = await chatClient.GetChatMessageContentAsync(prompt);

            return result.ToString();
        }
    }
}
