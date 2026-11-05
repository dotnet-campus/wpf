# DrawingImage 理想tile、裁剪与双向映射统一网格（未关闭）

## 生产修改

GeneratedVisualRenderer.DrawingSnapshot 正常尺寸的理想tile端点、整数尺寸、可见bounds裁剪、crop与flip shift统一到sample-scaled viewport空间，与正向/返回映射共用网格。内容Bounds仅在裁剪后换回内容空间，不再参与正常网格整数尺寸计算。返回映射在sample-scaled空间减shift，再去除viewport尺度。CropTileAxis按FLOAT scale、offset、逐步multiply/add及半像素触及范围计算。超大局部网格保留double有界路径；没有恢复强制FLOAT最终矩阵组合。

原生证据：src/Microsoft.DotNet.Wpf/src/WpfGfx/core/common/BaseMatrix.cpp 756–788，GetScaleDimensions采用sqrtf；相关ViewportAligned方法证据见上一归档。本轮未完成独立viewport/world尺度分解，不能宣称原生方法族完整直译或旋转等价关闭。

未修改COM ABI、拥有/借用关系、首错HRESULT、失败逆序释放、无锁回调及golden。没有新公开API或新增测试；既有真实COM验收用于回归，不替代新路径故障注入。

## 三次发布与验收

每次完整命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`。
目标项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64；三次均成功、退出码0。共同产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll；原始日志被测试覆盖，完整警告未独立保存，不声明零警告。

|发布|修改/全量|SHA256|TRX Run ID|报告|
|---|---|---|---|---|
|1|起始1080/1|368379390D6550A9D7E93F990075D57D43BCCE3F0F10784A9A4E08AD1796DC9C|56bafb33-a525-46d6-adeb-fdb210ce87db|wpf-calibration-f03a6011ecb0432d914daa86c8af8b5f.json|
|2|统一网格1080/1|6F2F8D5F14DCE1A96FD01563068DC445D61413BA6F61D2C43F92329245FA4AFB|a791a8f7-c2a9-4cef-8b84-4187955224aa|wpf-calibration-3ad3975ec26940a78a2c8cd2ca60258f.json|
|3|FLOAT像素触及范围1080/1|EAA79BAEF3B53965C0FB81E4EF2FC012066579F408E58A1F41988D74B0137D60|22c41dee-cba2-40a1-88d8-d09c7f859043|wpf-calibration-08fa0ea6c3c3448fb824c2c4abd098da.json|

发布3联合DrawingViewbox/CombinedGeometry/Decoder/HdrGradient/CroppedDrawingTile：707通过、0失败、0跳过、1s，退出码0；定向TRX身份未提取，后续全量覆盖。最终全量1080通过、1失败、0跳过，总1081、2s、退出码1。报告及固定com-acceptance.trx在Tests/WpfGfxShape.ComAcceptance/TestResults/。

严格原版差分仍float66/450、直接Pbgra32 42/180、格式判定差24。报告核验固定原版DLL与样本SHA256不变，BitmapSource各组零差异。不能以本轮回归通过宣称差分根因已修复。

## 后续合并边界

本轮推进正常网格统一，完整旋转intermediate未关闭。独立尺度分解、两次矩阵尺度应用的FLOAT顺序、实际目标格式span、Visual mask作用域、新路径失败重试与释放仍须联合关闭。没有原生证据缺失的外部阻塞，不将下一轮缩为补测或重复发布。保持HDR、1e10、Vector4距离<1e-7；阶段4 Active，不能替换原库。
