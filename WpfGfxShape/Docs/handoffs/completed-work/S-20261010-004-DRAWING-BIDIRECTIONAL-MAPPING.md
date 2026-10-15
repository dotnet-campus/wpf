# DrawingImage sample-scaled 双向映射推进（未关闭）

## 生产修改与原生证据

GeneratedVisualRenderer.DrawingSnapshot 正常网格不再通过 content→surface 的逆矩阵乘 content→device 构造返回映射。正向从 content→viewport→sample-scaled viewport→surface 组合，返回从 surface→sample-scaled viewport→viewport→device 组合，保留整数crop、cap和tile shift。超大局部网格保留原有双精度有界路径；未改变COM所有权、首错HRESULT或失败逆序释放。

原生证据只读取 src/Microsoft.DotNet.Wpf/src/WpfGfx/，每次不超过400行：resources/ViewportAlignedIntermediateRealizer.cpp 185–243、465–514、1090–1170、1174–1239；common/BaseMatrix.cpp 804–895。证据覆盖surface逆映射、tile shift、content正向映射、surface返回映射及尺度分解。

试验强制FLOAT最终矩阵组合同时影响ViewportAligned返回及DeviceAligned正向，导致30项新增严格float精度回归（示例source19旋转期望0.95、实际0.95000017），已撤回；不能以放宽1e-7或修改golden接受该候选。当前仍使用既有double消费者变换组合。没有证明仅改矩阵精度可消除严格原版差分。

## 四次发布证据

每次完整命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`。
目标项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64；四次均成功、退出码0。共同产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。发布日志被后续测试覆盖，完整警告未独立保存，不声明零警告。

|发布|结果|SHA256|TRX Run ID|校准报告|
|---|---|---|---|---|
|1 起始|1080通过/1失败|45ED41EF3E35057BAF981951D603265054666056BE64B49B5FC86A66B5F33C95|37209729-435d-4619-918b-2bcd7ce22c7d|wpf-calibration-55fc56b7cfc24d96926e2dd6c9da8640.json|
|2 双向映射初版|1080通过/1失败|A977F9A1A75FDD8FDD55324AB557CD28B71F1264018EFF099EF5601BFAC81046|ba3b4993-4435-4d89-9923-3c586627fdd6|wpf-calibration-c4f37413a2664e80a354222253411763.json|
|3 FLOAT最终组合候选|1050通过/31失败|D21C6A76B2BD23D819EBC26978CBC687C35D2D418A257E1F6BC9CF852FB2401C|0f76aeec-4cfc-4730-a10e-4a8203f2ec29|wpf-calibration-e4461d4a17f84404b57ccd651fa7ed88.json|
|4 撤回回归候选、保留双向映射及清理|1080通过/1失败|368379390D6550A9D7E93F990075D57D43BCCE3F0F10784A9A4E08AD1796DC9C|eaa67a1b-27b9-4ad3-888b-43e3bd3b0609|wpf-calibration-d55ec534157848ad95da4faaba592908.json|

所有报告在Tests/WpfGfxShape.ComAcceptance/TestResults/；TRX固定路径com-acceptance.trx会覆盖，表内保留已提取身份。发布4联合DrawingViewbox/CombinedGeometry/Decoder/HdrGradient/CroppedDrawingTile定向成功，筛选与前轮707项相同，但本轮未读取计数和定向TRX身份，不冒称已提取。最终全量总1081、0跳过、2s、测试退出码1。

## 未关闭与后续合并

float投影66/450、直接Pbgra32 42/180、格式判定差24均未减少；唯一失败严格原版校准。固定原版DLL及样本SHA256由报告核验不变，不修改golden或放宽HDR、1e10、Vector4距离<1e-7。

本轮有生产双向映射修改，但旋转intermediate完整闭环仍未关闭：当前理想尺寸及裁剪尚混用内容空间double与sample-scaled FLOAT，独立viewport/world尺度分解及最终目标格式span仍需合并实现/验收。下一轮必须联合这些生产方法及消费者生命周期，不重复强制FLOAT最终组合，不以补测为独立交付。阶段4 Active；不能替换原库。
