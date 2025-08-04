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
