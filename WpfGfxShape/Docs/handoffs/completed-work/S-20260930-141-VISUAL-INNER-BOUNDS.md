# S-20260930-141：Visual自身内边界与子节点外边界

原生RenderDataBounder.cpp:259-296明确GetVisualInnerBounds为内容并集子节点outer bounds；drawingcontext.cpp:5012使用该边界定位透明遮罩，同时另传自身Clip。precompctx.cpp:552起对子节点outer bounds先clip再transform。

因此没有沿用最初“逐图元继承所有Visual clip”的假设。GeneratedImageContentBounds忽略当前Visual自身Clip来求inner bounds；对子节点独立求inner、矩形clip求交、变换/偏移后合并。未知/复杂clip仍失败，不用目标边界兜底。

真实相对viewport测试新增自身Clip，仅右半内容可见，但遮罩仍按完整宽2定位，准确右红而非半红；旧DLL失败，新DLL全量通过。子节点clip+变换组合未独立新增用例。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64 build/publish成功；产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取，中间Red未独立保存。

下一步：独立验证子Visual裁剪、偏移、嵌套变换的父级mask边界；其后解决render-data旋转图元/clip边界，不将第1步提前关闭。整个五步计划未完成。
