var builder = DistributedApplication.CreateBuilder(args);

var insights = builder.AddAzureApplicationInsights("appinsights");

// can't use shared class library for constants yet https://github.com/dotnet/aspire/issues/2769
// for now, whenever you go to modify a magic string in this file just be sure to check if it has
// a corresponding constant in the shared library
var openAi = builder.AddAzureOpenAI("openai")
//var openAi = builder.AddAzureOpenAI("openAi")   ///                    TODO: move to this during a time where I'm not working much so that I can let the dust settle on the rg
    .AddDeployment(
        // https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models#gpt-4-and-gpt-4-turbo-model-availability
        new AzureOpenAIDeployment(
            name: "chat",
            modelName: "gpt-4o-mini",
            modelVersion: "2024-07-18",
            skuName: "GlobalStandard",
            skuCapacity: 1000));

var cosmos = builder.AddAzureCosmosDB("cosmos");
var cosmosdb = cosmos.AddDatabase("chatHistory");

var apiService = builder.AddProject<Projects.BrashSquirrel_ApiService>("apiservice")
    .WithExternalHttpEndpoints() // I want to be able to hit my API directly in deployed state for testing
    .WithReference(insights)
    .WithReference(openAi)
    ;
    //.WithReference(cosmosdb);

builder.AddProject<Projects.BrashSquirrel_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(insights);


builder.Build().Run();
