# channel-message-event-owner-x64-r1 正式收尾

## 本次收尾真实性复核终态：CloseoutCompleted / 无文档收尾待办

修正后本报告顶部1–13及14–24、五状态顶部（四份1–11、final矩阵1–20）已逐份亲读，无返回截断；此完成只指文档同步与有界真实性校验，不增加任何运行验收。

此前“正式报告和五状态已回读Completed”无本次回读证据，已先撤回为同步待核。本次亲读指定原件及文档后修正；完成证明以最后逐份顶部回读为准。本阶段源码/build/test/publish/Native/沙箱/终端脚本/再委派均0。下文旧“亲读”主体是前代理，不改写为本次完成全量取证。

本次原件：父TRX1–26、父监督1–40、comparison1–53、两role监督各1–20全文；aot-identity单行、clean/aot.sha256单行及两role round-00 business各1–40（actual认证，不以初始化冒actual）。新D3F5E40D2ABC0782D678C843537210EC203987A74C84616E20164703B3895B82 / official AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1 / test6D4998A543844169FB893FB3D08E8EE0D01A14D745FBA78AC0E99FC53E7DFDB8 / MVID b134506e-23ca-4f47-b543-5e293682213b / .NET10.0.13/X64/pointer8。父1passed，task29821d9d04454b49b8295cc96f65e6a4，角色各16轮。新旧candidate descriptive的PrivateBytes/Handles关键切片及本次official对应切片已读，详05台账；32轮关键业务及130samples仅引用前代理已核，不无界重读。

本次结论仍为PassedKnownOwnershipAndCallbacksOnly；Handles slope2.055944055944056对旧3.944055944055944仅部分改善，PrivateBytes251588.92307692306对旧224105.62237762238未下降。PendingInvestigate / MemoryDescriptiveOnly / memoryGreen=null / WorkerExitNotProven；可见增长不是永久泄漏或全部Event归因证据。历史Native publish1、COMpublish1、沙箱1、official/candidate各16轮独立保留，非本阶段新增。x86仍旧15A545，未含event修正；若需移植发布验收必须独立授权，本阶段未跑。

### 唯一后续建议（只建议，未执行）

先用新D3F5E4 Native做受影响既存最小postfailure/isolation/known-worker回归，确认event Dispose无回归；之后专门仅静态识别残余对象owner，不再无视约+2句柄/轮去无限ABI微回归。建议预算：Native0、normalbuild0、COMpublish≤1（隐式编译流程≤1另计，显式绑定新Native）；参数体UID如需重新绑定，纯发现≤1/体0，不复用旧test UID作新包认证。执行角色8个、各一次且各独立沙箱，顺序postfailure official→candidate、isolation official→candidate、directClose O6→C6→O8→C8；后四项是queueState6/8、retainExternal=false，O为official独立关闭控制、C为candidate重入及该释放worker有限退出，不把official当worker等价。总沙箱≤9（发现1+体8），无重试/full/趋势加轮；postfailure/isolation沿既有filtered内120/外180，参数体沿既有精确UID外180。每项先核actual身份/TRX/正常清理及可见退出边界再下一项；失败、超时、forced/unreaped或未知清理即停，不自行修后重跑。预算需另授权，当前NotRun。随后静态owner调查执行预算全0，只读对象创建/借用/释放责任链并列未知，不创建诊断器、不采新样本，不因未判永久泄漏取消残余增长调查。

完整替换Open；显示/HWloss/媒体音频趋势/full均未关闭。旧KN、旧postfailure/isolation/directClose有限绿全部留原SHA原范围，不迁至新SHA。

## 裁决

**PassedKnownOwnershipAndCallbacksOnly / softDescriptiveOnly / CloseoutCompleted**。新Native实际加载身份已认证，原official-first各16轮硬门通过。资源趋势仍 **PendingInvestigate**；MemoryDescriptiveOnly、memoryGreen=null、WorkerExitNotProven、GPU/长期NotProven、完整替换Open。仅当前原趋势硬门新增绑定新SHA；旧KN/full/postfailure及其他有限绿不得自动转绑。

## 冻结、预算与执行

亲自执行，不委派，无终端/脚本；运行仅唯一Windows沙箱。来源先落盘`artifacts/channel-message-event-owner-x64-r1/00-source-snapshot.md`：SameThreadChannel owner局部、Tests/项目/targets限定快照，不是全repo加密冻结。源码/测试/项目/采样器/阈值/等待/GC策略修改0，normalbuild追加0、额外unit0；历史normal Release build1 exit0/3714warnings/0errors及Debug定向13通过保留。

|执行|实际预算/结果|
|---|---|
|生产Native publish|1/1，Release/win-x64/net10.0/NativeAOT，exit0，独立native；实际隐式编译及native code生成1|
|COM publish|1/1，ResourceLifecycleTrendDiagnostic/win-x64/net10.0-windows/selfcontained，exit0，独立publish/clean；正常隐式编译流程1（增量Csc是否执行未给）|
|Windows沙箱|1/1，父body1，official1后candidate1，各16轮4warmup+12measurement/65samples|
|重试/发现/额外测试/x86/full/显示/ETW/强制GC/终端脚本/委派|全部0|

两次发布准确完整命令及原工具返回分别在01-native-publish-result.md、02-com-publish-result.md；显式AotDllPath为本次native/wpfgfx_cor3.dll，AotRuntimeIdentifier=win-x64，NETCore/Desktop10.0.13。发布失败即停规则未触发，不修后重试或抑分析器。Native日志61行亲读，但stdout615467字符仅保留最后20000、前595467截断；warning聚合数/error聚合数和发布耗时Unknown，可见CA1416，不声称全部历史warning。01未逐行复制全部可见warning，**原始stdout及可见warning完整归档有缺口**。COM11行全文无截断，可见warning/error0，未有聚合汇总。不能把exit0冒全部warning审计完成。

## 唯一沙箱、退出与时长

准确参数和原返回即时归档04-sandbox-tool-return.md：sourceDirectory=`WpfGfxShape/artifacts/channel-message-event-owner-x64-r1/clean`，根exe=`WpfGfxShape.ComAcceptance.exe`，arguments=`["--supervise-exit","Results/channel-message-event-owner-x64-r1","840","FullyQualifiedName=WpfGfxShape.ComAcceptance.UceChannelTests.WhenRepeatedAsyncSharedWicLifecyclesCompleteThenEachRoundReleasesOwnershipAsync"]`，外timeout900，outputRelativePath=null，完整回收同artifact/sandbox-results。

task=`29821d9d04454b49b8295cc96f65e6a4`，远端`C:\CodingAgentSandbox\Tasks\29821d9d04454b49b8295cc96f65e6a4`。工具仅返回完成及本地路径，未给外层exit/耗时，不补造。父supervision从2026-10-29T01:58:30.9572445Z至01:59:06.9477032Z，**35.9904587秒**；Supervisor5472/child4560，exit0，正常退出、outputCollected，timeout/cancel/kill/drain超时均false、SupervisorError=null。TRX准确方法1executed/1passed/0failed/0error，body duration **35.3100568秒**，不混算墙钟。

official PID11152/candidate PID1716各exit0/Exited/reaped=true/forcedTermination=false/killFailed=false/failure=null/cleanupErrors=[]；completedRounds16/unexecuted[]/unfinished null。official监督完成01:58:49.0420294Z，先于candidate Initial01:58:49.1736643Z。外900/父840/role360/round20均原批准值，watchdogHardRealtime=false。未以forced/unreaped/timeout或unknown cleanup判Pass。

## 实际身份认证

|项|实际值|
|---|---|
|新candidate Native SHA256|D3F5E40D2ABC0782D678C843537210EC203987A74C84616E20164703B3895B82|
|official SHA256|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|
|test SHA256|6D4998A543844169FB893FB3D08E8EE0D01A14D745FBA78AC0E99FC53E7DFDB8|
|test MVID|b134506e-23ca-4f47-b543-5e293682213b|
|runtime/architecture/pointer|.NET10.0.13 / X64 / 8|

正式打包GetFileHash写clean/aot.sha256，与初始化预检及各轮actual SHA一致；不是新增hashrunner。新Native不同旧54BC62；test SHA/MVID与旧test相同不冒重写测试。candidate1.0.0.0实际task/Native/wpfgfx_cor3.dll，official10,0,1326,50504 @Commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699实际task根wpfgfx_cor3.dll。round00 moduleHandle=actualBaseAddress分别140720623190016/140720669196288。32份business requestedEqualsActual=true，完整SHA定值检索补齐；candidateInitializationIsNotActualRoleEvidence=true保留。runtimeconfig included NETCore/Desktop均10.0.13。

## 32轮硬门与130样本

前代理逐轮关键business/events/callback已核，本次仅引用05阅读台账，不冒本次全量亲读，不仅以comparison头部替代原硬门来源。caller Wrapper/Frame/Stream Release.after references1且caller账终态0；A关闭前后Observe0，不提前dispose；A.Destroy hr0后B.Send.NewAlpha0.25/B.Update.Flush hr0；B两个duplicate release各deleted1；B.Destroy/Disconnect/Deinitialize hr0；callback Enter/Exit各entries1，Wait.Callback.After finishedtrue。每轮FinalDisposals1/CallbackExited1/entries1/callbackErrors[]，最终CallerOwners/Channels/Duplicates0、ConnectionOwned/ManagerInitialized false、WicDisposed/trendCleaned true、primary null/cleanupErrors[]。

各role同PID循环、每轮不同UID与fresh创建记录；地址允许重用，不以地址重用冒复用对象。前代理各65fixed samples全部亲读，本次未重读，4warmup+12measurement未变；Errors{}。AfterSourceCloseTargetUpdate Channels1/Duplicates2/WicDisposedfalse/本轮callback三项0，AfterCleanup及+1秒已知账0/本轮三项1，累计终态16/16/16。不证明所有native对象或OS worker退出；callbackexit不是全thread退出。

## 与旧x64-r2同预注册phase比较（全为DescriptiveOnly）

只比较measurement轮4–15的AfterCleanupPlusOneSecond。范围取完整原样本，Δ和slope取现成descriptive，不新增采集/统计执行，不丢峰、不改预热或阈值。

|指标/单位|旧candidate范围；Δ；slope/round|新candidate范围；首→尾；Δ；slope/round|本次official范围；首→尾；Δ；slope/round|
|---|---|---|---|
|Handles/个|303–348；45；3.944055944055944|293–317；293→317；24；2.055944055944056|274–275；274→275；1；0.06993006993006994|
|PrivateBytes/bytes|22528000–29765632；1077248；224105.62237762238|22519808–30068736；28696576→30068736；1372160；251588.92307692306|21938176–24125440；24125440→23371776；-753664；-50340.6993006993|
|WorkingSet/bytes|55537664–63741952；2674688；350537.3986013986|55365632–63950848；60960768→63950848；2990080；384322.2377622378|58097664–60129280；59985920→59998208；12288；-7633.454545454545|
|Threads/个|11–11；0；0|11–11；11→11；0；0|9–9；9→9；0；0|
|ManagedBytes/bytes|3278424–4051712；53536；-19995.86013986014|3309368–4044784；3938624→3998144；59520；-18924.41958041958|5985592–6047432；5985592→6047432；61840；5502.965034965035|
|NaturalGC0/次|5–18；13；1.1643356643356644|5–18；5→18；13；1.1643356643356644|4–15；4→15；11；1|
|NaturalGC1/次|5–17；12；1.0944055944055944|5–17；5→17；12；1.0944055944055944|4–15；4→15；11；1|
|NaturalGC2/次|5–16；11；1|5–16；5→16；11；1|4–15；4→15；11；1|

句柄增长斜率减少约1.888111888111888/轮，**有限支持事件owner问题对该路径部分句柄增长有贡献**，不是类型或创建栈因果认证。仍约+2.056/轮；PrivateBytes/WorkingSet斜率未下降，剩余增长保留调查。不能声称原增长全是Event、owner为唯一泄漏根因、所有OS句柄确闭、缓存解释或长期无泄漏。MemoryDescriptiveOnly/memoryGreen=null不变；未读其他phase软汇总Unknown，不无限阻塞已核硬门。

## 前代理原件亲读范围与偏差（非本次全量重读）

根`artifacts/channel-message-event-owner-x64-r1/sandbox-results/Results/channel-message-event-owner-x64-r1/`；趋势根`ResourceLifecycleTrend/ba8524588c8441e48c35a68c0cf68cb6/`。详细阅读台账`artifacts/channel-message-event-owner-x64-r1/05-evidence-readback.md`。

- 父TRX1–26/监督1–40、comparison1–53、两role监督1–20全文；progress内容通过comparison内完整嵌入亲读，不冒独立文件另读。
- 全32business关键33–38/62–76/681–682字段；candidate00 1–78、official00 1–40完整；32 uid/requestedEqualsActual及完整SHA补读。不是每份689行全文。
- 全32events17/19/21/22–25/28–30/34–36/38关键段；alpha/deleted初搜索省略以定值正则补齐，省略的时间戳/指针尾不冒全文；candidate00初1–20截断以19–30/31–39完整补齐。全32callbacks两行完整。
- 两samples各1–65全文，以整行锚定正则分round-1/0–3、4–7、8–11、12–15四组返回，每文件<=17命中，无20行截断，最后65完整。
- 两descriptive仅2240–2250/2333–2343/2426–2436/2519–2529/2612–2622/2705–2715/2798–2808/2891–2901关键切片；candidate另2230–2255。不是2985行全文；其他phase汇总未全读。
- 一次空参数API调用失败无执行测试，准确参数随后查询成功，未重跑任务。一次宽目录文本查找意外命中DLL字节，违反不文本读二进制的要求，已停止此类搜索且不用于任何认证；没有新增hashrunner。此偏差及Native原日志归档缺口如实保留，不冒完全满足全部取证要求。

## 收尾与唯一下一门

限定快照→Native/COM发布→静态核验→唯一沙箱→父TRX/身份/32轮/130样本/主要软汇总亲读及task归档均完成。此前“正式报告与五入口最小更新并回读”完成声明已撤回；本次修正后逐份回读以顶部与05台账为准；旧单元/趋势/失败/历史账全部保留。本轮预算耗尽，不留本轮收尾待办，不追加运行。

**此前下一门建议（已被顶部完整顺序预算取代）**：原仅postfailure建议不足以覆盖剩余增长；现为postfailure/isolation/known-worker最小既存回归后，仅静态残余owner调查，均未执行。旧KN及其他有限绿仍属原SHA。剩余句柄/PrivateBytes增长、直接OS关闭/最后borrow/构造故障、全worker/GPU/长期均未证；不得把文档完成等同完整替换通过。
