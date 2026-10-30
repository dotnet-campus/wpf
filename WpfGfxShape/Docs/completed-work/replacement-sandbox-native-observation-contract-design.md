# 沙箱内原生互操作调查：裁决更正与唯一单次方案

## 0. 本阶段事实与用户纠正

本阶段仅阅读代码、文档、官方 API 合同并修改文档；源码/项目配置修改、normal build、test、COM publish、Native publish、沙箱任务、终端、脚本、再委派 **全部新增0**。下文所有 API 返回均为待测规格，不是本轮结果。没有重新认证包 SHA、运行身份或历史原件。

用户纠正成立：能够在 Windows 沙箱执行编译后的 C# 程序，就能够由程序直接调用其令牌允许的 Windows 管理 API。工具没有 session/desktop 参数，**不能推出程序没有管理权限，更不能推出必须外部维护者**。原 `replacement-real-hw-resize-resumption-decision.md` 的 B/ExternalBlocked 与其外部唯一责任结论过早，现撤销其作为当前排程的效力；原文及所有失败证据保留，不能重写成未曾作出该判断。

当前裁决：**显示内容失败根因未定，沙箱内能力尚未充分核实**。既不认定环境故障，也不认定 candidate 根因。显示执行仍未授权，完整替换 Open、既有有限绿和独立历史预算不变。

## 1. 阅读范围与已有信号为什么不够

已分段读完整 `wpfgfx-complete-replacement-validation-plan.md`（1–181）、当前规则全文、基线最新节、恢复入口/当前事项/状态最新节、real-hw-resize-resumption 全文、public 控制全文、direct clear 全文、WGC退场必要裁决与覆盖节、HN必要合同/实测段，及正式沙箱说明全文。方案§1–10五维、两架构、真实调用方、最终矩阵要求不变。

代码实读：ComAcceptance csproj/global.json（net10.0-windows、SDK10.0.107、MSTest4.0.1）；HardwareClosure原两个无参入口及身份；VisualCache原前置、取色helper；WindowThread全文；OutputWindow创建与提交；PresentDiagnostics全文；HN声明/映射段、KS会话/采样段、JB入口/身份。未穷举全部旧报告/整个源码，不声称重新核证历史二进制。

- 正式 exe/NativeRun/ExitDiagnostics 已有精确 FQN、120秒子进程监督、identity/TRX/阶段回收，足够作为唯一入口，不需要第三工程/Host/脚本。
- 原 resize 的 `DiagnoseNativePresent` 记录当前测试线程 desktop、owner TID/PID、input名称、cover/window/screen颜色，但没有进程 window station 可见标志、真正 owner desktop 的 input属性、input desktop中目标 HWND实际成员关系，也没有 DC裁剪/映射及两线程 DPI合同。
- HN 已有 Session1/Active/UNLOCK/Default、owner名称、CCD active/1024×768/source(0,0)、LUID/monitor/DXGI交叉映射、uncloaked、无no-redirection位、全局DWM timing；KS已有非空DC、非CLR_INVALID、client/screen与Release检查。**不把这些旧值换名再收集当增量**，不重开这些旧入口。它们未把新增信号绑定到当前原resize的同一 HWND/采样点/失败时刻。
- public expected00563412→001E1D1C、direct空root绿expected0000FF00→001E1D1C都是真失败；direct红分支未达、actual backend未知。WGC已退场，不恢复捕获。共同失败不能单归interop或沙箱。
- 已有clean不能凭入口元数据得到下面的新对象关系和DC合同，故零构建重跑旧包不是本方案。只在授权后增加默认关闭的同工程诊断。

## 2. 只检验两个可证伪假设

### H1：当前原resize的真正HWND不在本进程可输入桌面上

不是再问“是否叫Default”。新信号是 `process SessionId + process windowstation(UOI_NAME/UOI_FLAGS) + GetWindowThreadProcessId(HWND) + GetThreadDesktop(actualOwnerTid) + owner UOI_IO + EnumDesktopWindows(OpenInputDesktop(...)) 中是否包含该确切HWND` 的同点关系。

GetProcessWindowStation借用句柄；GetUserObjectInformationW的UOI_FLAGS读取12字节USEROBJECTFLAGS（两BOOL/int32、DWORD，WSF_VISIBLE位），owner/test桌面用UOI_NAME与UOI_IO/BOOL4。OpenInputDesktop(0,FALSE,DESKTOP_READOBJECTS=1)返回独立需关闭的HDESK。枚举callback全程返回TRUE，仅保存目标成员布尔，不保存其他窗口标题/内容，不以不同句柄数值判断桌面不同。枚举成功且包含目标、owner UOI_IO=true、station可见时，H1在该快照被反驳；名字相同/Active/枚举失败均不能反驳。输入桌面在断开的会话也可成功打开，故成功句柄本身不证明连接。

owner PID必须等于本进程；ProcessIdToSessionId分别绑定本进程与真实owner PID（不拿WTSActiveConsoleSessionId作为远程会话必须相等的标准）。在owner Invoke内核实际TID/desktop，而非仅由测试线程推断。两端快照非原子，前后矛盾记瞬态/未定，不推远端全局session结论。

### H2：原GetPixel(16,8)没有满足有效、同一点的DC采样合同

新信号是**原GetPixel同一HDC、同一线程、调用前的裁剪与逻辑→设备坐标映射**，不是新增颜色/等待实验：GetObjectType、GetMapMode、GetWindowOrgEx/GetViewportOrgEx、GetWindowExtEx/GetViewportExtEx、LPtoDP(16,8)、GetClipBox、PtVisible；GetProcessDpiAwareness(HRESULT)、当前线程/owner线程/窗口的DPI_AWARENESS_CONTEXT及GetAwarenessFromDpiAwarenessContext、GetDpiForWindow。GetDC零、CLR_INVALID、区域ERROR/NULLREGION或采样点不在clip内，与“有效DC返回错误内容”分开。

GetPixel参数是逻辑单位，COLORREF为00BBGGRR，00563412就是RGB(12,34,56)；不能以ARGB解释。记录窗口client(16,8)在测试与owner线程各自ClientToScreen所得点；结合DC的LPtoDP与DPI上下文确定是否真指向同一物理内部点。若转换不能可靠建立同点，不比较两颜色，更不能自动改DPI或expected。中心点落入client本身不能证明DPI映射正确。

仅在第一处原native前置采样，额外获取一次screen DC并以其原始上下文ClientToScreen结果及screen LPtoDP/clip记录一次GetPixel，明确逻辑单位与映射；不切DPI上下文、不另读owner颜色。若映射不一致，该screen颜色仅原样诊断，不作为“同物理点”证据。原raw仍由原window DC取得，原断言不改。

H2可被有效DC、点可见、映射一致、CLR有效且两线程坐标合同一致反驳；若这些成立而颜色仍错，只排除该类采样合同错误，不证明D3D/DWM实际交付，也不证明GetDC(NULL)是指定帧的合成/scanout读回。window==cover、client==screen可能仍是任务自身表面或旧输出，**绝不升级为合成消费证据**。

## 3. 唯一待授权实现范围（非本轮修改）

仅在 `Tests/WpfGfxShape.ComAcceptance`：

1. 新 `D3DImageTests.NativeObservationContract.cs`，整文件 `#if NATIVE_OBSERVATION_CONTRACT`。私有P/Invoke、不可变record诊断、单次scope、限目标枚举callback、JSONL落盘，避免复用现有缺SetLastError的声明解释错误；必要同名API用带前缀EntryPoint别名。所有字符串需要用户可读时复用/追加 `AcceptanceResources.resx`，不新增依赖。
2. csproj增加默认false的 `NativeObservationContract` 属性；仅显式true追加同名编译符号，保持 **Release/win-x86/net10.0-windows**，不新增第三配置、不改SDK/TFM/runtime默认、分析器、Native来源或通用打包逻辑。
3. `D3DImageTests.VisualCache.cs` 仅编译条件内三处hook：原resize分支建立诊断scope；EstablishObservableWindow后且D3D创建前作H1/坐标只读快照；原AssertCombinedPixel的GetDC/GetPixel之间记录同DC合同并在取色后保存原raw（只对本scope有效），finally保存ReleaseDC真实返回。第一处前置额外screen读一次，其余原颜色点只记已有raw，不追加采样。原shader、native Present、connect/target/resize/expected/容差/等待/消息顺序不改。scope finally清理，不影响其他用例。

不改HardwareClosure、WindowThread共享实现、PresentDiagnostics、NativeRun、ExitDiagnostics、Tests、生产、Native、WGC或Package.targets。不新public测试入口；通过现有windowThread.Invoke进入真正owner查询，仍仅原无参FQN单体。新增私有JSONL写 `Results/native-observation-contract.jsonl`，每阶段即时写入，包含QPC/UTC/PID/TID/HWND/原阶段，禁止事后把缺字段补成成功。

## 4. 程序直接调用顺序、权限及终止

1. 正式监督器进入原candidate测试，保持原actual native身份日志；scope记录test SHA/MVID、runtime/arch、实际模块文件身份，不沿用旧test SHA。OpenProcessToken(TOKEN_QUERY)→GetTokenInformation(TokenSessionId/TokenElevation/TokenIntegrityLevel)，失败保留，不假定SYSTEM/管理员，也不启用特权/修改令牌。CloseHandle关闭自开token。
2. 原窗口建立后：真实owner PID/TID和SessionId → process station FLAGS/name → 测试desktop → owner.Invoke内desktop/UOI_IO与DPI/ClientToScreen → OpenInputDesktop读权限 → UOI_IO与EnumDesktopWindows目标成员关系。记录每个BOOL/NULL/HRESULT/直接错误值的真实类型与即时错误。
3. 同点记录DwmIsCompositionEnabled HRESULT/BOOL、DwmGetWindowAttribute(CLOAKED) HRESULT和GetWindowLongPtr扩展style（0合法时需预清last-error判断），无NOREDIRECTIONBITMAP不等于有有效重定向面。它们仅解释两假设适用边界，不加timing等待或声称目标帧已消费。
4. 显示关联不重跑HN全环境枚举：MonitorFromWindow(MONITOR_DEFAULTTONULL)及MonitorFromPoint采样点 → GetMonitorInfoW(MONITORINFOEXW104) → QDC_ONLY_ACTIVE_PATHS的source GDI名匹配 → **取有效sourceMode索引的position/width/height，验证采样点是否处于该source矩形**并记录对应adapter LUID/sourceId/targetId。source名称/monitor/source坐标三者必须连到当前HWND，不把active当scanout。布局HnPath72/HnMode64等已有定义可按独立私有前缀复用布局，不启用HN；遇ERROR_INSUFFICIENT_BUFFER最多重查一次，仅容量竞争处理，非颜色重试。其它错误直接落盘停止；不枚举allpaths/驱动/所有WTS会话。此步骤仅是H2物理点合同的上下文，不新立第三根因假设。
5. H1已证不满足（例如station不可见、成功枚举无目标且owner UOI_IO=false）或必要对象查询拒绝/失败：记录具体违反项/权限错误并以测试失败退出，不能改为skip/Passed。后续shader未覆盖。没有用错误查询结果冒“沙箱无权限”。H1成立也只证明当前输入桌面不匹配，不证明它是历史像素失败唯一原因。
6. 合同可继续时按原程序执行native前置，原GetDC/像素点挂H2同DC记录；GetDC为0不调用GetPixel，记录后失败；CLR_INVALID/clip错误按采样失败退出；原raw非预期仍由原断言失败，**不切换取色源、不跳过native前置**。若原前置通过，同一次调用自然继续原shader→64/48/32×24 resize→后态，所有原内容断言不变；后续点只记录已有raw，不多采样。
7. 每个GetDC与ReleaseDC在同一线程try/finally配对；screen DC也一样。释放token、自开input HDESK；GetThreadDesktop/GetProcessWindowStation借用句柄不关闭。Enum callback持根、不抛跨native异常；清理逐项记录并保留主异常，不用finally新异常覆盖。scope不持全局可跨用例状态。沿原finally释放所有实际创建资源、owner销毁与ClassCleanup；保留监督stdout/stderr/TRX/cleanup异常。任一关键错误、timeout/崩溃/原件缺失结束唯一任务，无第二次执行。

GetDC/GetPixel合同没有承诺可靠GetLastError；这两者记录返回值、可附即时last-error但标记**非规范诊断，不能据其归因**。GetPixel只认CLR_INVALID；GetDC非零不等于取色有效。其余按API合同：HRESULT直接保存（含S_FALSE语义），QueryDisplayConfig/DisplayConfigGetDeviceInfo保存LONG返回，不误读last-error；GetUserObjectInformation/OpenInputDesktop/枚举等失败才解释即时last-error。必要声明SetLastError=true，不复用缺该属性的历史声明进行错误裁决。

## 5. 管理能力不是工具schema问题，也不是本次随机修复

本次唯一方案为只读关系验证及原用例；**不授权任何改变session/desktop/显示拓扑动作**，不是声称这些动作不可用。

- 若实际证明owner绑定不同desktop，Windows的SetThreadDesktop可由C#直接调用；官方合同要求目标desktop属于进程当前windowstation，且调用线程没有窗口/hooks（除非仍为当前desktop）。现有owner已在Run里PeekMessage且已创建HWND，不能把已建HWND迁桌面。准确的后续修复位置将是该owner创建任何窗口/hooks之前，而不是随意调用SwitchDesktop或强迁现有窗口；是否允许应由本次对象关系/访问结果及另授决定。不能凭窗口站名称猜有切换权。
- 若只读OpenInputDesktop/EnumDesktopWindows具体ERROR_ACCESS_DENIED，则仅证本次令牌对所请求对象/访问不足，不推出所有管理API失败，也不要求先管理员提权。TokenElevation不是权限充分性证明。
- 若source矩形/采样点真的不相交，先处理本测试已证坐标/窗口放置合同，不猜DisplayConfig设置会修复；本次不调用SetDisplayConfig、RDP/WTSConnectSession、解锁、驱动、电源、SendInput，不伪造连接。
- 不因composition=false或重定向位异常自动开关DWM/重建窗口。只记录实际返回并终止；管理变更必须绑定确证违反的合同、明确恢复/清理步骤后再授权。本报告没有假定哪一条件已不满足。

## 6. 单次预算与准确运行身份

待监督一次性授权：上述最小源码/配置范围 + **normal COM build≤1 / COM publish≤1 / 沙箱≤1 / 原测试体≤1**，Native publish0、独立发现0、official0、x640、full0，失败不修后重跑、不增加任务。此预算未使用，也未获自动执行授权。

使用Release/win-x86、显式 `NativeObservationContract=true`、`ComAcceptanceSandbox=true`、固定现有current x86输入 `artifacts/async-shared-ownership-x86-r1/clean/Native/wpfgfx_cor3.dll`（预期15A545完整SHA见原恢复报告，发布前按包文件重新核），独立obj/bin/publish/clean输出于 `artifacts/native-observation-contract-x86-r1/`，不覆盖任何历史clean。唯一normal build使用正式csproj、configuration=Release/runtimeIdentifier=win-x86/targetFramework=net10.0-windows验证默认关闭编译面，不认证诊断hook。随后唯一COM publish携带诊断与独立输出属性，在该发布内完成启用hook的编译验证，不使用--no-build。两者各自失败即停，不偷加第二构建/脚本，不改变默认配置。

COM publish专用入口可带显式属性；同一发布内的隐式Build不冒第二次独立normal build。使用既有打包targets生成并亲核新clean；self-contained .NET/WindowsDesktop10.0.13采用既有包策略显式固定，不改项目默认。新test SHA/MVID预期改变，Native不重发不扰hash；核当前候选与official角色文件存在但不执行official。

仅推送 `WpfGfxShape/artifacts/native-observation-contract-x86-r1/clean`，exe=`WpfGfxShape.ComAcceptance.exe`，参数数组依次：`--supervise-exit`、`Results/native-observation-contract-x86-r1`、`120`、`FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive`；timeout180，整任务回收，localOutputDirectory=`WpfGfxShape/artifacts/native-observation-contract-x86-r1/readback/candidate`。不经shell，不用旧UID冒当前节点，实际TRX必须恰1执行且FQN正确。归档完整调用/原返回、manifest/sha、identity、JSONL、TRX、stages、监督stdout/stderr，再作裁决。

## 7. 预注册裁决与恢复主线（诊断不是无限终点）

|真实信号|可增加的结论|不能推出|
|---|---|---|
|H1查询成功，owner非input、input枚举无目标且station/owner关系确定|当前原用例具体desktop合同不符；给出可在owner创建前直接API修复的位置，另授修复|历史失败全由会话、永久外部Blocked|
|关键查询访问拒绝/失败/输入桌面变化|记录具体能力/对象查询未闭，停止；无shader覆盖|所有API无管理能力、必须外部维护者|
|H1反驳，H2出现DC/clip/坐标合同错误|采样链具体错误；下一是最小测试夹具修正而非生产shader改色|DWM/adapter故障已证|
|H1/H2均反驳，原raw仍错|排除这两类简单合同错误；原shader因原前置失败仍未覆盖，剩下真实投递/观测表面边界未定|有效GDI==合成输出，或已证明环境坏/target没Present|
|所有原像素/resize/后态通过|同一次原用例有限内容恢复，转回完整计划的official独立对照及target尺寸/device/backend缺口|HW backend/真实loss/media/full/完整替换自动关闭|

本方案不声称一任务必能定位一切。它用**原主线同一次真实执行**反驳/定位两项目前缺证的合同，而非追加public/native-only/WGC或有界探针接有界探针。若仍失败，本门结束，必须按所得具体证据裁决下一项根因工作，不能再默认要求外部维护者，也不能无新增判别信号申请第二次环境扫描；本次不预授权任何后续运行。原shader/native前置/expected保留，完整方案§6.1/§6.2/§6.3及§8最终矩阵继续开放。

## 8. 官方合同来源与读取限制

本次成功取得正文/官方检索正文：

- https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-openinputdesktop ：输入桌面、断开session也可返回句柄、返回/CloseDesktop及访问要求。
- https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumdesktopwindows ：DESKTOP_READOBJECTS、目标desktop的top-level HWND枚举、失败last-error与callback协议。
- https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdc ：client与screen DC语义、同线程ReleaseDC、NULL失败，不保证last-error。
- https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-getpixel ：逻辑单位、COLORREF、裁剪与CLR_INVALID，不保证合成输出/指定帧。
- 官方 SetThreadDesktop 正文（官方检索的nl-nl版本）：https://learn.microsoft.com/nl-nl/windows/win32/api/winuser/nf-winuser-setthreaddesktop ：当前windowstation与已有窗口/hooks限制。
- 官方 GetUserObjectInformationA 正文（ru-ru/pt-pt版本，经检索）：UOI_FLAGS/UOI_NAME/UOI_IO；W版直接拉取超时，不冒已拉取成功。数值字段合同不受A/W字符串编码差异影响，实际实现只用W。
- 官方 High DPI Reference 正文（is-is版本，经检索）：process/thread/window awareness与LogicalToPhysicalPointForPerMonitorDPI的区分。单个GetWindowDpiAwarenessContext URL返回404，不能据此推API不存在；实现前应核具体函数合同，不能试着改DPI求绿。
- GetThreadDesktop直接拉取504；官方Window Station and Desktop Functions正文明确它查询指定TID的desktop。API管理能力限制不由抓取失败或工具schema推出。

这些是设计依据，不是沙箱API执行结果；没有宣称本轮取得任何WindowsAPI值。
