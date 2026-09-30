# S-20260930-098：GeometryGroup有向轮廓填充

依据core/resources/geometrygroup.cpp:40-92，先聚合子形状，再应用组FillRule。SoftwarePathCoverage递归聚合矩形/线段路径/嵌套组的有向边并累计绕组，保留各级变换，不将Winding错误实现为子遮罩并集。GeneratedImageClip接入既有组遮罩生命周期。深度超过256及未支持类型明确失败。

真实Composition：两个同向半像素矩形，Alternate抵消、Winding保留；第二矩形负缩放后与第一重叠但方向相反，Winding抵消，准确像素通过。旧DLL定向失败，新DLL全量回归成功。路径子项与嵌套组未取得单独矩阵验收，不能泛称全部GeometryGroup完成；含CombinedGeometry/曲线子项仍未支持。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。
SHA256 `F9DB23934A5901448EF2AD6D2AD21D052F2B196644E395476111452D0A055449`。
中间失败日志未单独存档。

完整模块仍未完成，无工具阻塞；通用内部目标/上下文、曲线/圆角/椭圆以及完整几何组合合同仍缺，阶段与替换资格不变。
