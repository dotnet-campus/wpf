# 有限source clip：原版差异先失败再修复

新增原版WPF实验：目标rectangle(0.6,0.2,1.9,1.1)，source parallelogram[(0.2,-0.3),(2.6,0.4),(2.1,1.8),(-0.3,1.1)]；Geometry.Combine Intersect后红色填充4×2 Pbgra32。固定原版文件Tests/WpfGfxShape.WpfReference/TestResults/source-clip-intersection-9ea80318e1d241db98efb079ef626374.json，包含输入、原版DLL hash及32字节输出。此实验验证显式几何交集，不宣称是原版私有brush ABI直接调用。

BitmapRealizer生产ABI测试增加顺逆source clip、有限PathShape与生产DrawPath实际消费者。首次19通过2失败，原版两个边缘pixel红/alpha84/36，旧sample mask intersection88/40，构成可区分算法的TDD证据；保留effects首错及output lock失败/重试。

SoftwarePathCoverage保存TryCreateFigures的未量化轮廓，新增有限数/四顶点校验、顺逆clip方向及空交集处理；半平面几何裁剪后重新量化扫描，保持原fillRule。SoftwareImageCoverage对path和可分配Boolean组合传播clip；没有原轮廓的coverage明确失败，不能silently退化sample mask近似。BitmapRenderTargetExports.FillInternalBitmapBrush真正调用新几何clip，未恢复场景realization直连，mask原路径不变。

## 发布1

完整命令dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64。
目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，退出码0，60行日志；完整诊断未在测试覆盖前保存，不声明无警告。
产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
SHA256 C561B057A8B15AF549BCA6C24C017AA1A7DC16F8A67D0D700385D219CA63BE10。

发布后定向21/21通过，两个新case真实进入geometry clip，边缘字节与原版精确一致。全量896通过1失败0跳过，总897，1秒。旧场景校准仍失败：bitmap73、DrawingImage86，不因局部生产ABI修复而减少。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，Run ID38af9e7a-9c45-4465-afd7-66860751b7e2。
报告Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-94dffc9aac6a451f8512f1f58f52c187.json，对应发布1。

未放宽HDR/1e10/1e-7断言；不是全量稳定基线。完整曲线/复杂Boolean clip与range溢出合同未闭合，仅有原轮廓且可表示的半平面clip。下一步联合realization保留source clip/mapping接线、target contour保存和半透明stroke原版fixture；恢复直连必须验证此前两个旧回归并区分错误旧oracle与实际回归，再完成fill/stroke/mask生命周期发布验收。
