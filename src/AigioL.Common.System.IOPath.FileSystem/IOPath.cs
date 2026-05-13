using System.Security.Cryptography;

namespace System.IO;

public static partial class IOPath
{
    // 函数名参考 Win32 函数定义

    public static DirectoryInfo CreateDirectory(string dirPath)
    {
        if (File.Exists(dirPath))
        {
            File.Delete(dirPath);
        }
        var dirInfo = Directory.CreateDirectory(dirPath);
        return dirInfo;
    }

    /// <summary>
    /// 删除文件，如果传入的路径为文件夹，则删除整个文件夹
    /// </summary>
    /// <param name="fileName"></param>
    public static void DeleteFile(string fileName)
    {
        if (Directory.Exists(fileName))
        {
            // 如果传入路径为文件夹，调用文件删除会抛出 UnauthorizedAccessException
            // 一些用户使用将路径创建为文件夹并禁止访问权限来实现防止删除或创建文件的目的
            Directory.Delete(fileName, true);
        }
        else
        {
            File.Delete(fileName);
        }
    }

    /// <inheritdoc cref="DeleteFile(string)"/>/>
    public static void DeleteFile(this FileInfo fileInfo)
        => DeleteFile(fileInfo.FullName);

    /// <summary>
    /// 删除目录，如果传入的路径为文件，则删除该文件
    /// </summary>
    /// <param name="dirPath"></param>
    /// <param name="recursive"></param>
    public static void RemoveDirectory(string dirPath, bool recursive = true)
    {
        if (File.Exists(dirPath))
        {
            // 如果传入路径为文件，调用目录删除会抛出 IOException 目录名称无效
            File.Delete(dirPath);
        }
        else
        {
            Directory.Delete(dirPath, recursive);
        }
    }

    /// <inheritdoc cref="RemoveDirectory(string, bool)"/>/>
    public static void RemoveDirectory(this DirectoryInfo directoryInfo, bool recursive = true)
        => RemoveDirectory(directoryInfo.FullName, recursive);

    /// <summary>
    /// 打开文件流，如果传入的路径为文件夹，则删除该文件夹后再创建文件流
    /// </summary>
    /// <param name="path"></param>
    /// <param name="mode"></param>
    /// <param name="access"></param>
    /// <param name="share"></param>
    /// <returns></returns>
    public static FileStream GetFileStream(string path, FileMode mode, FileAccess access, FileShare share)
    {
        switch (mode)
        {
            case FileMode.CreateNew: // UnauthorizedAccessException: Access to the path
            case FileMode.Create:
            case FileMode.Open:
            case FileMode.OpenOrCreate:
            case FileMode.Truncate:
            case FileMode.Append: // ArgumentException: Append access can be requested only in write-only mode. (Parameter 'access')
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                }
                break;
        }
        var fileStream = new FileStream(path, mode, access, share);
        return fileStream;
    }
}

static partial class IOPath
{
    public static string WriteFile(string dirPath, string fileName, Stream stream)
    {
        var filePath = Path.Combine(dirPath, fileName);
        try
        {
            using var fileStreamR = GetFileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

            Span<byte> hashDest = stackalloc byte[SHA384.HashSizeInBytes];
            var lenDest = SHA384.HashData(fileStreamR, hashDest);
            hashDest = hashDest[..lenDest];

            Span<byte> hash = stackalloc byte[SHA384.HashSizeInBytes];
            var len = SHA384.HashData(stream, hash);
            hash = hash[..len];

            if (hash.SequenceEqual(hashDest))
            {
                return filePath; // 文件已存在且内容相同
            }
        }
        catch (FileNotFoundException)
        {
        }
        catch (DirectoryNotFoundException)
        {
        }
        catch (Exception)
        {
            DeleteFile(filePath);
        }
        var parent = Directory.GetParent(filePath);
        if (parent != null)
        {
            CreateDirectory(parent.FullName);
        }
        using var fileStreamW = GetFileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
        stream.Position = 0; // 重置流位置
        stream.CopyTo(fileStreamW);
        fileStreamW.Flush();
        fileStreamW.SetLength(fileStreamW.Position); // 确保文件长度正确
        return filePath; // 返回新创建的文件路径
    }
}

public sealed class ReverseByteStream : Stream
{
    readonly Stream _baseStream;
    readonly long _length;
    long _position;

    internal ReverseByteStream(Stream baseStream)
    {
        _baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
        if (!_baseStream.CanSeek)
            throw new ArgumentException("Base stream must support seeking", nameof(baseStream));

        _length = _baseStream.Length;
        _position = 0;
    }

    public override bool CanRead => _baseStream.CanRead;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => _length;

    public override long Position { get => _position; set => _position = value; }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var bytesToRead = Math.Min(count, (int)(_length - _position));
        if (bytesToRead <= 0) return 0;

        for (int i = 0; i < bytesToRead; i++)
        {
            // 从末尾开始逐字节读取
            var basePosition = _length - _position - 1 - i;
            _baseStream.Seek(basePosition, SeekOrigin.Begin);

            var byteValue = _baseStream.ReadByte();
            if (byteValue == -1) break;

            buffer[offset + i] = (byte)byteValue;
        }

        _position += bytesToRead;
        return bytesToRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        long newPosition = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            SeekOrigin.End => _length + offset,
            _ => throw new ArgumentException("Invalid seek origin", nameof(origin))
        };

        if (newPosition < 0 || newPosition > _length)
            throw new ArgumentOutOfRangeException(nameof(offset));

        _position = newPosition;
        return _position;
    }

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override void Flush() { }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _baseStream?.Dispose();
        }
        base.Dispose(disposing);
    }
}