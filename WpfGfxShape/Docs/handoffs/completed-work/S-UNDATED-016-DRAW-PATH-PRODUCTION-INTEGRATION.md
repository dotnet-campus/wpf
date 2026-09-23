# DrawPath fill/stroke 强类型生产集成闭环

## 原生证据

- `hwsurfrt.cpp` 的 `CHwSurfaceRenderTarget::DrawPathInternal`：进入 device/use context 并检查 render target；fill 分支先 `EnsureRealization`，再取 tight bounds 并调用 `FillPath`；stroke 分支同样先 realization，再取得 scratch widen shape、执行 `WidenToShape`、取 widened tight bounds，并以空 shape-to-device 调用 `FillPath`。
- 同方法在 fill 首错时直接进入 cleanup，不继续 stroke；`IgnoreNoRenderHRESULTs` 只归一化原生 no-render HRESULT。
- `DrawPath` 仅把 `WorldToDevice` 作为 `ShapeToDevice` 传给 `DrawPathInternal`；显示 render target 仍由外层完成 drawing/valid-contents 状态。

## 托管实现

- 新增 `Direct3D9ProductionPathDrawOperations`，集中保存 path 生产闭环所需的 realization、bounds、widen、safe clip、realized brush/effects、state、software fallback、guideline/brush clip、hardware brush、AA/aliased geometry generator、compositing/AA mode 和 current clip。
- 新增 `Direct3D9SurfaceRenderTarget.ProductionDrawPath`。该入口复用现有 `DrawPathInternal` 的 fill/stroke、device/use-context、invalid target、no-render 归一化和 display completion 逻辑，但内部固定把 `FillPath` 的 hardware 回调接到 `ProductionFillPathWithBrush` 与 `ProductionAcceleratedFillPath`，调用者不再传入 `Direct3D9FillPathWithBrush` 绕过生产 pipeline。
- fill 路径保持原始 shape-to-device；stroke 先 widen，并由既有 `DrawPathInternal` 以空 shape-to-device 发送 widened geometry，避免重复变换。
- hardware `E_NOTIMPL` 仍由既有 `FillPath` 进入 software fallback；其他 hardware brush、geometry、pipeline、mapping、realization 和 draw 失败保持首错并阻止后续 stroke。

## 所有权与错误语义

- fill realization、bounds、clip、brush/effects、state 和 production fill 全部成功后才进入 stroke。
- stroke realization 发生在 widen 之前；widened shape 使用 render-target bounds，并以空 transform 进入 production fill。
- hardware brush、geometry generator、pipeline color sources、builder/cache 继续由上一轮强类型 path pipeline 确定性释放；software fallback 只在 `NotImplementedHResult` 时执行。
- rendering disabled 或 invalid render target 继续在既有 `DrawPathInternal` 边界短路，不创建生产 brush、geometry 或 pipeline。

## 验证

- 新增 production DrawPath 定向测试 3 个：fill+stroke 原生顺序与 widened transform、fill 失败短路、hardware `E_NOTIMPL` software fallback。
- path production 联合定向测试：7/7。
- 全量主测试：3940/3940。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未修改或新增生产 ABI/导出；shape、pen、brush realizer、effects 仍停留在已有托管委托边界。未扩展 UCE/effects 协议、完整 glyph 模型、shader bytecode、layer/mask、scene 生命周期、GDI presenter、software-DC present context 或 `Reset/ResetEx`。