# BP：安全收口——仅实验与精确撤回

## 1. 最终裁决

未验收 present/堆损坏修复。BO 未验收行为已精确撤回；BN/BL 同步诊断、默认历史生产回归和 --supervise-exit 保留。BM 新增探索实验已改为同 exe 显式 --bm-fill-control-points 角色，保留全部原始场景、零阈值和失败断言，不使用 Ignore/skip。

本轮不改生产、NativeShapeReader、CaptureFreshFrame、WindowQ、既有曲线实验、项目打包、其他代理文档。无终端/脚本/新 Host；仅专用读写、发布及 Windows 沙箱正式纯 C# exe。所有本轮构建和回收均在 artifacts/bp-safe-closeout/。

## 2. 生命周期审查与有界实验

已读执行规则、完整方案（长段输出有截断，90–120补读）、BN全文、BL主要节、BO实际两个文件和已有失败证据。官方 apifunc.cpp:62–119 的初始化为 WINAPI HRESULT(int priority)，deinitialize 为 WINAPI HRESULT(void)，创建/释放 partition manager、scheduler 和工作线程。CompositionNotificationTests.Observe 在 connect 前 initialize(0)，在通道/连接清理后反初始化。AsyncUceChannelTests.Api 自身不初始化，不能将候选辅助类当作官方初始化充分依据。

BO 在异步 connect(0) 后建 channel，却未初始化 partition manager。BP 临时补 initialize(0)，仍按 destroy channel → disconnect → deinitialize，同一调用线程持有连接/通道，事件由 EventWaitHandle 持有原柄、只转交 duplicate，等待限5秒、监督限30秒。没有擅自让调用者充当合成工作线程。

最小合同 task60397d1f53d44696a3f35bec57d866be，probe/readback/bp-contract/：监督9336/子1676，退出0，TRX83cb3548-83a1-4898-89a3-70c584b0eb85，4执行4通过，其余异常计数0。官方/旧候选同步与异步各3次提交；92行 stages 已分段全文回读，异步3次事件均 signalled，最后 destroy/disconnect/deinitialize 完成。测试SHA F4C0453E0D63054D7CBF7E335B59FBACC3E31F754E94FA7AEE1E9C23D18AF44E，MVIDc9c7dd9b-58e1-4fb3-808e-93180f3f537a。

但 image 无真实 surface，此结果仅证明该最小事件/生命周期序列，不证明内容重复更新、工作线程资源趋势或堆损坏根因修复。

随后一次真实官方 sampler taska59f9da1f7bc4c06a87cac4d1a488a92，content/readback/bp-content/：监督8808/子2252，退出2，TRXdd4cd3f5-bdf8-4e18-b9af-c89ac8deaac2，1执行1失败，timeout/aborted/notExecuted=0。24行阶段完整至 cleanup complete；在原生前置 AssertCombinedPixel 即失败，expected00563412，actual00CBAE87，尚未 connect，不能写成异步真实内容验证成功。测试SHA E7F5DC64D1D61022EB308421F9201EE37859AB3DD768BA9DF7D042CC0326FD5B。两份监督JSON全文亲读，均非timeout/cancel/kill，双流收集完成、SupervisorError=null。

## 3. 精确撤回及源码读取确认

- D3DImageTests.CaptureHistoricalControls.cs：删除 BO 新增四数据 WhenHistoricalPresentContractRepeatsThenEventsAndCleanupComplete（含 BP 临时初始化）；历史调用移除 asynchronousPresent:true。原5个候选数据、官方 geometry/sampler 测试、模块identity及 BL Stage 全保留，最终39行全文回读。
- D3DImageTests.VisualCache.cs：移除 asynchronousPresent 参数、异步 SyncFlush 替换、completion、复制事件 SendHistoricalPresent 和 HistoricalCloseHandle，以及 BP 临时初始化；connect恢复1，初始和mutation两个原 PresentPacket恢复，present恢复 connection。没有整文件覆盖。回读参数、connect、初始/更新提交、Commit诊断和finally；全目录搜索 asynchronousPresent/SendHistoricalPresent/HistoricalCloseHandle/新合同名为零。BN/BL 的逐阶段与原HRESULT记录保留。已知同步 Event=0 合同偏差及潜在退出仍未修，撤回不是认可旧行为正确。
- BmFillControlPointTests.cs：仅移除 TestClass/TestMethod 默认发现标记、类型/方法改internal，子角色参数由MTP filter改为同exe显式角色。Render、16场景、差分文件、nonempty、threshold=0、exact断言均保留。
- NativeRun.cs：仅增加 --bm-fill-control-points 分派，await 原实验方法；成功才return0，失败异常不转成功。--supervise-exit 优先及默认 MTP 返回码路径未改。

## 4. 最终源码编译与控制

最终撤回后 x86/x64 Release self-contained 正常依赖图发布均成功退出0。独有 final-x86 与 final-x64 下 obj/bin/publish/package，无NoDependencies。AotDllPath复用 BO包内旧候选，其SHA与BN报告BD候选一致；未重发生产。最初 probe 发布因误填 x86/package 路径失败 MSB3030，改为实际 com-acceptance-package 后成功，未掩盖该失败。WarningLevel=0用于避免警告输出截断，不宣称零警告。

|最终格|task/监督/子|测试SHA / MVID|结果|
|---|---|---|---|
|x86|4af8284f2a214d7cb0fdd85c9adb1319 / 3008 / 7804|83B35C8205BF24B541FE663FA7BCBED3DB75ED3655FEB6B1089A35E93DB89DDD / 51a184a4-864c-4499-8282-c5e2c4e04db8|路径控制1/1，ExitCode0|
|x64|3a8c11f6b4eb4de49c52db2a71b6e23b / 6504 / 9432|3618466036359A53BC1030F6F3BB949B7BBEA8304E519A185EB41466B5441FBC / bb4fb7f5-a654-452a-9b3b-defec0e1a303|路径控制1/1，ExitCode0|

各 final-*/readback/bp-control/supervision.json 全文38行与 TestResults/child.trx 全文26行已亲读：Completed、其余失败/未执行/超时/中断0；Reason=exited，非timeout/cancel/kill，双流完成，SupervisorError=null。这是路径控制，不是sampler/full通过。两份 Results/aot-identity.json 已亲读，X86/4与X64/8，.NET10.0.13；旧候选SHA分别 E0D3F5EFEDFACFD5CC3491DD43B194E19028E8B541A462123C8F73C73D6E25B5 与 1C381539E7C82DDD75908C0563F0AC062E583166BDB23AEE066BDD21E838752F。

最终 x86 同exe list-tests 对 BM 原方法名与 BO新合同名联合筛选：task975a34092de64786908ad22ae172d043，列出0项，结果在discovery-x86。不是skip，也没有禁用既有历史生产测试。

## 5. BM 显式实验仍真实失败

最终x86显式角色 task2ba9fb83b2624ea4ba1b72b8ad0188d7，结果 bm-explicit-x86/Results/bm-siblings/1179cbcf3e954e6c961cec25c3e0c9a3/。官方与候选子渲染完成，原exact断言抛 AssertFailedException，执行接口返回未处理异常；没有独立数值 Process.ExitCode，不冒填2或其他值。显式实验不走MTP，无TRX。

comparison.json全文已读：threshold0、exact=false、hardwareVerified=false；16场景均nonempty，差异计数依次 arc fill22/4、stroke128/122；cubic fill10/12、stroke108/120；ellipse fill8/40、stroke190/200；rounded fill24/32、stroke104/94。这是复用旧候选，不代表当前生产源码结果；没有删除fill或stroke失败。两份 original/candidate/identity.json全文已读，绑定最终x86测试SHA和MVID；实际官方SHA5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834、版本10.0.13，候选SHA同上，backend RenderTargetBitmap software，不冒称HW。

## 6. 文件清单与未闭项

最终源码仅上述4文件；文档新增 BO事实保全 replacement-supervised-present-event-contract-fix.md 与本报告。BO/BP试验产物完整保留。未新增Host、无生产/项目/其他代理文档改动。

未闭：真实surface内容重复更新及双架构cleanup验收、原0xC0000374/0xC0000005首次破坏根因、完整线程/资源收敛、原生像素前置稳定性、BM曲线fill/stroke差异、默认全量与完整替换矩阵。未运行x64内容扩展，因x86真实前置已阻塞且本轮要求有界收口。结论只能是实验+撤回及实验运行边界纠正，不是已验收修复或完整替换。
