# S-20260929-079：普通图像源格式转换接线（完整模块未完成）

## 生产变更

Direct3D9SoftwareImageRenderer保留PBGRA32/BGR32直接路径，其他格式进入已有BitmapFormatConverter，转为PBGRA32后复用同一采样/混合链。IWGX源先QI取得WIC源；converter及QI引用finally逆序释放，HRESULT原样传播，不手写预乘。

原生证据：src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/swlib/swbitmapcolorsource.cpp:225-285,490-590，源格式决定realization，WrapInClosestBitmapInterface后WIC FormatConverter转换。当前实现只有可QI WIC源的适配子集，尚无完整IWGX-only包装器；不能称全部外部源均支持。

## 验证

在现有3个Composition场景增加普通BGRA32源，使用真实双缓冲创建导出取得未转换后缓冲，但通过普通BitmapSource资源绘制（不经过CopyForward转换）。旧DLL定向3失败，均Present E_NOTIMPL。生产接线后新AOT全量116/116。

进一步增加转换源写锁冲突：Present返回0x88982F0D、目标保持蓝色；解锁、释放调用方位图及owner后绘制得到FF80007F/FF00807F准确预乘source-over像素，之后切回原源正常。最终116通过/0失败/0跳过。仅BGRA32转换取得本轮像素验收，不泛称其他格式已经验证。

## 发布流水

本轮发布1次。完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，工具成功，60行日志；未独立归档完整诊断。产物`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。Red使用上轮DLL，未在修改前重新发布。发布后两次全量均116/116，第二次仅新增测试，无生产修改。

最终TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，SHA256 `9517BD86E34F80A33497BD2024660D05A6B222ABFB83C3CBF3E0DF5D20E9CF10`。测试程序集Debug/net10.0直接加载Release Native AOT DLL。Red及中间TRX被后续覆盖；最终日志耗时222ms。

## 剩余差集和下一轮

完整通用绘制/Compose模块仍未完成，无外部环境阻塞。本轮只是格式转换子集。下一轮须继续合并通用变换、缩小预滤波、覆盖率裁剪和原生组透明度；完整IWGX/WIC包装及通用目标消费接口、活动绘制回调重入仍需接线与验收。不得将单纯补其他格式测试作为下一轮。阶段4不关闭，不能替换wpfgfx。
