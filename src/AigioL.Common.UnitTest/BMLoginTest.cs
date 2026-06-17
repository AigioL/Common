using AigioL.Common.AspNetCore.AdminCenter.Helpers;
using System.Text.Json;

namespace AigioL.Common.UnitTest;

public sealed class BMLoginTest : BaseUnitTest
{
    [Fact]
    public async Task CallTest()
    {
        const string rsaPublicKey =
"""

""";
        const string baseAddress = "";
        const string username = "";
        const string password = "";

        if (string.IsNullOrWhiteSpace(baseAddress) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(rsaPublicKey))
        {
            return;
        }

        var jwt = await BMLoginHelper.LoginAsync(Convert.FromBase64String(rsaPublicKey), new(baseAddress, UriKind.Absolute), username, password);

        Console.WriteLine(JsonSerializer.Serialize(jwt, new JsonSerializerOptions(BMLoginHelperJsonSerializerContext.Default.Options)
        {
            WriteIndented = true,
        }));
    }
}
