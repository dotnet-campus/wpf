# Generated Brush 资源依赖更新生命周期

## 完成切片

完整实现 `SolidColorBrush`、`LinearGradientBrush`、`RadialGradientBrush`、`ImageBrush`、`DrawingBrush`、`VisualBrush`、`BitmapCacheBrush` 七类 generated brush 的 command、data、factory、router、dependency update、gradient payload、changed notification 与 duplicate/update/delete 生命周期。

## 原生证据

- `include/Generated/wgx_commands.h` 与 `wgx_commands.cs`：冻结七类 Pack=1 command 的 ID、size、offset、固定值、枚举、animation/transform/source handle 和 gradient stop 尾随 payload。
- `core/resources/data_generated.h`：确认 opacity/color/point/rect animation、transform/relative transform、gradient stop 数据、tile brush 字段以及 image/drawing/visual/cache source 类型。
- `core/resources/marshal_generated.cpp`：确认所有非空 dependency 的精确类型、gradient stop size/alignment、notifier 注册/注销和动态 payload 释放。
- `core/uce/generated_resource_factory.cpp` 与 `generated_process_message.inl`：确认七类 concrete factory、目标资源类型和固定/动态 command dispatch。
- PresentationCore `LinearGradientBrush.cs`、`RadialGradientBrush.cs` 与 generated brush 发送者：确认 `MilGradientStop` 24 字节布局、发送顺序、tile brush 字段和 null handle 语义。
- `include/Generated/wgx_misc.h`：冻结 color interpolation、mapping、spread、stretch、tile、alignment 和 caching hint 枚举值。

## 托管实现

- 新增七类显式 command 结构、gradient stop 值类型和全部 brush 枚举；保持 32 位 handle 与原生 Pack=1 layout。
- 新增共享 brush writer：固定 brush 写精确 command；linear/radial gradient 写固定头加 `MilGradientStop` 尾随数组；三类 tile brush 复用相同 148 字节布局但保留各自 command ID。
- 新增强类型 solid、linear、radial、image、drawing、visual、bitmap-cache brush resource，并接入现有 factory、router、handle table、channel 和 batch 首错路径。
- 复用 `GeneratedDependencyResource` 完成事务式 dependency replacement、changed notification 和最终引用释放。
- 精确类型解析覆盖 double/color/point/rect value resource、transform family、image-source family、drawing family、Visual 和 BitmapCache。
- gradient update 验证声明字节数、24 字节 stop 对齐、truncated/oversized 和完整 payload 消费后再提交新 stop 数组。
- tile brush 验证 viewport/viewbox、cache threshold、mapping/stretch/tile/alignment/caching 枚举、rect animation 以及各 concrete source family。

## HRESULT、所有权与失败语义

- unknown target/dependency、dependency type mismatch、无效枚举、gradient 长度不匹配或未按 `MilGradientStop` 对齐、固定 packet 尺寸错误均返回 malformed packet HRESULT。
- animation、transform、relative transform 和各 source handle 均允许为空；非空时必须匹配精确 family/concrete 类型。
- 全部 dependency 和 payload 完整校验后才持有新引用并一次性提交；失败时旧 data、gradient stops、source、引用和 change count 不变。
- 成功替换先持有新依赖，再提交 data，最后释放旧依赖；重复依赖按出现次数独立持有和释放。
- dependency changed notification 传播到 brush；资源图重入保护继续阻止环形通知无限递归。
- duplicate handle 共享同一 brush identity；最后 handle 删除时确定性释放全部 dependency 引用和动态 gradient 数据所有权。
- batch 在首个失败 packet 处停止，后续 brush update 不执行。

## 验证

- generated brush resource 定向测试：11/11。
- 全量主测试：4058/4058。
- ABI 集成测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未扩展 dash style、pen、drawing、visual/target、render-data、effects、UCE connection/composition、生产 ABI 或 PresentationCore E2E；下一轮继续按 generated drawing dependency family 推进。
