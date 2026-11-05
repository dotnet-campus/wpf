# S-20261009-240：DrawingImage source 生命周期保护与嵌套消费者验收

## 交付范围

本轮起始先发布、再执行唯一 COM 项目全量测试，746/746通过。确认工作区已有上一轮透明 padding 修复：DrawingSnapshot 的采样范围由 viewbox/viewport 确定，content bounds 只用于判空；设备裁剪按原网格裁整数 texel 并保留双线性邻域。该修复不是本轮新增代码，本轮重新发布验证并补齐遗漏的完整 hash 证据。

本轮新增生产修改：GeneratedImageMask 维护 disposed 状态，RealizeDrawingSource 对释放后调用及无可实现 source 返回 WgxInvalidCall，而不是返回成功后让消费者解引用空 COM 指针；Dispose 幂等、先断开拥有字段，再逆序释放 tiles、drawing 和 source。DrawingSnapshot 自身已有释放后保护，本轮未改动它。

**原批准的完整 source 采样模块未关闭。** 不能将生命周期组合通过或新增测试数等同于原生 intermediate 方法族翻译完成。

## 原生及生产证据

- 原生：`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/ViewportAlignedIntermediateRealizer.cpp:400–490`，sample clip 反变换到 world、与可见范围相交，再到 scaled viewport 的空间关系。
- 本轮读取每次不超过400行，未修改原 WPF 源码。
- 生产：`GeneratedVisualRenderer.DrawingSnapshot.TryCapture/Realize/Dispose`、`GeneratedImageMask.TryCapture/RealizeDrawingSource/Dispose`、`Direct3D9SoftwareImageRenderer.ImageMask.AddImageMask/AddTiledImageMask/RealizeBrush`。
- Capture 在 target COM 回调前保留 source 快照；Realize 首错返回并 AbortLayers；临时 session、bitmap lock、bitmap 和 COM source 保持 finally 清理。RealizeBrush 的 candidate realizer 在失败路径释放，成功转移给调用者。
- 未改变 Stdcall ABI、公共导出、资源协议、TFM、SDK 或依赖版本。
- disposed mask 没有公共导出可单独注入，本轮该 guard 为源码核对；不得声称通过反射或内部调用验收。公共生产链路的关闭/重入/失败清理有直接 AOT COM 用例。

## 测试修改与诊断

- ActiveRender fixture 新增 scalingMode 参数，以真实 visual options 报文传入生产路径。
- 新增10项 DrawingImage brush source 模式1/2/3生命周期组合：回调替换、channel/connection关闭、重入、目标外零解码、fill首错阻断与释放锁后的失败重试，含 opacity-mask 消费者。
- source 为嵌套 ImageDrawing 的红/绿位图，并非均匀色块；这些用例验收模式传播下的稳定像素和所有权，**不证明模式之间的过滤差异**。
- 增加 PublishedArtifactTests，将生产 DLL 完整 SHA256 写入 TRX；不创建第二测试项目或 Host，不引用生产托管程序集。
- 任意角旋转试验曾添加5个用例（0.8/0.6旋转矩阵，sourceMode17，tile模式0–4），定向173通过、5失败。原因是沿用了未旋转的固定像素预期，旋转后反算的source坐标和tile归属改变，未构成可信生产Red。试验的5个DataRow和矩阵分支均已撤回，未按实际输出回填预期，也未以此修改生产采样算法。下一轮需独立计算正确预期。
- 保留已有1e10、HDR、低alpha及Vector4误差小于1e-7断言。没有放宽既有精度断言。

## 本轮每次发布及对应证据

共同完整命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。

共同目标：`WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj`；配置 Release；RID win-x64。

共同产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

1. 起始发布：成功，发布工具退出成功，日志11行；起始全量746/746通过、0失败、0跳过、1s，Run ID `5326e681-ff22-40ac-9d48-ffe2b8b8d44a`。生产尚未修改时追加hash验收1/1通过，Run ID `2d27ab78-a946-43d3-aa15-ba3d08a84068`，SHA256 `D25530AE223872C3B6876C1441661BDE3722B9D09181B8B2FEAD807CD9561908`。其固定TRX随后已覆盖。
2. 修改后发布：成功，发布工具退出成功，日志60行；随后定向391/391通过、0失败、0跳过、1s；最终全量757/757通过、0失败、0跳过、2s。SHA256 `765FC91E90466F78537815C8744E3D1170A38A88480683756504DF3EBE44B686`；最终Run ID `74a6a737-fe6a-4828-852a-c6d7bb406790`。

唯一测试项目：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`，直接加载生产 Native AOT DLL。

TRX：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。固定路径覆盖，本文保留两次发布对应的hash与Run ID。测试编译存在仓库既有MSTest分析器警告；本轮不处理无关告警，成功不等于零告警。

## 剩余边界与下一轮

没有确认的外部技术阻塞。任意旋转与跨viewport/tile/flip需要非均匀内容的独立数学像素oracle；嵌套位图需要实际缩放导致的模式差异；过滤邻域与裁剪前后相位一致仍需完整生产验收。下一轮继续合并同一模块的映射、邻域、真实消费者和失败生命周期，不再安排仅补参数的独立切片。

完整intermediate原生直译、曲线成本/拓扑、MSVC ABI、层栈/UCE及PresentationCore E2E未关闭。阶段1–3Closed、阶段4Active、阶段5–7Pending；Ledger RepairRequiredPartial，不能替换原wpfgfx_cor3.dll，不报告整体完成百分比。
