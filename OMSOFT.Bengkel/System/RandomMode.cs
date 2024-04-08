namespace System
{
    /// <summary>
    /// Represents the mode that used when generating sequential random data.
    /// </summary>
    public enum RandomMode
    {
        /// <summary>
        /// The generated random data is always random, that mean no caching are available for this mode. Multiple enumerations will always get different values.
        /// </summary>
        AlwaysRandom = 0x0,

        /// <summary>
        /// The generated random data will cached in memory, so after the first enumeration complete the values will be keep remain same for next enumeration.
        /// </summary>
        KeepAfterDone = 0x1,
    }
}