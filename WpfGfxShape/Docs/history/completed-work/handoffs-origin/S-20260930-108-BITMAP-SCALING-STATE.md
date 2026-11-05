# S-20260930-108：位图缩放状态消费

VisualSetRenderOptions此前未接分发，不是已接收但忽略。接通36字节状态命令的BitmapScalingMode flag，Visual继承与绘制快照带入消费者。最近邻禁用预滤波并以逆变换源像素采样；Linear禁用预滤波，默认及HighQuality沿用现有链。其他flag明确E_NOTIMPL，不成功忽略。

依据node.cpp:697-718保存选项，include/Generated/wgx_misc.h的MilRenderOptions布局/flags/modes。尚未逐项证明默认/HighQuality与原生全部质量分派等价。

真实Composition在缩放时设置NearestNeighbor得到原红绿像素，清除flag恢复双线性混合；旧DLL失败，新DLL全量成功。子Visual继承、缩小最近邻及全部模式矩阵未单独验收。

## 发布

发布1、2完整命令均为：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

发布1失败：GeneratedVisualTargetResources.cs:135 CS1597属性尾多余分号；修正后发布2成功，Release/win-x64。
产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。最终哈希未提取，中间日志未独立存档。

完整模块仍未完成，通用内部目标/上下文、完整RenderOptions、组合与原生差分等门禁保持，无工具阻塞。
