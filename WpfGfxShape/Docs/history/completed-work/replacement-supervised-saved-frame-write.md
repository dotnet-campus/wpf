# GL 已认证 callee 保存 EBP 槽写入定位

## GR独立终态：已证写访问；factory QI输出别名F的窄链，ABI机制未证
仅离线审计/文档，无新task、运行、构建、发布、提取、源码或生产修改。GQ task8988e80364b343a0945fb41c863a9040原件亲读，不以监督转述代证据。先登记handoff进行中，规则27行及完整方案120行分段读取；以下GQ/GO/GN/GM/GL历史保留，不代表GR新增运行。

### 独立身份、写访问及退出核证
亲读hardware.jsonl全36行（超长段缩小补读）、file-identity全6行、context-av-1/2各全122行、cleanup全4行、dr-candidate全6行、stages全6行、preflight及aot-identity；native-events仅1–3、625–636、710–728、745–757关键范围，未称757行全文。没有独立名为writecontext/actualidentity的文件；实际写context是hardware第24行，actual identity取dr-candidate与aot-identity及实际module-loaded，不补造文件。
监督7436/子5892/TID2432，.NET10.0.13/X86/指针4；testSHA765F48E74C67A908461358F74C5AF6B319A76D43AFF9445219A767116CF0DA83、MVID7a8e00dd-0e4f-4cba-9f1e-406c2a01c6e9。候选实际任务Native/wpfgfx_cor3.dll、base6D390000/Size4055040、SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53。LOAD_DLL文件句柄同步读3995648字节；Volume/Index/Size/Write前后相同、pointer0恢复、handle-sha匹配。系统模块实际路径C:/Windows/SysWOW64/d3d9.dll、base6E7C0000/1853620224、Size1548288、SHA F4757445D75D619BC5695294EE5CA6BFA0AD8763C6604A8E884739F605A6DF28；不是把候选SHA套给系统模块。此为读取现有身份记录，未重新计算文件SHA。
P12042404/S12042252/F12042244=S-8；入口EBP=ESP=F，[F,F+8)=A4C0B700F0514C6D。slot0 write4 DR0=F、DR1=6D4C569E、DR7=000D0005，设置及读回一致。事件1 DR6=FFFF0FF1低四位1，指向数据slot0，不是slot1；同TID，[F]P→0，return6D4C51F0不变。事件2 DR6=FFFF0FF2低四位2是尾exec，EBP=ESP=F、槽仍0；随后DR0/1/2/3/6/7全0读回成功。不能把尾exec算第二次写，不能排除其它线程或更早同值写。
写后EIP6E874394=B4394。raw读取VA1854358388=6E874374、请求/实读64、成功，范围[6E874374,6E8743B4)，EIP在偏移20(hex)，前3字节[6E874391,6E874394)=832300。其AND DWORD PTR[EBX],0解释与EBX=F、写零、紧邻停机点一致，支持最窄候选；没有可靠系统函数起点/实际入口目标/前向完整解码认证，不能将6E874394叫写指令，亦不把反向三字节当边界认证或系统bug。当前窗口可候选解8B5D10为MOV EBX,[EBP+10]，但该指令边界仍属窗口解释。
context-av-1实际1351F0 EBP0/ESP=S，读取FFFFFF68；第二1959121/RVA1DE4D1读FFFFFF7C并second-chance。三次Continue NOT_HANDLED成功。子C0000005、监督Exit1、TimedOut=false、CleanupConfirmed=true；cleanup detach/wait/两句柄关闭成功。工具执行完成不是产品通过，无完成TRX不造计数；context原字段FirstBadWriteAuthenticated/StackBalanceAuthenticated=false未改。

### 同SHA完整384字节和COM合同（所有RVA/相对偏移为hex）
亲读artifacts/gm-saved-write/readonly-result/result/fv-pe.txt全37行：输入同候选SHA/I386/PE32，.text raw400/RVA1000，[135570,1356F0)384字节、file offset134970、slice SHA F6EF760CA4ADA31AAE1C4E4F33F010A70528FB3C319396FD6735CB551A4356F0；24个16字节hex段连续无缺。此为现有提取原件的身份链，未运行提取或独立重算slice SHA。由实际认证CALL1351EB E880030000→135570，55/8BEC建立F，前向核到13569F RET；窗口含后续清理/另一序言，不冒称完整函数范围认证。
独立亲读SDK10.0.26100.0 shared/d3d9.h的IDirect3DDevice9、IDirect3D9、IDirect3D9Ex相关声明，basetyps.h的STDMETHODCALLTYPE=__stdcall和显式This展开，d3d9types.h Creation四字段，d3d9caps.h Caps字段/嵌套类型相关范围。x86 COM指针4、HRESULT有符号32，包含this，非变参Stdcall；QI是(this,Guid*,void**)，不是只压两个业务参数。正常callee清理QI参数C字节，GetDirect3D/Creation各8，GetAdapterLUID C；Release4。AOT interop helper还可能另作栈管理，不能据外层ADD ESP断言双清栈。

|实际间接call/表偏移/SDK slot|可见this和业务参数，正常输出范围相对F|
|---|---|
|1355A1 CALL EDI /+18/6|输入device(ESI)，GetDirect3D(F-10)；DWORD out [F-10,F-C)|
|1355EB CALL EDI /+0/0|[F-10] factory，QueryInterface(Guid*=F-24,void**=F-14)；GUID读[F-24,F-14)，DWORD out [F-14,F-10)|
|135615 CALL EDI /+24/9|device(ESI)，GetCreationParameters(F-34)；16字节out [F-34,F-24)|
|135643 CALL ESI /+54/21|[F-14] extended factory，AdapterOrdinal=[F-34]值，LUID*=F-3C；8字节out [F-3C,F-34)|
|13567C→006250；13568F→006220；异常清理1356AE/1356C1|装入对象及表+8(slot2 Release)，经helper直接call，不是直接CALL EDX；无业务out，helper内部未取得|

四次COM外围1DA460/1DA491的ECX=F-50，EDX分别8/C/8/C；前者调用RVA13559C/1355E6/135610/13563E，后者1355AB/1355F5/13561C/13564D。135664→1D534E为条件helper，不认证其合同。显式清零[F-3C,F-C)、GUID复制[F-24,F-14)均不含F；所有静态LEA out均为负偏移，不能说源码本来要求把F传出。
本callee可见slot6/0/9/21，不是device GetDeviceCaps(slot7)。Caps9应304字节，若真实调用需单独缓冲区并检验[addr,addr+130)；不可误套到GetCreationParameters的16字节[F-34,F-24)或本callee中任一DWORD out。前序GetUpdateMethod Caps调用不在此384字节窗口，未认证其实际输出地址；不会因当前归零事件而宣布前序无损坏。

### GQ写后128字节栈：输出别名的可证链与边界
hardware第24行bounded-stack实际VA12042120=F-7C，请求/实读128成功。D=当时EBP12042152=F-5C；仅列原始槽值，不称已认证完整unwind：

|相对D/绝对槽地址|原始DWORD|与合同的关系|
|---|---|---|
|D /12042152|12042244=F|形似系统函数saved EBP，不据此认证帧链|
|D+4 /12042156|6D4C55ED|候选base+1355ED，恰为1355EB两字节FFD7的下一地址|
|D+8 /12042160|0B2C2640|形似QI this，未读该对象vtable认证类型|
|D+C /12042164|6D4C55EB|若为QI帧则riid竟是callsite代码地址，不是静态F-24|
|D+10 /12042168|12042244=F|若为QI帧则第二业务out=F，不是静态F-14|

此返回槽值与同SHA可靠call边界直接对应，将内部候选具体缩到factory QueryInterface，而非笼统所有COM：静态QI out应F-14，写后系统候选参数槽却为F；EBX也恰为F，窗口候选MOV EBX,[EBP+10]及AND [EBX],0连上实际保存槽归零。故可证“数据监视写F”和“原始栈中QI形状的out值F”相互吻合；尚不能无条件认证该系统函数就是这次QI、入参槽未被更早破坏、riid有效或完整调用栈。不能只靠返回地址一个栈值升级为可靠unwind/已证ABI根因。
读取当前InteropDeviceBitmapExports.cs 250–310只作设计参照：GetAdapterLuid先GetDirect3D、factory QI、Creation、GetAdapterLUID及finally Release，与上述slot序列吻合；当前源不是该SHA编译输入认证。FE snapshot artifacts/fh-crash-evidence/fe-production-experiment.cs.txt及名称检索均缺失；EP原始calli目录亦缺失，历史报告记Cdecl只能作线索，不冒充本轮亲核生成体或同SHA NativeAOT栈清理。因此未证明是Cdecl/Stdcall降低、helper、之前调用失衡或其它破坏机制；不扩大COM替换、不取粗行号当故障语句。

### 唯一缺证与下一最小动作（本轮不执行）
缺少同SHA1355EB实际CALL EDI前的参数栈，无法认证“静态F-14是怎样变为系统候选out F”。下一唯一动作须另行授权：在现有隔离设施对同一候选1355EB（1DA460返回后、FFD7执行前）做一次最小动态采样，保存TID/F/EBP/ESP/EDI、[ESP,ESP+10)（含三参数及邻槽）、固定[F,F+8)，认证this、riid、out分别是否[ F-10 ]、F-24、F-14，并记录EDI所属模块/入口原字节。与已存D+4返回槽及写后D+10=F对照；不需要广窗口/泛符号或全COM替换。本轮没有创建或执行该采样，也不追加已耗尽GQ预算。
仅本报告及handoff/session-summary/issues文档变更；生产修复0，默认隔离与EF/CF/CX/CY保持，Available/HW恢复/full四格/x64Event0独立线全部开放，完整替换未完成。历史报告全文已读，三共享仅最新受改节；历史保留未全文，不重复长历史。GR新增节及报告全文和共享受改节已写后分段回读；无测试执行，文档审计不是新的通过证据。

GR最后补记：本报告写后1–117行全文、handoff1–7/session-summary1–6/issues1–6受改节已实际回读；历史共享正文未全文。此补记随后再读。不认证完整unwind、系统指令起点或ABI机制；下一唯一动作仍是另授权1355EB CALL前最小参数栈采样，本轮执行0/生产修复0。

## GQ终态：唯一候选已真实执行，固定保存槽写后命中d3d9
本轮先读current-execution-rules并核GM正式包/入口存在，随后实际调用沙箱一次：sourceDirectory=WpfGfxShape/artifacts/gm-saved-write/com-acceptance-package；executableRelativePath=WpfGfxShape.ComAcceptance.exe；arguments=[--fh-crash-evidence,candidate]；timeoutSeconds=240；未指定outputRelativePath（整任务回收）；localOutputDirectory=WpfGfxShape/artifacts/gq-saved-write/candidate。工具原始返回：“沙箱执行完成。结果已保存到：d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\gq-saved-write\candidate”。实际task8988e80364b343a0945fb41c863a9040；不是GP委派失败的延续假定。没有再次上传回收目录；候选1/1已消费，不再留待下轮。GN控制成功依据授权交接，本轮未重跑控制或亲读GN全件。

### 实际身份与原件
原件根artifacts/gq-saved-write/candidate；核心证据Results/fh-candidate/{hardware.jsonl,file-identity.jsonl,context-av-1.json,context-av-2.json,cleanup.txt,native-events.jsonl,dr-candidate.jsonl,stages.jsonl}，另preflight.json、Results/aot-identity.json。监督PID7436，子PID5892，受监视TID2432，.NET10.0.13/X86/PointerSize4；testSHA765F48E74C67A908461358F74C5AF6B319A76D43AFF9445219A767116CF0DA83，MVID7a8e00dd-0e4f-4cba-9f1e-406c2a01c6e9。preflight Verified=true；actual DR identity路径为任务Native/wpfgfx_cor3.dll，SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53，版本1.0.0.0/Product1.0.0+2e2a3c7e64d74642cb46f64247b40e73b4431299。aot-identity同SHA/X86，子参数仅独立ABI合法Create测试过滤。
LOAD_DLL句柄前后Volume2759389505/Index7012352:294805/Size0:3995648/Write31285785:3967683863相同；Type1、同步读3995648字节成功；pointer0保存并恢复0；handle-sha同候选SHA。候选base6D390000/1832452096，Size4055040。CreateDevice/CreateRenderTarget/ColorFill均Hr0；Create before后无业务完成记录。

### 固定F写监视实际值（偏移/原字节为hex，未标hex的数值为十进制）
CALL1351EB前P=12042404、S=12042252；callee135573入口EBP=ESP=F=12042244=S-8，DWORD对齐，[F,F+8)=A4C0B700F0514C6D，即savedP=P、return6D4C51F0。入口原字节57565383EC440F57E40F1165C40F1165。slot0由exec转换为固定F write4，slot1设callee尾13569E；DR0=12042244/DR1=1833719454/DR7=851973（000D0005），Set/Get/readback Verified=true。
首次受监视写事件Event1、同PID/TID：OldValue12042404→NewValue0，Changed=true；[F,F+8)=00000000F0514C6D。写后EIP/ExceptionAddress=1854358420（6E874394），真实模块C:/Windows/SysWOW64/d3d9.dll，base1853620224，ModuleOffset738196（B4394），Size1548288，模块SHA F4757445D75D619BC5695294EE5CA6BFA0AD8763C6604A8E884739F605A6DF28。
完整通用/调试寄存器：EAX12042140、EBX12042244（恰等于F）、ECX12042140、EDX7012352、ESI187442752、EDI1833719275、EBP12042152、ESP12042120、EIP1854358420、EFLAGS582；ContextFlags65559，DR0=F、DR1=1833719454、DR2=DR3=0、DR6=4294905841、DR7=851973。原件64字节窗口起于EIP-32=1854358388：8D460423C86A00518D4DF4E86046F9FF8B5D108B7D0C6A1068287F7C6E83230057E8EF7FFDFF83C40C85C00F84E00000006A1068BC407E6E57E8D77FFDFF83C4。
数据断点为访问后停机，6E874394不是直接声称的写指令。窗口相对位置显示前3字节832300可解为AND DWORD PTR [EBX],0，EBX=F与写零吻合；这是基于有界窗口的解释，不冒充完整函数边界/源码映射或独立ABI认证。写入线程在此事件确认为2432；OtherThreadsExcluded=false，不能排除其它线程更早写或同值写，也不将模块命中直接等同d3d9自身缺陷。内部call、错误输出地址/ABI为何别名F仍需只读分析。
Event2为slot1尾部exec而非第二次数据写：13569E/6D4C569E，EBP=ESP=F，OldValue0/NewValue0，固定ret未变；DR6=4294905842。随后DR0/1/2/3/6/7全0且清零读回Verified=true，clear-confirmed Phase5/Tail=true/BudgetReached=false/Paired=false；不虚构return硬件配对。实际return AV context另证1351F0 EBP0/ESP=S。

### 真实退出、监督与覆盖
native-events 718起：首个AV6D4C51F0，读取地址4294967144；第二AV6D56E4D1，读取4294967164，随后同址second-chance。三次Continue均Status2147549185（DBG_EXCEPTION_NOT_HANDLED）、Succeeded=true；硬件事件Continue Status65538成功。context-av-1/2均有效GetThreadContext且句柄关闭；两文件原始FirstBadWriteAuthenticated/StackBalanceAuthenticated=false，保留原值，不为新硬件证据篡改旧context字段。
completed：子ExitCode3221225477/0xC0000005，TimedOut=false，SecondChanceExceptions=1，ExitEventCollected=true；supervisor-result ExitCode=1、CleanupConfirmed=true。不能写监督0。cleanup全文：Detach=True;Error=0;ExitEvent=True；WaitForExit=0;Error=0；两句柄0x36C/0x374关闭成功。工具完成仅说明执行与回收成功，不是测试通过。Results无完成TRX，不造测试通过/失败计数；只有一次候选任务事实。
本轮未改任何源码/生产/Host/打包，无终端、脚本、控制重跑、提取、build/publish/full；未执行可选默认隔离build，不能补称验证通过。Available/HW恢复/full四格/x64Event0保持开放，生产修复0、完整替换未完成。下一步仅离线核本次d3d9窗口、调用参数与固定F别名来源，不追加本预算运行、不直接修生产。
核心hardware36行、file-identity6行、两个context各122行、cleanup4行、actual identity及关键debug事件已亲读；native-events只身份、模块及关键700–730/745–757等范围，未称757行全文。报告历史54行已分段读完；下方全部历史原文保留，旧“待候选/缺件/待实现”不是最新状态。共享文件仅最新受改节回读，长历史未全文亦未覆盖。辅助检索有一次“目录不存在”及两次原始“Error: Function failed.”，均为读取检索失败而非沙箱执行失败，未据此重跑。报告全文和三共享新增节写后分段回读。

## GO接续进行中
接收GN控制task63919188bfc749c4869603c7953daf7f真实write4成功的交接；不重复控制。先亲核GM正式包/现有FH参数及控制原件，随后仅执行剩余候选1 task，实时回收原件；再补默认FH隔离正常发布。生产修复0，不新建报告，不运行只读/COM变体/full。

## GN进行中（接续GM实际进度）
GM已重新核同SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53的callee384字节（slice SHA F6EF…），无需重复提取。FH+DR write4角色已编译发布于artifacts/gm-saved-write/com-acceptance-package，GM未运行控制，normal默认隔离验证尚欠。此段GM进度依据监督交接，GN正离线亲核最终源/Build.props/包，继而实际控制1次，通过才同SHA surfaceCreate候选1次；不重跑旧加载/exec控制，不动生产。下文GL缺件阻断为历史，不再作为GN阻断。

## GM已做与GN进行中
GM已重新认证同SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53的callee384字节；不再提取或寻找旧档。GM已编译发布FH+DR write4设施至artifacts/gm-saved-write/com-acceptance-package，尚无write4控制运行。以上GM进度来自监督交接，GN正在离线核最终源码/Build.props/正式包，接续一次正常write4控制，通过才一次同SHA合法surfaceCreate候选。最多control1+candidate1，不重复加载控制，不改生产和禁改区。以下GL为历史，原件缺失不再作为当前阻断。

## GL终态：前置连续字节原件缺失，未进入硬件控制
本轮未定位写入指令。control0/1、candidate0/1均未消费；无新task、actual testSHA/MVID/runtime、监督/子退出码、硬件事件、清理记录或TRX。没有实施FH扩展、构建、发布或沙箱执行；不是正常控制失败或硬件不支持。生产修复0，默认隔离保持。

## 具体阻断
用户要求第一步只读已提取同SHA [135570,1356F0) 连续字节。FW/FZ报告指向artifacts/fx-callee-tail/result/result/fv-pe.txt；按实际根目录及WpfGfxShape子根两种路径读取均不存在，WpfGfxShape/artifacts/fx-callee-tail目录亦不存在。名称搜索fv-pe.txt/fx-callee无结果，实际.coding-agent为空。不能将报告转述的片段SHA或手解文字当成本轮亲读384字节原件。
实际WpfGfxShape/artifacts/gk-frame-context/com-acceptance-package/Native/wpfgfx_cor3.dll存在，并非声称候选丢失。专用文本工具不能直接读取其二进制窗口。未新增提取角色或运行已有提取器，避免超出“第一步只读已有原件”及control1+candidate1有效任务预算。恢复原件即可继续，不必重建生产或扩窗。

## 静态调用参数范围（仅据FW/FZ手解，未独立核原字节）
以下偏移/RVA均hex，F为callee序言后EBP；不是新动态参数值，不命名未知函数，不认证输出尺寸或ABI。栈参数顺序按最后PUSH在前记载，helper是否保持它们未知。

|调用RVA/目标|可见传值/相对F范围|
|---|---|
|13559C→1DA460|ECX=F-50、EDX=8；此前PUSH F-10、ESI；helper写范围未知|
|1355A1→EDI，来自输入对象表+18|可见栈参数ESI、F-10；若输出DWORD则[F-10,F-C)，真实长度/目标未知|
|1355AB→1DA491|ECX=F-50；先ADD ESP,8；helper范围未知|
|1355E6→1DA460|ECX=F-50、EDX=C；此前PUSH F-14、F-24、ECX对象|
|1355EB→EDI，来自[[F-10]]表首项|可见栈参数对象、F-24、F-14；F-24此前初始化16字节[F-24,F-14)，F-14若DWORD为[F-14,F-10)；实际写宽未知|
|1355F5→1DA491|ECX=F-50；先ADD ESP,C；范围未知|
|135610→1DA460|ECX=F-50、EDX=8；此前PUSH F-34、ESI|
|135615→EDI，来自输入对象表+24|可见栈参数ESI、F-34；若DWORD输出则[F-34,F-30)；之后无立即ADD ESP,8，不能单据此定ABI错误|
|13561C→1DA491|ECX=F-50；真实ESP/helper范围未知|
|13563E→1DA460|ECX=F-50、EDX=C；此前PUSH F-3C、[F-34]、[F-14]|
|135643→ESI，来自[[F-14]]表+54|可见栈参数[F-14]对象、[F-34]值、F-3C；第二项不是F-34地址；若双DWORD输出则[F-3C,F-34)，实际写宽未知|
|13564D→1DA491|ECX=F-50；先ADD ESP,C；范围未知|
|135664→1D534E|条件分支到达，未见新显式参数；隐式输入未知，随后跳1355C7|
|13567C→006250（E8 CF4BEDFF相对计算）|ECX=[F-14]，EDX=[[ECX]+8]；无新PUSH；不是直接CALL EDX，目标合同未知|
|13568F→006220（E8 8C4BEDFF相对计算）|ECX=[F-10]，EDX=[[ECX]+8]；无新PUSH；目标合同未知|

显式三个连续MOVUPS零写合计[F-3C,F-C)，另F-24的16字节赋值也不包含[F,F+4)。所有可见LEA输出为负偏移，不直接等于F；但helper的F-50传址尺寸未知，不能证明callee不会跨界或实际ESP未变。条件失败合流13565E、135664及清理分支均不能伪造动态到达。
GK已证task448644f7c5b74a67870c0aed5619d3f6入口F51888324=S-8、[F]=P51888484，pop前固定槽变0、ret6D4C51F0不变；该事实不回退。首坏写/写入线程仍未知，源码checksum未证，不拿粗行号当fault语句。

## FH相关核查与待实现合同
已读当前HardwareSample字段/日志、Write/Arm/Hit及四位置文件映射认证相关范围，非全文。Write只写DEBUG_REGISTERS组、清DR6后完整读回；Arm/Hit目前只接受exec本地位1/4并要求EIP/ExceptionAddress等于阶段地址，不是现成数据写监视能力。旧GK exec控制不能替代write4控制。
x86 slot0 write4必须RW0=01、LEN0=11、本地L0=1，即相关DR7=000D0001；保留slot1执行尾部L1时为000D0005，其RW1/LEN1仍00。这只是待实现编码合同，不是实际Set/readback。入口须认证F=S-8、ESP=F、[F]=P及DWORD对齐；每事件严核PID/TID/阶段/DR6/DR7。数据断点EIP为访问后位置，不等于直接写指令，首次触发可能同值写，不能必称首次坏写。需保存固定槽前后/返回地址/栈窗口/EIP前后有界字节/完整寄存器/模块基址SHA及偏移。
每线程DR不能排除其它线程写。timeout/线程切换/异常clear/Get/Set/readback失败均未新增覆盖。每事件Continue一次、未知异常NOT_HANDLED、清DR失败仍终止等待并关句柄是必须维持的门；本轮未动态验证。无改保护页、业务out位置、生产COM或吞真实异常。

## 阶段历史与下一唯一行动
接手时先建本报告并前置handoff GL进行中；后读规则、方案分段、GK终态、FW/FZ关键手解与FH相关范围；发现原件缺失后明确阻断收口。不冒称亲读实际FX/GK运行原件，不继承他人全文核证。
下一唯一行动：恢复既有FX task155dbcf5b66f4d199ace2507fc72f40b的result/result/fv-pe.txt（384字节hex及整文件SHA/窗口SHA身份），只读核证后接续授权write4控制1，通过才同SHA候选1。不重建生产、不重复加载/exec基础控制、不二分整个调用组。无法恢复时应先由监督明确授权既有正式只读角色重新提取，不伪装已有原件。
仅本报告与三共享交接变更，无源码/打包/生产/禁改区变更，无新构建测试；不需要对未改代码声称正常隔离发布成功。Available/HW消费恢复/full最终四格/x64Event0独立线仍开放，完整替换未完成。

## 写后覆盖
本报告全文与三共享GL最新受改节写后回读，历史保留未全文。本轮明确前置阻断、无运行、无后台悬挂，不是监视点控制通过或已跑未读。
