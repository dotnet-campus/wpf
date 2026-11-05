# JD 默认离屏 sampler 隔离修复

## JE 终态：x86 自包含有限通过，x64 独立审查完成（证据边界明确）

### 最终裁决与身份

保留JD两文件默认进程隔离修复，JE源码修改0、生产修改/重发0。x86新增唯一task **10ef4f53172e4e61b02ec12102669745**，监督3828/业务父7652，9/9 passed、failed/skip0、BL Exit0，26.459秒，无timeout/cancel/kill/drain。专用沙箱返回回收完成但没有数值工具ExitCode，不把BL码冒作工具码。不是full，也未进行桌面前置。正常build1/publish1/沙箱1，均无diskfull，容量不阻塞本次；实际剩余字节与包总字节未知，不借DO旧大小。无清理、无置空DLL、无安装共享runtime。

本地R=artifacts/je-x86-isolation/readback/Results/Results/je-x86-final；任务绝对父TRX：C:/CodingAgentSandbox/Tasks/10ef4f53172e4e61b02ec12102669745/Results/je-x86-final/TestResults/child.trx。亲读8–16各结果及65–74计数。发现9例：Offscreen三个方法、软件shader三DataRow、HY/Listener/动态state各1。重复回归同一父两轮direct+group+Bitmap兄弟真实通过，未把内部role追加到父分母。代表group=2dfd8679fca24b5eb1bb88df7f45e132，三角色业务PID6860/2716/2472，direct=2711e2f88a42491b9d75abbdac87e502 PID5720，均异于业务父及各监督。四份sampler/evidence.json全97行分段亲读；三role及direct TRX计数各1/1亲读，候选role-exit及supervision全文亲读（父7652/监督1264/业务2716，Exit0）。role绝对TRX为上述任务Results/je-x86-final/offscreen-sampler/{group}/{official,candidate,official-repeat}/TestResults/child.trx及offscreen-sampler-direct/{direct}/TestResults/child.trx。

x86实际test SHA99259117418F01C4ACDD0D5DC68AFB342E1092F43164F9821D54ED43499391E3，MVID8fb933a4-fdb6-4183-b504-0ef38035977e；actual候选Native SHA63B91E6CD6B5C1E8D0095E1673933DD5BFC539744F7EC36DC06C8DCB074E3136，actual官方任务根wpfgfx_cor3.dll SHA5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834。actual WPF任务根PresentationCore SHA D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D/MVID2934bccd-58e4-4afe-8677-ec985888f34e；.NET/WPF/官方同版10.0.13、servicing26504.104/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699、X86。并非x64的10.0.1，也不宣称同版双架构。

三角色及direct原expected RGB0000FF80→00FF0080，actual FF00FF80→FFFF0080，Nonempty=true；8weakrefs全false，scene解绑、target清空、dispatcher/finalizers完成，NativeComRefsMeasured=false，不认COM refs0。group真实比较304byte shader、两1024byte缓冲全部字节并与official-repeat全等；保留每256像素原RGB零阈值断言。未用额外二进制解码器人工复算所有字节，不冒全审其余重复role或HY/Listener大型事件。

### x64 独立核查与静态审查

JE独立读取JD task6da931b0949547f6a1c56f1ab9b1e25b父supervision全39行、TRX8–18结果及65–74计数：1976/2232，9/9 Exit0无timeout/cancel/kill/drain，.NET10.0.1。候选代表group8cbfa5101f3e4d2988efe9e08db8618e/evidence全97行，actual PID8112、候选Native0C16A66C898269A17DBDD062012898FEBE338E341FED8D0B7FEC47FB21B6A23A、test494B675A62B46846861E15D189DF3290A54A033E5D8144195792129DAF0E0470/MVID1d59285b-c86e-480c-97bc-111e29131279；内容更新及8weakref全false。候选role-exit亲读父2232/监督10464/Exit0。官方identity1–30亲读PID9596，官方94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F，PresentationCore605FD26A2D1DF0D81B3DE3CB233365DC6091EC7C72C48B22FB805819C8F57654/MVID591d6312-bdf3-4e4b-b68a-c8b6bc5ab6e2；实际共享WindowsDesktop10.0.1 servicing25569.105。JE未重新全文审其他x64 role，不把JD亲读范围冒作JE范围，未重跑x64。

静态读OffscreenSamplerTests全118和helper全112：默认parent不安装WPF resolver、不调用sampler dispatcher shutdown/GC、不修改Environment全局变量，只对子ProcessStartInfo.Environment设置角色/绝对输出；role只执行helper不递归，缺role真实candidate。shader MemoryStream归role局部using，无新增共享stream锁；原全字节/expected/cleanup保留，无skip/关并行/阈值/golden/fallback。parent回归只读NativeLibrary resolver map的两个程序集存在性、原线程Dispatcher引用及HasShutdownStarted，前后断言真实通过；它不是resolver delegate内容完整快照，也未检查HasShutdownFinished前后值，未输出可单独重读的父状态JSON。此“完整parent快照”证据要求尚未满足，不能把Passed冒作独立快照全审；未为此追加发布/重跑或撤回已过修复。

发布闭包由现有正式targets白名单形成，manifest418行首尾、aot、runtimeconfig已亲读：应用/MTP/runtime/Contracts/官方root与候选Native分离，未上传publish父根/回收根/JA目录。离线核到源码默认三方法无guard、正常编译成功和本次TRX真实发现；没有独立反射工具逐类型扫描全部编译闭包，不能冒完整全包逐文件验签。build日志仅末63行可读，publish末60行可读且前段字符被接口截断，不声称完整原始日志另存；JE没有diskfull错误需另存。

### 下一唯一任务与末次补记

不再要求用户安装x86 runtime；JD旧框架依赖缺runtime不是产品失败，其后续动作被本次自包含授权与实跑覆盖。下一唯一任务JB **0xA引用所有权控制**，报告replacement-supervised-window-sampler-ownership.md；不得绕过所有权直接full。Available/HW恢复/mediaresources/full四格仍开放，完整替换未完成。本文沿用JD报告，旧JD/JC历史不重写。报告与三共享JE最新受改节写后回读，历史范围不冒全审。

## JE 阶段1/2历史：容量探测、自包含发布与唯一正式运行已完成，正在亲读

JE normal Release/win-x86 build成功（124warnings/0errors/6.92秒），随后仅一次selfcontained 10.0.13正式发布成功，正常分析器、无专属符号/NoWarn。独有artifacts/je-x86-isolation/obj/bin/publish/com-acceptance-package，显式PublishDir、源/回收分离，复用IY x86 Native SHA63B91E6CD6B5C1E8D0095E1673933DD5BFC539744F7EC36DC06C8DCB074E3136。无diskfull、剩余字节未知、清理0；无现成DriveInfo机制，不添加容量工程。包manifest418行，runtimeconfig两framework均10.0.13，aot.sha256已读；只上传白名单com-acceptance-package，不上传父publish/历史/回收根，不用JA损坏目录。

唯一正式task10ef4f53172e4e61b02ec12102669745，120秒filtered/外180，26.459秒退出；监督3828、业务父7652，Exit0、无timeout/cancel/kill/drain。父TRX原件65–74实读9/9、failed0，尚在亲读代表role及x64独立证据，不以此初记代最终审查。JE未改源码、生产发布0、full0。

## 进行中

已读取执行规则及JC/JA/IY现状。优先读取完整方案与默认测试/helper/既有同exe角色机制，实施最小测试隔离。生产、NativeRun/ExitDiagnostics、打包与版本不改，不新增Host/工程。不使用终端或脚本。

容量：当前专用接口无剩余空间/删除能力；未知，不声称恢复。允许一次正常分析器默认项目构建作为编译/容量探测；若真实磁盘不足立即停止额外生成。JA final-x64/publish禁止部署。历史证据不删除。

## 阶段1：容量/编译与初次有限实跑

正常默认Release/win-x64 build真实Exit0，124warnings/0errors/3.56秒；可读日志64行，输出97914字符前77914被接口截断，不冒完整保存。无diskfull，容量字节仍未知、清理0。最小修改OffscreenSamplerTests：统一同exe角色启动、显式角色与绝对结果路径、无role真实候选子进程、两轮direct/group+Bitmap兄弟与父resolver map/dispatcher状态不变。helper仅identity新增Pid，shader/两缓冲/期望/8weakref原断言不动。

已正式框架依赖x64独有目录publish成功，复用IY包IP Native，不改产品版本。第一次沙箱传任务外绝对结果根未回收业务，不作通过证据；第二次改任务相对入口后由监督规范成task绝对根，task d1836fa939964e7387fae3b485e3fbba，父9192/9244，三测试Passed，监督Exit0无timeout/kill/drain。实际共享runtime .NET10.0.1（非IY10.0.13），WPF同版10.0.1；正式同版官方由AcceptancePaths定位，不混13 oracle。direct候选PID2864内容/cleanup全文已读，Native0C16A66C…；testC1F50902…/MVID02744c94…为初版，不绑定随后新增PID断言源码。

现新增父/子监督/业务PID互异断言，随后最终包与有限验证如下；初版非最终证据。

## 阶段2：最终源码有限验证与保留裁决

明确保留两个离屏测试文件修改，不恢复IY同进程direct。没有catch alreadyset、skip、阈值/golden/fallback或默认并行修改。helper仍只在显式角色中运行；group无role之外不准递归；无role direct实际启动候选、验证精确role TRX及三个输出长度并读取业务PID，断言不等父PID/监督PID。父不执行sampler resolver安装、dispatcher关闭或GC观察；回归仅只读resolver map和原dispatcher状态。私有runtime map字段不支持时测试失败，不静默绕过。

最终x64 task **6da931b0949547f6a1c56f1ab9b1e25b**，监督1976/父2232；120秒filtered、外180秒，实际21秒完成，Exit0，timeout/cancel/kill/drain全部false。绝对task TRX为C:/CodingAgentSandbox/Tasks/6da931b0949547f6a1c56f1ab9b1e25b/Results/jd-x64-final/TestResults/child.trx，9 executed/9 passed/0failed。包含离屏group/direct/重复回归各1、软件shader3、HY/Listener/动态state各1；Bitmap兄弟在重复回归每轮真实调用两次。parent resolver存在状态前后相等、原线程dispatcher身份及shutdown状态前后相等，两轮通过；不冒证明其他未测顺序。

最终证据根R=artifacts/jd-offscreen-isolation/readback/final-x64/Results/jd-x64-final。R/offscreen-sampler/8cbfa5101f3e4d2988efe9e08db8618e/{official,candidate,official-repeat}/sampler/evidence.json各97行全文亲读，业务PID9596/8112/3828（与父2232不同）；R/offscreen-sampler-direct/3ef124cd52a64026943ecb60a05916d8/sampler/evidence.json全97行亲读，PID1744。同组候选监督10464、真实8112，role精确TRX1/1、Exit0/无timeout/kill/drain全文亲读。其余role由真实父helper强制TRX1/1及Exit0后才比较；没有声称全部role监督/业务全文人工审计。最终父TRX计数与主要结果行已读，长行返回部分截断，不冒全文。

三角色及direct初态actual FF00FF80、mutation FFFF0080，256像素原RGB expected0000FF80→00FF0080零阈值、非空；8weakref全false、Effect/VisualBrush解绑/target清空、dispatcher/finalizers完成，NativeComRefsMeasured=false，不是native refs0。group对304byte shader和两1024byte输出与official-repeat全字节比较真实通过；二进制未另行独立解码，不能冒人工像素全审。保留原有cleanup断言，没有追加JA finally断言。

实际最终身份：test SHA494B675A62B46846861E15D189DF3290A54A033E5D8144195792129DAF0E0470，MVID1d59285b-c86e-480c-97bc-111e29131279；IP Native0C16A66C898269A17DBDD062012898FEBE338E341FED8D0B7FEC47FB21B6A23A，来自IY/x64/Native原包复用，生产未重发。实际官方C:/Program Files/dotnet/shared/Microsoft.WindowsDesktop.App/10.0.1/wpfgfx_cor3.dll SHA94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F；WPF同目录PresentationCore SHA605FD26A2D1DF0D81B3DE3CB233365DC6091EC7C72C48B22FB805819C8F57654、MVID591d6312-bdf3-4e4b-b68a-c8b6bc5ab6e2；runtime .NET10.0.1/X64，官方/WPF servicing25569.105 commitfad253f51b461736dfd3cd9c15977bb7493becef。这是新同版10.0.1有限合同，不用IY10.0.13官方混oracle，也不外推13或完整替换。

## 阶段3：x86准确缺口、最终默认编译和执行计数

正式框架依赖包按x64后x86逐架构发布，JD独有obj/bin、publish和架构artifacts根，显式PublishDir；现有Package target仅回收JD独有com-acceptance-package，不指向PublishDir/readback/历史根。x86复用IY/x86/Native来源，发布成功；task **f3fd51a8fcba49fb9916fbd47086baa3** 启动失败：Architecture x86、apphost10.0.13，注册/默认/环境均无x86 .NET位置，You must install .NET。尚未进入监督/测试，无TRX、实际moduleSHA或MVID，不造工具退出码；x86通过0。容量余量未知，不追加自包含大发布，没有删除或清理任何证据。JA损坏目录未用。

JD计数：normal build2次均成功（首3.56秒，最后源码修改后末次1.44秒、124warnings/0errors、Exit0）；publish3次成功（x64初版、x64最终、x86最终），sandbox4次（初版绝对根未回收1、初版有限通过1、最终x64通过1、x86启动阻塞1）；full0、生产重发0、清理0。最后normal build使用正常分析器，无专属DefineConstants。最后默认构建日志63行，已读末汇总；接口只保留输出末20000字符，不能称完整原日志已存。没有真实diskfull，仍不声称实际剩余字节或存储侧容量恢复。

下一唯一动作：为现有x86框架依赖包提供可用同版x86 Microsoft.WindowsDesktop.App/Microsoft.NETCore.App共享运行时后，先按同filter一次正式沙箱有限验证并亲读身份/结果；如必须自包含，先取得可靠可用空间证据再只发布x86一架构。不得重做x64追Green或进入full。JB0xA所有权控制后续，Available/HWbackend/loss/mediaresources/full四格仍开放，完整替换未完成。

末次：本报告全文与handoff/session-summary/issues最新JD节写后回读；历史原样保留。
