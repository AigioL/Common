using System.Buffers;
using System.Buffers.Binary;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Windows.Win32;

/// <summary>
/// 从 Win32 PE 文件（.exe / .dll）的资源段中提取 icon 二进制资源。<br/>
/// 只读取 .rsrc 节区（不加载整个文件），节区缓冲来自 <see cref="ArrayPool{T}"/>，
/// <see cref="Span{T}"/> 贯穿解析过程以减少分配。<br/>
/// 返回延迟序列——只有迭代到的 icon 组才会执行 .ico 构建。
/// </summary>
public static class PeIconExtractor
{
    const ushort RT_ICON = 3;
    const ushort RT_GROUP_ICON = 14;
    const uint DIR_FLAG = 0x80000000u;
    const int IMAGE_DIRECTORY_ENTRY_RESOURCE = 2;

    // ── Public API ──────────────────────────────────────────────────────────

    /// <summary>
    /// 打开文件并延迟提取所有 icon 组，枚举结束（完成或中断）后自动关闭文件。
    /// </summary>
    public static IEnumerable<byte[]> ExtractIcons(string peFilePath)
    {
        using var fs = File.OpenRead(peFilePath);
        foreach (var ico in ExtractIcons(fs))
            yield return ico;
    }

    /// <summary>
    /// 从可寻址可读 Stream 延迟提取所有 icon 组。<br/>
    /// 头部校验立即执行；.ico 构建按需延迟。
    /// </summary>
    public static IEnumerable<byte[]> ExtractIcons(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw new ArgumentException("Stream 必须可读且可寻址。", nameof(stream));

        // ── DOS header (64 bytes, stackalloc) ─────────────────────────────────
        Span<byte> dos = stackalloc byte[0x40];
        stream.ReadExactly(dos);
        if (R16(dos, 0) != 0x5A4D) throw new InvalidDataException("缺少 MZ 签名。");
        int peOff = R32s(dos, 0x3C);

        // ── PE signature + COFF file header (4 sig + 20 COFF = 24 bytes) ──────
        stream.Seek(peOff, SeekOrigin.Begin);
        Span<byte> coff = stackalloc byte[24];
        stream.ReadExactly(coff);
        if (R32(coff, 0) != 0x00004550) throw new InvalidDataException("缺少 PE 签名。");
        ushort secCount = R16(coff, 6);   // NumberOfSections
        ushort optHdrSize = R16(coff, 20);   // SizeOfOptionalHeader

        // ── Optional header: 只读 magic + resource 数据目录项 ─────────────────
        // PE32=96, PE32+=112 字节为可选头内数据目录起始偏移
        Span<byte> magic2 = stackalloc byte[2];
        stream.ReadExactly(magic2);
        int dataDirBase = R16(magic2, 0) == 0x20B ? 112 : 96;

        stream.Seek(peOff + 24 + dataDirBase + IMAGE_DIRECTORY_ENTRY_RESOURCE * 8,
                    SeekOrigin.Begin);
        Span<byte> rsrcDirEntry = stackalloc byte[8];
        stream.ReadExactly(rsrcDirEntry);
        uint rsrcRva = R32(rsrcDirEntry, 0);
        uint rsrcSize = R32(rsrcDirEntry, 4);
        if (rsrcRva == 0 || rsrcSize == 0) return [];

        // ── Section table → 找到 .rsrc 原始文件偏移（ArrayPool，立即归还） ────
        stream.Seek(peOff + 24L + optHdrSize, SeekOrigin.Begin);
        int secTableSize = secCount * 40;
        byte[] secBuf = ArrayPool<byte>.Shared.Rent(secTableSize);
        long rsrcRawOff;
        try
        {
            stream.ReadExactly(secBuf, 0, secTableSize);
            rsrcRawOff = FindRsrcRawOffset(secBuf.AsSpan(0, secTableSize), secCount, rsrcRva);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(secBuf);
        }
        if (rsrcRawOff < 0) throw new InvalidDataException("无法将 .rsrc RVA 映射到文件偏移。");

        return ExtractIconsCore(stream, rsrcRawOff, rsrcRva, rsrcSize);
    }

    // ── Iterator core ────────────────────────────────────────────────────────

    static IEnumerable<byte[]> ExtractIconsCore(
        Stream stream, long rsrcRawOff, uint rsrcRva, uint rsrcSize)
    {
        int size = checked((int)rsrcSize);
        byte[] rsrcBuf = ArrayPool<byte>.Shared.Rent(size);

        Dictionary<ushort, byte[]> iconData;
        List<byte[]> groupDirs;
        try
        {
            stream.Seek(rsrcRawOff, SeekOrigin.Begin);
            stream.ReadExactly(rsrcBuf, 0, size);
            iconData = [];
            groupDirs = [];
            // Span 仅在此 try 块内使用，不跨任何 yield → 合法
            WalkDir(rsrcBuf.AsSpan(0, size), rsrcRva,
                    dirOff: 0, depth: 0, typeId: 0, nameId: 0,
                    iconData, groupDirs);
        }
        finally
        {
            // 元数据已复制完毕，尽早归还池缓冲
            ArrayPool<byte>.Shared.Return(rsrcBuf);
        }

        // 此时仅持有 iconData（各帧字节副本）和 groupDirs（组头副本）
        // BuildIco 只在迭代到时执行，调用方用 .First() 只构建一个
        foreach (byte[] grpBytes in groupDirs)
        {
            var ico = BuildIco(grpBytes, iconData);
            if (ico is not null) yield return ico;
        }
    }

    // ── Resource directory walker (Span-based, no lambda capture) ────────────

    // IMAGE_RESOURCE_DIRECTORY: +12 namedCount(2), +14 idCount(2), +16 entries
    // IMAGE_RESOURCE_DIRECTORY_ENTRY (8 bytes):
    //   +0 NameField  uint  bit31=1 → 命名字符串; bit31=0 → 整数 ID（低 16 位）
    //   +4 DataField  uint  bit31=1 → 子目录偏移; bit31=0 → 数据条目偏移
    static void WalkDir(
        ReadOnlySpan<byte> rsrc, uint rsrcRva,
        int dirOff, int depth, ushort typeId, ushort nameId,
        Dictionary<ushort, byte[]> iconData, List<byte[]> groupDirs)
    {
        int total = R16(rsrc, dirOff + 12) + R16(rsrc, dirOff + 14);
        int entries = dirOff + 16;

        for (int i = 0; i < total; i++)
        {
            int eOff = entries + i * 8;
            uint nameField = R32(rsrc, eOff);
            uint dataField = R32(rsrc, eOff + 4);

            // bit31=0 时取低 16 位为 ID；bit31=1 为命名条目，ID 置 0
            ushort entryId = (nameField & DIR_FLAG) == 0 ? unchecked((ushort)nameField) : (ushort)0;
            bool isSubDir = (dataField & DIR_FLAG) != 0;
            int subDirOrOff = (int)(dataField & ~DIR_FLAG);  // ≤ 0x7FFFFFFF，转 int 安全

            switch (depth)
            {
                case 0 when isSubDir: // 类型层
                    WalkDir(rsrc, rsrcRva, subDirOrOff, 1, entryId, 0, iconData, groupDirs);
                    break;
                case 1 when isSubDir: // 名称/ID 层
                    WalkDir(rsrc, rsrcRva, subDirOrOff, 2, typeId, entryId, iconData, groupDirs);
                    break;
                case 2: // 语言层（叶节点）
                    if (!isSubDir)
                    {
                        // IMAGE_RESOURCE_DATA_ENTRY: +0 RVA(4), +4 Size(4)
                        uint dataRva = R32(rsrc, subDirOrOff);
                        uint dataSize = R32(rsrc, subDirOrOff + 4);
                        int dataOff = (int)(dataRva - rsrcRva);
                        if (typeId == RT_ICON)
                            iconData[nameId] = rsrc.Slice(dataOff, (int)dataSize).ToArray();
                        else if (typeId == RT_GROUP_ICON)
                            groupDirs.Add(rsrc.Slice(dataOff, (int)dataSize).ToArray());
                    }
                    return; // 只取第一个语言变体
            }
        }
    }

    // ── .ico assembler (直接写入预分配数组，无 MemoryStream/BinaryWriter) ────

    // GRPICONDIR (6 bytes):       reserved(2) type(2) count(2)
    // GRPICONDIRENTRY (14 bytes): W H Colors Res Planes(2) BitCount(2) BytesInRes(4) nId(2)
    static byte[]? BuildIco(byte[] grpBytes, Dictionary<ushort, byte[]> iconData)
    {
        ReadOnlySpan<byte> grp = grpBytes;
        if (grp.Length < 6 || R16(grp, 2) != 1) return null;
        ushort idCount = R16(grp, 4);
        if (idCount == 0) return null;

        const int GRP_ENTRY = 14;
        const int ENTRY_BASE = 6;

        // 查找各帧图像并统计总字节数
        byte[][] imgs = new byte[idCount][];
        int totalImgBytes = 0;
        for (int i = 0; i < idCount; i++)
        {
            iconData.TryGetValue(R16(grp, ENTRY_BASE + i * GRP_ENTRY + 12), out imgs[i]!);
            imgs[i] ??= [];
            totalImgBytes += imgs[i].Length;
        }

        byte[] output = new byte[6 + idCount * 16 + totalImgBytes];
        Span<byte> dst = output;

        // ICONDIR
        BinaryPrimitives.WriteUInt16LittleEndian(dst, 0);       // idReserved
        BinaryPrimitives.WriteUInt16LittleEndian(dst[2..], 1);       // idType = icon
        BinaryPrimitives.WriteUInt16LittleEndian(dst[4..], idCount);

        uint imgOffset = unchecked((uint)(6 + idCount * 16));
        int pos = 6;

        // ICONDIRENTRY 数组
        for (int i = 0; i < idCount; i++)
        {
            int e = ENTRY_BASE + i * GRP_ENTRY;
            dst[pos] = grp[e];     // bWidth
            dst[pos + 1] = grp[e + 1]; // bHeight
            dst[pos + 2] = grp[e + 2]; // bColorCount
            dst[pos + 3] = grp[e + 3]; // bReserved
            BinaryPrimitives.WriteUInt16LittleEndian(dst[(pos + 4)..], R16(grp, e + 4));                   // wPlanes
            BinaryPrimitives.WriteUInt16LittleEndian(dst[(pos + 6)..], R16(grp, e + 6));                   // wBitCount
            BinaryPrimitives.WriteUInt32LittleEndian(dst[(pos + 8)..], unchecked((uint)imgs[i].Length));    // dwBytesInRes
            BinaryPrimitives.WriteUInt32LittleEndian(dst[(pos + 12)..], imgOffset);                          // dwImageOffset
            pos += 16;
            imgOffset = unchecked(imgOffset + (uint)imgs[i].Length);
        }

        // 图像原始数据
        foreach (byte[] img in imgs)
        {
            img.AsSpan().CopyTo(dst[pos..]);
            pos += img.Length;
        }

        return output;
    }

    // ── Section table: RVA → raw file offset ─────────────────────────────────

    // IMAGE_SECTION_HEADER (40 bytes): Name(8) VirtualSize(4) VA(4) RawSize(4) RawOff(4) ...
    static long FindRsrcRawOffset(ReadOnlySpan<byte> secTable, int secCount, uint rva)
    {
        const int SEC = 40;
        for (int i = 0; i < secCount; i++)
        {
            int off = i * SEC;
            uint va = R32(secTable, off + 12);
            uint rawSz = R32(secTable, off + 16);
            uint rawOff = R32(secTable, off + 20);
            if (rva >= va && rva < va + rawSz)
                return (long)rawOff + (rva - va);
        }
        return -1;
    }

    // ── Little-endian read helpers (unchecked: 允许安全截断，checked 上下文也不抛) ──

    // unchecked 对 R32 不可省略：b[3] >= 128 时 << 24 使 int 为负，
    // 有 checked 上下文的项目里 (uint)(负 int) 会抛 OverflowException。
    static ushort R16(ReadOnlySpan<byte> b, int off) =>
        unchecked((ushort)(b[off] | (b[off + 1] << 8)));

    static uint R32(ReadOnlySpan<byte> b, int off) =>
        unchecked((uint)(b[off] | (b[off + 1] << 8) | (b[off + 2] << 16) | (b[off + 3] << 24)));

    static int R32s(ReadOnlySpan<byte> b, int off) =>
        unchecked(b[off] | (b[off + 1] << 8) | (b[off + 2] << 16) | (b[off + 3] << 24));
}
