namespace WeatherCollector.Exceptions;

public sealed class OpenMeteoException : Exception
{
    public OpenMeteoException(string message)
        : base(message)
    {
    }
}
