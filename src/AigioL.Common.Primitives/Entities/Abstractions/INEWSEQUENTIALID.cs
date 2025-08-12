namespace AigioL.Common.Primitives.Entities.Abstractions;

/// <summary>
/// 使用 <see cref="Guid.CreateVersion7()"/> 或 SQLServer 的 newsequentialid() 或 PostgreSQL 18+ uuidv7()
/// <para>顺序 GUID，适合索引</para>
/// </summary>
public interface INEWSEQUENTIALID : IEntity<Guid>
{
}
