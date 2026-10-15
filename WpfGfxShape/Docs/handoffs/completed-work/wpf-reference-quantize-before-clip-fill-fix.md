# 原版校准：旋转fill量化先于裁剪

## 生产修复

SoftwarePathCoverage.TryCreateFigures原先先以surface clip对浮点polygon裁剪，再将新交点量化到28.4。这会改变旋转边斜率，导致可见区扫描覆盖与原版不同。对固定点范围内原顶点保留边并量化，绘制输出仍受消费者clip约束；仅超出当前量化支持范围的顶点保留旧浮点裁剪fallback，避免破坏1e10输入。该fallback不是完整原生饱和/溢出合同关闭。

原生证据：core/sw/swlib/aarasterizer.cpp1370–1465，InitializeEdges接收28.4 pointArray、读取clip bounds，在AA分支对已量化点应用(坐标+8)<<shift，然后进行edge clipping。每次读取≤400行。

## 发布1

完整命令：dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64
目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，工具退出码0，返回59行日志。测试前未独立保存完整发布诊断，不能声明无警告。
产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
SHA256 158F1B548F6613164147991BAB8E84828B6CF893DB884D04A2BA8F47EEBFD82F。

定向校准及最终全量均使用本次发布。实色control180组不一致45降至15：source17旋转fill与无遮罩红填充均一致；剩余source17旋转stroke15组不同。270组尺寸矩阵保持一致，主图像450组仍86不同；直接8位180组仍86不同，与float投影判定差0。

最终全量890通过1失败0跳过，总891，2秒；唯一失败为严格校准测试，既有890项通过。没有放宽或替换HDR断言，没有宣称全量稳定。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，Run ID5627a65e-fc77-41df-8ef4-96087b0a4820。
报告Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-fa19357cb40947839372c750e397cbc8.json。
固定原版样本658eb1f21ad34043beef26e8a72dc567，SHA256 E81C740649274C14C3F24A7053B47B5497FC4C7A8DAA64C7424433926FA7BADE。
原版DLL SHA256501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A。

## 剩余闭环

已修复独立fill覆盖根因，但不是整个生产模块关闭。需继续检查stroke widening/裁剪顺序及中间面源padding/变换采样，联合fill/stroke/mask和生命周期修复，原版全图及发布全量验收后才更新稳定基线。当前发布含已确认fill修复及未关闭86组图像差异，不具备原库替换资格。
