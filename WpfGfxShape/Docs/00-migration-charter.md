# wpfgfx C#/.NET Native AOT 迁移章程

## 用户强制纠正：生产禁止原版媒体委托
禁止生产保留或加载任何原版 wpfgfx 提供媒体服务；允许 Windows 公开媒体组件。既有“合法第二wpfgfx”许可及排程已撤销，历史绿不认证独立媒体。先删除链路，再实现独立后端。当前普通媒体创建 E_NOTIMPL，独立媒体全部 Open / NotImplemented；完整替换 Open。见[删除报告](completed-work/replacement-remove-original-media-delegation.md)。

强制执行；适用于 WpfGfxShape 全部调查、设计、实现、测试和交接。
目标源码 `src/Microsoft.DotNet.Wpf/src/WpfGfx` 及直接依赖、生成协议、托管调用方。
生产根 `WpfGfxShape/Code/WpfGfxShape`；主测试根 `WpfGfxShape/Tests/WpfGfxShape.Tests`；文档根 `WpfGfxShape/Docs`。
本页与下列两页共同构成完整章程，不以索引代替正式条款。恢复见[唯一入口](next-session-handoff.md)，运行事实只见[基线](handoffs/current-stable-baseline.md)。

## 1. 目标
[完整条款](charter-implementation-contract.md#1-目标)。
## 2. 约束优先级
[正确性、ABI、可追溯、可验证优先](charter-implementation-contract.md#2-约束优先级)。
## 3. 最强实现约束：逐文件近似直译，禁止提前重构
[逐文件约束与全部禁止项](charter-implementation-contract.md#3-最强实现约束逐文件近似直译禁止提前重构)。
## 4. ABI 与导出兼容原则
[完整 ABI 合同](charter-implementation-contract.md#4-abi-与导出兼容原则)。
## 5. 项目形态原则
[生产/测试项目及 COM 边界](charter-implementation-contract.md#5-项目形态原则)。
## 6. DirectX 与 Silk.NET 原则
[低层互操作、来源与验证](charter-implementation-contract.md#6-directx-与-silknet-原则)。
## 7. 测试优先级与增量门禁
[六类测试与逐级 E2E](charter-implementation-contract.md#7-测试优先级与增量门禁)。
## 8. 工具与证据规则
[禁止终端脚本、专用能力许可及本轮限制](charter-evidence-and-continuity.md#8-工具与证据规则)。
## 9. 文档先行与跨会话协议
[必备文档、调查最低内容与过程要求](charter-evidence-and-continuity.md#9-文档先行与跨会话协议)。
### 9.1 必备文档
[完整清单](charter-evidence-and-continuity.md#91-必备文档)。
### 9.2 专题调查文档最低内容
[完整条件](charter-evidence-and-continuity.md#92-专题调查文档最低内容)。
### 9.3 过程文档要求
[完整条件](charter-evidence-and-continuity.md#93-过程文档要求)。
## 10. 当前交接结构强制格式
[七职责与恢复顺序](charter-evidence-and-continuity.md#10-当前交接结构强制格式)。
## 11. 逐文件迁移台账状态
[全部状态与证据纪律](charter-evidence-and-continuity.md#11-逐文件迁移台账状态)。
## 12. 变更与例外管理
[用户授权与独立优化阶段](charter-evidence-and-continuity.md#12-变更与例外管理)。
## 13. 首版规划交付轮次边界（历史记录）
[仅限首版规划的历史边界](charter-evidence-and-continuity.md#13-首版规划交付轮次边界历史记录)。
