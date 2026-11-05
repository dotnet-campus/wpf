# Glyph draw 窄生产集成闭环

## 原生证据

- `hwsurfrt.cpp` 的 `CHwSurfaceRenderTarget::DrawGlyphs`：进入 device/use context，计算 target ClearType 支持，检查 render target 与 device text 能力；POW2/NPOT 和 source clip/border-color 条件可禁止 hardware text。
- 仅准备 hardware text 时 realization brush；空 brush 直接成功。bitmap brush 的 source clip 若不是完整 source，则 realization 后仍转 software。
- `EnsureState` 必须位于 brush realization 之后、software fallback 之前；hardware 使用局部 `CD3DGlyphRunPainter`，`DEVICECANNOTRENDERTEXT` 或 `E_NOTIMPL` 才进入 `SoftwareDrawGlyphs`。
- software 路径重新按 software cache realization brush，取得 effect alpha、software fallback 与 glyph painter memory，再绘制 glyph run。

## 托管实现

- 新增 `Direct3D9ProductionGlyphRenderer`，强类型拥有一次 hardware glyph painter/renderer 的 `Paint` 与确定性释放生命周期。
- 新增 `Direct3D9ProductionGlyphDrawOperations`，集中保存 hardware brush realization、state、hardware renderer factory、software renderer 与 empty glyph-run 状态。
- 新增 `Direct3D9SurfaceRenderTarget.ProductionDrawGlyphs`，复用既有 `DrawGlyphsCore` 的 target/device 能力、POW2/NPOT、source clip、brush realization、state、fallback HRESULT、software brush alpha、device/use-context 和 display completion 语义。
- hardware renderer 创建成功后以 `using` 包围 paint；paint 返回 `E_NOTIMPL`/device-cannot-render-text 时，renderer 先释放，再进入既有 software brush realization/fallback/draw 链。
- empty glyph run 在创建 brush、renderer、shader/color source 前直接成功，并仍通过 display completion 维护 target contents。

## 所有权与错误语义

- hardware renderer factory 失败时释放可能返回的部分 renderer；返回成功但空 renderer 映射为 internal error。
- hardware renderer 无论 paint 成功或失败均确定性释放；software fallback 只接受原生允许的两个 HRESULT。
- software realization 无 brush 时成功且不取得 fallback、不 draw；effect alpha 原样传入 software glyph renderer。
- 其他 realization、state、renderer 创建或 paint 失败保持首错并经既有 no-render 归一化。

## 验证

- 新增 production glyph 定向测试 4 个：hardware paint/释放、hardware 不可用的软件 brush alpha fallback、`E_NOTIMPL` 时先释放 renderer 再 fallback、empty glyph run 短路。
- production glyph 定向测试：4/4。
- 全量主测试：3947/3947。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未新增 production ABI/导出，未推测完整 glyph run、glyph bank、painter memory 或 texture atlas 私有布局；现有私有对象继续由委托边界提供。未新增或伪造 shader bytecode，text pixel shaders 继续使用已有资源加载与设备生命周期。