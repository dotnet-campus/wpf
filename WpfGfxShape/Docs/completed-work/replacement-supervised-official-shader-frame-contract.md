# KG：官方 shader 首消费帧合同独立审查与一次隔离验证

## 终态：原生前置失败早停，未覆盖旧首 shader 帧失败
本轮生产0修改；共享 VisualCache/historical、DRDS、NativeRun、ExitDiagnostics、捕获/WindowQ、项目打包/版本及 CF/CX/CY 未修改。保留并行 KF。仅新增全文件 `#if KG_OFFICIAL_SHADER_CONTRACT` 独有测试，默认无新行为；未新建 Host/工程/入口/脚本/机器码，未装 resolver。
唯一正式 official x86 task `d6ccccf4771a41f2994173e987c1a23a`，部署纠正0，candidate业务任务0，full/COM变体/重复矩阵0。已完成 TRX：1执行/0过/1失败/0skip，Failed，子 Exit2。不是旧shader失败复现：本次在连接/创建资源/提交shader之前，native窗口像素 `001E1D1C` 不等于原前置 `00563412`，按合同早停，不调整窗口或重跑首红。

## 原失败精确身份与阶段
原件 `artifacts/kd-shader-integration/inventory/inputs/x86.trx:2362-2394`：
- 原 ID `14e862b3-9b71-843a-9ff5-67f779272c33`，Name `WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive`，完整类 `WpfGfxShape.ComAcceptance.D3DImageTests`。
- 原 task `087548bed8dc4a7db312504c02debe41`，官方 actual 根 wpfgfx_cor3.dll SHA `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`，.NET10.0.13/X86。
- native前置 `00563412` 真实通过；76 DWORD/304 bytes 的 CreatePixelShader HRESULT0；第一shader消费观测 `00000000` 而应 `0000FF80`，随后断言失败。没有 HardwareResize 记录。不能称“官方resize后坏”。创建shader仅证明该字节码能在该原生device上创建，不证明WPF资源绑定、实际shader设备或消费成功。
- KE全分类文件 `classified/Results/kd-inventory/final-classification.tsv/json` 保留原指针：44记录、40唯一ID、交集4/仅x6436/仅x860；x64 23首帧+3fresh+2隔离首帧+2public像素+9Present状态+1loss；x86 AV/候选resize后/官方resize前/loss各1。历史full x64 1814/1774/40、x86 1814/1810/4保持实际历史，不从本次单例相减。

## 静态合同：真实角色与资源连接
入口 HardwareClosure.cs 原方法传 `cache=true,color=FF00FF00,expected=0000FF80,geometry=false,resizeWindow=true,official=true`，不是 VisualBrush 分支。KG方法只打印身份/bytecode SHA后直接调用原方法，没有缩减内容、后态、resize、mutation或cleanup。
路径 RunVisualCacheConsumer：官方 Load原始DLL → native HAL device/window前置 → initialize(0) → async connect(0)/channel → 两个12×1 format21 surface ColorFill(首红/次绿) → InteropDeviceBitmap_Create(96,96,7,Available,0) → image97/drawing43/visual39 → image update0A/drawing18/image record47/visual content22/dirty。
shader33、implicitBrush34、secondary83(BitmapCacheBrush)、effect38；bitmapcache94/scale1经0x8D绑定secondary0x84的hBitmapCache@28、hInternalTarget@32=visual1。implicit sampler0来自visual0原内容；sampler1来自独立visual1，不是把producer surface直接当target。visual0通过0x1D绑定effect；target46经0x35 root=visual0。因此目标消费链是：D3D surface→interop bitmap→image→drawing→visual→implicit/cache sampler→effect→HWND target→window DC GetPixel(16,8)。
非cache分支创建82 VisualBrush，148字节0x83中viewport/viewbox=(0,0,1,1)，relative mapping@108/112=1，Stretch@124=1，hVisual@144=source；本次原场景不执行该分支。IW真实WPF路径则确实使用VisualBrush，不能混淆。

## SDK布局与责任字段
当前可读官方源码为仓库 `src/Microsoft.DotNet.Wpf`，不宣称已认证到实际servicing commit的逐字节源码。Generated/wgx_commands.cs与官方C++ marshal/ShaderEffect.cpp/PixelShader.cpp已聚焦对照：
- packed1资源句柄uint32，两架构相同；D3DIMAGE=24，interop/software pointer均UInt64；PRESENT=16，hEvent@8 UInt64，不因x86变12。HWND target=92，hwnd@8 UInt64、flags@56、DPI context int32@72、double@76/84，x86/x64协议相同，不能用进程指针宽度截短。
- PixelShader packet20+304=324，mode@8=2(HardwareOnly)，bytecode size@12=304，CompileSoftwareShader@16=0。当前WPF PixelShader.ManualUpdateResource对非ps3.0会设置该BOOL=1；因此原夹具与真实ps2.0 WPF发送字段不同。native PixelShader.cpp在该BOOL非零时预先调用软件编译器以验证，不能未经运行就宣称设1必修首帧；HardwareOnly与CompileSoftwareShader不是同一个开关，不能换SoftwareOnly求绿。
- ShaderEffect头80，总122=80+2+16+16+8；float register bytes@48=2，values@52=16，samplerInfo@72=16，samplerValues@76=8；尾部short register0@80、float4@82、两组(int register,int sampling)@98=[0,1,1,1]、两uint资源句柄@114/118。官方marshal按字节长度解析，SHORT/FLOAT/INT相应整除合法，两个sampler数相符。未发现该处应按架构变宽的依据。shader原常量def c0=.5与输入内容保持。
- 原target flags12、实际window DPI context映射(-1..-4)、GetDpiForWindow/96，与HB合法化后的来源一致；updatewindowsettings72字节/alpha1/renderingEnabled1。原shader路径没有HB的initial invalidate0x37/24字节。HB setup flush后显式invalidate/close/commit，再dirty/Present。缺少这一显式请求是确定的发送差异，不等于已证它单独导致旧零帧；未进行盲改或逐开关消融。

## Present、完成与资源责任
原official initial setup close/commit/flush后，逐个officialImages执行JhPresentImage；每次独立 EventWaitHandle，DuplicateHandle转交UInt64，send separate=1/commit/等待<=5秒/flush；不是Event0。官方d3dimage.cpp ProcessPresent调用interop Present/GetSoftwareBitmapSource/NotifyOnChanged，Cleanup即使失败也SetEvent并CloseHandle重复句柄；原事件仍归测试，测试不猜测关闭已转交duplicate。事件完成不代表目标像素已绘制。
原场景释放caller bitmap0后flush两次并GetPixel，才进入64/48/32×24 resize，再修改第二surface绿→蓝/dirty/官方JhPresentImage/释放caller bitmap1/两次flush与后态 `00FF0080`。原expected与原AssertCombinedPixel既有每通道<=1保持，未改阈值/golden/skip/fallback/并行。finally destroy channel/disconnect/deinitialize，逆序release bitmap/surface及device/factory/window；现有Release未输出精确ref0，不能伪造资源收敛。旧IS官方sampler AV属另案，未调用明知Event0路径诱发，也不宣称这里的字段差异是历史AV唯一因果。

## HD/HB与IW对照边界
HD/HF历史报告 replacement-supervised-hwnd-consumer-contract.md:55-86/137-162：官方真实三色window DC读回、duplicate完成及最终refs0；target flags12/context-2/DPI1/1，显式initial invalidate。那是单输入consumer，backend仍未知，不是本shader HardwareOnly已过的证据。HWND DC GetPixel不等同consumer GPU target surface直接读取。
IW OffscreenSampler：相同token，PixelShader SoftwareOnly，implicit sampler0+VisualBrush sampler1，RenderTargetBitmap.CopyPixels全像素initial0000FF80/mutation00FF0080；detach/weakrefs/dispatcher清理。与HWND/interop/BitmapCacheBrush/HardwareOnly不同，不能拿IW合同替代本次硬件消费证明。当前原例既有未验证字段差异，仍需真实硬件消费门，不将官方像素失败自动判candidate bug。

## 本次输入、发布和真实结果
输入差异逐项：图形参数/窗口/producer尺寸格式/设备flags/bytecode/mode/CompileSoftwareShader/target/提交顺序/expected/resize/mutation/cleanup全部不变；仅独有外层测试名称、filter、身份输出、testSHA/MVID、运行时任务路径和运行时窗口句柄不同。未专属copyhelper/复制整Host。
正式ComAcceptance x86 Release self-contained10.0.13正常依赖图发布，显式KG符号及独有 `artifacts/kg-official-frame/{obj,bin,publish,com-acceptance-package}`/PublishDir。候选仅复制KD既有Native DLL，不重发生产；aot.sha256=`7E0936D24BBB7415DD2A8701AE260E8F5E5D8140E61E8510C3F5B6CD6DBA82DD`。clean正式package与readback分离，正常MTP/runtime/Contracts依赖，不上传源码/obj/publish父目录。runtimeconfig includedFrameworks两项10.0.13。没有ReferenceData输入复制，本方法不消费参考JSON；运行生成结果不是原输入。未完整重算包总字节。
离线API确认发布DLL存在D3DImageTests及KG方法（API检查发生回收后，不虚称运行前已全部核完）；实际TRX发现并执行1条，不是零发现。
本task绝对TRX：`C:\CodingAgentSandbox\Tasks\d6ccccf4771a41f2994173e987c1a23a\Results\kg-official-x86\TestResults\child.trx`。
本地回收根 `artifacts/kg-official-frame/readback/official-x86/`；supervision.json、child.trx、stages.jsonl、stdout.txt、stderr空全部已亲读。
- 父PID5068，子9124，filtered120秒，外180；Started07:11:47.483→Ended07:11:49.563，Reason exited/HasExited=true/OutputCollected=true，Exit2，timeout/cancel/kill/drain均false，SupervisorError=null。
- 沙箱工具仅报告执行完成，没有单列外层数值Exit，不将其文字成功写作业务Exit0；子MTP明确Exit2。
- actual官方包根DLL SHA `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`，文件10,0,1326,50504，servicing.26504.104/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；.NET10.0.13/X86，WPF assembly10.0.0.0（本轮未额外实测PresentationCore文件SHA，不冒充完整WPF身份）。
- test SHA `D0D2DB079C8977A62C201BF4E098BD7D73110AD6625EEE3E98A3301BAE8C97CD`，MVID `ed8991c2-4a31-4f18-82e4-0b823a8d6373`；shader SHA `4B6CCE69D31356ACAF977F9D451E8F358BE4701B0F38FAB2DDF153E3F07DC595`，304bytes。
- target0x1E40592/cover同一窗口、Default/session1、visible1/cloaked0、32×16/DPI96，native CreateDevice/PresentNativeControl返回后读取pixel001E1D1C。现有日志production字段只打印包Native SHA，不证明候选被消费。
- stages在AssertCombinedPixel前后直接进入cleanup；没有connect/channel/images/shader/Present事件。故本次原始sendbytes和资源创建/转交事件记录为未到达，不制造空成功；bitmap/surface初始0，仅device/factory/window清理实际到达，cleanup complete为空错误串，不证明ref0。

## 下一唯一根因门与不保留行为
本次第一个实证断点为native producer Present后到window DC可见内容（001E1D1C/00563412）；尚未进入官方shader consumer。旧失败的第一个缺证是：在同任务native前置成立后，合法target初始化与ps2.0 CompileSoftwareShader字段下的首个实际shader消费帧/资源绑定证据。下一只安排该单一官方路径根因门：专属合法协议副本显式审清initial invalidate及CompileSoftwareShader=1来源，记录发送bytes/完成/原FF80与后态内容；此前不建议修改共享夹具，不宣称任一字段单独根因。此轮预算已耗，不执行该下一门。
默认安全：KG整文件条件编译，无默认入口/行为；没有任何未验收修正落入default。最后源码后默认无KG符号的Release/x86 normal Build使用独有normal/obj/bin及正常分析器成功（通过专用发布能力的-t:Build，仅构建，不是新运行），未NoWarn/关闭规则，未重发生产。

## 亲读范围与开放项
已读执行规则、完整方案（首次输出长截断后补读40-120）、最新handoff相关节、KD/KE报告可见各段/最终分类表1-45及JSON计数/分类、原x86精确TRX业务项、相关共享测试与官方SDK/解析/Present源码聚焦段、HD/HF报告相关节、IW代码。未宣称44个巨大RawResult逐字符审完、全历史/所有二进制字节或完整servicing源码认证；不以继续找旧ABI来源阻塞真实执行。
旧full1814/40及4不改。Available、实际HWbackend、真实loss、mediaresources、长期资源和最终四格仍开放；本次不能宣布完整替换或官方shader根因关闭。
最后补记：本报告及handoff/session-summary/issues自有KG节写后回读；不覆KF及其它历史。最终normal构建结果以专用构建日志退出0及实际输出路径核对，不造未提供的warning总数。
