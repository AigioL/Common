// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 
// https://github.com/dotnet/reactive/blob/asyncrxnet-v6.0.0-alpha.18/AsyncRx.NET/System.Reactive.Async/Threading/AsyncGate.cs

using System.Diagnostics;

namespace System.Threading;

sealed class AsyncGate
{
    readonly Lock _gate = new();
    readonly SemaphoreSlim _semaphore = new(1, 1);
    readonly AsyncLocal<int> _recursionCount = new();

    public ValueTask<Releaser> LockAsync()
    {
        var shouldAcquire = false;

        lock (_gate)
        {
            if (_recursionCount.Value == 0)
            {
                shouldAcquire = true;
                _recursionCount.Value = 1;
            }
            else
            {
                _recursionCount.Value++;
            }
        }

        if (shouldAcquire)
        {
            return new ValueTask<Releaser>(_semaphore.WaitAsync().ContinueWith(_ => new Releaser(this)));
        }

        return new ValueTask<Releaser>(new Releaser(this));
    }

    void Release()
    {
        lock (_gate)
        {
            Debug.Assert(_recursionCount.Value > 0);

            if (--_recursionCount.Value == 0)
            {
                _semaphore.Release();
            }
        }
    }

    public readonly struct Releaser : IDisposable
    {
        readonly AsyncGate _parent;

        public Releaser(AsyncGate parent) => _parent = parent;

        public void Dispose() => _parent.Release();
    }
}