# 共享 STA 媒体事件生命周期

> 实现及测试在上一轮完成；本轮补齐此前遗漏的归档与事实源同步。本文件不将整个 AV 播放模块标记完成。

## 原生证据与实现

- `core/av/StateThread.cpp:130-280,310-399,400-610,619-939` → `Core/Av/StateThread.cs`：共享事件线程分支、全局/使用者引用、FIFO、初始化等待、原生 COM 初始化和 Windows 消息泵、显式最终关闭。
- `core/av/mediaeventproxy.cpp:42-190,205-425` → `Core/Av/MediaEventProxy.cs`：两个 RaiseEvent 入口、工作项持有 proxy 引用、事件线程序列化/调用、Shutdown 与 owner 释放。
- `include/processed/wgx_av_types.h:10-40`：AVMediaNewFrame=13，AVEventData 四个 32 位字段及 WCHAR[1]，总固定部分 20 字节。
- `core/av/MediaInstance.cpp:32-127` → `Core/Av/MediaInstance.cs`：唯一 ID、notifier/event proxy 拥有关系。
- `NativeMethods.txt` 增加 CsWin32 COM/消息泵 API。生产仍单一 C# / Native AOT 项目。

## 已贯通路径

`CompositionNotifier → MediaInstance → MediaEventProxy → StateThread STA queue/message pump → EventProxy Stdcall descriptor callback`。

生产线程是真实 STA，不使用测试 dispatcher、同步派发或 Task.Run。启动前设置 ApartmentState.STA，线程内 CoInitializeEx/CoUninitialize 配对；初次测试发现默认 apartment 与 STA COM 初始化冲突，已修复。

成功排队转移工作项所有权；失败由发送方释放 proxy 引用。每个事件工作项独立保留 proxy，释放 owner 不提前销毁排队回调上下文。Shutdown 与事件 callback 共用 EventProxy 锁，已排队事件可安全抑制。MediaEventProxy 不在 owner 锁内取得 callback 锁，避免 callback 重入 Dispose 的锁序死锁。

字符串按 UTF-16 code unit 拷贝，遇 NUL 截断；两段在 offset 16 连续拼接，末尾一个终止符与 padding。最大包 4096 字节，超限在工作项执行时拒绝。记录 LastDispatchResult 用于诊断，不把异步失败伪装成同步 RaiseEvent 返回值。

## 与原生的明确差异

- 只实现 StateThread 的一次性媒体事件工作项，不含 WMP apartment 的可复用 queued flag、ReleaseItem、CancelAllItemsWithOwner 完整方法族。
- managed Thread、Queue 与独占工作项所有权替代原生线程和 intrusive COM item list；每个队列项仍实际保留 EventProxy 引用。
- packet padding 清零，避免暴露未初始化内存；原生动态包 padding 无确定内容，不作逐字节等价声明。
- 内部 MediaInstance 使用 IDisposable，而非导出的 COM 对象；Dispose 防御性 Shutdown。视频必须先注销，不能依赖 GC 回收已注册视频。
- FinalShutdown 必须在可等待、运行时可回调的显式关闭阶段调用，不能在 loader lock 下 Join。
- 正常外部最终 Release 会 Join 并释放初始化 gate；若最后引用在事件线程自身释放，则避免 self-join，但 gate 尚依赖 GC 回收。这一确定性清理边界未关闭。
- 工作项及 unmanaged callback 要求不抛异常；未知 managed 异常不被静默吞掉。native COM exports/QueryInterface 和原生 process-shutdown SEH filter 未迁移。

## 验证

上一轮实际执行：MediaEventLifecycleTests 16/16，主测试 4227/4227，ABI 8/8，Debug 解决方案增量构建 0 警告、0 错误。

覆盖真实 STA 派发、FIFO、共享线程和不同实例 ID、Unicode/golden bytes/4K/NUL、未配对 surrogate、排队后 Shutdown、callback HRESULT、回调内释放、WM_QUIT 消息泵退出、全局引用释放与仍在使用的 lease。

没有原生差分、CoInitialize/分配失败注入、完整窗口消息分派差分、进程退出 SEH 或真实解码验收；测试重新编译曾有警告，不能凭增量结果声明全量无警告。

## 状态与下一生产闭环

事件派发子模块已实现并通过组件测试，Ledger 状态 UnitVerified；不是 AV 整体完成或全部生命周期边界 Accepted。

后续要补全共享线程失败/最终释放边界并实现真实 CWmpPlayer/state engine/presenter/sample 来源，才能接通 MediaPlayer 原生 packet 更新；当前外来 provider E_NOTIMPL 保持。下一轮执行顺序与完整验收边界只维护在 current-work-item.md。
