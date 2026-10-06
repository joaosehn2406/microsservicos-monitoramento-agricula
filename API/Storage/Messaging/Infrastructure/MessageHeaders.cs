using System.Globalization;
using System.Text;

namespace Storage.Messaging.Infrastructure;

/// <summary>
/// Header names used by retry/DLQ handling and tolerant readers: the broker decodes AMQP
/// strings as byte[], and messages published from the management UI carry strings only.
/// </summary>
public static class MessageHeaders
{
    public const string RetryCount = "x-retry-count";
    public const string Reprocessable = "x-reprocessable";
    public const string FailureReason = "x-failure-reason";
    public const string DlqReprocessCount = "x-dlq-reprocess-count";

    public static int GetInt(IDictionary<string, object?>? headers, string name)
    {
        if (headers is null || !headers.TryGetValue(name, out var value) || value is null)
        {
            return 0;
        }

        return value switch
        {
            int number => number,
            long number => (int)number,
            short number => number,
            byte number => number,
            sbyte number => number,
            uint number => (int)number,
            ushort number => number,
            byte[] bytes when int.TryParse(Encoding.UTF8.GetString(bytes), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var parsed) => parsed,
            string text when int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture,
                out var parsed) => parsed,
            _ => 0
        };
    }

    /// <summary>Returns null when the header is absent or unreadable.</summary>
    public static bool? GetBool(IDictionary<string, object?>? headers, string name)
    {
        if (headers is null || !headers.TryGetValue(name, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            bool flag => flag,
            byte[] bytes when bool.TryParse(Encoding.UTF8.GetString(bytes), out var parsed) => parsed,
            string text when bool.TryParse(text, out var parsed) => parsed,
            _ => null
        };
    }

    public static string? GetString(IDictionary<string, object?>? headers, string name)
    {
        if (headers is null || !headers.TryGetValue(name, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            string text => text,
            _ => Convert.ToString(value, CultureInfo.InvariantCulture)
        };
    }
}
