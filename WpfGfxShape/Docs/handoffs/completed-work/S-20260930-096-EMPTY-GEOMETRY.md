# S-20260930-096：空矩形遮罩一致性修复及验证

前轮发现SoftwareImageCoverage将负宽高矩形连接为可见四边形，和整数裁剪的空几何语义不一致。依据core/resources/rectanglegeometry.cpp:91-95，非正尺寸返回空覆盖，不将负尺寸解释为反射多边形。负缩放变换作用于正尺寸矩形的原有行为不变。

前轮新增负宽度斜切裁剪测试在旧DLL取得准确像素Red，实际输出有色而非保持蓝色背景；修复后build成功。本轮发布后验证负宽度、负高度、零宽度、零高度及恢复合法裁剪，全量131通过、0失败、0跳过，304ms。扩展既有参数测试，不增加测试数。组合几何中的空操作数已有回归，但本次未新增负尺寸组合操作数的独立像素用例，不扩大验收结论。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，成功。
产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。
SHA256 `18FA4D4BCC38435292DA37D8EDB8007B5D9F3725DE052C547F42BACA6086D55A`。
中间Red日志未独立保存。

原大模块仍未完成，本次仅修复已发现的生产错误，不关闭阶段或提升替换资格。通用内部目标/上下文及完整路径合同仍待实现，无工具阻塞。
