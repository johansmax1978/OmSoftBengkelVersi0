namespace System
{
    /// <summary>
    /// Represents a delegate function that supported to filter or control the produced random <typeparamref name="T"/> data in set.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> of the random data that produced and should be filtered or controlled through function.</typeparam>
    /// <param name="index">The index or position of random data in the underlying set that should be controlled or filtered.</param>
    /// <param name="value">The current produced or generated random <typeparamref name="T"/> data to control or filter using the current function.</param>
    /// <returns><see langword="true"/> if the <paramref name="value"/> is accepted through this filter; otheriwse, <see langword="false"/>.</returns>
    [Serializable]
    public delegate Boolean RandomFilter<T>(Int32 index, T value);
}