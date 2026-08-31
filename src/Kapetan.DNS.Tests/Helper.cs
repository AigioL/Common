using DNS.Protocol;
using System.Runtime.CompilerServices;

namespace DNS.Tests;

public static class Helper
{
    static string GetRootDirPath([CallerFilePath] string sourceFilePath = "")
    {
        return Path.GetFullPath(Path.Combine(sourceFilePath, ".."));
    }

    public static byte[] ReadFixture(params IEnumerable<string> paths)
    {
        var paths2 = new string[] { GetRootDirPath(), "Fixtures" }.Concat(paths).ToArray();
        var path = Path.Combine(paths2);
        return File.ReadAllBytes(path);
    }

    public static T[] GetArray<T>(params T[] parameters)
    {
        return parameters;
    }

    public static IList<T> GetList<T>(params T[] parameters)
    {
        return new List<T>(parameters);
    }

    public static byte[] ToArray(this IMessageEntry i)
    {
        var a = GC.AllocateUninitializedArray<byte>(i.Size);
        i.Write(a);
        return a;
    }

    public static byte[] ToArray(this IMessage i)
    {
        var a = GC.AllocateUninitializedArray<byte>(i.Size);
        i.Write(a);
        return a;
    }

    public static byte[] ToArray(this Header i)
    {
        var a = GC.AllocateUninitializedArray<byte>(i.Size);
        i.Write(a);
        return a;
    }

    public static byte[] ToArray(this CharacterString i)
    {
        var a = GC.AllocateUninitializedArray<byte>(i.Size);
        i.Write(a);
        return a;
    }

    public static ReadOnlyMemory<byte>[] AsReadOnlyMemory(this byte[][] b)
        => [.. b.Select(static x => new ReadOnlyMemory<byte>(x))];
}
