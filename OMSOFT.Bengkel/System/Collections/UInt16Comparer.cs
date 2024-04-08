namespace System.Collections
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
#if CLS
    [CLSCompliant(false)]
#endif
    [Serializable]
    public sealed partial class UInt16Comparer : IComparer, IEqualityComparer, IComparer<UInt16>, IEqualityComparer<UInt16>
    {
        public static readonly UInt16Comparer Default = new();

        public UInt16Comparer() { }

        public Int32 Compare(UInt16 x, UInt16 y) => x < y ? -1 : x > y ? 1 : 0;

        public Boolean Equals(UInt16 x, UInt16 y) => x == y;

        public Int32 GetHashCode(UInt16 obj) => obj;

        Int32 IComparer.Compare(Object x, Object y) => ReferenceEquals(x, y) ? 0 : this.Compare(x is null ? (UInt16)0 : Convert.ToUInt16(x, CultureInfo.CurrentCulture), y is null ? (UInt16)0 : Convert.ToUInt16(y, CultureInfo.CurrentCulture));

        Boolean IEqualityComparer.Equals(Object x, Object y) => ReferenceEquals(x, y) || this.Equals(x is null ? (UInt16)0 : Convert.ToUInt16(x, CultureInfo.CurrentCulture), y is null ? (UInt16)0 : Convert.ToUInt16(y, CultureInfo.CurrentCulture));

        Int32 IEqualityComparer.GetHashCode(Object obj) => obj is null ? 0 : this.GetHashCode(Convert.ToUInt16(obj, CultureInfo.InvariantCulture));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Boolean Equals(Object obj) => obj is UInt16Comparer;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Int32 GetHashCode() => RuntimeHelpers.GetHashCode(this);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override String ToString() => nameof(UInt16Comparer);
    }
}