using AigioL.Common.Primitives.Columns;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.EntityFrameworkCore.Extensions;

static partial class DbContextExtensions
{
    /// <summary>
    /// 更新所有修改状态的实体的更新时间
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="kind"></param>
    [Obsolete("use p.Property(nameof(IUpdateTime.UpdateTime)).ValueGeneratedOnAddOrUpdate().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);", true)]
    public static void UpdateModifiedTrackerEntriesTime(this DbContext dbContext, DateTimeKind kind = DateTimeKind.Utc)
    {
        var query = from e in dbContext.ChangeTracker.Entries()
                    where e.State == EntityState.Modified
                    select e.Entity;
        var now = kind == DateTimeKind.Utc ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
        foreach (var e in query.OfType<IUpdateTime>())
        {
            e.UpdateTime = now;
        }
    }
}
