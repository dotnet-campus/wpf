# BC SwapChain 夹具最小修复与托管 full

## 结论与范围

**原 x86 十项失败已有界重现并最小修复；修后 SwapChain 双架构各13/13，适用兄弟族各95/95，无过滤托管 full 各4256/4256。不是 COM full、最终四格或完整替换完成。**

本报告在修改/实验前创建，结束更新回读。已读执行规则、完整替换方案、交接及 BA scene-diagnosis。仅修改 `Tests/WpfGfxShape.Tests/Direct3D9SwapChainTests.cs` 和指定四文档；不改生产、其他测试、入口、项目、曲线、golden/阈值/skip/fallback，不新建 Host/项目，不禁并发。仅专用工具及 Windows 沙箱执行正式纯 C# 测试 exe，无终端/shell/直接间接脚本。独有产物根 `artifacts/bc-swapchain-fixture/`，未清理别人产物，未重发 NativeAOT。

本文路径相对根 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape`；不可遗漏末级 WpfGfxShape。

## BA 原始证据亲核与修前有界复现

亲读 `artifacts/ba-scene-diagnosis/full-x86/TestResults/ba-full-x86.trx:25716-25724`：Failed，4254执行/4244通过/10失败，其余非通过计数0，RunInfo明示退出2。搜索确认十个 failed业务结果；4130起原始栈为 AddRefSurface:269 或 ReleaseSurface:276 的 IndexOutOfRangeException。亲读 x64同名full TRX:25533-25536，Completed 4254/4254。两full实际identity JSON全文已读；BA scene44→45槽修复见其报告，不把该夹具修复说成历史AX终止唯一根因。

先使用现成 BA `artifacts/ba-scene-diagnosis/managed-x86/publish/WpfGfxShape.Tests/Release/win-x86/` 包，不重建扰动身份，定向一次：SwapChain+既有OwnershipFixtureAssemblyIdentityTests，120秒上限。回收 `pre-swapchain-x86/TestResults/bc-pre-swapchain-x86.trx:266-274`：11执行/1通过/10失败，退出2；十个业务均同原异常，身份用例通过。actual .NET10.0.13、X86/4，Tests SHA `561B943614982415E64CC9B787FA1987CDD55556B60FA240DD7BB4FC10AAF9CB`、MVID `f037850d-e833-4e9d-94ff-2d62ea89c130`；wpfgfx_cor3 SHA `4762736A33F6591DB81BB5F7537282B1C2F284D3065D4677C040930CB576706C`、MVID `5f91b380-9b4d-4aa8-b6f2-5a1112b31479`，与BA full-x86一致。无需额外修前 full。

## 确定根因、合同与最小修复

surface对象槽0为LpVtbl，内嵌vtable始于对象+sizeof(nint)。原索引读写 `*((int*)surface+2)` 固定字节偏移8，并非对象第三个指针槽。x86：vtable[1]也在偏移8..11，安装AddRef指针覆盖0/1索引，之后GetDesc/AddRef/Release把代码地址当两元素数组索引。x64：偏移8..11覆盖未使用的vtable[0]低半，已使用回调未覆盖索引，解释原x64通过，但布局仍不自洽。不是数组太短、refcount增加或随机竞争。

原分配已是 `14*sizeof(nint)+sizeof(int)`：一个对象指针+13个vtable槽+尾部int。修复仅把索引写入和读取统一移到对象+14个指针，即x86偏移56..59/分配60字节，x64偏移112..115/分配116字节；最高GetDesc vtable[12]分别在52..55/104..111，彼此不重叠。未扩大分配或数组、未取模，保留两表面独立的AddRef/Release计数及原全部断言。新增参数化最小回归 `WhenSurfaceVtableIsInitializedThenFixtureIndexIsPreserved(0/1)`，构造完成后独立期望索引等于输入，full分母增加2。

独立合同核对：COM对象首字段是函数表指针，IUnknown AddRef/Release为槽1/2且返回uint，IDirect3DSurface9 GetDesc槽12返回HRESULT并写SurfaceDesc；本夹具显式Stdcall函数指针与UnmanagedCallersOnly签名一致。SwapChain QI/AddRef/Release/GetBackBuffer槽0/1/2/5，Ex Release槽2，返回与参数一致。base分配11指针、最高绝对槽6；Ex分配4、最高绝对槽3，无同类越界。不宣称直接观测ESP，也不把该静态核对冒充官方实调用验证。

生命周期：测试先using两surface后swapchain，业务在释放夹具前Dispose或manager DestroyAll；失败初始化也先释放缓存资源再离开using。原DoNotParallelize和Initialize重置保留，进程默认Workers4/MethodLevel不变。索引只标识表面，计数只在独立托管数组，释放不改索引。AddRefSurface、ReleaseSurface、GetSurfaceDescription共用同一读取点，三条同因路径一起修复。全Tests范围固定int*+2文本搜索仅本文件这两处；不是全库unsafe审计。适用兄弟族实跑Surface、DeviceSwapChainRetrieval、ObjectsLifetime、DeviceInterface，不修改其源码。

## 构建与发布

读取global.json SDK10.0.107/latestFeature、局部Directory.Build.props/targets、Tests net10.0/Nullable enable/既有LangVersion latest/MSTest4.0.1及原SandboxTestEntryPoint。先常规Release win-x86依赖构建成功。首次独有root发布误先设置BuildProjectReferences=false，CS0006缺独有root生产ref文件，未产出运行包；随即两架构分别在同一独有root以正式Tests项目 `-t:Build -c Release -r win-arch` 正常依赖图构建，均成功，然后自包含发布成功。不是隐藏构建失败。

正式发布保持原入口，参数为Release、win-x86/win-x64、self-contained true、RuntimeFrameworkVersion10.0.13、BuildProjectReferences=false、EnableMSTestRunner=true、DefineConstants=SANDBOX_TEST_ENTRY_POINT、StartupObject=WpfGfxShape.Tests.SandboxTestEntryPoint。最后false只用于已正常构建的同root依赖，不跳过生产依赖构建。双架构共四个成功构建/发布日志均58行，有既有CA1416/MSTEST0044警告，不宣称零警告。独有包根 `managed-{arch}/publish/WpfGfxShape.Tests/Release/win-{arch}/`，生产源码未改，但构建二进制身份不假定与BA相同。未修改TFM/SDK/语言版本。

## 实际运行与顶层TRX回收

以下目录均相对 `artifacts/bc-swapchain-fixture/`，每次只上传对应publish包、执行WpfGfxShape.Tests.exe，完整任务回收。全部120秒上限，默认Workers4/MethodLevel；原夹具DoNotParallelize保留。参数均有 `--report-trx --report-trx-filename <下表文件>`。入口把results-directory设为任务内绝对 `C:\CodingAgentSandbox\Tasks\<task>\TestResults`，身份在同任务Results。

|目录|task|parent TRX文件|亲读实际结果|
|---|---|---|---|
|pre-swapchain-x86|b01446d294384d19b3e183995b04ea14|bc-pre-swapchain-x86.trx|11执行/1通过/10失败，退出2|
|swapchain-x86|827975fd9cd941d9924de06b0b1a409a|bc-swapchain-x86.trx|Completed13/13|
|swapchain-x64|ccd41fcfb2c843b1b7298975a1fb33a9|bc-swapchain-x64.trx|Completed13/13|
|family-x86|d58948ba4e4b4e3c9b6866016c5453f0|bc-family-x86.trx|Completed95/95|
|family-x64|a227dc4b97814ca7954113c0ab86adc2|bc-family-x64.trx|Completed95/95|
|full-x86|3cccc43545b242c58961b94bb6bb3604|bc-full-x86.trx|Completed4256/4256，runner3s561ms|
|full-x64|56231e885e534e938a76a331abb758a3|bc-full-x64.trx|Completed4256/4256，runner2s998ms|

parent路径为 `<目录>/TestResults/<文件>`。定向TRX:95-98、族:587-590、full:25545-25548均已亲读。修后failed/error/timeout/aborted/inconclusive/notExecuted等非通过计数全0；工具未返回明确数值退出字段，不杜撰exit0，结论来自完成TRX而非console进度。

定向filter：`FullyQualifiedName~Direct3D9SwapChainTests|FullyQualifiedName~OwnershipFixtureAssemblyIdentityTests`（10原业务+2新增参数+1身份）。族filter：`FullyQualifiedName~Direct3D9SwapChainTests|FullyQualifiedName~Direct3D9SurfaceTests|FullyQualifiedName~Direct3D9DeviceSwapChainRetrievalTests|FullyQualifiedName~Direct3D9ObjectsLifetimeTests|FullyQualifiedName~Direct3D9DeviceInterface|FullyQualifiedName~OwnershipFixtureAssemblyIdentityTests`。两full完全无filter，各仅一次，没有追Green重跑。

## 修后actual SHA/MVID

七份运行 `Results/av-ownership-assembly-identity.json` 全文亲读；修后同架构定向/族/full身份一致。以下为full实际身份，不沿用BA哈希。实际进程和程序集路径均是上表task根下 `WpfGfxShape.Tests.exe`、`WpfGfxShape.Tests.dll`、`wpfgfx_cor3.dll`。两程序集Version1.0.0.0/Culture neutral/PublicKeyToken null；x86 X86/4，x64 X64/8，均.NET10.0.13。

|架构/程序集|actual SHA256|MVID|
|---|---|---|
|x86 Tests|ED48CB1AD26CEB1993144EF70A4773E14BC34BE15C72EB55DDA857CD1E2EA0A8|a825679d-e07b-4df6-99f4-b62aeeb6bf82|
|x86 wpfgfx_cor3|BE54FE6DAA3CCCD81A4F5BA7EA921E8E5CF0EAAD226FD06B141B03E7A9ED026E|5d84c201-78a0-48f4-8a71-0a6ee5191015|
|x64 Tests|E31CB7FFCBE00D3F7DB6D1FA7438B5941F98C5E69DA5B82E329FC3FBEC6ABA36|b0221029-074f-4cbc-9010-fb2484052164|
|x64 wpfgfx_cor3|EA4EC77E968B3ABFE905CFB5FBDD3F8DD978676510D0A8AC7F3E57EBDE509E50|a88ed0dd-6925-4238-9bf6-b1694219cb3d|

## 剩余边界

本包托管full无剩余失败或新失败；未运行ComAcceptance，不是最终四格。BA修复的scene已在本包正常依赖快照full中经过，但不追认历史AX未完成的原始终止唯一根因。AY原44/10差、float舍入/真实WPF变换栈/完整HFD仍开；AZ仅AP归档14/14，不绑定本包新SHA、不外推251/251。Capture WAIT_TIMEOUT/resource0、真实屏幕HW/duplication、真实loss→Reset→新色HW消费、持续媒体、长期CPU/GPU/线程/callback与异步关闭、GPU圆角、通知有效target同步/非零SyncModeStatus/迟到消息未覆盖合同继续开放。完整替换未完成。

下一动作：独立审阅本报告/TRX/actual identity；继续有界曲线及真实五维缺口工作。最终稳定集成源码仍需两正式项目×双架构Release完整适用full，不拼BA/BC快照和AZ归档Green。已更新并回读本文、next-session-handoff.md、replacement-issues-and-plan-status.md、replacement-supervised-session-summary.md。
