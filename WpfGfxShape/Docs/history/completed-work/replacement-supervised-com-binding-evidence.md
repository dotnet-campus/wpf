# EP：实际 COM 绑定只读证据

## EQ 独立核证结论（最新优先）
四文档更新后已全文小段回读至末：本报告62行、handoff474行、session-summary573行、issues590行；截断段缩小补读完成。本补记随后再读。EP证据审阅与交接闭环完成，生产问题仍开放。
本节补完 EP，不新增认证工程或运行。EQ 亲读规则27行、完整方案120行、EO60行、当前诊断137行及 Interop627行；截断段缩小补读。两架构原件各742行及摘要各54行已逐段读到 COMPLETE，不承袭 EP 未读内容。

### 原件、身份与不能冒充的运行结果
原始大 IL/standalone signature/生产 IL 调用偏移均保留在 [x86-final 原件](../../artifacts/ep-com-binding/run-x86-final/Results/ep-com-binding.txt)、[x64 原件](../../artifacts/ep-com-binding/run-x64/Results/ep-com-binding.txt)，精确方法清单见同目录 ep-summary.txt。两 Results 实际仅这两个 txt；不存在业务 JSON/BL 监督 JSON/运行响应/原测试 TRX/独立 manifest 可供本次回读。ep-com-binding 根也未找到 response/manifest；工作区 .coding-agent 为空。不能把随包 TrxReport DLL 当 TRX，不能从 COMPLETE 或源码 return 0 推出实际进程 Exit0；EP 历史“完成”仅降格为文件写至末。
实际记录 Windows10.0.26200，.NET10.0.13；x86 task00209cfb04094dbd85bb264616617866/X86/4，x64 task079d2a3399844447a15dea643e2d9076/X64/8。两包 runtimeconfig.json 各16行亲读，includedFrameworks Microsoft.NETCore.App10.0.13，与自包含 JIT 读取吻合；未记录实际 WPF/native 模块身份，不补造。
Silk.NET.Direct3D9 2.23.0.0 两架构实际 SHA F08ACBB76DD7C60803F8F0D4D8212C77A85D3EF67B1FB42FBB18F3C15A8586E4，MVID deec18f5-2a98-450b-93cd-9117248c6163。Tests x86 SHA24C026F90C27B80194E9038907F74DB31746D56E6B0AC05A94835DA3877B0444/MVID11fa3dc0-fab3-479e-af43-878a3415e024；x64 SHA3BEE05314527A90E3DF134F0F3228E6AE617BF9583B3472D3288CA929BC26386/MVID8471d1f4-3e3c-49fb-9f85-a3a32ecfe6f4。
Assembly.Load 得到的是托管 wpfgfx_cor3.dll，SHA BDF8C532A219DDCDD241FA3933D1236E82A340849C1D149842A2580B1CC5C387/MVIDf3a9f807-9ea9-49c0-913d-110fccce3d31；不是 NativeLibrary.Load 的 NativeAOT 候选。诊断只遍历/ResolveMethod/读方法体/读布局，不调用目标 COM 方法、生产 Create 或真实 consumer。因此本次已证两架构 JIT 反射读取，未证 x86 JIT COM 连续执行、NativeAOT 实际清栈、候选加载/消费、退出成功或生产验收；JIT 正常不外推 AOT 正确。

### 27 个实际绑定逐项对照
缩写：F=IDirect3D9，FX=IDirect3D9Ex，D=IDirect3DDevice9，DX=IDirect3DDevice9Ex，S=IDirect3DSurface9，T=IDirect3DTexture9；C=Create，U=GetUpdateMethod，K=CheckDevice，A=GetAdapterLuid，R=ReadSurface，P=PresentLocked。u=uint32，i=int32，n=native int；enum 均32位。参数列列出精确重载的全部业务参数，另显式添加对应接口 this*；N 为包含 this 的 standalone 参数总数。表中 token 为0600前缀后的低四位。
每一项实际 IL 均取 lpVtbl[slot]、按原次序压 this 与参数后 calli；QI slot0没有额外索引加法。全部27项两包 calli 字节一致、约定01=Cdecl、native HasThis=false；managed Standard,HasThis 是托管实例方法属性，不能代替 native this。全部 SDK 为非变参 STDMETHOD/ STDMETHOD_(ULONG) 的 STDMETHODCALLTYPE：Windows x86 Stdcall、显式 COM this；x64按统一Windows ABI。表中“C/S”逐项表示 IL Cdecl / SDK Stdcall，绝非已证物理清栈错误。HRESULT=i、Release ULONG=u；没有发现这27项的slot、显式参数数量/顺序或返回宽度与SDK不同。

|接口.精确方法（token）|生产调用|slot/N|业务参数；返回|IL/SDK|结构及认证边界|
|---|---|---|---|---|---|
|F.QueryInterface(0920)|A|0/3|Guid*,void**；i|C/S|GUID16；仅元数据|
|F.Release(0925)|A finally|2/1|无；u|C/S|接口指针4/8；仅元数据|
|FX.Release(094B)|A finally|2/1|无；u|C/S|同上|
|FX.GetAdapterLUID(0975)|A|21/3|u,Luid*；i|C/S|LUID8合同；未核实际消费|
|D.QueryInterface(0A6F)|U、K|0/3|Guid*,void**；i|C/S|GUID16；EN负QI历史非本轮运行|
|D.Release(0A74)|C/R/P finally|2/1|无；u|C/S|引用释放执行未由EP覆盖|
|D.TestCooperativeLevel(0A75)|K|3/1|无；i|C/S|仅元数据；历史EN Hr0|
|D.GetDirect3D(0A78)|A|6/2|F**；i|C/S|输出指针4/8；仅元数据|
|D.GetDeviceCaps(0A7A)|U|7/2|Caps9*；i|C/S|304；Caps2@12/DevCaps2@212|
|D.GetCreationParameters(0A7E)|C、A|9/2|DeviceCreationParameters*；i|C/S|16/24；EL单点因果有限背景|
|D.CreateTexture(0A9E)|P|23/9|u,u,u,u,Format,Pool,T**,void**；i|C/S|HANDLE*对应void**；无EP执行|
|D.CreateRenderTarget(0AB2)|R、P|28/9|u,u,Format,MultisampleType,u,Bool32,S**,void**；i|C/S|BOOL32；无EP执行|
|D.GetRenderTargetData(0ACE)|R|32/3|S*,S*；i|C/S|接口指针4/8；无EP执行|
|D.StretchRect(0AD4)|R、P|34/6|S*,Box2D<int>*,S*,Box2D<int>*,Texturefiltertype；i|C/S|SDK const RECT*，16合同；const不影响ABI|
|D.ColorFill(0AE4)|P|35/4|S*,Box2D<int>*,u；i|C/S|SDK const RECT*/D3DCOLOR；无EP执行|
|D.CreateOffscreenPlainSurface(0AE8)|R|36/7|u,u,Format,Pool,S**,void**；i|C/S|HANDLE*；无EP执行|
|DX.Release(0BF5)|U、K成功支|2/1|无；u|C/S|EN负QI跳过，不造执行|
|DX.CheckDeviceState(0D39)|K成功支|128/2|n；i|C/S|SDK HWND；EN未覆盖|
|S.Release(0E78)|C/R finally|2/1|无；u|C/S|无EP执行|
|S.GetDevice(0E79)|C、R、P|3/2|D**；i|C/S|接口输出4/8；历史EN Hr0|
|S.GetDesc(0E91)|C|12/2|SurfaceDesc*；i|C/S|32；历史EN Hr0不证全部字段|
|S.LockRect(0E93)|R|13/4|LockedRect*,Box2D<int>*,u；i|C/S|8/16，SDK const RECT*；无EP执行|
|S.UnlockRect(0E97)|R及补偿finally|14/1|无；i|C/S|无EP执行|
|S.GetDC(0E98)|U旧OS支|15/2|n*；i|C/S|SDK HDC*；EN无进入证据|
|S.ReleaseDC(0E9A)|U旧OS支|16/2|n；i|C/S|SDK HDC；EN无进入证据|
|T.Release(0EFA)|P失败支|2/1|无；u|C/S|无EP执行|
|T.GetSurfaceLevel(0F17)|P|18/3|u,S**；i|C/S|无EP执行|

SDK实际亲读来源 C:/Program Files (x86)/Windows Kits/10/Include/10.0.26100.0/shared/d3d9.h：F339–357、D432–470、T1178–1198、S1625–1643、FX2041–2064、DX2267–2276；basetyps.h28明确 STDMETHODCALLTYPE=__stdcall。创建链入口 resource QI 是源码直接 Stdcall slot0/Guid*/void**/HRESULT，已是正确合同，不计入27个Silk方法。U/K的同一QI精确重载计一个方法但有两调用点；不能把相邻接口 token 当同一个认证。
实际结构打印不是新造布局试验：SurfaceDesc32，字段0/4/8/12/16/20/24/28；Creation x86 16/0,4,8,12，x64 24/0,4,8,16；Caps304/Caps2@12/DevCaps2@212。与亲读 d3d9types.h1495–1501、1734–1745字段次序相符；Caps结构本轮只核实际打印与已知304，不声称重新独立认证全部SDK嵌套类型。LockedRect8/16、PresentParameters56/64是已打印的辅助布局，不当输出内容/哨兵通过。
夹具仅审 DrInteropAbiTests.cs 创建链：factory slot16 Stdcall，显式factory,u,u,HWND,u,Parameters*,device*，HRESULT；device slot28 Stdcall，显式device,u,u,format,multisample,u,intBOOL,surface*,nint，末项传0与SDK HANDLE*空指针等宽；slot35 device,surface,RECT空指针,D3DCOLOR，HRESULT；Release slot2返回uint。Parameters自然布局独立声明56/64、HWND@28/32，未修改。nint末参是此调用的空指针载体，不把它泛认作任意非空HANDLE*声明认证；未泛扫全仓device/surface声明。

### 因果裁决、当前源码与唯一下一动作
EO/EL/EN属于既有历史证据，本轮未新增运行，不能重认证其退出计数。EL同DLL仅slot9在其余direct背景切换的外部因果仍有效；EN仅slot9+两device QI的组合失败，故三处不足。27项Cdecl一致是可定位的元数据差异，不是首坏指令、栈破坏或整个Silk库bug证明；不同JIT/AOT降低与平台ABI仍需实际证据。第二QI负Hr/null时DX Release不执行；QI.after整行仍未证明TraceCreate尾部Dispose/Flush已返，OS判断、函数返回/caller接收、外层日志亦未分开。没有故障地址/访问类型/native栈/匹配符号，不选定唯一因果。
唯一下一可执行任务（另行取得运行授权后，不在EQ执行）：将本类“首Create进入至GetUpdateMethod返回以及其失败cleanup”限定为一个局部、私有、精确Stdcall COM适配边界，只覆盖本表10种重载：S.GetDesc/GetDevice/Release，D.QueryInterface/TestCooperativeLevel/GetCreationParameters/GetDeviceCaps/Release，DX.CheckDeviceState/Release；保留入口已direct QI，保留原this/结构/输出/引用/失败分支，不全替27项、不扩库级包装、不跳cleanup。对比EN的新增变量明确仅剩四种前序generated调用 GetDesc/GetDevice/TestCooperativeLevel/GetDeviceCaps及该边界必要cleanup/成功Ex分支，从实际表而非猜测选边界；失败可判别“该创建窄链闭包仍不足”，成功只支持这个边界的修复候选，不能定位其中唯一方法或直接宣布生产验收。后置GetAdapterLuid及R/P仍保持generated，若新的失败推进到它们只记录边界，不扩替代范围。
保留生产前的门：正常分析器构建；官方/候选同版x86/x64 Release实际DR循环及精确TRX/身份/refs；再受影响生命周期/BW/CK和真实consumer验证，失败精确撤回本轮适配且保留CF/CX/CY。不以再跑full代根因、不复刻EN三处试验。本次只交此一个后续任务，不实施或发布。
EP_COM_BINDING_READONLY由诊断文件首尾#if包住；默认项目/本地props/targets不定义它，唯一启用在ignored artifacts/ep-com-binding/Diagnostic.props同时指定Tests StartupObject。Code检索无该符号，默认入口/业务/生产未改。EQ源码/项目修改0、发布/沙箱/新AOT试验0；原trace、EF、CF/CX/CY保持，default未修。未动DR/DS、NativeRun/ExitDiagnostics、VisualCache historical/捕获/WindowQ/打包。Available/HW消费/恢复/full最终四格均开，x64 Event0独立。
EQ实际专用默认Release方案build一次，只有configuration=Release，无NoWarn或关闭分析器：Exit0、2182警告/0错误、6.09秒。普通增量，未clean/rebuild；不能保证所有项目/分析器全量重跑。日志62行已读；stdout1862323字符仅保留末20000，前1842323不可见，具体项目命中细目不补造。正常build不替NativeAOT运行或生产验收。


## EP-2 只读实际运行阶段
显式 EP_COM_BINDING_READONLY Tests 入口沿生产 InteropDeviceBitmapExports 及嵌套类型的 IL token 自动解析精确 Silk 重载。JIT 自包含 x86 .NET10.0.13；首次泛型签名0x15未支持失败，补解码后两次完成，末次另存摘要。发布3次均成功，正常分析器未压制，输出有截断。最终 task00209cfb04094dbd85bb264616617866，证据 artifacts/ep-com-binding/run-x86-final/Results。无 NativeAOT/DR运行、生产修改或低干扰条件设施；正在核读，不称完整认证。

## EP-1 进行中（历史）
已读取执行规则和 EO/EN/EL 最新结论；正在补读完整方案及被截断尾段，再检查现有 DU/EK Tests 只读机制。优先同 SHA Silk.NET.Direct3D9 2.23.0 实际方法体、standalone calli 签名及生产重载对应；不改默认 ABI/业务，不以静态疑点当修复。尚无代码修改、构建、发布或沙箱任务。仅有区分力假设才考虑最多一次低干扰 AOT 诊断；当前未决定运行。
