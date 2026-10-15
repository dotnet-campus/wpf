# 直接source realization与插值状态联合修复

根因：此前直连候选只改DrawBrush入口，SoftwareBrushRealization.BeginPrimitive仍对图像刷子强制nearest。原路径先预采样整面bitmap，nearest正确；直接source/mapping路径必须使用原brush scaling，不能沿用该状态。原版null pen实验64成立，之前候选128是状态错误，不是旧oracle应改。

实现GeneratedImageMask.CanRealizeDirectly统一限定：非DrawingImage、无VisibleTiles、TileMode.None、MIL source；RealizeBrush经directConsumer保留source/mapping/source clip/opacity，以已验证几何clip消费者处理目标；BeginPrimitive与DrawBrush两条入口均按资格保留brush.ScalingMode及prefilter默认，否则保持预采样bitmap nearest。opacity mask仍AddImageMask原路径，无直连。source AddRef、candidate未提交释放、primitive结束逆序清理保持。

## 发布1

完整命令：dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64
目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，退出码0，60行日志；发布日志54–60确认CA1416警告、Generating native code、lib/exp及固定publish目录，未宣称无警告。
产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
SHA256 4B3A350CE8550FDC49A0F24045B0347816B5E96A751366A9F2B930E5143B0ED7。

全量896通过1失败0跳过、总897，2秒。此前两项relative bounds/半透明或null pen旧失败消除，21项直接source clip ABI及锁冲突/重试通过。普通bitmap180组差异73降至64；DrawingImage450组仍86不同；实色control180全部一致。唯一失败严格原版校准，非新稳定通过基线。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，Run IDe71adeb4-6cd0-47f0-8bc9-c41d105e39a8。
报告Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-e505d8fe708e43ae805397da270046da.json对应发布1。

原版预期及HDR/1e10/1e-7断言保持。直连仅普通非重复MIL位图，tiled/partitioned/DrawingImage尚原路径；完整复杂几何source clip支持仍不关闭。下一完整闭环按残余bitmap分组定位tile物化/固定点filter，联合DrawingImage对应路径、消费者及失败/回调/释放后验收；不能把64/86不同隐藏或换golden。阶段4仍Active，不能替换原库。
