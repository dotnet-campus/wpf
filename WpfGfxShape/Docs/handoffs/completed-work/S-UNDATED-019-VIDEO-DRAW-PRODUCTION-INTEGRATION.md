# Video draw bitmap production 集成闭环

## 原生证据

- `hwsurfrt.cpp` 的 `CHwSurfaceRenderTarget::DrawVideo`：进入 device/use context、检查 render target；surface renderer 分支调用 device `DrawVideoToSurface`，其最后动作是 `BeginRender`，成功后必须在 cleanup 调用 `EndRender`。
- 无 surface renderer 时对调用方 bitmap source `SetInterface`；两条分支都取得临时 bitmap source 引用。空 source 表示音频等无视频帧场景，直接成功。
- 绘制前暂时关闭 `PrefilterEnable`，调用同一 render target 的 `DrawBitmap`；cleanup 中忽略 `EndRender` 失败、释放 bitmap source 并恢复 prefilter，保留主绘制首错。

## 托管实现

- 新增 `Direct3D9ProductionVideoDrawOperations`，聚合 bitmap draw state、effects 与已完成的 `Direct3D9ProductionBitmapDrawOperations`。
- 新增 `Direct3D9SurfaceRenderTarget.ProductionDrawVideo`，复用既有 `DrawVideo` 的 device/use-context、invalid/disabled target、surface `BeginRender/EndRender`、bitmap source AddRef/Release、空帧、prefilter 保存恢复和 display completion 生命周期。
- video 的 bitmap 分支固定调用 `ProductionDrawBitmap`，因此继续进入 scratch bitmap brush、shape、safe clip、path geometry、shader/fixed-function production pipeline 和 software fallback，不再由调用者传入最终 bitmap draw 委托。
- surface renderer 取得的 frame 与调用方提供的 bitmap source 均通过同一 production bitmap pipeline 消费，effects、source rect 和 world-to-device 由 operations 原样传播。

## 所有权与错误语义

- `BeginRender`/device surface draw 失败时不标记 EndRender、不进入 bitmap pipeline；prefilter 仍恢复。
- surface begin 成功后，无论 bitmap pipeline 成功或失败都调用 EndRender；EndRender HRESULT 按原生 `IGNORE_HR` 不覆盖主结果。
- current bitmap source 在 EndRender 后释放；调用方 source 分支先 AddRef 后统一释放。
- 空 frame 成功且不修改 production bitmap resources；invalid/disabled target 在 source 引用和 renderer 调用前短路。

## 验证

- 新增 production video 定向测试 3 个：调用方 bitmap source 的完整 production pipeline 与 prefilter、surface begin 成功后 bitmap 失败仍 EndRender、surface begin 失败短路。
- production video 定向测试：3/3。
- 全量主测试：3950/3950。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未新增 production ABI/导出，未虚构 media/UCE、video presenter、surface renderer 或 frame 私有布局。surface renderer 与 bitmap source 继续由已有 ABI 委托/COM 指针边界提供；未实现 GDI presenter、software-DC present context 或普通 scene 生命周期。