# S-20260930-107：退化Arc路径消费者语义

核对core/resources/PathGeometryWrapper.cpp:1048-1106，PathFigureData::SetArcData将ArcToBezier所有cPieces<=0结果当线段，区别于figure.cpp中忽略-1的构造接口。SoftwarePathCoverage修正arc.Length==0时仍补到端点的边，避免近同点弧在放大后与后续线段断裂。

新增真实Composition近同点Arc后接Line，路径放大2000000倍产生三角形。旧DLL失败；修复发布后最初连续面积预期不符，按8行子扫描复算覆盖20/64、52/64，修正预期后全量通过。该测试不是原生像素差分；中间失败日志未独立保存。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功。产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
最终TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取。

完整模块仍未完成，无工具阻塞。通用内部目标/上下文与完整组合、原生差分等门禁仍未满足。
