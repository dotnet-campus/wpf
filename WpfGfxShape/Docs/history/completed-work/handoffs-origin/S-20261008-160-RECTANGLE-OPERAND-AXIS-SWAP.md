# S-20261008-160：矩形组合操作数轴交换合同修复

## 生产差集和证据

原生证据：`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/combinedgeometry.cpp:1-135`，GetShapeDataCore 取得实际几何当前值并调用 CShapeBase::Combine，空操作数为 EmptyShape，外层矩阵作用于组合；未修改原生代码。

`Core/GeneratedGeometryBounds.cs` 的 TryCombinedRectangle 原先拒绝所有非对角操作数矩阵，而同文件递归组合分支允许轴交换。现在统一为对角缩放或纯轴交换；两类变换均保持矩形区域表示，仍拒绝剪切/一般旋转，不以包围框冒充布尔区域。

`Tests/WpfGfxShape.ComAcceptance/CompositionEffectsTests.ImageMask.cs` 新增四种布尔模式的直接矩形操作数轴交换。矩阵将既有矩形映射回相同区域，测试经真实 channel/Present、相对 PushOpacityMask/clip/Pop 消费；复用来源写锁失败、重试、空 viewport 与恢复断言。没有新增导出、Host、托管生产引用或方案修改。

没有修改 COM ABI、引用计数、HRESULT 首错或清理顺序；这只是现有生产模块的内部修复，不是完整模块交付。

## 发布记录

本轮两次均执行完整命令：

`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

项目为 `Code/WpfGfxShape/WpfGfxShape.csproj`，配置 Release，RID win-x64；产物均为 `artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

1. 恢复发布：成功，发布工具返回 11 行日志。随后全量 264/264、0 失败/跳过、349ms。未在测试覆盖前独立读取本次 SHA/TRX ID；不把上轮 SHA 当成本次证据。原始发布日志未独立保留，不能声称本次零警告。
2. 修复后发布：成功，退出码 0，59 行日志，已检查到 CA1416 平台分析警告；产物路径已核实。定向 60/60、72ms，TRX `692cddd9-0320-455f-8552-58e7d35c7744`；最终全量 268/268、0 失败/跳过、276ms，TRX `7aaa1b6e-c65f-4c7c-9e45-27d5fe6c9fdd`。全量 TRX 中实际加载 DLL 的 SHA256 为 `40C5F94FDE90ED3E93C09E3776C873A467AA09FB77F1285667FA746556422781`，对应发布 2。

## TDD 验收

新增测试先运行恢复 DLL：既有 56 项通过，新增 4 项均在 Present 返回 E_NOTIMPL（0x80004001），定向共 60 项、72ms。保持测试断言不变，修复并重新发布后 60 项全部通过；四项同断言 Red→Green。

测试重编译记录既有 MSTEST0037/MSTEST0044 警告。TRX 固定路径 `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx` 会被覆盖；最终全量文件当前保留，恢复/Red/定向完整历史 TRX 和逐次发布原始日志未独立保存。以上 ID、数值和诊断为本轮读取所得，不宣称已建立不可变原始证据集。

## 未关闭边界和下一轮

仍缺非空原生 IShapeData/IFigureData、CBrushRealizer/CMILBrush、CPlainPen/CContextState 私有对象生产创建与共同扫描消费者、一般曲线组合及非轴保持操作数、完整层生命周期及 Compose 联合消费。没有确认外部阻塞；不以这次小修复缩小原模块完成条件。

下一轮保持完整 ImageBrush 遮罩映射、层消费与 Compose 生命周期模块，执行步骤以 `../current-work-item.md` 为唯一权威；矩形轴交换作为回归，不再领取补测小轮。

阶段 1–3 正式关闭，阶段 4 Active，阶段 5–7 Pending；Ledger 仍 RepairRequiredPartial。未修复/验证 Ledger，未执行覆盖率、原生差分或 PresentationCore E2E；不能替换原 DLL。
