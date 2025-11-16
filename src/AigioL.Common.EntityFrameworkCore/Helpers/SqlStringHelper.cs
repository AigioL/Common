using AigioL.Common.Primitives.Columns;
using System.Runtime.CompilerServices;

namespace AigioL.Common.EntityFrameworkCore.Helpers;

/// <summary>
/// 用于 SQL 语句字符串相关的常量和方法，使用字符串拼接时应注意防止 SQL 注入攻击
/// <para>EFCore 可采用 <see cref="FormattableString"/> 参数化传入参数</para>
/// </summary>
public static partial class SqlStringHelper
{
    /// <summary>
    /// Microsoft.EntityFrameworkCore.SqlServer
    /// </summary>
    public const string SqlServer = "Microsoft.EntityFrameworkCore.SqlServer";

    /// <summary>
    /// Npgsql.EntityFrameworkCore.PostgreSQL
    /// </summary>
    public const string PostgreSQL = "Npgsql.EntityFrameworkCore.PostgreSQL";

    /// <summary>
    /// 当前数据库提供程序
    /// </summary>
    public static string DatabaseProvider { get; set; } = PostgreSQL;

    public static void ConfigPostgreSQL(bool? postgreSQL18Plus = true)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        if (postgreSQL18Plus.HasValue)
        {
            PostgreSQL18Plus = postgreSQL18Plus.Value;
        }
    }

    /// <summary>
    /// 使用启用 Z.EntityFramework.Plus.EFCore
    /// </summary>
    public static bool ZPlusEnable
    {
        get
        {
#if USE_ZPLUS
            if (_._ZPlusEnable.HasValue)
                return _._ZPlusEnable.Value;
            return DatabaseProvider switch
            {
                SqlServer => true,
                _ => false,
            };
#else
            return false;
#endif
        }
#if USE_ZPLUS
        set => _._ZPlusEnable = value;
#endif
    }

    /// <summary>
    /// 是否使用 PostgreSQL 18 及以上版本
    /// </summary>
    public static bool PostgreSQL18Plus
    {
        get
        {
            if (_._PostgreSQL18Plus.HasValue)
                return _._PostgreSQL18Plus.Value;
            return false;
        }
        set => _._PostgreSQL18Plus = value;
    }

    /// <summary>
    /// 返回包含计算机的日期和时间的 datetimeoffset(7) 值，SQL Server 的实例正在该计算机上运行。 时区偏移量包含在内。
    /// <para>https://docs.microsoft.com/zh-cn/sql/t-sql/functions/sysdatetimeoffset-transact-sql?view=azuresqldb-current</para>
    /// </summary>
    public const string SYSDATETIMEOFFSET = "SYSDATETIMEOFFSET()";

    /// <summary>
    /// 在启动 Windows 后在指定计算机上创建大于先前通过该函数生成的任何 GUID 的 GUID。 在重新启动 Windows 后，GUID 可以再次从一个较低的范围开始，但仍是全局唯一的。 在 GUID 列用作行标识符时，使用 NEWSEQUENTIALID 可能比使用 NEWID 函数的速度更快。 其原因在于，NEWID 函数导致随机行为并且使用更少的缓存数据页。 使用 NEWSEQUENTIALID 还有助于完全填充数据和索引页。
    /// <para>https://docs.microsoft.com/zh-cn/sql/t-sql/functions/newsequentialid-transact-sql?view=azuresqldb-current</para>
    /// </summary>
    public const string NEWSEQUENTIALID = "newsequentialid()";

    /// <summary>
    /// 表示 ON 的字符串常量
    /// </summary>
    public const string ON = "ON";

    /// <summary>
    /// 表示 OFF 的字符串常量
    /// </summary>
    public const string OFF = "OFF";

    #region PostgreSQL Functions

    /// <summary>
    /// PostgreSQL 的生成随机 UUID 函数的字符串常量
    /// <para>https://www.postgresql.org/docs/15/functions-uuid.html</para>
    /// </summary>
    public const string gen_random_uuid = "gen_random_uuid()";

    public const string uuidv7 = "uuidv7()";

    /// <summary>
    /// PostgreSQL 的获取当前时间函数的字符串常量
    /// <para>https://www.postgresql.org/docs/15/functions-datetime.html#FUNCTIONS-DATETIME-CURRENT</para>
    /// </summary>
    public const string now = "now()";

    #endregion

    /// <summary>
    /// 获取 DateTimeOffset 类型字段的默认值 SQL 表达式
    /// <list>根据当前数据库提供程序判断：
    /// <item>如果为 SqlServer，则返回  <see cref="SYSDATETIMEOFFSET"/></item>
    /// <item>如果为 PostgreSQL，则返回 <see cref="now"/></item>
    /// <item>其它情况则抛出异常</item>
    /// </list>
    /// </summary>
    public static string DateTimeOffsetDefaultValueSql => DatabaseProvider switch
    {
        SqlServer => SYSDATETIMEOFFSET,
        PostgreSQL => now,
        _ => throw ThrowHelper.GetArgumentOutOfRangeException(DatabaseProvider),
    };

    /// <summary>
    /// 根据序列名称获取下一个值的默认值 SQL 表达式
    /// </summary>
    public static string NextValueSequenceDefaultValueSql(string sequenceName) => DatabaseProvider switch
    {
        SqlServer => $"NEXT VALUE FOR {sequenceName}",
        PostgreSQL => $"nextval('\"{sequenceName}\"')",
        _ => throw ThrowHelper.GetArgumentOutOfRangeException(DatabaseProvider),
    };

    /// <summary>
    /// 允许将显式值插入到表的标识列中。
    /// <para>https://docs.microsoft.com/zh-cn/sql/t-sql/statements/set-identity-insert-transact-sql?view=azuresqldb-current</para>
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="enable"></param>
    /// <returns></returns>
    public static string IDENTITY_INSERT(string tableName, bool enable)
    {
        var value = enable ? ON : OFF;
        var sql = $"SET IDENTITY_INSERT {tableName} {value}";
        return sql;
    }

    /// <summary>
    /// "select * from "
    /// <para>查询所有列</para>
    /// </summary>
    public const string SelectALLFrom = "select * from ";

    /// <summary>
    /// "select count(Id) from "
    /// <para>查询总数，仅Id列</para>
    /// </summary>
    public const string SelectCountFrom = "select count(Id) from ";

    /// <summary>
    /// "select count({0}) from "
    /// <para>查询总数，仅某一列</para>
    /// <para>注意：参数仅使用列名固定值，不可传递变量，防止SQL注入攻击</para>
    /// </summary>
    public const string SelectCountFrom_0 = "select count({0}) from ";

    /// <summary>
    /// " order by CreationTime desc"
    /// <para>根据 创建时间 倒序</para>
    /// </summary>
    public const string OrderByCreationTimeDescending = " order by CreationTime desc";

    /// <summary>
    /// " order by ReceiveTime, CreationTime desc"
    /// <para>根据 接收时间, 创建时间 倒序</para>
    /// </summary>
    public const string OrderByReceiveTimeThenByCreationTimeDescending = " order by ReceiveTime, CreationTime desc";

    /// <summary>
    /// " order by {0} desc"
    /// <para>根据 某一列 倒序</para>
    /// <para>注意：参数仅使用列名固定值，不可传递变量，防止 SQL 注入攻击</para>
    /// </summary>
    public const string OrderBy_Descending_0 = " order by {0} desc";

    /// <summary>
    /// "delete from "
    /// </summary>
    public const string DeleteFrom = "delete from ";

    /// <summary>
    /// " where Id = {0}"
    /// <para>注意：参数仅使用列名固定值，不可传递变量，防止 SQL 注入攻击</para>
    /// </summary>
    public const string WhereIdEqual_0 = " where Id = {0}";

    /// <summary>
    /// "update "
    /// </summary>
    public const string Update = "update ";

    /// <summary>
    /// " where Id in ("
    /// </summary>
    public const string WhereInIds_ = " where Id in (";

    /// <summary>
    /// 右括号
    /// </summary>
    public const char RightBracket = ')';

    /// <summary>
    /// "delete from {tableName} where Id = {0}"
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static FormattableString DeleteFromTableNameWhereIdEqual(string tableName, object id)
    {
        var sql = DeleteFrom + tableName + WhereIdEqual_0;
        return FormattableStringFactory.Create(sql, id);
    }

    /// <summary>
    /// "update from {tableName} set IsSoftDeleted = 1 where Id = {0}"
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static FormattableString SoftDeleteFromTableNameWhereIdEqual(string tableName, object id)
    {
        var sql = Update + tableName + " set " + nameof(ISoftDeleted.SoftDeleted) + " = 1" + WhereIdEqual_0;
        return FormattableStringFactory.Create(sql, id);
    }
}

file static class _
{
    /// <summary>
    /// 是否开启 ZPlus 功能
    /// </summary>
    internal static bool? _ZPlusEnable;

    internal static bool? _PostgreSQL18Plus;
}