# S-20260930-103：圆角矩形几何消费

按core/geometry/figure.cpp:984-1000、1038-1074的半径截断及16控制点接入四段Bezier与四条边。SoftwarePathCoverage支持圆角矩形独立遮罩和GeometryGroup子项，复用CurrentValue动画槽和变换、HFD细分、有向边扫描。空尺寸为空几何；非有限半径明确失败。

真实Composition在旧DLL失败，新DLL验证超大半径截断后的内部像素、空宽度不绘制、解除裁剪后恢复。尚未独立验收圆角边缘、半径动画、多级组或原生差分，不泛称完全兼容。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
全量回归成功；TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。
SHA256 `E395D490A0A9533398924A672FC10651482EB777D71AF64474DDB2C9C5EFE1EB`。
中间Red未独立保存。

完整模块仍未完成；Arc、完整组合几何、通用内部目标/上下文及原生差分仍有缺口，无工具阻塞。
