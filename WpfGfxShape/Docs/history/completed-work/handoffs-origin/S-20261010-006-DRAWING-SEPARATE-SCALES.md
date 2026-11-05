# DrawingImage 独立尺度及两次FLOAT应用推进（未关闭）

## 生产修改

GeneratedImageMask独立传入brush与world的X/Y尺度，DrawingSnapshot不再先将两组尺度相乘后应用。正常网格理想tile端点及content→grid按brush、world依次进行FLOAT乘法，返回映射按world、brush顺序去尺度。GeneratedImageMask采用MathF.Sqrt(float分量平方和)取得intermediate尺度，NaN按原生归零；超大有界路径及double最终消费者矩阵保留。

原生证据：src/Microsoft.DotNet.Wpf/src/WpfGfx/core/common/BaseMatrix.cpp 756–838，GetScaleDimensions/DecomposeMatrixIntoScaleAndRest。每次读取不超过400行。当前只实现独立尺度计算与应用；没有将完整non-scale rest矩阵及world-space clip转换直译，不能标记完整尺度分解方法关闭。

未改变COM ABI、拥有/借用关系、首错HRESULT、逆序释放、无锁回调或golden。实际目标格式span未修改，本轮不足以满足用户要求的完整生产闭环；明确保持未关闭。

## 三次发布

每次完整命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`。
目标WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release，win-x64；三次成功、退出码0。共同产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。完整警告未独立保存，日志被测试覆盖，不声明零警告。

|发布|结果|SHA256|全量TRX Run ID|校准报告|
|---|---|---|---|---|
|1 起始|1080通过/1失败|EAA79BAEF3B53965C0FB81E4EF2FC012066579F408E58A1F41988D74B0137D60|7bcff27a-437e-4936-829b-11249bc9b3d4|wpf-calibration-844c8ad672604e87addf2fd31f9089c6.json|
|2 两次尺度应用|1080通过/1失败|B2A60D5513517D069C302677B80BA05596A63C53960A38B675093043DC1105E0|40bd8636-780d-41c7-9f50-85c215f0a762|wpf-calibration-6961c5be124945758b3048b6ae195163.json|
|3 sqrtf独立尺度|1080通过/1失败|C23D9C6A3F1E766571C3E6F830D896044ABBEF76EA9325A66274F13F5DC5AA2F|d204c2cf-22b4-4ea3-9465-dfcf15720520|wpf-calibration-8782827cf76143d58c94e8d3f26074a8.json|

发布3联合DrawingViewbox/CombinedGeometry/Decoder/HdrGradient/CroppedDrawingTile：707通过、0失败、0跳过、1s、退出码0；定向TRX身份未提取，后续全量覆盖。全量总1081、0跳过、2s、退出码1。报告及固定com-acceptance.trx均在Tests/WpfGfxShape.ComAcceptance/TestResults/。

严格差分未减少：float66/450、直接Pbgra32 42/180、格式判定差24；BitmapSource各组零差异。固定原版DLL与参考样本SHA256由最终报告核验不变。不放宽HDR、1e10或Vector4距离<1e-7。

## 后续合并闭环

连续映射改动未减少严格差分，下一轮不能继续默认以单项精度修正交付。应合并完整scale/rest矩阵分解、world clip/render bounds变换、surface返回映射及实际目标格式span生产消费，使用source17/18旋转同合同fixture作端到端验收，并合并新增路径失败重试、锁冲突、回调重入与释放。没有已记录的外部技术阻塞，不能把同一闭环缩成仅定位或补测。阶段4 Active，不能替换原库。
