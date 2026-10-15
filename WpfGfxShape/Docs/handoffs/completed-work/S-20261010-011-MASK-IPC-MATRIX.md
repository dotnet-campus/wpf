# 字节alpha mask IPC矩阵步进（完整闭环未关闭）

SoftwareBitmapMask.Prepare按颜色源字节span合同一次量化IPC矩阵与offset，Alpha使用整数矩阵步进，不再逐像素double变换后量化。仅bilinear字节mask改变，nearest及float保持原路径；没有新增公开API、COM接口或所有权变化。

原生证据仅src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/swlib/bilinearspan.cpp 4440–4525、4610–4667：mask调用颜色源scan op，然后按alpha缩放。每次少于400行。

同时试验移除AddVisibleTileMask浮点分区累加重定向，导致source15超大tile三项半透明回归，已撤回。分区贡献提前字节量化不能等同单个目标兼容intermediate；保留浮点累加保护，不更改断言或golden。

## 三次发布

每次命令`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`，项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64，均成功、退出码0；共同产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。完整警告未独立保存。

1. 起始1102/1，总1103；SHA256 5815E495E5CA9C5E89CDF09BA8E7F891579223769916EA7D2F57B83B2E9E8FE7；TRX b5861636-2215-4e80-9da1-fa82a3ff8b3a；报告wpf-calibration-cf9bfd1aa1944e2c83eae18ce4f0d438.json。
2. mask及字节分区候选1099/4；SHA256 0BC41C872B797D0E2304D0D4296C179D400C56CB7267BB7C4896ACD74E22C479；TRX身份未提取，已覆盖；报告wpf-calibration-d54d786a86a14d0d8dc0a9de9b161599.json。
3. 撤回分区候选、Prepare预计算后联合869/869、1s、退出码0；最终1102通过/1失败/0跳过，总1103、2s、退出码1；SHA256 F7F7B7E77DD69262445FBDAD622B127EB72D14A76955B33547A884D10C165310；TRX ac5afbeb-122a-4d52-9697-a06d52400f56；报告wpf-calibration-1b31145fa20f423689a2e279ca0d96f3.json。

报告及固定com-acceptance.trx在Tests/WpfGfxShape.ComAcceptance/TestResults/。定向TRX身份未提取。严格差分未减少：float63/450、直接Pbgra32 9/180、格式判定差54。固定原版DLL/样本身份不变，保持HDR、1e10、Vector4距离<1e-7。

本轮仅推进mask生产合同，未完成用户要求的大块旋转闭环；不能以回归通过宣布关闭。剩余mask实际作用域及重复采样、float旋转覆盖/采样、新路径故障生命周期须合并实现。不能重复移除浮点分区累加或仅调整矩阵精度。阶段4 Active，不能替换原库。
