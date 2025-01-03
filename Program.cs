using BuscarRegistroSanitarioService.Hubs;
using BuscarRegistroSanitarioService.services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
public class Program
{
    public static async Task Main(string[] args)
    {
        var serviceType = Environment.GetEnvironmentVariable("SERVICE_TYPE");

        var host = CreateHostBuilder(args).Build();

        var _hubContext = host.Services.GetRequiredService<IHubContext<NotificationHub>>();
        var scrapingService = host.Services.GetRequiredService<ScrapingService>();

        var apiTask = Task.Run(async () =>
        {
            await host.RunAsync();
        });

        var chromeTask = Task.Run(async () =>
        {
            await scrapingService.InicializarAsync();
            scrapingService.OnInitialized += async (sender, e) =>
            {
                await _hubContext.Clients.All.SendAsync("ReceiveStatus", "ChromeDriver Abierto.");
            };
        });

        await Task.WhenAny(apiTask, chromeTask);

        if (serviceType == "Worker")
        {
            Console.CancelKeyPress += async (sender, eventArgs) =>
            {
                Console.WriteLine("Interrupción Ctrl+C detectada. Cerrando la aplicación...");
                await scrapingService.Dispose();
                eventArgs.Cancel = true;
            };

            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopping.Register(async () =>
            {
                await scrapingService.Dispose();
                Console.WriteLine("ScrapingService liberado al detener la aplicación.");
            });
        }

        await Task.WhenAll(apiTask, chromeTask);
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddUserSecrets<Program>(optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}


