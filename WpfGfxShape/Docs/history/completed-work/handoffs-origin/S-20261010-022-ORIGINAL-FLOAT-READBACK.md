# 原版float存储读回契约探针（绘制闭环未关闭）

在现有WpfReference项目新增FloatReadbackTests，绝对路径加载已安装WindowsDesktop原版DLL，经factory创建格式26软件目标，确认WIC float格式；写锁输入HDR/负分量后CopyPixels完整2×2 Vector4精确一致。此证据证明float位图存储与读回可达，不证明DrawingImage渲染/旋转或私有绘制ABI。

最初错误假设MilColorF为ARGB，探针失败；原生wgx_core_types.w声明typedef D3DCOLORVALUE MilColorF，改RGBA。Clear HDR仍失败，实测(1,0,0.122138776,1)而非(1.25,-0.25,0.1234567,1)。sw软件Clear float分支有特殊回退，因此把Clear仅作为观察，与无损存储读回分开，不降低HDR要求或用AOT回填。测试最终1/1通过、70ms、退出码0。报告代码写入ReferenceResultsPath并AddResultFile，但工具按original-float-readback枚举未找到产物；报告文件/SHA256未独立取得，明确尚未归档。

原生证据仅src/Microsoft.DotNet.Wpf/src/WpfGfx/：core/api/exports.cpp 54–93、140–163；include/wgx_render.h 264–336；core/api/api_factory.cpp 251–324；core/sw/swlib/swsurfrt.cpp 335–378；wgx_core_types.w通过文本索引确认MilColorF typedef。每次少于400行。不修改原WPF、slnx、固定golden，不启动Host。

## 本轮唯一发布与生产回归

命令`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`；项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64；成功、退出码0。产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll；本轮未改生产代码，SHA256保持CCD2C01F350C9E8BA1F3D3DFA2F0E945BFE9BD9C3D5E39AFCE40602162197D1D（上一已核验身份，最终报告未再读取头部，不冒称新提取）。完整警告未独立保存。

起始1111/1，总1112，报告wpf-calibration-051b8dc127274195af971a9d7d85d5c3.json；起始TRX未提取。最终联合生产定向成功（未提取计数）；全量1111通过/1失败/0跳过，总1112、2s、退出码1；TRX e04905e6-b6f9-45cc-aa68-53b10f4ee764；报告wpf-calibration-bb153dfc62a445d2ac6e6220cdae09b3.json。生产报告及固定com-acceptance.trx位于Tests/WpfGfxShape.ComAcceptance/TestResults/。

生产差分基线仍float57/450、直接字节0/180。本轮仅建立缺失契约证据，不满足用户要求的生产大块交付。下一轮应合并原版float DrawingImage实际绘制可达路径、同格式独立golden及生产旋转/失败生命周期；不能将写锁读回或限幅Clear当成渲染等价证明。阶段4 Active，不能替换原库。
