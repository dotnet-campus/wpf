# Generated 2D Transform 资源依赖更新生命周期

## 完成切片

完整实现 `TransformGroup`、`TranslateTransform`、`ScaleTransform`、`SkewTransform`、`RotateTransform`、`MatrixTransform` 六类 generated 2D transform 的 data、factory、update packet、router、dependency lookup、changed notification、duplicate/update/delete 生命周期。

## 原生证据

- `include/Generated/wgx_commands.h` 与 `wgx_commands.cs`：冻结六类 Pack=1 command 的 command ID、size、字段 offset、固定值字段、animation handle 字段，以及 `TransformGroup.ChildrenSize` 和尾随 handle payload。
- `include/Generated/wgx_command_types.*` 与 `wgx_resource_types.*`：命令 ID 为 `0x72` 至 `0x77`，资源类型 ID 为 `61` 至 `66`。
- `core/resources/data_generated.h`：普通 transform 保存固定值与强类型 animation 资源指针；`TransformGroup` 保存 transform 子资源数量和数组。
- `core/resources/marshal_generated.cpp`：普通 transform 对非空 animation handle 执行精确值资源类型解析，并注册/注销 notifier；`TransformGroup` 使用 `TYPE_TRANSFORM`、禁止 null child，并按 `ChildrenSize` 解包动态数组。
- `core/uce/generated_process_message.inl`：固定命令使用精确 size，`TransformGroup` 使用固定头加尾随 payload，并按精确目标资源类型调用 `ProcessUpdate`。
- `core/uce/generated_resource_factory.cpp`：六类资源分别创建对应 transform DUCE 类型。
- PresentationCore generated `TransformGroup.cs`、`TranslateTransform.cs`、`ScaleTransform.cs`、`SkewTransform.cs`、`RotateTransform.cs`、`MatrixTransform.cs`：确认生产发送者的固定字段、可空 animation handle 和 children handle payload 顺序。
- `core/uce/resslave.cpp`：notifier 注册持有依赖引用，注销释放引用，并将 dependency changed notification 向监听资源图传播。

## 托管实现

- 新增六类显式 command 结构和 packet writer，保持 32 位 handle、Pack=1、固定 offset 与 PresentationCore 发送布局一致。
- 扩展共享 router：五类固定 transform 先做 exact-size 校验；`TransformGroup` 先校验固定头，再由资源层验证声明 payload 长度和 handle 对齐。
- 新增强类型 transform data/resource；固定字段和依赖均使用显式类型，不使用字段字典。
- 扩展共享 factory 与 handle table，使六类资源继续复用现有 create、duplicate、update、delete 和 channel 路径。
- 将 generated 可更新资源统一到共享 `GeneratedUpdatableResource`；值资源成功更新后发送 changed notification。
- 资源基类新增 notifier/listener 图、依赖引用持有、变更传播和最终释放回调，并用重入保护阻止资源环导致无限递归。
- 普通 transform 在解析全部可空 animation handle 且验证精确类型后，一次性注册新依赖、提交新 data、注销旧依赖并发送 changed notification。
- `TransformGroup` 验证 `ChildrenSize`、尾随 payload 精确长度、4 字节 handle 对齐、非 null child 和 transform 派生类型；全部 child 解析完成后才提交替换。

## HRESULT、所有权与失败语义

- unknown target handle、target type mismatch、unknown dependency、dependency type mismatch、null group child、truncated/oversized fixed packet、children 长度不匹配和非 handle 对齐均返回 malformed packet HRESULT。
- animation handle 为零表示无 animation dependency；`TransformGroup` child handle 为零不允许。
- 任一 dependency lookup 或类型校验失败时，旧 data、旧依赖引用、旧 children 和 change count 保持不变。
- 成功替换先持有全部新依赖，再提交 data，最后释放旧依赖；重复依赖按出现次数独立持有和释放。
- dependency 成功更新时 changed notification 传播到直接 transform 和上层 `TransformGroup`。
- duplicate handle 继续共享同一 transform identity；删除最后一个 handle 时确定性注销并释放全部 dependency 引用。
- batch 在首个失败 packet 处停止，后续 transform update 不执行。

## 验证

- generated transform resource 定向测试：11/11。
- 全量主测试：4036/4036。
- ABI 集成测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未扩展 geometry、brush、drawing、visual/target、render-data、effects、UCE connection/composition、生产 ABI 或 PresentationCore E2E；这些能力继续按独立 generated 资源族和后续阶段推进。
