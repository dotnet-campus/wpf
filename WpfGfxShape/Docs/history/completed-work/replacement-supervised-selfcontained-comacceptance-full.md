# DP：当前源码双架构自包含 ComAcceptance full

## 边界与状态
- 本轮仅新增本报告及 artifacts/dp-comacceptance-full/ 独有证据；不修改生产、业务、入口、打包项目或共享文档。
- 目标：正式默认 COM 项目 Release，x86/x64 各一次无 filter full；优先正常解析的同版 10.0.13 自包含，不硬改 SDK/TFM/runtime。
- 历史初稿状态已失效：两架构均已发布并各执行一次真正默认无 filter full，均原生异常退出，未生成父完成 TRX，不能声明 full 完成或验收通过。DQ 独立审查续记见 replacement-supervised-current-full-crash-review.md。
- 禁止终端、脚本、新 Host/第三工程；仅专用工具和 Windows 沙箱正式纯 C# exe。

## 执行记录
1. 已定位正式项目，读取规则、交接及 DO/DM/DL/DB/DE，正常默认发布，无探索符号、生产重发或代码修改。
2. x86 自包含正常解析.NET/WPF10.0.13；官方输入任务4b9ce2d1a29543ae92f7cb5d84fb5fa2 Exit0；清洁最终包审计任务b670eb75b0de471cb7b68ee0d5405506实测147788743字节。
3. x86唯一full任务624f2b6cf6f54172af4894312e8908b9完整回收full-x86。监督全文已读：full/1500秒，无filter，09:52:25.1809266Z–09:53:14.2923564Z；Exit -1073741819 / 0xC0000005，无timeout/cancel/kill/drain超时。DQ已完成下节所列TRX/业务关键证据补核，不据最后用例猜唯一根因。
4. x64 已完成清洁上传及唯一 full：task 2f63b2fd89e7405fa076d5faa1a65464，结果 full-x64，Exit 0xC0000374。两包约 147788743 / 157457338 字节，清洁上传成功，不是传输失败。两次均非 timeout/cancel/kill，父完成 TRX 未生成；子角色 TRX 不拼接为 full 计数。以上为已发生事实纠正；DQ 已亲读补核，详见独立报告。

## DQ 最终原件补核
- 两监督各37行全文：Mode=full/1500秒、Arguments无filter，均exited、双流完成，无timeout/cancel/kill/drain异常，stderr空。x86监督3708/父11180，09:52:25.1809266Z–09:53:14.2923564Z；x64监督1256/父4920，09:54:34.5404975Z–09:55:32.7976081Z。
- x86 testSHA DEFB218C8B4A7ED8A7456CEF6D5BB95B38EFC090B357E70844C2B93710A14526，MVID63d513f8-e18c-4917-998c-1f5a8bcb3e02；x64 SHA48AA0616596E5A8B9C1B7FA5078724F7BEDC08FC9C187E55E1E3E981E5848921，MVID61ec3ff1-90d1-422b-a4ba-ed92a157a715；runtime均.NET10.0.13。
- x86同步diag至14761 EOF最后执行Stage4HardwareCacheGeometry首DataRow，stages全16行最后InteropDeviceBitmap_Create before，无after；x64至15773 EOF最后OfficialGeometry，stages1005行末为首次Commit E_HANDLE→cleanup DestroyChannel before，无after。不是同一最小崩溃复现，不将Destroy当首次坏写。
- 双架构CombinedGeometry/DrawingImage与tile的present(connection)=0x80070057为已观测业务失败，不直接称崩溃根因；x64另有BJ swapEffect3首红桌面交付失败。未导出全UID去重计数，不造full总数。
- 两架构各两份source-clip角色TRX全文亲读，各Completed/1执行1通过、异常计数0，不能拼成父full；父绝对TRX为各任务Results/dp-full-{arch}/TestResults/child.trx，均无完成文件。
- 顶层aot-identity被BW official-repeat覆盖，不据其filter断父误跑。候选包SHA7B590883…/F34DEAB0…与actual消费分层；x64实际官方module为任务根、SHA AD1C9DBB…/Product10.0.13-servicing.26504.104，候选场景另有Native/F34D…记录；x86source-clip角色官方SHA5EBD18E0…可核，但最后调用actual模块快照不足。full实际PresentationCore SHA/版本/加载路径未闭，不冒称全身份认证。
- 唯一后续为RunVisualCacheConsumer独立纯C# ABI/事件所有权夹具切片，认证Create/Available及异步DuplicateHandle初始/mutation合同，不主动重入官方悬空batch；CL源码UAF不绑定servicing因果，无已确认生产修复。DQ无新执行、不再full，不改DP_FULL_READBACK或压制规则，专属build未修不称通过。
- 保留CF/CX/CY fill；BW有限Passed亲核，原44/10具体frame本轮消费未认证。GPU/loss/resize/media/resources/线程callback/通知和最终双项目双架构四格仍开，完整替换未完成。
