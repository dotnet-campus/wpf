# WGC task55fb7bcd：诊断设施失败有界静态根因审查

## 1. 裁决、权限与证据边界

本阶段仅静态审查和新增本报告；生产/测试源码、项目、包、原始证据修改均0；build/publish/沙箱/发现/测试/脚本/终端/委派均0。用户本次授权覆盖旧文档的静态审查待授权限制，不授权修码或执行。

**已确定两项HWND清理失败的实现根因：WGC当前STA创建的窗口被既有DestroyWindow包装器转交另一线程销毁。** 原件没有实际GetLastError，不能补写具体Win32数值。

**透明首帧的来源仍未唯一确定；已确定标尺不是WM_PAINT持久绘制，时间门只证明compositor帧时间晚于flush门，不能证明该帧包含这次GDI绘制。** ABI审查未发现能够直接解释透明帧的签名/slot/布局/行距错误。不能因此反向认证WGC内容正确，也不能判产品失败或笼统归环境。

本次直接读取：最新next-session-handoff顶部、official-readback完整报告、revision-validation关键历史及修订段、draft-audit、current-execution-rules/current-work-item/current-stable-baseline、章程实现合同；两WGC partial、依赖的OutputWindow/WindowThread、D3DImageTests相关P/Invoke、DesktopDuplication结构与设备声明、项目诊断开关。适用AGENTS/instructions文件名检索未找到文件。没有展开无关生产代码或旧线程/native-only探针。

原件根R：`WpfGfxShape/artifacts/consumer-wgc-kp20b3-bound/official-readback/`。亲读consumer-wgc/evidence.jsonl全部26行、supervision.json全部40行、child.trx结果/异常段。`ruler-0.bgra`原件存在且未改；图片加载失败，文本读取仅返回NUL。**该文本结果不是二进制长度/hash/逐像素解码认证。** 已有JSON独立表达中心RGB=0、全部alpha最小/最大=0及RGB全图不匹配；不能把它进一步写成已独立统计全图RGB全0。保存函数静态预期2048字节，不冒充实际文件长度认证。

真实历史保持：Exit2、TRX执行1失败1、无超时；独立标尺1/2，consumer真正创建提交/标记0/2（not observed），第二标尺未运行。官方实际路径/hash/version、候选20B3身份及受信资产/PE/PresentationCore缺口沿official-readback，不重复冒认证。

## 2. 确定缺陷：HWND销毁派发错误，不是笼统线程路线重开

代码位置均相对`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/`：

- `ConsumerWgcDiagnostic.cs:169–172`：`owner`是`WgcThread`，Invoke同步执行`CreateOwnedOutputWindow`；`captureOwner=owner`。实际geometry两个窗口owner Tid均7708。
- `ConsumerWgcCapture.cs:42–64`：WgcThread记录当前managed thread并校验同步Invoke；Dispose只RoUninitialize，**不创建线程，也不join线程**。
- `ConsumerWgcDiagnostic.cs:337–338`：调用的是`DestroyWindow`而非直接P/Invoke。
- `D3DImageTests.cs:161–164`：真正P/Invoke名为`DestroyOwnedWindow`；`DestroyWindow`包装器无条件调用`windowThread.Invoke`。
- `WindowThread.cs:8–15,40–45,69–85`：ClassInitialize建立另一专用STA；Invoke会把action排入该线程。因此即使337–338日志的Tid=7708，真正user32调用也不在7708。TRX StdOut也确认ClassInitialize专用owner启动。

官方DestroyWindow要求不能销毁另一线程创建的窗口；非零成功、零失败。当前流程精确违反这个条件。原件在WgcCheck处报错而非wrapper空值异常，与实际native返回0一致，但具体last error没有留存；不推定为5/1400等值。

`DestroyOwnedWindow`声明没有SetLastError=true，WGC也没有立即读取错误。后来在日志/异常处理或owner线程之外读取错误均无法修复历史证据。未来需在真正调用线程、native调用返回后第一时间缓存错误，再做其他P/Invoke/序列化；只在返回0时解释该值。

本次两个capture close Errors0发生于销毁前（evidence18–21）；关闭capture不按公开合同销毁目标HWND。代码没有close后IsWindow/owner/WM_NCDESTROY实证，故“当时仍活”和“最终泄漏归零”都未测。但不能将capture关闭当成已毁窗理由去忽略DestroyWindow失败。

异常路径：本方法每个非零HWND只尝试一次destroy，没有本方法重复销毁。两个窗口为无parent/owner的顶层popup，非互相拥有，不存在这里的父销毁自动带走另一个。共享WindowProc不PostQuitMessage；WgcThread.Dispose不发WM_QUIT。ClassCleanup的WM_QUIT只针对历史专用线程、且在测试体之后，不能解释本次先前两个销毁返回0。`HWND owner join`日志名称错误：实际仅当前STA RoUninitialize，不能当线程join或窗口清理证明。

当前WGC没有IsWindow断言，更没有将其返回值当“销毁成功”；未来IsWindow仅辅助状态快照，不能代替DestroyWindow返回值/owner/NCDESTROY证据（HWND可复用）。Class注册失败允许1410是ERROR_CLASS_ALREADY_EXISTS，合理，不是销毁错误；两处destroy的`!=0`符合BOOL语义，不存在反向常量判断。

## 3. Surface → Texture → CPU：逐项ABI结论

下称C=`D3DImageTests.ConsumerWgcCapture.cs`，D=`D3DImageTests.ConsumerWgcDiagnostic.cs`。直接读取本机官方Windows SDK **10.0.26100.0**头文件；这用于公开ABI核对，不冒本次实际构建使用了该SDK头，也不替代运行时验证。远程raw头抓取404没有作为证据；最终用本地SDK原文。

|位置|核查结果与公开定义|
|---|---|
|C:97–103|IGraphicsCaptureItemInterop IID3628E81B…，IUnknown后三槽，CreateForWindow为slot3；参数this/HWND/REFIID/void**，HRESULT，Stdcall正确。SDK `um/windows.graphics.capture.interop.h:15–27`直接确认。绑定的是传入ruler/consumer HWND，不按标题搜索。|
|C:105–109；DesktopDuplication.cs:9–10|D3D11CreateDevice输入adapter0、HARDWARE=1、BGRA_SUPPORT=0x20、默认levels、SDK_VERSION7；native out指针及HRESULT形式正确。IDXGIDevice IID54EC77FA…及CreateDirect3D11DeviceFromDXGIDevice包装路径符合公开接口；SDK interop.h:15–17为IDXGIDevice*→IInspectable**。不是D3D9 surface混用。|
|C:180–190|frame.Surface投影创建自有marshaler并QI IDirect3DDxgiInterfaceAccess。SDK `um/windows.graphics.directx.direct3d11.interop.h:28–32`确认IID A9B3D012…、继承IUnknown、唯一GetInterface为slot3；REFIID+void**/HRESULT/Stdcall正确。请求ID3D11Texture2D IID6F15AAF2…与d3d11.h:2879/15171一致。|
|C:191–195|ID3D11Texture2D.GetDesc slot10正确：IUnknown0–2、DeviceChild3–6、Resource7–9、GetDesc10，void返回。SDK d3d11.h:2892–2968可顺序核数。|
|DesktopDuplication.cs:12–23|DESC是11个32位字段：Width0 Height4 Mip8 Array12 Format16 SampleCount20 Quality24 Usage28 Bind32 Cpu36 Misc40，总44字节；SampleDesc拆成两个UINT布局等价。MAPPED为pointer/UINT/UINT：x86偏移0/4/8大小12，x64偏移0/8/12大小16。Sequential无自定义Pack破坏布局。官方两个结构页直接确认。|
|C:198–200|保留源纹理完整尺寸/mip/array/format/sample；Usage3(STAGING)、Bind0、CPU_READ0x20000、Misc0正确。CreateTexture2D为device slot5、HRESULT，pInitialData=null正确；SDK d3d11.h:14448–14495直接核数。CopyResource context slot47，void(this,dst,src)，staging在前texture在后正确。|
|C:206–218|Map slot14、Unmap15正确。Map(this,resource,subresource0,READ1,DO_NOT_WAIT0x100000,mapped*)返回HRESULT。SDK d3d11.h:8573–8704按声明序核数；:1222/1244/1275/1285确认枚举。0x887A000A为WAS_STILL_DRAWING，重试同一staging不取后续帧；失败Hr抛出，成功才读；finally Unmap一次。|
|C:212–216|非零Data及RowPitch>=128；逐行按y*RowPitch读128字节写紧密BGRA32×16；源从row0/col0，未误把RowPitch当像素数，未拷padding。checked偏移失败会抛而非绕回。此案RowPitch128，没有裁剪到padding。|
|D:74–80,183–190|BGRA8=DXGI87；像素比较B/G/R分别取ARGB的低/中/高字节，COLORREF转换正确。中心offset=(8*32+16)*4=1088，alpha byte+3；文件直接WriteAllBytes紧密数组，在内容assert和DC读取前落盘。原始保存没有文本转换/alpha归一化/颠倒通道。|

context slots按SDK声明序核数：0–2 IUnknown，3–6 DeviceChild，7 VSSetConstantBuffers，8 PSSetShaderResources，9 PSSetShader，10 PSSetSamplers，11 VSSetShader，12 DrawIndexed，13 Draw，14 Map，15 Unmap；16 PSSetConstantBuffers，17 IASetInputLayout，18 IASetVertexBuffers，19 IASetIndexBuffer，20 DrawIndexedInstanced，21 DrawInstanced，22 GSSetConstantBuffers，23 GSSetShader，24 IASetPrimitiveTopology，25 VSSetShaderResources，26 VSSetSamplers，27 Begin，28 End，29 GetData，30 SetPredication，31 GSSetShaderResources，32 GSSetSamplers，33 OMSetRenderTargets，34 OMSetRenderTargetsAndUnorderedAccessViews，35 OMSetBlendState，36 OMSetDepthStencilState，37 SOSetTargets，38 DrawAuto，39 DrawIndexedInstancedIndirect，40 DrawInstancedIndirect，41 Dispatch，42 DispatchIndirect，43 RSSetState，44 RSSetViewports，45 RSSetScissorRects，46 CopySubresourceRegion，47 CopyResource。不是按文档方法页的字母排序猜slot。

CopyResource是异步GPU操作，但后续成功的READ Map建立CPU读取边界，DO_NOT_WAIT未完成则返回WAS_STILL_DRAWING；不能简单把“无显式Flush”判成读到未完成数据。此实现若命令提交不前进可能超时，但原件成功Map且落盘；不支持直接据此归因透明。仍未知实际驱动执行/内容本身及设备移除状态，CopyResource void也未采集debug layer消息。不能把静态签名正确冒实际内容已被独立认证。

alpha方面：WgcMatches明确只比较RGB，alpha仅记录；它可能允许正确RGB/错误alpha通过，属于透明诊断信息与RGB合同分离，不是本次失败根因（红RGB已经不符）。格式UNORM并不单独定义窗口捕获alpha应如何解释。不能根据alpha0自动断言黑色、被保护、没有重定向或产品透明；也不能据此给所有正确帧追加未经定义的alpha255产品门。

## 4. 几何、时间、WM_PAINT：确定的观测缺口与可证伪假设

C:65–72,159–175,254–270：selector先按ContentSize32×16及SystemRelativeTime门选第一个；几何/format等随后检查，不按颜色选帧。错误geometry直接失败，不找下一张；仅旧timestamp被释放。本次必须保持这个首帧/no-retry合同，**没有证据要求改变首帧合同**。

`WgcCurrent`用Int128比较 Time100ns*QpcFrequency 与 ThresholdQpc*10^7，避免整数除法及溢出；TimeSpan.Ticks就是100ns单位。官方SystemRelativeTime描述为compositor rendered frame的QPC时间，不是GDI命令ID、WM_PAINT完成回执或内容版本。此案Frequency=10^7，frame38453782031250比threshold38453782021278晚9972ticks，约0.9972ms，换算正确；由此推断“包含本次红绘制”不成立。

D:30–49检查client/window/extended bounds一致、32×16、未cloaked、源PID/class/DPI/origin；实际无边框popup/96DPI，crop(0,0)合理。C:167、193–195锁ContentSize、capacity、format87、sample1、mip1/array1。可以排除本案明显屏幕坐标当纹理offset、标题栏offset和capacity小于内容等错误。source==expected实际只是同一capture保存的HWND比较，非对返回帧独立反查来源；绑定可信度来自实际CreateForWindow调用，不夸大该内存负控。

**标尺实现缺口**：D:201–219用GetDC/FillRect/GdiFlush/ReleaseDC，再DwmFlush；窗口却复用了`OutputWindow.cs:31–45,87–92`专供consumer的不绘制类：背景brush0，WM_ERASEBKGND只return1，WM_PAINT仅ValidateRect。标尺没有保存颜色/绘制代数，没有InvalidateRect→BeginPaint/EndPaint绘制路径，也没有paint完成时间。因此代码不能证明后续重绘时窗口仍重画红色。GetDC画图本身合法，不能说Windows禁止；但它不提供这项持久绘制/提交关联合同。

GdiFlush只flush调用线程GDI batch。DwmFlush等待调用应用排队DirectX变更进入绘制，不flush整个会话，更不是等待特定HWND被WGC采到。现有二者返回成功不能补成WGC对应paint的保证。Drain在首次null即返回（两次MaximumTime=-1），不等于捕获warm-up已完成；但也无证据应固定睡眠或丢弃首帧追绿。

**H1，可证伪**：标尺未建立持久WM_PAINT内容/代数，捕获startup/compositor窗口内容尚未与GetDC红绘制关联，产生时间门之后的透明帧，而后续DC红。证据允许该解释，但不能单凭本次确认。若诊断专用持久paint、owner及读回链负控均正确，仍在同一首帧合同下出现同样首帧/DC分离，则“仅由旧GetDC绘制实现导致”假设被否定；保留边界分离，不追加帧。

**H2，可证伪**：CPU读回链实际执行有未被静态检查发现的错误。用同设备32×16非均匀已知BGRA/alpha图，经同一staging CopyResource/Map/逐行保存路径做一次独立内存纹理回读负控；若byte-exact通过则缩小到WGC输入/内容关联，若失败优先判设施读回错误。此负控不能算WGC真实帧、native-only Present或产品证明。

## 5. STA/WinRT/framepool/生命周期

C:42–64在STATestMethod当前STA RoInitialize(0)，成功包括S_FALSE均由一个RoUninitialize平衡，异常未返回owner时不错误uninitialize。C:110–114为CreateFreeThreaded→CreateCaptureSession→StartCapture→Drain，随后TryGetNextFrame；CreateFreeThreaded公开合同移除DispatcherQueue依赖，FrameArrived在内部worker触发，不要求订阅后才能轮询。没有Create而非CreateFreeThreaded的DispatcherQueue混淆、先TryGetNextFrame后StartCapture、跨线程迁移frame或回调竞态证据。两个HWND与capture同步当前STA；不建议新增工作线程。

C:173–175在Read与保存托管数组后frame.Dispose；Read先Unmap、释放staging/texture/access、自有marshaler，然后frame归还；session/pool/device在finally关闭，item没有Close。公开Dispose成功不等于完整内部缓存/资源收敛已测；既有revision-validation披露继续保留。异常保护逐项尽力清理，不见此案双重frame dispose或在读完前归还frame。

## 6. 唯一推荐修正包与验证门（待另授权，本阶段不实施）

只推荐一个“诊断观测实现修正包”，不改默认业务、不重开架构/线程路线，不改首帧合同：

|最小位置|未来修改内容|
|---|---|
|仅ConsumerWgcDiagnostic.cs，两处销毁及诊断专用P/Invoke|使用本partial内SetLastError=true的直接user32 DestroyWindow声明，在当前WgcThread owner同步调用，禁止既有windowThread包装器；立即捕获返回/error/调用Tid/ownerTid，记录关闭capture前后和销毁前后HWND状态及NCDESTROY；一次销毁，成功置0，失败保留原异常。将owner join记录更名为WinRT owner release。|
|仅ConsumerWgcDiagnostic.cs，独立标尺窗口/绘制|在该诊断partial内建立专用ruler类/WindowProc及状态，保存color和generation，InvalidateRect/UpdateWindow触发正常BeginPaint/FillRect/EndPaint路径；记录paint开始/完成generation/QPC、flush时间和采帧门；callback内异常不得越过unmanaged边界。consumer仍用原类，不给consumer加GDI。保持尺寸/位置/两色顺序、首合格帧no-retry。|
|仅ConsumerWgcCapture.cs，读回的诊断复用及负控|如需提取最小私有texture→staging读回helper，仅用于让已知纹理非均匀BGRA/alpha图和真实frame共用完全相同路径；不新增公共API/依赖/项目/Host。记录实际保存长度/hash与可独立解释元数据、原始首失败帧。不要做颜色筛帧/自动alpha修正。|

先对上面最小修改重新静态审查，再请求**正常诊断build至多1、COM publish至多1、official-only监督沙箱至多1（精确真实体1；独立发现0、Native发布0、candidate/native-only运行0）**。任何前置失败即停，不挪预算修后重跑；本阶段这些预算全部0且未使用。维持显式KP20B3输入、新独立输出、官方实际身份门及历史可信来源缺口；监督120秒、外180秒，无追加startup试跑/第二样本/发现。负控应在同一体内固定次数执行，不另起探针。

预定义分类（分轴记录，不能互相覆盖）：

1. `readback-control-failed`：非均匀已知纹理未byte-exact或保存长度/hash异常，设施读回失败，禁止解释WGC颜色，停止。
2. `paint-contract-failed`：paint未完成/代数不对/geometry或owner变化/API失败，标尺设施失败，停止。
3. `capture-api-or-timeout-failed`：绑定/Map/取帧错误或deadline；独立保留HRESULT，不能改称像素不符。
4. `first-eligible-content-mismatch`：负控与paint合同已过，首帧不符；立刻保存原始帧、全部元数据及DC观察，停止，不补第二帧。结论仅“当前首帧边界不对应”，不是产品失败。
5. `ruler-two-markers-correspond`：两个标尺各自首合格帧和两DC对应，才允许原有official consumer阶段；不等于consumer已通过。
6. `consumer-boundaries-separated` / `consumer-two-markers-correspond`：只据实际consumer两次提交和各自首帧/DC记录分类，保留不同观察时刻，无SamePhysicalScanFrame断言；官方结果不是candidate结论。
7. 独立`cleanup-failed`：任何销毁/关闭失败，无论内容轴怎样均失败，绝不因进程退出或“join”改绿。

固定负控：保留已有首个错误eligible后面即使有正确值也必须返回首错的内存selector测试；旧时间、wrong source/format/crop/size拒绝；非均匀图检查通道/行pitch/alpha不被归一化；持久paint同generation重绘记录；每HWND一次实际owner销毁及NCDESTROY证据。禁止为证明Win32跨线程错误再开线程负探针，当前静态证据足够。

所有未来改动应只在默认关闭的两WGC partial内；不要修改共享OutputWindow/WindowThread/DestroyWindow helper来迎合诊断，避免影响原业务。此阶段实际没有源码改变，默认业务行为不变，SDK诊断专属配置临时保留冻结。

## 7. 官方依据与账目

本地官方SDK根：`C:/Program Files (x86)/Windows Kits/10/Include/10.0.26100.0/`，直接读取上述interop.h与d3d11.h声明。官方公开文档亲读：

- https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-destroywindow
- https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-map
- https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-copyresource
- https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ns-d3d11-d3d11_texture2d_desc
- https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ns-d3d11-d3d11_mapped_subresource
- https://learn.microsoft.com/en-us/windows/win32/api/windows.graphics.directx.direct3d11.interop/ns-windows-graphics-directx-direct3d11-interop-idirect3ddxgiinterfaceaccess
- https://learn.microsoft.com/en-us/windows/win32/api/windows.graphics.capture.interop/nn-windows-graphics-capture-interop-igraphicscaptureiteminterop
- https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.direct3d11captureframe.systemrelativetime?view=winrt-26100
- https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.direct3d11captureframepool.createfreethreaded?view=winrt-26100
- https://learn.microsoft.com/en-us/windows/win32/api/roapi/nf-roapi-roinitialize
- https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-gdiflush
- https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/nf-dwmapi-dwmflush
- https://learn.microsoft.com/en-us/windows/win32/gdi/wm-paint

WGC累计不变：build3（1败2成）、COM publish3（1败2成）、沙箱2（发现1+真实1）、单独发现1、真实体1失败、本链Native发布0。KS不变：build1、COM publish1、沙箱4、精确请求3、发现1、真正体1、A1/B1/native提交2。历史失败、C427输入漂移、SDK恢复记录及原始首失败帧全部保留。整体替换未完成。
