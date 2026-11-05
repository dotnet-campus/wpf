# official cache full r6 — ExecutedFailed / cacheNotReached / Overall Open

## 1. 执行与范围
2026-10-29 已亲自完成唯一一次授权共享进程 full；不是 r5 隔离 task1 的同条件重跑。运行前已持久化 `WpfGfxShape/artifacts/official-cache-full-r6/preregistration.md`。源码/build/publish/Native/生产修改均0，sandbox1/1，内1500秒/外1800秒。无终端脚本、再委派、新探针、expected/skip/阈值/线程/paint/RDP/GC改动。full 原有测试全部保留，故原有 native/capture 相关测试也按默认发现执行；没有另行选择或追加这些调查。

receipt核定的精确 sourceDirectory：`WpfGfxShape/artifacts/official-cache-firstframe-r5/clean`；入口 `WpfGfxShape.ComAcceptance.exe`；arguments `["--supervise-exit","--full","Results/official-cache-full-r6","1500"]`。完整任务回收 `WpfGfxShape/artifacts/official-cache-full-r6/readback`，未限定输出子目录。参数与工具原返回已即时写入 receipt.md；工具仅返回完成和保存路径，没有独立顶层OS exit，不冒称OS0。

静态核：csproj159–173仅启用OFFICIAL_CACHE_FIRSTFRAME及runtime/package；OfficialCacheObservation.cs1–60无TestMethod；VisualCache.cs76–79仅official && cache && resizeWindow实例化hook；HardwareClosure.cs18–21原测试与FF80保留。KG新增测试受独立KG_OFFICIAL_SHADER_CONTRACT保护，此配置未启用。ExitDiagnostics.cs36–49 full无filter、原1500上限不变。实际发现分母1823：这是**诊断配置full**，不是未插桩Release默认full。

## 2. 父full实际结果与身份
任务 `50be892a532746da8540b04a06d21cfe`；supervisor11348、child2236。14:50:20.1929208至14:58:39.2057293 UTC，约499秒。supervision.json完整38行：Mode full，exited/child exit2，TimedOut/Cancelled/KillAttempted均false，OutputCollected true，两drain timeout false，SupervisorError null；stderr为空。

父 `Results/official-cache-full-r6/TestResults/child.trx`14368–14376：**1823 total/executed，1783 passed，40 failed，timeout/aborted/notExecuted/notRunnable/inconclusive/error均0**。MTP退出2；stdout1135–1147同样报告1823/1783/40，8m18.658s。原始stdout中文编码有乱码，错误文本以TRX为准。

运行.NET10.0.13 X64；test SHA `F4AAEF955DB2945B5A0A9AC0B99CB5EE606A6B3398DECB67A3682A4461E64BFB`，MVID `12f205e0-042d-43a3-b75d-48dfef61d532`。candidate `632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671`。目标official实际任务根wpfgfx_cor3.dll，handle140720707338240，SHA `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，ProductVersion10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。非以producer身份冒充consumer。

## 3. 目标official cache真正现场：NotReached
精确FQN `WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive`。父TRX2600–2630 Failed，testId14e862b3-9b71-843a-9ff5-67f779272c33，executionId7b9b1ef5-352a-4088-837f-e473129886a5；Definition10546–10549匹配。14:52:44.7515275至14:52:45.7744411。

`official-cache-2236-48F00EE.jsonl`完整只有2行：
- identity：QPC41066359357553/Frequency10000000，HWND76480750=0x48F00EE，上述实际official/module/candidate身份。
- pixel：QPC41066368989703，ElapsedTicks9820636，(16,8) expected5649426=`00563412`，actual15987699=`00F3F3F3`。

父TRX日志：同HWND visible1/iconic0/foreground自身/cover自身/default desktop/session1/cloaked0/client32x16/dpi96；ownerPid2236、ownerThread7444，记录currentThread2708；producer swapchain32x16/format22/count1/swapEffect1/windowed1/cooperative0。以上只是现场记录，不开启线程/paint/捕获调查，不认定永久沙箱故障。

源码VisualCache.cs112在connect/initialize/channelCreate之前断言native像素；120–127才connect/channel/记录。stages457–477完整回读：CreateWindow→Establish→CreateDevice→PresentNativeControl→DiagnoseNativePresent→AssertCombinedPixel before→cleanup；没有AssertCombinedPixel after/connect。cleanup477为`visual cleanup complete: `空错；阶段名称含destroy channel并不证明channel曾创建。

**nativeGate Failed → cacheNotReached。** connection/channel/target/root/world/cachebrush/source dirty/初始两FF80/resize/mutation/resourcegraph/commandHR/publicPeek全部未到。不是实际0≠FF80失败，也不是消息队列为空；没有Presented/NoPresent/BadPixelShader现场内容，consumer device Unknown。hook只有业务完成后清理前有限Peek并出队；即使未来Peek无消息也不能断言从未呈现。本次根本没有调用Peek，不能做通知缺失推断。

与历史默认full native过→FF80实际0是不同阶段，不能取代历史失败或据此归因官方cache协议/候选生产。未发现可确证资源协议错误或状态污染，没有可证最小fix；本轮代码0。

## 4. 全部40个失败实例（无排除）
以下名称统一前缀 `WpfGfxShape.ComAcceptance.D3DImageTests.`。L指上述父TRX的1-based行号；完整testId/executionId/原始输出/堆栈保存在每条UnitTestResult，Definition均为该D3DImageTests类。Definition区10302–10400、10434–10476、10538–10565已按名称检索及目标/尾段直接回读。参数是实际显示值，不合并计数、不从历史扣减。

| 测试名称 | 失败参数/实例 | 数 | 父Result行 | 原错误类别 |
|---|---|---:|---|---|
|WhenStage4HardwareCacheGeometryDrawsThenHitAndMutationPreservePixelsAfterCallerRelease|(4278255360,65280);(4278190335,16711680)|2|1476,1501|native像素F3F3F3≠563412|
|WhenStage4HardwareCacheConfigurationFailsThenRecoveryAndMutationRenderRealPixels|同上两参数|2|1521,1541|同上|
|WhenStage4HardwareWicCacheDrawsThenHitAndSourceReplacementPreservePixels|同上两参数|2|1561,1581|同上|
|WhenStage4HardwareLazyWicReentersThenFrozenCacheFrameAndStreamLifetimeSurvive|(0),(1),(2),(3),(4)|5|1601,1621,1641,1661,1681|同上|
|WhenStage4HardwareWicCandidateIsWriteLockedThenCommittedFrameSurvivesAndSameGenerationRetries|(4278255360,65280);(4278190335,16711680)|2|1701,1721|同上|
|WhenStage4HardwareCacheHasTwoDevicesThenBothCommittedKeysHitWhileWicIsWriteLocked|(False),(True)|2|1741,1761|同上|
|WhenFreshNativeGatePassesThenOfficialAndCandidatePublicTargetsAreCompared|无|1|1781|CheckFreshDesktopDelivery passed断言false，bj-fresh-1.json|
|WhenNativeOwnerThreadSubmitsRedBlueRedThenFreshDesktopContentIsRequired|(1),(3)|2|1873,1962|passed断言false|
|WhenHistoricalCaptureCandidateRunsThenExpectedUpdatesRemain|(True,False,4278255360,65280);(True,False,4278190335,16711680);(False,False,4278255360,65408);(False,True,4278190335,16711808);(False,True,0,128)|5|2051,2083,2115,2147,2179|native像素F3F3F3≠563412|
|WhenHistoricalCaptureOfficialGeometryRunsThenExpectedUpdatesRemain|无|1|2211|隔离child exit2≠0（原件完整保留）|
|WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain|无|1|2261|隔离child exit2≠0（原件完整保留）|
|WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels|(40,40,21,False,0,False);(120,80,21,False,0,False)|2|2319,2340|GetPixel15987699≠5649426|
|同上|(40,40,21,True,0,False);(120,80,22,True,0,False);(40,40,21,True,1,False);(40,40,21,True,2,False);(40,40,21,True,3,False);(40,40,21,True,0,True)|6|2361,2377,2393,2409,2425,2441|PresentNativeControl result141953144≠0|
|WhenHardwareShaderGenerationIsReplacedThenTwoFramesConsumeLegalBytecode|无|1|2457|同上HR|
|WhenMalformedHardwareShaderFallsBackThenIdentityPixelsAndBadShaderFifoAreReal|无|1|2474|同上HR|
|WhenShaderChannelAndDeviceAreClosedThenNewPublicTargetRendersReplacementGeneration|无|1|2491|同上HR|
|WhenNativeGpuPresentsThenTopmostWindowIsObservable|无|1|2547|native像素F3F3F3≠563412|
|WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive|无|1|2569|同上，candidate也未过native前置|
|WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive|无|1|2600|同上，本次目标cacheNotReached|
|WhenNativeGpuPresentsThenFrontBufferAndClientPixelsAgree|(1)|1|2649|actual15987699≠1193046（F3F3F3≠123456）|

计数15+3+7+8+3+3+1=40。全部HR非零失败9项（6个shared public rows+3 shader tests）保留，不按“正HR”抹除原断言，不擅改expected；HR十进制141953144是本次实际值，未据此认定生产错误。另2个隔离child exit2、3个fresh delivery断言、其余26个像素失败均在父full分母内。未发现表外失败的排除式检索仅用作辅助，Counters40及逐类实例才是计数依据。

## 5. 新fixture真实loss结果及上下文局限
父TRX2631–2646 / Definition10550–10553：`WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover` **Passed**，testId16ff21fd-ec04-8a70-814e-ca43404cfbd3，executionIdb6d2a33d-ef9e-4883-843d-2cdcebfdcfb5。firstCreate0/owner7444/fullscreen1920x1080/format22；真实前台triggerPid11416，displacedPresent及cooperative=`88760868`；triggerExit0；restore后beforeReset=`88760869`，Reset0，cleanup device/factory/window complete。stages478另记实际adapter0/type1/focus0x49000EE/flags0x26/windowed0。这是新fixture在本次full的真实通过，不能把旧loss未触发失败搬成本次失败，也不能据控制层通过宣称候选HWND consumer恢复。

同full中原有WhenOriginalPublicHwndTargetDrawsThenWindowContainsHardwarePixels2508–2513 Passed且563412实测相等；Kn两rows2647–2648 Passed；FrontBuffer (3)2683 Passed、Alternates (1)/(3)2712/2792 Passed。这些仅作为完整结果边界，未重新调查，不把nativeGate偶发失败当永久沙箱无能力。目标仍未取得cache现场，full40红，不符合“目标通过且仅loss或全绿”的有限验收分支；不能验收整个COM默认适用full或完整替换。

## 6. 终态与可执行接续
本次cache诊断已按预注册终止：**当前无法取得目标cache现场，根因未定**。不建议native生命周期/线程/paint/捕获旧路线，不重跑相同包追绿，不由phase结束宣称替换完成。

后续具体动作（本轮不执行）：以本报告40红及原TRX作固定输入，由主代理继续完整替换剩余工作；先静态审核9项HR对应`PresentNativeControl`断言与其调用API的官方返回值合同，必须有具体源码位置/官方依据后才另立最小fixture修复，不把它预称cache根因、不移除本次失败。cache工作只在已有真实可达目标现场证据或经证实的资源协议/状态污染修复后重新立项，不安排native新探针或同条件full。产品consumer真实loss恢复仍NotCovered；HW target身份、音频、自然资源趋势、双架构最终适用验证、整体替换仍Open，memoryGreen=null。

历史原默认x64/x86各1823/1821/2和更早23红独立保留；r6诊断full1823/1783/40不是相减后的默认基线。x86本轮NotRun。

## 7. 回读与审计
完整原件都在独立r6/readback，未覆盖r5或历史。即时receipt先存原参数/返回，再由supervision补task。实际读到：supervision1–38全、业务JSONL1–2全、stderr空、target TRX2600–2630（初次长输出截断，2624–2630补尾）、Counters14368–14376全、目标/控制Definition10538–10566、stages457–489、stdout1135–1147尾；全部失败Result头按Stage4/其余分类搜索，40项在表中逐行映射；Message按类别检索、HR9项、loss完整结果均读。MTP diag仅读1–8，不声称全文读取；stages全876行只读目标相关及loss片段，不声称全读。其余大TRX按定向片段/检索回读，不声称14376行全读。宽范围搜索曾扫到二进制并截断，随即收窄父TestResults；未用该截断输出判断全部失败。所有工具准确参数/原返回保留于会话，receipt记录关键执行及截断边界。恢复入口/current item/baseline/plan/matrix/总方案/full失败索引同步加入r6当前状态；下层旧建议均历史，被本节停止规则覆盖。
