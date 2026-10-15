# S-20261009-217：solid共同消费校验与真实创建者尝试撤回（计划未完成）

用户授权执行真实solid/bitmap创建→private消费→失败恢复/释放大闭环。本轮未完成该计划，不宣布交付。

原生核对resources/BrushRealizer.cpp:45–260、795–880，api/api_brush.h:35–140、api_base.h、RefCountBase.h：immediate嵌入solid、引用初值和resource/null方法族。尝试生产分配realizer和借用solid字段、generated资源消费内部对象；首轮编译/测试通过但vtable存在空槽且只内部自调用，不能证明真实ABI，随后撤回所有unmanaged对象/vtable实现。最终ImmediateSolidBrushRealizer仅为纯C#静态颜色快照消费辅助，不是原生CBrushRealizer或CMILBrush对象。

保留生产修正：generated solid与private借用solid统一颜色消费校验，非有限颜色返回InvalidArgument，Ensure/Effects失败先返回首错。新增2项边界realizer坏色/Ensure首错与恢复像素测试；没有在校验实现前运行，不称TDD Red。未新增Host/测试导出/C++，不把静态辅助类称为真实创建者闭环。

## 发布

三次完整命令均`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，项目同路径Release/win-x64；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 成功11行日志未读，启动测试工具成功未读取统计/TRX/SHA。
2. 不完整unmanaged试验发布成功59行日志未读，定向工具成功未读统计/身份，SHA未取得；之后撤回，不能拿此Green证明ABI。
3. 撤回后纯C#共同校验发布成功60行日志未读。定向258/258、210ms，TRXc1a83f71-468d-4121-857d-c7a95fb9b4c1；全量379/379、402ms、0失败/跳过，TRX3b5ee31d-504a-4c71-88c1-c6a073ee5b02；SHA5182D19EBCE79663F615525C6A2F6F558DC157614FD05258356F882BD234AF32。

测试项目Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj；综合筛选FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~BitmapRenderTargetTests。固定TRX同项目TestResults/com-acceptance.trx；历史完整副本覆盖未保存，仅已读身份摘要。SHA来自加载实际DLL。没有覆盖率/原生差分/E2E/Ledger验证。

## 计划剩余和下一动作

真实resource/null/immediate realizer生产创建方法族、完整secondary CMILBrush和LocalMILObject借用、真实solid/bitmap创建与private消费均未完成；当前managed辅助没有COM引用生命周期。fill→Widen→stroke联合失败次序、回调释放调用者引用、删除替换/最终释放、bitmap映射和层Compose重入仍待关闭。无确认外部阻塞，本轮偏离用户要求的大交付，不以新增两个测试抵扣模块完成。

下一轮必须继续用户已批准的同一真实创建者/消费者与生命周期大闭环，不再发布空槽vtable或以内部自调用测试冒称ABI。保持纯C#，阶段及替换资格不变，不能替换原DLL。
