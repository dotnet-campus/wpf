# S-20260929-085：整数矩形裁剪接线（完整模块未完成）

GeneratedImageClip将无圆角/无动画RectangleGeometry在入栈时转换到目标坐标，继承父裁剪并求交。Visual和PushClip/Pop通过绘制快照传递裁剪；Pop恢复变换/裁剪/透明层类型状态。SoftwareImageRenderer复用SoftwareRenderTargetSurface.DrawBitmap裁剪接口，不修改源采样映射。

原生证据：src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/drawingcontext.cpp:2070-2080,2670-2710，PushClip经PushEffects将mask变换到目标空间。本轮仅等价整数轴对齐矩形路径，不是完整几何mask实现。

现有3个Composition场景增加Visual右侧单像素裁剪，左像素保持蓝色；解除Visual裁剪后PushClip/Pop再绘制完整图像，验证状态恢复。旧DLL定向失败，发布后全量121通过/0失败/0跳过，234ms。未单独保存Red具体诊断；不能宣称已验收嵌套裁剪/变换/层所有组合。

发布1完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

目标生产csproj，Release/win-x64，工具成功，60行日志，未独立归档完整诊断。产物`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，SHA256 `3538ED774A7B452416D12FA8A270D830CCBA5DE98D4EB7152E3393531F461A33`。Debug/net10.0测试直接加载Release原生DLL。Red使用前轮产物，TRX已覆盖。

非整数边界、圆角/动画/其他几何仍明确失败。通用仿射覆盖率、内部目标/上下文、反向源包装和活动Render重入仍未完成，无外部环境阻塞；原完整模块未交付，不关闭阶段4。下一轮继续联合这些生产依赖与已有裁剪/变换/层的真实消费验收，不将单独补裁剪测试视为完整交付。
