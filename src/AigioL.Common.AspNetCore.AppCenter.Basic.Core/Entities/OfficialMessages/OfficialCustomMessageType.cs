//using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
//using AigioL.Common.AspNetCore.AppCenter.Basic.Models;
//using AigioL.Common.Primitives.Entities.Abstractions;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.OfficialMessages;

///// <summary>
///// 官方消息类型实体类
///// </summary>
//[Table("OfficialCustomMessageTypes")]
//public class OfficialCustomMessageType :
//    TenantBaseEntity<Guid>,
//    INEWSEQUENTIALID
//{
//    public required string Name { get; set; }

//    public OfficialMessageType Type { get; set; }

//    /// <summary>
//    /// 获取预设的数据种子值
//    /// <para>https://learn.microsoft.com/zh-cn/ef/core/modeling/data-seeding</para>
//    /// </summary>
//    /// <param name="tenantId"></param>
//    /// <param name="isRootTenantId"></param>
//    /// <returns></returns>
//    public static IEnumerable<OfficialCustomMessageType> GetDataSeeding(Guid tenantId, bool isRootTenantId)
//    {
//        var query = from m in Enum.GetValues<OfficialMessageType>()
//                    let name = m.ToDisplayString()
//                    let id = isRootTenantId ? m.ToCompatNoticeGroupId() : Guid.CreateVersion7()
//                    select new OfficialCustomMessageType
//                    {
//                        Id = id,
//                        TenantId = tenantId,
//                        Name = name,
//                        Type = m,
//                    };
//        return query;
//    }
//}
