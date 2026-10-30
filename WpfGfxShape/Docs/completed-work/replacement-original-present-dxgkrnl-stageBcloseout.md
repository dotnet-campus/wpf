# DxgKrnl 有限门阶段B：发布、原采集与独立收尾

## 1. 终态与预算

Publish=ExecutedPassed；Capture=Executed.SuccessfulControllerPath；Business=ExecutedFailed.NativePrerequisitePixel；Readback/Closeout=Completed；Offline/TDH=NotRun。166/171/172及D3D9实际事件数、schema、关联均Unknown，非0。完整替换及原shader/实际resize仍Open，根因Unknown。

本阶段亲自COMpublish1、Windows沙箱1、原精确体1；normalbuild0、源码/项目修改0、Native0、独立发现0、重试0、official0、x64/full0、终端脚本0、再委派0。发布自身编译不冒额外normalbuild。阶段A build1/133warnings/0errors独立保留。运行内枚举1822项不是额外发现任务，TRX只执行1。所有旧ETL、失败及历史账/双架构有限绿保留、不移绑。

## 2. 路径与发布工具事实

工作区 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape`；以下R=`WpfGfxShape/artifacts/original-present-dxgkrnl-x86-r1`，审计账B=`WpfGfxShape/artifacts/original-present-dxgkrnl-stageB`。

准确发布命令逐字存B/publish-request.json，唯一调用：

`dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c OriginalPresentDxgKrnlDiagnostic -r win-x86 -f net10.0-windows --self-contained true -p:RuntimeFrameworkVersion=10.0.13 -p:ComAcceptanceSandbox=true -p:AotRuntimeIdentifier=win-x86 -p:AotDllPath=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/postfailure-source-rebuild-x86-r1/native/wpfgfx_cor3.dll -p:ComAcceptancePackageDirectory=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/original-present-dxgkrnl-x86-r1/clean/ -o WpfGfxShape/artifacts/original-present-dxgkrnl-x86-r1/publish/`

工具即时原返回“执行成功。完整日志共 60 行，可使用 read_last_log_lines 按行读取。”存B/publish-return.json。实际日志1–60已亲读，exit0；原stdout52788字符仅尾20000可见，前32788不可得，行8残行；warning总数Unknown，不挪阶段A的133到本发布。可见MSTEST0044/0045/0037/0017、CA2007/CA2014。无独立stderr段，不认证发布stderr为空。参数/截断账见B/publish-log-head.txt、preflight.md。

clean/aot.sha256全文15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620；runtimeconfig includedFrameworks NETCore/Desktop10.0.13；deps runtimeTarget net10/win-x86、两runtimepacks10.0.13。根正式exe存在；package-files头确认根入口/测试依赖，PackageSelfContainedComAcceptance的逐依赖Exists门随发布成功；未冒清单所有文件逐个审计。Native和根DLL均存在；未运行publish目录。

执行工具参数/即时原返回分别原样持久化request/return JSON；文件工具返回未提供独立原返回文件路径，读取参数/可见结果按批存tool-ledger/read-receipts/readback-receipts。**未做到所有文件工具原返回逐字即时独立归档**，不伪称完全满足该要求。原件未修改，执行原返回/准确命令均保存；不可见stdout不伪造。

## 3. 唯一沙箱与原件完整性

B/sandbox-request.json保存实际参数：source R/clean；executableRelativePath=`WpfGfxShape.ComAcceptance.exe`；arguments=`["--supervise-exit","Results/original-present-dxgkrnl-x86","120","FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive"]`；timeout180；outputRelativePath=null整任务回收；localOutputDirectory=R/readback。无斜杠filter、额外发现或重试。

即时原返回“沙箱执行完成。结果已保存到：d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\original-present-dxgkrnl-x86-r1\readback”存B/sandbox-return.json。工具本身未返回数字exit/task/timeout；这些由回收原件确证，不补造工具字段。task=`e67641eec26c40d3bf78142d7dc107b5`，全任务回收，不只Results。

已亲读R/readback/Results下：aot-identity.json1行；hw-module-Native-75d6a843de1e4a77ba783f32e3f816c4.json10行；session WpfGfxShape-OriginalPresent-b56df1ea692949ed9f4b0f7e93364ff2/setup.json1行及controller.json145行；original-present-dxgkrnl-x86/supervision.json40行、stages.jsonl21行、stdout.txt43行、stderr空、TestResults/child.trx64行、Diagnostic/log_261028080349714.diag120行。按分段读取到尾，无原件未读尾项。初次目录漏列TRX，依supervision精确路径读到，不以列表漏项认不存在。stdout中文显示乱码保留原件，TRX/diag可读同错误。

ETL仅确认目录项存在及controller采后大小/hash，未当文本读、未解码、未本机重新哈希。ETL=327680bytes，SHA256=`045B1CD110EBFB3A7945DF24633BE97954007C7AE5F590F868B8FC8D38DD7CB3`；非空不保证目标166/171/172或D3D9事件存在，离线可读性仍待C。

## 4. 身份、业务与监督

监督6860/子PID5836，2026-10-28T08:03:49.5687866Z至08:03:51.6503226Z；Reason=exited，HasExited=true，ExitCode2/0x00000002；TimedOut/Cancelled/KillAttempted=false；OutputCollected=true，两drain timeout=false，SupervisorError=null。根正式exe自带监督生成原filter/TRX/diagnostic/no-progress子参数，120秒；不是全部系统进程/线程收敛认证。

testSHA=`B1D0B0C76B994732C71228D21BC0FF849D7A776424CCA751EFA3CE8FB1BCC41F`；MVID=`51ccb930-9b99-43c3-b08f-2828e195fbbd`；runtime .NET10.0.13/X86/pointer4；UID=`1eded079-5019-809c-bf7c-07ef4caf0e29`。TRX total1/executed1/passed0/failed1，原体duration1.2205582秒。

文件初始化aot-identity确认15A545；hw-module的Requested=Actual=本task Native/wpfgfx_cor3.dll及同SHA，确认候选实际模块加载，FileVersion1.0.0.0/ProductVersion1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a。**模块加载不等于产品业务覆盖。** 原路径先加载取exports，系统普通d3d9原Present/Complete/Diagnose后前置AssertCombinedPixel失败：00F3F3F3≠00563412，容差原样；产品connect在此断言后，未达候选连接、shader或实际resize。不是跳过、不是环境无权限、不是collector绿代业务绿。

stages清理before/after及complete空串已回读；源码显示连接/通道/两bitmaps/surfaces在该前置失败时尚未创建，循环仍输出占位清理。不得冒真实四refs0或候选资源生命周期全收敛；只确认原device/d3d/window清理路径返回、业务原异常保留。无独立cleanup错误记录，不把占位日志当释放计数。

## 5. 实际ETW控制与精确边界

宽keyword双provider，同一个自有handle0x17，无event-ID专采/目标解析。D3D9 GUID783aca0a-790e-4d7f-8451-aa850511c6b9 level6 any0xf；DxgKrnl GUID802ec45a-1e99-4b83-9920-87c98277ba9d level4 any0x08000000/all0，timeout500ms。descriptor选择来自mutable main，不冒同机schema。

|事实|状态/QPC|
|---|---|
|StartRequested|39957016132203|
|StartTraceW|0 / 39957016149818|
|EnableTraceEx2.D3D9|0 / 39957016160815|
|EnableTraceEx2.DxgKrnl|0 / 39957016161846|
|原Present Before/After|39957016165426 / 39957016171383，HRESULT0|
|原Complete退出|39957020613121，Entered=true/Returned=true/State=Returned|
|StopRequested|39957020622674，Reason=CompleteReturned|
|StopCompleted|39957020729831|
|ControlTraceW.Stop日志|0 / 39957020730425|
|ScopeClosed|39957020738401|

QPC frequency10000000。实际调用PID5836/TID11732，精确HWND0x181049C/device0xD2BCD40，Ex=false；窗口ownerPID5836/TID9932，观察QPC39957015840470，queryError0。窗口生命仅原VisualCache scope观察，不冒创建时间；owner与调用线程不混同。实际swapchain32x16/format22/count1/swapEffect1/windowed1/DPI96/sourceRectNULL/destinationRectNULL/override原HWND均原日志保留。

**Stop原因与顺序双重确证**：controller记录CompleteExit < StopRequested < StopCompleted，源码Complete finally第一处取QPC后CompleteExit锁内一次Stop；所以本次Stop确在Complete返回后，TruncatedBeforeComplete=false，2秒watchdog未先取得Stop。不是由总耗时猜测。StopCompleted在API返回立即取，Api日志QPC稍后记录，非第二Stop。自有session一次Stop0，无按名兜底或重试。锁调度不宣称硬实时；同次kernel/history是否全覆盖仍Unknown。

StatisticsValid=true，EventsLost/LogBuffersLost/RealTimeBuffersLost均0，BuffersWritten5/NumberOfBuffers8；lost0不能推出目标事件有/无或GPU提交/完成。

OpenProcessToken0、LookupPrivilegeValueW0；AdjustTokenPrivileges Bool1/LastError0；PreviousPrivilegeCount1/Attributes0/Bytes16；RestoreTokenPrivileges Bool1 QPC39957016163821、LastError0 QPC39957016163824、PrivilegeRestored=true；CloseTokenHandle0。Errors=[]。仅API恢复成功证据，无恢复后token快照，不冒权限完全状态认证。API权限/cleanup错误本次未见，绝不概括为“无权限”。setup是Start前快照，初始false/null不能与最终controller混用。

## 6. 停止与唯一后续申请

阶段B已停止，收尾无待办。仅申请独立阶段C：复用上述固定ETL/controller及task/hash/bytes/test身份，既有同机正式exe离线TDH解析D3D9锚和166v0/171v2/172v2三类kernel事件；实际schema/RAW_TIMESTAMP/clock/字段/API状态为准，严格同次PID/TID/QPC+HWND及非零唯一token关联。有效零目标事件也如实Unknown，未知schema/桥不足/竞争均不猜。阶段C尚未授权、未实施/编译/发布/执行。

不得重采、扩provider、追加原体或等待追绿。至多kernel Blit发起/history交接，非GPU完成、DWM消费/合成、scanout或像素证明；不能回溯补证旧PID2748旧ETL那次Present。完整替换Open、原业务失败与历史账全部保留。
