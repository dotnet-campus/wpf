# 原 native Present 目标/窗口生命周期审计及唯一下一门

## 0. 决策与本轮边界

**选择：实施一个有界的 WM_PAINT 内呈现事务基线，不再静态报告接力，也不重跑目标/线程/捕获矩阵。** 待监督另授后，在现有 ComAcceptance exe 中一次 x86 测试体完成 A/B 两个独立窗口、各一次 FF123456 Clear/普通9 Present。唯一自变量为“初始化后的直接提交”与“由真实 WM_PAINT 处理函数提交后 ValidateRect”。共同条件为合法 owner 生命周期、原 format0/DISCARD/32×16、同 HWND override、同完成/单次消费观察。它是诊断基线，不冒原 shader/resize 验收；不修改原前置顺序、不绕过失败去运行产品。

没有找到把某次 D3D9 Present 的内容可靠绑定到目标 DWM 私有重定向 surface 的公开 API。GetSwapChain/GetPresentParameters/creation/adapter/CCD 只能建立对象及显示拓扑关系，不能证明内容。本门不伪造这样的绑定，也不增 frontbuffer/dup/WGC/capture。**未发现新确定 ABI 或清屏覆盖错误；原夹具已知线程合同误用仍存在，但 KS 已否定完整 owner 合规足以恢复，不能重开线程方向。**

本阶段只读源码/已有原件/报告/官方网页并写文档；实现/配置修改、build、publish、Native、test、沙箱、终端脚本、再委派均新增0。没有新 Windows API 返回、像素或二进制认证。工作区实际文档根为 WpfGfxShape/Docs，规则 current-execution-rules.md 全文已读；AGENTS 名称搜索未找到。原87d673efe07345f9b0133d2c3287e0f9及所有旧账不改。

## 1. 审计依据、位置和证据上限

先读 next-session-handoff 最新节、native-observation-closeout、sandbox-native-observation-contract-design 关键节及当前事项/计划状态/执行规则。静态亲读 VisualCache65–129、490–555，OutputWindow1–94，SwapChainObservation全文，GpuReadback68–92，WindowThread全文，D3DImageTests.cs结构与消息helper，PresentDiagnostics前段，CC/KS/HH/CG相关实现片段。历史复核报告：KR native-api-contract-audit、KS lifecycle-thread-scope 最新实测、CC全文主要段、CG9–33/44–60、HH42–64、consumer-observability-audit65–85、BJ身份段及BX/HD–HF在该审计的索引。不是重读所有旧原件；大段工具返回截断处不声称全文亲核。

最新原件额外直接核 native-observation-contract.jsonl68–75：原 GetDC 记录在03:55:00.2317157。约211ms持DC到取色扰动沿用 closeout §6，不重新精确拆 API 耗时。H1 owner/input-desktop成员关系和 H2简单DC/clip/GDI同点合同在非原子快照被反驳，不推广为全部物理映射/重定向/指定帧来源已排除。owner Invoke、同步日志和额外screen读不是零扰动。原actual00F3F3F3、expected00563412、shader/resize未到保持。

### 1.1 实际 Present 目标与配置

- VisualCache:92–95附近：CreateDevice16，adapter0/HAL1，focus=目标 HWND，behavior0x26（FPU_PRESERVE|MULTITHREADED|SOFTWARE_VERTEXPROCESSING）；后者不等于软件像素 fallback。PP requested32×16/format0/count1/swap1/windowed1，hDeviceWindow=同 HWND。
- SwapChainObservation:23–31：Clear43 TARGET1/FF123456；普通 Present17(NULL,NULL,同 HWND,NULL)，extended=false。按官方 Present 合同，非COPY时 source/dest/dirty 必须 NULL；当前满足。override明确选该 HWND client；NULL override退回 PP.hDeviceWindow。CC已交叉测试两者而失败，禁止再作为新主线。
- GetSwapChain14(index0)→SwapChain.GetPresentParameters9 获取的是实际隐式chain；最新原案 actual32×16/22/count1/swap1/windowed1。format0=UNKNOWN在windowed合法，解析成22/X8R8G8B8不是ABI错或alpha透明证据。DISCARD后不保留backbuffer，不能提交后读它作内容证据，也无需为Clear强加BeginScene/EndScene。
- 补齐对象关系的合法最小链是 device.GetCreationParameters9 取 AdapterOrdinal/Focus/Flags，device.GetDirect3D6→D3D.GetAdapterMonitor15(实际ordinal)，device.GetDisplayMode8(0)或swap.GetDisplayMode7，GetMonitorInfoW得GDI名称，CCD active paths→DisplayConfigGetDeviceInfo(SOURCE_NAME，adapterLuid/sourceId)关联同一GDI source。**不把CCD sourceId当D3D adapter ordinal，不把坐标包含当frame绑定。** GetCreationParameters不是内容接口。
- CC报告§4已实际记录creation focus/device HWND、GetDisplayMode=[1920,1080,64,22]、adapterMonitor=windowMonitor=65537，12帧仍错；HN和最新原案又有CCD/monitor/点包含关系。把这些查询再加一遍没有足够新增判别力；本门只保留target/实际PP和owner必要不变量，不发起新拓扑扫描。

### 1.2 ABI裁决

现有Stdcall函数指针显式首参this，HRESULT/int32、DWORD/UINT/enum32、HWND/nint、BOOL/int32未见错签。关键slot：D3D.CreateDevice16/GetAdapterMonitor15；Device.GetCreationParameters9/GetSwapChain14/Present17/Clear43/CreateQuery118；SwapChain Present3/GetFrontBufferData4/GetBackBuffer5/GetRasterStatus6/GetDisplayMode7/GetDevice8/GetPresentParameters9；Query Issue6/GetData7。Ex PresentEx121不同签名且原案未进入，不能混用。

PP顺序布局静态计算：x86 size56、Window28/Windowed32；x64 size64、Window32/Windowed40；七DWORD在0..24、无Pack1。creation参数x86 size16（HWND8/flags12），x64 size24（HWND8/flags16）；D3DDISPLAYMODE四DWORD size16。WNDCLASSEXW x86 size48/x64 size80，回调指针与WPARAM/LPARAM为指针宽度。没有修改slot/Pack的依据。

匹配SDK头全文独立认证仍未取得：本次尝试微软公开仓库d3d9.h两个位置404，不能写成已逐slot读头或已运行OffsetOf。函数签名由Learn正文、既有KR顺序核查及真实producer行为交叉支持，后二者不取代头文件证明。下一实现新增结构只限实际需要并在同一测试体提交前检查SizeOf/OffsetOf，不另开微ABI沙箱。

### 1.3 窗口与生命周期：没有确定“灰色画刷覆写”路径

OutputWindow:33–46 class Style0/Background0，exStyle8=TOPMOST而非TRANSPARENT/LAYERED/NOREDIRECTIONBITMAP；WS_POPUP|VISIBLE、无框32×16。WM_ERASEBKGND返回1且不画；WM_PAINT仅ValidateRect；WM_NCPAINT/WM_SIZE/WM_SHOWWINDOW及其他消息转DefWindowProc。没有native resize处理自动Clear/Reset。WM_SIZE可能使client内容失效，不能据此证明发生了清屏；D3D PP说明窗口尺寸变化不隐式重建backbuffer。

EstablishObservableWindow先ShowWindow、SetWindowPos TOPMOST（0x43=NOSIZE|NOMOVE|SHOWWINDOW）、SetForegroundWindow、UpdateWindow、消息处理，再检查cover。这在原Clear/Present之前，不是Present后又主动Show/移动窗口。UpdateWindow可同步发送WM_PAINT；ValidateRect移除更新区，不渲染内容。创建/显示/移动可产生失效区，但当前未取得原案逐事件时序，不能把00F3F3F3猜为某次系统背景刷实际绘制。

CompleteNativePresent创建EVENT query8/Issue END1/GetData(NULL,0,FLUSH1)，上限5秒，期间ProcessWindowMessages、两次DwmFlush。没有ShowWindow/SetWindowPos/InvalidateRect/RedrawWindow/Clear/再次Present；可能经分派消耗窗口消息/更新区域，不是内容读取。query完成不等于consumer；DwmFlush有等待calling app排队DirectX变化到screen的合同，但不返回该目标帧内容身份。

当前ProcessWindowMessages在test线程用owner HWND作PeekMessage过滤、CreateDevice/TestCooperativeLevel/最终device Release不在focus WndProc线程，为KR已确认合同问题；owner本身持续GetMessage，不能称无消息泵。最终Release在采样后，不是先前像素失败前因。KS完整owner作用域仍失败，停止把线程违规当充分解释。

DPI unaware/96只证明调用上下文；同DC LPtoDP和两线程ClientToScreen相等反驳简单偏移，不证明无虚拟化/全部物理映射正确。无NOREDIRECTIONBITMAP仅为样式，不认证DWM内部表面存在或内容。采样时device/HWND仍活；未见提前释放。

## 2. 历史去重与官方最小顺序

|历史|已做/裁决|本门不能重复什么|
|---|---|---|
|BJ fresh-frame §2–6|双架构原生red/blue/red、front/dup/GDI曾真成功，非owner亦成功|不能宣称Basic Render Driver永不产帧，不能用新颜色轮转重试|
|BX §3–5|producer正确而client/screen/front保留GDI绿、dup无fresh|换GetPixel为front/dup不自动有新信息；GDI标尺失败也不是真值|
|CC §4|owner创建×NULL override四格、actual对象/monitor关系成立，12帧失败|不重做owner创建、override或monitor身份矩阵|
|CG §2–6|独立无GDI/Present前GDI paint填绿，之后无paint覆盖仍失败；最终撤回行为钩子|不能再说Present后paint覆写充分解释，也不重做预paint绿/背景刷|
|HH §42–64|原format0单帧及format21三帧都失败，consumer未入|不试格式追绿、不得把official加载作consumer已执行|
|KR→KS最新实测|owner完整生命周期A/B仍001E1D1C，正常清理|不重开线程作用域假设|
|public/direct/WGC各收尾|公共和空root direct内容失败；WGC亦未恢复|这些失败不自动证明环境坏，也不能把新捕获自封权威|

本轮取得官方 Creating a Device 的windowed示例及同focus WndProc线程create/release/reset要求；取得Present、PP、GetPP、GetAdapterMonitor、BeginPaint、WM_PAINT、GetFrontBufferData正文。微软历史Direct3D9教程 Step3（cc324402）明确 **WM_PAINT→Render→ValidateRect**；Step4（cc324403）为Clear→可选scene绘制→EndScene→Present。教程采用这一模式不意味着WM_PAINT是所有D3D Present的强制前提，原直接Clear/Present本身不能因此判非法。

工作区真实WPF d3ddevice.cpp:3293–3335 PresentWithD3D 使用 **实际swapchain.Present(source,dest,目标HWND,source为NULL则dirtyNULL,flags)**，返回S_OK仅标PresentProcessed；该调用链不是要求产品把D3D调用塞进WM_PAINT的依据，也没有source→DWM surface私有绑定。WPF渲染线程呈现策略与示例paint策略都是合法形态，不把后者迁移为产品架构。

**新增判别力只在“把唯一D3D提交置于更新事务之内”**：CG是在WM_PAINT中画GDI，然后返回再Present；KS是在owner Invoke中Present，不在WM_PAINT。两者均未检验WM_PAINT实际处理期间的D3D提交及提交后验证更新区。当前不重复“是否有paint覆盖”的老假设。

## 3. 仅一个可证伪假设

Hpaint：在当前运行条件，初始化后独立提交与真实WM_PAINT处理中的提交有可观测差异；后者能把原00563412呈现到同目标client而前者不能。它是**低置信、可一次否定的消息事务敏感性假设**，不是已证明的Windows必需语义或灰色来源解释。

对象：每角色独立HWND/device/chain0；实际PP、focus/owner及sample绑定该角色，不跨窗口拼接。时间：建立/清空初始paint完成→提交前非目标色基线→一次Clear/Present→完成→一次GetDC/GetPixel→ReleaseDC→清理。目标内容全程FF123456；采样仍COLORREF00563412、原每通道差<=1，不改变色值/阈值。没有frame token则承认没有指定帧身份认证，不能报物理scanout通过。

可证伪信号：A失败且B成功支持事务敏感性但不独证DWM内部原因；A/B都失败否定该修正足以恢复，**停止paint/线程/override方向，不再变形基线**；A/B都成功仅证明当前最小能力恢复，不解释历史；A成功B失败反对修正。提交前已目标色、身份/PP变化、关键事件缺失/溢出、清理异常均不能凑通过。

## 4. 唯一下一门：单体、两角色、一次任务，规格已可实现

名称建议：WhenPaintScopedNativePresentIsComparedThenOriginalClientPixelIsRequired（新增默认关闭诊断测试，不借旧UID）。同一owner内顺序A/B；不是两次沙箱，不调用旧fresh/control入口，不加载official业务。

共同准备：复用现有CreateOutputWindow/owner Invoke，设备create、TestCooperativeLevel、最终Release及DestroyWindow全部owner；原32×16/format0/count1/DISCARD/windowed/HAL/0x26不变。共同初始化Establish只在owner运行。owner合同合规是固定基线，不作为自变量，也不声称重测KS有新增信息。初始paint/Show在arm前完成。

- A：owner直接一次Clear FF123456/Present(NULL,NULL,window,NULL)，普通WM_PAINT保持只ValidateRect。
- B：相同状态准备后arm一个per-HWND一次性呈现动作，调用InvalidateRect(window,NULL,FALSE)→UpdateWindow，必须取得真正WM_PAINT且非空更新区。WM_PAINT内先标记entered/解除arm（防重入），只执行一次同参数Clear/Present，再ValidateRect并记录返回；不FillRect/BeginPaint擦色，不新增scene、sleep、颜色循环。若Present产生重入paint，记录且不第二次提交。
- WndProc仅固定容量内存环（建议64项，QPC/TID/HWND/msg/wParam、序号）；追踪WM_PAINT/ERASE/NCPAINT/SIZE/SHOWWINDOW/WINDOWPOSCHANGED/DPICHANGED，溢出判证据不完整。异常不得越过UnmanagedCallersOnly边界；保存失败状态，回到测试方法后断言；不在WndProc中Invoke、等待、hash、Console或文件I/O。事务内不创建query、不调用Complete（其泵消息会递归）。
- A/B在提交返回后相同位置调用现有CompleteNativePresent一次；相同query轮询与5秒上限/两次DwmFlush，不重复提交。记录实际PP/creation及线程不变量安排在提交前；GetUpdateRect、目标矩形/事件摘要放在GetDC前或ReleaseDC后。
- 首采样仅GetDC→GetPixel(16,8)→ReleaseDC相邻调用，以内存保存QPC/返回；中间不owner Invoke、不写盘、不screen单读，消除上门约211ms人为持DC延迟（不把此时序差冒paint因果，两角色完全相同）。A/B都在相同owner上下文采样；DC非零、raw非CLR_INVALID、释放成功分别记录。结果全部采样/清理后一次JSON落盘。
- 一般像素失败允许第二个预注册角色完成，以得到A/B；身份/ABI/关键API失败、超时或崩溃立即停止，不继续B。device/query/factory Release返回与窗口销毁实际结果记录，保留主异常、cleanup失败另列并使整体非绿；borrowed句柄不释放。

不加frontbuffer：其官方合同为copy device front buffer、windowed destination为desktop大小、A8R8G8B8；它是显示侧独立观测而非producer，但CC/HH/CG已有相似成功HRESULT而旧内容。本门没有新fresh token/目标内容绑定，不能证明再读有新增判别力。不加dup/WGC/PrintWindow/BitBlt或Windows截图；现有工具只能分析已存在图片而不能自动赋予截图采集来源。本门无需新capture，不称GetDC采样为producer或物理scanout。

### 4.1 最小文件、配置和执行预算（全部待授权，当前0）

仅 Tests/WpfGfxShape.ComAcceptance/：

1. 新 D3DImageTests.NativePaintBaseline.cs：整文件受新NATIVE_PAINT_BASELINE符号保护，私有独立声明/状态/诊断测试/A-B流程，复用owner和完成helper；不复用候选结构作为独立ABI唯一预期。
2. D3DImageTests.OutputWindow.cs：在原WndProc开头增加同符号下的精确HWND内存钩子；未注册窗口/默认配置完全原行为，B唯一一次事务后即解除。不改默认paint策略或共享消息helper。
3. WpfGfxShape.ComAcceptance.csproj：新增可选NativePaintBaselineDiagnostic配置/Optimize及仅此配置runtime10.0.13 pin（仿现有诊断配置），默认Release/Debug关闭。不能复用NativeObservationDiagnostic同时打开旧hook，避免重引211ms路径。
4. AcceptanceResources.resx仅新增必要用户可见诊断键；沿用现有资源规则，不新增库/Host/工程/生产改动。若不需要新用户文本则不改此文件。

正常COM build≤1：默认Release/win-x86正式依赖图；COM publish≤1：显式NativePaintBaselineDiagnostic/win-x86、独有artifacts/native-paint-baseline-x86-r1/{obj,bin,publish,clean}，内部编译验证新分支（不另计独立build）。打包复用原15A545 native，Native publish0。不得以旧official根DLL替候选Native。测试身份新SHA/MVID必须实际登记，不移绑A48736C8。

沙箱≤1/测试体≤1/A-B各1/总native Clear与Present各≤2；发现0、x640、额外official0、full0、新Host0、重试0。正常build失败即止；publish失败即止，不以剩余沙箱修包。正式clean根exe，现有--supervise-exit、任务内Results/native-paint-baseline-x86、120秒、精确等值FQN WpfGfxShape.ComAcceptance.D3DImageTests.WhenPaintScopedNativePresentIsComparedThenOriginalClientPixelIsRequired；外180秒，完整回收，按真实TRX UID核体，不预借旧UID。配置和输入需监督显式认可后才实施，不自动使用历史剩余预算。

### 4.2 停止/转向标准与原主线关系

这是一次最小可见呈现基线，**无论结果均结束此门，不要求下一轮继续静态研究**。有界输出必须包括原始A/B像素/事件环、owner/actual PP/creation、identity、TRX、OS监督退出/超时、清理。A/B都失败后不得再申请paint/owner/override/format/DPI变体或同类capture；转向显示内容路径不能闭合的明确失败项，保留原shader/resize Red/未达，后续工程只能选择有新内容证据的产品工作，不宣称环境根因或强推外部维护者。A/B同过或仅B过则提供“当前最小可见呈现可用”的有限事实；仅B过时可据事件范围申请把该呈现事务作为原native前置的夹具修正，**另授权后**原00563412前置仍必须先过，原FF80/shader/真实64/48/32×24 resize及后态不变。B成功不等于可先跳过native断言直接验产品，也不授权整个候选业务迁移到WndProc。

本设计不调整原shader前置顺序；若未来采用paint内原生前置，那是为了按已测消息事务修正测试夹具，非改变产品可见像素要求。若本门失败则没有采用此修正的证据。

## 5. 官方出处与取得限制

- [Present](https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3ddevice9-present?view=vs-2019)：NULL矩形、目标override、client坐标、scene限制；无query/内容token保证。
- [PP](https://learn.microsoft.com/en-us/windows/win32/direct3d9/d3dpresent-parameters)：windowed UNKNOWN合法、默认窗口、位置跟踪/尺寸非隐式reset、DISCARD。
- [GetPP](https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3dswapchain9-getpresentparameters)：associated parameters，非内容；surface.GetContainer可追parent chain，仍非消费证据。
- [Creating a Device](https://learn.microsoft.com/en-us/windows/win32/direct3d9/creating-a-device)：windowed示例和focus WndProc生命周期线程要求。
- [GetAdapterMonitor](https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3d9-getadaptermonitor)、[CCD source name](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-displayconfig_source_device_name)：实际creation ordinal→monitor与LUID/source→GDI名称，不能跨层当surface。
- [WM_PAINT](https://learn.microsoft.com/en-us/windows/win32/gdi/wm-paint)、[BeginPaint](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-beginpaint)：更新区域、显示/尺寸失效、背景刷、Begin/End配对；本门采用D3D教程Render后Validate，不以空BeginPaint当修复。
- [D3D9教程Step3](https://learn.microsoft.com/ja-jp/previous-versions/direct-x/cc324402(v=msdn.10))（搜索返回正文）、[Step4](https://learn.microsoft.com/ja-jp/previous-versions/direct-x/cc324403(v=msdn.10))（官方搜索返回完整主要示例，content空）：WM_PAINT内Render、Clear/Present。不复制大段示例。
- [GetFrontBufferData](https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3ddevice9-getfrontbufferdata)：显示侧copy合同及成本，不是本门新增建议。

Present首次正文超时/仅标题，带view参数后取得正文；GetCreationParameters/GetDisplayMode本次直取超时，签名参照已有KR/CC，不冒本轮取得正文。官方GitHub教程猜测路径404，未把它作为已读样例；真实WPF仅核上述仓内片段，非整条产品绘制链认证。

## 6. 保留终态

87d…原门build1/1（133warn0err）、COM publish1/1、沙箱1/1、体1/1均已耗尽，与本设计申请独立。诊断原件/hook和默认关闭状态不改。x86 Native15A545、x64 Native54BC62的双架构有限生命周期证据独立保留，不扩大target GetDesc/device identity/backend和内容覆盖。显示内容根因未定、完整替换Open，非已证ExternalBlocked；沙箱管理能力不因工具参数缺省而被否认。不需要外部维护者才允许实施本规格。

计划状态：静态定位/历史去重/官方合同决策已完成；唯一下一门规格ReadyForAuthorization，Implementation/Build/Publish/Sandbox均NotStarted，当前预算全0；不是继续静态调查排程。
