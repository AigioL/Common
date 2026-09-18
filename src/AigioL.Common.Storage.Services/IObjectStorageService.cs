using ImageMagick;
using Microsoft.IO;
using System.Buffers;

namespace AigioL.Common.Storage.Services;

/// <summary>
/// 对象存储服务接口
/// </summary>
public partial interface IObjectStorageService
{
    const string HttpClientName = "ObjectStorageServiceHttpClient";
    const MagickFormat DefaultSetImageFormat = MagickFormat.WebP;
    const uint DefaultSetImageQuality = 75;

    ///// <summary>
    ///// 获取临时下载链接
    ///// </summary>
    ///// <param name="key">对象存储键</param>
    ///// <param name="expiration">有效期</param>
    ///// <returns></returns>
    //string? GetTempDownloadUrl(string key, TimeSpan expiration);

    /// <summary>
    /// 获取 CDN 临时下载链接
    /// </summary>
    /// <param name="resourceAccessPath">资源访问路径</param>
    /// <param name="expiration">有效期</param>
    /// <returns></returns>
    string GetCdnTempDownloadUrl(string resourceAccessPath, TimeSpan expiration);

    /// <summary>
    /// 读取数据流到内存流中，并尝试将其转换为指定格式的图片，如果不是图片则直接返回读取的内存流
    /// </summary>
    /// <param name="stream">原始数据流，比如是请求的 Body</param>
    /// <param name="useOriginal">当数据为图片时，使用原图，不进行默认的图片处理</param>
    /// <param name="resizeWidth">当数据为图片时，且需要调整大小的宽度</param>
    /// <param name="resizeHeight">当数据为图片时，且需要调整大小的高度</param>
    /// <param name="resizeFilter">当数据为图片时，且需要调整大小的 <see cref="FilterType"/></param>
    /// <param name="setImageFormat">当数据为图片时，需要更改的格式，默认值为 <see cref="MagickFormat.WebP"/></param>
    /// <param name="quality">当数据为图片时，设置图片质量，默认值为 75</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>返回一个元组，包含结果内存流和图片格式（如果是图片的话）</returns>
    static async Task<(Stream resultStream, MagickFormat? imageFormat)> ReadToStreamAsync(
        Stream stream,
        bool useOriginal = false,
        uint? resizeWidth = null,
        uint? resizeHeight = null,
        FilterType? resizeFilter = null,
        MagickFormat setImageFormat = DefaultSetImageFormat,
        uint quality = DefaultSetImageQuality,
        CancellationToken cancellationToken = default)
    {
        var readStream = await ReadOnlyMemoryStream2.ReadToStreamAsync(stream, cancellationToken); // 先读取到内存流中，避免传入的流不支持设置读取位置

        Stream? resultStream = null;
        try
        {
            MagickFormat? imageFormat = null;
            try
            {
                // 先尝试创建 MagickImage，如果流中的数据是图片则不会抛出异常
                using MagickImage magickImage = ReadOnlyMemoryStream2.ToMagickImage(readStream); // MagickImage 内部直接读取流中所有数据，所以释放不会影响原流
                try
                {
                    imageFormat = magickImage.Format;
                    if (!useOriginal)
                    {
                        magickImage.Format = setImageFormat;
                        if (resizeWidth.HasValue && resizeHeight.HasValue)
                        {
                            if (resizeFilter.HasValue)
                            {
                                magickImage.Resize(resizeWidth.Value, resizeHeight.Value, resizeFilter.Value);
                            }
                            else
                            {
                                magickImage.Resize(resizeWidth.Value, resizeHeight.Value);
                            }
                        }
                        magickImage.Quality = quality; // 设置图片质量
                        resultStream = ReadOnlyMemoryStream2.GetRecyclableMemoryStream();
                        await magickImage.WriteAsync(resultStream, cancellationToken);
                        resultStream.Position = 0;
                    }
                    else
                    {
                        resultStream = readStream;
                        resultStream.Position = 0;
                    }
                }
                finally
                {
                    if (resultStream != readStream)
                    {
                        await readStream.DisposeAsync();
                    }
                }
            }
            catch (MagickException)
            {
                readStream.Position = 0;
                resultStream = readStream;
            }
            return (resultStream, imageFormat);
        }
        catch (Exception)
        {
            if (resultStream != null)
            {
                await resultStream.DisposeAsync();
            }
            throw;
        }
    }
}

/// <summary>
/// 使用池化数组与 <see cref="RecyclableMemoryStreamManager"/> 优化的只读内存流
/// </summary>
file sealed class ReadOnlyMemoryStream2 : MemoryStream
{
    readonly static global::Microsoft.IO.RecyclableMemoryStreamManager m = new();

    byte[] bytes;
    readonly int index, count;

    private ReadOnlyMemoryStream2(byte[] bytes, int index, int count) : base(bytes, index, count, false)
    {
        this.bytes = bytes;
        this.index = index;
        this.count = count;
    }

    /// <summary>
    /// 获取一个 <see cref="RecyclableMemoryStream"/>
    /// </summary>
    public static RecyclableMemoryStream GetRecyclableMemoryStream() => m.GetStream();

    /// <summary>
    /// 读取流，如果流长度小于 <see cref="int.MaxValue"/>，则使用池化数组，否则使用 <see cref="RecyclableMemoryStream"/>
    /// </summary>
    public static async Task<Stream> ReadToStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        // 👇 当流为以下派生类时直接返回，避免重复读取当流为以下派生类时直接返回
        if (stream is MemoryStream ms)
        {
            return ms;
        }
        else if (stream is ReadOnlyMemoryStream2 rms2)
        {
            return rms2;
        }
        else if (stream is RecyclableMemoryStream rms)
        {
            return rms;
        }
        else if (stream is UnmanagedMemoryStream ums)
        {
            return ums;
        }

        try // 传入的原始流可能不支持读取长度
        {
            var len = stream.Length;
            if (len < int.MaxValue)
            {
                var len32 = unchecked((int)len);
                var buffer = ArrayPool<byte>.Shared.Rent(len32);
                await stream.ReadExactlyAsync(buffer.AsMemory(0, len32), cancellationToken);
                return new ReadOnlyMemoryStream2(buffer, 0, len32);
            }
        }
        catch
        {
        }
        var recyclableMemoryStream = m.GetStream();
        await stream.CopyToAsync(recyclableMemoryStream, cancellationToken);
        return recyclableMemoryStream;
    }

    /// <summary>
    /// 从流中创建 <see cref="MagickImage"/>，如果流中的数据不是图片，则会抛出异常 <see cref="MagickException"/>
    /// </summary>
    public static MagickImage ToMagickImage(Stream stream)
    {
        if (stream is ReadOnlyMemoryStream2 stream2)
        {
            // index 和 count 已由 base 构造函数使用 ThrowIfNegative 验证，不会为负数
            return new MagickImage(stream2.bytes, unchecked((uint)stream2.index), unchecked((uint)stream2.count));
        }
        return new MagickImage(stream);
    }

    protected sealed override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            if (bytes != null)
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
            bytes = null!;
        }
    }
}