namespace System.Runtime.CompilerServices
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    /// <summary>
    /// Provides metadata formatter for any of <see cref="Type"/>, <see cref="MethodInfo"/>, and <see cref="PropertyInfo"/>, that can displayed in C# format. This class is not inheritable.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public static partial class IdentityFactory
    {
        #region >>> internal members

        static IdentityFactory()
        {
            var rank = new String[33];
            var buff = new StringBuilder(512);
            for (var i = 2; i <= 32; i++)
            {
                rank[i] = buff.Append('[').Append(',', i - 1).Append(']').ToString();
                buff.Length = 0;
            }
            rank[0] = "";
            rank[1] = "[]";
            RankSymbols = rank;
            UnmanagedSet = new(0xff, MultiTypeComparer.Default);
            TypeVoid = typeof(void);
            TypeVoidPtr = typeof(void*);
            ConstraintComparer = ConstraintComparer_;
            RuntimeHelpers.PrepareDelegate(ConstraintComparer);
            CachedTypeID = new(0xff, MultiTypeComparer.Default);
            CachedMethodID = new(0xff, MultiTypeComparer.Default);
            CachedPropertyID = new(0xff, MultiTypeComparer.Default);
            CachedEventID = new(0xff, MultiTypeComparer.Default);
            CachedFieldID = new(0xff, MultiTypeComparer.Default);
            CachedClassID = new(0xff, MultiTypeComparer.Default);
            FullNameHash = SimpleANSIHash("FULLNAME");
            ConstraintHash = SimpleANSIHash("CONSTRAINT");
            ReturnTypeHash = SimpleANSIHash("RETURNTYPE");
            AttributesHash = SimpleANSIHash("ATTRIBUTES");
            WriteSpecsHash = SimpleANSIHash("WRITESPECS");
            var methods = typeof(Enum).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            for (var i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                if (method.Name == nameof(Enum.TryParse) && method.IsGenericMethodDefinition && method.GetParameters().Length == 3)
                {
                    EnumTryParse = method;
                    break;
                }
            }
        }

        private sealed partial class MultiTypeComparer : IEqualityComparer<Int32>, IEqualityComparer<Int64>, IEqualityComparer<Type>
        {
            public static readonly MultiTypeComparer Default = new();
            private MultiTypeComparer() { }
            public Boolean Equals(Int64 x, Int64 y) => x == y;
            public unsafe Int32 GetHashCode(Int64 obj)
            {
                const UInt32 FnvBasis = 0x811C9DC5, FnvPrime = 0x01000193;
                unchecked
                {
                    var hv = FnvBasis;
                    var ps = (Byte*)&obj;
                    hv = (hv ^ ps[0]) * FnvPrime;
                    hv = (hv ^ ps[1]) * FnvPrime;
                    hv = (hv ^ ps[2]) * FnvPrime;
                    hv = (hv ^ ps[3]) * FnvPrime;
                    hv = (hv ^ ps[4]) * FnvPrime;
                    hv = (hv ^ ps[5]) * FnvPrime;
                    hv = (hv ^ ps[6]) * FnvPrime;
                    return (Int32)((hv ^ ps[7]) * FnvPrime);
                }
            }
            public Boolean Equals(Int32 x, Int32 y) => x == y;
            public Int32 GetHashCode(Int32 obj) => obj;
            public Boolean Equals(Type x, Type y) => ReferenceEquals(x, y);
            public Int32 GetHashCode(Type obj) => RuntimeHelpers.GetHashCode(obj);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly String[] RankSymbols;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Dictionary<Int32, Boolean> UnmanagedSet;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Type TypeVoid, TypeVoidPtr;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Dictionary<Int64, String> CachedTypeID, CachedMethodID, CachedPropertyID, CachedEventID, CachedFieldID, CachedClassID;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Int32 FullNameHash, ConstraintHash, ReturnTypeHash, AttributesHash, WriteSpecsHash;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Comparison<String> ConstraintComparer;
        [ThreadStatic, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static StringBuilder CachedTypeBuilder_, CachedMethodBuilder_, CachedPropertyBuilder_, CachedEventBuilder_, CachedFieldBuilder_, CachedClassBuilder_;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly MethodInfo EnumTryParse;

        private static StringBuilder CachedTypeBuilder()
        {
            var value = CachedTypeBuilder_;
            if (value is null)
            {
                CachedTypeBuilder_ = value = new StringBuilder(8192);
            }
            return value.Length == 0 ? value : value.Clear();
        }

        private static StringBuilder CachedMethodBuilder()
        {
            var value = CachedMethodBuilder_;
            if (value is null)
            {
                CachedMethodBuilder_ = value = new StringBuilder(8192);
            }
            return value.Length == 0 ? value : value.Clear();
        }

        private static StringBuilder CachedPropertyBuilder()
        {
            var value = CachedPropertyBuilder_;
            if (value is null)
            {
                CachedPropertyBuilder_ = value = new StringBuilder(8192);
            }
            return value.Length == 0 ? value : value.Clear();
        }

        private static StringBuilder CachedEventBuilder()
        {
            var value = CachedEventBuilder_;
            if (value is null)
            {
                CachedEventBuilder_ = value = new StringBuilder(8192);
            }
            return value.Length == 0 ? value : value.Clear();
        }

        private static StringBuilder CachedFieldBuilder()
        {
            var value = CachedFieldBuilder_;
            if (value is null)
            {
                CachedFieldBuilder_ = value = new StringBuilder(8192);
            }
            return value.Length == 0 ? value : value.Clear();
        }

        private static StringBuilder CachedClassBuilder()
        {
            var value = CachedClassBuilder_;
            if (value is null)
            {
                CachedClassBuilder_ = value = new StringBuilder(8192);
            }
            return value.Length == 0 ? value : value.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Int32 ConstraintComparer_(String type1, String type2) => type1 is "notnull" or "class" or "new()" or "unmanaged" or "struct" ? -1 : type1 == type2 ? 0 : 1;

        private static unsafe Int64 CombineHash64(Int32 hash1, Int32 hash2, Int32 hash3, Int32 hash4)
        {
            unchecked
            {
                const UInt64 offset = 0xcbf29ce484222325ul, prime = 0x00000100000001B3ul;
                var hash = offset;
                var ps = (Byte*)&hash1;
                hash = (hash ^ ps[0]) * prime;
                hash = (hash ^ ps[1]) * prime;
                hash = (hash ^ ps[2]) * prime;
                hash = (hash ^ ps[3]) * prime;
                ps = (Byte*)&hash2;
                hash = (hash ^ ps[0]) * prime;
                hash = (hash ^ ps[1]) * prime;
                hash = (hash ^ ps[2]) * prime;
                hash = (hash ^ ps[3]) * prime;
                ps = (Byte*)&hash3;
                hash = (hash ^ ps[0]) * prime;
                hash = (hash ^ ps[1]) * prime;
                hash = (hash ^ ps[2]) * prime;
                hash = (hash ^ ps[3]) * prime;
                ps = (Byte*)&hash4;
                hash = (hash ^ ps[0]) * prime;
                hash = (hash ^ ps[1]) * prime;
                hash = (hash ^ ps[2]) * prime;
                hash = (hash ^ ps[3]) * prime;
                return (Int64)hash;
            }
        }

        private static unsafe Int32 CombineHash32(Int32 hash1, Int32 hash2, Int32 hash3, Int32 hash4)
        {
            unchecked
            {
                const UInt32 offset = 0x811C9DC5u, prime = 0x01000193u;
                var hash = offset;
                var ps = (Byte*)&hash1;
                hash = (hash ^ ps[0]) * prime;
                hash = (hash ^ ps[1]) * prime;
                hash = (hash ^ ps[2]) * prime;
                hash = (hash ^ ps[3]) * prime;
                ps = (Byte*)&hash2;
                hash = (hash ^ ps[0]) * prime;
                hash = (hash ^ ps[1]) * prime;
                hash = (hash ^ ps[2]) * prime;
                hash = (hash ^ ps[3]) * prime;
                ps = (Byte*)&hash3;
                hash = (hash ^ ps[0]) * prime;
                hash = (hash ^ ps[1]) * prime;
                hash = (hash ^ ps[2]) * prime;
                hash = (hash ^ ps[3]) * prime;
                ps = (Byte*)&hash4;
                hash = (hash ^ ps[0]) * prime;
                hash = (hash ^ ps[1]) * prime;
                hash = (hash ^ ps[2]) * prime;
                hash = (hash ^ ps[3]) * prime;
                return (Int32)hash;
            }
        }

        private static Int32 SimpleANSIHash(String value)
        {
            unchecked
            {
                unsafe
                {
                    const UInt32 offset = 0x811C9DC5u, prime = 0x01000193u;
                    var hash = offset;
                    var length = value.Length;
                    fixed (Char* chars = value)
                    {
                        var pointer = chars;
                        while (length-- > 0)
                        {
                            hash = (hash ^ (Byte)(*pointer++)) * prime;
                        }
                    }
                    return (Int32)hash;
                }
            }
        }

        private static Boolean IsUnsafeMethod(MethodInfo method)
        {
            if (method.ReturnType.IsPointer)
            {
                return true;
            }
            var args = method.GetParameters();
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].ParameterType.IsPointer)
                {
                    return true;
                }
            }
            return false;
        }

        private static Boolean IsUnsafeProperty(PropertyInfo property)
        {
            if (property.PropertyType.IsPointer)
            {
                return true;
            }
            var args = property.GetIndexParameters();
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].ParameterType.IsPointer)
                {
                    return true;
                }
            }
            return false;
        }

        private static Boolean IsOverriden(MethodInfo method)
            => method.GetBaseDefinition().DeclaringType != method.ReflectedType && !method.IsAbstract;

        private static void WriteEscape(StringBuilder into, String value)
        {
            _ = into.Append('\"');
            for (var i = 0; i < value.Length; i++)
            {
                var next = value[i];
                _ = next == '\"'
                    ? into.Append('\\').Append('\"')
                    : next == '\n'
                    ? into.Append('\\').Append('n')
                    : next == '\r'
                    ? into.Append('\\').Append('r')
                    : next == '\b'
                    ? into.Append('\\').Append('b')
                    : next == '\t'
                    ? into.Append('\\').Append('t')
                    : next == '\v'
                    ? into.Append('\\').Append('v')
                    : next == '\f'
                    ? into.Append('\\').Append('f')
                    : next == '\0'
                    ? into.Append('\\').Append('0')
                    : next == '\\'
                    ? into.Append("\\\\")
                    : into.Append(next);
            }
            _ = into.Append('\"');
        }

        private static void WriteEscape(StringBuilder into, Char value)
        {
            _ = into.Append('\'');
            _ = value switch
            {
                '\\' => into.Append("\\\\"),
                '\n' => into.Append("\\n"),
                '\r' => into.Append("\\r"),
                '\t' => into.Append("\\t"),
                '\b' => into.Append("\\b"),
                '\v' => into.Append("\\v"),
                '\0' => into.Append("\\0"),
                '\f' => into.Append("\\f"),
                _ => into.Append(value),
            };
            _ = into.Append('\'');
        }

        private static void WriteNameInternal(Type type, StringBuilder into, Boolean fullName, Boolean constraint)
        {
            if (fullName && !type.IsGenericParameter)
            {
                _ = type.Namespace is String nspace && nspace.Length != 0 ? into.Append(nspace).Append('.') : into.Append("global::");
            }
            if (fullName && type.IsNested && !type.IsGenericParameter)
            {
                var parent = type.DeclaringType;
                var listing = new Queue<Type>(0x8);
                listing.Enqueue(parent);
                while ((parent = parent.DeclaringType) is not null)
                {
                    listing.Enqueue(parent);
                }
                while (listing.Count > 0)
                {
                    WriteNameInternal(listing.Dequeue(), into, false, false);
                    _ = into.Append('.');
                }
            }
            if (type.HasElementType)
            {
                WriteNameWithElement(type, into, fullName);
            }
            else
            {
                var name = type.Name;
                if (type.IsGenericType)
                {
                    var index = name.IndexOf('`');
                    _ = into.Append(index == -1 ? name : name.Substring(0, index));
                    var args = type.GetGenericArguments();
                    _ = into.Append('<');
                    for (Int32 i = 0, bound = args.Length - 1; i < args.Length; i++)
                    {
                        var next = args[i];
                        var plusSpace = false;
                        if (constraint && next.IsGenericParameter)
                        {
                            var flags = next.GenericParameterAttributes;
                            if (flags != GenericParameterAttributes.None)
                            {
                                if ((flags & GenericParameterAttributes.Contravariant) != GenericParameterAttributes.None)
                                {
                                    _ = into.Append("in ");
                                    plusSpace = true;
                                }
                                else if ((flags & GenericParameterAttributes.Covariant) != GenericParameterAttributes.None)
                                {
                                    _ = into.Append("out ");
                                    plusSpace = true;
                                }
                            }
                            WriteNameInternal(next, into, fullName, false);
                        }
                        else if (next.IsGenericType)
                        {
                            WriteNameInternal(next, into, fullName, false);
                        }
                        else if (next.HasElementType)
                        {
                            WriteNameWithElement(next, into, fullName);
                        }
                        else
                        {
                            WriteNameInternal(next, into, fullName, false);
                        }
                        if (i < bound)
                        {
                            _ = into.Append(',');
                            if (plusSpace)
                            {
                                _ = into.Append(' ');
                            }
                        }
                    }
                    _ = into.Append('>');
                }
                else
                {
                    _ = into.Append(name);
                }
            }
        }

        private static void WriteNameWithElement(Type type, StringBuilder into, Boolean fullName)
        {
            var element = type.GetElementType();
            if (element.HasElementType)
            {
                var ranks = RankSymbols;
                var queue = new Stack<String>(0x4);
                queue.Push(type.IsPointer ? "*" : type.IsByRef ? "&" : ranks[type.GetArrayRank()]);
                do queue.Push(element.IsPointer ? "*" : element.IsByRef ? "&" : ranks[element.GetArrayRank()]);
                while ((element = element.GetElementType()).HasElementType);
                WriteNameInternal(element, into, fullName, false);
                while (queue.Count > 0) _ = into.Append(queue.Pop());
            }
            else
            {
                WriteNameInternal(element, into, fullName, false);
                _ = type.IsPointer ? into.Append('*') : type.IsByRef ? into.Append('&') : into.Append(RankSymbols[type.GetArrayRank()]);
            }
        }

        private static Boolean IsUnmanagedType(Type type)
        {
            if (type.IsPrimitive || type.IsPointer)
            {
                return true;
            }
            var logs = UnmanagedSet;
            var hash = RuntimeHelpers.GetHashCode(type);
            if (!logs.TryGetValue(hash, out var state))
            {
                foreach (var custom in type.CustomAttributes)
                {
                    if (custom.AttributeType.Name.IndexOf("UnmanagedAttribute") != -1)
                    {
                        logs[hash] = true;
                        return true;
                    }
                }
                logs[hash] = false;
                return false;
            }
            return state;
        }

        private static void WriteAttributeValue(StringBuilder into, Type argType, Object value, Boolean fullName, Regex isnumeric, Char[] enumSplit)
        {
            String @string;
            if (value is null)
            {
                _ = into.Append("null");
            }
            else
            {
                if (argType.IsEnum)
                {
                    var baseName = OfType(argType, fullName, false, false);
                    var enumName = value.ToString();
                    if (isnumeric.IsMatch(enumName))
                    {
                        var callArgs = new Object[3];
                        callArgs[0] = enumName;
                        callArgs[1] = true;
                        if ((Boolean)EnumTryParse.MakeGenericMethod(argType).Invoke(null, callArgs))
                        {
                            enumName = callArgs[2].ToString();
                        }
                    }
                    if (enumName.IndexOf(',') != -1)
                    {
                        var parts = enumName.Split(enumSplit, StringSplitOptions.RemoveEmptyEntries);
                        for (var j = 0; j < parts.Length; j++)
                        {
                            _ = into.Append(baseName).Append('.').Append(parts[j]);
                            if (j < parts.Length - 1)
                            {
                                _ = into.Append('|');
                            }
                        }
                    }
                    else
                    {
                        if (isnumeric.IsMatch(enumName))
                        {
                            _ = into.Append('(').Append(baseName).Append(')').Append(enumName);
                        }
                        else
                        {
                            _ = into.Append(baseName).Append('.').Append(enumName);
                        }
                    }
                }
                else
                {
                    if (argType == typeof(String))
                    {
                        WriteEscape(into, value.ToString());
                    }
                    else if (argType == typeof(Char))
                    {
                        WriteEscape(into, (Char)value);
                    }
                    else if (argType == typeof(Boolean))
                    {
                        _ = into.Append((Boolean)value ? "true" : "false");
                    }
                    else if (argType == typeof(UInt32))
                    {
                        _ = into.Append(value).Append('U');
                    }
                    else if (argType == typeof(UInt64))
                    {
                        _ = into.Append(value).Append("UL");
                    }
                    else if (argType == typeof(Int64))
                    {
                        _ = into.Append(value).Append('L');
                    }
                    else if (argType == typeof(Decimal))
                    {
                        _ = into.Append(value).Append('m');
                    }
                    else if (argType == typeof(Double))
                    {
                        @string = value.ToString();
                        if (@string.IndexOf('.') == -1 && @string.IndexOf(',') == -1)
                        {
                            _ = into.Append(value).Append(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Append('0');
                        }
                        else
                        {
                            _ = into.Append(value);
                        }
                    }
                    else if (argType == typeof(Single))
                    {
                        @string = value.ToString();
                        if (@string.IndexOf('.') == -1 && @string.IndexOf(',') == -1)
                        {
                            _ = into.Append(value).Append(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Append('0');
                        }
                        else
                        {
                            _ = into.Append(value);
                        }
                        _ = into.Append('f');
                    }
                    else if (argType == typeof(DateTime))
                    {
                        WriteEscape(into, value.ToString());
                    }
                    else if (argType == typeof(Type))
                    {
                        _ = into.Append("typeof(").Append(OfType((Type)value, fullName, false, false)).Append(')');
                    }
                    else if (argType == typeof(Int32))
                    {
                        _ = into.Append(value.ToString());
                    }
                    else
                    {
                        _ = into.Append('(').Append(OfType(argType, fullName, false, false)).Append(')').Append(value.ToString());
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteAttributes(StringBuilder into, IEnumerable<CustomAttributeData> attributes, Boolean fullName)
            => WriteAttributes(into, attributes, fullName, MethodImplAttributes.IL);

        private static void WriteAttributes(StringBuilder into, IEnumerable<CustomAttributeData> attributes, Boolean fullName, MethodInfo method, String indent = "")
        {
            if (method is not null)
            {
                WriteAttributes(into, attributes, fullName, method.GetMethodImplementationFlags(), indent);
            }
            else
            {
                WriteAttributes(into, attributes, fullName, MethodImplAttributes.IL, indent);
            }
        }

        private static void WriteAttributes(StringBuilder into, IEnumerable<CustomAttributeData> attributes, Boolean fullName, MethodImplAttributes opt, String indent = "")
        {
            foreach (var attribute in attributes)
            {
                _ = into.Append(indent).Append('[').Append(OfType(attribute.AttributeType, fullName, false, false));
                var cList = attribute.ConstructorArguments;
                var mList = attribute.NamedArguments;
                if (cList.Count != 0 || mList.Count != 0)
                {
                    _ = into.Append('(');
                    var isnumeric = new Regex(@"\d+");
                    var enumSplit = new Char[] { ',', ' ' };
                    if (cList.Count != 0)
                    {
                        var cargs = attribute.Constructor.GetParameters();
                        var wname = mList.Count != 0;
                        for (var i = 0; i < cList.Count; i++)
                        {
                            var node = cList[i];
                            if (wname) _ = into.Append(cargs[i].Name).Append(": ");
                            WriteAttributeValue(into, node.ArgumentType, node.Value, fullName, isnumeric, enumSplit);
                            _ = into.Append(", ");
                        }
                    }
                    if (mList.Count != 0)
                    {
                        for (var i = 0; i < mList.Count; i++)
                        {
                            var node = mList[i];
                            _ = into.Append(node.MemberName).Append(" = ");
                            WriteAttributeValue(into, node.TypedValue.ArgumentType, node.TypedValue.Value, fullName, isnumeric, enumSplit);
                            _ = into.Append(", ");
                        }
                    }
                    into.Length -= 2;
                    _ = into.Append(')');
                }
                _ = into.Append(']').AppendLine();
            }
            if (opt != 0)
            {
                var att = OfType(typeof(MethodImplAttribute), fullName, false, false);
                var val = OfType(typeof(MethodImplOptions), fullName, false, false);
                _ = into.Append(indent).Append('[').Append(att).Append('(');
                var num = 0;
                if ((opt & MethodImplAttributes.AggressiveInlining) != 0)
                {
                    _ = into.Append(val).Append('.').Append(nameof(MethodImplOptions.AggressiveInlining)).Append('|');
                    num++;
                }
                if ((opt & MethodImplAttributes.ForwardRef) != 0)
                {
                    _ = into.Append(val).Append('.').Append(nameof(MethodImplOptions.ForwardRef)).Append('|');
                    num++;
                }
                if ((opt & MethodImplAttributes.InternalCall) != 0)
                {
                    _ = into.Append(val).Append('.').Append(nameof(MethodImplOptions.InternalCall)).Append('|');
                    num++;
                }
                if ((opt & MethodImplAttributes.Unmanaged) != 0)
                {
                    _ = into.Append(val).Append('.').Append(nameof(MethodImplOptions.Unmanaged)).Append('|');
                    num++;
                }
                if ((opt & MethodImplAttributes.NoOptimization) != 0)
                {
                    _ = into.Append(val).Append('.').Append(nameof(MethodImplOptions.NoOptimization)).Append('|');
                    num++;
                }
                if ((opt & MethodImplAttributes.NoInlining) != 0)
                {
                    _ = into.Append(val).Append('.').Append(nameof(MethodImplOptions.NoInlining)).Append('|');
                    num++;
                }
                if ((opt & MethodImplAttributes.Synchronized) != 0)
                {
                    _ = into.Append(val).Append('.').Append(nameof(MethodImplOptions.Synchronized)).Append('|');
                    num++;
                }
                if ((opt & MethodImplAttributes.PreserveSig) != 0)
                {
                    _ = into.Append(val).Append('.').Append(nameof(MethodImplOptions.PreserveSig)).Append('|');
                    num++;
                }
                if ((opt & MethodImplAttributes.SecurityMitigations) != 0)
                {
                    _ = into.Append(val).Append('.').Append(nameof(MethodImplOptions.SecurityMitigations)).Append('|');
                    num++;
                }
                if (num != 0)
                {
                    into.Length--;
                    _ = into.AppendLine(")]");
                }
                else
                {
                    into.Length -= 2 + att.Length;
                }
            }
        }

        private static void WriteTypeName(Type type, StringBuilder into, Boolean fullName, Boolean constraints, Boolean attributes)
        {
            if (attributes)
            {
                WriteAttributes(into, type.CustomAttributes, fullName, method: null);
            }
            var invoke = constraints && type.IsSubclassOf(typeof(Delegate)) ? type.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) : null;
            if (invoke is not null)
            {
                var retval = invoke.ReturnType;
                if (ReferenceEquals(retval, TypeVoid))
                {
                    _ = into.Append("void");
                }
                else if (ReferenceEquals(retval, TypeVoidPtr))
                {
                    _ = into.Append("void*");
                }
                else
                {
                    WriteNameInternal(retval, into, fullName, constraints);
                }
                _ = into.Append(' ');
            }
            WriteNameInternal(type, into, fullName, constraints);
            if (invoke is not null)
            {
                _ = into.Append('(');
                var args = invoke.GetParameters();
                for (var i = 0; i < args.Length; i++)
                {
                    WriteNameInternal(args[i].ParameterType, into, fullName, constraints);
                    if (constraints)
                    {
                        _ = into.Append(' ').Append(args[i].Name);
                    }
                    if (i < args.Length - 1)
                    {
                        _ = into.Append(',');
                        if (constraints)
                        {
                            _ = into.Append(' ');
                        }
                    }
                }
                _ = into.Append(')');
            }
            if (constraints && type.IsGenericType)
            {
                var args = type.GetGenericArguments() ?? Type.EmptyTypes;
                var maps = new Dictionary<Type, String[]>(args.Length);
                var buffer = new List<String>(0x4);
                var local = new StringBuilder(255);
                for (var i = 0; i < args.Length; i++)
                {
                    var next = args[i];
                    if (next.IsGenericParameter)
                    {
                        var isunmanaged = IsUnmanagedType(next);
                        if (!isunmanaged)
                        {
                            var flags = next.GenericParameterAttributes;
                            if (flags != GenericParameterAttributes.None)
                            {
                                if ((flags & GenericParameterAttributes.NotNullableValueTypeConstraint) != GenericParameterAttributes.None)
                                {
                                    buffer.Add("notnull");
                                }
                                if ((flags & GenericParameterAttributes.ReferenceTypeConstraint) != GenericParameterAttributes.None)
                                {
                                    buffer.Add("class");
                                }
                                if ((flags & GenericParameterAttributes.DefaultConstructorConstraint) != GenericParameterAttributes.None)
                                {
                                    buffer.Add("new()");
                                }
                            }
                        }
                        else
                        {
                            buffer.Add("unmanaged");
                        }
                        var definition = next.GetGenericParameterConstraints();
                        if (definition is not null && definition.Length != 0)
                        {
                            for (var n = 0; n < definition.Length; n++)
                            {
                                var nextdef = definition[n];
                                if (nextdef == typeof(ValueType))
                                {
                                    if (isunmanaged)
                                    {
                                        continue;
                                    }
                                    buffer.Add("struct");
                                }
                                else
                                {
                                    local.Length = 0;
                                    WriteNameInternal(nextdef, local, fullName, false);
                                    buffer.Add(local.ToString());
                                }
                            }
                        }
                        maps[next] = buffer.ToArray();
                        buffer.Clear();
                    }
                }
                foreach (var pair in maps)
                {
                    if (pair.Value.Length != 0)
                    {
                        local.Length = 0;
                        WriteNameInternal(pair.Key, local, fullName, false);
                        _ = into.Append(" where ").Append(local.ToString()).Append(": ");
                        var clauses = pair.Value;
                        Array.Sort(clauses, ConstraintComparer);
                        for (var i = 0; i < clauses.Length; i++)
                        {
                            _ = into.Append(clauses[i]);
                            if (i < clauses.Length - 1)
                            {
                                _ = into.Append(", ");
                            }
                        }
                    }
                }
            }
        }

        private static void WriteMethodInfo(MethodInfo method, StringBuilder into, Boolean fullName, Boolean returnType, Boolean writeSpecs, Boolean attributes)
        {
            String tname;
            if (attributes)
            {
                WriteAttributes(into, method.CustomAttributes, fullName, method);
            }
            if (writeSpecs)
            {
                if (method.IsPublic)
                {
                    _ = into.Append("public ");
                }
                else if (method.IsFamilyOrAssembly)
                {
                    _ = into.Append("protected internal ");
                }
                else if (method.IsFamilyAndAssembly)
                {
                    _ = into.Append("private internal ");
                }
                else if (method.IsFamily)
                {
                    _ = into.Append("protected ");
                }
                else if (method.IsAssembly)
                {
                    _ = into.Append("internal ");
                }
                else
                {
                    _ = into.Append("private ");
                }
                if (method.IsStatic)
                {
                    _ = into.Append("static ");
                }
                else
                {
                    if (IsOverriden(method))
                    {
                        _ = into.Append("override ");
                        if (method.IsFinal)
                        {
                            _ = into.Append("sealed ");
                        }
                    }
                    else if (method.IsAbstract)
                    {
                        _ = method.IsFinal ? into.Append("override sealed ") : into.Append("abstract ");
                    }
                    else
                    {
                        if (method.IsVirtual)
                        {
                            _ = into.Append("virtual ");
                        }
                        var type = method.DeclaringType.BaseType;
                        while (type is not null)
                        {
                            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic);
                            for (var i = 0; i < methods.Length; i++)
                            {
                                if (methods[i].Name == method.Name)
                                {
                                    _ = into.Append("new ");
                                    goto leave;
                                }
                            }
                            type = type.BaseType;
                        }
                    leave: { }
                    }
                }
                if (method.IsDefined(typeof(DllImportAttribute)) || (method.GetCustomAttribute(typeof(MethodImplAttribute)) is MethodImplAttribute mi && (mi.Value is MethodImplOptions.ForwardRef or MethodImplOptions.Unmanaged)))
                {
                    _ = into.Append("extern ");
                }
                if (IsUnsafeMethod(method))
                {
                    _ = into.Append("unsafe ");
                }
                if (typeof(Task).IsAssignableFrom(method.ReturnType) && method.IsDefined(typeof(AsyncTaskMethodBuilder)))
                {
                    _ = into.Append("async ");
                }
                if (!returnType)
                {
                    _ = into.Append("? ");
                }
            }
            if (returnType)
            {
                tname = OfType(method.ReturnType ?? TypeVoid, fullName, false, false);
                if (writeSpecs && tname[tname.Length - 1] == '&')
                {
                    tname = String.Concat("ref ", tname.Substring(0, tname.Length - 1));
                }
                _ = into.Append(tname).Append(' ');
            }
            _ = into.Append(method.Name);
            if (method.IsGenericMethod)
            {
                var generic = method.GetGenericArguments();
                if (generic.Length != 0)
                {
                    _ = into.Append('<');
                    for (Int32 i = 0, bound = generic.Length - 1; i < generic.Length; i++)
                    {
                        WriteTypeName(generic[i], into, fullName, false, false);
                        if (i < bound)
                        {
                            _ = into.Append(',');
                        }
                    }
                    _ = into.Append('>');
                }
            }
            _ = into.Append('(');
            var vparams = method.GetParameters();
            for (Int32 i = 0, bound = vparams.Length - 1; i < vparams.Length; i++)
            {
                var next = vparams[i];
                if (writeSpecs)
                {
                    if (next.IsOut)
                    {
                        _ = into.Append("out ");
                    }
                    else if (next.IsIn)
                    {
                        _ = into.Append("in ");
                    }
                }
                tname = OfType(next.ParameterType, fullName, false, false);
                if (writeSpecs && tname[tname.Length - 1] == '&')
                {
                    if (!next.IsOut)
                    {
                        tname = String.Concat("ref ", tname.Substring(0, tname.Length - 1));
                    }
                    else
                    {
                        tname = tname.Substring(0, tname.Length - 1);
                    }
                }
                _ = into.Append(tname);
                if (writeSpecs)
                {
                    _ = into.Append(' ').Append(next.Name);
                }
                if (i < bound)
                {
                    _ = writeSpecs ? into.Append(", ") : into.Append(',');
                }
            }
            _ = into.Append(')');
            if (writeSpecs && (method is not DynamicMethod))
            {
                var args = method.GetGenericArguments() ?? Type.EmptyTypes;
                if (args.Length != 0)
                {
                    var maps = new Dictionary<Type, String[]>(args.Length);
                    var buffer = new List<String>(0x4);
                    var local = new StringBuilder(255);
                    for (var i = 0; i < args.Length; i++)
                    {
                        var next = args[i];
                        if (next.IsGenericParameter)
                        {
                            var isunmanaged = IsUnmanagedType(next);
                            if (!isunmanaged)
                            {
                                var flags = next.GenericParameterAttributes;
                                if (flags != GenericParameterAttributes.None)
                                {
                                    if ((flags & GenericParameterAttributes.NotNullableValueTypeConstraint) != GenericParameterAttributes.None)
                                    {
                                        buffer.Add("notnull");
                                    }
                                    if ((flags & GenericParameterAttributes.ReferenceTypeConstraint) != GenericParameterAttributes.None)
                                    {
                                        buffer.Add("class");
                                    }
                                    if ((flags & GenericParameterAttributes.DefaultConstructorConstraint) != GenericParameterAttributes.None)
                                    {
                                        buffer.Add("new()");
                                    }
                                }
                            }
                            else
                            {
                                buffer.Add("unmanaged");
                            }
                            var definition = next.GetGenericParameterConstraints();
                            if (definition is not null && definition.Length != 0)
                            {
                                for (var n = 0; n < definition.Length; n++)
                                {
                                    var nextdef = definition[n];
                                    if (nextdef == typeof(ValueType))
                                    {
                                        if (isunmanaged)
                                        {
                                            continue;
                                        }
                                        buffer.Add("struct");
                                    }
                                    else
                                    {
                                        local.Length = 0;
                                        WriteNameInternal(nextdef, local, fullName, false);
                                        buffer.Add(local.ToString());
                                    }
                                }
                            }
                            maps[next] = buffer.ToArray();
                            buffer.Clear();
                        }
                    }
                    foreach (var pair in maps)
                    {
                        if (pair.Value.Length != 0)
                        {
                            local.Length = 0;
                            WriteNameInternal(pair.Key, local, fullName, false);
                            _ = into.Append(" where ").Append(local.ToString()).Append(": ");
                            var clauses = pair.Value;
                            Array.Sort(clauses, ConstraintComparer);
                            for (var i = 0; i < clauses.Length; i++)
                            {
                                _ = into.Append(clauses[i]);
                                if (i < clauses.Length - 1)
                                {
                                    _ = into.Append(", ");
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void WritePropertyInfo(PropertyInfo property, StringBuilder into, Boolean fullName, Boolean returnType, Boolean writeSpecs, Boolean attributes)
        {
            if (attributes)
            {
                WriteAttributes(into, property.CustomAttributes, fullName, MethodImplAttributes.IL);
            }
            var getter = property.CanRead ? property.GetGetMethod(true) : null;
            var setter = property.CanWrite ? property.GetSetMethod(true) : null;
            var hasget = getter is not null;
            var hasset = setter is not null;
            var method = hasget ? getter : setter;
            var isdiff = hasget && hasset ? getter.Attributes != setter.Attributes : false;
            String tname;
            if (writeSpecs)
            {
                if ((hasget && getter.IsPublic) || (hasset && setter.IsPublic))
                {
                    _ = into.Append("public ");
                }
                else if ((hasget && getter.IsFamilyOrAssembly) || (hasset && setter.IsFamilyOrAssembly))
                {
                    _ = into.Append("protected internal ");
                }
                else if ((hasget && getter.IsFamilyAndAssembly) || (hasset && setter.IsFamilyAndAssembly))
                {
                    _ = into.Append("private internal ");
                }
                else if ((hasget && getter.IsFamily) || (hasset && setter.IsFamily))
                {
                    _ = into.Append("protected ");
                }
                else if ((hasget && getter.IsAssembly) || (hasset && setter.IsAssembly))
                {
                    _ = into.Append("internal ");
                }
                else
                {
                    _ = into.Append("private ");
                }
                if ((hasget && getter.IsStatic) || (hasset && setter.IsStatic))
                {
                    _ = into.Append("static ");
                }
                else
                {
                    if (IsOverriden(method))
                    {
                        _ = into.Append("override ");
                        if ((hasget ? getter : setter).IsFinal)
                        {
                            _ = into.Append("sealed ");
                        }
                    }
                    else if (method.IsAbstract)
                    {
                        _ = method.IsFinal ? into.Append("override sealed ") : into.Append("abstract ");
                    }
                    else
                    {
                        if (method.IsVirtual)
                        {
                            _ = into.Append("virtual ");
                        }
                        var type = method.DeclaringType.BaseType;
                        while (type is not null)
                        {
                            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic);
                            for (var i = 0; i < methods.Length; i++)
                            {
                                if (methods[i].Name == method.Name)
                                {
                                    _ = into.Append("new ");
                                    goto leave;
                                }
                            }
                            type = type.BaseType;
                        }
                    leave: { }
                    }
                }
                var opt = method.GetMethodImplementationFlags();
                if ((opt & MethodImplAttributes.ForwardRef) != 0 || (opt & MethodImplAttributes.InternalCall) != 0 || (opt & MethodImplAttributes.Runtime) != 0 || (opt & MethodImplAttributes.Native) != 0)
                {
                    _ = into.Append("extern ");
                }
                else if (hasget && hasset)
                {
                    opt = setter.GetMethodImplementationFlags();
                    if ((opt & MethodImplAttributes.ForwardRef) != 0 || (opt & MethodImplAttributes.InternalCall) != 0 || (opt & MethodImplAttributes.Runtime) != 0 || (opt & MethodImplAttributes.Native) != 0)
                    {
                        _ = into.Append("extern ");
                    }
                }
                if (IsUnsafeProperty(property))
                {
                    _ = into.Append("unsafe ");
                }
                if (typeof(Task).IsAssignableFrom(property.PropertyType) && method.IsDefined(typeof(AsyncTaskMethodBuilder)))
                {
                    _ = into.Append("async ");
                }
                if (!returnType)
                {
                    _ = into.Append("? ");
                }
            }
            if (returnType)
            {
                tname = OfType(property.PropertyType ?? TypeVoid, fullName, false, false);
                if (writeSpecs && tname[tname.Length - 1] == '&')
                {
                    tname = String.Concat("ref ", tname.Substring(0, tname.Length - 1));
                }
                _ = into.Append(tname).Append(' ');
            }
            _ = into.Append(property.Name);
            var vparams = property.GetIndexParameters();
            if (vparams.Length != 0)
            {
                _ = into.Append('[');
                for (Int32 i = 0, bound = vparams.Length - 1; i < vparams.Length; i++)
                {
                    var next = vparams[i];
                    if (writeSpecs)
                    {
                        if (next.IsOut)
                        {
                            _ = into.Append("out ");
                        }
                        else if (next.IsIn)
                        {
                            _ = into.Append("in ");
                        }
                    }
                    tname = OfType(next.ParameterType, fullName, false, false);
                    if (writeSpecs && tname[tname.Length - 1] == '&')
                    {
                        if (!next.IsOut)
                            tname = String.Concat("ref ", tname.Substring(0, tname.Length - 1));
                        else
                            tname = tname.Substring(0, tname.Length - 1);
                    }
                    _ = into.Append(tname);
                    if (writeSpecs)
                    {
                        _ = into.Append(' ').Append(next.Name);
                    }
                    if (i < bound)
                    {
                        _ = writeSpecs ? into.Append(", ") : into.Append(',');
                    }
                }
                _ = into.Append(']');
            }
            if (writeSpecs && attributes)
            {
                const String indent = "  ";
                _ = into.AppendLine().Append('{').AppendLine();
                if (hasget)
                {
                    WriteAttributes(into, getter.CustomAttributes, fullName, getter.GetMethodImplementationFlags(), indent);
                    _ = into.Append(indent);
                    _ = writeSpecs && isdiff
                        ? getter.IsPublic
                            ? into.Append("get; ")
                            : getter.IsFamilyOrAssembly
                            ? into.Append("protected internal get; ")
                            : getter.IsFamilyAndAssembly
                            ? into.Append("private internal get; ")
                            : getter.IsFamily
                            ? into.Append("protected get; ")
                            : getter.IsAssembly ? into.Append("internal get; ") : into.Append("private get; ")
                        : into.Append("get; ");
                }
                if (hasset)
                {
                    _ = into.AppendLine();
                    WriteAttributes(into, setter.CustomAttributes, fullName, setter.GetMethodImplementationFlags(), indent);
                    _ = into.Append(indent);
                    _ = writeSpecs && isdiff
                        ? setter.IsPublic
                            ? into.Append("set; ")
                            : setter.IsFamilyOrAssembly
                            ? into.Append("protected internal set; ")
                            : setter.IsFamilyAndAssembly
                            ? into.Append("private internal set; ")
                            : setter.IsFamily
                            ? into.Append("protected set; ")
                            : setter.IsAssembly ? into.Append("internal set; ") : into.Append("private set; ")
                        : into.Append("set; ");
                }
                _ = into.AppendLine().Append('}');

            }
            else if (isdiff)
            {
                _ = into.Append(" { ");
                if (hasget)
                {
                    _ = getter.IsPublic
                        ? into.Append("get; ")
                        : getter.IsFamilyOrAssembly
                        ? into.Append("protected internal get; ")
                        : getter.IsFamilyAndAssembly
                        ? into.Append("private internal get; ")
                        : getter.IsFamily
                        ? into.Append("protected get; ")
                        : getter.IsAssembly ? into.Append("internal get; ")
                        : into.Append("private get; ");
                }
                if (hasset)
                {
                    _ = setter.IsPublic
                        ? into.Append("set; ")
                        : setter.IsFamilyOrAssembly
                        ? into.Append("protected internal set; ")
                        : setter.IsFamilyAndAssembly
                        ? into.Append("private internal set; ")
                        : setter.IsFamily
                        ? into.Append("protected set; ")
                        : setter.IsAssembly ? into.Append("internal set; ") : into.Append("private set; ");
                }
                _ = into.Append('}');
            }
            else if (hasget && hasset)
            {
                _ = into.Append(" { get; set; }");
            }
            else if (hasget)
            {
                _ = into.Append(" { get; }");
            }
            else
            {
                _ = into.Append(" { set; }");
            }
        }

        private static void WriteClassInfo(Type type, StringBuilder into, Boolean fullName, Boolean constraints, Boolean attributes)
        {
            if (attributes)
            {
                WriteAttributes(into, type.CustomAttributes, fullName);
            }
            if (type.IsPublic)
            {
                _ = into.Append("public ");
            }
            else if (type.IsNested)
            {
                if (type.IsNestedPublic)
                {
                    _ = into.Append("public ");
                }
                else if (type.IsNestedFamORAssem)
                {
                    _ = into.Append("protected internal ");
                }
                else if (type.IsNestedFamANDAssem)
                {
                    _ = into.Append("private internal ");
                }
                else if (type.IsNestedFamily)
                {
                    _ = into.Append("protected ");
                }
                else if (type.IsNestedAssembly)
                {
                    _ = into.Append("internal ");
                }
                else
                {
                    _ = into.Append("private ");
                }
            }
            else if (type.IsNotPublic)
            {
                _ = into.Append("internal ");
            }
            if (!type.IsValueType)
            {
                if (type.IsInterface)
                {
                    _ = into.Append("interface ");
                }
                else if (type.IsSubclassOf(typeof(MulticastDelegate)))
                {
                    _ = into.Append("delegate ");
                }
                else
                {
                    if (type.IsSealed && type.IsAbstract)
                    {
                        _ = into.Append("static ");
                    }
                    else if (type.IsSealed)
                    {
                        _ = into.Append("sealed ");
                    }
                    else if (type.IsAbstract)
                    {
                        _ = into.Append("abstract ");
                    }
                    _ = into.Append("class ");
                }
            }
            else
            {
                if (type.IsDefined(typeof(IsReadOnlyAttribute)))
                {
                    _ = into.Append("readonly ");
                }
                if (type.IsDefined(typeof(IsByRefLikeAttribute)))
                {
                    _ = into.Append("ref ");
                }
                _ = into.Append("struct ");
            }
            WriteTypeName(type, into, false, constraints, false);
        }

        #endregion

        /// <summary>
        /// Generates the signature of the requested <paramref name="objectType"/> using the default signature generation settings.
        /// </summary>
        /// <param name="objectType">Specify the target <see cref="Type"/> to generate the signature. If this <see langword="null"/> then return "<c>null</c>" string.</param>
        /// <returns>A <see cref="String"/> that represents the signature for the specified <paramref name="objectType"/> using the default signature settings.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static String OfType(Type objectType) => OfType(objectType, false, false, false);

        /// <summary>
        /// Generates the signature of the requested <paramref name="objectType"/> using the custom settings or default settings.
        /// </summary>
        /// <param name="objectType">Specify the target <see cref="Type"/> to generate the signature. If this <see langword="null"/> then return "<c>null</c>" string.</param>
        /// <param name="fullName"><see langword="true"/> to include the namespace name, and the parent type path (if nested). The default is <see langword="false"/>.</param>
        /// <param name="constraints"><see langword="true"/> to include the generic type parameter constraints, if any. The default is <see langword="false"/>.</param>
        /// <param name="attributes"><see langword="true"/> to include all of attributes that decorating the given type, if any. The default is <see langword="false"/>.</param>
        /// <returns>A <see cref="String"/> that represents the signature for the specified <paramref name="objectType"/> using the default or custom settings.</returns>
        public static String OfType(Type objectType, Boolean fullName, Boolean constraints, Boolean attributes)
        {
            if (objectType is null)
            {
                return "null";
            }
            else if (ReferenceEquals(objectType, TypeVoid))
            {
                return "void";
            }
            else if (ReferenceEquals(objectType, TypeVoidPtr))
            {
                return "void*";
            }
            else
            {
                Int64 hash;
                unsafe
                {
                    *(Int32*)&hash = RuntimeHelpers.GetHashCode(objectType);
                    ((Int32*)&hash)[1] = CombineHash32(fullName ? FullNameHash : 0, constraints ? ConstraintHash : 0, 0, attributes ? AttributesHash : 0);
                }
                var store = CachedTypeID;
                if (!store.TryGetValue(hash, out var result))
                {
                    var buffer = CachedTypeBuilder();
                    WriteTypeName(objectType, buffer, fullName, constraints, attributes);
                    store[hash] = result = buffer.ToString();
                    buffer.Length = 0;
                }
                return result;
            }
        }

        /// <summary>
        /// Generates the signature of the requested object <paramref name="instance"/> using the default signature generation settings.
        /// </summary>
        /// <param name="instance">Specify the instance of an <see cref="Object"/> to generate the signature. If this <see langword="null"/> then return "<c>null</c>" string.</param>
        /// <returns>A <see cref="String"/> that represents the signature for the given object <paramref name="instance"/> using the default or custom settings.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static String OfType(Object instance) => OfType(instance, false, false, false);

        /// <summary>
        /// Generates the signature of the requested object <paramref name="instance"/> using the custom settings or default settings.
        /// </summary>
        /// <param name="instance">Specify the instance of an <see cref="Object"/> to generate the signature. If this <see langword="null"/> then return "<c>null</c>" string.</param>
        /// <param name="fullName"><see langword="true"/> to include the namespace name, and the parent type path (if nested). The default is <see langword="false"/>.</param>
        /// <param name="constraints"><see langword="true"/> to include the generic type parameter constraints, if any. The default is <see langword="false"/>.</param>
        /// <param name="attributes"><see langword="true"/> to include all of attributes that decorating the given type, if any. The default is <see langword="false"/>.</param>
        /// <returns>A <see cref="String"/> that represents the signature for the given object <paramref name="instance"/> using the default or custom settings.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static String OfType(Object instance, Boolean fullName, Boolean constraints, Boolean attributes) => OfType(instance?.GetType(), fullName, constraints, attributes);

        /// <summary>
        /// Generates the signature of the requested <paramref name="classType"/> using the default signature generation settings.
        /// </summary>
        /// <param name="classType">Specify the target <see cref="Type"/> to generate the signature. If this <see langword="null"/> then return "<c>null</c>" string.</param>
        /// <returns>A <see cref="String"/> that represents the signature for the specified <paramref name="classType"/> using the default or custom settings.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static String OfClass(Type classType) => OfClass(classType, false, true, false);

        /// <summary>
        /// Generates the signature of the requested <paramref name="classType"/> using the custom settings or default settings.
        /// </summary>
        /// <param name="classType">Specify the target <see cref="Type"/> to generate the signature. If this <see langword="null"/> then return "<c>null</c>" string.</param>
        /// <param name="fullName">Used if the <paramref name="attributes"/> is <see langword="true"/>, that is to include the namespace name, otherwise, ignored.</param>
        /// <param name="constraints"><see langword="true"/> to include the generic type parameter constraints, if any. The default is <see langword="false"/>.</param>
        /// <param name="attributes"><see langword="true"/> to include all of attributes that decorating the given type, if any. The default is <see langword="false"/>.</param>
        /// <returns>A <see cref="String"/> that represents the signature for the specified <paramref name="classType"/> using the default or custom settings.</returns>
        public static String OfClass(Type classType, Boolean fullName, Boolean constraints, Boolean attributes)
        {
            if (classType is null)
            {
                return "null";
            }
            if (classType.HasElementType)
            {
                return OfType(classType, fullName, constraints, attributes);
            }
            else
            {
                Int64 hash;
                unsafe
                {
                    *(Int32*)&hash = RuntimeHelpers.GetHashCode(classType);
                    ((Int32*)&hash)[1] = CombineHash32(fullName ? FullNameHash : 0, constraints ? ConstraintHash : 0, 0, attributes ? AttributesHash : 0);
                }
                var store = CachedClassID;
                if (!store.TryGetValue(hash, out var result))
                {
                    var builder = CachedClassBuilder();
                    WriteClassInfo(classType, builder, fullName, constraints, attributes);
                    store[hash] = result = builder.ToString();
                    builder.Length = 0;
                }
                return result;
            }
        }

        /// <summary>
        /// Generates the signature of the requested object <paramref name="instance"/> using the default signature generation settings.
        /// </summary>
        /// <param name="instance">Specify the instance of an <see cref="Object"/> to generate the signature. If this <see langword="null"/> then return "<c>null</c>" string.</param>
        /// <returns>A <see cref="String"/> that represents the signature for the given object <paramref name="instance"/> using the default or custom settings.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static String OfClass(Object instance) => OfClass(instance, false, false, false);

        /// <summary>
        /// Generates the signature of the requested object <paramref name="instance"/> using the custom settings or default settings.
        /// </summary>
        /// <param name="instance">Specify the instance of an <see cref="Object"/> to generate the signature. If this <see langword="null"/> then return "<c>null</c>" string.</param>
        /// <param name="writeSpecs"><see langword="true"/> to include the generic type parameter constraints, if any. The default is <see langword="false"/>.</param>
        /// <param name="attributes"><see langword="true"/> to include all of attributes that decorating the given type, if any. The default is <see langword="false"/>.</param>
        /// <param name="fullName"><see langword="true"/> to include the namespace name, and the parent type path (if nested). The default is <see langword="false"/>.</param>
        /// <returns>A <see cref="String"/> that represents the signature for the given object <paramref name="instance"/> using the default or custom settings.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static String OfClass(Object instance, Boolean writeSpecs, Boolean attributes, Boolean fullName) => OfClass(instance?.GetType(), attributes, fullName, writeSpecs);

        /// <summary>
        /// Generates the signature of the given method using the default settings, that is using the these specifications:<br/>
        /// 1. The method name is included in the signature;<br/>
        /// 2. All of generic type parameters is included, if any;<br/>
        /// 3. All of parameters of the method is included, using short type name, and without the parameter names;<br/>
        /// 4. The return type of the method IS NOT included in the signature;<br/>
        /// 5. The method attributes, accessibilities, modifiers, and the generic type constraints is also excluded.<br/>
        /// ie. The method "<see cref="Array.Exists{T}(T[], Predicate{T})"/> will output: "<c>Exists&lt;T&gt;(T[],Predicate&lt;T&gt;)</c>"
        /// </summary>
        /// <param name="methodInfo">Set the <see cref="MethodInfo"/> instance as the metadata of the requested method to get the signature.</param>
        /// <returns>A <see cref="String"/> that represents the method signature using the default signature generation settings.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static String OfMethod(MethodInfo methodInfo) => OfMethod(methodInfo, false, false, false, false);

        /// <summary>
        /// Generates the signature of the given method using the specified generation settings or the default settings.
        /// </summary>
        /// <param name="methodInfo">Set the <see cref="MethodInfo"/> instance as the metadata of the requested method to get the signature.</param>
        /// <param name="fullName">Set with <see langword="true"/> to make all of <see cref="Type"/> in the method (ie. parameters) have full name; The default is <see langword="false"/>.</param>
        /// <param name="returnType">Set with <see langword="true"/> to include the method return type information in the signature. The default is <see langword="false"/>.</param>
        /// <param name="writeSpecs"><see langword="true"/> to include the method accessibility, modifiers, and constraints in the signature. The default is <see langword="false"/>.</param>
        /// <param name="attributes"><see langword="true"/> to include all of the attributes that decorating the method in the signature. The default is <see langword="false"/>.</param>
        /// <returns>A <see cref="String"/> that represents the method signature using specified generation settings or using the default settings.</returns>
        public static String OfMethod(MethodInfo methodInfo, Boolean fullName, Boolean returnType, Boolean writeSpecs, Boolean attributes)
        {
            if (ReferenceEquals(methodInfo, null))
            {
                return "null";
            }
            else
            {
                Int64 hash;
                unsafe
                {
                    *(Int32*)&hash = RuntimeHelpers.GetHashCode(methodInfo);
                    ((Int32*)&hash)[1] = CombineHash32(fullName ? FullNameHash : 0, returnType ? ReturnTypeHash : 0, writeSpecs ? WriteSpecsHash : 0, attributes ? AttributesHash : 0);
                }
                var store = CachedMethodID;
                if (!store.TryGetValue(hash, out var result))
                {
                    var buffer = CachedMethodBuilder();
                    WriteMethodInfo(methodInfo, buffer, fullName, returnType, writeSpecs, attributes);
                    store[hash] = result = buffer.ToString();
                    buffer.Length = 0;
                }
                return result;
            }
        }

        /// <summary>
        /// Generates the signature of the given property using the default settings, that is using the these specifications:<br/>
        /// 1. The property name is included in the signature;<br/>
        /// 3. All of parameters of the property is included, using short type name, and without the parameter names;<br/>
        /// 4. The return type of the property IS NOT included in the signature;<br/>
        /// 5. The property attributes, accessibilities, modifiers, and the generic type constraints is also excluded.
        /// </summary>
        /// <param name="propertyInfo">Set the <see cref="PropertyInfo"/> instance as the metadata of the requested property to get the signature.</param>
        /// <returns>A <see cref="String"/> that represents the property signature using the default signature generation settings.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static String OfProperty(PropertyInfo propertyInfo) => OfProperty(propertyInfo, false, false, false, false);

        /// <summary>
        /// Generates the signature of the given property using the specified generation settings or the default settings.
        /// </summary>
        /// <param name="propertyInfo">Set the <see cref="PropertyInfo"/> instance as the metadata of the requested property to get the signature.</param>
        /// <param name="fullName">Set with <see langword="true"/> to make all of <see cref="Type"/> in the property (ie. parameters) have full name; The default is <see langword="false"/>.</param>
        /// <param name="returnType">Set with <see langword="true"/> to include the property return type information in the signature. The default is <see langword="false"/>.</param>
        /// <param name="writeSpecs"><see langword="true"/> to include the property accessibility, modifiers, and constraints in the signature. The default is <see langword="false"/>.</param>
        /// <param name="attributes"><see langword="true"/> to include all of the attributes that decorating the property in the signature. The default is <see langword="false"/>.</param>
        /// <returns>A <see cref="String"/> that represents the property signature using specified generation settings or using the default settings.</returns>
        public static String OfProperty(PropertyInfo propertyInfo, Boolean fullName, Boolean returnType, Boolean writeSpecs, Boolean attributes)
        {
            if (ReferenceEquals(propertyInfo, null))
            {
                return "null";
            }
            else
            {
                Int64 hash;
                unsafe
                {
                    *(Int32*)&hash = propertyInfo.GetHashCode();
                    ((Int32*)&hash)[1] = CombineHash32(fullName ? FullNameHash : 0, returnType ? ReturnTypeHash : 0, writeSpecs ? WriteSpecsHash : 0, attributes ? AttributesHash : 0);
                }
                var store = CachedPropertyID;
                if (!store.TryGetValue(hash, out var result))
                {
                    var buffer = CachedMethodBuilder();
                    WritePropertyInfo(propertyInfo, buffer, fullName, returnType, writeSpecs, attributes);
                    store[hash] = result = buffer.ToString();
                    buffer.Length = 0;
                }
                return result;
            }
        }

    }
}