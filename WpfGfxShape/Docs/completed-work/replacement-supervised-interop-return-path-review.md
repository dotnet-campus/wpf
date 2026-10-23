# EO：GetUpdateMethod 返回区间独立审计

## EO-1 进行中（历史）
先亲读规则、完整方案、EL、EN原件和当前实现。静态审计优先，不把after缺失当函数未返，不重跑三处组合或全direct。

## EO-2 EN原件亲核
根 artifacts/en-interop-com-fix/run-candidate-x86。监督39行、业务6行、stdout2行、stderr1行、DT trace19行、aot-identity1行均读至末。精确 Results/en-candidate-x86/TestResults/child.trx读取不存在，不造计数；父监督非MTP。
task aaf1acf656f9493a852c30f85ec90f30，父8712/子5652；.NET10.0.13/X86/4；Exit -1073741819/C0000005，无timeout/cancel/kill，SupervisorError=null。actualNative为该task/Native/wpfgfx_cor3.dll，SHA BF98CB728DEF274674769CC84B36F21099A186DDAEB990F1BC1EC1479DC775C9；testSHA EAB5BEFA56FD62BEC368685F4BFFF64FFA824181C683DF26DD7184809B7ED712，MVID c43b0534-64a2-423d-a541-3d37c4f926a9；WPF10.0.0.0。业务identity与aot-identity一致。失败包不绑定为默认源码产物。
两device QI.after均E_NOINTERFACE(-2147467262)/null；GetDesc/GetDevice/TestCooperativeLevel/GetCreationParameters/GetDeviceCaps均Hr0。CreateDevice/RenderTarget/ColorFill Hr0；首次Create无after。stderr只有Access Violation终止，无本次故障地址/栈/NRE。generated trace标签保留，不据标签误判EN实际QI方式。

## EO-3 逐句返回区间
当前Interop246–282及调用者206–214：
1. caps为地址传出的Caps9局部。第二QI发生意味着capsResult非负且DevCaps2位0x10通过，不证明caps其余值正确。
2. extended初始null；Device9Ex.Guid在QI.before前取值。GUID16字节；DZ实际IID b18b10ce-2649-405a-870f-95f777d4313a。QI.after后无GUID操作。
3. QI.after参数只读取queryResult和extended转整数，无解引用；但该次TraceCreate写出JSON后还要writer.WriteLine/Flush、stream.Flush(true)、各Dispose。完整行不是已证明TraceCreate返回，不能排除日志尾声。
4. if(queryResult>=0)在EN为假；extended.Release及其trace、caps.Caps2高位判断全跳过。本次不是成功/null导致Release，也不是extended双释放。成功支的违规COM输出风险不作本次根因。
5. 必经OperatingSystem.IsWindowsVersionAtLeast(6)。这是负QI后唯一必要方法调用。EN没有独立OS版本字段，不拿.NET版本证明该分支；现代Windows通常跳旧OS支，但本次无分段证据。
6. 仅OS<6才dc=0、GetDC.before、surface.GetDC(&dc)、after；成功则ReleaseDC.before/调用/after，成功return BitBlt。EN无GetDC.before，不能称已执行这些COM，也不能仅无日志排除before记录内部故障。
7. 最后return Software=0；enum底层int32，SharedSurface=1、BitBlt=2。无对象分配/装箱、无finally、无IDisposable局部清理；负QI未拥有Ex引用。caps/GUID为非托管局部。物理栈偏移、内联、尾声未取得AOT指令，不能由源码推定。
8. caller接收enum；外层after参数仅常量字符串、device转nint、method转int，无device解引用、无parameters字段读取。但日志内部仍有路径/目录/FileStream/StreamWriter/Utf8JsonWriter分配、I/O及Dispose，不能当无故障探针。故缺after兼容callee未返、caller已接收但日志失败、前序破坏延迟显现，不选定其一。
9. description多采样检查、parameters.AdapterOrdinal、BitmapState构造/GetAdapterLuid均在外层TraceCreate之后，不是其参数求值。Create finally的Free(instance)/Free(handle)/device.Release/surface.Release仅在离开try时执行，不是GetUpdateMethod必经finally；日志异常可触发但本次无栈证实。不得跳cleanup。

## EO-4 generated COM全清单与认证边界
slot为COM合同及既有DX/DZ切片，非全数generated IL认证。I为原件IL/calli亲核；缺为没有该方法实际方法体/calli，不能套用相邻方法或公开API。HRESULT=int32，ULONG=uint32，接口输出指针4/8。EN三处direct是历史诊断背景，当前默认均generated。

|方法/位置|slot；显式参数不含this|结构/EN状态|实际IL/calli|
|---|---|---|---|
|surface.GetDesc/Create|12；SurfaceDesc*|32；Hr0|缺|
|surface.GetDevice/Create、ReadSurface、Present|3；Device9**|4/8；Create Hr0|缺|
|device.QI/CheckDevice及GetUpdateMethod|0；Guid*,void**|16及4/8；EN direct负Hr/null|I：DU签名0103080F1182580F1180C90F0F01|
|device.TestCooperativeLevel|3；无|Hr0|缺|
|deviceEx.CheckDeviceState|128；HWND|未证进入|缺|
|device.GetCreationParameters/Create及GetAdapterLuid|9；DeviceCreationParameters*|16/24；EN Create direct Hr0|I：EK签名0102080F1182580F1181A4|
|device.GetDeviceCaps|7；Caps9*|304；Hr0|缺|
|deviceEx.Release/两成功支|2；无|ULONG；EN均不执行|缺|
|surface.GetDC、ReleaseDC/旧OS|15 HDC*；16 HDC|4/8；无before|缺|
|device.GetDirect3D/GetAdapterLuid|6；Direct3D9**|4/8；无记录|缺|
|factory.QI/GetAdapterLuid|0；Guid*,void**|16及4/8；无记录|缺，不能套Device9 token|
|factoryEx.GetAdapterLUID|21；uint,Luid*|LUID8；无记录|缺|
|device/surface/factory/factoryEx.Release|2；无|ULONG；各finally|缺；不混同已有Direct3D9Factory.Release|
|device.CreateRenderTarget/ReadSurface、Present|28；uint,uint,Format,MultisampleType,uint,Bool32,Surface**,void**|返回HRESULT；不在返回窄区间|缺|
|device.StretchRect/ReadSurface、Present|34；Surface*,RECT*,Surface*,RECT*,filter|RECT16|缺|
|device.CreateOffscreenPlainSurface|36；uint,uint,Format,Pool,Surface**,void**|HRESULT|缺|
|device.GetRenderTargetData|32；Surface*,Surface*|HRESULT|缺|
|surface.LockRect|13；LockedRect*,RECT*,uint|LockedRect合同8/16，非本轮实测|缺|
|surface.UnlockRect|14；无|HRESULT；含finally补偿|缺|
|device.CreateTexture/Present|23；uint,uint,uint,uint,Format,Pool,Texture**,void**|HRESULT|缺|
|texture.GetSurfaceLevel|18；uint,Surface**|HRESULT|缺|
|texture.Release|2；无|ULONG|缺|
|device.ColorFill/Present|35；Surface*,RECT*,uint|HRESULT|缺|

DU实际QI IL：0228A300002B28A400002B0A067BE30700044D0B0603040729C10200112A。EK pointer slot9 IL：0228A300002B28A400002B0A067BE30700041F09D3FE1C3C00001B5A584D0B06030729D30200112A。EK x86 ek-abi.json297行全文核，pointer/ref均同calli。Silk2.23.0 SHA F08ACBB76DD7C60803F8F0D4D8212C77A85D3EF67B1FB42FBB18F3C15A8586E4/MVID deec18f5-2a98-450b-93cd-9117248c6163。签名首01=Cdecl，与Windows x86 COM Stdcall合同不同；显式this，managed Standard,HasThis非native HasThis。元数据不是AOT最终清栈因果，不称Silk全库bug。缺失表项的实际IL认证仍未完成。
EN实测SurfaceDesc32/Caps9304/Creation16。EK独立/实际Creation x86 offsets0/4/8/12，x64首60行核24及0/4/8/16，本轮未重读其余x64 IL。Guid16/LUID8/RECT16为合同。EN没记录parameters字段/caps原值，Hr0不能认证输出。DZ历史direct slot9 bytes00000000010000000C00010026000000解为Adapter0/Type1/HWND0x1000C/flags0x26，但不是EN同任务合法device/window/flags独立对照，不可移绑。没有EN实际字段足以证明明显非法，也不能称正确。

## EO-5 实际状态和下一最小定位
五文档全文分段回读至末已完成：更新前本报告59行、EN28行、handoff463行、session-summary568行、issues585行。handoff386–390/423–425、summary32–35超长截断已缩小补读；完成后本补记及四份状态文字再次读取。EN历史回读待办闭环，文档完成不冒充生产修复或完整替换。
执行规则27行、完整方案120行、EL46行、当前Interop627行分段亲读，源码截断尾205–228补读。没有发现EN负QI后必然空解引用、错误enum宽度或重复Ex释放的精确静态根因。前序栈/输出破坏、OS判断、返回和日志尾声尚不能区分。当前读取/API工具不返回任意DLL方法体或本次故障地址/栈；现有EK只读程序仅提取slot9。没有已验证无混杂标记方案，不为猜测消耗唯一条件诊断。
本轮新发布/包/沙箱task/条件诊断均0，生产/测试/项目源码修改0、条件行为新增0/撤回0。EN三处已撤回，当前默认generated不变；原trace、EF、CF/CX/CY未触碰。不改DR/DS、NativeRun/ExitDiagnostics、VisualCache/historical/捕获/WindowQ/打包。Code实验标记文本检索只命中无关PEEK_MESSAGE枚举，Interop无残留发现。无终端/脚本/Host/机器码。
默认正常分析器Release方案build一次Exit0，0警告/0错误，0.94秒；19行日志全文核，三工程输出在列。普通增量，不称所有分析器重跑、AOT发布或双架构验收。
下一最小定位：现有Tests纯C#只读提取同SHA上表缺失方法实际IL/calli/slot，优先GetDeviceCaps/GetDesc/GetDevice/TestCooperativeLevel/Release，不套QI元数据、不泛替全部。若后续取得可运行低混杂标记方案及新诊断授权，再分第二QI日志真正退出、OS判断前后、Software return前、caller接收且外层TraceCreate前、日志返回后；EN三处仅诊断背景，Ex/refs/输出不变，结束精确撤回。还缺故障地址、访问类型、native栈及匹配AOT符号才能推进到首坏指令/写；不造根因。EL同DLL因果限其余direct背景，EN三处不足。Available/HW消费/device-loss/resize恢复/最终四格未闭，x64 Event0独立。
