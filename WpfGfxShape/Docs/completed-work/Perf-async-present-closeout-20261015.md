# 异步 Present / HRESULT 最终集成：候选通过，原版通知差异 Open

## 最终身份与结论

本轮已先将四份旧文档标 WIP，再实际 build、publish、沙箱执行并回读最终 JSON、TRX、identity。当前生产 SHA256：`1DAA53397996A44E78C90D9070DFC6B68F4132AC121A4EFB13D64D2516EB0168`。旧 C8108… 的 222/222、1429/1429 不作为新证。

原版为 WindowsDesktop **10.0.1**，SHA256 `94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`，不是完整 10.0.12 golden。最终执行器 SHA256：`920373FFDCE36569A10012D7D6958CE3FC02DDE62C67D2CCE4143A6F3152D1C3`。

**候选异步组通过，相关集及 COM full 通过；原版异步组失败，整体对照仍 Open，不得称全组 Green/Closed。** 本轮未扩展 glyph，也未按不稳定原版通知数更改生产。

## 生产合同与本轮修改

- `PartitionScheduler` 保留 partition 内部首个失败；异步 zombie completion 对外将普通失败映射为 `0x88980406`（RENDERTHREADFAILURE），OOM/OOVM 按原生映射保留。重复 flush 不覆盖内部首错。此次不修改同步返回合同。
- `GeneratedVisualTargetResources.UpdateWindowSettings` 当前接受 69–72 字节，71 字节不再被候选拒绝。原生结构为 72 字节、末尾 gdiBlt 位于 68–71；原生 generated dispatch 的完整 sizeof 检查仅 DEBUG，release 不能据此推导所有短包均安全或零填充。
- 最终71字节原版与候选均 Send/Close/Commit=0、第一次和重复 SyncFlush=0；明确接受该包。原版先前一次执行出现失败，说明缺失尾字节/padding 的稳定性尚不能由单例冻结；未硬编码输入、未扩大枚举邻近长度或伪造 padding oracle。
- 本轮修改 `AsyncUceChannelTests` 两处异步 flush 预期为精确 `0x88980406`；原有同步测试不改。修改 PublicAbi 成功 settings 场景通知预期为一次：重复 SyncFlush 屏障不等于第二次 compose。没有改成按候选/原版分别放宽或允许任意通知数。
- flags 0/2/0、同步 signalPresent=false、异步 signal bit2 保持。候选 dirty/empty 为0/1/0；空visual清屏、WndProc真实回调内detach、关闭无通知均执行。

## 最后测试源码后的原始证据

根目录：`WpfGfxShape/Investigations/Perf-async/`。下列最终 JSON 已分段回读（包括 Error/Passed/identity），不是只看执行完成提示。

| 集合 | 保存路径 / RunID | 真实结果 |
|---|---|---|
| 候选 core+七场景 | `final-integration-candidate/Results/efcbf0064e3c4d43920f94d53472d7c8/report.json` | Error=null，Passed=true，LifecyclePassed=true；生产SHA如上 |
| 原版 core+七场景 | `final-integration-reference/Results/f8c6de6f9f744fd8ad51b2e0ad4ec3ac/report.json` | Error=AsyncPerf.ContractMismatch，Passed=false，LifecyclePassed=false |
| 相关集 | `final-integration-related/Results/final-integration-related.trx`；RunID `3fcf7147-b973-4543-80d5-d1087c8d46f0` | 222 executed / 222 passed / 0 failed / 0 notExecuted |
| COM full | `final-integration-full/Results/final-integration-full.trx`；RunID `11b6cf6b-5af1-43cf-acfa-ac678818995e` | 1429 executed / 1429 passed / 0 failed / 0 notExecuted |

相关/full各自 `Results/aot-identity.json` 已完整回读，Sandbox=true，实际Native SHA均为上述1DAA…；任务分别为 `f389c4991316457998480939ac2b39b3` / `737741b515f543898fbaddc8e51a02b5`。full TRX第25–26行明确记录下列两项 Passed：

- `WhenShutdownDrainsClosedAndOpenFailedBatchesThenReleasesAreFifoAndWorkerExits`：失败closed/open batches、FIFO release、OS worker退出。
- `WhenReleaseCallbackClosesAnotherChannelThenQueuedCleanupDoesNotDeadlock`：真实release callback跨channel关闭，无死锁。

最终错误场景 invalid-target / invalid-settings / zero-target / wrong-type / short-size(68) / invalid-target-and-settings：两端均 Send/Close/Commit=0、首次及重复flush=`-2003303418` (`0x88980406`)、零通知、Destroy/Disconnect/Deinitialize=0。这里是包验证失败，不冒充底层设备resize/Present失败注入。两端WndProc detach Count=1、HRESULT=0；关闭后观察窗口均零通知。

## 不可隐去的失败与具体 Open

最终原版71字节场景，首次及重复flush成功，通知Count=2、ExpectedCount=1、Passed=false；候选Count=1、Passed=true。原版最终core通知组通过，但先前integration-reference/verified-reference有dirty/rebound重复通知，不把后一次通过用于删除失败或称已定位根因。完整原版report依然失败，有限HRESULT匹配不覆盖此通知差异。

首次旧执行包候选为另一SHA `1D1738B042431FF0B06DCD3B19F69A197BB028395D650A319E1FFDA6736A6D4A`，不是最终生产；保存在integration-candidate。重发布后verified-candidate因旧的两次通知预期失败。integration-related曾221/222失败，精确暴露旧async HRESULT预期；修正后重新发布测试，取得上述最终结果。历史失败证据不删不冒充成功。

仍 Open：原版重复通知/缺失尾字节的buffer padding稳定合同、设备resize/Present失败恢复、OOM/OOVM真实注入、原生异常边界、未证并发取消/关闭组合、完整PresentationCore消费者矩阵。不得把源码映射或COM full通过说成这些故障已注入验证。

## 构建、发布与执行

本轮实际执行生产 `dotnet build` Release/win-x64，退出0、0警告/0错误；随后生产 `dotnet publish` Release/win-x64退出0，产物位于 `artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。生产本轮未再修改，测试修改后重新发布COM标准MTP小包及PublicAbi执行包；最终测试均在上述发布之后执行。PublicAbi最终包位于 `Investigations/Perf-async/final-integration-package`。

PublicAbi参数：`candidate perf-async` / `reference perf-async`。相关集filter：`FullyQualifiedName~Uce|FullyQualifiedName~RenderTarget|FullyQualifiedName~Perf|FullyQualifiedName~AcceptancePathsTests`；full无filter。两者均指定`--report-trx`、相应TRX文件名及`--results-directory Results`。无终端脚本、fake、Host、skip或直接伪造通知。

## 总门与后续

候选PE Missing2（MilPlayer_Create/Process）/Extra0，PeContractPassed=false、AllAbiPassed=false、FixedGolden1012Satisfied=false；正式总门Red，无替换资格。七阶段、A/B/C、E2E-01–09硬门禁全部保留。authority、ARM64、完整10.0.12 runtime及player官方protocol/ownership/oracle仍需用户输入。DEC-0004进程驻留合同不变，资源释放、callback静止和线程退出仍验收；unload/reload不是已通过项。

glyph真实有效消费方法族留待后续，不在本轮扩展。当前async集成保留上述具体Open；不以本轮回归Green掩盖原版reportPassed=false。
