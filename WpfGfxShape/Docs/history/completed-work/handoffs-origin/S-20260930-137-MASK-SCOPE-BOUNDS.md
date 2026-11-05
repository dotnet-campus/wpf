# S-20260930-137：PushOpacityMask作用域边界

依据renderdata.cpp:851-884，bounds字段在bounding pass由作用域计算，不直接信任输入缓存。新增TryGetMaskScope，从Push后一条至匹配Pop累计本地DrawImage边界，处理嵌套变换/透明层，作用域外图元不参与；相对viewport/RelativeTransform经同一ImageMask映射入口使用该边界。clip及其他不支持指令仍明确失败。

新增真实相对viewport用例：作用域内x=1宽1，Pop后x=0宽1。输出左不透明红、右半透明红，区别于使用整个Visual宽2或信任零缓存字段的错误实现。旧DLL失败，新DLL全量通过。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64 build/publish成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取，中间Red未单独保留。

计划第1项部分完成，含clip内容边界、空内容、平铺及后续生命周期/层位图接线仍未闭合。没有宣称全部计划完成。
