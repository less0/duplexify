using duplexify.Application;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<WatchDirectoryWorker>();
builder.Services.AddSingleton<IPdfMerger, PdfMerger>();
builder.Services.AddSingleton<IConfigDirectoryService, ConfigDirectoryService>();

var host = builder.Build();
host.Run();
