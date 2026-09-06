## Steam 相关库
- Stm.Shared ```Steam 相关功能静态函数，由调用方管理状态，例如 Web 代理池、账号的登录 Cookies、等```
- Stm.Models.Shared ```Steam 相关模型类，包含 WebApi、PInvoke、Protobuf 等```

### 文件夹结构
- Converters ```自定义 JSON 序列化转换器```
- PInvoke ```本机库结构、类型```
- Protobuf ```.proto 模型类```
- WebApi 
  - Generals ```通用模型类```
  - Econs ```经济服务``` https://steamapi.xpaw.me/#IEconService
    - Markets ```经济市场服务``` https://steamapi.xpaw.me/IEconMarketService
    - SteamEconomies ```Steam 的经济（用户的饰品/资产）服务```
  - Inventories ```用户库存```
  - SteamTrades ```交易服务```
  - SteamUsers ```用户服务``` https://steamapi.xpaw.me/#ISteamUser