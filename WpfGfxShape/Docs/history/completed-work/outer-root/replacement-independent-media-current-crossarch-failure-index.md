# Stage19 当前x86默认full失败实例索引

## 本轮证据恢复与定向关联（不减91）
详细见replacement-reference-ten-recovery-closeout.md。独立执行核验子智能体亲读既有factory3/3与MFx861/1 child0完整原件，纠正下方“原件缺失”历史错误，不重跑，不将定向绿推新full。task04bd64c73db547de8d7010fb96450bc3有限精确8体8Passed覆盖原#9,10,11,12,87,88,89,90；同格式Pbgra450/0、跨format57差保留。#52/#84仍BlockedUntrustedGlyph/NotRun，旧glyph自报候选F4D4不是官方5EBD；#90没有原版source15 golden，只既有float合同通过，#87只16数量断言，不夸大。旧1843/1752/91原样保留，79归因未证不泛环境；x64timeout分母Unknown。独立GetSetSampleDuration未被MF组合桥调用，x64NotRun。此次normal0/tests0/COM1/sandbox1/Native0/production0/full0；audioendpoint0/AVI16samples缺口/AVsync/cancel/HWloss/resources/fullOpen不变。

## PROPVARIANT 后继关联（91条不减）
见 replacement-propvariant-crossarch-contract-closeout.md。交接新factory3passed非本次亲读，旧3failed/关闭0及下方91条均保留；旧full1843/1752/91不相减，x64timeout分母Unknown。指定MF回收原件缺失，不能认证Passed/Failed；x64新门NotRun，十ReferenceData未跑，audio/AVsync/cancel/HWloss/resources/fullOpen不变。此次新执行0。

## factory residual 后继补记（不删除下方91条）
旧FQN WpfGfxShape.ComAcceptance.MediaExportsTests.WhenCandidateMediaBackendIsNotImplementedThenFactoryFailsWithoutAnObject(0/1) 映射新 WhenCandidateFactoryReceivesRealEventProxyThenPlayerAndRollbackReferencesAreReleased(0/1)；旧 WhenCandidateCreatesMediaThenSystemModuleMatchesLoadedPresentationCoreRuntime 映射新 WhenCandidateCreatesMediaThenOnlyIndependentNativeModuleIsLoaded。旧测试测candidate:true且无外provider参数，不是独立用户provider合同；只替换旧内建官方委托前提，不改其他provider测试。方法数和DataRow行数净0；未新full，分母Unknown，不推1843新full。taska43bbff6d6a04d02bfb795a357722be2精确新3体executed3/passed0/failed3，模块误筛测试exe/dll；修正后仅编译成功，无系统复验，不从旧91减。三份实际CloseShutdownRelease/回滚/finalrefs0及独立模块名单已读。详见replacement-current-native-factory-contract-closeout.md residual及.coding-agent/current-native-factory-residual-receipts.md。十ReferenceData、两PVAR前置及其他未证项原状态保留。

原件T=`WpfGfxShape/artifacts/stage19-final-crossarch/readback-x86/Results/stage19-final-default-x86/TestResults/child.trx`。下列L均为T的1-based行号，不依赖第三工程。Counters L15083亲读：total/executed1843，passed1752，failed91，其余结果项0。每行testId唯一；参数为原testName参数。FQN由ErrorInfo业务栈/Message复核，定义补核首三项L4730/4738/4742。除明确fixture外，不凭失败表象判定生产根因。

分类代码：F=确定fixture/测试前置缺陷；U=实际失败但归因/能力未证（不是环境免责）；P=已由契约和源码确定的生产缺陷（仅错误码不足以认定）。分批写入，不把尚未完成条目称为完成。

共用原文消息（XML实体仅解码供阅读）：
- M1：`Assert.AreEqual 失败。应为: <8>，实际为: <4>。‘expected’表达式:‘8’,‘actual’表达式:‘IntPtr.Size’。`
- M2：`CollectionAssert.AreEqual 失败。索引 0 处的元素不匹配。` 下一行 `预计值: <1, 0, 1>` 下一行 `实际值: <1.25, -0.25, 1>`。
- M3：`Test method {FQN} threw exception:` 下一行 `System.NotImplementedException: The method or operation is not implemented.`
- M4：`Test method {FQN} threw exception:` 下一行 `System.IO.FileNotFoundException: Could not find file 'C:\CodingAgentSandbox\Tasks\9e6f1257c0ff433ea553d70ce4a18db5\ReferenceData\original-wpf.json'.`
- M5：`Assert.IsTrue 失败。‘condition’表达式:‘Math.Abs((int)((actual >> shift) & 255) - (int)((expected >> shift) & 255)) <= 1’。 Pixel 0x00F3F3F3, expected 0x00563412`

## 批1：1—15（已逐条读Message及必要栈）
|#|T起始/消息栈行|testId|FQN（完整类型+方法）及参数|分类/已到阶段/第一Message|
|---|---|---|---|---|
|1|9/12..16|19316999-ff78-8902-bffd-45b2336e366a|WpfGfxShape.Tests.MediaFoundationResourceTests.WhenOriginalAviIsReadBySystemSourceReaderThenAllVideoFramesReachEosAndClose|F：硬8位宽断言，SourceReader读取前；M1。尚不认证AVI EOS。|
|2|29/32..39|12eb5401-bc89-8fc6-9c43-5d22a3161a54|WpfGfxShape.WpfReference.FloatReadbackTests.WhenOriginalFloatTargetDrawsBitmapThenSrgbClampingIsRecorded (6)|U：official float readback值不符；M2。|
|3|43/46..54|17bf5b86-6daa-8c57-a564-73cd2737520e|WpfGfxShape.WpfReference.FloatReadbackTests.WhenOriginalFloatTargetDrawsBitmapThenSrgbClampingIsRecorded (10)|U：同M2，独立参数实例。|
|4|58/61..68|172b187b-4aa7-84e2-b77c-ecf31f3e6647|WpfGfxShape.WpfReference.ReferenceBehaviorTests.WhenPaddingConsumersUseScalingScopesThenOriginalBehaviorIsVerified|U：M3；Renderer.Render→BitmapVisualManager.Render。|
|5|72/75..82|174ca116-7a10-8e02-a666-b05884b5fd09|WpfGfxShape.WpfReference.ReferenceBehaviorTests.WhenExplicitPaddingTileIsSampledThenOriginalSourceVariantsAreRecorded|U：M3；Renderer.Render→BitmapVisualManager.Render。|
|6|97/100..107|1e7fe804-bd96-8833-b59c-0228c3db10db|WpfGfxShape.WpfReference.ReferenceBehaviorTests.WhenTileFixturesAreRenderedThenOriginalWpfEvidenceIsRecorded|U：M3；Renderer.Render→BitmapVisualManager.Render。|
|7|141/144..151|116f8755-fe58-88a9-a3ba-9ee74313b245|WpfGfxShape.WpfReference.WorkingFormatContractIndependentTests.WhenSamePublicGeometryRequestsDifferentFormatsThenActualFormatIsRecorded|U：M3；Renderer.Render→BitmapVisualManager.Render。|
|8|841/844..847|12b838e1-e9b0-8a64-b36e-36c024a5e11b|WpfGfxShape.ComAcceptance.CompositionTests.WhenTransferredBitmapChangesThenSoftwareDrawPixelsChange (True,False,True)|U：`Assert.IsTrue 失败。‘condition’表达式:‘raw.All(pixel => pixel.W == 1 && pixel.X == 1 && pixel.Y == 0)’。`；system软件float像素阶段。|
|9|1176/1179..1188|19594365-cf0b-8531-80b7-2aa8c645531c|WpfGfxShape.ComAcceptance.CompositionTests.WhenDrawingImageVisualMaskIsRenderedThenCompletePbgraSurfaceMatchesOriginalWpf (17,0)|F：M4；File.ReadAllText，golden读取前。|
|10|1195/1198..1206|144a7ef2-c69f-8a26-b5da-54a632e43600|WpfGfxShape.ComAcceptance.CompositionTests.WhenDrawingImageVisualMaskIsRenderedThenCompletePbgraSurfaceMatchesOriginalWpf (17,3)|F：M4；File.ReadAllText。|
|11|1213/1216..1224|11797fd5-dcf5-8e4e-9e42-18c9ef02bfd6|WpfGfxShape.ComAcceptance.CompositionTests.WhenDrawingImageVisualMaskIsRenderedThenCompletePbgraSurfaceMatchesOriginalWpf (18,3)|F：M4；File.ReadAllText。|
|12|1453/1456..1464|1379a1aa-75bd-8efb-9fe1-4e79ec35236b|WpfGfxShape.ComAcceptance.CompositionTests.WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements|F：M4；File.ReadAllText。|
|13|1498/1512..1515|163b209d-b0ee-847d-8693-99661ef36347|WpfGfxShape.ComAcceptance.D3DImageTests.WhenStage4HardwareCacheGeometryDrawsThenHitAndMutationPreservePixelsAfterCallerRelease (4278255360,65280)|U：M5；AssertCombinedPixel→RunVisualCacheConsumer；实际模块SHA F4D4F13DD2F6DD8EFB47B3455C23921FA8F1B87D1F2494E27A765DE6D29DA68D（L1504），硬件窗口可见/未cloak/foreground，不能仅称环境。|
|14|1522/1532..1535|1a425113-e5de-887c-ba34-4358042833d3|WpfGfxShape.ComAcceptance.D3DImageTests.WhenStage4HardwareCacheGeometryDrawsThenHitAndMutationPreservePixelsAfterCallerRelease (4278190335,16711680)|U：M5；AssertCombinedPixel→RunVisualCacheConsumer。|
|15|1540/1550..1553|1e07eac4-3e65-8e7f-86dd-fad6f0403e27|WpfGfxShape.ComAcceptance.D3DImageTests.WhenStage4HardwareCacheConfigurationFailsThenRecoveryAndMutationRenderRealPixels (4278255360,65280)|U：M5；AssertCombinedPixel→RunVisualCacheConsumer。|

## 批2：16—30
本批FQN前缀均为 `WpfGfxShape.ComAcceptance.D3DImageTests.`；均U。16—27第一Message均M5，栈均AssertCombinedPixel→RunVisualCacheConsumer→表中方法，已到初次可见窗口像素断言，尚未证明后续cache/retry。

|#|T起始/消息栈行|testId|方法及参数|
|---|---|---|---|
|16|1559/1569..1572|1c311245-f56d-88db-b3b7-8bd6565da465|WhenStage4HardwareCacheConfigurationFailsThenRecoveryAndMutationRenderRealPixels (4278190335,16711680)|
|17|1577/1587..1591|1de8aecd-ec9e-831a-92c0-97c897a8d822|WhenStage4HardwareWicCacheDrawsThenHitAndSourceReplacementPreservePixels (4278255360,65280)|
|18|1598/1608..1612|197f02db-2800-8760-bf5b-f09aaa116829|WhenStage4HardwareWicCacheDrawsThenHitAndSourceReplacementPreservePixels (4278190335,16711680)|
|19|1618/1628..1632|11194225-4f95-866d-a08b-aa2ba00ccdd7|WhenStage4HardwareLazyWicReentersThenFrozenCacheFrameAndStreamLifetimeSurvive (0)|
|20|1639/1649..1653|16d0ec26-f97d-8928-a222-03fbddeb3447|WhenStage4HardwareLazyWicReentersThenFrozenCacheFrameAndStreamLifetimeSurvive (1)|
|21|1659/1669..1673|1872055b-1354-8b7c-9335-95091b1ef30e|WhenStage4HardwareLazyWicReentersThenFrozenCacheFrameAndStreamLifetimeSurvive (2)|
|22|1679/1689..1693|168b8cd4-a952-85c0-896a-38f2cdb32f76|WhenStage4HardwareLazyWicReentersThenFrozenCacheFrameAndStreamLifetimeSurvive (3)|
|23|1699/1709..1713|1fa69fb3-65cf-83e9-a1a7-8d9751225bc2|WhenStage4HardwareLazyWicReentersThenFrozenCacheFrameAndStreamLifetimeSurvive (4)|
|24|1719/1729..1733|17eef5c8-05f7-80a1-981e-62d1751e1f91|WhenStage4HardwareWicCandidateIsWriteLockedThenCommittedFrameSurvivesAndSameGenerationRetries (4278255360,65280)|
|25|1740/1750..1754|12f170ce-3ec4-8f31-96c6-7b2f12c5c550|WhenStage4HardwareWicCandidateIsWriteLockedThenCommittedFrameSurvivesAndSameGenerationRetries (4278190335,16711680)|
|26|1760/1770..1774|1a7331dd-3ad6-86dd-b9ce-4c54eadbeffa|WhenStage4HardwareCacheHasTwoDevicesThenBothCommittedKeysHitWhileWicIsWriteLocked (False)|
|27|1781/1791..1795|1ac32cda-4545-81eb-b4db-a89cb5c4351e|WhenStage4HardwareCacheHasTwoDevicesThenBothCommittedKeysHitWhileWicIsWriteLocked (True)|
|28|1801/1884..1888|15d158a4-e944-8881-bbfc-271bff3c3332|WhenFreshNativeGatePassesThenOfficialAndCandidatePublicTargetsAreCompared|
|29|1896/1979..1982|1d4afea9-fe59-8b80-9b90-30c096032d34|WhenNativeOwnerThreadSubmitsRedBlueRedThenFreshDesktopContentIsRequired (1)|
|30|1989/2072..2076|19b3355c-10bc-80a4-b748-5cbfce845d4b|WhenNativeOwnerThreadSubmitsRedBlueRedThenFreshDesktopContentIsRequired (3)|

28/29第一Message原文：`Assert.IsTrue 失败。‘condition’表达式:‘passed’。 C:\CodingAgentSandbox\Tasks\9e6f1257c0ff433ea553d70ce4a18db5\Results\bj-fresh-1.json`；30同消息但文件`bj-fresh-3.json`。栈CheckFreshDesktopDelivery→WhenNativeOwnerThreadSubmitsRedBlueRedThenFreshDesktopContentIsRequired，28再到public比较入口。已到native fresh gate，DesktopDuplication 0x887A0027/无当前提交帧，official/candidate比较未证，不能与x64的native gates passed混淆。

## 批3：31—45
FQN前缀仍为`WpfGfxShape.ComAcceptance.D3DImageTests.`，均U。

|#|T起始/消息栈行|testId|方法及参数|消息/阶段|
|---|---|---|---|---|
|31|2081/2101..2107|118b1250-19be-8107-ba64-b1750f57c1d3|WhenHistoricalCaptureCandidateRunsThenExpectedUpdatesRemain (True,False,4278255360,65280)|M5；RunVisualCacheConsumer→RunHistoricalCaptureControl初次像素|
|32|2114/2134..2140|1e784501-7af7-85ef-9cbd-48b8ae94a6c0|WhenHistoricalCaptureCandidateRunsThenExpectedUpdatesRemain (True,False,4278190335,16711680)|M5；同栈阶段|
|33|2146/2166..2172|1817ff57-e333-8653-b23b-985784a01e97|WhenHistoricalCaptureCandidateRunsThenExpectedUpdatesRemain (False,False,4278255360,65408)|M5；同栈阶段|
|34|2178/2198..2204|1e3d5f5f-ccd2-8af8-ab63-b6dcdc121649|WhenHistoricalCaptureCandidateRunsThenExpectedUpdatesRemain (False,True,4278190335,16711808)|M5；同栈阶段|
|35|2210/2230..2236|12075ab2-10ad-8d10-bc58-007453dc09ea|WhenHistoricalCaptureCandidateRunsThenExpectedUpdatesRemain (False,True,0,128)|M5；同栈阶段|
|36|2242/2245..2288|18e39062-2997-8a5c-bf02-39b275a17ace|WhenHistoricalCaptureOfficialGeometryRunsThenExpectedUpdatesRemain|M6；HistoricalProcessIsolation.RunAsync；子进程exit2，未timeout/kill|
|37|2293/2296..2340|1c6be0d5-e7e9-8416-85f5-e4e394a6710c|WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain|M6；同栈，子进程exit2，未timeout/kill|
|38|2352/2362..2368|1dc5baf7-ad20-8107-aba8-8f62d9a39e17|WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels (40,40,21,False,0,False)|M7；RunPublicShaderFrames GetPixel|
|39|2374/2384..2390|108742fb-3d56-8287-991f-4b22fb6a5fdd|WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels (120,80,21,False,0,False)|M7；同栈阶段|
|40|2396/2403..2410|1c02f3cb-6784-82ac-838e-bf32a84a0213|WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels (40,40,21,True,0,False)|M8；PresentNativeControl→RunPublicShaderFrames|
|41|2416/2423..2430|14ed735c-b0f8-82de-a25f-eefda6ec8de9|WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels (120,80,22,True,0,False)|M8；同栈阶段|
|42|2436/2443..2450|15291d4a-2b12-809f-9091-80f2e1e5889f|WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels (40,40,21,True,1,False)|M8；同栈阶段|
|43|2456/2463..2470|105bcc70-f151-817f-b468-07b2d2f087f1|WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels (40,40,21,True,2,False)|M8；同栈阶段|
|44|2476/2483..2490|15e72681-ae9f-8e8a-8630-ff83fba836c8|WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels (40,40,21,True,3,False)|M8；同栈阶段|
|45|2496/2503..2510|172e7d85-04d6-82cc-90dc-b18ce8d1778a|WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels (40,40,21,True,0,True)|M8；同栈阶段|

M6第一Message原文前缀：`Assert.AreEqual 失败。应为: <0>，实际为: <2>。‘expected’表达式:‘0’,‘actual’表达式:‘exitCode’。` 后接原件隔离目录及监督JSON（保留于T2245..2285、2296..2336，不冒全文已转录）。36目录`Results\stage19-final-default-x86\historical-isolation\WhenHistoricalCaptureOfficialGeometryRunsThenExpectedUpdatesRemain\cf192a355d074641938b4ade7fc38c1c`；37目录同层Sampler方法名/`c10a54c6b8e54664b4f978a808629273`，均位于task根。M7：`Assert.AreEqual 失败。应为: <5649426>，实际为: <15987699>。‘expected’表达式:‘0x00563412u’,‘actual’表达式:‘GetPixel(dc, 16, 8)’。` M8：`Assert.AreEqual 失败。应为: <0>，实际为: <141953144>。‘expected’表达式:‘0’,‘actual’表达式:‘result’。`

## 批4：46—60
类型前缀A=`WpfGfxShape.ComAcceptance.`，下表与方法拼接为完整FQN。

|#|T起始/消息栈行|testId|A后类型.方法及参数|分类/第一Message/阶段|
|---|---|---|---|---|
|46|2516/2523..2530|15d634ec-21d2-8e73-b808-4f1e30b75a93|D3DImageTests.WhenHardwareShaderGenerationIsReplacedThenTwoFramesConsumeLegalBytecode|U；M8；PresentNativeControl→RunPublicShaderFrames，未到replacement证明|
|47|2538/2545..2552|1f833676-6572-8932-8888-63e5b0bbc626|D3DImageTests.WhenMalformedHardwareShaderFallsBackThenIdentityPixelsAndBadShaderFifoAreReal|U；M8；同栈阶段|
|48|2560/2567..2574|13c24575-b993-8a9c-a589-0175dbbe6334|D3DImageTests.WhenShaderChannelAndDeviceAreClosedThenNewPublicTargetRendersReplacementGeneration|U；M8；同栈阶段|
|49|2621/2631..2636|1f84a53c-28e9-87cc-96d5-17c2eea1e9b4|D3DImageTests.WhenNativeGpuPresentsThenTopmostWindowIsObservable|U；M5；AssertCombinedPixel→RunNativeReadbackControl|
|50|2644/2663..2668|1eded079-5019-809c-bf7c-07ef4caf0e29|D3DImageTests.WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive|U；M5；AssertCombinedPixel→RunVisualCacheConsumer|
|51|2676/2695..2700|14e862b3-9b71-843a-9ff5-67f779272c33|D3DImageTests.WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive|U；M5；同栈阶段，official对照也红|
|52|2996/2999..3005|1ddba233-aca8-81da-add4-441e56188ede|DdUploadBoundaryTests.WhenPackageIsInspectedThenRuntimeAndInputSizesAreRecorded|F；M4；File.ReadAllText读取golden|
|53|3457/3460..3466|1233ae02-ffd4-81e1-8ad9-377eaac61f4b|FloatHdrPublicTargetTests.WhenPublicFloatTargetClearsHdrThenRawReadbackAndLockedRetryAreRecorded (True)|U；M9；original float readback|
|54|3531/3534..3537|178a34b5-0373-8910-b8de-e057d371f90a|IndependentAudioTests.WhenCompleteWaveRendersThenRealEndpointClockAndDrainAreRequired|U；M10；Tests.IndependentAudioTests.WhenCompleteWaveIsRenderedThenEndpointClockAndDrainAreRequired，IndependentAudioOutputTests.cs:67，真实endpoint/clock未通过|
|55|3542/3545..3549|1f0d0305-ba27-8260-a5d0-f2e28885ac40|IndependentAudioTests.WhenAudioDecodesThenFixedOraclePassesOnDedicatedMta|U；M11；Tests.IndependentAudioTests.WhenWaveAndAviAreComparedThenBothFixedOraclesAreRequired，IndependentAudioComparisonTests.cs:160；AVI精确样本不能放宽|
|56|3578/3581..3584|16e1125f-d67f-8584-8e68-a3640f66f786|IndependentNativeAbiTests.WhenStage14ProductionNativeIsLoadedThenFactoryGateAndEventAbiAreReal|F；`Assert.AreEqual 失败。应为: <X64>，实际为: <X86>。‘expected’表达式:‘Architecture.X64’,‘actual’表达式:‘RuntimeInformation.ProcessArchitecture’。`；架构硬编码前置|
|57|3605/3608..3613|126b509f-bcec-8336-9fd4-6800088debca|IndependentVideoPlaybackOwnerTests.WhenStage15InternalBridgeRunsThenRealResourceAndControlsComplete|U；`Assert.AreEqual 失败。应为: <40>，实际为: <0>。‘expected’表达式:‘40’,‘actual’表达式:‘owner.PublishedFrames’。`；Tests.MediaFoundationResourceTests.WhenInternalBridgePlaysThenResourceNotificationsAndDrawVideoAreReal，IndependentMediaResourceBindingSystemTests.cs:92/143|
|58|3621/3624..3630|1e0ddbfd-84ef-84fe-a5df-80629973bc61|IndependentVideoPlaybackOwnerTests.WhenPublicComProxyRunsThenPacketsAndRealVideoControlsPass|U；M12；Tests.MediaFoundationResourceTests.WhenComControlsPauseSeekResumeThenRealEventsAndDrawVideoRemainOrdered，IndependentMediaControlsTests.cs:263/275；事件序列未契约复核不能直接改expected|
|59|3635/3638..3644|10b5500f-1fb7-8d15-8145-87d84f9aa33e|IndependentVideoPlaybackOwnerTests.WhenScrubbingRunsThenRealPausedFramesAndControlsPass|U；M12；同业务栈|
|60|3649/3652..3658|1d9cd25e-e32d-89be-a42e-daa6103d5437|IndependentVideoPlaybackOwnerTests.WhenControlsAndEventsRunThenPauseSeekResumeReachProductionDrawVideo|U；M12；同业务栈（桥接FQN已由定义L11838复核）|

M9：`CollectionAssert.AreEqual 失败。索引 0 处的元素不匹配。` 下一行`预计值: <0.5019608, 0, 0.06000361, 0.5019608>` 下一行`实际值: <0.625, -0.125, 0.06172835, 0.5>`。
M10：`Assert.IsTrue 失败。‘condition’表达式:‘hr == 0 && output.Status == "RenderingSubmittedAndClockAdvanced" && before == Convert.ToHexString(SHA256.HashData(pcm))’。`
M11：`Assert.IsTrue 失败。‘condition’表达式:‘wavResult.Passed && aviResult.Passed’。 Both independent container hard gates are required; see saved comparison evidence.`
M12：`CollectionAssert.AreEqual 失败。索引 1 处的元素不匹配。` 下一行`预计值: 7` 下一行`实际值: 11`。

## 批5：61—75
除61外，FQN前缀均`WpfGfxShape.ComAcceptance.MediaExportsTests.`。全部U，旧预期待官方契约与源码核查，不将E_NOTIMPL预期机械改为另一失败码。

|#|T起始/消息栈行|testId|方法及参数|第一Message/阶段|
|---|---|---|---|---|
|61|3662/3665..3671|1a2cbec1-b9af-85ab-8bdb-b71017fd53c3|WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenDurationControlsAndReplayRunThenProductionContractIsPreserved|M12；IndependentMediaControlsTests.cs:263/275，同58|
|62|3954/3957..3962|1a8d1446-c38f-8c45-ab36-5bec5ad3905c|WhenCandidateMediaBackendIsNotImplementedThenFactoryFailsWithoutAnObject (0)|M13；factory返回E_INVALIDARG/null，非创建成功证明|
|63|3969/3972..3977|1761eab5-6bb1-84e4-b61d-d1f876c9a617|WhenCandidateMediaBackendIsNotImplementedThenFactoryFailsWithoutAnObject (1)|M13；同阶段|
|64|3994/3997..4001|1553061c-3701-812c-b5ba-4e1f1b2595e5|WhenRealSystemMediaIsTransferredThenBothChannelModesConsumeTheBatch (1)|M14；MilChannel_CommitChannel|
|65|4008/4011..4015|1e59a6bd-01ac-8d4b-85e4-830c428cced8|WhenRealSystemMediaIsTransferredThenBothChannelModesConsumeTheBatch (0)|M15；MilComposition_SyncFlush|
|66|4023/4026..4030|123cbaaa-636e-8c53-90b2-2d2e17196136|WhenProviderCallerOwnsReferenceThenPlayerSurvivesMediaCallerRelease|M16；provider QI失败，尚未证caller-release lifetime|
|67|4038/4041..4046|1f32c53c-528c-862c-88a9-54320ad4bc0b|WhenRealCallbackClosesAndReturnsLastCallerThenCallbackReferenceKeepsPlayerAlive|`Assert.AreEqual 失败。应为: <(0, 1)>，实际为: <(0, 0)>。‘expected’表达式:‘(0, 1u)’,‘actual’表达式:‘(result, remaining)’。`；callback/reference断言|
|68|4056/4059..4064|19d08c99-eb12-8882-883e-32bde0febb05|WhenCandidateMediaOwnershipIsMeasuredThenRealReferencesBalance ("CommittedDestroy")|`Assert.AreEqual 失败。应为: <0>，实际为: <-2003303418>。‘expected’表达式:‘0’,‘actual’表达式:‘result’。`；MeasureOwnership，88980406归因未知，不改expected|
|69|4073/4082..4086|157e04a9-523e-80eb-9c2b-708321005bf8|WhenCandidateCreatesMediaThenSystemModuleMatchesLoadedPresentationCoreRuntime|M17；实际root wpfgfx版本1.0.0.0 vs PresentationCore10.0.1326.50504，不能为过测加载official|
|70|4094/4097..4103|131157a7-5606-815f-9d29-e682a067c773|WhenRealSolidVideoIsOpenedThenPublicVisualConsumesDecodedPixels (4294901760)|M18；ReadOriginalPixels→VerifySolidVideo，reference帧前置失败|
|71|4110/4113..4119|1a6177c7-1a8e-89bc-8934-235b803e32f5|WhenRealSolidVideoIsOpenedThenPublicVisualConsumesDecodedPixels (4278255360)|M18；同栈阶段|
|72|4125/4128..4134|1f4e47d2-d7ac-8c2b-b0a7-47bcae43b5e6|WhenRealSolidVideoIsOpenedThenPublicVisualConsumesDecodedPixels (4278190335)|M18；同栈阶段|
|73|4140/4143..4149|11324d3f-c0bb-8bb1-9506-5935f8f12006|WhenRealPlayersAreRepeatedlyCreatedAndClosedThenAllColorsRemainCorrect|M18；同栈阶段|
|74|4157/4160..4168|1177749a-83d9-87e0-ba67-6119254b2a07|WhenRealPlayersRunInParallelThenAllColorsRemainCorrect|M19；ReadOriginalPixels→VerifySolidVideo→Parallel工作项|
|75|4190/4193..4199|107f2012-d6d1-8f46-b582-8466db4c8e83|WhenRealPlayerIsReboundAndSourceIsReplacedThenNotificationsDriveAllColors|M18；ReadOriginalPixels→VerifySolidVideo|

M13：`Assert.AreEqual 失败。应为: <(-2147467263, 0)>，实际为: <(-2147024809, 0)>。‘expected’表达式:‘(unchecked((int)0x80004001), (nint)0)’,‘actual’表达式:‘(result, media)’。`
M14：`Assert.AreEqual 失败。应为: <0>，实际为: <-2147467262>。‘expected’表达式:‘0’,‘actual’表达式:‘((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel"))(channel)’。`
M15：`Assert.AreEqual 失败。应为: <0>，实际为: <-2003303418>。‘expected’表达式:‘0’,‘actual’表达式:‘((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilComposition_SyncFlush"))(channel)’。`
M16：`Assert.AreEqual 失败。应为: <0>，实际为: <-2147467262>。‘expected’表达式:‘0’,‘actual’表达式:‘((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)media)[0])(media, &providerId, &provider)’。`
M17：`Assert.AreEqual 失败。应为: <(10, 0, 1326, 50504)>，实际为: <(1, 0, 0, 0)>。‘expected’表达式:‘(managedVersion.FileMajorPart, managedVersion.FileMinorPart, managedVersion.FileBuildPart, managedVersion.FilePrivatePart)’,‘actual’表达式:‘(nativeVersion.FileMajorPart, nativeVersion.FileMinorPart, nativeVersion.FileBuildPart, nativeVersion.FilePrivatePart)’。`
M18：`Assert.Fail 失败。Original renderer did not provide a decoded frame: provider=80004002`
M19：`Test method WpfGfxShape.ComAcceptance.MediaExportsTests.WhenRealPlayersRunInParallelThenAllColorsRemainCorrect threw exception:` 下一行`System.AggregateException: One or more errors occurred. (Assert.Fail 失败。Original renderer did not provide a decoded frame: provider=80004002) (Assert.Fail 失败。Original renderer did not provide a decoded frame: provider=80004002) (Assert.Fail 失败。Original renderer did not provide a decoded frame: provider=80004002) ---> Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException: Assert.Fail 失败。Original renderer did not provide a decoded frame: provider=80004002`。

## 批6：76—91
前缀A=`WpfGfxShape.ComAcceptance.`。

|#|T起始/消息栈行|testId|A后类型.方法及参数|分类/第一Message/阶段|
|---|---|---|---|---|
|76|4207/4210..4216|17516b12-f33a-8040-b918-e2cbabba1811|MediaExportsTests.WhenRealVideoTargetIsLockedThenPresentFailsAndReleasedTargetRetriesOriginalPixels|U；M18；ReadOriginalPixels→VerifySolidVideo，lock/retry未证|
|77|4224/4227..4233|180bee6b-f1f7-8f7b-9053-8e4bca99b825|MediaExportsTests.WhenCallerReleasesRealPlayerThenResourceStillConsumesOriginalPixels|U；M18；同栈阶段，release消费未证|
|78|4241/4244..4247|1460db1a-1e2f-8476-876e-2488edf46bd0|MediaExportsTests.WhenRealPlayerClosesThenNativeDimensionsAndLengthAreCleared (False)|U；M20；Close后getter width|
|79|4254/4257..4260|1f253752-dbda-8f3e-b5c3-5cc94272166b|MediaExportsTests.WhenRealPlayerClosesThenNativeDimensionsAndLengthAreCleared (True)|U；M20；同阶段|
|80|4266/4269..4272|1b917cc1-f642-8ce8-ad5e-e2e05db61530|PublicAbiComparisonTests.WhenPublicAbiRunsThenSameRunOfficialContractIsUsed|U；M21；Original control exit1，比较未证；目录Results\publicabi\334bdd6ab7484885841097e39a93b9ad|
|81|4283/4286..4290|1588bddf-4ec2-892a-94cc-f207eb2011a3|PcoreComparisonTests.WhenSameSceneRunsThenOriginalAndCandidatePixelsMatch ("media")|U；M21；CompareSceneAsync original exit1，目录Results\pcore\media\default\53c650c95326461c8e88307fef2c1b79|
|82|4295/4298..4302|11c55c1a-dd8b-8232-b273-2bc2b089276f|PcoreComparisonTests.WhenSameSceneRunsThenOriginalAndCandidatePixelsMatch ("dispatcher")|U；M21；同栈，目录Results\pcore\dispatcher\default\d4a8a5cf25c2424fa9bd2653fc8a298a|
|83|4307/4310..4314|109a6a3c-ce02-81a4-b0e7-3a05e03e1672|PcoreComparisonTests.WhenContinuousMediaRunsOnceThenOriginalAndCandidateFramesMatch|U；M21；同栈，目录Results\pcore\media\default\2747fc6eb5744aff860a12272266074e|
|84|4359/4362..4368|1382095e-4915-8fc8-b4e9-e834216f7fca|ReferenceRevalidationEvidenceTests.WhenGeneratedOfficialReferencesAreConsumedThenTheirModuleIdentityMatchesThePackage|F；M4；File.ReadAllText读取golden|
|85|4433/4436..4440|14cf7d28-12a5-8269-8fd5-1638592860d9|SystemMediaSampleTests.WhenRealSystemMediaHasNoFrameThenSoftwareSampleLeaseCanBeClosed (False)|U；M16；provider QI，sample lease未证|
|86|4447/4450..4454|19848fa1-766b-8fc0-a18c-c08222f973e9|SystemMediaSampleTests.WhenRealSystemMediaHasNoFrameThenSoftwareSampleLeaseCanBeClosed (True)|U；M16；同阶段|
|87|4640/4643..4649|11043a0e-1ade-8cf8-a4a7-7a1a1d87590b|WorkingAlphaFirstDivergenceTests.WhenPublicDrawingAlphaIsObservedThenFloatContractIsPreserved (17,3,0,1,1,0.5)|F；M4；File.ReadAllText读取golden|
|88|4655/4658..4664|19b8475c-b586-8f83-bd10-2aa2dad3db3c|WorkingAlphaFirstDivergenceTests.WhenPublicDrawingAlphaIsObservedThenFloatContractIsPreserved (18,3,1,1,2,0.5)|F；M4；同阶段|
|89|4670/4673..4679|196dc425-bdb1-806d-b3c1-bf18eb7ba706|WorkingAlphaFirstDivergenceTests.WhenPublicDrawingAlphaIsObservedThenFloatContractIsPreserved (18,3,0,1,2,0.5)|F；M4；同阶段|
|90|4685/4688..4694|1db0829c-4aca-89e5-bb3f-239742ac6ab4|WorkingAlphaFirstDivergenceTests.WhenPublicDrawingAlphaIsObservedThenFloatContractIsPreserved (15,0,0,1,1,0.001)|F；M4；同阶段|
|91|4700/4703..4715|18541350-3a1c-8b39-8fb2-1408f84012da|X86ContentDifferenceReproTests.WhenAffineSource18MaskIsRenderedThenSameFormatMatchesFreshOfficial|U；M22；DUCE.Channel.SyncFlush→MediaContext.CompleteRender/CreateChannels→MediaSystem.ConnectChannels→Visual.set_VisualOpacityMask，尚未完成同format比较|

M20：`Assert.AreEqual 失败。应为: <0>，实际为: <-2147483629>。‘expected’表达式:‘0’,‘actual’表达式:‘((delegate* unmanaged[Stdcall]<nint, uint*, int>)table[19])(source.Media, &width)’。`
M21：`Assert.AreEqual 失败。应为: <0>，实际为: <1>。‘expected’表达式:‘0’,‘actual’表达式:‘original’。 Original control failed: ` 后接`C:\CodingAgentSandbox\Tasks\9e6f1257c0ff433ea553d70ce4a18db5\`与表内精确目录。
M22：`Test method WpfGfxShape.ComAcceptance.X86ContentDifferenceReproTests.WhenAffineSource18MaskIsRenderedThenSameFormatMatchesFreshOfficial threw exception:` 下一行`System.Runtime.InteropServices.COMException: UCEERR_RENDERTHREADFAILURE (0x88980406)`。

## 91实例互斥分类汇总
- F确定fixture/前置缺陷12：架构硬编码2（1、56）；缺受信original-wpf.json 10（9—12、52、84、87—90）。不表示修复后业务自动通过。
- P已完成官方契约+源码根因确认的生产缺陷0：本轮只证据闭环，没有把错误码强行确认为根因；不是宣称生产无缺陷。
- U能力/根因未证79：其余全部，包含真实红、原件对照失败、事件序列、native desktop控制、audio、ownership。不得改Pass/Skip或一概算环境。
- 12+0+79=91，与T15083 failed相符；1—91每项独立Result/testId，八组executionId首字符分区搜索（01/23/45/67/89/ab/cd/ef）共10+11+12+9+15+8+12+14=91，互斥无漏，不使用最初截断的前20命中冒全量。

业务栈已能确认FQN；60已按testId补核TestDefinitions L11836..11839，类型确为IndependentVideoPlaybackOwnerTests。首三项定义L4730..4745已亲读。定义来源核验不改变91实例结果。未转录巨大stdout，每项保留精确原件行号。历史旧full仍独立，不为缺Counters旧TRX补分母。
