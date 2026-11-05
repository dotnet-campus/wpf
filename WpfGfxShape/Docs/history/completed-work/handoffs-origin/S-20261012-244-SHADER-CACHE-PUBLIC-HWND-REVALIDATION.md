# S-20261012-244：Shader cache真实公开HWND发布复验（阶段4未关闭）

## 实际修改

移除 `Core/GeneratedHardwareShaderEffect.cs` 中临时 `Diagnose` 方法、输入/输出GPU回读、硬编码绝对路径文件写入及未使用的native变量；清空 `shader-diagnostic.txt`。生产代码搜索 `shader-diagnostic|Diagnose\(` 无命中。未修改fake、dummy、golden、阈值或原WPF源码；没有引入全8bit降float路径。

本轮验证的是工作区已有GeneratedHardwareBitmapOperations/SurfaceRenderTarget/ShaderEffect cache修改；不冒称这些修改由本轮新增，也不以普通测试代替真实公开输出。

## 发布流水

两次使用同一完整发布命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。

1. cache最新工作区立即发布成功；日志59行，实际Generating native code并输出发布目录。随后精准公开HWND定向8/8，RunID `451830d2-1b29-4bfb-b962-78a0dd50dfec`。未取得此第一次发布独立SHA，不以最终SHA替代。
2. 移除临时生产诊断后重新发布成功；日志59行，包含CA1416警告。项目 `Code/WpfGfxShape/WpfGfxShape.csproj`，Release/win-x64，产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`（相对工作区根）。最终实际SHA256 `85DF3895FE5E1EFFF232F099FC171E04C1341D8FA64FBB1742B3C065D92266BA`，由COM全量产物测试与校准报告实际回读。

## 真实public定向与全量

精准筛选 `FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels`。最终发布后8/8，0失败、0跳过，RunID `e5d6d315-07ce-49ed-bc6d-431d83c96ee4`。8个DataRow中实际发现并执行1个 `shaderEffect=true`：`(40,40,21,True,0,True)`。不以Shader名字零命中验收。

公开通道创建/绑定独立自有pass-through shader、真实D3D9Ex共享表面及HWND；前两帧窗口GetPixel均断言 `0x00563412`，更新设备颜色和消费者矩形后窗口断言 `0x00efcdab`，旧范围断言黑色。断言未修改。

普通全量 `WpfGfxShape.Tests.csproj`：4182/4182，0失败、0跳过。本次工具日志没有普通TRX RunID，明确不虚构。

COM全量 `WpfGfxShape.ComAcceptance.csproj`：1294通过/1失败/1295执行，0跳过、完整结束。RunID `9a6db5ae-29f3-47e8-936d-549ce5cefe26`。固定TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx` 已回读；全量中shaderEffect=true实例Passed，executionId `44d90387-40a9-435b-8452-711a360dc571`。固定TRX会被后续运行覆盖，定向RunID仅由本轮实际回读记录保留。

独立报告 `Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-ec98232e16034ec7a739b377067fd404.json`：最终SHA一致，FLOAT57/450、Direct Pbgra32 0/180，AotBitmapMismatches 0。唯一失败仍为 `CompositionTests.WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements`，与已有57/450基线一致，未发现新增失败；COM不是全Green。

## 未关闭范围与唯一后续模块

pass-through公开HW输出成功，不等于完整shader/effects或阶段4完成。真实公开sampler/constants、malformed bytecode/type、资源引用替换/关闭及首错failuretests未在本轮新增并独立验收。当前生产Render仍明确拒绝padding/Ddx（E_NOTIMPL），隐式输入之外sampler仍拒绝；inputscope/HDR目标格式的权威阶段4完整有限表尚未落实。保持阶段4 Active、不可替换原库。

下一动作必须合并完整shader生产方法族、上述公开成功/失败/生命周期合同、padding/Ddx/inputscope/HDR目标格式原生证据及有限表、最终AOT发布/精准定向/普通full/COMfull/独立归档，不能将本次通过当阶段4全部交付。继续source17/18同格式FLOAT及D3DImage剩余门禁；不通过修改golden或float降8bit掩盖既有失败。
