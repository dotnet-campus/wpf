# S-20260930-116：内部目标完成门禁Red

本轮重新核对drawingcontext.cpp:1572-1636实际DrawPath参数，BrushRealizer.h:40起的原生CBrushRealizer多态方法及内嵌solid brush，与现有Direct3D9ImmediateBrushRealizer托管回调对象并非原生ABI等价。不能以同名类型或空QI替代。

新增真实DLL门禁WhenBitmapTargetBeginsNativeFrameThenInternalTargetCanBeAcquired，类别InternalTargetContract：生产工厂创建位图目标后，查询InternalGUIDs.h的真实IID b73b1159-a295-4c76-bb56-c18e282ae007，期望S_OK。实际E_NOINTERFACE(0x80004002)，确认核心未实现。该测试仅检查必要前置，不调用未定义槽，不代表完整内部接口验收。

最终全量134通过、1失败、0跳过，257ms。TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。本轮仅新增测试，未修改生产，未发布。失败测试未跳过、未改为期待E_NOINTERFACE。工作区当前不是全绿。

大模块仍未完成。不能只让QI返回成功来解决Red；必须提供正确内部目标视图以及上下文/形状/画刷的实际消费，并继续像素/生命周期验收。现有方法族部分可复用，但原生ABI仍需实现。没有外部工具阻塞。
