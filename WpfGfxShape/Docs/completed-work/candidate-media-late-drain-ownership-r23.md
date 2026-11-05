# r23：媒体关闭晚 drain 最终责任与系统合同核对

## 结果与决定
条件性失去可执行清理责任可以由当前源码静态确证；不是“全部强引用立即丢失”，也没有动态证明本次发生泄漏。未实施生产修复：失败 Flush 没有文档化的完成/重试收敛保证，而把原前台 MTA 永久留作等待者会违背不得无限 reaper 的要求。成功 Flush 后的晚 drain 可以独立设计回收，但不能将其冒充失败 Flush 的完整解决方案。OverallOpen。

本轮推进是核对系统合同、复核完整引用/线程责任、实际运行相关33项测试并限定下一实验；不是重新认证 r22，也不是宣布资源故障已修复。生产/测试源码修改均0，仅新增本文、测试收据并更新恢复入口。没有新项目、生产状态/开关/钩子、终端/脚本、原版 gfx 加载/复制 fallback；没有改 async 默认值、oracle、expected、阈值、DPI 或 flags。

## 当前源码中的确切根因
以下路径均相对 Code/WpfGfxShape/Core/Av，行号对应本轮读取的工作树。

1. MediaFoundationSample.cs 692–730：SourceReader.Close 先 CheckThread，再 RequestClose/BeginFlush；负 FlushResult 在 reader.Release、callback.ReleaseOwner、MFShutdown、CoUninitialize 之前返回。成功 Flush 但 WaitDrain(10秒)失败也在相同位置返回。FlushResult 缓存负值，因此即使在原线程再次 Close 也不会重新发 Flush。成功 Flush 的超时则理论上可由原线程再次 Close 完成，但没有生产调用路径。
2. IndependentVideoPlaybackOwner.cs 252–335：CloseAsync 固定返回 _completion；finally 先 ClosedReader=_prepared，然后 Dispose，异常汇总到 _closed，最后清 _prepared/_pending、释放 Frames/命令队列/事件并退出创建 MTA。ClosedReader 确实强持 reader，故“清 _prepared 当场完全丢失对象”错误。真正缺口是 Dispose 未安全终结也无条件销毁唯一合法执行器。WaitForExitAsync 的 finally Join 还把关闭结果与线程退出耦合。
3. MediaFoundationReaderCallback.cs 35–42、76–149、180–255：native identity 的 GCHandle 强根 callback；callback owner 引用初始为1，reader/native 可能另持引用。OnReadSample 对借用 sample 显式 AddRef，再在 closing/flushing 分支释放；Leave 只减 active、完成 flush Task 和唤醒事件，OnFlush 只标 flushed。callback 无 reader/owner 回指、无最终释放调度；其强根不是清理 owner。OnEvent 也计入 active，但 Flush 的公开文字不是“永久禁止全部未来 OnEvent”。
4. IndependentMediaIdentity.cs 381–443：BeginClose 将 _owner 置 null；CompleteCloseAsync 只汇总 owner.CloseAsync/pending/events。故身份对象不是 ClosedReader 的持久回收注册表。208–236 的失败路径虽等待 Close 并汇总清理异常，也不会恢复原 MTA。
5. CancellableVideoReaderOwner.cs 110–143 同族：Close 缓存同一 _close，失败也 CompleteAdding，Run 退出；FinishCloseAsync 先 await cleanup，错误时跳过后续 exit/Join。保留 _reader 不等于有可执行重试路径。
6. MediaFoundationSample.cs 350–366：Create 尚未成功转移 out owner 时，在 finally pending.Dispose；若已得到 native reader 而后续协商失败，此 Dispose 也可能走相同保留分支。只改 playback finally 会漏掉此责任入口。

因此静态可证的是：一旦这两种早返回发生，已有生产控制流不会在晚到完成后再执行 reader/callback Release 和 MF/COM 配对；不是证明系统永远不再释放任何内部引用，也不能量化资源泄漏。没有发现已有安全重试路径。读者原始 COM 指针不是托管 RCW 自动清理保证，callback GCHandle 更不会代替 reader/MF/COM 终结。

## 本轮核对的 Microsoft 公开合同
以下网页由网页读取直接返回正文；不是 SDK 实现反编译或对所有 Windows 版本的证明。

- [IMFSourceReader::Flush](https://learn.microsoft.com/en-us/windows/win32/api/mfreadwrite/nf-mfreadwrite-imfsourcereader-flush)：丢弃队列样本、取消 pending sample requests；提供 callback 时异步，完成时调用 OnFlush，pending 时 ReadSample 返回 MF_E_NOTACCEPTING。失败仅说返回 HRESULT，没有承诺失败调用也必发 OnFlush、没有有限完成时限、没有失败后重试必成功保证。另明确 Windows 7 曾有 OnFlush 早于真正完成的缺陷，不应把该老平台无条件纳入新证明。
- [OnFlush](https://learn.microsoft.com/en-us/windows/win32/api/mfreadwrite/nf-mfreadwrite-imfsourcereadercallback-onflush)：完成 Flush 时调用，ALL_STREAMS 可作参数；返回 HRESULT 当前被 reader 忽略。不能用返回错误触发系统重试。
- [异步 Source Reader](https://learn.microsoft.com/en-us/windows/win32/medfound/using-the-source-reader-in-asynchronous-mode)：callback 必须线程安全，系统 worker 调用，可只通知应用线程。官方示例在错误/EOS 后 SafeRelease reader/callback，没有先 Flush；所以不能声称“COM 规范要求所有 reader Release 必须有成功 Flush”。但该示例不证明本项目任意 active callback 与 MFShutdown 并发安全，也不给出非收敛 source 的 MFShutdown 时限。
- [OnReadSample](https://learn.microsoft.com/en-us/windows/win32/api/mfreadwrite/nf-mfreadwrite-imfsourcereadercallback-onreadsample)、[OnEvent](https://learn.microsoft.com/en-us/windows/win32/api/mfreadwrite/nf-mfreadwrite-imfsourcereadercallback-onevent)：回调返回 HRESULT 当前被忽略；sample 可为空。active==0 是本地瞬时条件，不独自证明没有未来回调；OnFlush 是样本 flush 完成通知，不是所有事件生命周期的永久栅栏。
- [MFShutdown](https://learn.microsoft.com/en-us/windows/win32/api/mfapi/nf-mfapi-mfshutdown)：每次 MFStartup 配一次，不能从 MF work queue 线程调用。不能在 OnFlush/最后 Leave 内直接做最终 MFShutdown。
- [CoUninitialize](https://learn.microsoft.com/en-us/windows/win32/api/combaseapi/nf-combaseapi-couninitialize)：在当前线程为每次成功的 CoInitialize/Ex 配对（包括 S_FALSE）。本项目还有 CheckThread 限制；任意 Task.Run 线程不能替代创建 MTA 的 CoUninitialize。
- [GetServiceForStream](https://learn.microsoft.com/en-us/windows/win32/api/mfreadwrite/nf-mfreadwrite-imfsourcereader-getserviceforstream) 可查询底层 source，调用者需释放返回接口，但不保证能获取可取消 source，不给出 reader Flush 失败后的关停时限。未据此新增强制 source.Shutdown 生产分支。

补充：using-the-source-reader-to-process-media-data 页面读取404；没有将未返回内容当合同证据。其余8页正文成功返回。网页读取不是本地测试。

## 最小安全实现边界与未实施原因
可接受的成功 Flush 晚完成分支应在原 MTA 保留 reader 与初始化责任，callback 仅完成通知，由原 MTA 检查完成条件、最终释放 reader、释放 owner callback 引用、配对 MFShutdown/CoUninitialize，一次且仅一次；不得由 callback 调用关闭。公开关闭错误与最终清理完成必须分离，否则现有 _completion 的 Join 使公开 Close 无界等待。原队列/唤醒对象不得先销毁；现有 ClosedReader 诊断字段不足以承担这项职责。

但这个局部实现仍有三个不能隐藏的边界：
- 成功 Flush 也没有最大晚完成时限；永久不回调时，保留原前台线程会影响进程退出。改成后台线程仅隐藏退出问题，不是完成回收；增加超时后 Release 仍用 COM 违反本任务安全限制。
- 失败 Flush 的缓存应区分历史错误与后续完成，但删缓存/循环重试并无收敛合同。有限重试耗尽后仍需明确隔离资源/线程所有权；无限重试或静默丢弃责任均不成立。
- 若公开 Close 先报告原错误、随后回收又失败，必须有既有可观察错误通路。将其塞入无人 await 的 Task/仅添加诊断字段不够；不能为了测试引入新的生产状态或改变公开 Close 语义。

结论不是“不可修复”，而是上述要求下不能仅根据现有网页合同安全交付涵盖永不完成 source 的通用进程内回收器。本轮不以扩展生产关闭状态机试错，也不删早返回强制释放。允许 source 非收敛的情形下，“有限返回、有限回收、绝不释放活动对象”三者不能同时保证；需要经证据缩小到成功 Flush 的可完成范围，或另行定义非收敛隔离合同，不能暗改默认行为。

## 实际测试与执行账
专用测试调用目标 Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj，筛选：FullyQualifiedName~MediaFoundationResourceTests|FullyQualifiedName~MediaFoundationCallbackEpochTests|FullyQualifiedName~IndependentVideoPlaybackOwnerTests。

工具记录：dotnet test，退出码0；失败0、通过33、跳过0、总33，持续5秒，net10.0。构建输出为 artifacts/bin/WpfGfxShape/Debug/net10.0/wpfgfx_cor3.dll 与 artifacts/bin/WpfGfxShape.Tests/Debug/net10.0/WpfGfxShape.Tests.dll。这是托管测试运行，不是新 NativeAOT DLL 认证。日志包含 CA1416 警告，不能称零警告。工具输出原616032字符，前596032字符未返回；最后日志62行已读取，不冒称截断部分已审阅。摘录收据见 candidate-media-late-drain-ownership-r23-test-receipt.txt。

本轮独立 build0、run_tests1成功（隐含构建）、publish0、sandbox0；没有新源码冻结/Native发布/候选业务执行。没有“生产改了但没测”的状态。没有新二进制SHA/MVID认证，不把测试程序集路径冒充产物身份。

已有 admitted 测试通过现存 ReadCallbackAdmitted 暂停点观察 callback，不是公开外部故障驱动；原测试在10秒内释放，不可靠触发10秒后的 drain timeout，本轮原样运行未修改断言。没有新增或延长生产钩子。33项通过不能证明内部 Flush 失败/晚 drain/永不回调已被触发；这三类动态覆盖均 Unknown。

## r22 原件和身份保持独立
先读 r22 完整报告及 r21 资源审查；r22 原件根实际为 WpfGfxShape/artifacts/candidate-media-source-bound-r22（相对本项目根为 artifacts/...，不是外层 workspace/artifacts）。本轮一次查询外层 artifacts 路径失败，未将其解释为 r22 原件丢失。本轮未重新读取四业务全部原件或重新 hash DLL，以下身份是 r22 报告记载，不是新认证：

- 源冻结814文件，manifest SHA256 8B109FD9BBCA919FCD15B49F27DC7E87671F4712B249158DB31A7B72F06539A6。
- x64 Native A9B9F69FC56E48072B9FA05B44D526FAC03287A4C4C61A766C464AEBEC1F639E，4895744字节/Amd64；x86 93405EB4A14F50CB556972F19D749B2580302A0F70BBE1342763215C83B4F9F4，4333056字节/I386。
- recovery x64/x86 task 0e07c8edbfbe423297fcd1c46bc73808 / 2c6edf6e76f9402ca81a53ad8ea8222e；video x64/x86 task 045abfd96b4946bb9d09e2b0700ad52c / 7efc2c0160714330992a4b552edd3ebc。各自 Evidence 下 candidate-raw/result.json、candidate-video-process.json、supervisor-exit-probe.json、负控与 package identity 是原件；harness SHA/MVID 精确表仍见 r22，不混包。

生产未修改，因此没有新的 x86 生产版本需要认证；r22双架构仍仅认证其冻结源/产物。若随后改任何生产，r22双架构对于该新版本立即过期，不只 x86。不得将本轮33项测试移绑到旧 Native 或给未来改版背书。

## 有限下一可执行实验（不是再跑一次 r22）
1. 限定一次独立 Windows sandbox 合同实验，先不改生产：直接使用公开 MF COM API 和真实系统 reader，输入外部可控 IMFByteStream/媒体源依赖；依赖仅控制 I/O 完成/取消，不模拟本项目 reader/owner。以有效有限视频作为正常对照。若现有 FromURL 入口无法接该依赖，实验仅独立验证 OS 合同，不先向生产添加注入入口。
2. 仅设两组外部行为：正常完成、取消返回外部 I/O 错误后明确结束所有外部工作。记录 Flush HRESULT、每个 callback 进入/离开、OnFlush stream、reader/callback 引用释放和 MFShutdown/CoUninitialize线程/次数。测试 driver 自己的受控 COM 依赖引用计数可测，不把系统内部 refs 全部可见化。
3. 每子进程固定预算例如30秒，父进程到期杀子并保留 stdout/stderr/时间线/退出收据。杀子只是实验隔离，不是生产回收方案。若未得到负 Flush 或晚于10秒完成，结论必须是“未触发”，结束该实验，不靠无限循环/扩大等待/新生产钩子碰运气。公开失败URL和缺文件不能替代这个条件。
4. 只有实验/更明确系统合同提供可验证停止保证后，实施一个限定分支；若只证成成功 Flush 的晚完成，只修该分支并显式保留失败 Flush 的未解决边界。同步处理 playback owner、cancellable owner、Create失败转交与公开错误/Join关系，不能仅改 finally 字段。
5. 如改生产，先运行本轮相关测试且保留原断言；再以既有 CurrentSourceAuditR15 冻结新源并保留逐字发布收据，发布新 x64 Native，把它置入现有 CandidatePureVideo 与 CandidateMediaFailureR16/监督包，跑正常视频/恢复及原三负控，读回真实产物身份/父子收据/像素与 stderr。x86 若未同批完成，明确新版未认证，下一任务以同一冻结源发布 win-x86并执行相同两业务与负控；禁止沿用 r22 绿灯。

## 不得外推的门
full 仍缺可信 original-wpf.json / glyph-arial-original.json；桌面原 nativeControl 失败并停止捕获派生。内部 Flush 失败、晚 drain、永不回调、source端pending/URL强取消、最终COMrefs0 均未获动态证明。不能称整体通过。本文与恢复入口写后需回读；最终交付以回读内容为准。
