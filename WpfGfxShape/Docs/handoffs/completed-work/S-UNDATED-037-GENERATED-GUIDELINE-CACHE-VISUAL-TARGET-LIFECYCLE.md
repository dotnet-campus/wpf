# Generated Guideline、Cache、Visual 与 Target 生命周期

## 完成切片

完整实现 generated `GuidelineSet`、`BitmapCache` 资源生命周期，并建立 Visual/Target 核心命令族的强类型状态、依赖通知、事务式树修改、parent-child identity 与确定性释放闭环。

## 原生证据

- `include/Generated/wgx_commands.h`、`wgx_commands.cs` 与 command/resource type 输出：冻结 `GuidelineSet`、`BitmapCache`、Visual 属性/children 和 Target root/clear/invalidate/flags 的 command ID、Pack=1 size/offset、固定字段与 payload 起点。
- `core/resources/marshal_generated.cpp`、`guidelinecollectionresource.cpp` 与 `BitmapCacheMode.cpp`：确认 X/Y double payload 的独立长度与顺序、dynamic 标志、render-at-scale animation、布尔字段和 notifier 生命周期。
- `core/resources/node.cpp`、`node.h`：确认 offset、alpha、transform、clip、content、alpha mask 的强类型状态，Visual child 的 parent identity、索引规则、树修改和通知传播。
- `core/uce/rendertarget.cpp`、`hwndtarget.cpp` 与 `RenderTarget.h`：确认 root、clear color、invalidate、flags 的命令顺序、类型检查和允许的 target flag 集合。
- `core/uce/generated_process_message.inl` 与 `generated_resource_factory.cpp`：确认 command dispatch、目标资源类型和 factory identity。

## 托管实现

- 新增 `GeneratedVisualTargetResources.cs`，包含 `GuidelineSet`、`BitmapCache`、Visual 与 Target 的显式 command 结构、packet writer 和强类型资源状态。
- `GuidelineSet` 独立验证 X/Y 字节长度、8 字节 double 对齐、总 payload 完整消费和 dynamic 布尔值，并在完整校验后事务式替换数组。
- `BitmapCache` 精确解析 `DoubleResource` animation dependency、render-at-scale 和两个布尔字段，复用 dependency notifier 图完成通知传播和最终释放。
- Visual 使用明确属性保存 offset、alpha、transform、clip、content、alpha mask、parent 和 children，不使用通用字段字典。
- Visual dependency 更新先完成 handle 与 resource-family 验证，再 AddRef/commit/release；失败保持旧状态、引用和 change count。
- children insert/remove/clear 维护单一 parent identity、索引边界、自插入/祖先环、重复 child 与跨 parent 规则，并在成功后传播 changed notification。
- Target 使用明确状态保存 root、clear color、最后 invalidated rect、invalidation count 和 flags；flags 仅接受原生允许集合。
- 扩展现有 factory、router、handle table 和 production context，接通 fixed/variable resource update 与 Visual/Target state command dispatch。

## HRESULT、所有权与失败语义

- malformed 固定尺寸、未知 handle、目标类型错误、dependency family mismatch、未对齐/越界 payload、非法布尔值、非法树修改均返回 malformed packet HRESULT。
- Target flags 包含原生不允许位时返回 `E_INVALIDARG`，且旧 flags 和 change count 不变。
- dependency replacement、root replacement 和 tree mutation 均先验证后提交；失败不产生部分状态或引用变化。
- dependency、child 和 target root 通过 listener 引用保持 identity；删除最后 handle 时按 Visual children、Visual properties、Target root 的顺序确定性解除引用。
- batch 保持首错停止，失败 packet 后的命令不执行。

## 验证

- generated guideline/cache/visual/target 定向测试：10/10。
- 全量主测试：4079/4079。
- ABI 集成测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未扩展 effects、完整 render-data、Visual3D/Viewport3DVisual、普通 scene 生命周期、UCE connection/composition、生产 ABI 或 PresentationCore E2E。阶段 4 仍需关闭 render-data 与 3D visual generated 命令族后再评估完成条件。