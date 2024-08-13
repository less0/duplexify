using duplexify.Application;
using duplexify.Application.Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IConfigDirectoryService, ConfigDirectoryService>();
builder.Services.AddSingleton<IConfigurationValidator, ConfigurationValidator>();

// We are using this approach for WatchDirectoryWorker to be able to reference IPdfMerger
builder.Services.AddSingleton<PdfMerger>();
builder.Services.AddSingleton<IPdfMerger>(x => x.GetRequiredService<PdfMerger>());
builder.Services.AddHostedService(x => x.GetRequiredService<PdfMerger>());
builder.Services.AddHostedService<WatchDirectoryWorker>();

var host = builder.Build();

host.Services
    .GetRequiredService<IConfigurationValidator>()
    .ThrowOnInvalidConfiguration();

host.Run();
