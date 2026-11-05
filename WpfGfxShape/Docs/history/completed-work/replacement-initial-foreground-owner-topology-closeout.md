# 默认 full 初始 foreground owner 拓扑核验

> 后续实现见 [可恢复沙箱foreground scope](replacement-sandbox-foreground-control-closeout.md)。本报告旧“下一仅owner协作”已被实际C#窗口管理实现替代，不再是唯一前置。新两task未遇精确Prism，scope动作0/恢复0，诊断full1823/1821/2仍Failed；不据此否定旧12604现场或宣称已验证让位。

状态：EvidenceClosedLimited；initial foreground 修复未完成，整体替换 Open。先写草稿后实际编辑、编译、发布和沙箱 full；不是仅文档收尾。

## 1. 源合同和实际修改

亲读恢复入口、执行规则、上一consumer-contract报告、ExclusiveFocusLoss、ExitDiagnostics、NativeRun及COM工程（net10.0-windows/nullable/unsafe）。直接回读旧task2a5cb30fa5214ca58e891858eb17ad97 supervision全部38行：supervisor5116/child2496，不是6252。

新增ForegroundTopology.cs：纯C# Toolhelp进程快照（finally CloseHandle）、user32真实HWND/TID/class、session及父链、只读当前相关模块列表；访问64位模块失败明确记录，不冒路径已知。ExitDiagnostics把真实supervisor PID传给自建child，并记录启动前拓扑。ExclusiveFocusLoss在initial设备owner线程单次SetForegroundWindow前后记录实际拓扑；仍检查BOOL和actual HWND。NativeRun未改。没有新增进程/Host/工程/IPC，也没有授权未知owner。无AttachThreadInput/SendInput/SPI修改/ASFW_ANY/循环争抢/skip/测试隔离/像素放宽。

## 2. 构建发布预算和身份

完整实时账：[execution.md](../../artifacts/initial-foreground-owner-topology/execution.md)。normalCOMbuild1/2（失败诊断），COMpublish2/2（首失败、同x86编译修正复验成功，implicit build2），sandbox1/3；Native0/生产0/cache单项0。首次发布错误落在被截断前部；显式绝对COM Release/win-x86 build确证CA1838 StringBuilder P/Invoke，改char*固定缓冲区后同参数发布成功，未抑分析器。每架构1的原计划未完全达成：实际消耗两次x86发布调用，但只有一份有效包；不隐瞒、不再发布x64。

成功发布明确绝对COM项目/Release/win-x86/self-contained/RuntimeFrameworkVersion10.0.13，ComAcceptanceSandbox=true、显式AotDllPath final-candidate-major-x86-r1/clean/Native/wpfgfx_cor3.dll，独立initial-foreground-owner-topology/clean及publish。clean/aot.sha256全文147B8D862F315A9E5FB0F00A2BEE8F0ED1DFB258C870E69D3C32C5D3BE9F9304；runtimeconfig NETCore/Desktop均10.0.13。生产x64仍632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671，无新x64验收包。

首publish日志59行、stdout57495前37495截断；build64行stdout115139前95139截断；成功publish60行stdout57564前37564截断，warning总数Unknown。不宣称零warning。

## 3. 默认x86 full：1823/1820/3，完整失败保留

Task **3f7c93c1ed624998aa8da5402ea71cab**。source上述clean，exe WpfGfxShape.ComAcceptance.exe，args `--supervise-exit --full Results/default-full-x86 1500`，外1800。目的是新owner拓扑证据，不是修复后绿声明。原件：[full-readback](../../artifacts/initial-foreground-owner-topology/full-readback/Results/default-full-x86/)。

监督38行已完整回读：supervisor11648/child11360、exit2、正常exited，无timeout/cancel/kill/drain/error，stderr空。test SHA **BB504516F0B4EF317D3F99D4EE17EED0CAFE72DDD602D986DF97759AD492DA13**，MVID **3f2ac9f7-7750-4def-a160-918101b6a651**，.NET10.0.13 X86。TRX14034 Counters total1823/executed1823/passed1820/failed3，其余0，14041完整尾；未逐字读所有Passed。

|TRX行|全部失败|实际结果|
|---|---|---|
|2309–2340|WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive|actual official5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834；native00563412通过，初始FF80实际0，resize未到。|
|2341–2362|WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover|初始SetForegroundWindow0；FailedNotTriggered，未触发真实loss；device/factory/window cleanup complete。|
|2503–2536|WhenNativeGpuAlternatesFramesThenEachDuplicatedPixelMatchesItsSubmission (3)|新增失败保留：staging红色FFFF0000，复制actualFFF3F3F3，RGB expected16711680/actual15987699；Basic Render Driver，desktop duplication acquire0，非产品恢复证明。|

official清理stages1318–1331 destroy/disconnect/deinitialize HR0、surface/device/windows complete，最后错误字段空。TRX各失败、计数、完整尾/监督/实际API/身份及关键清理已读；stages1332/1334单行超过4000字符被截断，owner/父链完整可见但模块尾未全读，不能称完整加载表已认证。

## 4. 精确 owner 边界与可行动根因

启动实际foreground：HWND **0x600FE**、TID **7720**、PID **6252**、class **CabinetWClass**，Toolhelp名 **explorer.exe**、session1。本次链11648→10596 WinRemoteShell.exe→1324 cmd.exe→6252 explorer.exe→6224（已退出）。因此6252是此时外部shell祖先，不是本COM监督器；旧task只有旧PID日志，不能补造旧时刻全树。

真实initial入口stages1332–1334：target **0x33400D6**，foreground **0x458056E**，owner **12604**，TID **10940**，class **HwndWrapper[PrismCodingAgent.WpfShell.Tests;WPF test UI;b6a271f0-4411-44cb-a4cd-a807d9781115]**，进程 **PrismCodingAgent.WpfShell.Tests.exe**，session1。caller11360→supervisor11648→10596；owner12604→10596。因此是外部远程执行器下的同父兄弟，不是监督器自建被监督child；不能凭同session/共同祖先认定拥有合作端点。32位查询64位MainModule失败明确记录，进程名/父PID来自Toolhelp，session查询成功。

单次SetForegroundWindow返回0，actual仍0x458056E，sessionQueryTrue/error0；精确失败为初始前台权限未取得，并非已经LOST后Reset失败。启动owner与测试时owner不同，启动前一次Allow无法保证就近资格。当前workspace未提供explorer或PrismCodingAgent.WpfShell.Tests的自有授权协议/源码控制证据，故不实现未知RPC、不对它注入。只有外层真正控制的有资格owner能在就近时刻验证确切自建child/PID/session后合法AllowSetForegroundWindow；当前COM supervisor不能代owner保证成功。

下一可行动项：外层主代理核实并提供该Prism WPF测试进程的合法协作控制权/端点；若无，initial资格为明确外部交互边界，显式interactive suite只能由用户另决策，当前仍留默认full。未实现协作握手，不能宣称foreground已修。没有修正前置，不用剩余sandbox原样重跑/x64追绿。

## 5. official cache共享状态检查与剩余门

本轮沿真实失败身份及VisualCache源码核：每次按official选择模块并从同一module取得create/connect/channel/resource/send/present/initialize/flush等exports；connection/channel/surface/bitmap/资源ID数组均方法局部；official明确initialize(0)、connect(0)，candidate connect(1)。两个source由本次device创建，非producer snapshot替代。没有找到可证明跨实现wrapper资源ID串用或不可逆global flags/surfaceprovider污染的证据；不能从coexist直接推导根因。全加载表尾受截断，未完成逐module ownership认证，cache根因仍Unknown，不盲加reset或隔离。

旧task2a5cb30f的1823/1821/2、独立b7fd380a系统loss1/1/0均独立保留，不能相减、不代表新full通过。x64新full NotRun：无确定foreground修正且publish额度耗尽；不是x64通过。产品consumer真实loss/HWbackend/audio/自然资源/最终双archfull/完整替换仍Open，memoryGreen=null。
