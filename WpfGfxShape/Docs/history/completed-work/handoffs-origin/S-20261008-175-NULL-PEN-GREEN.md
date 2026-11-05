# 无画刷 Pen 生产续接与全量验收

## 发布及证据

本轮仅发布一次：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，目标 Code/WpfGfxShape/WpfGfxShape.csproj，Release / win-x64，退出码 0。产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。立即读取全部可用 60 行日志，可见 CA1416、CA1822；原输出 409681 字符只保留末尾 20000 字符，不宣称保存完整原始诊断。详见 S-20261008-172-NULL-PEN-INITIAL.md。

- 初始全量：284 通过、1 失败、0 跳过，325ms，TRX fad321e4-2a05-4a9c-afb8-05059db11b20。
- 定向失败复现：74 通过、1 失败、106ms，TRX 3efbe629-6303-4286-b360-09b393ca9952；诊断见 S-20261008-173-NULL-PEN-DIAGNOSIS.md。
- 修改后定向：76/76、0 失败/跳过，101ms，TRX 377cea53-4a66-422e-bebc-8e028bd3c539；覆盖前摘要见 S-20261008-174-NULL-PEN-DIRECTED.md。
- 最终全量：286/286、0 失败/跳过，301ms，退出码 0。
- 最终 TRX：eaa419d9-e384-47d1-b3e1-3bb5c00e92aa，start 2026-10-08T19:19:46.8218214+08:00；默认路径 Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。
- 最终 TRX 核实 DLL SHA256：06D06BAEE8977E032659F89FA27BAE09FA093FA84A21A8520667E751277F87AC；均对应本轮发布 #1。
- 定向重新编译测试可见既有 MSTEST0044、MSTEST0037；最终全量增量构建无新可见分析器警告。没有执行覆盖率、原生差分、PresentationCore E2E 或 Ledger 修复验证。

历史 TRX 被默认测试路径覆盖前保存上述独立摘要；不是完整原始 TRX/日志副本。最终 TRX 当前保留默认路径。

## 生产和测试边界

前轮未发布修改已随本轮发布验证：GeneratedPenResource 保留 thickness/miter/cap/join、brush、厚度动画和 dash style；GeneratedVisualRenderer 与 GeneratedImageContentBounds 不再一律拒绝非空 Pen，仅拒绝画刷非空的 Pen。

本轮没有再修改生产代码。ImageMask.cs 新增 Pen 相对 viewport 场景误以不透明红色作为预期，实际其参数和已有无 Pen 的 0x40400000 场景相同；填充与 Visual 遮罩各约半 alpha。修正为相同预期，并增加绝对 viewport 的不透明红色对照，不改变生产颜色/采样逻辑。实色作用域 Pen 场景及源锁失败、层丢弃、解锁恢复、空/NaN 映射与下一帧恢复一并通过。

原生证据重新核实：WpfGfx/core/resources/pen.cpp:30-110，core/uce/drawingcontext.cpp:886-994、1009-1138。DrawShape 先填充再获取 Pen/nullable stroke brush；GetPen 保留完整状态及动画厚度。此 generated 状态/窄消费不等同于私有 Pen realization ABI。

未新增导出、Host、slnx 或依赖，未改原 WPF。保持已有 Stdcall、HRESULT 首错、快照持有与逆序释放。

## 未关闭差集及下一轮

完整 ImageBrush 遮罩映射、层消费与 Compose 生命周期模块仍未关闭。无画刷 Pen 回归不是完整描边实现：有画刷 Pen/Widen、真实 resource/null/immediate realizer 创建者与完整 finite/infinite 私有消费者、MSVC 虚析构和内嵌 LocalMILObject 布局仍待实现。不得用 COM 三槽或 resource handle 冒充非 IUnknown 私有对象。

下一轮继续同一完整模块：合并真实创建者及全部参数消费、空 realized brush 与 effects 失败、fill→Widen→stroke、Begin/End/Abort 与 Pop 首错清理、精确一般曲线布尔区域及映射动画、Compose 重入/帧快照/调用者释放/恢复/最终释放，最后一次联合发布与直接 AOT 验收并归档 Ledger 差集。已完成的无画刷 Pen、Union、NaN 映射等仅作为回归，不另立纯复验或补测轮。

阶段 1–3 已关闭，阶段 4 Active，阶段 5–7 Pending；Ledger RepairRequiredPartial，无已确认外部阻塞，不能替换原 wpfgfx_cor3.dll。不报告整体完成百分比。
