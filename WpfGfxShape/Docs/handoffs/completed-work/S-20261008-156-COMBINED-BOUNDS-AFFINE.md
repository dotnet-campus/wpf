# S-20261008-156：矩形布尔组合内容边界与外层仿射顺序

## 恢复与生产差集

开始先正常发布，随后全量240/240通过、0失败/跳过、271ms。源码已包含上轮未归档的矩形CombinedGeometry内容bounds和10项验收；不将其重新声明为本轮实现。上轮发布次数、日志、Red及32/32定向TRX未独立保留，本归档只确认当前恢复事实，不追溯补造证据。

本轮读取原生 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/combinedgeometry.cpp:1-135`：空操作数为Empty shape，CShapeBase::Combine按四种模式消费两个形状及同一变换。本轮修改 `Core/GeneratedGeometryBounds.cs`：在组合局部坐标系内划分两矩形边界单元，按Union/Intersect/Xor/Exclude选择剩余区域，再变换每个区域四角累计精确边界。避免外层仿射提前施加导致可计算矩形组合被拒绝。操作数自身仍须为轴对齐、非圆角矩形；一般曲线组合不以包围盒代替真实布尔结果。

未增加导出、改变COM ABI、改变引用计数或清理顺序。实际裁剪继续走GeneratedImageClip/SoftwareImageCoverage，bounds不是像素覆盖率。

## 验收过程与证据限制

在唯一ComAcceptance项目真实命令→PushOpacityMask→PushClip→DrawImage→Pop→Present链新增四种模式仿射用例。初始剪切用例在旧DLL均于Present得到E_NOTIMPL。实现后Present成功，但沿用未变换像素预期失败；该预期不适用于剪切边缘。本轮明确修正测试模型为轴交换仿射与重叠单位矩形，独立按布尔恒等式确定结果：Union/Intersect保留原区域，Xor/Exclude为空。不是把实际输出反写为期望。最终四项模型未在实现前DLL重跑，不能宣称最终四项同断言Red→Green；初始E_NOTIMPL只证明仿射组合消费者缺口。

复用真实源写锁失败、解除锁后重试、空viewport恢复、嵌套scope和Pop后的绘制链。定向36/36通过，0失败/跳过，54ms；最终全量244/244通过，0失败/跳过，236ms。

最终TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，ID `e4837e67-e62b-4037-bb91-e2ddd3ef7421`。
最终实际加载DLL SHA256：`53EF0473847385F8E8610D46F1277DE66A542A6AAEDFB592077EFCA15F64BFAA`，对应本轮发布2。
恢复全量TRX ID：`21e30cb3-749f-427b-a499-e7f9ecae4a51`，后续运行已覆盖此文件；恢复DLL SHA未单独保存。Red/中间失败/定向TRX亦被最终运行覆盖；不声称已独立留存。

## 本轮实际发布流水

两次均调用专用发布能力，未执行脚本。

完整命令（发布1和发布2相同）：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`。

- 发布1：生产项目WpfGfxShape.csproj，Release / win-x64，成功（工具11行日志）；用于恢复240/240和初始仿射E_NOTIMPL Red。日志未独立保存，SHA未单独取得。
- 发布2：相同项目/配置/RID，成功（工具59行日志）；已核实末尾Generating native code及publish产物输出，包含CA1416平台警告。用于实现后的中间像素预期失败、修正测试模型后的36/36及最终244/244。SHA/TRX见上文。测试模型修正后未再修改生产代码，未额外发布。

两次产物路径：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64\wpfgfx_cor3.dll`。

## 未完成边界

仅完整ImageBrush遮罩映射、层消费与Compose生命周期模块的内部进展，不能据此关闭模块。非空原生形状/画刷指针消费、fill/stroke扫描、完整Begin/End/Abort生命周期、一般CombinedGeometry及原生边缘差分、联合浮点/预滤/接缝矩阵和效果回调释放调用者引用仍缺。没有已确认外部阻塞，不授权缩小下一轮模块。覆盖率和PresentationCore E2E未运行；Ledger仍RepairRequiredPartial，未修复或计算完成分母。阶段4 Active，不能替换原DLL。
