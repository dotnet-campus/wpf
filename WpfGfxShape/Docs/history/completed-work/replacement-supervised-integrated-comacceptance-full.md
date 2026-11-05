# IP 当前源码 ComAcceptance 双架构 Release full：IQ 独立核证终态

## 1. 最终裁决与执行边界

IQ已完成本地已回收现场核证，不是初稿或“已跑未读”。两次正式无filter full均以原生堆损坏0xC0000374中断，均无精确parent完成TRX，不能给full完成/失败总数或通过率。IO Tests各4256通过独立保留；COM两格中断，绝不是四格通过。完整替换未完成。

IQ仅修改本报告及next-session-handoff、replacement-supervised-session-summary、replacement-issues-and-plan-status四文档；生产/测试/项目0修改，构建/发布/沙箱/测试/整理exe执行0。未访问消失远端、未上传readback根、无终端或直接/间接脚本。已先登记handoff核证进行中再取证。

读取范围：执行规则27行；完整方案1–120（首次长段截断后补读40–89及90–120）；latest handoff顶部IP/IO/IN/IM及共享顶部；旧DQ报告1–32可见段、BP1–43可见段与HY/HZ1–32，当前VisualCache关键提交/清理源码。共享长历史保持，不作千行全文审计。

## 2. 真实full监督与精确parent缺失

证据根为本项目artifacts/ip-integrated-full/full-x86、full-x64；下文相对路径均在该根。两个Results/ip-full-<arch>/supervision.json各37行全文及stderr全文已亲读；Mode=full，Arguments只有报告/结果/同步diagnostic/no-progress，没有filter。TimeoutSeconds=1500，.NET10.0.13，MSTest4.0.1。

|架构|真实task；监督PID/子PID|UTC起止；时长|实际子退出|
|---|---|---|---|
|x86|d41f442050e743ddbf359ec0e4a9516c；9860/10624|2026-10-25 05:20:52.6262316–05:21:50.160307；57.534秒|-1073740940 / 0xC0000374|
|x64|9cadcdae08eb4718afda7192740b7f27；8372/8456|2026-10-25 05:22:14.0631533–05:23:07.6935984；53.630秒|-1073740940 / 0xC0000374|

两份Reason=exited、HasExited/OutputCollected=true；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，SupervisorError=null，stderr为空。不是超时杀进程或传输失败；不把外层回收成功写成子Exit0。

精确parent分别为C:\CodingAgentSandbox\Tasks\d41f442050e743ddbf359ec0e4a9516c\Results\ip-full-x86\TestResults\child.trx及C:\CodingAgentSandbox\Tasks\9cadcdae08eb4718afda7192740b7f27\Results\ip-full-x64\TestResults\child.trx。本地对应路径专用读取均明确不存在，run目录也无TestResults。stdout的两次1/1 Passed来自source-clip独立角色，不是parent完成统计，绝不拼role为full。

## 3. actual身份分层

|层级|x86|x64|
|---|---|---|
|父test SHA256（supervision）|5E8F778F001685F3FB6D89419B84B3744EB18319FACD464CA3BC61460E301750|315F62A898CD01539EFE6AC100F546EBC9877C287DD6BFE5C4DCC4E095B624B2|
|父test MVID|b6205b31-80d7-4c88-9f38-8394f373fd1d|a3f3ed7a-00ac-44cc-a63e-3d8a49b17a26|
|本轮新候选Native SHA256|63B91E6CD6B5C1E8D0095E1673933DD5BFC539744F7EC36DC06C8DCB074E3136|0C16A66C898269A17DBDD062012898FEBE338E341FED8D0B7FEC47FB21B6A23A|
|实际官方SHA256|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|

两aot.sha256与Results/aot-identity.json原件一致，Native路径均为各task/Native/wpfgfx_cor3.dll、X86/4及X64/8。但顶层identity已被BW official-repeat过滤角色覆盖：其Arguments是WhenNarrowPrimitivesAreTransformedThenIndependentOfficialContentMatches，不是父full参数；AssemblyInitialize每角色写同一文件可解释覆盖，不能据此说full误跑filter，也不能当官方角色实际消费候选的证明。

候选actual补证：x86 diag15701 PresentCondition明确当前进程10624、Native路径及上述新SHA；x64 Results/hw-module-Native-3b7049669fba454c94845e25baa9802b.json明确Requested=Actual=Native路径、新SHA、上述父test SHA、.NET10.0.13、ProductVersion1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a。未重新计算文件hash，不冒称对全部包字节独立hash。

官方actual：Results/hw-module-d41f442050e743ddbf359ec0e4a9516c-429d3032af414e36925fc8f87ae861f3.json及hw-module-9cadcdae08eb4718afda7192740b7f27-525b9459b9234d37b9b31ad1b1d50333.json各10行全文，Requested=Actual=各task根/wpfgfx_cor3.dll（不是Native子目录），各SHA/父test SHA/架构一致。File10,0,1326,50504，Product10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。不是沿用IH hash绑定新full。

## 4. 原生中断最后真实阶段

双架构最后MTP UID均18e39062-2997-8a5c-bf02-39b275a17ace，WhenHistoricalCaptureOfficialGeometryRunsThenExpectedUpdatesRemain，只有InProgress/Executing，没有Passed/Failed终态。x86 diag16134–16138至EOF，x64 diag15755–15759至EOF。这次x86不是旧DQ最后InteropDeviceBitmap_Create前边界。

x86 stages875–889至EOF：两surface创建/interop/resources/draw/dirty已经返回，05:21:42.8667562 close后commit前，05:21:42.8677001 Commit=0x80070006/E_HANDLE，05:21:42.8685631 cleanup destroy channel before后无after；约7.29秒后监督记录堆损坏退出。

x64 stages991–1005至EOF：同样两surface建成，05:23:02.7846109 close后commit前，05:23:02.7856578 Commit=E_HANDLE，05:23:02.7867025 DestroyBefore后无after；约4.91秒后退出。两次原生前置AssertCombinedPixel已越过，不能沿用BP“尚未connect”解释这两次。

DestroyChannel是最后可见边界，不是首次堆破坏位置。无原生堆栈/首次坏写证据，不断言候选生产、调用约定或某一event为已证唯一根因。当前同一官方geometry合同在两架构均中断，是下一根因工作的精确起点。

## 5. 明确失败清单与证据限制

完整UID去重失败集合未形成：搜索每文件超过20命中会截断，diag多行节点不得按命中行计数；本轮不运行整理器。因此以下是亲读可见下界与明确类别，不是full失败总数。原始全部参数/消息仍在两stdout与同步diag，不删失败。

- 双架构CompositionTests.WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked：stdout25/26及diag9322、节点UID1bbc964c-b6e5-8d5d-8d59-fba9c3c3ca06（结果节点及FailedTestNodeStateProperty已核）。参数(0,False,4294901760,4294901760,0,1,0,0,0,True,True,False,0,7)。原始消息：Assert.AreEqual failed. Expected:<0>. Actual:<-2147024809>. 'expected' expression: '0', 'actual' expression: 'present(connection)'. 即E_INVALIDARG/0x80070057。
- 同方法双架构还亲读source8、source7/tile4、source8/tile4、source12/tile4（firstPixel0/second4294901760）；x86另亲读source13/tile4（firstPixel0）、14/tile4（2155872256两像素）、15/tile4（1431633920两像素）及alpha0.1/0.2/0.8、浮点tile1/2/3/4与scaling/consumer分支。共同前缀mode0/strokeFalse/relationship0/scale1/directGeometry0/join0/cap0/curvesTrue/drawingImageTrue/drawingMaskFalse/affine0。stdout88–152的原始尾参数分别15,4,0.1；15,4,0.2；15,4,0.8；15,1,0.001,True,0；15,1,0.001,True,1；15,2,1,True,2；15,3,0.33333334,True,3；15,4,0.2,True,0；15,4,0.001,True,0,1及15,4,0.001,True,0,2，全部同present E_INVALIDARG。未补齐全部行/UID，不声称全集。
- 双架构WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements（无参数）：x86 stdout367–377、x64 295–302，仍经CombinedGeometry在present(connection)同E_INVALIDARG失败。此为DrawingImage/组合业务差异待定位，不把它归为HW环境。
- x86 WhenStage4HardwareWicCacheDrawsThenHitAndSourceReplacementPreservePixels两行参数(4278255360,65280)/(4278190335,16711680)，stdout378/399；WhenStage4HardwareWicCandidateIsWriteLockedThenCommittedFrameSurvivesAndSameGenerationRetries同两参数，504/525；WhenStage4HardwareCacheHasTwoDevicesThenBothCommittedKeysHitWhileWicIsWriteLocked(False)/(True)，545/566。消息均Expected0 Actual-2003303421，send(pointer, (uint)packet.Length, 0, channel)，Command0xC,16bytes。属于WIC命令/资源业务链失败，是否共同合同污染须定位，不能全归环境。两device节点在diag15306/15359，搜索输出UID前部截断，故不登记未核完整UID。
- x86 WhenStage4HardwareLazyWicReentersThenFrozenCacheFrameAndStreamLifetimeSurvive(action0/1/2/3/4)，stdout419/436/453/470/487：Expected1 Actual0，actual expression streamDisposals；是流生命周期不收敛断言，不能由最终finally断言反推出首次失败位置。

x64 stdout共302行，仅搜索到CombinedGeometry及TileCalibration两个失败家族；x86586行，额外上述四个Stage4家族。搜索截断/未逐UID终态去重，均不称“全部失败”。中断官方geometry没有完成失败消息，不把它强记一个Failed；两stdout source-clip Passed也不顶替full。

## 6. 本full对已有修复的确证范围

- 新defaultstate：WhenPartitionPresentStateRunsThenDynamicStateMatchesOfficialAsync及WhenPartitionPresentStateRoleRunsThenRealContentAndSnapshotsShareState在两完整父diag搜索均无节点；未见开始/通过，不确定未调度还是中断未到，不绑定IK/IM历史绿到本SHA。
- HY两方法WhenHyDeviceStateGroupRunsThenUnrenderedSnapshotsContainNoDeviceAsync / WhenHyDeviceStateRoleRunsThenSnapshotsMatchIndependentLayout：两父diag均无；未渲染NoDevice初态独立回归本full未证执行/通过。
- ListenerBroadcast两方法/类型：两父diag均无，未证开始/通过。IC/IF显式探索入口也无节点，不冒默认覆盖；初态1/1和广播/动态完整合同只保留此前各自SHA有限证据。
- 可明确确证的兄弟：WhenPartitionManagerIsInitializedTwiceThenOneDeinitializationKeepsAsyncWorkAlive，UID14a5f3f8-b670-85ef-ada4-e02e1895d516，两diag733 PassedTestNodeStateProperty；不是设备状态payload通过。CompositionNotification的通知家族(0) UID18c284de-50e6-8857-98f4-a8eb14352b03两diag5001 Passed；x64(1) UID140233d3-0326-86dc-b350-3f0b6080ecb0在5012 Passed。只陈述已亲读节点，不外推整个兄弟集合。
- x86最后官方geometry之前，diag16132有历史candidate内容Passed，仍不是默认动态回归通过，更不等于实际HWbackend验证。IH/IA/ID/COMfill历史有限验收保留，当前新Native full未完成不能替换其身份账。

## 7. 官方参考来源、输入清单与工具问题

IP预生成真实task：x86 d94a1acdf3a54dc6a6914b3862a51685，x64 3b69be49685342ceb485ee74b8d072f8。reference-readback/<arch>/Results/ip-reference-<arch>/supervision.json各39行全文：filtered两个正式生成方法（Arial及Tile），Timeout120，Exit0，非timeout/cancel/kill/drain失败，test SHA/MVID与对应full一致；这是参考生成而非重跑full。

reference-readback/<arch>/ReferenceData/original-wpf.json头部真实字段：分别05:19:15.1465431Z/05:19:40.7144011Z、X86/X64、.NET10.0.13、各生成task根官方DLL与上述官方SHA/version、SoftwareOnly，明确Pbgra32量化不是float/HDR oracle。full回收同名文件CapturedAtUtc变为05:20:54.0067152Z/05:22:15.1930546Z、路径对应full task：full本身生成器会重写ignored ReferenceData，不能误说回收文件仍是预生成字节。

来源代码：WpfReference/ReferenceBehaviorTests.cs394–425核实际模块集合等于NativeMethods.LibraryPath；记录SchemaVersion/CapturedAtUtc/Runtime/Architecture/PresentationCore/NativeDll/NativeVersion/NativeSha256/RenderMode/Limitation/Samples，写ReferenceResultsPath下随机名并复制original-wpf.json，末断言450 samples。未读取全部450样本，不伪造差异计数。CompositionTests.ReferenceCalibration.cs13–40从BaseDirectory/ReferenceData/original-wpf.json取Samples和真实SourceMode/Tile/Scaling/Consumer等字段；不存在可擅造officialReferenceData字段。Package.targets通过ComAcceptanceReferenceInputDirectory只链接glyph-arial-original.json及original-wpf.json到包内ReferenceData。不改tracked golden。

本地输入目录为artifacts/ip-integrated-full/x86/com-acceptance-package与x64/com-acceptance-package，回收为full-x86/full-x64，不是上传artifacts根。实际sourceDirectory API调用原始日志本轮工具不可回溯，故此路径按IP既有记录及打包日志/清单交叉支持，不冒称亲读上传API调用。package-files.txt分别420/421行（清单条目数，不是独立实际文件枚举计数），首部正式exe/test/MTP，尾部Contracts、Native/wpfgfx_cor3.dll、两参考、aot.sha256、自清单；同架构runtime资产命名可见。PackageSelfContainedComAcceptance只复制正式执行依赖、RuntimePackAsset、Contracts、Native、显式参考，无Results/回收目录通配上传。未逐项全manifest/hash审计，未取得实际总字节；明确未知，不执行任何程序为拿总字节。

ignored IpReadback.cs本拟扫描Evidence/x86/x64、package-files和diag写ip-readback.json；readback-audit/Results当前仅aot-identity，未见该输出。未运行/重试该工具。IP整理包曾WarningLevel0发布只是取证工具问题，不是正式full压警告规则。正式两full默认分析器/并行未改，生产与项目未改；原build日志可搜索到analysislevel_10_default及/analyzer，未以整理包设置覆盖正式配置，未声称零warnings或全日志审计。

## 8. 下一唯一精确根因任务与有限验收设计

下一仅处理共享RunVisualCacheConsumer的真实surface提交/退出所有权合同：以本次两架构官方geometry UID18e39062-2997-8a5c-bf02-39b275a17ace的Commit E_HANDLE→DestroyBefore→C0000374为入口，先核官方合法async初始化、connect(0)、PresentPacket非零duplicate Event的所有权移交/失败回收、完成等待及通道/连接/partition释放偏序，再决定最小共享夹具迁移；不是立刻给生产贴补丁或预设event就是堆损坏根因。

当前VisualCache.cs84为connect(1)，初始124和更新403附近PresentPacket均未填Event，present为SameThreadPresent；finally无论失败都会destroy。BP旧迁移已撤回，原因之一原生前置像素失败且无真实surface合同验收；本次原生前置已实际越过，可据新现场推进合法异步Event ownership有限验证，而非无限机械迁移/撤回循环。不得跳cleanup、删consumer、压断言或以空image事件成功冒内容。

后续另行授权的有限验收应保留原surface/shader/两帧mutation/caller-release/真实内容及全部业务断言，独立official/candidate/official-repeat、每次交接事件唯一归属、成功与失败路径回收、destroy/disconnect/deinitialize/线程收敛证据。若原生前置再次阻止，明确保留该前置阻塞，同时以同合同有效普通surface场景有界核ownership与真实内容，不用“无surface控制”宣称历史geometry全通过。定位后才做必要修复/双架构兄弟和最终full，不在IQ重跑。

CombinedGeometry/DrawingImage E_INVALIDARG和x86 WIC/流释放差异各自仍开，不能用共享合同修正自动消账。所有HW/backend、非零sync、Available、loss、media/resource、full四格、x64Event0仍开；IO两格独立绿，COM两格中断。

## 9. 写后核验登记

本报告全文及三共享最新IQ受改节/最后补记已写后分段回读；历史范围准确登记：handoff只新增IQ并保留IP及其后历史；session-summary/issues只在标题后新增IQ，不修改IO及更旧历史。IQ核证已收口，不以未形成full计数伪造成功。

## 10. IP阶段历史保留（不作为当前进行中状态）

IP初始授权仅独立报告及必要显式只读结果整理；正常依赖图、独有artifacts/ip-integrated-full、self-contained10.0.13，清洁正式包与IO独立，每架构唯一full内1500/外1800、默认并行不变。阶段1先读规则及IO/IN/IM等，尚未构建/运行；阶段2两架构NativeAOT独立正常发布，ComAcceptance首次及纳入新参考最终发布成功，预生成官方task d94a1acdf3a54dc6a6914b3862a51685 / 3b69be49685342ceb485ee74b8d072f8 Exit0；阶段3 x86唯一full d41f442050e743ddbf359ec0e4a9516c回收、57.534秒子C0000374，原登记待读父TRX和失败并继续x64。以上“尚未/即将/待读”均历史，现由IQ终态替代，不能当任务仍悬挂。
