using AigioL.Common.Net.ReverseProxy.Models;
using AigioL.Common.Net.ReverseProxy.Services.Abstractions;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.FlowAnalyzer;

/// <summary>
/// 通过 <see cref="DelegatingStream"/> 重写流的读取和写入，调用 <see cref="IFlowAnalyzer"/> 记录流量字节长度
/// </summary>
sealed class FlowAnalyzeStream(Stream inner, IFlowAnalyzer flowAnalyzer) : DelegatingStream(inner)
{
    #region Read

    public sealed override int Read(byte[] buffer, int offset, int count)
    {
        var read = base.Read(buffer, offset, count);
        flowAnalyzer.OnFlow(FlowType.Read, read);
        return read;
    }

    public sealed override int Read(Span<byte> destination)
    {
        var read = base.Read(destination);
        flowAnalyzer.OnFlow(FlowType.Read, read);
        return read;
    }

    public sealed override int ReadByte()
    {
        var read = base.ReadByte();
        if (read >= 0)
        {
            flowAnalyzer.OnFlow(FlowType.Read, 1);
        }
        return read;
    }


    public sealed override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var read = await base.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
        flowAnalyzer.OnFlow(FlowType.Read, read);
        return read;
    }

    public sealed override async ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
    {
        var read = await base.ReadAsync(destination, cancellationToken);
        flowAnalyzer.OnFlow(FlowType.Read, read);
        return read;
    }

    #endregion

    #region Write

    public sealed override void Write(byte[] buffer, int offset, int count)
    {
        flowAnalyzer.OnFlow(FlowType.Wirte, count);
        base.Write(buffer, offset, count);
    }

    public sealed override void Write(ReadOnlySpan<byte> source)
    {
        flowAnalyzer.OnFlow(FlowType.Wirte, source.Length);
        base.Write(source);
    }

    public sealed override void WriteByte(byte value)
    {
        flowAnalyzer.OnFlow(FlowType.Wirte, 1);
        base.WriteByte(value);
    }

    public sealed override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        flowAnalyzer.OnFlow(FlowType.Wirte, count);
        return base.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public sealed override ValueTask WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
    {
        flowAnalyzer.OnFlow(FlowType.Wirte, source.Length);
        return base.WriteAsync(source, cancellationToken);
    }

    #endregion
}