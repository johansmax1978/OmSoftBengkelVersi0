namespace System
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    [Serializable]
    public sealed partial class PersistentStorage : IDictionary<String, Object>, IReadOnlyDictionary<String, Object>, IDictionary, ISerializable
    {
        private sealed partial class StorageHelpers
        {
            private static readonly Dictionary<Type, Object> GetDefaultValue_;

            static StorageHelpers()
            {
                var getdef = new Dictionary<Type, Object>(0x10, TypeComparer.Default)
                {
                    [typeof(Boolean)] = false,
                    [typeof(Char)] = Char.MinValue,
                    [typeof(SByte)] = SByte.MinValue,
                    [typeof(Int16)] = Int16.MinValue,
                    [typeof(Int32)] = Int32.MinValue,
                    [typeof(Int64)] = Int64.MinValue,
                    [typeof(nint)] = (nint)0,
                    [typeof(Byte)] = Byte.MinValue,
                    [typeof(UInt16)] = UInt16.MinValue,
                    [typeof(UInt32)] = UInt32.MinValue,
                    [typeof(UInt64)] = UInt64.MinValue,
                    [typeof(nuint)] = (nuint)0,
                    [typeof(Double)] = 0.0,
                    [typeof(Single)] = 0.0f,
                    [typeof(Decimal)] = 0m,
                    [typeof(DateTime)] = DateTime.MinValue,
                    [typeof(DateTimeOffset)] = DateTimeOffset.MinValue,
                    [typeof(Guid)] = Guid.Empty,
                    [typeof(TimeSpan)] = TimeSpan.Zero,
                    [typeof(String)] = "",
                    [typeof(Object)] = null,
                    [typeof(GCHandle)] = default(GCHandle),
                    [typeof(RuntimeMethodHandle)] = default(RuntimeMethodHandle),
                    [typeof(RuntimeTypeHandle)] = default(RuntimeTypeHandle)
                };
                GetDefaultValue_ = getdef;
            }

            public static Object GetDefaultValue(Type type)
            {
                var storage = GetDefaultValue_;
                if (!storage.TryGetValue(type, out var cache))
                {
                    if (!type.IsValueType)
                    {
                        if (type == typeof(String))
                        {
                            cache = "";
                            goto leave;
                        }
                        else if (type.IsArray)
                        {
                            var rank = type.GetArrayRank();
                            var zero = new Int32[rank];
                            cache = Array.CreateInstance(type.GetElementType(), zero);
                            goto leave;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    DynamicMethod method;
                    var hashcode = RuntimeHelpers.GetHashCode(type);
                    var identity = hashcode.ToString("X8", CultureInfo.InvariantCulture);
                    var name = String.Concat("GetDefaultValue_", identity);
                    var @return = typeof(Object);
                    try
                    {
                        method = new DynamicMethod(name, @return, null, type, true);
                    }
                    catch
                    {
                        try
                        {
                            method = new DynamicMethod(name, @return, null, type.Module, true);
                        }
                        catch
                        {
                            cache = FormatterServices.GetUninitializedObject(type);
                            goto leave;
                        }
                    }
                    var stream = method.GetILGenerator();
                    _ = stream.DeclareLocal(type);
                    stream.Emit(OpCodes.Ldloca_S, (SByte)0);
                    stream.Emit(OpCodes.Initobj, type);
                    stream.Emit(OpCodes.Ldloc_0);
                    stream.Emit(OpCodes.Box, type);
                    stream.Emit(OpCodes.Ret);
                    cache = method;
                leave:
                    storage[type] = cache;
                }
                return cache is DynamicMethod fn ? fn.Invoke(null, null) : cache;
            }

            public static Type TypeFromString(String type)
            {
                if (type is null || type.Length == 0 || (type = type.Trim()).Length == 0)
                {
                    return null;
                }
                var result = Type.GetType(type, false, true);
                if (result is null)
                {
                    var asm = AppDomain.CurrentDomain.GetAssemblies();
                    for (var i = 0; result is null && i < asm.Length; i++)
                    {
                        result = asm[i].GetType(type, false, true);
                    }
                }
                return result;
            }

            public static Type GetTypeAndSync(String type, ref Object data, Boolean convert)
            {
                if (type is null || type.Length == 0)
                {
                    return data?.GetType();
                }
                var result = Type.GetType(type, false, true);
                if (result is null)
                {
                    var asm = AppDomain.CurrentDomain.GetAssemblies();
                    for (var i = 0; result is null && i < asm.Length; i++)
                    {
                        result = asm[i].GetType(type, false, true);
                    }
                    if (result is null)
                    {
                        return data?.GetType();
                    }
                }
                if (convert)
                {
                    if (data is null)
                    {
                        data = GetDefaultValue(result);
                    }
                    else
                    {
                        var dtype = data.GetType();
                        if (dtype != result)
                        {
                            try
                            {
                                var converted = Convert.ChangeType(data, result, CultureInfo.CurrentCulture);
                                data = converted;
                            }
                            catch { }
                        }
                    }
                }
                return result;
            }
        }

        private static partial class SerializableUtils
        {
            private interface ISRProvider
            {
                Object Restore();
                void Serialize(Object value);
            }

            [Serializable]
            private sealed partial class SRSecureString : ISerializable
            {
                private Boolean Locked;
                private String Content;

                public SecureString Restore()
                {
                    var value = this.Content;
                    if (value is null)
                    {
                        return null;
                    }
                    SecureString secure;
                    if (value.Length == 0)
                    {
                        secure = new SecureString();
                    }
                    else
                    {
                        unsafe
                        {
                            fixed (Char* chars = value)
                            {
                                secure = new SecureString(chars, value.Length);
                            }
                        }
                    }
                    if (this.Locked)
                    {
                        secure.MakeReadOnly();
                    }
                    return secure;
                }

                public unsafe void Serialize(SecureString value)
                {
                    if (value is null)
                    {
                        this.Content = null;
                        this.Locked = false;
                        return;
                    }
                    this.Locked = value.IsReadOnly();
                    nint handle = Marshal.SecureStringToGlobalAllocUnicode(value);
                    if (handle == 0)
                    {
                        handle = Marshal.SecureStringToBSTR(value);
                        if (handle != 0)
                        {
                            try
                            {
                                this.Content = Marshal.PtrToStringBSTR(handle);
                            }
                            finally
                            {
                                Marshal.ZeroFreeBSTR(handle);
                            }
                        }
                        else
                        {
                            handle = Marshal.SecureStringToCoTaskMemUnicode(value);
                            if (handle == 0)
                            {
                                this.Content = "";
                            }
                            else
                            {
                                try
                                {
                                    this.Content = Marshal.PtrToStringUni(handle);
                                }
                                finally
                                {
                                    Marshal.ZeroFreeCoTaskMemUnicode(handle);
                                }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            this.Content = new String((Char*)handle, 0, value.Length);
                        }
                        finally
                        {
                            Marshal.ZeroFreeGlobalAllocUnicode(handle);
                        }
                    }

                }

                public SRSecureString() { }

                public SRSecureString(SecureString value) => this.Serialize(value);

                public SRSecureString(SerializationInfo info, StreamingContext context)
                {
                    this.Locked = info.GetBoolean("locked");
                    this.Content = info.GetString("content");
                }

                public void GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    info.SetType(typeof(SRSecureString));
                    info.AddValue("locked", this.Locked);
                    info.AddValue("content", this.Content, typeof(String));
                }
            }

            [Serializable]
            private sealed partial class SRJToken : ISerializable
            {
                private String Data;

                public Object Restore()
                {
                    if (this.Data is String json)
                    {
                        try
                        {
                            return JToken.Parse(json, new JsonLoadSettings
                            {
                                CommentHandling = CommentHandling.Load,
                                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace,
                                LineInfoHandling = LineInfoHandling.Load
                            });
                        }
                        catch
                        {
                            return json;
                        }
                    }
                    return null;
                }

                public void Serialize(JToken value) => this.Data = value?.ToString(Newtonsoft.Json.Formatting.None);

                public SRJToken() { }

                public SRJToken(JToken value) => this.Serialize(value);

                public SRJToken(SerializationInfo info, StreamingContext context)
                    => this.Data = info.GetString("json");

                public void GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    info.SetType(typeof(SRJToken));
                    info.AddValue("json", this.Data, typeof(String));
                }
            }

            [Serializable]
            private sealed partial class SRXml : ISerializable
            {
                private static readonly XmlSerializerFactory Factory = new XmlSerializerFactory();

                private String Type;
                private String Data;

                public Object Restore()
                {
                    if (this.Data is String xml && xml.Length != 0)
                    {
                        var type = this.Type;
                        try
                        {
                            using var reader = new StringReader(xml);
                            using var xreader = XmlReader.Create(reader, new XmlReaderSettings { Async = true, CheckCharacters = false, IgnoreWhitespace = true, IgnoreComments = true, IgnoreProcessingInstructions = true, CloseInput = false });
                            if (type == "XmlDocument")
                            {
                                var doc = new XmlDocument();
                                doc.Load(xreader);
                                return doc;
                            }
                            else if (type == "XNode")
                            {
                                return XNode.ReadFrom(xreader);
                            }
                            else if (type == "XElement")
                            {
                                return XElement.Load(xreader, LoadOptions.None);
                            }
                            else
                            {
                                var otype = StorageHelpers.TypeFromString(type);
                                if (otype is not null)
                                {
                                    var serializer = Factory.CreateSerializer(otype);
                                    return serializer.Deserialize(xreader);
                                }
                            }
                        }
                        catch { }
                        return xml;
                    }
                    return null;
                }

                public void Serialize(XNode value)
                {
                    if (value is not null)
                    {
                        this.Data = value.ToString(SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces);
                        this.Type = value is XElement ? "XElement" : "XNode";
                    }
                    else
                    {
                        this.Data = null;
                        this.Type = null;
                    }
                }

                public void Serialize(XmlDocument value)
                {
                    if (value is not null)
                    {
                        var builder = new StringBuilder(8192);
                        using (var writer = new StringWriter(builder, CultureInfo.CurrentCulture))
                        {
                            using var xwriter = XmlWriter.Create(writer, new XmlWriterSettings
                            {
                                Async = true,
                                CheckCharacters = false,
                                CloseOutput = false,
                                Encoding = new UTF8Encoding(false, false),
                                Indent = false,
                                OmitXmlDeclaration = true,
                                NamespaceHandling = NamespaceHandling.OmitDuplicates
                            });
                            value.Save(xwriter);
                        }
                        this.Type = "XmlDocument";
                        this.Data = builder.ToString();
                    }
                    else
                    {
                        this.Data = null;
                        this.Type = null;
                    }
                }

                public void Serialize(Object value)
                {
                    if (value is not null)
                    {
                        var type = value.GetType();
                        var builder = new StringBuilder(8192);
                        using (var writer = new StringWriter(builder))
                        {
                            using var xwriter = XmlWriter.Create(writer, new XmlWriterSettings
                            {
                                Async = true,
                                Encoding = new UTF8Encoding(false, false),
                                CloseOutput = false,
                                Indent = false,
                                OmitXmlDeclaration = true
                            });
                            Factory.CreateSerializer(type).Serialize(xwriter, value);
                        }
                        this.Type = type.ToString();
                        this.Data = builder.ToString();
                    }
                    else
                    {
                        this.Type = null;
                        this.Data = null;
                    }
                }

                public SRXml() { }

                public SRXml(XNode value) => this.Serialize(value);

                public SRXml(XmlDocument value) => this.Serialize(value);

                public SRXml(Object value) => this.Serialize(value);

                public SRXml(SerializationInfo info, StreamingContext context)
                {
                    this.Type = info.GetString("type");
                    this.Data = info.GetString("data");
                }

                public void GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    info.SetType(typeof(SRXml));
                    info.AddValue("type", this.Type, typeof(String));
                    info.AddValue("data", this.Data, typeof(String));
                }
            }

            [Serializable]
            private sealed partial class SRJson : ISerializable
            {
                private static readonly JsonSerializer Serializer = JsonDefault.CreateJsonEngine(JsonOptions.Inline | JsonOptions.Typed);

                private String Type;
                private String Data;

                public Object Restore()
                {
                    if (this.Data is String data && data.Length != 0)
                    {
                        var type = StorageHelpers.TypeFromString(this.Type);
                        try
                        {
                            using (var reader = new StringReader(data))
                            {
                                using var jreader = new JsonTextReader(reader)
                                {
                                    ArrayPool = JsonCharsPool.Instance,
                                    CloseInput = false,
                                    Culture = CultureInfo.CurrentCulture,
                                    DateParseHandling = DateParseHandling.DateTime,
                                    DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                                    FloatParseHandling = FloatParseHandling.Double,
                                    SupportMultipleContent = true
                                };
                                return Serializer.Deserialize(jreader, type);
                            }
                        }
                        catch
                        {
                            return data;
                        }
                    }
                    return null;
                }

                public void Serialize(Object value)
                {
                    if (value is not null)
                    {
                        var type = value.GetType();
                        var builder = new StringBuilder(8192);
                        using (var writer = new StringWriter(builder))
                        {
                            using var jwriter = new JsonTextWriter(writer)
                            {
                                Formatting = Newtonsoft.Json.Formatting.None,
                                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                                ArrayPool = JsonCharsPool.Instance,
                                CloseOutput = false,
                                Culture = CultureInfo.CurrentCulture,
                                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                                FloatFormatHandling = FloatFormatHandling.DefaultValue
                            };
                            Serializer.Serialize(jwriter, value, type);
                        }
                        this.Type = type.ToString();
                        this.Data = builder.ToString();
                    }
                    else
                    {
                        this.Type = null;
                        this.Data = null;
                    }
                }

                public SRJson() { }

                public SRJson(Object value) => this.Serialize(value);

                public SRJson(SerializationInfo info, StreamingContext context)
                {
                    this.Type = info.GetString("type");
                    this.Data = info.GetString("data");
                }

                public void GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    info.SetType(typeof(SRJson));
                    info.AddValue("type", this.Type, typeof(String));
                    info.AddValue("data", this.Data, typeof(String));
                }
            }

            [Serializable]
            private sealed partial class SRDataContract : ISerializable
            {
                private sealed partial class SRDCResolver : DataContractResolver
                {
                    private readonly Dictionary<String, XmlDictionaryString> memoize;

                    private SRDCResolver() => this.memoize = new Dictionary<String, XmlDictionaryString>(StringComparer.CurrentCulture);

                    public override Type ResolveName(String typeName, String typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
                        => this.memoize.TryGetValue(typeName, out var tName) && this.memoize.TryGetValue(typeNamespace, out var tNamespace)
                        ? StorageHelpers.TypeFromString($"{tNamespace.Value}.{tName.Value}")
                        : null;

                    public override Boolean TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
                    {
                        var name = type.Name;
                        var namesp = type.Namespace;
                        typeName = new XmlDictionaryString(XmlDictionary.Empty, name, 0);
                        typeNamespace = new XmlDictionaryString(XmlDictionary.Empty, namesp, 0);
                        memoize[name] = typeName;
                        memoize[namesp] = typeNamespace;
                        return true;
                    }

                    public static readonly SRDCResolver Default = new();
                }

                private static readonly Dictionary<Type, DataContractSerializer> caches;
                private static readonly Dictionary<Type, HashSet<Type>> known;
                private static readonly HashSet<Type> commons;

                static SRDataContract()
                {
                    caches = new Dictionary<Type, DataContractSerializer>(0x10, TypeComparer.Default);
                    known = new Dictionary<Type, HashSet<Type>>(0x10, TypeComparer.Default);
                    var cm = new HashSet<Type>(0x10, TypeComparer.Default)
                    {
                        typeof(String),
                        typeof(DateTime),
                        typeof(DateTimeOffset),
                        typeof(TimeSpan),
                        typeof(Guid),
                        typeof(Decimal),
                        typeof(MemberInfo)
                    };
                    commons = cm;
                }

                private static HashSet<Type> GetKnownTypes(Type type)
                {
                    var caches = known;
                    if (!caches.TryGetValue(type, out var result))
                    {
                        var key = type;
                        var ignore = typeof(IgnoreDataMemberAttribute);
                        var marker = typeof(DataMemberAttribute);
                        result = new HashSet<Type>(0x10, TypeComparer.Default) { type };
                        var backing = new Char[] { '<', '>' };
                        while (type is not null && type != typeof(Object))
                        {
                            var found = 0;
                            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                            for (var i = 0; i < properties.Length;)
                            {
                                var property = properties[i++];
                                if (property.GetIndexParameters().Length == 0 || !property.CanRead) continue;
                                if (!property.IsDefined(ignore) && property.IsDefined(marker))
                                {
                                    var ptype = property.PropertyType;
                                    if (ptype != type && result.Add(ptype) && !ptype.IsInterface)
                                    {
                                        if (!ptype.IsEnum && !ptype.IsPrimitive && !commons.Contains(ptype) &&
                                            !typeof(MemberInfo).IsAssignableFrom(ptype))
                                        {
                                            while (ptype.IsArray)
                                            {
                                                ptype = ptype.GetElementType();
                                            }
                                            foreach (var vtype in GetKnownTypes(ptype))
                                            {
                                                _ = result.Add(vtype);
                                            }
                                        }
                                    }
                                    found++;
                                }
                            }
                            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                            for (var i = 0; i < fields.Length;)
                            {
                                var field = fields[i++];
                                if (field.Name.IndexOfAny(backing) != -1) continue;
                                if (field.IsDefined(ignore) || !field.IsDefined(marker)) continue;
                                var ftype = field.FieldType;
                                if (ftype != type && result.Add(ftype) && !ftype.IsInterface)
                                {
                                    if (!ftype.IsEnum && !ftype.IsPrimitive && !commons.Contains(ftype)
                                        && !typeof(MemberInfo).IsAssignableFrom(ftype))
                                    {
                                        while (ftype.IsArray)
                                        {
                                            ftype = ftype.GetElementType();
                                        }
                                        foreach (var vtype in GetKnownTypes(ftype))
                                        {
                                            _ = result.Add(vtype);
                                        }
                                    }
                                }
                                found++;
                            }

                            if (found == 0)
                            {
                                for (var i = 0; i < properties.Length;)
                                {
                                    var property = properties[i++];
                                    if (!property.CanRead || property.GetIndexParameters().Length != 0 || property.IsDefined(ignore)) continue;
                                    var getter = property.GetGetMethod(true);
                                    if (getter.IsPublic || getter.IsFamilyOrAssembly || getter.IsFamily)
                                    {
                                        var ptype = property.PropertyType;
                                        if (ptype != type && result.Add(ptype) && !ptype.IsInterface)
                                        {
                                            if (!ptype.IsEnum && !ptype.IsPrimitive && !commons.Contains(ptype) && !typeof(MemberInfo).IsAssignableFrom(ptype))
                                            {
                                                while (ptype.IsArray)
                                                {
                                                    ptype = ptype.GetElementType();
                                                }
                                                foreach (var vtype in GetKnownTypes(ptype))
                                                {
                                                    _ = result.Add(vtype);
                                                }
                                            }
                                        }
                                        found++;
                                    }
                                }
                                for (var i = 0; i < fields.Length;)
                                {
                                    var field = fields[i++];
                                    if (field.Name.IndexOfAny(backing) != -1) continue;
                                    if (field.IsDefined(ignore) || field.IsPrivate || field.IsAssembly || field.IsFamilyAndAssembly) continue;
                                    var ftype = field.FieldType;
                                    if (ftype != type && result.Add(ftype) && !ftype.IsInterface)
                                    {
                                        if (!ftype.IsEnum && !ftype.IsPrimitive && !commons.Contains(ftype) && !typeof(MemberInfo).IsAssignableFrom(ftype))
                                        {
                                            while (ftype.IsArray)
                                            {
                                                ftype = ftype.GetElementType();
                                            }
                                            foreach (var vtype in GetKnownTypes(ftype))
                                            {
                                                _ = result.Add(vtype);
                                            }
                                        }
                                    }
                                    found++;
                                }
                            }
                            type = type.BaseType;
                        }
                        caches[key] = result;
                    }
                    return result;
                }

                public static DataContractSerializerSettings Settings(Type type) => new DataContractSerializerSettings
                {
                    IgnoreExtensionDataObject = false,
                    KnownTypes = GetKnownTypes(type),
                    SerializeReadOnlyTypes = true,
                    MaxItemsInObjectGraph = Int32.MaxValue,
                    DataContractResolver = SRDCResolver.Default
                };

                public static DataContractSerializer Factory(Type type)
                {
                    var caches = SRDataContract.caches;
                    if (!caches.TryGetValue(type, out var result))
                    {
                        caches[type] = result = new DataContractSerializer(type, Settings(type));
                    }
                    return result;
                }

                private String Type;
                private String Data;

                public Object Restore()
                {
                    if (this.Data is String data && data.Length != 0)
                    {
                        var type = StorageHelpers.TypeFromString(this.Type);
                        if (type is not null)
                        {
                            try
                            {
                                using var reader = new StringReader(data);
                                using var xreader = XmlReader.Create(reader, new XmlReaderSettings { Async = true, CheckCharacters = false, IgnoreWhitespace = true, IgnoreComments = true, IgnoreProcessingInstructions = true, CloseInput = false });
                                return Factory(type).ReadObject(xreader);
                            }
                            catch { }
                        }
                        return data;
                    }
                    return null;
                }

                public void Serialize(Object value)
                {
                    if (value is null)
                    {
                        this.Type = null;
                        this.Data = null;
                    }
                    else
                    {
                        var type = value.GetType();
                        var builder = new StringBuilder(8192);
                        using (var writer = new StringWriter(builder, CultureInfo.CurrentCulture))
                        {
                            using var xwriter = XmlWriter.Create(writer, new XmlWriterSettings
                            {
                                Async = true,
                                CheckCharacters = false,
                                CloseOutput = false,
                                Encoding = new UTF8Encoding(false, false),
                                Indent = false
                            });
                            Factory(type).WriteObject(xwriter, value);
                        }
                        this.Data = builder.ToString();
                        this.Type = value.ToString();
                    }
                }

                public SRDataContract() { }

                public SRDataContract(Object value) => this.Serialize(value);

                public SRDataContract(SerializationInfo info, StreamingContext context)
                {
                    this.Type = info.GetString("type");
                    this.Data = info.GetString("data");
                }

                public void GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    info.SetType(typeof(SRDataContract));
                    info.AddValue("type", this.Type, typeof(String));
                    info.AddValue("data", this.Data, typeof(String));
                }
            }

            public static Object Restore(Object value)
                => value is ISRProvider provider ? provider.Restore() : value;

            public static Object Restore(Object value, ref Type type)
            {
                if (value is null)
                {
                    return type is not null ? StorageHelpers.GetDefaultValue(type) : null;
                }
                if (value is ISRProvider provider)
                {
                    var result = provider.Restore();
                    type = result?.GetType();
                    return result;
                }
                try
                {
                    return Convert.ChangeType(value, type, CultureInfo.CurrentCulture);
                }
                catch
                {
                    type = value?.GetType();
                    return value;
                }
            }

            public static void Serialize(Object value, String name, SerializationInfo info)
            {
                if (value is SecureString secure)
                {
                    info.AddValue(name, new SRSecureString(secure), typeof(SRSecureString));
                }
                else if (value is JToken token)
                {
                    info.AddValue(name, new SRJToken(token), typeof(SRJToken));
                }
                else if (value is XNode node)
                {
                    info.AddValue(name, new SRXml(node), typeof(SRXml));
                }
                else if (value is XmlDocument xdoc)
                {
                    info.AddValue(name, new SRXml(xdoc), typeof(SRXml));
                }
                else if (value is IJsonSerializable jsonable)
                {
                    info.AddValue(name, new SRJson(jsonable), typeof(SRJson));
                }
                else
                {
                    var type = value.GetType();
                    if (type.IsDefined(typeof(JsonObjectAttribute), false))
                    {
                        try { info.AddValue(name, new SRJson(value), typeof(SRJson)); return; }
                        catch { }
                    }
                    if (type.IsDefined(typeof(DataContractAttribute), false))
                    {
                        try { info.AddValue(name, new SRDataContract(value), typeof(SRDataContract)); return; }
                        catch { }
                    }
                    if (typeof(IXmlSerializable).IsAssignableFrom(type) || type.IsDefined(typeof(XmlTypeAttribute)) || type.IsDefined(typeof(XmlRootAttribute)))
                    {
                        try { info.AddValue(name, new SRXml(value), typeof(SRXml)); return; }
                        catch { }
                    }
                    info.AddValue(name, value.ToString(), typeof(String));
                }
            }
        }

        [Serializable]
        private sealed partial class StorageElement : ISerializable
        {
            public String Name;
            public Type Type;
            public Object Data;

            public StorageElement() { }

            public StorageElement(String name, Object data) : this(name, data, null) { }

            public StorageElement(String name, Object data, Type type)
            {
                this.Name = name;
                this.Type = type is null && data is not null ? data.GetType() : type;
                this.Data = data;
            }

            private StorageElement(SerializationInfo info, StreamingContext context)
            {
                Type type = null;
                var iterator = info.GetEnumerator();
                while (iterator.MoveNext())
                {
                    var current = iterator.Current;
                    var value = current.Value;
                    if (value is null) continue;
                    var name = current.Name.ToUpperInvariant();
                    if (name == "NAME")
                    {
                        this.Name = Convert.ToString(value, CultureInfo.CurrentCulture);
                    }
                    else if (name == "TYPE")
                    {
                        type = StorageHelpers.TypeFromString(Convert.ToString(value, CultureInfo.InvariantCulture));
                        this.Type = type;
                    }
                    else if (name == "DATA")
                    {
                        this.Data = SerializableUtils.Restore(value, ref type);
                    }
                }
                var dtype = this.Data?.GetType();
                var vtype = this.Type;
                if (vtype is null)
                {
                    this.Type = dtype;
                }
                else if (dtype is not null && dtype != vtype)
                {
                    try
                    {
                        this.Data = Convert.ChangeType(this.Data, vtype, CultureInfo.CurrentCulture);
                    }
                    catch
                    {
                        this.Type = dtype;
                    }
                }
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.SetType(typeof(StorageElement));
                info.AddValue("name", this.Name, typeof(String));
                info.AddValue("type", this.Type?.ToString(), typeof(String));
                if (this.Data is Object data)
                {
                    var type = data.GetType();
                    if (type.IsSerializable)
                    {
                        info.AddValue("data", data, type);
                    }
                    else
                    {
                        SerializableUtils.Serialize(data, "data", info);
                    }
                }
                else
                {
                    info.AddValue("data", null, typeof(Object));
                }
            }


        }

        private static partial class ExportImport
        {
            private static readonly BinaryFormatter Formatter;
            private static readonly String DataFolder;
            private static readonly WaitCallback ExportFunc;
            private static readonly Action<Object> ImportFunc;

            static ExportImport()
            {
                Formatter = new BinaryFormatter(new SurrogateSelector(), new StreamingContext(StreamingContextStates.File))
                {
                    AssemblyFormat = FormatterAssemblyStyle.Simple,
                    TypeFormat = FormatterTypeStyle.TypesWhenNeeded,
                    FilterLevel = TypeFilterLevel.Full
                };
                DataFolder = Path.Combine(Path.GetDirectoryName(Path.GetFileName("dummy.file")), "data", "store");
                ExportFunc = ExportCore;
                ImportFunc = ImportCore;
            }

            private abstract class ObjectState
            {
                private static readonly Guid DefaultGUID;

                private static Guid GenerateGUID(String name)
                {
                    var bytes = Encoding.UTF8.GetBytes(name);
                    using (var algorithm = MD5.Create())
                    {
                        bytes = algorithm.ComputeHash(bytes);
                    }
                    return new Guid(bytes);
                }

                static ObjectState()
                {
                    DefaultGUID = GenerateGUID("DEFAULT");
                }

                public ObjectState(String name)
                {
                    if (name is null || name.Length == 0 || (name = name.Trim()).Length == 0)
                    {
                        this.Guid = DefaultGUID;
                    }
                    else
                    {
                        this.Guid = GenerateGUID(name.ToUpperInvariant());
                    }
                }

                public readonly Guid Guid;
            }

            private sealed partial class ExportState : ObjectState
            {
                public ExportState(String name, Dictionary<String, StorageElement> data) : base(name)
                {
                    if (data.Count != 0)
                    {
                        var array = new StorageElement[data.Count];
                        data.Values.CopyTo(array, 0);
                        this.Data = array;
                    }
                    else
                    {
                        this.Data = Array.Empty<StorageElement>();
                    }
                }

                public readonly StorageElement[] Data;
            }

            private sealed partial class ImportState : ObjectState
            {
                public ImportState(String name) : base(name) { }

                public StorageElement[] Data;
            }

            private static void ExportCore(Object args)
            {
                var folder = DataFolder;
                if (!Directory.Exists(folder))
                {
                    folder = Directory.CreateDirectory(folder).FullName;
                }
                var state = (ExportState)args;
                var path = Path.Combine(folder, $"{state.Guid}.persistent");
                using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 8192, true))
                {
                    if (stream.Length != 0)
                    {
                        stream.SetLength(0);
                    }
                    using var deflate = new GZipStream(stream, CompressionLevel.Optimal, true);
                    Formatter.Serialize(deflate, state.Data);
                }
            }

            private static void ImportCore(Object args)
            {
                var state = (ImportState)args;
                var folder = DataFolder;
                var path = Path.Combine(folder, $"{state.Guid}.persistent");
                if (File.Exists(path))
                {
                    try
                    {
                        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8192, true);
                        using var deflate = new GZipStream(stream, CompressionMode.Decompress, true);
                        state.Data = (StorageElement[])Formatter.UnsafeDeserialize(deflate, null) ?? Array.Empty<StorageElement>();
                    }
                    catch
                    {
                        state.Data = Array.Empty<StorageElement>();
                    }
                }
                else
                {
                    state.Data = null;
                }
            }

            public static void Export(String name, Dictionary<String, StorageElement> data)
            {
                if (data is not null && data.Count != 0)
                {
                    var state = new ExportState(name, data);
                    ExportCore(state);
                }
            }

            public static void ExportAsync(String name, Dictionary<String, StorageElement> data)
            {
                if (data is not null && data.Count != 0)
                {
                    var state = new ExportState(name, data);
                    _ = ThreadPool.UnsafeQueueUserWorkItem(ExportFunc, state);
                }
            }

            public static void Import(String name, Dictionary<String, StorageElement> data)
            {
                var state = new ImportState(name);
                ImportCore(state);
                if (state.Data is not null)
                {
                    data.Clear();
                    foreach (var item in state.Data) data[item.Name] = item;
                }
            }

            public static async Task ImportAsync(String name, Dictionary<String, StorageElement> data, CancellationToken ctoken)
            {
                if (ctoken.IsCancellationRequested) return;
                var state = new ImportState(name);
                await Task.Factory.StartNew(ImportFunc, state, ctoken, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
                if (state.Data is not null)
                {
                    data.Clear();
                    foreach (var item in state.Data) data[item.Name] = item;
                }
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly String name;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Dictionary<String, StorageElement> data;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly Object root;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private NameCollection names;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ValueCollection values;

        public PersistentStorage() : this(null) { }

        public PersistentStorage(String name)
        {
            this.name = name is null || name.Length == 0 ? "Default" : name;
            this.data = new(0x10, StringComparer.CurrentCultureIgnoreCase);
            this.root = new();
        }

        private PersistentStorage(String name, Dictionary<String, StorageElement> source)
        {
            this.name = name;
            var data = new Dictionary<String, StorageElement>(source.Count, source.Comparer);
            foreach (var item in source.Values)
            {
                data[item.Name] = new StorageElement(item.Name, item.Data, item.Type);
            }
            this.data = data;
            this.root = new();
        }

        public Int32 Count => this.data.Count;

        public Object this[String name] { get => this.Read(name); set => this.Write(name, value); }

        public NameCollection Names => this.names ??= new(this);

        public ValueCollection Values => this.values ??= new(this);

        public PersistentStorage Copy() => new(this.name, this.data);

        public void Copy(KeyValuePair<String, Object>[] array)
        {
            var count = this.data.Count;
            if (count == 0) return;
            if (array is null) throw new ArgumentNullException(nameof(array));
            if (array.Length < count) throw new ArgumentOutOfRangeException(nameof(array));
            var iterator = this.data.GetEnumerator();
            var index = 0;
            while (iterator.MoveNext())
            {
                var current = iterator.Current;
                array[index++] = new(current.Key, current.Value.Data);
            }
        }

        public void Copy(KeyValuePair<String, Object>[] array, Int32 arrayIndex)
        {
            var count = this.data.Count;
            if (count == 0) return;
            if (array is null) throw new ArgumentNullException(nameof(array));
            if ((UInt32)arrayIndex >= (UInt32)array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < count) throw new ArgumentOutOfRangeException(nameof(array));
            var iterator = this.data.GetEnumerator();
            while (iterator.MoveNext())
            {
                var current = iterator.Current;
                array[arrayIndex++] = new(current.Key, current.Value.Data);
            }
        }

        public Boolean Exists(String name) => this.data.ContainsKey(name ?? "");

        public Object Read(String name)
            => this.data.TryGetValue(name ?? "", out var element) ? element.Data : null;

        public Boolean Read(String name, out Object value)
        {
            if (this.data.TryGetValue(name ?? "", out var element))
            {
                value = element.Data;
                return true;
            }
            value = null;
            return false;
        }

        public T Read<T>(String name)
        {
            _ = this.Read<T>(name, out var value);
            return value;
        }

        public Boolean Read<T>(String name, out T value)
        {
            if (this.data.TryGetValue(name ?? "", out var element))
            {
                try
                {
                    value = element.Data is T actual ? actual : (T)Convert.ChangeType(element.Data, typeof(T), CultureInfo.CurrentCulture);
                    return true;
                }
                catch { }
            }
            value = default;
            return false;
        }

        public void Write(String name, Object value)
        {
            if (name is null)
            {
                name = "";
            }
            if (!this.data.TryGetValue(name, out var element))
            {
                this.data[name] = new StorageElement(name, value);
            }
            else
            {
                element.Data = value;
                element.Type = value?.GetType();
            }
        }

        public void Write<T>(String name, T value)
        {
            if (name is null)
            {
                name = "";
            }
            if (!this.data.TryGetValue(name, out var element))
            {
                this.data[name] = new StorageElement(name, value, value is null ? typeof(T) : value.GetType());
            }
            else
            {
                element.Data = value;
                element.Type = value is null ? typeof(T) : value.GetType();
            }
        }

        public void Save() => ExportImport.Export(this.name, this.data);

        public void Save(Boolean async)
        {
            if (async)
            {
                ExportImport.ExportAsync(this.name, this.data);
            }
            else
            {
                ExportImport.Export(this.name, this.data);
            }
        }

        public void Load()
            => ExportImport.Import(this.name, this.data);

        public Task Load(CancellationToken ctoken)
            => ExportImport.ImportAsync(this.name, this.data, ctoken);

        public void Clear() => this.data.Clear();

        public void Strip(String name) => _ = this.data.Remove(name ?? "");

        public void Strip(params String[] names)
        {
            if (name is not null && name.Length != 0)
            {
                var data = this.data;
                for (var i = 0; i < names.Length; i++)
                {
                    _ = data.Remove(names[i] ?? "");
                }
            }
        }

        public Enumerator Views() => new(this);

        public partial struct Enumerator : IEnumerator<KeyValuePair<String, Object>>, IDictionaryEnumerator
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly PersistentStorage parent;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private Dictionary<String, StorageElement>.Enumerator iterator;

            public Enumerator(PersistentStorage parent)
            {
                this.parent = parent;
                this.iterator = parent.data.GetEnumerator();
            }

            public readonly KeyValuePair<String, Object> Current
            {
                get
                {
                    var value = this.iterator.Current;
                    return new(value.Key, value.Value.Data);
                }
            }

            public Boolean MoveNext() => this.iterator.MoveNext();

            public void Reset() => this.iterator = this.parent.data.GetEnumerator();

            public readonly void Dispose() => this.iterator.Dispose();

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            readonly Object IDictionaryEnumerator.Key => this.iterator.Current.Key;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            readonly Object IDictionaryEnumerator.Value => this.iterator.Current.Value;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            readonly DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    var value = this.iterator.Current;
                    return new(value.Key, value.Value.Data);
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            readonly Object IEnumerator.Current => this.Current;
        }

        public sealed partial class NameCollection : ICollection, IReadOnlyCollection<String>, ICollection<String>
        {
            private readonly PersistentStorage parent;

            public NameCollection(PersistentStorage parent) => this.parent = parent ?? throw new ArgumentNullException(nameof(parent));

            public Int32 Count => this.parent.data.Count;

            public void CopyTo(String[] array)
            {
                var data = this.parent.data;
                var count = data.Count;
                if (count == 0) return;
                if (array is null) throw new ArgumentNullException(nameof(array));
                if (array.Length < count) throw new ArgumentOutOfRangeException(nameof(array));
                var iterator = data.Keys.GetEnumerator();
                var arrayIndex = 0;
                while (iterator.MoveNext())
                {
                    array[arrayIndex++] = iterator.Current;
                }
            }

            public void CopyTo(String[] array, Int32 arrayIndex)
            {
                var data = this.parent.data;
                var count = data.Count;
                if (count == 0) return;
                if (array is null) throw new ArgumentNullException(nameof(array));
                if ((UInt32)arrayIndex >= (UInt32)array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                if (array.Length - arrayIndex < count) throw new ArgumentOutOfRangeException(nameof(array));
                var iterator = data.Keys.GetEnumerator();
                while (iterator.MoveNext())
                {
                    array[arrayIndex++] = iterator.Current;
                }
            }

            public Boolean Contains(String name) => this.parent.data.ContainsKey(name);

            public Enumerator GetEnumerator() => new(this);

            public partial struct Enumerator : IEnumerator<String>
            {
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly NameCollection parent;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private Dictionary<String, StorageElement>.KeyCollection.Enumerator iterator;

                public Enumerator(NameCollection parent)
                {
                    this.parent = parent;
                    this.iterator = parent.parent.data.Keys.GetEnumerator();
                }

                public readonly String Current => this.iterator.Current;
                public Boolean MoveNext() => this.iterator.MoveNext();
                public void Reset() => this.iterator = parent.parent.data.Keys.GetEnumerator();
                public void Dispose() => this.iterator.Dispose();
                public readonly NameCollection Parent => this.parent;

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                readonly Object IEnumerator.Current => this.iterator.Current;
            }

            void ICollection<String>.Add(String item) => throw new NotSupportedException("This collection is a read only view, use the parent collection to modify the contents.");
            void ICollection<String>.Clear() => throw new NotSupportedException("This collection is a read only view, use the parent collection to modify the contents.");
            Boolean ICollection<String>.Remove(String item) => throw new NotSupportedException("This collection is a read only view, use the parent collection to modify the contents.");
            Boolean ICollection<String>.IsReadOnly => true;
            IEnumerator<String> IEnumerable<String>.GetEnumerator() => new Enumerator(this);
            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);
            void ICollection.CopyTo(Array array, Int32 index)
            {
                var data = this.parent.data;
                var count = data.Count;
                if (count == 0) return;
                if (array is null) throw new ArgumentNullException(nameof(array));
                if ((UInt32)index >= (UInt32)array.Length) throw new ArgumentOutOfRangeException(nameof(index));
                if (array.Length - index < count) throw new ArgumentOutOfRangeException(nameof(array));
                var iterator = data.Keys.GetEnumerator();
                if (array is String[] strings)
                    while (iterator.MoveNext())
                        strings[index++] = iterator.Current;
                else if (array is Object[] objects)
                    while (iterator.MoveNext())
                        objects[index++] = iterator.Current;
                else
                {
                    if (array.Rank != 1) throw new RankException();
                    var element = array.GetType().GetElementType();
                    while (iterator.MoveNext())
                        array.SetValue(Convert.ChangeType(iterator.Current, element, CultureInfo.CurrentCulture), index++);
                }

            }
            Object ICollection.SyncRoot => this.parent.root;
            Boolean ICollection.IsSynchronized => false;
        }

        public sealed partial class ValueCollection : ICollection, IReadOnlyCollection<Object>, ICollection<Object>
        {
            private readonly PersistentStorage parent;

            public ValueCollection(PersistentStorage parent) => this.parent = parent ?? throw new ArgumentNullException(nameof(parent));

            public Int32 Count => this.parent.data.Count;

            public void CopyTo(Object[] array)
            {
                var data = this.parent.data;
                var count = data.Count;
                if (count == 0) return;
                if (array is null) throw new ArgumentNullException(nameof(array));
                if (array.Length < count) throw new ArgumentOutOfRangeException(nameof(array));
                var iterator = data.Values.GetEnumerator();
                var arrayIndex = 0;
                while (iterator.MoveNext())
                {
                    array[arrayIndex++] = iterator.Current.Data;
                }
            }

            public void CopyTo(Object[] array, Int32 arrayIndex)
            {
                var data = this.parent.data;
                var count = data.Count;
                if (count == 0) return;
                if (array is null) throw new ArgumentNullException(nameof(array));
                if ((UInt32)arrayIndex >= (UInt32)array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                if (array.Length - arrayIndex < count) throw new ArgumentOutOfRangeException(nameof(array));
                var iterator = data.Values.GetEnumerator();
                while (iterator.MoveNext())
                {
                    array[arrayIndex++] = iterator.Current.Data;
                }
            }

            public Boolean Contains(Object value)
            {
                var view = this.parent.data.Values.GetEnumerator();
                if (value is null)
                {
                    while (view.MoveNext())
                    {
                        if (view.Current.Data is null)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    while (view.MoveNext())
                    {
                        var current = view.Current.Data;
                        if (current is not null && value.Equals(current))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public Enumerator GetEnumerator() => new(this);

            public partial struct Enumerator : IEnumerator<Object>
            {
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly ValueCollection parent;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private Dictionary<String, StorageElement>.ValueCollection.Enumerator iterator;

                public Enumerator(ValueCollection parent)
                {
                    this.parent = parent;
                    this.iterator = parent.parent.data.Values.GetEnumerator();
                }

                public readonly Object Current => this.iterator.Current.Data;
                public Boolean MoveNext() => this.iterator.MoveNext();
                public void Reset() => this.iterator = parent.parent.data.Values.GetEnumerator();
                public void Dispose() => this.iterator.Dispose();
                public readonly ValueCollection Parent => this.parent;
            }

            void ICollection<Object>.Add(Object item) => throw new NotSupportedException("This collection is a read only view, use the parent collection to modify the contents.");
            void ICollection<Object>.Clear() => throw new NotSupportedException("This collection is a read only view, use the parent collection to modify the contents.");
            Boolean ICollection<Object>.Remove(Object item) => throw new NotSupportedException("This collection is a read only view, use the parent collection to modify the contents.");
            IEnumerator<Object> IEnumerable<Object>.GetEnumerator() => new Enumerator(this);
            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);
            void ICollection.CopyTo(Array array, Int32 index)
            {
                var data = this.parent.data;
                var count = data.Count;
                if (count == 0) return;
                if (array is null) throw new ArgumentNullException(nameof(array));
                if ((UInt32)index >= (UInt32)array.Length) throw new ArgumentOutOfRangeException(nameof(index));
                if (array.Length - index < count) throw new ArgumentOutOfRangeException(nameof(array));
                if (array.Rank != 1) throw new RankException();
                var iterator = data.Values.GetEnumerator();
                if (array is Object[] objects)
                {
                    while (iterator.MoveNext())
                    {
                        objects[index++] = iterator.Current.Data;
                    }
                }
                else
                {
                    var element = array.GetType().GetElementType();
                    while (iterator.MoveNext())
                    {
                        array.SetValue(Convert.ChangeType(iterator.Current.Data, element, CultureInfo.CurrentCulture), index++);
                    }
                }
            }
            Boolean ICollection<Object>.IsReadOnly => true;
            Object ICollection.SyncRoot => this.parent.root;
            Boolean ICollection.IsSynchronized => false;
        }

        #region >>> private members

        IDictionaryEnumerator IDictionary.GetEnumerator() => new Enumerator(this);
        IEnumerator<KeyValuePair<String, Object>> IEnumerable<KeyValuePair<String, Object>>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);
        Boolean IDictionary.Contains(Object key) => this.Exists(key as String ?? key?.ToString());
        Boolean ICollection<KeyValuePair<String, Object>>.Contains(KeyValuePair<String, Object> item) => this.data.ContainsKey(item.Key ?? "");
        Boolean IReadOnlyDictionary<String, Object>.ContainsKey(String key) => this.data.ContainsKey(key ?? "");
        Boolean IDictionary<String, Object>.ContainsKey(String key) => this.data.ContainsKey(key ?? "");
        void IDictionary.Add(Object key, Object value) => this.Write(key as String ?? key?.ToString(), value);
        void IDictionary.Clear() => this.data.Clear();
        void ICollection<KeyValuePair<String, Object>>.Clear() => this.data.Clear();
        void IDictionary.Remove(Object key) => this.data.Remove(key as String ?? key?.ToString());

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Object IDictionary.this[Object key] { get => this[key as String ?? key?.ToString()]; set => this[key as String ?? key?.ToString()] = value; }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection IDictionary.Keys => this.Names;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection<String> IDictionary<String, Object>.Keys => this.Names;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<String> IReadOnlyDictionary<String, Object>.Keys => this.Names;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection IDictionary.Values => this.Values;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection<Object> IDictionary<String, Object>.Values => this.Values;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<Object> IReadOnlyDictionary<String, Object>.Values => this.Values;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Boolean IDictionary.IsReadOnly => false;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Boolean ICollection<KeyValuePair<String, Object>>.IsReadOnly => false;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Boolean IDictionary.IsFixedSize => false;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Object ICollection.SyncRoot => this.root;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Boolean ICollection.IsSynchronized => false;

        void ICollection.CopyTo(Array array, Int32 index)
        {
            var data = this.data;
            var count = data.Count;
            if (count == 0) return;
            if (array is null) throw new ArgumentNullException(nameof(array));
            if ((UInt32)index >= (UInt32)array.Length) throw new ArgumentOutOfRangeException(nameof(index));
            if (array.Length - index < count) throw new ArgumentOutOfRangeException(nameof(array));
            var iterator = data.GetEnumerator();
            if (array is KeyValuePair<String, Object>[] commons)
            {
                while (iterator.MoveNext())
                {
                    var current = iterator.Current;
                    commons[index++] = new(current.Key, current.Value.Data);
                }
            }
            else if (array is Object[] objects)
            {
                while (iterator.MoveNext())
                {
                    var current = iterator.Current;
                    objects[index++] = current.Value.Data;
                }
            }
            else if (array is String[] strings)
            {
                while (iterator.MoveNext())
                {
                    var current = iterator.Current;
                    strings[index++] = current.Key;
                }
            }
            else
            {
                throw new ArrayTypeMismatchException();
            }

        }
        void ICollection<KeyValuePair<String, Object>>.CopyTo(KeyValuePair<String, Object>[] array, Int32 arrayIndex) => this.Copy(array, arrayIndex);
        Boolean IReadOnlyDictionary<String, Object>.TryGetValue(String key, out Object value) => this.Read(key, out value);
        Boolean IDictionary<String, Object>.TryGetValue(String key, out Object value) => this.Read(key, out value);
        void IDictionary<String, Object>.Add(String key, Object value) => this.Write(key, value);
        void ICollection<KeyValuePair<String, Object>>.Add(KeyValuePair<String, Object> item) => this.Write(item.Key, item.Value);
        Boolean IDictionary<String, Object>.Remove(String key) => this.data.Remove(key ?? "");
        Boolean ICollection<KeyValuePair<String, Object>>.Remove(KeyValuePair<String, Object> item) => this.data.Remove(item.Key ?? "");

        #endregion

        #region >>> Serialization members

        private PersistentStorage(SerializationInfo info, StreamingContext context)
        {
            this.name = info.GetString("name") ?? "Default";
            if (info.GetValue("data", typeof(StorageElement[])) is not StorageElement[] items)
            {
                this.data = new Dictionary<String, StorageElement>(0x10, StringComparer.CurrentCultureIgnoreCase);
            }
            else
            {
                var data = new Dictionary<String, StorageElement>(items.Length, StringComparer.CurrentCultureIgnoreCase);
                for (var i = 0; i < items.Length; i++)
                {
                    var next = items[i];
                    if (next is null) continue;
                    data[next.Name] = next;
                }
                this.data = data;
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(PersistentStorage));
            var data = this.data;
            var count = data.Count;
            info.AddValue("name", this.name, typeof(String));
            if (count != 0)
            {
                var array = new StorageElement[count];
                data.Values.CopyTo(array, 0);
                info.AddValue("data", array, typeof(StorageElement[]));
            }
            else
            {
                info.AddValue("data", Array.Empty<StorageElement>(), typeof(StorageElement[]));
            }
        }

        #endregion
    }
}