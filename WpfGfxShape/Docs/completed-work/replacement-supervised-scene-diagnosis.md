# BA x86 scene 族终止有界诊断

## 最终结论与范围

**Scene确定越界已最小修复44→45槽；双架构兄弟族各99/99；x86 full完成但4254中4244通过、10失败，x64 full4254/4254。整体未完成。** 本文件在源码修改和实验前创建，结束更新回读。已读current-execution-rules、完整替换方案、AX lifetime-fixture-full。仅修改本文及Direct3D9DeviceSceneTests.cs一行，不改生产、入口、项目、曲线或其他测试。无新增测试/Host/项目，不改断言/golden/阈值/skip，不禁并发，保留原DoNotParallelize。无终端/shell/直接间接脚本；仅专用工具与Windows沙箱正式纯C# exe，无清理别人产物。

## 只读审查与修复

FakeDeviceObject原分配sizeof(nint)*44，对象槽0，vtable从memory+1开始；vtable[2]/[41]/[42]/[43]实际写绝对槽3/42/43/44。最高槽44需要45槽。x86原分配176字节却写176..179，x64原分配352字节却写352..359。每次构造都写Clear指针，即使业务仅调用BeginScene。唯一源码变更是分配44→45，已回读744-768行。

Release/BeginScene/EndScene/Clear的UnmanagedCallersOnly与函数指针一致为Stdcall，槽2/41/42/43，返回uint/int/int/int；Clear参数self,uint,Rect*,uint,uint,float,uint。未发现本文件另一确定调用约定缺陷，不声称直接测量ESP。静态回调无委托GC根悬挂；业务先声明FakeDeviceObject再device再swapchain，using逆序释放，device释放时夹具内存尚存活。静态计数由Initialize重置，原DoNotParallelize不变。注入深度表面地址1仅由测试委托消费，不据此擅改。不是全部unsafe代码审计结论。

## 修前AX包有界隔离

使用AX现成包artifacts/ax-lifetime-fixture-full-20260721/managed-x86/publish/WpfGfxShape.Tests/Release/win-x86/，先不重建扰动身份。独有回收根artifacts/ba-scene-diagnosis/，以下目录均相对此根。

|目录|task|上限秒|结果|
|---|---|---|---|
|pre-scene-x86|de85e7e2969648ddbb79942470de61f9|120|无完成TRX，trace157行末为指定参数开始|
|pre-parameter-x86|21a132a17d0c44b89066fa434cf3fcab|120|ba-pre-parameter-x86.trx Completed 3/3|

Scene filter为FullyQualifiedName~Direct3D9DeviceSceneTests|FullyQualifiedName~OwnershipFixtureAssemblyIdentityTests。参数方法filter为FullyQualifiedName~WhenBeginSceneFailsThenHResultIsPreservedAndSceneIsNotCommitted|FullyQualifiedName~OwnershipFixtureAssemblyIdentityTests，包含-2147467259与-2005530520两行+身份1，不伪称单参数独占隔离。TRX两参数均Passed，35-38行计数3/3。

pre-scene-x86/TestResults/log_261019140916063.diag:153-157末时间2026-10-19T14:09:16.7581416+00:00，仅指定参数InProgress/Executing。目录无ba-pre-scene-x86.trx。工具未给数值退出、崩溃栈、dump或明确timeout；如实未知，不能声称访问冲突或已观测timeout。方法隔离成功不证明原缺陷稳定无害；不追加修前重试追Green。

两份Results/av-ownership-assembly-identity.json全文亲读，实际ProcessPath/Location在各task根，X86/4/.NET10.0.13。Tests SHA=871338D4ABDC4A5D1703BC22940293F4F68216B28760F3667506462C0200DC01，MVID=ac2724df-e5a7-483b-b531-fbde8037e8de；wpfgfx_cor3 SHA=5DCF389A383DDA8F93B431337DF73F63806C1A62C5EB41CFEFED52F281DC6011，MVID=24804a9c-7e6a-476e-bab8-3510a846cbe0，均与AX绑定一致。

## 构建及修后运行

读取global.json SDK10.0.107/latestFeature，项目net10.0/Nullable enable/既有LangVersion latest/MSTest4.0.1、局部Directory.Build.props/targets、原SandboxTestEntryPoint。每架构先正式Tests项目-t:Build -c Release -r win-arch正常依赖构建，再Release自包含发布RuntimeFrameworkVersion=10.0.13、BuildProjectReferences=false、EnableMSTestRunner=true、DefineConstants=SANDBOX_TEST_ENTRY_POINT、StartupObject=WpfGfxShape.Tests.SandboxTestEntryPoint。四次专用工具成功，日志各58行；有既有CA1416/MSTEST0044警告，不宣称零警告。包在本根managed-{x86,x64}/publish/WpfGfxShape.Tests/Release/win-{x86,x64}/。未重发NativeAOT，未运行ComAcceptance，不冒充两项目四格验收。

全部Workers=4/Scope=MethodLevel，--report-trx、--report-trx-filename、--diagnostic --diagnostic-verbosity Trace。入口补任务内绝对C:\CodingAgentSandbox\Tasks\task\TestResults，身份在同根Results；整个任务回收，不限制output子目录。

|回收目录|task|秒上限|parent TRX结果|
|---|---|---|---|
|post-min-x86-1|68e9366418b54b0bb693f6527cc814ee|120|ba-post-min-x86-1.trx Completed 3/3|
|family-x86|e64a5d43e6f944f5a3dd23756b217152|180|ba-family-x86.trx Completed 99/99|
|family-x64|c4f2f7c5bd4b47e9878180e0d2f34e8e|180|ba-family-x64.trx Completed 99/99|
|full-x86|db3e0c42d6584287ad923eb875ff640e|120|ba-full-x86.trx Failed 4254执行/4244通过/10失败，runner3s120ms|
|full-x64|d1ba2b988c6b4664ac1166f0b8baa5ba|120|ba-full-x64.trx Completed 4254/4254，runner3s270ms|

最小filter同修前参数方法+身份。族filter为FullyQualifiedName~Direct3D9DeviceSceneTests|FullyQualifiedName~Direct3D9DeviceDepthStencilStateTests|FullyQualifiedName~Direct3D9DeviceInterface|FullyQualifiedName~Direct3D9ObjectsLifetimeTests|FullyQualifiedName~OwnershipFixtureAssemblyIdentityTests。两full完全无filter，各仅一次，不再1800秒无信息重试。

亲读TRX：最小35-38行、族611-614行、x86full25716-25724行、x64full25533-25536行。除x86 failed10外，所有非passed计数均0。x86full RunInfo明确记录Exit code indicates failure: '2'，可见退出2；其他运行工具未提供明确数值退出字段，保持未知，不凭banner杜撰exit0。x86full是完成失败，不是无TRX终止。

## 修后actual identity

五份修后JSON均全文回读；实际ProcessPath为对应task根WpfGfxShape.Tests.exe，Location为同根WpfGfxShape.Tests.dll/wpfgfx_cor3.dll。两程序集FullName均Version1.0.0.0/Culture neutral/PublicKeyToken null。所有.NET10.0.13，x86为X86/4，x64为X64/8；同架构最小/族/full一致。

|架构/程序集|actual SHA256|MVID|
|---|---|---|
|x86 Tests|561B943614982415E64CC9B787FA1987CDD55556B60FA240DD7BB4FC10AAF9CB|f037850d-e833-4e9d-94ff-2d62ea89c130|
|x86 wpfgfx_cor3|4762736A33F6591DB81BB5F7537282B1C2F284D3065D4677C040930CB576706C|5f91b380-9b4d-4aa8-b6f2-5a1112b31479|
|x64 Tests|F8BCA2BDD6D108615DCF7972674B4B5AB89FA22681015EC71E88EDAED9FAC2BD|90e64f23-d161-4755-ab09-7e837632216e|
|x64 wpfgfx_cor3|67DAAAB0BA7467F7CFEA7C11C375E0EEA431A3E6C74A7F61D7F8A6D5821AFAC5|99728a30-ebf1-4fc9-8b6d-47a7b146ceb8|

生产托管包身份与AX不同，仅绑定本次正常依赖图快照，不把AX旧full算到新SHA；未修改生产源码不等于构建二进制身份必相同。

## 剩余与下一动作

x86 full可见10个失败均为Direct3D9SwapChainTests，原始异常IndexOutOfRangeException，AddRefSurface:269或ReleaseSurface:276；调用链含TryGetBackBuffer/ReleaseD3DResources等。范围外不改，异常名不直接证明其根因。由该文件所有者基于actual identity、full-x86/TestResults/ba-full-x86.trx及log_261019141218437.diag审查x86布局/索引/生命周期，先有界隔离10例，不盲禁并发/skip或无限full。

Scene越界修复有确定边界依据；修前族无完成而方法隔离成功，说明最小复现依赖上下文，不以成功追Green算原终止闭合。修后full已越过scene并完成，但不能据此证明历史AX终止唯一根因。最终仍需稳定源码Tests/ComAcceptance×x86/x64 Release完整适用矩阵及官方差分、HW恢复、长期资源等五维原始证据；不得拼旧快照或局部Green。
