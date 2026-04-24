namespace SportClubApi.Services;

public class MetricsUpdateWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MetricsUpdateWorker> _logger;

    public MetricsUpdateWorker(IServiceProvider serviceProvider, ILogger<MetricsUpdateWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("📊 Сервис метрик запущен");

        // Первичное обновление после запуска
        await UpdateMetrics();

        // Периодическое обновление каждые 30 секунд
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await UpdateMetrics();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("📊 Сервис метрик остановлен");
        }
    }

    private async Task UpdateMetrics()
    {
        using var scope = _serviceProvider.CreateScope();
        var metricsService = scope.ServiceProvider.GetRequiredService<BusinessMetricsService>();
        await metricsService.UpdateMetricsAsync();
    }
}
