# BK x86 ComAcceptance full 中断专项诊断

## 1. 裁决及范围

**BD x86 无过滤 full 仍未完成：无父完成 TRX，最终计数、进程退出码和根因未知。BK 没有修复根因，没有重跑 full，也不把分组通过拼为 full。**

本轮得到新的区分证据：BD 旧包的 historical 组与官方 sampler 单项再次出现执行开始后无完成记录；其中 historical 最后开始的是候选 geometry DataRow，不是官方 sampler。参考组 2/2、位图组 186/186 均完整完成，MTP 日志分别明确会话退出 0。它们证明同包、同接口能够运行和回收完成结果，不证明中断任务的被测进程退出原因。

仅专用文件工具与 Windows 沙箱正式纯 C# ComAcceptance exe；无终端、直接/间接脚本、额外 Host、构建或发布。源码、生产、曲线、捕获/硬件、WindowQ、项目/入口/打包、共享交接文档均未改。没有绑定 BI/BJ 新源码到 BD 旧包，没有关闭并行、skip、改阈值或改变失败上限。唯一新增文档为本报告，产物仅 `artifacts/bk-x86-comacceptance/`。

工作根（以下本地相对路径均据此）：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\`。

## 2. 先读规则与原始现场

亲读最新执行规则、完整替换方案 1–120 全文，以及 BD baseline、BF baseline-review、BH baseline-curve-hardware-review 全文；长输出截断部分另分段补读。只读 NativeRun、historical controls、VisualCache 原生前置与 D3DImageTests 类属性，用于理解角色边界，不以现源码替代旧包执行身份。

BD full 原始远程 task=`8a27a4d36f404ad0ac0dc7fca974a14d`，绝对根 `C:\CodingAgentSandbox\Tasks\8a27a4d36f404ad0ac0dc7fca974a14d\`；本地 `artifacts/bd-comacceptance-baseline/full-x86/`。亲列 TestResults：只有继承的 bd-smoke-x86.trx 和部署目录，没有 bd-full-x86.trx。不得将 smoke 3/3 当 full。原目录未找到 .log；原 full 没有可用父 diagnostic 日志。原报告所述工具快速返回无独立退出/timeout 字段，本轮无法从归档还原工具原响应或进程状态。

原 full 的两个 source-clip 子进程 TRX 已全文亲读：
- `Results/source-clip-reference/668824e52b0441779d14f5c30c256def/Results/Administrator_EN-DEV-01_2026-10-20_00_53_09.4582055.trx`：00:53:09.0418831Z→09.458744Z，Completed、1/1。
- `Results/source-clip-reference/fc8507632e1a46eb8bc3748a8dd08eb8/Results/Administrator_EN-DEV-01_2026-10-20_00_53_10.2203516.trx`：00:53:09.8080302Z→10.2207588Z，Completed、1/1。

这是原 full 内可确认完成的参考角色，不是父完成，也没有显式 OS Process.ExitCode 字段。顶层 aot-identity Arguments 已被第二个 source-clip 子进程覆盖，不能据此称父 full 有 filter。两子角色没有 timeout/aborted/notExecuted，不能把它们倒推成父中断原因。

BD 后续诊断 task=`4379d65efef041088a749b87b7871591`：亲读 `diagnose-x86/Diagnostic/log_261020010009863.diag` 全文。MTP 2.0.1、PID1704、X86/.NET10.0.13、4CPU/2GB；01:00:10.2694958 发现1740，01:00:10.3398702 开始官方 sampler，之后文件终止。无完成/退出/timeout 记录。其 hw-module JSON 明确加载真实官方 SHA，而非候选。这里只能说官方单项相关路径也能出现中断形态。

BF 反证亲读 `official-sampler-x86/TestResults/bf-official-sampler.trx` 的 Times、stdout、堆栈、Counters/退出：task=`9cc902d479174da1b80d352aac1bf241`，01:14:13.7153765Z→15.2711392Z，1执行/0通过/1失败，退出2；堆栈为 AssertCombinedPixel，原生前置 0x00CBAE87 对 0x00563412。bf-diag 125 行也明确会话退出2。这只能证明该次前置断言能正常结束，并未进入更深 sampler ABI，不解释 BD 原 full。

## 3. BK 运行方法与任务绝对路径

所有任务输入均为原 `artifacts/bd-comacceptance-baseline/full-x86/` 完整旧包。未改包，不重发；输出回收整个任务。其旧 Results/TestResults 也随输入继承，因此只认新 task 路径和新指定 bk-parent/bk-diag，不把继承文件当本轮新证据。

先运行同 exe `--help`（沙箱上限60秒，回收 help/），确认该旧 MTP 实际支持 filter、timeout、diagnostic、diagnostic-synchronous-write、report-trx、results-directory；未假定不存在的 crash/hang dump 选项。帮助输出中文编码损坏，但选项名可读。工具未返回 help task ID，不补造。

下面每项远程绝对根均为 `C:\CodingAgentSandbox\Tasks\<task>\`，exe 为该根 `WpfGfxShape.ComAcceptance.exe`；本地绝对根为工作根加 `artifacts/bk-x86-comacceptance/<目录>/`。

|目录|task|受限 filter（FullyQualifiedName~）|MTP/沙箱上限|结果|
|---|---|---|---|---|
|historical|3ce22cc4fdb3475ebe0916969575e83f|WhenHistoricalCapture 或 ReferenceRevalidationEvidenceTests|60s/120s|无父 bk-historical.trx，最终计数/退出未知|
|reference|f9a42232370546589575e346fadb8b65|ReferenceRevalidationEvidenceTests 或 WhenFiniteShapeIntersectsSourceClipThenOriginalCoverageIsRecorded|45s/90s|父 bk-reference.trx，Completed、2/2，会话退出0|
|sampler|4cec9fbb8fec45fdb9ddc79d49fd0f40|WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain|45s/90s|无父 bk-sampler.trx，最终计数/退出未知|
|bitmap|38c93cb08d244cfaaa8204275637466c|BitmapSourceWrapperTests 或 BitmapRenderTargetTests|60s/120s|父 bk-bitmap.trx，Completed、186/186，会话退出0|

共同参数：report-trx、对应 report-trx-filename、results-directory=bk-parent、diagnostic、diagnostic-synchronous-write、diagnostic-output-directory=bk-diag、no-progress；historical/sampler 另 output=Detailed。这是正式 exe 的参数数组，不是命令行终端或脚本。调用顺序为 help→historical→reference→sampler→bitmap；没有并发启动本任务的沙箱调用，未修改测试默认并行语义，不能控制 BI/BJ 或其他系统负载。

## 4. 新日志、父退出与子角色亲读

historical：`bk-diag/log_261020022830902.diag`，PID3716，02:28:30.9083017 开始记录；02:28:31.413 最后记录 Executing test。InProgress 为候选 geometry 第一行 `(True,False,4278255360,65280)`。新 hw-module-Native-8e8d3c8a08e840a3aad341c11eb14966.json 明确实际候选加载。bk-parent 只有部署目录，没有完成 TRX。官方 sampler 在这个组是否执行未知；不能说8项已执行，也不能按首个开始用例认定它导致异常。

reference：`bk-parent/bk-reference.trx` 亲读全文，TestRun=b8b1f932-813d-492e-8c66-999f519213bb，02:28:55.6550618Z→56.2853038Z，2执行/2通过，其余计数0；`bk-diag/log_261020022855419.diag:70–76` 明确 session=5023420e-a2b3-45f4-9d57-6b0a04cc6b96，退出0，0.7089694秒。这里的0来自 MTP 自身会话结束，不冒称沙箱另返回了 OS Process.ExitCode。

sampler：`bk-diag/log_261020022906857.diag`，PID9968，02:29:06.8634749 开始记录；02:29:07.366 最后记录开始官方 sampler。新 module JSON 证实实际官方加载；bk-parent 只有部署目录，无父完成 TRX。没有测试阶段 stdout 留存、异常堆栈或数值退出，不能声称到达原生呈现、sampler ABI 或访问冲突。

bitmap：`bk-parent/bk-bitmap.trx:2267–2270` 亲读 Completed、186执行/186通过，timeout/aborted/notExecuted及其余异常计数均0；`bk-diag/log_261020022934491.diag:2095–2100` 明确 session=f51ca986-7964-4bfc-9646-f1f49f8ac4ec，退出0，7.4989171秒。

bitmap 内新 source-clip 角色（不是继承的 BD 两份）也亲读：`Results/source-clip-reference/fc1b71ecef7b48148ffc81cc532001ec/Results/Administrator_EN-DEV-01_2026-10-20_02_29_36.4774830.trx` 与 `73d4c238e8c441a8bdf47be1a20fb404/Results/Administrator_EN-DEV-01_2026-10-20_02_29_37.2712466.trx` 均 Completed、1/1、timeout/aborted/notExecuted=0；工具输出显示两者先完成，再产生父186项完成。子 TRX 不单列 OS 退出；父成功是它们完成的补充证据，不造子退出码。

两次中断型调用工具均只返回开始 banner、diag 绝对路径和“结果已保存”，没有 IOException、客户端错误或数值退出/超时字段。故本轮没有可确认的工具客户端故障，也没有足够证据排除工具未暴露的异常终止；更不能把“结果已保存”当被测进程正常退出。

## 5. Actual identities 与快照隔离

亲读 reference/Results/az-reference-consumption.json:1–31，及 historical/sampler 新 module JSON 全文。运行时 .NET10.0.13、X86、4字节，MTP2.0.1/MSTest4.0.1。实际任务模块路径均在第3节对应绝对根：候选 Native/wpfgfx_cor3.dll、官方 wpfgfx_cor3.dll、测试 WpfGfxShape.ComAcceptance.dll、PresentationCore.dll。

|身份|SHA256/值|
|---|---|
|候选|E0D3F5EFEDFACFD5CC3491DD43B194E19028E8B541A462123C8F73C73D6E25B5|
|官方|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|
|测试程序集|84190DE3614F5983BAB69CE9CE0F9FB031EC5AB1964D55482461D442E31FC31F|
|测试MVID|91091a82-fd75-4e42-b7f9-bc2939583436|
|PresentationCore|D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D|

官方 FileVersion=10,0,1326,50504，ProductVersion=10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；PresentationCore10.0.1326.50504。候选/测试1.0.0.0，ProductVersion=1.0.0+b4de881638949d7fdbb653f9650fb21fd07bc685，不是当前 dirty 源码凭证。historical Requested=Actual候选，sampler Requested=Actual官方。MVID/WPF来自同包 reference 实跑，不伪称每个中断任务都独立生成完整身份。未把 BF 新测试、BI/BJ 新源码绑定这些结果。

## 6. 四类假设的证据裁决

1. **传输/工具中断：未排除。** 新任务确实推送并执行至 MTP，且新 diag/module 已回收，排除本轮“完全没启动”的推送失败；中断调用没有显式客户端错误。但接口不交付独立被测 ExitCode/TimedOut/终止原因，无法区分父异常退出和外部终止。BF另一次10054客户端失败仅是历史报告事实，不移作BD/BK被测崩溃证据。
2. **父 MTP/本进程退出：未定位。** 新 synchronous diag 仍在测试开始后断尾，排除“仅异步日志队列尾部丢失”作为全部解释；无 completed/timeout/RunInfo/退出日志。reference/bitmap正常证明 MTP/TRX并非对整个旧包普遍失效，不能证明 historical/sampler 父进程正常。
3. **子角色超时/崩溃：没有支持为当前唯一原因。** historical/sampler 方法直接在父测试进程调用 RunHistoricalCaptureControl→RunVisualCacheConsumer，不是 NativeRun.RoleAsync 包装的子进程；原full已识别 source-clip 两角色确实完成，新bitmap两角色也完成。NativeRun.RoleAsync的3分钟超时不能拿来解释这些直接调用。但原full其他后台活动/未归档子角色仍未排除，原完整执行时间线缺失。
4. **并行/资源耦合：未排除，且不是复现的必要前提已获有限支持。** 官方单项在新进程也中断，不需同full其他用例同时运行；historical组首个候选用例也出现相同外观。D3DImageTests源码原有DoNotParallelize，本轮未更改。日志机器4CPU/2GB不是OOM证据；无采样、峰值、dump，不能排除桌面/GPU全局资源、跨任务负载或此前状态。

因此只收窄到“同旧包某些直接本进程硬件路径可产生无完成记录，完成型纯参考/位图控制正常”；不能确定原full最后用例、异常码、出错API、产品/夹具/工具归属。BF前置失败正常退出与BK中断并存，更不能宣称BF已经修好中断或所有HW都只是环境。

## 7. 最小下一动作（建议，未越界实施）

**不再重跑 full。先协调补齐父进程级证据；这是下一步的必要条件，而不是再调整filter碰运气。**

优先让沙箱执行接口交付被测Process.ExitCode、是否超时/被杀、启动/结束UTC及PID、客户端异常与任务异常分列；保留独立stdout/stderr与原始task生命周期响应。接口无这些字段时，不能用MTP无尾日志补造退出。

若必须在现有正式项目内做最小改动，建议协调 `Tests/WpfGfxShape.ComAcceptance/NativeRun.cs`（并核对应包构建版本）：增加显式诊断监督角色，由同exe以UseShellExecute=false启动既有filter子角色，独立记录pid/UTC/退出码/timeout/kill/stdout/stderr，严格有界并收敛进程，不改正常入口运行语义。另在 `D3DImageTests.CaptureHistoricalControls.cs/RunHistoricalCaptureControl` 与 `D3DImageTests.VisualCache.cs/RunVisualCacheConsumer` 的窗口创建、原生device、Present、前置像素、connect/channel、资源提交及finally清理前后写独立同步阶段证据；依据实际异常再考虑系统异常/dump采集。此建议会改变测试SHA，必须单独标记新测试+BD候选，不伪装仍是BD旧测试。不能用catch吞异常、禁并行、skip或软件fallback。

本轮未获授权修改上述入口/共享诊断设施，故没有实施，也没有创建第三工程/外围脚本。下一次只需一个已复现的sampler或候选geometry定向监督运行，取得真实退出与最后阶段后再决定修复边界；只有证明根因并修复后才评估一次合理有界的新x86无filter full。

## 8. 交付结论

实际修复：无。实际完成结果：reference 2/2及bitmap186/186，分别父MTP会话退出0；新子参考两项各1/1。中断结果：historical/sampler无父完成TRX，计数和退出未知。BD原full同样未知，不能宣布full完成、完整替换或关闭HW/恢复合同。

报告仅亲核所列TRX/退出、身份、角色及日志段，不声称逐字审阅所有旧full业务产物或重新计算所有像素。后续须保留工具层与被测层的区分。本任务到证据接口缺口为止，最小下一动作是先取得退出/终止原因与同步阶段，而非第三次重复同外观运行。
