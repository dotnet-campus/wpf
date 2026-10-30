# 持续媒体 default-only x64 r1：ExecutedPassed / CloseoutCompleted

## 裁决与实际预算

前代理执行阶段亲自探索、构建发布、唯一沙箱执行及完整原件回读；本收尾阶段仅由当前代理亲自读取既有文本与同步文档，供主代理监督，不新增执行或再委派。以下构建、运行与完整亲读索引均为前代理阶段记录，本次复核范围单列于末节。正式无参体 `WpfGfxShape.ComAcceptance.PcoreComparisonTests.WhenContinuousMediaRunsOnceThenOriginalAndCandidateFramesMatch` 1/1 Passed，父exit0，Failed/timeout/notExecuted均0；official1独立oracle成功→candidate1独立oracle成功→comparator1 Green。仅当前x64持续视频四帧/公开事件/零阈值差分有限通过，整体Open。

源码/项目/测试/资源/SDK/包规则修改0，新素材/Host/库0，Native发布0。normal COM build1（Release/win-x64/net10.0-windows）exit0、139 warnings/0 errors、4.21秒；COMpublish1 exit0（正常隐式编译1另计，未no-build），warning/error合计Unknown。唯一Windows沙箱1/正式体1/official1/candidate1/comparator1；独立发现/额外角色/重试/full/显示探针/x86重跑/终端脚本/再委派0。

执行前先写 `artifacts/continuous-media-default-x64-r1/execution-ledger.md` 冻结范围、预算、弱语义与预分类。准确build参数、publish完整参数及可见诊断转录分别在01-build-tool-return.md、02-publish-tool-return.md；03-preflight.md为包核验；04-sandbox-tool-return.md保留唯一沙箱完整直接返回。普通build默认obj/bin，正式publish显式AotRuntimeIdentifier=win-x64、AotDllPath=`d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/postfailure-source-rebuild-x64-r1/native/wpfgfx_cor3.dll`；self-contained、RuntimeFrameworkVersion10.0.13、ComAcceptanceSandbox=true，独立本轮publish/clean。无分析器抑制，无失败修后重试。

## 来源链及执行前核包

已读最新next-session-handoff顶部、x86 r2报告及尾部亲读索引、independent-oracle-review正文及尾部、current-execution-rules全文及当前事项/基线/计划/矩阵顶部；必要源码为NativeRun新入口/独立oracle/official先停与原CompareSceneAsync、MediaScenes采样/换drawing/Close/CompareMedia、SystemMediaModule.Acquire。非全仓源码加密冻结。

权威Native目录已枚举，source-rebuild-x64报告及旧clean摘要均54BC62；新publish/clean的aot.sha256再次为 `54BC62FBBEF9A55EA75E49AF5EFDCEC70F7C8C2808BDF4E2C80B916CF92D5ADD`。包规则PackageSelfContainedComAcceptance 42–72亲读，对publish/Native真实GetFileHash并检查依赖、复制到独立clean，清理仅本轮指定clean；最终actual模块再次核同SHA。未拿旧40D0或requested/frozenCandidate冒actual。

新clean runtimeconfig21行全文核NETCore与WindowsDesktop均10.0.13；deps1–48核win-x64、MSTest4.0.1及两个runtimepack10.0.13。package-files共622行，亲读1–85/190–260/375–455/590–622（非全manifest）：根正式exe/dll/deps/runtimeconfig、MTP/TRX/MSTest、coreclr/PresentationCore/WindowsBase、根官方wpfgfx_cor3.dll与Native候选。公开API读取新clean测试DLL确认无参新入口确在本包，无额外运行发现。

**日志保存偏差（不得略去）**：build工具64行与publish工具60行均在下一操作覆盖日志前实际亲读，但持久文件只保存全部可见诊断的压缩转录，省写重复长路径/URL/标准建议，未满足“完整可见工具返回逐字持久化”；不是完整原stdout。build源109536字符仅末20000可见、前89536缺失；publish源55059字符仅末20000可见、前35059缺失。即时记录准确参数/exit及边界，不追认丢失头部。未补跑获取日志。一次检索误以package-files.txt作为目录参数返回目录不存在，核已存文件后改按行读；不是程序失败，也未对clean目录递归内容搜索或文本读二进制。AVI/帧/DLL未作为文本读取。

## 唯一运行、父身份及亲读索引

task `4a92dc1f6614451d9b426e4ba5b518e5`，source为本轮clean、根WpfGfxShape.ComAcceptance.exe，参数 `--supervise-exit Results/continuous-media-default-x64-r1 120 FullyQualifiedName=WpfGfxShape.ComAcceptance.PcoreComparisonTests.WhenContinuousMediaRunsOnceThenOriginalAndCandidateFramesMatch`，外180秒，outputRelativePath省略整任务回收独立readback。工具直接返回只有完成及本地路径，不给数值exit；数值来自以下实际监督。

以 `artifacts/continuous-media-default-x64-r1/readback/` 为R：
- R/Results/continuous-media-default-x64-r1/supervision.json 1–40全文：2026-10-28 13:01:50.5040292–13:02:11.5991593 UTC，supervisor9252/child2624，Reason exited、HasExited true、ExitCode0、TimedOut/Cancelled/KillAttempted false、OutputCollected true，两drain timeout false、SupervisorError null。
- 同目录TestResults/child.trx 1–26全文，runId32683e6e-051f-4e1a-88ee-81938a89070c；testId109a6a3c-ce02-81a4-b0e7-3a05e03e1672，executionIdedc142ff-b54e-4d08-a7b0-7998bd363236；准确类/无参方法，duration20.4220628秒，total/executed/passed1，failed/error/timeout/aborted/notExecuted0。业务UID另为1ae5b67c0cf94fc5bec70b7f89cb62f0，不能混同TRX IDs。
- stdout.txt 1–12全读（中文编码替换保留真实边界）；stderr.txt空。Diagnostic/log_261028130150635.diag共65行，1–22/23–43/44–60，最后一段4000字符截断后58–60及60–65补齐至尾。内部Discoverer1823为同次正常测试实现步骤，不算新增独立发现；实际只运行本体1。
- R/Results/aot-identity.json单行全读：X64、pointerSize8及候选初始化54BC62（不是actual加载认证）。实际test DLL根路径，SHA `6E2E2AC04F7462A0D7E1C6AEA366778BAB9F239582A25518E3D4C317D033B191`，MVID `3347eed0-add1-48e6-a0a6-0f9f6138873d`；监督/diag/两角色均.NET10.0.13/X64。
- 业务根B=R/Results/pcore/media/default/1ae5b67c0cf94fc5bec70b7f89cb62f0/。B/Results/original/result.json全507行，按1–70/71–130/131–190/191–250/251–310/311–370/371–430/431–507亲读无遗漏；candidate/result.json全514行，1–60/61–120/121–180/181–240/241–300/301–360/361–420/421–475/476–514亲读至尾。全部resolver/身份/事件/帧/cleanup/exit均已读，不以首段代全件。
- B/Results/original及candidate/continuous-oracle.json各27行全文，Passed；B/Results/diff-media/comparison.json34行全文Green。Baseline/ComparisonCandidate仅复制件未冒亲读；comparator没有独立result.json或独立exit文件，父Await返回0且Passed证明comparator exit0，不造字段。角色沿用父stdout/stderr，不造独立角色输出文件。

## 实际模块身份分层

以下路径以 `C:\CodingAgentSandbox\Tasks\4a92dc1f6614451d9b426e4ba5b518e5\` 为根：

|角色/模块|actual路径|SHA256及版本|
|---|---|---|
|official actualNativeModules|wpfgfx_cor3.dll|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，1956648bytes，file10,0,1326,50504 / product10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699|
|candidate actualNativeModules|Native/wpfgfx_cor3.dll|54BC62FBBEF9A55EA75E49AF5EFDCEC70F7C8C2808BDF4E2C80B916CF92D5ADD，4527616bytes，file1.0.0.0 / product1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a|
|candidate systemMediaServiceModule|根wpfgfx_cor3.dll|实际AD1C9D完整值同上，不套x86的5EBD|
|两角色PresentationCore|根PresentationCore.dll|0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31，8324904bytes，10.0.1326.50504|
|两角色WindowsBase|根WindowsBase.dll|1569EAFC8370E8F6AD29087BEDA0430EC04C08A339862341E20DAEDF9632A6AE，2176848bytes，10.0.1326.50504|

PresentationCore/WindowsBase product10.0.13-servicing.26504.104+4099f83977f4d29604a9aeb971fb0a7bc1d5f699。同版同架构来自本次真实记录，不移绑x86。SystemMediaModule.Acquire读取实际PresentationCore目录官方、文件身份排除自身、进程驻留；候选复用官方系统player/decoder/provider/sample，与候选绘制合成角色分层，不等于整个RTB交给官方，不宣称全媒体栈独立替换。共享decoder共同缺陷不能由同次差分排除。

## 四帧、语义、时间及生命周期

两角色独立oracle均成功；父Passed亦证明内存合成正常平台及黑/透明/轮转错序/旧平台负控断言通过。固定内区x8..23/y4..11每像素A255/非空，顺序红R>G/B、蓝B>R/G、绿G>R/B、黄R/G>B，四全图两两不同，文件2048bytes/hash/asset-request-file/Position及事件一致；未按本次结果改谓词/expected/素材。只是本素材的适用性语义门，不是普遍色彩保证，内区之外没有新增独立语义认证。

|帧文件（各2048bytes）|请求|official实际秒|candidate实际秒|共同BGRA前两像素sample|共同SHA256|
|---|---:|---:|---:|---|---|
|media-0-0.pbgra32|0.5|0.6679292|0.6802905|1110EBFF1110EBFF|56E48108ACA0E07CF0D9F531679C1036E1980631DDF3A76C0409D7AAF9F6A8A4|
|media-0-2.pbgra32|2.5|2.5351964|2.5299402|EB1011FFEB1011FF|8F5F79953DF7C91E4BF56AC75222912DFE5E3C57B5B5E3164A63FA83F7FF07A5|
|media-1-0.pbgra32|0.5|0.5372429|0.5465926|11EA11FF11EA11FF|D800E6BCB44942F4D4DEF110BE0625AA4C45DEE86DCB597940CDE163FD70B6A0|
|media-1-2.pbgra32|2.5|2.529271|2.5291755|10EBEAFF10EBEAFF|BD6FD4A7D9D5B84E82D5681BC174193A170A208B368BF794B1E86CFF6B4056A7|

四项comparison equal=true、bytes2048，完整32×16/96dpi/Pbgra32共8192bytes，threshold0，原SequenceEqual无crop/normalize/放宽。不同字节0仅由全字节相等推导，JSON无独立differentByteCount字段。通用input仍32×32，不拿它作媒体尺寸。Position原[request,request+1)窗口且每asset递增；Pause后属性不是surface精确PTS，20张同色平台不能保证所有PTS不陈旧或所有帧呈现。

每角色HasVideo/HasAudio真、NaturalVideoWidth2/Height1。两AVI各65284bytes：asset0 SHA01BE17E50794DD8A076967E49D0C2CD6265A3908247748B4B95689AB2F07C872，asset1 SHA07FBF8F7BFBF80BC2832E714BBB374CAB8CB7E57F91D10AAAAA141E177DEF802，两角色一致。本次hash来自父程序真实二进制计算及oracle，非本人文本解析AVI；不补造历史r1输入hash。

official start13:01:51.1733665/completed checkpoint13:02:01.2594733，candidate start13:02:01.3554484/completed13:02:11.3728513；代码先official exit/oracle再candidate再独立comparator，comparison无独立时间字段，不虚构原子时序。两角色opened1→ended1→opened2→ended2，Ended位置均4秒；official opened0/0.0027105、candidate0/0.0026086。expected-failed-close各1、HRESULT -2147024894不作为环境精确标准；parallel-close-first/second各1，candidate两parallel事件次序与official相反，未要求跨角色同序；非预期failed0。

MediaScenes成功路径证实Pause并更换VideoDrawing引用、解除旧drawing.Player后全帧SequenceEqual保留检查、seek3.6后自然Ended、MediaFailed回调内Close、两个Opened回调Close及finally主player.Close/解除drawing引用。没有独立retained或全COM引用统计字段，不造字段。两角色stage completed/status Green/exit0；official OriginalOnly，candidate PendingIndependentComparator后由独立comparator通过，不回改原字段。

cleanup记录DrawingContext disposed、bitmap/visual引用释放、Dispatcher shutdown、finalizers drained、模块驻留至exit；父正常退出无强杀。只支持有限清理路径，不证明全部COMrefs0、所有系统worker退出、全部callback长期静默或长期资源趋势。mediaClosed仍HasVideo/HasAudio真，Close不等于同步清空属性。Volume0/HasAudio只说明静音及识别音轨，不认证实际音频内容/设备输出。

## 历史账、整体边界及下一主线

保留x86 r1父Failed、official有效四帧与跨层exact oracle不足历史；r2 actual15A545/official5EBD通过、原预算及DLL文本误扫偏差独立不冲销。保留所有Native有限回归、4199托管、显示/HW与ETW阶段历史账，不将旧红追认绿或把旧40D0冒当前54BC62。本次新增仅当前54BC62/AD1C9D、X64/.NET10.0.13有限持续视频及公开事件与8192bytes零阈值差分。

整体包1–8/完整替换仍Open；HW/shaderresize/real-loss/实际音频/全worker/长期趋势/full不自动闭合，显示根因Unknown。下一主要工作建议：另授权按完整方案优先静态确定现存非显示资源长期生命周期可度量合同的样本数、资源指标与停止门，不再无限接力微门；实际音频若无decoder输出接口则明确未证，不以HasAudio代替。不自动重跑本媒体门，不重启显示/ETW/full；本次及建议的下一静态阶段均不新增执行。

## 本次收尾亲读范围与终态

本次亲读本报告原文1–74（工具超长截断处以30起及64起续读至尾）；即时记录execution-ledger.md 1–8、01-build-tool-return.md 1–58、02-publish-tool-return.md 1–58、03-preflight.md 1–7、04-sandbox-tool-return.md 1–7全文。实际文档与artifacts均位于工作区下WpfGfxShape/，最初按工作区根Docs读取返回文件不存在，经目录枚举定位；另一次将完整方案文件作为内容搜索目录返回目录不存在，未读取二进制，均非程序执行。

本次以R、B沿用上文路径，亲读R/Results/continuous-media-default-x64-r1/supervision.json 1–40、TestResults/child.trx 1–26、R/Results/aot-identity.json 1–1、B/Results/original与candidate/continuous-oracle.json各1–27、B/Results/diff-media/comparison.json 1–34全文。两role result仅复核original 1–65及465–507、candidate 1–65及472–514，确认actual候选54BC62完整SHA、actual官方及candidate systemMediaService AD1C9D完整SHA、角色Green/exit0、Close属性与cleanup记录；不是本次完整重读507/514行。监督原件确认test SHA6E2E2AC0完整值、MVID3347eed0-add1-48e6-a0a6-0f9f6138873d、.NET10.0.13/X64；pointer8来自实际identity记录。TRX确认正式体1passed，监督确认exit0；两oracle Passed与comparison四项2048bytes/equal=true/threshold0直接复核。official先oracle绿才candidate的入口顺序沿用前代理完整源码与执行审阅，本次未重读源码，不把result时间戳当oracle原子时序证明。

本次未重读diag/stdout/stderr、全部role result、源码、包清单或runtime文件；前代理完整亲读索引仍为其阶段记录，不移作本次全亲读。未文本读取或递归内容搜索DLL/AVI/帧等二进制。无关图像请求相关任何无证执行声称撤回，不登记为执行、证据或预算。

本次实际读取并最小同步next-session-handoff、current-work-item、current-stable-baseline、replacement-issues-and-plan-status、final-acceptance-remaining-matrix五处顶部；修改后分别回读1–11/1–11/1–11/1–11/1–18确认状态、阶段范围及静态下一主线一致。current-execution-rules 1–27全文及完整方案顶部1–46作为边界参照，未声称完整方案全文本次亲读。

收尾裁决：当前x64 r1与既有x86 r2各自绑定实际模块、同版同架构官方的持续视频/公开事件有限门通过；不是普遍媒体色彩、完整独立媒体栈或完整替换通过。不同字节0只由8192bytes全等推导。前代理账normalCOMbuild1/139warnings0errors、COMpublish1含隐式编译1、sandbox1/正式体1passed/official1/candidate1/comparator1/Native0不变；日志压缩及源截断不补造，所有历史独立。本收尾源码/build/test/publish/沙箱/终端脚本/再委派新增0。

收尾状态：五处正式状态已同步并实际回读，随后本报告改为CloseoutCompleted；本报告终态回读完成后提交主代理监督，无收尾待办。整体Open与上述未证项不因文档收尾关闭。
