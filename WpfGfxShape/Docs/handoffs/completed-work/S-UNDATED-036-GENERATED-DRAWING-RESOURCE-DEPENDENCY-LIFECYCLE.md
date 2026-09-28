# Generated Drawing 资源依赖更新生命周期

## 完成切片

完整实现 `DashStyle`、`Pen`、`GeometryDrawing`、`GlyphRunDrawing`、`ImageDrawing`、`VideoDrawing`、`DrawingGroup` 七类 generated 资源的 command、data、factory、router、dependency update、动态 payload、changed notification 与 duplicate/update/delete 生命周期。

## 原生证据

- `include/Generated/wgx_commands.h` 与 `wgx_commands.cs`：冻结七类 Pack=1 command 的 ID、size、offset、枚举、dependency handle、dash payload 与 drawing children payload。
- `core/resources/data_generated.h`：确认 dash double 数组、pen brush/dash/animation、叶子 drawing 依赖，以及 drawing group children、clip、opacity mask、transform、guideline 和 render-option 状态。
- `core/resources/marshal_generated.cpp`：确认全部非空 handle 的精确 family/concrete 类型、动态数组禁止 null child、notifier 注册/注销和 payload 释放。
- `core/uce/generated_resource_factory.cpp` 与 `generated_process_message.inl`：确认 concrete factory、目标资源类型和固定/动态 command dispatch。
- `include/Generated/wgx_misc.h`：冻结 pen cap/join、edge mode、bitmap scaling mode 与 ClearType hint 枚举。

## 托管实现

- 新增七类显式 command 结构、pen/group 枚举和 packet writer，保持 32 位 handle 与原生 Pack=1 layout。
- 新增强类型 dash、pen、五类 drawing resource，并接入现有 factory、router、handle table、channel 和 batch 首错路径。
- `DashStyle` 验证声明字节数、8 字节 double 对齐、完整 payload 消费，并在事务成功后替换 dash 数组。
- `Pen` 精确解析 brush family、double animation 和 concrete DashStyle，并校验 cap/join 枚举。
- 叶子 drawing 精确解析 brush、pen、geometry、GlyphRun、image-source family、MediaPlayer 与 rect animation。
- `DrawingGroup` 验证固定状态枚举、可空 clip/opacity animation/opacity mask/transform/guideline dependency、非空 drawing child、4 字节 handle 对齐和完整 children payload。
- 复用 `GeneratedDependencyResource` 完成事务式 dependency replacement、changed notification、重复引用计数和最终释放。

## HRESULT、所有权与失败语义

- unknown target/dependency、dependency type mismatch、无效枚举、dash/children 长度不匹配或未对齐、null group child 和固定 packet 尺寸错误均返回 malformed packet HRESULT。
- 各普通 dependency 允许为空；DrawingGroup child 不允许为空 handle。
- 全部 dependency 和 payload 完整校验后才持有新引用并一次性提交；失败时旧 data、动态数组、引用和 change count 不变。
- 成功替换先持有新依赖，再提交 data，最后释放旧依赖；重复依赖按出现次数独立持有和释放。
- changed notification 从 value/brush/geometry/glyph/image/media dependency 传播到 leaf drawing，并从 child drawing 传播到 DrawingGroup。
- duplicate handle 共享同一资源 identity；最后 handle 删除时确定性释放全部 dependency 引用和动态 payload 所有权。
- batch 在首个失败 packet 处停止，后续 update 不执行。

## 验证

- generated drawing resource 定向测试：11/11。
- 全量主测试：4069/4069。
- ABI 集成测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未扩展 `GuidelineSet`、`BitmapCache`、visual/target commands、render-data、effects、UCE connection/composition、生产 ABI 或 PresentationCore E2E；下一轮继续完成剩余 generated value/dependency resources 并建立 visual/target 命令闭环。
