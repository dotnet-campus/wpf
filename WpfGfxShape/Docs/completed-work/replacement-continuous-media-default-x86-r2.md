# 持续媒体 default-only x86 r2：Passed / Closeout Completed

> 状态纠正及完成依据：原报告在恢复入口、工作项、计划状态、最终矩阵及当前基线尚未同步时提前写了 Closeout Completed，属于过早声明，不是执行证据变化。本次仅文档收尾与既有原件复核，现已真实更新并逐处回读五处新增正式条目，报告新增亲读索引与终态也已回读，才恢复本标题为 Closeout Completed。原报告下文“亲读/实施”是原执行阶段记录，本次亲读范围单列于末节，不追认为本次实施或全量源码复审。

## 裁决与范围

亲自实施、未再委派。唯一正式无参体 `WpfGfxShape.ComAcceptance.PcoreComparisonTests.WhenContinuousMediaRunsOnceThenOriginalAndCandidateFramesMatch` 实际执行1、Passed1、Failed0、notExecuted0、timeout0，父exit0。official1→candidate1→comparator1，各自成功后才进入下一角色；本轮停止执行，无收尾待办。

仅修改 `Tests/WpfGfxShape.ComAcceptance/NativeRun.cs` 新入口及私有独立oracle，148→234行。旧DataRow三模式与helper默认false路径不变；MediaScenes/WriteMediaAsset/WaitMedia/CompareMedia、生产、项目、SDK、包依赖、新Host、Native均0改。先写 `artifacts/continuous-media-default-x86-r2/execution-ledger.md` 冻结范围/预算/谓词，再编辑运行。未按FFEB1011拟合exact/tolerance。

源顺序红R>G/B、蓝B>R/G、绿G>R/B、黄R/G>B，固定内区x8..23/y4..11每像素A255/RGB非空；四全图两两不同，逐帧asset/request/file/hash/2048bytes核验，Position有限且原[request,request+1)窗口并递增，Opened/Ended各2、非预期Failed0及三种Close事件各1、mediaClosed计数核验。仅本素材同版official适用条件，不是普遍pipeline保证；边缘没有新增独立语义保证。纯断言负控使用内存合成四平台与黑/透明/轮转错序/同asset旧平台，未产品故障注入；父Passed证明这些断言全部通过，未另生成不存在的负控结果字段。

同次official/candidate仍四帧完整全图2048×4=8192bytes，原SequenceEqual、threshold0；不裁剪、不归一化、不放宽。comparison.json四项equal=true且长度2048，因此不同字节数为0（由全字节相等推得；报告没有独立differentByteCount字段），不是套历史差分数。

## 构建发布与预算

normalbuild1：Release/win-x86/net10.0-windows，exit0，139 warnings、0 errors，12.37秒。COMpublish1：同配置/RID/TFM，self-contained true，RuntimeFrameworkVersion10.0.13，ComAcceptanceSandbox=true，AotRuntimeIdentifier=win-x86，显式AotDllPath=`d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/postfailure-source-rebuild-x86-r1/native/wpfgfx_cor3.dll`。独立r2/publish及r2/clean；正常publish隐含编译1另计，未用no-build、未重复build/能力门，Native0。publish exit0，warning总数Unknown，无抑制分析器/失败修后重试。

clean runtimeconfig全文核NETCore/Desktop均10.0.13；PackageSelfContainedComAcceptance检查依赖后复制，清理只r2/clean，package-files含正式exe/dll/deps、MTP/MSTest/TRX、coreclr/PresentationCore、官方和Native候选。aot.sha256=15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620。

执行工具后即时持久build/publish/sandbox-tool-return与参数。build可见64行全读，源109536字符仅末20000，前89536丢弃；publish60行全读，55059字符仅末20000，前35059丢弃。回读摘要在build-log-readback.md、publish-log-readback.md；没有声称不可恢复编译头部已保全，没有补跑。

**合规偏差**：核包一次文本搜索目录误设整个clean，工具返回package-files之外四DLL文本命中片段（coreclr、TRX、MTP、MSTest）；违反用户二进制不作文本读要求。已即时持久披露，不把片段当二进制认证，不再进行此类目录搜索。AVI/pbgra32未作为文本读取，无额外程序探针。首次结构查询以目录作solutionPath返回Function failed，随后仅用目录确认真实slnx；不是构建或程序失败，未执行重试。

## 唯一运行与父身份

task `7fa383b0c0214d1fb929c1ad123768e7`，clean正式exe唯一沙箱调用，`--supervise-exit Results/continuous-media-default-x86-r2 120 FullyQualifiedName=WpfGfxShape.ComAcceptance.PcoreComparisonTests.WhenContinuousMediaRunsOnceThenOriginalAndCandidateFramesMatch`，外层180秒，整任务回拉r2/readback。无独立发现/x64/full/显示探针/额外测试/脚本/终端/再委派。

监督40行全读：09:43:20.5944316–09:43:42.166258 UTC，supervisor3576/child7140，X86/.NET10.0.13，Reason exited/ExitCode0/HasExited true，无TimedOut/Cancelled/KillAttempted，OutputCollected true、两种drain timeout false、SupervisorError null。实际test DLL在任务根，SHA `A304FA4C7D8E2171324DBF8046294B54433209121942D9158577F939AC68FD8A`，MVID `47d3436e-810e-490f-b311-06505cde05c5`。

父TRX26行全读，run0895f7de-40d8-4c2d-9965-75c83039ee26，准确类/方法、单体20.7260459秒、total/executed/passed1，failed/error/timeout/aborted/notExecuted0。stdout12行含中文编码替换，stderr空；diag65行分段补截断全读，同次内部Discoverer1823不算独立发现，实际仅新无参体1。角色沿用父输出，无独立角色stdout/stderr文件，不声称读取不存在文件。比较器只输出comparison.json，没有独立role result.json，不造字段。

## 实际模块与分层

业务根：`artifacts/continuous-media-default-x86-r2/readback/Results/pcore/media/default/12cc062245d1437fa701f2318e8a7467/`。original/result.json507行、candidate/result.json514行已分段读至尾，截断段补齐；两者stage completed/status Green/exitCode0，分别OriginalOnly与PendingIndependentComparator（后续独立comparator成功，不回改原角色字段）。

以 `C:\CodingAgentSandbox\Tasks\7fa383b0c0214d1fb929c1ad123768e7\` 为任务根：
- official actualNativeModules为根wpfgfx_cor3.dll，SHA `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`，1807144bytes，10.0.13-servicing.26504.104/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。
- candidate actualNativeModules为Native\wpfgfx_cor3.dll，SHA `15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620`，4003328bytes，file1.0.0.0/product1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a；不是requested/frozenCandidate代替actual。
- candidate systemMediaServiceModule实际记录根官方5EBD18E0…；SystemMediaModule.Acquire亲读确认按已加载PresentationCore同目录找官方、文件身份排除自身、进程驻留。SystemVideoSample.Acquire亲读确认provider/renderer/sample服务借用。候选复用官方系统player/decoder/provider/sample，候选负责其绘制/合成角色；并非全媒体栈独立替换或官方代做整个RTB。
- 两角色PresentationCore根SHA D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D、WindowsBase根SHA C047D97A1BD3CD29E50806340C246B01482CC71CA6E41216AD5C9A0C1568F1ED，均10.0.1326.50504。同版同架构身份由实际记录核验。

## 四帧、输入与事件

|文件（各2048bytes）|请求秒|official实际秒|candidate实际秒|两角色共同sample BGRA首2像素|共同SHA256|
|---|---|---|---|---|---|
|media-0-0.pbgra32|0.5|0.6697366|0.6782935|1110EBFF1110EBFF|56E48108ACA0E07CF0D9F531679C1036E1980631DDF3A76C0409D7AAF9F6A8A4|
|media-0-2.pbgra32|2.5|2.5382199|2.5405477|EB1011FFEB1011FF|8F5F79953DF7C91E4BF56AC75222912DFE5E3C57B5B5E3164A63FA83F7FF07A5|
|media-1-0.pbgra32|0.5|0.5335907|0.5468061|11EA11FF11EA11FF|D800E6BCB44942F4D4DEF110BE0625AA4C45DEE86DCB597940CDE163FD70B6A0|
|media-1-2.pbgra32|2.5|2.5341442|2.5323764|10EBEAFF10EBEAFF|BD6FD4A7D9D5B84E82D5681BC174193A170A208B368BF794B1E86CFF6B4056A7|

每角色四帧HasVideo/HasAudio真、NaturalVideoWidth2/Height1；通用input仍32×32，媒体不采用它，按CopyMedia及新oracle固定32×16/96dpi/Pbgra32。两份continuous-oracle.json27行全读，Passed、内区/长度明确；父实际读取二进制计算hash而非文本处理。每角色两个AVI各65284bytes；asset0 SHA01BE17E50794DD8A076967E49D0C2CD6265A3908247748B4B95689AB2F07C872，asset1 SHA07FBF8F7BFBF80BC2832E714BBB374CAB8CB7E57F91D10AAAAA141E177DEF802，两角色相同。原件全部保留，输入历史r1 hash仍Unknown，不补造历史。

每角色事件opened1→ended1→opened2→ended2，Ended位置均4秒；parallel-close-first/second、expected-failed-close各1（实际HRESULT -2147024894不作环境精确标准），非预期failed0。official opened位置0/0.0032624，candidate0/0.0026754。帧/事件分表，未记录跨表原子时序；actualPosition为Pause后而非surface精确PTS。原实时WaitMedia仍15秒窗口，换VideoDrawing引用后全帧SequenceEqual、自然Ended与finally Close/解除引用均沿用成功路径，不虚构retained字段。

角色记录Dispatcher shutdown/finalizers drained并正常exit0，父正常退出无强杀；这不证明全部系统worker归零、COM ref0、全部callback静默或长期资源趋势。mediaClosed仍HasVideo/HasAudio真，不能宣称Close同步清空属性；Volume0/HasAudio不认证实际音频内容或设备输出。共同系统decoder缺陷不由exact差分排除；同色20帧平台不能证明每个PTS没有陈旧帧、复杂空间结构或所有帧都呈现。

## 最终账、旧账与唯一下一门

normalbuild1/1成功；COMpublish1/1成功（隐含编译1另计）；Native0；sandbox1/1；正式体1/1 Passed；official1/candidate1/comparator1均成功；独立发现/额外执行/重试/x64/full/显示探针/终端脚本/再委派0。源码最后修改后只执行本轮唯一体，无运行后代码尾改。

r1父Failed、首帧FFFF0000≠FFEB1011和官方真实四帧/OpenedEnded/Close有限证据、原normalbuild/publish/sandbox各1及candidate0/comparator0全部保留，不追认Passed。r2当前15A545新增的只是x86/10.0.13有限真实连续视频帧/公开事件及8192bytes零差分，不媒体全闭/全替换。显示Unknown，音频输出/全worker/长期趋势/HW/loss/full/完整替换Open；x64媒体当前未覆盖。

**唯一下一门建议：另授权x64同一default-only持续媒体有限门。** 现有x64旧clean缺本次新入口，不能直接运行旧包冒本门；须另授权COM build≤1、COM publish≤1（正常隐含编译1另计）、official-first沙箱≤1/正式体≤1，official→candidate→comparator各≤1，复用54BC62/Native0，先核实际身份与同版官方，120/180秒及首错即停合同不变。本次不执行，不把预算建议视为授权。该旧包缺入口是本次用户给定约束；本次只读旧x64 closeout中包路径与身份记载，未重新读取或运行旧DLL证明入口清单。

## 本次正式文档收尾：亲读范围与归属

本次仅本人读原件、写文档，交主代理监督；源码修改/build/publish/沙箱/test/终端脚本/再委派新增均0。原报告中“亲自实施”“build日志全读”“SystemMediaModule/SystemVideoSample亲读”均属原执行阶段记载，不是本次重新实施或源码复审。本次未重新计算二进制hash、未文本读取DLL/ETL/AVI/pbgra32、未新增程序或探针；仅目录枚举与明确文本读取，文本检索只限Docs，目录列表所列二进制不是内容读取或认证。

本次亲读原件（以 `artifacts/continuous-media-default-x86-r2/` 为根）：
- execution-ledger.md 1–11；build-tool-return.txt 1–3、publish-tool-return.txt 1–3、sandbox-tool-return.txt 1–3；build-log-readback.md 1–4、publish-log-readback.md 1–3。原工具数字exit未直接给，exit0/139warnings0errors及截断统计沿用即时摘要；没有重新取得不可恢复的编译日志头，也不把前代理“64/60行全读”改称本次原日志全读。预注册末行0是执行前快照，不冲销完成账。
- readback/Results/continuous-media-default-x86-r2/：supervision.json 1–40、TestResults/child.trx 1–26、stdout.txt 1–12、stderr.txt空、Diagnostic/log_261028094320736.diag 1–65（1–25/26–40/41–50/51–65分段无截断）。最初名称检索未命中child/TRX，随后精确列TestResults并亲读真实child.trx，不以未命中宣称缺证。
- 业务根 `readback/Results/pcore/media/default/12cc062245d1437fa701f2318e8a7467/` 下Results/original/result.json 1–507及Results/candidate/result.json 1–514。首次1–85调用均4000字符截断，已从65开始补读至original83/candidate82；随后84/83–145、146–205、206–265、266–325、326–375、376–420、421–455、456–490、491–尾连续分段补齐。本次确实读到两角色尾，含全部事件、四帧asset/request/actualPosition/file/sample/hash、mediaClosed与actual模块，不以首段或报告摘录代全件。
- 同业务根Results/original/continuous-oracle.json及Results/candidate/continuous-oracle.json各1–27；Results/diff-media/comparison.json 1–34完整。未把Baseline/ComparisonCandidate的复制件冒亲读，未造comparator独立result或角色stdout。四项equal=true/2048bytes及匹配hash亲见；SequenceEqual方法名和oracle内区每像素断言语义依据原报告与预注册，不冒本次重新执行。

本次核验official start09:43:21.4286984、completed checkpoint09:43:31.5674537，candidate start09:43:31.6895075、completed checkpoint09:43:41.8448444，父结束09:43:42.0569575；符合报告的official独立oracle先过→candidate后过→comparator顺序。独立oracle Passed与父Passed、comparison Green共同支持此门，comparison无独立时间字段，不虚构更细原子时序。Opened/Ended各2、expected-failed-close/parallel-close各1真实；两角色Close相关事件顺序不完全相同，不冒跨角色同序保证。HasAudio/Close/dispatcher及Position不是音频、全worker、精确PTS或趋势认证。

报告先前过早Closeout Completed已显式纠正。本次实际同步且回读：next-session-handoff.md、handoffs/current-work-item.md、replacement-issues-and-plan-status.md、final-acceptance-remaining-matrix.md、handoffs/current-stable-baseline.md；旧段保留但标为历史。本门只增加当前x86有限媒体绿，r1父Failed/官方四帧有效及oracle跨层exact缺保证的修正原因、所有独立历史账和DLL文本误扫偏差均保留。shader/resize、真实HW、loss、实际音频、全worker/长期趋势、full及完整替换Open，显示Unknown；没有文档待办或自动后续执行。
