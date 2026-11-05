# S-20260930-119：内部creator实际创建位图

实现内部视图第二基类CreateRenderTargetBitmap，复用抽出的CreateBitmapTarget生产分配/失败清理，不伪装空成功。按SwIntermediateRTCreator.cpp:47-99限制最大尺寸2^24，使用父目标格式，96DPI；当前两种目标格式PBGRA32/PRGBA128Float已是混合格式。软件原生忽略dwFlags/activeDisplays，当前保持。ReadEnabledDisplays仍未实现。

真实DLL新增两项（usage=0/ForBlending）：通过第二基类调用创建、父目标/工厂释放后中间目标独立存活、96DPI、Clear绿色像素及目标释放后位图保活。旧DLL失败，新DLL全量138通过、0失败、0跳过，273ms。浮点中间目标、尺寸边界和跨架构尚未独立验收。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取，中间Red未独立保存。

第1项内部接口方法族部分推进；DrawPath/DrawBitmap/层和原生上下文仍未实现，不声称完整模块完成。
