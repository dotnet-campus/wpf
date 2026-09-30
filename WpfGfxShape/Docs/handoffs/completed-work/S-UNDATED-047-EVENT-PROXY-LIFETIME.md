# EventProxy descriptor 回调生命周期

## 本轮完成

纯 C# / Native AOT 路线不变。新增 `Core/Av/EventProxy.cs` 与 `EventProxyTests.cs`；补齐上一轮 S-UNDATED-046 归档及 composition Ledger satisfiedEvidenceIds。

原生证据：`core/av/eventproxy.h:25-107` 的三指针 descriptor、`eventproxy.cpp:127-267` 的 Create/RaiseEvent/Shutdown/AddRef/Release；析构中的进程关闭 SEH 特例见 `eventproxy.cpp:74-126`。

## 所有权与错误规则

- descriptor 拷贝到稳定 NativeMemory，Stdcall 回调收到同一地址及原 handle；不是传递可移动托管字段地址。
- Create 校验两个函数指针，失败不取得 handle 所有权；分配失败返回 E_OUTOFMEMORY。
- RaiseEvent 在锁内调用并原样返回 HRESULT；Shutdown 共用锁，只屏蔽事件，不提前调用 dispose。
- 最后一个引用调用 dispose 一次并释放 descriptor 内存；释放后不可复活、不可再次回调。
- 回调期间临时保留一个引用，防止重入 Release 提前释放 descriptor；这属于托管承载安全差异，非原生逐字复制。回调必须遵守 ABI，不得让异常越过 unmanaged 边界。
- 原生 Interlocked 计数改为同一 Monitor 下计数；尚无吞吐或完整原生并发差分。

## 验证

定向 7/7，主测试 4211/4211，ABI 8/8，Debug 增量构建 0 警告、0 错误。测试重新编译有警告；增量零警告不代表全量无警告。

测试覆盖尺寸/偏移、HRESULT、Shutdown、最终释放、descriptor 稳定地址、释放后保护和无效 descriptor。当前不宣称已经覆盖进程退出、所有回调重入或线程竞争情形。

## 未完成及后续整体任务

这是内部 lifetime owner，不是导出的 IMILEventProxy COM 对象，没有 QueryInterface/COM vtable。没有复制原生 CLR process-shutdown SEH filter，必须在显式、运行时仍可回调的关闭阶段释放；进程退出兼容性未验收。

CMediaEventProxy/MediaInstance 和 StateThread 的共享 STA 消息泵仍未迁移；不得直接把本类 RaiseEvent 接到 CompositionNotifier 以同步派发冒充原生队列。后续整体交付 StateThread 事件队列、EventItem 包/引用与 MediaInstance 初始化—通知—Shutdown—释放。真实播放器、EVR sample 来源及旧 provider E_NOTIMPL 仍未解除，阶段 4 Active。
