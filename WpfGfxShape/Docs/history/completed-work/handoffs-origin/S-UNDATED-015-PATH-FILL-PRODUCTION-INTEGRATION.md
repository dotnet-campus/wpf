# Path fill 强类型生产集成闭环

## 原生证据

- `hwsurfrt.cpp` 的 `CHwSurfaceRenderTarget::FillPath`：依次执行 guideline/brush clip、设备空间 bounds 与当前 clip 求交、hardware brush 派生、AA rasterizer 或 aliased tessellator 创建，再进入 `AcceleratedFillPath`；`WGXHR_EMPTYFILL` 归一化为成功，结束时释放 brush 与 geometry generator。
- `hwsurfrt.cpp` 的 `CHwSurfaceRenderTarget::AcceleratedFillPath`、`ShaderAcceleratedFillPath`：shader pipeline 初始化并执行，退出时无条件 `ReleaseExpensiveResources`。
- `hwpipeline.cpp`、`hwpipelinebuilder.cpp`：geometry、outside bounds、color-source mapping、懒 realization、draw 与昂贵资源释放由同一 pipeline 生命周期承载。

## 托管实现

- `Direct3D9PathGeometryGenerator` 不再保存占位 `nint` handle，只拥有强类型 `Direct3D9SendGeometryToVertexBuilder` 与确定性释放回调；ordinary triangle geometry 与 complex-scan 均直接写入同一 `Direct3D9VertexBufferBuilder`。
- `Direct3D9PathHardwareBrush` 不再暴露占位 handle，改为拥有 shader/fixed-function pipeline 创建入口和释放回调，调用者不再向 accelerated-fill 外部拼装两个 pipeline delegate。
- `Direct3D9ProductionPipelineInitializer` 增加强类型 path geometry 初始化入口；旧 `nint` 初始化路径继续保留为既有兼容边界。
- `Direct3D9Pipeline` 的强类型执行状态不再依赖非零 geometry handle，仍保持 builder 创建、mapping、geometry 发送、empty-fill/outside、flush、draw、缓存与释放语义。
- `Direct3D9SurfaceRenderTarget.ProductionAcceleratedFillPath` 直接从 hardware brush 取得 shader pipeline，只有 `E_NOTIMPL` 时进入 fixed-function fallback；每次尝试均在退出时释放 pipeline 昂贵资源。
- `ProductionFillPathWithBrush` 保持 guideline/brush clip、bounds、AA/aliased generator 选择、empty-fill 成功归一化以及 brush 后 generator 的既有 finally 释放顺序。

## 所有权与错误语义

- shader 优先，只有 `NotImplementedHResult` 触发 fixed-function fallback；其他失败保持首错返回。
- mapping、finalization、geometry、realization、state 和 draw 失败均沿 production pipeline 原样传播。
- pipeline 的 color sources、owned color sources、builder/cache 在 `ReleaseExpensiveResources` 中确定性释放；path 调用结束后再释放 hardware brush 和 geometry generator，不把其生命周期泄漏到 render target。
- 空 geometry 且无 outside bounds 时不 realization、不发送 state、不 draw；geometry generator 返回 `EmptyFillHResult` 时 path fill 归一化为成功。

## 验证

- path fill 与 production pipeline 联合定向测试：10/10。
- 全量主测试：3937/3937。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未新增生产导出或原生 ABI；未虚构 path 私有原生布局、effects/UCE、shader bytecode、完整 glyph 模型、scene/layer 生命周期、GDI presenter、software-DC present context 或 `Reset/ResetEx`。现有 `DrawPathInternal` 的 shape、pen、brush realizer 边界仍维持既有委托形式，留待后续按原生调用顺序接入本次强类型 fill 闭环。