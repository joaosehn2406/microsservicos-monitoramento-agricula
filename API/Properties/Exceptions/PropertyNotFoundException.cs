namespace Properties.Exceptions;

public sealed class PropertyNotFoundException : Exception
{
    public PropertyNotFoundException()
        : base("Property was not found.")
    {
    }
}
