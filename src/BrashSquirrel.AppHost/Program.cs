var builder = DistributedApplication.CreateBuilder(args);

var insights = builder.AddAzureApplicationInsights("appinsights");

var apiService = builder.AddProject<Projects.BrashSquirrel_ApiService>("apiservice")
    .WithReference(insights);

builder.AddProject<Projects.BrashSquirrel_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(insights);

builder.Build().Run();
