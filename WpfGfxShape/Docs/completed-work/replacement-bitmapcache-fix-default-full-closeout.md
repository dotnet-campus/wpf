# BitmapCache 修正产物默认 full 正式收尾

状态：两架构默认 Release full 已实际执行并回收，均 Failed；本轮执行收尾完成，完整替换 Open。不是报告草稿。

## 1. 执行与预算
复用 artifacts/cache-packet-fix-{x64,x86}-r1/clean，未误用 r1native（仅Native源）。既有发布均-c Release；亲读两aot.sha256、runtimeconfig NETCore/Desktop10.0.13、正式NativeRun/ExitDiagnostics full入口。默认Release不启用heap/ETW/sidecar/前台控制诊断配置。不改skip/阈值/并行/fallback。本轮源码修改0、normalbuild0、Nativepublish0、COMpublish0；本次新增sandbox2/2独立用尽，前轮剩1未搭车，失败不重试。
精确参数及返回见 artifacts/public-cache-contract/default-full-{x64,x86}-receipt.md、default-full-execution.md。每次正式exe `--supervise-exit --full Results/bitmapcache-fix-default-full-{arch} 1500`，outer1800，outputRelativePath省略整任务回收，先x64再x86。

## 2. 真实 Counters 与监督
|架构|task|total/executed|passed|failed|监督PID/child|child exit|
|---|---|---|---|---|---|---|
|x64|41e93ce4468949a48d345f9f1cded100|1823/1823|1783|40|8268/11144|2|
|x86|edf40a74d42f4f9bb38e8e0f517ea024|1823/1823|1786|37|2340/9980|2|
两TRX其余Counters全部0，ResultSummary Failed、RunInfo明确exit2。监督Mode full/1500、Reason exited、HasExited/OutputCollected true，TimedOut/Cancelled/KillAttempted/OutputDrainTimedOut/OutputDrainCancellationTimedOut均false、SupervisorError null。工具未返回独立数值OSexit；supervision ExitCode是被监督child退出，不冒顶层OS exit。x64 UTC02:02:11.7616289–02:10:35.5408602；x86 UTC02:11:43.0893002–02:19:07.2906806。
原件根目录：`../../artifacts/bitmapcache-fix-default-full-{arch}-readback/`。主TRX在 `Results/bitmapcache-fix-default-full-{arch}/TestResults/child.trx`，完整监督/stages/stdout/stderr同级。x64 Counters14330/尾14337、x86 Counters14375/尾14382已读。不得拿旧红减新绿计算本次计数。
公开cache-contract只是Pcore命令分支，不是新增TestMethod，默认full分母实际仍1823，不编1833或183?。

## 3. 身份及混合角色边界
- x64 supervision TestSha256=01389677F636E2504F3448075889F22774239B2C38DD629351018005E4F9E970；MVID=0fb49365-116b-4fca-8a93-02cfd8f8b007；.NET10.0.13 X64。
- x86 supervision TestSha256=E8EFAC02F71EDEC4A6D2B06F7F997DB18B4CC5C9EDF2D63B2771D9D8585776B7；MVID=0673f5aa-3a2c-40de-bb82-c1f9012c6bce；.NET10.0.13 X86。
- candidate包及实际PresentCondition已见：x64 6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403；x86 B07C9073F75278ED61C44638EB3644547B413A281E2859B25211CEAAC3D1466C。
- official失败自身Requested/Actual/sha/testSHA已核：x64 AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1；x86 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834；版本10.0.1326.50504。
- full混合candidate/official/直接d3d9控制，不把包hash冒充所有actual。fresh native gate的PresentCondition明确loadedModule=Unknown/sha256=Unknown；直接d3d9不是candidate实际消费者认证。Results/aot-identity.json最后写入是隔离RoundedRectangle角色，只证该文件记录的角色，不冒整个full唯一模块。
- 原公开四角色程序集SHA/MVID未知仍保留；本full TestSHA/MVID不能倒填前次角色。

## 4. 全部失败索引与分类
以下每个参数实例均为真实Failed，不排除。完整 testId/executionId/原始Message及Definition权威存于各主TRX，按testId一对一联结Results与TestDefinitions；两arch相同实例testId一致。前缀C= WpfGfxShape.ComAcceptance.CompositionTests，E= WpfGfxShape.ComAcceptance.CompositionEffectsTests，D= WpfGfxShape.ComAcceptance.D3DImageTests。x64共40；x86同表除最后3个fresh native实例，共37。表内将多实例逐个列出，不从旧基线推算。

|FQN（类前缀见上）及参数|testId|类别/消息|
|---|---|---|
|E.WhenSoftwareGenericSamplerCombinesTwoInputsThenOffscreenPixelsAreReal(2,4286611584)|113dc426-fcac-8001-a1aa-0c2ed0ab26f5|确定fixture旧32；commit -2003303421|
|C.WhenStage4CacheMissFailsThenCommittedBitmapSurvivesAndOriginalKeyHits(0)|10fd0f54-f868-8a34-8a6b-937305326e15|fixture旧32/commit同上|
|同方法(1)|1c07b2d7-9858-8051-bdc9-088bd866ab7f|同上|
|同方法(2)|1a13aa98-f67e-8b6e-ba73-7f31baf7c7c0|同上|
|同方法(3)|184ca029-d2e5-8a9e-8af8-cafb65020b7d|同上|
|C.WhenStage4CacheDecoderReentersThenFrozenOwnersSurviveReplacementDeletionAndClose(0)|108ed28b-dd50-8313-a12b-b9234317ac6d|fixture旧32/commit同上|
|同方法(1)|113fc375-b3f3-8724-988b-8263d58b5aec|同上|
|同方法(2)|116225c6-8523-8768-a092-9748ebd68721|同上|
|同方法(3)|1c6ffd22-dfa9-8bfc-a94f-556c075ae310|同上|
|同方法(4)|19bd9739-fed8-896d-a401-7ed7bb375eb3|同上|
|同方法(8)|1152fd97-8793-8043-a96d-eefb74928874|同上|
|同方法(9)|1466059c-58cf-8e4a-a74c-077e374cb4bf|同上|
|C.WhenStage4NestedDrawingCacheTargetMutatesDuringLazyReadThenActiveFrameOwnsCacheRequest()|1423450d-9e33-86c7-9dd2-b0a6ba9ac29d|fixture旧32/commit同上|
|D.WhenStage4HardwareCacheGeometryDrawsThenHitAndMutationPreservePixelsAfterCallerRelease(4278255360,65280)|163b209d-b0ee-847d-8693-99661ef36347|x64 native00563412/F3F3F3未证；x86后续commit malformed|
|同方法(4278190335,16711680)|1a425113-e5de-887c-ba34-4358042833d3|同组，保持红|
|D.WhenStage4HardwareCacheConfigurationFailsThenRecoveryAndMutationRenderRealPixels(4278255360,65280)|1e07eac4-3e65-8e7f-86dd-fad6f0403e27|x64 native像素前置未证；x86fixture32|
|同方法(4278190335,16711680)|1c311245-f56d-88db-b3b7-8bd6565da465|同组|
|D.WhenStage4HardwareWicCacheDrawsThenHitAndSourceReplacementPreservePixels(4278255360,65280)|1de8aecd-ec9e-831a-92c0-97c897a8d822|x64 native像素前置未证；x86fixture32|
|同方法(4278190335,16711680)|197f02db-2800-8760-bf5b-f09aaa116829|同组|
|D.WhenStage4HardwareLazyWicReentersThenFrozenCacheFrameAndStreamLifetimeSurvive(0)|11194225-4f95-866d-a08b-aa2ba00ccdd7|x64 native像素前置未证；x86fixture32|
|同方法(1)|16d0ec26-f97d-8928-a222-03fbddeb3447|同组|
|同方法(2)|1872055b-1354-8b7c-9335-95091b1ef30e|同组|
|同方法(3)|168b8cd4-a952-85c0-896a-38f2cdb32f76|同组|
|同方法(4)|1fa69fb3-65cf-83e9-a1a7-8d9751225bc2|同组|
|D.WhenStage4HardwareWicCandidateIsWriteLockedThenCommittedFrameSurvivesAndSameGenerationRetries(4278255360,65280)|17eef5c8-05f7-80a1-981e-62d1751e1f91|x64 native像素前置未证；x86fixture32|
|同方法(4278190335,16711680)|12f170ce-3ec4-8f31-96c6-7b2f12c5c550|同组|
|D.WhenStage4HardwareCacheHasTwoDevicesThenBothCommittedKeysHitWhileWicIsWriteLocked(False)|1a7331dd-3ad6-86dd-b9ce-4c54eadbeffa|x64 native像素前置未证；x86fixture32|
|同方法(True)|1ac32cda-4545-81eb-b4db-a89cb5c4351e|同组|
|D.WhenHistoricalCaptureCandidateRunsThenExpectedUpdatesRemain(True,False,4278255360,65280)|118b1250-19be-8107-ba64-b1750f57c1d3|fixture32 initialCommitResult -2003303421|
|同方法(True,False,4278190335,16711680)|1e784501-7af7-85ef-9cbd-48b8ae94a6c0|同上|
|同方法(False,True,4278190335,16711808)|1e3d5f5f-ccd2-8af8-ab63-b6dcdc121649|同上|
|同方法(False,True,0,128)|12075ab2-10ad-8d10-bc58-007453dc09ea|同上|
|D.WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive()|1eded079-5019-809c-bf7c-07ef4caf0e29|candidate initialCommitResult malformed，fixture32|
|D.WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive()|14e862b3-9b71-843a-9ff5-67f779272c33|official独立未证根因：expected0000FF80 actual0|
|D.WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover()|16ff21fd-ec04-8a70-814e-ca43404cfbd3|前台前置FailedNotTriggered，非产品loss已触发|
|D.WhenSecondSamplerUsesVisualOrCacheThenNonIdentityShaderCombinesBothInputs(True,4278190335,16711808)|1bb8f030-8d7b-8cf7-a51f-1ecf373f06e7|fixture32 initialCommitResult malformed|
|同方法(True,0,128)|1ff78723-b194-81fe-bd33-ebfeaa07a211|同上|
|D.WhenFreshNativeGatePassesThenOfficialAndCandidatePublicTargetsAreCompared()（仅x64红）|15d158a4-e944-8881-bbfc-271bff3c3332|未证：fresh current-submission deadline/像素F3F3F3|
|D.WhenNativeOwnerThreadSubmitsRedBlueRedThenFreshDesktopContentIsRequired(1)（仅x64红）|1d4afea9-fe59-8b80-9b90-30c096032d34|未证，同组，不移绑candidate|
|同方法(3)（仅x64红）|19b3355c-10bc-80a4-b748-5cbfce845d4b|未证，同组|

精确首红Definition已亲读x64 TRX6931–6935，Result519–530：E方法(2,4286611584)，execution23a784ab-16da-4baf-a44b-42dcd73f206a。x64 official execution f74caa53-7e66-432e-a82a-73badbd4fb00；x86 official131d3c9d-40a0-4f0d-8396-d4f3bb749c38，loss分别0bba0203-b5a6-46e1-9a3d-d0ef4d8ade7b/7770c73e-8a7c-45a2-8fc2-26b3be4bf509。
本轮未发现已可确认新的生产缺陷；确定夹具仍错，不等于修好夹具后所有HW会绿。其余具体根因未证，不能全归环境。以上分组消息不替代完整原始TRX；尚未逐实例全文回读全部Definition/所有隔离子TRX，故不冒完整独立审计完成。

## 5. 原malformed是否消除及下一唯一直接修复
生产公开真实managed发28、payload20，双架构公开VisualBrush/BitmapCacheBrush四帧512/512及清理已通过（见产品fix报告）；旧147B公开0x8D malformed/cleanupfailed永久保留。新full仍有0x88980403（-2003303421），已找到仍发旧32的COM夹具：
- CompositionEffectsTests.SoftwareShader.cs181–183：stackalloc32/send32，首红直接依据。
- CompositionTests.ActiveRender.cs751–753、784–786：旧32，Stage4 miss/reentry/nested。
- CompositionTests.Stage4Hardware.cs72–76：new byte[32] 0x8d。
- D3DImageTests.VisualCache.cs426/448/626：0x8d configuration32/send32。
下一工作直接将这些合法0x8D writer按独立官方合同改为28（所有字段保留、0x84仍36、非法32负控仍拒绝），补夹具wire断言，先首红定向再验证同族/两arch最终适用矩阵。不要回退生产exact检查或用>=吞包；本轮源码修改预算0故未擅改。不得选择泛门/旧paint线程/捕获路线代替这项已知修复。

## 6. 生命周期及剩余主门
监督完整回收且未kill不等于每项清理无错误。stages x64 21等有visual cleanup complete，373/431/553/611等明确world release commit 0x88980403；x86 stages41/55同错，随后destroy/disconnect0、bitmap/surface/device/windows处理完成。保留cleanup业务错误，不能称全绿。loss stdout明确device/factory/window cleanup complete，但真实loss未触发。official两arch native00563412通过、CreatePixelShader0、初帧FF80读0仍独立红；candidate packet fix不会自动修官方消费。
actual consumer HW/backend、产品target loss/recovery、实际音频、自然资源与memoryGreen（仍null）、final matrix、完整替换仍Open。先修确定旧fixture writer，再分别处理official合法consumer内容证据/初始前台资格/真实backend输出；不以producer代consumer，不将历史有限绿整体迁移新hash，不以本次默认full已运行冒最终通过。

收尾：两次授权full完成，预算用尽无重试，正式报告与状态入口同步；验证执行完成≠产品全部完成。
