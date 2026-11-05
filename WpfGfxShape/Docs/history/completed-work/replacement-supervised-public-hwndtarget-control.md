# Public HwndTarget official 矩形控制：一次失败后停止

## 裁决与独立运行账

本次亲自执行唯一现存 public 控制，不再委派；源码/配置/代码包修改0、build0、publish0、独立发现运行0、Native控制0、candidate0、Windows沙箱1、真实测试体1失败。没有重试、重复采样、新探针、public D3DImage实现或候选执行。

**stdout raw expected=0x00563412 / actual=0x001E1D1C，严格内容控制失败，且原每通道≤1断言同样失败。** 不是缺raw、仅容差通过、ContextIdle超时或监督超时。最窄结论：本次现存public矩形路径的window DC中心像素没有取得预期内容；不能唯一归环境、interop、手工协议或candidate。另有实际模块身份覆盖不足，不能声称exact5EBD实际加载认证。

本账独立于direct累计build4(1败3成)/publish3/official3真实体3失败/candidate0；不改成direct第4次。WGC build4(1败3成)/publish4(1败3成)/沙箱3(发现1真实2)/体2失败、撤后default build1成功、通用包退场残项及KS build1/publish1/沙箱4/请求3/发现1/体1/A1/B1/native2另账不变。完整替换Open。

## 静态前检与执行合同

已全文读最新frame-contract-audit 1–81（截断段补读）、当前规则全文及恢复入口/当前事项最新节；必要范围读D3DImageTests.cs298–325、WindowThread全文、AssertCombinedPixel、AcceptancePaths初始化、NativeRun监督入口、ExitDiagnostics参数及身份记录。没有修改任何这些源码。

包来源为clear-control报告指明且实存的 `WpfGfxShape/artifacts/direct-consumer-clear-control/com-acceptance-package/`，不是publish根。核package-files.txt首18/末12行、入口exe/dll/runtimeconfig存在、根official和Native候选角色文件存在、aot.sha256；没有重hash全621项。离线DLL公开API再次确认准确入口：

`WpfGfxShape.ComAcceptance.D3DImageTests.WhenOriginalPublicHwndTargetDrawsThenWindowContainsHardwarePixels`

源码该方法不调用native-only控制或candidate入口；AssemblyInitialize只核候选PE架构/文件SHA并输出identity，不加载候选执行。ClassInitialize启动STA窗口线程，没有额外D3D9控制前置。两includedFrameworks均10.0.13。

唯一沙箱上传上述clean，执行已有WpfGfxShape.ComAcceptance.exe，参数依次为 `--supervise-exit`、`Results/public-hwndtarget-control-official`、`120`、`FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenOriginalPublicHwndTargetDrawsThenWindowContainsHardwarePixels`；外180秒。无斜杠filter、无发现专用调用。MSTest执行内部枚举1820项是正常过滤运行的一部分，不是额外发现任务；只执行一个精确方法。

## 原件、监督及真实体证据

回收根 `WpfGfxShape/artifacts/public-hwndtarget-control/readback/official/`；以下路径相对该根：

- `Results/public-hwndtarget-control-official/supervision.json` 全40行；stdout全28行；stderr为空。
- `Results/public-hwndtarget-control-official/TestResults/child.trx` 全49行，已直接读取；目录/名称工具最初未列出该TRX，不能因此宣称缺失，实际读取成功为准。
- `Results/public-hwndtarget-control-official/Diagnostic/log_261027040431218.diag` 核runtime、准确过滤器、RecordStart/Executing/Executed/RecordResult/Failed、TRX产物及会话Exit2。未冒称全文审阅长行中所有属性。
- `Results/aot-identity.json` 已读。`Results/public-hwndtarget-control-official/stages.jsonl` 直接读取返回不存在；本方法无Stage调用，不能虚构阶段原件。

任务03041eb56b8f4775a250759d0966e617；监督PID10604/子PID4128；2026-10-27 04:04:31.0754128→04:04:32.5679183 UTC；Reason=exited、Exit2、HasExited/OutputCollected=true，TimedOut/Cancelled/KillAttempted/两项drain超时=false，SupervisorError=null。

TRX total/executed/failed=1/1/1、passed/notExecuted=0/0；UID/testId `10650a83-d6e3-864b-9082-7267b6205d01`，executionId `6e785139-84ed-4e02-956d-e11c8b48ef84`，runId `32012768-2dbd-4e66-8fa0-0e362d8ffdca`，duration0.6094071秒。诊断04:04:31.719 Executing→04:04:32.422 Executed；失败栈落AssertCombinedPixel及准确public方法，stdout/TRX/诊断均有同一raw，足以证明真实体及取色，不仅凭TRX分母1。stdout中文替代字符，UTF-8 TRX明确为像素断言失败。

## 身份分层

- 监督test SHA256 `CB821F7F92434D636BA8E98AFA44F6C11E9E15F1BDD907E5DC9E6797D2460FA7`，MVID `d947710e-6534-4473-8ce1-285381190852`，与既有clean报告一致。
- 监督/诊断runtime `.NET 10.0.13`、X86；aot-identity PointerSize4。实际runtime位置为本task根System.Private.CoreLib.dll；runtimeconfig两框架10.0.13是包声明，不冒独立WindowsDesktop模块枚举。
- 初始化候选文件为本task根 `Native/wpfgfx_cor3.dll`，SHA256 `20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D`、sandbox=true。这是文件架构/哈希校验，非actual加载候选证据；candidate运行0。
- 源码OriginalWpfTarget行仅由Visual.Assembly.Location推导wpfgfx文件路径并hash，不是实际模块枚举。**本次stdout/TRX及已读诊断没有这条OriginalWpfTarget身份输出**，也无Requested/Actual模块记录；不推造路径/SHA。现源码、离线方法存在与运行体一致，不足证明包体逐行等同当前源码。历史direct actual official `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834` 不嫁接本次；actual official模块路径/SHA、PresentationCore实际SHA均不足。没有补代码或重跑补录。

## public视觉、调度、像素和清理证据范围

源码合同是public HwndSourceParameters：32×16、位置40,40、WindowStyle0x90000000(WS_VISIBLE|WS_POPUP)、ExtendedWindowStyle8(topmost)，DrawingVisual以RGB12/34/56填充Rect(0,0,32,16)，设置RootVisual。它是另一个managed target，不是既有direct同target，也无public D3DImage/SetBackBuffer；不能据方法名中的Hardware推断actual硬件backend。

源码在专用STA GetMessage/TranslateMessage/DispatchMessage线程创建HwndSource，经Dispatcher ContextIdle回调置event、最多等10秒，DwmFlush断言0，再GetDC/GetPixel(16,8)一次。执行实际到达取色，按现有顺序支持等待与DwmFlush前置已通过，但无独立阶段时间戳/窗口句柄、显示可见性、内部target/frame ID/backend/Present日志。ContextIdle不是帧栅栏，DwmFlush也不证明指定target内容已呈现。ClassInitialize通用WM_PAINT文字不能冒public HwndSource自身只ValidateRect的证明。

像素raw `001E1D1C` 与expected `00563412` 不同，原helper逐RGB通道≤1未改；首通道差10已失败。唯一DC读取，无颜色循环、重发或额外采样，不以同值于历史direct就认定同一根因。

源码finally ReleaseDC（未检查返回值）、owner线程source.Dispose，ClassCleanup PostQuit/Join；本次失败结果未附cleanup异常，进程正常退出，无kill/drain问题。**无独立Dispose完成、ReleaseDC成功、线程join完成或refs/资源计数日志**，只能报告既有清理路径及未报告异常，不冒direct四refs0/owner销毁凭据。

## 停止及唯一下一步

已经停止，public D3DImage/candidate/full/resize/backend均未认证。当前内容控制和actual模块身份都未闭，不能转为成功后的D3DImage实现门，也不追加显示基础设施调查。

**唯一下一步：主线转向已有真实WPF应用的最小public D3DImage消费者内容合同，并先由主代理取得可验证实际加载身份且官方内容可见的既有运行入口/证据，再另授权实现门；此前冻结当前沙箱window-DC consumer验收，不再扩展或重跑direct/native/WGC/等待/消息探针。** 这是明确的真实调用方转向和入口前提，不是判定本次失败皆为环境，也不授权本子任务实现或采样。
