# S-20260930-110：空目标裁剪先于源实现

DrawTransformed在读取源尺寸/像素、转换/预滤波前检查目标边界与整数裁剪是否为空；统一覆盖整数与仿射分派。并非对任意离屏形状新增忽略源错误语义。

依据已有SoftwareRenderTargetSurface.DrawPathInternal先UpdateCurrentClip再Lock/Fill的顺序；原生swrast.cpp:655-745色源在目标调用的填充阶段实现。没有改变目标会话自身锁定合同。

真实Composition在持有BGRA源写锁时：正常绘制仍报原锁冲突；空矩形裁剪+半像素图像成功且目标不变；解除裁剪又报锁冲突；释放源锁后原有恢复像素继续通过。旧DLL定向失败，新DLL全量通过。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取，中间Red未独立保存。

完整模块仍未完成，无工具阻塞；通用内部目标/上下文等门禁仍开放，不以本修复结案。
