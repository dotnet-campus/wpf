# S-20261009-229：DrawingImage仿射组合与资源画刷生命周期

## 生产修改

继承S-228的GeometryDrawing/DrawingGroup→共同指令→DrawingImage目的矩形映射生产实现。本轮补充未更新GeometryDrawing为空绘图、DrawingImage非有限目的矩形及非有限组合映射的早期InvalidArgument保护，不拒绝有限的退化Visual矩阵。

新增六项真实DrawingImage入口资源ImageBrush生命周期验收：present重入、源替换、关闭channel/connection并释放target/factory、private第10槽重入、目标外零解码、fill锁失败首错与解除锁后重试。原像素、回调次数及HRESULT断言保持不变。新增四项相交布尔曲线DrawingImage的非均匀缩放/90度旋转和DrawingGroup实色遮罩组合像素验收。

现有快照资源由捕获外层finally逆序释放，primitive保持stroke→fill释放；绘制失败abort layers。未新增fake COM、测试导出或托管生产引用。

## 验证

起始656项全量通过；新增六项生命周期和四项仿射组合定向均通过；最终666/666，零失败/跳过，日志持续时间1s。真实曲线输出仍是扁平化多边形，不声称原生曲线恢复或完整拓扑精度。

本次没有独立新增原生方法翻译；沿用S-228 drawingimage.cpp 25–69证据。目的矩形映射及group完整原生合同仍需核对，不能把像素通过等同完整MSVC ABI。

## 发布记录

两次发布完整命令均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。

项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 起始发布成功，11行日志，随后656项全量通过；该次hash/TRX ID未独立保留，S-228已保留此前656项基线。
2. 有限坐标与空drawing保护后发布成功，60行日志，包含CA1416平台警告；最终666/666通过。SHA256 `FB37F2C7A1B56042EC22B784A819023F3619E2C579EF079FB53593B88BA92A92`，TRX ID `20f31bc4-4671-48ac-8132-a2412f83354b`。来自WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx的真实测试输出。定向文件被全量覆盖，未保留独立ID。

## 未关闭事项

DrawingImage作为ImageBrush/opacity mask source仍未实现；GeneratedImageMask.TryCapture仅接受bitmap source，是明确生产分支缺口。需要完整中间面实现、映射/tile/opacity、循环检测、回调失败与确定性释放，不得仅增加测试或空实现掩盖它。此前请求的大模块在该项关闭前仍未完全完成。

阶段1–3Closed、阶段4Active、阶段5–7Pending；Ledger RepairRequiredPartial。当前不能替换原库。
