# Direct consumer：真实资源图、通知与批次路由静态审计

## 1. 范围、结论与证据等级

本阶段生产/测试源码及配置修改0、build0、publish0、沙箱0、测试0、终端/脚本/再委派0；仅新增本报告及更新恢复文档。完整替换未完成。

**没有发现确定的 command ID、资源类型、packed 布局、资源依赖、dirty rect、AddRef、独批 Present 或 target enable 协议错误；不应无证据修改生产候选。** 已沿实际资源图追到官方 DrawImage、GetBitmapSource、Render 和 Present，确认不是缺少手写的 image→renderdata 通知订阅。真正未观测的断点是 bitmap front buffer/bitmap source、target 内部实际 backend/可绘制状态以及该次 target Present 的内容。不能从返回码倒推这些内部状态。

新增的实质依据不是泛称“通道不等于像素”：官方 `CInteropDeviceBitmap::Present` 会在 disabled/null surface 时成功空操作，吞掉 D3D failure；`CMilD3DImageDuce::GetBitmapSource` 可成功返回 NULL，随后 `CDrawingContext::DrawImage` 成功 no-op；HWND target 本身也有成功跳过 Render/Present 的分支。**这几条是参考源码确证的合法控制流，不是本次实际命中证明，更不等于笼统归环境。** 本次 callback 只有两个 Available1/Version7，反而不支持宣称已观察到 unavailable/loss。

证据分层：
- R：亲读 fixed-window 当前回收原件 `artifacts/direct-consumer-fixed-window/readback/official/Results/direct-fixed-official/` 的 `direct-consumer.jsonl:1–57`，`TestResults/child.trx:7–22`，`supervision.json:25–40`。JSON全部57行已分段核读，TRX/监督本轮只核关键段，不冒全文重审。
- S：当前测试与候选源码，不能冒称包内反编译结果。包未保存逐命令原始字节/完整数值句柄图；JSON只记录资源创建阶段及清理句柄。图中 image1/data2/visual3/destination4 是结合S创建顺序和R删除4→1的对应解释，不是独立字节追踪。
- A：仓库 `src/Microsoft.DotNet.Wpf/src/` 官方参考源码，不冒充与实际 servicing 二进制逐行绑定的源码。以下官方路径相对此根；测试路径相对 `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/`。
- H：历史报告。HD/HE/HF原始成功包、其编译源码及每次采样原件当前不可得；继承 historical-contract-audit 的缺口，不以现HB源码冒历史、不用旧candidate SHA通过认证20B3。

已读最新恢复入口、fixed-window、historical-contract-audit、kp20b3-r2（首次长输出截断部分已补关键尾段）、direct原定义、完整替换方案1–120、current-execution-rules全文、当前事项与基线最新节、章程实现合同主要节、仓库贡献规则。未找到AGENTS。SDK10.0.107、net10.0-windows/Nullable enable、ConsumerDirectDiagnostic 的两框架10.0.13钉住只核未改。

## 2. 当前原件绑定（不是新运行）

R task=`3d53aad485524c258da5402289f91e79`，UID=`12af8f6c-d70b-8ecd-87a5-0f07906181a9`，真实Failed/Exit2，监督无timeout/kill/drain错误。
- actual official Requested==Actual为该任务根 `wpfgfx_cor3.dll`，SHA256=`5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`，10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。
- .NET10.0.13/X86/PointerSize4；test SHA=`A56E6981D0289B0AE19755C0275A168EC6EA863BEA4F1F32B37BA130A0769C05`、MVID=`4dd062be-2383-4722-9e1a-b0f539884b74`；PresentationCore SHA=`D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D`。
- candidate=`20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D`仅文件认证，candidate运行0。
- 一次红，两DC均1973532（001E1D1C）而非255；间隔5.2690026s，TimingInvalid=true。HR0/event=true，target4/HWND23528798/owner3860；Visible1/Iconic0/Foreground与Cover均同HWND。四refs实际0、owner线程Destroy1、Errors=[]。这些事实不被本静态结论覆盖。

## 3. 逐节点合同表

|节点/真实关系|当前S发送/创建；R证明|官方A消费位置与内容/所有权传播|确定性与unknown|
|---|---|---|---|
|producer device→surface|DirectConsumer.cs128–135：D3D9 SDK32/HAL1/behavior0x26；32×16、format21 A8R8G8B8/MS0/quality0/lockable1，无shared输出；182–184 ColorFill FFFF0000→CompleteNativePresent→dirty。R2–4/25–26 HR0|WpfGfx/core/hw/InteropDeviceBitmap.cpp251–290：QI IDirect3DSurface9/GetDesc/格式及RT usage验证；333–385按设备能力选择method、构造bitmap并注册adapter listener；构造取surface引用，释放临时QI引用。OutputWindow.cs49–70使用EVENT query8/Issue1/GetData FLUSH1等GPU完成|没有要求producer调用swap-chain Present才使此独立surface成为D3DImage内容；CompleteNativePresent名字不能冒swap-chain Present。没有producer内容读回，实际surface字节unknown；无证据认定ColorFill未执行|
|surface→interop bitmap|Direct134：create(surface,96,96,version7,DirectAvailable,fallback0)。调用方保留bitmap/surface，回调为静态函数。R4 HR0、尺寸断言通过|InteropDeviceBitmap.cpp355–381建立引用；512–552 unavailable会释放用户surface/realizations并永久disable（fallback0）；715–817懒建front buffer，依赖同adapter已有windowed HW device，NotifyAdapterStatusInternal报告结果|不是必须手传共享句柄/keyed mutex。当前两个Available1不是每帧完成或front-buffer字节证据；actual method、front buffer、device存在与loss历史unknown|
|interop→CMilD3DImage(image)|Direct139–146：type97 resource；0x0A/ImagePacket24，offset0 ID/4 uint handle/8 UInt64 bitmap/16 UInt64 software0；发送前bitmap IUnknown AddRef。R8/12 HR0|Common/Graphics/wgx_core_types.cs888=97；WpfGfx/include/Generated/wgx_commands.cs47–61 Pack1完全匹配；uce/generated_process_message.inl204–258按TYPE_D3DIMAGE找resource；resources/d3dimage.cpp180–235释放旧引用、偷取发送端AddRef，软件0按fallback获取，NotifyOnChanged|不是漏AddRef、不是consumer需再AddRef。x86 native pointer4但wire slot8合法；结构无update flags字段。原始指针值/包内24字节未落盘，无法byte-exact复核|
|image→DrawImage record（不是另一个ImageDrawing资源）|Direct147–148：0x18/renderdata handle/DataSize48；DrawPacket60；offset12 recordSize48/16 command0x47/20四个double X0 Y0 W32 H16/52 image/56 padding0。D3DImageTests.cs702–707 Pack1|include/Generated/wgx_command_types.cs44/99：0x18 RenderData/0x47 DrawImage；wgx_renderdata_commands.h119–125含type、矩形、image、quadword pad。resources/renderdata.cpp79–130验证payload长度48、存流、GetHandles；renderdata_generated.cpp199–208解析TYPE_IMAGESOURCE；uce/resslave.h243–282将handle替换为依赖数组索引并RegisterNotifier（含取引用）|“drawing resource”日志实际type43=RENDERDATA，不是TYPE_IMAGEDRAWING；没有缺创建ImageDrawing层。record48=4字节size前缀+44字节packed命令体（type4+矩形32+handle4+pad4）；与外层头12合计60。未取得包原始记录，完整运行字节仍unknown|
|renderdata→image bitmap内容消费|同一image handle在DrawPacket中；R13 draw send0；后续Present更新同image，没有新建绘图资源|resources/renderdata.cpp759–770解索引后调用DrawImage；uce/drawingcontext.cpp1302–1365先GetBitmapSource，有bitmap则DrawBitmap，否则非DrawingImage时no-op；5548–5570对TYPE_IMAGESOURCE取source/bounds；resources/d3dimage.cpp90–115优先software，否则仅未disabled interop，允许source=NULL/S_OK|GetBitmapSource为NULL可无错不画，是具体断点。actual source非空、DrawBitmap实际执行、GPU复制内容均unknown；不能从draw send0说已进入DrawBitmap|
|renderdata→visual|Direct142/149–150：type39 visual、0x22/visual/data 12字节；R10/14 HR0|Common/Graphics/wgx_core_types.cs830/834：visual39/renderdata43；resources/node.cpp720–752按TYPE_RENDERDATA或DRAWING查content；234–260 SetContent注册notifier并dirty；189–204 OnChanged传播bbox/dirty；1191–1212 RenderContent调用renderdata.Draw|image change经renderdata订阅→visual订阅传播，不需要测试另外发“消费者内容通知”。实际依赖数组及dirty状态不可见，标unknown，不声称运行时已逐点取证|
|visual→root→HWND target|Direct143/169–173：type46，0x31 packed92，再0x33 settings72，再0x35 target/visual12；R11/15–20 HR0。Root为当前visual，不是另一个target或producer backbuffer|Common/Graphics/wgx_core_types.cs837–838：HWND46，generic47不同；wgx_commands.cs305–346/358–364；uce/rendertarget.cpp138–166 SetRoot查TYPE_VISUAL并RegisterNotifier。hwndtarget.cpp398–418保存HWND/DPI/flags|未找到错type、错root或错HWND。R显示target4/正确可见HWND，但完整92/72字节和root状态未序列化|
|target参数与enabled状态|Direct151–169清零92；HWND UInt64@8，section/master0@16/24，W32/H16@32/36，RGBA(0,0,0,1)@40，flags12@56，bitmap/stride/format0，DPI context-2@72、double1@76/84。OutputWindow73–83：screen RECT@8、notLayered0、opaque0、alpha1@32、child/RTL0、enabled1@44、cookie0@64/gdiBlt0|wgx_commands.cs305–346偏移一致；wgx_misc.h676–711：12=PresentImmediately4+RetainContents8，TypeMask0即默认允许HW或SW，不是Null3/强制HW2；hwndtarget.cpp25–55初始enabled=true/cookie0；511–575 enable1/cookie0匹配；398–418无需再SetFlags才能应用flags|没有确定遗漏enabled/初始disable cookie，也没有必须另发SetFlags的依据。flags12不证明HW；actual m_pRenderTarget/backend/m_fNoScreenAccess/zombie/bounds unknown。R可见性不能代替这些私有字段|
|invalidate→Render/Present|Direct174–176：0x37/target/RECT(0,0,32,16)24字节；close/commit在fill前；R21–23 HR0|wgx_commands.cs373–378 rc是头内字段；generated_process_message.inl1737–1765 payload0合法路由；hwndtarget.cpp82–195：Ensure target，enabled/no-screen/zombie检查，root非NULL及非空bounds才Render；263–297只有needsPresent且状态允许才target.Present，均可能无错误跳过|初始invalidate不是bitmap dirty的替代，已有各自两步。actual render pass、needsPresent与present结果没有独立内容观测；不能把内部跳过指认本次根因|
|dirty→frontbuffer→NotifyOnChanged|Direct184 dirty(0,0,32,16,bitmap)，随后Present；R26 HR0|InteropDeviceBitmap.cpp113–140按XYWH解析（非LTRB/非额外UseDirtyRect flag）；634–693 Present懒建frontbuffer、UpdateFrontBuffer；882–910 UpdateSurface(user dirty rects,user surface)→AddUpdateRect→清dirty；resources/d3dimage.cpp264–276再取fallback并NotifyOnChanged|范围完整非空、在GPU完成后，未漏dirty。无“Present update flags”字段；bitmap invalidate命令的UseDirtyRect属于另一协议，不能混加。disabled或surface空会no-op；D3D failure被转S_OK；本次实际分支unknown|
|duplicate event→独批→commit|Direct185–203：原manual-reset先Reset；SafeWaitHandle临时AddRef；DuplicateHandle SAME_ACCESS2/不继承；0x0B/image/UInt64 duplicate，PresentPacket16，send separate1、commit、Wait≤5s、flush。R27–31 dup1816/send0/commit0/event true/flush0|PresentationCore/.../InterOp/D3DImage.cs657–725同duplicate/独批；uce/clientchannel.cpp315–362独批立即close并恢复原open batch；resources/d3dimage.cpp249–285 Present后Notify，cleanup即使失败也SetEvent+CloseHandle duplicate|先Reset与真实WPF send后Reset是合法独立驱动差异：实际Commit前Reset，不存在后Reset抹信号。原句柄由托管Dispose；成功消费关闭duplicate，失败send部分写入所有权仍unknown；本次未做逐柄close证明|
|SyncFlush→合成/呈现→DC|Direct173 setup flush、203 consumer flush；R20/31为0，后DwmFlush/两次DC旧色|uce/clientchannel.cpp802–841等待引擎接收处理并返回zombie错误；composition.cpp1331–1359登记合成完成通知；partitionthread.cpp132–204：Compose需要present则调度Present后FlushChannels，否则无present即FlushChannels|SyncFlush不只是入队，也不是无条件绘制/scanout/指定像素保证。未绘制的合成可完成flush。DwmFlush没有本提交内容ID凭据。实际绘制target颜色unknown；本次DC两读与时间无效事实保留|

注：DrawPacket的细粒度大小进一步按生成头核定：wire record48=4字节record size前缀+44字节命令体（4字节type+32字节矩形+4字节handle+4字节pad）。因此命令体为44，不是40或48。整个60=12字节RenderData头+48字节record。double在wire offset20未自然8对齐，官方wire是packed，不应插入CPU自然对齐padding。

## 4. 与真实 WPF 调用顺序的差异及最小处置

|检查项|已确定差异/遗漏与裁决|本轮处置|
|---|---|---|
|HwndTarget双channel|PresentationCore/.../InterOp/HwndTarget.cs726–805在out-of-band创建target，duplicate到main channel，再initialize/transform/clear/root/settings；Direct为单channel全部初始化close/commit/flush后再invalidate。前者用于WM_PAINT可先于main commit；本测试WM_PAINT只ValidateRect，不另发out-of-band invalidate|这是架构差异，不是单channel无效的证明；不要机械加入第二channel、duplicate或world transform（当前DPI1/无变换）|
|主图更新与present顺序|真实D3DImage.cs670–672等待UpdateResource之后才Present；Direct在setup flush后才fill/dirty/Present，偏序更强。真实Unlock263–272订阅commit/WritePostscript；Direct不走Freezable，但native依赖通知由ProcessPresent执行|没有发现必须手补Unlock/WritePostscript命令的依据；不要把managed调度API当缺失wire opcode|
|availability恢复|真实callback745–759转Dispatcher按version更新IsFrontBufferAvailable；native fallback0失效后需新SetBackBuffer。Direct仅存回调，不重建。当前R没有Available0，仅两个1|若将来观察到0并继续使用同bitmap，才有具体恢复合同问题；当前不能按假定loss给补重建或判候选缺陷|
|当前日志资源名称|drawing实际renderdata，入口仍三色标签，运行实际固定单红|确定为描述问题，非内容根因；未来新增诊断使用独有准确资源键，不尾改旧包/历史报告|
|确定协议错误最小修正|**未发现确定协议错误，没有生产/协议修正表项可实施**|保持原失败/原严格像素；不修改flags、等待长度、fallback、格式、目标、阈值|

候选只作合同参照：`Code/WpfGfxShape/Core/GeneratedD3DImageResource.cs:6–20,94–147`同24/16 packed字段，update接管interop引用、Present后NotifyChanged、finally完成event；`Abi/InteropDeviceBitmapExports.cs:44–61`独立surface/device调用声明。本阶段未做候选全图等价审计，不以这些S证明20B3包内容或执行结果。

## 5. 剩余假设（仅两个，给出判别信号）

1. **HWND target/可绘制状态这一级没有把内容交给所观察DC**，不是interop特有问题。判别信号：同一HWND、同一target、同一connection/flags/DPI、空root的独立已知清屏颜色也失败。此结果只定位到target/交付/观测共同段，仍不能泛称环境；本次内部flags/backend/no-screen状态unknown。
2. **target可显示独立内容，但surface→interop→image这一段未提供实际bitmap内容**。判别信号：同一target先独立清屏严格成功，随后绑定现有image/renderdata并一次红Present，DC仍保持清屏或其他非红。该结果将具体缩小到image内容路径/该次更新调度，不单凭它宣布frontbuffer损坏、device-loss或候选缺陷。两个Available1只能作辅助反证，不能替代bitmap source实证。

不继续列“多等可能好”作为第三假设；fixed-window timing-invalid的限制保留，但不授权加长等待。

## 6. 唯一推荐下一门（需另授权）：同target清屏→原interop单红的有界分段控制

不是再跑同一个direct窗口：在原official direct设施中，增加**一个前置独立内容信号**，以同一 HWND/target 分开两段。先让root为空visual，target clearColor设严格不透明绿(0,1,0,1)，保留原flags12/32×16/DPI1/settings enable1/cookie0。setup close/commit/flush后明确invalidate该target一次、close/commit/flush/DwmFlush，只采一次中心DC，期望65280。清屏错误即保存原值并转清理，不再发红。

只有清屏严格成功，才发送原0x22将已建renderdata绑定到**同一visual/root/target/HWND**，close/commit使绑定进入批次；然后原producer一次红fill→GPU完成→dirty→独批Present→commit→event→flush→DwmFlush→一次DC，严格255。不可重建target、换窗口、换surface格式、改flags或fallback；不可按颜色循环、增加固定5秒等待、追加present/dirty。每段最大一次DC，全部最多2次；各命令/flush已有监督约束，新增JSON标明阶段、句柄关系及实际packet原始字节（指针只作为诊断，不作为跨进程等值）。清屏阶段不绑定image，避免将未初始化bitmap绘制污染独立控制。

新增判别力：原探针只有“整图失败”；本门在**相同target和采样对象**上证明或否定target基础内容输出，避免新public窗口通过掩盖原target失败。清屏通过/红失败可排除“target连独立内容都完全不能显示”，清屏失败即不再浪费interop任务。两段成功仅为一次official分段控制成功，不认证candidate/三色/历史首次/resize/完整替换。前置清屏也可能初始化WPF device/frontbuffer所需的target状态，因此“随后红成功”不能证明只是原采样过早，更不能抹掉原direct失败；须将这项明确的状态预热差异记为干预，而不是偷偷恢复历史native前置。若清屏失败只能否定该次既定完成条件下的输出，不能外推永久不能显示。

### public WPF consumer可复用性已核，不选作本次下一门

现有 `D3DImageTests.cs:298–325 WhenOriginalPublicHwndTargetDrawsThenWindowContainsHardwarePixels` 确为合法public HwndSource→DrawingVisual→DrawRectangle，但它**没有D3DImage/SetBackBuffer/AddDirtyRect**，只是矩形控制。`RunPublicShaderFrames:328–344`虽名Public，却手动加载candidate/构造底层协议，不是独立官方public D3DImage路径。对Tests内容搜索SetBackBuffer无命中。不能把任一现有方法当现成public interop对照。

public矩形创建部分可复用，STA/GetMessage owner现有；但ContextIdle只说明dispatcher到达该优先级，不是内容完成身份。引入public D3DImage需新增绑定/事件和managed调度合同，另建HwndSource也会更换target/后端决策，当前没有比同target清屏更小的独立对照。故本门不做public改造、不新增dispatcher/线程设施；若分段仍无判别，记录阻塞并停止，不能自动改走public路径继续研究。

### 最小预算与预定义停止条件

- 仅现有DirectConsumer独有partial增加一个默认关闭精确FQN入口/私有分段选择，必要时AcceptanceResources.resx一个准确键；原三色/固定窗口方法与断言保持。生产、公共helper、NativeRun、ExitDiagnostics、SDK/TFM、Package.targets不改。不是授权本阶段实施。
- 正常COM build≤1，失败停；COM publish≤1，失败停；显式绑定既有KP20B3文件/self-contained win-x86/两框架10.0.13/全新独立clean，Native publish0；只上传clean。独立build与publish内部build不重复计。
- official精确FQN任务≤1，监督120s/外180s；candidate0、发现0、native-only0、线程/WGC0、x64/full/resize0、额外public对照0。
- 实际official/WPF SHA或runtime/架构与R不符、TRX不是恰一真实体、身份/owner/大小错误、首个HR/event失败均停止。清屏错立即停止不发红；红错立即停止；cleanup错误独立保存且不覆盖主错。成功同样停止，不自动消费candidate预算。
- 不失败后修码重build/publish，不延长等待、换坐标/窗口、改预期、补跑原探针。唯一新增控制给出一次分段定位后必须回到结果裁决：若仍不能缩小，明确本门证据不足并提交具体缺少的运行状态，不新开连续基础设施工程。后续候选内容验收需独立授权且绑定实际20B3加载。

## 7. 账本、未闭覆盖与交接

本阶段所有执行增量0。
- direct累计build3（1败2成）/publish2/official2/真实体2失败/candidate0。最新fixed-window两DC失败且timing-invalid不升级基线。
- WGC独立账：build4（1败3成）/publish4（1败3成）/沙箱3（发现1真实2）/真实2失败；退场后default build1成功另列，WGC不重开。
- 通用打包退场残项独立：ReferenceCopyLocalPaths、FullPath去重、PackagePath/Copy与manifest引入前可信快照缺失，保留未恢复，不称全部打包差异复原。
- KS独立账：build1/publish1/沙箱4/请求3/发现1/体1/A1/B1/native2，不混入direct。
- KN/KO双架构各17条有限resize/lifetime历史保留；原ID resize像素、Available/actual HWbackend、真实loss/recovery、media/resources、最后源码对应最终四格仍Open。旧full1814/40与另4不相减。历史旧SHA成功不认证当前20B3，完整替换未完成。
