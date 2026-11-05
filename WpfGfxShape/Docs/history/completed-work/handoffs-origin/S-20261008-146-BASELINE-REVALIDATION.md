# S-20261008-146：恢复基线重新发布与验收记录

## 范围

本记录仅归档恢复会话的实际发布和回归，不是新的生产翻译切片，不代表完整ImageBrush/Compose模块完成。已重新读取交接入口、执行约束、工作项和稳定基线；工具实际调用成功，无工具阻塞。

## 发布1：恢复基线

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64 -o d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64`

生产项目：`Code/WpfGfxShape/WpfGfxShape.csproj`；配置Release；RID win-x64。发布工具返回执行成功，日志共11行；完整发布日志未独立保存，后续测试日志已覆盖工具的最后日志。

产物：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64\wpfgfx_cor3.dll`。

## 直接生产COM验收

发布1之后对`Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`执行全量测试，退出码0，174/174通过、0失败、0跳过，218ms。

- TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。
- TestRun ID：`fc8edb01-4fe6-43a1-b100-7fa30b87f609`。
- 运行时间：2026-10-08 14:10:44。
- 测试实际加载DLL的SHA256：`A8D2B8215D74942E7378DA6EE13F0FA257CD7D6F46B4DD0ECB0E7757FBE5DF23`，从该TRX读取，与发布1对应。

本次未修改生产代码、测试代码、TFM、SDK、原生WPF源码或slnx；不宣称新增覆盖或新的生产能力。

## 后续与完成边界

唯一下一动作继续遵循`../current-work-item.md`：合并完成ImageBrush遮罩映射、层消费与Compose生命周期闭环，包括复杂几何安全内容边界、平铺完整差分矩阵、真实IStream/WIC读取重入及最终释放、原生内部目标消费者合同。不得将本次基线验收作为模块结案，也不把单项补测规划为独立下一轮。

阶段4仍Active，当前不能替换原wpfgfx_cor3.dll。
