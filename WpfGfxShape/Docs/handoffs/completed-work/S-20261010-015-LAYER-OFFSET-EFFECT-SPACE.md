# Visual mask局部合成表面与effect坐标分离（未关闭）

生产GeneratedImageMask保存LayerBounds，CaptureMaskLayer传递实际layer clip。EndOpacityLayer按该clip与目标交集创建局部合成bitmap，从活动layer像素提取对应行段，回合成恢复left/top设备offset。SoftwareImageDrawingContext新增内部EffectToDevice；SoftwareBitmapMask.Prepare使用独立effect变换，避免颜色surface offset重复应用到已在device空间的mask。普通layer默认保持全目标；活动layer捕获仍是全目标数组，未实现完整局部绘制存储或mask局部表面，不宣布完整layer生命周期关闭。

所有签名均内部，无新增导出；沿用bitmap/lock/effect释放及首错传播。未更改golden/HDR/1e10/Vector4距离<1e-7。原生依据为上一轮drawingcontext.cpp PushLayer及alphaMaskLayer offset证据，本轮未新增原生读取。

两次发布均为`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`，目标WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64，成功、退出码0；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。完整警告未独立保存。

1. 起始1102通过/1失败，总1103；SHA256 4764A0F2E43A22027B870907DCC9F74CB4613351D3072D369937B16E94AE4B1F；报告wpf-calibration-55be85184e7e4bdbad5218ec0fabfb8c.json；TRX身份未提取。
2. 局部回合成初次1102/1；报告wpf-calibration-112278ba4dfe4c9aaff2008157b84853.json。联合869/869、2s、退出码0；最终1102通过/1失败/0跳过，总1103、2s、退出码1；SHA256 199818477A9B903BABD95D9DAC3C26DA4E8C9A37186A6E560DBF0FD6B4DC6F14；TRX 58a42abe-5913-4d49-a67b-1456256aabb9；报告wpf-calibration-7c103bfd3b3040fb8cf83b8afaf9e28c.json。报告及固定com-acceptance.trx在Tests/WpfGfxShape.ComAcceptance/TestResults/；初次及定向TRX身份未提取。

严格差分仍float63/450、直接Pbgra32 9/180、格式判定差54，未减少；固定原版DLL/样本身份不变。本轮推进局部回合成及effect坐标合同，但未完成预定大块闭环。下一轮应合并mask局部表面、完整layer绘制存储/offset、新路径失败生命周期及剩余旋转差分，不以仅复制像素或仅精度修改交付。阶段4 Active，不能替换原库。
