public sealed partial class ImageHandleRequestModel
{
    /// <summary>
    /// 目标上传路径
    /// </summary>
    public required string UrlPath { get; set; }

    /// <summary>
    /// 图片 URL 地址
    /// </summary>
    public required string ImageUrl { get; set; }

    /// <summary>
    /// 目标保存类型 
    /// ImageMagick 的 ImageMagick.MagickFormat
    /// 260 为 WebP 格式
    /// </summary>
    public int HandleType { get; set; } = 260;

    /// <summary>
    /// 目标宽度
    /// </summary>
    public uint Width { get; set; }

    /// <summary>
    /// 目标高度
    /// </summary>
    public uint Height { get; set; }

    /// <summary>
    /// 目标指令 默认 75
    /// </summary>
    public int Quality { get; set; } = 75;
}