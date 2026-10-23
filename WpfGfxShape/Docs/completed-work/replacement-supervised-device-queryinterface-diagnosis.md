# DU：Device QueryInterface 精确调查与安全收口

## 1. 范围与已读材料
遵守用户限定：只允许Interop实现、DU独有显式诊断测试及DT/DU报告；不改DR/DS、NativeRun、ExitDiagnostics、VisualCache、historical、项目打包及共享交接。不运行full，不用终端/脚本/原生探针/机器码，不新增工程。
已读当前执行规则、完整120行实施与验证方案、DR报告全文（截断尾段另读）、DT初稿、产品与两个测试工程配置、global.json及InteropDeviceBitmapExports.Create/CheckDevice。SDK10.0.107；产品net10.0、Nullable enable；正式ComAcceptance net10.0-windows。未改版本。

## 2. DT已得证据（非DU新运行）
两次原始supervision全文、业务jsonl全文及阶段全文已亲读，已先补入DT报告。
trace任务bb07e1b0a1b34c56afa5b3cfeb0994d4，nativeSHA027A85B3B0289A022AFE362522987E765105A9BDB6131A1456DC83C55BE99F07；check任务c7956fed558a40b79bd0f1b464f7980f，nativeSHA334846117BE935338454E3FEF9DD0F74305058DDAC5C0FDB0AFA0A90A6457EC1。两次ExitC0000005；测试SHA871D68BB670B1A7AA543B32DE5A30F6404F32E0BB54B4E66AA33DE1A3367E522/MVID7299f517-0a11-4b19-a667-308884546436；.NET10.0.13/X86。
合法surface.QI/GetDesc/GetDevice均S_OK；第二次最后为CheckDevice.QI.before。没有故障指令地址、QI返回、对象完成释放或完成父TRX。只定位边界，尚不确定根因。

## 3. 精确API及待定分支
产品明确引用Silk.NET.Direct3D9/2.23.0；已通过包API查询核public IDirect3DDevice9包含void** LpVtbl、QueryInterface(Guid*,void**)返回int及三种ref重载。该公开API结果不提供方法体/calli签名，不能由公开签名断言实际x86调用约定。
当前CheckDevice使用局部Guid=IDirect3DDevice9Ex.Guid、extended=null，再device->QueryInterface；失败仍TestCooperativeLevel，成功CheckDeviceState，finally Release。兄弟GetUpdateMethod及GetAdapterLuid也存在生成QI封装。未证明同因，不预先替换。
最早未定分支：同一surface.GetDevice所得拥有引用的device在独立Stdcall vtable[0]调用能否正常返回，以及生成封装实际calli签名/this是否不同；随后才可区分GUID/栈、绑定、AOT或前序内存破坏。before日志不是此分支答案。

## 4. DU实际运行及身份
新增Tests/DuDeviceQueryInterfaceTests.cs，全文受DU_DEVICE_QI条件控制，默认不发现。使用现有正式Tests.exe、正常自包含依赖图，专用发布和Windows沙箱；未新增工程。独有产物根artifacts/du-device-qi/。未使用BL监督，因此没有可认证的进程ExitCode/parentTRX，不把工具正常返回冒称Exit0。

共3次运行，每次1/1 Passed，TRX Completed，均亲读26行TRX及14行JSON全文。第一次x86 cfc14ffd013e4dc0a45b1a4e20bcb5ee为初版诊断，testSHA210F49808AC045C3F05F78776CAB5F5D572F0E810128D4B5D44A4E68FBA68EAD/MVIDea83e815-1763-419e-9c3d-d78c041c305c；补充calli签名后最终两格如下。

|最终格|任务/PID|actual testSHA / MVID|
|---|---|---|
|x86|fa87756712334a10b37731fd76602d3e/5296|DB2C6A111DA3DFA2B0EA521CDE4E083FB9B1B71F24A6611BFF0DA8DC5F4A16DE / 4fa78531-fbf6-4451-b31d-953e17190053|
|x64|590f43bf923e45afb0a9a9ca4e28573e/2652|B731CEF5AC8AD4B934B41CDEC7CC11B59CC6480778D94C29BB6562B9B3FC09B3 / f87744d9-92d5-4874-a523-77c132870270|

两格.NET10.0.13/MSTest4.0.1，Silk实际2.23.0.0，SHA F08ACBB76DD7C60803F8F0D4D8212C77A85D3EF67B1FB42FBB18F3C15A8586E4。没有加载官方/候选wpfgfx原生DLL，故无新nativeSHA或官方来源认证，不能冒充DR四格。
绝对TRX分别为C:\CodingAgentSandbox\Tasks\fa87756712334a10b37731fd76602d3e\TestResults\du.trx及C:\CodingAgentSandbox\Tasks\590f43bf923e45afb0a9a9ca4e28573e\TestResults\du.trx。全任务目录回收至run-x86-final及run-x64，业务记录在Results/du-qi.jsonl。未逐项包清单审计/独立obj隔离，属于本次证据纪律缺口，不声称完成正式清洁验收包门。

## 5. 实证与最早未定分支
两架构CreateDevice/CreateRenderTarget/GetDevice均S_OK，GetDevice输出等于原device。合法IID b18b10ce-2649-405a-870f-95f777d4313a与库Guid实值相等。独立delegate* unmanaged[Stdcall]<nint,Guid*,void**,int>调用vtable[0]，然后生成绑定调用同一device，均返回E_NOINTERFACE(80004002)、输出0。owned Release回2，surface/device/factory最终各回0。
x86 device196555616，vtable196567740，slot01944207280，IID栈地址193849456；slot0位于apphelp.dll base1943666688，SHA A8109618C3BF3623BE23E8E18F1E7184538600C953169297FD0009ED77564C6E。x64 device2461183646560，vtable2461183664464，slot0140720678496112，IID地址286658120592；slot0位于系统d3d9.dll base140720678109184，SHA87E157E78536C66FC413ED202296E39053CC9E14DC8D8381F8A3D18F444A4C0D。apphelp出现是观测，不归因shim。
实际绑定IL末尾calli token=110002C1，ResolveSignature返回0103080F1182580F1180C90F0F01；首字节01为Cdecl元数据，参数数3、返回int32，存在显式this参数。此为精确生成签名证据，不是机器码/汇编。JIT独立调用正常，并不证明该Cdecl元数据在x86 NativeAOT同样安全，也不单独证明DT崩溃就是此原因。
当前最早未定分支已缩为：候选NativeAOT内同一surface.GetDevice输出在直接Stdcall QI与生成绑定间是否产生差异，及AOT实际约定适配。尚未进行候选阶段，不排除前序内存破坏/lifetime差异；没有坏ABI或无效指针实验。

## 6. 最终源码与未完成门
未修改CheckDevice或同因兄弟；没有绕过Ex检测、吞HRESULT、改变null/refs/释放或扩大分配。DU未引入任何未验收业务行为，无此类变更需要撤回。仅保留默认禁用的只读诊断测试，DT条件trace保留；空Dt文件未删，无悬空引用。CI/CX/CY fill及其他人的文件未改。
最后C#修改后最终双架构诊断发布/运行均成功；随后整个slnx默认Release构建成功，退出码0、0错误/3762警告（含既有CA1416/MSTEST0044），不是零警告。该构建不是NativeAOT发布或真实Create验收。
DU官方/候选双架构DR真实Create循环=0，生命周期必要回归=0，BW/CK=0；DT既有两次失败不能补作通过。本轮未达到用户要求的完整修复验收，根因未确证。Available未实际触发；full/HW消费/恢复均未完成。后续最小行动为在Interop独有条件诊断内采同device直接QI返回与绑定QI返回，绑定新AOT actualSHA并使用既有BL正式监督复跑DR四格原断言；在此之前不要应用推测性的生产约定修复。
