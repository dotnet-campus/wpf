# S-20261008-147：工具正常与生产 AOT 基线复核

## 范围

交接入口、执行约束、稳定基线和唯一工作项均已读取。工具实际调用正常，无工具阻塞。本次没有修改生产代码，没有新增生产能力，也没有关闭完整模块。

## 发布流水

- 发布序号：1（本次唯一发布）。
- 完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`。
- 目标项目：`Code/WpfGfxShape/WpfGfxShape.csproj`；配置：Release；RID：win-x64。
- 结果：发布工具返回执行成功，日志共11行；未单独保存完整发布日志，不宣称具体警告数量。
- 产物：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64\wpfgfx_cor3.dll`。
- 对应该次发布后测试实际加载DLL的SHA256：`A8D2B8215D74942E7378DA6EE13F0FA257CD7D6F46B4DD0ECB0E7757FBE5DF23`。

## 直接生产 COM 验收

- 测试目标：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`。
- 测试工具退出码：0。
- 全量：174/174通过，0失败，0跳过，230ms。
- TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。
- TestRun ID：`b24aabe4-caf9-4608-97c4-d1afb7577e83`；运行时间：2026-10-08 14:17:12。
- TRX记录的加载路径与SHA256对应上述产物。TRX为固定输出路径，后续运行可能覆盖，以本档案保留本次对应关系。

## 后续边界

唯一下一动作保持完整ImageBrush遮罩映射、层消费与Compose生命周期闭环，按current-work-item.md中的原生证据、真实读取重入、最终释放、内部消费者合同与AOT联合验收要求继续。此次复核不是独立生产模块交付，不授权缩小范围。阶段4仍Active，当前不能替换原wpfgfx_cor3.dll。
