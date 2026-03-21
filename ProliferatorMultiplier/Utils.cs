using System;

namespace ProliferatorMultiplier;

internal static class Utils
{
#if DEBUG
    internal const bool IsDev = true;
#else
    internal const bool IsDev = false;
#endif

    public static T[] CopyTable<T>(T[] original)
    {
        var copy = new T[original.Length];
        Array.Copy(original, copy, original.Length);
        return copy;
    }
}
