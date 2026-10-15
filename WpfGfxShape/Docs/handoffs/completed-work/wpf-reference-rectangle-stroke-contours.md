# 原版校准：矩形描边轮廓

原生证据core/geometry/compactshapes.cpp506–603：CRectangle::WidenToShape对simple circular pen生成外/内rectangle figures，Alternate填充；inner退化时省略。miter矩形不等价于分别量化线段矩形/连接角后的sample union。

SoftwareStrokeWidening.TryCreate加入无dash、closed四角轴对齐矩形、非smooth、miter且limit>=sqrt(2)的轮廓路径。先计算outer/inner，变换后以Alternate扫描；inner退化时仅outer。其他join、dash、非矩形继续原路径。bounds沿用outer累积。无需新增接口或修改ABI。

## 发布1

完整命令：dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64
目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64。退出码0，工具返回59行日志；完整发布诊断未在测试覆盖前独立保存，不声明无警告。
产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
SHA256 D4ADFCE83113262F852FC2DABA0DB966FF1E46429DBBF41A2FEFAEBFA37C7BD8。

实色control180/180全部一致，修复上一轮15组旋转stroke差异。主图像450组仍86不同；直接Pbgra32180组同样86，逐fixture格式判定差0；原270组尺寸矩阵仍一致。不能因此宣称图像刷子合同关闭。
最终全量890通过1失败0跳过，总891，2秒；失败为严格原版校准测试，既有890项通过。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，Run ID d653c4b5-278b-4c2c-af24-6ec90b7ead50。
报告Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-6fb0a120a55649d4be96015eac25f4f2.json，对应本发布1。
固定原版输入SHA256 E81C740649274C14C3F24A7053B47B5497FC4C7A8DAA64C7424433926FA7BADE，原版DLL SHA256 501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A。

下一完整闭环聚焦剩余86组DrawingImage intermediate源映射/采样：从原生realization到software bitmap sampler核对模式、透明padding、pixel-centre固定点与AA输出；同轮联合fill/stroke/mask生命周期修复并发布全量验收。实色control一致只排除当前fixture可见目标覆盖差异，不证明所有几何或层覆盖合同一致。HDR与<1e-7断言未放宽。当前不是稳定通过基线，也不能替换原库。
