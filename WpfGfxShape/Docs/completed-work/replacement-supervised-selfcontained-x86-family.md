# DO：自包含 x86 对应 Bitmap / Geometry 族

## 1. 最终结论
正式 Release win-x86 self-contained 包已成功上传并执行：**203/203 passed，failed0，BL监督Exit0**。父监督1/1 passed另计，不与业务相加。本次对应族通过，不代表完整替换完成。

最先创建本报告，随后持续回读收口。全部新产物独有 artifacts/do-selfcontained-x86/。仅专用工具与正式纯C# ComAcceptance exe，无终端、直接间接脚本、新Host/第三工程、安装共享runtime、重试或full。没有代码修正；生产、NativeRun、ExitDiagnostics、项目版本、默认打包、capture、VisualCache、historical、WindowQ、共享交接均未动。DN架构SHA诊断及DL完整路径/基址绑定和默认双模块回归保留。未skip、关闭并行、改阈值/golden或fallback。

## 2. 探索与正式发布
已读当前执行规则、完整替换方案、DN/DM/DL相关实现与报告、DD/DE打包修复、当前ComAcceptance.Package.targets、正式项目/props/targets/global.json、NativeRun、ExitDiagnostics及身份定位源码。大型历史报告有工具截断，未声称逐行重审全部历史业务原件。

TFM net10.0-windows、UseWPF/Nullable enable、SDK10.0.107/latestFeature未变。正常依赖图专用发布成功（Exit0），Release、win-x86、self-contained true、ComAcceptanceSandbox=true、DefineConstants=DM_BITMAP_GEOMETRY、AotRuntimeIdentifier=win-x86，WpfGfxShapeArtifactsRoot指向本轮build/。未指定或猜测RuntimeFrameworkVersion；发布后的runtimeconfig实读includedFrameworks：Microsoft.NETCore.App与Microsoft.WindowsDesktop.App均10.0.13。发布含既有分析器警告；日志有前段字符截断，不称零警告或完整无截断日志。

Native源为 artifacts/de-upload-review/clean-x86-02/com-acceptance-package/Native/wpfgfx_cor3.dll；未重发生产。打包GetFileHash结果及运行actual均为7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA。

## 3. 精确上传边界与大小
唯一sourceDirectory绝对路径：
`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\do-selfcontained-x86\build\com-acceptance-package`

仅上传该全新白名单包，不上传PublishDir、父目录、回收根或旧ReferenceData。当前targets显式取应用/MTP、RuntimePackAsset、Contracts、Native；本族无静态ReferenceData输入。保留正常依赖图runtime资产及官方DLL（包根），不能误删为候选重复。没有Results/TestResults、历史PDB、obj、coverage跨平台或异架构资产。RuntimePack包含WindowsDesktop本地化资源和SDK附带诊断文件，未擅改默认选择规则，不声称逐文件绝对最小。

**418文件，实际146391579字节**（逐文件FileInfo.Length之和，不是HTTP字节）。完整逐文件相对路径清单在源package-files.txt（418行）；实际每文件字节在回收Results/dm-package-identity.json的Files数组（418项），运行中读取本次源清单得到。分类根据打包组和实测长度：

|大类|文件数|实际字节|
|---|---:|---:|
|应用exe/dll/deps/runtimeconfig|4|1650998|
|显式MTP/MSTest及支持依赖|15|3657992|
|RuntimePackAsset（.NET/WPF、native、资源）|394|137068144|
|Contracts|2|4522|
|候选Native|1|3995648|
|aot.sha256及package-files.txt|2|14275|
|合计|418|146391579|

包大小为同次诊断实测，不借用DE或DN旧数。逐文件原件已完整回收；本轮亲读身份/总量、应用/MTP头部、候选/contract/manifest尾部和inventory，未声称全文逐项人工复算418个长度。

## 4. 唯一任务与有界执行
一次正式exe调用，直接选择DmBitmapGeometryTests.WhenExpandedFamiliesAreSelected，带report-trx/parent.trx、results-directory Results/do-parent及no-progress。无需再单独发现或上传启动控制：该正式监督入口成功即提供启动控制证据。外预算180秒；原DM监督内部filtered120秒（已读Parse允许上限），同一包执行主体。

任务ID：**73b9d2c8d18647bb8098f3814dc239f2**。
任务绝对根：`C:\CodingAgentSandbox\Tasks\73b9d2c8d18647bb8098f3814dc239f2`。
全任务回收至 artifacts/do-selfcontained-x86/readback-x86/，严格不再上传。工具原响应显示MSTest4.0.1、win-x86、.NET10.0.13，父1成功/0失败/0跳过，9s383ms，回收完成；工具没有单列外层数值ExitCode，不猜填。

以下路径以任务根为前缀，本地回收保持同构：
- 父TRX：Results/do-parent/parent.trx，1–26行全文亲读，Completed，total/executed/passed1，failed及notExecuted等0。
- 原始业务TRX绝对路径：`C:\CodingAgentSandbox\Tasks\73b9d2c8d18647bb8098f3814dc239f2\Results\dm-bitmap-geometry\TestResults\child.trx`，2395–2402行亲读，Completed，total/executed/passed203，failed/error/timeout/aborted/notExecuted等0。递归原件搜索RunInfo和outcome=Failed无命中；不是仅信stdout，目录枚举漏列TestResults不影响直接原件读取。
- Results/dm-bitmap-geometry/supervision.json 39行全文亲读：09:43:51.0200114Z至09:43:59.9678382Z（约8.948秒），filtered/120，Reason exited，ExitCode0/0x00000000，HasExited/OutputCollected=true；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，SupervisorError=null。

实际filter原文在supervision第9行；完整BitmapRenderTargetTests.、GeometryUtilityTests.、IndependentRealizerAbiTests.、IndependentBitmapRealizerAbiTests.，加OfficialRasterModuleTests.WhenSameNamedLibrariesAreLoaded和DmBitmapGeometryTests.WhenActualPackage，均FullyQualifiedName~后以|联接。没有选Br/CK控制或full。

## 5. 真实分母与actual身份
身份JSON的Inventory（1710–1859段）已亲读29个bitmap方法及DataRow数；BusinessCases201。x86实际计数与源码inventory及本次TRX相符，不借x64强造分母：Bitmap183、Geometry12、独立ABI2+4=6，业务共201；双模块身份1、专属诊断1，共203。父监督和两个官方输入生成角色不叠加。

实际.NET10.0.13，X86，独立ABI PointerSize4。测试SHA BF33A78A6897ED92C4AAE41E4028E9B18AF4EDA5E55CABB13E91951051BEDAE4，MVID f5e175c3-feec-44d3-a355-292aeae51b86。

实际WPF：任务根PresentationCore.dll，版本10.0.1326.50504，SHA D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D。
实际官方：任务根wpfgfx_cor3.dll，SHA 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834，版本10,0,1326,50504，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699，实际句柄/基址6D350000。
实际候选：任务根Native/wpfgfx_cor3.dll，SHA 7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA，版本1.0.0.0，实际句柄/基址6D700000。

Results/dl-oracle-identity/dual-modules.json全文17行亲读：SameNameCount2，官方/候选句柄分别等于实际选择基址，路径不同。身份诊断按NativeLibrary实际句柄Single匹配ProcessModule.BaseAddress；DL按官方完整路径唯一绑定。不是Program Files下x64共享10.0.1，不是同名DLL任意First或fallback，更不是仅哈希期望路径冒充加载。

## 6. ABI与同次内容亲核
六份Results/independent-{bitmap-}realizer-*.json全文亲读。成功ABI四份对应官方/候选×Translated false/true：均16个0，Ensured/Brushes/Effects16，ContextMatches=true；x86 MatrixOffset48、TextureOffset164。未平移像素[0,0,255,255,0,0,255,255]，平移[0,0,0,0,0,0,255,255]，双方精确一致且非空。
失败ABI两份均16次-2147024891（E_ACCESSDENIED）、Calls16、Adapter0、Display4294967295、ContextMatches=true；后续绿色像素base64 AP8A/wD/AP8=。每份记录本次官方/候选SHA与X86/PointerSize4。该证据验证重复成员调用与后续状态，不冒称直接ESP/寄存器或长期资源收敛。

同次官方shape输入两份全文各65行亲读：
- Results/source-clip-reference/9d0a9e9b76c24e2ca575a57507a53c8f/ReferenceData/source-clip-intersection.json
- Results/source-clip-reference/a2d10327588a4d57992e5c248768142f/ReferenceData/source-clip-intersection.json

均本次任务根官方SHA5EBD18E0…；矩形[0.6,0.2,1.9,1.1]，四点clip，4×2/stride16，红/alpha两行72,191,84,0与36,96,36,0，确实非空。两个生成角色TRX尾段均亲读1/1 passed，失败/跳过0。通过既有正式exe独立官方WPF机制在本次ignored目录生成，不是旧10.0.13 golden或x64结果。候选既有32字节CollectionAssert全等通过；没有修改预期。

其余Bitmap包含固定像素、锁、clip/layer、ownership、HDR及错误输入/空状态合同，不能把183项全部称实时独立官方差分或全部nonempty。Geometry12保留原官方/候选差分及现有数值容差，通过计数由本次TRX验证；原始12份JSON完整回收，未声称逐份大型轮廓全文人工审计。

## 7. 分类与剩余边界
本次失败分类：候选产品差异0、夹具失败0、缺输入0、环境阻塞0、崩溃/超时0、跳过0。无需用允许的一次确定根因纠正额度，也未重复上传/执行。自包含必要约146MB正常执行成功，不再把缺共享runtime作为阻塞。

本次x86对应族通过；无生产修正，默认断言/打包/并行均不变。未执行full，未验证物理GPU、真实device-loss/resize恢复、持续media、长期资源收敛及所有真实WPF组合调用闭包；不能绑定旧full到本次测试SHA，不承诺任意.NET/WPF版本或完整替换已完成。
