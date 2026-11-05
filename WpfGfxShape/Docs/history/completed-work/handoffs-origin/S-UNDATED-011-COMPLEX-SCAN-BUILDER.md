# AddComplexScan 完整生产闭环

## 原生证据

- `CHwRasterizer::GenerateOutputAndClearCoverage` 把 pixel-space Y 和带 `INT_MAX` 尾哨兵的 coverage interval 链传入 `CHwTVertexBuffer<TVertex>::Builder::AddComplexScan`。
- `AddComplexScan` 首先调用 `PrepareStratum(y, y + 1, false)`；失败时立即返回，不申请或输出顶点。
- 有有效 waffler 时 line pipeline 优先；无有效 waffler且 `pixelCenterY < viewportTop + 1` 时走 vertex-buffer line sink；其余路径先按过滤后的 segment 数一次性申请 `segmentCount * 2` 个 line-list 顶点，再连续写入。
- interval 输出保持 `NeedCoverageGeometry` 过滤、coverage/64、X/Y `+0.5f` pixel center、outside bounds 固定 min/max 顺序与首个失败 HRESULT 原样传播。
- 顶部行和 waffle 拆分后的 line sink 使用 `AddLineAsTriangleStrip`：每线申请六顶点，X 回退 `0.5f`，Y 上下各偏移 `0.5f`，顺序为重复 TL、TL、BL、TR、BR、重复 BR，以退化三角连接一像素高 quad。

## 托管实现

- 新增 `Direct3D9ComplexScanBuilder`，把现有 interval 转换、line path 选择和 waffle pipeline 组合为统一的 `AddComplexScan` 生产组件。
- 新增 `Direct3D9ComplexScanVertex`、stratum preparation delegate 和显式 line-list/triangle-strip allocation delegate，使批量申请、连续写入与失败边界可独立接入后续 vertex buffer。
- `Direct3D9ComplexScanIntervalBuilder` 新增过滤后 segment 预计数，并与实际输出共享尾哨兵及 coverage 过滤规则。
- direct line-list 路径只申请一次；空扫描和全部过滤扫描不申请顶点。顶部严格边界与 waffle 路径逐线申请六顶点 triangle strip。
- 当前 `PrepareStratum` 接口覆盖 `AddComplexScan` 可达的 outside 前置调用和 HRESULT 顺序；完整 outside stratum 状态机仍留给后续独立切片。

## ABI、HRESULT 与所有权结论

- 本切片为纯托管 HW 内容管线组件，不新增生产 ABI、COM 调用或引用计数。
- interval、texture-coordinate 配置和分配器由调用方拥有；builder 只保存配置值和 delegate。
- `PrepareStratum`、批量 line-list 申请、triangle-strip 申请及 waffle 下游返回的首个负 HRESULT 均原样返回，并停止后续输出。
- 分配器必须至少返回请求数量的可写顶点；违反该内部契约时抛出 `InvalidOperationException`，不静默越界或部分写入。

## 测试与构建

- 新增 `Direct3D9ComplexScanBuilderTests` 10 项，覆盖成功与多 interval、空扫描、inside/outside 过滤、outside bounds 原生裁剪顺序、顶部 triangle strip、严格顶部边界、waffle 拆分、line-list 批量申请失败、triangle-strip 首错和 `PrepareStratum` 首错。
- complex-scan 定向测试：22/22。
- 全量主测试：3904/3904。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未实现通用 expanded-vertex 映射、`EndBuilding`/`FlushInternal`、完整 outside stratum 状态机、effects/UCE、glyph 私有模型、shader bytecode、GDI/software-DC、生产 ABI、普通 scene/render-target/present 生命周期或 `Reset/ResetEx`。
