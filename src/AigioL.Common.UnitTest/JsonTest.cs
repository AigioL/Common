using AigioL.Common.FeishuOApi.Sdk.Services;
using System.Buffers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AigioL.Common.UnitTest;

public sealed class JsonTest : BaseUnitTest
{
    readonly JsonSerializerOptions o = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 不转义字符！！！
    };

    [Fact]
    public void GetRichTextMessage()
    {
        var title = "测试标题";
        var text = "测试内容";
        var richTextMessage = FeishuApiClient.GetRichTextMessage(title, text, out var bytesWritten);
        string? json = null;
        try
        {
            json = Encoding.UTF8.GetString(richTextMessage, 0, bytesWritten);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(richTextMessage);
        }

        var n = JsonNode.Parse(json)!; // 解析 JSON 字符串以验证格式正确
        Console.WriteLine(n.ToJsonString(o));
    }
}