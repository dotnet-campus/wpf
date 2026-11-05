# AddComplexScan line 路径选择翻译

## 原生证据

- 原生声明位于 `core/hw/HwVertexBuffer.h`，实现位于 `core/hw/hwvertexbuffer.cpp`，生产调用者由 HW rasterizer 通过 geometry sink 调用 `AddComplexScan`。
- `AddComplexScan` 先调用 `BuildWafflePipeline`。只有 `fWafflersUsed` 为真时才保留 waffle line sink；仅请求 waffling 但所有矩阵列均因最小宽度限制被过滤时，等同于没有 waffler。
- 没有有效 waffler 且扫描线中心满足 `rPixelY < GetViewportTop() + 1` 时，改用 vertex-buffer line sink。该 sink 的 `AddLine` 会把顶部 viewport 行转换为 triangle strip，规避 D3D9 顶部半像素裁剪。
- 没有有效 waffler且不在顶部行时，原生预先批量申请 line-list vertices，并直接写入端点和 coverage。
- 有效 waffle pipeline 优先于顶部行 fallback；waffler 的最终 sink 仍是 vertex buffer，由其按拆分后线段位置决定 line-list 或 triangle-strip。
- 任一 sink 的 HRESULT 立即返回并停止后续扫描段。
- `fWafflersUsed` 不直接控制 `FlushInternal` 或 `m_fHasFlushed`；其局部作用仅是避免把“构建调用发生但没有有效 waffler”误当作 sink 路径。expanded vertex 在 `EndBuilding` 中仍按 builder 的整体 `AreWaffling()` 状态执行 packed-coordinate 转换。

## 托管实现

- 新增 `Direct3D9ComplexScanLineBuilder`，把现有 `Direct3D9WafflePipelineBuilder.BuildLine` 接入独立的 complex-scan line 消费组件。
- 新增 `Direct3D9ComplexScanLinePath`，明确区分 `DirectLineList`、`VertexBufferSink` 与 `WafflePipeline`。
- 路径选择保持原生优先级：有效 waffler 优先；否则使用严格小于 `viewportTop + 1` 的顶部行 fallback；其余走直接 line-list sink。
- 组件公开 `WafflersUsed`，确保密度过滤后的空 pipeline 不会错误屏蔽顶部行 fallback。
- 当前仅翻译 line sink 选择和 HRESULT 转发，不扩展 coverage interval 遍历、outside bounds 或完整 vertex-buffer 存储。

## ABI、HRESULT 与所有权结论

- 本切片为纯托管路径组装，不调用 COM，不涉及 Windows `Stdcall` 或引用计数。
- texture-coordinate 配置仅在构造时用于建立 pipeline；组件持有调用方提供的 sink delegate。
- 所选 sink 或 waffle 链返回的 HRESULT 原样传播，首错停止。

## 测试与构建

- 新增 `Direct3D9ComplexScanLineBuilderTests` 6 项，覆盖普通 direct line-list、顶部行 fallback、严格边界、全列密度过滤、waffler 优先级和 sink HRESULT 传播。
- 定向测试：6/6。
- 全量主测试：3888/3888。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未实现完整 `CCoverageInterval` 遍历、outside geometry、vertex allocation/flush、expanded vertex、effects/UCE、glyph、shader bytecode、普通 scene/render-target/present、GDI/software-DC、生产 ABI 或 `Reset/ResetEx`。
