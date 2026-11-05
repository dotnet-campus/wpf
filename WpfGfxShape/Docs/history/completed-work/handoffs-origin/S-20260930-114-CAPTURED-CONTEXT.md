# S-20260930-114：捕获阶段统一上下文

GeneratedVisualRenderer.ImageDraw直接持有SoftwareImageDrawingContext，捕获时确定矩阵/裁剪/采样/合成值，执行时不再重组。移除永远为零的X/Y字段，层指令用专用构造避免位置参数混淆。仅内部状态重构，不新增绘制能力，不声称原生ABI已实现。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
真实DLL全量回归成功，TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。哈希未提取，无新行为Red。

大模块仍未完成：原生内部目标ABI、完整上下文和组合合同仍缺，无工具阻塞，完成标准不变。
