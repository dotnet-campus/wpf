# Expanded-vertex 映射与消费方法族完整闭环

## 原生证据

- 核对 `CHwTVertexBuffer<TVertex>::Builder::SetupConverter`、`FinalizeMappings`、`ExpandVertices`、`ExpandVerticesFast`、`ExpandVerticesGeneral`、`TransferAndOrExpandVerticesInline` 与 `TransferAndExpandVerticesGeneral`。
- `SetupConverter` 以 `mvfOut & ~mvfIn` 计算生成属性；Z、Diffuse、UV1 的组合使用快速表，特殊 XY/XY+Diffuse 到 Z+UV8 也走快速路径，Normal/Specular 生成返回 `E_NOTIMPL`，其余走通用路径。
- `FinalizeMappings` 为未显式映射的生成 Z 设置 `0.5f`，为未映射的生成 Diffuse 设置不透明白色；constant diffuse 使用预转换的 packed premultiplied sRGB 值。
- 转换时 position transform 必须先于 UV 映射；UV 从最终二维位置通过各目标坐标组的矩阵生成。
- coverage 暂存在基础顶点的 Diffuse 位槽中，并按 float 位模式读取；0 和 1 有专用分支，其余按 `round(falloff * 256)` 和原生双 lane packed ARGB 公式缩放 constant/default diffuse。
- `ExpandVertices` 对 triangle-list、non-indexed triangle-list、triangle-strip 和 line-list 使用相同 converter；complex-scan 的 direct line-list、顶部 strip 与 waffle strip 因而消费相同最终映射。

## 托管实现

- 新增 `Direct3D9ExpandedVertexConverter` 与统一 `Direct3D9ExpandedVertex` 存储，覆盖原地扩展、源/目标分离转换和 complex-scan 基础顶点直接写入最终顶点。
- 支持 position transform、默认或显式 Z、constant/default diffuse、coverage-to-diffuse、最多八组累计 UV 映射，并保持顶点顺序及未转换 float/uint 位模式。
- `Create` 保持不支持 Normal/Specular、非累计 UV 及无 XY 格式的 `E_NOTIMPL`；未完成生成 UV 映射时 `FinalizeMappings` 返回 `WGXERR_NOTINITIALIZED`；重复或越界映射返回准确失败 HRESULT。
- 保留原生快速路径选择条件，并提供强制通用转换入口用于等价性验证；两条路径共享单顶点映射核心，避免语义漂移。
- 扩展 `Direct3D9VertexFormatAttribute`，加入独立 XY/Z 和 UV5-UV8 表达。
- `Direct3D9ComplexScanBuilder` 新增最终 expanded-vertex 分配模式。direct line-list 在一次批量分配中连续转换；顶部和 waffle triangle-strip 直接写入最终目标，仅使用六顶点栈上基础输入，不创建堆中间数组。
- 旧基础 `Direct3D9ComplexScanVertex` 分配模式继续保留，既有调用者与测试不受影响。

## HRESULT、位模式与所有权结论

- 本切片为纯托管 HW 内容管线，不新增 COM ABI、引用计数或生产导出。
- converter 与分配器由调用方拥有；complex-scan builder 只保存引用，不负责释放。
- 分配、stratum、waffler 和转换的首个负 HRESULT 原样返回并停止后续输出。
- direct copy 保持 NaN payload、负零和 UV float 位模式；coverage 使用 `BitConverter` 在 float/uint 间按位转换，不做数值重解释之外的额外归一化。
- 输出容量不足返回 `E_INVALIDARG`；分配器违反已申请容量的内部契约仍抛出 `InvalidOperationException`。

## 测试与构建

- 新增 12 项 expanded-vertex 定向测试，覆盖无附加属性复制、position transform、Z/constant diffuse、coverage packed ARGB、单组和多组 UV、快速/通用一致性、空集合、不支持格式、初始化失败，以及 direct multi-interval、顶部行和 waffle 的端到端转换。
- expanded-vertex 与 complex-scan 定向测试：34/34。
- 全量主测试：3916/3916。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未实现完整 outside stratum 状态机、`EndBuilding`/`FlushInternal` 提交与重置闭环、effects/UCE、生产 ABI、完整 glyph 私有模型、shader bytecode、普通 scene/render-target/present 生命周期或 `Reset/ResetEx`。
