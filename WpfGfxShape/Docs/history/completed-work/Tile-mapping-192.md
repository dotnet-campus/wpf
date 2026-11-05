# Tile mapping 完整方法族：实现回读与证据待补

## 状态与边界

本次仅Docs读写，无生产/测试/配置修改、终端脚本、构建、测试或发布。用户交付摘要：tilemapping192精确差分Green，候选SHA `9D24EDD523B01B636140676F1049239F44DF30E38F2D27A853A1B06B8BE376A2`；PublicAbi Missing10/Extra0，LifecyclePassed=true；full1397/32/1429，related1003/32/1035。这些数字当前仅为交付摘要，不冒称本执行者已实读；正式总门仍Red，无替换资格。

## 方法族与修复内容

回读 `Code/WpfGfxShape/Abi/TileMappingExports.cs`：`MilUtility_GetTileBrushMapping` 处理stretch、X/Y alignment、viewport/viewbox units、shape/content、relative及absolute transform。

- units值1分别相对shape/content绝对化，修改对应viewport/viewbox；空bounds或空value生成精确空rect `(0,0,-1,-1)`，空viewport/viewbox置empty并保留未写的结果矩阵。
- mapping先作Identity×mapping；relative围绕shape换算，随后乘absolute transform，最终mapping×brush。标量矩阵乘法按 `(a0*b0+a2*b2)+(a1*b1+a3*b3)` 求值，保留原生float舍入顺序，不能用SIMD融合或代数等价替换来假设精确一致。
- 回读 `Tests/WpfGfxShape.PublicAbi/TileMappingContract.cs`：192组含units/stretch/alignment、null transform、零/负/NaN/Infinity边界；记录rect的double位及矩阵float位，使用独立reference逐项比较。本次未修改比较器、阈值、golden或测试。

## 原始证据恢复门

用户指定以下工作区相对目录；本次目录检索未找到 `artifacts`，直接读取首目录亦返回不存在，SHA全文检索无命中。

| 指定目录 | 待实读内容 |
|---|---|
| `artifacts/sandbox-readback/tile192-native-final` | 候选JSON、strict完整集合、192精确差分及identity |
| `artifacts/sandbox-readback/tile192-independent-original` | 独立原版JSON及完整DLL身份，不用候选作oracle |
| `artifacts/sandbox-readback/tile192-final-full` | full TRX、JSON及候选identity |
| `artifacts/sandbox-readback/tile192-final-related` | related TRX、JSON及候选identity |

已实读历史[系统策略族档](System-policy-validation-20261014.md)的原版身份为WindowsDesktop10.0.1，SHA `94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`；不能据此宣称tile独立原版身份已核验，也不替代合法完整10.0.12。本档不填写猜测RunID，不拼历史产物验证新SHA。

## Closed / Open

- 实现内容已回读；tile192 Green仅记录交付状态，证据缺失时不自行授予完整方法族Closed。
- [Missing10新清单入口](../investigations/tile192-publicabi-missing10.md)待原始报告逐名回读，禁止由旧11减1生成；旧11/19历史保留。
- 另一代理进行3D bounds、glyph outline/release、perf四项；均Active，不预标Green或减Missing。player2、HWND4及其它自主合同继续Open。
- full32失败、七阶段正式门、A/B/C、PresentationCore E2E与架构/unload不被局部Green关闭；[总计划](../remaining-gap-closure-plan.md)及[三类外部输入](../all-completion-external-prerequisites.md)保留权威条件。
