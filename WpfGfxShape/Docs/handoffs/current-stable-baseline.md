# 当前稳定基线

> 本文只保存恢复工作所需的稳定摘要与最新一份验证基线；更新验证时替换整节，不追加历史。
> 新完成切片历史见 [`completed-work/`](completed-work/)；旧完成归档 [`progress-completed-work.md`](progress-completed-work.md) 和 HW render-target 历史 [`progress-hw-rendertarget.md`](progress-hw-rendertarget.md) 均保留为冻结历史。

## 当前状态

- 当前位于 [`../remaining-gap-closure-plan.md`](../remaining-gap-closure-plan.md) 的阶段 4：冻结并实现 generated protocol；阶段 1 的 HW backend 生命周期和直接设备边界、阶段 2 的 HW 内容管线、阶段 3 的最小软件呈现链均已正式关闭。
- DirectX 绑定固定为 Silk.NET 2.23.0；Win32 优先 Microsoft.Windows.CsWin32 0.3.298。
- D3D9/HW 内容管线与最小软件 HWND/GDI 呈现链已形成生产闭环，但 generated protocol、生产 ABI、UCE/资源协议和 PresentationCore E2E 尚未完成。
- 当前不能替换原 `wpfgfx_cor3.dll`。
- 当前阶段状态为：正式关闭阶段 3 个、Active 阶段 1 个、Pending 阶段 3 个；阶段 4 当前 Active。由于机器 Ledger 仍为 `RepairRequiredPartial` 且生产完成分母不可确定计算，不维护整体完成百分比。阶段完成条件与剩余大项见 [`../remaining-gap-closure-plan.md`](../remaining-gap-closure-plan.md)。

## 稳定能力摘要

- loader、display/device manager、device/resource/use-context、HW render state、surface/texture/swap-chain、registry/display 已形成大量闭环。
- HW display/window/surface render target、软件 3D fallback、3D geometry renderer、shader/fixed-function pipeline、bitmap color source/cache、software rasterizer/render target、glyph/text 窄路径已有可运行实现。
- `CSwRenderTargetSurface` 的 pipeline、intermediate buffers、alpha、SetSurface、current clip 与 Clear/32bpp 格式分派已形成闭环，并已接通 compatible DC、top-down DIB、dirty-region BitBlt 与 HWND 生命周期。
- 直接设备边界已覆盖大量 capability、资源创建/更新、状态设置、绘制、render-target、GPU marker query、错误映射与释放后保护。
- generated protocol 已冻结完整 command/resource ID、SDK fingerprint、32 位 handle，并形成 transport/channel 五类核心命令、九类基础值资源、六类 2D transform、六类 2D geometry、七类 brush、七类 dash/pen/drawing、`GuidelineSet`/`BitmapCache`、Visual/Target 核心状态、代表性 render-data 指令流以及 Visual3D/Viewport3DVisual 状态命令的 data/factory/update/notification/duplicate/delete 生产闭环。
- 3D rotation/camera/model/light/geometry/material/Transform3D 已有强类型状态、factory/update 和引用生命周期；动画 line/rectangle/image/video、rounded rectangle、ellipse 与 PushOpacityAnimate 已有 record 解析。render-data 保留 nullable slots 和独立数据快照；不能把协议状态验收等同于完整绘制执行。
- DrawingImage 已作为 ImageBrush/opacity mask source 接通延迟中间面；source 分辨率使用 viewbox/viewport/stretch 及设备矩阵基向量长度，大内容缩小、小内容放大与基础 tile/flip 像素已有验收。
- 详细完成事实统一查询独立完成归档，不在本文复制历史验证或旧下一动作。

## 最近验证基线

- 最新复验：S-20261009-232（文档整理，未修改生产代码）；生产能力继承 [`S-20261009-231`](completed-work/S-20261009-231-DRAWINGIMAGE-DEVICE-RESOLUTION.md)。本轮发布明细见 [`文档整理归档`](completed-work/S-20261009-232-STABLE-BASELINE-DOCUMENT-CLEANUP.md)。
- 发布：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，成功。
- 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- 全量 COM 验收：686/686 通过，0 失败、0 跳过，耗时 2s。
- SHA256：`CD2BCD99F8E0EE4AB75B6C8798D149BEFA05508A8B2EF5BC6AD919ADA26AC801`（本轮测试实际加载产物，与上一生产切片相同）。
- TRX：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`；Run ID：`50515a34-a07e-463f-a406-7690810007c5`。该固定路径后续测试会覆盖，历史以各切片归档为准。
- 未关闭：目标 clip 裁剪、绝对 viewbox 子区域及跨 tile/flip 接缝；不能宣称完整原生 intermediate 直译或模块关闭。阶段 4 Active，不能替换原库。
