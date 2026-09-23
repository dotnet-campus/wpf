# Vertex builder 与 HW pipeline 生产集成闭环

## 原生证据

- `hwpipelinebuilder.cpp` 的 `CHwPipelineBuilder::SetupVertexBuilder`：先选择 builder，再按 fixed-function/shader item 发送 mapping；pre-generated vertices 向 color source 传空 builder；最后 `FinalizeMappings`，失败时删除 builder。
- `hwpipeline.cpp` 的 fixed-function/shader `InitializeForRendering`、`CHwPipeline::Execute`、`RealizeColorSourcesAndSendState`、`ReleaseExpensiveResources`：保持 builder 创建、outside bounds、geometry generator 保存、懒 realization、empty-fill、缓存重画及释放顺序。
- `hwvertexbuffer.cpp` 的 `Builder::FlushInternal`：首次 flush 内 realization 和 draw，仅未发生内部 reset flush 时返回可复用 vertex buffer。

## 托管实现

- 扩展 `Direct3D9VertexBufferBuilder`：直接承载 texture/constant mapping、`FinalizeMappings`、complex-scan expanded allocations、ordinary triangle/indexed geometry、outside geometry和缓存重画。
- 新增 `Direct3D9VertexPipelineOwner`：明确拥有当前 builder 与可缓存 vertex buffer，支持 converter/builder 创建失败、首次构建、缓存保留、builder 释放及最终销毁。
- 新增 `Direct3D9ProductionPipelineInitializer`：为 shader/fixed-function item 提供强类型 mapping 描述和创建入口；pre-generated geometry 以空 builder 发送 mapping，不扩展生产 ABI。
- 扩展 `Direct3D9Pipeline` 强类型执行路径：保持 `BeginBuilding -> SendGeometry -> EMPTYFILL/outside 判断 -> FlushTryGetVertexBuffer -> builder 释放`；缓存命中只 realization/send state 并重画，不重新生成 geometry。
- bitmap color source 增加强类型 builder mapping；complex-scan 的 direct line-list、顶部 triangle-strip 与 waffle sink 通过同一 builder allocation 进入 expanded、packing、realization 和 draw 流程。
- 旧 `nint` 委托入口仅作为已有 ABI/测试兼容边界保留；新生产闭环不再使用占位 builder 句柄，未新增导出或原生 ABI。

## 所有权与错误语义

- pipeline state 仍在 builder flush 内首次懒 realization；无 geometry 且无 outside bounds 时不 realization、不发送 state、不 draw。
- empty-fill 配合 outside bounds 时继续生成 outside triangle strip 并完成 draw。
- mapping、finalization、realization 和 draw 均传播首个失败 HRESULT；初始化失败释放 builder、owned color source 和 color-source 集合。
- 单一 builder 足以容纳本次 geometry 时保留缓存；第二 pass 只发送 state 并重画缓存；`ReleaseExpensiveResources` 清除 builder、缓存和 color-source 所有权并提供释放后保护。

## 验证

- production pipeline 定向测试：6/6。
- pipeline、vertex builder、complex-scan 联合定向测试：186/186。
- 全量主测试：3933/3933。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未扩展 effects/UCE、生产 ABI、shader bytecode、完整 glyph 私有模型、普通 scene/render-target/present 生命周期、GDI presenter、software-DC present context 或 `Reset/ResetEx`。真实 D3D vertex-buffer 锁定/解锁仍沿用既有设备与 draw 抽象，本切片未虚构新的设备 ABI。
