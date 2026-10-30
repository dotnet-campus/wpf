# Connection Acquire owner r1：修改前裁决与执行边界

## Before / After
Before：Create 在 Uce Gate 内 Acquire 成功后仅将 scheduler 写入未发布 Connection。NewIdentity checked overflow 或 Connections.Add OOM 被映射为 0x8007000E，output=0，但 manager 引用没有可达 caller 负责归还。
After（实施目标）：Acquire 成功设置本地 ownsScheduler；Connections.Add 成功才清除标志，将引用移交已发布 Connection。外层 try/finally 包围 lock；失败返回或异常离开 lock 后才 Release。同步路径与 Acquire 失败不 Release；成功发布不 Release；原精确 catch/HRESULT 和 output=0 保持。

## 并发证明
Uce Gate 串行化身份/字典发布。finally 在 Monitor.Exit 后调用 manager.Release，manager 自身 CompositionGate 保护引用计数；最后引用 Dispose/Join 不持 Uce Gate，允许 worker callback 重入。只归还本次 Acquire，不清其他 connection/scheduler，不发生双 Release。成功 Disconnect 摘表后在 Gate 外 Release；有存活 channel 时由最后 Destroy 归还。兄弟 CreateChannel 未独立 Acquire，注册失败摘表/Destroy，不应新增 manager.Release；完成失败仅在 disconnected 且最后 channel 时归还 connection 引用。

## 可测与不可测
合法公开调用不能有限地产生真实 OOM 或耗尽 nint identity；禁止反射写计数器/伪造公开故障注入。最小生产修改不为 mock 抽象 helper。失败分支由实际源码所有权与异常映射静态证明，动态回归验证正常 Create、lateCreate、failed partition、disconnect，不宣称 OOM 动态覆盖。现有 ChannelMessageEventLifecycleTests 与 PartitionSchedulerTests 必须进入准确 filter。必要新增单元只覆盖真实 manager 引用合同，不新增可见性/InternalsVisibleTo。

## 回撤与冻结
仅必要 UceChannelExports.Create 修改；公共 ABI、TFM、包、诊断默认不变。失败保留错误和原件，预算内至多一次有证据修正；业务失败不盲重跑。发布前冻结源码及参数；发布后不尾改生产/测试冒用产物。旧 D3F5/845B/F4A 不认证新修复。

## 授权预算
normalbuild<=2；定向run_tests<=2（隐式 Debug 独立计）；Nativepublish<=2（各架构1）；COMpublish<=2（各架构1）；sandbox<=6（各架构3，若 UID discovery 则占用）。目标先 Release Tests build1 + tests1，成功再双架构默认正式 Native/COM 与同 clean official postfailure、candidate postfailure、candidate isolation。内监督120/外180。每调用记录真实参数、exit/warnings/errors/截断。完整替换、显示/HW/loss/音频、自然资源趋势仍 Open；旧 heap 仅有限可回收性。
