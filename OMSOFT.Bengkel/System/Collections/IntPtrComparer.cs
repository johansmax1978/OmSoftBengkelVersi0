namespace System.Collections
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    [Serializable]
    public sealed partial class IntPtrComparer : IComparer, IEqualityComparer, IComparer<nint>, IEqualityComparer<nint>
    {
        private static readonly BigInteger BigULong;

        private static Int32 ImplHashCode32(nint obj) => (Int32)obj;

        private static Int32 ImplHashCode64(nint obj) => unchecked((Int32)obj ^ (Int32)(obj >> 32));

        private static Int64 ParseInt64(String @string)
        {
            if (@string.Length == 0 || (@string = @string.Trim()).Length == 0)
            {
                return 0;
            }
            if (!Int64.TryParse(@string, NumberStyles.Integer, CultureInfo.CurrentCulture, out var integer))
            {
                return unchecked((Int32)integer);
            }
            if (@string.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || @string.StartsWith("&h", StringComparison.OrdinalIgnoreCase))
            {
                @string = @string.Substring(2);
                if (@string.Length == 0 || (@string.Length == 1 && @string[0] == '0'))
                {
                    return 0;
                }
            }
            if (!Int64.TryParse(@string, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out integer) || !Int64.TryParse(@string, NumberStyles.Any, CultureInfo.CurrentCulture, out integer))
            {
                return unchecked((Int32)integer);
            }
            return Boolean.TryParse(@string, out var boolean)
                ? boolean ? 1 : 0
                : throw new FormatException(String.Concat("Invalid string to parse as numerical value: ", @string.Length > 100 ? String.Concat(@string.Substring(0, 100), "...") : @string));
        }

        private static Int64 ParseInt64(Single value) => Single.IsNaN(value) || Single.IsInfinity(value) ? throw new NotFiniteNumberException("Cannot parse NaN or Infinite number to the integer number.") : unchecked((Int64)Math.Round(value, 0, MidpointRounding.AwayFromZero));

        private static Int64 ParseInt64(Double value) => Double.IsNaN(value) || Double.IsInfinity(value) ? throw new NotFiniteNumberException("Cannot parse NaN or Infinite number to the integer number.") : unchecked((Int64)Math.Round(value, 0, MidpointRounding.AwayFromZero));

        private static Int64 ParseInt64(Decimal value) => unchecked((Int64)Math.Round(value, 0, MidpointRounding.AwayFromZero));

        private static Int64 ParseInt64(Object value)
        {
            if (value is null)
            {
                return 0;
            }
            var code = Type.GetTypeCode(value.GetType());
            switch (code)
            {
                case TypeCode.Empty:
                    return 0;
                case TypeCode.Object:
                    break;
                case TypeCode.DBNull:
                    return 0;
                case TypeCode.Boolean:
                    return System.Convert.ToBoolean(value, CultureInfo.CurrentCulture) ? 1 : 0;
                case TypeCode.Char:
                    return System.Convert.ToChar(value, CultureInfo.CurrentCulture);
                case TypeCode.SByte:
                    return System.Convert.ToSByte(value, CultureInfo.CurrentCulture);
                case TypeCode.Byte:
                    return System.Convert.ToByte(value, CultureInfo.CurrentCulture);
                case TypeCode.Int16:
                    return System.Convert.ToInt16(value, CultureInfo.CurrentCulture);
                case TypeCode.UInt16:
                    return System.Convert.ToUInt16(value, CultureInfo.CurrentCulture);
                case TypeCode.Int32:
                    return System.Convert.ToInt32(value, CultureInfo.CurrentCulture);
                case TypeCode.UInt32:
                    return System.Convert.ToUInt32(value, CultureInfo.CurrentCulture);
                case TypeCode.Int64:
                    return System.Convert.ToInt64(value, CultureInfo.CurrentCulture);
                case TypeCode.UInt64:
                    return unchecked((Int64)System.Convert.ToUInt64(value, CultureInfo.CurrentCulture));
                case TypeCode.Single:
                    return ParseInt64(System.Convert.ToSingle(value, CultureInfo.CurrentCulture));
                case TypeCode.Double:
                    return ParseInt64(System.Convert.ToDouble(value, CultureInfo.CurrentCulture));
                case TypeCode.Decimal:
                    return ParseInt64(System.Convert.ToDecimal(value, CultureInfo.CurrentCulture));
                case TypeCode.DateTime:
                    throw new InvalidCastException("Cannot convert any of date and time object to the integer number. If you mean to convert as Epoch (UNIX time) number, use the \"DateTimeOffset.FromUnixTimeSeconds\" method instead.");
                case TypeCode.String:
                    return ParseInt64(System.Convert.ToString(value, CultureInfo.CurrentCulture));
                default:
                    break;
            }
            return value is BigInteger bigint
                ? (Int64)(UInt64)(bigint & BigULong)
                : throw new InvalidCastException($"Cannot convert the object of type \"{value.GetType()}\" into the integer data type.");
        }

        private static nint ImplConvert32(Object value) => unchecked((nint)ParseInt64(value));

        private static nint ImplConvert64(Object value) => (nint)ParseInt64(value);

        unsafe static IntPtrComparer()
        {
            BigULong = (BigInteger)UInt64.MaxValue;
            if (IntPtr.Size == 4)
            {
                ImplHashCode = &ImplHashCode32;
                ImplConvert = &ImplConvert32;
            }
            else
            {
                ImplHashCode = &ImplHashCode64;
                ImplConvert = &ImplConvert64;
            }
            Default = new();
        }

        private static readonly unsafe delegate*<nint, Int32> ImplHashCode;
        private static readonly unsafe delegate*<Object, nint> ImplConvert;

        public static readonly IntPtrComparer Default;

        public static unsafe nint Convert(Object value) => ImplConvert(value);

        public IntPtrComparer() { }

        public Int32 Compare(nint x, nint y) => x < y ? -1 : x > y ? 1 : 0;

        public Boolean Equals(nint x, nint y) => x == y;

        public unsafe Int32 GetHashCode(nint obj) => ImplHashCode(obj);

        unsafe Int32 IComparer.Compare(Object x, Object y) => ReferenceEquals(x, y) ? 0 : this.Compare(ImplConvert(x), ImplConvert(y));

        unsafe Boolean IEqualityComparer.Equals(Object x, Object y) => ReferenceEquals(x, y) || this.Equals(ImplConvert(x), ImplConvert(y));

        unsafe Int32 IEqualityComparer.GetHashCode(Object obj) => obj is null ? 0 : this.GetHashCode(ImplConvert(obj));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Boolean Equals(Object obj) => obj is IntPtrComparer;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Int32 GetHashCode() => RuntimeHelpers.GetHashCode(this);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override String ToString() => nameof(IntPtrComparer);
    }
}