# S-20260930-140：矩形PushClip内容边界

合并GeneratedImageContentBounds的Visual render-data和PushOpacityMask作用域遍历，实现变换/clip联合状态栈。轴对齐矩形PushClip在入栈坐标变换后求交，Pop恢复；每个图像先与clip求交再合并内容边界。旋转/斜切图元与clip、复杂几何和Visual.Clip仍明确拒绝，不以包围盒交集冒充精确几何。

真实相对viewport用例新增mask内PushClip：原图宽2，clip仅x=1宽1，Pop后另一左侧图元不影响mask边界。普通/相对翻转+绝对平移两用例旧DLL失败，新DLL准确像素通过。全量回归成功。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取，中间Red未独立保存。

五步计划仍在第1步；未关闭整个边界任务，也未完成平铺和其他后续步骤。方案进度已同步。
