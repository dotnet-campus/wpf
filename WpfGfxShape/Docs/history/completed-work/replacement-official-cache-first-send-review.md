# Official BitmapCacheBrush 首次发送协议审查

## 裁决
**未确证可修正的必要协议差异；FixtureUnchanged / ValidationNotRun / OverallOpen。** 不是“完全相同已证明”，也不是完成整体。没有新增 Passed，没有改生产、fixture、expected、阈值、两初帧、resize、secondary mutation、skip 或并行。normalCOMbuild **0/2**、COMpublish **0/1**（implicit0）、sandbox **0/2**、Native **0**、full **0**。未满足“确定差异才修复验证”的前提，未消耗执行预算；不追绿。

实质草稿在审查过程中先落盘，再完成本报告。没有终端、脚本、再委派、官方 DLL patch、新平台或无来源函数调用。desktop fresh/capturehelper/frame-token、native-only、foreground、paint、ETW 与 desktop 变体全部暂停；LastPresent 是 output 时间而不是目标 Present token，不能凭新 marker 宣称唯一 source。

## 来源与确切边界
- 亲读本地官方源：PresentationCore/System/Windows/Media/{Generated/BitmapCacheBrush.cs、BitmapCacheBrush.cs、Generated/BitmapCache.cs、Visual.cs、DrawingVisual.cs、RenderData.cs、CompositionTarget.cs、ChannelManager.cs、MediaSystem.cs、Composition.cs、Imaging/BitmapVisualManager.cs}相关函数；InterOp/HwndTarget.cs714–790；WpfGfx/core/resources/{BitmapCacheBrush.cpp/.h、node.cpp、visualbrush.cpp}；core/uce/{VisualCacheSet.cpp、VisualCache.cpp、VisualCacheManager.cpp、apifunc.cpp}相关函数；core/dll/wpfgfx.def1–141。
- 当前 fixture：D3DImageTests.VisualCache.cs 的资源创建、BindTwoInputEffect、目标/root、首帧、resize、mutation、SendSamplerPacket；JhPresent.cs全文；OfficialCacheObservation.cs现有机制；Pcore/PublicCacheContract.cs全文及 Program 路由关键部分。没有修改自动生成文件。
- 通过公开 API 检查读取既有 owned-desktop-full-x64-r1/clean/PresentationCore.dll 的 BitmapCacheBrush、BitmapCache、Brush、CacheMode、Visual API；类型与当前公开调用相符。该检查不是加载渲染运行或二进制方法体反编译，也没有返回该 DLL 独立版本/hash认证。因此只能绑定既有包的10.0.13来源记录，不能声称本轮重新核验其全部身份。
- 实际 official AD1C 历史记录为10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。本地源码没有经工具证明精确匹配该 commit；以下必要条件排除仅是本地官方源码依据，不冒当前 servicing 二进制逐字证明。
- 已读当前执行规则、当前工作项/基线顶部、remaining-contracts报告全文及完整方案§1–10。next-session-handoff 长历史有超长截断，仅最新入口及若干历史段已亲读，**没有完成其500余行全历史逐字阅读**；完整方案长历史也没有全部逐字读完，不把关键现行规范已读说成全文件完整阅读。

## 逐条首次发送对照
以下是必要偏序，而非伪造的当前 official 动态完整命令 trace。路径以 PresentationCore/System/Windows/Media 和 WpfGfx/core 为相对前缀。

|项目|真实 WPF 本地实现|当前手工 fixture|必要性/裁决|
|---|---|---|---|
|公开类型/基类|BitmapCacheBrush : Brush, ICyclicBrush；BitmapCache : CacheMode；Brush有Opacity/Transform/RelativeTransform|brush resource83、mode94、Opacity1|本地及包公开API一致；不是已查得的 VisualBrushBase 公共类型。两个源码范围检索均未发现该名字，不能造此接口/基类|
|首次 AddRef|Generated/BitmapCacheBrush.cs373–390 创建brush后依次引用两transform、cache、InternalTarget cyclic、animations，再UpdateResource|先创建brush83；source内容更早发送；cache94更新后发送brush|brush声明和依赖命令可有不同先后；实际引用前依赖均存在，没有证实该先后产生非法状态|
|Transform/RelativeTransform|UpdateResource明确对null或同一Transform.Identity发送null；AddRef可能仍引用identity对象|0x84两个字段都为0|**空handle与默认identity语义相符**；不能因WPF额外AddRef identity就补变换或猜错类型|
|Opacity/animation|非动画Opacity直接写double，animation handle空|Opacity1，animation0|一致；没有默认opacity缺项证据|
|BitmapCache值|Generated/BitmapCache.cs更新Scale及可选动画、SnapsToDevicePixels、EnableClearType|scale1，动画0，两个BOOL0|默认语义相符；已修0x8D28/0x8436保持，不重开同件|
|Target/AutoWrap|BitmapCacheBrush.cs Target直通InternalTarget；Generated默认AutoWrapTarget=false|secondary Visual39直接作为target|不必无条件多建wrapper/cache root；AutoWrap是条件行为|
|AutoLayout|AutoLayoutContent=true且Target是符合离树条件的UIElement才排布局|原生Visual39/DrawingVisual语义；PublicCacheContract明确DrawingVisual|不是UIElement，不能据此补Measure/Arrange或layout packet|
|cyclic注册|Visual.cs813–880维护托管channel/brush引用计数、NodeIsCyclicBrushRoot，立即调用RenderForCyclicBrush|手工不建托管visual bookkeeping；直接建资源图|没有对应独立native“注册cyclic”必发命令；注册保证发送可达，不等于fixture欠一条同名命令|
|WalkContent/Precompute|DrawingVisual.WalkContent→RenderData.DrawingContextWalk；GetContentBounds走bounds walker；Visual.PrecomputeRecursive先内容后children并缓存托管bounds|DrawImage record明确32×16，image12×1，native内容通过SetContent连接|WalkContent不是独立DUCE transport API；不能直接把缺托管调用当缺packet。未观察当前native实际bounds|
|RenderData顺序|DrawingVisual.RenderContent先AddRef RenderData；其AddRef先依赖image再UpdateResource，随后SetContent|image97 update→RenderData43 draw→Visual39 SetContent；两source均在cache brush update前|满足依赖先更新/内容后绑定。WPF可先创建visual声明，fixture先创建其它资源，不证明必要差异|
|Visual.RenderRecursive|首次Create/AddRef后依次cachemode/transform/clip/offset/effect/guidelines/content/opacity/mask/options/children|默认属性省略；primary后绑effect；secondary内容预先发送|首次默认省略是否错不能只靠函数数量判断；已具非空内容与effect。无确证默认属性命令缺项|
|children/offtree|UpdateChildren递归发送未上channel的children，完成后InsertChildAt；离树brush source无需接target root|contentRoot39→primary Visual39；secondary Visual39只由brush引用|离树是官方支持路径；不能为追绿把secondary插入显示树或强制多一个cacheRoot|
|cache注册|BitmapCacheBrush.cpp ProcessUpdate在mode/target变化后RegisterCache；node.cpp1263–1290懒建CacheSet，RegisterNotifier，AddCache|首次brush从空mode/target到非空mode/target|本地源码上可达首次注册；不存在必须先设置Visual.CacheMode才允许brush cache的条件|
|dirty关键状态|VisualCacheSet.cpp650–665新cache注册后**MarkDirtyForPrecompute**；cache构造m_isDirty=true/m_needsFullUpdate=true|source SetContent早于brush注册|**已排除具体假设：内容发送早于cache注册必然漏dirty，因此需额外invalidate。** 注册本身重置precompute可达条件；不盲补flush/重发内容|
|实际bounds/失效|PreComputeHelper对target无限surface边界遍历；FullInvalidate/EndPartialInvalidate→MarkCacheForUpdate→UpdateCaches|提交DrawImage尺寸与root矩阵已知，实际native bounds/dirty Unknown|这是消费状态而非少发命令已证。托管GetContentBounds不能读取该native cache状态|
|HW首次realization|VisualCache.cpp470–510在HW尚无resident display时可推迟实际texture；GetRenderTargetBitmap675–760读取dest.ReadEnabledDisplays，必要时置resident、ReleaseDeviceResources、Update|HAL HWND目标46，shader sampler可能经intermediate RT；producer HAL不证明consumer residency|**具体可证伪消费分支**：若dest显示mask无新active display且无cache RT，GetBitmapSource可得到null；RTB软件路径可立即更新。没有实际mask/RT，不能认定这就是根因或补fixture|
|brush取样|BitmapCacheBrush.cpp125–143主动PreCompute→UpdateCaches→CacheSet.GetBitmapSource；null source返回null realization|cache brush作为第二shader sampler；原期望FF80|额外managed render不是本地源码强制的首次消费前提；当前返回null/有RT但像素空/未进入分支三者尚不能区分|
|world/root|HwndTarget.CreateUCEResources为contentRoot绑定DPI MatrixTransform并SetRoot；CompositionTarget.Compile用contentRoot初始化RenderContext|独立root39/transform66，60byte DPI矩阵→SetTransform→InsertChild→SetRoot|已具层级；worldroot实际bounds未知不等于缺提交矩阵|
|target/out-of-band|真实HwndTarget先在OOB channel创建target并duplicate到主channel/提交，之后HwndInitialize；用于避免WM_PAINT invalidate先于target创建|一个channel先创建target再initialize/settings/root，最后统一Close/Commit|确有结构差异，但源码注释针对OOB paint偏序，不证明单channel cache必需duplicate。不得只看到WPF多一步就改|
|transport/partition|MediaSystem.ConnectTransport用false/CrossThread，首service channel null参考创建partition；后channel共享；AllocateSyncChannel用true|official initialize0/connect0/channelCreate(connection,0)；所有资源同channel|apifunc.cpp186/228明确false→CrossThread，null参考可新建partition；合法，不必伪造service channel。candidate connect1通过不能作official时序oracle|
|SyncFlush/图像present|wpfgfx.def导出MilComposition_SyncFlush；apifunc308调用channel.SyncFlush；D3DImage还有独立present资源完成合同|原Close/Commit→SyncFlush；JhPresentImage type0x0b/send separate1→Commit→event完成→SyncFlush；两帧flush/DwmFlush/assert|C++ bool参数用byte，handle/指针位宽、HRESULT int与源码相符；已有event不能冒cache消费完成。保持原流程，不加强制flush/Sleep|
|公开RTB路径|BitmapVisualManager.AllocateSyncChannel→Renderer.Render，源WriteableBitmap；软件可立即更新cache|Pcore PublicCacheContract off-tree DrawingVisual，32×16 RTB，四帧各512像素，历史候选通过|合法公开API内容证据，但不等HAL HWND/InteropDeviceBitmap/shader sampler/async首次时序。不能以此抹official手工红|

## 唯一精确阻断与合法观察边界
剩余工程阻断锁定为：**当前 official 首次消费时 `CMilVisualCache::GetRenderTargetBitmap(pDestRT)` 的实际显示驻留/texture状态**，具体依赖 `IRenderTargetInternal::ReadEnabledDisplays` 的mask、`m_rgResidentDisplays`、`m_pIRenderTargetBitmap`，以及该次 `Update` 所用 `m_rcLocalBounds`。需要区分 `GetBitmapSource` 取null（未建texture）、已有texture但内容为空、或根本未进入cache realization；这不是泛称“再选一个静态门”。

本地源码已经提供可证伪条件，不是已证现场：在HW无resident display时Update允许不创建texture且清dirty；GetRenderTargetBitmap只有发现新active display或render mode变化才重新Update。若mask/RT证实已正确非空，此具体解释即被否定，不继续拿它补命令。

**合法可观察性：** 本轮查得公开BitmapCacheBrush/CacheMode/Visual API无此getter；本地wpfgfx.def完整导出无channel-handle→cache-texture getter。MILRenderTargetBitmapGetBitmap针对已有公开RTB对象，不是cache resource83的转换接口，InteropDeviceBitmap_GetAsSoftwareBitmap仅producer，不能强转native slave指针。现有OfficialCacheObservation记录发送命令及事后Peek，Pcore无查得encoder/transport instrumentation；不能读取上述native状态。默认关闭托管method hook最多证明发送顺序/托管bounds，**不足以观察ReadEnabledDisplays/内部RT**，故本轮没有添加不能回答阻断的新钩子或平台。结论限本轮已核API：未发现合法读取这组内部状态的途径，不宣称数学证明所有官方诊断接口都不存在。不能调用内部C++成员、乱猜vtable或patch DLL。

## 16行现场复核边界：未完成原件再认证
最新历史报告位于根 `Docs/completed-work/replacement-exclusive-foreground-and-cache-evidence-closeout.md`（不是嵌套同名路径）。该报告明确引用 `official-cache-8836-7970572.jsonl`全部16行：connect0、两源12×1、dirty0、mode11 scale1、brush9→mode11/visual6、target12/root13/transform14/child3、world identity、flush0、FF80/0、post-result Peek retrieved0。

**本轮未找到并亲读该16行原件**：按精确basename/8836及present-occlusion-state目录关键词，在当前工具可见workspace递归查找未返回；artifacts枚举只有现可见各包。不能据枚举未返回宣称文件缺失（工具可见性有限），也不能把历史报告说成已完成原件现场复核。此项用户要求仍未完成，必须如实交接，不以报告替代原件。当前红色fixture另有受控full实际FF80/0证据，但不是此16行替身。

## 已纠正旧TRX误判（没有新运行）
按旧监督/输出所指精确路径直接读取成功：`artifacts/controlled-full-remaining-contracts-r1/official-original-readback/Results/remaining-official-cache-original-x64/TestResults/child.trx`，全文67行，其中长行分段补读32–45。普通父目录枚举仍未列TestResults，但精确路径可读，直接否定“枚举未见就缺件”。

- task9343dc001fec44cb93f16c0b247994af：testId14e862b3-9b71-843a-9ff5-67f779272c33，executionb229983c-3c5d-4728-8145-f58610649d20；FQN WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive，MSTest4.0.1；total/executed/passed/failed=1/1/0/1，error/timeout/aborted/notExecuted0；MTP2。
- actual official任务根AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，10.0.13/commit4099f839；testE08B135C7AF6813C70E7909BDB7D4762DAB88A644AD5148F1104CEA2B766A387，监督MVIDb3744fec-14e3-4271-94c6-ef5b0a6857fd，X64。
- 失败00563412/00141414，AssertCombinedPixel→RunVisualCacheConsumer；stages21行回读到cleanup空错，connect前失败，cache/resize/mutation未到。Native6BDF只是预检，不冒candidate运行。
- supervision40行全部回读：exited2、timeout/cancel/kill/drain超时false、output collected true/error null。父journal51–61回读Default/close/final Restored/Reaped/HandlesClosed true、Result2；之前记录的完整job计数本轮仅历史引用。不冒独立OSexit或活cache资源释放。此纠正不产生通过，也不覆盖full。

## before/after与回撤
|项|Before|After|回撤|
|---|---|---|---|
|fixture/Native|official首cache不稳定、根因未知|完全未改；必要缺项未确证|无需代码回撤|
|首次dirty假设|注册后是否需额外invalidate未证|本地AddCacheInternal主动MarkDirtyForPrecompute，排除必然漏dirty假设|文档裁决可随精确二进制证据修订|
|消费可观察性|笼统Unknown|限定至GetRenderTargetBitmap显示mask/residency/RT分支，公开getter未查得|不引入接口/平台|
|旧9343 TRX|误记回传缺件|精确路径全文亲读1/1/0/1，仍Failed|保留旧说法为历史并加顶部更正|
|16行现场|历史已读报告|本轮原件未再认证，明确未完成|不删历史认证、不倒签|

## 全替换判据与后续禁止事项
fixture稳定性与DLL功能缺陷必须分开：当前official手工红与历史通过不能证明候选功能正确或错误；候选公开RTB全帧绿只闭合该公开RTB有限路径。两边均不构成全替换验收。必须两架构Release全部适用关键能力、真实调用方内容/backend闭包、必要时序、资源自然收敛、loss/resize恢复及最后源码产物身份闭合，无未解释关键差异/未覆盖；不能靠绿数量。

原ControlledDesktopFull x64 **1823/1822/1**、x86 **1823/1821/2**独立保留，三个红不相减。真实consumer HWbackend/productloss/audio/naturalresources/finalfull及完整替换仍Open，memoryGreen=null。

不授权另一轮静态选门或原样重跑。本轮没有足够依据实施fixture fix；若后续不能取得上述消费状态的合法证据，应维持此精确阻断与红色能力未证，不能退回token平台、OOB多一步、强制flush、identity handle补齐等猜修。用户要求全部完成**尚未达到**。
