var builder = DistributedApplication.CreateBuilder(args);

var insights = builder.AddAzureApplicationInsights("appinsights");

var openAi = builder.AddAzureOpenAI("AZURE-OPENAI-CONNSTR")
    .AddDeployment(
        // https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models#gpt-4-and-gpt-4-turbo-model-availability
        new AzureOpenAIDeployment(
            name: "chat",
            modelName: "gpt-4o-mini",
            modelVersion: "2024-07-18",
            skuName: "GlobalStandard",
            skuCapacity: 2000));

var apiService = builder.AddProject<Projects.BrashSquirrel_ApiService>("apiservice")
    .WithReference(insights)
    .WithReference(openAi);

builder.AddProject<Projects.BrashSquirrel_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(insights);

builder.Build().Run();
