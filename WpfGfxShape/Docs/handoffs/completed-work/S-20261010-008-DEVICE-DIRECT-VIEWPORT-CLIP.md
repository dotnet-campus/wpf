# DeviceAligned直接消费与独立viewport裁剪

## 生产方法族差集

DeviceAligned DrawingImage允许直接brush消费，保留snapshot surface→device映射，不再额外全目标栅格化。不是仅开放开关：NativeImmediateSolidRealization自有BitmapBrush尾部由320扩至376字节，保存独立ExactClipMapping；原生prefix字段/外部对象不变，只在自有vtable身份下读取尾部。CreateBitmap记录viewport rectangle与brush→device裁剪变换，采样映射与几何裁剪映射独立。BitmapRenderTargetExports.FillInternalBitmapBrush消费独立viewport coverage，在目标边界内裁剪后才转换定点，避免1e10坐标溢出；结合形状coverage而不是以surface矩形替代viewport。RealizeBrush使用该合同，按scaling选择Border/Extend，保留float intermediate与目标格式生产span。

所有权沿用CreateBitmap AddRef、失败finally释放source/native allocation及realization owner释放；没有新增COM接口/导出。Direct3D9SoftwareImageRenderer.TargetBounds仅内部属性。既有decoder/锁冲突/重入测试回归，不宣称全部新故障注入已闭合。

原生证据只来自src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/DeviceAlignedIntermediateRealizer.cpp 110–171，viewport clip参与render bounds与surface逆映射；每次读取少于400行。

## 诊断与修复

发布2独立viewport polygon走SoftwarePathCoverage但未先按目标裁剪，source3/7/11/22等超大bounds触发E_INVALIDARG，校准在present阶段终止，不能报告其差分或SHA256。发布3改用SoftwareImageCoverage.TryCreate的有界裁剪并组合实际形状coverage，6项新增回归消除。

12项新增真实COM完整表面回归补齐旋转DeviceAligned三种过滤与fill/stroke/mask交叉组合，分别验证float/Pbgra32 viewbox扩展不变性。

## 三次发布证据

每次命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`。
项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release，win-x64；三次均成功、退出码0。共同产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll；发布日志被测试覆盖，完整警告未独立保存，不声明零警告。

|发布|全量结果|SHA256|TRX/报告|
|---|---|---|---|
|1 起始|1080/1，总1081|9FEFFC30AFEB02F821EA3CDEA584B406CBBD466805612B4C470CDE309C988608|TRX fbaccf3b-2c49-452a-a556-43674c53d7c7；wpf-calibration-8a4ecab87a87443ab96b0a575984360f.json|
|2 独立裁剪初版|1074/7，总1081|未取得，校准present失败且无报告|TRX身份未提取，固定路径已覆盖；E_INVALIDARG见上|
|3 有界viewport coverage|最终1092/1，总1093|670F9281EE446208A8BA935B83418EA038A8FB3E8D5D669394747472D3B20E39|TRX 4fabba27-a9cd-444f-947f-a2daec1ac10b；wpf-calibration-7cf7653de4154780aae1cec332b594a6.json|

发布3首次全量1080/1，TRX cc4165b4-2292-4708-9026-462655c6b78a，报告wpf-calibration-1361d20b5a684928b990818d93b4a069.json。随后联合826/826通过；新增12项测试不修改生产DLL，最终联合838/838通过、2s、退出码0，最终全量1092通过/1失败/0跳过、2s、退出码1。定向TRX身份未提取。所有报告及固定com-acceptance.trx位于Tests/WpfGfxShape.ComAcceptance/TestResults/。

## 校准改善与剩余

float投影66→63/450、直接Pbgra32 42→36/180；格式判定差24→27，不以8位改善证明HDR等价。BitmapSource与实色对照保持一致，最终报告核验固定原版DLL/样本SHA256不变。保持HDR、1e10、Vector4距离<1e-7。

DeviceAligned独立viewport clip直接消费路径已有生产进展及回归，完整旋转intermediate仍未关闭。下一轮合并ViewportAligned旋转最终span及剩余source17/18同合同差分、mask实际作用域、新路径失败重试与释放生命周期；不重复仅开关或仅精度修正。阶段4 Active，不能替换原库。
