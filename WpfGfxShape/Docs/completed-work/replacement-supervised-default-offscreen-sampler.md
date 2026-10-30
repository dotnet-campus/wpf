# IY 默认独立离屏 sampler 回归

## IZ 终态：保留默认测试重构，双架构有限合同验收成立

裁决：保留 OffscreenSamplerTests 默认 group/direct、D3DImageTests.OffscreenSampler 默认 helper，以及 IW 显式 guarded 包装。IY 已经过关键门，不因历史尾项未全审撤销；本轮 IZ 为源码与既有原件独立核证，新增运行、build、publish、沙箱、脚本、Host、工程均为 0。仅补本报告和三份共享文档，不修改任何源码、生产、NativeRun/ExitDiagnostics、DR/DS、共享 VisualCache/historical、公共捕获、打包、CF/CX/CY。

这是把 IW/IX 原本已绿的 SoftwareOnly 合同纳入默认独立回归，不是生产旧红新绿，不是旧 HardwareOnly/window sampler、0xA 或 event 转交修复。旧硬件失败原样保留。Available/HWbackend/loss/mediaresources/full 四格仍开放，完整替换未完成。

## 1. 原件定位与实跑计数

以下路径相对 WpfGfxShape，R = artifacts/iy-default-offscreen/readback；输入包在 artifacts/iy-default-offscreen/{x64,x86}/com-acceptance-package，回收单独在 R，未把回收结果当输入重部署。

|架构|既有 default task|监督/测试 PID|父 TRX|Exit|
|---|---|---|---|---|
|x64|30cef74af86149aeadf1147c57c6f153|6088/4228|2 total/2 executed/2 passed/0 failed|0|
|x86|4ec0c0cd78da4c569e4c288f73361ff0|5720/11128|2 total/2 executed/2 passed/0 failed|0|

父 filter 精确为 FullyQualifiedName~OffscreenSamplerTests；各 TestResults/child.trx 两项是 WhenOffscreenSamplerGroupRunsThenPixelsAndUpdatesMatchOfficialAsync 和 WhenOffscreenSamplerRoleRunsThenConsumerPixelsChange，并非仅 group 的两条伪角色。supervision.json 各39行、父TRX各32行全文亲读：Reason exited，HasExited/OutputCollected true，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut false，SupervisorError null。外层非MTP监督不另造父TRX或工具exit。

G64 = R/x64/Results/iy-x64/offscreen-sampler/1ddb61958b0046cd930b3d4bb297c5f9；G86 = R/x86/Results/iy-x86/offscreen-sampler/3bcb435b17814e41a61602de59f60c14。

|架构/role|role-exit 中监督PID|真正测试PID|精确 role TRX|监督与role Exit|
|---|---:|---:|---|---|
|x64 official|1128|2232|1/1 executed/passed，0 failed|0/0|
|x64 candidate|10896|4208|1/1 executed/passed，0 failed|0/0|
|x64 official-repeat|4820|3828|1/1 executed/passed，0 failed|0/0|
|x86 official|3360|9912|1/1 executed/passed，0 failed|0/0|
|x86 candidate|10176|10032|1/1 executed/passed，0 failed|0/0|
|x86 official-repeat|11220|1132|1/1 executed/passed，0 failed|0/0|

每行来自 G64/G86/<role>/{role-exit.json,supervision.json,TestResults/child.trx}，六份退出记录、六份39行监督和六份26行TRX全文亲读。三个业务进程PID独立，不混监督PID；精确filter均为 FullyQualifiedName~WhenOffscreenSamplerRoleRunsThenConsumerPixelsChange，监督30秒，group等待35秒。上述timeout/kill/drain均false。

默认无 IY_OFFSCREEN_ROLE 时实际选择 candidate，不空返回；direct在父测试PID4228/11128执行，父TRX各有对应Passed，stages.jsonl各7行将身份到finally绑定该PID。全部业务JSON：六份 G/<role>/sampler/evidence.json，加 R/x64/Results/iy-x64/offscreen-sampler-direct/2ce337003a164df18ac0324a24c1982e/evidence.json 和 R/x86/Results/iy-x86/offscreen-sampler-direct/99ba8d0a007b4af4be3b2a3b6fa381da/evidence.json，各96行全文亲读。direct没有独立子进程/TRX/exit，不能重复计为额外task。

## 2. 内容、更新、清理与精确比较的证据层次

源码两默认文件全文及IW包装全文已读。两个16×16 WriteableBitmap生产：first固定FF0000FF，second从FF00FF00原对象更新为FFFF0000；DrawingVisual/ShaderEffect与s1 VisualBrush，独立RenderTargetBitmap.CopyPixels读取consumer，不是源backbuffer/event。PixelShader明确SoftwareOnly；原 TwoInputShaderTokens ps_2_0共76个uint/304字节，来自未改的共享VisualCache常量。

八份业务记录均 Threshold=0、Backend=SoftwareOnly；初态Actual仅FF00FF80/Expected RGB0000FF80，更新Actual仅FFFF0080/Expected RGB00FF0080，两态Nonempty=true。helper逐个比较全部256像素的RGB，不是采点；两个非零且不同expected排除空白与旧帧。group对shader.bin初始长度304、initial.bgra/mutation.bgra各1024进行长度断言，并对candidate及official-repeat分别CollectionAssert全字节相等。两个group真实Passed结合已读源码确认这些精确比较已执行；不是只继承role Passed。

比较记录边界：当前源码没有另外写comparison.json或哈希比较清单；精确比较证据为父group TRX与对应源码，不伪称另有逐字节日志。八套shader.bin/initial.bgra/mutation.bgra回收路径已列目录核存在；IZ未独立解码或重新哈希这些二进制，没有新增诊断程序。direct也不额外进行跨进程全BGRA比较；它执行全RGB expected检查。不能将本次审阅冒称所有二进制原件独立全审。

八份业务均Effect/VisualBrush解绑、TargetCleared=true；8个弱引用（两bitmap、两visual、effect、brush、target、shader）全false，DispatcherShutdown/FinalizersDrained=true；NativeComRefsMeasured=false、ModuleRetained=true。源码先detach，再shutdown及GC/finalizers，再assert弱引用全false，finally再次收尾。这仅证明有限托管场景清理，不证明native COMrefs0、长期内存/GPU资源趋势、callback或device-loss收敛。

真实断言缺口/风险登记（不隐瞒、不将其扩大为本次已发生失败）：Nonempty是记录字段而非单独Assert，但全数组非零expected已经保证非空；finally完成布尔也是记录而非独立Assert，亲读原件为true。actualNative断言模块唯一且实际路径与selected相同，但并未用独立预期SHA作断言；本轮亲读真实SHA补身份核证。group没有单独验TRX方法名/failed=0，但执行/通过均1，IZ已亲读六份方法名正确且failed0。

default direct安装PresentationCore/WindowsBase进程级DllImportResolver，Native模块驻留并关闭当前dispatcher，不能在同一进程恢复resolver。当前独立default task/direct已经真实通过；任意与其他WPF测试同进程顺序、已安装resolver、已有dispatcher状态的完整集成闭包未证明。保留独立进程使用边界，不声称全量同进程安全或新增full避开待修崩溃。本轮只报告风险，不在无重验预算时尾改源码。

## 3. 实际身份（全部取业务identity/actualNative及监督，不取预期文件名代替）

绝对任务根 T64=C:\CodingAgentSandbox\Tasks\30cef74af86149aeadf1147c57c6f153；T86=C:\CodingAgentSandbox\Tasks\4ec0c0cd78da4c569e4c288f73361ff0。每架构official/repeat实际模块均T\wpfgfx_cor3.dll，candidate/group与direct均T\Native\wpfgfx_cor3.dll，actualNative各仅一模块且真实路径断言通过。

|身份|x64 SHA256|x86 SHA256|
|---|---|---|
|IP candidate Native|0C16A66C898269A17DBDD062012898FEBE338E341FED8D0B7FEC47FB21B6A23A|63B91E6CD6B5C1E8D0095E1673933DD5BFC539744F7EC36DC06C8DCB074E3136|
|官方同架构Native|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|
|T\WpfGfxShape.ComAcceptance.dll|B341D2EE11FC6D051A45E9CE14BDA8D21B73CB763381169FEFC7E25B93BD2418|CFFAB48988D3A3982F6FE715A15778E11198A4B7CDC1DAC0C98B2DE3678168CF|
|T\PresentationCore.dll|0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31|D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D|

Test MVID x64=99ac2275-8d43-4913-a007-db1708ef759d；x86=cc06de1e-3ff9-4cdb-bd42-c99e5a49c289。WPF MVID x64=6cb5dedb-eea3-4832-9a08-c0225e9ff82f；x86=2934bccd-58e4-4afe-8677-ec985888f34e。

两架构Runtime .NET10.0.13，Architecture分别X64/X86；官方Native 10.0.13-servicing.26504.104 @Commit:4099f83977f4d29604a9aeb971fb0a7bc1d5f699；PresentationCore同servicing+同commit。candidate/test产品版本1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a。回收runtimeconfig全文均includedFrameworks Microsoft.NETCore.App与Microsoft.WindowsDesktop.App10.0.13，self-contained，不混旧10.0.1/IW测试MVID。IP Native未重发；不能据此把旧full绑定新测试SHA。

包aot.sha256两份全文已读与actual candidate完全一致。package-files.txt x64共419行、x86共418行，仅首14行和末20/19行亲读：正式exe/dll/config、末尾官方模块、Contracts两项、Native候选与aot清单存在；中段runtime/资源清单未逐项全审、未逐文件重新哈希。输入与readback目录已区分，未声称整个包manifest全校验或IL方法体全审。

IY正常default build日志：在本轮定位的IY产物目录与artifacts/log未取得可核正常build日志，不编造Exit/warnings/errors/耗时，不新跑。历史IW/IX normalbuild分别见原IW报告，不能冒充IY。IY原阶段声称正常分析器独有发布成功，由实际运行self-contained身份支持可执行性，但不等于本轮亲读完整发布日志。

## 4. 相关兄弟：真实7/7，非完整产品闭包

|架构|task|监督/测试PID|父TRX|Exit|
|---|---|---|---|---|
|x64|064759a314134b63ba133bfb6dc13e0d|10032/10488|7 total/executed/passed，0 failed|0|
|x86|fcd1817a1bb248309d33feec40f01ecc|2848/9420|7 total/executed/passed，0 failed|0|

对应R/siblings-<arch>/Results/iy-siblings-<arch>/{supervision.json,TestResults/child.trx}：监督全文、TRX结果行/计数已读，timeout/kill/drain全false。真实filter是下列五个FullyQualifiedName~方法名以|连接（不是full）：
- WhenBitmapTargetBeginsNativeFrameThenInternalTargetCanBeAcquired：defaultBitmap 1项；
- WhenSoftwareOnlyShaderUsesConstantThenGenericTargetConsumesCpuPixels：3个数据行(0.25,4282384384,False)/(0.5,4286578688,False)/(0.5,8388608,True)；
- WhenHyDeviceStateGroupRunsThenUnrenderedSnapshotsContainNoDeviceAsync：1项；
- WhenListenerBroadcastRunsThenEveryRegisteredChannelMatchesOfficialAsync：1项；
- WhenPartitionPresentStateRunsThenDynamicStateMatchesOfficialAsync：1项。

每架构另外三个group各official/candidate/official-repeat真实子角色：HY各1 executed/1 passed/Exit0，Listener各1/1/Exit0，动态state各1/1/Exit0，合9个角色结果；不与父7重复当完整16产品用例。HY和Listener精确role.trx在Results/{hy-payload,listener-broadcast}/<role>；动态在Results/iy-siblings-<arch>/partition-present-state/<guid>/<role>/TestResults/child.trx（x64 guid6f0e9dd3f88e46bf863b6a60f4bb6586，x86 guid443264c160f7471199f21bb357b3a508）。HY/Listener计数经定向原件搜索读取，动态六份计数直接读到1/1/0failed；不称18份role TRX全文全审。

按official/candidate/repeat顺序的真实业务PID：x64 HY1132/6372/7112、Listener7940/10376/9204、动态11132/10568/3156（其监督9980/7076/11004）；x86 HY9496/4348/9436、Listener6008/10448/3648、动态8412/3576/7776（监督11220/6496/6220）。来自business、exit、stages/supervision定向读取；退出全0。兄弟test SHA/MVID同本架构default，实际candidate/official对应本报告同Native SHA，路径改为各兄弟task根。

HY业务头部六份亲读：layout64/oldOffset8/newOffset12、counts[1,0,0,1,0]、type5 old/new1/1完整64-byte样本；cleaned=true已定向核，两份尾部直接读destroy/disconnect/deinitialize0。Listener六份阶段定向核包含重复enable/disable、windowless广播和destroy后订阅；两份头/片段/尾直接读，cleaned/joined true、windowError null，targetCommandsSubmitted0与actualStateTransitionCovered=false，不能拿它冒动态设备恢复。

动态state两candidate身份、首帧consumer255=expected255 Accepted=true、NoDevice1→0完整消息、尾部delete/flush/destroy/disconnect/deinit0与Bitmap/Surface/Device/Factory refs0已直接读；六份各三consumer记录存在已定向核。它记录ProducerReadback=false、HardwareConsumerProven=false，不能拿有限窗口内容/state或该夹具refs0填补离屏sampler NativeComRefsMeasured=false。

额外角色的大数组/完整事件序列未全审：Listener各1230行、HY各137行的全部HRESULT、动态各128行的全部帧/事件/所有权未逐项阅读全文。兄弟7parent只保留真实有界回归计数，不宣称全部业务数组、全产品闭包、HW backend或长期资源验收完成。

## 5. 默认闭包、历史范围与下一唯一门

默认helper闭包为RunOffscreenSampler→RenderOffscreenSampler/IwEffect→TwoInputShaderTokens；不调用旧窗口RunVisualCacheConsumer、IS实验或IV底层Send/event协议。IW两个显式入口仍全文件#if IW_OFFSCREEN_SAMPLER；IV_ISOLATED_SAMPLER、IS_SHARED_CONSUMER_EXPERIMENT全文件guard已读，其他探索guard定向源码搜索核仍在。不把默认helper名中的IW阶段标签当作旧guard泄漏。源码级隔离成立；本轮未重新反汇编包验证全公开元数据。

历史核证范围：执行规则全文27行、完整方案分段1–120行、稳定基线20行、三个默认相关源码全文、共享shader常量1–38行、相关角色模式指定片段、IW/IX报告顶部及旧共享最新节；没有千行长历史全审，旧记载保留，不把历史报告自述当本轮新运行。

下一唯一门：旧window官方sampler/0xA AddRef与event副本的所有权安全闭包静态准备，明确Send→Close→Commit→处理或丢弃→drain→回收及失败责任；未闭前不启用旧探索为默认，不新full绕过待修崩溃。当前COM full仅可准备适用清单与身份/回收合同，不能宣称已运行或用旧full补当前四格。完整替换未完成。

## IY 阶段记录（历史保留，已由IZ终态收口）

原目标：把 IW/IX 已验收的 SoftwareOnly 离屏合同纳入默认独立 parent/role 产品回归；保留原始shader、零阈值、expected、同一资源更新和有限托管清理，官方/候选进程隔离。生产不修改、不重发；不关闭旧硬件/窗口/event所有权失败。

原阶段记录：先读取规则、完整方案和现有Hy/广播/动态state角色模式，随后抽取默认helper，IW显式入口保留guard。默认公开group/role元数据已核，正常分析器Release x64/x86 self-contained10.0.13独有正式包发布成功，aot.sha256与当前IP候选一致，生产未重发。两架构各一次default group+direct candidate实跑Exit0、精确父TRX2/2；group实际逐字节比较原304字节shader和两态1024字节像素，并非仅看role Passed。兄弟有界组各7/7，原阶段身份/清理/监督回读进行中。Native COM refs未测。此处保留当时叙述，不表示IZ新增运行或完整日志审计。

## IZ 最后补记

最终保留裁决不再待定：双架构默认独立离屏sampler内容/更新及有限托管清理通过，保留测试重构；默认全量同进程组合、显式完成断言、二进制独立解码与兄弟大数组全审边界已登记，不隐去风险也不撤销已过关键门。报告全文及handoff/session-summary/issues最新IZ节与本补记写后回读；历史范围仅如上，不声称全部旧报告全审。无新增执行、源码尾改或身份扰动；Available/HWbackend/loss/mediaresources/full四格仍开放，完整替换未完成。
