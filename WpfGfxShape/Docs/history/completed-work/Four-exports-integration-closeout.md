# 四项最终集成收口：已测，但正式门 Red

## 本次边界与身份

不新增生产能力，不修改生产、player/HWND、共享测试、strict required/raw/阈值/golden。最后已有解绑/重绑定测试修改已重新发布并分别实际调用 candidate/reference；验证后没有再改生产或测试源码。仅新增独立只读证据审计程序及文档。生产没有重新 publish，使用已发布 DLL。

- 候选生产 SHA：`627F273204BF6C247D6B3F09055AFA71ECAE1F5FE1D713D572BCB194EB985A35`。
- 独立原版 WindowsDesktop 10.0.1 SHA：`94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`；不替代完整 10.0.12。
- PublicAbi 测试程序集 SHA：`62EF6376D6F3E6C4146404D363BD46C44F6EBE0299A09A980E3BCAE44A3E17D4`。

## 原始证据与完整解析

全部相对路径以 `WpfGfxShape/Investigations/Projection-glyph-perf/` 为根。

| 证据 | 实读结果 |
|---|---|
| `closeout-candidate/Results/e9d7de62e1734e549a790ef9ed6ee5ae/report.json` | 同指定 SHA，1207 Calls 完整 JSON 解析；Missing6/Extra0，LifecyclePassed=true，Passed/PeContractPassed/AllAbiPassed=false |
| `closeout-reference/Results/8ac06ec2475942d4be9d1eeb2ef30340/report.json` | 独立原版 SHA，934 Calls 完整解析；perf flags2 的 Passed=false，不能被顶层 Passed=true 掩盖 |
| `closeout-full-trx/Results/closeout-full.trx` | RunID `b34c33a6-1a11-4ba6-a15e-37cfd9fb8778`；1429执行/1397通过/32失败/0未执行 |
| `closeout-full-trx/Results/aot-identity.json` | 本次沙箱 e7b839c7…，Sandbox=true，DLL 同指定 SHA |
| `closeout-audit/Results/audit.json` | 独立程序完整读取两份 JSON、TRX、identity，逐项统计、32项失败名单、原始文件 SHA；SameFullIdentity=true |

原始文件哈希分别为 candidate `B8C6A0DF83972F4C48AA973605C9802DC89010D446B6AB539D19498CA2AC3A27`、reference `83C0354A473BB6781C395A402ACFFE464B504425DB93632389521A21EF0C36C4`、TRX `598ADFB6B4A78440118D175C86D98A448DB4D9044B2C0FC9CA17AD08482AE2C5`、identity `FA5C25AAFE738C969CE832EA967BA563BB034796FAF4FF173E5B4B4DEC158F38`。审计不是替代原始证据，也没有改写报告。

Missing6逐名实读：MilPlayer_Create、MilPlayer_Process、MilVisualTarget_AttachToHwnd、MilVisualTarget_DetachFromHwnd、MilContent_AttachToHwnd、MilContent_DetachFromHwnd。这是已测 DLL 的集合，不是另一代理正在实现的工作树状态；player2/HWND4实施中，本阶段未发布，不预判全ABI。

## 四项裁决

- [3D bounds](Four-3D-bounds-closeout.md)：24/24位级比较、null1/1有限 Green，不授完整ABI资格。
- [glyph outline](Four-glyph-outline-closeout.md)：raw22/30，定义字段独立诊断30/30；ForcePacking未初始化字节的验收裁决待定，strict保持原样。
- [glyph release](Four-glyph-release-closeout.md)：10次非空释放+30次null释放 HRESULT=0，有限通过；不是全部寿命/OOM证明。
- [perf](Four-perf-closeout.md)：真实同调用候选0/1/0、原版0/0/0；触发分支未证明一致，Open，不无限扩测或改生产猜修。

本 SHA tile192 的报告 Differences=[]，有限 Green。历史几何12 Green与历史tile192 SHA9D24…及currentstage不同SHA全量分开保存，不把历史12/12拼成此SHA定向执行。当前 COM1397/32/1429来自本次同SHA全量；32项像素失败完整保留，不能一概归环境。

## 发布与运行记录

1. 测试包发布成功：`dotnet publish WpfGfxShape/Tests/WpfGfxShape.PublicAbi/WpfGfxShape.PublicAbi.csproj -c Release -r win-x64 --self-contained true -o WpfGfxShape/Investigations/Projection-glyph-perf/closeout-package`。net10.0-windows/Release/win-x64，携带已发布候选DLL，不发布生产。沙箱分别执行 `WpfGfxShape.PublicAbi.exe candidate` 与 `reference`，输出上述两个RunID。
2. 使用既有 `final-full/WpfGfxShape.ComAcceptance.exe` 全量执行。首次TRX默认落TestResults、仅Results回收未含TRX，不能冒称已回收；随后实际重跑 `--results-directory Results --report-trx --report-trx-filename closeout-full.trx`，同SHA取得并实读上述TRX/identity。两次均1397/32/1429。
3. 只读审计发布成功：`dotnet publish WpfGfxShape/Investigations/Projection-glyph-perf/CloseoutAudit/CloseoutAudit.csproj -c Release -r win-x64 --self-contained true -o WpfGfxShape/Investigations/Projection-glyph-perf/closeout-audit-package`；沙箱执行 CloseoutAudit.exe，完整解析并回读audit.json。无生产引用、无比较规则修改。

仅指定Results回收，原始JSON/TRX/identity保全；回收服务实际包含额外依赖副本，不据参数谎称目录已瘦身或释放空间。本次未删除文件，无终端脚本。

阶段4/5/6/7、A/B/C、ABI/E2E、x86 Shared/unload仍Open，无替换资格。[三类外部输入](../all-completion-external-prerequisites.md)：historical input authority、真实ARM64工具链/机器、合法完整WindowsDesktop10.0.12确未收到，仍需用户提供；不宣布全部完成。
