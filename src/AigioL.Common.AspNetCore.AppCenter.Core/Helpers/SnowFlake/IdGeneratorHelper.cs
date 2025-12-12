using Yitter.IdGenerator;

namespace AigioL.Common.AspNetCore.AppCenter.Helpers.SnowFlake;

/// <summary>
/// 雪花算法 Id 生成助手类
/// </summary>
public static partial class IdGeneratorHelper
{
    static readonly ushort instanceId;

    static IdGeneratorHelper()
    {
        if (ushort.TryParse(Environment.GetEnvironmentVariable("InstanceId"),
            out ushort instanceId))
        {
            IdGeneratorHelper.instanceId = instanceId;
        }
    }

    /// <summary>
    /// 获取一个雪花算法 Id
    /// </summary>
    /// <returns></returns>
    public static string GetNextId()
    {
        EnsureInitialization();
        var nextId = YitIdHelper.NextId();
        return nextId.ToString();
    }

    /// <summary>
    /// 确保初始化
    /// </summary>
    /// <remarks>
    /// 如果未初始化优先尝试从环境变量 InstanceId 获取 WorkerId，
    /// 否则随机一个（目前就这样处理啦，并发量不高，生成重复 Id 的机率较小）
    /// </remarks>
    static void EnsureInitialization()
    {
        if (YitIdHelper.IdGenInstance is null)
        {
            ushort workerId = instanceId;
#if !DEBUG
            if (workerId == 0)
            {
                workerId = (ushort)Random.Shared.Next(1, ushort.MaxValue);
                //throw new ArgumentException("需要为微服务的传递 InstanceId 参数，才能使用 GetNextId 来获取一个 Id。");
            }
#endif
            YitIdHelper.SetIdGenerator(new(instanceId));
        }
    }
}