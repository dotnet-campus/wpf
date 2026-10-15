# S-20261008-198：发布后初始基线

## 发布 #1

- 命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。
- 项目：WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj；Release / win-x64；退出码 0。
- 已在测试前读取全部可取得的 11 行日志：还原最新；输出 net10.0/win-x64 DLL 及发布目录；未显示重新 native 编译，无警告/错误文本。未另存原始日志文件。
- 产物：WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
- SHA256：19CE757E88E5C75ACA8820B66EF5A63828B46037080E28CA02A342BB13433ACD。

## 验收及覆盖前证据

唯一 ComAcceptance csproj 直接加载生产 DLL：302/302，失败/跳过均 0，341ms。

TRX：c9e0ea17-ff13-4f76-96f9-3a64c93b46dd；start=2026-10-08T20:20:06.1939757+08:00；finish=2026-10-08T20:20:06.9018848+08:00。固定文件 Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。本文件保存后续覆盖前身份与结果摘要，非完整 TRX 副本。

本次仅复验，不新增生产能力。完整模块未关闭，不能替换原 DLL。
