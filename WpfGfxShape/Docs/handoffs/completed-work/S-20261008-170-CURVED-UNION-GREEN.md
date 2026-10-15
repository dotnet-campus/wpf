# S-20261008-170：曲线 Union bounds 生产修复完成

## 生产与证据

本轮按用户要求先发布再全量测试，初始281通过/2失败；前置证据见S-20261008-168-CURVED-UNION-RED.md。GeneratedGeometryBounds.Append对Union递归累积操作数已有曲线极值，并将组合的共同仿射传入操作数；支持null空操作数与递归深度保护。没有修改原WPF、COM ABI、导出、引用计数或释放顺序；本轮没有修改测试，复用上轮留在工作区的两项真实椭圆Union普通/外层仿射Red用例。

原生证据：WpfGfx/core/resources/combinedgeometry.cpp:35-135，共101行；null为空、当前操作数和共同矩阵交给Combine。这里交付的是Union bounds消费者修复，不是完整原生布尔曲线算法翻译。Intersect/Xor/Exclude保持精确矩形区域限制，不用包围框冒充布尔区域。

## 发布和直接AOT验收

- 发布#1完整命令、产物与Red/TRX见168归档；该发布未独立提取SHA256。
- 发布#2：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，net10.0、Release/win-x64，成功/退出码0；产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。立即读取全部可用60行，原409681字符仅末尾20000字符可取得；可见CA1416及native生成成功，不声称完整诊断已保存。
- 发布#2定向73/73、0失败/跳过、95ms，TRX和覆盖前摘要见169归档；没有保留完整原始TRX副本。
- 同一发布#2全量283/283、0失败/跳过、341ms；TRX `5a8e92da-814d-49bb-9401-7427e649586e`，开始 `2026-10-08T19:05:29.9154523+08:00`。
- 全量TRX记录实际生产DLL SHA256 `1CA1BE8ECDFAB2B1EAF0FA7CBD3F6CDC359091F8371348173CD1135389C30BF2`；该值关联发布#2。默认完整TRX当前保留于 `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。

## 下一轮计划及边界

继续完整ImageBrush遮罩映射、层消费与Compose生命周期模块，不另开复验/调查/补测轮：真实resource/null/immediate realizer创建者和shape/pen/context布局及所有权，与完整finite/infinite EnsureRealization消费者一起交付；同链传播effects失败、fill→Widen→stroke、共同扫描和Begin/End/Abort/Pop首错逆序清理；合并精确一般曲线布尔区域、Visual/作用域动画、映射和真实来源重入/帧快照/调用者释放/失败恢复/最终释放；发布后直接AOT定向与全量、证据归档及Ledger差集收口。Union两场景作为回归，不再规划单独补测轮。

本轮只修复已取得Red的生产前置，未完成上述完整模块；没有已确认外部阻塞。阶段1–3关闭、阶段4 Active、阶段5–7 Pending，Ledger RepairRequiredPartial，不能替换原DLL。没有运行覆盖率、原生差分、PresentationCore E2E或Ledger修复验证。
