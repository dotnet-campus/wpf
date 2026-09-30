# S-20260929-075：软件图像双线性放大（完整模块未完成）

## 原生证据与实现

证据均位于src/Microsoft.DotNet.Wpf/src/WpfGfx：core/sw/swlib/swrast.cpp:225-295说明DrawBitmap使用Extend及PrefilterAndResample；bilinearspan.cpp:407-454说明反变换、AdjustForIPC和源引用；790-848说明16.16矩阵舍入。未修改原生代码。

Direct3D9SoftwareImageRenderer在既有软件目标/扫描链中支持整数边界正向放大，保留PBGRA32/BGR32源。复用Direct3D9SoftwareBilinearSpan.Select64BitFallbackTexels和InterpolateBilinearArgb；使用像素中心反映射、16.16步进、两行源缓存、Extend夹边及预乘source-over。无新增ABI，不在Visual遍历器新增像素循环。

缩小需要原生预滤波，非整数边界需要覆盖率光栅化；当前明确E_NOTIMPL，不作为完整缩放支持。大坐标/全部浮点舍入差分、普通transform矩阵、格式转换、复杂裁剪/组透明度、跨目标重入尚未闭合。两行缓存未做跨输出行复用，目标仍每图元复制。

## 验收

在原3个Composition参数化场景中加入水平2倍放大并裁剪：半透明红绿源在蓝色目标上准确输出FF60207F/FF20607F；单行源垂直放大验证Extend。修改前定向0通过/3失败，均Present返回E_NOTIMPL（-2147467263）。新DLL全量113通过/0失败/0跳过；新增缩小与非整数边界拒绝、失败不改目标像素及后续合法帧恢复断言后仍113/113。用例数未增加；尚未覆盖多行源的垂直混色和全部采样边界。

## 发布流水

### 发布1：基线成功

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

目标生产csproj，Release/win-x64，工具成功，11行日志。基线测试成功；新增放大断言定向3失败。产物`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。未在本轮提取基线SHA256，不借用上轮哈希。TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`后被回归覆盖，没有保留独立副本。

### 发布2：放大实现后成功

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

同一生产csproj/Release/win-x64，工具成功，59行日志，未另存完整诊断。产物同上。全量113/113；仅追加测试后再次全量113/113。最终TRX路径同上，SHA256 `90812A9279200F1A32B7E77550E838ADC8A4B1A4CD64502999A51259E7F545F9`。测试程序集Debug/net10.0直接加载Release原生DLL。

## 未完成与下一轮

本轮未按原计划完成跨目标重入与完整通用绘制模块，也没有外部阻塞；本记录不将其改写为完整交付。阶段4保持Active，不能替换wpfgfx。下一轮仍须合并跨目标/通道生命周期、通用变换/格式转换/预滤波、原生组透明度及多目标多帧成功失败验收，维持原完成条件。不得将单独补采样覆盖设为默认下一轮。
