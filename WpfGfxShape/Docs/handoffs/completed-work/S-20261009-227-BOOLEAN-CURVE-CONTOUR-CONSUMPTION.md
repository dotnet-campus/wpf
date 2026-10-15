# S-20261009-227：布尔曲线结果轮廓边界与填充/描边消费

## 生产修改与原生证据

收口连续会话的布尔曲线实现。原生证据为 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/combinedgeometry.cpp` 41–132：GetShapeDataCore取得两个operand（null为空shape）、当前矩阵，调用CShapeBase::Combine，随后清理临时shape及LeaveResource。原生要求retrieve curves；本库目前仍为扁平化多边形结果，不声称曲线恢复已完成。

GeneratedGeometryBounds 的Intersect/Xor/Exclude此前只支持精确矩形区域。现在保留矩形分支，其他几何由SoftwareGroupStrokeContours构建实际布尔轮廓，累积结果顶点边界，不使用operand包围盒冒充结果。

新增独立toleranceTransform参数，Visual局部画刷边界使用实际设备消费变换调整曲线扁平化误差，输出仍保持局部坐标。CombinedGeometry使用既有Integer26工作空间、交点分割、边界分类与闭合轮廓构建，NativeGeneratedPath供真实private第10槽消费。

## 验收

唯一COM项目直接加载生产Native AOT DLL。新增20项填充及15项描边曲线用例，四种布尔模式覆盖相交、重合、分离以及1e-8/1/1e8尺度。相交椭圆在目标内包含两种不同区域，验证fill结果及新生成的内部描边边界；Union不应描绘内部operand边界，重合Xor/Exclude及分离Intersect为空。

最初新增填充用例中9项因E_NOTIMPL失败；接线后进入真实像素验收。早期采样落在AA边界，调整为4像素目标，填充采样0/3、描边采样0/2；几何参数及取样点按区域内部选择，不把观测颜色写成期望值。全部保留精确红色/透明像素断言。描边初次9项失败是像素3位于描边边缘，采样内部像素2后通过。

最后布尔定向91/91通过；最终全量648/648，455ms，零失败/跳过。未修改Stdcall、COM引用计数、首错或释放逻辑；全量包含既有回调、重入与逆序释放验收。本次没有新增DrawingImage联合验收，不能把全量通过等同其模块关闭。

## 全部发布记录

连续工作链五次发布完整命令均为：

`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`

项目 `WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj`；Release / win-x64。产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

1. 布尔曲线工作起始：成功，11行日志，随后613项全量通过；hash/TRX ID未独立保留。
2. 实际曲线边界接线：成功，59行日志；定向9项进入像素不符阶段。hash/TRX ID未独立保留。
3. 独立设备容差接线：成功，60行日志；定向通过，全量633/633（417ms）。编译存在CA1416平台警告；该次hash/TRX ID未独立保留。
4. 本次续接起始：成功，11行日志，随后633项全量通过；hash/TRX ID未独立保留。
5. 描边验收收口：成功，11行日志，退出码0；最终648/648（455ms）。SHA256 `722AA2DF4DF7083FCF51D1C8AB88FFBC62C78DA7151A9145D89FE6E1FAD50C5D`，TRX ID `3b9516b2-25a8-44b0-814c-53c0e858c5d9`。hash由最终测试TRX输出确认。TRX路径 `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`；定向结果文件已被全量覆盖，不补造对应ID。

## 剩余边界

DrawingImage联合布尔轮廓映射、曲线恢复、全部交点拓扑/退化轮廓精度、完整MSVC对象ABI尚未关闭。当前软件轮廓消费不能视为完整CScanner/CShapeBase直译。下一轮合并DrawingImage内容边界、映射、画刷/遮罩与失败生命周期完成独立生产闭环。

阶段1–3Closed、阶段4Active、阶段5–7Pending；Ledger RepairRequiredPartial。不能替换原wpfgfx_cor3.dll，不报告整体完成百分比。
