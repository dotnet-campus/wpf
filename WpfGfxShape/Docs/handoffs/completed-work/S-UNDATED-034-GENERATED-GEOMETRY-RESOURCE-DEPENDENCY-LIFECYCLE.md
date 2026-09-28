# Generated 2D Geometry 资源依赖更新生命周期

## 完成切片

完整实现 `LineGeometry`、`RectangleGeometry`、`EllipseGeometry`、`GeometryGroup`、`CombinedGeometry`、`PathGeometry` 六类 generated 2D geometry 的 command、data、factory、router、dependency update、动态 payload、changed notification 和 duplicate/update/delete 生命周期。

## 原生证据

- `include/Generated/wgx_commands.h` 与 `wgx_commands.cs`：冻结六类 Pack=1 command 的 ID、size、offset、固定值、transform/animation/geometry handle、枚举字段，以及 group/path 尾随 payload。
- `core/resources/data_generated.h`：确认固定 geometry 的 transform 和 animation 指针、group children 数组、combined geometry 双依赖及 path figures 数据模型。
- `core/resources/marshal_generated.cpp`：确认 transform、value animation 和 geometry dependency 的精确类型，group child 禁止 null，group/path 使用声明字节数解析尾随 payload，并注册/注销 notifier。
- `core/uce/generated_process_message.inl`：固定 geometry 使用精确 command size；group/path 使用固定头加尾随 payload；目标 handle 按精确 concrete resource type 查找。
- `core/uce/generated_resource_factory.cpp`：六类资源分别创建对应 geometry DUCE 类型。
- PresentationCore generated geometry 发送者与 `PathGeometry.cs`：确认 transform identity 映射为空 handle、animation handle、children 顺序、fill/combine 枚举和 serialized figures 数据发送方式。
- `core/resources/pathgeometry.cpp` 与 `wgx_misc.h`：确认 `MilPathGeometry`、figure、segment、poly segment 的结构、back pointer、segment count、figure size、last-segment offset、region 限制和尾随字节验证。

## 托管实现

- 新增六类显式 command 结构、fill/combine/segment 枚举和 packet writer，保持 32 位 handle 与原生 Pack=1 layout。
- 将 transform 专用 dependency 基类提升为通用 `GeneratedDependencyResource`，供 transform 和 geometry 共享事务式引用替换、通知传播与最终释放。
- 新增 geometry 资源族类型解析：transform dependency 接受六类 concrete transform，geometry dependency 接受六类 concrete geometry；值动画继续要求精确 value resource 类型。
- 新增强类型 line、rectangle、ellipse、group、combined、path resource data，并接入现有 factory、router、handle table、channel 和 batch 首错路径。
- 固定 geometry packet 先 exact-size 校验；group/path 先校验固定头，再验证声明 payload 长度和内部结构。
- `GeometryGroup` 验证 fill rule、可空 transform、非空 geometry child、4 字节 handle 对齐、全部 child 类型和事务式数组替换。
- `CombinedGeometry` 验证 combine mode、可空 transform 和两项可空 geometry dependency。
- `PathGeometry` 不将 figures 当作任意 byte blob：验证 geometry header size/figure count、figure back size/declared size/segment count/last offset、segment type/back size、固定 segment size、poly count/倍数/溢出、region polyline 限制和无剩余字节。

## HRESULT、所有权与失败语义

- target/dependency unknown、精确类型不匹配、无效 fill/combine/segment 枚举、null group child、声明长度不匹配、截断、内部计数/back pointer/offset/poly count 错误和尾随字节均返回 malformed packet HRESULT。
- transform、animation、combined geometry dependency 可为空；group children 不允许为空 handle。
- 所有新 dependency 和动态 payload 完整解析成功后才持有新引用并一次性提交；失败时旧 data、旧 payload、旧引用和 change count 不变。
- 成功替换先持有新依赖，再提交 data，最后释放旧依赖；重复依赖按出现次数持有和释放。
- value/transform dependency changed notification 传播到 geometry；child geometry notification 继续传播到 group 和 combined geometry。
- duplicate handle 共享同一 geometry identity；最后 handle 删除时确定性释放全部 dependency 引用和动态数据所有权。
- batch 在首个失败 packet 处停止，后续 geometry update 不执行。

## 验证

- generated geometry resource 定向测试：11/11。
- 全量主测试：4047/4047。
- ABI 集成测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未扩展 brush、drawing、visual/target、render-data、effects、UCE connection/composition、生产 ABI 或 PresentationCore E2E；下一轮继续按 generated brush 资源族推进。
