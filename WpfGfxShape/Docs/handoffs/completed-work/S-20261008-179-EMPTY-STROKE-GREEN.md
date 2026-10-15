# S-20261008-179：空笔画合同修复Green

## 实现与原生依据

本轮生产修改GeneratedDrawingResources、GeneratedVisualRenderer、GeneratedImageContentBounds，统一Brush非空且原生float厚度非零的拒绝判定。厚度动画先覆盖inline，再转float；零及float下溢不再错误拒绝填充。沿用既有依赖所有权和帧快照，不增加导出或伪造私有对象。原生证据与Red见176/177归档。

新增四个真实52字节Pen协议场景：ImageBrush零厚度、1e-100转float零、作用域实色零厚度、inline2被真实DoubleResource零动画覆盖。测试联合WIC锁失败首错、层丢弃、解锁后下一帧像素恢复、空viewport及NaN映射透明恢复、最终释放。

## 发布与验证绑定

- 发布#1：完整命令、成功结果、初始286/286和SHA/TRX见176。
- 发布#2：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，Release/win-x64，退出码0；原输出409681字符截断，可见CA1416及native生成成功，诊断取得边界见177。
- 产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- 发布#2定向80/80、102ms，TRX `aa41bbd9-9be4-4a63-a1b8-d88b4bd262c4`，覆盖前摘要见178。
- 同一发布#2全量290/290，0失败/跳过，306ms；TRX `0f9a8253-1f7c-4d4e-a79c-d887eeee531e`，开始 `2026-10-08T19:26:22.2137655+08:00`。
- 全量实测DLL SHA256 `A83BA3EF26B7CBF289B2674AF2E9A16D06662C40DD7B03746F65F76EF01C4BB7`。
- 最终TRX仍在默认 `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`；176–178为覆盖前摘要，不是完整原始TRX/日志备份。

## 未完成边界与下一轮

本轮只交付完整模块内部的空笔画合同修复，未完成原先要求的整个生产大模块；不能将本次Green表述为完整模块交付。非零有画刷Pen仍E_NOTIMPL，真实resource/null/immediate私有创建者、MSVC虚析构布局、完整finite/infinite消费者、EnsureRealization/effects失败传播、一般曲线精确布尔bounds仍有缺口。当前bool折叠不能视为私有路径ABI翻译完成。没有已确认外部阻塞；本次缩小交付不是外部阻塞导致，后续必须重新合并完整闭环，不默认继续补测切片。

下一轮完整模块计划：真实创建者/布局/所有权 → 完整路径参数及fill/Widen/stroke → EnsureRealization/effects与层首错逆序清理 → 精确区域及映射 → Compose重入/删除替换/帧保活/失败恢复/最终释放联合验收 → 逐次发布诊断和覆盖前证据归档并同步Ledger实际差集。全部完成条件满足才关闭。

阶段1–3关闭、4 Active、5–7 Pending，Ledger RepairRequiredPartial，不能替换原DLL。未运行覆盖率、原生差分、PresentationCore E2E或Ledger修复验证。
