# source clip直连候选：回归后撤回

尝试普通MIL source、None的realization直连，携带已有bitmap brush ABI mapping、source parallelogram和opacity，由FillInternalBitmapBrush消费，opacity mask仍原路径。候选将普通bitmap差异73降至64，但新增两个既有半透明stroke失败：期望(0,1077936128)，实际(0,2155872256)，alpha0.25变0.5；全量888通过3失败。未用候选实测回填旧预期，已完整撤回。

已有消费者用SoftwareImageCoverage.Combine sample bitmask交集，不是CShapeBase::ClipWithParallelogram几何求交后量化扫描，不能将该接线称为原生合同闭合。下一步需要保留未扫描contours及clip parallelogram，完成真正几何裁剪后再栅格；mask按独立合同处理。

## 实际发布

完整命令两次均为：dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64
目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
1. 直连候选，退出码0、60行日志；SHA256 A7B750339995394AB0A24E77DE019AF5ACC59F5744A67729B08775F319FF641C。报告85490014b58d4908b19c9bca78471d90对应本发布，DrawingImage86不同、普通bitmap64；888通过3失败。TRX后来覆盖，Run ID未独立保存。
2. 撤回候选，退出码0、59行日志；SHA256 ED9288E3DBC7939D24CEE8087170240CC409E8DE7353DC35E452AE0E833B8629。最终890通过1失败0跳过，总891，2秒。报告Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-292fa6dc138847e8902d5c4aa0d0ff01.json对应发布2：DrawingImage86、普通bitmap73、实色control0不同。
发布完整诊断未在测试覆盖前独立保存，不能声明无警告。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx；Run ID2e23d977-a8ea-4e96-8bef-e183e6c3dc03。

本轮没有保留生产变更，没有隐藏回归，非新稳定基线。source clip合同不能以sample mask交集替代；需要实际几何拓扑和场景生命周期闭环。原有HDR、1e10和1e-7断言未放宽。
