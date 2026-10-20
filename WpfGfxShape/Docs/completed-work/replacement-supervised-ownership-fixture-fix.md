# AV ownership 夹具越界修复与验证

## 最终状态

**ownership 确定越界已最小修复；x86 最小用例独立两次成功，双架构 interface 族成功；x86 无过滤全量实际执行但无完成 TRX，整体未完成。**

本文在源码修改前先创建，结束更新并回读。仅修改本文、`Tests/WpfGfxShape.Tests/Direct3D9DeviceInterfaceOwnershipTests.cs`，新增 `Tests/WpfGfxShape.Tests/OwnershipFixtureAssemblyIdentityTests.cs`；产物仅写独有 ignored `artifacts/av-ownership-fix-20260721/`。未修改生产、入口、项目、曲线或其他文件；未清理别人产物、未禁并发/skip、未新增Host/项目。未运行终端、shell或脚本；采用专用构建/发布工具及Windows沙箱运行正式C#测试exe。

## 规则、修复和兄弟检查

已读 current-execution-rules、wpfgfx-complete-replacement-validation-plan、replacement-supervised-x86-managed-diagnosis、解决方案与正式测试项目、global.json、局部Directory.Build.props/targets、既有SandboxTestEntryPoint。SDK要求10.0.107/rollForward latestFeature；项目net10.0、nullable enable、既有LangVersion latest未改、MSTest4.0.1。

ownership第88行唯一夹具修改：分配 `sizeof(nint) * 4` → `* 5`。对象头槽0、GCHandle槽1，vtable从槽2起；Release在vtable[2]即槽4，现合法范围0..4。原x86分配16字节却写偏移16..19，x64分配32字节写偏移32..39，均确定越界。调用约定与原有DoNotParallelize不变。

新增独有纯C#正式MSTest身份测试：从实际typeof测试类/Direct3D9Device取得Assembly，记录Location、FullName、SHA256、ManifestModule.ModuleVersionId，以及进程路径、架构、指针宽度、runtime，写到 `AppContext.BaseDirectory/Results/av-ownership-assembly-identity.json`。未改既有preflight或入口。

有界兄弟检查搜索memory+2及4/5槽分配并读布局：

- **`Direct3D9ObjectsLifetimeTests.cs:143-150` 同根因确定越界**：分配4槽，vtable=memory+2，写vtable[2]即槽4。遵守本任务仅ownership/身份文件的独占边界，未修改，必须交该文件所有者修复。
- BitmapLockTests:152-158分配11槽，最高槽10，合法。
- DirectWriteFactoryTests:178-190分配10槽，两个vtable最高槽4/8，合法。
- DirectWriteDisplaySettingsTests:170-189分配24槽，最高槽22，合法。
- query-failure:460-465的dummy backbuffer分配4槽，但vtable+1、[2]即槽3，合法。不能把所有4槽一律改5。不宣称已全面审计全部unsafe布局。

## 最后源码正常依赖构建和自包含发布

两个源码文件最后写入后，x86/x64分别正常依赖Release Build，再自包含Release publish；四次工具均“执行成功”（每次58行日志），存在既有CA1416/MSTEST0044等警告，不宣称零警告。

第一步专用工具命令：`dotnet publish WpfGfxShape/Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj -t:Build -c Release -r win-{x86,x64} -p:WpfGfxShapeArtifactsRoot=<独有架构根>/`。
第二步同项目：`-c Release -r win-{x86,x64} --self-contained true -p:BuildProjectReferences=false -p:EnableMSTestRunner=true -p:DefineConstants=SANDBOX_TEST_ENTRY_POINT -p:StartupObject=WpfGfxShape.Tests.SandboxTestEntryPoint -p:WpfGfxShapeArtifactsRoot=<同根>/`。

架构根为 `d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/av-ownership-fix-20260721/managed-{x86,x64}/`。依赖已在第一步常规配置构建，第二步入口宏不传播给生产依赖。未改既有入口配置源码，未拿AM旧包冒充最后源码。

发布包为 `managed-{x86,x64}/publish/WpfGfxShape.Tests/Release/win-{x86,x64}/`。实际运行该包正式 `WpfGfxShape.Tests.exe`，不是仅工具console。全部沙箱任务完整目录均回收，保留exe/DLL/runtime/TRX/diag/Results。

## 全部5次实际执行与原始证据

以下目录均相对 `artifacts/av-ownership-fix-20260721/`。保留Workers=4、Scope=MethodLevel；实际banner MSTest4.0.1/.NET10.0.13。均传 `--report-trx --report-trx-filename <表中名称> --diagnostic --diagnostic-verbosity Trace`。

|回收目录|沙箱任务号|过滤/时限|实际结果|
|---|---|---|---|
|min-x86-1|bb54127b051142f4909804b27ac9d90f|目标单测 OR 身份；90秒|TestResults/av-min-x86-1.trx，Completed，2/2|
|min-x86-2|e51f6a7fbfa34598ab1d3f189739e472|同过滤独立进程；90秒|TestResults/av-min-x86-2.trx，Completed，2/2|
|interface-x86|ffe22da71f574c8c943a82452a27ef25|interface族 OR 身份；120秒|TestResults/av-interface-x86.trx，Completed，16/16|
|interface-x64|5affc659031446bea324e41e3e82f732|同族与身份；120秒|TestResults/av-interface-x64.trx，Completed，16/16|
|full-x86|824a5a03cdaa40d4a3cbc958c7532f08|无filter；timeoutSeconds=1800|无顶层完成TRX，不能给全量通过数|

最小filter原文：`FullyQualifiedName~WhenBaseDeviceIsDisposedThenOwnedInterfaceIsReleasedOnce|FullyQualifiedName~OwnershipFixtureAssemblyIdentityTests`。每次2个包含业务目标1个+身份1个，不说成两个业务测试。两次TRX第9行业务目标均Passed，分别13:32:38.6658706→38.6831492Z、13:32:56.8901324→56.9074110Z；不包含query族或其他业务测试。

族filter：`FullyQualifiedName~Direct3D9DeviceInterface|FullyQualifiedName~OwnershipFixtureAssemblyIdentityTests`。每架构16含ownership4+query11+身份1，即15/15业务。

最小TRX:29-30、族TRX:113-114已实际回读：total=executed=passed分别2/2/16/16，failed/error/timeout/aborted/notExecuted及其他非成功计数全0。不能相加冒充全量。五次Results身份JSON均回收且逐个回读；x86四次SHA/MVID一致，路径按任务号不同。

## 实际加载身份

每次JSON完整Location是 `C:\CodingAgentSandbox\Tasks\<上表任务号>\WpfGfxShape.Tests.dll` 和同目录 `wpfgfx_cor3.dll`，ProcessPath为同目录exe；不是预期本地路径。Runtime均.NET10.0.13；X86/PointerSize4，X64/PointerSize8；两个程序集FullName版本均1.0.0.0。

|架构/程序集|SHA256|MVID|
|---|---|---|
|x86测试|CA5583D123D979A815AB5535BF1867053050F491957F02F06982F8AE1DA32533|4549363b-60d4-4484-8a7e-28d936a34cca|
|x86生产|5DA250227E0132086E3805A0B880B9AE429536D7999DF897C02057538F21A679|37e49976-51f2-420b-afef-9ab4aef82ea8|
|x64测试|F359E38ECC67AB40617BDE7B2570F93197875F562CE599544C6D6603BF725FAC|043ceee1-92de-4ec8-ae81-321f9aa52eca|
|x64生产|A25CCA474791DBE6743D711A3CBD942E7D38BB96724F5438DC1A5BC8739918F5|9327faa9-ab9c-4db1-b9c3-77ebead14398|

## 全量未完成及终止状态

full-x86实际无filter执行、上限1800秒，接口只返回banner、4worker及“结果已保存”，没有完成摘要。TestResults仅部署目录和 `log_261019133407169.diag`，没有 `av-full-x86.trx`；Results有独有身份、原preflight、present-notifications。不能把上限配置当观察到超时，也不能把回收当成功。

diag共43759行，亲读43753-43759：先有ObjectsLifetimeTests.WhenFactoryObjectsAreDisposedThenExtendedIdentityAccessIsRejected Passed，之后PixelShaderBoolConstantStateTests.WhenConstantMatchesThenNativeWriteIsSkipped开始；末时间13:34:10.0712829Z，无后续完成。末测试名不是根因；即使objects-lifetime另有确定越界，也不能由相邻关系断定本次终止根因。

五次接口均未提供数值退出码；full未给异常类型、超时标志、栈或dump，具体终止原因未知。前四次成功依据Completed TRX而非“已保存”；full不能给total/passed/failed计数。未重复full追Green，未跑x64 full；时间允许的x86 full已执行但未完成。

## 判定及交接

原AM x86同一最小用例两次只有开始无完成TRX、query族11/11；本次修复后目标两次Passed且有Completed，双架构interface业务15/15通过，支持ownership夹具根因判断，但不是受控负控证明，更不证明它是全量唯一问题。不能把AR末测试当根因。

剩余行动：将ObjectsLifetimeTests同根因4→5槽交该文件所有者，最后源码正常依赖构建/发布后重跑适用矩阵并取身份；继续诊断全量无完成状态，不得关闭整体完整替换任务。

报告写入期间一次专用写入因未先回读初始文档而拒绝，随后读初始内容再正确写入；未导致源码变化或额外测试尝试。
