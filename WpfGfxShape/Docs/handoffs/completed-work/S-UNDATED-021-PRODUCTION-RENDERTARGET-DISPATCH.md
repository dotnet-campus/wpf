# Production render-target dispatch 闭环

## 原生与现有托管证据

- `CHwTextureRenderTarget` 的各绘制入口转发到内部 surface render target；texture target 自身负责内容失效和 bitmap 输出缓存一致性。
- 托管 `Direct3D9TextureRenderTarget` 已对旧 `Func<int>` bitmap/path/glyph/video/mesh 入口统一执行 `InvalidateContentsAndDraw`，但此前没有对应强类型 production 路由。
- `Direct3D9SurfaceRenderTarget` 已分别形成 `ProductionDrawBitmap`、`ProductionDrawPath`、`ProductionDrawGlyphs`、`ProductionDrawVideo`、`ProductionDrawMesh3D`，各自拥有 display completion、fallback、HRESULT 和资源生命周期。

## 托管实现

- `Direct3D9TextureRenderTarget` 新增强类型 production dispatch：bitmap、path、glyph、video 和 mesh 全部直接转发到内部 surface target 的对应 production 入口。
- 新增通用 `InvalidateContentsAndDraw(Func<Direct3D9SurfaceRenderTarget, int>)`，在转发前统一标记 texture contents 无效并失效已缓存 bitmap；旧 delegate 入口复用该实现。
- dispatch 层不重新拼装 shader/fixed-function、geometry、brush、fallback 或所有权，仅负责 texture contents 状态与 surface production 路由。
- display surface target 继续由各 production 入口自身维护 `CompleteDisplayDrawing`；texture wrapper 不重复执行 display 状态变更。
- 旧 `Func<int>`/最终 draw delegate 入口完整保留，作为已有测试与兼容边界。

## 所有权与错误语义

- texture target 在调用任何 production 依赖前执行 disposed 检查；disposed 时不失效 contents、不调用底层 operations。
- production 结果原样返回，不吞掉 no-render、fallback 或 draw HRESULT。
- glyph empty、video empty frame、path 无 brush等短路继续由 surface production 入口决定；dispatch 不创建额外资源。
- texture bitmap cache 仅失效一次，后续 `GetBitmap` 仍使用既有 revalidate 逻辑。

## 验证

- 新增 dispatch 定向测试 2 个：五类 production primitive 的 texture 路由/短路行为，以及 disposed 后依赖不调用。
- production dispatch 定向测试：2/2。
- 全量主测试：3956/3956。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未新增 production ABI、COM interface 或导出；未修改 surface/texture 原生身份和 QueryInterface 契约。旧委托入口继续存在，ABI Host/测试无需变更；未扩展 UCE、effects、scene、presenter 或 shader bytecode。