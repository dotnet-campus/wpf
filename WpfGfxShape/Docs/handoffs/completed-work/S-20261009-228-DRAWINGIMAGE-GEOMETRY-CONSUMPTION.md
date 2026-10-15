# S-20261009-228：DrawingImage几何与DrawingGroup共同消费接线

## 生产推进

原先Visual DrawImage只接受BitmapSource/DoubleBufferedBitmap，DrawingImage直接E_NOTIMPL。现在新增GeneratedDrawingInstructions，把GeometryDrawing与DrawingGroup转成现有共同消费指令；没有创建独立renderer或测试导出。

Drawing资源保留nullable slots及group opacity；group遍历保留transform、clip、opacity、opacity mask，检测循环和256深度限制。不支持的guideline、非默认edge/scaling/ClearType及其他drawing类型明确失败，不静默忽略。slots在final release清空；依赖监听清理由基类执行。

GeneratedImageContentBounds对相同drawing指令计算内容bounds；Visual将内容bounds仿射映射到DrawImage目的矩形，继续调用原始shape/pen、画刷realizer及layer消费。空内容不产生绘制。捕获期间失败的画刷/遮罩快照仍由外层finally逆序释放，实际绘制首错仍abort layers。

## 原生证据

`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/drawingimage.cpp` 25–69：GetBounds委托ContentBounder.GetContentBounds，Draw委托Drawing.Draw。此次读取小于400行，未修改原WPF源码。目的矩形映射及完整DrawingGroup细节尚需继续核对原生调用者，不宣称完整直译已完成。

## 验收与边界

唯一COM项目直接加载生产Native AOT DLL。新增8项GeometryDrawing→DrawingImage→DrawImage真实报文像素验收，覆盖Union/Intersect正常映射、重合Xor/Exclude空结果、1e-8/1e8坐标尺度及DrawingGroup实色opacity mask。全部通过；最终全量656/656，零失败/跳过，日志持续时间1s。

本轮不宣称整个请求完成：资源ImageBrush回调失败/关闭/retry的DrawingImage联合验收、非均匀缩放/旋转、DrawingImage作为ImageBrush source，以及group完整状态仍未闭合。现有bitmap生命周期全量通过不等同DrawingImage独立生命周期证明。几何输出仍是扁平化多边形，不代表曲线恢复或完整MSVC ABI。

## 发布记录

三次发布命令均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。

项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64。产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 起始成功，11行日志，随后648项全量通过；hash/TRX ID未独立保留。
2. 首次DrawingImage接线发布成功，13行日志；四项新增定向通过，随后八项新增用例通过。hash/TRX ID未独立保留。
3. slots final-release与group不支持状态保护后发布成功，60行日志，存在CA1416平台警告；全量656/656通过。SHA256 `CF41C65985B6929F770703646982BAF01D8A531CEF02B6C766770D2FAA304D07`，TRX `bfc2320b-b1a4-4b84-8e87-a3f0a232aa7f`，由Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx输出确认（相对WpfGfxShape目录）。中间TRX被覆盖，不补造其对应关系。

阶段1–3Closed、阶段4Active、阶段5–7Pending，Ledger RepairRequiredPartial；当前不能替换原wpfgfx_cor3.dll。
