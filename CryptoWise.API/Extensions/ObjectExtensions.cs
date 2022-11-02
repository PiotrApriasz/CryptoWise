namespace CryptoWise.API.Extensions;

public static class ObjectExtensions
{
    public static void CheckForNull<T>(this T? obj)
    {
        if (obj is null)
            throw new NullReferenceException("Object shouldn't be null at this place");
    }
}