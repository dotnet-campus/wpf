# scRGB逐texel wrap分离（完整旋转闭环未关闭）

生产Direct3D9SoftwareImageRenderer.Float不再复用Select64BitFallbackTexels的16.16字节span辅助。nearest按IPC坐标取整数texel，bilinear保留FLOAT分数并独立wrap四个整数texel。WrappedFloatPixel分别处理Border、Extend、Tile、FlipX/Y/XY及负坐标，避免先规范连续坐标再转fixed-point。保留HDR恒定值插值合同和既有IPC累计、coverage、effects、compositing顺序。没有修改COM引用、首错或失败释放。

原生证据仅src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/swlib/bilinearspan.cpp：309–371（整数ApplyWrapMode）、4010–4134（nearest）、4134–4280（scRGB bilinear）；每次读取不超过400行。

新增10项真实COM浮点brush wrap/world-space/opacity/lock-retry回归。原测试float期望只适用Border，新增非Border数据暴露测试辅助的错误预期；按原生Extend/Tile/Flip合同修正为红色texel，不改固定golden或已有Border预期。

## 两次发布

完整命令均为`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`，项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64；两次成功、退出码0。产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll；完整警告未保存，不宣称零警告。

1. 起始1092通过/1失败，总1093；SHA256 670F9281EE446208A8BA935B83418EA038A8FB3E8D5D669394747472D3B20E39；报告wpf-calibration-d3a6c1d247334b968c5da43c77f75a01.json；起始TRX身份未提取。
2. scRGB分离后首次1092/1；SHA256 184CC2CF398EC8717D67F0930718EFBCBA7494B9CF7D14BF2738706C3391B83B；TRX 9edb80e1-5fc0-4b8e-8d0f-5e0058ed04e6；报告wpf-calibration-561c3806e33042eb84f88828d3bc8fe7.json。

新增测试不修改生产DLL；私有brush定向修正后成功，联合DrawingViewbox/CombinedGeometry/Decoder/HdrGradient/CroppedDrawingTile/ImageMaskSourceIsLocked/PrivateBitmapBrush为869/869、2s、退出码0。最终全量1102通过、1失败、0跳过，总1103、2s、退出码1；TRX 72d467e1-0920-4777-9cd4-45fb48e34785；报告wpf-calibration-3455becd1b4c4232b05cff436087f484.json。报告及固定com-acceptance.trx在Tests/WpfGfxShape.ComAcceptance/TestResults/；定向TRX身份未提取。

严格差分仍float63/450、直接Pbgra32 36/180、格式判定差27，未减少。固定原版DLL/样本由报告核验不变，BitmapSource各组零差异。保持HDR、1e10、Vector4距离<1e-7。

本轮有生产采样路径分离，但没有达到完整旋转方法族交付。剩余source17/18旋转span差分、原生nearest rounding边界、实际格式采样作用域及新增路径失败生命周期仍须合并实现，不能以新增测试数量宣称关闭。阶段4 Active，不能替换原库。
