using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CertTestConsoleApp;

static class Program
{
    static void Main(string[] args)
    {
        try
        {
            //{
            //    using var certificate2 = CertGenerator.GenerateBySelfPfx(null, default, DateTimeOffset.UtcNow.AddYears(1), "ca.pfx");
            //}

            Console.WriteLine("请输入根证书所在路径：");
            var filePath = Console.ReadLine()?.TrimStart('"').TrimEnd('"');

            Console.WriteLine("TestGenerateByCa: ");
            var b1 = TestGenerateByCa(filePath!);
            Console.WriteLine(Convert.ToBase64String(b1));

        }
        catch (Exception ex)
        {
            if (ex is CryptographicException cryptographicException)
            {
                if (unchecked((uint)cryptographicException.HResult) == NTE_BAD_ALGID)
                {
                    // Windows CNG Key Isolation 服务未启用时抛出
                }
                Console.Error.WriteLine($"CryptographicException: HResult={cryptographicException.HResult}");
            }
            Console.Error.WriteLine(ex.ToString());
        }
        finally
        {
            Console.WriteLine("键入回车键退出应用程序...");
            Console.ReadLine();
        }
    }

    const uint NTE_BAD_ALGID = 0x80090008;

    static byte[] TestGenerateByCa(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var domains = new[] { "steampp.net", "www.steampp.net", };
        var subjectName = new X500DistinguishedName($"CN={domains.First()}");
        var x509Certificate2 = new X509Certificate2(filePath);
        using var certificate2 = CertGenerator.CreateEndCertificate(x509Certificate2, subjectName, domains);
        byte[] exported = certificate2.Export(X509ContentType.Cert);
        return exported;
    }
}
