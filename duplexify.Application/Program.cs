using duplexify.Application.Configuration;
using duplexify.Application.Contracts;
using duplexify.Application.Contracts.Configuration;
using duplexify.Application.Workers;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IConfigDirectoryService, ConfigDirectoryService>();
builder.Services.AddSingleton<IConfigValidator, ConfigValidator>();
builder.Services.AddSingleton<IPdfMergerConfiguration, PdfMergerConfiguration>();
builder.Services.AddSingleton<IWatchDirectoryWorkerConfiguration, WatchDirectoryWorkerConfiguration>();

// We are using this approach for WatchDirectoryWorker to be able to reference IPdfMerger
builder.Services.AddSingleton<PdfMerger>();
builder.Services.AddSingleton<IPdfMerger>(x => x.GetRequiredService<PdfMerger>());
builder.Services.AddHostedService(x => x.GetRequiredService<PdfMerger>());
builder.Services.AddHostedService<WatchDirectoryWorker>();

var host = builder.Build();
 
host.Services
    .GetRequiredService<IConfigValidator>()
    .ThrowOnInvalidConfiguration();

if (host.Configuration.GetValue<bool>("AddHealthcheckEndpoint"))
{
    host.MapGet("/health", () => "healthy");
}

host.Run();
