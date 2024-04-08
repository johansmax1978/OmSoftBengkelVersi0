namespace System
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides random <typeparamref name="T"/> data generation sets.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of random data to generate.</typeparam>
    public interface IRandomSet<T> : IEnumerable<T>
    {
        /// <summary>
        /// Represents the total count (length) of the random <typeparamref name="T"/> to generate.
        /// </summary>
        /// <value>The <see cref="Int32"/> number as total count of <typeparamref name="T"/> to generate in the current set</value>
        /// <filterpriority>2</filterpriority>
        Int32 Count { get; }

        /// <summary>
        /// Represents the persistence option of the current <see cref="IRandomSet{T}"/> instance.
        /// </summary>
        /// <value>One of the <see cref="RandomMode"/> constant, the default is <see cref="RandomMode.AlwaysRandom"/>.</value>
        /// <filterpriority>2</filterpriority>
        RandomMode Option { get; }

        /// <summary>
        /// Represents the custom function that used to filter the generated random <typeparamref name="T"/> data, or <see langword="null"/> if not used.
        /// </summary>
        /// <value>The <see cref="RandomFilter{T}"/> function delegate that provides random data filtering, or <see langword="null"/> if not available.</value>
        RandomFilter<T> Filter { get; }

        /// <summary>
        /// Represents the total count of random <typeparamref name="T"/> that already cached, only used if the <see cref="Option"/> is <see cref="RandomMode.KeepAfterDone"/>.
        /// </summary>
        /// <value>The <see cref="Int32"/> number, ranged from 0 (nothing are cached) until <see cref="Count"/> of <typeparamref name="T"/> to generate in the current set instance.<br/>
        /// If <see cref="Option"/> is <see cref="RandomMode.AlwaysRandom"/> option, the <see cref="Cached"/> value is always zero (never cached).</value>
        Int32 Cached { get; }

        /// <summary>
        /// Writes the random data in the current set into the destination <paramref name="array"/>, starting at specified <paramref name="arrayIndex"/>.
        /// </summary>
        /// <param name="array">Specify the destination array of <typeparamref name="T"/> that should writen by the random data from the current set.</param>
        /// <param name="arrayIndex">Set the starting index of the element in the <paramref name="array"/> to begin writen by copied random data.</param>
        /// <exception cref="ArgumentNullException">Thrown if the destination <paramref name="array"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="arrayIndex"/> is outside the valid range of <paramref name="array"/>, -or the <paramref name="array"/> is not have sufficient spaces to store the random data for the current <see cref="Count"/> number, starting at specified <paramref name="arrayIndex"/>.</exception>
        void CopyTo(T[] array, Int32 arrayIndex);

        /// <summary>
        /// Retireve the reference (read only) of random <typeparamref name="T"/> that cached in this <see cref="IRandomSet{T}"/> at specified entry <paramref name="index"/>.<br/>
        /// This indexer is only supported if the current <see cref="Option"/> is set with <see cref="RandomMode.KeepAfterDone"/>.
        /// </summary>
        /// <param name="index">The index of cached random number to retrieve, the negative value mean the tailing index.</param>
        /// <returns>The read only reference of the cached random number at specified <paramref name="index"/>, represented as <see cref="Int32"/> data type.</returns>
        /// <exception cref="NotSupportedException">Always thrown if the current <see cref="Option"/> is not <see cref="RandomMode.KeepAfterDone"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> is outside the valid range of the current <see cref="IRandomSet{T}"/>.</exception>
        /// <filterpriority>2</filterpriority>
        ref readonly T this[Int32 index] { get; }
    }
}