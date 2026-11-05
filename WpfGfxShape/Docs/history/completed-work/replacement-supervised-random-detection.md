# 测试建设线 R：合法随机序列及检测负控

## U 外部重放独立验证（执行前登记）

本节先于运行登记，现补最终结果：经专用文件写入 R 两架构 final-bundle 包根 replacement-r-replay.json，内容取 R 最终 seed104729 的合法 7×8、16步完整序列。复用 R 实际候选归档，不重发生产、不改测试或共享入口。正式 ComAcceptance exe 在 Windows 沙箱独立执行，两架构各3执行/3通过/0失败，非full；未改代码，无需修复。

独有输出：`artifacts/replacement-u-2026/readback/{x64,x86}/`。顶层 `TestResults/replacement-u-replay-<arch>.trx:60` 均直接回读 total/executed/passed=3，failed/error/timeout/aborted/notExecuted=0。筛选 WhenFixedSeedRunsThenEverySoftwareFrameMatchesOfficial；三个 DataRow 实际均导入同一 seed104729，不计作三个不同随机序列。特别回读标为130363的目录 input/identity 仍为104729，证明导入而非默认生成：
- x64 `Results/replacement-r/seed-130363-e051fdc92b1e455799f93910b8257783/`
- x86 `Results/replacement-r/seed-130363-bfdb59c74525480eaf2cbae4f168b7d1/`

六份 input/identity 均检出 Seed104729，六份 comparison 均 Failure=null；两架构上述目录 comparison 直接回读 Frames16/Threshold0。已回读原始base64输出与全0 HRESULT记录，检查独立BGRA逐帧文件归档；实际逐字节及非空颜色断言由正式测试执行，未另编审计器重复计算。每次16帧/角色，共48帧/角色/架构，属于同序列三次重放而非扩大随机覆盖。整个沙箱目录回收，输入/官方/候选/identity/comparison和原始BGRA均保留；本次无失败输入触发缩减，R早期失败/缩减档保留未删。

直接回读身份：.NET10.0.13，官方 FileVersion `10,0,1326,50504 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`，PresentationCore10.0.0.0。
- x64任务7103e386020542889192165f96d5550f，pointer8；官方SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1；候选SHA AF310A91CF68711CCD53D17F89BC1C02A7E339662BFD025AB5B4D6F3907619D5。
- x86任务17ac69bf92ec4eeb8d60c2e672949b35，pointer4；官方SHA 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834；候选SHA 5D212B5B42E661ADCF5A16C90CBE5AD565083ADC97DA8FA074D201B74E91322B。

官方实际包根wpfgfx_cor3.dll、候选Native/wpfgfx_cor3.dll；均为identity记录，未独立重算SHA。这是R/O归档，不绑定T或最后生产源码。R/U官方10.0.13不同于S的10.0.9，不能混同/外推。仍为同进程两个独立模块、离屏软件、零阈值，不是HW或进程隔离。包根重放文件刻意保留，本线两个final-bundle现在是重放包，勿当无重放的默认seed包。

下文“外部导入未单独实跑”为R交付时历史状态，现由本节限定覆盖；广泛合法随机、进程隔离及HW/恢复/媒体/资源缺口均仍存在。

## 结论与交付

本包已完成有界测试建设、最后源码 Windows x64/x86 Release 编译发布及 Windows 沙箱实际执行：**两架构各 9 执行 / 9 通过 / 0 失败 / 0 跳过**。只表示本包离屏软件/同步事件代理检测范围，不是 full 或完整替换结论。没有生产修改，也没有重新发布生产 DLL。

新增文件仅：
- `Tests/WpfGfxShape.ComAcceptance/ReplacementConstrainedSequenceTests.cs`
- `Tests/WpfGfxShape.ComAcceptance/ReplacementDetectionControlTests.cs`
- 本报告（先创建、再更新）。

未改共享生产、窗口、捕获、ABI、入口、项目、golden、skip、阈值。未执行终端、shell、直接或间接脚本；使用专用发布工具编译 C#，以 Windows 沙箱执行正式 ComAcceptance exe。

## 盘点与选择

读取交接入口、执行规则、完整方案、工作项、当前状态/基线、监督汇总、独立审查；检查两个正式项目结构和本地 props/targets/global.json。现成 BitmapRenderTargetTests 提供真实工厂/target/bitmap Clear 与 CopyPixels ABI 示例；EventProxyTests 提供真实描述符复制、回调及最终释放示例；NativeRun/AcceptancePaths 已有沙箱、实际身份和结果目录能力。已有 WPF 状态序列与捕获负控不等于随机输入档、缩减器或回调轨迹检测。

真实缺口是没有本线可直接复用的固定 seed 操作档、保存失败输入、同类失败确定性删除缩减，以及与实际差分/生命周期检测共用函数的输出变异负控。最小补齐这些能力，不抢改共享基础设施。

本包选用 `MILCreateFactory → MILFactoryCreateBitmapRenderTarget(format16, usage1) → MILRenderTargetBitmapGetBitmap → Clear(slot4)/CopyPixels(slot7) → Release`。离屏软件 BitmapTarget，**不是 HW、物理 GPU、窗口呈现、resize 或 device-loss**。官方与候选用各自真实模块/导出创建各自对象，官方无候选转发，原始帧 byte-exact；另用 RGB 饱和色的独立 BGRA 内容断言防止双方空白相等。

边界：本包在同一正式测试进程加载两个路径明确不同的模块，不是官方/候选子进程隔离。现有入口不变；崩溃时进程级 JSON 写完之前的证据仍可能不完整。后续需要故障隔离角色时先协调入口所有权。

## 随机、保存和重放缩减

固定 seeds：`104729`、`130363`、`155921`。显式 uint LCG（1664525/1013904223），避免依赖 System.Random 实现。每 seed 16 步、尺寸 4..16，每四步 full clear，其余为目标内非空整数矩形，颜色 RGB、alpha=1。逐步原始读回，48 帧/角色/架构，共 96 帧/架构主差分。另有同色局部重写回归。

每次独有目录 `Results/replacement-r/<label>-<guid>/`：
- `input.json` 完整 seed、尺寸及全部操作；`identity.json` 实际加载路径/SHA/文件版本/架构/指针宽度/.NET/WPF。
- `official.json`、`candidate.json` 保存全量 base64 原始字节和每次 HRESULT；`official-00.bgra` 等保存逐帧紧密 BGRA（stride=Width*4，无头）。
- `comparison.json` 零阈值、帧数、失败类别。
- 真失败自动保存 `failure-input.json`，从尾部逐项删除非首步操作，只有同类失败才接受，固定点时写 `minimal-input.json`、`shrink-summary.json`。每个 shrink-NNN 保存输入和两角色原始输出，不覆盖原失败。最多 16 步产生有界删除尝试；不做数值/尺寸最小化，不承诺全局最小。

重放：将任一保存的 `input.json`/`minimal-input.json` 内容放入待运行包根的 `replacement-r-replay.json`，通过专用文件写入即可；运行 `WhenFixedSeedRunsThenEverySoftwareFrameMatchesOfficial` 的筛选项。三个 DataRow 都读取同一重放文件（可只筛一个显示名）；新结果仍写新 GUID。删除重放文件须限本线包，正常 seed 运行前确保文件不存在。本包已实跑 seed 生成及自动缩减；**外部重放文件导入分支未单独实跑**。

## 共用检测函数负控

`Detect` 同时服务主差分、真实官方帧变异负控、注入缩减试验。错误颜色和旧帧分别由真实官方输出复制后变异，未经变异输出先通过该检测；负控只变测试副本。`DetectLifecycle` 同时检查真实官方/候选 MILCreateEventProxy 回调输出和轨迹变异。

实际同步回调 payload 为 `17,63,101,209`，最后 Release 的 descriptor.Dispose 记录 `-1`，真实轨迹均为 `[17,63,101,209,-1]`。每个角色注入并拒绝：丢失回调、重复回调、丢失释放、重复释放、释放后回调、回调顺序错乱。每架构 2 种帧负控 + 2 角色×6 种轨迹变异，共 14 个拒绝检查（内含在 MSTest 项中，不能另累加为 14 个测试）。同步 Raise 合同下使用严格 payload/顺序；没有把这种全序外推为异步墙钟时序合同。

缩减负控：真实官方执行 4 步，测试副本在蓝色操作输出注入错误颜色，使用相同 Detect 和 Reduce；两架构均 4 次尝试缩至 2 步（强制初始 red clear + 必需 blue 操作）。每 trial 保存真实官方与变异输出。它证明检测/缩减机制被实际触发，不是生产缺陷注入。

## 实跑、TRX 与身份

统一根：`artifacts/replacement-r-2026/`（ignored，独有；未清理别人产物）。专用发布工具以现有 ComAcceptance 项目、Release、win-x64/win-x86、自包含、ComAcceptanceSandbox=true、对应 AotRuntimeIdentifier 和显式 AotDllPath 发布到 `final-package-<arch>/`，打包目录 `final-bundle-<arch>/`。生产候选直接选用 `artifacts/replacement-window-o-2026/native-<arch>/wpfgfx_cor3.dll`，没有重发生产扰 SHA。项目构建同时覆盖两个新增源文件，最后源码之后两架构均编译发布成功。

Windows 沙箱执行包内 `WpfGfxShape.ComAcceptance.exe`，参数为 `--filter` 配合 `FullyQualifiedName~ReplacementConstrainedSequenceTests|FullyQualifiedName~ReplacementDetectionControlTests`，并启用 TRX。首次使用不支持的 --filter-class 被拒绝，未执行测试，证据保留 `readback/x64/`，随后使用现有支持的 --filter；非测试失败未当产品失败。

最终直接回读：

|架构|顶层 TRX|Counters（第96行）|
|---|---|---|
|x64 Release|`readback/x64-v2/TestResults/replacement-r-x64.trx`|9/9 passed，failed/error/timeout/aborted/notExecuted 全0|
|x86 Release|`readback/x86-v2/TestResults/replacement-r-x86.trx`|9/9 passed，failed/error/timeout/aborted/notExecuted 全0|

两架构：.NET 10.0.13；官方 `10,0,1326,50504 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`，PresentationCore assembly 10.0.0.0；不能外推其他 servicing。

|架构|候选 SHA256|官方 SHA256|
|---|---|---|
|x64 pointer8|AF310A91CF68711CCD53D17F89BC1C02A7E339662BFD025AB5B4D6F3907619D5|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|
|x86 pointer4|5D212B5B42E661ADCF5A16C90CBE5AD565083ADC97DA8FA074D201B74E91322B|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|

实际 x64 官方为沙箱任务 `e8eb0e55b5144b46a7cd1d590e9f9e70/wpfgfx_cor3.dll`，候选同任务 `Native/wpfgfx_cor3.dll`；x86 任务 `9aa87d1d6baf4b428084745b3971ee8e`，相同相对路径。

原始业务根（均在 readback 对应架构的 Results/replacement-r 下）：
- x64 seed104729：`seed-104729-551f2ff7f74c456193c4bb5cb1e72ffb/`；其 identity/candidate 原始输出已回读。另 seeds `seed-130363-529342a67387456b956c3207b47dcca7/`、`seed-155921-c387686458eb4a578d245d6685af7115/`。
- x86 seed104729：`seed-104729-a62c787bf38d4939a792b53021db5a86/`，identity 已回读；另 `seed-130363-60aa9ea41b9f46a28b420ed7ed1b5e7a/`、`seed-155921-042269549f844ef0bc5278741d419728/`。
- 缩减控制 x64 `shrink-control-0139102a9e1d4e9980cacdb253b728f3/`，x86 `shrink-control-ef99875af8c4460f93decff25df3071a/`，summary 均直接回读 4 attempts/2 remaining。
- 真实候选轨迹 x64 `callback-True-6ccf9ff2162c4e8c82a19834f0ae4ef0/actual.json` 已回读。两架构各个 wrong-color/old-frame/callback*/legal-no-change 目录保存原始和变异证据。

## 首次失败、根因与最小复现

首次源码两架构各 8 执行/6通过/2失败，原始 TRX/输入/输出/缩减均保留在 `readback/x64-final/` 和 `readback/x86-final/`。失败 seeds104729、130363 报 stale-frame，但官方和候选字节相等；这是**本线检测夹具误报，不是候选缺陷**。

根因：相邻操作颜色不同，并不代表本次局部区域先前像素与本次颜色不同。最小可解释合法复现：2×1 full red → 左像素 green → 右像素 red；最后整帧不变完全正确。新增真实两模块回归 `WhenClipRewritesExistingColorThenUnchangedFrameIsLegal`；检测先检查被写位置的先前官方像素是否确需改变，再检查帧更新，独立内容及零阈值比较未减弱。不是替换 seed 追 Green。修复后相同三个 seed 全部通过，两架构最后源码重建实跑。

首次 seed104729 的自动缩减从16到4，34次尝试；保留了 `minimal-input.json`，该结果只对应当时错误的 stale-frame 检测器，不能当产品最小缺陷或最终检测语义证据。最终注入控制另验证了缩减器的有效性。

## 未覆盖与下一步

本包没有发现新生产差异；不关闭历史1字节、窗口、捕获、x86 BitmapRealizer、VBlank 问题。不覆盖真实 HW/backend、窗口状态机/捕获、device-loss/resize、媒体、异步回调竞争、资源趋势、任意几何/shader/glyph 组合、格式转换或数值缩减。没有运行两个正式项目全量，不拼接为full；测试构建新鲜，候选明确为 O 已有产物，不冒称最后生产源码。

后续有界扩展：先实际运行外部 JSON 重放分支；协调既有角色入口支持官方/候选进程隔离及阶段持久化；扩展一种已知合法几何/资源更新操作并保持完整输入档/同类失败缩减；HW 与真正异步生命周期检测交相应所有者，不用本包软件或同步负控代验收。
