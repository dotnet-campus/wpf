# S-UNDATED-028：Bitmap、Path、Glyph、Video 生产绘制链闭环

## 完成切片

完整核验并正式关闭阶段 2 的 bitmap、path、glyph、video 生产绘制链。四类图元均从 surface/texture render-target production 入口进入既有 brush/effect、typed pipeline、geometry/vertex、Direct3D draw、软件 fallback、display completion 与确定性释放顺序；未另建平行渲染抽象。

## 原生证据

- `core/hw/hwsurfrt.cpp` 的 `CHwSurfaceRenderTarget::DrawBitmap`：进入 device/use context，取得 scratch bitmap brush，计算 source rect，创建 bitmap shape，EnsureState 后经 FillPath 绘制。
- `core/hw/hwsurfrt.cpp` 的 `DrawPathInternal`：fill 先于 stroke；每路先 realization，stroke 先 widen，再进入 FillPath；首错停止后续路径。
- `core/hw/hwsurfrt.cpp` 的 `DrawGlyphs` 与 `d3dglyphpainter.cpp`：按设备文字能力、brush tiling/source clip 与 ClearType 条件选择 HW painter，仅对原生 unsupported 类结果进入软件 fallback。
- `core/hw/hwsurfrt.cpp` 的 `DrawVideo` 与 `d3ddevice.cpp` 的 `DrawVideoToSurface`：surface renderer 的 BeginRender 成功后必须 EndRender；视频临时关闭 prefilter，经 DrawBitmap 消费 bitmap source，最后释放 source 并恢复状态。
- `core/hw/hwtexturert.cpp`：四类图元均转发到 surface render target，texture target 在调用前使内容状态失效。

## 托管闭环

- Bitmap：`ProductionDrawBitmap` 已覆盖 scratch brush 获取/绑定/清除、source rect 或 source size、shape 创建、immediate brush realization、effect retain、safe clip、guideline/brush clip、AA/aliased geometry generator、shader 后 fixed-function pipeline、vertex conversion、D3D draw、unsupported software fill 与临时资源逆序释放。
- Path：`ProductionDrawPath` 已覆盖 fill/stroke 原生顺序、brush realization、tight bounds、stroke widen、safe clip、AA/aliased generator、typed hardware brush/effect、shader/fixed-function fallback、software fallback、empty/no-render 归一化与首错停止。
- Glyph：强类型 `Direct3D9GlyphRun` 路径已覆盖输入验证、target bounds、ClearType/gray 与 subpixel bank key、device/display bank miss/hit/LRU、realization、短生命周期 painter、真实 paint、unsupported software fallback、device invalidation 和释放；旧 renderer delegate 仅保留为外部适配兼容面，不再作为强类型 glyph run 核心模型。
- Video：`ProductionDrawVideo` 已覆盖 supplied bitmap source 和 surface-renderer source 两条入口、device consumer、Begin/End 配对、COM AddRef/Release、prefilter 临时关闭/恢复、复用完整 bitmap production pipeline、首错保持和 display completion。
- Texture render target 已为 bitmap/path、两种 glyph production 入口和 video 提供内容失效后转发，并保持释放后保护。

## HRESULT、所有权与边界

- 四类入口均保持 device/use-context 作用域、无效 render target 的 no-render 行为以及 display completion 顺序。
- shader/fixed-function 只在原生支持边界内切换；真实失败不伪装为 unsupported，首个失败阻止后续创建或绘制。
- scratch brush、effect、geometry generator、hardware brush、pipeline color source、glyph painter、video bitmap source 均按实际拥有关系释放；partial creation 失败释放已创建对象。
- bitmap source、shape、pen、brush realizer 和外部 effect 的裸 COM identity 仍属于待冻结的 generated protocol/UCE/生产 ABI 适配边界；本轮未虚构其协议对象，但这些 identity 进入 production 入口后不再绕过核心绘制链。

## 验证

- Production 定向回归：51/51 通过，覆盖四类成功路径、shader/fixed-function、unsupported software fallback、empty/no-render、真实失败、partial creation、释放顺序和 texture dispatch。
- 全量主测试：3994/3994 通过。
- ABI 测试：8/8 通过。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

- 未实现 generated protocol、UCE resource/command、生产 ABI 或 PresentationCore E2E。
- 未引入新的 glyph 私有 ABI、GDI presenter、software-DC present context、兼容 DC/DIB、`Reset/ResetEx` 或普通 scene 生命周期。
- 未修改原生 WPF/wpfgfx 源码。