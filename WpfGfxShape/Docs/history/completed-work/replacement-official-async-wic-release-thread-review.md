# x86 official AsyncDrain O6：最终Release线程合同有界审查

状态：ReviewCompleted；后续本轮已按更严格证据门完成[正式收尾](replacement-official-wic-release-thread-contract-closeout.md)：ContractEvidenceInsufficient / FixtureUnchanged / ValidationNotRun。已具体核caller/server与worker/Detach引用路径，但未闭合actual5EBD最后stream owner合法同步合同，不能仅凭未查到固定线程保证弱化断言。以下最小修正为未实施设计而非授权直接追绿；源码/build/test/publish/沙箱新增0。不能用本文把现有失败改成Passed。

## 精确失败与身份
FQN=`WpfGfxShape.ComAcceptance.UceChannelTests.WhenOriginalAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates`，参数(6)，UID/testId=`1e4c35fb-589a-8de1-838e-4845fd1028f4`，executionId=`6d791afe-2711-4b89-aa25-56a429276d1d`。当前x86 full task1e99f9304d9b4e88871dc63b200d0b80；主TRX Result3953–3968、Definition11928–11931。原消息3956：releaseThread=caller=2204，AreNotEqual失败。

亲读`artifacts/writer-fixed-default-x86-readback/Results/async-wic-drain-True-6-False.json`全文1–96：system=true、queueState6、retainExternal=false、completed=false，actual official根wpfgfx_cor3.dll/5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834，runtime10.0.13/X86，testSHA1639A355E25ECBA6A49654CF5C9ADEAAF1E6377AD06106B28237EC46DF8F0A25/MVIDd4264b67-4a92-4c94-b37d-22ca54b5d91e。不是candidate Native身份。

queueState6在现源码表示健康open directClose：failed=false、directClose=true、state=0；不主动CloseBatch/Commit、不走AfterDisconnect flush、不保留external stream。三次Release.External均references1，Transfer0、Disconnect.ReferenceOnly0、Destroy.Drain0。callback.enter order1、callbackOrder2、callback.exit order3；closeReturned4、closeReturn5，均thread2204，disposals1、callbackExited1。说明回调已在destroy返回前完成，不能归为死锁/漏释放；也不能因finally存在就认证后续正常Destroy.Sibling/Deinitialize成功，原JSON无这两正常calls，兜底HR未记录，cleanup完整性Unknown。

## 当前fixture与所有权
亲读`Tests/WpfGfxShape.ComAcceptance/UceChannelTests.AsyncDrain.cs`1–38、79起声明、90–260（79–89首次大块截断，主要结论取已完整可见段）：CreateCallbackStream、DecodeFrame、Wrap、Transfer；先Release wrapper/frame，再disconnect，再Release stream，健康分支要求disposals仍0，再destroy。回调对official不重入；candidate才Callback.DestroySibling。第210行跨两角色硬断言`releaseThread != DrainGetCurrentThreadId()`；第230–231行worker退出只限candidate。retainExternal=true是另一个诊断合同，不能混到本失败。

当前正常引用链未见fixture意外保留external所有权的实证：其可见三次references1及最终disposals1与预期一致，但仅凭返回引用计数不能证明内部每个owner。不能把“有正常引用”直接等同“线程错误”；必须区分最后哪个owner释放及其线程约束。

## 官方可证合同与未证边界
本地官方源码只读对照（未编辑）：
- `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/apifunc.cpp`626–661、718–752：SendCommandBitmapSource额外AddRef用于transport，传给slave bitmap，用完release；失败则本函数Release。写明存活/所有权转移，未写所有最终Release必须composition线程。
- `clientchannel.cpp`765–795：Destroy先IGNORE_HR(SyncFlush)，再connection DestroyChannel，最后Release本channel；注释要求delete命令已处理，zombie例外。802–841：SyncFlush委托SynchronizeChannel。
- `connection.cpp`252–345：SynchronizeChannel会Send同步命令、CloseBatch、Commit并等待；DestroyChannel释放client表引用并CloseChannel。故不能把state0简单说成“从未送往worker的裸packet在caller直接删除”：Destroy内部可能flush，fixture未显式commit不等于官方不会commit。
- `clientchannel.cpp`100–126：channel析构释放connection、关闭事件、释放消息及delete当前command buffer；`clientchannel.h`12–38、`cmdbatch.cpp`90–110：batch/channel引用参与寿命，处理后撤引用。并未得到线程固定承诺。

这些是工作区官方源码，不是已加载servicing二进制精确符号证明；没有证明该commit与actual5EBD逐行相同，亦未穷尽全部WIC/COM owner析构链。因此结论是：**现有证据不支持official“最终Release必须非caller”的强门；本次同线程现象与合法同步关闭释放最后owner相容，但尚不把精确owner调用栈或所有版本合法性说成已证明。** COM引用计数本身不指定必须不同线程；若对象有apartment/线程亲和约束，须另证该对象的实际合同，不能从async连接名称推得。此次未找到官方明确保证非caller的源码。未新增外部网页/源码匹配实验。

旧`replacement-async-drain-directclose-x86-closeout.md`35–61亲读：同official5EBD旧O6 release168/caller9244，旧test D8A5F57F/MVIDe207b681；只证明那一次的实际时序，通过不建立官方永久线程保证，且test身份不同。旧callback在closeReturn后，本次在closeReturn前，两者都不自动构成资源合同违约。未重读旧O6业务原件；引用该报告已明示的有限证据，不迁移绿。

## 下一直接任务：有据最小修正设计（本阶段不实现/不运行）
1. official分支不再以“必须异线程”作为未经证明的硬门；保留releaseThread/caller与顺序为观测。先追溯有没有更强官方线程亲和声明；若找到，停止放宽方案，按最后owner引用链差异排查。
2. 以callback.exit有界等待而非只等disposals==1作为完成门；恰一次最终disposal、callback退出、transfer后到close前资源仍存活、合法close完成、sibling关闭和Deinitialize正常返回、进程退出及资源收敛作官方硬门。callback可以在close返回前或后，不固定两者顺序；retainExternal=true仍要求external引用真正放弃前不销毁，其独立顺序门不混用。
3. 对官方finally补齐可观测兜底清理结果/错误记录设计；不能把忽略HR的finally当正常清理认证。wait失败应记录退出/次数，不以换线程或延时追绿。
4. **candidate专有callback内重入、非caller worker及deinitialize后worker退出门原样保持**；official合同修正不下移候选强度，不改生产、expected或skip。
5. 后续另行授权后，先当前x86准确O6及同族official0/1/2/7/8、retainExternal独立切片验证，再回归candidate C6/C8重入/worker与两架构最终适用矩阵；此文不给执行预算、不重启全盘foreground/paint/ETW/捕获循环。

读证限制：未审计全部Passed资源趋势，未读精确官方二进制符号或最终Release调用栈，未证明所有owner/apartment；当前失败保留，不将合同依据不足伪装产品已修。完整替换/HWtarget身份/consumer loss/audio/自然资源/final矩阵仍Open。
