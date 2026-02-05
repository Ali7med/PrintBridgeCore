using PrinterServer.Api.Printing;
using PrinterServer.Api.Middleware;
using PrinterServer.Api.Services;
using PrinterServer.Api.Storage;
using Microsoft.Extensions.Hosting.WindowsServices;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
});

builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "PrintBridgeCore";
});

builder.Services.AddOpenApi();
builder.Services.AddControllers();

var sqlitePath = builder.Configuration["Storage:SqlitePath"] ?? "data/printer.db";
var baseDir = AppContext.BaseDirectory;
var fullPath = Path.GetFullPath(Path.Combine(baseDir, sqlitePath));
Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? baseDir);
var connectionString = $"Data Source={fullPath}";

builder.Services.AddSingleton(new SqliteDatabase(connectionString));
builder.Services.AddSingleton<IPrinterService, WindowsPrinterService>();
builder.Services.AddSingleton<ISettingsService, SqliteSettingsService>();
builder.Services.AddSingleton<IHistoryService, SqliteHistoryService>();
builder.Services.AddSingleton<ITokenService, SqliteTokenService>();
builder.Services.AddSingleton<IPrintService, InMemoryPrintService>();
builder.Services.AddSingleton<IPrintExecutor, PrintExecutor>();

builder.Services.AddSingleton<PrintQueueService>();
builder.Services.AddSingleton<IPrintQueue>(sp => sp.GetRequiredService<PrintQueueService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<PrintQueueService>());

var app = builder.Build();

var database = app.Services.GetRequiredService<SqliteDatabase>();
database.EnsureCreated();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseMiddleware<TokenAuthMiddleware>();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
