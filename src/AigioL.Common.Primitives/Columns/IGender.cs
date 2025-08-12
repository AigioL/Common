using AigioL.Common.Primitives.Models;

namespace AigioL.Common.Primitives.Columns;

/// <inheritdoc cref="Models.Gender"/>
public interface IGender : IReadOnlyGender
{
    /// <inheritdoc cref="Models.Gender"/>
    new Gender Gender { get; set; }
}

/// <inheritdoc cref="Models.Gender"/>
public interface IReadOnlyGender
{
    /// <inheritdoc cref="Models.Gender"/>
    Gender Gender { get; }
}