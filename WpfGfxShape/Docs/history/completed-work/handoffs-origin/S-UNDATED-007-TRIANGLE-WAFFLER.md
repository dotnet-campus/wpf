# TriangleWaffler<PointXYA> 多 cell 三角形翻译

## 原生证据

- 原生声明位于 `core/hw/Waffler.h`，实现位于 `core/hw/waffler.cpp`。
- `Set` 保存分区方程并只保留 `c` 的带符号小数部分；`AddTriangle` 计算三个 score，并用固定三次 compare/swap 同步排序顶点和 score。
- score 含 NaN 时返回 `WGXERR_BADNUMBER`；最高 cell 饱和到 `INT_MAX` 时同样拒绝。
- 原生逐个遍历包含顶点的 cell，按 left/right 顶点数量形成 `0x00`、`0x01`、`0x02`、`0x10`、`0x11`、`0x12`、`0x20`、`0x21` 八种有效配置；无几何配置不应到达。
- 每种配置按固定 edge split 和顶点顺序生成 triangle、quad 或 pentagon；quad 拆成两个共享首顶点的三角形，pentagon 拆成三个扇形三角形。
- 任一下游 `AddTriangle` 失败立即保留首个 HRESULT 并停止当前扇形和后续 cell 输出。
- 生产调用者位于 `CHwTVertexBuffer<TVertex>::Builder` 的 indexed triangle、parallelogram 与 trapezoid waffle 路径。

## 托管实现

- 新增 `Direct3D9TriangleWaffler` 与 HRESULT 风格的 `Direct3D9AddWaffleTriangle` delegate。
- 复用 `Direct3D9WafflePoint`，实现分区参数归一化、score 排序、饱和 floor、八种有效配置、对称 edge split、位置/alpha 限制和 triangle fan 输出顺序。
- NaN、最高 cell 饱和或不可能配置返回 `Direct3D9Factory.BadNumberHResult`。
- 当前组件保持独立，尚未在同一切片接入完整 `CHwTVertexBuffer` waffle pipeline。

## ABI、HRESULT 与所有权结论

- 本切片为纯托管几何拆分，不调用 COM，不涉及 Windows `Stdcall` 或引用计数。
- 下游 HRESULT 原样传播，首错停止；成功返回 `S_OK`。
- point 和三角形数据按值传递，不持有调用方集合或资源。

## 测试与构建

- 新增 `Direct3D9TriangleWafflerTests` 9 项，覆盖同 cell 排序、quad/triangle、pentagon、多 cell 范围、插值 alpha、首错停止、NaN、饱和 cell 和 sink 替换。
- triangle 定向测试：9/9。
- line + triangle waffler 定向测试：16/16。
- 全量主测试：3875/3875。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未接入 `BuildWafflePipeline`、expanded vertex、完整 pipeline builder、effects/UCE、glyph、shader bytecode、普通 scene/render-target/present、GDI/software-DC、生产 ABI 或 `Reset/ResetEx`。