namespace System.Collections
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    [Serializable]
    public sealed partial class TypeComparer : IComparer, IEqualityComparer, IComparer<Type>, IEqualityComparer<Type>
    {
        public static readonly TypeComparer Default = new();

        public TypeComparer() { }

        public Int32 Compare(Type x, Type y) => x == y ? 0 : x is null ? 1 : y is null ? -1 : StringComparer.CurrentCultureIgnoreCase.Compare(x.ToString(), y.ToString());

        public Boolean Equals(Type x, Type y) => x == y;

        public Int32 GetHashCode(Type obj) => RuntimeHelpers.GetHashCode(obj);

        Int32 IComparer.Compare(Object x, Object y) => ReferenceEquals(x, y) ? 0 : x is null ? 1 : y is null ? -1 : StringComparer.CurrentCultureIgnoreCase.Compare(x is Type xt ? xt.ToString() : x.GetType().ToString(), y is Type yt ? yt.ToString() : y.GetType().ToString());

        Boolean IEqualityComparer.Equals(Object x, Object y) => ReferenceEquals(x, y) || (x is not null && y is not null && (x is Type xt ? xt : x.GetType()) == (y is Type yt ? yt : y.GetType()));

        Int32 IEqualityComparer.GetHashCode(Object obj) => RuntimeHelpers.GetHashCode(obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Boolean Equals(Object obj) => obj is TypeComparer;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Int32 GetHashCode() => RuntimeHelpers.GetHashCode(this);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override String ToString() => nameof(TypeComparer);
    }
}