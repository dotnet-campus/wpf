# r7 独立监督审查与本轮交接（OverallOpen）

## 当前状态

r6 两轮有限公开视频门保留通过，不代表整体 Open 闭合。本轮未修改生产、默认 async、可见性、expected、DPI、flags、容差；未加载/复制原版或使用 fallback。当前仅亲读原件和运行专用测试，新增执行账见下。不得以旧绿认证未来源码修改。

## 已亲核的原始证据

证据根 `artifacts/candidate-purevideo-r6-final/{readback,recheck}/Evidence`。

* candidate-video.json 46–56、390–397：两轮 harness SHA `057AFD4201CC4075497FBEF8252B063316C79070D12668471B6DA5BCF0932B29`，MVID `ff4cf6a1-b019-4f42-bce4-55a0a336ab92`；actualNative 均为各 task 的 Package/Native/wpfgfx_cor3.dll，SHA `2E5DC1BCC9CC7DEF60B0EE05C622954DC9BC7C2C3A57888FEE5767ED5705AFEE`，4853248 字节，nativeBasenameHandleMatches=true。此处是亲读历史运行证据，不是本轮重新计算二进制 SHA。
* candidate-video-process.json：readback 子12852、recheck 子5664 均 exit0/timeout=false/reaped=true/drained=true/killError=null/failures=[]。
* supervisor-exit-probe.json：父10812、1160 均真实 exit0/timeout=false/reaped=true/drained=true。未把外层启动器未提供的 exit 编造为0。
* candidate-raw/result.json：readback 917–978，recheck 924–985 包含 AVI 身份 F277087DA15898B6C96720AAEF17062A6E9D221856055CD2B51014FED323E7E4/864字节；连续帧0.8/2.8、中心0000FFFF/FF0000FF、paused=false；最终cycle2/opened4/ended2，4秒、2x1、video=true/audio=false。cleanupDiagnostics=[]。
* readback 775–812、recheck 782–819：第二EOS后cycle0/1/2各有Close-Open-return、Opened，最终Close-return、Window-close IsDisposed=true。
* PureVideoPublic.cs 135–145：Close与Open确实相邻，中间无I/O/等待；事件保存位于Open之后。该测试仅在各等待检查点认证opened/ended，不是任意跨epoch交错证明。

## 生产静态审查的精确结论

`IndependentMediaIdentity.cs` Notify 66–77、ObserveOpenAsync 187–202、OnFrame 294–307、BeginClose 381–410：owner检查确实在identity锁内，但RaiseEvent/NotifyFrame在锁外；Close先分离owner，稍后锁外Dispose旧proxy。因此存在检查成功后发生Close/Open再进入旧proxy admission的调度窗口，不能写成“身份检查已排除全部旧通知”。新旧proxy实例不同，旧操作不读取新proxy，但底层registered EventProxy仍可相同。

`MediaEventProxy.cs` 105–140 明确admission边界：Run在proxy锁内增加callbacksInFlight；Dispose取消未admission项目，已admission允许完成。串行StateThread只串行队列，不与所有Close调用构成跨epoch原子边界。不能通过把客户端回调/Join搬到identity锁内解决，否则回调Close/Release有自锁风险。需要区分合同允许的已admission结束与Close分离后新admission；当前后者仍未确定性公开consumer测试。

`SystemMediaSource.NotifyFrame`只调用无帧参数的失效通知；`GeneratedMediaPlayerResource.OnSystemEvent`检查listener身份/释放状态，再标记pending；AcquireSystemFrame调用当前identity.AcquireFrame。故迟到失效提示不直接携带旧帧。它仍可能产生跨epoch冗余通知；不能将此视为旧帧已被动态排除的证明。

无验收耦合的检查范围：亲读identity完整相关Close/Open/通知路径，Av目录正则检索Acceptance/candidate-purevideo/reopen-r6/TestHook/TestMode只发现owner既存PlayAsync兼容入口注释；未见Close/Open依赖caseId、验收计数、环境测试开关或测试包路径。新增EvidenceAudit没有生产引用。未把这一有限静态检查夸大为全仓依赖审计。

`BeginClose`通过Task.WhenAll累积_operation/_exitEvents/_cleanup，`CompleteCloseAsync`等待owner关闭、pending及events完成，旧清理不覆盖新State。最终Release移除registry并terminal Close，FreeIdentityAsync在finally释放identity内存，保障await故障时仍走Free。

但`_releaseCompletion`只赋值，当前Av目录无读取；任务故障不能因“字段保存了Task”就声称被日志/上层观察。async await WhenAll会等所有任务，但其向外重抛不能当作完整inner failures清单。owner.Run finally中的reader/Frames/queue/event释放在同一try中，前一Dispose抛出可跳过后续释放；只设置_closed异常不能证明后续资源已释放。尚无可控公开API故障注入路径，未据理论窗口大改生产；这些是精确未闭合边界，不是本轮已复现缺陷或模拟通过。

## 测试原件与本轮执行账

历史“85+2”尚未认证：本轮在WpfGfxShape内按名称查找.trx没有命中，不能把r6叙述当测试原件。本轮新专用run_tests第1次成功：项目Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj，筛选FullyQualifiedName~IndependentMedia|FullyQualifiedName~IndependentVideoPlaybackOwner|FullyQualifiedName~MediaEventProxy|FullyQualifiedName~SystemMediaSource；真实输出83通过/0失败/0跳过，53ms。它不是历史85，不是Native AOT consumer，也不是故障注入证明。

本轮最终执行账：run_tests2，均exit0（83项与精确2项，不称历史85+2）；专用publish1成功（仅只读EvidenceAudit）；sandbox1（仅原始证据回读）；独立build0；生产Native publish0；新真实WPF视频0；original/compare/full/resize consumer0。未提供外层sandbox进程数值exit，因此只按输出raw-audit.json的passed=true记录核验成功，不编造该exit。测试工具完整输出摘录已保存artifacts/r7-review-test-receipts.md。

## 原件字节复核及测试补充

新增只读核验器源码artifacts/r7-evidence-audit-src，专用publish到artifacts/r7-evidence-audit，Windows执行回读artifacts/r7-evidence-audit-readback/Evidence/raw-audit.json，passed=true、两轮failures=[]。实际读取每轮四份Pbgra32均2048字节：红色SHA `1D6F3E7946101E19E458A63004805269FA94526B51BB8DA2FACDDF08F32A1579`，蓝色SHA `243565B2512B60233EF3EC38459C55D387445F31A42D9D9FE98BAF793A1EED00`；中心字节与result一致，时钟0.8/2.8，连续paused=false，AVI实际SHA/864字节/RIFF与AVI签名相符。此辅助只校验历史原件一致性，绝非重新执行consumer或AVI独立解码证明。

亲读两轮normal-process-results.json：三负控均exit1/passed=true/failures=[]，timeout=false/reaped=true/drained=true/killError=null。allWpfgfxModules（readback997、recheck1004）都只有Package/Native下上述唯一candidate身份。

新第二次run_tests精确执行WhenClosedSourceReopensThenSameProxyReceivesOpenedAndFrames与WhenMissingOpenFailsThenCallbackCanCloseAndRelease，2通过/0失败/0跳过，203ms。随后亲读新stage13/close-reopen.json和stage12/failed-{close,release}.json，均passed=true、references0、descriptorDisposed=true、registryRemoved=true；新stage13 opened2/frames2。原件位置及null字段限制见artifacts/r7-review-test-receipts.md；其中cleanup=null，失败两份threadExited=null/nativeReferences=Unknown，不能借此声称最终故障已全部观察、原生资源全部释放。

## 下一resize前置（暂不直接运行正式入口）

已定位`ReplacementStateSequenceWindowTests.WhenWindowImageUpdatesAndResizesThenFreshFramesAndCallbacksConverge`。其顶层明确先RunAsync(original)再candidate，禁止直接运行这个测试；即使role=candidate，probe.Run仍读取AcceptancePaths.OriginalDll作officialNative身份。现有入口不满足完全无original依赖要求。缺reference并不是可关闭打包门。

不能用Pcore/LifecycleSequenceScenes替代：它自身注明仅RenderTargetBitmap，不证明实际HWND resize。也不能以producer字节替代consumer。

定向方案应使用candidate-only隔离bootstrap（沿用CandidatePureVideo的固定10.0.13运行时、SHA/PE验证、唯一Native、默认DLL目录约束、预加载和resolver），源链接保留原RunCycle消费者：3周期、4颜色Red/Blue/Lime/Yellow、frame2内容改40x24、原96DPI/Pbgra32/逐像素IsExpectedFreshFrame/ActualWidth和ActualHeight增长/Rendering回调/Close解绑与弱引用回收原断言全部不改。去除的只能是外层original身份与比较依赖，不改变消费者或oracle；建立独立命令candidate window-image-resize，不能复用会自动跑original的MSTest顶层。监督启动前必须核查方法JIT边界及项目只包含必要源，不引用正式统一oracle打包链。

验收需新harness publish并冻结SHA/MVID；Native不改时仍核r6 SHA。先missing digest/wrong SHA/wrong architecture负控（不得进入WPF），再有界真实consumer；保存12份RTB帧及source文件但只以consumer帧作判断、实际HWND尺寸、回调/退出/模块证据。失败保留，不改expected/flags/DPI/容差、不retry-to-green。尚无该独立可执行入口，因此当前不能声称有可直接运行的合规resize方案或已完成resize前置。
