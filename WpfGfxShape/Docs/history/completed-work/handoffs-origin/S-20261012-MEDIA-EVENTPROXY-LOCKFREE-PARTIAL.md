# Media事件代理锁外回调：Partial，未完成media族

## 实际生产修改

`core/av/eventproxy.cpp` / `eventproxy.h` → `Core/Av/EventProxy.cs`：RaiseEvent锁内获取非零CAS引用与描述符快照，锁外调用callback，finally释放引用。Shutdown后返回S_OK不调用callback；最终释放不能复活。公开COM测试新增首个callback阻塞时第二个callback有界完成；并发测试不再把callback重叠误判为描述符提前销毁，仍检查Dispose时Active为零。

真实AVI公开Visual两像素FFFF0000仍通过。测试保留固定元数据预期(1,0,2,1)，只在测试中有界等待真实系统异步元数据，15秒未ready仍失败；不skip、不回填golden、不把生产轮询说成异步桥。

## 发布01

完整命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。
目标：Code/WpfGfxShape/WpfGfxShape.csproj；Release；win-x64。实际成功，退出码0，NativeAOT生成成功。
产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
报告实际回读SHA256：`4D5FD8A5B5FBF346E0AE855EEC1F56F1AA25544C2519A392AF11D1D6B78BB0C9`。

## 验收与最终身份

- Release/win-x64生产构建成功。
- 事件代理定向29/29通过。
- 元数据有界等待修改后MediaExportsTests及EventProxyTests联合定向成功；不虚构未读取的定向RunID。
- 首次全量1276通过/2失败/1278，RunID `52270e18-1ed9-4b1c-82cf-a9862bf037fc`：严格FLOAT既有失败与AVI元数据异步初态失败；后者通过上述有界等待修复。
- 最后代码修改后全量1277通过/1失败/1278，0跳过，RunID `5c6a6623-02b1-4108-bdcb-f960fb99b32e`。
- TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，文件仍可被下一运行覆盖；本归档保留RunID及结论，不声称已复制完整TRX。
- 独立报告：`Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-c74d39694b52449893ff8903f2102c6e.json`，同SHA；FLOAT57/450，Pbgra32 0/180。golden/阈值未变。

## 未完成事实与下一动作

用户要求的完整media族未交付：GeneratedMediaPlayerResource仍在AcquireSystemFrame中同步检测newFrame后NotifyComposition；其自身notifier不是system source异步事件桥。MILFactoryCreateMediaPlayer候选创建/source wrapper未实现，真实system事件→generated notification→composition请求未接通；颜色帧变化、媒体替换/close通知、target锁冲突解除后sample重试及成功/失败租约生命周期尚缺公开验收。不得认为本次29项事件代理测试完成这些条件，也不得宣称外部阻塞。

继续同一完整media模块，之后shader/source17float/D3DImage剩余依赖。阶段1–3 Closed、4 Active、5–7 Pending；Ledger RepairRequiredPartial，不能替换原库。不以本次局部修改领取media Closed。
