# 真实 media wrapper / async bridge 复测：Red，未收口

## 发布绑定

本轮专用发布1：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。工具报告成功，11行发布日志；未保留该日志全文，后续测试覆盖最后日志。目标Release/win-x64 NativeAOT，产物`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。完整实际SHA256从全量校准报告回读：`BCF685CAC08AC5CAF20553851F23C9CF1E0028C440C72E11025E1F21A8694A19`。本轮没有修改生产代码。

## 实际执行顺序

1. 路径修正产物定向：`FullyQualifiedName~MediaExportsTests|FullyQualifiedName~VideoTests`，32通过/0失败/0跳过，退出码0。
2. 将红帧验收改为真实系统eventproxy callback唤醒AutoResetEvent，等待通知后调用公开composition present并读取真实WIC像素；没有手工RaiseEvent、fake provider、internal调用或自建进程。元数据部分仍有有界查询，不计为异步验收。单红帧1/1，退出码0。
3. 专用发布成功，随后新增绿/蓝DataRow，预期颜色按自有真实AVI BGR数据生成，未修改golden或容差。新增发现数应为2，总颜色实例3。
4. media定向日志报告32已通过，但测试主机崩溃，退出码1。不能按32/32 Green归档。
5. 精确颜色复测再次崩溃：TRX RunID `a1817fcd-3f4d-43e3-b644-50d988bcdc4a`，只完成红色实例1，第二/第三实例未完成；总运行Failed。日志没有原生堆栈或新HRESULT，不能声称已定位根因。
6. COM全量再次执行，RunID `fd55b88c-e96f-4947-95e6-acf847a9ccf6`，1248通过/1既有FLOAT失败/1249已报告，随后测试主机崩溃，运行中止，不能当完整全量。报告`wpf-calibration-7b962199192342a6bfc0939baaca24db.json`实际回读同SHA，FLOAT57/450、Pbgra32 0/180。TRX路径`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，该固定文件会被后续测试覆盖，独立归档本报告所引用的RunID以免混淆。

## 修改映射与未满足门禁

`MediaExportsTests.cs`新增真实descriptor上下文和通知等待，失败时由调用方释放尚未转交proxy的GCHandle，成功创建proxy后由descriptor dispose释放。`MediaExportsTests.SolidFrame.cs`真实callback唤醒帧消费，红/绿/蓝参数化。当前多player顺序运行出现可复现原生崩溃，必须继续调查，不移除颜色实例来隐藏回归。

`core/api/exports.cpp:76–91`确认原生MILFactoryCreateMediaPlayer(factory,eventproxy,bool,out media) ABI。本轮阅读确认SystemMediaSource现有真实系统模块加载、系统事件→订阅→generated pending→composition请求已接线；现有按最高版本目录选系统DLL仍缺明确同版本binding校验。红像素成功不等于生命周期完整。

尚未完成：绿/蓝顺序运行、同source换色、资源source替换和close真实通知；target锁失败sample归还重试；provider caller释放后resource保活；subscriber unregister与callback重入；同版本路径绑定；崩溃根因修复和完整无中止回归。普通测试未重跑。机器ledger仍RepairRequiredPartial，没有晋升Closed。

状态：本次未完成用户要求的完整可审计交付。阶段4仍Active；后续shader/float/D3DImage不在本次关闭。下一动作是合并完成上述media生产生命周期与全部验收门禁，并在崩溃修复后重新专用发布、定向及完整全量。
