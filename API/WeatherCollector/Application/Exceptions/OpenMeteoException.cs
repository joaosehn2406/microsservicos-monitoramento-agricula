namespace WeatherCollector.Application.Exceptions;

public sealed class OpenMeteoException : Exception
{
    public OpenMeteoException(string message)
        : base(message)
    {
    }
}
