# shader / product-loss r4 — StaticDiagnosticCompleted / Overall Open

## 本轮实际推进与预算

完成具体调用合同诊断与产品loss API边界核查；没有确定可修因果，不盲改或为满预算执行。预注册：`WpfGfxShape/artifacts/shader-loss-contract-r4-before-after.md`，先记录判别再产生本结论。新增 normalCOMbuild **0/2**、COMpublish **0/2**、sandbox **0/4**、Native **0**；无新task/运行参数/exit，不将无执行写成exit0。终端、脚本、委派0。历史real-loss累计build1/publish2/sandbox2单列，不挪成当前额度。生产/测试/SDK/TFM/expected/阈值/skip/并行均未改，诊断仅文档，无待回撤运行开关。

## officialshader 静态进展（已落实正式报告）

直接读取当前 `D3DImageTests.VisualCache.cs:38–165,250–385`、`JhPresent.cs:7–29`、`HardwareClosure.cs:13–21`、`CaptureHistoricalControls.cs:18–40`、`OutputWindow.cs:29–48,73–85`、`VisualCache.cs:588–645`。本地官方源码用于合同核对，未认证等同servicing commit。

|字段/时序|candidate resize|official resize|已过隔离official sampler|
|---|---|---|---|
|入口参数|cache=true/green/FF80/geometry=false/resize=true|相同，official=true|cache=false/green/FF80/geometry=false/resize=false，独立角色|
|连接|connect1 same-thread|initialize0/connect0 async|同official async|
|producer|系统D3D9，adapter0/HAL/flags26，32×16 windowed|相同|相同helper|
|source|两12×1 format21 lockable surface，ColorFill红/绿后create96DPI/version7/fallback0|相同|相同|
|image更新|0A，interop UInt64，software=0，先AddRef，send separate0|相同|相同|
|dirty与Present|dirty12×1后0B/event0/separate0，随setup close/commit|dirty同；setup close/commit/SyncFlush后逐image独批0B|同official|
|official完成|不适用|DuplicateHandle→UInt64 event，send separate1/commit/WaitOne≤5s/SyncFlush|同helper|
|sampler1资源|83 BitmapCacheBrush，94 BitmapCache scale1，0x84 mode@28/source@32|相同|82 VisualBrush，0x83 148bytes viewport/viewbox/mapping/Stretch/source@144|
|shader|33/HardwareOnly2/304bytes/CompileSoftwareShader0|相同|相同|
|target/root|46/92bytes；contentRoot39+matrix66，60byte DPI矩阵，bind→child→SetRoot|同；额外official flags12、实际DPI context/scales|同official|
|初始尺寸|窗口32×16；target32×16；settings实际bounds|相同|相同|
|读像素前|SameThreadPresent(connection)/DwmFlush|SyncFlush(channel)/DwmFlush|同official|

**新确证的比较边界**：已过隔离official sampler不是失败BitmapCacheBrush场景的同合同对照，expected相同不等于消费路径相同。candidate resize才是相同cache合同但不同实现/连接模式。对于相同cache参数的official helper，resizeWindow在初始读像素前只增加模块身份日志及条件编译观测（正式full未开启这些诊断）；不会改变surface填充/dirty/sourcebitmap/event/root/target flags/尺寸/窗口状态。真实resize在两初帧断言后才SetWindowPos。不能把后续bool分支解释成初帧尺寸变化。既有独立official resize通过与共享full失败仍只能称同合同、不同运行上下文，不能认定进程隔离因果。

## 完成合同：已确证的不等价，不冒根因

- 官方 `PresentationCore/.../InterOp/D3DImage.cs:657–735`：UpdateResource必须先于Present；独批发送为进入当前commit，防止等事件死锁。现有fixture已先setup flush再独批Present，未发现应把separate1改0的依据。
- 官方 `core/resources/d3dimage.cpp:180–289`：ProcessUpdate接管传输引用并记录interop/software source；ProcessPresent调用interop Present/GetSoftwareBitmapSource/NotifyOnChanged，**失败也SetEvent并CloseHandle**。事件只说明命令清理已到，不是目标绘制成功或PresentHR=0。
- `core/uce/clientchannel.cpp:800–840`及`connection.cpp:298–350`：SyncFlush发控制命令、CloseBatch/Commit并等待channel flush；`composition.cpp:1331–1361`加入完成列表。`composition.cpp:2063–2105`明确FlushChannels从Present分离，occluded时可不Present仍发flush。`composition.cpp:747–758`明确NoPresent通知。因此SyncFlush0+DwmFlush0不能推出这个HWND已输出shader帧。
- 工作区全文搜索未找到名为FlushConsumerChannel的实现；本轮不杜撰该函数的额外完成语义，以实际flush(channel)调用链为准。

这确证了**证据缺口**，不是确证“缺栅栏导致actual0”。不能添Sleep/等色/重复flush当修复，也不把foreground或共享进程当默认根因。

## 原失败现场直接复核与最小后继信号

已直接回读两arch post-fixture `TestResults/child.trx:2309–2339`（x86截断尾已另读2335–2340补齐；不称整个TRX全文本轮回读）。x64 target=0x26C02A0/PID9772/ownerThread6108，x86 target=0x23804B8/PID9664/ownerThread11448；actual official AD1C/5EBD，testC3D4/B04E，.NET10.0.13。native前置00563412实测通过，CreatePixelShader0，初始shader actual0。actual swapchain/native cooperative0仅producer/native control，不是official消费device/target。无resize记录。

原件不含失败瞬间official消费target的GetDevice/GetDesc、producer source IUnknown连接身份、interop available实值、HWND实际PresentHR；不可将源码请求值写成已测现场。最小下一信号必须在**同一个失败HWND及同一channel**绑定这组身份与Present/NoPresent/BadPixelShader通知，而不是再采全进程state。ABI已有PeekNextMessage/通知入口可用于后续最小只读测试诊断，但本轮未实现或执行，也不声称它能给出内部device指针。没有新的确定修正，所以没有调用official重跑，更没有越过official跑candidate/x86。

## 产品真实loss：确切API能力边界（本轮已核）

核对官方 `core/dll/wpfgfx.def:55–135`及候选Abi exports：WgxConnection_Create只给opaque连接，MilResource_CreateOrAddRefOnChannel只给uint资源句柄；Hwnd attach/detach不是消费device getter。InteropDeviceBitmap_Create接收外部producer surface，Detach/AddDirty/GetAsSoftwareBitmap不返回HWND消费device/swapchain或其Reset/loss控制。没有公开导出将HWND target handle解成IDirect3DDevice9、强制其全屏独占、或真实置lost的接口。bitmap/offscreen getter不是HWND消费target getter。

候选内部证据：`Direct3D9DeviceManager.cs:919–994`实际unusable→NotifyDeviceLost→adapter false→interop listener；`InteropDeviceBitmapExports.cs:94–135`按adapter应用状态并发Available回调；恢复创建NotifyAdapterStatus(true)见manager:507。静态链存在不等于现场覆盖。`Direct3D9Device.cs:1529–1571`消费device优先Ex CheckDeviceState，occluded映为0，lost/hung/removed置unusable；非Ex分支返回NotImplemented，不能套系统传统D3D9全屏lost→Reset测试作消费链证明。`SurfaceRenderTarget.cs:4049–4068`GetDevice/GetDesc观测在生产侧KI_RESIZE_TRACE条件内，本轮Native0/生产不改，不擅自启用重建。

因此现有可控系统exclusive producer失焦不能保证同adapter候选windowed/Ex消费device真实lost；强行内部NotifyInteropAdapterStatus、伪HR、反射注入均是模拟，不采用。当前限制下能验证的最小既有目标是**系统D3D9控制lost/reset/新内容**，已有通过无需重跑；不能升级为产品恢复。本轮无法实施满足“实际消费device真实lost+available通知+重建后新内容”的确定测试，边界是缺少该消费设备的公开访问/真实可控触发，非预算不足或缺用户授权。没有证据保证独占失焦使该后台设备lost，故不发布一个注定可能只测producer的替代夹具。

## 保留终态与本轮闭环

system real-loss x64=ClosedLimited；candidate产品loss=NotCovered；defaultPoolReferences=0仍是固定日志不是枚举，reset后新内容仅Passed强断言支持，独立原始像素未捕获。历史full两arch各1823执行/1821过/2败，全部历史红保留，定向绿不抵销。HWtarget身份、产品loss、audio、naturalresources、finalfull与完整替换均Open，memoryGreen=null。

本轮关闭项：调用参数/BitmapCacheBrush与VisualBrush差异诊断、resize前bool无尺寸副作用核查、事件与SyncFlush输出不等价合同、公开ABI不能控制候选消费device真实loss的边界，及正式入口更新。不是产品修复完成。下一直接工作为原cache=true official场景的同HWND/channel通知与consumer身份可观测性最小诊断；须先解决如何取得真实消费身份，不改颜色阈值或盲跑独立producer。回读确认记在本轮提交结果；被截断的历史长文不声称全文已回读。
