# 唯一当前基线：有限成果可复用，正式总门仍 Red

## 合同

Windows x86/x64 全配置正式门保留；ARM64 全部验收为 NotApplicable-UserScopeDecision，非 Passed（DEC-0005）。DLL 进程驻留，无 unload/reload/热替换需求（DEC-0004）。
采用已安装官方完整 runtime，同版同架构独立原版/候选对照（[DEC-0006](../decisions/DEC-0006-installed-official-runtime-contract.md)）；10.0.12 只属历史采集身份，不再待澄清或索取。
本轮仅过程清理入口编译/实际调用与 Docs 收口，没有生产发布或全量测试；正式生产与 Ledger 保留。清理事实、最小保留输入及删除路径历史解释见[交付记录](../completed-work/process-cleanup.md)。

## 生产与真实回归身份

生产 SHA256：`3F291D3E6EF3A74688B8FDCBF124B725DC115D165FAE94DB9B2010BFDCE2570D`。
下列路径相对于 `WpfGfxShape/artifacts/sandbox-readback/`，不同 run 不混绑。

| 证据 | 当前结论与边界 |
|---|---|
| `dispatcher-fixed-full/Results/dispatcher-fixed-full.trx` | RunID `262f277c-5f57-44d7-80a2-1ece70cba1e0`；1616 total / 1583 pass / 33 fail / 0 skip，失败退出2；不是最新 startup smoke 的 full |
| `dispatcher-final-strict/Results/05045b688eca4e9c9ee8b735ed0efe65/report.json` | 同上述生产 SHA；Missing=`MilPlayer_Create, MilPlayer_Process`，Extra=0；严格 PE/总体失败，有限 LifecyclePassed=true，不等于全 ABI |
| `nativecaller-routing-smoke/TestResults/nativecaller-routing-smoke.trx` | 最新入口启动 smoke，RunID `123659bb-331c-4529-bca2-39cd9346085a`，40/40；只证明路径/身份/EventProxy 选定范围，不是 full/E2E 重跑 |
| `dispatcher-fixed-compare/Results/dispatcher-comparison.json` | 两轮各3 STA、36帧 threshold=0 零差，两角色各 joined=6；仅该有限 E2E-09 |

本次只读审计回读 full 的 RunID/Counters/失败退出码、strict 的 SHA/Missing/Extra/LifecyclePassed 与 smoke RunID/Counters；33 类归并沿用现行矩阵的既有审计，未重新逐条验证全部失败或机器解析 full。比较 JSON 只抽读，不据此声称重新核验全部帧或退出事件。所核材料未发现替代本基线的更晚完整运行；不据目录名“final/latest”推断新结果。核验定位见[本轮记录](../documentation-readonly-audit.md)。
生产身份与有限场景链亦见[多 Dispatcher 完成档](../completed-work/PresentationCore-multidispatcher-E2E09-finite-acceptance.md)。

## 可复用有限成果

- primitive、Image10、nested16、Blur14、Shadow28、媒体4帧与既有 owning/async drain 等有限 Closed 保持，按各完成档原身份使用，不都绑定当前生产 SHA。
- 原版/候选多 Dispatcher 上述有限场景已闭，不重领相同场景；软件成功不覆盖持续 HW、全格式、失败 refs/callback 和全架构配置。
- NativeCaller 调试 CRT 截图是历史入口诊断。现行 MTP smoke 已 Green；旧 caller 缺 EXE/项目不列生产必需输入。详见[既有报告](../history/nativecaller-startup.md)。

## GenProto 与 C5

最后保留源码 GenProto full **46/46**，RunID `f1dd9225-fc28-441f-954a-6f43b388b047`，TRX `Tests/WpfGfxShape.GenProto.Tests/TestResults/lindexi_DESKTOP-51A5UGG_2026-10-16_16_08_57_net10.0.trx`；本轮完整 XML 解析 counters 与全部46项结果，全 Passed，未重跑。旧12_22_49身份属于此前历史。
后续 C5 审计 build/run 与 17,377 行完整回读比较是独立审计，不是新的 testfull。审计程序集 SHA `F2590E8CFABD36E4DCE7844CC9467379D2460F430A984D98B4A8AF4D285382FD`；原报告见[唯一详档](../completed-work/GenProto-C5-final-adjudication.md)。
正式 6 physical+3 evidence；333 保守风险不是必须清零的新增 required，319 未知保留原表。原风险表 NA=0 不是架构适用性裁决。
selected v143/14.44.35207/SDK26100：12 配置16必要 TU 预处理已真实完成；不是完整对象编译/链接。bilinearspan 四个 x86/x64 配置 GetTargetPath 为 MSB3202，缺原项目来源。
C1–C4/D1–D3、再生378/275 byte-exact/所列378等价、consumer/factory、x86 CLR/Shared 有限成果保持；D4 历史 binding 与 C5 完整实际 physical-record 闭包未满。

## 当前未满

C5/两个required/parent **Blocked**，stage4Closed=false，总门 **Red**；原ARM64字段由[Ledger范围覆盖](../../Ledger/current-user-scope-overrides.json)显式登记NotApplicable-UserScopeDecision，不改Satisfied。
普通历史 4224/1/4225 的旧 runtime 绝对路径加载失败现归自主测试工程修复，不再外部等待；本轮未修代码，见[版本来源](../windowsdesktop-version-provenance.md)。
全项目仍有自主实现/验收，见[唯一九包](../remaining-gap-closure-plan.md)与[最终矩阵](../final-acceptance-remaining-matrix.md)。文档完成不代表生产推进或正式通过。
