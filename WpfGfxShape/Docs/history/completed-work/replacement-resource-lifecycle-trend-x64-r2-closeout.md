# resource-lifecycle-trend-x64-r2 正式收尾

## 裁决：PassedKnownOwnershipAndCallbacksOnly / softDescriptiveOnly / CloseoutCompleted

亲自执行唯一授权沙箱，无终端脚本、无再委派、无重试。official先16轮全部成功后candidate16轮；各4warmup+12measurement、各65samples、同rolePID循环fresh对象。完整替换仍Open，显示Unknown，Memory不判Green，WorkerExitNotProven；所有GPU及无界长期无泄漏不证。

## 预算与前阶段

前阶段normalCOMbuild1 exit0、144 warnings/0 errors、3.78秒；COMpublish1 exit0（正常隐式编译1），publish总warning/error数Unknown。01/02关键日志及截断边界原样保留，不冒完整raw日志保存。本接续源码0/build0/publish0/Native重发0/独立新采集0/终端脚本0/再委派0；沙箱1/1、父body1、official role1、candidate role1，角色各16轮，额外测试/发现/full/显示/x86/重试0。独立新采集0指不增加ETW等采集，正式运行自身生成130趋势样本如实计入本沙箱。

r1 SourceBlocked零执行历史不冲销；本r2经用户批准使用continuous-media-default-x64-r1/clean/Native连续同SHA副本。x86 trend、初StageA失败/后成功、旧publish未跑及其他旧账独立保留；不移绑旧full到本test SHA。

## 参数、task与退出

准确参数及完整可见原返回已在工具返回后立即保存：`artifacts/resource-lifecycle-trend-x64-r2/03-sandbox-tool-return.md`。
sourceDirectory=`WpfGfxShape/artifacts/resource-lifecycle-trend-x64-r2/clean`；exe=`WpfGfxShape.ComAcceptance.exe`；arguments=`["--supervise-exit","Results/resource-lifecycle-trend-x64-r2","840","FullyQualifiedName=WpfGfxShape.ComAcceptance.UceChannelTests.WhenRepeatedAsyncSharedWicLifecyclesCompleteThenEachRoundReleasesOwnershipAsync"]`；timeout900秒，outputRelativePath=null，整任务回收到同artifact/sandbox-results。

远端task=`0f69154d75934855a8443980a0c12f82`，目录=`C:\CodingAgentSandbox\Tasks\0f69154d75934855a8443980a0c12f82`。工具原返回仅“沙箱执行完成”及本地路径，未提供外层exit/外层耗时，不补造。
父supervision：2026-10-29T01:00:46.8129664Z→01:01:22.8821566Z，36.0691902秒；SupervisorPID10000/子PID7796，exit0、HasExited=true、KillAttempted=false、TimedOut/Cancelled=false、输出已收、drain未超时、SupervisorError=null。TRX精确方法1 executed/1 passed/0 failed/0 error/0 skipped，body duration35.3837607秒（TRX起止墙钟字段不等同duration，不混算）。
original PID7452、candidate PID4560：各exit0/status Exited/reaped=true/forcedTermination=false/killFailed=false/failure=null/cleanupErrors=[]。各progress Passed/completedRounds16/unexecutedRounds[]/unfinishedRound null/error null；角色360秒、轮20秒、父840秒，watchdogHardRealtime=false。original监督完成01:01:05.010458Z在candidate Initial01:01:05.1415277Z之前。

## 实际身份（不是文件预检冒模块认证）

|项|actual证据|
|---|---|
|candidate Native模块SHA256|54BC62FBBEF9A55EA75E49AF5EFDCEC70F7C8C2808BDF4E2C80B916CF92D5ADD|
|official模块SHA256|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|
|test SHA256|6D4998A543844169FB893FB3D08E8EE0D01A14D745FBA78AC0E99FC53E7DFDB8|
|test MVID|b134506e-23ca-4f47-b543-5e293682213b|
|Runtime/架构/指针|.NET10.0.13 / X64 / 8 bytes|

两role逐轮business identity requestedEqualsActual=true；candidate actual位于task/Native/wpfgfx_cor3.dll，official actual位于task/wpfgfx_cor3.dll。round00亲读moduleHandle=actualBaseAddress：candidate140720668672000，official140720242819072。official版本10,0,1326,50504，candidate1.0.0.0。candidateInitializationFile/SHA是文件初始化预检，原件明确candidateInitializationIsNotActualRoleEvidence=true；根Results/aot-identity.json也仅文件预检，不替代各轮actual字段。

## 32轮业务硬门

original/candidate round00–15均核关键字段/事件命中，不以completed头部代替：caller Wrapper/Frame/Stream Release.after references=1且caller所有者账终态0；A关闭前后Observe各0（未提前dispose）；A.Destroy hr0后B.Send.NewAlpha(0.25)/B.Update.Flush hr0；B.Release.Visual与Bitmap各deleted1；B.Destroy/Disconnect/Deinitialize hr0；Callback.Enter/Exit各entries1、Wait.Callback.After finished=true。每轮FinalDisposals1/CallbackExited1/callbackEntries1，callbackErrors[]；最终CallerOwners/Channels/Duplicates均0，ConnectionOwned/ManagerInitialized=false，WicDisposed/trendCleaned=true、primary=null/cleanupErrors[]。末轮与样本均补核，未仅依据comparison Passed。

各sample AfterSourceCloseTargetUpdate仍Channels1/Duplicates2/WicDisposed=false、RoundCallbackEntries/Exits/Disposals0；AfterCleanup及+1秒均known对象账0且Round三项1，累计终态16/16/16；所有130样本Errors={}。这仅证明该固定WIC路径已知对象和callback，不证明全部native资源、OS worker退出或GPU归零。

## 主要软指标（全为DescriptiveOnly）

以下取measurement轮4–15的AfterCleanupPlusOneSecond；min/max从已读全部样本，Δ为现成descriptive.firstLastDelta（轮15减轮4），slope为现成每轮斜率。bytes保持原单位，不将bytes与MiB混用，不解释增长为缓存或泄漏。

|指标（单位）|official min–max；Δ；slope/round|candidate min–max；Δ；slope/round|
|---|---|---|
|PrivateBytes (bytes)|21446656–24137728；-327680；-9251.804195804196|22528000–29765632；1077248；224105.62237762238|
|WorkingSet (bytes)|57872384–60039168；24576；4081.6783216783215|55537664–63741952；2674688；350537.3986013986|
|Handles (个)|274–275；1；0.06993006993006994|303–348；45；3.944055944055944|
|Threads (个)|9–9；0；0|11–11；0；0|
|ManagedBytes (bytes)|5984128–6121376；112384；12176.587412587412|3278424–4051712；53536；-19995.86013986014|
|NaturalGC0 (次)|4–15；11；1|5–18；13；1.1643356643356644|
|NaturalGC1 (次)|4–15；11；1|5–17；12；1.0944055944055944|
|NaturalGC2 (次)|4–15；11；1|5–16；11；1|

PrivateBytes三窗口4–7/8–11/12–15的median(bytes)：official23779328/21895168/23578624；candidate27891712/27590656/28854272。WorkingSet窗口median(bytes)：official59619328/58302464/59475968；candidate60344320/60563456/62345216。candidate窗口与样本峰值保留，不丢峰、不额外预热、不强制GC、不改阈值。各role最终Initial→轮15+1秒：official Private9404416→23810048 bytes、WorkingSet33984512→59998208 bytes、handles245→275、threads9→9、managed1011488→6096512 bytes、GC0/1/2均0→15；candidate Private8527872→29765632 bytes、WorkingSet31084544→63741952 bytes、handles233→348、threads9→11、managed913320→3998064 bytes、GC0/1/2分别0→18/17/16。原始时序全保留，无因果归因。

## 原件及准确阅读范围

根：`artifacts/resource-lifecycle-trend-x64-r2/sandbox-results/Results/resource-lifecycle-trend-x64-r2/`；趋势相对根：`ResourceLifecycleTrend/1493b441b1ae4d86a804fcae8b8eb4ea/`（下称T）。

- 父TestResults/child.trx1–26、supervision1–40、stdout1–12、comparison(T)1–53全文；角色各progress1–17、supervision1–20全文。不存在独立命名summary/identity文件，用progress作为role完成汇总，各轮business identity为实际模块证据。根Results/aot-identity1行全文。
- 两samples.jsonl各1–65全文：original每4行小块1–60、61–65末项截断后65单行补全；candidate1–5及6–65每4行小块，均已见尾。不把4+12仅当报告头声明。
- 32份business各核关键行33–38、62–76、681–688；以锚定正则按字段返回核验，不冒每份689行全文。original round15 31–76补读；两role round00 1–76（candidate初读1–100）及candidate660–689明确亲读。actual/test身份另逐文件字段命中；长SHA/路径搜索显示省略，只round00/父监督已读完整值，未声称每轮长SHA字符串全文逐字回读。
- 32份events核行17/19/21/22–25/28–30/34–36/38–39关键业务；字段正则限定真实hr0/references1/deleted1/finishedtrue再返回，截断只在其后timestamp/pointer尾部时不冒全文。candidate00初1–39截断，18–33、34–39补读。32份callbacks1–2经匹配entries1正则返回完整两行，先前模糊搜索尾部省略由此补齐。逐轮前后零dispose/真实关闭另由business和130samples交叉核。
- original descriptive1–7、2240–2250、2333–2343、2426–2436、2519–2529、2612–2622、2705–2715、2798–2808、2891–2901及2308–2331/2401–2424；candidate1–90、2240–2250、2315–2343、2410–2436、2503–2529、2596–2622、2690–2715、2783–2808、2876–2901及2308–2331/2401–2424。仅关键汇总切片，不冒2985行全文；其他阶段的汇总未全读，不外推。
- source-check1–24/predefined-scope1–10/01和02四记录全文、规则1–27、runtimeconfig1–21/aot1行全文；启动纠正1–37与csproj1–56有4000字符截断，Parse22–58/协调体209–267/专用role45–110补核入口链。clean DLL公开API确认精确无参Task方法。未作无限静态审计。

检索工具曾漏报child.trx不存在；精确目录列举后已成功读原件，不是结果缺失，未触发任何重复执行。父stdout中文解码有替换符，数值用TRX及JSON交叉认证。外层工具没有提供Supervisor自身exit码，不能把子exit0冒作工具外层数值；业务父子正常结束与两role回收证据如上。

## 计划终态与下一主要建议

现成包必要静态核验Completed→唯一沙箱Completed→父TRX/身份/监督/32轮硬门/130样本与主要软汇总核验Completed→原返回/task/边界归档Completed→正式报告与恢复入口/工作项/基线/plan状态/final矩阵同步及回读Completed。r1阻塞仅历史，本r2已执行；无剩余沙箱预算。

下一主要建议：转向完整替换矩阵中仍未闭合的主要功能/恢复缺口，先基于既有证据选择单一高价值门并另授权；不要自动重跑趋势、显示、full或采集。内存与句柄增长保持描述，不作未经证据支持的解释；长期、全worker、GPU、HW/loss、真实音频及full均不因本切片升级。
