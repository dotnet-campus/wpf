# present-occlusion 有限收尾：静态核对完成，最新执行原件不可用

## 当前追加更正：原件存在，旧缺件结论为代理误判

本节覆盖下方旧当前裁决，旧标题和正文保留历史。[正式证据与文档收尾](../../../Docs/completed-work/replacement-exclusive-foreground-and-cache-evidence-closeout.md)状态为CloseoutCompleted，仅指本证据与文档工作，不指完整替换。前代理已核present-occlusion-state绝对嵌套artifacts原件真实存在；下文“不存在/仅用户交接/用户认证”的结论为代理误判，不是用户认证。本次只读写文档，未重新亲读运行原件或源码。

已证df9e6a43f21f48cab95ce47a6f40e634 / 3ab6be85d5ed4e5f837cc852ab33bb04各1823/1821/2，九项occlusion未复现；official actual AD1C、初始FF80/actual0且native过，旧loss foreground前置Failed。新loss task0f364…系统实际SetForegroundWindow三次1、双Allow1/session1、88760868→88760869→Reset0、实际新旧FF123456/FF654321、OSexit0/TRX Passed，只是system control ClosedLimited，不是产品consumer loss。既有代码范围ExclusiveFocusLoss/resx，首build CS0103失败、次146warn0error、COMpublish2/sandbox2，不能继续沿用下方仅交接/无normalbuild成功的旧说法；本次新增代码及执行0。

前代理已亲读official-cache-8836-7970572.jsonl全16行cacheReached：connect0、两source12×1、dirtyHR0、mode11 scale1、brush9→mode11/visual6、target12/root13/transform14/child3/单位world、flush0/FF80 actual0。publicPeek retrieved0只是事后无消息；BitmapCache94/BitmapCacheBrush83/HwndTarget46，不得混69/73/74。未发现确定layout/scalemode/root缺项；candidate同源码通过不保证official source位图可用。根因仍未定，GetBitmapSource public ABI是否可直接调用Unknown；下一只建议先核可观察合法COM接口最小范围，不虚构接口、不强开诊断。consumer HW/产品loss/audio/自然资源/finalfull/全替换Open，memoryGreen=null。下方预算及下一动作均仅历史。

## 1. 终态与证据等级

本次静态核对及文档收尾已完成；最新两次full的原件认证未完成，不能标成Complete evidence readback。完整替换Open，actual target/device loss恢复NotCovered，音频Open，自然资源收敛Open，最终验收full未闭，memoryGreen=null。没有改生产、expected、skip、颜色、等待或loss必须真实发生的要求。

用户交接的最新执行事实如下，**仅为交接记录，不是本次独立读回认证**：

|角色|task|用户报告 total / passed / failed|用户报告occlusion九项|
|---|---|---|---|
|default|df9e6a43f21f48cab95ce47a6f40e634|1823 / 1821 / 2|未复现|
|diag|3ab6be85d5ed4e5f837cc852ab33bb04|1823 / 1821 / 2|未复现|

本机当前工作区根及WpfGfxShape子目录均没有artifacts目录；指定artifact/present-occlusion-state不存在。按名称检索并扩大到d:/lindexi/Code/dotnetcampus/WpfLab亦未找到present-occlusion-state；工作区内容检索没有这两个task ID。不能从旧task复制最新Counters、所有Result/Definition、testId/executionId、actual加载SHA/MVID、子OS/MTP exit、supervision及cleanup。上述字段在本次均未认证，未填造数值。两项失败的精确身份也不能仅凭旧full推定。

用户说明错误分支没有JSON：这不等于采集失败，也不等于修复成功。九项未复现不是occlusion根因已确定。exclusive前置foreground失败不是device loss，不能变expectedskip。

用户交接的构建历史为：首次normalbuild失败，之后修正并由publish implicit build成功；不能写normalbuild成功。本次没有取得这两份构建日志，故仅保留该交接说明，不伪称已核具体错误。没有添加NoWarn。

## 2. 前台建立：确证的控制流缺口与API边界

直接读取：Tests/WpfGfxShape.ComAcceptance/NativeRun.cs 1–60；D3DImageTests.ExclusiveFocusLoss.cs 1–112；D3DImageTests.GpuReadback.cs 68–92；D3DImageTests.cs 174–185。

- NativeRun的--exclusive-focus-role只把执行交给RunExclusiveFocusRole；没有初始前台授权协议。
- RunExclusiveFocusLoss先调用EstablishObservableWindow，再Assert GetForegroundWindow==window；只有通过此断言、独占设备创建/首帧/Present成功后才启动trigger，并调用AllowSetForegroundWindow(trigger.Id)。因此现有child授权不能修复最初foreground前置失败：执行根本未到这一步。
- EstablishObservableWindow检查topmost/ClientToScreen/WindowFromPoint，却忽略SetForegroundWindow的BOOL。可见且topmost不等于获得foreground。exclusive后面的相等断言避免了把此处失败误当成功，但没有留下该次API BOOL。
- SetForegroundWindow声明没有SetLastError=true，trigger随后打印Marshal.GetLastWin32Error不能证明该次SetForegroundWindow失败原因。更重要的是该API官方合同没有承诺GetLastError，改声明也不能创造可靠拒绝原因。应记录BOOL、目标及实际foreground；不能把残留last-error命名为该调用的错误。
- AllowSetForegroundWindow声明有SetLastError=true；失败时可立即读取真实last-error，成功时不应把残留值解释成失败。此API要求调用者本来就有设置前台的资格，不能自行授予资格。

已读取Microsoft官方API合同：
- https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow
- https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-allowsetforegroundwindow
- https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-attachthreadinput

SetForegroundWindow受foreground lock、active menu及调用进程资格限制，即使满足所列资格也可能被拒绝。AttachThreadInput仅共享输入队列状态；要求不同线程、已有消息队列、非系统线程、同desktop，无journal hook；失败返回0，现代Windows支持GetLastError，且会重置键状态。它不是合法用户输入授权的替代，也不证明能绕过foreground lock。本次没有加AttachThreadInput或SendInput。

### 可直接实施但需具备owner控制权及下一publish授权的最小交接合同

1. 初始前台建立之前，由当前有资格的foreground拥有者通过既有受控协议对测试进程PID调用AllowSetForegroundWindow；不能让尚无资格的测试子进程自授权限，也不能改全局foreground lock。
2. 测试窗口必须由当前同session、同desktop的owner线程操作；记录SetForegroundWindow BOOL及实际foreground并严格断言，不把topmost覆盖检查当foreground证明。
3. 初始资格与真正独占帧建立成功后，沿现有父到trigger的AllowSetForegroundWindow(childPID)交接；立即检查BOOL，失败立即记录该API支持的last-error。child记录激活BOOL，父检查实际foreground。只有随后Present/TestCooperativeLevel真实返回DEVICELOST才算触发成功。
4. 若child尚存且要回交，须由此时有资格的child对父PID授权，再进行恢复前台确认；保持DEVICELOST→DEVICENOTRESET→Reset→不同内容真实帧原断言。不能以交接或失焦本身代替lost。

当前代码和可用原件没有证明sandbox调度者就是可协作的foreground owner，也没有提供其授权IPC入口，故不能声称以上资格已取得或纯C#管理即可强制成功。确证的是初始授权发生点缺失与API返回观测缺口，不是某个具体foreground lock拒绝原因。本次不改共享EstablishObservableWindow行为，避免把普通可见输出测试无端升级为必须foreground；不引入未经发布验证的新loss协议。

## 3. official cache：源码合同，不给无证补丁

直接读取VisualCache.cs资源创建/dirty/绑定/target/root构建段及BindTwoInputEffect 603–662；官方BitmapCacheBrush.cpp 49–98、106–265，VisualCacheSet.cpp 598–655，visualbrush.cpp 234–300。

fixture在cache分支创建94(BitmapCache)，发送0x8d、scale=1，再发送0x84(BitmapCacheBrush)，将mode/source放入28/32偏移；源image/drawing/content命令及dirty先提交。不能称其完全没设置cache mode或没有resource init。

官方ProcessUpdate执行GeneratedProcessUpdate后对新InternalTarget调用RegisterCache(mode)。VisualCacheSet::AddCacheInternal对未指定mode也有node cache/default cache路径；缺少Visual.SetCacheMode本身不构成BitmapCacheBrush一定失效的证据。

官方GetBrushRealizationInternal明确先PreCompute，再UpdateCaches，再GetBitmapSource；无bitmap时返回空realization。GetRenderTargetBitmap也先PreCompute/UpdateCaches。PreCompute调用VisualBrush::PreComputeHelper，以无限surface bounds遍历InternalTarget，即使该visual未挂其他可见树也会处理；helper会保留跨树precompute产生的dirty regions到parent。由此不能要求fixture另发不存在的“rootbounding/childrendirty”公开命令来替代官方内部遍历，也不能擅改生产清除dirty的时序。

尚未获得最新actual0现场的资源快照、同channel批次/通知、内部CanRender/缓存bitmap/dirty时序原件，不能从相同API参数推出async共享global state无差异，也不能从空输出反推某个内部状态缺项。此次没有新取色、等待、多flush、隔离或wholefull实验；没有发现足以定位actual0根因的源码缺项，不给猜测cache修复。

## 4. 预算与完成边界

本次新增normalbuild=0、COMpublish=0、implicit build=0、sandbox=0、Native=0、terminal/script=0、再委派=0。按用户交接，COMpublish2已尽，剩normalbuild1与sandbox1均未使用：当前既无现包目录/receipt可认证，也无新修正能由旧包验证；再wholefull不回答新的问题。文档改动无需消耗构建。没有暗发。

六入口已同步本报告并回读：current-work-item、current-stable-baseline、next-session-handoff、remaining-gap-closure-plan、replacement-issues-and-plan-status、final-acceptance-remaining-matrix。另同步完整验证方案入口，避免继续执行过期预算。旧报告与历史失败原样保留，不以本次交接数字相减。

原始信号不足的具体阻塞是指定receipt/readback及现包在当前提供的工作区不存在，而非泛称环境或采集失败。文档已经写完；不能完成的是缺失原件的真实性认证，不是还有报告未写。恢复这两task原件后才能补齐全部counter/faildefinitions/identity/子exit/cleanup，不需要为取这些已有证据重跑或新增发布。完整替换及actual target loss/音频/自然资源/最终full仍未闭。
