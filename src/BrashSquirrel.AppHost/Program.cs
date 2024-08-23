var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.BrashSquirrel_ApiService>("apiservice");

builder.AddProject<Projects.BrashSquirrel_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
