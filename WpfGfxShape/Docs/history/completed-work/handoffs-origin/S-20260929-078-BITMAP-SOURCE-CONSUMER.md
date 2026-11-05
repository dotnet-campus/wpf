# S-20260929-078：普通BitmapSource生产消费者

## 实现与原生证据

原生 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/bitmapres.h:80-97` 的GetBitmap/GetBitmapSource返回带引用的IWGXBitmap。GeneratedBitmapSourceResource增加AcquireBitmapSource；GeneratedVisualRenderer按资源类型捕获普通位图与双缓冲源，统一沿用既有绘制列表释放。Direct3D9SoftwareImageRenderer显式区分IWGX的MilPixelFormat枚举槽与WIC的GUID槽，避免ABI混用；尺寸/CopyPixels签名相同，复用现有扫描链。没有新增导出或假COM。

范围为PBGRA32/BGR32源，原目标仍PBGRA32；普通IWGX源不强求支持WIC QI。不等同支持所有外部COM源或完整格式转换。活动Render回调重入未验收；通用IRenderTargetInternal、变换、缩小预滤波、复杂裁剪、组透明度仍未闭合。

## 行为验收

现有3个Composition场景增加普通BitmapSource资源绑定/DrawImage像素验收。使用上轮发布DLL定向测试3失败，均Present返回E_NOTIMPL -2147467263；新生产实现发布后全量116/116。追加普通位图源替换、源写锁冲突返回0x88982F0D且目标不变、解锁重试、释放调用方源/源目标引用后仍可绘制、切回原源及输出位图存活验收，最终仍116通过/0失败/0跳过。用例数未增加。

## 发布流水

本轮实际发布1次，发生于生产实现后；Red使用前轮发布产物，不冒称本轮修改前重新发布。

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

项目Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，工具返回成功，60行日志，未单独归档完整诊断。产物`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。生产修改后全量通过，之后只增加测试并再次全量通过，没有再次改生产。

最终TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，记录SHA256 `E5087DAC62E9A4701683B3DC6FB528C2E2B858F999805E5C13F1F3052A8C597B`；116通过/0失败/0跳过，耗时234ms，测试程序集Debug/net10.0直接加载Release Native AOT DLL。Red TRX被后续测试覆盖。

## 交付边界与下一轮

本轮交付普通位图源窄路径，不是原定完整通用绘制/Compose模块，没有外部环境阻塞。不降低完整完成条件，不关闭阶段4。下一轮继续合并通用目标/源消费者、格式转换与变换/预滤波/裁剪/组透明度及实际回调生命周期验收；不得退回纯补测。当前仍不能替换wpfgfx。
