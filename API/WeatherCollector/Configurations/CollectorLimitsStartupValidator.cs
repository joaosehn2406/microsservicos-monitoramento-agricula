using System.Globalization;
using Microsoft.Extensions.Options;

namespace WeatherCollector.Configurations;

public sealed class CollectorLimitsStartupValidator(
    IOptions<CollectorOptions> collectorOptions,
    IOptions<OpenMeteoOptions> openMeteoOptions,
    ILogger<CollectorLimitsStartupValidator> logger) : IHostedService
{
    private readonly CollectorOptions _collectorOptions = collectorOptions.Value;
    private readonly OpenMeteoOptions _openMeteoOptions = openMeteoOptions.Value;
    private readonly ILogger<CollectorLimitsStartupValidator> _logger = logger;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var intervalSeconds = _collectorOptions.IntervalSeconds;
        ValidatePositiveConfiguration(intervalSeconds);

        var totalProperties = FarmCatalog.TotalProperties;
        var requestsPerMinute = totalProperties * 60d / intervalSeconds;
        var requestsPerHour = totalProperties * 3600d / intervalSeconds;
        var requestsPerDay = totalProperties * 86400d / intervalSeconds;
        var limits = _openMeteoOptions.Limits;

        var violations = new List<string>();
        AddViolation(
            violations,
            "por minuto",
            requestsPerMinute,
            limits.RequestsPerMinute,
            totalProperties * 60d / limits.RequestsPerMinute);
        AddViolation(
            violations,
            "por hora",
            requestsPerHour,
            limits.RequestsPerHour,
            totalProperties * 3600d / limits.RequestsPerHour);
        AddViolation(
            violations,
            "por dia",
            requestsPerDay,
            limits.RequestsPerDay,
            totalProperties * 86400d / limits.RequestsPerDay);

        if (violations.Count > 0)
        {
            var minimumInterval = Math.Max(
                totalProperties * 60d / limits.RequestsPerMinute,
                Math.Max(
                    totalProperties * 3600d / limits.RequestsPerHour,
                    totalProperties * 86400d / limits.RequestsPerDay));

            throw new InvalidOperationException(
                $"A configuração do coletor excede os limites da Open-Meteo: " +
                $"{string.Join("; ", violations)}. " +
                $"Para {totalProperties} propriedades, use Collector:IntervalSeconds >= " +
                $"{minimumInterval.ToString("0.###", CultureInfo.InvariantCulture)} s.");
        }

        _logger.LogInformation(
            "Consumo previsto da Open-Meteo para {PropertyCount} propriedades e intervalo de {IntervalSeconds}s: " +
            "{RequestsPerMinute:F2} req/min, {RequestsPerHour:F2} req/h e {RequestsPerDay:F2} req/dia",
            totalProperties,
            intervalSeconds,
            requestsPerMinute,
            requestsPerHour,
            requestsPerDay);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void ValidatePositiveConfiguration(double intervalSeconds)
    {
        var limits = _openMeteoOptions.Limits;
        if (!double.IsFinite(intervalSeconds) || intervalSeconds <= 0 ||
            !double.IsFinite(_openMeteoOptions.TimeoutSeconds) || _openMeteoOptions.TimeoutSeconds <= 0 ||
            !double.IsFinite(limits.RequestsPerMinute) || limits.RequestsPerMinute <= 0 ||
            !double.IsFinite(limits.RequestsPerHour) || limits.RequestsPerHour <= 0 ||
            !double.IsFinite(limits.RequestsPerDay) || limits.RequestsPerDay <= 0)
        {
            throw new InvalidOperationException(
                "Collector:IntervalSeconds, OpenMeteo:TimeoutSeconds e todos os valores de " +
                "OpenMeteo:Limits devem ser números finitos maiores que zero.");
        }

        if (!Uri.TryCreate(_openMeteoOptions.BaseUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("OpenMeteo:BaseUrl deve ser uma URL absoluta válida.");
        }
    }

    private static void AddViolation(
        ICollection<string> violations,
        string period,
        double predicted,
        double limit,
        double minimumInterval)
    {
        if (predicted <= limit)
        {
            return;
        }

        violations.Add(
            $"limite {period} calculado em " +
            $"{predicted.ToString("0.###", CultureInfo.InvariantCulture)} " +
            $"(máximo {limit.ToString("0.###", CultureInfo.InvariantCulture)}); " +
            $"intervalo mínimo {minimumInterval.ToString("0.###", CultureInfo.InvariantCulture)} s");
    }
}
