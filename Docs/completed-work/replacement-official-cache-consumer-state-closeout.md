# Official cache consumer state 收尾

## 正式终态：符号完成；消费 UnknownNotRun；整体 Open
本次亲读恢复入口顶部、current-execution-rules 全文、基线/工作项顶部、预注册全文、recovery-r1 两 receipt 全文、r1 preparation 1–12、r2 preparation 1–15，以及最后构建日志1–12/48–64。长文件工具截断部分及前代理阅读范围不冒本次亲读。以下替代原草稿的当前状态，不删除历史失败。

## 全部已执行账（分轮，不挪用预算）
- 最早断点尝试：normalbuild1，MSB3030 历史 PDB Content 路径缺失；撤回断点实现，FailedBuildNotRun。来自 execution-receipt 历史摘要，非本次原始构建回执。
- recovery-r1：normalbuild2、COMpublish1（implicitcompile1）、sandbox1。build exit0，150/148 warnings；stdout117487/115911字符，分别前97487/95911未显示。publish exit0及参数见 artifacts/cache-consumer-recovery-r1/execution-receipt.md，本次亲读摘要，不冒亲读旧构建原始输出。sandbox task148925640a3f4ef3864e391e9db25b6e：下载HTTP200，PDB匹配，但旧代码提前拒绝SymType5延迟加载；SymCleanup=true，原件无ApplicationExit。BusinessRuns0/Breakpoints0，UnknownNotRun。
- 接续 recovery-r2：normalbuild2、COMpublish2（implicitcompile2）、sandbox1。第一 build 配置CacheConsumerSymbolPreparation，exit0/148warnings/0errors/3.66秒，stdout115911、前95911未显示，来自 continuation-receipt。publish1漏clean/inputs，确定修正仅诊断配置打包白名单；publish2/沙箱原工具完整调用与原return未持久化到现有receipt，不重构为亲读原回执。成功产物及application信号见下；不能将ApplicationExit等同独立OSexit。
- 最后 normalbuild2：本次补读最后日志1–12、48–64。目标 WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj；Release/win-x64/net10.0-windows；原日志exit0，146warnings/0errors，3.10秒。stdout114542字符，仅尾20000，前94542未显示；未声称全文警告审计。默认Release编译成功，不是业务验收。
- 本次子智能体新增 normalbuild0/2、COMpublish0/1、sandbox0/1、Native0、生产0、full0；只回读与文档收尾。历史合计（含最早失败）normalbuild5、COMpublish3、sandbox2，不把publish隐式compile再次算normalbuild。

## 已认证符号与身份（准备身份不是业务模块加载身份）
r2/readback/Results/cache-consumer-symbol-preparation/preparation.txt 全文：task3a07842867fd47b2a152aff0aeb9e499，PID12016，fixture MVIDbf1ad759-7aa3-4808-bc33-faf60158be26，X64/.NET10.0.13，Desktop10.0.13-servicing.26504.104。candidate SHA256=6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403（输入预检）；official image AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1。PDB SHA732FB582AF469CA52FD3B1C39520FFAD892EE6E9FAFDA1BC69A6066EF6212582，20287488 bytes，GUID8756f755-10f4-48be-9886-22c9a80c75b8 age1；MSF/RSDS匹配。延迟加载后SymType3/PdbUnmatchedfalse；GetRenderTargetBitmap RVA10DDE0 size571，Update RVA10D5C0 size2079；SymCleanuptrue/ApplicationExit0/NetworkRequests0。符号阶段结束，不再下载或重跑准备。

## 六状态与监督边界
|项|正式状态|依据/边界|
|构建|ExecutedPassed|最后Release exit0，非零警告保留，截断明示|
|符号身份|ExecutedPassed|匹配PDB及两个ExactFunction，非返回指令/局部变量位置证明|
|原official无参业务|NotRun|本轮BusinessRuns0；无ClassInitialize/TRX/像素或HWND业务现场|
|消费状态|UnknownNotRun|Breakpoints0；不是cacheNotReached、null bitmap或Update未调用|
|清理与监督|Limited|符号SymCleanuptrue/ApplicationExit0；无业务desktop/job/TRX，独立OSexit Unknown，不冒全部cleanup|
|完整替换|Open|无新产品Passed，所有历史失败保留|

## 保留历史
controlled full x64 1823/1822/1、x86 1823/1821/2；默认latest x64 1823/1821/2、x86 1823/1820/3，历史42/23/40及所有旧红独立保留。MemoryDescriptiveOnly / memoryGreen=null。expected00563412/FF80不变；无Sleep、poll色、额外flush、disableparallel、skip或fallback。安全决策与直接产品剩余实现项见以下正式补充（不以符号完成冒完整替换）。

## 最终安全裁决：停止本次断点方向，不交接entry/return框架
亲读 SamplerNativeCrash.cs 1–68（后部截断）、70–180：现Method锁定历史sampler，candidate硬绑632FC243而非6BDF；CreateProcessW使用DEBUG_PROCESS，自建子进程；仅每PID首个first-chance breakpoint被Continue，其余异常NOT_HANDLED。CacheConsumer.cs全文为撤回占位。这里不是现成可消费业务断点的框架。

优先审查单次内部返回站点：本地VisualCache.cpp 674–759明确HRESULT返回、成功路径*ppIRTB=m_pIRenderTargetBitmap并AddRef；但该仓库文本不是AD1C机器指令证明。r2只有函数起点和size，没有认证ret/共享epilogue边界、RDX的实际保存寄存器/栈槽、outptr存活区间或调用者局部位置。不能用RVA+size-1猜ret；RCX/RDX/R8是entry ABI，不能套在函数末端。unwind可恢复调用帧/非易失寄存器，不自动恢复已被覆盖的volatile RDX，也不能凭home-space猜outptr。即使ret断点暂停了线程，无outptr位置证明也读不到合法输出。

安全最低要求还包括：认证命中线程与自有PID/image、原字节保存/校验/恢复、FlushInstructionCache、正确回退IP、退出/异常的恢复分支；单次断点必须明确恢复后不重装并处理其他线程待处理命中，循环断点则需单步与暂停/恢复其他线程及递归所有权。现有首断点吞异常逻辑不能代替这些要求。无证据支持在授权最小改动中安全闭合，故不写内存、不设业务断点、不运行原case，不以工具限制为借口，也不宣称理论上永远不可行。**此方向终止；下一工作项不是实现entry/return框架，也不是重跑符号准备。**

## 产品能力实证与边界（本次回读，不新增运行）
亲读public-cache-contract/packet-fix-{x64,x86}-candidate-readback/Results/Results/candidate/result.json各1–42、250–427：
- x64 task83146a2ab92e4a498c2e31767f23cfcf，actualNativeModules=6BDF完整SHA；x86 task973754123a164ba489c541d1dfce0948，actual=B07C9073F75278ED61C44638EB3644547B413A281E2859B25211CEAAC3D1466C。均.NET10.0.13。
- 每arch visual绿色/红色与BitmapCacheBrush绿色/红色四帧，均512/512，stagecompleted/statusGreen/exitCode0；cacheSceneDetachedtrue，记录DrawingContext释放/引用释放/Dispatcher shutdown/finalizers drained，模块驻留。comparisonStatus仍PendingIndependentComparator，不冒独立比较已执行或OSexit。
- 明确consumer=Public RenderTargetBitmap; not original HAL HWND consumer。两架构公共缓存正确，不等HWtargetbackend/原HAL HWND/shader/resize通过。官方对照四帧由本次亲读receipt摘要支持，官方JSON本次未再次亲读。
- 亲读controlled-full正式报告1–36（尾被截断）：同Native主要功能已有大范围通过，x64 1822、x86 1821，但失败与覆盖边界不消失；本次不冒重新审计1823条TRX。真实产品loss不能由独立d3d9控制loss代替；audio与自然GC长期仍无实证。

## 确切未实现能力：不依赖官方wpfgfx的媒体创建链
亲读MediaExports.cs 1–66（尾截断）及SystemMediaModule.cs 1–80（末行截断）：MILFactoryCreateMediaPlayer直接调用SystemMediaSource.Create；SystemMediaModule.Acquire要求PresentationCore模块，取其同目录wpfgfx_cor3.dll，比较文件实体，若与自身相同直接抛DllNotFoundException，否则NativeLibrary.Load官方文件并进程驻留。**当前没有自主媒体后端的闭包：若真正就地替换使该路径等于候选，媒体创建不能靠这条链成功；若保留官方副本，则是依赖官方服务的替换，而不是消除官方wpfgfx依赖。**普通OS媒体组件依赖与回载被替换wpfgfx不是一回事。

既有real-media-resource-contract-review允许将其作为系统媒体服务委托，有历史真实视频通过；本次不倒写为所有媒体都是stub，也不擅自扩大成独立codec工程。完整替换若要求部署不保留官方wpfgfx，这一实现边界与目标存在明确缺口；若正式允许该外部服务，则必须在最终部署合同明列，不能宣称自包含媒体迁移已实现。当前不改生产，保留此前批准的依赖边界。

定向查Abi中E_NOTIMPL只找到NativeAotAbiProbe未知operation分支，亲读60–91，属测试探针，不当产品缺失。Av定向命中StateThread.cs36为非Windows guard、VideoSlave.cs85为_composition为null的拒绝；已亲读上下文，不能据此伪造Windows有效媒体stub。未泛扫全树或修改这些错误分支。

## 下一直接主要实现：媒体后端替代与音频可执行验收，而非再选诊断门
1. 生产实现候选（需确认后才改生产）：在现有Core/Av及MediaExports边界，用Windows媒体服务承接创建/Open/Play/Position/Rate/Volume/Balance/Shutdown、异步事件和真实视频source；替换SystemMediaSource对官方MILCreateFactory/MILCreateEventProxy/MILFactoryCreateMediaPlayer的依赖。复用现有事件proxy、GeneratedMediaPlayerResource消费与lease释放顺序，不新增Host、不伪造私有CMilSlaveVideo指针、不改已有导出ABI，不把第三方高层播放器包装当无差分等价迁移。先交付可播放本地AVI/DIB/PCM的完整生命周期及失败回收，不启动通用decoder平台。此为明确生产缺口的直接实现建议，不是本轮授权执行。
2. 如保留官方服务是最终认可契约，则不要无谓重写decoder；直接实现现ComAcceptance内的**实际音频输出验收**。本次亲读Pcore/MediaScenes.cs1–58：Volume=0、只查HasAudio及RTB视频像素，因此它无法验证音量/平衡/静音/音轨内容。实现C# WASAPI render端loopback事件驱动采集，使用已知PCM左右声道的现有内存生成素材，独立official先行→candidate，按音频格式/采样位置核信号、volume/balance变化与stop/close后收敛；不要求像素时钟相等，不把HasAudio当声音，不加Sleep或poll颜色。设备/loopback不可用必须Blocked/NotRun，不是假Passed。只用现有测试项目，不新增微ABI门。
3. 实际主要验收剩余清单：两arch真实WPF HAL HWND cache+shader+resize与backend身份；由产品自身消费触发并恢复的loss；上述音频内容输出；无强制GC的长期对象/资源/线程/callback收敛；最终源码后双arch正式Release适用full。每项需实际模块身份、原TRX/业务结果/监督/cleanup闭包；公开RTB、控制device、静音视频与短期cleanup各自只认有限证据。

本轮已经完成诊断收尾与上述能力判别，不再把显示缺信号阻塞所有产品工作。未声称完成生产实现或新增产品Passed，OverallOpen。
