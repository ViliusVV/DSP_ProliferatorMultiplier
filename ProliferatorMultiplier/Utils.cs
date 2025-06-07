namespace ProliferatorMultiplier;

public static class Utils
{
    #if DEBUG
        internal const bool IsDev = true;
    #else
        internal const bool IsDev = false;
    #endif
}