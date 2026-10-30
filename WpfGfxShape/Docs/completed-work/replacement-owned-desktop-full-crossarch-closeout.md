# ControlledDesktopFull 双架构正式收尾

## 最新失败索引补充（无新运行、不扣红）
[official首次协议完整审查](replacement-official-cache-first-send-review.md)：未确证fixture少发必要命令；GetRenderTargetBitmap显示驻留/内部RT消费现场未观测。原两official FF80/0与x86 swap3旧灰三失败全部保留。后续9343单例的精确TestResults/child.trx现可读并已全文核证1/1/0/1、MTP2，testId14e862b3-9b71-843a-9ff5-67f779272c33/executionb229983c-3c5d-4728-8145-f58610649d20；native前置失败，不覆盖本full。旧缺TRX说法纠正，不产生Passed。

## 后续核证索引（不改变本full结果）
[剩余合同正式收尾](replacement-controlled-full-remaining-contracts-closeout.md)：新增task9343dc00同x64原包official单例native前置00563412/141414在connect前失败，cache未到；恢复/Active0/close成功，监督2，TRX回传缺件明确。原三红不减。x86 full原stdout给出submit41593198494928<LastPresent41593198668318<completed41593198889408，仍旧灰，独立output时间谓词不认证目标source帧。无生产/fixture修改或新full，预算build0/3 publish0/2 sandbox1/4；完整报告含官方契约、历史cache=true与真实下一必要observable。

## 终态
ExecutionCloseoutCompleted / ControlledDesktopFullFailed / OverallOpen，memoryGreen=null。本轮已连续执行授权两 full，无业务追绿重跑；并非默认 desktop full，也不覆盖旧默认红项。

|架构|task|total/executed/passed/failed|MTP exit|父 final|恢复/收敛/关闭|
|---|---|---|---|---|---|
|x64|512614cccc4642f0bf163ff5284a243d|1823/1823/1822/1|2|2|true/true/true|
|x86|5db5163c671d4958a6ce1f32a8859dd4|1823/1823/1821/2|2|2|true/true/true|

TRX error/timeout/aborted/notExecuted 全0。两监督 Reason=exited，TimedOut/Cancelled/KillAttempted/OutputDrainTimedOut/OutputDrainCancellationTimedOut=false，SupervisorError=null。工具只返回启动拓扑及保存路径，未提供独立 OSexit；不将 journal 返回冒工具 exit。

## 配置与范围
只改测试项目配置及 SandboxDesktopController：独立 ControlledDesktopFull 仅 SANDBOX_DESKTOP_DIAGNOSTIC 宏，不开 heap/ETW/sidecar；最小增加 full optin，既有 loss/official 精确过滤不变。源码 ExitDiagnostics.Parse 核定参数为 --supervise-exit --full ROOT 1500，父 wait1650秒，收敛最多两段10秒，外层1800秒。全部原 TestMethod/guard 原样，未新增测试、删skip、disableparallel或改expected；原1823分母全执行，未新增分母。生产/Native修改0。

绝对 COM 项目 net10.0-windows，RID win-x64/win-x86，selfcontained true，Core/Desktop10.0.13，显式 AotRuntimeIdentifier、AotDllPath 指向已有同SHA clean Native，ComAcceptanceSandbox=true，正式 ComAcceptancePackageDirectory=对应 clean。未发布根无aot，未为路径重建 Native。

## 全部失败 Result / Definition 索引
共同 class=WpfGfxShape.ComAcceptance.D3DImageTests，adapter executor://MSTestExtension/4.0.1。原件根为 artifacts/owned-desktop-full-{x64,x86}-r1/readback/Results；TRX在 controlled-desktop-full-{arch}/TestResults/child.trx。

1. x64 WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive，无参。testId14e862b3-9b71-843a-9ff5-67f779272c33，execution59f54638-fdcf-4ac2-988b-0dc6156f73b9。Result2309–2338，Definition10251–10254，TestEntry13570。真实 official AD1C；native expected563412=actual563412；创建shader HR0；首cache expected0000FF80/actual00000000。AssertCombinedPixel失败，RunVisualCacheConsumer→该TestMethod；未到 resize，不冒 resize合同通过。可见/非iconic/无遮挡/foreground同目标，thread与input均自有desktop。
2. x86 同一方法/testId，无参。execution52ad6500-a000-4eab-aae2-aeb8dcb3c228。Result2316–2347，Definition10259–10262，TestEntry13578。真实 official5EBD；相同native563412正确、shader HR0、首cache FF80/0；同一断言栈，未到resize。不能由单例绿推full绿，不能据有desktop就将剩余失败归外部竞争。
3. x86 WhenNativeOwnerThreadSubmitsRedBlueRedThenFreshDesktopContentIsRequired (3)，swapEffect3。testId19b3355c-10bc-80a4-b748-5cbfce845d4b，execution79472358-fb5e-4c9e-9748-69aeb69d493f。Result1897–1957，Definition10083–10086，TestEntry13534。Assert.IsTrue(passed)失败，CheckFreshDesktopDelivery→该方法。Results/bj-fresh-3.json全60行：首红 expectedFFFF0000/backbufferFFFF0000/Present0，但 duplicated/frontFF141414、screen00141414，accepted=false；第二蓝与第三红accepted=true，不相减。线程owner5788=submit5788；窗口可见/无遮挡/foreground自身，input/thread自有desktop。原生d3d9独立control，不是候选模块失败；该观测loadedModule/SHA=Unknown，不冒认证。

共同 official 首帧失败根因 Unknown。自有desktop不是full充分修复；不改expected、不重跑。

## 身份分层
x64 fixture SHA E08B135C7AF6813C70E7909BDB7D4762DAB88A644AD5148F1104CEA2B766A387，MVID b3744fec-14e3-4271-94c6-ef5b0a6857fd，runtime.NET10.0.13/X64。x86 fixture SHA36362CA9DC7124DF6A08D7CE32814C4D96C5FDA653B44F6720046F6A893A8E23，MVID2c1823f3-04db-44d7-9f4e-2e92d74e652a，runtime.NET10.0.13/X86；以上均亲读supervision，不由包名推断。

候选输入/clean aot.sha256：x64 6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403；x86 B07C9073F75278ED61C44638EB3644547B413A281E2859B25211CEAAC3D1466C。TRX historical candidate 的 Requested/Actual 均任务Native/wpfgfx_cor3.dll，实际相应SHA，x64 PresentCondition亦明确loadedModule相应路径/SHA。不是仅拿包hash冒全部角色actual。

official失败角色实际模块为任务根wpfgfx_cor3.dll，x64 AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1；x86 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834。版本10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。pcore x64 blur Baseline/result.json记录实际official AD1C、PresentationCore0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31；frozenCandidate6BDF是输入而非Baseline实际加载。不同角色逐项身份仍以原件为准，不宣称所有模块已逐一亲读认证。

## desktop/job 独立硬门
x64父12828→监督10124→MTP8704；自有 WinSta0/WpfGfxOwned_12828_1a85c234379c441fbc4254cceba4d289。x86父4996→监督2968→MTP3716；自有 WinSta0/WpfGfxOwned_4996_2c8f6372842c4e7080db5e70beba6f05。CreateProcessW flags4/STARTUPINFO.lpDesktop显式，suspended时AssignJob成功、SwitchDesktop成功再Resume；后代继承desktop/job，不设置breakaway。父始终Default，完整父环境继承，角色入口均有独立durable journal。已读MTP/trigger角色绑定与窗口观测，无匹配process-entry逃回Default记录；这不替代每个角色完整身份人工审计。

两arch job-reap均Total165/Active0/Terminated0，Query.final成功。SwitchDesktop.original true，OpenInputDesktop.restored true，核 Expected=Actual=Default/Confirmed=true。child-thread/process/job/owned/original句柄Close各true；最后Restored/Reaped/HandlesClosed=true，Result2忠实保留业务失败。三个门分别通过，非全部业务资源收敛证明。无全局WinSta/RDP/第三方窗口/驱动修改。

两full原real-loss方法均Passed：实际88760868→88760869→Reset0，triggerExit0且device/factory/window complete；仅受控原生fixture，不证明产品consumer loss。

## 预算与历史
本轮normalCOMbuild0/2；COMpublish2/2，隐式compile2；sandbox2/2；Native0/生产0/额外full重跑0。发布成功，x86日志exit0，存在既有warning；工具保留日志60行且原57570字符前37570截断，总warnings无法认证。x64返回成功日志60行，尾部warning存在，未保全被后续日志替换部分，不能虚报完整警告数。实际工具账 artifacts/owned-desktop-full-execution-receipt.md。历史两单例与旧默认full预算及所有旧红独立保留，不扣减。

原受控单例loss taskb08149f473a141f9b2e6e83bdb2936b6和official task90e5fa15e1344be8a44d501d4283407c均1passed/恢复成功/active0/关闭成功，已回填原desktop closeout，非待写草稿。

## 剩余主要验收与直接行动
1. 对照full失败与单例通过的 official exact resource/消费状态及真实WPF发送序列，定位可证确定差异后再决定最小测试或产品修正；当前无新增运行预算，不能再追绿。
2. 独立调查x86 swap3第一帧真实desktop交付，保留Basic Render Driver及原生control失败，不转嫁候选、不放宽期望。
3. 真实HW backend、product device-loss、audio、natural resource收敛及最终完整替换矩阵仍Open；受控loss绿或候选大多数绿均不足关门。
4. 后续证据审计需要按原件逐角色完善pcore等actual模块/MVID与desktop绑定全集；本报告明确未将抽样认证冒全集认证。无运行阻塞，但这项审计完整性仍非已完成。
