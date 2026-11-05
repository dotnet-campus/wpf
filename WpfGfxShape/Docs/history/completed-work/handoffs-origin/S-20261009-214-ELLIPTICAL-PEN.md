# S-20261009-214：纯C# private椭圆/旋转实线笔消费（模块未关闭）

NativeStrokeReader非圆形实线笔不再无条件拒绝：构造笔椭圆到形状空间的仿射矩阵，将实际figure点映射到圆笔空间，用单位笔共同Widen，再将轮廓映回设备空间。保留形状而非bounds代理。不调用MSVC Widen，不引入C++。非圆dash长度度量不同，仍明确拒绝，零轴退化笔仍未实现。

新增3项非圆/90度旋转笔边界回归，真实private target调用包含surface lock首错、Ensure/Effects失败恢复和裁剪像素。旧DLL定向失败58行完整诊断未读；实现后同一断言通过。测试仅证明窄消费与恢复，不证明非均匀Bezier容差、旋转端帽或miter实际边缘原生差分。笔/shape/realizer仍为边界对象，真实创建者ABI未关闭。

## 发布

两次完整命令均`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，项目同路径，Release/win-x64；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 成功11行日志未读，工具成功可核实；启动测试成功但统计/SHA/TRX未读取，不借用旧值。
2. 成功59行日志未读，完整诊断未保存。定向250/250、177ms，TRX8d344f03-16c0-4cd5-80ed-6efa134a7ac7；全量371/371、377ms、0失败/跳过，TRX2343b90d-005d-49b7-857b-756081ff5641；SHA4B21C6FE45C00C54EA82D1B51B11B415D06A650DC37DCA947FE0BA3BC5F95618。

测试项目Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj；定向筛选FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~BitmapRenderTargetTests。固定TRX同项目TestResults/com-acceptance.trx；完整历史副本未保存，仅已读身份摘要。SHA来自实际加载DLL。覆盖率/原生差分/E2E/Ledger修复未运行。

## 剩余条件

本轮为内部生产推进，非完整模块交付。椭圆笔仿射实现未完成原生证据差分，非圆dash/零轴/marker/MiterClipped/回折和完整CShape方法族仍待闭合；真实brush/pen/shape创建者ABI、fill→Widen→stroke联合首错与Begin/End/Abort/Compose重入生命周期仍未关闭。没有确认外部阻塞。唯一下一动作继续同一方法族/真实ABI/生命周期大模块，不默认单个笔参数补测或复验。阶段/替换资格不变，不能替换原DLL。
