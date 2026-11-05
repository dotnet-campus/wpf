# KR 原 native 前置 Win32 / D3D9 合同静态审计

## 结论与授权边界

本轮仅文件及微软正式文档静态调查；代码修改0、构建0、发布0、测试执行0、Windows沙箱0、终端0、脚本0、委派0。新增本报告，恢复入口追加最新结论；历史保留。没有新像素、HRESULT、引用计数、TRX或成功实验。

**发现两处确定的夹具 API 合同问题，但尚未证明它们导致 KP 的旧色：**

1. 非窗口拥有线程调用 CreateDevice / TestCooperativeLevel / device 最终 Release，与微软 focus HWND 窗口过程线程合同不符。0x26 已含 MULTITHREADED，不能把它误写成缺少该标志，也不能据此豁免特定生命周期线程要求。
2. `ProcessWindowMessages(window)` 在测试线程向 PeekMessageW 传入另一个线程拥有的 HWND；微软明确要求此 HWND 属于当前线程。该调用不是给窗口 owner 泵消息。

**历史去重后的推进：** HO 已提出生命周期线程疑点，但尚未取得正式合同；本轮取得微软原文并定位完整调用链。HH/HL只改变提交线程，不能排除创建/诊断/最终释放线程合同；但 **CC确实做过 owner CreateDevice 对照**，不能再把“改 CreateDevice 到 owner”包装成新实验。CC仍在测试线程 Diagnose/TestCooperativeLevel、最终Release，且其参数Format21、三帧、duplication前置与原单帧不同。CC的失败及BJ非owner成功强烈反对“创建线程是普遍充分根因”，必须保留。

未发现所审原 native 链有确定 ABI 错签、COLORREF反序、透明窗口或提前释放。没有把失败归为环境坏；不要求用户先证明显示变化才准调查/修正明确API误用。完整替换、KP原shader/resize像素仍未完成。

## 范围、位置与阅读边界

路径以 `WpfGfxShape/` 为项目内根；从工作区根访问需加此目录。起初按用户提供工作区根的 `Docs/next-session-handoff.md` 读取不存在，名称检索确认实际是 `WpfGfxShape/Docs/next-session-handoff.md`；不是缺失交接。

已读 current-execution-rules全文、current-stable-baseline全文、完整替换方案1–45、current-work-item开头；AGENTS.md名称检索未找到。已读当前交接开头、KQ主要节及尾节、KP主要终态；长返回有截断，不声称全历史逐字读取。原代码聚焦 VisualCache55–100/460–525、OutputWindow全关键段、WindowThread全文、SwapChainObservation全文、PresentParameters/MSG/Win32声明、GpuReadback68–92、PresentDiagnostics34–90、HH旧前置及CC全文分段。没有遍历生产领域。

独立核查历史：`replacement-supervised-shared-consumer-contract-fix.md` HH42–64；`replacement-supervised-present-delivery-controls.md` CC关键节；`replacement-supervised-capture-fresh-frame.md` BJ1–33；`replacement-supervised-discard-present-differential.md` HL1–33；`replacement-supervised-display-blocker-strategy.md` HO32–53/75–100；`HW-dedicated-STA-presentation-current.md`全文。历史数字是这些报告记载，本轮未重新认证所有旧任务原始产物/二进制。

## 1. ABI / slot / 布局 / HRESULT

下表签名省略显式this以便对应微软接口；实际C#函数指针均有首参nint this、unmanaged[Stdcall]，HRESULT为32位有符号int，UINT/DWORD/枚举32位，指针为nint。x86 stdcall及x64统一调用约定形状未见不符。

|接口/slot（从0计）|源码位置（Tests/WpfGfxShape.ComAcceptance/）|核查及历史支持/反证/未知|
|---|---|---|
|Direct3DCreate9导出，无slot；UINT→IDirect3D9*|D3DImageTests.cs:10–11；VisualCache82–84|uint→nint正确，32为SDK常量，返回值判非0，不把它当HRESULT。官方[D1]明确返回NULL表示失败。HH/KP创建成功支持，不能由此证明消费。|
|IDirect3D9 CreateDevice16；UINT,DEVTYPE,HWND,DWORD,PP*,Device**→HRESULT|VisualCache85–88|形状正确；adapter0/HAL1/focus同HWND/0x26，输出真实指针；Assert0。0x26=FPU_PRESERVE2 + MULTITHREADED4 + SOFTWARE_VERTEXPROCESSING20，仅一个vertex processing标志。软件顶点处理不等于软件像素fallback；不证明物理GPU。线程问题见§2。|
|Device Clear43；DWORD,RECT*,DWORD,D3DCOLOR,float,DWORD→HRESULT|SwapChainObservation23–25|Count0/RectsNULL/flagsTARGET1/FF123456/Z=1f/Stencil0。float不是double，颜色为uint；没清未建depth/stencil，不需要为了Clear添加BeginScene/EndScene。HH/HL提交前backbuffer正确反对本签名导致固定旧色。|
|Device Present17；RECT*,RECT*,HWND,RGNDATA*→HRESULT|SwapChainObservation26–29|普通9仅4参数，无Ex flags；NULL三指针及同HWND override符合DISCARD要求[D4]。扩展121有额外uint flags，但原链extended=false不进入，不混用slot。|
|Device GetSwapChain14；UINT,SwapChain**→HRESULT；SwapChain GetPresentParameters9；PP*→HRESULT|SwapChainObservation33–46|index0，双指针正确、PP局部清零，finally Release(swapChain)。这是实际chain配置读取，不是DWM消费；KP actual32×16/22/count1/swap1/windowed1支持布局与调用一致。|
|Device CreateQuery118；QUERYTYPE,Query**→HRESULT；Query Issue6；DWORD→HRESULT；GetData7；void*,DWORD,DWORD→HRESULT|OutputWindow49–72|EVENT8/END1/GetData(NULL,0,FLUSH1)合法[D5]。0字节状态轮询不要求BOOL输出。GetData S_FALSE1继续、负值失败、到5秒失败、最后Release query。未把S_FALSE当完成；没有返回HRESULT被截成bool。|
|Device TestCooperativeLevel3；无参数→HRESULT；IUnknown Release2→ULONG|PresentDiagnostics82–83；VisualCache480；HH83–84|宽度/slot形状未见问题；前者只打印，不是消费门。最终Release线程不符见下。原方法不打印Release返回数值，不造refs0。|

**slot认证限制：** 上述slot与标准D3D9 COM顺序一致（IUnknown0/1/2；Query的GetDevice3/GetType4/GetDataSize5/Issue6/GetData7；SwapChain的Present3/GetFrontBufferData4/GetBackBuffer5/GetRasterStatus6/GetDisplayMode7/GetDevice8/GetPresentParameters9）。本轮未取得可逐项枚举的匹配SDK头：工作区无d3d9.h，微软仓库尝试地址404，Learn接口页不直接提供slot数；不能称“匹配SDK头全文独立认证”或实际二进制vtable验证。官方函数文档及历史正确producer行为支持，并非以运行现象代替ABI证明。没有依据建议改slot。

`D3DImageTests.cs:17–24` PresentParameters与官方[D3]字段顺序一致。自然布局静态计算：

- x86：前7个DWORD offset0..24，HWND28，Windowed32/AutoDepth36/DepthFormat40/Flags44/Refresh48/Interval52，sizeof56。
- x64：前7个DWORD0..24，padding28..31，HWND32，Windowed40/AutoDepth44/DepthFormat48/Flags52/Refresh56/Interval60，sizeof64。
- BOOL用int不是C# bool；HWND用nint不是uint；未使用Pack1。此为静态计算，未运行Marshal.SizeOf/OffsetOf。
- OutputWindowClass为顺序WNDCLASSEXW：x86 size48/proc8/instance20/name40；x64 size80/proc8/instance24/name64。MSG (`D3DImageTests.cs:211–221`) 含Private DWORD：x86 size32，x64 size48；RECT16/POINT8。未见Win32回调参数或WindowFromPoint按值POINT错传。

Create/Clear/Present/Query创建/Issue/GetSwapChain/GetPP断言严格0。GetData仅接受非负并对1轮询；文档正常仅0/1，不存在本已观察错误码被截断成0的证据。Ex可能返回正状态，不得概括SUCCEEDED=消费；HL明确08760878为S_PRESENT_OCCLUDED，原KP不是Ex。原代码没有错误HRESULT被捕获后假通过native前置的路径。

## 2. 线程、消息、重入、生命周期（最高优先级）

**官方依据[D2]：** Creating a Device的windowed示例后明确create/release/reset应只在focus window的WndProc线程；Multithreading Issues还列TestCooperativeLevel/最终Release。不能仅用后文full-screen背景把windowed示例要求忽略。D3DCREATE MULTITHREADED用于允许跨线程API访问/消息处理，并未撤销上述特定限制。

|怀疑点与位置|支持|反证/历史排除范围|静态裁决|
|---|---|---|---|
|CreateDevice线程不等于focus HWND owner：VisualCache55/88，WindowThread31–90|CreateOutputWindow通过Invoke返回owner创建窗口；VisualCache设备创建在调用测试线程。HH owner7932/测试8204的报告与此相符|CC ownerCreation=true确实改过CreateDevice且3帧仍失败；BJ非owner对照历史成功|确定合同不符；不是已证像素根因。不能推荐仅再改CreateDevice。|
|诊断TestCooperativeLevel及device最终Release在测试线程：PresentDiagnostics83；VisualCache480；HH83；CC102–112|调用链无Invoke；即使CC owner创建设备，诊断与最终Release仍不在owner|最终Release发生在像素采样之后，不能倒因为本次先前像素失败；CC此项未修不意味着其先前帧必受影响|需修正线程合规；Release单点不是当前失败的合理前因。|
|非owner HWND用于PeekMessage：D3DImageTests.cs263–267；GpuReadback74；OutputWindow60/65/67|微软[D6]明确window must belong to current thread。原链Establish和Complete在测试线程调用，错把别线程HWND作为filter|WindowThread已持续GetMessage，因此不是“整个窗口没有泵”；HH/HL与BJ都用过同helper。CC owner提交期间Complete在owner，不能以CC完全排除所有初始Establish时序|确定Win32调用不满足合同。仅返回0不等于窗口已排空；需owner调用，不能靠额外等待。|
|跨线程Show/SetWindowPos/UpdateWindow和重入：GpuReadback68–75|UpdateWindow直接发WM_PAINT[D7]，不是异步排队屏障；CreateDevice可能触发窗口消息；Invoke同步等待调用者|WndProc仅ValidateRect/DefWindowProc，无D3D、锁、文件I/O或等待测试线程；owner本身持续泵。当前没有具体互锁环或重入释放路径，KP无超时|存在调度耦合但未证明死锁/覆盖；不可宣称这些Win32 API一律禁止跨线程。|
|WM_PAINT/WM_ERASEBKGND：OutputWindow87–92|ValidateRect移除update region，非像素绘制；Erase返回1无背景刷，无默认EDIT/STATIC覆写|HW-dedicated-STA历史已替换stock控件；CC/BJ相同策略|无发现GDI明确覆盖Present。不能用Validate当作Present完成，也没有依据加GDI背景填充“修复”。|
|窗口创建/销毁与线程寿命：WindowThread/ClassCleanup；D3DImageTests.cs161–164；VisualCache480–484|窗口创建与DestroyWindow已路由owner，owner在fixture cleanup后WM_QUIT/Join。device先Release再destroy HWND，factory晚于device释放；CreateDevice在CreateWindow返回/Establish后，不在WM_CREATE|原finally只记录路径，不验证destroy返回；查询Release早于采样但device仍在|未见采样前销毁窗口/设备。缺资源返回数值是证据边界，不是提前释放的证据。|

**最小修正说明（本轮未实施）：** 对一个隔离的native-only前置，将Establish→Direct3DCreate9/CreateDevice→Clear/Present→Complete/query→Diagnose/TestCooperativeLevel→采样→Release device/factory作为同一个owner同步Invoke作用域；窗口创建/销毁仍复用原owner设施，消息轮询才使用本线程HWND。不要只把Clear/Present挪过去，也不要用MULTITHREADED追加标志掩盖；不改共享helper/默认shader方法。纯native前置此改动不引入跨线程候选调用或channel迁移。若原shader未来真正修夹具，需另审其同步channel归属，不能把整个业务盲搬线程。

## 3. 样式、透明、重定向和采样DC

- `OutputWindow.cs:33–46` class Style0、Background0、无parent/owner HWND；exStyle8=WS_EX_TOPMOST，不是WS_EX_TRANSPARENT(20)、WS_EX_LAYERED(80000)、WS_EX_NOREDIRECTIONBITMAP(200000)。style90000000=WS_POPUP|WS_VISIBLE，无caption/frame。HH/HL实际establish后94000000多出CLIPSIBLINGS，不构成透明层。正式依据[D8]。**明确排除“把8当透明/分层flag”的解释**；未验证系统内部redirection surface身份，不能把无NOREDIRECTIONBITMAP推成DWM一定已消费。
- 原Clear FF123456为AARRGGBB，GetPixel应COLORREF00563412（00BBGGRR）[D9]；X8R8G8B8的X不参与透明alpha。HH旧色00CDB38B/frontFF8BB3CD同RGB，KP001E1D1C也非单纯红蓝颠倒。HH已排除颜色通道误换，禁止改expected追绿。
- `VisualCache.cs:503–516` GetDC(window)取client DC，采client(16,8)，ReleaseDC同一调用线程；`PresentDiagnostics.cs:47–54` GetDC(NULL)取screen DC，先ClientToScreen再采。符合[D10]。普通common DC不是必须在HWND owner获取，只需不并发使用同DC、获取/释放同线程。不能把跨线程GetDC本身说成确定误用。
- client32×16中点有效；KP/HH DPI96、client16,8→screen56,48且cover自身，既有client/screen/front一致反对简单边框偏移。源码未改DC mapping/world transform。DPI96不是一切虚拟化已排除，未知线程DPI变化仍不能凭推测改DPI/位置。
- 原GetDC未判NULL，GetPixel未显式判CLR_INVALID；这是失败分类不足。[D9] GetPixel不支持或剪裁外返回FFFFFFFF，但KP/HH实际非FFFFFFFF，所以不能解释本次旧色。原每通道<=1不是严格零阈值；本轮不更改/夸大原断言。新的校准须client/screen零阈值及非同色提交前基线，而不是producer读回通过。

## 4. Present / actual配置 / producer-consumer边界

原输入 `VisualCache85–90`：SDK32/adapter0/HAL1/behavior26、32×16、Format0、count1、MS/Q0、DISCARD1、Windowed1、AutoDepth/DepthFormat/Flags/Refresh/Interval0。FormatUNKNOWN在windowed合法[D3]，实际22是预期解析，不是结构错位或格式错误；不能未经单因子证据改成21。requested PP是in/out，不能用调用后局部PP替代actual chain。当前另调GetSwapChain/GetPP正确，但日志没有输出每个零字段，不能称KP所有actual字段已逐一落盘。

Present NULL src/dst/dirty符合DISCARD；target override与hDeviceWindow同一，不存在错采另一个窗口的源码反例。CC已独立对照override HWND/NULL且失败，重做同一对照无新价值。Clear为整viewport，创建后未更改render target或viewport；无需凭空加入draw、BeginScene或重复Present。

CompleteNativePresent的EVENT命令完成只证明该设备队列经过标记；DwmFlush等到DWM present/挂起surface更新[D11]，但没有将本次device swapchain token绑定到某个DWM consumer帧的返回身份。二者不能证明目标窗口内容已被消费。GetPP只证明配置，TestCooperativeLevel0只证明设备状态；visible/Active/cloaked0也不是内容证明。

HH/HL backbuffer正确、Present0、EVENT完成而consumer旧色，是此边界的历史实证，不是producer读回作为通过。本轮无DWM目标latch/discard原因，未知项明确保留。

## 5. 采样与资源存活

原链采样在Present/两次DwmFlush之后、device/factory最终Release之前；HWND在finally末销毁。query在其完成后Release，swapchain诊断临时引用用完Release，底层device仍活；没有把这些对象先释放再采样的路径。HH旧段同样最后device/factory0/0再destroy1（历史报告数值），不移用为KP数值。

DISCARD后backbuffer内容未定义，HH明确不读其作为提交后证据；提交前staging仅证producer，不替代采样。原Assert仅取单次样本，无本帧唯一标记/提交前基线，故旧色恰同目标存在假阳性风险；当前实际旧色不同目标，风险不解释本轮假阴性。延长固定Sleep/重试采样不是根因修复建议。未见有意Release所有native资源后才等待consumer的确定错误。

## 6. 优先级根因假设（仅1个，避免凑数）

**H1：原前置的窗口/设备生命周期跨线程与无效消息轮询共同造成目标初始化/呈现时序不满足合同，导致提交后未在所采目标形成新内容。优先级P1，但置信度中低。**

- 支持：§2两项正式API要求与现代码确定不符；HH/HL只移动提交；HO此前明确尚未核正式原文。
- 强反证：CC ownerCreation=true/owner提交仍失败，且其Complete已在owner；BJ非owner对照曾成功。因此“CreateDevice单点”“提交单点”“多线程必失败”已否，不作本假设内容。
- 未知：当前普通9窗口链是否实际因这些违规丢交付；最终Release在采样后，不可被当成单次采样失败前因。完整线程作用域修正成功亦不能直接解释内部DWM原因。
- 单因子：**线程执行作用域**（原测试线程vs窗口owner）；不是格式/Present API/次数/窗口style/采样策略多因素扫描。同一角色的native初始化、完成、诊断、释放一起服从该线程作用域，消除混合生命周期；API参数字节与每角色一次Clear/Present保持不变。采样基线和日志两角色完全对称。
- 可判别：原作用域失败而owner成功，仅支持线程作用域敏感，不独证某条调用内因；两边失败否定“完整线程合规足以恢复当前交付”，停止该方向，不再owner变体；两边成功只能证当前能力，不解释历史；原成功owner失败反对所提修正。提交前已目标色/身份漂移/异常/清理失败均记无效或失败，不凑通过。

没有再列Format21、override NULL、提交owner、透明flag、颜色交换、延长等待为新假设：前四已有反证或合法合同，后两无支持。观察者生命周期/GDI标尺是HO旧建议，不冒本轮新发现，也不扩大成本并列运行。

## 7. 推荐唯一下一步与最小预算申请（尚未授权执行）

**下一步只实施并验证H1的原参数native-only线程作用域成对对照；当前先由监督批准范围，不再要求外部先证明显示已变化。**

最小修改：正式ComAcceptance新增一个默认关闭的纯C#诊断partial（2个精确角色/同一方法参数化），复用已有owner和原native完成/诊断调用，不修改生产、默认原shader/resize方法、公共捕获、WindowThread、NativeRun、ExitDiagnostics、项目及打包文件；不加载候选业务或官方WPF业务，不运行HH三帧控制。每角色独立资源；原参数和一次Clear FF123456/普通Present17/完成调用次数不变；对称记录owner/current TID、actual PP、实际test/d3d9身份、提交前client/screen基线与提交后严格00563412、真实Release返回/destroy返回。日志在关键提交前后对称收集，不在WndProc中I/O；不得使用producer读回充内容。

**需调整KQ的两条限制：**

1. “保持原线程不变”只对历史复刻有效；此处批准把线程作用域作为唯一自变量，否则确定API误用无法修正。保留所有数值参数/窗口/颜色/提交次数/consumer门。
2. “总新提交1”改为**每角色1、成对总2**，同一个沙箱任务内两个独立精确角色，才有同阶段原作用域反事实，不能拿跨task HH/KP当严格对照。CC不是此原单帧合同，禁止换名重跑CC。若只批准单角色owner1次，可用于合同合规后的能力观察，但不可作成对根因裁决，本报告不推荐降为这个目标。

申请上限：测试源码新增1文件；生产0；正常COM x86 Release build最多1、正式COM publish最多1、Native publish0；沙箱任务1（角色各一次，总Clear/Present2、无重试、BL120/外180）；不追加x64/official/shader/resize/full。未知ABI静态尺寸在两角色运行前对称断言，不额外提交。首崩溃/超时/身份或参数不闭合停止；两边内容均失败仍正常记录两条failed和资源终态，随后停止不追绿。任何校准结果后先交证据，原ID shader/resize另授权。

本轮执行预算实际全部0；以上不是已执行/已通过。KN有限保留沿用历史，不产生新验收。

## 官方资料索引及获取边界

[D1] [Direct3DCreate9](https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-direct3dcreate9)：本轮取得正文。

[D2] [Multithreading Issues](https://learn.microsoft.com/en-us/windows/win32/direct3d9/multithreading-issues)、[Creating a Device](https://learn.microsoft.com/en-us/windows/win32/direct3d9/creating-a-device)、[D3DCREATE](https://learn.microsoft.com/en-us/windows/win32/direct3d9/d3dcreate)：第一/第三取得正文；第二正文直取超时，官方搜索返回完整windowed示例及同WndProc线程要求。另[CreateDevice正式历史页](https://learn.microsoft.com/ja-jp/previous-versions/direct-x/cc324295%28v%3dmsdn.10%29)官方搜索返回签名及线程/WM_CREATE要求；不是只据full-screen背景外推。

[D3] [D3DPRESENT_PARAMETERS](https://learn.microsoft.com/en-us/windows/win32/direct3d9/d3dpresent-parameters)：官方日文同页搜索返回结构字段与UNKNOWN windowed要求。最初api/d3d9types地址多次超时，未把该失败当取得正文。

[D4] [Present](https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3ddevice9-present)：官方da-dk同页搜索取得签名/NULL矩形要求。 [Clear](https://learn.microsoft.com/ja-jp/previous-versions/direct-x/cc324090(v=msdn.10))官方搜索取得签名、ARGB及Count0；当前英文Clear直取仅标题。[GetSwapChain](https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3ddevice9-getswapchain)取得正文。[GetPresentParameters](https://learn.microsoft.com/ja-jp/previous-versions/direct-x/cc324053(v=msdn.10))官方搜索返回签名。

[D5] [GetData](https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3dquery9-getdata)：官方sl-si同页搜索取得正文；[CreateQuery](https://learn.microsoft.com/ja-jp/previous-versions/direct-x/bb174360(v=vs.85))、[D3DISSUE_END](https://learn.microsoft.com/ja-jp/previous-versions/direct-x/ee416500(v=vs.85))官方搜索返回签名/END1有效EVENT。

[D6] [PeekMessageW](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-peekmessagew)：官方fil-ph同页搜索返回完整参数和current-thread要求。

[D7] [Synchronous and Asynchronous Drawing](https://learn.microsoft.com/en-us/windows/win32/gdi/synchronous-and-asynchronous-drawing)、[ValidateRect](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-validaterect)：官方本地化同页搜索返回直接WM_PAINT/update-region语义。

[D8] [Extended Window Styles](https://learn.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles)：英文直取超时；官方nl-be同页搜索确认TOPMOST=8。其他位值为标准Win32常量静态解码，未声称实际逐位API回读。

[D9] [GetPixel](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-getpixel)最终取得正文；[COLORREF](https://learn.microsoft.com/en-us/windows/win32/gdi/colorref)官方搜索取得00bbggrr定义。

[D10] [GetDC](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdc)：官方nl-nl同页搜索返回client/screen、单线程使用及同线程ReleaseDC要求，英文直取超时。

[D11] [DWM Functions](https://learn.microsoft.com/en-us/windows/win32/dwm/functions)：官方nl-be同页搜索返回DwmFlush等待next present/挂起更新说明；单独DwmFlush页面超时。没有声称获取系统内部目标关联完成合同或匹配SDK头全文。
