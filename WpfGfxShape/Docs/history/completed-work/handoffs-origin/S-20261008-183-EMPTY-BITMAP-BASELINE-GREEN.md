# S-20261008-183：恢复空 BitmapSource 修复验收

本轮发布#1：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，目标项目同命令，Release/win-x64，退出码0。立即读取完整11行可用日志：还原最新、net10.0编译输出和publish目录，无可见警告；这是增量发布，不代表全量重编译无警告。产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

直接生产DLL全量290/290，0失败/跳过，300ms。TRX `c2e05e85-2ee9-436e-b96e-2102e5a9835c`，开始 `2026-10-08T19:35:31.0073210+08:00`；SHA256 `5B8168E2FF94EEAAAAD833F7796620466BAB0299C92E97A52A6C67C3C2D1EB34`。本发布验证了S-182待验收修复，但SHA仅归属于本轮发布#1，不追溯声称取得上一轮发布SHA。

本文件在下一次测试覆盖TRX前保存摘要；未复制完整原始TRX。完整ImageBrush模块仍未关闭，私有realizer ABI、非零画刷Pen等仍待实现。检查兄弟资源发现原生doublebufferedbitmapres.h:38–41同样以底层对象非NULL判定HasContent，下一步在同一消费者链验证实际未初始化资源。