using BuscarRegistroSanitarioService.services;

namespace BuscarRegistroSanitarioService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ScrapingService _scrapingService;

    public Worker(ILogger<Worker> logger, ScrapingService scrapingService)
    {
        _logger = logger;
          _scrapingService = scrapingService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }

     public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker is stopping.");

        _scrapingService.Dispose();

        await base.StopAsync(cancellationToken);
    }
}
