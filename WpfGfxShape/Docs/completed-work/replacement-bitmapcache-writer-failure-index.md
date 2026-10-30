# writer 恢复轮 defaultBaselineFailures：42 个跨架构失败实例

这是实例索引，不是唯一方法数；不相减、不排除，不将前置失败冒业务通过。源码/build/test/publish/沙箱本索引原收尾阶段新增均0。

后续独立[official O6合同修正收尾](replacement-official-wic-release-thread-contract-closeout.md)：双端最后Release合法竞争已核，fixture只改official线程门为记录并增加退出检查；taskdc59e26d409a4a8d97bce71b74beef11单UID1/1/1、业务完整、MTP terminal0（独立OSexit Unknown），不是旧caller2204复现。此绿不扣本表U，不改42和旧full1823/1821/2、1823/1783/40；新fixtureCC384959不迁移绑定旧full。下一主修正G组真实HWcache首帧消费/失效；HWcache/consumerloss/audio/自然资源/finalfull/整体仍Open。

## 定位与展开规则
T64=`artifacts/writer-fixed-default-x64-readback/Results/writer-fixed-default-x64/TestResults/child.trx`；T86=`artifacts/writer-fixed-default-x86-readback/Results/writer-fixed-default-x86/TestResults/child.trx`（均相对WpfGfxShape）。每行R是Result起始行，D是Definition起始行；按testId联结，参数来自testName。除U外FQN均为`WpfGfxShape.ComAcceptance.D3DImageTests.`加下列完整方法名；U为`WpfGfxShape.ComAcceptance.UceChannelTests.`加完整方法名。消息按键展开为下列原Message（XML实体解码），不以摘要替代原消息；长父Message后附原supervision的准确行范围。所有行的独立资源cleanup审计=Unknown，唯x64 L的stdout明确device/factory/window complete；不宣称所有清理已审计。

|键|完整方法名|
|---|---|
|G|WhenStage4HardwareCacheGeometryDrawsThenHitAndMutationPreservePixelsAfterCallerRelease|
|C|WhenStage4HardwareCacheConfigurationFailsThenRecoveryAndMutationRenderRealPixels|
|W|WhenStage4HardwareWicCacheDrawsThenHitAndSourceReplacementPreservePixels|
|R|WhenStage4HardwareLazyWicReentersThenFrozenCacheFrameAndStreamLifetimeSurvive|
|K|WhenStage4HardwareWicCandidateIsWriteLockedThenCommittedFrameSurvivesAndSameGenerationRetries|
|D|WhenStage4HardwareCacheHasTwoDevicesThenBothCommittedKeysHitWhileWicIsWriteLocked|
|F|WhenFreshNativeGatePassesThenOfficialAndCandidatePublicTargetsAreCompared|
|N|WhenNativeOwnerThreadSubmitsRedBlueRedThenFreshDesktopContentIsRequired|
|H|WhenHistoricalCaptureCandidateRunsThenExpectedUpdatesRemain|
|HG|WhenHistoricalCaptureOfficialGeometryRunsThenExpectedUpdatesRemain|
|HS|WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain|
|P|WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels|
|S|WhenHardwareShaderGenerationIsReplacedThenTwoFramesConsumeLegalBytecode|
|M|WhenMalformedHardwareShaderFallsBackThenIdentityPixelsAndBadShaderFifoAreReal|
|X|WhenShaderChannelAndDeviceAreClosedThenNewPublicTargetRendersReplacementGeneration|
|T|WhenNativeGpuPresentsThenTopmostWindowIsObservable|
|B|WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive|
|O|WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive|
|L|WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover|
|U|WhenOriginalAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates|

## 原消息字典
- A：Assert.IsTrue 失败。‘condition’表达式:‘Math.Abs((int)((actual >> shift) & 255) - (int)((expected >> shift) & 255)) <= 1’。 Pixel 0x00F3F3F3, expected 0x00563412
- Z：Assert.IsTrue 失败。‘condition’表达式:‘Math.Abs((int)((actual >> shift) & 255) - (int)((expected >> shift) & 255)) <= 1’。 Pixel 0x00000000, expected 0x0000FF80
- Q：Assert.AreEqual 失败。应为: <0>，实际为: <141953144>。‘expected’表达式:‘0’,‘actual’表达式:‘result’。
- V：Assert.AreEqual 失败。应为: <5649426>，实际为: <15987699>。‘expected’表达式:‘0x00563412u’,‘actual’表达式:‘GetPixel(dc, 16, 8)’。
- J1：Assert.IsTrue 失败。‘condition’表达式:‘passed’。 C:\CodingAgentSandbox\Tasks\1e99f9304d9b4e88871dc63b200d0b80\Results\bj-fresh-1.json
- J3：同J1逐字仅文件名为bj-fresh-3.json。
- EG：Assert.AreEqual 失败。应为: <0>，实际为: <2>。‘expected’表达式:‘0’,‘actual’表达式:‘exitCode’。 C:\CodingAgentSandbox\Tasks\1e99f9304d9b4e88871dc63b200d0b80\Results\writer-fixed-default-x86\historical-isolation\WhenHistoricalCaptureOfficialGeometryRunsThenExpectedUpdatesRemain\b83d74e0eab74a76af29f088f1f04ba6；原Message接续完整supervision JSON在T86:2220–2259（由2219起始至2259结束，保留原件，不改写JSON）。
- ES：Assert.AreEqual 失败。应为: <0>，实际为: <2>。‘expected’表达式:‘0’,‘actual’表达式:‘exitCode’。 C:\CodingAgentSandbox\Tasks\1e99f9304d9b4e88871dc63b200d0b80\Results\writer-fixed-default-x86\historical-isolation\WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain\deb1a7af0deb42ab8998e8783372ef96；接续完整supervision JSON在T86:2271–2310（父Message2270–2310）。
- L0：Assert.AreNotEqual 失败。应为: <0> 以外的任意值，实际为: <0>。‘notExpected’表达式:‘0’,‘actual’表达式:‘result’。 FailedNotTriggered: exclusive foreground prerequisite failed. Inspect DeviceLossForeground API BOOL, owner PID and session; no real device loss is claimed.
- U0：Assert.AreNotEqual 失败。应为: <2204> 以外的任意值，实际为: <2204>。‘notExpected’表达式:‘DrainGetCurrentThreadId()’,‘actual’表达式:‘releaseThread’。

阶段键：a=初始native control 0x563412像素失败，未证明后续cache/HW业务进入；j=独立d3d9 fresh desktop gate失败，public比较未进入；v=初始native control GetPixel失败；q=PresentNativeControl返回S_PRESENT_OCCLUDED(141953144)，尚未到后续业务，不再查void/slot；o=official初native过，shader初帧FF80读0，未到resize；l=exclusive初始foreground失败，未触发真实loss；u=Destroy.Drain0、callback.exit已观测/disposals1，同线程断言失败，正常sibling/deinit断言未到（finally兜底不作成功证据）。角色C=candidate加载身份可见、N=独立d3d9/nativeControl（Unknown不冒候选）、O=official、I=official隔离父/child。stage a中的C/O表示场景加载身份，不代表已消费候选/官方HW业务。

## x64：2实例（T64）
|#|方法|参数|testId|消息|角色/阶段|R / D / Message|
|---|---|---|---|---|---|---|
|1|O|无|14e862b3-9b71-843a-9ff5-67f779272c33|Z|O/o|2309 / 10254 / 2330|
|2|L|无|16ff21fd-ec04-8a70-814e-ca43404cfbd3|L0|N/l|2340 / 10258 / 2345|

## x86：40实例（T86）
|#|方法|参数|testId|消息|角色/阶段|R / D / Message|
|---|---|---|---|---|---|---|
|3|G|4278255360,65280|163b209d-b0ee-847d-8693-99661ef36347|A|C/a|1476 / 10364 / 1490|
|4|G|4278190335,16711680|1a425113-e5de-887c-ba34-4358042833d3|A|C/a|1501 / 10368 / 1510|
|5|C|4278255360,65280|1e07eac4-3e65-8e7f-86dd-fad6f0403e27|A|C/a|1519 / 10372 / 1528|
|6|C|4278190335,16711680|1c311245-f56d-88db-b3b7-8bd6565da465|A|C/a|1539 / 10376 / 1548|
|7|W|4278255360,65280|1de8aecd-ec9e-831a-92c0-97c897a8d822|A|C/a|1557 / 10380 / 1566|
|8|W|4278190335,16711680|197f02db-2800-8760-bf5b-f09aaa116829|A|C/a|1577 / 10384 / 1586|
|9|R|0|11194225-4f95-866d-a08b-aa2ba00ccdd7|A|C/a|1595 / 10388 / 1604|
|10|R|1|16d0ec26-f97d-8928-a222-03fbddeb3447|A|C/a|1615 / 10392 / 1624|
|11|R|2|1872055b-1354-8b7c-9335-95091b1ef30e|A|C/a|1633 / 10396 / 1642|
|12|R|3|168b8cd4-a952-85c0-896a-38f2cdb32f76|A|C/a|1651 / 10400 / 1660|
|13|R|4|1fa69fb3-65cf-83e9-a1a7-8d9751225bc2|A|C/a|1669 / 10404 / 1678|
|14|K|4278255360,65280|17eef5c8-05f7-80a1-981e-62d1751e1f91|A|C/a|1687 / 10408 / 1696|
|15|K|4278190335,16711680|12f170ce-3ec4-8f31-96c6-7b2f12c5c550|A|C/a|1707 / 10412 / 1716|
|16|D|False|1a7331dd-3ad6-86dd-b9ce-4c54eadbeffa|A|C/a|1725 / 10416 / 1734|
|17|D|True|1ac32cda-4545-81eb-b4db-a89cb5c4351e|A|C/a|1748 / 10420 / 1757|
|18|F|无|15d158a4-e944-8881-bbfc-271bff3c3332|J1|N/j|1769 / 10424 / 1853|
|19|N|1|1d4afea9-fe59-8b80-9b90-30c096032d34|J1|N/j|1865 / 10428 / 1948|
|20|N|3|19b3355c-10bc-80a4-b748-5cbfce845d4b|J3|N/j|1960 / 10432 / 2043|
|21|H|True,False,4278255360,65280|118b1250-19be-8107-ba64-b1750f57c1d3|A|C/a|2053 / 10436 / 2073|
|22|H|True,False,4278190335,16711680|1e784501-7af7-85ef-9cbd-48b8ae94a6c0|A|C/a|2088 / 10440 / 2108|
|23|H|False,False,4278255360,65408|1817ff57-e333-8653-b23b-985784a01e97|A|C/a|2120 / 10444 / 2140|
|24|H|False,True,4278190335,16711808|1e3d5f5f-ccd2-8af8-ab63-b6dcdc121649|A|C/a|2152 / 10448 / 2172|
|25|H|False,True,0,128|12075ab2-10ad-8d10-bc58-007453dc09ea|A|C/a|2184 / 10452 / 2204|
|26|HG|无|18e39062-2997-8a5c-bf02-39b275a17ace|EG|I/a|2216 / 10456 / 2219–2259|
|27|HS|无|1c6be0d5-e7e9-8416-85f5-e4e394a6710c|ES|I/a|2267 / 10460 / 2270–2310|
|28|P|40,40,21,False,0,False|1dc5baf7-ad20-8107-aba8-8f62d9a39e17|V|N/v|2326 / 10496 / 2336|
|29|P|120,80,21,False,0,False|108742fb-3d56-8287-991f-4b22fb6a5fdd|V|N/v|2348 / 10500 / 2358|
|30|P|40,40,21,True,0,False|1c02f3cb-6784-82ac-838e-bf32a84a0213|Q|N/q|2370 / 10504 / 2377|
|31|P|120,80,22,True,0,False|14ed735c-b0f8-82de-a25f-eefda6ec8de9|Q|N/q|2390 / 10508 / 2397|
|32|P|40,40,21,True,1,False|15291d4a-2b12-809f-9091-80f2e1e5889f|Q|N/q|2410 / 10512 / 2417|
|33|P|40,40,21,True,2,False|105bcc70-f151-817f-b468-07b2d2f087f1|Q|N/q|2430 / 10516 / 2437|
|34|P|40,40,21,True,3,False|15e72681-ae9f-8e8a-8630-ff83fba836c8|Q|N/q|2450 / 10520 / 2457|
|35|P|40,40,21,True,0,True|172e7d85-04d6-82cc-90dc-b18ce8d1778a|Q|N/q|2470 / 10524 / 2477|
|36|S|无|15d634ec-21d2-8e73-b808-4f1e30b75a93|Q|N/q|2490 / 10528 / 2497|
|37|M|无|1f833676-6572-8932-8888-63e5b0bbc626|Q|N/q|2512 / 10532 / 2519|
|38|X|无|13c24575-b993-8a9c-a589-0175dbbe6334|Q|N/q|2534 / 10536 / 2541|
|39|T|无|1f84a53c-28e9-87cc-96d5-17c2eea1e9b4|A|N/a|2595 / 10600 / 2605|
|40|B|无|1eded079-5019-809c-bf7c-07ef4caf0e29|A|C/a|2618 / 10604 / 2637|
|41|O|无|14e862b3-9b71-843a-9ff5-67f779272c33|A|O/a|2650 / 10608 / 2669|
|42|U|6|1e4c35fb-589a-8de1-838e-4845fd1028f4|U0|O/u|3953 / 11928 / 3956|

## official historical child 原件复核
父stdout路径准确回读：T86同级`historical-isolation/WhenHistoricalCaptureOfficialGeometryRunsThenExpectedUpdatesRemain/b83d74e0eab74a76af29f088f1f04ba6/stdout.txt`及Sampler/deb1a7af0deb42ab8998e8783372ef96/stdout.txt，1–25可见编码乱码，但路径/栈/像素可读；随后各`TestResults/child.trx`全文1–66精确核对。各child Result8、Message34、Definition46–49、Counters59（本轮重新亲读两child的55–66行确认；原记58是ResultSummary而非Counters），均1执行/0过/1败，exit2、非旧crash；actual official5EBD，native初帧F3F3F3而非563412，业务geometry/sampler更新未进入。parent两个失败仍各计1，不额外加入42也不冲销。

## 覆盖与限制
42个Result头/Message/Definition按以上行定位读取；大块输出出现4000字符截断后对Definition及ErrorInfo小块补读。未读取所有Passed，也未完整逐字读每条巨大PresentOcclusionState内嵌CallStack或所有重复acquire行；q九项以Result、ErrorInfo与栈确认前置，141953144已确认为S_PRESENT_OCCLUDED，不猜槽位。所有Passed的业务及资源收敛未全审计。精确主Counters T64:14077、T86:14431；两supervision全文1–38。索引终态=42失败完整登记；全面cleanup审计不在此宣称完成。
