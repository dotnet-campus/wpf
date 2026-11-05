# DeviceAligned viewport几何裁剪与消费者统一

生产FillInternalBitmapBrush对正常可表示viewport parallelogram先执行coverage.TryClipParallelogram，再栅格化；不再仅组合两个离散samples。坐标超大或现有coverage不支持几何裁剪时仍使用已验证的目标边界裁剪coverage交集，保留1e10安全路径。AddImageMask的DeviceAligned局部mask表面采用同合同SoftwarePathCoverage viewport polygon，超大坐标仍保留有界coverage。没有改变COM prefix、引用或首错释放；未修改golden/精度。

原生证据src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/DeviceAlignedIntermediateRealizer.cpp 85–110：viewport保持parallelogram作为source clip；每次读取少于400行。托管相邻普通bitmap brush已使用几何裁剪，本轮合并独立viewport路径与mask消费者。

## 三次发布

命令均为`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`，项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64；三次成功退出码0；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。完整警告未独立保存。

1. 起始1109/1，总1110；SHA256 6061A6CBE08E94197656C392D5CECE7DDED21B4D27FC6965839EAC82DBF1CDA5；报告wpf-calibration-f158788378824b9bb906d1d0f42024d4.json；TRX未提取。
2. 几何消费者clip后1109/1；SHA256 2AB9F6DC4A3E9926D111A1154EC67900C10CA4B77489AADFFFF331E5A3DCE159；报告wpf-calibration-b7743fcbbd4d47c489d065610d63a3f3.json；float63→57/450，直接9→3/180，剩余仅mask77/79。
3. mask viewport同合同后首次1109/1；报告wpf-calibration-e36bd0b7c5ac4447af98268cb4c4ff05.json。联合876/876、1s、退出码0；最终1109通过/1失败/0跳过，总1110、2s、退出码1；SHA256 8E635FDDC04000B13BDB450B67976C576079E084F2392E8E847CF3FCE2AF4424；TRX f32f40be-3d5b-4e26-8a55-e3ea31360e7a；报告wpf-calibration-d6f78ac2e0054bc4a490d572fd1bace2.json。初次及定向TRX未提取。

报告及固定com-acceptance.trx在Tests/WpfGfxShape.ComAcceptance/TestResults/。

## 可核验关闭边界

直接Pbgra32固定原版校准180/180一致，source17旋转无tile跨三种过滤/fill/stroke/mask的9项差分全部消除；BitmapSource及实色对照各180组保持一致。float投影63→57/450，格式判定差57，唯一严格失败仍float投影。固定原版DLL及样本SHA256保持；不能以字节完全一致推断float/HDR等价。HDR、1e10、Vector4距离<1e-7未放宽。

本轮关闭既定直接字节fixture差分，不关闭完整ViewportAligned/DeviceAligned方法族。剩余57项float跨格式投影应先核验可比合同与真正float原生证据；不得从字节反推float golden，也不修改固定样本。新增路径专门故障注入仍缺，既有生命周期只作回归。下一轮合并float真实合同/旋转采样与失败生命周期生产验收；阶段4 Active，不能替换原库。
