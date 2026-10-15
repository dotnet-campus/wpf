# Float mask span真实COM锁失败与重试验收（未关闭）

新增CompositionEffectsTests.FloatMaskSpan，直接加载生产AOT，在float目标上使用HDR颜色(1.25,-0.25,0.1234567,1)和两texel alpha(0.25,0.75)，宽度6/10。持有mask写锁验证首错0x88982F0D且输出全零；解锁后重试，按独立FLOAT IPC起终点/累计公式核验所有Vector4距离<1e-7。释放source/target/channel/connection。初次清理导出名写错MilChannel_Destroy，已改MilConnection_DestroyChannel；没有以失败结果回填像素。

生产SoftwareBitmapEffects字节constant alpha改为原生half-up 16.16因子，与已实现affine opacity一致。证据仅src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/swlib/bilinearspan.cpp 4286–4339；每次小于400行。HDR仍float，固定golden不变。

参考WpfReference/README明确RenderTargetBitmap仅Pbgra32，剩余57项是跨格式float投影，缺少真正原版float读回证据，不能从字节推导float golden；严格失败继续保留。

两次发布命令均为`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`，项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64；成功、退出码0，产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。完整警告未独立保存。

1. 起始1109/1，总1110；SHA256 8E635FDDC04000B13BDB450B67976C576079E084F2392E8E847CF3FCE2AF4424；报告wpf-calibration-8f1de121a73d4ccbbe30ac2701f75350.json；TRX未提取。
2. constant alpha后float mask专门2项通过，联合885/885、1s、退出码0；最终1111通过/1失败/0跳过，总1112、2s、退出码1；SHA256 CCD2C01F350C9E8BA1F3D3DFA2F0E945BFE9BD9C3D5E39AFCE40602162197D1D；TRX 7e94f9d2-b418-4f55-8c02-220684e4b714；报告wpf-calibration-7da2635054154f1b845b1354b9d4dd19.json。报告及固定com-acceptance.trx在Tests/WpfGfxShape.ComAcceptance/TestResults/；定向TRX未提取。

float投影57/450、直接0/180、格式判定差57保持。该float源mask span已有专门失败/重试证据，但不等同旋转DrawingImage方法族、新路径所有故障或HDR原版等价关闭。本轮仍不足完整生产大块目标。下一轮合并真正原版float读回可达契约与旋转DrawingImage端到端验收、失败生命周期；不能把跨格式投影失败强行修成字节一致。阶段4 Active，不能替换原库。
