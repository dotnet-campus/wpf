# S-20260930-138：空ImageBrush遮罩语义

依据TileBrushUtils.cpp:198-253，空viewport/viewbox不绘制。GeneratedImageMask区分未知内容边界（失败）与已知空边界（成功空结果），CaptureMaskLayer将成功空结果转为alpha=0遮罩层，而非取消mask导致内容外泄。映射不支持仍失败。

8项ImageMask参数用例新增viewport宽0及恢复，旧DLL失败，新DLL指定ComAcceptance全量163通过、0失败、0跳过，282ms。空viewbox/空内容已接相同分支，尚未独立像素测试。

## 发布与验证

发布1命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`，成功。
产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
曾误用未指定目标的测试，触发原WPF工程构建并因缺fxc.exe失败；随后按唯一ComAcceptance csproj测试成功。该误选目标不属于本模块环境阻塞。未修改原WPF源码。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取。

计划未全部完成：带clip边界、tile、回调释放及完整层位图接线仍开放。
