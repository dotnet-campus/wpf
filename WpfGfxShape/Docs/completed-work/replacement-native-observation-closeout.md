# native-observation-contract-x86-r1 原件回读与正式收尾

## 1. 终态与预算

原任务 `87d673efe07345f9b0133d2c3287e0f9` 已结束，本门收尾完成，停止执行。**原 native 前置像素失败，根因未定；不是 shader/resize 通过，也不是已证 ExternalBlocked。** H1 在非原子快照范围被反驳；H2 所定义的简单 DC/裁剪/坐标合同错误在本次记录范围被反驳，但物理表面及目标帧来源仍未知。

本接续仅亲读文件、静态核必要源码和写文档：源码/配置/build/publish/test/Native/沙箱/终端脚本/再委派全部新增0。历史本门 normal COM build **1/1**（exit0、133 warnings、0 errors）、COM publish **1/1**（exit0）、沙箱 **1/1**、原FQN体 **1/1**；Native/独立发现/official/x64/full/重试0。发布内部编译不另冒一次独立build。其他所有历史预算、有限绿、失败及身份分母独立保留，不冲销。

实际执行配置为用户后续授权的 **NativeObservationDiagnostic/win-x86/net10.0-windows**，不是旧设计中的 Release+属性方案。依据 artifact `00-scope-and-budget.md`，后续显式授权取代旧设计§3/6的配置/build安排；不能把本次诊断构建说成默认 Release 分支编译验证。

## 2. 原件位置与本次亲读范围

artifact根：`WpfGfxShape/artifacts/native-observation-contract-x86-r1/`。先亲读00–06七份即时登记全文（scope/build参数/工具返回/截断记录/publish预检/sandbox返回），从06读取准确回收根为 `readback/candidate/`；未因目录枚举未显示文件而判缺件。监督文件给出的准确TRX路径直接读取成功。

以下相对于该回收根，关键运行文本均亲读至末尾，超长响应以小段补齐：

|原件|完整范围|
|---|---|
|`Results/native-observation-contract.jsonl`|1–94：1–15、16–30、31–44、45–57、58–72、73–83、84–94；末22行全部补读|
|`Results/native-observation-resize-x86/TestResults/child.trx`|1–64，34–64补齐首段截断|
|`Results/native-observation-resize-x86/Diagnostic/log_261028035458429.diag`|1–120，28–36、37–60、61–65、66–83、84–105、106–120补齐|
|`Results/native-observation-resize-x86/supervision.json`|1–40|
|同目录`stdout.txt` / `stderr.txt` / `stages.jsonl`|1–43 / 空文件无可读行 / 1–21|
|`Results/aot-identity.json`|单行全文|
|`Results/hw-module-Native-8f7352a2b53f48728e634b3237b5278a.json`|1–10|
|`WpfGfxShape.ComAcceptance.runtimeconfig.json`|1–21|

stdout部分中文已出现解码替代字符，未修写原件；错误中文以TRX/diag交叉核对。原build/publish完整原stdout并不存在于即时记录：build日志原输出105018字符仅保留末20000，前85018未显示；publish52813仅保留末20000，前32813未显示。本接续读的是即时摘要，不谎称恢复或重新亲读丢失stdout；publish没有可核warnings/errors总数，不能填0/0或沿用build133。

最新设计1–109及当前执行规则1–27已分段回读；current-work-item/next-session-handoff/plan-status/基线及覆盖矩阵读取当前入口必要段。必要静态核：NativeObservationContract partial 1–367、csproj 1–61、VisualCache入口/前置与cleanup/同HDC hook、WindowThread1–96、OutputWindow1–94、GpuReadback68–92、HardwareClosure入口及PresentDiagnostics必要前段。不声称全仓库/历史二进制重新认证。

## 3. 身份、单体结果及错误边界

- actual native：`15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620`，实际模块路径与Requested均为原任务 `Native/wpfgfx_cor3.dll`；不只靠初始化identity推实际加载。
- test：`A48736C8B707CAE70B119CBFB569755D80EAB6E7F907570DD9852DF344C838E8`；MVID `2fe25f22-6982-439a-8456-1fbd4a982653`；`.NET 10.0.13 / X86 / pointer4`。运行scope与监督身份一致，runtimeconfig的NETCore/Desktop均10.0.13。
- 原FQN：`WpfGfxShape.ComAcceptance.D3DImageTests.WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive`。本次TRX真实节点UID `1eded079-5019-809c-bf7c-07ef4caf0e29`，execution `850cd3f2-ed60-4a4d-a3cb-5313dc125ea4`；与历史UID相同是本次原件核出的结果，不是预先移绑。total1/executed1/passed0/failed1/error0/timeout0，diag同UID Failed及会话exit2。
- 监督父PID12200、子PID6576，03:54:58.2841703至03:55:00.6049778 UTC；子OS ExitCode2/0x00000002、Reason=exited、HasExited=true、TimedOut/Cancelled/KillAttempted=false，输出收集完成，无drain timeout，SupervisorError=null。工具直接返回未给父进程数值exit，不把子exit冒父exit。
- 原失败为 `AssertCombinedPixel` 每COLORREF分量差<=1断言：actual `00F3F3F3`，expected `00563412`（RGB 12/34/56，00BBGGRR）。非CLR_INVALID，非访问拒绝，非超时。与旧actual `001E1D1C`不同，仍为失败，不能据颜色变化推环境/内容恢复。
- stages到原native Present/Diagnose/Assert前，随后直接cleanup；VisualCache源码在该断言后才connect/channel/候选shader场景。故本次候选业务shader、64/48/32×24 resize与后态均未覆盖。原D3D9控制的swapchain32×16/format22/count1/windowed1及cooperative0不认证候选target尺寸/device/backend。

## 4. H1：分层反驳，不外推全部session问题

目标 HWND=53806720/0x3350680，owner PID6576/TID6048（owner Invoke实核），测试TID7840；本进程/owner session查询都成功且为1。WinSta0 UOI_FLAGS读取12字节、Flags1/WSF_VISIBLE；owner/test/input desktop UOI_IO均1，owner在真实线程内核对。OpenInputDesktop READOBJECTS=1成功；EnumDesktopWindows成功且精确目标HWND成员=true，不以同名Default或句柄数值替代成员证明。

故“真正owner HWND不在当前可输入desktop”的H1在这些快照被反驳。记录明确atomic=false：窗口阶段约03:54:59.162–.188，像素约03:55:00.442，不能证明期间无桌面切换、持续连接、实际scanout或全部远程session条件健康。DWM composition HRESULT0/enabled=true、cloak HRESULT0/value0、style8/noRedirectionBitmap=false仅适用上下文，不证明有效重定向面。

token TOKEN_QUERY成功：SessionId1、Elevation1、Integrity RID12288（high）；容量探查error122/needed20是预期缓冲区查询，不是权限失败，随后查询成功。未出现ERROR_ACCESS_DENIED。不能称本次没有管理能力，也不能由high/elevated证明所有管理动作都获授权或可成功；本次未尝试管理变更。

## 5. H2：同DC有效性与同点映射的实际证据和未知

原GetDC非零（x86有符号显示-1425992777不是失败），原GetPixel确在同一测试线程、同一HDC上。type3=OBJ_DC，map1=MM_TEXT；window/viewport origin均(0,0)，extents均(1,1)，相关BOOL均true/error0；LPtoDP(16,8)=(16,8)。GetClipBox2/SIMPLEREGION [0,0,32,16)，PtVisible=true，点在内部；raw有效，ReleaseDC1。

process GetProcessDpiAwareness HRESULT0/awareness0；test/owner/window awareness0（unaware），context记录均24592，window DPI96。建立阶段与首采样阶段两线程ClientToScreen均成功得(56,48)；不能只凭context整数认合同相等，本次另有awareness和实际转换一致。screen DC非零，type3/map1/origins0/extents1、LPtoDP(56,48)=(56,48)，clip [0,0,1920,1080)，PtVisible=true。monitor相同，DISPLAY1 source身份/索引匹配、矩形(0,0,1920,1080)包含该点，QueryDisplayConfig等LONG结果0。

这些联合证据反驳本次**简单的无效DC、点被裁掉、逻辑映射偏移、owner/test转换不一致**。额外screen单读也得00F3F3F3，记录established=true可解释为上述GDI坐标合同下的比较适用，不能升级为已认证同一物理呈现surface/帧。LPtoDP仅到该DC设备坐标，不给出D3D backbuffer/DWM重定向面的身份；DPI-unaware调用可能虚拟化，GetDpiForWindow=96对unaware窗口不独立证明显示器真实物理DPI；CCD矩形包含只能作上下文。没有原子绑定的physical-point/帧认证、目标DC surface身份或scanout回读。因此宽泛“任何物理映射/表面来源都已排除”不成立，仍有未知；但没有实际返回证据证明H2坐标错误，不能反向宣判其成立。

GetDC/GetPixel的last-error不是规范错误来源，本次仅用返回值/CLR_INVALID判断；HDC负数不误作HRESULT。其它成功API的error0只是记录，不制造权限失败。HRESULT与CCD LONG直接解释；clip/PtVisible等不凭无合同last-error补因。

## 6. hook静态审查、时序扰动及清理

partial及VisualCache hook未直接调用ShowWindow/SetWindowPos/SetForegroundWindow/UpdateWindow、切DPI、Invalidate/Paint、desktop/session/显示设置、D3D Clear/Present或resize。原EstablishObservableWindow本来含显示/topmost/foreground/UpdateWindow/消息处理，不是新增hook行为。原OutputWindow类Background默认0，WM_ERASEBKGND返回1，WM_PAINT仅ValidateRect；不能从00F3F3F3猜成系统画刷实际覆盖，未有WM_PAINT事件原件证明它发生在采样之间。

**不是零扰动观察**：新增scope hash/JSON同步AutoFlush、token/desktop/CCD查询；windowThread.Invoke投递0x8001并等待owner执行，可能改变既有消息/paint调度。原window HDC取得后，在原GetPixel之前还执行owner坐标Invoke及screen单读；JSON时间约03:55:00.231至.442，增加约211ms持DC/采样延迟（含日志/查询/屏幕读，不把全部归于单一API）。screen读记录相对前一合同记录约198ms后完成，但不能据日志时间精确拆出API耗时。hook没有显式新增窗口状态写入，仍有调度与I/O时序扰动；不能声称与无hook历史现场时序等价或解释颜色变化的因果。

CloseHandle(token)/CloseDesktop(input)均true，screen/window ReleaseDC均1且调用结构同测试线程finally配对；borrowed station/thread desktop不关闭。scope complete samples1。stages完整走到device/windows及cleanup complete空失败列表。断言在connect前，channel/connection/bitmaps等尚为0，遍历这些cleanup标签不是已创建资源的实际Release0证明；device/d3d已创建且经过Release调用，但无逐项refcount数值。ClassCleanup按源码停owner/join，TRX/diag未报告其异常，不冒有独立join事件证据。

新增CleanupRecord/Dispose只对IO/UnauthorizedAccessException输出stderr而不替换主异常；本次stderr空，无可见诊断清理异常。关闭失败值仅记录，不统一升级断言，sameThread=true为源码结构佐证而非独立调度测量。原fixture仅businessCompleted=true时断言cleanupFailures，本次false保留原像素主失败；若原finally内调用自身抛异常仍有覆盖风险，不能从此次无异常外推通用异常安全已证明。没有改代码掩盖这些边界。

## 7. 四分支正式裁决

|预注册分支|本次判定|
|---|---|
|H1具体对象关系违反|未命中；快照中station可见/owner input/精确成员均成立|
|H2具体DC/clip/坐标违反|未命中；同DC与两线程GDI映射证据一致，物理surface身份仍未知|
|H1/H2简单合同均被反驳但原像素错|**命中（限定上述范围）**；原native前置失败，候选shader/resize未达，投递与观测表面边界根因未定|
|关键查询失败/缺失导致无法裁决|本次必要查询未失败；不把物理帧/表面未知伪装成访问拒绝。若把H2扩大为所有物理来源认证，该扩大命题仍未闭，而非本次API失败|

全像素通过的恢复条件未达。关闭的是这一次调查与文档收尾，不是内容门或完整替换。ExternalBlocked不是已证事实，不再把工作默认推给外部维护者。沙箱内可调用令牌允许管理API的能力未被否认，任何新执行仍需另授。

## 8. 保留状态与唯一下一主线（只建议，不执行）

临时partial、VisualCache条件hook、csproj非默认配置及资源项全部原样保留；未移除、未重建、未重发Native。Debug/Release在已读csproj中不追加NATIVE_OBSERVATION_CONTRACT，10.0.13 pin仅NativeObservationDiagnostic条件生效；这是静态默认关闭登记，不是本轮默认分支实跑认证。诊断clean/publish/readback及00–06即时记录保留供审计，不自动续用来重跑。

**唯一下一动作建议：静态追踪原native呈现窗口/DC surface来源与生命周期，形成能区分“D3D目标已写但GDI观察其他/旧表面”和“内容未进入目标呈现面”的可证伪规格。** 从本次32×16 swapchain/hDeviceWindow/hDestWindowOverride与同HWND有效GDI DC、00F3F3F3、无背景刷/paint只validate的矛盾出发，逐一绑定CreateDevice→Clear/Present→既有消息/paint/销毁路径及DC获取位置；不是再扫描session、随机设置、等待或重复取色，不重开WGC/public/native-only门，不先改shader/expected。

有判别力的后续规格必须能独立绑定**同一目标surface内容/身份、Present结果与HWND生命周期事件顺序**；仅再得一次client==screen或添加WM_PAINT计数都不够。可证伪标准：若经独立可靠绑定的目标面在同一呈现边界确含00563412而GDI仍为F3，则反驳“producer根本没写入目标面”；若目标面本身不含预期，则不能归GDI观察错面；若paint/重建事件能被明确排序到内容替换之间，再检验具体生命周期原因，不能从颜色猜原因。本次没有这样的surface/事件原件，也未确认哪条可用API可无扰动可靠实现该绑定，**不把上述判别标准冒充已就绪实验**。若静态合同仍不能建立可靠绑定，应明确剩余证据/观测能力缺口并停止，不转推外部维护者，不申请泛环境扫描或第二次同色读取。本接续未执行该下一动作或任何新实验。

完整计划的真实HW/resize内容、独立target GetDesc/GetDevice身份/backend、real-loss/media、最后源码双架构Release最终适用矩阵仍Open；旧KN/ownership/partition/AsyncDrain等绿保持原有限边界和SHA，不移绑本test、不提升整体基线。
