# 非显示真实媒体资源合同调查：完成，完整替换仍 Open

> 历史裁决已被用户明确撤销：下文“不许改独立解码器目标”及官方委托排程不再有效。生产原版wpfgfx媒体链已删除；普通工厂 E_NOTIMPL/null，独立Windows后端全部 Open / NotImplemented。历史绿不认证独立媒体，完整替换 Open。见[删除报告](replacement-remove-original-media-delegation.md)。

## 实质裁决

不是“现有媒体只有 stub”，也不需要用户决定是否把普通有效媒体扩入范围。完整方案 §3、§6.3、§8.6 已明确要求持续媒体和真实 WPF；§9 排除的是私有 PRERELEASE MilPlayer，不是 MediaPlayer/IMILMedia。retail MilPlayer 的 Closed 不排除正常视频与音频。现有系统媒体服务委托已有明确实现及历史实证，不能自行改成完全独立解码器目标，也不能把本次调查当媒体验收通过。

唯一下一门：另授权在现有 ComAcceptance 增加一个**无参、仅 default 模式、官方失败立即停止**的媒体比较入口，复用现成 Pcore MediaScenes/CompareMedia。先 x86，Native0，不改生产、不添工程/Host/SDK/第三方库/外部媒体文件；normalbuild≤1、COMpublish≤1，official≤1、candidate≤1、独立 comparator≤1、MSTest体≤1。当前只设计，不实施、不运行。选择理由不是缺真实接口，而是现有比较测试会展开三模式且先启动 candidate 再检查 official，不能满足当前有界停止合同。

本阶段生产/测试/项目/配置/样本修改0；build/test/publish/Native/沙箱/发现/采集/终端/脚本/再委派全部0。只读源码、文档、既有程序集元数据及清单，新增报告和导航。历史预算逐门保留，不重置。4199 Debug通过与Release编译成功保持，不能冒 Release运行/双架构Native full。显示 Unknown，HW/resize/loss/长期资源趋势/随机差分/full 与完整替换仍 Open。

## 1. 亲读范围与证据层次

已亲读最新 handoff 顶部、managed-major-regression-current-source 全53行；完整实施方案全203行（初次截断后分段补齐）、remaining-gap 全70行、current-execution-rules 全27行、remaining-matrix 全201行；current-work-item/plan-status/baseline 当前段。长导航下方旧排程不是新授权。亲读下列生产及测试关键实现、官方源码接口，不声称本次重读所有历史任务原件。

正式项目已枚举：Tests/net10.0 的 SystemVideoSampleTests 只有 null 入参测试；GeneratedMediaPlayerResourceTests 明确用 MediaStub；Direct3D9VideoDrawProductionTests 用 FakeVideoBitmapSource/委托设备。Av notifier/controller 和这类 synthetic/COM 引用测试不是系统解码实证。ComAcceptance/net10.0-windows/UseWPF/exe 才有真实系统 player、公开 WPF 场景与独立对照入口。

## 2. 生产真实合同及官方依赖边界

- `Code/WpfGfxShape/Abi/MediaExports.cs`：MILFactoryCreateMediaPlayer 调 SystemMediaSource.Create；Open/Stop/Close/Get/SetPosition/Rate/Volume/Balance/Scrubbing、HasVideo/HasAudio、尺寸/长度/进度/Shutdown 等转发真实 IMILMedia 槽，而非固定成功 HRESULT。接口存在不等于本次认证所有格式、方法或版本。
- `Core/Av/SystemMediaModule.cs:13–36`：必须已有 PresentationCore.dll；按其实际加载目录找同目录 wpfgfx_cor3.dll，比较文件实体避免加载自身。官方模块缓存、进程驻留，不 FreeLibrary。这是候选的外部系统媒体服务依赖，不是候选自主 decoder，也不是新 Host。
- `SystemMediaSource.cs:29–78,80–205`：官方 MILCreateFactory/MILCreateEventProxy/MILFactoryCreateMediaPlayer 建真实 player；候选包装持有 native player/proxy/client；QI provider 包装身份，GetSurfaceRenderer 转发系统对象。最后 Drop 执行 Shutdown/Release/root与identity释放；系统模块驻留不代表对象免清理。
- `SystemVideoSample.cs:24–120`：QI `E6F1CC74-A0EB-4EBE-8241-089D1CB079D7` → GetSurfaceRenderer → BeginComposition(null,false,true,lastTime,ready) → BeginRender(null,out source)。真实 IWGXBitmapSource/IWIC 可读 source 是解码帧，不是另造静态 WIC 输入。lease 必须 EndRender → EndComposition → Release(source/renderer)，provider 临时引用也归还；Close 幂等且保留首个结束错误。零 source/ready0 不能算非空媒体通过。
- `GeneratedMediaPlayerResource.cs:45–90,100–166`：transport 引用接管、旧订阅注销、真实媒体保持、RegisterVideo；AcquireSystemFrame 临时 AddRef，采样结束归还；资源释放取消订阅、UnregisterVideo、释放媒体。候选仍负责 UCE/绘制消费，不是把候选 RTB 合成转给官方。
- 官方仓库 `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_render.h:728–825` 给出 IMILMedia；`core/av/internal.h:35–88` 给出 IAVSurfaceRenderer/IMILSurfaceRendererProvider。后者是 WPF **内部** COM 源码合同，并非跨版本公开稳定 Windows SDK ABI。RegisterResource/UnregisterResource 接受真实 CMilSlaveVideo，不能伪造指针给官方；候选不靠它注册托管对象。普通系统公开调用是 CoInitializeEx 与公开 WPF MediaPlayer/VideoDrawing/RenderTargetBitmap，下一门优先走后者。
- 官方 `core/av/avloader.cpp:883–895` 实际 CoCreateInstance(CLSID_WindowsMediaPlayer, CLSCTX_INPROC_SERVER, IID_IWMPPlayer)。需要 Windows 媒体组件、同架构 COM 注册、AVI splitter/DIB 与 PCM 支持；不能假定精简/N版系统具备，也不把缺组件当候选通过。当前普通 AVI/DIB/PCM 不需要 H.264、网络、下载、FFmpeg 或新 NuGet。
- 音频：当前可控 AVI 含合法 PCM 音轨并检查 HasAudio；未找到本合同向调用方导出 PCM 样本的接口，不能把 HasAudio/静音播放写成实际音频内容或音频设备输出验收。

## 3. callback/apartment 与结束合同

SystemMediaSource.OnEvent 在系统调用它的线程直接 Notify，再转发 client；不派送到 UI。Retain/Drop 覆盖回调体；订阅快照+Cancel 不等待正在执行回调，不承诺 Unsubscribe 返回即所有在途回调已退出。GeneratedMediaPlayerResource 的 listener 身份/IsReleased 检查及锁避免旧订阅驱动新资源。仅源码推导，非本次竞态实证。

ComAcceptance 的 SystemMedia 构造 CoInitializeEx(0,2)，接受 RPC_E_CHANGED_MODE，因此现有底层测试不能宣称严格 STA。RaiseEvent 原始字节入队，回调直接执行，finally Set notification；记录 CallbackError。现有 callback内Close/返还caller/rebind测试确实由真实 Open 驱动，但并不完整记录原生 callback TID、关闭后静默区间或所有系统worker退出。

Pcore 经 NativeRun 的 STA 入口直接进入 Run，MediaPlayer 事件在 Dispatcher 上处理；WaitMedia 使用25ms DispatcherTimer、每条件15秒、finally Stop，失败立即抛出；没有等待窗口像素或显示链探针。公开场景先 Opened，再 Position/非空且变化帧，Pause替换Drawing保持内容，再自然 Ended；finally Close、清空Drawing/Visual，程序 Dispatcher shutdown/finalizers 后退出。该收尾不是所有 decoder线程退出、COM ref0或长期趋势证明。不能为了本门增加线程/等待/ETW矩阵。

## 4. 最多两个候选（均不是新微 ABI 门）

### A：首选真实公开持续媒体（现入口需要小改以符合预算）

现有 FQN：`WpfGfxShape.ComAcceptance.PcoreComparisonTests.WhenSameSceneRunsThenOriginalAndCandidatePixelsMatch(string scene)`，DataRow `scene="media"`。`NativeRun.cs:82–119` 当前每行循环 default/false/true；CompareSceneAsync 先启动 official 再 candidate，但到 candidate返回后才断言official，**不满足官方失败先停**。不能整FQN误跑8场景，不能把一个media行称official1/candidate1。

可直接调用的既存角色是正式exe参数 `--pcore original media default` 与 `--pcore candidate media default`；比较角色 `--pcore compare-media`。前两角色各能完整运行，不需新构建；但比较器硬读同根 Baseline/ComparisonCandidate，而角色只写 Results/original 与 Results/candidate。只有现有 CompareSceneAsync 自动复制二进制帧，当前没有已核验的零构建“单模式、先停、复制后比较”整体入口；不编造工具可复制二进制文件/设置环境，也不以分开跑后没有独立比较冒通过。

两当前clean的公开API元数据均确认 PcoreComparisonTests/方法存在；x86 clean 另以符号文本确认 RenderMedia 存在（不是方法体逐字验证、不是运行发现或UID）。未重散列clean文件。路径为 `artifacts/async-shared-ownership-x86-r1/clean` 与 `...x64-r1/clean`；aot.sha256亲读分别15A545E6…与54BC62FB…。旧报告 test身份D8A5F57F…/4A835620…只作历史引用；新wrapper会改变test身份，须重新记录。

资源前置/样本：`Pcore/MediaScenes.cs:165–210` 已用 BinaryWriter纯C#生成2个4秒RIFF AVI，2×1、24-bit BI_RGB/DIB、10fps/40帧，每行8字节（含padding）；双stream，mono 8kHz/16-bit PCM、每帧800 sample/1600bytes，原创440Hz正弦，无版权素材、无外部CLI/脚本。asset0前2秒红后2秒蓝；asset1绿后黄。按0.5/2.5秒平台采样，32×16 Pbgra32每帧2048bytes，四帧8192bytes独立SequenceEqual零阈值；新帧不能等于旧帧，且不能全黑。无需新增样本或容器生成器，已有历史系统播放实证；本次未重新认证当前系统codec可用。

可观测序列：真实MediaPlayer Opened/HasVideo+HasAudio/2×1 → Play与Position推进/seek → 两个不同decoded帧 → Pause/VideoDrawing替换保持帧 → Ended → 下一asset → 并行Opened内Close和missing asset MediaFailed内Close → 正常清理/退出。RTB是非窗口offscreen真实消费，不依赖窗口像素；不证明HW。

### B：已有非显示解码+资源更新下界（不推荐替代A）

FQN `WpfGfxShape.ComAcceptance.MediaExportsTests.WhenRealPlayerIsReboundAndSourceIsReplacedThenNotificationsDriveAllColors()`，无参，当前x86 clean公开API亲读确认存在。内部VerifySolidVideo(red,replace:true)：生成10秒100帧2×1 DIB AVI，真实Open/Rate1；同一player多次Open替换为红/绿/蓝，真实SendCommandMedia/Visual/同步software channel/系统sample → offscreen WIC输出读回；每色用独立official系统renderer取样，不是WIC静态输入或mock。WIC此处仅接收解码消费结果。

但它不是同一个流连续时间变化，且candidate先Open后才ReadOriginalPixels，不满足本次official先行；oracle仅要求两个相同非零像素，并没有独立断言返回color必等expected，存在陈旧同色oracle风险。finally执行销毁/断连/Release但未断言全部清理返回；Dispose忽略Shutdown/Release结果。无自然Ended，纯视频无音轨；所以不能以这个既存无参体当完整持续媒体门。当前可build0运行也不值得因此优先，不能冒真实音频/趋势/全闭。没有建议重跑其它ownership/ref0或无帧SystemMediaSampleTest补数量。

## 5. 同SHA与历史其他SHA分账

当前15A545/54BC62的postfailure/isolation/ownership/directClose/KN有限绿保留，已读当前矩阵未给当前SHA持续媒体业务通过。4199托管Debug里有synthetic媒体单元，但未执行ComAcceptance，不能补当前Native媒体格。

历史 `completed-work/PresentationCore-media-E2E08-finite-closeout.md` 明确 x64 candidate9FDFE101…、official94C06484…、WindowsDesktop10.0.1；当时公开媒体四帧全字节8192bytes差分、Opened/Ended/Failed/Position/HasAudio、双模块服务与候选合成以及有限Close有实证。该报告还含现已不允许的新第三工程历史，不据此恢复该工程；今天代码已在正式ComAcceptance Pcore。历史结论不移绑15A545/54BC62，不认证10.0.13或所有版本。本次只读历史报告，不冒亲读其全部原始task/TRX。

## 6. 唯一下一门具体实施/预算/停止设计（本阶段不实施）

最小范围：只改 `Tests/WpfGfxShape.ComAcceptance/NativeRun.cs` 的 PcoreComparisonTests，新增有XML说明的无参TestMethod，建议FQN `WpfGfxShape.ComAcceptance.PcoreComparisonTests.WhenContinuousMediaRunsOnceThenOriginalAndCandidateFramesMatch()`，只调用现有CompareSceneAsync("media","default")。在helper内把official断言移到candidate启动之前，保留candidate断言、帧复制和CompareMedia零阈值；检查sibling同helper一并正确首错停。不复制MediaScenes，不改生产/算法/样本/expected/模式旧矩阵，不添加新字符串或库。如实际工程必须显式隔离输出，则只增同csproj一个专用x86 Release等价配置块，固定已有Native路径与ignored输出目录；不修改SDK/TFM/全局runtime。优先复用既有发布属性，配置块非无条件必需。

构建发布：先冻结diff/原参数与依赖图；normalbuild≤1，COMpublish≤1（明确no-build，不能隐含第二次build）；既存x86 Native15A545不重发。新COM test SHA/MVID、配置/RID/实际runtime及官方模块、生产候选actual重新认证，清单不是实际加载证明。官方必须是已加载PresentationCore同目录同架构，候选另有systemMediaServiceModule允许项，不能把第二官方模块错当候选业务或静默递归。配置/工具不能保证上述合同则停止，不擅自追加重建。

执行：通过正式exe监督入口精确无参FQN一次；父测试调用现有RoleAsync依次official1 → 成功才candidate1 → 成功才comparator1。角色每次既有3分钟deadline不延长；监督器filtered最大120秒，因此本门建议监督120秒、外层沙箱180秒（正常场景约数十秒，达上限即失败/停止，不据既有role3分钟额外延长）。官方失败、缺媒体组件、MediaFailed非预期、15秒条件失败、空/旧帧、actual身份不符、比较失败、cleanup/退出失败任一即停止；不补跑、不换codec、不改阈值、不扩三模式/x64/full。单沙箱≤1/正式测试体≤1，内部业务角色3个另列，不能把它们隐瞒成单业务执行；comparator不是额外official/candidate。

必须归档TRX实际非零体且零skip、监督exit/timeout、两角色result.json、四帧各原件及输入AVI/hash、comparison.json；确认8192bytes的exact差分、各角色Opened2/Ended2/预期Failed1、前后帧变化、Close/Dispatcher收尾及actual模块身份。cleanup字符串不是逐COM/系统线程回收证明，未新增的观测明确Unknown。若只拿到角色Green而比较未达，不通过。

本门成功仅当前x86/default公开持续媒体有限内容/生命周期证据，不称媒体全闭、实际音频输出、长期资源趋势、所有callback竞态、HW/loss/显示/完整替换通过。当前方案已明确有效媒体在范围内，无需用户再选“支持媒体还是私有player”；需要的只是另授权这一已具体设计的最小实现与执行预算，不再接力无理由选门审查。
