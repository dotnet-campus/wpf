# CQ supervised fill exact matrix

## 1. 裁决与边界

已交付并实际运行可独立筛选的真实 WPF fill exact 矩阵。最终双架构 Release 父测试均 **5 executed / 2 Passed / 3 Failed，监督 Exit2**。这是正确暴露现存像素差异，不是产品通过。没有恢复 CN 挂接，没有修改生产、CF 三文件、NativeShapeReader、SoftwareFillBezier、NativeRun、ExitDiagnostics、打包、历史测试或共享交接。

唯一新增测试：`Tests/WpfGfxShape.ComAcceptance/CqFillExactMatrixTests.cs`；本报告为唯一新增说明。专属根 `artifacts/cq-fill-exact/`。仅使用专用读写/发布/沙箱工具，无终端、直接或间接脚本、新入口、Host 或第三工程。

读取执行规则27行、完整方案120行、CO32行、CN25行、CH59行；最新handoff最新 CO/CM/CH节已读，更早长历史未全文回读，不冒称完整交接所有305行均亲核。

## 2. 可运行合同

编译符号：`CQ_FILL_EXACT_DIAGNOSTICS`。只隔离本次新测试，不隔离 BW/CK 或既有失败。使用原项目 net10.0-windows、MSTest4.0.1、原 NativeRun MTP入口。

筛选：`FullyQualifiedName~CqFillExactMatrixTests`。独立组后缀分别是 `.WhenframesFill`、`.WhendeviceFill`、`.WhenprecisionFill`、`.WhensiblingsFill`、`.WhenHistoricalStroke`；方法名大小写以源码为准。

fill每组启动独立 official/candidate/official-repeat 进程，通过现有 BL ExitDiagnostics 监督，角色各自固定任务内绝对 TestResults/child.trx。真实 WPF DrawingVisual/StreamGeometry/RenderTargetBitmap，Pbgra32，逐字节 threshold0，alpha非空；任何目标差异、官方repeat差异或空缓冲均导致父 Assert Failed。不存在 separator/be 豁免、historical fill 豁免或 evidenceOnly通过。

复用 CurveDeviceQuantizationExperiment.Render 生成因果场景与全部 A–F 模式、原始像素及参数/位级trace。仅 real-cubic/real/line/line-sibling 是正式候选fill目标，实验显式 A–F 不是替代真实输入的预期。官方 B/E、D/F差异另断言至少一例非零；本次各16/16可判别。复用测试私有场景生成器使用反射，不反射生产候选生成预期。

原十帧复用 RoundedRectangleContractTests.Render(directory,true)，保留其中心非空、新颜色和非旧帧断言；额外保存 scene-contract.json。BW narrow参数原样复用。siblings覆盖arc/cubic/ellipse/rounded，以及Bz rectangle/ellipse/rounded/group identity/nested。

stroke单独16场景，完整缓冲旧新比较，不与官方exact混算。`CQ_BASELINE_DLL`可指定旧候选；未提供时以当前包作冻结快照，本次 baselineIsCurrentSnapshot=true，故本轮只证明同当前CF快照独立重复不变，**不声称不同修订stroke不回归**。下一轮生产包应显式指定保留CF旧候选绝对路径。完整旧新原始缓冲均保留。

## 3. 包、执行与绝对路径

复用确实存在的 `artifacts/cf-curved-precision/default-{x64,x86}/com-acceptance-package/Native/wpfgfx_cor3.dll`，未寻找遗失BU归档，未生产重发，不用官方代候选。

最终包：`artifacts/cq-fill-exact/final-{x64,x86}/com-acceptance-package/`；同根 obj/bin/publish 独占。发布使用专用发布工具、原csproj Release/RID/self-contained，ComAcceptanceSandbox=true、DefineConstants=CQ_FILL_EXACT_DIAGNOSTICS、AotDllPath指向上述CF真实包，WpfGfxShapeArtifactsRoot/BaseIntermediateOutputPath/BaseOutputPath/PublishDir均指向各自final-arch根。双架构发布成功，无项目或依赖图改动。

沙箱专用执行参数数组：`--supervise-exit`、`Results/cq-final-<arch>`、`120`、`FullyQualifiedName~CqFillExactMatrixTests`。整任务回收，没有只取Results漏TRX。

|架构|最终task|完整回收目录|
|---|---|---|
|x64|c83487e1b462477ea949c036bae4083c|artifacts/cq-fill-exact/final-run-x64|
|x86|834ce6996c8c40af85e3f47f00e8ffb1|artifacts/cq-fill-exact/final-run-x86|

父TRX绝对路径：`C:\CodingAgentSandbox\Tasks\<task>\Results\cq-final-<arch>\TestResults\child.trx`。角色TRX：`C:\CodingAgentSandbox\Tasks\<task>\Results\cq-fill-exact\<frames|device|precision|siblings|stroke>\<role>\TestResults\child.trx`。

先控制：初次DisplayName筛选未匹配，task78a92e9a333a40ecbc84a3fd5919c763真实TRX total0/Exit8，非通过，保留control-x64。已改独立无参方法筛选；control-final-x64 task eb6b4094ae104c68a91f49082f0176a9父TRX1/1 Passed亲读，之后才跑整套。首次矩阵matrix-x64/x86亦保留；最终变更后重新双架构发布执行，以下只绑定final-run。

## 4. 实际身份

从实际子进程Modules按加载基址取得actual路径与SHA，不是仅aot输入清单。final各组candidate identity已直接亲读，frames三角色identity/监督/TRX计数亦读取。实际.NET10.0.13，X64/8与X86/4，software RTB，不声称硬件。

|身份|x64 SHA256|x86 SHA256|
|---|---|---|
|实际候选|36353DDE3BA720B642A6F12CEE8AB0EDDA501BD7DC49F35270063480B8A0532A|C4D7D4699F6334AC164DE95573D382807803ED65E4FE42AA05A9559FEA7F44EB|
|实际官方|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|
|最终测试|F38E983BE6C303BDC5AF64D5C7675714C4DDE1E5BE6BCBBBF933A6F00CB9A708|27145F5BD32D0DEEE8471BD9D5283E519E61AFD5FA19871081D4458C17DA141B|

测试MVID：x64 `496b7502-f1c2-49e4-9f31-174820e8752e`；x86 `4a6f1484-1aee-445e-a399-6b51333b479b`。官方版本 `10,0,1326,50504 @Commit:4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。actual候选为任务 `Native/wpfgfx_cor3.dll`，官方为任务根同名DLL，与requested一致。

## 5. 实际结果（两架构一致）

父TRX计数5/5/2/3，frames/device/siblings Failed，precision和独立stroke Passed。两父监督全文38行亲读：exited/Exit2，非timeout/cancel/kill/drainTimeout，SupervisorError=null。子角色通过仅表示完成渲染，父exact负责真实差异失败，不能将子Passed当fill通过。

每架构正式fill206场景：frames10、device178、precision2、siblings16。每场景差数、repeat差数、nonempty及Passed/Failed完整表均在 `final-run-<arch>/Results/cq-fill-exact/<group>/scenes.tsv`；完整comparison.json含每侧像素SHA，逐字节diff.json列offset/expected/actual。所有原始pbgra32、参数JSON、separator-search与位级trace均随整任务回收保留。两架构TSV已逐组分段亲读；长返回截断尾115–120已补读。未手动重算所有原始像素SHA。

原十帧0–9差数：`0,44,0,44,0,44,0,0,0,10`；对应frame1/3/5/9 Failed，其余Passed。全部非空，官方repeat0。

负半格real cubic below/exact/above分别14/30/30，全部Failed；对应line sibling全0 Passed。float-below/above cubic各30 Failed。drawing/geometry/visual cubic各46 Failed，nested20 Failed；其line sibling均0。

BW narrow rectangle/ellipse各0 Passed。siblings arc identity/nested22/4、cubic10/12、ellipse8/40、rounded24/32全Failed；primitive ellipse/rounded nested各18 Failed；primitive ellipse identity、rounded identity、rectangle identity/nested、group identity/nested各0 Passed。

B/E-positive4样本 × placement4：real差数按drawing/geometry/nested/visual分别：sample0=22/36/10/22，sample1=18/40/22/18，sample2=16/26/22/16，sample3=36/44/22/36，全部Failed。geometry line分别44/40/48/40 Failed，其余对应line0 Passed。

D/F-positive4样本 × placement4：real差数按drawing/geometry/nested/visual分别：sample0=42/42/38/42，sample1=46/46/32/46，sample2=46/46/26/46，sample3=48/48/32/48，全部Failed。nested line28/20/20/18 Failed，其余line0 Passed。官方B/E和D/F各16个模式对全部可判别，discrimination.json两架构已亲读。

AB/CD附加因果目标也严格exact，无豁免；逐场景结果在178行device表，不用“差数相同”替代完整缓冲比较。所有正式fill目标nonempty=true且official-repeat差0。

## 6. 回收核验与限制

直接读取两最终父TRX计数/关键失败和两监督全文，六frames角色实际identity与监督/计数；device/precision各六原角色TRX计数直接读取，均1/1 Passed；其他角色TRX均完整存在且由C#读取计数写role-evidence.txt，已搜索确认14个/架构角色计数，不声称全部角色TRX全文逐字亲读。完整关键逐场景比较以scenes.tsv全读，stroke完整comparison两份87行已读；长JSON逐项SHA尚未全部人工全文复核。缺失未被隐瞒，没有“无TRX”结论。

本轮没有默认BW/CK/历史full实跑，未改变它们，旧失败仍保留；无不同生产修订stroke比较。本矩阵已经真实red且可用于后续fill挂接，不声称SoftwareFillBezier已消费、生产集成通过、GPU/资源恢复等关闭或完整替换完成。
