# S-20260929-074：软件图像管线接线（完整模块未完成）

## 原生证据

- core/uce/samethreadcomposition.cpp:97-139：SubmitBatch在当前线程立即处理，不能把Commit改为等Present才处理。
- core/uce/printtarget.cpp:46-105：GenericTarget保留已有目标像素，不Clear，BeginFrame/Render/EndFrame。
- core/sw/swlib/swsurfrt.cpp:635-696：DrawBitmap更新裁剪、读写锁、调用软件渲染器、无论成败解锁。
- 上述文件均位于 src/Microsoft.DotNet.Wpf/src/WpfGfx/，没有修改原生源码。

## 生产修改

GeneratedVisualRenderer不再直接混合像素。先捕获矩形/累积偏移和持有COM引用的双缓冲图像源，后调用目标COM和执行软件绘制；finally逆序释放源列表。GeneratedTargetResource保活根、目标并冻结尺寸。目标自身的引用不再仅依赖可重绑定资源字段。

新增Direct3D9SoftwareImageRenderer，接入已有SoftwareRenderTargetSurface.DrawBitmap → SetupPipeline → OutputSpan，提供真实source-over扫描回调。原生锁缓冲经步长/长度校验适配byte[]，逐行回写，失败保留已经处理的像素，复用目标裁剪/清理。不是默认空Run回调，也没有创建新的ABI。

仍仅支持PBGRA32目标、PBGRA32/BGR32源和整数平移1:1；缩放、非整数覆盖、复杂Visual状态明确E_NOTIMPL。目标适配每图元分配/复制，尚有性能差集。根/图像列表保活不等同完整跨目标快照，重入执行未取得真实DLL验收。

## 测试与构建

在现有3个Composition参数化用例中增加半透明source-over准确像素、保留目标原像素、负偏移与目标边界裁剪、关闭未Commit批次不被Present执行的断言。先对修改前DLL运行：定向3/3通过，为行为基线，不冒称Red。新DLL全量113通过/0失败/0跳过；用例数未增加。Release/win-x64生产build成功。

## 发布流水

### 发布1：基线

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

项目Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64。工具返回成功，日志11行。产物`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。基线全量113/113，随后扩展断言定向3/3通过。测试TRX记录SHA256 `C69A33ECE324372075EB873B35EC0F004CE156A39EAE5F2CADA023532A4197EF`。TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`已被后续回归覆盖，没有独立保留副本。

### 发布2：生产接线后

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

同一项目、配置和RID。工具返回成功，日志60行，未另存完整诊断。产物同上。全量113通过/0失败/0跳过；TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`记录SHA256 `B4669D001D4B4D5159A02B489F048222883FCD7A0BA964B7C27AEA1796B7A2D8`。测试程序集Debug/net10.0，直接加载Release/win-x64生产AOT DLL。

## 下一轮与差集

没有外部技术阻塞，完整模块未完成，不降低原完成条件。下一轮保持完整模块：跨目标/通道重入生命周期；软件管线上的变换/采样/格式转换；原生组透明度/裁剪生命周期；多目标多帧像素和失败释放验收；AOT及文档收口。详见current-work-item.md。

阶段4仍Active，不能替换wpfgfx。Ledger只登记本轮窄范围证据，不升级完整文件/阶段状态。
