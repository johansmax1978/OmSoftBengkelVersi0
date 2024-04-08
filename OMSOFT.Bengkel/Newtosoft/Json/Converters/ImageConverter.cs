#if NETFX && WIN32 && JSON
namespace Newtonsoft.Json.Converters
{
#if DEVEXPRESS
    using DevExpress.Utils.Svg;
#endif
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;

#if DEVEXPRESS
    /// <summary>
    /// Represents the default JSON converter for <see cref="Image"/>, <see cref="Icon"/>, <see cref="SvgImage"/>, and <see cref="SvgBitmap"/> objects.
    /// </summary>
#else
    /// <summary>
    /// Represents the default JSON converter for <see cref="Image"/> or <see cref="Icon"/> classes.
    /// </summary>
#endif
    public partial class ImageConverter : JsonConverter
    {
        private static partial class Internals
        {
            [ThreadStatic]
            private static MemoryStream _SharedStream;

            public static MemoryStream SharedStream
            {
                get
                {
                    var value = _SharedStream;
                    if (value is null)
                    {
                        _SharedStream = value = new MemoryStream(1024 * 1024);
                    }
                    else if (value.Length != 0)
                    {
                        value.SetLength(0L);
                    }
                    return value;
                }
            }
        }

        private enum ImageModel
        {
            Invalid,
            Null,
            Image,
            Icon,
#if DEVEXPRESS
            SvgImage,
            SvgBitmap
#endif
        }

        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ImageModel GetImageModel(Type type)
            => type is null
                ? ImageModel.Null
                : typeof(Image).IsAssignableFrom(type)
                    ? ImageModel.Image
                    : type == typeof(Icon)
                        ? ImageModel.Icon
#if DEVEXPRESS
                        : typeof(SvgImage).IsAssignableFrom(type)
                            ? ImageModel.SvgImage
                            : typeof(SvgBitmap).IsAssignableFrom(type) ? ImageModel.SvgBitmap : ImageModel.Invalid;
#else
                : ImageModel.Invalid;
#endif

        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ImageModel GetImageModel(Object value)
            => GetImageModel(value?.GetType());

        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ImageFormat GetImageFormat(Image image)
            => image.RawFormat is ImageFormat format && format.Guid != Guid.Empty
                ? format
                    : image is Metafile
                        ? ImageFormat.Wmf
                        : image.PixelFormat switch
                        {
                            PixelFormat.Format16bppArgb1555 or PixelFormat.Format32bppArgb or PixelFormat.Format32bppPArgb or PixelFormat.Format64bppArgb or PixelFormat.Format64bppPArgb or PixelFormat.Alpha or PixelFormat.PAlpha => ImageFormat.Png,
                            _ => ImageFormat.Jpeg,
                        };

        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static String GetMediaType(Object image)
            => image is Image img ? GetMediaType(GetImageFormat(img))
#if DEVEXPRESS
             : image is SvgImage || image is SvgBitmap ? "image/svg+xml"
#endif
             : image is null ? "null" : null;

        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static String GetMediaType(Image image)
            => GetMediaType(GetImageFormat(image));

        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static String GetMediaType(ImageFormat format)
            => format is null || format.Guid == Guid.Empty || format.Guid == ImageFormat.Jpeg.Guid
                ? "image/jpeg"
                : format.Guid == ImageFormat.Bmp.Guid || format.Guid == ImageFormat.MemoryBmp.Guid
                ? "image/bmp"
                : format.Guid == ImageFormat.Emf.Guid
                ? "image/emf"
                : format.Guid == ImageFormat.Exif.Guid
                ? "image/exif"
                : format.Guid == ImageFormat.Gif.Guid
                ? "image/gif"
                : format.Guid == ImageFormat.Icon.Guid
                ? "image/vnd.microsoft.icon"
                : format.Guid == ImageFormat.Png.Guid
                ? "image/png"
                : format.Guid == ImageFormat.Tiff.Guid ? "image/tiff" : format.Guid == ImageFormat.Wmf.Guid ? "image/wmf" : "image/jpeg";

        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Boolean GetMediaType(String mime, out ImageModel model, out ImageFormat format)
        {
            if (mime is null || mime.Length == 0 || (mime = mime.Trim()).Length == 0)
            {
                model = ImageModel.Null;
                format = null;
                return true;
            }
            else
            {
                switch (mime.ToLower())
                {
                    case "image/jpeg":
                    case "image/jpg":
                    case "jpeg":
                    case "jpg":
                    case ".jpg":
                    case ".jpeg":
                        format = ImageFormat.Jpeg;
                        model = ImageModel.Image;
                        return true;
                    case "image/png":
                    case "png":
                    case ".png":
                        format = ImageFormat.Png;
                        model = ImageModel.Image;
                        return true;
                    case "image/bmp":
                    case "bmp":
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        model = ImageModel.Image;
                        return true;
                    case "image/gif":
                    case "gif":
                    case ".gif":
                        format = ImageFormat.Gif;
                        model = ImageModel.Image;
                        return true;
                    case "image/x-icon":
                    case "image/vnd.microsoft.icon":
                    case "ico":
                    case "icon":
                    case ".ico":
                    case ".icon":
                        format = ImageFormat.Icon;
                        model = ImageModel.Icon;
                        return true;
                    case "image/emf":
                    case "emf":
                    case ".emf":
                        format = ImageFormat.Emf;
                        model = ImageModel.Image;
                        return true;
                    case "image/wmf":
                    case "wmf":
                    case ".wmf":
                        format = ImageFormat.Wmf;
                        model = ImageModel.Image;
                        return true;
                    case "image/tiff":
                    case "tiff":
                    case ".tiff":
                        format = ImageFormat.Tiff;
                        model = ImageModel.Image;
                        return true;
                    case "image/exif":
                    case "exif":
                    case ".exif":
                        format = ImageFormat.Exif;
                        model = ImageModel.Image;
                        return true;
#if DEVEXPRESS
                    case "image/svg":
                    case "image/svg+xml":
                    case "svg":
                    case ".svg":
                    case "svgimage":
                    case "svgi":
                        format = null;
                        model = ImageModel.SvgImage;
                        return true;
                    case "svgb":
                    case "svgbitmap":
                    case "svg+":
                    case "image/svg+xml.bmp":
                    case "image/svg+xml.bitmap":
                        format = null;
                        model = ImageModel.SvgBitmap;
                        return true;
#endif
                    default:
                        format = null;
                        model = ImageModel.Invalid;
                        return false;
                }
            }
        }

        private static void TryWrite(Image image, JsonWriter writer)
        {
            writer.WriteStartObject();
            {
                var format = GetImageFormat(image);
                writer.WritePropertyName("format");
                writer.WriteValue(GetMediaType(format));
                var stream = Internals.SharedStream;
                image.Save(stream, format);

                writer.WritePropertyName("length");
                writer.WriteValue(stream.Length);

                writer.WritePropertyName("width");
                writer.WriteValue(image.Width);

                writer.WritePropertyName("height");
                writer.WriteValue(image.Height);

                writer.WritePropertyName("content");
                if (writer is JsonTextWriter jtw)
                    jtw.WriteValue(Convert.ToBase64String(stream.GetBuffer(), 0, (Int32)stream.Length, Base64FormattingOptions.None));
                else
                    writer.WriteValue(stream.ToArray());
                stream.SetLength(0L);
            }
            writer.WriteEndObject();
        }

        private static void WriteIcon(Icon icon, JsonWriter writer)
        {
            writer.WriteStartObject();
            {
                writer.WritePropertyName("format");
                writer.WriteValue("image/vnd.microsoft.icon");
                var stream = Internals.SharedStream;
                icon.Save(stream);

                writer.WritePropertyName("length");
                writer.WriteValue(stream.Length);

                writer.WritePropertyName("width");
                writer.WriteValue(icon.Width);

                writer.WritePropertyName("height");
                writer.WriteValue(icon.Height);

                writer.WritePropertyName("content");
                if (writer is JsonTextWriter jtw)
                    jtw.WriteValue(Convert.ToBase64String(stream.GetBuffer(), 0, (Int32)stream.Length, Base64FormattingOptions.None));
                else
                    writer.WriteValue(stream.ToArray());
                stream.SetLength(0L);
            }
            writer.WriteEndObject();
        }


#if DEVEXPRESS
        private static void WriteSvgImage(SvgImage svgimg, JsonWriter writer)
        {
            writer.WriteStartObject();
            {
                writer.WritePropertyName("format");
                writer.WriteValue("image/svg+xml");
                var stream = Internals.SharedStream;
                svgimg.Save(stream);

                writer.WritePropertyName("length");
                writer.WriteValue(stream.Length);

                writer.WritePropertyName("width");
                writer.WriteValue(svgimg.Width);

                writer.WritePropertyName("height");
                writer.WriteValue(svgimg.Height);

                writer.WritePropertyName("content");
                if (writer is JsonTextWriter jtw)
                    jtw.WriteValue(Convert.ToBase64String(stream.GetBuffer(), 0, (Int32)stream.Length, Base64FormattingOptions.None));
                else
                    writer.WriteValue(stream.ToArray());
                stream.SetLength(0L);
            }
            writer.WriteEndObject();
        }

        private static void WriteSvgBitmap(SvgBitmap svgbmp, JsonWriter writer)
        {
            var svgimg = svgbmp.SvgImage;
            if (svgimg is null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteStartObject();
                {
                    writer.WritePropertyName("format");
                    writer.WriteValue("image/svg+xml.bmp");
                    var stream = Internals.SharedStream;
                    svgimg.Save(stream);

                    writer.WritePropertyName("scale");
                    writer.WriteValue(svgbmp.Scale);

                    writer.WritePropertyName("length");
                    writer.WriteValue(stream.Length);

                    writer.WritePropertyName("width");
                    writer.WriteValue(svgimg.Width);

                    writer.WritePropertyName("height");
                    writer.WriteValue(svgimg.Height);

                    writer.WritePropertyName("content");
                    if (writer is JsonTextWriter jtw)
                        jtw.WriteValue(Convert.ToBase64String(stream.GetBuffer(), 0, (Int32)stream.Length, Base64FormattingOptions.None));
                    else
                        writer.WriteValue(stream.ToArray());
                    stream.SetLength(0L);
                }
                writer.WriteEndObject();
            }
        }
#endif

        private static unsafe Boolean ReadImageFrom(String path, Byte[] bytes, ref ImageModel model, out Object image, out String error)
        {
            var length = bytes is null ? 0 : bytes.Length;
            if (length != 0)
            {
                fixed (Byte* buffer = bytes)
                {
                    using var stream = new UnmanagedMemoryStream(buffer, length, length, FileAccess.Read);
                    if (model != ImageModel.Invalid)
                    {
                        try
                        {
                            switch (model)
                            {
                                case ImageModel.Image:
                                    image = Image.FromStream(stream);
                                    error = null;
                                    return true;
                                case ImageModel.Icon:
                                    image = new Icon(stream);
                                    error = null;
                                    return true;
#if DEVEXPRESS
                                case ImageModel.SvgImage:
                                    image = SvgImage.FromStream(stream);
                                    error = null;
                                    return true;
                                default:
                                    image = SvgBitmap.FromStream(stream);
                                    error = null;
                                    return true;
#else
                                default:
                                    image = null;
                                    error = $"[{path}] Unsupported image model: {model}";
                                    return true;
#endif
                            }
                        }
                        catch (Exception exception)
                        {
                            error = $"[{path}] Failed to load {model}: ({exception.GetType().Name}) {exception.Message}";
                            image = null;
                            return false;
                        }
                    }
                    else
                    {
                        try
                        {
                            image = Image.FromStream(stream);
                            error = null;
                            model = ImageModel.Image;
                            return true;
                        }
                        catch (Exception error1)
                        {
                            try
                            {
                                stream.Seek(0, SeekOrigin.Begin);
                                image = new Icon(stream);
                                error = null;
                                model = ImageModel.Icon;
                                return true;
                            }
                            catch (Exception error2)
                            {
#if DEVEXPRESS
                                try
                                {
                                    stream.Seek(0, SeekOrigin.Begin);
                                    var svgb = SvgBitmap.FromStream(stream);
                                    if (svgb is null || svgb.SvgImage is null)
                                        throw new ArgumentException($"Invalid SVG bitmap data to load ({stream.Length} bytes).", "value");
                                    image = svgb;
                                    error = null;
                                    model = ImageModel.SvgBitmap;
                                    return true;
                                }
                                catch (Exception error3)
                                {
                                    try
                                    {
                                        stream.Seek(0, SeekOrigin.Begin);
                                        image = SvgImage.FromStream(stream);
                                        error = null;
                                        model = ImageModel.SvgImage;
                                        return true;
                                    }
                                    catch (Exception error4)
                                    {
                                        image = null;
                                        error = $"[{path}] Failed to load Image: ({error1.GetType().Name}) {error1.Message}" + Environment.NewLine +
                                                $"[{path}] Failed to load Icon: ({error2.GetType().Name}) {error2.Message}" + Environment.NewLine +
                                                $"[{path}] Failed to load SvgBitmap: ({error3.GetType().Name}) {error3.Message}" + Environment.NewLine +
                                                $"[{path}] Failed to load SvgImage: ({error4.GetType().Name}) {error4.Message}";
                                        model = ImageModel.Invalid;
                                        return false;
                                    }
                                }
#else
                                image = null;
                                error = $"[{path}] Failed to load Image: ({error1.GetType().Name}) {error1.Message}" + Environment.NewLine +
                                        $"[{path}] Failed to load Icon: ({error2.GetType().Name}) {error2.Message}";
                                model = ImageModel.Invalid;
                                return false;
#endif
                            }
                        }
                    }
                }
            }
            image = null;
            error = null;
            return true;
        }

#if DEVEXPRESS
        /// <summary>
        /// Try to write the given image object into the JSON output using the given <see cref="JsonWriter"/> instance.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> instance that should be used to write the given <paramref name="image"/> object as JSON.</param>
        /// <param name="image">The appropriate image object to write as JSON, can be one of the following classes:<br/>
        /// - <see cref="Bitmap"/> class (inherited from <see cref="Image"/> class);<br/>
        /// - <see cref="Metafile"/> class (inherited from <see cref="Image"/> class);<br/>
        /// - <see cref="Icon"/> class (independent icon class);<br/>
        /// - <see cref="SvgImage"/> class (from DevExpress library);<br/>
        /// - <see cref="SvgBitmap"/> class (from DevExpress library);<br/>
        /// </param>
        /// <returns><see langword="true"/> when writing the given <paramref name="image"/> as JSON are suceeds; otherwise, <see langword="false"/> on failed.</returns>
#else
        /// <summary>
        /// Try to write the given image object into the JSON output using the given <see cref="JsonWriter"/> instance.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> instance that should be used to write the given <paramref name="image"/> object as JSON.</param>
        /// <param name="image">The appropriate image object to write as JSON, can be one of the following classes:<br/>
        /// - <see cref="Bitmap"/> class (inherited from <see cref="Image"/> class);<br/>
        /// - <see cref="Metafile"/> class (inherited from <see cref="Image"/> class);<br/>
        /// - <see cref="Icon"/> class (independent icon class);
        /// </param>
        /// <returns><see langword="true"/> when writing the given <paramref name="image"/> as JSON are suceeds; otherwise, <see langword="false"/> on failed.</returns>
#endif
        public static Boolean TryWrite(JsonWriter writer, Object image)
            => TryWrite(writer, image, out _);

#if DEVEXPRESS
        /// <summary>
        /// Try to write the given image object into the JSON output using the given <see cref="JsonWriter"/> instance.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> instance that should be used to write the given <paramref name="image"/> object as JSON.</param>
        /// <param name="image">The appropriate image object to write as JSON, can be one of the following classes:<br/>
        /// - <see cref="Bitmap"/> class (inherited from <see cref="Image"/> class);<br/>
        /// - <see cref="Metafile"/> class (inherited from <see cref="Image"/> class);<br/>
        /// - <see cref="Icon"/> class (independent icon class);<br/>
        /// - <see cref="SvgImage"/> class (from DevExpress library);<br/>
        /// - <see cref="SvgBitmap"/> class (from DevExpress library);<br/>
        /// </param>
        /// <param name="error">Retrieve a <see cref="String"/> as the detailed error message when failed; otherwise, <see langword="null"/> on succeeds.</param>
        /// <returns><see langword="true"/> when writing the given <paramref name="image"/> as JSON are suceeds; otherwise, <see langword="false"/> on failed.</returns>
#else
        /// <summary>
        /// Try to write the given image object into the JSON output using the given <see cref="JsonWriter"/> instance.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> instance that should be used to write the given <paramref name="image"/> object as JSON.</param>
        /// <param name="image">The appropriate image object to write as JSON, can be one of the following classes:<br/>
        /// - <see cref="Bitmap"/> class (inherited from <see cref="Image"/> class);<br/>
        /// - <see cref="Metafile"/> class (inherited from <see cref="Image"/> class);<br/>
        /// - <see cref="Icon"/> class (independent icon class);
        /// </param>
        /// <param name="error">Retrieve a <see cref="String"/> as the detailed error message when failed; otherwise, <see langword="null"/> on succeeds.</param>
        /// <returns><see langword="true"/> when writing the given <paramref name="image"/> as JSON are suceeds; otherwise, <see langword="false"/> on failed.</returns>
#endif
        public static Boolean TryWrite(JsonWriter writer, Object image, out String error)
        {
            if (writer is not null)
            {
                if (image is null)
                {
                    writer.WriteNull();
                    error = null;
                    return true;
                }
                else if (image is Image img)
                {
                    TryWrite(img, writer);
                    error = null;
                    return true;
                }
                else if (image is Icon icon)
                {
                    WriteIcon(icon, writer);
                    error = null;
                    return true;
                }
#if DEVEXPRESS
                else if (image is SvgImage svgi)
                {
                    WriteSvgImage(svgi, writer);
                    error = null;
                    return true;
                }
                else if (image is SvgBitmap svgb)
                {
                    WriteSvgBitmap(svgb, writer);
                    error = null;
                    return true;
                }
#endif
                else
                {
#if DEVEXPRESS
                    error = $"Unsupported image type ({image.GetType()}) to write into the underlying JSON stream. The list of supported image types are \"Image\", \"Icon\", \"SvgImage\", and \"SvgBitmap\" classes.";
#else
                    error = $"Unsupported image type ({image.GetType()}) to write into the underlying JSON stream. The list of supported image types are \"Image\", \"Icon\" classes.";
#endif
                    return false;
                }
            }
            error = "The JSON writer to be used to write the image must not be null.";
            return false;
        }

#if DEVEXPRESS
        /// <summary>
        /// Try to parse the image from JSON that being readed by the given <see cref="JsonReader"/> into the associated image class.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read the image data. This method is accepting string, bytes, object, or null tokens.</param>
        /// <param name="image">Retrieve the appropriate image object instance, that is could be one of the following image classes:<br/>
        /// - <see cref="Bitmap"/> class (inherited from <see cref="Image"/> class);<br/>
        /// - <see cref="Metafile"/> class (inherited from <see cref="Image"/> class);<br/>
        /// - <see cref="Icon"/> class (independent icon object);<br/>
        /// - <see cref="SvgImage"/> class (from DevExpress library);<br/>
        /// - <see cref="SvgBitmap"/> class (from DevExpress library);<br/></param>
        /// <returns><see langword="true"/> if loading image from the readed JSON using the given <see cref="JsonReader"/> are succeeds; otherwise, <see langword="false"/> on failed.</returns>
#else
        /// <summary>
        /// Try to parse the image from JSON that being readed by the given <see cref="JsonReader"/> into the associated image class.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read the image data. This method is accepting string, bytes, object, or null tokens.</param>
        /// <param name="image">Retrieve the appropriate image object instance, that is could be one of the following image classes:<br/>
        /// - <see cref="Bitmap"/> class (inherited from <see cref="Image"/> class);<br/>
        /// - <see cref="Metafile"/> class (inherited from <see cref="Image"/> class);<br/>
        /// - <see cref="Icon"/> class (independent icon object);<br/></param>
        /// <returns><see langword="true"/> if loading image from the readed JSON using the given <see cref="JsonReader"/> are succeeds; otherwise, <see langword="false"/> on failed.</returns>
#endif
        public static Boolean TryRead(JsonReader reader, out Object image)
            => TryRead(reader, out image, out _);

#if DEVEXPRESS
        /// <summary>
        /// Try to parse the image from JSON that being readed by the given <see cref="JsonReader"/> into the associated image class.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read the image data. This method is accepting string, bytes, object, or null tokens.</param>
        /// <param name="image">Retrieve the appropriate image object instance, that is could be one of the following image classes:<br/>
        /// - <see cref="Bitmap"/> class (inherited from <see cref="Image"/> class);<br/>
        /// - <see cref="Metafile"/> class (inherited from <see cref="Image"/> class);<br/>
        /// - <see cref="Icon"/> class (independent icon object);<br/>
        /// - <see cref="SvgImage"/> class (from DevExpress library);<br/>
        /// - <see cref="SvgBitmap"/> class (from DevExpress library);<br/></param>
        /// <param name="error">When failed, retrieve the information about the occured parsing error, or retrieve <see langword="null"/> when succeeds.</param>
        /// <returns><see langword="true"/> if loading image from the readed JSON using the given <see cref="JsonReader"/> are succeeds; otherwise, <see langword="false"/> on failed.</returns>
#else
        /// <summary>
        /// Try to parse the image from JSON that being readed by the given <see cref="JsonReader"/> into the associated image class.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read the image data. This method is accepting string, bytes, object, or null tokens.</param>
        /// <param name="image">Retrieve the appropriate image object instance, that is could be one of the following image classes:<br/>
        /// - <see cref="Bitmap"/> class (inherited from <see cref="Image"/> class);<br/>
        /// - <see cref="Metafile"/> class (inherited from <see cref="Image"/> class);<br/>
        /// - <see cref="Icon"/> class (independent icon object);<br/></param>
        /// <param name="error">When failed, retrieve the information about the occured parsing error, or retrieve <see langword="null"/> when succeeds.</param>
        /// <returns><see langword="true"/> if loading image from the readed JSON using the given <see cref="JsonReader"/> are succeeds; otherwise, <see langword="false"/> on failed.</returns>
#endif
        public static Boolean TryRead(JsonReader reader, out Object image, out String error)
        {
            if (reader is not null)
            {
                var path = reader.Path;
                if (reader.TokenType == JsonToken.Null)
                {
                    image = null;
                    error = null;
                    return true;
                }
                else if (reader.TokenType == JsonToken.StartObject)
                {
                    var model = ImageModel.Invalid;
                    ImageFormat format = null;
                    Object content = null;
                    while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                    {
                        if (reader.TokenType == JsonToken.PropertyName)
                        {
                            var property = Convert.ToString(reader.Value, CultureInfo.CurrentCulture);
                            if (reader.Read() && property is not null && property.Length != 0)
                            {
                                var value = reader.Value;
                                if (value is not null)
                                {
                                    switch (property.ToLower())
                                    {
                                        case "format":
                                            _ = GetMediaType(Convert.ToString(value, CultureInfo.CurrentCulture), out model, out format);
                                            break;
                                        case "content":
                                        case "image":
                                        case "data":
                                        case "bytes":
                                            if (reader.TokenType == JsonToken.Bytes)
                                            {
                                                content = (Byte[])Convert.ChangeType(value, typeof(Byte[]), CultureInfo.CurrentCulture);
                                            }
                                            else if (reader.TokenType == JsonToken.String)
                                            {
                                                content = Convert.ToString(value, CultureInfo.CurrentCulture);
                                            }
                                            else if (reader.TokenType == JsonToken.Null)
                                            {
                                                content = null;
                                                model = ImageModel.Null;
                                            }
                                            break;
                                        default:
                                            reader.Skip();
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    if (model == ImageModel.Invalid)
                    {
                        if (content is null)
                        {
                            error = $"The current readed JSON object ({path}) is not represents the valid image object.";
                            image = null;
                            return false;
                        }
                        if (content is String base64)
                        {
                            try
                            {
                                content = Convert.FromBase64String(base64);
                            }
                            catch (Exception exception)
                            {
                                error = $"The content in the loaded JSON object ({path}) is not represents the valid BASE-64 string to load as image. Error: ({exception.GetType().Name}) {exception.Message};";
                                image = null;
                                return false;
                            }
                        }
                        return ReadImageFrom(path, (Byte[])content, ref model, out image, out error);
                    }
                    else if (model == ImageModel.Null)
                    {
                        image = null;
                        error = null;
                        return false;
                    }
                    else
                    {
                        if (content is String base64)
                        {
                            try
                            {
                                content = Convert.FromBase64String(base64);
                            }
                            catch (Exception exception)
                            {
                                error = $"The content in the loaded JSON object ({path}) is not represents the valid BASE-64 string to load as image. Error: ({exception.GetType().Name}) {exception.Message};";
                                image = null;
                                return false;
                            }
                        }
                        unsafe
                        {
                            var buffer = (Byte[])content;
                            var length = buffer is null ? 0 : buffer.Length;
                            if (length != 0)
                            {
                                fixed (Byte* bytes = buffer)
                                {
                                    using var stream = new UnmanagedMemoryStream(bytes, length, length, FileAccess.Read);
                                    try
                                    {
                                        switch (model)
                                        {
                                            case ImageModel.Image:
                                                image = Image.FromStream(stream);
                                                error = null;
                                                return true;
                                            case ImageModel.Icon:
                                                image = new Icon(stream);
                                                error = null;
                                                return true;
#if DEVEXPRESS
                                            case ImageModel.SvgImage:
                                                image = SvgImage.FromStream(stream);
                                                error = null;
                                                return true;
                                            default:
                                                image = SvgBitmap.FromStream(stream);
                                                error = null;
                                                return true;
#else
                                            default:
                                                image = null;
                                                error = "Unsupported image model: " + model.ToString();
                                                return true;
#endif
                                        }
                                    }
                                    catch (Exception exception)
                                    {
                                        image = null;
                                        error = $"Failed to load {model} from JSON value at following path: {path}. Error: ({exception.GetType().Name}) {exception.Message};";
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                image = null;
                                error = null;
                                return true;
                            }
                        }
                    }
                }
                else if (reader.TokenType == JsonToken.Bytes || reader.TokenType == JsonToken.String)
                {
                    var model = ImageModel.Invalid;
                    var value = reader.Value;
                    if (value is Byte[] bytes)
                    {
                        return ReadImageFrom(path, bytes, ref model, out image, out error);
                    }
                    else if (value is String base64)
                    {
                        try
                        {
                            bytes = Convert.FromBase64String(base64);
                            return ReadImageFrom(path, bytes, ref model, out image, out error);
                        }
                        catch (Exception exception)
                        {
                            error = $"[{path}] The readed JSON value is not represents the valid BASE-64 string. Error: ({exception.GetType().Name}) {exception.Message};";
                            image = null;
                            return false;
                        }
                    }
                    else if (reader.TokenType == JsonToken.String)
                    {
                        try
                        {
                            base64 = Convert.ToString(value, CultureInfo.CurrentCulture);
                            bytes = Convert.FromBase64String(base64);
                            return ReadImageFrom(path, bytes, ref model, out image, out error);
                        }
                        catch (Exception exception)
                        {
                            error = $"[{path}] The readed JSON value is not represents the valid BASE-64 string. Error: ({exception.GetType().Name}) {exception.Message};";
                            image = null;
                            return false;
                        }
                    }
                    else
                    {
                        try
                        {
                            bytes = (Byte[])Convert.ChangeType(value, typeof(Byte[]), CultureInfo.CurrentCulture);
                            return ReadImageFrom(path, bytes, ref model, out image, out error);
                        }
                        catch (Exception exception)
                        {
                            error = $"[{path}] The readed JSON value is not represents the valid bytes data. Error: ({exception.GetType().Name}) {exception.Message};";
                            image = null;
                            return false;
                        }
                    }
                }
                error = $"The JSON token ({reader.TokenType}) is not suite to load the image, supported token for image are \"Null\", \"StartObject\", \"Bytes\", or \"String\".";
                image = null;
                return false;
            }
            image = null;
            error = "The JSON reader to be used to read the image must not null.";
            return false;
        }

        /// <summary>
        /// Represents the default singleton instance of the <see cref="ImageConverter"/> class, this field is <see langword="readonly"/>.
        /// </summary>
        public static readonly ImageConverter Default = new ImageConverter();

        /// <summary>
        /// Initializes a new default instance of the <see cref="ImageConverter"/> class.
        /// </summary>
        public ImageConverter() { }

        /// <inheritdoc/>
        public override Boolean CanConvert(Type objectType) => GetImageModel(objectType) != ImageModel.Invalid;

        /// <inheritdoc/>
        public override Boolean CanRead => true;

        /// <inheritdoc/>
        public override Boolean CanWrite => false;

        /// <inheritdoc/>
        /// <exception cref="JsonException">Thrown when failed to load the image.</exception>
        public override Object ReadJson(JsonReader reader, Type objectType, Object existingValue, JsonSerializer serializer)
        {
            var path = reader.Path;
            var model = GetImageModel(objectType);
            if (model == ImageModel.Invalid)
                return existingValue;
            else if (TryRead(reader, out var image, out var error))
            {
                if (image is null || objectType.IsAssignableFrom(image.GetType()))
                    return image;
                else
                {
                    var source = GetImageModel(image);
                    switch (model)
                    {
                        case ImageModel.Image:
                            switch (source)
                            {
                                case ImageModel.Image:
                                    if (objectType == typeof(Bitmap))
                                    {
                                        return new Bitmap((Image)image);
                                    }
                                    else if (objectType == typeof(Metafile))
                                    {
                                        if (image is IDisposable disposable)
                                            disposable.Dispose();
                                        throw new InvalidCastException($"[{path}] Cannot convert Bitmap to Metafile directly.");
                                    }
                                    else
                                    {
                                        return image;
                                    }
                                case ImageModel.Icon:
                                    return ((Icon)image).ToBitmap();
#if DEVEXPRESS
                                case ImageModel.SvgBitmap:
                                    return ((SvgBitmap)image).Render(null, 1.0);
                                default:
                                    return new SvgBitmap((SvgImage)image).Render(null, 1.0);
#else
                                default:
                                    return null;
#endif
                            }
                        case ImageModel.Icon:
                            switch (source)
                            {
                                case ImageModel.Image:
                                    if (image is Metafile meta)
                                    {
                                        using (var bitmap = new Bitmap(meta))
                                            return Icon.FromHandle(bitmap.GetHicon());
                                    }
                                    else
                                    {
                                        var bitmap = (Bitmap)image;
                                        var icon = Icon.FromHandle(bitmap.GetHicon());
                                        bitmap.Dispose();
                                        return icon;
                                    }
                                case ImageModel.Icon:
                                    return image;
#if DEVEXPRESS
                                case ImageModel.SvgBitmap:
                                    using (var svgb = ((SvgBitmap)image).Render(null, 1.0))
                                        return Icon.FromHandle(((Bitmap)svgb).GetHicon());
                                default:
                                    using (var svgb = new SvgBitmap(((SvgImage)image)).Render(null, 1.0))
                                        return Icon.FromHandle(((Bitmap)svgb).GetHicon());
#else
                                default:
                                    return null;
#endif

                            }
#if DEVEXPRESS
                        case ImageModel.SvgImage:
                            if (source == ImageModel.SvgBitmap)
                            {
                                return ((SvgBitmap)image).SvgImage;
                            }
                            else
                            {
                                if (image is IDisposable disposable) disposable.Dispose();
                                throw new InvalidCastException($"[{path}] Cannot convert {source} to {model} directly.");
                            }
                        case ImageModel.SvgBitmap:
                            if (source == ImageModel.SvgImage)
                            {
                                return new SvgBitmap((SvgImage)image);
                            }
                            else
                            {
                                if (image is IDisposable disposable) disposable.Dispose();
                                throw new InvalidCastException($"[{path}] Cannot convert {source} to {model} directly.");
                            }
#endif
                        case ImageModel.Null:
                            if (image is IDisposable) ((IDisposable)image).Dispose();
                            return null;
                        default:
                            return existingValue;
                    }
                }
            }
            else
            {
                throw new JsonException(error);
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer)
            => _ = TryWrite(writer, value, out _);
    }
}
#endif