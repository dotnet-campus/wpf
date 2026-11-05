# 独立视频帧 lease stage3：r2 实际消费通过 / 完整替换 Open

## r2真实终态（覆盖下方r1当前措辞）
LimitedFrameLeaseValidated / CloseoutCompleted（仅本功能slice）；ProductionPlayerOpen / OverallOpen。原r2系统执行已完成；本次仅亲自读写证据与文档，主代理监督，无终端、脚本或再委派，源码/build/tests/publish/沙箱均0。不是完整player或完整替换验收。

### 实际任务与证据
- task：`a66918ecb3ee4271a47465717741d597`；远端 `C:\CodingAgentSandbox\Tasks\a66918ecb3ee4271a47465717741d597`。
- sourceDirectory=`WpfGfxShape/artifacts/independent-frame-lease-stage3-r2clean/clean`，exe=`WpfGfxShape.ComAcceptance.exe`；arguments=`[--supervise-exit, Results/independent-frame-lease-stage3-r2, 120, FullyQualifiedName=WpfGfxShape.ComAcceptance.IndependentVideoFrameLeaseTests.WhenRealMfFramesAreLeasedThenManagedProductionBitmapConsumptionSurvivesReplacementAndClose]`，外层180秒，整任务回收到同artifact的`readback`。调用精确参数与原return存`.coding-agent/stage3-r2-sandbox1-receipt.json`。
- supervision 40行完整亲读：2026-10-30T09:43:39.3628813Z..09:43:40.1378248Z，supervisor10424/child11448；child ExitCode=0，Reason=exited，TimedOut/Cancelled/KillAttempted=false，OutputCollected=true，两个drain timeout=false，SupervisorError=null。工具本身未给父进程exit数值，不补造。
- stdout12行：1 total/1 passed/0 failed/0 skipped，480ms；stderr空。前代理已完整读取diagnostic65行（PassedTestNodeStateProperty/session exit0）。本次按stdout绝对路径及监督child的--results-directory直接映射，亲读`WpfGfxShape/artifacts/independent-frame-lease-stage3-r2clean/readback/Results/independent-frame-lease-stage3-r2/TestResults/child.trx`全部26行：TestRun=428f8cbc-1e33-4df7-8a5b-e32d63f437c4，指定独立lease用例Passed，total/executed/passed=1，failed/error/timeout/aborted/notExecuted=0，ResultSummary=Completed。TRX原件可得，先前目录枚举漏报不等于缺失；撤销“TRX归档缺口”结论，未重跑或补造原件。
- 前代理已完整读取业务367行、identity/监督/stdout/stderr及用例源码60..103；本次亲读业务result.json全部1..367行、identity单行、监督40行、stdout12行和TRX26行。close后新borrow两处0x80000013、重复Close/Dispose及调用顺序依据前代理源码核读与本次指定用例Passed，不冒这些字段直接存在业务JSON；业务原件直接记录四次内容及最终Release0。未重复读取二进制或把DLL当文本。

### 内容、时序与生命周期
- 实际frames数组21项（原件没有framesDecoded字段，以数组实际count=21报告），index0..20，100ns time0..20000000，步1000000；每帧2×1、signed stride=-8，aperture(0,0,2,1)。未读完整40帧/EOS，不借用stage2数字。
- 通过真实资源绑定Acquire及MIL source slot3 GetSize/slot4 format/slot7 CopyPixels：2×1、format16/PBGRA32。旧lease Timestamp0，pixels=`0000FFFF0000FFFF`，sha256=`B978F97A664CC8FAE883C8185C0E72E4C16C3BE1FB8901B5306B20FDB24A34B4`；新lease Timestamp20000000，pixels=`FF0000FFFF0000FF`，sha256=`42AF801193FAD22A2C6B98D9FE22F22D9C00F8F927539F65D98A41178FA31142`。替换后先读旧红/新蓝，owner.Dispose后再次旧红/新蓝，四条observation内容/hash一致。
- 用例源码60..103及成功状态共同证明：owner.Acquire和resource.AcquireSystemFrame在owner close后均返回0x80000013；旧lease两次Close、新lease两次Dispose，额外AddRef独立最终Release，finalOldReference=0/finalNewReference=0。lease自有bitmap像素，不依赖owner继续存活；resource只借绑定外部owner、不负责关闭owner。Source借用不能跨并发lease.Dispose，无并发压力认证。
- Cleanup原样：currentType.Release0/nativeType.Release0/attributes.Release1；Unlock HRESULT0、buffer.Release1、sample.Release0各21。buffer返回1不是泄漏结论或强填0，sample持有的引用随sample释放；reader.Release0/ReaderFinalReferences0，MFShutdown真实HRESULT0一次，CoUninitialize一次。UnlockCount/SampleReleaseCount/BufferReleaseCount均21，ShutdownCount/UninitializeCount均1。业务completed=true；不是全进程memoryGreen证明。

### 身份与所有边界
- .NET10.0.13/X64；SYSTEM32 mfplat.dll及mfreadwrite.dll均10.0.26100.9278。输入SHA=`01BE17E50794DD8A076967E49D0C2CD6265A3908247748B4B95689AB2F07C872`。
- COM wrapper SHA=`608A604C7C98AD1DD9303128EFAFC71106E2D472CF841C6B4BA23091CC14F94B`，MVID=`abf76ff3-21c7-4eff-9160-a3f021ee5a2b`；source-linked Tests SHA=`E2D7496C4F16CDE1430A4F7FF8B6CA073D2B54B4431D472C4FB06FDC6BBA12F8`，MVID=`03345bdd-83fe-47e6-a660-e7ce88277de0`；managed product SHA=`E6F5A1431C33058789A553197D03939EF715D876FA6DD0B51AD716C55539D256`，MVID=`548cc616-7e5b-42ec-b1ec-34eaf1e7ce0f`。三个身份不能混用。
- fixture identity SHA=`6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403`，Native/wpfgfx_cor3.dll，仅旧fixture初始化，不是新生产媒体NativeAOT。默认生产AssemblyName=wpfgfx_cor3/NativeLib=Shared/PublishAot=true不变；可选ManagedAcceptance自有托管命名不代表替换官方模块。
- r2恢复合法调用方COM provider QI/transport/Begin-End scope合同，不恢复候选主动回载官方媒体服务。SystemMediaModule禁止恢复；MediaExports仍E_NOTIMPL/null。下方r1“对所有provider拒绝”及“旧合同4failed”仅历史，不代表当前。
- r2此前tests1=18 passed（原合同12+negative4+owner2，隐式Debug1）；四旧fail修复，不改expected/skip/像素阈值。本次不无必要重复。
- 仅CPU software managed production bitmap-source消费，不是Renderer/DrawVideo/RTB实际绘制、硬件/零拷贝、完整player、生产Native导出或双架构/full验收。未调用的MSHResult/Begin-End不填0。没有已认证直接player绘制入口，本次不新建假player、不用第二sandbox冒充接入。

### 预算与下一直接工作
r2累计normal1/3（3725warnings/0errors）、tests1/2（18passed，implicitDebug1）、COMpublish1/2成功（implicitcompile1）、sandbox1/2、Native0。其中sandbox1为原r2执行；本次文档收尾新增normal0/tests0/publish0/sandbox0/Native0。账面余normal2/tests1/COMpublish1/sandbox1不构成本次执行授权。r1 normal3/tests2/COMpublish2失败/合同4failed等历史完整保留下方，不冲销。
下一在现有生产层实现独立player COM identity/注册与MTA串行状态owner，把SourceReader→IndependentVideoFrameOwner绑定到GeneratedMediaPlayerResource，由生产时钟调度100ns PTS及帧失效事件，先接现有Renderer.DrawVideo/RTB真实绘制并保持lease活到draw消费结束；验证replace/close/取消flush-drain和事件关闭后无回调，再seek/rate、AAC输出同步、NativeAOT/真实WPF/x86+x64/full。不得以此有限闭环宣称下一阶段已完成；旧full失败、资源unknown、memoryGreen=null及完整替换Open不变。
六状态入口与执行规则同步当前有限终态；以下全部为r1历史（包括其中旧“当前/下一/预算已尽/不存在运行”措辞），不覆盖本节。

# r1历史：实现草拟 / 验证受阻

## r2开始快照
r1全部执行/失败账保留如下，不冲销。补齐六状态真实ValidationBlocked后继续生产闭环；r2预算normal≤3/tests≤2/COMpublish≤2/sandbox≤2/Native0。禁止终端/脚本/委派；仅实际工具与Windows沙箱。四回归初核：资源层对所有外部provider返回E_NOTIMPL过度，不是撤销原版主动回载的必要条件；逐项ABI依据与结果将在本节追加。r2已完成normalbuild1（3725 warnings/0 errors）、tests1（隐式Debug，18 passed）、COMpublish1（成功）；已发布independent-frame-lease-stage3-r2clean/clean，当前待首次实际sandbox，不重publish。aot.sha256=6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403；runtimeconfig两框架10.0.13，正式exe存在，包装源码确认namespace WpfGfxShape.ComAcceptance。执行账没有已有r2 task记录；本次亲自执行，不委派。

## 当前结论
ImplementedDraft / ValidationBlocked / OverallOpen，**本轮没有完成用户要求的真实生产消费闭环**。不得将本文称为 lease 验收通过或阶段 CloseoutCompleted。生产工厂仍精确 E_NOTIMPL/null；原版 wpfgfx provider、复制改名及 fallback 没有恢复。CPU 软件复制路径，不是硬件或零拷贝。MP4 r1失败/r2无声诊断通过、所有历史 full失败、资源 unknown 和 memoryGreen=null 原样有效。

## Before / After 与实际源码
- Before：`SystemVideoSample` 仍通过 provider IID `E6F1CC74-A0EB-4EBE-8241-089D1CB079D7` 查询外部 renderer；资源命令也执行 provider QI。`SystemMediaSource.Resolve` 恒 null，独立 MF 解码没有自有 bitmap lease。
- 新增 `WpfGfxShape/Code/WpfGfxShape/Core/Av/IndependentVideoFrameOwner.cs`（internal）：Publish 复制并裁取合法整数 visible aperture，固定尺寸和100ns时间，输出 top-down 紧凑 BGRA/PBGRA opaque 像素；锁内替换当前不可变快照。Acquire 每次创建独立现有 SoftwareBitmap，写锁填充后释放锁、Protect 为只读，QI 取得既有 MIL source 视图。无新接口 IID/vtable。
- 重写手写 `SystemVideoSample.cs`：仅持有自有 source 引用和 Timestamp；Close/Dispose 串行一次 Release。旧 nint Acquire 对非零媒体返回 E_NOTIMPL，对 null 保持 E_INVALIDARG，绝不调用外部 provider。
- 手写 `GeneratedMediaPlayerResource.cs` 新增 internal BindIndependentFrames 并由 AcquireSystemFrame 取得独立 lease；release 清理借用 owner 绑定，不负责 Dispose 外部 owner。原命令在真实类型 Resolve 为 null 时返回 E_NOTIMPL，不再进行 provider QI。独立 player 的真实 COM identity/注册仍未实现，Bind 尚无生产 player 调用方；仅建立可调用内部消费路径，不能称默认 DrawVideo 已运行。
- `WpfGfxShape.Tests/IndependentVideoFrameLeaseTests.cs` 复用同 partial 原创AVI生成器，新增真实系统用例与两项无系统调用关闭边界；COM 新增包装用例，通过带别名的现有 Tests ProjectReference 调用同一真实测试。不增加工程/包/public生产测试接口/InternalsVisibleTo，不编辑 .g.cs。
- 生产 csproj 增加可选 `WpfGfxShapeManagedAcceptance=true`，仅自有托管测试程序集名改为 WpfGfxShape.ManagedAcceptance，避免自有 wpfgfx_cor3.dll 与 Desktop runtime 原生同名发布冲突；默认生产 Native 名称保持原值。不是改名复制原版 DLL。COM 引用项目方案尚未得到合法 clean 包运行认证。

## ABI 核读与证据边界
本轮亲读 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/av/internal.h`35–85：IUnknown 后 slot3 BeginComposition(CMilSlaveVideo*,BOOL,BOOL,LONGLONG*,BOOL*)；slot4 BeginRender(CD3DDeviceLevel1*,IWGXBitmapSource**)；slot5 EndRender()；slot6 EndComposition(CMilSlaveVideo*)，均 HRESULT。独立 CPU lease 不伪造这些 begin/end 调用，更不把它们当 Release。
亲读项目 SoftwareBitmap.cs/Mil.cs 和 WicBitmapAdapter.cs 的创建与 table：MIL source slot3 GetSize、slot4 MIL格式整数（不是WIC GUID）、slot7 CopyPixels；复用已有 MIL IID dd0bf622-0650-4a1e-b20f-4b4ab6edfca3。GeneratedVisualRenderer 的 DrawVideo 实际取 sample.Source、slot3尺寸、AddRef后进入 draw list；本轮**没有执行该 Renderer/DrawVideo/RTB 绘制**。
SourceReader 原始 signed stride 已在 Read 展开为 Width×Height×4 top-down；owner 接收该已展开数据而非 raw MF buffer，以 DisplayX/Y/Width/Height 裁取 visible aperture。当前 Publish 无 subtype参数并强制 alpha255，透明 ARGB32语义未验证，不能声称通用 alpha 支持。分数 aperture仍由 SourceReader拒绝；MP4 coded640×368/visible640×360是历史 r2实证，不是本阶段新运行。

## 精确 owner 表
|资源|取得与持有|释放/关闭合同|
|---|---|---|
|MF sample/buffer|既有 SourceReader.Read 同线程锁内复制|既有 finally Unlock、buffer/sample Release，本轮未新增系统运行证明|
|owner当前快照|Publish 复制输入并裁 aperture，数组不向外暴露|锁内原子替换；Dispose清当前并禁止后续获取，已开始获取仍持有局部强引用|
|每lease bitmap|Acquire 新 SoftwareBitmap，自有像素副本；写锁填完后Protect只读|Release写锁后取得MIL source QI引用，释放创建引用；失败 finally释放部分引用|
|MIL source|SystemVideoSample持有一个引用，Timestamp固定|Close/Dispose一次Release；下游额外AddRef必须独立Release，Source借用有效期不跨并发Dispose，调用者须保持lease存活|
|resource绑定|仅强引用外部owner，不转移owner关闭职责|resource释放清绑定；关闭owner拒绝新lease但已借lease自有bitmap不受影响，运行待证|

## 预注册用例与实际状态
真实用例 `WpfGfxShape.ComAcceptance.IndependentVideoFrameLeaseTests.WhenRealMfFramesAreLeasedThenManagedProductionBitmapConsumptionSurvivesReplacementAndClose`：固定AVI前21帧经真实MF发布；resource Acquire旧PTS0红帧、新PTS20000000蓝帧，MIL slot3/4/7读回2×1/PBGRA32，关闭owner后旧lease仍红，重复Close/Dispose与额外AddRef最终Release检查；业务JSON记录test/product SHA、MVID、读回hash、cleanup。**仅编译，未执行**。当前只设计到真实 bitmap-source COM CopyPixels，并非 Renderer绘制；在途并发压力、分数aperture、透明alpha、帧驱动事件、完整renderer及player注册仍缺。
无系统依赖两项 owner关闭边界及四工厂negative实际6passed。旧 provider stub资源合同回归4failed，未修改原expected/阈值/skip掩盖。

## 实际工具执行账
未使用终端/脚本/委派，程序未在本机启动（仅允许的专用构建/定向纯测试）。预算 normal3/3、定向tests2/2、COMpublish2/2、sandbox0/2、Nativepublish0；implicitDebug2、publish编译2另列。
1. normal1：绝对Tests项目 Release/win-x64/net10.0；工具回失败，CA2208 ArgumentException参数位置，1494warnings/1error，39.78s。63行日志读尾及错误搜索，未保存首部exit数值，exit数字Unknown。改为ArgumentException(null,nameof(bgra))后复验。
2. normal2：同目标，exit0、3718warnings/0errors、8.96s；62行日志读首尾。stdout3116467chars，前3096467截断。
3. tests1：绝对Tests，无配置参数工具默认Debug，filter IndependentVideoFrameOwnerTests|MediaCreationUnavailableTests；exit0，6passed/0failed/0skipped，20ms；implicitDebug1。stdout1558864chars，前1538864截断，warning总数Unknown。
4. COMpublish1：绝对COM Release/win-x64/net10.0-windows、self-contained10.0.13、ComAcceptanceSandbox=true、显式fixture路径、全新 artifacts/independent-frame-lease-stage3-r1/publish与clean，PublishAot=false。exit1 NETSDK1152（自有托管wpfgfx_cor3与Desktop runtime同名），59行日志读首尾；stdout1617739chars前1597739截断，warnings总数Unknown。新增仅自有程序集可选条件名，未禁用重复输出检查。
5. COMpublish2：同合同加WpfGfxShapeManagedAcceptance=true，全新stage3-r2目录；exit1 MSB3030，显式历史fixture `WpfGfxShape/artifacts/owned-desktop-full-x64-r1/clean/Native/wpfgfx_cor3.dll` 不存在，随后目录读取确认不存在。59行日志首尾已读；stdout1617619chars前1597619截断，warnings总数Unknown。**未形成合法clean包；不再发布追绿，未随意搜DLL/替代fixture，不冒已核6BDF同SHA。**发布前未核fixture存在导致预算浪费，是本轮执行缺陷。
6. tests2：filter GeneratedMediaPlayerResourceTests|SystemVideoSampleTests，exit1，12total/8passed/4failed/0skipped，33ms；implicitDebug2。88行可见日志全部读取；stdout618478chars前598478截断。失败：WhenProviderIsAcceptedThenResourceRetainsMediaUntilDeleted；WhenQueryFailsWithOutputThenFirstErrorAndCleanupArePreserved；WhenBatchFailsThenUnexecutedPacketReferenceRemainsWithCaller；WhenQueryReturnsNullThenNoInterfaceAndTransportReleaseResult。均旧QI/接受合同与新E_NOTIMPL冲突，实际transport Release一次，非提前伪造零引用。原expected未改，回归Open。
7. normal3：绝对COM Release/win-x64/net10.0-windows，exit0、3867warnings/0errors、8.83s；64行日志首尾已读；stdout3233522chars前3213522截断。构建不代发布或系统运行。

不存在本轮sandbox task/TRX/supervision/业务运行JSON、真实product/test identity/hash/sample时序或COM Release运行回执，全部NotExecuted/Unknown，不填0，不借用历史6BDF/B07C或MP4诊断身份。本轮正式日志工具最后存储仅保留最后构建；上表保存先前亲读回执，不声称原件均已归档。

## 回撤与下一直接工作
仅回撤本阶段自有owner/lease、资源绑定及测试/可选命名配置；不得恢复任何原版provider/loader或成功工厂。当前不是可验收稳定基线。
下一唯一目标仍为完成stage3真实lease/bitmap consumer闭环：先在既有证据清单核合法已验收fixture实际路径及SHA并修正打包身份（不得泛搜DLL），在新增发布授权后形成正式clean包、执行上述唯一系统用例并完整回读TRX/监督/业务身份/逐帧内容和清理；同时建立真正独立player owner注册到手写资源，处理旧provider合同测试迁移而非改expected追绿，再跑Renderer/DrawVideo/RTB至少一条生产绘制。现预算已尽不可擅自重发。clock/事件/seek-rate/AAC输出与同步/取消flush-drain仍未实现；不另开可疑player stub支线。完整替换Open，旧full和资源unknown继续阻塞。
