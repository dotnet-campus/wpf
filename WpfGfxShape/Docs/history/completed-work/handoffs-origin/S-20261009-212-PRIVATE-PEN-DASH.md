# S-20261009-212：纯C# private笔Dash快照与marker保护（模块未关闭）

NativeStrokeReader按Release/x64 CPlainPen尾部读取style/offset、DynArray<REAL>数据/容量/计数并复制到托管快照，接入共同Dash/Widen消费；不再拒绝所有non-solid笔。读取marker类型/指针并明确拒绝尚未翻译的marker，避免静默丢失。保持纯C#，不新增C++或MSVC输出对象。

原生证据geometry/cpen.h:550–576字段顺序，include/Generated/wgx_misc.h:878–892 style；DynArray布局证据沿用S-209。新增Custom[10,1]边界笔测试执行真实private target，覆盖锁首错、Ensure/Effects失败和恢复。该长dash覆盖完整figure，是接线/所有权验收，不证明dash切点、偏移或非Flat cap差分；shape/pen/realizer仍是边界对象，不是创建者ABI。

## 发布与验证

两次完整命令均`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，目标同路径，Release/win-x64。统一产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 成功11行日志未读取，工具成功可核实，启动测试成功但统计/身份/SHA未读取。新增3项private dash旧DLL失败58行，完整错误汇总未读取，不编造Red细节。
2. 成功60行日志未读取，完整诊断未保存。定向241/241、190ms，TRX01c16f2d-a537-4a99-8a68-3adc1606b3ef；全量362/362、408ms、0失败/跳过，TRX68d4e1c2-efe4-4a8f-ae76-204a3e4f3815；SHA9469817672E88145600BA52FBA45F3BB106EC0450400E019CFB6CE08ED11EB60。

测试项目Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，定向筛选FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~BitmapRenderTargetTests。固定TRX同项目TestResults/com-acceptance.trx；历史完整副本覆盖未保存，只有已读身份。SHA来自加载实际生产DLL。未运行覆盖率/原生差分/E2E/Ledger修复。

## 剩余完成条件

Release/x64布局尚需真实创建者验证，其他架构明确不支持。marker仍E_NOTIMPL但不再静默忽略。非圆/旋转笔、MiterClipped、闭合gap、dash切点/相位/复杂变换、原生坏数语义与完整CShape方法族尚未闭合。真实brush创建者ABI、fill/stroke联合首错和Begin/End/Abort/Compose重入生命周期未关闭。

下一轮继续合并同一private方法族与真实ABI/生命周期，不把单一dash参数测试或复验作为独立交付。没有确认外部阻塞，本轮是实际生产内部推进，不是完整模块交付。阶段1–3 Closed、4 Active、5–7 Pending，Ledger RepairRequiredPartial，不能替换原DLL。
