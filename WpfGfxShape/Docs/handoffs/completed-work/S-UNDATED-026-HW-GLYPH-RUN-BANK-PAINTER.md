# S-UNDATED-026：HW glyph run、bank 与 painter 生产生命周期

## 完成切片

完整关闭当前阶段可由原生证据支持的强类型 glyph run、device/display glyph bank 与短生命周期 painter 生产闭环。原有只携带 `HasGlyphs` 并由调用者直接创建最终 renderer 的入口保留为兼容薄边界；新的 production 入口从 run 数据开始完成验证、cache key、realization、paint 与软件 fallback。

## 原生证据

- `InternalRT.h` 的 `DrawGlyphsParameters`：生产绘制携带 context、brush context、`CGlyphRunResource`、brush realizer 与 device bounds。
- `glyphrunslave.h/.cpp`：glyph run realization 使用 font face、em size、glyph indices、advances、offsets、scale、text rendering/hinting 与 display settings；推荐 blend mode覆盖 grayscale/ClearType。
- `hwsurfrt.cpp`：先检查 HW text/brush capability，仅在需要时 realization brush；EnsureState 位于 brush realization 之后；HW 返回 device-cannot-render-text/NotImplemented 时进入 software fallback，真实失败不 fallback。
- `d3dglyphpainter.cpp`：painter 为短生命周期；先 bounds/clip、Init、ValidateGlyphRun、ValidateGeometry；empty realization no-render；ClearType 由推荐模式与 target capability共同决定；析构释放 painter 持有的 HW color source。
- `ValidateGlyphRun` 按 device cache index 查找 realization；miss 创建并缓存，hit 标记 persistent。
- `d3dglyphbank.cpp`：bank 属于 device，不 AddRef device；管理 persistent/temporary glyph storage，GC/设备销毁释放相关资源。

## 托管实现

- 新增强类型 `Direct3D9GlyphRun`，包含 cache key、font face、em size、indices、advances、offsets、glyph-to-device transform、bounds 与 subpixel 标记。
- run 在进入生产路径前验证长度一致、finite 数值、有效 font/em size/transform/bounds；empty run 和 clipped-out bounds 直接 no-render。
- 新增 `Direct3D9GlyphBankKey`，纳入 device identity、display index、run key、grayscale/ClearType、subpixel positioning 和 X/Y scale。
- 新增 `Direct3D9GlyphBank`，覆盖 miss 创建、hit 更新并标记 persistent、LRU eviction、device invalidation、partial creation failure 释放及 Dispose 全释放。
- 新增 `Direct3D9GlyphRealization` 与 `Direct3D9GlyphRunPainter`；bank 拥有 realization，painter 只借用 realization并拥有自身短生命周期资源。
- `Direct3D9SurfaceRenderTarget.ProductionDrawGlyphs` 新增强类型 run/operations overload，直接复用现有原生顺序的 HW eligibility、brush realization、EnsureState、unsupported software fallback 和 display completion。
- ClearType 仅在推荐模式与 target capability 同时成立时使用，否则 key 与 painter 统一降为 grayscale；subpixel 标记进入 cache key。
- `Direct3D9TextureRenderTarget` 新增强类型 glyph production 转发，并保持绘制前 texture/cache invalidation。

## HRESULT 与所有权

- invalid run/key/scale 返回 InvalidArgument；empty/clipped/empty realization 返回成功 no-render。
- HW realization/paint 的真实失败保持首错；只有 NotImplemented/device-cannot-render-text 进入软件 fallback。
- creation failure 即使返回 partial realization 也立即释放且不缓存；eviction、device invalidation 与 bank Dispose 确定性释放 realization。
- painter 总在 paint 返回后释放；paint 真实失败不会误触发 fallback。

## 验证

- 新增 8 个定向测试，覆盖 cache miss/hit/persistent、ClearType/grayscale、subpixel key、LRU eviction、device invalidation、empty/invalid/clipped run、unsupported fallback、paint failure、partial creation 和释放顺序。
- 主测试：3984/3984 通过。
- ABI 测试：8/8 通过。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

- 未实现 UCE/generated glyph command、完整 DWrite font 私有协议或真实 glyph bitmap 上传格式；realization 创建仍由具备原生字体/bitmap 证据的 production adapter 提供。
- 未扩展 shader bytecode、生产 ABI 或 PresentationCore E2E。
