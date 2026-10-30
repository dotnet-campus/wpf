# consumer HWND 双观察：实施前可行性门（静态，停止实施）

## 1. 裁决与本轮账本

**原约束下不可直接批准实施。API 路线可行，现成依赖路线未闭。** 合法 official consumer 协议可独立复用，毋须再次通过 native-only 前置；但两个正式项目没有可直接使用的 WGC/WinRT 公共投影。若保持项目、依赖和打包完全不变，必须自行实现一组尚未认证的 WinRT ABI，而不是只加一个 CreateForWindow P/Invoke。本门不采用这种做法，不把“C# 能声明接口”当作实现已经可行。

本轮代码/项目/依赖/包/生产/公共捕获/入口修改 **0**；build **0**、publish **0**、程序执行/测试发现/测试 **0**、沙箱 **0**、终端/脚本/委派 **0**。仅新增此调查文档并更新当前工作项。没有重新认证历史任务的二进制或原始帧，也没有提供新的像素实证。

上一份 audit 的“同帧”在此明确收窄为：**同一次递交、下一次修改前稳定阶段的两种观测，不是同一物理扫描帧，也没有两个 API 共享的 frame token。** 上份三色建议由本次用户要求收窄为固定两次不同标记；不是扩大运行预算。

GetPixel 没有通用 GPU surface/指定 Present 帧读取保证，仅表示保证不足，不构成它必错、KS 已成功呈现或环境故障的确定根因。WGC 不自动成为真值。

## 2. 亲读范围与工程能力

已分段读完最新 `replacement-supervised-consumer-observability-audit.md` 1–141（长返回截断处补读）、`handoffs/current-execution-rules.md` 全27行、`current-stable-baseline.md` 全20行，读 current-work-item 最新1–20。未声称全历史全文。AGENTS.md、instructions 名称检索未找到适用规则文件。两个正式测试项目文件清单已取得。

已读两个正式 csproj 全文、生产 csproj 全文、WpfGfxShape 本地 Directory.Build.props/targets 全文、根与 ComAcceptance global.json 全文。SDK 10.0.107/latestFeature；没有修改 SDK/C# 版本。

|对象|已确认能力|不能当作现成 WGC 的理由|
|---|---|---|
|ComAcceptance|net10.0-windows、UseWPF、unsafe、Nullable；MSTest4.0.1；正式 NativeRun/ExitDiagnostics、STA HWND owner|TFM 无 Windows API 版本，没有 Windows SDK .NET 投影引用；UseWPF 不导出 Windows.Graphics.Capture。没有产品 ProjectReference。|
|Tests|net10.0、unsafe、Nullable、MSTest4.0.1；引用生产项目|传递 Silk.NET.Direct3D11/DXGI/Direct3D9 是 native COM 绑定，不是 WGC WinRT 投影。|
|生产项目（只读）|Silk.NET 2.23.0 上述三包、CsWin32 0.3.298 PrivateAssets|不移动/公开内部 WinRT 类型，不改生产依赖，不把生产 interop 当 official 独立预期。|
|现有 DesktopDuplication partial|D3D11CreateDevice、texture desc、staging、CopyResource、Map/Unmap、RowPitch、Release|能够参考/复用同 partial 的私有 native 声明；output duplication 生命周期、screen crop 和 freshness 不能拿来充 WGC。旧 device flags=0，不能无审计照搬为 WinRT device。|
|WPF 框架源码|PresentationFramework/MS/Internal/WindowsRuntime/Generated/WinRT.cs 有 internal Platform/activation 支持；PresentationCore 有 internal InputPane RCW|不是正式验收项目可用的公开 WGC 投影；不编辑生成文件、不整套拷贝、不用反射越界访问内部 API。|

在 WpfGfxShape 范围搜索 Windows.Graphics.Capture/CreateFreeThreaded/CreateDirect3D11DeviceFromDXGIDevice/RoInitialize/WinRT.Runtime，除报告外未找到捕获实现；src 的相关命中是上述内部 activation 支持，没有 WGC 实现。现有 artifacts 未取得 project.assets.json；obj 文本检索无 SDK 投影命中不是当前二进制依赖闭包认证。

**新增关键阻塞：** `ComAcceptance.Package.targets` 33–55 的 self-contained clean 包只复制明确列出的测试/MTP/Contracts/Native、RuntimePackAsset 与指定 ReferenceData，不按全部 publish 输出或 CopyLocal 依赖复制。即使批准 Windows SDK 投影并成功 publish，也不能假定 Microsoft.Windows.SDK.NET/WinRT.Runtime 等非 runtime-pack 文件进入 clean 包。不能手动塞 DLL、篡改清单、上传 publish 目录绕过这个事实。

## 3. 正式 consumer 合同能复用什么

亲读 HbSurfaceConsumer 主要创建、提交、清理路径，OutputWindow/WindowThread 全相关段，以及 HD 报告合同43–62和身份/内容相关段。历史成功仍以 HD/HE/HF 报告为依据，本轮未运行重验。

- `RunHbConsumer` 82–98 默认会调用 CheckFreshDesktopDelivery 并早停，不能直接调用它完成本门；observePartition=true 还注册分区观察，不是合法的“只跳前置”开关。
- 新的默认关闭诊断独立采用其 official Load/export、32×16 format21 surface、interop create、async connect(0)、四资源图、92字节 target(flags0xC/合法协议 DPI context/真实 DPI)、72字节 settings、setup close/commit/flush、invalidate 的已认证协议片段。不改旧入口或 helper。
- 两次各自 ColorFill → producer GPU completion → dirty → 原 event reset → DuplicateHandle → separate Present send → commit → 最多5秒 event wait → consumer flush。成功转交的 duplicate 由 native 关闭，不能测试侧再 close。事件是处理/生命周期信号，不是像素成功证明。
- **准确区分 Present：** 这里是 official D3DImage PresentPacket，不是另做 Device9 swapchain Present。`CompleteNativePresent` 名称虽含 Present，其实际实现是 EVENT query/GetData/泵/DwmFlush，无 Device9::Present 调用；可以复用已认证 GPU 完成片段，不引入 native-only 颜色前置。native-only Present 预算0。
- 不复制旧按 expected 重试 GetPixel 的循环。两种观察都保留首个有效观察结果，错误内容立即归类，不能改 expected/阈值。
- 旧 finally 的 detach/逆序资源注销/delete close+commit+flush/channel destroy/disconnect/deinitialize/COM Release/owner Destroy 是所有权依据，不照搬会在 finally 首个断言失败时中断剩余释放的结构。新诊断独有清理必须尽力释放全部已拥有资源，最后统一报告清理异常；DLL 按进程驻留。

由此 consumer 本身**静态可复用且无必要 native 前置阻断**；没有证明新诊断当前已存在或已可被发现。

## 4. WGC 最小公开 API 合同与未闭实现

本轮成功读取微软正式正文（正文顶部导航授权提示不等于正文不可读）：

1. [desktop WinRT 配置](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/desktop-to-uwp-enhance)：.NET6+ 用带 Windows OS 版本的 TFM，引入适当 SDK targeting package；不是无版本 net10.0-windows 自动具备投影。SDK.Contracts 是旧框架路线，不推荐给本项目。
2. [CreateForWindow](https://learn.microsoft.com/en-us/windows/win32/api/windows.graphics.capture.interop/nf-windows-graphics-capture-interop-igraphicscaptureiteminterop-createforwindow)：精确 HWND、返回 GraphicsCaptureItem，最低18362/1903。
3. [CreateFreeThreaded](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.direct3d11captureframepool.createfreethreaded?view=winrt-26100)：17763+，移除 DispatcherQueue 依赖，FrameArrived 在内部 worker。
4. [screen capture](https://learn.microsoft.com/en-us/windows/uwp/audio-video-camera/screen-capture)：IsSupported、pool/session/StartCapture、可手动 TryGetNextFrame；SystemRelativeTime 是 QPC 时间；Dispose 归还帧，归还后不能继续引用其 underlying surface；ContentSize 外区域可能未定义。
5. [D3D11 WinRT device 桥](https://learn.microsoft.com/en-us/windows/win32/api/windows.graphics.directx.direct3d11.interop/nf-windows-graphics-directx-direct3d11-interop-createdirect3d11devicefromdxgidevice)：IDXGIDevice → IInspectable，QI 为 IDirect3DDevice。
6. [surface native access](https://learn.microsoft.com/en-us/windows/win32/api/windows.graphics.directx.direct3d11.interop/ns-windows-graphics-directx-direct3d11-interop-idirect3ddxgiinterfaceaccess)：IUnknown/GetInterface 获取 wrapped native 接口。该页面另有明显不相干的继承文字，不据其推导 vtable；以声明/实际 SDK 为准。
7. [RoInitialize](https://learn.microsoft.com/en-us/windows/win32/api/roapi/nf-roapi-roinitialize)：所有交互线程先初始化；S_OK/S_FALSE 均须同线程平衡 RoUninitialize；RPC_E_CHANGED_MODE 不是成功，不强改现有 STA。
8. [GraphicsCaptureItem](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.graphicscaptureitem?view=winrt-26100)：Agile/Both，Size、Closed；不意味着事件必须切回 HWND UI。
9. [RequestAccessAsync](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.graphicscaptureaccess.requestaccessasync?view=winrt-26100)：特殊捕获功能权限，Borderless 需要 manifest capability；不为本门主动取消边框/申请额外权限。
10. [D3D11 Map](https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-map)：DO_NOT_WAIT 时忙返回 WAS_STILL_DRAWING，CPU read 可报 device removed。

RoGetActivationFactory 文档抓取504、IsBorderRequired页面404，不把失败页面算已读合同。尚未逐IID/槽位核验 SDK metadata；没有声称已认证原始 framepool ABI。

最小链为：专用 MTA capture owner 初始化 → IsSupported/系统版本 → activation factory 的 IGraphicsCaptureItemInterop.CreateForWindow(consumer HWND) → D3D11 device(BGRA support，不开 debug layer、不软件 fallback追绿) → QI IDXGIDevice → WinRT wrapper → CreateFreeThreaded(BGRA8、2 buffers、item.Size) → session.StartCapture → drain → 两次递交各取帧 → surface access/texture/staging/copy/map。

保留系统默认边框，不用 picker、不需要用户桌面点击，不请求 borderless 或 programmatic 新权限特性，不改 manifest。CreateForWindow 是本门 desktop 路线而非假造用户同意；IsSupported=true、版本满足、创建成功仍不保证实际访问、策略允许或内容交付。E_ACCESSDENIED、item closed、protected/excluded content、远程/会话限制、设备创建失败逐项记录为能力不足，不能绕过安全策略。不承诺当前 sandbox 支持 WGC。

若不用投影，新增的不止两个 IUnknown：HSTRING/activation、IInspectable 的正确布局、item/statics、pool statics2及实例、session/statics、frame 的 ContentSize/Surface/SystemRelativeTime、IClosable，加上事件委托或可靠轮询与引用所有权；泛型 delegate 的参数化 IID/跨线程生命周期尤其不能猜。**不批准在两个小文件里压缩大量陌生 ABI。** 可采用 CreateFreeThreaded+手动 TryGetNextFrame 降低事件复杂度，但这只是减少事件 CCW，不消除其余投影需求。

## 5. 固定两次标记的新鲜性合同（后续实现要求）

只运行一个 official consumer 场景；固定 A=opaque red(FFFF0000)、B=opaque blue(FF0000FF)，各覆盖目标有效32×16 client，绝不重复/追加绿帧。producer 值只作递交标签，不能填 consumer 观测列。

1. 先创建 consumer HWND/资源/setup，保存 PID/TID/HWND、窗口类、client/window/DWM几何、DPI、session、item 创建来源、item.Size。使用无边框 popup 的已认证32×16合同；若 item/client/frame 几何无法唯一映射，停止而非扫描找颜色/移动 crop。不能先假定整个 WGC texture 就是 client。
2. 两个 target 的 WGC session 均在各自第一次标记前建立、StartCapture。取尽当前可用帧并 Dispose，记录 drain 最大时间戳。基线观察与 drain 全部受5秒截止限制；一直有帧无法完成 drain 则不具判别力。
3. 每次 consumer 修改前再次有界 drain；记录 QPC frequency、submissionStart/dirty/separate-send/commit/event/flush 时间，选择门槛采用 flush 返回后 QPC（保守稳定阶段），而非任意 desktop 更新时刻。SystemRelativeTime 的 TimeSpan 为100ns单位，与 Stopwatch ticks 必须按频率转换，使用足够精度的比较，不能直接比整数。
4. 截止从本次 submissionStart 固定10秒，涵盖 event≤5秒及flush后的取帧/拷贝/读取；不会每拿到空帧、旧帧或 busy 就重置截止。同步系统调用本身不承诺可中止，最终由120秒子监督/180秒任务监督兜底，不冒称每个API严格可取消。
5. 首个满足该 item 来源、时间门槛、有效 ContentSize/固定 crop/format/边界的帧立即锁定；内容检查不参与选择条件。旧时间帧可释放后继续；尺寸/格式/目标身份异常为合同失败即停；首个合格帧即使是黑/旧色/错误色也记录并判失败，不能丢掉去等下一正确帧。若无合格帧为 timeout/not observed，不用黑初值填充。
6. 锁定后，在下一次修改之前采一次 window DC 与一次 screen DC（记录各自QPC、有效DC、ClientToScreen、CLR_INVALID、遮挡/桌面状态）。各 GetDC/ReleaseDC 同线程成对。读取完整有效crop的WGC RGB并保存原始BGRA字节、尺寸/rowpitch/时间/来源；固定中心样点与所有有效32×16 RGB逐字节精确比较，alpha单列不猜opaque语义。不得屏幕截图/duplication/producer staging替代窗帧。
7. 对A、B分别报告 HWND/crop/时间/实际内容。item没有逐帧HWND字段，用创建绑定与连续生命周期证明来源；时间只证明晚于门槛，A→B对应内容才补充递交关联。不宣称因果token、全图复杂图形覆盖或物理扫描帧证明。

## 6. 独立 GDI 标尺与线程、内存、释放

标尺**必须两次不同标记**，否则不能排除静态旧帧。使用另一独立、不重叠、非D3D窗口，其两次 A/B 更新也分别记录window/screen DC及该标尺自己的 CreateForWindow 帧；单标记正控不足以建立本门判别能力。标尺先完成，第二次后保持静止，不在consumer测量区间反复paint；不向consumer FillRect、不创建duplication、不切换session/display。若标尺失败，保全已经采得证据后停止，consumer列可为not entered，不补跑。

标尺窗口/session可能影响composition、时序和边框，不能说绝对无扰。保持标尺静止且配置固定，两种观察是同一递交下的测量路径比较，不是证明观察者无影响的严格因果单因子实验。不额外添加 WGC off/on 运行；要证明WGC导致恢复须另行授权反事实实验。

线程：保留现有专用 STA HWND message pump；测试/协调线程不在owner Invoke回调内等待event/capture。capture专用MTA拥有全部WinRT对象和immediate context；优先CreateFreeThreaded+有界TryGetNextFrame手动拉取，无 FrameArrived回调可消除事件泵和委托ABI。采用事件时只signal，不Dispatcher.Invoke、不等待测试/owner、不在锁内调用外部回调，先静态审计后授权。不能使用依赖当前线程DispatcherQueue的Create再同步Wait。

现有 PresentationWindowThread 的 ready.Wait/completed.Wait/Join 无超时；因此不能把“复用helper”描述成所有等待已固定截止。默认不改该公共helper，新诊断采用独有有界协调封装/调用，owner操作仅创建/销毁和短GDI工作；任何owner不收束由监督终止并分类失败，不重开线程作用域对照。该独有实现仍须先审核，不能把长等待藏到Invoke里。

内存：BGRA support device；WinRT wrapper、QI返回值各有独立所有权。只在frame仍checkout时取native texture，copy到自有staging；在归还帧前完成所需copy/read，或明确证明GPU copy同步/生命周期合同，最小实现选前者。检查texture desc/sample/format/ContentSize、checked rowpitch和坐标；Map用DO_NOT_WAIT有界等同一纹理，不换帧追内容；成功后finally Unmap，复制有效行到自有managed内存后再释放。禁止保留归还帧后的surface/mapped指针。

停止顺序：标记stopping；若订阅则解除事件并有界等待在途handler；session.Close/Dispose停生产；处理释放已checkout frame/texture；pool.Close/Dispose；释放staging/WinRT device/IDXGIDevice/context/device/item/factory/HSTRING所有权；同capture线程RoUninitialize；释放consumer channel/bitmap/surface/device资源；最后owner销毁窗口并有界收束。实际依实现建立逐项所有权表；不强断言所有WGC/系统COM Release计数必须0，记录可观察的close/引用平衡。不可中止的native close挂起由监督归类，不用漏释放假装clean。

## 7. 最小替代范围请求（本轮不实施）

**推荐申请一次局部范围放宽，优先不加显式第三方包：** 仅 ComAcceptance 改为带 Windows API版本的 net10.0-windows10.0.19041.0（或经实际SDK确认的最低18362），启用官方 Windows SDK .NET 投影；明确这是 TFM/隐式SDK依赖变更，不是“零依赖”。无需 Win2D/Windows App SDK/新Host。受约束的更改即使只是诊断条件属性也必须明确获准，不能通过全局构建参数偷换TFM。

候选最小文件范围（均须另授权）：

- 新 `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.ConsumerWgcDiagnostic.cs`：唯一默认关闭符号 `CONSUMER_WGC_DIAGNOSTIC` 内的[TestMethod]、独立 official 合法协议、两次标记/标尺/证据/有界协调；不调用RunHbConsumer或native前置。
- 新 `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.ConsumerWgcCapture.cs`：同符号内的投影WGC owner、CreateForWindow薄interop、D3D11桥与frame/staging读取/cleanup/纯内存负控。复用同 partial 现有私有native声明，不改DesktopDuplication/OutputWindow/WindowThread/VisualCache。
- `Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`：仅获准的SDK投影配置，不能加产品引用；显式NuGet包若确需，则在确定精确官方版本/API/闭包后再申请，不先猜版本安装。
- `Tests/WpfGfxShape.ComAcceptance/ComAcceptance.Package.targets`：仅把经解析确认的投影运行时依赖及必需资源纳入现有clean包/清单，不整目录兜底复制，不改变Native角色。
- 仅若新增用户可见文本需要，更新现有 `AcceptanceResources.resx`；代码阶段再核其资源习惯。

Tests项目、生产、公共捕获、入口及其他打包规则保持0；没有必要向Tests强塞Windows能力。纯内存过滤/crop/旧色负控可在同一个新ComAcceptance测试体内先执行，复用真实选择器，不额外增加Tests源码或单独运行预算。若要求独立Tests测试，应单列范围与构建执行预算；两个不同实现的过滤器测试不能冒实际选择器覆盖。

**若项目/打包仍绝对不可改：本门停止，没有满足要求的现成替代实现。** 手写整套raw WinRT ABI须另行专项SDK核验和范围授权；PrintWindow、BitBlt、屏幕截图、producer读回均不能替代要求的HWND WGC窗帧。此处不以删减观察目标的方式追绿。

## 8. 后续真实发现、clean包前置及预算建议

以下是另行授权后的上限，不是当前授权；依赖静态门先关闭，再实施。新方法拟唯一FQN：`WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialConsumerTwoMarkersAreObservedByGdiAndWindowCapture`。当前源码中不存在、未发现、无UID，不冒称现成测试。

1. 先确认实际SDK投影精确版本与公开API、薄interop签名、投影运行时闭包及package copy路径；无此依据不进入一次编译碰运气。
2. build最多1，仅ComAcceptance、x86 Release、正常分析器、独立obj/bin/artifacts，默认关闭状态由符号保护静态审核；publish最多1、同输出self-contained且尽可能no-build，复用已认证Native，不发布生产。publish若必须重新编译，额外编译次数照实登记并须预算放宽，不把它隐掉。
3. clean包须由既有AfterPublish机制产生，检查根exe/test dll/deps/runtimeconfig、Contracts、aot.sha256/package-files、Native candidate及投影依赖。Native根与Native子目录角色不能混淆；根wpfgfx为official runtime，Native/wpfgfx为candidate包依赖（本门不运行candidate）。不能上传publish代clean。
4. 发布后可以只读公开API检验新方法存在且HB/KS旧实验符号未混入，但这不是MTP真实发现或属性/UID认证。**唯一沙箱的精确filtered正式运行必须由MTP真实发现并只执行1个新FQN**；TRX total/executed=1、stages进入新体、test SHA/MVID/UID和源码对应才算发现与执行成立。无需另跑--list-tests；若要求运行前单独真实list，则须另给发现执行预算，不能伪称本阶段完成。
5. 静态工具不能非执行完整认证PE/SHA/MVID时明确剩余未知，不伪造清单；授权后的正式AssemblyInitialize继续强制Native PE架构/无CorHeader/SHA匹配；监督记录test SHA/MVID，诊断Load返回句柄对应actual模块路径/SHA/版本。新投影依赖缺失为初始化/设施失败，不是像素产品失败。旧KS身份不能绑定新包。
6. 沙箱最多1、子监督120秒、外180秒；official场景1、consumer递交2、独立GDI标尺更新2、native-only Present0、candidate0、x64/full/单独发现/重跑0。每次标记固定10秒（event≤5秒），baseline/drain≤5秒，map在同一截止内；成功也立即停止。零命中/初始化失败同样消耗任务预算。
7. 必回读：clean清单/identity、监督stdout/stderr/exit/timeout、唯一TRX/UID、stages、两目标元数据/实际像素/原始crop、时间与几何关联、所有资源close/Destroy/owner及capture线程收束。不得仅凭Passed或截图认定内容成功。

## 9. 结果分类与停止

- **实施门Blocked（本轮）**：投影与clean打包能力不在原范围，尚未批准；没有新运行结论。
- **发现/身份/设施失败**：0或多命中、错误符号/UID/SHA/架构、缺依赖/清单、AssemblyInitialize失败；不归产品像素。
- **WGC能力不足/不具判别力**：不支持、权限/策略、item关闭、device失败、标尺两次未闭、尺寸/crop/时钟合同无效或无合格帧；保全后停止。
- **有效标尺且consumer WGC两次对应、GDI旧**：支持本次观察边界分离，不证明历史KS成功或GetPixel必错。
- **有效标尺且consumer WGC/window DC对应、screen旧**：支持目标内容与desktop观察分歧，不唯一归DWM/显示端。
- **有效标尺且consumer各路径两次对应**：本次official内容能力成功；不解释旧失败、不验candidate/backend/full/恢复/物理scanout。
- **首个合格帧错误/各路径旧**：内容合同不匹配，削弱“仅GDI漏采”的充分解释；不跳帧追绿，不唯一归环境。
- **崩溃、超时、device removed、清理/线程未收束**：分别记录执行/生命周期失败与已取得内容，不能靠强杀写clean。

一次compile/publish失败或上述任何停止项即收束，不修包重跑；所有路径都不自动进入candidate/shader/resize/线程变体。完整替换仍未完成。
