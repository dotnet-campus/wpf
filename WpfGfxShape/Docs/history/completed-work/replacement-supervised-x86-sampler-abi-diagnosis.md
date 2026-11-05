# BN：官方 sampler ABI 与退出阶段诊断

## 1. 裁决

未完成堆损坏根因闭环，未修复生产或夹具行为。已把 BL 的 connect-after 边界收窄为：**首次 MilChannel_CommitChannel 返回 0x80070006，原断言失败进入 finally，MilConnection_DestroyChannel 未记录返回，进程以 0xC0000374 退出。** x86 与必要 x64 对照均如此。不能将 DestroyChannel 的最后阶段当成首个写坏内存的位置，也不能据此宣称官方 bug。

发现确定的调用合同偏差线索：RunVisualCacheConsumer 对同步 channel 发送 Event=0 的 MILCMD_D3DIMAGE_PRESENT；真实 PresentationCore 只在异步 channel 发送，并转交 DuplicateHandle 得到的事件。它与 E_HANDLE 相符，但尚未通过独立回归证明其与堆损坏的因果关系。没有将推测修成产品行为。

BD 历史 full 的退出码和根因仍未知，不把本轮结果回填 BD；本轮不是 full 完成，也不是 sampler 通过。

## 2. 规则、范围与修改

亲读 current-execution-rules、完整替换方案1–120、BL报告全文、BK报告分段补齐到97行。读取正式 ComAcceptance 项目、global.json、Directory.Build.props/targets、既有打包设施；TFM net10.0-windows、Nullable启用、SDK配置10.0.107，未修改。

符号查询 CaptureHistoricalControls 无结果，VisualCache 的索引只返回 WPF tracing 同名符号，因此使用已定位测试文件与官方源码文本核对，未把无关索引结果当测试定义。

仅修改 Tests/WpfGfxShape.ComAcceptance/D3DImageTests.VisualCache.cs：
- 为 channelCreate、两组 surface/CreateRenderTarget/ColorFill/InteropDeviceBitmap_Create、资源与命令、原生 shader/effect、target/CloseBatch/Commit、present 及 finally 清理增加现有 ExitDiagnostics.Stage 同步边界。
- 首次 Commit 结果保存为局部变量，先同步记录原始 HRESULT 再执行原有零值断言。
- finally 仍按 destroy channel → disconnect → bitmap/surface →其余COM → device/D3D → window 的原顺序，不吞异常、不跳过释放。

默认无诊断环境变量时 Stage 立即返回；原断言、阈值、并行、资源大小及场景不变。没有新增Host/第三项目、原生探针、脚本或终端；未改 CaptureFreshFrame、曲线、WindowQ、项目打包、生产或共享交接，未覆盖 BM fill 文件。另新增本报告，产物仅 artifacts/bn-x86-sampler/。

## 3. 官方声明与资源合同审查

官方源根为 src/Microsoft.DotNet.Wpf/src/。

|路径/符号|核对结果及限制|
|---|---|
|WpfGfx/core/uce/apifunc.cpp WgxConnection_Create|WINAPI，bool + HMIL_CONNECTION*；测试 byte + nint*。两架构真实返回成功。|
|同文件 MilConnection_CreateChannel/DestroyChannel、MilChannel_CloseBatch/CommitChannel|WINAPI，连接/客户端channel为指针。测试使用 Stdcall/nint；创建、CloseBatch返回成功，Commit E_HANDLE。|
|同文件 MilResource_CreateOrAddRefOnChannel|MIL_CHANNEL指针、资源类型枚举、HMIL_RESOURCE*；测试nint/uint/uint*。|
|WpfGfx/include/processed/wgx_core_types.h:60–66|HMIL_RESOURCE/HMIL_CHANNEL是UINT32；MIL_CHANNEL是独立指针句柄，不能把资源句柄扩为nint。当前声明保持该区别。|
|WpfGfx/core/hw/InteropDeviceBitmap.cpp:29|WINAPI，IUnknown*、两个double、UINT版本、callback、BOOL fallback、对象**、两个UINT*。测试主体参数宽度相符；两组Create返回成功。callback typedef未显式调用约定，本轮未确证其最终编译约定，不凭猜测改全局Available。|
|WpfGfx/include/Generated/wgx_commands.cs/.h|D3DIMAGE为24字节，指针字段在8/16以UINT64传输；PRESENT为16字节，事件在8为UINT64。测试ImagePacket/PresentPacket Pack=1，x86也不能缩为原生指针宽度。HWNDTARGET_CREATE为92字节、窗口字段UINT64，shader头20字节、effect头80字节；核对已读字段偏移，没有据此证明所有可变payload有效。|
|WpfGfx/core/resources/d3dimage.cpp:180–237|ProcessUpdate偷取传输引用。测试提交ImagePacket前调用AddRef，保留调用方bitmap引用到Commit后；没有因本轮诊断提前Release。|
|同文件 ProcessPresent:249–289|注释说明同步channel不应发生；读取hEvent后Present，cleanup无条件SetEvent/CloseHandle。|
|PresentationCore/System/Windows/Interop/D3DImage.cs:657–727、765–817|SendPresent复制事件交给命令；UpdateResource区分同步channel，并明确Presents只发生于异步channel。测试connect(1)后却提交Event默认0的PresentPacket，含更新后的changed包，同一方法中存在兄弟偏差。|
|WpfGfx/shared/util/UtilLib/instrumentationapi.h:109–128|MIL_TW32失败会写hr=HRESULT_FROM_WIN32(GetLastError())，因此空事件的SetEvent/CloseHandle是E_HANDLE的具体来源候选，不是已证实唯一来源。|

此外读取 RefCountBase.h 与 CInteropDeviceBitmap → CDeviceBitmap → CWGXBitmap 继承入口，未将非IUnknown的IMILRefCount当成此返回对象的最终vtable证据；不能依据该通用基类擅自改bitmap的AddRef/Release槽。当前没有dump或native allocation/free栈，未定位首次内存破坏。

## 4. 构建与任务

通过专用发布接口对正式项目进行 Release/win-x86及win-x64 self-contained发布，正常还原/构建图，无NoDependencies。每架构显式设置独有 obj、bin、publish、package 路径于 artifacts/bn-x86-sampler/，复用既有 PackageSelfContainedComAcceptance。三次发布均成功退出0；WarningLevel=0仅为避免工具长警告截断，不宣称零警告。

AotDllPath显式复用 BD full-x86/full-x64/Native/wpfgfx_cor3.dll：旧生产二进制 + 新测试快照，不重发生产，不把 BM 并发源码绑定 BD hash。没有覆盖公共产物。

沙箱均执行正式纯C# exe --supervise-exit 角色，内部30秒、外层75秒；filter为 FullyQualifiedName~WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain。监督JSON保存完整实际参数，未关闭正常并行。

|本地产物（artifacts/bn-x86-sampler/下）|task|结果|
|---|---|---|
|official-x86/bn-sampler|351d01105a404e399054a8e1d905dffa|初版诊断，监督9676/子2916；03:02:23.9462328→29.3247379Z，0xC0000005；最后cleanup destroy channel before。未记录Commit返回值。|
|final-official-x86/bn-sampler|bf7a4fbfe70d43e0a1ea21989000e53a|最终诊断，监督2916/子2288；03:04:24.6496927→29.7786138Z，-1073740940/0xC0000374。|
|final-official-x64/bn-sampler|2ac1a430d4734550ba435809179ca49c|最终诊断，监督10356/子4104；03:06:44.4014516→49.9018433Z，-1073740940/0xC0000374。|
|final-control-x86/bn-control|dbc4a814094a47488f80468cd065a420|同最终x86测试SHA，监督11248/子6304；03:09:07.1471557→08.1855905Z，退出0，路径解析控制1/1。|

所有任务Reason=exited、HasExited=true、TimedOut/Cancelled/KillAttempted=false、OutputCollected=true、OutputDrainTimedOut=false、SupervisorError=null。ExitCode来自监督进程记录，不冒称沙箱接口提供了数值退出。

最终两架构均先通过原生present与像素前置，channel/资源/native shader/bind effect/target/CloseBatch有after；Commit分别03:04:25.6801573、03:06:45.2149749返回0x80070006，随后.6823232、.2176892为cleanup destroy channel before。没有进入bitmap0调用方释放或正式present循环；不能解释为捕获算法旧帧断言失败。

亲读最终两份supervision.json全文38行、各stages全文35行（分段）、stdout全文2行、空stderr；递归列TestResults仅Deploy/In/Out，无child.trx。初版stderr有Fatal error 0xC0000005及RunVisualCacheConsumer栈，仅阅读首20行，不声称完整原生栈或逐字读完107行；该初版不是最终测试身份。

控制亲读supervision全文38行、stdout全文12行、空stderr、child.trx全文26行：TestRun 8cbb275c-0c01-475c-9b4c-402da1983967，Completed、1执行1通过，其他异常计数0，03:09:07.5577203→08.0251143Z。控制filter=WhenSandboxPathIsContainedThenItResolvesUnderApplication。没有把它当sampler完成TRX。

## 5. 实际身份

最终module JSON与aot-identity均全文回读，Requested=Actual任务根/wpfgfx_cor3.dll，真实调用官方，不是Native下候选。

|身份|x86|x64|
|---|---|---|
|官方SHA|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|
|BD候选SHA（包内身份，非本轮被测）|E0D3F5EFEDFACFD5CC3491DD43B194E19028E8B541A462123C8F73C73D6E25B5|1C381539E7C82DDD75908C0563F0AC062E583166BDB23AEE066BDD21E838752F|
|最终测试SHA|B7E4902BFA47A792B108FBA0E7D87E3E6B3D5EB2E963A72A80FD015699597D1D|F781C165AB103EBD7F2218FC81616D262EDBB03CA0B63D9462BDA63E6DA0F98F|
|最终测试MVID|82da2e62-6250-4b71-8a2c-75e0ff7ca445|6e7abd51-aeb0-4887-a3ac-189db9e1c9d0|
|架构/指针|X86/4|X64/8|

官方FileVersion均10,0,1326,50504，ProductVersion10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；运行时.NET10.0.13，MSTest4.0.1见stdout。初版x86测试SHA9EC83A2E1FFB7641E2FB4E574183693CD978FC640F0B29BAE2BD0203B5535A00，MVID1f7f53e4-f1d1-466f-978e-ae035c02d096，不能混为最终。

## 6. 最后未定边界与必要后续

已证实首次业务失败先于cleanup崩溃，两架构同形态，故不能仅凭x86指针宽度推断根因。尚未证明哪个包导致E_HANDLE，哪个写入或释放破坏堆；无dump，无新独立ABI回归，无候选双架构定向通过，不宣称修复完成。

最小下一动作应是纯C#独立验证D3DIMAGE_PRESENT事件所有权/同步通道合同，记录有效事件转交后的Commit及完整cleanup；同时保留现有内容更新断言。必须区分“消除空事件E_HANDLE”与“证明堆损坏根因已修复”，前者单独成功不能替代后者。若要改夹具，应覆盖本方法初始与mutation两个PresentPacket提交，复核兄弟路径，并在真实官方/BD候选两架构定向验证后再评估更大矩阵。

本轮停止继续盲目sampler/full重复，不跳过DestroyChannel、不放大分配、不改阈值/skip/禁并行来求Green。完整替换、BD full和其他HW/恢复缺口仍未完成。
