# S-20261009-208：GeometryGroup整体轮廓描边消费（模块未关闭）

## 生产推进

原生core/resources/geometrygroup.cpp:40–92先AddShapeData汇聚子figure并应用group矩阵，再设置fill rule；描边不应让重复figure按Alternate填充相消。实现SoftwareGroupStrokeContours递归汇聚直线、路径、矩形/圆角轮廓，child变换后应用group变换，REAL坐标和循环保护；GeneratedStrokeGeometry对汇聚结果统一Widen/Dash及一次画刷消费，避免子描边多次alpha叠加。普通矩形轮廓保留实际角join，不被当成round smooth。原生未修改。

新增真实group资源命令包含重复直线子figure，走pen→Compose→bitmap像素，包含遮罩锁首错和解锁恢复；新增测试先失败（完整汇总/错误未读取），实现后同一像素断言通过。group内ellipse/combined不支持，不能把group汇聚等同完整几何协议关闭。

## 发布和验收

两次完整命令均`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 成功，11行日志未读取，工具成功可核实；启动352/352、364ms。SHA/TRX身份未取得，不借用旧值。新增group测试失败38行日志未读取，不能声称错误细节已证明。
2. 成功，59行日志未读取，工具成功可核实，完整诊断未保存。定向232/232、185ms，TRX29566eb3-62e3-4024-89ab-097418fc925a；全量353/353、366ms、0失败/跳过，TRXaebfcc80-8840-42b9-926b-b709923bc9e8；SHA4291DAC329BAAD6991F52C2F41F532BE6807A200E3B0B1EB72B68497D5557AE7。

测试项目Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，综合筛选FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~BitmapRenderTargetTests。固定TRX同项目TestResults/com-acceptance.trx，历史完整文件覆盖未备份，仅保存已读身份。SHA取实际加载DLL测试输出。覆盖率、原生差分、PresentationCore E2E、Ledger修复未运行。

## 剩余完成条件

group变换/多figure/Dash重置/重复曲线及循环等差分未全面验收；ellipse/combined仍拒绝，不能用bounds近似。group当前不是private CShape输出ABI。

本轮仍未交付用户指定完整模块：private fill→真实CShape Widen→stroke、实际创建者/消费者ABI、Begin/End/Abort首错逆序清理及Compose重入生命周期仍未关闭。无确认外部阻塞。下一轮继续同一大模块联合核心实现，不将group补测或复验独立交付。阶段1–3 Closed、4 Active、5–7 Pending，Ledger RepairRequiredPartial，不能替换原DLL。
