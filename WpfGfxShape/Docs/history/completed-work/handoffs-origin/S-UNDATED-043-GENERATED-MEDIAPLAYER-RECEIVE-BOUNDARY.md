# MediaPlayer 协议接收与消费者引用边界（完整生命周期受阻）

## 本轮结论

本轮原定 source/provider 注册、通知、VideoDrawing/render-data 释放生命周期未完整完成。已交付可独立验证的强类型接收、失败引用清理与消费者 identity/release 回归。不得将本归档视作真实 provider 注册完成；阶段 4 保持 Active。

## 原生证据

- `include/Generated/wgx_commands.h:164-171` 和 `include/wgx_core_types.w:336-343`：pack 1，Type/Handle/UINT64 pMedia/BOOL notifyUceDirect，大小 20，字段偏移 0/4/8/16；pMedia 不是 native-width 字段。
- `core/uce/apifunc.cpp:600-620`：发送完整命令，序列化不在此 AddRef。
- `core/uce/generated_process_message.inl:581-607`：精确 handle/type 校验后调用 ProcessUpdate。
- `core/resources/videoslave.cpp:244-300`：先写 notify flag；null 返回 E_HANDLE；QI provider；只允许一个 provider pointer identity；先 RegisterVideo 后 RegisterResource；失败留下 provider 给析构清理；cleanup 释放传输引用和临时 QI 引用。
- `core/av/internal.h:72-89`：provider 注册/注销接收 `CMilSlaveVideo*`，不是 IUnknown 或回调接口。
- `core/av/CompositionNotifier.cpp:63-195`：锁内存储非拥有 slave 指针；直接调用 `NewFrame` 与 `InvalidateLastCompositionSampleTime`。
- `core/resources/videoslave.cpp:49-87,130-199`：NewFrame 仅在 direct 模式请求 composition；析构 EndComposition → provider UnregisterResource → composition UnregisterVideo → provider Release。
- `core/uce/composition.cpp:538-553,1082-1125`：注册列表不持有引用；BeginComposition 报告新帧才 NotifyOnChanged。

## 阻塞与缩小范围

当前生产项目不存在 CMilSlaveVideo C++ 对象兼容桥、AV CompositionNotifier 或 UCE video composition owner。COM provider vtable 本身可调用，但不能把托管实例、GCHandle 或任意分配内存当作 CMilSlaveVideo* 交给真实 provider；非虚 C++ 回调会直接使用原生对象布局。现有 Direct3D9VideoDrawPipeline 只是 bitmap draw 承载，不提供此契约。

因此没有添加 no-op RegisterResource、伪造 C++ 对象或推测性 scheduler。provider QI 成功后明确返回 E_NOTIMPL 并释放两份临时引用，不保留 provider，不宣称单一 source 注册状态已实现。这是已标注的失败接收边界，不是原生 ProcessUpdate 全量直译。

## 托管差集

- 新增 `GeneratedMediaPlayerResource.cs`：固定宽度命令、writer、强类型资源、严格包长与 32 位指针可表示性检查、null/QI/未实现桥接错误、显式 Stdcall QI、finally 释放。
- `GeneratedProtocol.cs`、`GeneratedValueResources.cs`：factory/router 接入。
- 进入 handler 且指针可表示后消费一份传输引用；handler 前拒绝、已释放资源及未执行批次包仍由调用者清理。
- 未发送资源变更通知：source 更新并不等同于帧就绪。
- VideoDrawing、DrawVideo、DrawVideoAnimate 复用原有依赖计数，跨表 duplicate 与消费者保留强类型身份，最后依赖删除后释放。

## 验证

- 新增定向测试 11/11：size/offset、独立 golden bytes、null、QI failure with output/null output、E_NOTIMPL 清理、错误 handle、短包、释放后保护、批次首错、消费者与 duplicate 最终释放。
- 主测试 4174/4174；ABI 8/8。
- Debug 解决方案增量构建 0 警告、0 错误；测试重新编译有既有 MSTEST0044 警告。
- ABI 探针通过不代表真实 MediaPlayer 注册 ABI 已验收；未运行 x86、真实媒体/provider 或 PresentationCore E2E。

## 后续合并闭环

下一任务仍为 AV provider/composition 注册—帧就绪通知—确定性释放完整闭环，须先以原生调用链确定纯托管 AV 通知迁移边界或可验证桥接方案，再移除 E_NOTIMPL；不能把后续切成仅计数、仅指针占位任务。
