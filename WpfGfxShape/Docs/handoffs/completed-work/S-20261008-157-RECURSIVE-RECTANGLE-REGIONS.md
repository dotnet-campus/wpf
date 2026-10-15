# S-20261008-157：递归矩形组合区域的生产 bounds 消费

## 交付边界

这是完整 ImageBrush 遮罩映射、层消费与 Compose 生命周期模块的内部生产进展，不是模块交付或缩小范围的授权。没有确认外部阻塞。阶段4 Active，当前不能替换原 DLL。

## 原生证据

- core/resources/combinedgeometry.cpp:35-135：递归取得两操作数当前形状，null 为 empty，组合变换作用于形状后 Combine；资源 Enter/Leave 防止递归环。
- core/resources/BrushRealizer.h:35-128：CBrushRealizer 继承 CMILRefCountBase，EnsureRealization、GetRealizedBrushNoRef 和 effects 方法属于 C++ 对象合同，不能按 IUnknown 或 generated handle 调用。
- core/sw/swlib/swsurfrt.cpp:790-915：共同有限/无限路径先裁剪、再 Read|Write 锁、fill、widen/stroke，最后无条件解锁；本轮没有实现其有画刷路径。

## 生产差集

GeneratedGeometryBounds 将递归矩形布尔结果保留为不相交矩形区域，而非子组合包围框。父级 Union/Intersect/Xor/Exclude 按所有区域边界划分单元并判定占用；外层仿射作用于保留区域四角。递归操作数只接受保持轴对齐的变换（含轴交换），沿用256深度保护。一般曲线和非轴保持的子组合仍明确不支持，不以近似包围框假装支持。

CompositionEffectsTests.ImageMask 增加12个真实生产AOT参数用例：四种递归组合、四种不连通子区域组合、四种递归操作数轴交换。复用真实连接/通道/资源/Present、遮罩源锁失败、重试、资源释放、空viewport恢复、scope mask/clip/Pop及后续帧链。首批4项在修复前DLL返回E_NOTIMPL；另外8项在修复后添加，未取得同断言Red，不能宣称12项均TDD闭合。没有新增导出、Host、测试项目或slnx。

## 实际发布流水

两次完整命令均为：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`。

1. 恢复发布：生产项目、Release、win-x64，成功，日志11行。恢复244/244、0失败/跳过、248ms。恢复SHA/TRX未单独归档，后续测试覆盖结果文件，不借用历史哈希。
2. 修复发布：同项目/配置/RID，成功，退出码0，日志60行，包含CA1416与CA1822诊断；不得报告零警告。产物 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64\wpfgfx_cor3.dll`。修复后首轮遮罩40/40、61ms；追加8项后定向运行成功；最终全量256/256、0失败/跳过、252ms。

最终TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，TestRun `47ffd727-6bd2-4dc8-802a-b1631e28034c`。
实际加载发布2产物 SHA256：`4BB73D5D5B80E0A99356BE210C6193592E632C5D18E21B796239BA4F6F2B5D0A`，从最终TRX读取。恢复、Red和定向TRX被最终运行覆盖，未独立保存；发布日志也没有逐次独立文件，本文记录工具返回和核实结果，不虚构保存记录。

## 未完成与下一轮合并条件

完整原生IShapeData/IFigureData、CBrushRealizer/CMILBrush、CPlainPen的真实创建者和有画刷fill/stroke共同扫描消费；Begin/End/Abort、嵌套mask/clip/opacity及失败逆序释放；一般曲线布尔运算、原生边缘差分、Visual动画与tile/DPI/浮点/预滤接缝；回调释放调用者引用、读取重入和下一帧首错恢复仍须同模块完成。Ledger保持RepairRequiredPartial，本轮未修复或验证机器Ledger，不增加Complete项。覆盖率、原生差分与PresentationCore E2E未执行。下一轮不得仅领取递归bounds补测。
