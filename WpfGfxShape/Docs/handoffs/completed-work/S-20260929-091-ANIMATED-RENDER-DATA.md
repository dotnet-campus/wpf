# S-20260929-091：DrawImageAnimate与PushOpacityAnimate消费

GeneratedVisualRenderer接入已解析的两个动画指令，捕获时按ResourceSlots读取RectResource/DoubleResource当前值；空动画使用指令静态值。复用原绘制快照、透明层、有限数检查和释放路径。不新增时间线调度或测试专用入口。

原生依据：core/resources/renderdata.cpp:773-789、906-925分别将矩形动画和透明度动画传入DrawingContext。原生文件未修改。

真实Composition测试先于生产修改，旧DLL定向失败；新DLL验证动画矩形覆盖静态矩形、动画透明度覆盖静态值、仅更新资源不重发render-data即改变像素、NaN失败保留目标、解绑动画恢复静态0.25透明度与静态矩形。扩展既有用例，最终124通过、0失败、0跳过，239ms。中间Red日志未独立存档。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，成功。
产物：`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。
对应SHA256：`B0FF0AC94E9D4E91F4C4EC08373595D814AC110542FB182BCB853A05BE960883`。

## 状态

原完整模块未完成，没有外部阻塞。内部目标/上下文、完整路径/曲线/组填充、几何动画、IWGX-only反向包装和活动Render回调重入仍未闭合。此处仅图像和透明度指令，不代表其他render-data动画都能绘制。阶段4Active、无替换资格；Ledger完成状态不提高。
