# S-20260929-080：组透明层生命周期（完整模块未完成）

## 用户要求与实际结果

用户明确要求确保完整模块。本轮实现透明层生产生命周期，但尚未满足原完整模块条件；不得以116项通过或本层闭环代替完整交付。本轮没有完成通用目标ABI、通用变换、缩小预滤波、复杂裁剪、IWGX-only包装及活动绘制重入。

## 原生证据

均位于src/Microsoft.DotNet.Wpf/src/WpfGfx：
- core/common/InternalRT.h:114-172：BeginLayer累积、EndLayer整组应用、EndAndIgnoreAllLayers失败丢弃；允许嵌套。
- core/uce/drawingcontext.cpp:2295-2315：PushOpacity经PushEffects处理；3030-3134说明层边界与裁剪、目标、内容交集及中间层。
- core/common/InternalRT.h:44-112：IRenderTargetInternal多重继承IMILRenderTarget/CIntermediateRTCreator，DrawBitmap接收CContextState及IWGX源。
- core/api/api_rendercontext.h:20-125：CContextState包含显示集/DPI私有指针、矩阵、裁剪、RenderState、LightData及snapping状态，不能以现有简化软件回调布局冒充原生ABI。

## 生产修改

GeneratedVisualRenderer绘制快照保存Visual Alpha及render-data PushOpacity/Pop边界。Direct3D9SoftwareImageRenderer实现嵌套透明中间缓冲：图像继续经过原SoftwareRenderTargetSurface/scan pipeline；Lock映射到顶层中间缓冲；结束层整组乘Alpha/source-over到父层或目标；Dispose丢弃未结束层。不是对每个图元独立乘Alpha。源引用继续随快照finally释放。

当前中间层取整个目标大小，未做原生内容/几何边界优化；不支持几何mask、effect、动画opacity及完整原生层ABI。非有限opacity明确失败。任意Alpha舍入未做原生全矩阵差分，准确像素验收集中0.5及嵌套0.25。

## 行为验收

现有三个Composition参数场景增加：
- 父Visual与子Visual重叠绘制，在父层0.5透明度下只应用一次，准确红/绿叠蓝FF80007F/FF00807F。
- 嵌套0.5层，在父无内容时最终0.25，准确FF4000BF/FF0040BF。
- render-data PushOpacity包裹两个重叠DrawImage，同样整组应用。
- 层中第一图元成功、第二图元非整数位置E_NOTIMPL，丢弃未结束层，蓝色目标保持不变；恢复合法内容后共享双目标和后续帧继续正常。

Visual层新增断言先在前轮DLL上3失败E_NOTIMPL；发布1后全量116/116。PushOpacity新增断言在发布1上定向失败；接线发布2后全量116通过/0失败/0跳过（223ms）。用例数未增加。

## 发布流水

发布1完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

生产项目Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，成功，工具日志60行。产物`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。随后全量116/116，之后PushOpacity测试失败；未提取发布1 SHA256，TRX同一路径已覆盖，不引用其他哈希替代。

发布2完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

同一项目/配置/RID和产物路径，成功，工具日志59行。最终全量116/116。TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`；SHA256 `6649BF835B45F55B7366D1BE448439DD6365B275221A28470A2E8BD114988D64`。测试程序集Debug/net10.0，直接加载Release/win-x64原生DLL。未独立保存完整发布日志及中间TRX。

## 完整模块的硬差集与续接

不是外部环境阻塞，而是尚未完成的生产接口依赖：目前直接QI位图目标取得缓冲，不是原生DrawingContext→IRenderTargetInternal消费。该接口多重继承和CContextState布局必须一起确认并实现，不能只添加同名接口或返回QI成功。下一轮必须将此接口/上下文/消费者生命周期作为原完整模块内部核心实现，同时合并变换、裁剪、预滤波、源包装及真实端到端失败重入验收；本轮层状态机合并进去，不重做。

保留原完整验收条件，阶段4仍Active，不能替换wpfgfx。
