namespace System
{
    /// <summary>
    /// Provides conversion between .NET <see cref="DateTime"/> and UNIX epoch time (vice versa), this class is <see langword="static"/>.
    /// </summary>
    public static partial class Epoch
    {
        /// <summary>
        /// The begining UTC date and time of the UNIX epoch time, this field is read-only.
        /// </summary>
        public static readonly DateTime BeginUTC = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// The begining local date and time of the UNIX epoch time, this field is read-only.
        /// </summary>
        public static readonly DateTime BeginLocal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);

        /// <summary>
        /// Retrieve the current UTC epoch time number.
        /// </summary>
        /// <returns>The <see cref="Int64"/> value as the current epoch time.</returns>
        public static Int64 GetEpoch() => (Int32)Math.Round((DateTime.UtcNow - BeginUTC).TotalSeconds, 0, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Retrieve UNIX epoch time (as <see cref="Int64"/> number) for the given <see cref="DateTime"/>.
        /// </summary>
        /// <param name="date">Specify the <see cref="DateTime"/> value that should converted as UNIX epoch time.</param>
        /// <returns>The <see cref="Int64"/> number as UNIX epoch time from the given <see cref="DateTime"/> value.</returns>
        /// <filterpriority>2</filterpriority>
        public static Int64 ToEpoch(this DateTime date) => (Int32)Math.Round(((date.Kind == DateTimeKind.Utc ? date : date.ToUniversalTime()) - BeginUTC).TotalSeconds, 0, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Retrieve <see cref="DateTime"/> object from the given UNIX <paramref name="epoch"/> time.
        /// </summary>
        /// <param name="epoch">The <see cref="Int64"/> value as UNIX epoch time to convert to <see cref="DateTime"/>.</param>
        /// <returns>The <see cref="DateTime"/> value as converted time from given <paramref name="epoch"/> time.</returns>
        public static DateTime ToDateTime(Int64 epoch) => BeginUTC.AddSeconds(epoch);

        /// <summary>
        /// Retrieve <see cref="DateTime"/> object from the given UNIX <paramref name="epoch"/> time.
        /// </summary>
        /// <param name="epoch">The <see cref="Double"/> value as UNIX epoch time to convert to <see cref="DateTime"/>.</param>
        /// <returns>The <see cref="DateTime"/> value as converted time from given <paramref name="epoch"/> time.</returns>
        public static DateTime ToDateTime(Double epoch) => BeginUTC.AddSeconds(epoch);

        /// <summary>
        /// Retrieve the UNIX epoch time from the current actual local date and time.
        /// </summary>
        /// <returns>The <see cref="Int64"/> value as the UNIX epoch time of the current local date and time.</returns>
        public static Int64 LocalTime() => (Int32)Math.Round((DateTime.Now - BeginLocal).TotalSeconds, 0, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Retrieve the UNIX epoch time from the specified <see cref="DateTime"/> <paramref name="target"/>.
        /// </summary>
        /// <param name="target">Set the target of <see cref="DateTime"/> to convert as UNIX epoch time.</param>
        /// <returns>The <see cref="Int64"/> value as UNIX epoch time of the specified <see cref="DateTime"/> <paramref name="target"/>.</returns>
        public static Int64 LocalTime(DateTime target) => (Int32)Math.Round(((target.Kind == DateTimeKind.Local ? target : target.ToLocalTime()) - BeginLocal).TotalSeconds, 0, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Retrieve local <see cref="DateTime"/> object from the given UNIX <paramref name="epoch"/> time.
        /// </summary>
        /// <param name="epoch">The <see cref="Int64"/> value as UNIX epoch time to convert to local <see cref="DateTime"/>.</param>
        /// <returns>The local <see cref="DateTime"/> value as converted time from given <paramref name="epoch"/> time.</returns>
        public static DateTime LocalDate(Int64 epoch) => BeginLocal.AddSeconds(epoch);

        /// <summary>
        /// Retrieve local <see cref="DateTime"/> object from the given UNIX <paramref name="epoch"/> time.
        /// </summary>
        /// <param name="epoch">The <see cref="Double"/> value as UNIX epoch time to convert to local <see cref="DateTime"/>.</param>
        /// <returns>The local <see cref="DateTime"/> value as converted time from given <paramref name="epoch"/> time.</returns>
        public static DateTime LocalDate(Double epoch) => Double.IsNaN(epoch) || Double.IsInfinity(epoch) || epoch == 0 ? BeginLocal : BeginLocal.AddSeconds(epoch);
    }
}