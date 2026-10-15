# S-20261008-148：恢复验证与内容边界缺口核对

## 范围与结论

工具实际调用正常。读取当前权威入口、规则、工作项、稳定基线、生产项目配置与直接 Native AOT 验收方法，并核对 GeneratedImageContentBounds.cs。未修改生产代码或测试，不增加生产能力，不关闭完整遮罩模块。

当前 TryClip 仅接受 GeneratedRectangleGeometryResource，且 RadiusX/RadiusY 非零时失败；复杂几何内容边界仍待原生证据与生产实现。旋转矩形既有修复不重复领取。未以光栅采样替代几何 bounds。

## 发布流水

1. 命令：`dotnet publish Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -o artifacts/publish/WpfGfxShape/Release/win-x64`。
   - 目标：Code/WpfGfxShape/WpfGfxShape.csproj；Release；win-x64。
   - 失败：工具返回9行日志，首个错误为 MSB1009：项目文件不存在。相对路径未包含工作区中的 WpfGfxShape 子目录；非工具故障。未取得该次产物或SHA256。
2. 命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64 -o d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64`。
   - 目标：上述绝对生产项目路径；Release；win-x64。
   - 成功：工具返回执行成功、11行日志；完整发布日志未另行保存。测试覆盖了最后日志缓冲，不补造诊断。
   - 产物：WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
   - 对应下述测试记录的SHA256：A8D2B8215D74942E7378DA6EE13F0FA257CD7D6F46B4DD0ECB0E7757FBE5DF23。

## 验收

直接按 WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj 执行全量 dotnet test，退出码0；174通过、0失败、0跳过，223ms。测试程序集为Debug/net10.0，加载的生产DLL为本次Release/win-x64发布产物。

TRX：WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx；TestRun ID：25a485b7-e515-4940-bf42-9aecadae59c5；时间：2026-10-08 14:21:44。TRX记录实际加载DLL绝对路径和上述SHA256。未创建Host、修改方案、原WPF源码或新增ABI。

## 后续边界

唯一下一动作仍为完整ImageBrush遮罩映射、层消费与Compose生命周期闭环。复杂几何bounds、完整平铺矩阵、真实读取重入/最终释放及原生内部目标消费者合同仍需联合完成；没有明确外部阻塞，不授权缩小为纯验证模块。阶段4仍Active，当前不能替换原wpfgfx_cor3.dll。
