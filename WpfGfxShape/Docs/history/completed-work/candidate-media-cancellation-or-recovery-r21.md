# r21：公开媒体失败恢复独立复核与有限资源审查

## 结论
公开失败恢复在既有双架构 NativeAOT 候选上有限通过；本轮独立读取原件，不是重跑。OverallOpen。先创建本文、更新 handoff 并回读后才进行所有权审查。未修改生产或测试代码。

原件根 `artifacts/candidate-media-recovery-r21/`，以下相对路径主要指 `supervised-{x64,x86}/Evidence/`。已亲读 x86 result 尾、双架构失败恢复段/身份/actualNative/计数、三负控关键段与过程收据、父子过程、父 stdout/stderr、业务 stdout/stderr；审计附件为定点读取，不宣称两份五万余行 PE 审计和两万余行 runtime 审计全文读完，也不宣称重新 hash 本机二进制。

## 实际身份（不能只保留摘要）
|项|x64|x86|
|---|---|---|
|Native SHA256|C49AF75D50341E18FBB1023BCEF9672114E662A3EE96E676858A205C6A4A9B7A|A513DD3446D179546873901866E8E9BDD15D08430E15BA08DC06B56A20DEDEE1|
|Native bytes / PE|4895744 / Amd64、managed=false|4333056 / I386、managed=false|
|CandidatePureVideo.dll SHA256|9B58F6595374CEA10521F37055B6CE40F8DCB57FDC74688706678DF72A8151C8|1FAE85B83DC94656BC4691F82030E5DBACC3240576C05438742467AEE59161EB|
|harness bytes|33280|33792|
|harness MVID|fc943c2d-890e-46bf-a554-65164fdf0911|81b86d0f-1ade-4771-b5eb-e68b92d9136f|
|PresentationCore SHA256|0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31|D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D|

证据：candidate-raw/result.json，package-identity.json；runtime-audit.json Native条目 x64行20626/x86行20622；pe-initialization-audit.json 行54604/54516，runtime-summary.json。Package/aot.sha256 与 actualNative、candidate、package identity 相同。实际模块均为对应 task/Package/Native/wpfgfx_cor3.dll，basename handle matches=true。PresentationCore Version10.0.0.0 的 wpfgfx 请求 flags2050、selected 指向该候选；没有把纯托管 r20 SHA 移绑到本次 Native。

Program.cs 35–44 在 NativeLibrary.Load 前先检查摘要、PE机器、PEHeader非空及 CorHeader为空；成功业务到 purevideo-completed。因此是原运行 PE/SHA 验证与收据的独立交叉复核，不是本轮重新计算文件。runtime-audit 的 imports=[] 是其结构化扫描字段，不解释成 Native 无 OS 导入；PE原件明确有 ADVAPI32/bcrypt/KERNEL32/ole32/USER32。x86 initialization-summary 81行分段读取，模块初始化有 DPI 查询、DWrite 加载、C++ runtime 初始化，不以字符串 containsLoadLibrary=true 断言原版gfx被加载，也不能据定点审计证明所有动态装载分支不存在。

## 精确覆盖
双架构 candidate-raw/result.json：缺文件 0x80070002、8字节无效 AVI 0xC00D3E84、http://127.0.0.1:1/r16-missing.avi 0xC00D0035；三次 MediaFailed 中调用 Close 均返回。每次后恢复有效视频，HasVideo=true/HasAudio=false/2x1/4秒，随后 playing-Close-return。FailureLifecycle.cs 原实现还要求 Position推进至至少200ms后才 Close；原件没有逐帧像素或音视频正常序列，因此只称失败后可恢复推进，不覆盖历史完整正常序列。

另三轮立即 Open/Close/Open，每轮收到替代 Opened、重复 Close 返回，最终 Close 返回。两侧 opened6/failed3/ended0、cleanupDiagnostics=[]、exitCode0。最后 Opened x64 elapsed9293ms/x86 9366ms。validAsset SHA F277087DA15898B6C96720AAEF17062A6E9D221856055CD2B51014FED323E7E4/864字节；invalidAsset 8A851FF82EE7048AD09EC3847F1DDF44944104D2CBD17EF4E3DB22C6785A0D45/8字节。

三负控两侧均 exit1、requests=[]，失败阶段分别 digest-required(FileNotFoundException)、sha-validation(InvalidDataException)、pe-architecture-validation(BadImageFormatException)。process-results 均 passed=true/failures=[]/timeout=false/reaped=true/drained=true/killError=null；不误记为业务失败。PID：x64 6232/4900/8092，x86 5480/4240/9964。不是三种媒体失败的同一个负控概念。

## 父子监督与运行分账
|架构|sandbox task（用户交接值）|父PID/exit|业务子PID/exit|
|---|---|---|---|
|x64|6cddcd95e0f34216a2adff2d2479714d|9608/0|8808/0|
|x86|ba0a7f8c0cc64bfbace854a54aaa7561|11432/0|4140/0|
父 supervisor-exit-probe.json 与子 candidate-video-process.json 均 timeout=false/reaped=true/drained=true/passed=true；子 killError=null，父收据没有 killError 字段，不补造。normal-process-results.json 实际只有三负控，不是正常业务子收据；业务必须读 candidate-video-process.json。两侧父 stdout 是三负控 JSON，父 stderr 空；业务 stdout/stderr 均空，不能据空 stdout 推断没执行。VideoRun.cs 业务预算120秒，超时 Kill(tree)，再10秒回收、10秒管道；SupervisorProbe 父预算180秒，再10秒回收、10秒管道。本次没触发超时，不证明异常 kill 路径已经实测，亦不承诺 URL同步创建硬取消。

前任务账按用户移交：publish6成功、sandbox3、生产修改0（本轮找到实际业务及监督收据，但没有六次发布工具原输出，不能独立认证六次每次参数/日志）。本轮 build0/test0/publish0/sandbox0、生产修改0，仅只读探索与两份文档编辑；没有新执行器，没有已新运行未读结果。没有终端脚本、原版gfx加载/复制fallback，不改defaultasync/oracle/expected/threshold/DPI/flags。

## 源码/发布身份追溯的真实限制
现有 WpfGfxShape.csproj 为 net10.0/Nullable enable/NativeLib Shared/PublishAot true/SelfContained true，诊断 ABI/heap 默认false；global.json SDK10.0.107、latestFeature。独立 FailureR16 项目 net10.0-windows/self-contained、PublishAot=false、固定 runtime10.0.13；双架构 Package runtimeconfig 确认 Core/Desktop10.0.13。项目默认 RID win-x86、CandidateNativeDirectory 仍指历史 r15/native-x86，但实际 x64/x86包明确用了新的r21身份，说明不能把 csproj 默认值当成六次发布实参。

旧 x64 r19 SHA E158C6277E3B3043E25A98D808C9D7B5ACB81CFE0DE8EBE5D3187B8381C1073B；旧 x86 r15/r16报告 SHA 9FE6044FCCEE5C5A3D6E9452E8FF2506D4A67716942D11732CC60611BDA3E7FE。本次不同 SHA 本身不证明生产改动，路径/发布配置、工具链及 NativeAOT 输入均可影响身份。

**未完成当前源码逐字节对应认证**：r21目录顶层和 .coding-agent 未找到本次冻结源清单或六次发布收据；当前能读 native DLL/PDB、测试源码和审计，但没有足够原始构建来源记录。现存 CurrentSourceAuditR15/Program.cs 能比较冻结源清单，但 r21 未提供其 Source/Previous 输入，不拿旧报告替代。本轮未发布全部Native以碰运气比较，也不宣称可重现构建或“零生产变更已由SHA独立证明”。当前源码清理控制流核实与旧/新Native二进制归属认证分开记账。

## 有限静态资源审查：条件性重试路径缺口，不是无条件丢引用
范围：Core/Av/MediaFoundationSample.cs Close 692–730，IndependentVideoPlaybackOwner.cs 252–335，MediaFoundationReaderCallback.cs 35–42/76–149/180–255，IndependentMediaIdentity.cs 208–236/381–443；兄弟 CancellableVideoReaderOwner.cs 110–143。

1. SourceReader.Close 在创建 MTA线程 CheckThread 后 RequestClose/BeginFlush，FlushResult<0 或 WaitDrain(10秒)失败立即返回；发生在 reader.Release、callback.ReleaseOwner、MFShutdown、CoUninitialize **之前**。这是避免仍有回调时释放COM的安全保留，不应直接删掉早返回。
2. reader持有 reader COM指针、callback、MFStartup/COM初始化状态，应由同一创建MTA的生产 owner 持有并在确认 Flush成功且 active=0 后依次释放reader、owner callback引用、MFShutdown、CoUninitialize。仍在执行的 OnReadSample 暂时持有借用sample的 AddRef，并在 closing/flushing 分支释放；不能替owner释放reader或在 MF callback线程 CoUninitialize。
3. owner finally **先执行 ClosedReader=_prepared（行315）**，Dispose异常汇总到 _closed，再清空 _prepared/_pending，释放 Frames/队列/事件并退出MTA。故“清空_prepared立即完全丢失强引用”不成立：只要owner活着，ClosedReader仍强持有reader。callback identity的 GCHandle 又强根 callback，自身COM owner引用尚未释放；native reader也可能持有callback。它们保护回调对象，不等于存在reader清理调度器。
4. callback Leave 只递减active、完成_flushCompletion并唤醒_changed；OnFlush只置_flushed。没有回调负责 reader.Release/MFShutdown/CoUninitialize，也没有指回 playback owner/ClosedReader 的清理委派。晚到回调可以使 Drained 成真，但 **不会自动恢复已退出MTA**。
5. owner重复CloseAsync只返回原 _completion；_closing永久true、队列已CompleteAdding并Dispose、线程已退出。ClosedReader虽可被同程序集查看，但 reader.Close有创建线程限制，跨线程再次调用不合法。IndependentMediaIdentity.BeginClose把_owner=null；CompleteCloseAsync只Task.WhenAll，不持久保存重试owner，不消费ClosedReader。因此条件触发后确实没有生产可执行重试路径；不能因ClosedReader仍存就宣布回收有保障，也不保证owner永久被强根。
6. Close把负 FlushResult 缓存，后续即使能再次调用仍立刻返回同一负值，不会再次发Flush；超时分支若重新进入且原Flush成功，则可再等drain，但当前owner已退出。兄弟 CancellableVideoReaderOwner保留_reader却同样缓存_close并停止命令循环，失败还会跳过FinishCloseAsync后面的exit/Join等待。这是同族静态条件缺口，不是本次运行曾触发的证据。
7. Create失败的 finally也会 pending.Dispose；若已经取得native reader且此Dispose遇同样失败，out owner仍null，仅靠局部pending，无延后回收转交。仅记录同一所有权合同的入口，不扩大修复范围。

**静态结论**：能证明“失败/超时后保留资源而失去可执行清理重试责任”的控制流根因；不能证明本次Native运行发生资源泄漏，不能量化保留资源，也不能说全部强引用即刻丢失。本次 cleanupDiagnostics=[] 只是公开Close无异常，不是内部最终COM refs0。原件明确 comFinalReferences=Unknown。

## 最小不耦合测试修复规格（仅规格，未实施）
- 让创建MTA的owner把读者作为关闭中资源保留，Dispose失败不得直接清空唯一生产责任并销毁工作线程/调度。ClosedReader诊断引用不能充当生产reaper。
- 把Close结果区分“已安全终结”和“需保留等待/重试”，只有安全终结后释放reader/callback、shutdown/uninitialize并结束MTA。错误仍向调用方报告，重复Close需有可用的清理状态，而非把故障Task当成回收完成。
- 对成功Flush但drain超时，晚到OnFlush/最后Leave仅通知原MTA，由原MTA检查drained并完成一次清理；不在callback线程直接释放/COM uninitialize。通知必须避免对已Dispose事件/队列写入。
- 对Flush返回失败，不能把缓存负值永久视作终态，也不能无条件Release；需要先明确系统MF对失败后重试/停止回调的合同，才能决定有限重试或保留隔离。永不回调时不得伪称进程内可保证回收；需显式保留并报告未收敛状态，不用强制释放换取测试通过。
- 生产不得新增测试开关、fault flag、mock耦合或新公共注入接口。现有admitted观察点能验证正常drain，但公开MediaPlayer缺文件/无效媒体/失败URL不能可靠强制内部Flush失败或10秒后的晚回调。**安全公开故障注入可行性=Unknown**；本轮不制造新执行器或故障钩子，不以理论风险大改。

## 下一真正可执行项与保留门
1. 先补齐前任务六次发布原始命令/结果及冻结生产源清单（含构建配置/SDK NativeAOT包/资源输入/诊断属性），用已有只读源审计核当前源码对应；缺失时继续标Unknown，不无故重发双架构Native。
2. 对上述Flush失败/晚drain责任规格先核系统MF合同，再在现有资源测试中确认是否有不新增生产注入的安全验证方法。没有则只交付静态缺口与Unknown，不把公开失败恢复冒充故障覆盖。修复需另项限定实施，不能只改 finally 不清空字段就宣称解决。
3. 本轮有限范围通过不外推源端pending、URL同步创建强取消、内部异常Flush/永不回调、所有竞态或COMrefs0。r20真实MF admitted/正常40帧EOS托管证据和更早真实视频完整正常序列均保留各自身份与时间，不并入r21新的正常序列验收。
4. full仍缺可信 original-wpf.json/glyph-arial-original.json；桌面nativeControl原门失败，停止捕获派生。未运行original/comparison、不替造oracle、不放宽任何门禁。
