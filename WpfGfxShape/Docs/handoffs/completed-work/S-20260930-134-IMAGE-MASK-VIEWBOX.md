# S-20260930-134：ImageBrush相对viewbox与DPI

GeneratedImageMask保留ViewboxUnits标志，在源尺寸/DPI可用时将相对viewbox按源内容DIP尺寸转换，再走已有Fill映射。相对viewport仍拒绝，不能用目标尺寸代替Visual内容边界。

真实ImageMask测试参数化绝对/相对viewbox与96/192DPI，四项均保留非均匀alpha准确像素、源写锁失败父目标不变和调用方源释放后重试。旧DLL相对模式失败；新DLL全量成功。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取，中间Red未独立保存。

方案C仍未完成：相对viewport/其他stretch/tile/RelativeTransform及完整回调释放矩阵保持开放。
