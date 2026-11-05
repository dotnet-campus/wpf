# S-20261008-180：发布恢复基线

## 发布 #1

命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。
目标项目：WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj；配置 Release；RID win-x64。退出码0，成功。测试前读取全部11行可用日志：还原最新、net10.0编译输出及publish输出，无可见警告。产物：WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

首次以目录调用测试被工具拒绝（目标不存在），未执行测试；随后以实际csproj测试成功。全量290/290、0失败/跳过、317ms；TRX `9eda1b0f-d6a9-418c-8812-aad029bc9712`，开始 `2026-10-08T19:29:21.7891231+08:00`。SHA256 `A83BA3EF26B7CBF289B2674AF2E9A16D06662C40DD7B03746F65F76EF01C4BB7`（TRX测试输出）。默认TRX路径为Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，工具提示覆盖既有结果。

本文在下一次测试前保存已取得证据摘要，不是完整原始TRX/日志副本。没有新增能力，完整模块未关闭。
