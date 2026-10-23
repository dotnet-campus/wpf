# DQ：双架构 full 原生退出独立审查

## 裁决及范围
两次真正默认无filter full均原生异常退出，未生成父完成TRX；没有共同最小崩溃复现或生产根因闭环。本轮采用先交审计分支，新增运行0，不再提交full或上传大包。仅文档及 artifacts/dq-full-crash/audit.json，不改生产、既有测试/诊断、入口、打包、默认并行、阈值、golden、skip。先纠正DP已发生事实再审查。
已读执行规则27行、完整方案120行、BN86行、CL87行全文及相关夹具。历史源级UAF不是已安装servicing因果证明。当前没有AGENTS搜索命中。

## 监督与完成边界（亲读）
两份 Results/dp-full-{arch}/supervision.json 各37行全文：Mode=full、1500秒，Arguments无--filter；开启同步diagnostic，绝对TestResults/child.trx。Reason=exited、HasExited/OutputCollected=true；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，SupervisorError=null。两份stderr为空。
|架构|task|监督/父PID|UTC 2026-10-21|实际退出|
|---|---|---|---|---|
|x86|624f2b6cf6f54172af4894312e8908b9|3708/11180|09:52:25.1809266–09:53:14.2923564|-1073741819 / 0xC0000005|
|x64|2f63b2fd89e7405fa076d5faa1a65464|1256/4920|09:54:34.5404975–09:55:32.7976081|-1073740940 / 0xC0000374|
远端固定父TRX：`C:\CodingAgentSandbox\Tasks\624f2b6cf6f54172af4894312e8908b9\Results\dp-full-x86\TestResults\child.trx`；`C:\CodingAgentSandbox\Tasks\2f63b2fd89e7405fa076d5faa1a65464\Results\dp-full-x64\TestResults\child.trx`。本地整任务为 artifacts/dp-comacceptance-full/full-{x86,x64}，递归只找到各两份source-clip角色TRX，无父完成TRX。不能编造full Counters/通过率/RunInfo，不拼角色计数。
包审计 audit-{arch}/Results/dd-upload-boundary.json 1–14亲读：147788743/157457338字节，测试SHA/MVID与监督一致。DP清洁上传并已启动执行，不是传输失败；DG历史未知不覆盖DP。

## 已核失败类别与异常阶段
- 双架构stdout和同步diag可证 CombinedGeometry/DrawingImage多DataRow在present(connection)返回-2147024809/0x80070057断言失败；代表sourceMode7/8与tile4、source12/13/14、22浮点路径。WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements同样经此方法失败。这是业务失败，不是原生崩溃根因证明。完整参数保留原stdout；本审查没有完整UID去重计数，不冒称全部Failed总表。
- x64另有WhenNativeOwnerThreadSubmitsRedBlueRedThenFreshDesktopContentIsRequired (3)失败：stdout295–312及bj-fresh-3.json全60行交叉。首红backbuffer正确、present=0，但duplicated/front=4287345613，expected=4294901760、accepted=false；后蓝/红通过。Basic Render Driver，不等于真实物理GPU验收，不归罪于fill。
- x86同步diag14757–14761至EOF：最后执行WhenStage4HardwareCacheGeometryDrawsThenHitAndMutationPreservePixelsAfterCallerRelease (4278255360,65280)，无结果。stages全16行至EOF：09:53:09.6541856Z surface0 ColorFill后、InteropDeviceBitmap_Create前，无after。Stage4Hardware.cs:7–11调用候选默认路径，不是BN首次Commit失败阶段；没有原生栈，不能认定回调调用约定或首次坏写根因。
- x64同步diag15755–15773至EOF：最后执行WhenHistoricalCaptureOfficialGeometryRunsThenExpectedUpdatesRemain，不是sampler末项。stages尾980–1005：两surface创建返回，09:55:28.2876294Z首次Commit=0x80070006，随后cleanup destroy channel before，无after，最终堆损坏。Destroy只是最后边界，不是首次破坏位置。
只有共享RunVisualCacheConsumer家族，没有已证同一最小崩溃场景。不得猜filter全集或据断言跳cleanup。本轮不追加filtered对照。

## actual身份与包文件证据分层
父运行时两监督均.NET10.0.13、MSTest4.0.1见stdout。测试身份如下：
|架构|测试SHA256|MVID|
|---|---|---|
|x86|DEFB218C8B4A7ED8A7456CEF6D5BB95B38EFC090B357E70844C2B93710A14526|63d513f8-e18c-4917-998c-1f5a8bcb3e02|
|x64|48AA0616596E5A8B9C1B7FA5078724F7BEDC08FC9C187E55E1E3E981E5848921|61ec3ff1-90d1-422b-a4ba-ed92a157a715|
包审计候选SHA：x86 `7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA`；x64 `F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7`。这是包文件/配置证据。顶层aot-identity两份全文明确被BW official-repeat角色覆盖；其filter不是父full误跑，其AotDllPath/SHA也不是角色actual消费候选证明。
x64实际官方：Results/hw-module-2f63b2fd89e7405fa076d5faa1a65464-3e3b1d1fb8af406a9aed99d474cc47d0.json全10行，Requested=Actual=任务根/wpfgfx_cor3.dll，SHA `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，File10,0,1326,50504、Product10.0.13-servicing.26504.104、commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699，测试SHA匹配。父diag15760的PresentCondition另记录Native候选路径/F34D…，只支持该候选场景，不代官方身份。
x86 source-clip角色业务JSON亲读NativeDll为任务根，SHA `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`；未找到该full的hw-module JSON，最后Interop调用没有actual模块快照。不能把包SHA升级为最后调用actual证据。
各两份角色TRX全文30行：均WpfGfxShape.WpfReference.ReferenceBehaviorTests.WhenFiniteShapeIntersectsSourceClipThenOriginalCoverageIsRecorded，各Completed/1执行1通过，其他异常计数0，codeBase为各自任务测试DLL，ResultFiles指向各自source-clip生成JSON。x86 RunId57ad3f7c…/c0548702…，x64 d1d96836…/f95f1e47…。它们不是父full、不是BW角色TRX；stdout两个Passed汇总只来自这些角色。
审计WpfAssembly指向自包含PresentationCore.dll，runtime10.0.13；未亲核full实际PresentationCore版本/SHA/加载路径，不声称.NET/WPF所有actual身份都闭合。

## 唯一可信下一最小根因任务
BN官方sampler首次Commit E_HANDLE→Destroy异常与本次x64形态相容；CL仅证明当前仓库官方同线程失败delete后closed槽未清、Destroy SyncFlush可重访的源级UAF，不能绑定servicing二进制或认定DP因果。候选不应复制该危险行为。
亲读VisualCache.cs:35–50、75–125：RunVisualCacheConsumer的InteropDeviceBitmap_Create及Available回调声明、connect(1)、ImagePacket转交前AddRef、PresentPacket.Event默认0；Stage4Hardware与CaptureHistoricalControls调用参数已交叉。唯一下一任务为独立纯C# ABI/事件所有权夹具切片：先认证Create实参、返回对象与Available回调合同，再对照真实PresentationCore异步DuplicateHandle转交以及初始/mutation两个PresentPacket。保留合法内容断言、完整cleanup与BL监督。在形成可区分且不主动再入官方悬空batch的方案前，不重复现有官方失败路径。
当前需要诊断夹具切片而非已有可确认的生产修复；没有足够阶段证据选本轮一次filtered共同最小复现，按允许的先交审计分支停止运行。
DpFullReadbackTests.cs全50行已读，DP_FULL_READBACK显式隔离下两处Regex.Match仍在。本地原件足以裁决上述边界，无需为辅助上传巨目录；未改诊断或压制SYSLIB1045，专属build未修，亦不声称通过。本轮无构建/发布/运行，默认不变。

## fill、覆盖边界与交付
BW两架构父diag2870–2880均亲读Passed，仅证明BW有限测试执行完成。原44/10的本轮具体frame比较原件未取得，不计为本轮消费或重验全0。CF/CX/CY已验收fill修复保留，full崩溃不证明其失效。
GPU/loss/resize新帧/media/resources/线程callback/通知/Event0/E_HANDLE/首次堆破坏及最终两个项目×双架构Release四格仍开，不能称完整替换。
更新DP、本报告及三份共享文档最新节，历史保留；机器可读摘要artifacts/dq-full-crash/audit.json。只读本地原件，不改写原结果。大diag仅关键段与EOF定向审查，stdout有长行截断后补关键类别，不冒称两大现场逐字全文审完或取得完整失败总数；以上裁决边界已核，不以不存在的full计数关门。
