using System;

namespace ProliferatorMultiplier;

public static class Utils
{
    #if DEBUG
        internal const bool IsDev = true;
    #else
        internal const bool IsDev = false;
    #endif
    
    public static T[] CopyTable<T>(ref T[] original)
    {
        var backup = new T[original.Length];
        Array.Copy(original, backup, original.Length);
        return backup;
    }
}