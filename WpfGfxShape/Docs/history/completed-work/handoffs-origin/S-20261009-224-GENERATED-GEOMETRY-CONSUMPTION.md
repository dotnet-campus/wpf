# S-20261009-224：generated 几何消费恢复与曲线联合生命周期验收

## 交付范围

本轮接收未归档的 generated 非零 shape/pen、NativeGeneratedPath、gap、空/短 figure、椭圆与圆角轮廓适配源码，恢复唯一 COM 项目的全量执行，并完成基础几何捕获失败保护和真实曲线/group 联合画刷生命周期验收。不是完整 MSVC ABI 或原批准模块全部关闭。

## 原生证据与生产差集

- 本轮读取 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/geometry/figure.h:25–155`，确认 IsEmpty 以点数为依据，HasNoSegments 与 IsEmpty 不等价；未修改原 WPF 源码。此前会话读取的 figure.cpp 椭圆与 CPlainPen 证据未在本轮重新读取，不冒称新增证据。
- 起始全量在 private DrawPath 出现 0xC0000005：NativeShapeReader/NativeStrokeReader 读取新增 IsEmpty 槽，旧 private-path 夹具槽1为零。补齐夹具 NoPathFlag；未让生产读取器绕过必需槽。该手写私有夹具只作既有回归，不用于证明生产 ABI。
- NativeShapeReader 对起点、变换后起点与直线终点增加有限坐标检查，错误返回 E_INVALIDARG。
- NativeGeneratedPath.ShapeEmpty 扫描全部 figure 的点数，避免仅因存在空 figure 就返回非空。
- GeneratedVisualRenderer 对非 CombinedGeometry 的原生捕获失败直接返回 E_NOTIMPL，不再悄然使用侧挂 coverage 掩盖失败。CombinedGeometry 原有回退仍保留，不宣称已经移除全部回退。
- 椭圆生命周期夹具原始半径1/0.5使曲线边缘落在2×1采样区域，不能复用矩形完全不透明预期。核对 MilEllipseGeometryCommand 的8/16半径、24/32中心字段后，改用半径2/1、中心1/0.5，将曲线边缘移出采样像素。首次调整曾误改中心字段，定向失败后纠正；未修改断言迎合实测像素。该夹具不承担椭圆边缘coverage精度证明。
- 新增20项真实 generated 圆角矩形与 GeometryGroup（椭圆子资源）联合 fill/stroke 验收，覆盖图像遮罩、资源替换、channel/connection关闭、private重入、目标外不解码、fill锁失败和重试。通过公开生产导出及真实资源命令消费生产 AOT DLL，无新增Host、测试导出、托管生产项目引用或slnx。
- 保留既有 fill首错阻断stroke、两个owner保活、会话renderer/目标锁、clip/layer及primitive逆序释放。测试通过不等价于完整Brush/QI/meta或MSVC布局完成。

## 每次实际发布

共同命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。
目标均为上述生产项目，Release / win-x64，产物均为 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

| 发布 | 结果与诊断 | 对应验证与证据 |
|---|---|---|
| 01：开始前 | 成功，工具日志11行；完整日志未另存 | 起始测试通过99项后因访问冲突中止，不能称99/99全量成功。修复夹具后同DLL全量481通过、10失败、共491项（454ms），10项均为椭圆像素预期。后续修正夹具定向通过；本次SHA256/TRX ID未保留，TRX已被后续运行覆盖。 |
| 02：生产修复后 | 成功，工具日志59行，含CA1416平台警告，生成native代码并输出publish目录 | 定向145/145（95ms）；全量511/511（403ms），0失败/跳过。SHA256和最终TRX如下。 |

最终发布02 SHA256：`BFDFB329AC8EFBDA77D8A838A7C23650019F9D90A1857F872AF3F8D48AD5CCAA`。
最终全量TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，TestRun ID `5e56185a-de1a-49a0-9795-f8a327f75fa9`。读取TRX头与测试StdOut确认对应产物SHA256；定向TRX被全量覆盖，ID未保留。测试编译仍有MSTEST警告，运行提示覆盖TRX。完整日志和TRX副本未单独保存，本文件保留核实结果，不冒称已归档完整原始证据。

## 继承工作与未关闭边界

上一文档稳定基线439/439之后的路径快照、非零shape/pen、填充路由、gap数组及椭圆生产源码均已在本轮发布与511项全量中消费；不将这些既有源码冒称本轮全部新增。上一会话463/463及其发布流水来自会话摘要，未重新核实原始文件，不替代本轮权威验证。

仍未关闭：预先扁平化曲线在设备变换下的容差保真、PointTangent在native适配链中的传递、CombinedGeometry真实布尔轮廓/描边、完整MSVC IShapeData/CPlainPen/CBrushResourceRealizer布局与析构槽、Brush/QI/meta、非圆dash/marker和完整层栈。本轮没有确认外部阻塞，原大模块仍Active，不因验收数量增长称其完成。

阶段1–3 Closed、阶段4 Active、阶段5–7 Pending；Ledger仍RepairRequiredPartial，生产分母不确定，不报告整体完成百分比，当前DLL不能替换原库。唯一下一动作见current-work-item.md。
