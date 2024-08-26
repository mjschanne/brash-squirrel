var builder = DistributedApplication.CreateBuilder(args);

var insights = builder.AddAzureApplicationInsights("appinsights");


var openAi = builder.AddAzureOpenAI("AZURE-OPENAI-CONNSTR")
//var openAi = builder.AddAzureOpenAI("openAi")   ///                    TODO: move to this during a time where I'm not working much so that I can let the dust settle on the rg
    .AddDeployment(
        // https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models#gpt-4-and-gpt-4-turbo-model-availability
        new AzureOpenAIDeployment(
            name: "chat",
            modelName: "gpt-4o-mini",
            modelVersion: "2024-07-18",
            skuName: "GlobalStandard",
            skuCapacity: 1000));

var apiService = builder.AddProject<Projects.BrashSquirrel_ApiService>("apiservice")
    .WithExternalHttpEndpoints() // I want to be able to hit my API directly in deployed state for testing
    .WithReference(insights)
    .WithReference(openAi);

builder.AddProject<Projects.BrashSquirrel_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(insights);


builder.Build().Run();
