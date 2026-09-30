# S-20260930-102：椭圆几何消费

GeneratedEllipseGeometryResource分离Transform及三个动画槽，提供CurrentValue。SoftwarePathCoverage按原生四段三次Bezier控制点构造椭圆，负半径取绝对值，复用细分和有向边；接入Visual/PushClip及组聚合路径。非有限值失败，零半径为空。

证据：core/resources/ellipsegeometry.cpp:58-95；core/geometry/shape.cpp:1144-1164；figure.cpp:1125-1168；utils.h的ARC_AS_BEZIER常量。浮点精度/边界原生差分尚未取得。

真实Composition先于发布新增负半径大椭圆覆盖目标、半径动画变零清空、仅更新值资源恢复绘制的准确像素断言。旧DLL失败，新DLL全量回归成功。当前验证的是内部/空覆盖，不代表椭圆边缘、中心动画和组内椭圆已有完整像素矩阵。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。
SHA256 `45F5F2B35C75CB01EA1FD66C35D951912766F60662BD9E882EEEB128C8C057F2`。
中间Red日志未独立保存。

完整模块仍未完成，圆角、弧段、完整组合及通用内部目标/上下文等缺口保持开放，无外部工具阻塞，不改变阶段和替换资格。
