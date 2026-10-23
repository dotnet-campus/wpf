# FE：有限生产 COM 修复组合评审与执行

## FG 最终回读闭环补记
四份既有文档历史正文已全文小段读至末，截断末行/余段已缩小补齐；FG新增终态也已分段回读（补记前本报告37/handoff626/session-summary656/issues673行）。本补记写后再次读取；历史未删除，无重复新报告。预检43/26行、正式监督39/业务6及双流/actual、源码627行及snapshot范围独立核证已闭；FF包manifest尾385–418已补读，未冒称全部逐文件hash独立重算或字节恢复认证。仅文档交付完成，默认安全但未修；下一唯一同正式包/同日志布局调用与配置差异审计，必要时转C#监督WER/小dump可行性审查，本轮均不实施。无新实验/build/发布/沙箱/full，开放门不变。

## FG 最终独立核证：预检边界已厘清，正式失败，默认安全但未修（最新优先）
FG仅专用读取/搜索及本报告、handoff、session-summary、issues四文档更新；无终端/直接间接脚本、新实验、发布、沙箱/full、生产测试或入口/打包修改。先登记handoff进行中，再独立读规则/完整方案及原件；下列进行中均为保留历史，由本节覆盖。
预检原件：artifacts/fe-interop-closure/run-preflight-x86/TestResults/fe-preflight.trx全文43行，task166181b0048744328deb69ddb8e63de2，total/executed=1、passed=0、failed=1、Exit2；原断言为目录SequenceEqual与Native hash联合条件，不能单凭失败判hash错误。artifacts/ff-interop-acceptance/run-preflight-x86/TestResults/ff-preflight.trx全文26行，task8481231bd53f4852bf42e916e93df676，Completed/1执行1通过0失败。两aot identity及FF upload-preflight.json身份/末段亲核：MissingInputs=[]、UnexpectedInputs=[]、ExecutionOutputs仅Results/aot-identity.json、ExactDirectoryMatches=false、NativeMatches=true。初始化运行输出不属于上传manifest，不能误称输入污染；记录逐输入Bytes/SHA且保留nonempty/去重/missing/unexpected/Native期望及固定hash断言，不删hash追绿。FeInteropPackageTests.cs全60行与FF snapshot、专属Build.props已核，整个类型仅FF_INTEROP_PREFLIGHT编译，默认props/csproj不启用，历史业务/发现不变；预检文件身份不能替代业务实际加载身份。
正式原件根artifacts/ff-interop-acceptance/run-dr-x86/Results：ff-dr-x86/supervision.json39行、dr-candidate.jsonl6行、stages.jsonl6行、stdout2行、stderr1行和aot-identity单行亲读。taskd71a90439db344ceb58e114fa643312d父3388/子5348，filtered/120秒，Reason=exited、Exit=-1073741819/0xC0000005，非timeout/cancel/kill，OutputCollected=true/DrainTimedOut=false/SupervisorError=null。监督实际filter、report-trx/child.trx与绝对results-directory均核；精确远端TRX应为C:\CodingAgentSandbox\Tasks\d71a90439db344ceb58e114fa643312d\Results\ff-dr-x86\TestResults\child.trx，对应本地run-dr-x86/Results/ff-dr-x86/TestResults/child.trx直接读不存在，无完成TRX计数。
actual identity是业务第1行：GetModuleFileName取得C:\CodingAgentSandbox\Tasks\d71a90439db344ceb58e114fa643312d\Native\wpfgfx_cor3.dll；actualNativeSHA=1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53；TestSHA=BD15F34C33B00DE64E3759835ED403663228EDF90E0CBF589062094610ABCF5E，MVID=983d0bd1-9777-4755-97fd-c0822ce0dc13，.NET10.0.13/X86/pointer4。CreateDevice/CreateRenderTarget/ColorFill Hr0，业务最后首次Create before（Iteration0），没有返回，不能造三轮/refs0。此次无内部DT/后置trace证据、无故障地址/native栈，故没有可认证的最早内部边界；不能移绑旧CheckDevice.QI或GetUpdateMethod/Adapter日志定位本次。stderr AV通用文字不是首次坏写证明。
源码核证：当前InteropDeviceBitmapExports.cs627行分段亲读至末，Code/Abi指定GetSurfaceDevice/QueryDevice/ReleaseDeviceEx/FE与FF模式无专属helper残留（只命中原generated GetCreationParameters调用）；FE前序及ReadSurface/Present共享适配均已撤，原generated、surface slot0 directQI、DT/DX默认关闭trace、自身IUnknown/Present AddRef及原Direct3D9Factory.Release保留。EF DR char* GetModuleFileName及独立声明保留；CF fillPrimitive/curved链、CX/CY fillLocalFloat默认false/Group传递/NativeShapeReader整数消费链只读核在，本轮未改。FF source/fe-production-experiment.cs.txt确存在640行（前序、helper、GetAdapterLuid、ReadSurface/释放和尾部亲读），ff-preflight.cs.txt60行已全文读；存在/范围不等于逐字节历史恢复认证，无独立hash工具未造当前源码hash。FF默认build Exit0/3762警告/0错误仅作历史交接引用，FG未重建、未新认证日志或AOT/运行通过。
与旧成功控制只列已见源码差异：FE前序direct经私有GetSurfaceDevice/GetCreationParameters/QueryDevice/ReleaseDeviceEx复用，Create/ReadSurface/Present cleanup用Direct3D9Factory.Release；EV/EX控制前序条件内联/runtime分支，Create cleanup内联slot2，后置另有EvTrace/ExTrace及标记/条件布局。FE后置D.GetDirect3D6/F.QI0/F.Release2仍generated，与EV-generated/EX-direct成功的实际后三绑定一致；FE Ex LUID21/Release2仍generated，而EV/EX条件实现为direct，非Ex旧控制未执行这些支，不能当已证差异因果。BitmapState构造→GetAdapterLuid→GCHandle/Instance分配→Register→输出/cleanup保留但此次无内部实跑证据。FE失败与历史全direct成功不是同DLL/同布局单变量控制，不增写回归判断、全Silk库bug或首次坏写结论。
唯一下一可执行任务（本轮仅建议、不实施）：在同一正式执行包/同一日志布局，先审FE失败与EV/EX成功控制的完整真实调用清单及编译配置（Native与测试常量、Release/RID/AOT/优化/分析器、输出地址、helper/内联、trace/cleanup），按实际非Ex/Ex区分。逐项列尚未一致的Create后置slot6/QI0/Release2/第二slot9/LUID21/Ex Release2及BitmapState构造、GCHandle/Instance、Register/失败cleanup/共享释放绑定，优先辨明已一致后三与仍缺证生命周期，不再猜扩大COM闭包。若多轮差异控制仍不足且缺故障地址/native栈，转为既有正式C#监督角色内安全启用Windows错误报告/小dump的可行性审查：评估WER LocalDumps的权限/进程作用域/恢复配置/空间与敏感数据边界，或纯C# P/Invoke MiniDumpWriteDump的句柄权限、崩溃上下文获取、架构匹配及线程安全；不把进程退出后无上下文dump当故障栈。当前未提供现成dump专用能力，不假装可直接采集，不脚本/外部命令/新增Host，不无止境变体。
x64 Event0/E_HANDLE独立问题、Available真实native通知/consumer、HW内容及resize/loss恢复、media/resources/线程callback与最终两个项目×双架构Release四格full继续开放。FE组合关键门失败，默认安全恢复不是生产修复；文档闭环不等完整替换完成。

## FG 独立核证进度（过程保留）
已亲读原预检 fe-preflight.trx 43行（task166181b0048744328deb69ddb8e63de2，执行1/通过0/失败1/Exit2）及 ff-preflight.trx 26行（task8481231bd53f4852bf42e916e93df676，执行1/通过1/失败0）；两精确路径分别为 artifacts/fe-interop-closure/run-preflight-x86/TestResults/fe-preflight.trx 与 artifacts/ff-interop-acceptance/run-preflight-x86/TestResults/ff-preflight.trx。不能因目录枚举未显示TestResults而判缺失。FF upload-preflight.json末段已核：MissingInputs/UnexpectedInputs空、ExecutionOutputs仅Results/aot-identity.json、ExactDirectoryMatches=false、NativeMatches=true；这是输入与运行输出边界，不是输入污染或Native hash失败。
正式taskd71a90439db344ceb58e114fa643312d监督39/业务6/双流/actual及aot identity已核，精确child.trx直读不存在；当前Interop627行已分段亲读，FE前序/共享适配及私有helper无残留，源码本轮未改。四文档历史全文回读与终态汇总仍在完成，不先称交付闭环。

## FF 关键门失败，安全撤回进行中（最新优先）
预检原 task166181b0048744328deb69ddb8e63de2/TRX1执行0通过1失败/Exit2，联合断言混淆运行目录与输入清单：初始化已生成 Results/aot-identity.json，输入 manifest 不含此运行输出。FF 分项诊断 task8481231bd53f4852bf42e916e93df676 正式 exe 1/1 Passed，无 missing/unexpected，ExactDirectoryMatches=false、NativeMatches=true；Native 保持1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53。诊断仅 FF_INTEROP_PREFLIGHT 编译，正常 nonempty/hash 断言保留，未改历史测试。
正式 DR 候选 taskd71a90439db344ceb58e114fa643312d：supervision Exit0xC0000005、非超时/取消/kill、双流回收完成；业务6行止于首次 Create before，未返回，不造三轮/refs0/TRX计数。actual路径由业务GetModuleFileName确认，实际SHA同上述，TestSHA BD15F34C33B00DE64E3759835ED403663228EDF90E0CBF589062094610ABCF5E/MVID983d0bd1-9777-4755-97fd-c0822ce0dc13/.NET10.0.13/X86/4。监督三参数/绝对task TestResults路径已核，完整任务目录已回收。关键门未过，停止x64及扩大闭包；保存完整源码并精确恢复默认generated中，非运行验收通过。

## FF 接续核证进行中（历史）
FF 已接管，先核实际 preflight TRX、业务输出和正式白名单输入，再决定有限正式验收或精确撤回 FE 生产变更。当前尚无 FF 新运行，FE 默认适配仍在，不能称已验收；授权在本报告续记，不新增重复报告。上传输入身份与执行新增 TestResults 分开核验，不以 manifest 代替实际业务加载 SHA。

## FE 评审：有限合同闭包
EP 的27项表显示 generated calli=Cdecl 与 SDK x86 Stdcall 不同，但不证明每项物理清栈错误。EL Creation、EZ CheckDevice QI、FC Update QI 因果仅各自固定背景成立；EN三处不足，ER十前序仍失败，EX前序十direct加后置generated成功，EV后置三generated也成功。本轮选前序十种重载及其本类共享生命周期调用，不宣称全库bug：S.GetDesc12(SurfaceDesc*)→HRESULT、S.GetDevice3(Device9**)→HRESULT、S.Release2→ULONG；D.QI0(Guid*,void**)→HRESULT、D.TestCooperativeLevel3→HRESULT、D.GetCreationParameters9(DeviceCreationParameters*)→HRESULT、D.GetDeviceCaps7(Caps9*)→HRESULT、D.Release2→ULONG；DX.CheckDeviceState128(HWND)→HRESULT、DX.Release2→ULONG。全部显式this/Stdcall，原栈输出保留。
两deviceQI/Creation以外均无独立单点因果，按同合同统一并以组合门决定保留。GetDevice和device/surface释放复用于ReadSurface/Present，Creation复用于GetAdapterLuid，CheckDevice复用于Present，必须同步。Detach/Dispose/HardwareTexture/通知/Present自引用已有Direct3D9Factory.Release→private slot2 Stdcall，保持。IUnknown自身表与Present AddRef已Stdcall；AddDirty无COM调用。
后置D.GetDirect3D6、F.QI0、F.Release2及FX.LUID21/Release2保持generated，前三有EV/EX正证据，Ex后三未执行不造覆盖；现代Windows不执行S.GetDC15/ReleaseDC16。其余Texture/CreateRenderTarget/transfer/LockRect等不在无channel/Present的DR实际消费闭包，本轮不全direct。优先实际组合执行，不再单点实验。

## FE 发布阶段
已实际完成正常分析器 x86 NativeAOT（独有obj/bin/publish/native-x86）及正式DR self-contained .NET/WindowsDesktop10.0.13发布；Native SHA由正式包GetFileHash生成1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53。正式包418行manifest已读至392发生截断，仍须补尾，不上传PublishDir。新增专属FeInteropPackageTests用于清单/实际hash核验；本地专用测试入口失败：Microsoft.Testing.Platform MSBuild targets263不再支持.NET10的VSTest target，本次没有执行任何测试。不将该设施失败算业务失败。DR因加入专属核验再发布一次，Native未重发。无分析器压制；发布输出截断，不称全部警告已读。

## FE 进行中
正在核对执行规则、完整方案、FD/EL/EZ/EX 与 ER/ET/EV 边界和当前源码。尚未修改生产、发布或执行新任务，不将历史证据冒称本轮运行。

范围仅 InteropDeviceBitmapExports 的真实创建、更新、Detach、释放消费闭包；不全库或全 Silk 替换，不改变能力、HRESULT、引用、分配、输出结构与位置；保留 CF/CX/CY 和 EF。正常分析器启用。先 x86 正式依赖图独有 NativeAOT/正式 DR self-contained 10.0.13 与正式白名单包，验明实际身份、三轮业务/TRX/Exit/refs 后再 x64 与必要回归。关键有限验收失败最多一次有信息修正，否则精确撤回本轮生产行为。

最终四格、HW loss/media/resources 通知、真实 consumer、Available0/native callback、未执行 Ex 分支和独立 x64 Event0 均未关闭。
