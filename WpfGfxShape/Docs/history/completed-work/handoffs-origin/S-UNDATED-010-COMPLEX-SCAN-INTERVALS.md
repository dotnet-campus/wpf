# AddComplexScan coverage interval 翻译

## 原生证据

- `CCoverageInterval` 定义于 `core/sw/aacoverage.h`，使用带 `INT_MAX` 尾哨兵的链表；当前节点的 `m_nPixelX` 是区间左边界，下一节点的 `m_nPixelX` 是右边界。
- `CHwRasterizer::GenerateOutputAndClearCoverage` 从 coverage buffer 取得首 interval，并把 pixel-space Y 与 interval 链交给 geometry sink 的 `AddComplexScan`。
- `CHwTVertexBuffer<TVertex>::Builder::NeedCoverageGeometry` 在不需要 inside geometry 时过滤 coverage 64，在不需要 outside geometry 时过滤 coverage 0。
- `AddComplexScan` 将 Y 和两个 X 边界分别加 `0.5f` 转为 pixel center，并将 coverage 除以 64 转为 alpha。
- outside geometry 模式使用固定顺序 `begin=max(begin,min(end,left))`、`end=min(end,max(begin,right))` 裁剪。该顺序会保留 bounds 外 interval 对应的零长度线，而不是直接剔除。
- 任一 line sink 返回失败 HRESULT 后立即停止 interval 遍历并原样返回首错。

## 托管实现

- 新增 `Direct3D9CoverageInterval`，保存 interval 左边界和 coverage。
- 新增 `Direct3D9ComplexScanIntervalBuilder`，独立完成尾哨兵遍历、`NeedCoverageGeometry` 等价过滤、coverage/64、X/Y pixel-center 转换及 outside bounds 裁剪。
- 组件通过现有 `Direct3D9AddWaffleLine` 输出 `Direct3D9WafflePoint` 线段，可连接上一切片的 complex-scan line path builder，但本切片不扩展完整 vertex allocation 或 flush。
- 对缺少 `INT_MAX` 尾哨兵以及“无 outside bounds 却禁用 inside geometry”的无效输入执行参数保护。

## ABI、HRESULT 与所有权结论

- 本切片为纯托管 coverage 转换，不新增生产 ABI、COM 调用或引用计数。
- interval 集合和 outside bounds 由调用方拥有；builder 仅保存不可变 bounds 值与 line sink delegate。
- line sink 的负 HRESULT 原样传播并停止后续输出。

## 测试与构建

- 新增 `Direct3D9ComplexScanIntervalBuilderTests` 6 项，覆盖 interval 遍历、pixel center、coverage/64、inside/full coverage 过滤、outside/zero coverage 过滤、无限与不相交 interval 的原生顺序裁剪、HRESULT 首错和尾哨兵校验。
- 定向测试：6/6。
- 全量主测试：3894/3894。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未实现完整 vertex-buffer 批量申请、直接顶点存储、expanded vertex、flush、effects/UCE、glyph、shader bytecode、普通 scene/render-target/present、GDI/software-DC、生产 ABI 或 `Reset/ResetEx`。
