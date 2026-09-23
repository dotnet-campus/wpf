# Bitmap draw 强类型生产集成闭环

## 原生证据

- `hwsurfrt.cpp` 的 `CHwSurfaceRenderTarget::DrawBitmap`：进入 device/use context、检查 render target、取得 scratch bitmap brush、选择显式 source rect 或 bitmap bounds、创建 parallelogram、先 `EnsureState`，再用 `CMILBrushBitmapLocalSetterWrapper` 配置 bitmap source/Extend/world-to-device，创建跳过 meta fixups 的 immediate brush realizer，最后通过 `FillPath` 绘制。
- 原生 bitmap shape 与 brush sampling/device transform 均使用 `WorldToDevice`；`FillPath` 自身负责 safe-device clip、brush/effects、hardware pipeline 与 `E_NOTIMPL` software fallback。
- scratch brush 是非拥有借用对象；局部 setter 与 immediate realizer 在退出时恢复/释放临时状态，no-render 与 clipped-to-empty 归一化为成功。

## 托管实现

- 新增 `Direct3D9ProductionBitmapDrawOperations`，集中保存 scratch brush、immediate realizer、shape、clip、software fallback、guideline/brush clip、hardware brush、AA/aliased geometry generator、compositing/AA mode 与 current clip。
- 新增 `Direct3D9SurfaceRenderTarget.ProductionDrawBitmap`。该入口复用既有 `DrawBitmap` 的 device/use-context、invalid target、source rect/default bounds、shape 创建、首次 ensure-state、scratch brush 设置、immediate realizer 和 display completion 生命周期。
- production bitmap fill 固定接入 `ProductionFillPathWithBrush` 与 `ProductionAcceleratedFillPath`，调用者不再传入最终 hardware draw 委托绕过 shader/fixed-function production pipeline。
- bitmap shape-to-device、brush world/base-sampling transform 保持同一 `WorldToDevice`；effects 从 immediate brush realizer 原样进入 hardware pipeline和 software fallback。

## 所有权与错误语义

- shape 创建或首次 state 失败时不设置 scratch brush、不创建 realizer或 pipeline。
- scratch brush 设置后，即使 realizer 创建、hardware brush、geometry、pipeline 或 fallback 失败，也先释放 realizer 中 effects/brush 引用，再清除 scratch brush。
- hardware 只有 `NotImplementedHResult` 进入 software fallback；其他失败保持首错。
- invalid/disabled target 继续在既有 DrawBitmap 边界短路；display target 继续通过 `CompleteDisplayDrawing` 维护 contents 状态。

## 验证

- 新增 production bitmap 定向测试 3 个：hardware production pipeline 完整顺序、hardware `E_NOTIMPL` software fallback 与清理、shape 创建失败短路。
- production bitmap 定向测试：3/3。
- 全量主测试：3943/3943。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未新增 production ABI/导出，未虚构 bitmap UCE resource、effects 协议、interpolation/wrap 的未知私有布局或 shader bytecode。现有 bitmap source、scratch brush、shape 与 color-source 创建仍通过已有托管委托边界进入生产闭环。