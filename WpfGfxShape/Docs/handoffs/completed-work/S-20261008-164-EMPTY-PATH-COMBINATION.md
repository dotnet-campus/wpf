# S-20261008-164：未初始化 PathGeometry 组合填充操作数

## 范围与原生证据

这是完整 ImageBrush 遮罩映射、层消费与 Compose 生命周期模块内部的生产修复，不是完整模块交付或新的补测模块。没有确认外部阻塞。

- `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/pathgeometry.cpp:40-76`：未初始化 figures 时 GetShapeDataCore 返回 CShape::EmptyShape，不求值 transform。
- `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/combinedgeometry.cpp:41-135`：取得两个当前形状，执行四种布尔组合，按所有权释放临时形状。
- 现有 SoftwarePathCoverage.TryCreate 对未更新 PathGeometry 返回空覆盖；组合 bounds 却拒绝相同资源，导致真实 Present 返回 E_NOTIMPL。

## 生产修改与验收

`Core/GeneratedGeometryBounds.cs` 的 TryCombinedRectangle 对 Figures.Length 为零的真实 GeneratedPathGeometryResource 返回空填充区域，遵守原生初始化合同。未将一般曲线或非轴保持区域替换为保守 bounds。

`Tests/WpfGfxShape.ComAcceptance/CompositionEffectsTests.ImageMask.cs` 新增四种布尔模式的真实 PathGeometry 资源操作数，不更新 figures；复用直接生产 AOT DLL 的相对遮罩、锁首错、失败层丢弃、来源引用释放、重试、空 viewport 及下一帧恢复链。没有新增测试导出或伪造私有对象；没有修改原 WPF 源码、Host 或解决方案。

新增用例在修复前取得定向 67 通过/4 失败，73ms；四个失败均为 Present E_NOTIMPL（0x80004001）。修复并重新发布后定向 71/71、68ms；全量 279/279、0 失败/跳过、257ms。

## 逐次发布与证据

两次均执行：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。

目标均为 `Code/WpfGfxShape/WpfGfxShape.csproj`，配置 Release，RID win-x64，产物为 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

1. 发布 #1：成功，工具报告完整日志11行；开始全量275/275、260ms，TRX `ee8ed9cc-11ac-4dfd-91a8-3076159ec885`，开始时间2026-10-08T18:27:34.0453328+08:00。本轮未独立保存发布#1日志或读取对应DLL SHA，不借用历史SHA冒充本次测量。
2. 发布 #2：成功，退出码0。读取日志1-8及54-59行：输出409681字符，前389681字符被截断，只保留末尾20000字符；可见CA1416及Generating native code和最终publish目录，不能声称完整日志或无警告。独立保存可核实摘要于本文。

发布#2定向TRX `13319d3d-74cb-4d02-891e-54efcebeada4`（开始2026-10-08T18:32:13.7392291+08:00），后被全量运行覆盖，未独立保存完整定向TRX。最终全量TRX `73ec8456-a142-4941-af69-e6a775cb1e3a`（开始2026-10-08T18:32:28.9659957+08:00），保留于 `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。

全量TRX中的生产DLL SHA256：`769340DCA97902F559DB372C3A19F16967E20E691745D914985F12972FE7BB30`，对应发布#2。初始和Red完整TRX未独立保存。Red编译诊断可见既有MSTEST0037/MSTEST0044。

## 未关闭边界

私有 MSVC shape/pen/context/realizer 创建者合同、非空有限/无限fill/stroke共同扫描、Widen、一般曲线组合精确bounds、非轴保持操作数、实色浮点专门验收和原生差分仍未完成。未运行覆盖率、PresentationCore E2E、Ledger修复或验证。阶段4 Active，Ledger RepairRequiredPartial，不能替换原DLL。

下一轮仍交付同一个完整生产模块，不按空路径操作数或补测单立小轮；具体合并计划维护于current-work-item.md。
