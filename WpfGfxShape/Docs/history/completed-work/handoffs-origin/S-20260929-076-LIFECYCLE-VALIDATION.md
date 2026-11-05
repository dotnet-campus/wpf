# S-20260929-076：生命周期验证与验收边界（未完成生产模块）

## 本轮事实

仅修改ComAcceptance测试及文档，未修改生产代码。新增三个参数化生命周期场景：生产EventProxy作为GenericTarget引用所有权载体，重绑定释放旧对象触发真实dispose回调，分别重入DestroyChannel、Disconnect和SameThreadPresent，断言回调结果、旧对象恰好释放一次、新对象引用清理。EventProxy没有被当作可绘制目标；无root的Present不证明图像渲染重入。

现有三个Composition场景增加共享partition双目标消费者：DuplicateHandle共享Visual，第二目标真实位图保持写锁，第一个目标像素已经绘制、第二目标锁失败首错返回；解除锁后重试像素正确；显式删除目标、销毁共享通道后输出位图仍可读。全部直接Green，不是Red→Green。

## 原生依据与边界

`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/rendertargetmanager.cpp:325-398,406-450`：顺序Render、错误分类、首个不可处理错误退出；未发现可据以声称全局不可变帧快照的契约。旧printtarget ProcessCreate引用替换约束继续适用。

已读生产BitmapRenderTargetExports、DoubleBufferedBitmapExports、GeneratedVisualRenderer、GeneratedTargetResource和SameThreadChannel：当前库自产位图目标/双缓冲源没有用户绘制回调入口。可达用户回调是目标重绑定释放EventProxy时的dispose，发生于Commit，不是活动Render内部。不得用假COM或测试getter替代真实消费者，也不得把本轮生命周期通过称为绘制期间重入已经关闭。外部目标/源通用消费链仍是生产依赖，不是外部环境阻塞；待其原生契约和真实接线完成后合并活动绘制重入验收。

## 发布流水

发布1完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

目标生产项目Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，工具成功，日志11行；产物`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。未修改生产，无第二次发布。先运行基线成功，两个重入场景定向通过，中间全量115/115；补第三重入场景后最终116通过/0失败/0跳过。测试程序集Debug/net10.0加载上述原生DLL。

TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`（各次测试覆盖同一路径）；本轮读取的115项TRX记录SHA256 `90812A9279200F1A32B7E77550E838ADC8A4B1A4CD64502999A51259E7F545F9`；之后116项回归使用同一发布且未再发布。未独立归档中间TRX。

## 未完成与下一轮

本轮只完成验证，未交付用户要求的生产大模块；不将纯补测视为完整切片关闭，也不以无回调入口为理由无限补测。当前工作计划保留完整模块条件，下一轮优先实施已有明确合同的通用绘制生产缺口，外部COM消费者与活动Render重入一并闭合；不能只再补一组回归。阶段4仍Active，不能替换wpfgfx。
