# 原版校准：source clip必须进入目标形状裁剪

核查source17无旋转None/模式1/2/3每组3 consumer仅1不同，不能再用通用位图边界修正解释全部问题。

原生关键证据core/targets/ShapeClipperForFEB.cpp58–143：ApplyBrush获取source clip device parallelogram，以CShapeBase::ClipWithParallelogram与目标shape相交，然后扫描final shape；不是源覆盖值×目标覆盖值。生产Direct3D9SoftwareImageRenderer.RealizeBrush先AddImageMask生成整面位图，再NativeImmediateSolidRealization.CreateBitmap把mapping设Identity/Border，source clip身份未携带到目标消费者；DrawBrush最终按目标coverage采样整面bitmap。此处合同顺序与原生不同，需要realization/shape消费者联合修复，不可仅改插值权重。尚未实现修复，也未证明此问题解释全部DrawingImage86差异。

测试SoftwareImageCoverage同类延后浮点裁剪候选，无改进，已撤回。源码恢复上一轮sourceBox实际边界修复逻辑，没有保留候选，没有放宽预期。

## 本轮实际发布

两次完整命令均为：dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64
目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，固定产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 延后coverage预裁剪实验，退出码0、60行日志；SHA256 A2CDDD0BD687C544995EE5FDA872ED3276BB47B7079D89E2801D85E905EE4CE3，定向报告38abc1d86d834b4da8adb11d75ef376e，普通bitmap仍73不同、DrawingImage86。定向TRX后覆盖，Run ID未保留。
2. 撤回候选恢复已确认逻辑，退出码0、60行日志；SHA256 67BA8969849DCFF9E05C0EFE80C0B2998F6ABF197DAF509B91286E7AAF53FF6A。最终报告Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-7d44e8c75814477fa58c7fc0850eb11d.json对应发布2。

完整发布诊断未在测试覆盖前独立保存，不声明无警告。最终全量890通过1失败0跳过，总891，2秒；普通bitmap73不同，DrawingImage86不同，实色180全一致。TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，Run IDc61f7444-f495-4a99-9479-e6b6a14ff5fe。非新的稳定通过基线。

下一完整闭环：保留realized brush原生source clip和mapping身份，将其送入fill/stroke目标几何相交扫描；opacity mask按其不同原生合同处理，不把mask也硬套shape交集。联合空内容、失败、回调/释放和原版全图校准再发布验收。禁止用sample mask相乘冒充原生几何相交、禁止回填AOT实测。完整intermediate与MSVC ABI仍未关闭。
