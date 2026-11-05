# S-20260930-136：图像内容边界及相对viewport

新增GeneratedImageContentBounds，在本地坐标累计受支持DrawImage/动画矩形及子Visual变换、render-data变换；不使用目标尺寸。带clip和非图像内容明确不支持，不能当作完整bounds renderer。

Visual ImageBrush相对viewport按内容边界换算，RelativeTransform经单位矩形到内容边界的共轭变换接线。原生依据TileBrushUtils.cpp:44-165。PushOpacityMask相对单位尚无边界计算入口，仍拒绝；空内容等边界尚需完善。

真实用例内容x=1宽1、目标宽2，相对viewport映射红色内容到右侧并以遮罩平均alpha=0.5输出0x80800000；继承源锁失败及释放后恢复验证。旧DLL失败，新DLL定向和全量通过。RelativeTransform/多子Visual尚未独立像素验收，不泛称完全支持。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取，中间Red未独立保存。

共同依赖开始落实，但本轮仍未完成整个方案C；不以该子集关闭。
