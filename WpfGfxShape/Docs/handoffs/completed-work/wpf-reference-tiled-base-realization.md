# 普通重复位图base tile消费者直连

恢复上一轮候选时改用tileSourceId，避免nested声明sourceId与后续同名local的C#作用域冲突。Release/win-x64 build成功62行；旧失败日志不足以追溯正式根因，不能宣称旧CS0136已日志确认。

普通MIL位图、无VisibleTiles的direct资格扩展至重复tile；AddTiledImageMask生成base tile后QI取得MIL source，直接传tile-to-device映射及FlipX/Y/XY/Tile wrap到NativeImmediateSolidRealization，而非预采样整面repeated。CreateBitmap支持已有wrap字段，source clip为null（重复模式不裁剪单tile）。引用由realization AddRef，tile/QI局部逆序finally释放；候选未提交释放保留。BeginPrimitive/DrawBrush资格同步保留源scaling。opacity mask、DrawingImage与partitioned原路径保持。

原生证据tilebrush.cpp358–387 SetBitmap(pBaseTile)、SetBitmapToXSpaceTransform、SetWrapMode、source clip、opacity。该接线修正realization结构，但不是完整原生realizer或ABI关闭。

## 发布1

完整命令：dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64
目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，退出码0；完整13行日志已读取，含Generating native code、lib/exp、固定publish目录。本次增量发布日志未输出警告，之前build仍有CA1416，不宣称项目无警告。
产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
SHA2561D2DA989730B403D06AE2C7A6A9DD9EF2D2BA3DB6D2EE6DC640A293967BEDB96。
全量896通过1失败0跳过，总897，2秒。普通BitmapSource180组仍64不同、DrawingImage450组仍86不同，实色180组一致；没有减少差异，不能宣布tile过滤修复完成。唯一失败严格校准，非新稳定基线。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，Run IDf200f0d4-a614-4390-b821-1e4771871c18。
报告Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-68fccd2231e04ebaa220dc661d653bf7.json对应发布1。

下一闭环聚焦实际base tile生成与固定点span，按原版单差异反算透明padding/源clip/旋转样点，对消费者模式1/2/3及mask分别处理。现有生命周期测试通过不代表新增tile路径所有callback覆盖完成，模块仍未关闭。HDR/1e10/1e-7预期未放宽。
