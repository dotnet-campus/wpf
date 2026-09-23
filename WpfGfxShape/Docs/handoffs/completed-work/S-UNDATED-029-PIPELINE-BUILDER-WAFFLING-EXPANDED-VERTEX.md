# S-UNDATED-029：Pipeline Builder、Waffling 与 Expanded Vertex 生产消费链

## 完成切片

本轮关闭阶段 2 最后一个计划项：shader/fixed-function pipeline construction、原生 operation ordering、typed vertex mapping、line/triangle waffling、expanded vertex conversion、vertex-buffer flush、Direct3D draw 和昂贵资源释放的生产闭环。

## 原生证据范围

- `core/hw/hwpipelinebuilder.cpp`、`HwPipelineBuilder.h`：`SendPipelineOperations` 按 primary color source、effects、geometry modifiers、lighting、clip 的顺序构造管线；`SetupVertexBuilder` 在 item construction 后选择并配置 vertex builder。
- `core/hw/hwpipeline.cpp`：fixed-function 与 shader rendering 初始化在 builder setup 后取得 vertex builder、设置 outside bounds，并保留 geometry generator 用于资源 realization 与 draw。
- `core/hw/hwvertexbuffer.cpp`、`HwVertexBuffer.h`：generated vertex attribute mapping、line/triangle waffling、expanded conversion、packing、outside geometry、flush 与 draw 的调用顺序。
- `core/hw/waffler.cpp`、`Waffler.h`：line/triangle 按 tile boundary 拆分、插值、数值异常和首错传播。

## 托管实现差集及修改

- 新增 `Direct3D9ProductionPipelineBuilder` 与强类型 build context，shader/fixed-function 共用同一生产入口，并通过 `Direct3D9PipelineOperationSender` 保持原生 operation ordering 和首错停止。
- 将 shader/fixed-function item builder、`Direct3D9ProductionPipelineInitializer`、typed vertex owner、waffler、expanded converter、vertex-buffer builder 与 production draw 连接为单一闭环。
- 为 constant color 与 constant alpha color source 补齐 typed builder constant mapping；bitmap texture mapping 继续使用同一 color-source device state 与 waffling 配置。
- 新增 `Direct3D9ProductionVertexDraw`，把 expanded batch 映射到 D3D9 FVF，并提交 TriangleList、TriangleStrip、LineList 与 IndexedTriangleList。
- 为 `Direct3D9Device` 增加强类型 indexed-UP draw 边界，保持 stream/index 解绑、HRESULT 映射、frame metrics 与释放后保护。
- `Direct3D9VertexBufferBuilder` 的 waffle coordinate 集合改为具体数组，保持 analyzer 与热路径类型明确。

## ABI、HRESULT、所有权与释放顺序

- 未增加或修改生产 DLL 导出、COM vtable 或外部 ABI。
- 所有 builder、mapping、conversion、flush 与 draw 继续使用 HRESULT 首错传播；失败后不执行后续 operation 或 draw。
- pipeline-owned shader/fixed-function color source、初始化失败资源和昂贵资源统一按创建顺序的逆序释放；borrowed color source 不被重复释放。
- Direct3D draw 保持 Windows D3D9 边界、现有 device error mapping、device-lost 传播和 disposed guard。

## 验证结果

- pipeline/waffling/expanded vertex 定向与相关回归：43/43 通过。
- 全量主测试：3999/3999 通过。
- ABI 集成测试：8/8 通过。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

- 未新增 `Reset/ResetEx`、UCE/resources、生产 ABI、完整 glyph 私有模型或普通 render-target 生命周期。
- 未实现 software-DC present context、compatible DC/DIB、GDI presenter、ScrollBlt 或 software dirty 通知；这些能力转入阶段 3，并继续受原生可达性证据约束。
