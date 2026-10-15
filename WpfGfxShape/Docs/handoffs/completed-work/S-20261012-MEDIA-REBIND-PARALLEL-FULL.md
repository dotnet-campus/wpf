# Media同源重绑修复与并行完整回归（Partial）

## 修改及真实覆盖

- GeneratedMediaPlayerResource.cs：同源重复绑定原先先Subscribe再Unsubscribe导致HashSet删除当前订阅。现于资源锁内先注销旧来源，再安装并订阅新来源；保留旧media逆序释放。
- GeneratedMediaPlayerResourceTests.cs：成功路径检查删除前References=1/Releases=1，删除后References=0/Releases=2，NotifyUceDirect=true、ChangeCount=1。批次失败改为明确QI E_NOINTERFACE，检查首包两引用归还及次包未执行/未释放；不是全部改S_OK。
- MediaExportsTests.SolidFrame.cs：新增同一player重复绑定后Open三个真实AVI来源，实际system callback等待驱动公开Visual消费，各帧完整两个像素分别红/绿/蓝及固定元数据。用例已定向及全量运行通过。
- 现有新增parallel实际运行成功：三路各红/绿/蓝，公开COM消费者；完整回归亦执行。既有15次顺序创建关闭也在媒体定向与完整回归执行。

## 本轮实际发布

两次均使用命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。

目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64 NativeAOT；均成功。

1. 修改生产订阅顺序之前，SHA CF6272750089A40994A72DFE198D7C95AB6B6088044C9A28E547756628025257；COM全量1282通过/1既有失败/1283，RunID ccc12668-6101-4d98-9f00-ee7d8e8000ba；report wpf-calibration-32809b0e5cf54c67b1d6a9c3bf0252f8.json。
2. 同源订阅修复后，最终SHA 0D661C9C2290E6904729844269FEBC1D4319B3AAF24371D990F500B1A80064B8；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll（项目默认发布根）。最终报告实际回读同SHA。

## 最后生产与测试修改后的验证

- 新增同源重绑/来源换色定向通过。
- 普通full：4182通过/0失败/0跳过。
- 媒体与Published名称筛选：53通过/0失败/0跳过，RunID 188c0d34-0917-48f4-9dca-e3b2a22b6b3e。此数为真实筛选结果，不声称Published筛选一定命中SHA专用测试。
- COMfull：1283通过/1既有FLOAT校准失败/1284总计/0跳过，无abort。RunID 4a35af6b-5264-4c7a-a50f-a0412680d4f3。
- 最终report：Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-122032fe03224b9eb7409ab74fb9f9ee.json；回读SHA同最终发布，FLOAT57/450，Direct0/180，AotBitmapMismatches=0。golden/阈值未修改。
- 原始TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx（会被后续运行覆盖，未制作独立TRX副本）；本文件保存独立RunID/report映射。
- 新增发现数：本轮新增1个COM测试实例，最终full比前次1283增加为1284；两项普通测试为修改旧实例，普通总数仍4182。parallel为恢复时已存在，本轮实际运行，不重复记作本轮新增。

## 未完成门禁及状态

完整媒体模块尚未交付：真实close事件语义、target锁失败sample归还后重试、provider caller_release后resource存活、subscriber unregister/callback重入的完整公开矩阵未补齐；system版本选择目前仍扫ProgramFiles最高版本，尚未校准用户运行时绑定/排除候选自身路径；模块静态句柄进程级保留，尚未有逐租约callback结束证明。0x0 sample已读取GetSize HRESULT后才按空尺寸跳过，但EndRender失败传播仍需完整验证。

不得将parallel像素成功当作全部thread/apartment/ref/event寿命证明。本轮没有在最后添加未运行测试。阶段1–3 Closed，4 Active，5–7 Pending；media Partial，Ledger RepairRequiredPartial；不可替换原库。继续完整media生产与上述门禁，再shader/FLOAT/剩余D3DImage。
