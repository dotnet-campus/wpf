# S-20260930-120：内部方法清单与ClearType setter

修改前14项未完整实现：Begin3D、End3D、DrawBitmap、DrawMesh3D、DrawPath、DrawInfinitePath、ComposeEffect、DrawGlyphs、DrawVideo、BeginLayer、EndLayer、EndAndIgnoreAllLayers、SetClearTypeHint、ReadEnabledDisplays。EndLayer仅错误返回、Abort空方法，不算实现。

本轮按BaseSurfRT.h:111-117完成SetClearTypeHint：持久保存bool并返回S_OK。重复QI不重新初始化已存在的内部视图。测试验证启用/禁用返回、重复QI身份及原目标释放后基类可用；没有测试getter，未宣称文字像素已实现，DrawGlyphs仍缺。

旧DLL定向失败，新DLL全量成功。剩余13项保持开放。推进顺序：上下文/DrawBitmap→形状画刷/DrawPath和DrawInfinitePath→层→显示信息；3D/文字/效果/视频另按原合同实现，不默认为软件DrawImage闭环完成。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功；产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取，中间Red未独立保存。

完整模块未完成，无外部工具阻塞。
