# UCE 最终集成验收 r28

## 范围与最后源码边界

本轮仅最终集成验证、必要编译修正与文档。未新增生产能力或测试代码。
回读最终异常边界：UceChannelExports 的 Create/CreateChannel/Destroy/Commit/SyncFlush；PartitionManagerExports.Initialize；PartitionScheduler 的优先级、scheduled Compose/work completion；SameThreadChannel 的逐包异常首错和 finally transport release。
首次 Release/win-x64 build 失败：UceChannelExports.cs:55、PartitionManagerExports.cs:72，CS0160；Win32Exception 派生于 ExternalException。仅将两个 catch 调整为派生类型在前，HRESULT 映射未变。第二次 build 成功，既有平台警告未作为已解决事项。
最后源码改动发生在以下发布和全部测试之前；测试完成后不再修改源码。

## 发布记录

- P1：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，退出0，Native AOT 生成成功。
- P1 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`（路径相对工作区根）。
- P2：`dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=true`，退出0；Build 后包目标清理并重建标准包。
- P2 输入 P1 DLL；传输仅 `WpfGfxShape/artifacts/com-acceptance-package`，直接运行标准 MSTest exe，无 Host、终端或脚本。
- 最终 SHA256：`59FA4BAB8C0A16C4B556656F4B32F6D915A80C3B8DA4775DA577322E13C28451`。
- r27 的 `DC63B613F20BA127654B1C5E840CE79502C7DE7E7E2EB3B3DFCF27AF9EED9D89` 为前一已测产物，不冒充覆盖后补异常边界。r28 已取代“最后五处未测”状态。

## 原始证据及独立回读

以下相对 `WpfGfxShape/Investigations/UCE-final-integration-20261014-r28/`。

| 运行 | 原始 TRX | RunID | pass/fail/total | 退出 |
|---|---|---|---|---|
| 定向 | targeted/TestResults/uce-final-targeted-r28.trx | ca2ea1d0-7cad-4d23-a718-5df40cb5e3c7 | 39/0/39 | 成功 |
| full | full/TestResults/uce-final-full-r28.trx | adb7aefd-cfc7-46da-bdbc-a715f10e207c | 1396/8/1404 | 2 |

定向 filter：`FullyQualifiedName~AsyncUceChannelTests|FullyQualifiedName~UceChannelTests|FullyQualifiedName~AcceptancePathsTests`。
两次均零跳过；分别读取 TRX 首部 RunID 与尾部 Counters，不只依赖执行摘要。
分别读取 `targeted/Results/aot-identity.json`、`full/Results/aot-identity.json` 和两份 `aot.sha256`：同最终 SHA，Sandbox=true，实际 DLL 均在各自任务 Native 目录。
任务身份分别为 `b490a753ebfa43ba81c49133bf700987`、`a6dafdd1d740414fa0f6c29ceaeaecef`；工作目录、BaseDirectory、Results 均位于对应任务，参数与 TRX 文件名一致。
完整任务目录已分别回收，原始 TRX、identity、SHA、包清单和实际运行依赖独立保存，不覆盖 r27。
full 八项失败：三色 WhenRealSolidVideoIsOpenedThenPublicVisualConsumesDecodedPixels；WhenRealPlayersAreRepeatedlyCreatedAndClosedThenAllColorsRemainCorrect；WhenRealPlayersRunInParallelThenAllColorsRemainCorrect；WhenRealPlayerIsReboundAndSourceIsReplacedThenNotificationsDriveAllColors；WhenRealVideoTargetIsLockedThenPresentFailsAndReleasedTargetRetriesOriginalPixels；WhenCallerReleasesRealPlayerThenResourceStillConsumesOriginalPixels。均未等到15秒内真实系统媒体通知，不能声称环境根因已确诊或模块完成。

## 有限实证条款裁决

| 条款 | 裁决 | 实证边界 |
|---|---|---|
| 异步批次→worker→Compose真实像素与FIFO | Closed | WhenAsyncBatchesAreCommittedThenSchedulerComposesPixelsInFifoOrder，两色公开WIC读回 |
| partition 首HRESULT跨后续批次/flush保持 | Closed | WhenAsyncPartitionFailsThenFlushPreservesFirstHResultAcrossLaterBatches |
| disconnect 后channel拥有、继续drain/close | Closed | WhenAsyncConnectionReferenceIsReleasedThenChannelStillDrainsAndCloses |
| failed batch transport 引用释放线程、同channel重入拒绝 | Closed | WhenAsyncFailedBatchIsClosedThenTransportReleaseRunsOnCompositionThreadAndRejectsReentry |
| closed/open失败批次FIFO释放、最后引用后OS worker退出 | Closed | WhenShutdownDrainsClosedAndOpenFailedBatchesThenReleasesAreFifoAndWorkerExits，实际OS线程枚举 |
| callback关闭另一channel无死锁与重复close保护 | Closed | WhenReleaseCallbackClosesAnotherChannelThenQueuedCleanupDoesNotDeadlock，关闭后再次destroy为E_INVALIDARG |
| 双初始化引用、一次deinit仍可工作 | Closed | WhenPartitionManagerIsInitializedTwiceThenOneDeinitializationKeepsAsyncWorkAlive |
| 非法priority初始化不遗留scheduler引用 | Closed | WhenInvalidPriorityInitializationFailsThenNoSchedulerReferenceRemains；仅非法参数，不推广为真实初始化异常注入 |
| 最终异常补充构建/发布/full集成 | Closed | r28重新发布并通过39项；full只有上述媒体8fail |
| 独立取消 | Open | 尚无独立取消入队/执行中/关闭竞争的原生合同与实证，不将正常drain冒充cancel |
| 独立异常注入 | Open | 尚未实证线程启动/CoInitialize失败、分配OOM、Compose或释放callback抛出各异常的首错、完成唤醒与逆序释放；异常catch存在不等于分支验收 |

以上是有限UCE子合同Closed，不等于5B/5C全部原生可达分支、全部架构ABI或PresentationCore E2E Closed。媒体失败不撤销已实证UCE条款，但阻止全量Green与总替换门禁。

## 其他回归与门禁

本轮专用普通测试实际退出1：4211 pass/1 fail/4212 total/0 skip。唯一 WhenRealSystemMediaHasNoFrameThenSoftwareSampleLeaseCanBeClosed，DllNotFoundException，`C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\10.0.12\wpfgfx_cor3.dll` 或其依赖加载失败0x8007007E；合法原版10.0.12是外部先决条件，不改golden/skip。
本轮 GenProto 专用测试退出0：23/23，零跳过。两者仅工具日志回读，未取得新原始TRX/RunID，不虚构身份；普通托管测试不冒充P1 AOT验证。
阶段1–3保留Closed；阶段4 Active，阶段5 Active（有限UCE条款已收口，取消/异常注入及其他合同仍Open），阶段6–7 Pending；A/B/C和正式替换门禁均保留。
下一可委派完整模块为真实媒体通知→sample→composition/target消费→替换/锁失败重试→caller release→并行与关闭退出闭环，不能仅加deadline或skip。本轮不宣称总计划完成。
