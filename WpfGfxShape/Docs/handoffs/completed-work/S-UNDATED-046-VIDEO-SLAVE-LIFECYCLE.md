# VideoSlave/composition 视频生命周期（补齐上一轮归档）

本文件补记上一轮已运行的证据，不表示本轮重新执行。路线固定纯 C# / Native AOT。

## 实现与证据

- `core/resources/videoslave.cpp:35-300` → `Core/Av/VideoSlave.cs`，作为 GeneratedMediaPlayerResource 分部实现，复用协议资源引用计数。
- `core/uce/composition.cpp:525-585,1082-1124` → `Core/Av/VideoComposition.cs` 的视频方法族。
- GeneratedProtocolHandleTable 支持注入 video composition owner；原生 COM packet 路径未改变。
- 单源 identity、同源不重复注册、异源拒绝、两级注册失败后可析构状态、renderer 获取/Begin/End/额外引用、帧就绪通知、最终注销释放已有实现。
- VideoDrawing、DrawVideo/DrawVideoAnimate、duplicate/delete 通过真实协议资源测试覆盖；provider/renderer/scheduler 是未迁移外部依赖的测试替身。

## 偏差与未完成

- direct flag 使用 Volatile；sample time 使用 Interlocked 和 invalidation version，避免失效请求被 renderer 写回覆盖。尚无原生并发差分。
- 内部托管对象身份替代内部 provider 指针身份，未宣称兼容旧原生 provider。
- CComposition 仅迁移视频列表和处理方法，不含 partition scheduler；调用方须保证 composition-thread ownership。
- MediaInstance、CMediaEventProxy、CEventProxy、共享 STA StateThread、CWmpStateEngine/EVR/sample scheduler、真实媒体创建仍缺失。外来 COM provider 保持 E_NOTIMPL。

## 上轮实际验证

- VideoSlaveTests 19/19；主测试 4204/4204；ABI 8/8。
- Debug 增量构建 0 警告、0 错误；重新编译出现 CA1416 和 MSTEST0044，不能声明全量零警告。
- 阶段 4 Active，不具备完整媒体或 wpfgfx 替换资格。

## 连续推进顺序

以真实 UI 事件派发生命周期为下一个完整交付单元：CEventProxy descriptor 回调/Shutdown/引用释放 → StateThread 共享 STA 队列及消息泵 → CMediaEventProxy EventItem 包与引用 → MediaInstance 所有权；再接 CWmpPlayer/state engine/presenter/sample 来源。禁止同步派发冒充原生异步队列。
