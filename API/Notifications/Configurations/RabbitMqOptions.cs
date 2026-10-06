namespace Notifications.Configurations;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; init; } = "rabbitmq";
    public int Port { get; init; } = 5672;
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string Exchange { get; init; } = "farm.events";
    public string Queue { get; init; } = "q.notifications";
    public string BindingKey { get; init; } = "alert.#";
}
