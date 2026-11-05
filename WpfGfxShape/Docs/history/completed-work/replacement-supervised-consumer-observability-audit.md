# KS 后非线程调查：consumer 可观测性合同审计（纯静态）

## 0. 结论、范围与账本

本报告是静态合同研究，不是新实证、修复或完整替换验收。本轮代码/项目/公共捕获/入口/包修改 **0**，build **0**、publish **0**、测试/发现 **0**、沙箱 **0**、委派/终端/脚本 **0**；仅新增本报告并更新恢复入口及当前工作项。没有重跑、修包或探索无关生产领域。

**裁决：没有找到能确定解释 KS 旧色的 consumer 采样 API 错用；找到了“证据含义超出 API 保证”的缺口。GetPixel 是 GDI DC 像素读取，不是指定 D3D9 GPU surface 的通用读取接口，也没有 Present→该次 GetPixel 的帧身份合同。不能据此断言 GetPixel 必然漏 GPU 内容，或断言 KS 已实际呈现而只有捕获失败。**

最新实证仍为 KS task `52c18aa8497149ee845a5ef2625cc47f`：普通 Device9、原参数，两侧 client/screen 提交前后 `001E1D1C`，不是 `00563412`；owner 完整生命周期亦失败，Release/query/device/factory 各0、DestroyWindow各1、owner joined/clean 成立。原 verdict 为 inconclusive。停止线程方向，不换名恢复 owner/提交/生命周期变体。

保留累计 KS：沙箱 **4**（零命中、仅发现、缺清单初始化失败、本次有效 A/B），精确运行请求 **3**、发现 **1**、实际 KS 体 **1**、A/B各 **1**、native提交 **2**；历史初始 build **1**、COM publish **1**。设施失败不删除，不计像素失败；本轮全部增量0。完整身份/原件路径以 [KS报告](replacement-supervised-lifecycle-thread-scope.md) 为准。候选 Native 文件认证不是加载；KS末尾限定模块快照仅 system d3d9，candidate not loaded。

**唯一推荐下一门：另行授权一个“合法 official 真实 consumer HWND 的同帧 GDI DC / HWND 定向 WGC 交叉观测”诊断。不是再跑 native-only 前置，不以另一 API 自动当真值；保留原 GDI 结果与失败，先回答是否存在 consumer 内容与观测边界分离。** 具体限界见§5–7。本轮到静态交付停止。

## 1. 已读依据与定位约定

源码路径下文以 `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/` 为根；文档以 `WpfGfxShape/Docs/` 为根。读 current-execution-rules 全文、current-stable-baseline 全文、current-work-item 最新 KS 节、完整替换方案1–120（分段）；AGENTS.md 名称检索无结果。交接及长历史只读相关段，不宣称全历史全文或全部原始产物重新认证。

亲读相关代码：VisualCache.cs:503–516；OutputWindow.cs:33–75、85–94；PresentDiagnostics.cs:34–54；DesktopDuplication.cs:44–90、180–约225；CaptureFreshFrame.cs:110–约170；HbSurfaceConsumer.cs:26–约65、76–115、145–约195及194–215；正式 ComAcceptance.csproj全文。部分长返回截断，不把其后未显示段列为亲读。原 D3D9 ABI及创建参数逐项审计引用 [KR](replacement-supervised-native-api-contract-audit.md)，不是本轮重新独立认证 vtable。

历史亲读定位：BJ1–47/58–90；BX27–约61；CC1–约34；CG1–约42；HH42–约63；HO1–38/74–110；HD/HE/HF报告1–约44、98–150；早期 capture-diagnosis1–50。表中“历史”均为这些报告的证据复述，**本轮未运行、未逐份重新认证原始 JSON/TRX/二进制**。

## 2. 官方 API 合同与确定性分级

本轮使用网页读取取得下列微软正式文档正文。网页导航的授权提示不等于正文不可读；失败/缺正文的请求不作依据。DWM overview 是体系概述，含 Vista 时代服务说明，不把那部分外推为当前 Windows 操作方案。

|编号|正式文档|
|---|---|
|M1|[GetDC](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdc?redirectedfrom=MSDN)|
|M2|[GetPixel](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-getpixel)|
|M3|[Device9::Present](https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3ddevice9-present)|
|M4|[DWM overview](https://learn.microsoft.com/en-us/windows/win32/dwm/dwm-overview)|
|M5|[DwmFlush](https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/nf-dwmapi-dwmflush)|
|M6|[BitBlt](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-bitblt)、[Capturing an Image](https://learn.microsoft.com/en-us/windows/win32/gdi/capturing-an-image)|
|M7|[PrintWindow](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-printwindow)、[WM_PRINT](https://learn.microsoft.com/en-us/windows/win32/gdi/wm-print)|
|M8|[DWM Thumbnail Overview](https://learn.microsoft.com/en-us/windows/win32/dwm/thumbnail-ovw)|
|M9|[DuplicateOutput](https://learn.microsoft.com/en-us/windows/win32/api/dxgi1_2/nf-dxgi1_2-idxgioutput1-duplicateoutput)|
|M10|[AcquireNextFrame](https://learn.microsoft.com/en-us/windows/win32/api/dxgi1_2/nf-dxgi1_2-idxgioutputduplication-acquirenextframe)、[DXGI_OUTDUPL_FRAME_INFO](https://learn.microsoft.com/en-us/windows/win32/api/dxgi1_2/ns-dxgi1_2-dxgi_outdupl_frame_info)|
|M11|[Screen capture / Windows.Graphics.Capture](https://learn.microsoft.com/en-us/windows/uwp/audio-video-camera/screen-capture)|
|M12|[IGraphicsCaptureItemInterop::CreateForWindow](https://learn.microsoft.com/en-us/windows/win32/api/windows.graphics.capture.interop/nf-windows-graphics-capture-interop-igraphicscaptureiteminterop-createforwindow)|

### 2.1 window DC 与 screen DC：不是同一合同

- **合同成立：** M1 的 GetDC(hwnd) 是指定窗口 client area DC；GetDC(NULL) 是 entire screen DC。VisualCache:505–508 用 client(16,8)；PresentDiagnostics:47–54 将同一点 ClientToScreen 后传 screen DC。没有把 client 坐标直接当 screen 坐标。common DC 每次默认属性、单时刻只能一个线程使用，ReleaseDC 与 GetDC 同线程；不要求这对调用必须在 HWND owner。不能借此重开线程方向。
- **仅不保证：** M2 按 DC 的逻辑坐标/剪裁区域返回 COLORREF。它没有接受 IDirect3DSurface9、swapchain 或 Present token 的参数，也未承诺“任何 GPU 表面都能读到”“window DC 等于 DWM 重定向纹理”“screen DC 等于物理扫描输出”。这是一项正向保证缺失，不是微软明确说 GPU 内容永远不可读。
- **确定健壮性/分类缺项，而非已证旧色根因：** VisualCache 不判 GetDC(NULL返回值)、不单列 CLR_INVALID；PresentDiagnostics 不将 ClientToScreen失败隔离为无效采样。M1失败返回NULL，M2剪裁外/不支持等返回FFFFFFFF。KS合法旧色不是FFFFFFFF，坐标转换/窗口诊断已有成功记录，故没有证据把这些缺项写成本次失败原因。
- **确定断言边界：** 原 AssertCombinedPixel 每通道容差<=1，不是零阈值；这与完整替换零阈值要求不等价，但不会把001E1D1C变成00563412。只比较低24位还可能掩盖特定白色场景的CLR_INVALID，属静态风险，非本次实证。新的诊断应精确比较并独立记无效返回，不改旧expected追绿。
- **未知：** KS该时刻 DC实际底层存储、DWM对目标source的消费、显示端扫描与远端客户端收到的内容。本轮没有可据以裁决的目标surface/token记录。

### 2.2 Present、GPU完成、DWM与会话

M3确实承诺呈现设备下一backbuffer至目标client；NULL矩形在DISCARD合法，override窗口是合法目标选择，不是 Present“只写producer”的借口。**但 Present成功不等于后续任意捕获API返回该帧，更不是物理显示端内容证明。** 原窗口/device及矩形合同未见新增确定错误，详KR。

M4说明开启composition时窗口绘制被重定向到video-memory offscreen surface，再组合为桌面并呈现；不是每个窗口直接写primary。不能从这个概述推断 GetDC(hwnd) 就是可CPU读的该重定向面，也不能由无NOREDIRECTIONBITMAP推断实际已latch。

**M5比“只等某个全局vblank”更强：** Remarks明确等待 calling application 排队的 DirectX changes drawn to screen，且“不 flush entire session rendering batch”。报告不能弱化成无意义等待。但它无目标帧ID/像素输出，不建立从某个普通9 swapchain到GetPixel/duplication/WGC帧的可检查映射。API成功与观测矛盾需要调查，不能靠多一次flush证明目标已消费，更不能据文字保证反推所有失败只是GetPixel错。

EVENT query只是设备命令执行完成；CreateDevice/HAL标志、cooperative0、visible/noncloaked、session Active/Default、CCD active/LUID对应分别证明各自状态，不是内容/物理GPU/显示端扫描保证。不能把失败笼统归环境；也不能要求用户先证明恢复才准做内容能力诊断。

### 2.3 各候选观察路径的保证、边界与干扰

|路径|能够保证/采的对象|不能自动证明；干扰与本次取舍|
|---|---|---|
|GetPixel(window DC)|指定client DC的单点RGB，逻辑坐标/剪裁适用|不是指定GPU surface读取，无帧时间戳；取DC可能影响路径/时序的程度未知。现有通过是真实DC内容证据，不取消。|
|GetPixel(screen DC)|screen DC相应屏幕位置RGB；受对应桌面/坐标/遮挡条件约束|不是窗口隐藏内容读回，也非物理scanout/远程客户端证明；和window DC相同不算两个完全独立存储证据。|
|BitBlt/SRCCOPY、CAPTUREBLT|M6从source DC矩形复制至destination DC；可建memory DC/bitmap保存图像。CAPTUREBLT包括叠在目标上的layered windows|不把普通GPU surface变成可读GDI surface；同DC换批量复制不是独立consumer。CAPTUREBLT不是“捕获GPU”的flag；覆盖物纳入、格式转换、映射/剪裁、资源选择都可能改变结果。无理由优先替换GetPixel。|
|PrintWindow|M7向指定DC复制visual window，调用同步可能阻塞；WM_PRINT是请求窗口向DC绘制，DefWindowProc可转WM_PRINTCLIENT/擦背景|不是通用GPU帧复制合同；原WndProc无专用打印渲染且paint只ValidateRect。取回可能是打印/重绘输出，不能自动认为比DC更真实；调用可能激发消息/重绘。本次不选。网页PrintWindow正文未返回绘制消息细节，不把WM_PRINT文档当作所有PrintWindow内部实现的证明。|
|DWM thumbnail|M8 source HWND→destination HWND的动态live连接；注册仅建关系，UpdateProperties后才渲染|不是像素读回，不给本次Present token；读取destination仍需一条观察链。新增窗口/合成、缩放、alpha可能干扰。不能用注册成功作内容证明；本次不选。|
|DXGI duplication|M9指定adapter的output桌面图；device必须同adapter，desktop访问/模式/会话限制明确。M10成功可取desktop bitmap|不是某HWND重定向source读回，timestamp是整个desktop更新。mouse-only可S_OK且LastPresent=0；timeout没有有效像素/元数据。保护遮蔽、rotation/crop、output错配须检查。预建duplication改变观察者生命周期，历史已有影响假设；不作为本轮“新捕获”。|
|WGC window / monitor|M11取window或display frame，M12按精确HWND建capture item（Win10 1903+）。帧有ContentSize/Surface/SystemRelativeTime(QPC)|window item不等于monitor扫描；monitor item也不是物理显示端证据。IsSupported/创建成功不是内容成功；有安全/策略、边框、HDR/格式/生命周期约束。创建capture可能影响composition/时序。只选择HWND item以减少output归属歧义，不称其必然权威。|

## 3. 既有证据的身份、参数、捕获边界与新鲜性

以下为历史报告引用；完整SHA/MVID/原件定位见所链报告。不同task/测试字节不拼为同一次对照，也不把旧candidate结果绑定当前20B3F944…。

|证据|身份与参数边界|内容/新鲜性、能排除与不能排除|
|---|---|---|
|[早期capture诊断](replacement-supervised-capture-diagnosis.md) §因果证据|.NET10.0.9，系统D3D9 control，swap1/3，输出1920×1080/rotation1/monitor10001、Basic Render Driver；详该报告原件根|旧dup post-Present创建后拿mouse-only S_OK当内容是**确定旧夹具错误**。同读回链改为预建/drain、提交感知后FF123456；screen原本00563412。否定“Acquire成功就有内容”，不解释KS无dup链旧色。|
|[BJ](replacement-supervised-capture-fresh-frame.md) §2–6|x64 task d664443b…，test7FFDF4F3…/MVID a185491a…；x86 gate27e80704…，testEB9F506B…/1dc8a8ca…；10.0.13；HAL1/0x26/32×16/format21/count1/windowed1，swap1/3；非owner对照也过|18 owner帧red/blue/red，window/screen/front/dup一致，非零LastPresent晚于submission，pointer-only剔除。x64公共official→candidate阶段完成，x86 public无闭合TRX。否定Basic永不产帧/owner必需的绝对说法，不是当前包、原format0单帧或物理GPU证明。|
|[HD/HE/HF](replacement-supervised-hwnd-consumer-contract.md) HE98–125/HF125–150|x64 HD9e32d79…/HE333b412…同test878F8815…/e148d839…；official AD1C9DBB…、candidate8F74588B…；x86 e0036f3…/71daf6c7…同testC61C02D7…/e37ffa74…；official5EBD18E0…、candidate75D136F7…；10.0.13|合法async/separate Present/event、target92/flags12/context-2/DPI1，producer surface21/32×16，真实consumer window DC红/蓝/绿严格命中。consumer只有window DC，不冒充screen/dup三重证据；screen/dup fresh属于独立native前置。真实变化排除固定空/单一旧帧作为该成功的充分解释；consumer内部backend与扫描仍未知。|
|[BX](replacement-supervised-desktop-delivery-differential.md) §3–5|x64 final86719c1e… test6813EB78…/78965af3…，x86 control26472b6b… test57DCE17B…/5d1ce80a…；BD旧candidate仅包；与BJ同类format21/swap1/3、owner提交|GDI绿确实进入window/screen/front；D3D三色backbuffer正确而三种观察仍绿、dup timeout。GDI经dup的标尺自己失败，不能记green。强烈反对“只有GetPixel单API错”充分解释，但仍不能证明显示环境故障；同窗GDI污染和dup观察者混杂保留。|
|[CC](replacement-supervised-present-delivery-controls.md) §4–5|ec32bcd7… x64/test0B19442C…/bbfebc71…；Native打包依赖实际上official AD1C9DBB…，非候选验收。format21/count1/swap1/flags38；owner创建×Present override/NULL四格|12帧backbuffer正确、Present0，client/screen/front不变、dup无fresh。排除这两个变量单独/组合充分恢复该次交付；报告保留childTRX回收缺口，不能忽略。不能重做同四格。|
|[CG](replacement-supervised-dwm-delivery-boundary.md) §2–4|task66f437… x64，独立新窗、format21/swap1、red/blue/red；无GDI vs prepaint绿|无后置paint仍失败，绿生效不恢复D3D，dup六帧各10次timeout。否定“Present后WM_PAINT覆写”充分解释；全局DWM timing递增不是目标source消费。|
|[HH](replacement-supervised-shared-consumer-contract-fix.md) 42–64|task4b901b8e… test9B1101DA…/457ab1fb…、actual官方AD1C9DBB…/.NET10.0.13 X64。原样HB native三帧format21 vs旧单帧format0→actual22独立HWND|HB producer正确，client/screen00CDB38B、front同RGB、dup无fresh；旧前置同旧色，expected00563412失败。一次task双前置不同于严格单因子，不能只怪线程/format；official加载不等于已进入official consumer，ConsumerEntered=false。|
|[KS](replacement-supervised-lifecycle-thread-scope.md) 最新节|52c18aa8… x86/testD56FB203…/d5082b0d…；Native文件20B3F944…认证，未加载；system d3d9 F4757445…末尾快照。原format0→A actual22/count1/swap1/HAL1/0x26，单Clear FF123456/Present|A/B client/screen前后001E1D1C，明确非目标，无dup新鲜帧合同；B coop/actual扩展not observed。对完整owner作用域充分恢复是否成立已得负结果；不是GetPixel错或环境错的直接证明。|

负控的精确边界：DesktopDuplication.cs:44–72四个时间谓词数据行拒绝0/旧、接受等于/新；“unrelated fresh frame”队列用例只验证首个新鲜错误颜色被独立断言拒绝、不追下一正确色。**LastPresent>=submission不是属于目标Present的充分条件**（其它桌面更新也可满足）；方法名不改变这个事实。这些负控不证明真实HWND/output/crop关联或生产缺陷注入。BJ真实交替内容才补内容层；原单点单色没有完整图形/coverage证明。

## 4. 尚未排除的假设（仅两个）

1. **Hobs：至少部分当前失败是观察路径/捕获对象的边界问题；目标HWND的consumer内容能在窗口定向捕获中更新，但GDI DC或output观察链未显示该内容。** 可证伪预测：合法official consumer的WGC窗帧与GDI读数出现稳定的、逐次不同标记对应的分离。反证：若WGC在已校准可捕获标尺的条件下同样只见旧标记，则削弱“仅GDI看不到”的充分解释。HH/BX front及dup共同失败已使“GetPixel单点实现坏了”很弱，故不把Hobs写成高置信根因。
2. **Hdelivery：在失败阶段，目标consumer/source至桌面output或会话显示端的交付确实没有形成所需更新，而非只漏采。** 可证伪的子结论是“整个会话都不能更新”：独立非重叠GDI标尺window→screen真实变化足以反证它。WGC目标更新而screen旧，收窄至source/output或观察分歧；全部观察旧只能支持未闭交付边界，不能唯一证明DWM、驱动、会话、输出、电源中的任何一个。

不另列线程、format21、override NULL、paint覆盖、颜色反序、加Sleep、重试红帧、Basic必失败。HO的dup生存期与独立GDI标尺建议仍属历史未执行方案，不冒称本轮发现；KS无dup也失败，说明dup存在不是当前旧色的必要条件，但不能排除它对其它路径的影响。

## 5. 唯一下一门：official consumer 的同帧双观察诊断（须另授权）

### 5.1 固定内容、唯一比较轴

选择 **HWND定向WGC与现有GDI DC观察边界的成对比较**；不是切换producer、设备、线程、格式、参数、会话或显示配置。使用一个显式、默认不编译的ComAcceptance诊断partial，独立合法official场景，参数严格取HD已认证合同（target92/flags12/合法DPI context、async/separate Present/event/flush、32×16 surface21），红→蓝→绿三次既有更新。actual官方Load句柄→模块路径/SHA与test SHA/MVID必须记录。candidate不跑、不声称HW backend，生产不重发。

**不调用CheckFreshDesktopDelivery，不进入HH/KS/native-only旧前置。** 这是独立诊断而非修改/绕过原测试验收：现有前置、原失败和生产用例完全保留；新诊断结果不得记作原shader/resize通过。HbSurfaceConsumer目前observePartition同时控制是否跳native及分区观测，不能滥用true充一个“跳门开关”；也不先放宽旧断言让其续跑。新partial只采用已认证的合法consumer协议最小片段，独有资源/日志，不动旧入口/公共helper。

WGC用CreateForWindow精确锁定这个consumer HWND，捕获session在更新前建立并drain基线；每次更新有独立QPC边界。保留window DC和换算screen DC原始值，在同一颜色稳定区间采WGC内容，记录SystemRelativeTime/ContentSize/texture format/rowpitch/坐标映射。等待只能为有限期帧交付，**首个时间合格的内容帧即记录并判内容，不按expected循环追绿**；若无帧则timeout/not observed，不能以初值黑或旧帧代替。不由producer surface读回产生consumer结果。

校准在独立、不重叠、非D3D目标的GDI小窗完成：两次不同标记，window/screen DC和WGC均记录；不在D3D目标上FillRect，不预建DXGI duplication，不改变显示/session。它是采样路径正控，不是第二个D3D实验因素；失败仅表示相应路径不可判别，仍保全已到达的official数据，不追跑。裁剪/format/旧帧/错误颜色负控用内存数据检验，不能伪称真实帧。

### 5.2 判别表与不能证明的事

|观测|可支持/排除|不得推出|
|---|---|---|
|标尺有效，consumer WGC三色更新、window/screen DC旧|支持观察边界分离；反对“consumer没有任何更新”|不证明WGC=原始GPU surface、物理scanout、历史KS其实成功；新观察者可能改变合成行为|
|标尺有效，consumer window DC与WGC更新但screen DC旧|收窄至目标内容/桌面output观察分歧|不能仅凭screen DC归责显示端或DWM|
|标尺有效，consumer各路径均旧/无对应新内容|削弱“仅GetPixel漏GPU”的充分解释；保留真实consumer交付/调用链疑点|不证明环境坏；不单凭WGC无帧推断source无更新|
|consumer各路径均三色更新|证明本次official内容能力；无需继续同类探针|不解释KS历史、不证明candidate/full、HW backend/恢复/scanout|
|WGC不支持、创建失败、校准无帧、crop/时间/身份不成立|本门能力不足/不具判别力，保全API与退出|不改包循环碰成功，不判产品失败或环境根因|

同一提交双观察是**测量路径这一单因子比较**，不是已消除观察者干扰的随机因果实验；WGC开着会改变composition的可能性明确保留。本次只判断观察是否分离。若必须证明“WGC的存在导致恢复”，需要另授权off/on反事实，不在本门加第二对照或重跑。

相对历史新增判别力：CC/HH/BX/CG看的是GDI/front/output duplication；HD–HF consumer只有window DC成功，native duplication是在另一个前置HWND。既有源码范围检索未发现WGC/thumbnail/PrintWindow/CAPTUREBLT实现。**本门首次对真实official consumer本身取得HWND item帧时间/内容，并与同窗口DC成对对应**，不是把旧native红帧重新包装。独立GDI标尺借鉴HO，不以此声称创新；不重复HO的Ex参数/dup off-on路线。

## 6. 工具与两个正式工程的最小实现能力

当前可用文件读写、正式.NET构建/发布工具、Windows沙箱应用执行和图像读取工具足以支持日后实现/运行纯C#诊断；本轮一个也未用于执行。它们**没有已证可用的宿主控制台连接、用户桌面picker操作或物理显示读取能力**，不能承诺会连接真实显示端。无需用户先证明结论：先由正式应用记录IsSupported、精确HWND item创建、实际会话/输出及两色内容正控决定能力。

ComAcceptance为net10.0-windows/UseWPF/Nullable/unsafe，现有D3D11/DXGI声明及staging资源处理可参考，但本门不复用output duplication充WGC。最小新范围是独有C# WGC interop（activation factory/IGraphicsCaptureItemInterop、D3D11 WinRT桥、framepool/session、frame surface接口、copy/map及清理）和一个显式测试partial；不新增第三工程/Host/Win2D包，不改TFM/SDK/入口/公共捕获/项目。原TFM并不自动证明现有引用有完整WinRT投影；优先可审计的直接C# COM/WinRT ABI，无机器码/原生辅助DLL。事件、IInspectable及framepool ABI需要在实施前按实际Windows SDK正式合同逐项核对；本报告没有完成这部分实现级审计，不能承诺零成本可编译。

若需要独立纯内存的frame/crop/旧色负控，放WpfGfxShape.Tests独有文件；不增加共享抽象或改产品可见性。两工程总新源码最多3文件（ComAcceptance2 + Tests1），资源文本需本地化时只用相应独有资源。若必须改项目/添包/公共入口才能桥接WGC，**暂停重新申请范围，不静默扩项**。不以生产GetAsSoftwareBitmap、backbuffer/staging或producer CRC填consumer列。WGC帧的staging读取是consumer捕获纹理读回，与producer surface读回严格分列。

若此后确需真实显示会话能力，最小诉求是允许正式exe在可见交互桌面上运行并保留关联HWND/desktop/session/LUID/output及独立标尺内容，不要求用户先出具“已解决”的证明；受支持工具不能自动提供时如实列能力缺口。不得随机重启、切RDP、换驱动/电源或把Active/Default当证书。物理scanout/远端客户端接收不是这批工具已提供的直接证据。

## 7. 预算、停止与交接

**本阶段授权/实际：文档修改；代码0/build0/publish0/沙箱0。以下只是下一门预算建议，不是预授权。**

- 下一门最小修改：上节显式独有源码至多3文件；生产/旧测试/公共捕获/入口/项目/包规则0；保留全部原失败。先静态关interop/资源/过滤器合同，不能边跑边补。
- build至多1（聚焦两个正式工程相关新文件；不扫full），COM正式正常分析器publish至多1，仅x86 Release/self-contained，复用已认证当前Native依赖而不重发生产；发布不冒产品成功。若内存负控需额外Tests构建/执行，须单列授权，不能隐含占用零预算。发布依赖导致编译次数按实际另记，不把publish暗含编译抹掉。
- 沙箱至多1：现有正式ComAcceptance.exe、精确唯一新FQN、测试体至多1，official consumer序列至多1（3次既有颜色更新），native-only Present **0**，candidate **0**、x64/full/发现/重跑 **0**。内监督120秒、外180秒；其中标尺/每帧等待有界，不能无限等待或扩大超时追绿。
- identity/TRX/监督/stdout/stderr/stages/原始帧元数据与像素、真实Release/close/Destroy/owner收束完整回读；candidate包身份和actual角色分别列。零命中/初始化失败/平台不可用均消耗任务预算并停止，不修包重跑或将其计像素产品失败。
- 首次compile/publish失败、API/ABI身份不明、WGC正控失败、窗口遮挡/坐标映射无效、session变化、崩溃/超时/清理不收束：保全并停止。双路径均失败后不再新增同类颜色探针；只有已具目标关联能力/权限的trace或明确合同发现才另议下一门。
- 通过也停止，不自动扩candidate/shader/resize/完整替换。当前仍有真实consumer backend、历史HW、资源/通知、媒体、resize/device-loss和最终双架构矩阵等开放项；本报告不改变已证非显示修复，也不宣布完整替换完成。
