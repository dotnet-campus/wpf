# wpfgfx 原生到 C# 文件映射（历史证据长档）

> 状态：历史逐轮映射，不是当前实现/运行状态源。下文所有“最新/当前”和阶段边界只代表各次阅读时刻，彼此不可拼接。本文长档保留追溯，不机械重写历史 SHA 或失败。
> 恢复只从[唯一入口](../../next-session-handoff.md)开始；当前运行事实只认[基线](../../handoffs/current-stable-baseline.md)，模块条件见[主题合同](../../plan-module-contracts.md)。按本文具名主题查历史源码映射，不从任何“下一步”领取工作。

## 最新块A映射与身份（阶段4 Active）

FrozenShaderInvocation → effect command/constants/samplerInfo/handles/shader bytes与内部依赖引用提前捕获；并非 FrozenBrush/FrozenVisualTree 整树捕获。SoftwareBrushRealization.Capture仍进入cache/visual realization；GeneratedVisualRenderer.CaptureContents仍在内容捕获后读live Children。完整两阶段及公开callback/CopyPixels证据未闭合。交接SHA `E798360D7B3C0E5C0254016C917D4F6803692B786D346A0FC0EAD78FD339BE32`，普通4199、COM1346全过，RunID `3322e91f-f596-412a-94a8-62cc2b23b96f`，report `37d0765da7b84724ae03961e4c583128`；不是本轮复验，见 `handoffs/completed-work/S-STAGE4-A-INVOCATION-IDENTITY-SYNC.md`。下文旧状态不覆盖此最新身份。

## 最新代码回读与实测身份

见 `handoffs/completed-work/S-20261013-COMFULL-FAILURE-READBACK.md`，SHA `DB52FF9502F45A7B43CDD910A876390B27FCDE4F77A5218F8B2C53E2FE512C55`，COM1322/12/1334、RunID `9f178940-f12c-451c-8b77-922b2cd8bbc8`、report `c07d711980dc49c8987344f5697c73e6`已回读。clientchannel.cpp:921–969短read先出队后mincopy → UceChannelExports.PeekNextMessage/SameThreadChannel.PeekMessage现有64字节FIFO；56字节托管prefix不改变消费。GeneratedSoftwareShaderEffect已接软件执行，但Capture仍拒绝非implicit sampler；GeneratedHardwareShaderEffect仍逐次创建shader、失败直接返回。此前“完全未执行软件effect”描述不能代表当前代码；完整冻结、perdevicecache与错误政策尚无本次闭合证明。无生产修改，阶段4Active。

## 当前Visual/Cache sampler映射（有限闭环待完成）

GeneratedHardwareShaderEffect / GeneratedHardwareShaderSampler / SoftwareBrushRealization 已有真实双输入Visual/Cache消费；76 DWORD/sourceMutation/nullcache原expected通过为用户交接验收，见 `handoffs/completed-work/S-20261013-VISUAL-CACHE-76DWORD-HANDOFF.md`。不能再将27 DWORD创建失败描述为当前阻塞。本轮同SHA定向3/3、普通4197/4197、COM1302/1/1303，新report6a14f127c9d94aebacebc51fa483ebb4已实际回读。root范围、冻结/refs/重试、缓存无重复绘制证明仍未完成。GeneratedHardwareShaderEffect.RenderFrozen shader创建失败直接返回，尚无BadShader接线；GeneratedVisualRenderer.Capture:531拒绝effect，SoftwareOnly也E_NOTIMPL，不能标Closed。具体原生policy及独立软件compiler探索问题见归档，无生产修改或新发布。

## 最新sampler生产映射（未关闭）

ShaderEffect.cpp两阶段合同 → GeneratedHardwareShaderSampler新增SoftwareBrushRealization/GeneratedImageMask/SoftwareImageRenderSession真实ImageBrush绘制及Direct3D9InteropColorSource上传；tile/stretch/opacity/一般仿射复用，目标float工作格式保持。GeneratedVisualRenderer.Capture明确拒绝未执行effect。VisualBrush/BitmapCacheBrush及新增第二输入专项验收仍Partial。证据 `handoffs/completed-work/S-20261013-IMAGE-SAMPLER-REALIZATION-PARTIAL.md`，最终SHA DBE21C3D0DDEC3B01A4DFB101614C3CE9E4FDEAB5B2A4ECDA9F876B721C11F21；普通4197/4197，COM1299/1/1300，无新增回归，阶段4 Active。

## 历史sampler实际复验（未关闭）

证据 `handoffs/completed-work/S-20261013-SAMPLER-PROTOCOL-BASELINE-REVALIDATION.md`：普通4197/4197；COM1275/25/1300，0跳过/abort。SHA BB8B9B9B504C8903275C4C98D4F28119359201B79F2A339D3CFF96457B967934，RunID c4f48c60-b6a8-4100-b5b1-a736be341dd7。24项glyph像素失败根因未确认，另1项既有FLOAT57/450。公开WhenPublic*无失败；不沿用历史锁屏8失败。GeneratedHardwareShaderSampler仍仅D3DImage、None tile、opacity1、正轴对齐；普通ImageBrush/VisualBrush/BitmapCacheBrush通用realization未交付，阶段4 Active。

> 本文只记录当前状态和已确认事实，不记录逐轮增量、历史测试数字或下一步任务。
> 当前工作入口见 [`next-session-handoff.md`](../../next-session-handoff.md)，剩余缺口顺序见 [`remaining-gap-closure-plan.md`](../../remaining-gap-closure-plan.md)，新完成历史见 [`handoffs/completed-work/`](../../handoffs/completed-work/)，旧完成归档保留在 [`handoffs/progress-completed-work.md`（已清理；合同/说明）](../../history-index.md)。

## 当前Shader映射：真实公开pass-through已验收，完整族Partial

`GeneratedHardwareShaderEffect.cs`已有真实GPU消费者，`GeneratedHardwareBitmapOperations`及`Direct3D9SurfaceRenderTarget`消费设备cache入口；不再把shader资源描述为只有typed状态。生产临时回读/绝对路径诊断已移除。公开HWND shaderEffect=true连续两帧及设备换色Passed；padding/Ddx仍明确E_NOTIMPL，完整sampler/constants、失败/引用寿命与HDR格式表未关闭。证据 `handoffs/completed-work/S-20261012-244-SHADER-CACHE-PUBLIC-HWND-REVALIDATION.md`。阶段4 Active。

## 历史Media映射：授权generated软件消费者子项已收口

`Core/Av/SystemMediaSource.cs`、`SystemVideoSample.cs`、`SystemMediaModule.cs`及GeneratedMediaPlayerResource组成真实系统来源/通知/sample/软件Visual消费者。公开provider独立引用归零、callback close/Release/rebind/unsubscribe、系统binding版本及进程拥有模块寿命证据见 `handoffs/completed-work/S-20261012-MEDIA-LIFECYCLE-FINITE-CLOSEOUT.md`有限表。本轮普通4182/4182、媒体加SHA49/49，最新COM全量RunID `3dfa7504-ee23-432d-8cc0-2e3154c94b54`，1293/1/1294；报告5ceda4865cc24c73868a5f814014617a，SHA `47DB2B69098DCA6BE0629F22FEDC704A98350B368E0274D567CD9F24A360AF1B`。不等于完整原生AV/HW兼容，E_UNEXPECTED未复现不是根因修复。shader的generated资源仍只有typed状态/factory，真实消费者尚待实现。

## 历史Media映射（Partial，完整回归无新增失败）

`GeneratedMediaPlayerResource.cs`同源重绑先注销旧订阅再订阅新来源，避免删除刚安装订阅。公开同player三色Open替换、真实callback驱动Visual像素及parallel通过。归档 `handoffs/completed-work/S-20261012-MEDIA-REBIND-PARALLEL-FULL.md`；SHA `0D661C9C2290E6904729844269FEBC1D4319B3AAF24371D990F500B1A80064B8`，普通4182/4182、COM1283/1/1284无abort，RunID `4a35af6b-5264-4c7a-a50f-a0412680d4f3`。其余完整media寿命门禁仍开放，不标Complete。

## 历史Media映射修正（Partial，Red）

`Core/Av/SystemMediaSource.cs`已有公开media wrapper和真实系统eventproxy→订阅→`GeneratedMediaPlayerResource` pending→composition请求。`MediaExportsTests.cs`/`MediaExportsTests.SolidFrame.cs`以真实callback通知等待驱动公开像素消费。路径修正32/32及单红帧通过；新增绿/蓝多player顺序运行引出测试主机崩溃，未完成其余生命周期验收。最新SHA `BCF685CAC08AC5CAF20553851F23C9CF1E0028C440C72E11025E1F21A8694A19`，全量RunID `fd55b88c-e96f-4947-95e6-acf847a9ccf6`中止。证据`handoffs/completed-work/S-20261012-MEDIA-ASYNC-NOTIFICATION-RETEST-RED.md`。以下旧映射摘要是历史。

## Media与事件代理历史映射（Partial）

`core/av/eventproxy.cpp` / `eventproxy.h` → `Core/Av/EventProxy.cs`：callback锁外调用，描述符以非零CAS引用保护；公开COM阻塞callback并发进入及终结释放验收通过。SystemVideoSample已接公开Visual消费真实自有AVI两个FFFF0000；固定(有视频1/无音频0/2×1)元数据在测试中有界等待，超时失败。当前GeneratedMediaPlayerResource仍同步检测newFrame，并非system异步桥；source wrapper/公开media creation/替换关闭与锁失败sample重试未收口。当前SHA `4D5FD8A5B5FBF346E0AE855EEC1F56F1AA25544C2519A392AF11D1D6B78BB0C9`，最终全量1277/1/1278 RunID `5c6a6623-02b1-4108-bdcb-f960fb99b32e`；独立证据 `handoffs/completed-work/S-20261012-MEDIA-EVENTPROXY-LOCKFREE-PARTIAL.md`。

## 系统媒体软件采样历史前置

`core/av/internal.h:35–89`、`core/av/evrpresenter.cpp:2896–2993` → `Core/Av/SystemVideoSample.cs`：系统provider QI/GetSurfaceRenderer、BeginRender(NULL)、IWGXBitmapSource/renderer拥有与EndRender逆序释放。真实系统初始状态托管验证通过，不是有效视频像素或公开资源消费者。最新发布SHA `81078D6BCE323650DDFFD801AB023DCB8EAF1A9F79A20B739CB3C75423F634EB`，COM全量RunID `cddbce62-ad55-4723-80de-723aa946a8a7`，详见 `handoffs/completed-work/S-20261012-SYSTEM-VIDEO-SAMPLE-PRODUCTION-PARTIAL.md`。

## 历史代码验证绑定

manager时序与测试ABI修复当前发布SHA `B68BB90B6B920F53D04689B422C3FB4D411C04E5A2CFAFB01B9FBC15A3D0AC03`；COM全量RunID `7d52dc95-d9ef-4702-b064-bffacd46bec1`，1275通过/1严格FLOAT失败。完整证据 `handoffs/completed-work/S-20261012-MANAGER-ABI-FULL-REVALIDATION.md`。此身份不晋升任何Partial文件族。

## HWND设备创建合同（修复已真实像素验收，文件族Partial）

`core/hw/d3ddevicemanager.cpp:1295-1331,1435-1467` → `Core/Direct3D9Objects.cs` / `Abi/HardwareRenderTargetExports.cs`：公开target destination HWND同时进入CreateDevice focusWindow与hDeviceWindow，secondary swapchain保持同一目标；空窗口offscreen默认合同不变。实际公开资源协议呈现、两处窗口位置、两帧GetPixel原expected通过。原生Occluded恢复语义保留processed=false。证据 `handoffs/completed-work/S-20261012-HWND-FOCUS-PRESENT-ROOTCAUSE.md`。

## D3DImage（Partial，未 Closed）

最新映射及验收边界见 `handoffs/completed-work/S-20261012-MANAGER-AVAILABILITY-BOUNDS-BUILD.md`：Direct3D9DeviceManager / Direct3D9Objects / HardwareRenderTargetExports 已接通公开目标生命周期通知；GeneratedHardwareVisualRenderer修复整数source矩形传参，D3DImageTests真实callback内detach与平衡引用、shared像素通过。仍欠并发注销、callback内替换/关闭、真实loss/recover/fallback和独立bounds输出完整验收。以下旧“未验收”描述不覆盖本段新增事实。

`core/resources/d3dimage.cpp` → `Core/GeneratedD3DImageResource.cs`；`core/hw/InteropDeviceBitmap.cpp` → `Abi/InteropDeviceBitmapExports.cs`：真实外部D3D9 surface身份、HW前buffer StretchRect脏区、软件快照消费者和公开Detach保留前buffer已有实际验收。Present捕获interop临时COM引用并检查回调后替换/释放；SameThreadChannel精确包长转移所有权已修正。HW color-source实际复用、device-manager注册注销、独立interop bounds、真实availability/fallback恢复和公开回调重入/事件/借用引用/失败重试完整矩阵仍未验收，不扩展合同、不宣称Closed。订阅通知已增加非零CAS COM引用保护；真实surface拥有、detach后快照存活、锁定读取失败重试及present成功/丢包event已有验收，但实际manager通知/并发销毁和availability公开回调重入仍未验收。最新证据 `handoffs/completed-work/S-20261012-001-D3DIMAGE-SUBSCRIPTION-OWNERSHIP.md`，机器视图 `../Ledger/views/d3dimage-module-current.md`。

## GlyphRun generated软件消费者与真实字体生命周期（范围验收完成，文件族Partial）

`core/glyph/glyphruncore.cpp:50-155`、`core/resources/glyphrunslave.cpp:151-175`、`include/Generated/wgx_commands.h:443-458` → `Core/GeneratedGlyphRunResource.cs`：76字节原版命令和紧密数组载荷、真实IDWriteFont CreateFontFace、字体转移引用、替换/失败逆序释放和最终释放保护。`SameThreadChannel`接通失败partition/open batch丢弃清理；`GeneratedDrawingInstructions`接通GlyphRunDrawing→DrawGlyphRun槽顺序。公开AOT COM创建/替换/截断/丢弃验收已有证据。`GeneratedGlyphAlpha`、`GeneratedVisualRenderer`、`GeneratedImageContentBounds`已接通真实DirectWrite analysis/alpha、中心子像素灰度/gamma、glyph bounds及两条软件像素消费者；完整surface、clip/opacity/translation、brush/fontface替换、锁失败重试、float HDR和整数over已验收。`SameThreadChannel`修复自动创建handle时字体转移引用重复释放；句柄释放保活、通道/连接关闭及自动handle真实像素验收通过。系统字体路径未注册公共回调，不虚构glyph重入；合法传输Dispose回调复用实际通道重入合同。该软件离屏消费者范围已完成验收，不宣称所有脚本/ClearType显示目标/动画缓存/所有brush与仿射或整个原生文件族Closed。证据见`handoffs/completed-work/S-20261012-GLYPH-PIXEL-CLOSEOUT.md`，ledger当前映射见`../Ledger/views/glyph-module-current.md`。

## 状态定义

- `Partial`：已有生产实现和测试承载面，但不表示对应原生文件已完整翻译。
- `Not started`：当前生产项目中没有可确认的对应实现。
- 本表没有 `Complete` 项；当前实现尚不能替换原 `wpfgfx_cor3.dll`。

## ImageBrush 空内容与区域边界（Partial）

`core/resources/TileBrushUtils.cpp:110-165,180-255`、`core/common/basetypes.h:130-169` → `Core/GeneratedImageMask.cs`：NaN Viewbox/Viewport、相对内容bounds和调整后的viewport作为空画刷捕获，避免来源Acquire与相对变换求逆；直接生产AOT消费者已验证八分量透明输出和恢复。保持零尺寸空画刷既有行为；无限值仍不宣称完整支持，不能以非有限值统一空化冒充原生合同。

`core/resources/imagebrush.cpp:120-141` → `Core/GeneratedImageMask.cs`：未设置ImageSource时按DoesContainContent无内容合同捕获为空遮罩，由generated层消费者输出透明，支持通道清空来源后下一帧恢复；不改变未知来源类型的失败合同。

`Core/GeneratedGeometryBounds.cs` 已保留递归矩形组合的真实区域而非子组合包围框，四种布尔组合在局部空间计算后作用外层仿射，递归组合节点和直接矩形操作数均支持轴保持/轴交换变换并限制深度。无笔画 LineGeometry 作为组合填充操作数按空区域消费，与直接填充 bounds 合同一致，四种布尔模式接入相对遮罩与失败恢复链；不影响线的笔画语义。一般曲线组合及非轴保持操作数仍不支持；不等同于完整原生形状/画刷扫描消费者。

## generated 几何实色填充（Partial）

`core/resources/solidcolorbrush.cpp:20-72` → `Core/GeneratedVisualRenderer.cs`：按当前动画颜色与 opacity 捕获无笔画 SolidColorBrush 几何填充值快照，仅将 opacity 乘到 alpha。`Core/SoftwareImageRenderSession.cs`、`Core/Direct3D9SoftwareImageRenderer.ImageMask.cs/SolidMask.cs` 通过常量 1×1 位图 Extend 复用覆盖、裁剪、合成和层消费者；8 位复用 scRGB→sRGB 预乘转换，浮点分支直接写预乘浮点值。真实相对遮罩、嵌套 opacity/Pop、源锁首错与丢弃/恢复已有联合验收；浮点实色专门验收与原生边缘差分未完成。不等同于原生私有形状/画刷/笔画 ABI，非空笔画仍缺。

## generated 几何 ImageBrush 填充（Partial）

`core/uce/drawingcontext.cpp:878-952` 的 DrawGeometry → 当前形状 → DrawShape 调用链，在 generated 承载面由 `Core/GeneratedVisualRenderer.cs` 捕获无笔画 ImageBrush 填充、局部内容 bounds、设备几何覆盖与来源快照；`Core/GeneratedImageContentBounds.cs` 消费几何 bounds，`Core/SoftwareImageRenderSession.cs` 和 `Core/Direct3D9SoftwareImageRenderer.ImageMask.cs` 复用普通/tile 映射物化的完整 RGBA。真实作用域相对遮罩、嵌套 opacity/Pop、源锁首错与恢复、透明来源/零 viewport 恢复已有联合验收。该内部 managed 消费边界不等同于原生 IShapeData/CBrushRealizer 私有 ABI，非空笔画和一般组合 bounds 仍缺。

## 原生内部层与路径消费者（Partial）

私有 ABI 边界：`common/shared/RefCountBase.h:18-63` 明确 IMILRefCount/CMILRefCountBase 不继承 IUnknown；`core/geometry/ShapeData.h:25-105`、`ShapeBase.h:40-95` 明确 IShapeData 是带虚析构的 CShapeBase，IFigureData 亦为 C++ 虚对象。`core/resources/brush.cpp:40-63` 和 `BrushRealizer.cpp:97-166` 的真实创建者创建后 AddRef、缓存并转移/返回引用；`core/sw/swlib/swrast.cpp:533-596` 消费软件 EnsureRealization 后借用的 brush/effects，空 realized brush 不绘制。当前 `Internal.cs` 将路径参数折叠为 bool，委托式 `Direct3D9ImmediateBrushRealizer` 不实现该私有对象 ABI；实际布局、创建者、有限/无限 fill/stroke 和借用生命周期尚未接通。不得将这些指针当 IUnknown 或 generated 资源 handle，不能套用通用 COM 三槽释放。

`core/targets/BaseSurfRT.cpp:43-202`、`core/targets/BaseRT.h:79-100` → `Abi/BitmapRenderTargetExports.Layers.cs/Internal.cs`：基础链式层栈、父层交集、格式相关透明度阈值、NaN空层、End/Abort/最终Release清理；Bounds/DrawBitmap/Clear消费层边界。`Abi/BitmapRenderTargetExports.cs`包含PRGBA128Float裁剪Clear，直接写预乘浮点RGBA，不经8位转换。非空alpha修正和形状/画刷mask仍未接通。

`core/sw/swlib/swsurfrt.cpp:790-957`、`core/common/rtutils.cpp:44-132` → `Abi/BitmapRenderTargetExports.Drawing.cs/Internal.cs`：真实DrawPath/DrawInfinitePath COM槽共用当前层/目标与CContextState.AliasedClip相交判定；空交集在形状/画刷及目标锁之前成功返回。非空裁剪共用目标Read|Write锁，首错传播并确定性释放；无fill且无pen+stroke时成功，不消费未使用形状/画刷。有有效画刷路径保持E_NOTIMPL，完整IShapeData/CBrushRealizer/CPlainPen消费者仍缺；不能将无输出分支标为Complete。原生swsurfrt.cpp:1182-1204拒绝alpha目标的修正层，必须与generated opacity中间层区分。

## 内部位图裁剪与回调生命周期

`core/sw/swlib/swsurfrt.cpp:635-687`、`core/api/api_rendercontext.h:20-106` → `Abi/BitmapRenderTargetExports.Drawing.cs`：读取Release上下文前缀，目标尺寸与当前裁剪无交集时在效果捕获、源读取和目标锁之前成功退出；目标和源引用在效果列表回调前持有，失败及成功均逆序释放。Partial：尚无专门效果回调释放调用者引用验收；完整IShapeData/画刷/DrawPath及内部层指针ABI消费者未闭合，不以托管SoftwareImageRenderSession代替原生合同。

## 当前稳定化证据与阶段4映射边界

S-20261012-MASK-ROLLBACK-DECODER-STABILIZATION：GeneratedVisualRenderer撤回全局nearest与Visual固定linear，恢复按消费者scalingMode捕获mask；drawingcontext.cpp:3895–4005的nearest仅约束物化层1:1合成，既有EndOpacityLayer保持不变。CompositionEffectsTests.FloatMaskSpan恢复FLOAT端点/增量累计且严格距离<1e-7不变。CompositionTests.DrawingLifecycle六项真实系统WIC/IStream首错STG_E_READFAULT与重试像素已在生产AOT直接COM验收；三项mask改为固定原版Pbgra32全surface合同，非float golden。

当前全量1170/1，新增9项全通过，source17/18 float投影57/450、Pbgra32 0/180；最新发布/SHA/TRX身份只见handoffs/current-stable-baseline.md。以下历史摘要中“decoder首错仍缺”已由上述六项实际范围取代，不晋升完整方法族。D3DImage的CInteropDeviceBitmap身份/factory/消费者、glyph真实字形私有模型、media真实provider/sample/composition、shader真实bytecode/GPU消费及双缓冲全格式/替换/失败合同仍Partial或未实现；阶段4 Active。逐项源码、E_NOTIMPL及依赖见独立归档的最终检查表；其中双缓冲仍有可安全推进的生产模块且未确认外部阻塞，不能将全部剩余族概括为不能安全扩展。本次仅同步文档，无新生产实现、发布或测试执行。

## 平铺遮罩与层位图共同消费

当前核实的生产补充（Partial）：`Core/GeneratedImageTransform.cs` 的TryDecompose使用FLOAT分量/MathF.Sqrt/逆尺度与rest；`Core/GeneratedVisualRenderer.cs` 保留两次FLOAT尺度应用和brush rest，`Core/GeneratedImageMask.cs`、`Core/SoftwareBrushRealization.cs`独立传递原始消费者world bounds。Prepend/逆变换仍有double运算，不能宣称完整原始FLOAT坐标链等价。`Core/SoftwareImageRenderSession.cs`校验目标buffer指针、stride/有效size及宽高/行字节/面积的int表示边界；`Core/Direct3D9SoftwareImageRenderer.ImageMask.cs`校验tile读取边界，并在Pbgra32目标上先使用浮点分区生成/累加再交既有消费者量化。直接FLOAT rest传递候选与分区提前字节量化均不属于当前保留实现。旋转无回调、读取重入关闭/替换、实际填充锁冲突释放重试及非透明周期完整surface有代表性COM验收；不等于decoder首错注入。真实float/HDR golden、同版本绑定/颜色源合同、旋转tile/mapping/span、mask作用域与全族失败路径仍未闭合；source17/18差分未减少。

`core/resources/ViewportAlignedIntermediateRealizer.cpp:677-755,1090-1175` → `Core/GeneratedVisualRenderer.cs`的DrawingSnapshot、`Core/GeneratedImageMask.cs`和`Core/Direct3D9SoftwareImageRenderer.ImageMask.cs`：DrawingImage保留viewport透明padding和原网格整数裁剪；可分配tile在模式1/2/3共享重复纹理网格，避免nearest局部分区旋转产生重复覆盖。完整同尺寸tile通过AddRef复用；filtered Border、nearest/超大分区Extend及禁用重复预过滤进入fill/stroke/mask消费者。旋转padding的独立反算、tile/flip归属和真实嵌套过滤生命周期已有COM验收。S-20261009-242依据ViewportAlignedIntermediateRealizer.cpp:1000–1085、DeviceAlignedIntermediateRealizer.cpp:271–338及BrushIntermediateRealizer.h:29接入二维2048/2046尺寸限制，capture和tile消费者共用限幅，双向映射按最终尺寸重算；百万viewport及半tile HDR独立oracle通过。Partial：完整尺寸/映射方法族、tile shift及DeviceAligned方向选择、任意仿射原生差分尚未闭合；不是整个原生intermediate模块完成。

`core/resources/ViewportAlignedIntermediateRealizer.cpp:480-735`、`core/sw/swlib/bilinearspan.cpp:10-55`、`core/common/typeconvert.inl:38-80` → GeneratedImageMask与SoftwareImageRenderer.ImageMask/Affine/Float：Tile/FlipX/FlipY/FlipXY保存快照、物化viewport基础tile并通过既有texel选择/软件扫描重复和翻转。Partial：完整浮点/DPI/仿射/预滤差分仍缺，Extend未接通。真实IStream/系统WIC遮罩读取的重入、通道/连接销毁、删除、替换、帧快照及最终释放已有代表性验收，不等于完整联合生命周期合同。

`core/uce/drawingcontext.cpp:3890-4040` → SoftwareImageRenderer.EndOpacityLayer：真实SoftwareBitmap物化层内容，释放写锁后以单位变换、最近邻、禁用再次预滤经DrawTransformed→DrawPath→scan pipeline合成，效果/几何遮罩进入共同上下文；无几何遮罩时关闭边缘抗锯齿。直接8位及浮点缓冲合成旁路已删除。Partial：不等于原生CContextState、IShapeData或画刷指针ABI及完整内部目标合同。

## 软件遮罩内容边界

`core/sw/swlib/boundsrt.cpp:700-758` → `Core/GeneratedImageContentBounds.cs`：DrawImage变换后四角包围矩形与PushClip保守矩形求交，Visual自身内边界和子节点外边界分离；矩形裁剪包含几何自身仿射变换，不再要求轴对齐。实际裁剪仍由GeneratedImageClip/SoftwareImageCoverage负责，边界计算不执行精确光栅交集。`core/geometry/utils.cpp:769-825,903-1048` → `Core/GeneratedGeometryBounds.cs`：端点及三次曲线导数极值累计，二次曲线等价转换，Arc复用SoftwareArcConverter；圆角矩形、椭圆、填充路径及组边界已接入TryClip。`core/resources/combinedgeometry.cpp:45-135` → 同文件矩形CombinedGeometry边界：Union/Intersect/Xor/Exclude按局部空间边界单元求剩余区域，之后变换每个区域四角，支持空操作数、重叠空结果、负尺度和外层仿射；真实遮罩scope消费者已验收。Partial：操作数自身须为轴对齐非圆角矩形，一般曲线/递归组合及非退化曲线边缘完整原生精度差分未完成；不代表完整原生bounder。

## 图像变换栈

`core/uce/drawingcontext.cpp:2384-2404,4863-4892` → GeneratedImageTransform/GeneratedVisualRenderer：无动画正向轴对齐Visual变换/offset组合，PushTransform/Pop与透明层共用带类型状态栈。Matrix放大平移/Pop恢复像素验收通过；通用旋转斜切、负缩放、动画和覆盖率仍未支持，Partial。

## 软件图像缩小预滤波

`core/common/BaseMatrix.cpp:918-1054,1090-1195`与`core/sw/swlib/swbitmapcolorsource.cpp:162-205,490-590` → SoftwareImageRenderer.PrefilterSize/DrawPrefiltered及BitmapFormatConverter.CreateScaler。默认sqrt(2)分桶/Fant后格式转换与双线性重建，已接入真实普通/双缓冲图像消费者。Partial：仅轴对齐整数矩形，非整数覆盖率和任意变换尚缺，二维/桶边界验证不完整。

## 已有位图目标

`core/api/api_factory.cpp:344-390`、`exports.cpp:94-102` → BitmapRenderTargetExports.CreateForBitmap及同名生产导出：双IWIC/IWGX接口位图保活、现有缓冲绘制及释放闭环。Bounds/Clear经COM查询尺寸/格式，不依赖本库对象布局。`common/scanop/bitmapwrappers.cpp:724-877` → WicBitmapAdapter，普通IWICBitmap包装为MIL/WIC视图，工厂不再要求双接口；包装随目标保留，统一IUnknown、MIL锁格式及真实系统位图作为目标/包装后源已验收。Partial：BitmapSource-only/反向包装、原生缓存和内部目标ABI尚未完成。

## 当前总体状态

- 当前生产代码集中在 `Code/WpfGfxShape/Core/`，NativeAOT ABI 探针位于 `Code/WpfGfxShape/Abi/NativeAotAbiProbe.cs`。
- 已有实现主要覆盖 D3D9 加载、显示与设备管理、设备状态和资源、HW render target、bitmap/纹理、窄 glyph/text 路径及部分软件渲染。
- 所有已启动的原生职责均为 `Partial`。
- 生产 ABI、UCE/资源协议、完整图元管线和 PresentationCore E2E 尚未完成。

## 媒体 STA 事件闭环

Partial，已实现并通过真实线程组件测试。文件映射：`core/av/StateThread.cpp` → `Core/Av/StateThread.cs`（共享事件线程分支，不含 WMP 可复用工作项）；`mediaeventproxy.cpp` → `MediaEventProxy.cs`；`MediaInstance.cpp` → `MediaInstance.cs`。验证合同 `MediaEventLifecycleTests`：真实 STA 消息泵/FIFO、共享线程、Unicode/golden bytes/4K、shutdown 与队列引用、失败清理。生产 COM 导出及真实播放器仍不在此闭环内。

## 软件组透明层

`core/common/InternalRT.h:114-172`、`core/uce/drawingcontext.cpp:2295-2315,3030-3134` → GeneratedVisualRenderer层边界快照、Direct3D9SoftwareImageRenderer嵌套中间缓冲。Visual Alpha与PushOpacity/Pop在层结束时整组应用，失败丢弃未结束层；重叠/嵌套及层内部分绘制失败像素已验收。Partial：全目标中间缓冲，未优化内容边界；几何mask/effect、动画透明度、原生通用目标ABI仍未完成。

## 普通位图源绘制消费者

`core/resources/bitmapres.h:80-97` → `GeneratedBitmapSourceResource.AcquireBitmapSource`，取得带引用IWGX源；GeneratedVisualRenderer纳入图像快照，SoftwareImageRenderer区分IWGX枚举/WIC GUID格式ABI，复用软件目标扫描管线。PBGRA32/BGR32普通源绑定、替换、写锁冲突重试和调用方释放后的像素验收通过。`core/sw/swlib/swbitmapcolorsource.cpp:490-590`对应SoftwareImageRenderer.DrawConverted复用BitmapFormatConverter；可QI WIC的非直接格式源转PBGRA32，BGRA32像素/锁失败重试已验收。Partial；IWGX-only源包装、全格式及外部回调重入完整验收仍缺失。

## 同线程 UCE 生产通路

`core/uce/apifunc.cpp` → `Abi/UceChannelExports.cs`；`connection.cpp/clientchannel.cpp/htmaster.cpp/samethreadcomposition.cpp` 的窄路径 → `Core/SameThreadChannel.cs` 与 `GeneratedProtocol.cs`。Partial：连接保活、共享partition通道、客户端资源计数、DuplicateHandle排队、关闭批次分发、partition首错及BitmapSource转移引用丢弃清理已有源码。`UceChannelTests` 已经新发布真实DLL行为验收。`core/uce/printtarget.cpp::ProcessCreate` → `GeneratedTargetResource`：GenericTargetCreate 精确包长和资源类型检查、执行时 AddRef、重绑定/解除绑定/最终释放已接线并有真实生产COM引用验收；已接入共享分区目标注册引用表。`connectioncontext.cpp:448-525` → `UceChannelExports.cs`：SameThreadPresent按存活根通道身份顺序选择partition（共享通道不独立触发），经目标快照调用GeneratedVisualRenderer，消费Visual/render-data DrawImage并绘制双缓冲图像至真实软件位图目标。GeneratedVisualRenderer先捕获矩形/偏移及持引用的图像源，GeneratedTargetResource保活目标和根；Direct3D9SoftwareImageRenderer将原生锁缓冲适配至既有SoftwareRenderTargetSurface.DrawBitmap→SetupPipeline→OutputSpan真实source-over扫描操作。已取得两帧、半透明、偏移/目标边界裁剪和未Commit批次不被Present执行的像素验收；内存适配每图元复制目标，尚未优化；通用IRenderTargetInternal、完整Compose、根通道/显示恢复时序、完整zombie/重入未闭合，不代表完整UCE实现。

## 软件双缓冲位图生产模块

`core/sw/swlib/doublebufferedbitmap.cpp`及四个MILSwDoubleBufferedBitmap导出 → `Abi/DoubleBufferedBitmapExports.cs`；`common/scanop/writeprotectedbitmap.cpp`及位图/锁窄路径 → `Abi/SoftwareBitmap.cs`；`core/resources/doublebufferedbitmapres.cpp` → `Core/GeneratedDoubleBufferedBitmapResource.cs`：Partial，现有位图/格式/引用测试已在新DLL验收，并有Visual/render-data DrawImage前缓冲两帧像素窄路径E2E。已有MIL范围字节对齐非索引格式分配、保护页、IWICBitmap/锁、五项脏区、CopyForward及资源引用/事件清理；Abi/BitmapFormatConverter.cs接入原生WPF WIC转换替代手写预乘。SoftwareBitmap.Mil.cs新增独立IWGXBitmapSource/Bitmap/Lock地址与统一IUnknown；palette副本、1/2/4位MSB复制及非对齐锁写回已接入。IWGX脏标记采用无原生缓存注册分支，不含原生缓存容器。前缓冲生产消费者具备整数平移1:1及整数边界双线性放大路径；`swrast.cpp:225-295`/`bilinearspan.cpp:407-454,790-848`对应SoftwareImageRenderer经SoftwareBilinearSpan完成像素中心16.16采样/Extend/插值。已完成授权28格式真实WIC字节合同、palette、前缓冲Visual消费/替换和CopyForward真实锁冲突、重复事件通知/关闭、partition首错与丢包引用清理联合验收，定向29/29。最新全量1199通过/1失败/1200，RunID d9790bb9-8f27-42e0-9cc4-8488d1d2f592；AOT SHA256 A88BC8331CEAB4E276C8F87AE02CCF69E938687DCD8F2EF8C5D40C6D5AD6307D；报告wpf-calibration-dc37bbf41a724fd69418bbd189997d7b.json，详见S-20261012-DBB-PALETTE-COPYFAIL-LIFECYCLE独立归档。缩小预滤波、非整数覆盖率、原生缓存容器与复杂Visual通用消费者不在本次关闭范围；文件族保持Partial，不将上述验收等同完整翻译。

## ManagedStreamWrapper 生产 COM 导出

`core/api/exports.cpp:531-890` 的 CStreamDescriptor/CManagedStreamWrapper 和 MILIStreamWrite → `Abi/ManagedStreamExports.cs`。编码前已编写 ManagedStreamTests，真实 DLL 缺创建导出的共同前置 Red 后，已实现稳定对象/原子计数/16槽 vtable 与两个导出；新 Release/win-x64 AOT 上流17项、全量45项通过，EVID-MANAGEDSTREAM-COM Passed。双生产对象CopyTo四种输出组合/EOF、真实Clone存活、读写定位/扩缩容及STATSTG无名称字段已由测试内存流descriptor验收（流24项、全量52项）。不等于真实PresentationCore/WIC来源集成；错误来源/部分写入、名称分配释放等尚待验收，文件族仍Partial。保留 IManagedStream 两个扩展槽、QI E_INVALIDARG 和可空输出适配；不新增原生未支持的 ISequentialStream QI。

## EventProxy 生产 COM 导出

`core/api/exports.cpp::MILCreateEventProxy` 与 `core/av/eventproxy.cpp` 的 COM 方法 → `Abi/EventProxyExports.cs` + `Core/Av/EventProxy.cs`。编码前已定义直接 AOT 测试 24 项，包括回调 QI 重入。已实现稳定 unmanaged 对象头、四槽 Stdcall vtable、单一 EventProxy 引用计数。S-UNDATED-055 当前源码 Release/win-x64 Native AOT 发布成功，唯一 COM 验收项目 24/24 通过；EVID-EVENTPROXY-COM-EXPORT 在此范围 Passed。S-20260928-056 补齐受控并发引用生命周期：先复现非最终 Release 等待回调锁的行为 Red，再以独立原子计数修复；新 AOT DLL 全量 28/28，TRX 留存 SHA256。完整文件族仍为 Partial，不代表全部 ABI、跨 apartment、所有线程交错、原生差分或进程退出已验收。

## EventProxy 回调生命周期

`core/av/eventproxy.cpp/.h` → `Core/Av/EventProxy.cs`：内部 descriptor 回调所有者。编码前验证定义：`EventProxyTests` 验证 Stdcall 三指针布局、稳定 descriptor 地址、HRESULT、Shutdown、引用释放和释放后保护。已接入 MediaEventProxy/共享 STA 队列。IMILEventProxy COM 导出/QueryInterface 已由上述生产 AOT 验收覆盖；内部 Shutdown 不作为公开 COM 槽位，进程关闭 SEH 未验收。

## AV 通知器迁移边界

`core/av/CompositionNotifier.cpp` → `Core/Av/CompositionNotifier.cs`：Partial，纯 C# 方法族与组件单测已实现，注册/注销与通知共用锁、唯一列表保持头插顺序、UI 请求在锁外合并派发。编码前验证定义：`CompositionNotifierTests` 覆盖重复注册、注销、direct/UI 混合、无资源 pending UI、sample invalidation 和并发注销。内部 managed callback 合同不是新增 COM ABI；已接入 VideoSlave 分部方法族及 MediaInstance/MediaEventProxy/共享 STA 事件路径。真实媒体 provider/sample 来源与 partition scheduler 仍未接入，不构成完整媒体生命周期。

## VideoSlave/composition 方法族

`core/resources/videoslave.cpp` → `Core/Av/VideoSlave.cs`（GeneratedMediaPlayerResource partial）；`core/uce/composition.cpp` 的 RegisterVideo/UnregisterVideo/BeginProcessVideo/EndProcessVideo → `Core/Av/VideoComposition.cs`。编码前验证合同：`VideoSlaveTests` 覆盖完整单源注册、两级失败、sample Begin/End、消费者通知与最后释放。provider/renderer/scheduler 是尚未迁移的外部依赖合同；不得将组件验收标记为真实媒体播放或改变旧 COM 命令接收边界。

## MILMedia公开方法族前置

`core/api/exports.cpp:198–419`、`include/wgx_render.h:728–836` → `Abi/MediaExports.cs`：21个公开生产导出，真实IMILMedia Stdcall调用/byte bool/借用引用/HRESULT原样传播。唯一COM测试项目MediaExportsTests定向31/31，其中10项消费固定同版本系统WPF创建的真实WMP媒体对象。发布SHA D491C68EBE5C74EB4A59A24A62B5CE31D63F0C5C829949C7F23A4740528BA3A4；全量1275/1/1276。映射仍Partial：未实现候选媒体factory、播放、sample或provider/composition/Visual闭环，不把系统媒体查询等同完整媒体迁移。事实见S-20261012-MEDIA-EXPORTS-SYSTEM-SOURCE-PARTIAL。

## MediaPlayer 已确认边界

`include/Generated/wgx_commands.h`、`core/uce/generated_process_message.inl`、`core/resources/videoslave.cpp` → `GeneratedMediaPlayerResource.cs`、`GeneratedProtocol.cs`、`GeneratedValueResources.cs`：Partial。已有固定 UINT64 pMedia/BOOL 的 20 字节命令、强类型工厂和路由、可信同进程 COM QI/失败引用清理，VideoDrawing/render-data 依赖保留与释放已有回归。provider QI 成功后返回 E_NOTIMPL：RegisterResource 接收 CMilSlaveVideo*，CompositionNotifier 直接调用 C++ 对象成员，当前没有兼容 AV/composition 桥。原生 packet 路径未实现真实注册；托管 ProcessUpdate 与 VideoSlave 分部实现已有单一 source identity、帧就绪通知及 provider 注销方法族，handle table 可注入视频 owner。托管 provider/renderer 仍是外部依赖合同，不构成完整 media 生命周期。

## 当前文件映射

表中的托管文件允许一对多或多对一；仅列主要生产承载面，不展开测试文件和辅助类型。

| 原生文件或职责 | 主要托管文件 | 状态 | 当前事实 |
|---|---|---|---|
| `core/common/d3dloader.cpp`、延迟加载辅助 | `DirectXModule.cs`、`DirectXSystemModule.cs`、`DirectXBindingInfo.cs` | Partial | 已有 D3D9 模块加载、入口解析和模块生命周期实现。 |
| `core/common/display.cpp`、显示快照职责 | `Direct3D9AdapterSnapshot.cs`、`Direct3D9DisplayDeviceSnapshot.cs`、`Direct3D9DisplayDriverSnapshot.cs`、`Direct3D9DisplayModeSnapshot.cs`、`Direct3D9DisplaySettingsSnapshot.cs`、`Direct3D9MonitorSnapshot.cs`、`Direct3D9DisplaySet*.cs` | Partial | 已有 adapter、display mode、显示集合与变更查询承载面。 |
| `core/hw/d3ddevicemanager.cpp` | `Direct3D9DeviceManager.cs`、`Direct3D9Factory.cs`、`Direct3D9HardwareCapabilities.cs`、`Direct3D9Level1DeviceTest.cs`、`Direct3D9MultisampleSupport.cs` | Partial | 已有设备创建、匹配复用、能力探测和 Level1 测试切片。 |
| `core/hw/d3ddevice.cpp`、`d3ddevice.h` | `Direct3D9Device.cs`、`Direct3D9PrimitiveVertexBuffer.cs`、`Direct3D9DefaultState.cs`、`Direct3D9DeviceEntryGuard.cs`、`Direct3D9DeviceDriverWorkarounds.cs`、`Direct3D9GpuMarker.cs`、`Direct3D9Buffers.cs`、`Direct3D9Objects.cs`、`Direct3D9StateBlock.cs` | Partial | 已有大量显式 D3D9 COM 委托、状态设置、资源创建、绘制及错误处理切片；三个 `StartPrimitive` 重载已保持 device-owned buffer identity、每次清空与精确 FVF，`EndPrimitiveFan` 已保持少于 3 顶点静默成功、triangle-fan 参数、原缓冲区保留与既有错误映射；`GetVB_XYZDUV2/GetVB_XYZRHWDUV8` 已分别返回稳定且隔离的 device-owned 泛型 vertex-buffer identity，保留已有内容且无 Draw/FVF/COM/device-entry 副作用；`GetSupportedTextureFormat` 已按 source/destination precision、alpha 与五个缓存 supported-format 字段完成精确选择及 unsupported HRESULT 边界；`DrawBox` 已按原生固定 8 顶点、36 索引、fill/depth/FVF/AlphaSolidBrush 状态、12 个 indexed triangles、首错恢复与 `HandleDIE` 边界完成核对；`GetNumQueuedPresents` 已保持 marker tested/enabled/WDDM 门控、成功 Present 计数驱动的首次 flush、队列超过 2 时的第二阶段强制 flush、仅观察到 marker 消费后报告活动数量以及零输出/零额外引用边界；`InitializeIMediaDeviceConsumer` 已把借用的底层设备恰好一次交给 consumer，由 consumer 自行管理保留引用，设备层不进入作用域且不增减 COM 引用；多个 capability accessor 已确认独立读取对应缓存 caps 字段且无设备副作用，原生设备类未完整翻译。 |
| `core/hw/d3drenderstate.cpp` | `Direct3D9RenderState.cs`、`Direct3D9TextureState.cs`、`Direct3D9TextureStageState.cs`、`Direct3D9SamplerState.cs`、`Direct3D9TransformState.cs`、`Direct3D9ScissorState.cs`、`Direct3D9MaterialState.cs`、各 shader constant state 文件 | Partial | 已有主要缓存状态、失败后重试和文本 capability/shader 初始化状态。 |
| `core/hw/d3dresource.cpp` | `Direct3D9Resource.cs`、`Direct3D9ResourceManager.cs`、`Direct3D9UseContextGuard.cs` | Partial | 已有资源注册、驱逐和嵌套 use-context 生命周期承载面。 |
| `core/hw/d3dsurface.cpp` | `Direct3D9Surface.cs`、`Direct3D9UntrackedSurface.cs` | Partial | 已有 surface 所有权、描述、锁定及释放后保护切片。 |
| `core/common/d3dutils.cpp`、`core/hw/d3dtexture.cpp`、`d3dlockabletexture.cpp`、`d3dvidmemonlytexture.cpp`、`D3DTextureSurface.cpp` | `Direct3D9Texture.cs`、`Direct3D9LockableTexturePair.cs`、`Direct3D9VideoMemoryTextureManager.cs`、`Direct3D9TextureDescription.cs`、`Direct3D9SystemMemoryReferenceTexture.cs`、`Direct3D9SystemMemoryUpdateSurface.cs` | Partial | 已有 minimal texture description、texture level、mipmap、lock/pitch/dirty、staging 与视频内存纹理生命周期切片。 |
| `core/hw/d3dswapchain.cpp` | `Direct3D9SwapChain.cs` | Partial | 已有 swap-chain 创建、Present、Resize 和引用生命周期切片。 |
| `core/hw/d3dswapchainwithswdc.cpp` | 无完整对应实现 | Not started | software-DC present context、兼容 DC/DIB 和 GDI presenter 未翻译。 |
| `core/hw/d3dgeometry.cpp` | `Direct3D9GeometryRenderer.cs`、`Direct3D9PrimitiveVertexBuffer.cs` | Partial | 已有 indexed/non-indexed 批次、顶点打包和设备绘制委托。 |
| `core/hw/hwpipeline.cpp`、`HwShaderPipeline.cpp`、fixed-function shader 职责 | `Direct3D9Pipeline.cs`、`Direct3D9FixedFunctionMeshShader.cs`、`Direct3D9Shaders.cs` | Partial | 已有部分 fixed-function/shader pipeline 状态发送和绘制恢复。 |
| `core/hw/hwpipelinebuilder.cpp`、`Waffler.h`、`waffler.cpp` | `Direct3D9LineWaffler.cs`、`Direct3D9TriangleWaffler.cs` | Partial | 已翻译 `LineWaffler<PointXYA>` 与 `TriangleWaffler<PointXYA>` 的分区参数归一化、score/排序、对称边界插值、逐 cell 分段、triangle/quad/pent 扇形输出和 HRESULT 首错停止；完整 pipeline builder、waffler 链组装和 expanded vertex 消费链仍未翻译。 |
| `core/targets/BaseRT.cpp`、`BaseSurfRT.cpp`，`core/hw/hwsurfrt.cpp`、`hwdisplayrt.cpp`、`hwtexturert.cpp` | `Direct3D9SurfaceRenderTarget.cs`、`Direct3D9TextureRenderTarget.cs`、`Direct3D9DelayedBounds.cs`、`Direct3D9Software3DSurface.cs` | Partial | 已有 layer 捕获后透明清理失败的 source-bitmap 保留、上层单次释放与 capture 失败无释放边界，以及 opaque partial-capture 成功返回空矩形时跳过 capture/clear、source bitmap 为空并由 EndLayer 走 no-fixup 但仍恢复状态的边界，以及 display/window/surface/texture 目标的创建、Clear、Begin3D/End3D、Present、Resize、失效门控、窄 layer、软件 3D fallback，以及 texture render-target 的格式检查、固定 surface description、尺寸验证、cache-index 门控、不可驱逐纹理与 level-0 surface 创建/失败清理、派生析构、bitmap-source 引用生命周期、底层 texture 有效性与 no-ref 借用，以及对基础 pixel format、逻辑尺寸、device transform、shader pipeline capability、realization cache index、bounds/Clear/Begin3D/End3D、接口发现、hardware-raster 目标类型和 queued-present 查询的直接委托；三个描述访问器只返回基础目标已保存值，两个设备能力访问器只复用基础目标依据 device capability/cache token 得出的结果，D3D texture format 访问器只返回基础目标构造时保存的 target-surface format，associated display 访问器只返回基础目标构造时保存的 display index；ClearType hint 入口只更新基础目标保存的 force-ClearType 状态并保持 `S_OK`；cacheable bitmap-source 入口恰好一次复用现有 `GetBitmapSource`；`GetBitmapSource` 先清空输出并由窄 `GetBitmap` 取得独立引用，失败时保留首个 HRESULT 并释放临时 texture 引用，成功时直接转移既有引用而不额外 AddRef；窄 `GetBitmap` 自身先清空输出，首次成功后由 render target 保留缓存对象引用，每次成功调用在内容重新验证后仅对同一对象增加一个调用者独立引用，失败后可重试且调用者与 render target 的释放顺序互不提前终止对方生命周期；两入口保持相同缓存对象、内容重新验证和独立引用语义且不创建第二套状态；这些访问器均不读取纹理描述、不从 device adapter 或当前 display set 推导、不进入 device entry/use-context 或创建额外 bitmap-source/cache；六个现有窄绘制入口已在委托基础 surface render-target 前使 texture-backed bitmap-source 内容失效，并由后续窄 `GetBitmapSource` 或 `GetCacheableBitmapSource` 访问恢复有效状态；surface render-target 的 HW intermediate 对象创建重载在既有门控后原样转发 width、height、device、associated display 与 `ForBlending` 到 texture render-target 创建链，失败保持输出为空、首错传播、无 software 回退和部分候选清理，成功后才转移调用者所有权并提交 hardware-used 状态。 |
| `core/hw/hwbitmapcolorsource.cpp`、`hwdevicebitmapcolorsource.cpp`、`hwtexturedcolorsource.cpp`、`bitmapofdevicebitmaps.cpp` | `Direct3D9BitmapColorSource*.cs`、`Direct3D9BitmapReusableRealization*.cs`、`Direct3D9BitmapSystemMemorySurfaceSource.cs`、`Direct3D9BitmapTexturePopulationPreparer.cs`、`Direct3D9DeviceBitmapColorSource*.cs` | Partial | 已有 bitmap color source、realization、staging、device bitmap 更新和 cache 生命周期切片。 |
| `core/hw/HwBitmapCache.cpp` | `Direct3D9BitmapCache*.cs`、`Direct3D9BitmapFormatCacheEntry.cs`、`Direct3D9BitBltColorSourceCache.cs` | Partial | 已有格式缓存、候选复用、检索和生命周期承载面。 |
| `core/hw/HwBitmapBrush.cpp` | `Direct3D9BitmapBrush.cs`、`Direct3D9ImmediateBrushRealizer.cs` | Partial | 已有窄 bitmap brush 和即时 realization 路径。 |
| `core/hw/d3dglyphpainter.cpp`、`d3dglyphrun.cpp`、`d3dglyphbank.cpp` | `Direct3D9SurfaceRenderTarget.cs`、`Direct3D9TextPixelShaderResources.cs`、`Direct3D9Shaders.cs` | Partial | 已有硬件 painter、软件 fallback、ClearType/text shader 与 alpha texture 的窄路径；完整 glyph 私有模型未翻译。 |
| `core/sw/swlib/bilinearspan.cpp` | `Direct3D9SoftwareBilinearSpan.cs`、`Direct3D9SoftwareRasterizer.cs` | Partial | 已有部分 Tile/Flip/Extend/Border fallback、插值与 batch 推进。 |
| `common/scanop/scanpipeline.h`、`core/sw/swlib/swsurfrt.cpp`、`core/sw/scanpipelinerender.h`、`core/sw/swlib/scanpipelinerender.cpp`、`core/sw/swlib/swhwndrt.cpp`、`core/sw/swlib/swpresentgdi.cpp`、`core/common/mildc.cpp` | `Direct3D9SoftwareIntermediateBuffers.cs`、`Direct3D9SoftwareRenderTargetSurface.cs`、`Direct3D9SoftwareRenderTargetBitmap.cs`、`Direct3D9SoftwareScanPipeline.cs`、`Direct3D9SoftwareGdiPresenter.cs` | Partial | 已有三个 `MilColorF` intermediate buffer 的单块有界分配、索引切片、代际与旧视图失效，并连接 surface 的 SetSurface/失败/重绑/Dispose 所有权；surface 另有 resize uniqueness、DPI device transform、锁定、基础 bitmap/glyph/path 绘制、surface-bounds current clip、CSpanSink 参数转发与 span 输出、`Pbgra32Bpp` 目标 alpha 判定及 glyph ClearType hint 决策，以及非 effect scan pipeline 的 owned operation、初始化代际、失败清理与 PPAA filler slot。最小 HWND 软件呈现已接通 compatible DC、32bpp top-down DIB、selected bitmap、dirty-region BitBlt、resize、窗口销毁错误映射、partial creation 和逆序释放；ScrollBlt、software dirty notification、layered-window 与低色深转换仍未实现。完整 scan-op builder/effect/glyph 私有算法仍未翻译。 |
| `core/common/dwritefactory.cpp`、显示文本设置职责 | `DirectWriteFactory.cs`、`DirectWriteDisplaySettings.cs` | Partial | 已有 DirectWrite factory 和显示设置承载面。 |
| registry 与设备开关职责 | `Direct3D9RegistryDatabase.cs` | Partial | 已有当前使用的 registry 查询切片；WOW64 view 未实现。 |
| `include/Generated/wgx_command_types.*`、`wgx_resource_types.*`、`wgx_commands.*`、`include/processed/wgx_core_types.*`、`core/uce/generated_process_message.inl`、`core/resources/marshal_generated.cpp`、`node.cpp`、`guidelinecollectionresource.cpp`、`BitmapCacheMode.cpp`、`core/uce/rendertarget.cpp`、`hwndtarget.cpp`、`composition.cpp`、`htmaster.cpp`、`htslave.cpp` | `GeneratedProtocol.cs`、`GeneratedValueResources.cs`、`GeneratedTransformResources.cs`、`GeneratedGeometryResources.cs`、`GeneratedBrushResources.cs`、`GeneratedDrawingResources.cs`、`GeneratedVisualTargetResources.cs`、`GeneratedRenderDataVisual3DResources.cs`、`Generated3DResources.cs`、`GeneratedEffectResources.cs`、`GeneratedDrawingImageResource.cs`、`GeneratedBitmapSourceResource.cs` | Partial | 已冻结完整 command/resource ID、MIL/DWM SDK fingerprint、32 位 HMIL handle，并实现 transport/channel、基础值、2D transform/geometry、brush、drawing、GuidelineSet/BitmapCache、Visual/Target、代表性 render-data 与 Visual3D/Viewport3DVisual 的显式 layout、writer/router、强类型 factory/update、事务式依赖和树修改、通知、identity/refcount 及确定性释放；3D generated family 已有强类型状态、group/mesh payload 与引用生命周期，动画 line/rectangle/image/video、rounded/ellipse 和 PushOpacityAnimate 已有解析；render-data 保留可空依赖槽位与独立 payload。五类 effects/shader 已有强类型更新、动态 payload 和依赖生命周期，implicit brush 已纳入 brush family；DrawingImage 已有 12 字节显式布局、强类型 drawing identity、事务更新及 ImageBrush/ImageDrawing/render-data 通知与引用生命周期；BitmapSource 已有 source/invalidate 接收、QI/COM 所有权与 image 消费者通知，证据见 bitmapres.cpp、apifunc.cpp 和 wgx_render.h；完整传输队列回收、真实 wrapper E2E、DoubleBufferedBitmap/D3DImage、media/glyph 更新仍有缺口，DrawingImage GetBounds/Draw、shader 编译/GPU 消费、sampler 边界兼容性、完整渲染消费与 PresentationCore E2E 未验收。 |
| NativeAOT 导出 ABI | `Abi/NativeAotAbiProbe.cs` | Partial | 当前仅有探针级 ABI 和独立 Host/集成测试，不是完整生产 ABI。 |

## 当前未覆盖边界

以下内容当前没有完整托管映射；其分类、依赖顺序、进入条件和完成条件统一见 [`remaining-gap-closure-plan.md`](../../remaining-gap-closure-plan.md)：

- effects、完整 UCE 与 generated resource data/factory/`ProcessUpdate` 协议；
- 完整 glyph 私有模型；
- software layered-window、低色深转换等扩展呈现分支；
- `Reset/ResetEx`；
- registry WOW64 view；
- surface ScrollBlt 和 software dirty 通知；
- 普通 render-target dummy 解绑；
- 已关闭链路以外的新 layer/mask/effect 原生差集；
- 已关闭链路以外的新 scene/dummy back-buffer 原生差集；
- 完整 PresentationCore 到托管替代库的 E2E 调用链。

## 当前验证承载面

- 主测试：`Tests/WpfGfxShape.Tests/`。
- ABI Host：`Tests/WpfGfxShape.AbiIntegration.Host/`。
- ABI 集成测试：`Tests/WpfGfxShape.AbiIntegration/`。
- 最近稳定验证数字记录在 [`handoffs/current-stable-baseline.md`](../../handoffs/current-stable-baseline.md)；每轮完成证据写入 [`handoffs/completed-work/`](../../handoffs/completed-work/) 的独立文件，不在本文重复维护。
