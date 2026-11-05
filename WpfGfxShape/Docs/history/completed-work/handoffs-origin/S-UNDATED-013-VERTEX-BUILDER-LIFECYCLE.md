# Vertex builder outside stratum 与提交复用生命周期闭环

## 原生证据

- 核对 `CHwTVertexBuffer<TVertex>::Builder::PrepareStratumSlow`、`EndBuildingOutside`、`EndBuilding`、`ExpandVertices`、`FlushInternal`、`RenderPrecomputedIndexedTriangles`，以及 `CHwTVertexBuffer<TVertex>::Reset`、`DrawPrimitive` 和 `CHwPipeline::Execute`。
- outside stratum 以 `[FLT_MAX, -FLT_MAX]` 为初始哨兵；首个 stratum 前、非相邻 strata 间及最后一个 stratum 后分别生成零 coverage 矩形。
- trapezoid stratum 开始时使用 `min(OutsideLeft, trapezoidLeft)` 创建左侧退化 strip，结束时使用 `max(OutsideRight, lastTrapezoidRight)` 创建右侧退化 strip。
- `EndBuilding` 先完成 outside 补形，再统一扩展 accumulated vertices；waffling 的 packed-coordinate 后处理位于扩展之后并保持 triangle-list、triangle-strip、line-list 的原生分组边界。
- `FlushInternal` 的顺序为 pipeline color-source/state realization、`EndBuilding`、precomputed indexed draw 或普通 primitive draw；无返回缓冲区请求时，无论成功失败都标记已 flush、reset vertex buffer 并清空 precomputed 状态。
- precomputed indexed 路径将源顶点转换到锁定 vertex buffer，将 `UINT` indices 收窄到 `WORD`，解锁后设置 stream/index buffer 并提交 indexed triangle-list；首个失败 HRESULT 返回，仍执行锁定资源清理。

## 托管实现

- 新增 `Direct3D9VertexBufferBuilder`，统一保存 indexed triangle-list、non-indexed triangle-list、triangle-strip、line-list 和 precomputed indexed geometry。
- 实现 outside bounds 与 need-inside 状态、首部/中间/尾部空带补形、trapezoid 左右补形、相邻 strata、结束哨兵和重复 `EndBuilding` 保护。
- 复用 `Direct3D9ExpandedVertexConverter` 执行 accumulated vertex 原地扩展以及 precomputed vertex 分离转换，不复制映射逻辑。
- 提供 packed-coordinate 后处理回调，并按 triangle-list 三顶点、triangle-strip 六顶点和 line-list 两顶点分组调用。
- 提供 pipeline realization、四类 primitive draw、precomputed indexed draw、可选返回 builder、reset flush、失败后清理和 `BeginBuilding` 后再次使用的生命周期。
- 当前 builder 已形成独立生产组件和完整测试契约，但尚未接入 `Direct3D9ShaderPipelineInitializer`、`Direct3D9FixedFunctionPipelineInitializer` 与真实 geometry-generator 执行路径；该接线被保留为下一轮完整生产集成闭环，而非继续拆分 builder 内部生命周期。

## HRESULT、状态与所有权结论

- converter、pipeline realization、packed-coordinate 和 draw 回调由调用方拥有；builder 仅保存引用，不负责外部资源释放。
- outside 状态错误、converter、pipeline realization、packed-coordinate 和 draw 的首个负 HRESULT 原样返回并停止后续阶段。
- `FlushReset` 在 pipeline realization、outside 收尾、转换或 draw 失败后仍清空所有 accumulated/precomputed geometry，并标记已有 flush。
- `FlushTryGetVertexBuffer` 仅在当前 build 尚未发生 reset flush 时返回当前 builder；`BeginBuilding` 重置几何和 flush 标记以允许复用。
- 本切片不新增 COM ABI、生产导出或设备 Reset/ResetEx。

## 测试与构建

- 新增 11 项定向测试，覆盖空 shape 的完整 outside bounds、顶部/间隙/底部/左右补形、相邻 strata、inside 过滤、混合 primitive 顺序、packed-coordinate 分组、precomputed indexed 转换、pipeline realization 失败、draw 首错、可选返回缓冲区、reset 后复用和重复 `EndBuilding`。
- vertex-builder 定向测试：11/11。
- 全量主测试：3927/3927。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未接入 effects/UCE、生产 ABI、完整 glyph 私有模型、shader bytecode、普通 scene/render-target/present 生命周期、GDI presenter、software-DC present context 或 `Reset/ResetEx`；未虚构原生未证明的 COM 所有权或设备恢复路径。
