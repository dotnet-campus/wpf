# BuildWafflePipeline waffler 链组装翻译

## 原生证据

- 原生声明位于 `core/hw/HwVertexBuffer.h`，实现与生产调用者位于 `core/hw/hwvertexbuffer.cpp`。
- `CHwTVertexBuffer<TVertex>::Builder::BuildWafflePipeline<TWaffler>` 按 texture-coordinate 索引顺序扫描映射；任意非零 `WaffleModeFlags` 都进入组装判断。
- 每个坐标使用 `MILMatrix3x2` 的两列分别形成 `ax + by + c = k` 分区；先加入第一列，再加入第二列。
- 原生最小分区宽度为 `0.25f` 像素，对应 `a*a+b*b < 16` 才创建 waffler；等于或超过阈值的列跳过。
- 每个有效 waffler 的 sink 指向链中下一项，最后一项连接 vertex buffer；没有有效 waffler 时直接返回最终 vertex-buffer sink，并通过 `fWafflersUsed` 报告是否组装了链。
- 生产调用者覆盖 indexed triangle、line、parallelogram 与 trapezoid waffle 路径。

## 托管实现

- 新增 `Direct3D9WafflePipelineBuilder`，分别组装现有 `Direct3D9LineWaffler` 和 `Direct3D9TriangleWaffler`。
- 新增 `Direct3D9WaffleMode`、`Direct3D9WaffleTextureCoordinate` 以及 line/triangle pipeline 结果值，保留最终输入 sink、是否使用 waffler 和实际 waffler 数量。
- 使用 `System.Numerics.Matrix3x2` 对应原生矩阵列：`M11/M21/M31` 为第一分区，`M12/M22/M32` 为第二分区。
- 保持坐标顺序、列顺序、严格小于阈值、无有效项直达最终 sink，以及现有 line/triangle waffler 的 HRESULT 首错传播。
- 当前组件保持独立，尚未接入完整 `CHwTVertexBuffer<TVertex>::Builder` 或 expanded vertex 消费链。

## ABI、HRESULT 与所有权结论

- 本切片为纯托管 pipeline 组装，不调用 COM，不涉及 Windows `Stdcall` 或引用计数。
- 组装结果持有内部 waffler delegate 链，最终 sink 由调用者提供；不复制或持有 texture-coordinate 集合。
- 下游 HRESULT 原样逐层返回，首错停止；无 waffler 时调用最终 sink 的语义不变。

## 测试与构建

- 新增 `Direct3D9WafflePipelineBuilderTests` 7 项，覆盖无 waffler 直达、双列链、阈值跳过、非 Enabled 的非零 mode、多坐标顺序、triangle 完整分区和最终 sink 首错停止。
- pipeline builder 定向测试：7/7。
- 全量主测试：3882/3882。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未接入完整 vertex builder、expanded vertex、effects/UCE、glyph、shader bytecode、普通 scene/render-target/present、GDI/software-DC、生产 ABI 或 `Reset/ResetEx`。
