# DrawingImage 浮点中间面的直接消费者接线与验收

## 范围与原生证据

读取 src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/ViewportAlignedIntermediateRealizer.cpp 的 Realize、CalculateIdealSurfaceSpaceBaseTile 和 CalculateSurfaceSizeAndMapping1D（100–190、420–515、677–762、810–895）。原生中间面返回 IWGXBitmapSource 与 surface→world 映射；tile 的内容栅格化与最终消费是两个不同阶段，不能随目标格式变化丢失已捕获映射。

生产项目 net10.0、Nullable enable，未修改 TFM、ABI、原 WPF 源码或方案。

## 本轮生产修改

Direct3D9SoftwareImageRenderer.ImageMask.cs：Pbgra32 目标为 DrawingImage 选择浮点中间面时，继续传递 directConsumer。此前只传 consumer，导致即使已有 snapshot grid 与映射，也额外物化全目标面；现在复用现有直接路径及 MIL bitmap 接口查询。

既有路径保持 QueryInterface 首错传播，realization 持有 source 引用，临时 MIL 接口在 finally 释放；未新增拥有者、缓存、锁或 ABI。已有 snapshot 精确 mapping/source clip、tile AddRef 复用和 QI 修复在本轮起始产物中已存在，不冒充本轮新增。

将上轮未验证的周期测试拆为 float/Pbgra32 两个方法，每个仅断言实际输出格式的完整表面；删除空格式断言和无意义 Concat/Take。source26/27、三种 scaling、fill/stroke/mask 共18组均通过。此为周期性质验收，不是独立原版 HDR golden。

## 发布记录

两次完整命令均为：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`。

目标 WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj；Release；win-x64。两次产物路径均为 WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 起始发布成功，退出码0；工具报告完整日志11行，发布日志随后被测试覆盖，未单独提取逐条诊断，不能称零警告。测试914通过/1失败/0跳过，总915，2s。SHA256 D962CD921CE6F15AEEEBA95B91B7ECC32FABEE9FC10CB2AEF8C5E25C15F8CA44；TRX Run ID 9cf7b899-fd1b-486d-ba45-3e1e31f42d07；报告 wpf-calibration-be0ce560b2f3401e96f938dcf5353854.json。
2. 生产修复后发布成功，退出码0；32行日志已读取，包含CS8600/CS8602/CS8604、CA1822既有警告，命令抑制CA1416，没有声称警告根因已修复。SHA256 7B3A432F0D3CBA81AF507FAE334D9DB6A27ADEF286C4A3CEE2E6BCE6CF0D126F。第一次全量914通过/1失败/0跳过，TRX Run ID fd4cf297-2d27-44cd-b0c9-70eef93f32c8；报告 wpf-calibration-b034285249e245ffa89139a2c276263c.json。

## 最终验收

项目：WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，直接加载发布2的 AOT DLL。

- 定向筛选 FullyQualifiedName~WhenCroppedDrawingTileMovesByOnePeriod|FullyQualifiedName~WhenDecoderReadsDuringRenderThenReentrantLifecyclePreservesActivePixels：263通过/0失败/0跳过，136ms。仅证明已有fixture覆盖，不声称补齐所有新的tile失败矩阵。
- 最终全量914通过/1失败/0跳过，总915，2s。
- TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx；最终 Run ID 3ee89c99-8332-4777-b84b-c19281b22c5d（固定文件会覆盖）。
- 最终报告：Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-f76e075ea97d484da529278196af11c8.json；SHA256与发布2相同。
- 固定原版DLL SHA256 501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A；样本SHA256 98D740A895372A31DAF4EB234F3942E31A8C99A15D278F121DA84C36B585CE58，均未改。
- float投影 DrawingImage 66/450不同（未改善）；直接Pbgra32 66/180→42/180不同，格式判定差0→24。不得将这些格式差异解释为HDR精度关闭。BitmapSource及实色对照各180组全一致。
- source17无旋转3组、旋转37组不同；source18无旋转0组、旋转26组不同；source22–27均0组不同。

## 未关闭与后续合并

本轮是实际消费者路径修复及历史未验收测试收口，不是整个tile模块完成。严格原版校准仍失败，完整ViewportAligned/DeviceAligned方法族未关闭。已确认的风险边界是float/Pbgra32消费与插值格式不同且mask scaling合同仍不能默认等价；不能从8位匹配反推HDR、放宽1e-7或回填golden。

下一轮按current-work-item.md合并原生尺寸/裁剪/tile shift/双向mapping、最终span及消费者生命周期，而不是仅做测试或文档。阶段1–3Closed、4Active、5–7Pending；Ledger RepairRequiredPartial，不能替换原库。
