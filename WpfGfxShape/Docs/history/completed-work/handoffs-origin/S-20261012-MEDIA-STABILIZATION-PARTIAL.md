# 媒体稳定化局部交付（阶段4仍Active）

## 实施

- Core/Direct3D9DeviceManager.cs：恢复显示切换时可用设备快照的逆序失效，两个既有逆序测试通过。
- Core/Av/VideoSlave.cs：BeginComposition传播前一次EndComposition、provider获取和renderer开始的失败HRESULT；EndComposition传播结束失败，并在finally释放renderer持有引用。不再把失败转换为成功。
- Tests/WpfGfxShape.Tests/AvVideoFailureTests.cs：三个依赖契约失败传播测试通过。这些替身测试仅验证生命周期/HRESULT，不是系统解码或native媒体闭环实证。

## 真实专用发布与验收

发布目标：Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64 NativeAOT。

SHA256：090700B38DCE897AB01DE878BBABF3D57E1D90570E2551C26330B8EB263A5093。

专用项目：Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj。

- 修改前重新发布全量：RunID d8aab66e-73dc-423a-a3b8-86a0c953897d，1275通过/1失败/1276，无崩溃；报告wpf-calibration-93dc0bcafb2c42b2bfbc804f3f7a23bd.json。
- 修改后媒体导出与发布SHA定向：RunID 794d2656-1dc1-4b86-91d8-c71cca15a479，32通过。
- 修改后全量：RunID 99716580-a46c-4135-b5d6-6b3ee552a63c，1275通过/1失败/1276，无崩溃；报告wpf-calibration-1e9c20ea73b849bc81076569cecdd05e.json。
- 原始TRX位于Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx；该文件每次运行覆盖，此导航记录定向RunID与SHA，不冒充完整TRX归档副本。

## 未收口问题

普通单元定向129项：122通过/7失败。剩余六项设备通知时序/初始化失败通知、一个未知命令HRESULT仍失败，未调整断言隐藏问题。原全量0xC0000005定位到Direct3D9DeviceInterfaceQueryFailureTests的20槽FakeDeviceObject被新增真实InitializeRenderState路径消费，测试未实现该native设备ABI；尚未修复且未重新运行普通全量，不声称消除崩溃。

GeneratedMediaPlayerResource.ProcessCommand仍在原版provider QI后返回E_NOTIMPL。原版RegisterResource接受真实CMilSlaveVideo*并调用非虚方法，禁止用GCHandle或COM回调伪装该对象。本次未跨不兼容native注册。

自身生产provider注册通知桥、系统解码/sample、BeginRender/EndRender及generated Visual像素消费者、生产创建薄入口、替换关闭失败重试专用实证尚未实施。媒体导出查询成功不代表上述链路完成。

本次不晋升Ledger，RepairRequiredPartial、阶段4 Active及后续Pending保持不变；未完成stableworkitem/长期计划机器权威一致收口，不宣称阶段4完成。
