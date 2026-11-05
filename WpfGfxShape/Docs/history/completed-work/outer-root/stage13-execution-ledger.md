# Stage13 本次接续执行账

## stage13-r2 当前接续（本节优先，历史不覆盖）
文档收尾：正式report38行分1–13/14–38回读；nextsession/currentitem/currentbaseline/planstatus/finalmatrix/完整方案/规则七份各最高Stage13新节已逐份回读，7修改全部成功，旧历史未删。ledger1–15长读截断后10–15补读；本节为逆序追加即时流水，以下“当前余额/待读”均指当时，不覆盖最终余额0与已读完成事实。无新增源码修改、无额外unit/沙箱/脚本/委派。
Nativepublish1实际参数与预注册一致，commandLine=dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64 -f net10.0 --self-contained true -p:RuntimeFrameworkVersion=10.0.13 -p:PublishAot=true -p:WpfGfxShapeManagedAcceptance=false -p:WpfGfxShapeRuntimeHeapDiagnostic=false -p:WpfGfxShapeDiagnosticAbi=false -p:PublishDir=d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\independent-media-stage13-final-native-x64\ 。原return：执行成功。完整日志共 61 行，可使用 read_last_log_lines 按行读取。52–61已读，58 Generating native code/59 lib-exp/60正式发布路径；DLL/PDB已列实存。数值exit未公开，不捏造；成功编译，不等Native媒体运行。SHA/binary exports/aggregate warnings/errors Unknown。
本接续实际normal1/tests0/COMpublish1/sandbox1/Native1；implicit publish compile2（托管+Native）独列；余额全0。全stage13历史normal前已用2+本次1=3，tests前2+本次0=2，COM前1+本次1=2，sandbox前1+本次1=2，Native1；76pure旧源码，不标本次全回归。首次system1failed仍保存，不覆盖、不追跑。有限slice CloseoutCompleted / OverallOpen。
最终system强门已证：supervision40行全文/child.trx26行全文/stdout12行全文/stderr空/identity1行全文/diag50–65尾已读，child ExitCode0/TimedOut=false/OutputCollected=true/TRX1passed0failed。FindEntriesByName漏列child.trx但准确路径实读成功，不称缺件。CloseReopen48行全文：opened2/frames2/refs0/descriptorDisposed/registryRemoved真。六帧case各110行已分1–65/66–110完整读（callback-close/release/external-close/callback-shutdown/exit/external-shutdown）：accepted0/pending真/latePackets0/lease0000FFFF0000FFFF/frames6/refs0/registryRemoved/descriptorDisposed/threadExited真，readerFinalReferences0/MFShutdown0/CoUninitialize，sample/buffer release7，buffer.Release1及nativeReferencesUnknown保留。opened-shutdown/opened-exit各53行全文，0frames/refs0/同顺序、cleanup完整，Exit owner仍运行真。missing两JSON各21行全文，failure0x80070002/refs0/cleanup=null。
wrapper SHA C16D92A1F53654574280FF701B2CF0FD64F082C2B138142805F1BB43FD21D2A9/MVID add03c35-1483-4ee8-9dd6-f329ca09011e；source-linked业务SHA5599CA20760B43506DCED932EFCF7E60A080D3B16527EC386518D45482C87CBF/MVID2bb3f65e-07cc-4ebc-985b-c4e3cbae8a72；旧fixture6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403仅初始化；X64/pointer8/.NET10.0.13。真实握手callback-enter→close-accepted→callback-exit→owner-completed；callback等待/返回/owner等待各10s，250ms仅late观察非握手sleep。官方WmpPlayer854–925已读，Exit只代理Shutdown，当前禁止后续操作为替换安全策略而非宣称官方逐方法合同一致。
预注册生产Nativepublish1：绝对Code/WpfGfxShape/WpfGfxShape.csproj；Release/win-x64/net10.0/selfcontainedtrue/runtime10.0.13/PublishAottrue/ManagedAcceptancefalse/RuntimeHeapDiagnosticfalse/DiagnosticAbifalse；默认AssemblyName wpfgfx_cor3/NativeLib Shared，独立artifacts/independent-media-stage13-final-native-x64。只编译不当媒体导出运行。
sandbox1已实际返回：task=8d6b8b0a34a946b3957fdd78bebcf7f7；原回执ForegroundTopology supervisor-before-child Caller9692、Owner5532/mstsc、远端C:\CodingAgentSandbox\Tasks\8d6b8b0a34a946b3957fdd78bebcf7f7，结果已保存到绝对WpfGfxShape/artifacts/independent-media-stage13-r2/readback。回执无独立exit字段，不从“保存”推断成功；待读supervision/TRX。参数与预注册完全一致，outputRelativePath省略=全回收；无脚本/委派。当前normal0/tests0/COMpublish0/sandbox0/Native1。
COMpublish1已实际成功：绝对目标WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj；Release/win-x64/net10.0-windows/--self-contained true；RuntimeFrameworkVersion=10.0.13/WindowsDesktopFrameworkVersion=10.0.13/PublishAot=false/WpfGfxShapeManagedAcceptance=true/ComAcceptanceSandbox=true；AotDllPath=绝对artifacts/independent-media-stage8-r1/clean/Native/wpfgfx_cor3.dll；PublishDir=绝对artifacts/independent-media-stage13-r2/publish/，PackageDirectory=同根clean/，两者兄弟互不包含。原return：执行成功。完整日志共 60 行，可使用 read_last_log_lines 按行读取。50–60实读，58正式dll/59publish路径。正常implicit compile1，不是no-build；aggregate warn/error与数值exit未在工具回执公开，Unknown。clean runtimeconfig全文21行确认NETCore及WindowsDesktop均10.0.13。起始fixture实存，仅旧Native初始化输入，非最新Native认证。当前余额normal0/tests0/COMpublish0/sandbox1/Native1。
预注册唯一sandbox：sourceDirectory=WpfGfxShape/artifacts/independent-media-stage13-r2/clean；exe=WpfGfxShape.ComAcceptance.exe；args=[--supervise-exit,Results/stage13-r2-supervision,120,FullyQualifiedName=WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenStage13ShutdownExitAndReopenRunThenRealLifecycleCompletes]；timeoutSeconds=300；完整任务回收artifacts/independent-media-stage13-r2/readback。
起始normal1/tests0/COMpublish1/sandbox1/Native1。正式report已先写入历史normal3851warn0err/76pure与首次system1failed握手死锁及callback-enter修正未验事实。读取Identity BeginClose/Exit/Release/ABI：Resolve返回托管强引用，self storage仅vtable；未证合法持ref调用的确定UAF，不实施未验证重构。cleanup异常与Exit后State保留有限边界，不能称全合同认证。
normal1参数：targetPath=d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.Tests\WpfGfxShape.Tests.csproj,configuration=Release,runtimeIdentifier=win-x64,targetFramework=net10.0。
原return：执行成功。完整日志共 62 行，可使用 read_last_log_lines 按行读取。50–62已实读，58=3851warnings/59=0errors/61=19.40s。工具未单独公开数值exit，成功回执；未造exit数字。当前余额normal0/tests0/COMpublish1/sandbox1/Native1。未跑unit，旧76不冒新回归。日志超过4000的源码读取截断已针对关键段重读；FindFilesMatchingPattern误把文件当目录两次均返回目录不存在，未执行/不耗预算。fixture Native目录已列实存。

状态：执行中，未认证。用户给定前序 normal1 Release 3843 warnings/0 errors，tests1 74 passed；EventProxy Dispose 移锁外后未验证，均非本次实测。

本次起始余额：normal2 / tests1 / COMpublish2 / sandbox2 / Native1。禁止终端脚本、委派；仅实际工具与 Windows 沙箱。

已调用（未消耗构建/测试预算）：
- get_projects_in_solution {}：返回根 Microsoft.Dotnet.Wpf.sln（非目标子方案）。
- ListDirectory {directoryPath:d:\\lindexi\\Code\\dotnetcampus\\WpfLab\\WpfGfxShape,maxResults:100}：成功。
- FindEntriesByName {query:stage13,maxResults:100}：唯一 Docs/completed-work/replacement-independent-media-shutdown-stage13.md。
- ReadFileLines stage13 1–260：实际全文22行，已读完，草稿预算与 Exit 语义待校正。
- FindEntriesByName Identity WpfGfxShape / ListDirectory Docs recursive / ListDirectory .github recursive：成功。
- ListDirectory WpfGfxShape：目标子目录包含 Code/Tests/Docs/Ledger。
- ReadFileLines IndependentMediaIdentity 1–260：回执截断于98行，后续必须分段重读。
- FindEntriesByName stage13 .coding-agent：无。
- ListDirectory WpfGfxShape/Docs：成功。
- FindEntriesByName EventProxy：定位生产与官方实现。
- get_files_in_project WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj：成功；实际系统入口 IndependentVideoPlaybackOwnerTests.cs。

## 本次 normal1 与 tests1
normal参数：targetPath=d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.Tests\WpfGfxShape.Tests.csproj, configuration=Release,runtimeIdentifier=win-x64,targetFramework=net10.0。回执执行成功62行；读54–62：3851 warnings/0errors/17.71s。
tests参数：targetPath同上，filter=FullyQualifiedName~IndependentMediaDurationTests|FullyQualifiedName~IndependentMediaIdentityTests|FullyQualifiedName~GeneratedMediaPlayerResourceTests|FullyQualifiedName~SystemVideoSampleTests|FullyQualifiedName~MediaCreationUnavailableTests|FullyQualifiedName~IndependentVideoFrameLeaseTests|FullyQualifiedName~MediaComEventBridgeTests|FullyQualifiedName~IndependentMediaStatusTests|FullyQualifiedName~IndependentMediaScrubbingTests|FullyQualifiedName~IndependentMediaLifecycleTests。
回执执行成功62行，读1–9/50–62：exit0/76passed/0failed/0skipped/76total/45ms，原74+终态重绑拒绝2。implicit Debug1独列。stdout1609850字符，前1589850被截断；warning总数Unknown。不再定向测试。
当前余额normal1/tests0/COMpublish2/sandbox2/Native1。无终端脚本/委派。

COM预注册参数：绝对COM csproj，Release/win-x64/net10.0-windows，selfcontained true，RuntimeFrameworkVersion10.0.13/WindowsDesktopFrameworkVersion10.0.13，PublishAot=false/WpfGfxShapeManagedAcceptance=true/ComAcceptanceSandbox=true；显式AotDllPath=绝对WpfGfxShape/artifacts/independent-media-stage8-r1/clean/Native/wpfgfx_cor3.dll（已列实存，仅fixture非最新Native认证）；PublishDir与ComAcceptancePackageDirectory为stage13-final-managed/publish与clean兄弟。新桥FQN=WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenStage13ShutdownExitAndReopenRunThenRealLifecycleCompletes，public TestMethod，Stage3Production ProjectReference已静态核入。
