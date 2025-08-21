using System.Runtime.CompilerServices;

namespace System.Security.Cryptography.X509Certificates;

public static partial class X509CertificateExtensions
{
    /// <summary>
    /// 获取证书的主题备用名称
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string[] GetSubjectAlternativeNames(this X509Certificate2 certificate)
    {
        foreach (X509Extension extension in certificate.Extensions)
        {
            // Create an AsnEncodedData object using the extensions information.
            if (string.Equals(extension.Oid?.FriendlyName, "Subject Alternative Name"))
            {
                var asndata = new AsnEncodedData(extension.Oid, extension.RawData);
                var asndataF = asndata.Format(true);
                var result = asndataF.Split([
                    Environment.NewLine,
                    "DNS Name=",
                ], StringSplitOptions.RemoveEmptyEntries);
                return result;
            }
        }
        return [];
    }
}
