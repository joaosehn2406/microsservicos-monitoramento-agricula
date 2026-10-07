namespace BackendApi.Services;

/// <summary>Allowed values of the alert_rules CHECK constraints (SPEC 3.2).</summary>
public static class DomainValues
{
    public static readonly HashSet<string> Variables = ["temperature", "humidity", "precipitation", "soil_moisture"];
    public static readonly HashSet<string> Operators = [">", "<", ">=", "<="];
    public static readonly HashSet<string> Severities = ["low", "medium", "high", "critical"];
}
