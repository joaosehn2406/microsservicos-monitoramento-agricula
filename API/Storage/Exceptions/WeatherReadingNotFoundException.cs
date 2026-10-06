namespace Storage.Exceptions;

public sealed class WeatherReadingNotFoundException : Exception
{
    public WeatherReadingNotFoundException()
        : base("Weather reading was not found.")
    {
    }
}
