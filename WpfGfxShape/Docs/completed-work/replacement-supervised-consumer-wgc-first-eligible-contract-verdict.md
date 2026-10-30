# WGC first-eligible / startup 内容关联静态裁决：停止诊断分支，返回替换主线

## 1. 实质裁决与本轮范围

**建议停止继续 WGC 实验，不再为透明首帧增加事件、启动顺序、等待、尺寸或线程变体。冻结已有证据，另授权精确撤回临时 SDK/诊断改动，回到完整替换方案 §8.2–3 的“当前候选回归基线与历史 HW 逐项覆盖裁决”，而不是再次证明捕获环境恢复。**

first-eligible 作为预先约定、与颜色无关的观察抽样规则是可复现的；把它当作“指定 paint/consumer 提交已进入此 Surface”的 API 保证则不成立。当前失败精确含义仍是该诊断观察合同不匹配，不能升级为 official consumer、candidate、WGC 实现或显示环境的产品失败。最新 task 在标尺即停止，真正 consumer interop/资源/提交 0/2；创建 consumer HWND 和 capture 不等于进入 consumer 内容链。

本阶段仅静态代码/官方文档研究及本报告、恢复入口、当前事项文档更新；生产/测试源码/项目/资源/包修改 **0**，build/publish/发现/测试/沙箱/终端/脚本/委派 **全0**。未重新计算二进制哈希、解码旧 BGRA 或重验历史 TRX，不把历史报告引用冒充新实证。

WGC 累计保持 **build4（1败3成）、COM publish4（1败3成）、沙箱3（发现1、真实2）、真实体2均失败**。最新 task `e1c69af0bdb04532ac71d22f5827f25f`。KS 独立保持 **build1/COM publish1；沙箱4、精确请求3、发现1、实际体1、A/B各1、native提交2**，不并入 WGC。

## 2. 阅读范围与定位

已读最新 next-session-handoff 顶部及相关恢复状态、owner-paint-control 全报告、observability-audit/feasibility 的结论与 API/抽样/线程/停止合同、current-execution-rules 全文、current-stable-baseline 全文、current-work-item 最新状态、完整替换方案（中部长返回截断，§8–10另读）、HD/HE/HF 与 KN/KO 报告相关成功/边界段。AGENTS/instructions 名称检索没有找到额外指令；读 .github/CONTRIBUTING.md。没有声称通读累计历史。

源码以 `Tests/WpfGfxShape.ComAcceptance/` 为根：
- C = `D3DImageTests.ConsumerWgcCapture.cs`，阅读初始化、owner、Drain/Observe/selector、Surface→texture→ReadTexture、纹理控制、Dispose 实际逻辑。
- D = `D3DImageTests.ConsumerWgcDiagnostic.cs`，阅读持久 WM_PAINT、创建/几何/DC、内存控制、两 capture 建立、标尺、consumer 提交与完整收尾相关逻辑。
- O = `D3DImageTests.OutputWindow.cs:30–94`，窗口样式、GPU completion 与普通 WndProc。
- csproj 全文、Package.targets 两种打包路径及版本钉住、AcceptanceResources.resx 全文。长返回截断处按相关段补读，不声称未显示的每一行均已认证。

## 3. 微软官方依据（本轮成功取得正文）

下列是 API/指南依据，不是对未载明行为的反向保证。Learn 顶部导航授权提示不影响这次已返回的正文。

|号|官方来源与实际陈述|
|---|---|
|M1|[CreateFreeThreaded](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.direct3d11captureframepool.createfreethreaded?view=winrt-26100)：移除 frame pool 对 DispatcherQueue 的依赖；FrameArrived 在内部 worker；17763+。|
|M2|[StartCapture](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.graphicscapturesession.startcapture?view=winrt-26100)：启动 session，允许捕获帧；没有首帧内容/ready/barrier 说明。|
|M3|[Screen capture 指南](https://learn.microsoft.com/en-us/windows/uwp/audio-video-camera/screen-capture)：可用事件，也明确可手动 TryGetNextFrame；Dispose 归还 buffer，归还后不可持有 Surface；surface 是创建/Recreate 的大小，内容大则裁剪，小则 ContentSize 外未定义；尺寸/device变化建议 Recreate，旧帧会丢弃。|
|M4|[SystemRelativeTime](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.direct3d11captureframe.systemrelativetime?view=winrt-26100)：compositor rendered the frame 时的 QPC 时间。|
|M5|[DwmFlush](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/nf-dwmapi-dwmflush)：等待 calling application 已排队 DirectX changes drawn to screen；不 flush 整个 session rendering batch；不返回 WGC token。|
|M6|[CreateForWindow](https://learn.microsoft.com/en-us/windows/win32/api/windows.graphics.capture.interop/nf-windows-graphics-capture-interop-igraphicscaptureiteminterop-createforwindow)：目标是给定 HWND，支持返回 GraphicsCaptureItem，18362+。|
|M7|[GraphicsCaptureItem](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.graphicscaptureitem?view=winrt-26100)：Agile/Both；Size、DisplayName、Closed。没有逐帧 HWND、paint generation 或 ready 属性。|
|M8|[GraphicsCaptureSession](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.graphicscapturesession?view=winrt-26100)：Agile/Both；IsSupported 是设备支持判断；Close 释放 session；没有内容成功证书。|
|M9|[Single-threaded apartments](https://learn.microsoft.com/en-us/windows/win32/com/single-threaded-apartments)：STA 必须有消息循环以处理跨 apartment 调用；同步阻塞有死锁风险。|
|M10|[UpdateWindow](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-updatewindow)：非空 update region 时直接调用 WndProc WM_PAINT，绕过应用队列。|
|M11|[CopyResource](https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-copyresource)、[Map](https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-map)：GPU 复制异步、资源约束明确；Map READ 获取子资源数据，DO_NOT_WAIT 忙时返回 WAS_STILL_DRAWING。|
|M12|[GetWindowDisplayAffinity](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowdisplayaffinity)、[SetWindowDisplayAffinity](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowdisplayaffinity)：有捕获保护设置；Get 有成功条件/失败及 GetLastError，查询失败不能冒充 WDA_NONE；EXCLUDEFROMCAPTURE 从2004支持。|
|M13|[DWMWINDOWATTRIBUTE](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute)：CLOAKED 是可查询状态（app/shell/inherited）；并非 capture-ready。26100新增 REDIRECTIONBITMAP_ALPHA 的说明仅是窗口重定向 alpha 设置，不是透明 WGC 帧错误码。|
|M14|[RequestAccessAsync](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.graphicscaptureaccess.requestaccessasync?view=winrt-26100)：请求需用户同意的捕获特性；Borderless 要 graphicsCaptureWithoutBorder capability。|
|M15|[DirtyRegionMode](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.graphicscapturesession.dirtyregionmode?view=winrt-26100)、[DirtyRegions](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.direct3d11captureframe.dirtyregions?view=winrt-26100)、[GraphicsCaptureDirtyRegionMode](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.graphicscapturedirtyregionmode?view=winrt-26100)：新 enum 的 ContractVersion=1245184（v19），ReportOnly=0/ReportAndRender=1；这次属性页没有提供足够默认值/首次帧初始化语义。不能补猜。|

官方示例另取得 [ScreenCaptureforHWND README](https://github.com/microsoft/Windows.UI.Composition-Win32-Samples/tree/master/cpp/ScreenCaptureforHWND)、[SimpleCapture.cpp](https://github.com/microsoft/Windows.UI.Composition-Win32-Samples/blob/master/cpp/ScreenCaptureforHWND/ScreenCaptureforHWND/SimpleCapture.cpp)、[main.cpp](https://github.com/microsoft/Windows.UI.Composition-Win32-Samples/blob/master/cpp/ScreenCaptureforHWND/ScreenCaptureforHWND/main.cpp) 正文。raw URL 两次404不作依据。示例用 Create + DispatcherQueue + STA GetMessage 循环，事件 TryGetNextFrame，CopyResource；检测 ContentSize 改变后释放帧/Recreate。main 先显示自己的捕获 UI，选择另一个 HWND 再开始捕获；**不能把“显示捕获 UI”冒充“被捕获 HWND 在首次 paint 后建立 session”的规定**。README 明示 minimized window 可枚举但不捕获。示例没有 paint→首帧 fence，也没有透明帧是正常 startup 或 access-denied 的约定。示例所用 Create 的 DispatcherQueue 要求不覆盖 M1 的 FreeThreaded 例外。

## 4. 分类裁决：确定问题 / 不保证 / 未决

### 4.1 确定的合同/证据使用问题（不等同已证 API 调用错）

1. **Drain 不是启动栅栏。** C:110–114 建池/StartCapture 后立即 Drain；C:140–150 遇到一次 TryGetNextFrame=null 即记录 drain complete。它只说明该次调用没有可取帧，没有等待首帧、ready 或 compositor 内容就绪。把 drain complete 用作“已越过 startup”属于确定证据过度解释（M2/M3）。方法本身作为清空当下可用队列没有错。
2. **source 检查不是逐帧身份负控实证。** C:193 把保存的 hwnd 与 geometry.Hwnd 交给 WgcFrameEligible；两者不是从 candidate frame 独立取出的身份。D:239 的 wrong source 仅验证内存 predicate。真实来源证据依靠 CreateForWindow 参数、pool/session 绑定及 HWND 生命周期（M6/M7），不能写成每一帧自报 HWND 已认证。未发现实际绑错 HWND 的证据。
3. **first eligible 内容断言过强，只可作为本诊断观察合同。** C:159–175/314–330 按时间选择首帧，D:278–288 保存后 WgcCheck(wgc)；它确实不按颜色筛帧，失败保留正确。但将该失败表述成“WGC 已保证包含红 paint 而 API/产品违约”不成立（M2–M5）。代码没有得到 paint generation/Present token。
4. **后 flush 时间门可能排除唯一有用帧。** D:313–316（consumer D:386–388 同理）在 flush 返回之后取 QPC；M4 是 compositor 渲染时间，不是 CPU dequeue 时间。若符合目标内容的帧在 flush 返回之前已渲染，就被 C:168–170 丢弃；静态窗口无承诺会再产生一帧。故“没有门后帧”不能推出没有交付。当前实际取得了门后透明帧，所以这不是该透明字节的充分原因。

**本轮没有发现足以确定造成透明首帧的新增 native API 错用。** 旧跨线程 DestroyWindow 错用已修复并有最新成功生命周期证据，不能重新列为当前未修根因。下列保证缺口也不自动变成实现缺陷。

### 4.2 API 不保证的推论

|代码位置|实际成立|不受保证的推论及依据|
|---|---|---|
|C:110–114、D:265–266|创建 pool/session 并发起捕获|StartCapture 返回或首次 null 后已完成 startup；首帧必须非透明/必须包含之后 paint（M1–3）。|
|C:29–30、168；D:313–316|Int128 正确换算 QPC/100ns，选 compositor 时间不早于门槛|这就是指定 generation 的 fence。时间相同域不意味着内容有同一提交序号（M4/M5）。|
|D:301–310、WM_PAINT:74–89|两次同代持久 paint 完成、EndPaint 返回，状态留存|compositor/WGC 已 latch 该代。UpdateWindow 同步 WndProc 不是捕获 fence（M10）。|
|C:161–171/314–330|轮询取帧、旧时间丢弃、首合格帧保留|缺 FrameArrived 就不能工作，或改事件会改变帧内容保证。M3 明确轮询合法；事件只改变通知，不新增内容关联。|
|C:91–104；D:113–131|最低平台、IsSupported、item.Size、几何/noncloaked 检查|每个目标都允许捕获、必有正确帧、非隐藏/非最小化的完整证明、物理显示正确（M6–8/M12–13）。|
|C:193–195|ContentSize32×16、capacity足够、format87/sample1/array1/mip1|Surface 的所有 capacity 字节都有定义；本代码只读32×16有效区域，不能把未定义padding作为透明根因（M3）。|

没有官方依据要求每次取帧前必须监听 FrameArrived，或必须把 STA 改 MTA。FreeThreaded 移除的是 frame pool DispatcherQueue 依赖，**不是 HWND owner 消息泵，也不是所有 WinRT/COM 投影调用的进展责任**。

### 4.3 未决项：不得凭猜测关闭

- **STA 同步等待：** C:35–39/144/163/319 有全线程 PeekMessage/DispatchMessage 泵，Map busy也泵；不是完全没有泵。D:385 completion.WaitOne 最长5秒期间未显式泵，D:313 DwmFlush 和同步记录/文件落盘也占用当前 owner。M9 支持存在进展/重入风险，M1 不保证消除全部风险；但没有调用栈、依赖链或实际阻塞证据证明它抑制了这次 WGC/compositor。尤其最新在 ruler 即停，D:385 **根本没执行**，不能拿这段 consumer wait 解释 ruler 全透明。UpdateWindow 两次直接完成 paint 也不证明 STA 所有队列已空。禁止据此重开 owner 线程实验。
- **创建/显示顺序：** D:94–104 与 O:30–46 用 `0x90000000`（WS_POPUP|WS_VISIBLE）、exstyle8（TOPMOST），先建可见窗，D:265–266 再开捕获，D:299以后才写红 generation。不是 hidden HWND 捕获后才 ShowWindow；不过首帧 paint 稳定/显示提交 ready 未认证。M2/M6 没要求唯一顺序，改为 paint-before-capture 是另一实验条件，不是已证修复。
- **尺寸/区域/dirty：** 固定几何每次比对，ContentSize变化即停，诊断不做 resize，所以未调用 Recreate 不是当前确定错用。M3/官方示例要求可变尺寸场景处理Recreate，不能据此给本固定32×16门加动态resize。M15是v19新能力，当前18362投影及代码没有设置 ReportAndRender；不能把未读取dirty rectangles说成当前“只获得脏区”的确定原因，也不能猜默认值。无须为了查它升级SDK。若将来使用增量模式，必须另闭合初始全帧/累积区域合同；它仍不给 paint token。
- **访问/支持：** IsSupported=true、CreateForWindow/StartCapture成功可证明各自调用成功，不证明目标内容可读。当前使用桌面 interop、默认边框，没有代码证据要求先额外调用 RequestAccessAsync/启动 picker；M14 的特定权限不是通用 ready barrier。保留HRESULT/策略缺口，不绕过限制。
- **item 属性：** Size已查，DisplayName/Closed未记录/订阅；DisplayName是标签，补上也不是内容栅栏。Closed可帮助分类关闭，但最新 IsWindow/NCDESTROY 证据只证明观察期间与最终销毁的相关状态，不给 compositor 内容保证。
- **display affinity/cloaking：** D:118–119已读DWMWA_CLOAKED并要求0，这是实际执行路径证据（日志没有单独序列化数值，不编造原始字段）。两 partial 没有调用Set/GetWindowDisplayAffinity；因此没有运行时 WDA_NONE 证据，也没有被设置保护的肯定证据。M12查询可能失败，不能把失败/未调用当0；只读证据有价值但不足以单独恢复内容判别力，不值得为它单开沙箱。不得擅自清除 affinity。
- **alpha：** 最新实际全alpha0且RGB不匹配是实测字节，不是被省略的帧。M1–8及示例没有定义“alpha0 = startup合法忽略”或“alpha0 = 不可捕获/拒绝访问”的协议。M13窗口redirection alpha默认忽略的说明也不等于WGC输出alpha约定。故既不能按透明剔帧，也不能据透明判 access-denied、cloaking、窗口根本没画或产品失败。透明原因仍未知。

## 5. Map 非均匀控制到底缩小了什么

C:267–296 创建同 capture D3D11 设备上的32×16非均匀BGRA/alpha纹理，复用 C:217–259 的 staging 创建、CopyResource、Map READ/DO_NOT_WAIT、逐行128字节、Unmap及D:267–274的保存/读回。owner-paint报告实际2048字节byte-exact通过。

这强烈反对“该次共用路径必然把任意输入清零、所有行地址/通道/alpha/保存都损坏”的充分解释；不是单一红色容易漏检的弱控制。它不经过 WGC item/session/帧生产、frame.Surface投影/IDirect3DDxgiInterfaceAccess.GetInterface（C:179–190），不能认证这些层返回了正确source；也不证明特定跨设备/驱动/捕获resource条件均正确。CopyResource是void异步（M11），负控成功不是给另一次WGC复制补一个独立错误检测结果。Map成功证明目标staging可读，不证明其中原始source是目标generation。

C:173–175/177–215在frame Dispose前完成Read，未发现“先归还帧后再读Surface”的确定错用。当前两边都为32×16/format87/sample1的记录进一步收窄，但不是WGC source正确性的证明。

## 6. 如何处置时间门；唯一可讨论的新抽样合同及收益

保留旧 first-eligible 失败、原始帧和元数据，名称降为“门后首个观察样本”，不得后验把全透明定成startup而删掉。时间门只作时间分类，不承诺内容身份；没有可检查的paint token就不能宣传严格同帧。旧observability报告的“同帧”此前已收窄，本轮进一步明确：稳定区间+顺序标签也是观察关联，不是API fence。

**若另日有人仍要研究WGC，唯一有新增判别力的合同是预注册固定窗口的全部样本序列，而非再改事件/线程。这里只评估，不推荐执行、不申请其执行预算：**

- 一个不重叠持久标尺，现有两次A/B更新；session、owner、format及样式不再换。每代从paint开始固定5秒，最多收集8个交付帧，达到8个或5秒即止，不因颜色/alpha/空队列延长；数目包括旧时间帧，不挑最后一个代替第一个。每帧保存原始有效crop、time/size/format/alpha，空轮询只计数；不新增baseline drain丢样本。保留先前首错单独字段。
- 按预定序号保存全部实际取出的帧，含旧、透明、错误；颜色只在采集结束后的报告中比较，不控制取帧/停止。预定义否定条件：任一来源/几何/格式不成立、API失败/关闭/生命周期不收束为观测无效；任一代8帧/5秒内无新时间样本为交付未证；已取样本不出现与两个代顺序相应的内容变化则该窗口内“可观测更新关联”未建立。出现对应样本也只支持这次有限关联，不回填旧首帧通过，不证明API保证或产品通过。
- 新增判别力只有一项：旧探针首错即停，不能区分“首次观察分离、随后有限窗口出现对应变化”和“整个有限窗口仍无对应变化”。固定序列可区分这两个**经验描述**，同时防止无限追绿。它不能唯一分辨startup/访问/投影/source/合成故障，也不能证明producer/consumer，仍可能受有限采样与观察者扰动影响。

**成本收益不足：不执行。** 该实验最多产出另一个标尺经验分布，不能提供API内容栅栏，且还需改采集保存合同、build/publish、完整身份和一次沙箱；即使标尺后来命中仍需真正consumer观测，失败又不唯一定位。WGC已耗4次build/4次publish/3次沙箱，两次真实均未进入consumer。继续会把完整替换任务变成捕获框架开发。事件化、延长超时、prepaint后StartCapture、补affinity再跑均不推荐，不能作为并列下一门。

## 7. 冻结与精确撤回范围（待授权，当前未改）

立即状态：保留默认关闭诊断源码与临时SDK配置冻结、全部 artifacts及历史报告，不再运行。**永久保留全局SDK变化的成本不为零**：csproj:3–4 的版本TFM/SupportedOSPlatformVersion对默认Release同样生效；Package.targets:32/69 的SDK DLL存在性检查也不是仅诊断配置生效。不能说“方法有#if所以默认项目完全没变”。

建议另授权按既有WGC引入差异做一次精准收口，不做仓库级回滚、不删证据、不重发生产：
1. 从正式编译源码撤出两份仅WGC partial（优先归档为非编译调查附件；保留报告定位与内容，不混删其他partial）。
2. csproj只恢复WGC之前 `net10.0-windows`、去掉本次SupportedOSPlatformVersion和ConsumerWgcDiagnostic属性组；保留其他原有字段、Native角色与路径。
3. Package.targets去掉PinConsumerWgcRuntimeFrameworkVersions和只为投影引入的依赖收集/去重/相对路径/存在性检查/清单变动，按引入前精确差异恢复两种clean包路径；不是把整个文件换成任一历史快照。其他原有打包/Contracts/身份约束不得删。
4. AcceptanceResources.resx仅撤出五个ConsumerWgc*键，保留全部其他资源。
5. 文档更新默认状态、差异清单、历史计数及回到主线的覆盖条目；旧包与原始失败完全保留。

实施前须取得可信的引入前内容或可审查精确diff来逐hunk核对，不能凭本报告重造旧targets；当前静态工具未取得该diff，不冒称已经具备可直接应用的撤回补丁。归档/撤回只负责减少诊断污染，不负责修复未定透明根因。正常default build只验证收口后可编译，不是内容验收。

## 8. 唯一下一门与如何利用已有合法consumer成功

**唯一待授权门：WGC诊断精确撤回 + 当前替换能力证据映射收口。** 返回完整方案 §8.2–3，不恢复旧窗口线程/GetPixel单点/native-only门，也不以环境先恢复为先决条件。

最小权限：仅上述5类诊断源码/项目/资源与相关文档；实现前静态核对引入前diff/基线；允许撤回完成后正常default ComAcceptance Release编译至多1（不以publish冒编译）。本门Native生产0、COM publish0、沙箱0、发现0、测试0；若diff无法取得或build失败立即停止记录，不转成捕获实验。当前本阶段未消耗该建议预算，也没有授权实施。

同一门的静态实质产物是把现有关键路径逐条映射到当前候选、架构、五维、实际目标与证据，而不只是“撤回完再想”：
- HD/HE/HF已有合法async/separate Present、92字节target/flags0xC/context/DPI、真实consumer window DC红蓝绿、事件/释放成功证据；x64 official AD1C9DBB…/candidate8F74588B…，x86 official5EBD18E0…/candidate75D136F7…，.NET10.0.13。引用它们证明该协议有真实consumer成功先例，避免再设计一次相同前置；它们不是screen/WGC/scanout证据，也不绑定当前20B3/40D0。
- 不调用仍带fresh-native前置的RunHbConsumer去要求“先证明环境恢复”；后续主线真正内容闭包须在两个正式项目的独立场景使用已认证协议，直接执行目标consumer、保留独立内容/生命周期断言，不能放松原测试、删除原失败或producer读回顶替consumer。是否迁移、何种实际内容目标，先在当前覆盖差异中明确，不在本门运行或偷改公共helper。
- KN/KO已证x86/x64各17/17的有限原地resize/lifetime修复保留：20B3F944…/40D09398…，实际尺寸/device身份、callback/引用/cleanup边界如原报告；ConsumerPixelsObserved/DesktopCaptureExecuted=false。不能因为WGC没校准就撤销这项有限修复，也不能把它升级为shader像素、device-loss、完整恢复或full通过。
- 历史full/旧SHA成功失败仍是历史资产；当前候选最终双架构Release完整适用矩阵、真实WPF/持续媒体/恢复等仍未完成。按§8.2准备当前基线、§8.3逐项重裁HW覆盖，再选产品差异最小切片；不要求重新认证捕获环境作为所有路径的总前置。

本门结束时只交“诊断退场/默认编译状态/当前候选证据差异与主线覆盖定位”，不宣称完整替换完成、不自动开始x64/full/candidate运行。WGC透明source原因保留未决即可，不需要先解出它才能继续其它有独立内容能力的替换工作。
