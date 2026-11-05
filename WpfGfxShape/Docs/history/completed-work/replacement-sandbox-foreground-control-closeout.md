# 沙箱可恢复 foreground control（预注册）

状态：ImplementedAndExecutedLimited；实际让位/placement恢复仍 NotExercised（匹配目标未出现），full Failed；非默认 Release 验收，不关闭完整替换。

## 预注册边界与分类

专用 SandboxForegroundControlDiagnostic 配置，默认 Release 不编译调用。父监督 scope 覆盖被监督 suite 及其角色退出；不改生产、expected、真实 loss 要求。启动只检查当前 foreground 一次；原 loss initial 激活前只再检查一次，且仅此前未处理的另一个匹配 HWND。每阶段最多一个目标，不枚举/监视/反复争抢。仅同 session、basename 精确 PrismCodingAgent.WpfShell.Tests.exe、class 为 HwndWrapper[PrismCodingAgent.WpfShell.Tests;WPF test UI;GUID] 的 top-level 窗口。保存 PID/starttime/session/HWND/TID/class/完整 WINDOWPLACEMENT，操作前及恢复前复核，防复用。未知身份/权限失败记 Unknown，不提权。

官方 API 已于实施前实际 Fetch：ShowWindowAsync 仅表示请求已启动，投递 show event 不等待宿主处理，不终止宿主消息循环或进程；SW_MINIMIZE 后有界观察 IsIconic 与 foreground，不保证前台资格。GetWindowPlacement/SetWindowPlacement length 必须 sizeof，BOOL 失败记录 last error。SetForegroundWindow 仍单次并核 BOOL/实际 HWND，不根据 last error 推原因。参考 https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindowasync 、nf-winuser-getwindowplacement、nf-winuser-setwindowplacement、nf-winuser-setforegroundwindow。

操作前将原 placement 持久化，父 finally 在 suite/角色退出后恢复两个阶段的实际目标并核实际 placement；自然退出、身份变更或恢复失败不是恢复成功，独立 cleanup 不合格、scope exit 非零。不调用注入/外部 kill/全局 SPI/RDP/驱动/输入模拟/AttachThreadInput。最小化可能影响执行工具 UI 可见性，但不终止消息或进程；finally 恢复，不请求安全确认。

分类：NoMatchingForeground / Unknown / MinimizeRequested / MinimizedObserved（不等于 foreground grant）/ FailedNotTriggered（保留原测试）/ Restored / CleanupFailed。等待只观察窗口状态（上限 2 秒，20ms），不观察业务颜色或等绿。最多两次请求、每窗口一次。恢复按保存 placement，一次请求、有界观察；不反复抢前台。

预算：normal COM build <=2、COM publish <=2（x86先）、sandbox <=3；Native0、新CLIHost0、脚本0。先原 loss 精确 filter，外180/内120；仅成功且 cleanup 合格后同配置 full 外1800/内1500；不原样追失败。严格绝对 COM csproj/net10.0-windows/win-x86/self-contained/NETCore+Desktop10.0.13、ComAcceptanceSandbox=true，147B原 native 输入/独立 clean。

## 结果

实际已新增 SandboxForegroundControl.cs、诊断 COM 配置、ExitDiagnostics 父 scope 与 ExclusiveFocusLoss initial hook；生产/Native/expected/默认测试分母未改。先写预注册后编辑和执行。标准 Release 的新行为由编译常量完全排除，本轮没有新跑标准 Release。

发布绝对项目 d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.ComAcceptance\WpfGfxShape.ComAcceptance.csproj；SandboxForegroundControlDiagnostic/win-x86/net10.0-windows/self-contained，显式 sandbox/AotRuntimeIdentifier/AotDllPath/独立 clean+publish；NETCore/Desktop10.0.13。完整参数和实时返回见 [execution](../../artifacts/sandbox-foreground-control/execution.md)。publish1 exit0，日志60行、stdout57458前37458截断，warnings总数Unknown。normalCOMbuild0/2、COMpublish1/2（implicit1）、sandbox2/3、Native0/CLIHost0/脚本0。x64未发布/未运行，不为追绿耗剩额。

actual Native SHA **147B8D862F315A9E5FB0F00A2BEE8F0ED1DFB258C870E69D3C32C5D3BE9F9304**，既有147B clean输入，同SHA未重建；official **5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834**。两task test SHA **EF928983FF7DD1B8D2E48E3ACAF82E85FE4C1DF2B866B2544354E9E595973395**，MVID **d0b3ee85-139d-488d-8946-33df2f2794ed**，.NET10.0.13 X86。未读二进制为文本。

### 原 loss 定向：1/1/0
Task **ea024fb36205402ab2d03509f4dd65e2**，[原件](../../artifacts/sandbox-foreground-control/loss-readback/Results/loss-x86/)。监督10864/6316子exit0，无timeout/cancel/kill/drain/error，stderr空；TRX48行全读及definition/counters，total1/execute1/pass1/fail0其余0。initial单次SetForeground1且actual=target0x82C0572；真实HAL fullscreen、newSurface FF123456/actual相同，child11528激活1，Allow1，displacedPresent/cooperative88760868，child授回1/exit0，recovery前台1/actual相同，beforeReset88760869、Reset0，newSurface FF654321实际相同，device/factory/window complete。

但scope三原始行只有启动与initial NoMatchingForeground、CleanupComplete；initial owner是本child6316。没有匹配外部窗口、没有最小化或恢复请求，零目标 cleanup 不等于实际恢复成功。拓扑两超长单行模块尾截断，不冒完整模块认证。

### 同配置 full：1823/1821/2，所有红项
Task **d79e98f25f67447ab24e65cc2cc9989d**，[原件](../../artifacts/sandbox-foreground-control/full-readback/Results/full-x86/)。监督2108/3032，子exit2，正常exited，无timeout/cancel/kill/drain/error，stderr空。监督38行全读；TRX14080 Counters total/executed1823/passed1821/failed2，其余0，14087完整尾已读；不是逐字读全部Passed。沙箱工具返回未另列runner exit，不补造。

|全部失败|实际|
|---|---|
|WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive|TRX2309–2340，official5EBD，native00563412过，shader初FF80实际0，resize未到；stages1318–1331 destroy/disconnect/deinitialize0、bitmap/surface/device/windows complete，最后错误空。|
|WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover|TRX2341–2362，initial SetForeground0，actual0x600FE、PID6252/TID7720/session1，FailedNotTriggered；真实loss/Reset/颜色NotReached，device/factory/window cleanup complete。|

scope三行全读，启动与initial均NoMatchingForeground=0x600FE（explorer），零目标CleanupComplete。精确匹配条件按预注册拒绝操作explorer：**ShowWindowAsync请求数0，实际让位NotExercised，SetWindowPlacement请求数0，实际恢复NotExercised**。不能说合法窗口管理失败，也不能说已恢复/已解决Prism竞争；本轮未遇到该目标，无法取得其API成败实证。没有注入、外部kill或扩展scope。旧Prism12604现场仍成立但不是当前实际owner；不能用旧PID操作新task。

旧NativeGpuAlternates(3)红本次未复现，scope动作0，不能归因minimize、也不能认cache/dup根因已定。旧默认Release1823/1820/3及历史1823/1821/2各自保留；本诊断full分母1823，不冒默认最终full。

## 下一实作及回撤入口

保留默认关闭配置；恢复入口为父ExitDiagnostics finally→SandboxForegroundControl.Restore，原placement先写foreground-targets.jsonl、请求/观察/恢复写foreground-scope.jsonl，身份变化/自然退出独立CleanupFailed，不能假绿。由于本轮两次窗口操作均未命中，窗口让位与恢复路径尚未动态覆盖，这是明确未闭项而非无能力断言。不原样追本次explorer full；下一有依据实作为：仅在再次观测到精确Prism竞争时执行同诊断scope并验证真实请求/IsIconic/身份/restore，再根据结果决定full；不先扩大到explorer或拆interactive。当前剩额build2/publish1/sandbox1，不认为该剩额已保证窗口出现，不使用它盲重跑/x64。

完整替换、产品consumer loss、HWbackend/audio/自然资源/最终双架构默认full仍Open，memoryGreen=null。
