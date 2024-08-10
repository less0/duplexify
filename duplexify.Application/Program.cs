using duplexify.Application;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<WatchDirectoryWorker>();
builder.Services.AddSingleton<IPdfMerger, PdfMerger>();
builder.Services.AddSingleton<IConfigDirectoryService, ConfigDirectoryService>();
builder.Services.AddSingleton<IConfigurationValidator, ConfigurationValidator>();

var host = builder.Build();

host.Services
    .GetRequiredService<IConfigurationValidator>()
    .ThrowOnInvalidConfiguration();

host.Run();
