namespace Analytics.Configurations;

public sealed class DlqReprocessOptions
{
    public const string SectionName = "Messaging:DlqReprocess";

    public int IntervalMinutes { get; init; } = 5;
}
