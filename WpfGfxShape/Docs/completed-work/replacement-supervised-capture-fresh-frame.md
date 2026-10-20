# BJ：fresh-frame 捕获证据收口

## 1. 保全、范围与结论

本报告先落盘保全交接结果，再亲读原始产物完成本版。根目录为 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape`，下文相对路径均据此；证据独有根记为 `B = artifacts/bj-capture-fresh-frame/`。

已有新增文件 `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.CaptureFreshFrame.cs` 保留。本次仅补报告，没有重新执行测试、发布、full、脚本或命令行，没有生产修改，也未修改 NativeRun、入口、historical/sampler 及同步阶段诊断，避免与另一代理冲突。

**原生 D3D9 red/blue/red 经 Desktop Duplication 真实交付并通过独立颜色检查。新证据推翻“Microsoft Basic Render Driver 无法产帧”的绝对判断，但不能证明 owner 线程就是 BG 根因：非 owner 原对照在本轮也通过。** x64 同任务 native gate → 官方 PublicHwnd → 候选 PublicHwnd 完成；x86 仅 native gate 的完成 TRX 闭合，PublicHwnd 候选结果仍未闭合。不是全量验收，也不是物理 GPU 或候选内部 backend 已验证的声明。

## 2. 任务、父 TRX、原始计数与退出边界

|任务|本地原始路径（B 下）|亲核结果|
|---|---|---|
|x64 `d664443b1dfd4315a14422dcea75aed8`|`stages-run-x64/TestResults/bj-stages-x64.trx`|411 行 Counters：total/executed/passed=16/16/16；410 行 outcome=Completed|
|x86 gate `27e80704bf4446cd84739dedbb479f5c`|`stages-gate-run-x86/TestResults/bj-final-gate-x86.trx`|404 行 Counters：total/executed/passed=17/17/17；403 行 outcome=Completed，与交接工具报 17/17 一致|
|x86 public `45caf5d0309544e7bdec8fb9e00b5969`|`stages-run-x86/TestResults/` 及 `Results/bj-public-stages.jsonl`|递归列目录仅有 Deploy/In/Out 空目录结构，无完成 TRX；阶段文件仅两条，无 candidate-passed|

两份完成 TRX 的 failed/error/timeout/aborted/inconclusive/passedButRunAborted/notRunnable/notExecuted/disconnected/warning/inProgress/pending 全为 0；Counters 的 completed=0 不覆盖 ResultSummary 的 Completed。x64 TRX Times 为 2026-10-20T02:25:53.3951208Z 至 02:25:56.5103616Z，x86 gate 为 02:26:19.9684701Z 至 02:26:23.1748762Z。

**退出核验限制：本轮可回读的两份 TRX 没有数值进程退出码或 RunInfo 退出记录；独有产物搜索未见 exit 文件，x86 gate 未见 .log 文件。交接的原始执行工具响应不在当前上下文，不能重新亲读其数值退出字段。因此只能确认 TRX Completed 与上述零异常计数，不能把它写成已亲核 ExitCode=0。** x86 public 连完成 TRX 都没有，不能由最后阶段、空 Out、缺 candidate-passed 推断崩溃、超时、正常退出、异常码或退出位置；需另一代理监督诊断补充，不以 gate 的完成结果替代 public 的退出结果。

计数口径：x64 16 个父测试包含一个顺序 public 比较（内部调用两个 owner gate）、两个非 owner 交替帧测试、其余捕获谓词/清理/身份控制；x86 gate 17 个包含两个独立 owner gate、两个非 owner 交替帧测试及相同捕获控制。不是 16/17 个独立硬件业务场景，不合并历史 run/final-run 计数。

## 3. actual 测试身份与 BD 旧候选隔离

已全文亲读三任务的 `Results/bj-fresh-{1,3}.json`（每份 60 行），两份完成任务的 `Results/aot-identity.json`，以及下节模块 JSON。测试路径均为各自 `C:\CodingAgentSandbox\Tasks\<task>\WpfGfxShape.ComAcceptance.dll`；不是仅凭发布目录推断的身份。

|本轮实际测试|SHA256|MVID|
|---|---|---|
|x64 d664…|`7FFDF4F38E97BB2E67ECBFE3219D2D8BA19E7A3B9AA6F2ACDCE653A3498220C7`|`a185491a-3e71-4a5d-88a6-bb93c71db7b9`|
|x86 27e… 与 45c…|`EB9F506B298B5BA11DC477937BCB62E7B33B7CEE75907F0FF2B1094932D46098`|`1dc8a8ca-6e55-42ae-bdaf-6f1aec58cae0`|

三任务 runtime 均为 `.NET 10.0.13`；X64/pointerSize8，X86/pointerSize4。x86 public 和 gate 是相同测试 SHA/MVID、不同任务，结果不得拼接成一次 public 成功。

候选仍为 BD 旧 native 快照，不是新生产发布；本轮新测试 SHA/MVID 不移绑 BD/BG 旧测试。BD 来源对照可见 `Docs/completed-work/replacement-supervised-baseline-curve-hardware-review.md:38–49`：其 BD/BG 旧测试 SHA x64 为 D64A04DE…，x86 为 84190DE3…；该旧报告仅作历史区分，本轮实际身份以上述 JSON 为准。

## 4. 官方/候选模块与 backend 身份

两份完成任务的 aot-identity 指向 task/Native/wpfgfx_cor3.dll，Sandbox=true，Results 位于本任务；参数明确 x64 筛选顺序 public 比较、非 owner 交替及捕获控制；x86 gate 筛选 owner gate、非 owner 交替及捕获控制，不含 public 比较。

|模块|x64 SHA256|x86 SHA256|
|---|---|---|
|官方 task 根/wpfgfx_cor3.dll|`AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`|`5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`|
|BD 候选 task/Native/wpfgfx_cor3.dll|`1C381539E7C82DDD75908C0563F0AC062E583166BDB23AEE066BDD21E838752F`|`E0D3F5EFEDFACFD5CC3491DD43B194E19028E8B541A462123C8F73C73D6E25B5`|

已核 Requested=Actual 的模块原始文件（均 B 下）：

- `stages-run-x64/Results/hw-module-d664443b1dfd4315a14422dcea75aed8-0b694058f70e46198d5b616a14ff9a4b.json`：官方；同任务 `hw-module-Native-bcfe40dcf7714c33966cd7add85b76f6.json`：候选。
- `stages-gate-run-x86/Results/hw-module-27e80704bf4446cd84739dedbb479f5c-3a0173cba171488d8863f8d7b0b5e636.json`：官方身份控制；**不表示 gate 执行了官方 PublicHwnd。** gate 的候选是 aot-identity 路径/哈希记录，不冒称存在同任务 loaded 候选模块文件。
- `stages-run-x86/Results/hw-module-Native-45123c4ce71646f58636ddb3cd9fb78c.json`：45c… public 候选 Requested=Actual；证明该模块身份记录已完成，不证明后续绘制完成。该任务没有独立官方 hw-module JSON，不把 27e… 官方身份记录伪装成 45c… 的记录。

官方 FileVersion=`10,0,1326,50504`，ProductVersion=`10.0.13-servicing.26504.104`，commit=`4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。候选 FileVersion=`1.0.0.0`，ProductVersion=`1.0.0+b4de881638949d7fdbb653f9650fb21fd07bc685`。模块 JSON 的 Runtime/TestAssemblySha256 与上述架构测试一致。

原生捕获链的 TRX 明确：Microsoft Basic Render Driver，vendor=0x1414/device=0x8C，LUID=00000000:00004E71；DISPLAY1=(0,0,1920,1080)，attached=1，rotation=1，outputMonitor=windowMonitor=0x10001；D3D11 readback featureLevel=0xB000，texture1920x1080，rowPitch7680，depthPitch8294400，local/screen=(56,48)。D3D9 device 创建使用 HAL 类型1、flags0x26、32x16、format21、windowed1、swapEffect1/3；这是系统 D3D9 原生 control 加 DXGI duplication 的观察身份，不是物理硬件证明。x64 public 日志有 Ex=False、format21、Caps2=0xE0000000、DevCaps2=0x7F；没有据此宣称候选内部 GPU/backend 已独立核实。

## 5. red/blue/red 完整关键业务 JSON 回读

亲读文件为 B 下 `stages-run-x64/Results/bj-fresh-{1,3}.json`、`stages-gate-run-x86/Results/bj-fresh-{1,3}.json`、`stages-run-x86/Results/bj-fresh-{1,3}.json`，六份各 60 行均读到文件尾。下面完整列出每帧变化的业务时间与 owner；其余字段逐帧核验的共同值列于表后，原始 JSON 保留不改写。

|任务/SwapEffect|帧|submitted|completed|submitThread=owner|
|---|---|---:|---:|---:|
|d664…/1|red|32842305462275|32842306068090|2064|
|d664…/1|blue|32842308049243|32842308329294|2064|
|d664…/1|red|32842310332245|32842310836694|2064|
|d664…/3|red|32842313174779|32842313819001|2064|
|d664…/3|blue|32842315689614|32842316163727|2064|
|d664…/3|red|32842318197968|32842318662449|2064|
|27e…/1|red|32842572042224|32842572552785|1072|
|27e…/1|blue|32842575042018|32842575777418|1072|
|27e…/1|red|32842577522583|32842578130109|1072|
|27e…/3|red|32842580535884|32842580835463|1072|
|27e…/3|blue|32842582425892|32842582894018|1072|
|27e…/3|red|32842585077124|32842585687785|1072|
|45c…/1|red|32841962422688|32841962910275|2368|
|45c…/1|blue|32841964972966|32841965632363|2368|
|45c…/1|red|32841968582804|32841968951711|2368|
|45c…/3|red|32841971371983|32841971628426|2368|
|45c…/3|blue|32841973530457|32841974000908|2368|
|45c…/3|red|32841976042108|32841976341155|2368|

六份顶层 passed=true。18 帧全部 present=0、frontResult=0、accepted=true、failure=null；red 的 expected/backbuffer/duplicated/frontPixel 均4294901760（FFFF0000），clientPixel/screenPixel 均255（COLORREF000000FF）；blue 前四值均4278190335（FF0000FF），clientPixel/screenPixel 均16711680（00FF0000）。submitted/completed 非零递增，18 帧均 owner=submitThread；不是只看时间戳或 Present HRESULT 判通过。

捕获源码走系统 D3D9 Clear/Present、GPU backbuffer、独立 GetPixel(client/screen)、GetFrontBufferData、Desktop Duplication 内容比对；没有用软件 RenderTargetBitmap/RTB 充当捕获输出，没有以候选软件图冒充桌面纹理。TRX 中 pointer-only 初始帧 lastPresent=0 被排除，之后取得 resource 和新鲜 lastPresent 再进行颜色检查。例如 x64 首红 submissionStart=32842305437961，acquire=0，lastPresent=32842307597748，resource 非零，读回 FFFF0000；x86 gate 首红 submissionStart=32842572021099，lastPresent=32842574727933，读回 FFFF0000。时间新鲜只是筛选，颜色及独立观察才是内容依据。

## 6. PublicHwnd 阶段与非 owner 反证

已全文亲读 `B/stages-run-x64/Results/bj-public-stages.jsonl`，线程8708、X64：

1. native-gates-passed-official-start，QPC32842320503140。
2. official-passed-candidate-start，QPC32842322201911。
3. candidate-passed，QPC32842325337092。

同任务父 TRX 第8行顺序比较测试 outcome=Passed。源码顺序为两个 native gate → WhenOriginalPublicHwndTargetDrawsThenWindowContainsHardwarePixels → 加载/记录候选 → RunPublicShaderFrames(40,40,21,false,0,false) → candidate-passed。x64 可据同任务记录及父测试完成确认该有限路径闭合，不扩大为全部 shader/sampler/geometry 或 full。

已全文亲读 `B/stages-run-x86/Results/bj-public-stages.jsonl`，线程9532、X86：native-gates-passed-official-start QPC32841978530226；official-passed-candidate-start QPC32841980219715；**无第三条**。两份 owner JSON 在该任务已完成并 passed，但 public 候选仍仅能认定阶段到达，不能认定通过或故障机制。27e… gate 没有 public 阶段文件，不能补齐45c…的缺口。

非 owner 对照 `WhenNativeGpuAlternatesFramesThenEachDuplicatedPixelMatchesItsSubmission` 在两份完成 TRX 中 swapEffect1/3 均通过，原方法在调用线程直接 Clear/Present，不经 windowThread.Invoke；日志 currentThread x64=8708/owner2064，x86=4736/owner1072。它逐帧 red→blue→red→FF123456 比较 duplication 内容；例如两份 TRX 的 swapEffect3 末段均有蓝、红、原色实际像素匹配。故 owner gate 通过不构成“线程切换修复 BG”的因果证明；同轮非 owner 也能产帧，旧会话状态/捕获时序等仍未隔离，不提出生产线程修复。

## 7. 未覆盖项、交付边界

- x86 public 候选没有完成阶段、父 TRX、进程退出码；崩溃/退出边界待同 exe 监督诊断，不猜测归因。
- 两份完成 TRX 的数值退出码原始工具响应未取得；明确留缺，不把 Completed 偷换成退出码。
- 没有重跑全量、真实物理 GPU、多显示器/旋转/DPI矩阵、设备丢失恢复或完整 sampler/historical；当前验证是本沙箱 Basic Render Driver 的有限帧及 x64 public 路径。
- 未证明 owner 是 BG 根因，未证明候选内部 backend，未把纯控制流负控当真实 native 故障注入。
- BD 旧 native 候选、本轮新测试、旧报告与三个独立 task 分开；未使用旧图/软件 RTB 代替本轮原生捕获证据。
- 本报告为实际落盘交付；本轮只读核验并完成此报告，不覆盖另一代理负责的诊断文件。以上原始 JSON/TRX 路径、计数、阶段、身份与未覆盖项保留供后续追溯。
