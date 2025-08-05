using System.Security.Cryptography;

namespace AigioL.Common.UnitTest;

public sealed class RSATest : BaseUnitTest
{
    [Fact]
    public void ParametersTest()
    {
        RSAParameters parameters = new()
        {
            D = "D"u8.ToArray(),
            DQ = "DQ"u8.ToArray(),
            DP = "DP"u8.ToArray(),
            Exponent = "Exponent"u8.ToArray(),
            InverseQ = "InverseQ"u8.ToArray(),
            Modulus = "Modulus"u8.ToArray(),
            P = "P"u8.ToArray(),
            Q = "Q"u8.ToArray()
        };

        using MemoryStream s = new();
        RSAUtils.WriteParameters(s, parameters);

        var parameters_s = s.ToArray();
        var parameters_b = new byte[RSAUtils.GetParametersLength(parameters)];

        Assert.True(parameters_s.Length == parameters_b.Length, "Parameters length mismatch");

        RSAUtils.WriteParameters(parameters_b, parameters);

        Assert.True(parameters_s.SequenceEqual(parameters_b), "Parameters byte array mismatch");

        var parameters_s2 = RSAUtils.ReadParameters(s);
        var parameters_b2 = RSAUtils.ReadParameters(parameters_b);

        Assert.True(parameters_s2.D.SequenceEqual(parameters_b2.D), "D mismatch");
        Assert.True(parameters_s2.DQ.SequenceEqual(parameters_b2.DQ), "DQ mismatch");
        Assert.True(parameters_s2.DP.SequenceEqual(parameters_b2.DP), "DP mismatch");
        Assert.True(parameters_s2.Exponent.SequenceEqual(parameters_b2.Exponent), "Exponent mismatch");
        Assert.True(parameters_s2.InverseQ.SequenceEqual(parameters_b2.InverseQ), "InverseQ mismatch");
        Assert.True(parameters_s2.Modulus.SequenceEqual(parameters_b2.Modulus), "Modulus mismatch");
        Assert.True(parameters_s2.P.SequenceEqual(parameters_b2.P), "P mismatch");
        Assert.True(parameters_s2.Q.SequenceEqual(parameters_b2.Q), "Q mismatch");
        Assert.True(RSAUtils.GetParametersLength(parameters_s2) == RSAUtils.GetParametersLength(parameters_b2), "Parameters length mismatch after read");

        Assert.True(parameters.D.SequenceEqual(parameters_b2.D), "D mismatch");
        Assert.True(parameters.DQ.SequenceEqual(parameters_b2.DQ), "DQ mismatch");
        Assert.True(parameters.DP.SequenceEqual(parameters_b2.DP), "DP mismatch");
        Assert.True(parameters.Exponent.SequenceEqual(parameters_b2.Exponent), "Exponent mismatch");
        Assert.True(parameters.InverseQ.SequenceEqual(parameters_b2.InverseQ), "InverseQ mismatch");
        Assert.True(parameters.Modulus.SequenceEqual(parameters_b2.Modulus), "Modulus mismatch");
        Assert.True(parameters.P.SequenceEqual(parameters_b2.P), "P mismatch");
        Assert.True(parameters.Q.SequenceEqual(parameters_b2.Q), "Q mismatch");
    }
}