# r19：最后一个有界WGC基线握手实验与full依赖盘点 / OverallOpen

## 范围与执行账

生产修改0；仅Tests诊断Capture、只读字节审计器及其项目输入、监督器项目输入和本报告/恢复入口改动。原expected/通道容差1/DPI/设备flags/采样点不变，没有producer替consumer、按颜色选帧、追加Present、原版gfx加载/复制/fallback、终端脚本或再委派。先读latest handoff/r18，监督者未承担执行采样。

本轮专用publish3次，全部exit0：consumer、supervisor、已保存字节审计器；单独build0/test0/生产Native publish0。Windows sandbox2次：桌面consumer仅1次；第2次只读已经保存的四张BMP和BGRA，不创建窗口、不加载Native、不捕获。工具未返回最外层数值exit，不补造0。consumer发布工具截去前3334513字符，保留尾20000字符/日志62行已读；不能声称全编译警告已读或零warning。另两次11行发布日志全读。

产物根 `artifacts/candidate-desktop-baseline-r19/`，桌面任务 **327226847cc7449ab047aa184b21b3dc**。`readback/Evidence`父子exit/identity/stdout/stderr、Results模块身份、WGC事件24行、桌面事件27行均亲读；实际WGC BMP已加载。`byte-audit-readback/Evidence`原始BGRA审计及BMP审计506行全读。没有已执行尚未读结果的任务。

## 实施：基线先消费，唯一原参数Present，序号与时间双门

`Tests/DesktopWgcR17/Capture.cs`仍用原有CreateForWindow/CreateFreeThreaded/FrameArrived路径。StartCapture返回后先等待并消费首个基线事件帧，记录序号、arrival、SystemRelativeTime、尺寸，保存未经Alpha转换的Bgra8与BMP。保存后取事件水位triggerSequence；只调用一次原PresentNativeControl，里面原Clear/Present参数和断言、CompleteNativePresent/DiagnoseNativePresent不变。后续选择首个事件序号大于水位、arrival QPC和帧100ns时间均严格大于实际Present返回阈值且尺寸匹配的帧；没有颜色判据。所有实际出队非选择帧记录原因并保存，容量溢出记录原因并释放（不宣称溢出也有像素）；尾部排队帧不重新选择。本次只有基线，无后续帧，故无捏造的拒绝帧。

基线等待预算3000ms，基线后共享总等待预算6000ms（从StartCapture返回后开始计时，不在Present后重新起表）；最多32帧，每个surface copy另有1000ms取消上限。无基线不调用Present，已移除旧capture-unavailable后补调用支路。无后续帧则有界超时并finally解绑/释放，原GetPixel门仍独立执行。等待在控制线程，不放进HWND owner Invoke；owner为独立STA GetMessage/DispatchMessage。

**上限口径必须准确**：实现是墙钟期限检查和有限WaitOne，不是硬实时保证。本次elapsed6012ms（调度/日志开销越过6000ms约12ms）；同步WinRT初始化、原Present/CompleteNativePresent及文件I/O本身无本地可取消期限，copy也可增加处置时间。外层子120秒、父170秒、sandbox210秒是最终强制回收兜底。本轮未发生外层timeout。保留的旧JSON字段supported/trigger-after/timeout中的boundMs=3000是陈旧标签，不能据其宣称后段只等3秒；真实循环总上限6000，baseline-ready.totalWaitBoundMs=6000。本轮没有为修正标签再跑捕获。严格硬实时/全部API六秒返回不成立，不隐藏此实现边界。

## 实际因果证据

QPC frequency=10000000；以下直接来自原件：

|位置|QPC/ticks或结果|
|---|---:|
|StartCapture已返回|45968424777324|
|唯一FrameArrived seq1 arrival|45968425453117|
|基线帧SystemRelativeTime|45968425468750|
|基线消费|elapsed70ms，32x16|
|基线保存完成/ready|45968425676392，elapsed89ms，watermark1|
|Clear前|45968425696242|
|Present前|45968425711138|
|Present实际返回|45968425718443，HRESULT0|
|原control整体返回（含CompleteNativePresent）|45968430694998|
|等待超时|45968484904925，elapsed6012ms，events1/observed1/selected=false|

基线帧时间确实早于Clear/Present；不再有r18“尚未交付基线”缺口。基线之后没有seq2，故没有可对Present后内容裁决的帧，也不能把基线全零归到这次Present。Present-return到最终超时约5.919秒；原control返回约0.498秒后继续等待，并未追加Present。不能把“没有第二个事件”证明为生产未呈现、DWM未接收或宿主明确禁止捕获：环境/呈现/观测边界仍未知。**到此停止派生捕获实验**，不再以改变启动时序、颜色等待或反复重跑续接。

实际HAL type1/adapter0/behavior38(0x26)/SWVP1；GetCreationParameters/GetDeviceCaps/GetAdapterIdentifier均HRESULT0，driver d3d10warp.dll、Microsoft Basic Render Driver、remote0/session1。NULLREF不是本设备。owner12836、控制12696，事件managed tid6；owner记录消息及末尾WM_DESTROY2/WM_NCDESTROY130，stdout original fixture cleanup joined。没有同线程等帧堵owner证据；不把末尾消息当成所有时刻活性证明。

## 身份、原始帧、原门与清理

实际Requested=Actual为上述task的Package/Native/wpfgfx_cor3.dll；SHA再次复核为 **E158C6277E3B3043E25A98D808C9D7B5ACB81CFE0DE8EBE5D3187B8381C1073B**。harness SHA 0AF455CE15AB7C2F6A71AD6B77E3F93C6D451C950962AC5F5870CA6404A4963E，fixture SHA 4612F1FE74305C93C3A9210D33392B8C6CE4CF9079E0C2ED7ECC5656F81D8505，与r18相同是正常的：改变的是独立引用DesktopWgcR17.dll，不谎称这两个摘要覆盖捕获程序集；本轮未单独输出该程序集摘要。

唯一基线 `readback/Package/Results/wgc-r18-1.bgra`（历史文件名保留）2048字节全零，SHA **E5A00AA9991AC8A5EE3109844D84A55583BD20572AD3FFCD42792F3C36B183AD**。只读审计重新读取原始文件，zeroBytes2048，和BMP的54字节头后载荷逐字节完全一致。BMP2102字节、32x16、黑512/alpha0共512，SHA **35D6B18A3D56BD28459E221A1D6596FAA1DA94AE6918300D0CC3E3321D4DAEE4**，RGB123456计数0。三张GDI均SHA89682291F1B7CCAF28D5564628B58249690933CE16B9D986EBA145C446457A88，白283/其余229，不是全图白；样点(16,8)白，RGB123456计数0。

原GetPixel expected00563412/actual00FFFFFF、原容差1失败，stderr保留AssertCombinedPixel栈；shader/connect/resize未进入。图像只诊断，**不认证原组合或resize通过**。

父监督子2880和桌面7568均exit1、timeout=false、reaped/drained=true、killError=null；parent stdout/stderr空。解绑events1，session/pool/projected device均释放，手动Release item2/factory0/inspectable3/dxgi2/context0/device0，HSTRING HRESULT0，cleanup invoked=true/selected=false。非零计数保留，**refs0 Unknown**。正常回收不等于逐COM归零；未覆盖无基线、copy失败、队列溢出异常分支。本次真实走的是基线后无帧超时清理分支。

## full门：两份缺失oracle的实际消费者与字段

`ComAcceptance.Package.targets:8–17`在RequireReferenceData=true时要求输入目录及文件存在，只链接两份JSON到ReferenceData；**门禁未关闭**。诊断项目已有Sandbox=false构建路径不是正式full打包认证。

|文件|直接消费者|实际字段/用途|
|---|---|---|
|original-wpf.json|CompositionTests.ReferenceCalibration.WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements|根Samples/NativeSha256；每样本SourceMode/AffineMode/TileMode/ScalingMode/Consumer/TargetGeometry/PenThickness/PixelsBgra；17/18另用SolidTargetControlPixelsBgra、BitmapSourceControlPixelsBgra，各4x4x4=64字节。直接Pbgra/float投影/独立bitmap对照，不允许替换期望|
|同上|CompositionTests.DrawingLifecycle.WhenDrawingImageVisualMaskIsRenderedThenCompletePbgraSurfaceMatchesOriginalWpf|按17/0、17/3、18/3与Tile/LowQuality/mask唯一选择样本，读取PixelsBgra作全表面精确比较|
|同上|WorkingAlphaFirstDivergenceTests|按SourceMode/AffineMode/TileMode/Consumer/ScalingMode选样本记录FixedGoldenMatches；float自身断言不等于reference可省略（它仍读文件）|
|glyph-arial-original.json|GlyphPixelTests.Run及所有调用它的像素/outline/owner生命周期变体|PixelsBgra(32x24x4=3072)、Weight/Stretch/FaceIndex/Index/Advance/Bounds；原固定字体/布局参数下比较，含既有平移/剪裁等推导，不是任意字形图像|
|两者|DdUploadBoundaryTests.WhenPackageIsInspectedThenRuntimeAndInputSizesAreRecorded|glyph.OriginalLibrarySha256 / drawing.NativeSha256对照实际官方文件SHA|
|两者|ReferenceRevalidationEvidenceTests|同上SHA一致性；drawing Runtime/Architecture/NativeDll/NativeSha256与Samples筛选，保存GlyphRaw及Mask样本、test MVID/SHA、PresentationCore身份|

生成器只是静态阅读，**未执行**：WpfReference/GlyphReferenceTests输出runtime/arch/pointer、ActualModuleHandle/Path/Modules、NativeVersion、TestSha256/TestMvid、Font/FontSha256/Family/Style/Stretch/Weight/FaceIndex/GlyphIndices/Advances/Offsets、96DPI/PixelsPerDip1/Natural、OriginalLibrary及SHA、Index/Advance/EmSize16/Origin(2,20)/Bounds、32x24/Pbgra32/Grayscale/Fixed/identity Transform/PixelsBgra。ReferenceBehaviorTests输出SchemaVersion1/CapturedAtUtc/Runtime/Architecture/PresentationCore/NativeDll/NativeVersion/NativeSha256/SoftwareOnly/Limitation及450 Samples，含source/affine/tile/scaling/consumer、viewport/viewbox、SourceScRgbRgba/SourceGeometry/TargetGeometry/PenThickness/Mapping/尺寸stride/像素与controls。Pbgra量化文件不是float/HDR oracle。

### 可恢复性与可信性

本轮全工作区按两个完整文件名查找均无匹配；.coding-agent按JSON关键字段查找未找到嵌入原始内容。现有文本可读的历史报告AZ/reference-revalidation、integrated-comacceptance-full保存了来源任务、版本、摘要和部分头部说明，**没有足够450样本及完整glyph像素可重建文件**。AZ历史x64 glyph SHA2C229560BECDA1E5DFF8FCD7CBC61D61F61603887FFC5757A031D91A1CB50273 / drawing F2B1FE085EF90147F4DECDDB6376EEAC8E0166D025DC1AA366BB33DFCB9ADC49，官方AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，.NET10.0.13/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699，生成task df8452791ab746ceb7aa9e6618e0eead；仅作为归档定位线索，非本轮亲读reference原件。x86对应task04429ec5a1ef476ab6d3003accd0fc15，摘要见历史报告，不拿x86自动认证x64。

.git/HEAD确实存在，但没有可用的版本历史查询接口，本轮未解码Git对象/索引、未执行git；**不能声称所有历史提交均无文件**。结论限当前可见文本与已保存原件：未找到可直接复核的可信reference，不能从摘要恢复像素。历史报告的通过/摘要不是任意旧产物认证，也不是当前源码版本匹配证明。

需要用户提供：对应目标架构与官方runtime版本的**原始** `ReferenceData/original-wpf.json` 和 `ReferenceData/glyph-arial-original.json`，连同原生成/消费收据（任务ID、时间、SHA、测试程序集MVID/SHA及对应生成器源码提交/源码快照，官方文件版本/SHA/commit，PresentationCore身份，glyph字体SHA与环境）。可优先找回上述归档的完整JSON及收据；收到后只读核文件SHA、字段完整性、450样本矩阵/fixture映射、字体与官方版本和当前生成器源码差异；不默认旧版本即适用。无需交付原版DLL到本轮执行包，也不会运行原版或复制它生成oracle。当前candidate输出只能证明candidate，不能作为独立官方预期；因此不能自行生成可信oracle，也不能关闭full门。

## 唯一下一可交付（不依赖oracle，不是捕获实验）

**当前源码双架构真实MF已准入回调的Close/Flush/drain独立验收收据**：复用 `MediaFoundationResourceTests.Cancellation.cs::WhenRealReadCallbackIsAdmittedThenCloseFlushesAndDrainsAsync`，使用系统MF和已有生成AVI，不加载原版gfx、不读这两份JSON、不新增生产钩子。专用隔离发布/有界监督后只运行该精确测试，核read取消/交付0、重复Close同Task、callback释放前Close未完成、FlushResult/OnFlush、drain/线程退出、reader/callback最终引用与Shutdown/Uninitialize，以及实际源码/程序集/架构身份；x64/x86分别留原件并亲读。交付物是一份双架构有限验收报告，不宣称新增功能或完整媒体通过。本轮仅盘点定位，**未运行此项**。

选择原因：r16公开错误/立即Close没有触及已创建reader的真实Flush/drain，而此既存测试能触及该生产边界，且不受桌面呈现与缺oracle阻塞。它只证明native回调已经准入后持有/释放的取消清理，不证明源端pending尚未回调、Flush失败/永不OnFlush、任意URL打开可取消或全部COM refs0；这些外部故障条件仍需额外证据，不能用本项冲销。

最终：最后一次WGC基线实验及静态依赖盘点完成；生产、桌面原门/resize/full整体仍 **OverallOpen**。r15/r16有限结果保留各自身份，不移绑本轮。明确外部阻塞是可信原始oracle及其源码/版本来源，以及仍未知的实际桌面呈现/捕获环境边界；不以无限捕获实验替代可交付验收。
