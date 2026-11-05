# S-20260930-123：非均匀源验收与分数裁剪

内部DrawBitmap测试源改为左红右绿，平移仍采左红，右侧源子区域仅绘右绿，清除SourceRect恢复红绿，区分采错位置/错误重映射。直接CopyPixels断言精确像素，无需截图分析。

增加分数aliased clip [0.6,0,1.6,1]，旧DLL到此返回E_NOTIMPL，先前红绿源区域断言通过。生产复用已有ClipCoordinate的28.4舍入（rtutils.h:108-124），分数边界转换后由软件目标与目标范围相交。新DLL仅右绿准确像素通过；全量回归成功。其他DrawBitmap缺口未关闭。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取，中间Red未独立保存。

未完成：Aliased图元覆盖、自定义预滤阈值、effects、浮点目标等；原生上下文差分仍缺，不报告DrawBitmap完整完成。
