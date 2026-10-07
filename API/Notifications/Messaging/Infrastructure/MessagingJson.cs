using System.Text.Json;

namespace Notifications.Messaging.Infrastructure;

public static class MessagingJson
{
    /// <summary>
    /// camelCase; a missing <c>required</c> member or a null in a non-nullable member
    /// fails deserialization, which the consumer treats as a permanent error.
    /// </summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true
    };
}
