// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Runtime.CompilerServices;

/// <summary>
/// https://github.com/dotnet/aspnetcore/blob/v11.0.0-preview.7.26381.103/src/Shared/ServerInfrastructure/RuntimeAsyncMethodGenerationAttribute.cs
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class RuntimeAsyncMethodGenerationAttribute(bool runtimeAsync) : Attribute
{
    public bool RuntimeAsync => runtimeAsync;
}
