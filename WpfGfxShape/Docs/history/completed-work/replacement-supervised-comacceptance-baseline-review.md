# BF 独立核验 BD 与阻塞诊断

## 结论与实际核验边界

完整替换未完成。BD x64 父 full 仍为 **1740 执行、1699 通过、41 失败、退出 2**；亲读 `artifacts/bd-comacceptance-baseline/full-x64/TestResults/bd-full-x64.trx` 13613–13621。BF 没有重跑 full，没有把修复后的定向通过从 41 中减去并伪造新 full。BD x86 `full-x86/TestResults` 只有部署目录与继承的 `bd-smoke-x86.trx`，没有父 `bd-full-x86.trx`，最终计数和退出码仍未知。

已读当前执行规则、完整替换方案、BD 报告、正式项目和构建隔离/打包配置。独立亲读上述父 Counters/退出、x64 实际身份、BD x86 中断诊断末段、新运行父 TRX、角色实际模块、WindowQ 原始帧审计、角色 report 和事件片段。没有人工逐字检查所有 1740 项及全部二进制；没有将 BD 报告中的所有分类当成本轮独立重算结果。

唯一源码修改：`Tests/WpfGfxShape.ComAcceptance/WindowQEvidenceTests.cs`。未改生产、曲线、项目、入口、共享文档、参考/golden/threshold/skip；未覆盖 BE 文件、公共包或他人产物。新增本报告，独有 ignored 产物根 `artifacts/bf-comacceptance-review/`。全程专用文件/发布工具和 Windows 沙箱正式纯 C# ComAcceptance exe；未执行任何脚本或终端命令。下文路径相对工作根 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\`。

## BD 与 BF 的实际身份

BD 原包诊断保留旧身份，不绑定当前生产源码（包括并行 BE 修改）：

| 项目 | x86 | x64 |
|---|---|---|
| BD 候选 SHA256 | E0D3F5EFEDFACFD5CC3491DD43B194E19028E8B541A462123C8F73C73D6E25B5 | 1C381539E7C82DDD75908C0563F0AC062E583166BDB23AEE066BDD21E838752F |
| BD 测试 SHA256 | 84190DE3614F5983BAB69CE9CE0F9FB031EC5AB1964D55482461D442E31FC31F | D64A04DE7F85C63176B16C18AAFC4B470FB2D4E8BD573B58BB939A42237B6FE4 |
| 官方 SHA256 | 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834 | AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1 |
| BF 新测试 SHA256 | 198801A86173E60291EEBAB2B9B10CF6F1A8D312CFA1875586172A05D9392FD4 | E49F32A612B2E58B3E264C0996A6D7748EAC9FDC1C4FBC9F50877CDC13B41121 |

x64 BD 身份亲读 `full-x64/Results/az-reference-consumption.json`：.NET 10.0.13、X64/8，测试 MVID ce5bf492-6283-4571-8ec8-563c6f1564c6。x86 旧测试 SHA/官方/候选来自 BF 新任务实际 module stdout；不是单凭 BD 表格。BF 新测试身份来自双架构 rectangle 角色 `startup-identity.json`，实际加载模块来自 `report.json/loadedNative/resolvedNative`。候选均任务根 `Native/wpfgfx_cor3.dll`；官方均任务根 `wpfgfx_cor3.dll`，不是候选转发。

官方版本 `10,0,1326,50504` / `10.0.13-servicing.26504.104`，commit 4099f83977f4d29604a9aeb971fb0a7bc1d5f699；PresentationCore 10.0.1326.50504，SHA x86 D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D、x64 0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31。候选版本 1.0.0.0/ProductVersion 1.0.0+b4de881638949d7fdbb653f9650fb21fd07bc685；版本不是 dirty 源码凭证。

只重新发布修改后的正式测试，不重发生产扰 hash。两个 Release 自包含发布均成功，RuntimeFrameworkVersion=10.0.13，ComAcceptanceSandbox=true，分别 win-x86/win-x64，AotRuntimeIdentifier 匹配，AotDllPath 显式引用 BD 对应 `x86/native`、`x64/native`；正常依赖图、未 NoBuild。独有 WpfGfxShapeArtifactsRoot 分别 `artifacts/bf-comacceptance-review/{x86,x64}/`，输出 `publish` 和既有 target 生成 `com-acceptance-package`。这证明新测试 + BD 候选快照，不证明新测试 + 最新生产源码。

顶层 aot-identity 会被同 exe 子角色覆盖；本轮两个 WindowQ 文件 Arguments 都是 candidate 子角色，不冒充父过滤参数。父过滤范围由专用调用与父 TRX 绑定。

## 优先 x86 有界诊断

全部复用 BD `full-x86` 正式 exe，新增 `--diagnostic` 原始日志写各任务 `bf-diag/`，单次上限 90 秒，不重跑 full。

| 本地 BF 子目录 | task | 角色/结果 |
|---|---|---|
| official-sampler-x86 | 9cc902d479174da1b80d352aac1bf241 | 官方 sampler，1 执行/0 通过/1 失败，父退出 2 |
| official-geometry-x86 | 25b0a53bbc2c4b0695cc22c00d322180 | 官方 geometry，1/0/1，父退出 2 |
| candidate-control-x86 | fd4353a2a02b460facfa12237a36fe2a | 候选 historical 五个 DataRow + 官方 PublicHwnd，6/0/6，父退出 2 |

各目录完整回收，父 TRX 分别 `TestResults/bf-official-sampler.trx`、`bf-official-geometry.trx`、`bf-candidate-control.trx`。Counters timeout/aborted/notExecuted 均 0。开始与完成：sampler 父 Times 2026-10-20T01:14:13.7153765Z→01:14:15.2711392Z，用例 01:14:13.9647323+00:00→01:14:15.2120295+00:00；geometry 父 01:14:19.4434838Z→01:14:21.0131218Z，用例 01:14:19.7031834+00:00→01:14:20.9529554+00:00。候选组工具报告 6.280 秒并产生父完整 TRX。没有本轮超时或原生崩溃证据。

sampler 原始日志 `bf-diag/log_261020011413502.diag`、geometry `log_261020011419227.diag`、候选 `log_261020011436924.diag`；TRX 保存完整 stdout/堆栈/实际 module SHA。官方 Requested=Actual 官方路径/SHA，候选 Requested=Actual Native 路径/SHA，不用 PresentCondition 中固定标为 production 的候选路径误判官方角色。

三组均在 **原生 d3d9 前置呈现读屏**失败：0x00CBAE87 对 0x00563412。源码 `D3DImageTests.VisualCache.cs/RunVisualCacheConsumer` 依次 CreateDevice→PresentNativeControl→DiagnoseNativePresent→AssertCombinedPixel，之后才 connect/channelCreate/资源消费。因此官方 sampler 本次并没有到达官方 sampler ABI 业务路径；本次正常结束不能证明该 ABI 安全，也不能证明 BD 中断已修复。

BD 诊断 `diagnose-x86/Diagnostic/log_261020010009863.diag` 35–51 独立读到：01:00:10.2694958 发现 1740，01:00:10.3398702 开始官方 sampler，之后文件结束。无完成、退出码、dump、可证实 timeout。BF 本次有完成，故排除了“每次独立官方 sampler 必然立即中断”的断言，**没有确定 BD full 中断根因**。环境呈现前置阻塞与原先可能更深路径的异常不能混为一谈；不得按末用例断唯一根因，不声称候选崩溃。下一步应在正常呈现前置可用时复用该官方单项有界重现，再以实际阶段/异常证据决定是否修 ABI；本轮不改入口/共享隔离基础设施。

追加一次 x64 原包官方 PublicHwnd + 候选 PublicHwnd 定向，上限90秒，回收目标 `hwnd-x64`，但 **推送阶段**专用工具报 IOException/SocketException10054、客户端退出 -1073740791，没有测试 task/TRX。该退出属于传输客户端，不是被测 exe，不计执行/失败/崩溃，也不能反推 BD x86 中断原因。未无限重试。

## WindowQ：明确夹具根因及最小修复

BD 父 TRX 3717–3728 的失败确切是 `candidate.Length == 24 * 16 * 4 && (red || blue)`，文件 `frame-0-2-40x24.pbgra32`；前面的 exact 断言已通过。独立读 `ReplacementStateSequenceWindowTests.cs` 222–255：序列红、蓝、Lime、黄；frame2 请求40×24，随后文件名记录真实 width/height，缓冲区 width*height*4。故原夹具同时有固定尺寸与只允许红/蓝两项错误，不能只修长度再让合法绿/黄失败。

仅 WindowQ 文件增加私有 `IsExpectedFrame`，解析帧号/尺寸，强制 frame0/1=24×16、frame2/3=40×24，完整长度及对应每像素 BGRA（包含 alpha255）严格核验；官方逐字节 exact 保留。没有任意尺寸接受、降低内容断言或改阈值。原审计 JSON 的 red/blue 仍只是诊断字段，绿/黄两者 false 正常，不代表拒绝。

增加4项颜色/尺寸正例，5项负控（旧红帧、截短、旧尺寸、空缓冲区、非4字节完整长度）。负控通过同一私有生产审计路径，不代表生产渲染故障注入。审计仍不把部分历史输出称完成序列；本轮另运行正式 rectangle producer 验证完整序列。

双架构最后源码修改后，正式新包父 filter 为 `WhenWindowSolidRectangleUpdatesAndResizesThenOfficialContentMatches|WindowQEvidenceTests`：

| 架构 | task | 父结果/时长 | 本地路径 |
|---|---|---|---|
| x86 | 34655e43b25640a1ae7ecdd81e1e9abd | Completed，11执行/11通过/0失败，18.288秒 | windowq-x86/TestResults/bf-windowq-x86.trx |
| x64 | df79305a822b44e3917110338d6030aa | Completed，11/11/0，18.012秒 | windowq-x64/TestResults/bf-windowq-x64.trx |

父 TRX 83–85 亲读，timeout/aborted/notExecuted等0；成功完成，工具未单列数值父退出码，不能伪称亲读父 Process.ExitCode=0。双方子角色 report 明确 exitCode=0、dispatcherShutdown=true，实际加载官方/候选 SHA如上。11=1完整rectangle差分+1审计+4正例+5负控；不是 full。

角色业务根：x86 `windowq-x86/Results/replacement-rectangle-window/cdfa097f2b30403096f74b3ee8200c9c/`；x64 `windowq-x64/Results/replacement-rectangle-window/1e1aa0788a5f40da9be514032b5a0a83/`，均含 original/candidate 的 report/events/resources/startup-identity、12帧原始pbgra32及source输入。审计 `audit-t/q-raw-frames.json`：1536字节红/蓝，3840字节绿/黄，exact=true；绿双方 SHA F3C4DE4999A80CA53BEFE2296BC674E69383507F2C26B7340A82D0C517112961，黄双方 SHA 9E95E57579D1BA517680918283994F852142C2CCE856F0A57F7EC8BC79FFD443，双架构一致。原始字节由正式审计完整读取比较，不只比hash。

亲读候选 events：24×16→40×24，真实 ActualWidth/Height同步增长，色序更新，close-drained closed1且callbacksAtUnsubscribe等于callbacks。业务明确 backend=RenderTargetBitmap software consumer，HW pixels NOT verified、deviceLoss not triggered；这不是37项硬件/真实恢复关闭证据。

## 37项硬件/呈现/恢复的裁决边界

BD 分组15 stage4 +7 historical +11 PublicHwnd +4 desktop/resize/loss=37；本轮没有将每项重新运行或重算总数。独立内容证据足以否定“全部环境”与“全部候选缺陷”两种一刀切结论：

- BD x64 PublicHwnd 候选失败段2025起显示原生 control 的 GetPixel=0x00CBAE87，预期0x00563412，候选目标业务尚未证明消费。BF x86官方historical/候选historical/官方PublicHwnd同样前置失败：这些运行记共同呈现/读屏前置阻塞，后续几何cache、sampler、更新、回调、重入、retry、two-device、resize 未覆盖，不能直接定位各自生产缺陷。
- BD x64 2215–2219 官方 PublicHwnd 实际通过、stdout像素0x00563412；同full候选组失败，保留候选呈现链/夹具时序嫌疑，不能把它擦成纯环境；但候选失败在原生控制阶段，现有证据还不足以直接修某候选draw函数。BF x86该官方项却失败，说明必须按运行环境/前置逐次绑定，不能跨任务借绿。
- BD x64官方resize的真实官方module SHA已独立读到，仍在相同前置像素失败；真正resize后的HW新帧未到达，记未覆盖而非官方/候选恢复等价。
- Ex的非零呈现结果、真实loss探针未证明第一设备loss、资源恢复session无post-reset oracle，仍是阻塞/未覆盖。本轮没有取得新物理GPU/loss内容，不声称这些由环境导致的失败已经关闭。其详细旧结果仍以BD原始证据为准，不宣称全部重新亲读。
- 曲线/rounded2项由BE并行处理；大shader1项内容差异保持未闭。本轮均未改、不从WindowQ绿外推解决。

## 给监督者的下一最小生产切片（仅建议，本轮未改）

先建立同一task/adapter/设备类型/窗口生命周期下稳定的native staging与实际桌面fresh-frame内容，隔离 `D3DImageTests.RunPublicShaderFrames`、官方PublicHwnd；前置不通过不得把后续cache/recovery失败直接改生产。必要采样实际呈现和读屏时序，不采用软件fallback或扩大阈值。

可协调的最小候选生产边界为 `Code/WpfGfxShape/Abi/HardwareRenderTargetExports.cs`：`Target.InitializeHardware`（100–123的adapter选择/manager/device及swapchain初始化）、`Render`（163–189的clear→visual→Renderer.Present），联动 `Core/Direct3D9SurfaceRenderTarget.cs` 的 Present/Resize调用链。该文件已亲读：Target.Width/Height只读、policy变化只重建backend；下一切片先用稳定候选HW新色与resize回归确认是否需要尺寸失效/重建，避免同时动cache、shader和恢复。**这是精确调查/潜在修复切片，不是本轮已证明该符号存在产品根因**。若可用环境下官方成功候选失败，再在该边界最小修复并双架构回归。

BD x86中断还需独立阶段/异常证据，不能先把官方member ABI猜测写成生产修复。整体四格最后源码full仍由监督主线在BE等修改集成后执行；本轮仅交付已核验失败基线、未确定的中断根因、一个确定的测试夹具修复及双架构定向结果。
