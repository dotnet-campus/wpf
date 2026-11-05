# JM resolver 修复：JN 独立核证与最终交接

## JN 终态裁决
**有限保留 JM 修复，允许进入后续对应族扩大回归/集成门；不恢复为全量 Green，不宣布完整替换。** 不因此前报告缺失而悬挂已过的有限修复。本轮创建本报告并先登记核证进行中，再亲读源码和现存证据。JN 未修改测试或生产源码、未发布、未执行新矩阵/新 full/大包/脚本/第三 Host；仅补一次正常 Release 默认项目构建。JD 修复、生产 COM/fill/partition、JL/JO 工作不变。

## 根因与实现审查
ExitDiagnostics.cs 明确为所有被监督业务设置 WPFGFX_RUN_ROOT，它是输出根而不是业务角色标志。此前 Rounded 以此判断角色，父 full 已具该变量，因而直接执行 WPF 与 SetDllImportResolver；首次 module=0 与后续 resolver already set 属不同直接症状，不能据此证明生产渲染错误。
RoundedRectangleContractTests.CompareAsync 现在仅用 JM_ROUNDED_ROLE 判角色，必须 original/candidate 且 JM_ROUNDED_METHOD 等于当前精确方法；错误值断言失败，不是 return 空过。无角色时生成唯一目录，顺序 original/candidate 各调用 JmResolverIsolation.RunAsync。
helper 使用 Environment.ProcessPath 同 exe（dotnet host 时补当前程序集），UseShellExecute=false，以 ArgumentList 传 --supervise-exit、角色输出根、35 秒及精确 FullyQualifiedName。专属两变量经 ProcessStartInfo.Environment 传递并由现有监督器继承给业务；未改父全局环境。业务分支只 Run，不重新调用 helper，无递归；native resolver 只在角色内安装，角色各独立进程。Run 断言非零 module，再按 BaseAddress 查实际模块路径并计算 SHA，不拿 requested 冒充 actual。

## 生命周期与默认安全
JN 完整读取 JmResolverIsolation.cs（91 行）与 JmResolverIsolationTests.cs（72 行）、Rounded（171 行），并核 NativeRun/ExitDiagnostics 相关路径。helper finally 对已启动未退出进程 Kill(entireProcessTree:true)，退出等待 45 秒、kill 后 5 秒、输出 drain 3 秒、取消后再 3 秒，均用 WaitAsync 有界；最后写 parent-role-exit。取消后第二次 TimeoutException 会传播失败，不会成功或无界等待。Kill 自身或 I/O 故障仍可能使后续清理不完整，本轮没有模拟所有未来故障，不能称全面异常验证；未发现必须为本次有限修复新增修改的确定无界 await。现有监督器另有35秒期限、kill后10秒等待、5+5秒drain。
helper 要求正常 exit0、drained、业务监督 HasExited/OutputCollected、精确 filter，identity PID等于业务监督PID且不同于父/内监督。TRX必须存在且可解析，executed=1/passed=1、唯一结果Passed、唯一TestMethod名称正确；缺TRX、0tests、多tests、非零exit或异常均不能被当作通过。独立负控注入未执行，不冒充已实测。
角色 STA thread.Join 45秒有界，内监督35秒先行收敛；没有把 process dispose 当作进程退出证明。运行中的正常退出/无kill仅证明本次正常路径，不能证明长期资源无泄漏或全部异常树回收。
默认无专属变量的 Rounded 入口不安装父 resolver；JD offscreen 未改。无skip/阈值修改/分析器压制/公共设施重构/Present文件争用。

## 双架构实际身份（不移绑旧 full）
证据本地根为 artifacts/jm-resolver-isolation/readback/{x64,x86}/Results/jm-{x64,x86}，相对于 WpfGfxShape 根。
|项|x64|x86|
|---|---|---|
|任务|05af1f852ce14bd3b87f31f1504f2579|3d85ef32e4b0495d95c442916d1cc580|
|外监督 / 业务父 PID|2076 / 5276|9304 / 7180|
|test SHA256|F8E793FC1052F04A35921143372185D754D2D8F03844EB9273C75AB524F67FA9|7BC7D8771768DB340192D7A732D514CE810651722E099EA85EA1BE8485BF43AD|
|test MVID|57d80e54-315c-433b-9983-ea633c2b40e0|74eda3ed-a790-4f90-a38f-7885bc45aff8|
|候选 actual SHA256|0C16A66C898269A17DBDD062012898FEBE338E341FED8D0B7FEC47FB21B6A23A|D5CE061DA53A1578AE7895AAF4F1118DAC6F42FF07806D53059AEE94CFC52ABA|
|官方 actual SHA256|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|
两架构 runtime 均 .NET 10.0.13；官方模块版本均 10,0,1326,50504 @Commit:4099f83977f4d29604a9aeb971fb0a7bc1d5f699，候选1.0.0.0。actual 官方路径为各 C:\CodingAgentSandbox\Tasks\<任务>\wpfgfx_cor3.dll，候选为同任务 Native\wpfgfx_cor3.dll；requested/actual相符。identity明示 RenderTargetBitmap software contract control only、hardwareVerified=false，不冒窗口GPU证据。未独立补测所有WPF版本/文件身份或生产源码精确闭包。
绝对父TRX分别为 C:\CodingAgentSandbox\Tasks\05af1f852ce14bd3b87f31f1504f2579\Results\jm-x64\TestResults\child.trx 与 C:\CodingAgentSandbox\Tasks\3d85ef32e4b0495d95c442916d1cc580\Results\jm-x86\TestResults\child.trx；本地对应根/TestResults/child.trx。JN读两父TRX Results/Definitions/Counters和完整监督/stdout/空stderr：各total=executed=passed=3、failed=notExecuted=0、Completed，三结果Passed，正常Exit0，无timeout/cancel/kill/drain超时，OutputCollected=true。外层工具数值退出未重新取得，不伪造。

## 角色核心内容与父状态
亲读双架构 rounded 代表目录 x64 rounded-ah/13525040526c4929b841975af56d10e0、x86 rounded-ah/013e3ca873df419e86dafb3aad1e8711 的双方identity及comparison；6帧每帧差异0，threshold0/exact=true。亲读曲线 x64 curve-an/2231bb7c69694b54a574b5a5b94de9d1、x86 curve-an/23e10883500d496e9f2fe13c43c34ea7 comparison及完整control-alpha：10帧逐帧差异0；两侧7/8帧相对6帧alpha差0，9帧7像素变化相同，包括(16,10)120→116、(7,16)128→116、(6,17)48→36，非仅空白相等。源码逐帧断言角点、中心alpha255、新红蓝通道255以及前后不相等；本轮未另解析所有原始pbgra32字节，内容结论由运行断言及亲读差分/alpha支持，非独立重算像素。
代表角色监督/TRX/parent-role-exit亲读：x64 rounded original监督10396/业务4624，x86 rounded candidate6336/6508；x64 curve candidate9520/6092，x86 curve original6736/7444。各精确方法filter、35秒、exit0、正常收集、无timeout/kill，角色TRX各1/1 Passed；方法/角色/父PID传递一致。未逐份重审所有重复角色和全部曲线附加产物，不称全审。
混合回归1db769bc-7295-84c4-9198-6119b49e4590实际Passed：Rounded→offscreen→Curve→Rounded→Bitmap。两父before/after完整JSON均读：dispatcherPresent=false、sameDispatcher=true、shutdownStarted/Finished=null；这不是已存在Dispatcher关闭状态测试。PresentationCore/WindowsBase resolver均present=false、unchanged=true，配合源码ReferenceEquals断言，证明所测父对应两程序集未新增或替换resolver。x64两MVID 6cb5dedb-eea3-4832-9a08-c0225e9ff82f / 4a9bd632-52d4-4987-ab0d-233b10ca1095；x86 2934bccd-58e4-4afe-8677-ec985888f34e / caaa784a-19de-4688-807c-60072df89c5d。
亲读jm-business-pids：x64[5200,6372,476,4624,7532,1160]，x86[1428,10412,6508,9896,1516,6812]。各6个互异且排除业务父；断言与目录筛选覆盖Rounded/Curve三组双方，不把offscreen另算入六个，也不把内角色计成父通过数。

## 完整93失败表：可证映射与未知范围
原完整两表保留：artifacts/jk-readonly/readback-complete/Results/jj-full-inventory/{x64,x86}-failures.json（各93项，原ID/父名/DataRow/message/stack/source/raw不改）。JN在完整两文件范围检索resolver、SetDllImportResolver、module及Rounded，精读两架构相同目标条目1235–1257行；检索而非逐条全文重新定因，不能称93项全面独立审结。
|旧full ID（两架构相同）|旧失败直接内容|本次可证结论|
|---|---|---|
|17bea8a5-bb70-8ccd-b35b-5694c41d27d2|Rounded Run module=0|修复专属角色后双方actual非零及6帧差分，父同ID Passed|
|13a2f345-685a-81b1-bcce-02ed89041f18|Curve Run→SetDllImportResolver：A resolver is already set for the assembly|同一类第二入口污染传播已由进程隔离消除，父同ID Passed|
这两项无显式DataRow，是目前唯一可证修复映射（每架构两项，不合并成新full）。完整表中的resolver字面命中只指向该Curve条目，module断言命中指向Rounded；没有证据推出额外已修传播簇。其它条目统一保持“本轮未证明受JM修复”，不是认定均无关或全已修。Present E_INVALIDARG、fresh显示前置、shaderBounds像素差（105b96c7-ae45-8225-b7ac-be58d2f59b51）、WindowQ current缺失（152af104-a31b-8c19-ab9f-377322bd9488）等不得归为本修复清零。源码搜索另见BmFillControlPointTests/ReplacementStateSequenceWindowTests仍使用WPFGFX_RUN_ROOT分支；这是后续检查线索，未改、未证明新失败，不能抢占JL/JO或外推本修复覆盖。
旧full各1802/1709/93/0、Exit2仍是历史真实值。禁止以本轮两项通过相减声称新full91失败；本轮定向3/3只绑定上表各架构test身份。

## 最终正常构建及执行账
JN专用构建一次：WpfGfxShape.ComAcceptance.csproj，Release，无额外属性/分析器压制，Exit0、129 warnings、0 errors、2.69秒。日志已读；含CA2007/MSTest等既有及当前提示，不称零警告，也未为消警告尾改源码。net10.0-windows/Nullable enable，SDK约定10.0.107/rollForward latestFeature；未改TFM/SDK/LangVersion。此构建不发布生产、不替代业务运行、不移绑JM testSHA，也不是最终四格。JN新增运行0/发布0/生产修改0/测试源码修改0。

## 唯一后续与开放门
保留当前有限修复，下一执行者仅以最后源码扩大对应resolver族回归，或在协调后进入后续集成full门；本轮停止，不重复已过矩阵、不做无信息异常大全。其它93失败仍按原完整表及JL/JO责任线逐项处理。HWloss、Available、media/resources、final四格继续开放，完整替换未完成。

## 历史：JN核证进行中
本报告最先创建时仅登记接管与核证进行中，无运行通过主张；handoff先登记同状态，JL/JO/JM及既有历史均未覆盖。当前以上JN终态取代进行中为本轮裁决。
