# 原版校准：裁剪先于限幅

## 修复与预期校正

移除 GeneratedVisualRenderer.DrawingSnapshot.TryCapture 在 CropTileAxis 前的 AdjustSurfaceSize 调用，保留裁剪后最终二维限幅。原生证据为 core/resources/ViewportAlignedIntermediateRealizer.cpp 677–755 的未限幅整数网格/半像素边界，以及620–665先完成逐轴裁剪再调用AdjustSurfaceSizeAndMapping1D。原版实测独立印证此顺序。

sourceMode25原始4096网格，viewport原点-2046，像素中心1.5/2.5对应2047.5/2048.5，分别在内容半区内/外。局部裁剪面不再提前降到2048，因此旧0.75/0.25权重改为1/0。HDR预乘色继续按输入1.25×0.5=0.625（mask consumer为0.5）推导，不从8位结果反推；Vector4距离<1e-7保持。原版两点Pbgra32为[0,0,128,128]/[0,0,0,0]。

## 本轮发布1

完整命令：dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64

目标：Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64。
退出码0；工具返回成功及59行日志。本轮未在日志被测试覆盖前保存完整诊断，无法据此声明无警告。
产物：WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
SHA256：3A5A375270B9DD5AEFE7F8F85550CDC41ADA3C843E3496E38D72DF880106C54C。

发布后定向原版135组对比通过；首次全量暴露旧sourceMode25权重断言失败，未改生产去迎合旧值。校正上述独立推导后，全量891/891通过、0失败0跳过，日志持续时间1秒。
TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。
Run ID：ae65cf34-9d44-4587-b400-0c464eb8309a。
报告：Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-3408a26eba1e44ae8c6139cf8bb48a02.json，Compared135、Mismatches0，对应本轮发布1。
原版DLL SHA256：501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A。
固定原版输入文件SHA256：A5C1B1D8ED17893D29B0F0CE98A9711B82C334BD58FD95CA74B7A987F108CB67。

## 边界与下一闭环

135组只对比(1,1)/(2,2)的Pbgra32投影，不代表全图、全部参数或原生HDR精确读回一致。既有释放/回调/失败恢复测试全量通过，但完整intermediate原生方法族仍未关闭。

下一步扩展共享参数描述与原版同格式全图采样，覆盖负viewbox、旋转/剪切、透明padding和跨tile过滤，针对每个差异联合修复映射、消费者及生命周期；不得重新引入裁剪前限幅或根据AOT实测回填oracle。
