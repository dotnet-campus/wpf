# 纯 C# CompositionNotifier 方法族

## 决策与范围

用户已明确继续纯 C# / Native AOT，不增加原生桥。该决策关闭路线选择关口，但不等于真实 AV 依赖已齐备。

本轮交付整个 CompositionNotifier 注册—通知—注销方法族，而非空注册。真实 MediaInstance/event proxy、播放器创建、sample scheduler、video composition owner 尚未迁移，因此缩小为可独立验证的通知组件；没有移除 MediaPlayer 接收的 E_NOTIMPL，没有宣称完成媒体生产闭环。

## 原生证据与映射

- `core/av/CompositionNotifier.cpp:63-217`、`CompositionNotifier.h:24-89` → `Code/WpfGfxShape/Core/Av/CompositionNotifier.cs`。
- `core/av/UniqueList.inl:36-117` 提供唯一性、头插及注销语义。
- 编码前在 Ledger/records.jsonl 登记原路径、目标路径、依赖阻塞与验证要求；evidence.jsonl 登记 EVID-AV-NOTIFIER-UNIT。

## 实现与偏差

- 注册按对象引用身份去重，头插；重复注册成功；节点分配 OOM 转换为 E_OUTOFMEMORY。
- 注册、注销、帧回调遍历、sample-time invalidation 共用锁。注销返回后不再有被其阻挡的帧回调；UI 事件在锁外派发。
- 每个资源都调用 NewFrame，即使先前资源已要求 UI；多个资源及 outstandingUIFrame 合并为一次 UI 事件；无资源也消费 pending UI。
- 遍历先保存 next，保留当前 callback 注销自身的原生顺序。
- 使用内部 ICompositionVideoNotification 与 Action 作为未迁移视频/事件依赖接缝及测试边界，没有外部 COM/C ABI 变更，也没有把它们传给旧原生 provider。
- 原生非拥有引用改为 GC 对象引用，不增加协议或 COM 引用计数；GC 会保活注册对象及事件委托目标，后续 MediaInstance 必须在确定性关闭时注销，不能依赖 GC 实现资源释放。
- 原生 Init 中的媒体 ID/跟踪与 critical-section 初始化改为构造函数初始化；尚未接上真实 MediaInstance。内部 callback 按原生要求不得抛出异常，不新增吞异常逻辑。

## 验证

- CompositionNotifierTests：11/11。
- 主测试：4185/4185。
- ABI 测试：8/8。
- Debug 解决方案增量构建：0 警告，0 错误。
- 测试重新编译出现既有 MSTEST0044 警告，不声明全量编译零警告。
- 并发测试覆盖锁外 UI 派发及注销等待在途回调；有限时间等待只作为回归证据，不代表穷尽并发模型。
- 未进行本组件原生差分、真实播放、Native AOT 媒体导出或硬件 sample 消费验收；既有 ABI 测试通过不能替代这些验收。

## 下一闭环

在已批准纯 C# 路线上联动迁移 MediaInstance/event proxy 和 VideoSlave/composition 生命周期，最终由真实 AV 创建与 sample 来源驱动当前通知器。先补齐原生 event proxy、关闭及 owner/scheduler 的证据和 Ledger，再实现；不以测试假 provider 接入来删除 E_NOTIMPL。唯一下一动作见 current-work-item.md。
