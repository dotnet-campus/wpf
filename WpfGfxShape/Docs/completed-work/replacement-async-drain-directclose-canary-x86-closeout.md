# AsyncDrain directClose：x86 C6 单体 canary 实际执行收尾

## 终态与独立预算

已实际调用一次 Windows 沙箱，执行现有 clean 的正式 exe，非仅静态审查。task `12d73d1e9c5b4c8f810c2cd86a3496ad`。本阶段沙箱1、测试体1，准确 C6 Passed；新增发现0、源码/项目/新增测试/build/publish/Native发布/终端脚本/再委派全部0，无重试，无 supervise-exit，无整组运行。本次授权预算已用尽，执行与文档收尾完成。

结论：**x86 C6 有限合同认证通过**，仅覆盖该健康 open/directClose 场景的关闭、真实释放回调、candidate sibling 重入和该释放 worker 有限退出。不是完整回归门通过，不升级整体，不冒 official 等价、像素、长期趋势、HW/loss/media/full。完整替换 Open、显示冻结，KN17 与 ownership/postfailure/旧 AsyncDrain 等全部历史账独立保留、不迁移。既有发现 x86/x64 各1、体0仍单列；本次另记 x86 C6 体1，不把内部 adapter 枚举当新增 list-tests 任务。

## 原始调用及原件

证据根（相对工作区）：`WpfGfxShape/artifacts/async-drain-directclose-canary-x86/`。

- `invocation.json`：真实工具字段，返回后立即持久化。
- `tool-return.txt`：真实工具原返回全文，返回后立即保存，保留中文编码损坏，不补造 ExitCode。
- `readback/`：完整任务回收（outputRelativePath 省略），不是只取 TRX。
- `readback/Results/asyncdrain-c6-x86/child.trx`。
- `readback/Results/asyncdrain-c6-x86/Diagnostic/log_261027133936708.diag`。
- `readback/Results/async-wic-drain-False-6-False.json`：完整业务/回调记录。
- `readback/Results/aot-identity.json`：候选磁盘预检及实际入口参数辅助证据。

已确认 clean 实际存在，sourceDirectory=`WpfGfxShape/artifacts/async-shared-ownership-x86-r1/clean`，executableRelativePath=`WpfGfxShape.ComAcceptance.exe`。参数依次为 `--filter-uid 1d757743-a95d-86bc-ad1a-035db6190dac --report-trx --report-trx-filename child.trx --results-directory Results/asyncdrain-c6-x86 --diagnostic --diagnostic-synchronous-write --diagnostic-output-directory Results/asyncdrain-c6-x86/Diagnostic --no-progress --timeout 60s`，均独立 token，无 shell。外层180秒，MTP60秒。localOutputDirectory 为上述 readback。

原工具输出为 MSTest4.0.1 / win-x86 / .NET10.0.13、总计1/失败0/成功1/跳过0/524ms及原件路径；没有独立数值 OS exit。完整回收目录按名称查找 std 无匹配，没有独立 stdout.txt/stderr.txt，不能声称分别读取或 stderr 为空；已亲读并保存工具可见合并输出及完整 TRX/diag，TRX 无 Output/ErrorInfo。未为补流或 OS exit 重跑。

## 亲读范围与准确测试体

先读发现收尾全文1–51（截断35以后补读）及 manifest 必要字段/末段至67，复用已有 `.coding-agent` 静态快照，不把其零执行历史当本次结果。

本次亲读 TRX1–26全文、identity单行全文、业务JSON1–90及89–118全文；diag1–25、26–48、49–65（尾部截断后62–65补齐，terminal56另单行复读）覆盖完整65行。没有拿头部摘要代替尾部或用源码断言冒执行事件。

TRX第8/11/13/17行一致为 UID `1d757743-a95d-86bc-ad1a-035db6190dac`，`WhenAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates (6)`；唯一 UnitTestResult Passed，executionId `f659ecb0-07bb-4bc5-9f2c-4d7758d6736f`不是 UID。第24行 total/executed/passed=1/1/1，failed/error/timeout/aborted/notExecuted等0。diag49为该UID InProgress，51–54真实Executing/Executed/RecordEnd/Result，56为该UID PassedTestNodeStateProperty（System.Int32参数），并非仅Discovered。diag45内部 adapter Found1822 不表示执行1822体，也不是新的发现运行。

## 实际身份

业务JSON2–14：system=false、queueState=6、retainExternal=false、completed=true，pid10928，X86 / .NET10.0.13；path=actualPath=`C:\CodingAgentSandbox\Tasks\12d73d1e9c5b4c8f810c2cd86a3496ad\Native\wpfgfx_cor3.dll`。

- candidate actual SHA256：`15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620`，version1.0.0.0。
- test SHA256：`D8A5F57FE4A61CC28459A89DCB50DC75E0ADE80E1D0AEDC35D7E18AC5069A7BB`。
- test MVID：`e207b681-4c15-453e-b0a4-a1690fa3db78`。

均匹配 manifest 期望。identity 另见 Sandbox=true、PointerSize4、同路径/hash及完整实际参数；它只作磁盘预检辅助，不代替业务 actual。按已审查实现，actualPath 是 NativeLibrary.Load 返回 handle 与 ProcessModule.BaseAddress 相等匹配得到，再 hash 该路径文件；未打印 handle/BaseAddress数值，没有GetModuleFileName或内存映像hash/抗替换证明。不借其他合同更丰富身份日志填补本次边界。

## 完整业务与真实清理证据

业务 calls 全文：Initialize、Connect.Async、Channel、Sibling、Create.Bitmap、Wrap、Transfer、Disconnect.ReferenceOnly、Destroy.Drain、Callback.DestroySibling、Deinitialize 的 result 全部0；三次 Release.External 的 references 均1，不能当 HRESULT0。未进入失败/CloseBatch/Commit分支，符合健康 open C6。externalReleaseReferences=4294967295 是本分支未走 surviving-external 路径的记录，不冒成功返回。

事件实际顺序：callback.enter order1/thread2464/disposals0；callbackOrder2；callback.exit order3/thread2464/disposals1；closeReturned4；closeReturn事件order5/thread8112/disposals1；deinit.before6、deinit.after7，均thread8112/disposals1。真实 stream callback count（disposals）=1，callbackExited=1，releaseThread2464，非caller8112；reentry=0且Callback.DestroySibling正常 Check result0。此次回调退出在主Destroy返回前，不能外推所有调度都必须该偏序；waits=[]不等于回调未发生。

主 Destroy.Drain 正常返回0；引用断连0、外部wrapper/frame/stream释放记录及最终callback1支持该资源释放合同，Deinitialize0及前后事件齐全。不把finally忽略HR的兜底当正常清理证据，不虚构不存在的cleanupErrors/primary字段。

既有实现232–233的候选非retainExternal健康分支，在Deinitialize后执行5秒内 `!DrainThreadExists(releaseThread)` 断言，之后才设completed=true。本次准确C6 TRX Passed、同UID成功terminal及最终completed=true联合支持**该次实际执行通过这条worker退出断言**，不是只因源码存在assert就宣称通过。JSON没有单独worker.exit事件或退出耗时，不冒独立线程监控；不认证所有worker或全进程无异常退出。

## 退出证据分层与下一门

diag65：会话 `e788a6d5-26bf-476e-aa7f-ac502cc4291a` ended with exit code '0'，0.5656557秒；diag63及TRX确认报告完成。TRX准确1/1/1 + 完整业务正常清理/回调退出 + 同UID Passed terminal + MTP会话0，足以按既定规则有限认证本合同及MTP正常会话完成。

**独立 OS 数值 exit 未证**，没有supervision.json；工具未返OS ExitCode，业务completed也不是进程exit。不能写外层Exit0/监督Exit0或全进程无异常强认证。分离stdout/stderr缺件边界如上，保留原合并返回，不造补件。

唯一下一最小建议：另授权 **x86 剩余 C8/O6/O8 三个单UID、各独立任务**，沿现有clean与同一回读规则，首错即停；只是建议，本次不运行。x64四体仍NotRun，暂不作为并列下一执行门。所有旧发现/静态报告为历史快照，其“C6尚未授权/NotRun”已被本报告真实终态取代，其他七体仍NotRun。无本次收尾待办。
