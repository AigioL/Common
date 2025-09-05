using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;

public partial interface IMerchantDeductionAgreementRepository : IRepository<MerchantDeductionAgreement, Guid>, IEFRepository
{
    ///// <summary>
    ///// 分页查询MerchantDeductionAgreement表格
    ///// </summary>
    ///// <param name="id">Id</param>
    ///// <param name="userId">UserId</param>
    ///// <param name="signingTime">签约时间</param>
    ///// <param name="unSigningTime">解约时间</param>
    ///// <param name="platform">平台类型</param>
    ///// <param name="agreementNo">签约协议号</param>
    ///// <param name="alipayUserId">支付宝用户 Id</param>
    ///// <param name="alipayLoginAccount">支付宝登录账号</param>
    ///// <param name="alipayAgreementNo">支付宝协议号</param>
    ///// <param name="validTime">生效时间</param>
    ///// <param name="invalidTime">失效时间</param>
    ///// <param name="signScene">签约场景码</param>
    ///// <param name="period">周期数</param>
    ///// <param name="periodType">周期类型</param>
    ///// <param name="executeTime">扣款执行日期</param>
    ///// <param name="nextDeductionTime">下次扣款时间</param>
    ///// <param name="singleAmount">单次扣款金额</param>
    ///// <param name="status">状态</param>
    ///// <param name="creationTime">创建时间</param>
    ///// <param name="updateTime">更新时间</param>
    ///// <param name="remarks">备注</param>
    ///// <param name="orderBy">排序字段</param>
    ///// <param name="desc">排序: false 为降序，true 为升序 </param>
    ///// <param name="businessType"></param>
    ///// <param name="current">当前页码，页码从 1 开始，默认值：<see cref="IPagedModel.DefaultCurrent"/></param>
    ///// <param name="pageSize">页大小，如果为 0 必定返回空集合，默认值：<see cref="IPagedModel.DefaultPageSize"/></param>
    ///// <returns>MerchantDeductionAgreement分页表格查询结果数据</returns>
    //Task<PagedModel<TableMerchantDeductionAgreementDTO>> QueryAsync(
    //    Guid? id,
    //    Guid? userId,
    //    DateTimeOffset?[]? signingTime,
    //    DateTimeOffset?[]? unSigningTime,
    //    PaymentType? platform,
    //    string? agreementNo,
    //    string? alipayUserId,
    //    string? alipayLoginAccount,
    //    string? alipayAgreementNo,
    //    DateTimeOffset? validTime,
    //    DateTimeOffset? invalidTime,
    //    string? signScene,
    //    long? period,
    //    string? periodType,
    //    DateTimeOffset?[]? executeTime,
    //    DateTimeOffset?[]? nextDeductionTime,
    //    decimal? singleAmount,
    //    AgreementStatus? status,
    //    DateTimeOffset[]? creationTime,
    //    DateTimeOffset[]? updateTime,
    //    string? remarks,
    //    string? orderBy,
    //    bool? desc,
    //    OrderBusinessType? businessType,
    //    int current = IPagedModel.DefaultCurrent,
    //    int pageSize = IPagedModel.DefaultPageSize);

    ///// <summary>
    ///// 添加业务商家扣款协议
    ///// </summary>
    ///// <param name="merchantDeductionAgreement"></param>
    ///// <param name="rechargeProduct">充值商品</param>
    ///// <returns></returns>
    //Task<MerchantDeductionAgreement> AddXunYouAgreement(MerchantDeductionAgreement merchantDeductionAgreement, XunYouRechargeProduct rechargeProduct);

    ///// <summary>
    ///// 添加业务商家扣款协议
    ///// </summary>
    ///// <param name="merchantDeductionAgreement"></param>
    ///// <param name="firstRechargeProduct">首次充值商品</param>
    ///// <param name="rechargeProduct">充值商品</param>
    ///// <returns></returns>
    //Task<MerchantDeductionAgreement> AddXunYouAgreement(MerchantDeductionAgreement merchantDeductionAgreement, XunYouRechargeProduct firstRechargeProduct, XunYouRechargeProduct rechargeProduct);

    /// <summary>
    /// 按协议号查找协议
    /// </summary>
    /// <param name="agreementNo"></param>
    /// <returns></returns>
    Task<MerchantDeductionAgreement?> GetByNo(string agreementNo);

    /// <summary>
    /// 解约业务商家扣款协议
    /// </summary>
    /// <param name="agreementNo"></param>
    /// <returns></returns>
    Task<bool> DoUnSignAgreement(string agreementNo);

    /// <summary>
    /// 完成商家扣款协议签约
    /// </summary>
    /// <param name="merchantDeductionAgreement"></param>
    /// <returns></returns>
    Task CompleteAgreementSign(MerchantDeductionAgreement merchantDeductionAgreement);

    /// <summary>
    /// 完成商家扣款协议解约
    /// </summary>
    /// <param name="agreementNo"></param>
    /// <param name="unSigningTime"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    Task CompleteAgreementUnSign(string agreementNo, DateTimeOffset unSigningTime);

    /// <summary>
    /// 更新商家扣款协议的下次扣款时间
    /// </summary>
    /// <param name="agreement">协议</param>
    /// <param name="paymentTime">扣款时间</param>
    /// <returns></returns>
    Task UpdateNextDeductionTime(MerchantDeductionAgreement agreement, DateTimeOffset paymentTime);

    /// <summary>
    /// 获取协议的初始化扣款订单
    /// </summary>
    /// <param name="agreementNo">协议号</param>
    /// <returns></returns>
    Task<Order?> GetFirstDeductionOrderToAgreement(string agreementNo);

    /// <summary>
    /// 获取需要扣款的商家扣款协议
    /// </summary>
    /// <param name="daysInAdvance">提前天数</param>
    /// <param name="debugTime">调试时间</param>
    /// <returns></returns>
    Task<List<MerchantDeductionAgreement>> GetAgreementsForDeduction(int daysInAdvance = 0, DateTimeOffset? debugTime = null);

    /// <summary>
    /// 获取用户的商家扣款协议
    /// </summary>
    /// <returns></returns>
    Task<List<AgreementModel>> GetAgreementsByUser(Guid userId);

    /// <summary>
    /// 获取协议配置
    /// </summary>
    /// <param name="configurationCode">配置编码</param>
    /// <returns></returns>
    Task<MerchantDeductionAgreementConfiguration?> GetConfiguration(string configurationCode);

    /// <summary>
    /// 获取协议带订单信息
    /// </summary>
    /// <param name="agreementNo">协议号</param>
    /// <returns></returns>
    Task<MerchantDeductionAgreement?> GetAgreementAndOrdersByNo(string agreementNo);

    /// <summary>
    /// 检查用户是否签约指定业务的商家扣款协议
    /// </summary>
    /// <param name="userId">用户 Id</param>
    /// <param name="businessType">业务类型</param>
    /// <returns></returns>
    Task<bool> CheckUserBusinessSigned(Guid userId, int businessType);

    ///// <summary>
    ///// 完成通知业务支付成功
    ///// </summary>
    ///// <param name="agreementId"></param>
    ///// <returns></returns>
    //Task FinishNoticeXunYou(Guid agreementId);

    ///// <summary>
    ///// [通知业务支付成功]操作失败
    ///// </summary>
    ///// <param name="agreementId"></param>
    ///// <returns></returns>
    //Task ErrorNoticeXunYou(Guid agreementId);

    /// <summary>
    /// 修改已通知次数
    /// </summary>
    /// <param name="id"></param>
    /// <param name="noticeCount"></param>
    Task UpdateNoticeCount(Guid id, int noticeCount);

    ///// <summary>
    ///// 查询未发送签约成功通知的协议（业务）
    ///// </summary>
    ///// <returns></returns>
    //Task<List<string>> QueryXunYouAgreementForNoticeSign();

    ///// <summary>
    ///// 查询未发送解约成功通知的协议（业务）
    ///// </summary>
    ///// <returns></returns>
    //Task<List<string>> QueryXunYouAgreementForNoticeUnSign();

    /// <summary>
    /// 获取扣款超时的商家扣款协议
    /// </summary>
    /// <returns></returns>
    Task<List<MerchantDeductionAgreement>> GetMerchantAgreementOfDeductionTimeout();
}
