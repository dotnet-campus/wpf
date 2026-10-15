# S-20261009-226：原始曲线快照与变换联合生命周期

## 范围与证据

本归档收口连续会话中未完成的曲线快照修改及验收，不宣称原批准几何大模块全部关闭。

原生证据：`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/drawingcontext.cpp` 945–1024。GetStrokeBounds 在 world 空间求边界，按 WorldToDevice 最大放大率调整容差；dash 使用 solid pen 保持画刷边界稳定。此次原生读取不超过400行，未修改原WPF源码。

## 生产修改与继承内容

- SoftwareStrokeContours 在原生快照模式保存原始控制点与端点，不先产生一份局部扁平化曲线。
- SoftwareCurvedStrokeContours 的 ellipse/rounded rectangle 增加内部 preserveCurves 模式；SoftwareGroupStrokeContours 为原生快照启用该模式，普通轮廓消费者保持原算法。
- CombinedGeometry 仍在实际布尔轮廓构建前显式扁平化，不把控制点当作布尔多边形。
- 继承未收口实现包括 GeneratedNativeStroke 的实线pen局部边界、独立设备容差、NativeShapeReader 原始控制点设备误差消费，以及矩形四角仿射包围盒。最终DLL包含这些实现并完成全量回归；不把其全部归为本次新实现。

## 真实COM验收与根因

唯一 WpfGfxShape.ComAcceptance 项目直接加载生产Native AOT DLL，未新增Host、托管生产引用、测试导出或fake COM。

起始593通过/8失败/601项。ImageBrush Transform句柄误写于偏移24（Viewport.Y）；修正到协议偏移100，保留原像素断言，定向179/179通过。生产快照修改后全量601/601通过。

新增12项非均匀缩放/旋转联合opacity mask、group、目标外、fill首错验收，首次182通过/9失败/191项。遮罩夹具原先没有和画刷应用相同逆变换，现更新真实遮罩资源报文；目标外位移20不足以越过宽pen及miter边界，改为100，仍断言零解码回调和零像素。修正后191项定向通过；该定向运行的TRX已被最终全量覆盖，未独立保存。

保留回调替换/关闭、primitive与private重入HRESULT、fill首错、失败重试、owner保活及stroke→fill释放验收。没有修改生产引用计数、Stdcall或释放顺序。

## 发布记录

以下三个发布属于本收口工作链，每次完整命令相同：

`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`

项目：`WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj`；配置Release，RID win-x64。统一产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

1. 起始发布：成功，退出码0，11行日志；其后全量593通过/8失败，425ms。该发布hash与TRX ID未独立保留，不能以最终hash代替。
2. 原始快照修改后发布：成功，59行日志，包含CA1416 Windows平台警告及Generating native code；其后全量601/601。该发布hash/TRX ID未独立保留。
3. 最终夹具修正后发布：成功，退出码0，11行日志，无该次日志中的警告；全量613/613，501ms，零失败/跳过。SHA256 `1EFDED056181CED697D48AC63D3B72A98EEFA0ECA1ED56995DC6D2C0498AE1CF`，TRX ID `08336cd4-374b-41da-9617-8da78aba6e0c`，路径 `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`（相对WpfGfxShape目录）。hash由该TRX中的测试输出确认。

此前更早中断会话的发布日志未在本次保留，不补造其hash或TRX对应关系。

## 未关闭边界

本次关闭原始快照重复预扁平化和已验收的非均匀缩放/旋转联合生命周期；并未证明全部曲线极值、全部退化切线或完整MSVC兼容。真实布尔曲线交点、DrawingImage映射与消费一致性仍需下一生产闭环。完整IShapeData/CPlainPen/Brush/QI/meta、非圆dash/marker、完整层栈、生产ABI/UCE及PresentationCore E2E未闭合。

阶段1–3Closed、阶段4Active、阶段5–7Pending，Ledger RepairRequiredPartial；当前不能替换原wpfgfx_cor3.dll，不报告整体完成百分比。
