# S-20261008-197：直线 Triangle 生产路径 Green

## 修改与合同

本轮从先发布/测试的 300/300 基线继续，新增实色正向及 ImageBrush 反向负厚度 Triangle 用例，先取得 90 通过/2 失败；按 WpfGfx/core/geometry/strokefigure.cpp:1880–1980 的 DoBaseCap Triangle 分支接通非退化直线端帽轮廓。GeneratedStrokeGeometry 以半厚度切向尖顶连接法向轨道，共用 SoftwarePathCoverage 多边形扫描和仿射 bounds；既有绘制与 ImageBrush 内容 bounds 消费者无需另加导出。保持 HRESULT 首错、逆序清理及原有资源所有权，不修改 COM ABI。

## 发布与验收关联

- 发布 #1 完整命令、成功状态、产物、SHA 与 300/300 TRX 见 S-194；该发布的 Triangle Red 见 S-195。
- 发布 #2 完整命令、诊断截断、native 生成、产物与定向 92/92 TRX 见 S-196。两次均 Release/win-x64 生产 AOT 发布。
- #2 全量唯一 ComAcceptance csproj 直接加载生产 DLL：302/302，0 失败/跳过，402ms。
- 最终 TRX `ded15732-c591-41fc-a506-1f766e87fe85`；start `2026-10-08T20:16:50.0779189+08:00`；finish `2026-10-08T20:16:50.8604075+08:00`。
- SHA256 `19CE757E88E5C75ACA8820B66EF5A63828B46037080E28CA02A342BB13433ACD`，由全量测试输出取得，对应 #2 产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。期间未另发布，亦关联 S-196 定向验收。
- 覆盖前保存初始、Red、定向 TRX 身份摘要，未保存完整副本；发布 #1 未保存诊断，#2 只核实截断日志首尾，不声称完整日志归档。

## 边界与下一轮

新增用例验证真实作用域、遮罩 WIC 锁定首错、失败丢层、释放锁重试、输出像素和最终释放；尖顶在输出裁剪外，尚无端帽边缘差分证据。非退化 Flat/Square/Triangle 不等于一般 Widen；Round、Dash、零长度非 Flat、private shape/pen/context/realizer ABI、完整 ImageBrush 方法族与生命周期模块仍待完成。

下一轮仍合并完整 ImageBrush 遮罩映射、层消费与 Compose 生命周期，而非独立补测轮：真实私有创建者 ABI→realizer 全参数/首错与 Widen→实际层 Begin/End/Abort/Pop 消费者→动画、映射、曲线 bounds 与共同扫描→重入、帧保活、销毁及失败恢复→直接 AOT 定向/全量与 Ledger 实际差集归档。端帽边缘和剩余 cap/dash 纳入上述同一模块。

阶段 1–3 关闭，4 Active，5–7 Pending；Ledger RepairRequiredPartial，无稳定分母。覆盖率、原生差分、PresentationCore E2E、Ledger 修复验证未运行，完整模块未关闭，无已确认外部阻塞，不能替换原 DLL。
