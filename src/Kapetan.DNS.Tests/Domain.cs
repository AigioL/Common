using DNS.Tests;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace DNS.Protocol;

partial class Domain
{
    public Domain(byte[][] labels) : this(labels.AsReadOnlyMemory())
    {
        // Compatible only with test code
    }
}