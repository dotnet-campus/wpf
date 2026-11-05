# LineWaffler<PointXYA> 一维分区翻译

## 原生证据

- 原生声明位于 `core/hw/Waffler.h`，实现位于 `core/hw/waffler.cpp`。
- `LineWaffler<PointXYA>::Set` 保存分区方程 `a*x + b*y + c`，并仅保留 `c` 的带符号小数部分，避免大平移造成后续数值溢出。
- `AddLine` 对两个端点评分；方向下降时同时反转两个 score 的符号，但保留调用方顶点方向。NaN 输入返回 `E_FAIL`。
- 同一 cell 内直接输出原线段；跨 cell 时按每个整数边界调用对称 `SplitEdge`，逐段交给下游 sink，并在首个下游失败时停止。
- `SplitEdge` 根据距离较小的一侧选择插值方向，比例限制在 `[0,1]`；位置和 alpha 插值结果分别限制在原端点范围内。
- 生产调用者位于 `CHwTVertexBuffer<TVertex>::Builder` 的 waffle pipeline，用于把线段拆成每段仅位于一个纹理分区 cell 内的片段。

## 托管实现

- 新增 `Direct3D9WafflePoint`，保存 `X`、`Y` 和 `Alpha`。
- 新增 `Direct3D9LineWaffler`，近似直译原生 `Set`、`SetSink`、`AddLine`、score、对称 split、插值限制和饱和 floor 行为。
- 下游 sink 使用 HRESULT 风格 `int` 返回值；成功和失败均原样传播，首错停止后续分段。
- 当前切片先建立可复用的一维 line-waffling 生产组件；尚未接入完整 `CHwTVertexBuffer` builder，因为对应 expanded vertex 与完整 pipeline builder 仍需后续独立切片。

## ABI、HRESULT 与所有权结论

- 本切片为纯托管几何拆分，不调用 COM，不涉及 Windows `Stdcall` 或引用计数。
- 未配置 sink 时抛出 `InvalidOperationException`；已配置后的数值失败返回原生对应 `E_FAIL`，下游 HRESULT 原样传播。
- 输入与输出 point 均按值传递，不取得外部集合或资源所有权。

## 测试与构建

- 新增 `Direct3D9LineWafflerTests` 7 项，覆盖同 cell、跨多个 cell、反向输入、巨大平移小数归一化、首错停止、NaN 拒绝和 sink 替换。
- 定向测试：7/7。
- 全量主测试：3866/3866。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未实现 `TriangleWaffler`、完整 waffle pipeline 组装、expanded vertex、effects/UCE、glyph、shader bytecode、普通 scene/render-target/present、GDI/software-DC、生产 ABI 或 `Reset/ResetEx`。