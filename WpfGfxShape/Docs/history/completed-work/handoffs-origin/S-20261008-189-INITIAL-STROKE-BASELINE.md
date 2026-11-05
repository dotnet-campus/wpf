# S-20261008-189：续接笔画基线（未关闭完整模块）

## 发布与覆盖前测试证据

- 本轮发布 #1：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，生产项目、Release、win-x64，成功；工具报告完整日志 11 行。测试前没有读取发布日志，随后被测试日志替换，完整发布诊断未保存，不声称已归档完整发布日志。
- 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- 唯一 ComAcceptance csproj 直接生产 AOT 全量：294 通过、1 失败、0 跳过，共 295，322ms。失败为 `WhenImageMaskSourceIsLockedThenLayerIsDiscardedAndRetryPreservesMapping` 的 96DPI、0.5 厚度、作用域实色、仅笔画参数行；预期第二像素 `0x40400000`，实际 `0x9f9f0000`。
- TRX：`90263279-71e5-432c-81ff-9cf68856f1c0`；开始/结束：`2026-10-08T19:59:10.4552946+08:00` / `2026-10-08T19:59:11.1194273+08:00`。
- 该 TRX 记录产物 SHA256：`227610A11B1C733EEC1165A5A6C002386FC688A1D37F94031FB37E47BE1DE78D`。记录来自覆盖前读取的 TRX 第 1–12 行，不是独立保存的完整 TRX 副本。
- 测试编译可见既有 MSTEST0044、MSTEST0037 警告；测试日志共 37 行，已完整读取。没有新增 Host、导出或测试项目。

## 续接边界

工作区已含前次未归档的矩形笔画生产代码及测试；其发布与测试不能归入本轮发布 #1。当前基线不是 Green。需要核实内孔样本合同和 fill→Widen 顺序，随后继续同一个 ImageBrush 遮罩、层消费与 Compose 生命周期模块，不视为已关闭一般 Widen 或 private ABI。
