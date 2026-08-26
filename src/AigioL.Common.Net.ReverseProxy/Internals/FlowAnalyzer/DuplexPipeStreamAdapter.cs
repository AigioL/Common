// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO.Pipelines;

namespace Microsoft.AspNetCore.Server.Kestrel.Core.Internal;

/// <summary>
/// A helper for wrapping a Stream decorator from an <see cref="IDuplexPipe"/>.
/// <para>https://github.com/dotnet/aspnetcore/blob/v11.0.0-preview.7.26381.103/src/Shared/ServerInfrastructure/DuplexPipeStreamAdapter.cs</para>
/// </summary>
/// <typeparam name="TStream"></typeparam>
sealed class DuplexPipeStreamAdapter<TStream> : DuplexPipeStream, IDuplexPipe where TStream : Stream
{
    bool _disposed;
    readonly Lock _disposeLock = new();

    public DuplexPipeStreamAdapter(IDuplexPipe duplexPipe, Func<Stream, TStream> createStream) :
        this(duplexPipe, new StreamPipeReaderOptions(leaveOpen: true), new StreamPipeWriterOptions(leaveOpen: true), createStream)
    {
    }

    public DuplexPipeStreamAdapter(IDuplexPipe duplexPipe, StreamPipeReaderOptions readerOptions, StreamPipeWriterOptions writerOptions, Func<Stream, TStream> createStream) :
        base(duplexPipe.Input, duplexPipe.Output)
    {
        var stream = createStream(this);
        Stream = stream;
        Input = PipeReader.Create(stream, readerOptions);
        Output = PipeWriter.Create(stream, writerOptions);
    }

    public TStream Stream { get; }

    public PipeReader Input { get; }

    public PipeWriter Output { get; }

    public sealed override async ValueTask DisposeAsync()
    {
        lock (_disposeLock)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
        }

        await Input.CompleteAsync();
        await Output.CompleteAsync();
    }

    protected sealed override void Dispose(bool disposing)
    {
        throw new NotSupportedException();
    }
}