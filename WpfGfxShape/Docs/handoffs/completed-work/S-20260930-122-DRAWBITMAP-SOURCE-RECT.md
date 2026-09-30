# S-20260930-122：内部DrawBitmap源区域

按swrast.cpp:239-272，SourceRect限定源/局部几何覆盖，采样仍用WorldToDevice。NativeRenderState.Options.SourceRectValid接入不可变SourceCoverage，形状与采样矩形分离，不将子矩形重新拉伸。空区域不绘制；清除标志恢复全源。

真实内部槽8用例增加右侧子区域、空宽度、恢复全源准确像素；旧DLL失败，新DLL全量通过。当前同色源用例未独立证明非均匀图案采样，仿射/越界Extend组合仍需补矩阵。没有宣称DrawBitmap完整完成。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功；产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取，中间Red未独立保存。

剩余DrawBitmap状态：非8x8抗锯齿、分数aliased clip、自定义预滤阈值、effects、其他插值/合成及浮点目标未闭合。原生上下文布局差分仍缺，GenericTarget尚未通过内部DrawPath消费。
