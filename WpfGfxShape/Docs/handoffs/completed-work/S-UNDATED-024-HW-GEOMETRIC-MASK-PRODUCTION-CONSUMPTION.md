# S-UNDATED-024：HW 几何 mask 生产消费闭环

## 完成切片

完整关闭 `CHwSurfaceRenderTarget::EndLayerInternal` 对应的几何 mask HW 生产消费：layer frame 中保留的 mask 不再只能交给裸 handle composite，而是进入强类型 shape、geometry generator、effect、brush 与 accelerated fill 生命周期。

## 原生证据

- `core/hw/hwsurfrt.cpp` 的 `CHwSurfaceRenderTarget::EndLayerInternal`：确认 opaque/alpha target 的 regular/complement compositing mode、AA rasterizer 的 complement bounds/inside、aliased `CShapeBase::Combine(Xor)`、constant alpha 合并以及 alpha target 最终 `SourceUnder` 顺序。
- 同文件 `AcceleratedFillPath`/`ShaderAcceleratedFillPath`：确认 geometry generator、brush、effect context、outside bounds 与 inside 标记进入 shader pipeline。
- 原生清理顺序为每次重新建立 generator 前释放旧 tessellator/rasterizer，并在方法退出时确定性释放当前 generator；外层 layer 生命周期继续拥有 captured source 与 retained mask。

## 托管实现

- 新增 `Direct3D9LayerMaskShape`、`Direct3D9LayerEffectList`、`Direct3D9LayerMaskOperations` 与相关强类型委托，明确 temporary shape、generator、effect 和 brush 的所有权。
- `EndLayer` 在存在 geometric mask 且没有尚未进入本切片的 alpha-mask brush 时直接进入 `CompositeLayerMask`；旧 composite 委托保留为兼容和 alpha-mask 后续消费边界。
- AA 路径创建 antialiased generator，传递 layer bounds 作为 complement bounds；constant alpha 作为 alpha-scale effect 合并，并以 `Alpha < 1` 决定 inside。
- aliased 路径先对 layer bounds 与 mask 执行 XOR combine，再创建 aliased generator；constant alpha 通过 bounds generator 和 `1 - Alpha` effect 独立恢复。
- opaque target 使用 `SourceOverNonPremultiplied`/`SourceInverseAlphaOverNonPremultiplied`；alpha target使用 `SourceInverseAlphaMultiply`/`SourceAlphaMultiply`，最后使用 source bitmap 执行 `SourceUnder`。
- `WGXHR_EMPTYFILL` 在 mask、combined geometry 和 bounds geometry 各阶段归一化为 no-render；真实失败保持首错并停止后续 fill/source-under。
- temporary shape、generator、effect、brush 按内层到外层逆序释放；EndLayer 外层仍负责 frame pop、父状态恢复、captured source 与 retained mask 释放。

## 验证

- 新增 5 个定向用例，覆盖 aliased/AA、inside/complement、constant alpha、opaque/alpha target、empty mask、fill failure、source-under 顺序和资源逆序释放。
- 主测试：3971/3971 通过。
- ABI 测试：8/8 通过。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

- 未实现完整 alpha-mask brush/effect list 协议；同时存在 alpha-mask brush 时仍走既有 composite 边界，留待下一阶段完整 effect 消费闭环。
- 未扩展 UCE、generated protocol、生产 ABI、shader bytecode 或 glyph 私有模型。
