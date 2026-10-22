namespace Notifications.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; set; } = "rabbitmq";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public ushort PrefetchCount { get; set; } = 10;
    public string Exchange { get; set; } = "farm.events";
    public string Queue { get; set; } = "q.notifications";
    public string BindingKey { get; set; } = "alert.#";
    public string ClientName { get; set; } = "notifications";
}
