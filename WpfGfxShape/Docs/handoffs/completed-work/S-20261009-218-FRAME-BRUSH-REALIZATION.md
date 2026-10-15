# S-20261009-218：纯C#资源画刷帧实现创建/消费/释放（原生ABI未关闭）

SoftwareBrushRealization将真实generated solid/image资源统一捕获为帧拥有实现；solid冻结当前颜色/透明度，bitmap通过GeneratedImageMask持有源COM引用，null实现不消费。GeneratedVisualRenderer在target回调前创建fill/stroke实现，统一消费，并在session清理后按draw逆序Dispose，Dispose先detach bitmap防释放重入。替换原先fill/stroke重复颜色/bitmap捕获代码，不新增测试导出、Host、C++或空槽vtable。

这是实际资源命令→生产帧工厂→绘制消费者→释放的纯C#闭环，但不是CBrushRealizer/CMILBrush私有ABI对象，也没有接通原生private target创建者。用户批准的真实ABI大模块仍未完成，不能将本重构冒称其交付。

原生证据resources/BrushRealizer.h:40–130、api/api_base.h:25–140：创建者内部方法、借用嵌入画刷和LocalMILObject合同。LocalMILObject Release在retail返回1且不销毁，不能按普通COM对象释放嵌入borrowed brush。没有原生导出创建接口，禁止为测试虚构导出。

## 发布与验收

两次完整命令均`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，目标同路径Release/win-x64；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 成功11行日志未读，启动测试工具成功，统计/身份/SHA未读取，不借用旧值。
2. 成功60行日志未读，完整诊断未保存。定向258/258、192ms，TRXeab9c54e-2ea7-477c-84a7-27b1aaffc3a3；全量379/379、376ms、0失败/跳过，TRX892c13b9-0cad-4303-a0c6-5dd099c5e5b4；SHA DA810203EF384DF489A776D988559C0E09E218E52F55FFF9255F0F6234C8578A。

没有新增测试、不声称TDD Red；现有真实Compose solid/image几何与失败恢复回归通过。测试项目Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，定向筛选FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~BitmapRenderTargetTests。固定TRX同项目TestResults/com-acceptance.trx；历史完整副本覆盖未保存，仅已读身份摘要。SHA来自实际加载DLL。覆盖率/原生差分/E2E/Ledger未运行。

## 下一动作和剩余

继续用户批准同一真实创建者/private消费者ABI大闭环：完整resource/null/immediate realizer及secondary CMILBrush实际槽/借用合同必须由生产命令路径连接到private DrawPath，不以managed快照工厂、自调用或边界字段测试替代。合并fill→Widen→stroke联合首错、回调释放caller引用、替换删除销毁与失败逆序释放验收；纯C#，不引入C++。无确认外部阻塞。本轮帧工厂是内部生产推进，不是完整模块交付，阶段与替换资格不变。
