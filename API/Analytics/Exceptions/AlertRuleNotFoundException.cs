namespace Analytics.Exceptions;

public sealed class AlertRuleNotFoundException : Exception
{
    public AlertRuleNotFoundException()
        : base("Alert rule was not found.")
    {
    }
}
