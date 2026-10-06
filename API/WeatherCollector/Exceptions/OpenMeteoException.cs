namespace WeatherCollector.Exceptions;

public sealed class OpenMeteoException : Exception
{
    public OpenMeteoException()
        : base("The weather provider request failed.")
    {
    }
}
