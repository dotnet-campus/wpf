# BL：x86 同 exe 有界退出监督诊断

## 1. 结论与边界

已取得原先缺失的独立 Process.ExitCode：最终源码下，官方 historical sampler 单项子进程 PID4124 在约4.87秒后退出 **-1073740940 / 0xC0000374**。不是30秒超时，不是监督角色取消或 kill；监督角色 PID2260 存活并完成最终 JSON，双流收集完成，SupervisorError=null。无被测完成 TRX。

0xC0000374 是 Windows 堆损坏终止状态名称；没有 dump，不能推出哪个调用写坏内存、夹具 ABI/系统/官方/候选归属，也不能宣称候选产品根因已找到。BD 原 full 的历史退出仍未知，不能把本次状态回填为它的退出码。完整替换、ComAcceptance full、BJ x86 PublicHwnd 均未因此通过。

最终控制先于最终 sampler：路径解析控制1/1、Process.ExitCode=0、有完成 TRX。两任务同一最终测试 SHA。没有运行 full、没有 x64 对照；当前已得到最小退出状态，不继续扩大矩阵。

## 2. 范围与实现

先读 current-execution-rules、完整替换方案1–120、BK报告（长输出有截断）、最新 next-session-handoff 入口及 BJ fresh-frame 报告相关节；检查正式项目、SDK/TFM、构建隔离和打包设施。采用 net10.0-windows，项目 global.json SDK10.0.107，Nullable启用；不改这些设置。

改动仅：
- NativeRun.cs：在正常测试入口前识别显式 --supervise-exit 角色。未改既有 RoleAsync 或默认测试并行。
- 新增 ExitDiagnostics.cs：同exe启动受filter限制的正式测试进程；记录监督/子PID、参数、UTC、runtime、测试SHA/MVID、ExitCode/hex、timeout/cancel/kill、双流收集及监督异常。启动前、启动后及最终同步flush JSON。两个原始输出流并行复制到独立文件，退出后收集限5秒；超时取消流复制，返回非0。执行上限参数1–120秒，超时取消等待并Kill(entireProcessTree:true)，清理等待限10秒；监督异常记录并重抛，不转成功。
- CaptureHistoricalControls.cs：记录场景、模块加载和consumer边界。
- VisualCache.cs：仅诊断环境变量存在时同步flush窗口创建、可观察窗口、Direct3DCreate9、CreateDevice、原生present/诊断、原生像素前置及connect前后。
- 本报告。

新增实现文件必要性：将诊断角色与正常测试逻辑隔离，不新建Host/工程、不改打包。阶段为机器诊断标识，不新增用户帮助/错误文案。没有改 CaptureFreshFrame、曲线、生产、WindowQ、项目/打包文件和共享交接。没有终端、脚本、机器码、原生探针。

## 3. 发布与部署教训

独有根 artifacts/bl-x86-exit-diagnostics/，obj、bin、publish、package均显式绑定此根；专用发布工具对正式 ComAcceptance.csproj 执行 Release/win-x86/self-contained，正常还原/构建图，无NoDependencies。AotDllPath显式引用BD full-x86/Native/wpfgfx_cor3.dll，不重发未改生产。没有覆盖公共产物。

两次带默认warning输出的发布工具报告失败，但保留日志只含末尾警告且无可见error，不能判定编译根因；为取得未截断输出以WarningLevel=0重试，两次均发布成功退出0，最后日志11行。此选项只影响诊断显示，不修改源码警告配置，不宣称零警告。最终正式执行输入为已有PackageSelfContainedComAcceptance生成的package，而非裸publish。

首次裸publish控制 task c86805b046304e2ba882e4009ff64d7f，02:40:09.7939823→10.594351Z，PID6484，退出2，aot.sha256缺失，1失败。改用既有package后 reference控制 task19103f27499b459a88a45899dfd7b10f，02:40:30.6134661→31.4595519Z，PID6532，退出2，ReferenceData/glyph-arial-original.json缺失，1失败。这是控制部署失败，不是原生退出，不计通过，也未补造参考数据。

初版sampler task651c0d960d9a47d09e6011e0c64a254e，02:40:39.1964069→44.4766762Z，PID10824，退出0xC0000374；随后路径测试类控制 taskcec9689141e346ca94e980fff8b29453，02:41:04.3516824→05.4707491Z，PID7516，11执行10通过1失败（package manifest白名单不匹配），退出2。没有修无关manifest断言。缩小到单独路径控制 task3bb6679d5ff5409c997a0ea52812600c，02:41:24.8994052→25.7620735Z，PID8712，1/1退出0。

初版测试SHA F607B28CC316555C8543D9CD46CDCC420A90063EAB8E7E01D052EAA1A010655F，MVID678653a3-adc7-4897-92c9-14cfd25e725d。补齐监督异常清理等待后重新发布并按控制→sampler验证最终源码，不混用初版身份。

## 4. 最终任务与原始证据

以下远程根为 C:/CodingAgentSandbox/Tasks/<task>/，输入均为独有package/WpfGfxShape.ComAcceptance.exe，沙箱外层上限75秒。参数数组为 --supervise-exit、bl-control或bl-sampler、30、对应FullyQualifiedName~过滤器。监督内子参数完整见JSON，包含TRX、同步diagnostic、独有结果目录，不传关闭并行参数。

|任务|本地产物根（artifacts/bl-x86-exit-diagnostics/下）|结果|
|---|---|---|
|d97aaa5966f74d53873f49363517b2be|final-control/bl-control/|监督9444、子5452；02:43:10.866396→11.7872126Z；退出0|
|ca99c8d97e4e4cc4ae4e8fb9fdbe2709|final-sampler/bl-sampler/|监督2260、子4124；02:43:20.8775895→25.749059Z；退出-1073740940|

控制filter=WhenSandboxPathIsContainedThenItResolvesUnderApplication；亲读supervision.json全文38行、stdout全文12行、空stderr、TestResults/child.trx全文26行。TRX b6a7771f-9fd8-4157-952d-764a811f661c，Completed，1执行1通过，其余异常计数0，时间02:43:11.302938→11.7212392Z。这是受监督测试父会话的完成TRX，不是监督角色自身的TRX；监督角色不运行MTP，不伪造完成测试。

sampler filter=WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain；亲读supervision.json全文38行、stdout全文2行、空stderr、stages.jsonl全文14行。递归列TestResults只有Deploy/In/Out目录，没有child.trx。双任务均Reason=exited、HasExited=true、TimedOut=false、Cancelled=false、KillAttempted=false、OutputCollected=true、OutputDrainTimedOut=false、SupervisorError=null。工具仅返回“执行完成/结果已保存”，数值退出来自同exe监督记录，不冒称接口交付了独立退出字段。

sampler阶段：02:43:21.6199837Z开始加载真实官方，.6358274加载返回；.6547723进入consumer；窗口创建、可观察窗口、D3D9、CreateDevice、PresentNativeControl、DiagnoseNativePresent均有返回记录；.8255296原生像素前置完成；最后.8269571为connect after。故这次不是卡在最初桌面颜色前置，不能说尚未完成原生present；也不能把connect after当故障调用。其后channelCreate、资源/COM/命令/绘制/cleanup仍未细分，原始退出不是阻塞证据。

## 5. 实际身份

最终监督JSON和sampler实际module JSON均亲读：.NET10.0.13、X86、pointer4，MSTest4.0.1（stdout）；MTP2.0.1见初版同步diag。最终两任务测试SHA **9C9C460AE376528299952B88DC123F1E246702A8CC8E22C06651CB01E2C9640C**，MVID **6f73bb3d-3311-4bd4-8859-9155414c07db**。

final-sampler/Results/hw-module-ca99c8d97e4e4cc4ae4e8fb9fdbe2709-0f523b67a51d439da481810ce3149ec8.json：Requested=Actual=任务根/wpfgfx_cor3.dll，官方SHA **5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834**，FileVersion10,0,1326,50504，ProductVersion10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。

同任务Results/aot-identity.json已全文读，Native/wpfgfx_cor3.dll SHA **E0D3F5EFEDFACFD5CC3491DD43B194E19028E8B541A462123C8F73C73D6E25B5**，与BD候选一致。这证明包内候选复用，不证明sampler调用了候选：本次official=True、geometry=False、cache=False、colorFF00FF00、expected0000FF80，实际加载官方。测试包含并发代理现有源码，不将其视为BD旧测试程序集。

## 6. 已知限制与下一定位

本轮提供了可回收监督JSON和明确原始原生退出；没有修复中断根因、没有dump、没有候选geometry实跑、没有x64对照，也没有关闭完整替换其他缺口。超时/取消/进程树kill代码存在，但本轮两次原生退出均未触发它；尚未以专门故障注入验证这些分支及遗留后代场景，不能报告清理分支实测通过。现有阶段覆盖到connect，后半consumer尚无逐调用完整覆盖。

下一最小动作是在仍受监督的官方单项内给channelCreate、surface/create、资源与shader成员ABI调用、present及finally释放逐段加同步边界，必要时同场景x64对照。若条件允许取得该子PID的崩溃dump，基于真实栈和分配/释放证据定位；不依据0xC0000374猜具体越界或内存根因，不以改断言、skip、关闭正常并行或重复full规避。

交付判定：退出诊断最小目标已取得；监督异常/超时全面验证及原生根因定位仍未完成。完整替换仍未完成。
