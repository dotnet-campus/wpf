# S-20260930-143：旋转图元与裁剪的保守内容边界

核对boundsrt.cpp:726-758 AddBounds：将变换后图元包围矩形与aliased clip矩形求交，再union；边界阶段不做精确光栅几何交集。删除GeneratedImageContentBounds对旋转/斜切图像在clip下的过严拒绝，不改实际SoftwareImageCoverage。

新增真实mask作用域内PushClip→PushTransform(90度)→DrawImage→Pop→Pop→Pop及作用域外图元测试，保留两组画刷变换。旧DLL失败，新DLL全量通过。90度用例不代表斜切数值差分全部覆盖。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取，中间Red未单独保存。

计划第1步继续开放：裁剪几何自身旋转及复杂几何bounds仍需补齐。下一步先复用当前支持几何的安全边界计算，而非继续要求边界遍历精确多边形裁剪；之后推进平铺方法族。
