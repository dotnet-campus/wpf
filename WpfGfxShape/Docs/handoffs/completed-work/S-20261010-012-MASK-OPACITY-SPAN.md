# Mask realization直接opacity span（旋转闭环未关闭）

生产AddImageMask/AddTiledImageMask移除为brush opacity创建的临时Begin/EndOpacityLayer及identity再采样，改为实际mask采样context.AlphaScale。保留真实scene layer行为、target兼容格式、viewport clip、分区float累加。字节affine AlphaScale采用原生16.16 half-up因子及+32768整数缩放，替代每通道MathF.Round；float仍直接乘法。COM首错与finally逆序释放保持，无新增公开API。

原生证据仅src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/swlib/bilinearspan.cpp 4669–4719、4280–4359（mask采样后缩放及ConstantAlpha整数公式），每次少于400行。

三次发布完整命令均为`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`；目标WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64；均成功、退出码0。产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll；完整警告未独立保存。

1. 起始1102/1，总1103；SHA256 F7F7B7E77DD69262445FBDAD622B127EB72D14A76955B33547A884D10C165310；报告wpf-calibration-f087f20f489444cc9eddc83b2ba63ee1.json；TRX身份未提取。
2. 移除临时layer后1102/1；SHA256 7BDD3EAE1B9D770EB21489B9F89700C472E2F61ADC7893E9B8059B15F933D13C；TRX 4bd5917a-9c64-408e-8b02-ca163ba06748；报告wpf-calibration-80f03dd17d3e42fb94886ac3f0e31e76.json。
3. 原生整数opacity后联合869/869、1s、退出码0；全量1102通过/1失败/0跳过，总1103、2s、退出码1；SHA256 F18C12E09EAFF2C1D346624CA90EDFFECFCA8C20B7BB5D2AED0F66E4A2E17C80；TRX 08f2b88a-32ba-4ada-b340-1fb8b20d30be；报告wpf-calibration-502e89c7b42f4706b2ec94296e86e6e0.json。报告及固定com-acceptance.trx在Tests/WpfGfxShape.ComAcceptance/TestResults/；定向身份未提取。

严格差分仍float63/450、直接Pbgra32 9/180，格式判定差54；golden身份未改，HDR/1e10/Vector4距离<1e-7未放宽。该修改消除生产额外分配/重采样，但未解决剩余严格差分，不能宣称完整旋转方法族关闭。无外部技术阻塞；下一轮合并实际Visual mask scope、mask最终采样及float旋转覆盖/采样和失败生命周期，不默认重复精度微调。阶段4 Active，不能替换原库。
