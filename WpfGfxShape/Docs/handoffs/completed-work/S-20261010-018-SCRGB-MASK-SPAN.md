# scRGB mask span生成与effects消费（未关闭）

SoftwareBitmapMask.GenerateSpan按整段起终点FLOAT矩阵变换计算增量并累计，预生成nearest/bilinear alpha数组，覆盖为零的颜色像素不改变mask坐标推进。SoftwareBitmapEffects.GenerateSpan与Direct3D9SoftwareImageRenderer.BlendSpan生产接线；缓存仅对当前span坐标有效，Prepare先失效缓存、失败不发布新像素，Dispose释放数组及COM source。字节mask保持整数IPC路径。

原生证据仅src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/swlib/bilinearspan.cpp 4134–4184、4669–4719：scRGB span起终点/累计及MaskAlpha调用颜色源scan op。每次少于400行。保持恒定HDR插值合同，未修改golden/精度或COM ABI。

两次发布均为`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`，目标WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64；成功、退出码0，产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll；完整警告未独立保存。

1. 起始1109/1，总1110；SHA256 A4DFC48BC611CED6504ECE3524DF3030FD131664A4C856A162CA47CF56F6AFA5；TRX c3609d2d-9f63-4fdf-9256-9a73ca6d7996；报告wpf-calibration-02a9ddb39402487189bce20919e78a8d.json。
2. mask span接线首次1109/1，报告wpf-calibration-621bcd316fac4b9c953db970af6d54ee.json；联合876/876、1s、退出码0；最终1109通过/1失败/0跳过，总1110、2s、退出码1；SHA256 314DB17A33886E210E26E18D65286E9FA4FEFF91A2E444C058512068F6D3C746；TRX dd463aeb-bc02-4e06-887f-e926918256c5；报告wpf-calibration-fee01591626d411187581ed17dd1f41f.json。初次及定向TRX未提取；报告及固定com-acceptance.trx在Tests/WpfGfxShape.ComAcceptance/TestResults/。

差分仍float63/450、直接Pbgra32 9/180、格式判定差54，没有减少。固定原版DLL/样本身份保持；HDR、1e10、Vector4距离<1e-7未放宽。本轮未满足大块闭环，既有876项仅作回归；没有新增span故障注入专门用例，不能宣称全部失败生命周期关闭。

下一轮应合并实际剩余9项mask可比合同与float旋转覆盖/采样根因、span边界及失败重试端到端验证，不再连续精度微调；阶段4 Active，不能替换原库。
