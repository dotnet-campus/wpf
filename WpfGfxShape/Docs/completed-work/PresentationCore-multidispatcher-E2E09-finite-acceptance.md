# PresentationCore 多 Dispatcher E2E-09 有限验收

## 当前身份与交付边界

当前生产 SHA256：`3F291D3E6EF3A74688B8FDCBF124B725DC115D165FAE94DB9B2010BFDCE2570D`。本次集成不新增代码、不扩大测试范围、不尾改源码；使用此前已 build/publish 的 root/service-channel 修复及最新 PublicAbi。旧 `57CE56EA…` async drain 证据是历史，不代表当前生产身份。

`UceChannelExports.cs` 的共享 async connection 允许跨 Dispatcher 创建 channel/disconnect；同步 connection 仍检查创建线程。`TryLifecycleChannel` 仅为有 scheduler 的 root/service channel 放开生命周期线程，符合 MediaSystem 串行共享、最后一个 Dispatcher 可负责关闭的合同。普通 channel 的资源操作、消息等仍经 `TryChannel` 检查所属线程；不是取消所有 channel affinity。当前 root 的 cleanup/提交与 shared service 生命周期修复已由本候选有限场景覆盖，不外推任意并发安全。

## 真实 original / fixed-candidate / compare

证据根为 `artifacts/sandbox-readback/`，下列 JSON 与原始 pixels 保留：

| 证据 | 实际结果与身份 |
|---|---|
| `dispatcher-original/Results/original/dispatcher.json` | passed=true，joined=6；两轮各3 STA；系统10.0.1，原版 SHA `94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F` |
| `dispatcher-fixed-candidate/Results/candidate/dispatcher.json` | passed=true，joined=6；候选 requestedNative SHA 为上述3F291D3E完整身份 |
| `dispatcher-fixed-compare/Results/dispatcher-comparison.json` | passed=true，threshold=0；36帧，每帧4096字节，全字节相等、differingBytes=0 |

两角色 PresentationCore SHA 均为 `605FD26A2D1DF0D81B3DE3CB233365DC6091EC7C72C48B22FB805819C8F57654`，承载 SHA 均为 `6B41608B8E96CE4D1C29B48BF69BEAEA5AB6374E759D1A0A3CF19A6EBD43FA97`。JSON 没有独立 RunID，以上角色路径与沙箱任务身份区分运行，不制造 TRX RunID。原版任务 `5a4be688237b484388815f45aad05557`；候选任务 `0a0b997ce69e460a8d7840494d67749b`。`DispatcherAcceptance.proj` 已读取 `dispatcher-fixed-candidate` 的 ComparisonCandidate，而非初次失败候选。

## 同 SHA full 审计

`dispatcher-fixed-full/Results/dispatcher-fixed-full.trx`：RunID `262f277c-5f57-44d7-80a2-1ece70cba1e0`，2026-10-16T00:45:08.7045858Z 至 00:46:28.9357618Z。

Counters 原值：total=1616，executed=1616，passed=1583，failed=33，notExecuted=0；error/timeout/aborted/inconclusive=0。ResultSummary=Failed，MTP 退出码2。不是旧57CE轮次同分母报告。相同 Results 中 `aot-identity.json` 绑定任务 `679f96a3143245008412c2ddbb159fa1` 的实际 Native DLL，SHA 与本页完整3F291D3E身份一致；TRX PresentCondition 也记录相同生产 SHA。

33项 HW/control 失败保留，不追绿、不全部归环境。例如 hardware cache geometry 原始记录 nativeControl=d3d9.dll、cooperative=0，屏幕实际 `0x00F3F3F3`，期望 `0x00563412`，像素容差断言失败。正常 original/native 可观测呈现成功的前置仍缺；本次不诊断扩修HW，也不声称失败集合已由新解析器全部重新审计。

## 最新 PublicAbi 严格 PE / 有限生命周期：已真实运行并回读

执行已有 `artifacts/publish/WpfGfxShape.PublicAbi/Release/win-x64/WpfGfxShape.PublicAbi.exe candidate`，Windows 沙箱专用执行，非脚本。报告：

`artifacts/sandbox-readback/dispatcher-final-strict/Results/05045b688eca4e9c9ee8b735ed0efe65/report.json`

- RunId=`05045b688eca4e9c9ee8b735ed0efe65`；任务=`aed8aef495a94dc6bc26bb1f25191f51`；Architecture=X64；生产 SHA 与本页完整身份相同。
- HarnessSha256=`A01B8683EBEC331D1F013208F0948019B3C18CBDB58A03EFCEEF0E3593B3CA2D`。
- Missing=`[MilPlayer_Create, MilPlayer_Process]`，Extra=`[]`；PeContractPassed=false，Passed=false；真实 Error=`System.InvalidOperationException: PEExportContractMismatch`（Program.cs:76）。
- LifecyclePassed=true；已有有限 bitmap、同步4轮/异步4轮 channel shutdown/recreate 完成。AllAbiPassed=false，FixedGolden1012Satisfied=false，NativeAotUnloadVerified=false。生命周期通过不覆盖严格PE失败，不代表全部方法语义通过。
- 最新同run真实 golden 引用已用于此次承载；没有用旧报告冒充新跑，没有为缺官方协议的两个 player export 加 stub。

执行记录另有一次误选 bin/win-x64 包的启动尝试，未产生 Results，不能计为有效 strict；随后使用正确 publish 包取得并读回上述实际报告。不是“strict未跑”或“报告待读”。

## 新源码普通 / GenProto 回归：各一次

1. 普通 `Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj`，专用测试工具本轮真实一次：4225 total / 4224 pass / 1 fail / 0 skip，退出码1。失败 `WhenRealSystemMediaHasNoFrameThenSoftwareSampleLeaseCanBeClosed`：加载 `C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\10.0.12\wpfgfx_cor3.dll` 或其依赖时 DllNotFoundException/0x8007007E（SystemVideoSampleTests.cs:32）。日志摘录保存 `artifacts/sandbox-readback/dispatcher-regression-tests.txt`。工具默认未生成TRX、未输出RunID；明确记为未提供，不编造、不借旧ID，也不为补ID重复跑普通回归。
2. GenProto 同样真实一次：34 total / 34 pass / 0 fail / 0 skip，退出码0。原始 `Tests/WpfGfxShape.GenProto.Tests/TestResults/lindexi_DESKTOP-51A5UGG_2026-10-16_08_51_19_net10.0.trx`，RunID=`1ada88cb-92ed-4a0f-9021-737b12ce2a9f`；日志摘录 `artifacts/sandbox-readback/dispatcher-regression-genproto.txt`。

## 验收结论与正式剩余

**Closed（有限）**：本E2E-09 case的两轮3STA、6线程 joined、36帧零阈值相等；共享 async connection/root/service-channel 有限生命周期与普通 channel affinity 保留；同SHA full身份和最新strict真实结果已记录。不是所有 concurrency、callback静止、异常/OOM、长时媒体、全架构/配置的证明，也不宣布完整E2E-09关闭。

**Open**：严格PE仍缺player2；全required/physical与正式ABI、完整HW/cache/shader、七阶段及E2E总条件。历史authority binding、ARM64 toolchain+真实环境、完整10.0.12、官方player protocol/oracle按[唯一外部输入页](../all-completion-external-prerequisites.md)保持Open，无新输入不解除。

DLL始终进程寿命驻留、不卸载/热替换。用户总计划持续，父级可继续委派其它自主正式门；本有限模块交付完成，正式总门 **Red**，不具备全替换资格。最后实际验证后仅同步本文档及导航，未再修改源码。
