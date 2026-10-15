# 原版校准：普通位图source box边界修复

## 输入与根因

普通BitmapSource fixture显式SetResolution(96,96)并GetResolution/GetSize断言96DPI、1×1，排除源DPI输入不一致；定向仍150组不同。无旋转source17/None/LowQuality原版在y1红64/191、y2透明，AOT原输出y1/y2均191/255/64。生产AddImageMask sourceBox仅交viewbox/viewport，没有与实际位图像素边界相交，Extend使源外区域也呈现红色。

将物理像素sourceBox相交[0,width]×[0,height]，DrawingPixelToDevice分支仍按真实drawing surface覆写，未改变DrawingImage中间面含透明padding语义。原生证据imagebrush.cpp480–529依据实际GetBitmapSourceBounds计算viewport空间内容边界。没有修改断言或回填AOT值。

## 发布1

完整命令：dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64
目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；工具退出码0，60行日志，完整发布诊断未在测试覆盖前独立保存，不声明无警告。
产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
SHA256 AAE36637EC03C3E29121E2A4ECB345502E891D6A9E577E34C7B22634F4FC7501。

定向及最终全量：普通BitmapSource180组不一致150降至73，107一致。DrawingImage450组仍86不同，直接Pbgra32180组同样86，实色control180组全部一致。既有890项通过，严格校准测试1失败，全量890通过1失败0跳过总891，2秒。不是新稳定基线。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，Run IDdc50616d-bfef-4067-a755-19d4db7a35ad。
报告Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-fec5ff7f27a24f47bf5014c16e48d0fb.json，对应发布1。
固定原版输入SHA256 A672207FF5EF1FCCE4CD4727116F0694D14DB38CA7016D7E13174721DBA560C5，原版DLL501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A。

下一闭环继续定位剩余普通BitmapSource73与DrawingImage86差异，核对源裁剪、tiling中间面及fixed-point过滤邻域；联合fill/stroke/mask生命周期发布验收。完整方法族仍未关闭，HDR和1e-7断言保持，不能替换原库。
