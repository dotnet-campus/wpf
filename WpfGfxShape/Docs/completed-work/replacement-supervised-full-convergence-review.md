# JI：正式 full 收敛独立监督审查

## 审查标准（先于结论建立）

1. 父进程必须运行正式 default、无 filter 的全部原适用测试和 DataRow；仅明确 native 危险边界可用同 exe 子进程承载。不得 Ignore、skip、筛绿、软件 fallback、放宽内容断言或减少分母。
2. 子进程必须真实执行目标方法和原参数，不递归；发现零测试、角色不匹配/空 return、缺 TRX 均不能 Passed。父成功必须同时核退出、监督、目标身份、目标 TRX 和原业务内容。
3. native 异常退出、timeout、缺 TRX 必须使对应父测试 Failed，保存原始退出码、stdout/stderr、方法/参数、PID/模块身份。父继续其他测试；kill、wait、drain 有界，清理错误不能静默丢失。
4. 隔离提高运行韧性，不修复被测产品。已知违规 Event=0 不因隔离变成合法协议；合法 async 修改需 SDK 依据且保留原内容/更新断言。OS 回收不能冒作业务 COM refs0。
5. 父 full Completed/退出2且有 Failed 是有效失败清单，不是完整替换通过。HW、恢复、长期资源和其他关键缺口如实开放；不要求先证明全部未来场景才能启动本轮 full。
6. 两架构身份独立；托管历史4256和最终新测试源码不能混绑。包清洁、ReferenceData同版、actual module SHA、test MVID、runtime 来源只按实际审阅程度报告。

## 范围与状态

只读代码与已有证据/报告，仅写本文件；构建/发布/测试/沙箱/脚本均0，不修改测试/生产/共享交接。执行规则27行、完整方案1–120已分段补齐；读取最新交接首节、IP/IQ当前full报告、IO托管报告、IR/IT/IS危险场景报告、IU合同和JD/JE隔离报告相关段。工程为net10.0-windows/WPF、MSTest4.0.1、Nullable；global SDK10.0.107。项目根AGENTS名称搜索无命中，不冒上级规则树全审。

JH报告读取两次仍仅阶段1/8行，但实际readback已有x64 full。以下以文件实物为准，属于**阶段静态审查+x64局部运行核证，不是JH双架构终审**。本轮不等待/阻塞JH，不覆盖其报告；行号是读取时快照，并行后续变化需重新核对。

## 1. 最先可复用的设施与三个具体陷阱

### 可直接复用，不需要新诊断框架

- `NativeRun.cs:20–21,35–40`已分流同exe监督与正常MTP；`ExitDiagnostics.cs:24–44`显式full生成零filter参数，filtered仍保留旧合同；`ExitDiagnosticsTests.cs:15–22,76–85`已有full无filter/通配仍filtered断言。正式父用full，内部危险role用精确filter，两个层级不冲突。
- `ExitDiagnostics.cs:65–118,149–156`已有test SHA/MVID、runtime/architecture、业务PID、原退出码/hex、同步stage、独立stdout/stderr和finally落盘；预算业务1500/内部90，外层留清理余量即可，无需另造Host。
- `OffscreenSamplerTests.cs:33–51,87–115`可复用“缺role真正起candidate、明确role才执行业务”、独立ProcessStartInfo.Environment、TRX executed/passed和业务内容/PID检查。`TestSettings.cs:1`既有DoNotParallelize不需要改。JD/JE说明STA线程并不能隔离进程级resolver和dispatcher；默认parent不得安装resolver或shutdown业务dispatcher。

### 陷阱A：零发现、空角色和参数偷换（当前两方法防线基本成立）

`HistoricalProcessIsolation.cs:11–17`角色必须等于方法、写receipt后真实scene()，无角色走spawn，不是空return；`:22,30–31`精确FQN且同exe，子role不再spawn。`:63–84`要求退出0、receipt PID对应业务PID、存在TRX、executed=passed=1、唯一Passed结果及精确class/method；因此discovery0/缺TRX不会被当Passed。

`CaptureHistoricalControls.cs:8–15`原candidate五DataRow仍在父进程，`:18–27`只隔离两个已知危险official无参数方法。geometry仍true/true/false/FF00FF00/0000FF00，sampler仍true/false/false/FF00FF00/0000FF80；`:38`仍geometry||cache分派，不偷换sampler为geometry。当前helper只能证明这两个无参方法；若后续隔离DataRow，必须逐行传/回读完整参数和目标行身份，不能把全部行压成一个角色Passed。

执行检查：父TRX保留五个candidate UID+两个official UID；对每个official核receipt PID、子TestMethod与原业务参数行，再核Failed仍反映父层。不得把内部role另加父分母或用role1/1替代full。

### 陷阱B：异常分支在证据落盘前脱出、把cleanup失败吞掉（最小必须修）

新helper `HistoricalProcessIsolation.cs:32–57`只有正常路径才写parent-role-exit；Kill/Wait10秒/输出I/O若抛异常会跳过该记录。`:54`取消drain后仍无第二期限await，不能仅因前面WaitAsync5秒就声称整个drain有界。`ExitDiagnostics.cs:128`也存在取消后无第二期限await。应最小用finally先记录PID/原已知退出/timeout/kill/drain/error，cleanup异常保留，不以成功记录覆盖；取消后观察任务必须有上限。不要照搬`NativeRun.RoleAsync:56`、`OffscreenSamplerTests:99`的无界kill后WaitForExitAsync。

`VisualCache.cs:455–478`仍丢弃destroy/disconnect/deinitialize返回值，不能写成“cleanup全部成功”；最低限度记录并传播非成功结果，保留首个业务错误和清理错误，后续独立清理尽量执行。不要求先修完所有未来失败清理才能启动full；这里是当前明确边界的可靠失败记账，OS终止允许作为未知资源责任的最终回收，但必须留Failed/缺口。

### 陷阱C：隔离旧Event0不等于合法；新async也不能提前宣布业务修复

当前`VisualCache.cs:89–99`official initialize(0)/connect0，`:249–258`真实DPI/flags12，`:282–285,442`调用新`JhPresentImage`；`JhPresent.cs:11–26`原event+DuplicateHandle、非零Event、separate1/send/commit、5秒wait/flush。不是仅给旧Event0套壳。仓库官方`core/resources/d3dimage.cpp:258–283`明确sync不走ProcessPresent，异步ProcessPresent的Cleanup负责SetEvent/CloseHandle；可支持“已执行ProcessPresent后的duplicate责任”，**不能证明send失败/未提交/批次丢弃的所有权**。该仓库源与实际10.0.13 servicing逐提交绑定未核，不冒SDK全认证。

helper注释已承认失败ownership未知，未猜测CloseHandle以免双关；只能将其记入对应Failed及OS回收，不能算业务refs0。原`VisualCache.cs:291–297,443–452`首帧/secondary mutation/caller-release/两帧内容仍在；`:494–502`原GetPixel逐通道<=1未改，不把原HW容差冒作零阈值差分通过。candidate分支`:136–137,436–437`仍Event0/same-thread，未证明其合法，不借official迁移美化candidate合同。

执行检查：最终报告分别写“父运行可收敛”“真实native危险目标是否越过原生前置”“业务提交/完成/refs证据”，不能只看ProcessPresent helper存在或父Completed。合法async成功还须在真实前置通过后核初帧、mutation、事件和cleanup；本轮不要求先穷举未来所有失败才准full。

## 2. 已有基线：不能覆盖成新绿

IP/IQ报告记两架构full均0xC0000374中断、无parent完成TRX，最后officialGeometry Commit E_HANDLE后DestroyBefore。该报告中candidate Native SHA：x86 `63B91E6CD6B5C1E8D0095E1673933DD5BFC539744F7EC36DC06C8DCB074E3136`；x64 `0C16A66C898269A17DBDD062012898FEBE338E341FED8D0B7FEC47FB21B6A23A`。JI尝试读取报告所指两份监督，精确本地路径不存在，目录亦不存在；这里是历史报告级事实，不冒本轮重新核原件。

IR实验两架构official geometry各1/1、candidate五行成功，不含official sampler；IS后来officialSampler AV无完成TRX；IT已恢复默认，不能用IR旧绿认证JH新源码。JD/JE离屏9/9只覆盖其独立身份；x64 10.0.1与x86 10.0.13不同，不能拼同版全矩阵。其weakref/dispatcher/GC证据不等于native COM refs0。

IO两架构Tests各4256独立保留，不绑定新ComAcceptance DLL/MVID。JH只改ComAcceptance且不改共享生产/Tests/其依赖时，不需要为取得当前COM失败清单额外重跑无关4256；若影响这些输入则对应托管两格需重跑。最终完整替换声明仍须在最后源码变更后跑最终适用矩阵，并列哪些未变产物仍能按变更范围保持对应，不能将旧绿笼统标成最终新源码四格通过。

## 3. JH已可得x64实际核证（不是双架构终态）

根 `artifacts/jh-com-full-convergence/readback/full-x64/Results/jh-full-x64/`。监督37行全文、父TRX头及14660–14668、stderr空已亲读：task `90574bc234a940b4b97033714264ec72`，监督8616/父10584，2026-10-26 00:52:13.5551258–00:59:11.770016 UTC；Mode full，Arguments确无filter，1500秒，.NET10.0.13/X64；正常exited **Exit2**，HasExited/OutputCollected true，无timeout/cancel/kill/drain、SupervisorError null。

父TRX有finish，ResultSummary **Failed**，total=executed=1802、passed1709、failed93、其他计数0；RunInfo明确Exit2失败。不是把ResultSummary写为Completed；只能说父运行正常结束并产出失败完成报告。未全文逐UID核93失败清单，不宣称全分母与旧发现集合逐项相等。

父test SHA `B7A12D431B0DEDBB39C8468ACBE18E356287B8DB44681796B8329983EF49D0EC`，MVID `140bd1ef-67e1-4d33-9d6a-184f5cb3cf6c`。两official子supervision各39行全文：geometry监督1036/业务3168，sampler监督11060/业务2880，均同test SHA/MVID、filtered90秒、Exit2，无timeout/kill/drain。子TRX真实官方Requested=Actual任务根wpfgfx_cor3.dll，SHA `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，10.0.13/4099f839。不能把PresentCondition诊断中的candidate Native路径当官方实际模块；那是另一字段来源。

sampler receipt与parent-role-exit全文核pid2880/parent10584/supervisor11060，method精确匹配且退出2持久化。两子TRX实际进入原方法：geometry=True/expected0000FF00、sampler=False/expected0000FF80，均在原生前置expected00563412/actual001E1D1C断言失败，**未进入待证异步提交目标**。父TRX2582处geometry UID18e39062…真实Failed并显示子Exit2；candidate五行UID在2422–2550可见仍发现，sampler父UID1c6be0d5…在2633可见。证明本次实际失败回传且父继续约至00:59结束；不证明native AV/timeout负控已执行，不证明历史AV已修复。

读取时readback一级仅full-x64和proof-x64，未核proof内容；x86未得结果，不预判未跑或成功。JH阶段报告尚未反映上述实际产物，最终需以其更新后的源码/身份为准。

## 4. 包、身份和源码对应：已审到哪里

静态核`ComAcceptance.Package.targets:5–7,33–55`：两ReferenceData显式输入，正式exe/MTP/runtimepack/Contracts/Native白名单打包，包目录与readback分离；`AcceptancePaths.cs:51–68`核Native架构与包aot SHA。但这不等于JI已逐文件审新包；未读新package-files全文、ReferenceData实际版本/所有样本、实际WPF模块SHA/MVID/runtime来源闭包，未独立算二进制hash或提取新DLL编译API。

`AcceptancePaths.cs:77–89`仍将identity写公共AcceptancePaths.Results而非每role独有root，角色可覆盖；最终必须以对应supervision test MVID、原业务actual模块和reference原件交叉，不能仅凭最后aot-identity绑定父full。JH新源码与上面B7A12D…编译版本精确对应尚未独立核，本报告是阶段核证，非全文件原件全审。

可执行收口清单：每架构分别核default编译发现集合/原DataRow保留；父full参数无filter；parent TRX finish、Counters、RunInfo及按UID/参数去重的全部失败；每危险role receipt/PID/原参数、supervision/原退出和stdoutstderr、精确TRX与actual模块；ReferenceData官方同版及实际WPF/runtime；最后源码后对应产物身份。不把嵌套role的绿或公共identity覆盖父报告。

## 5. 交付裁决及非本轮门

**原验收语义在已读两个official包装入口及五candidate DataRow层面保留；尚不能认证全部1802发现闭包或JH双架构最终版本。** x64实物已证明父正常结束、93个失败保留，不是全替换通过。最小必须修的是当前边界异常落盘/有界drain、cleanup失败可见性；不需要新诊断框架，不为这些审查追加全未来场景证明门。

HW真backend/Available、原窗口合法sampler实际内容、真实device-loss/resize恢复、长期资源与持续media仍开放；COM对象最终Release0和子OS回收必须分列。上述产品缺口不是“不得启动本轮full”的前置条件，但仍阻止宣布完整替换。未要求Debug/ARM64/卸载、未来全部版本证明或全文件逐字节认证。JI不修改JH代码、不替其决定是否继续一次有界正式运行，只交付明确可执行安全问题与已核事实。
