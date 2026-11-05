# GX GetDirect3D 清栈三点认证

## GX起始登记（历史，已由下方GX最终及GY独立终态覆盖）
已开始直接执行，主代理仅监督。先认证SDK/绑定IL/calli及同SHA连续原字节，再在门通过后至多一次同SHA候选三点硬件采样；必要新增控制至多一次。只读固定小窗口提取预算至多一次。尚无运行、构建、发布或生产修改。

候选限FE SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53，非当前default。不能从Cdecl静态元数据、SDK或前后ESP相等推断真实双清栈。不重建生产，不调整ESP/frame/COM参数，不关闭分析器。

## 阶段记录（保留历史）
1. 开始：正在读取规则、完整方案、GW/GV/GU、GS及FhCrashEvidence实现和现存原件。

2. 静态阶段：GM同SHA384字节原件已亲读全37行，无重提取。13558F LEA ECX,[EBP-10]；135592 PUSH ECX、135593 PUSH ESI；135594 LEA ECX,[EBP-50]；135597 MOV EDX,8；13559C E8BF4E0A00；1355A1 FFD7 CALL EDI（2字节）；1355A3 8BF8 MOV EDI,EAX（2字节）；1355A5 83C408 ADD ESP,8（3字节）；1355A8 8D4DB0为下一可靠边界。三点A1/A5/A8，非盲抄地址。SDK10.0.26100.0 shared/d3d9.h 429–440亲读，IUnknown三方法+TestCooperativeLevel/GetAvailableTextureMem/EvictManagedResources之后GetDirect3D为slot6，this/out两DWORD。
3. 专属发布成功exit0，正常分析器，独有GX obj/bin/publish/package；生产DLL来自GU包不重建。preflight Verified=true，testSHA04902AA6E7992CB3F0265EFFAD3A951DB26618B0AA32817A7562B1DA986C1E16/MVID9e5e0e0a-65b1-4e69-b7cc-cb01171890ac/.NET10.0.13/X86。已运行只读绑定PE元数据角色，不加载候选。现存Silk2.23.0/net5.0 binding SHAF08ACBB76DD7C60803F8F0D4D8212C77A85D3EF67B1FB42FBB18F3C15A8586E4/MVIDdeec18f5-2a98-450b-93cd-9117248c6163；GetDirect3D pointer/ref两重载MethodDef06000A78/79均calli110002CA，签名0102080F1182580F0F11823C：Cdecl(01)、参数2、int32返回、Device9*/D3D9**。泛型06000B94转调GetDirect3D。原始IL见binding/gx-binding.txt第1–4行/签名718行。现存绑定不能代替历史FE精确输入身份，历史源checksum/精确compiler版本仍缺；不因此阻止本次同SHA实际清栈采样。
4. 采样前：复用既有严格DR6/EIP/phase/readback与callee F认证机制，仅增加同槽三点切换和只读采样，不新增底层机制，不机械重复控制。候选预算0/1；即将消费唯一一次。原件范围和静态证据不冒称动态双清栈。

5. 动态/清理阶段：唯一候选task7b6833b1032b4acbb9bd6978b14004af，PID2236/TID4604/F7849764，三点ESP7849676→7849684→7849692，CALL返回已+8，随后ADD再+8；该候选callsite双清栈成立。hardware第29/37/45行已亲读，DR全清/readback第46–49行通过，cleanup全4行成功。正在补齐context/identity/异常与退出原件覆盖；不再追加候选。预算1/1消费，无新控制/重提取。

## GX最终：该候选callsite双清栈已实证

### 三点与合法参数
实际task7b6833b1032b4acbb9bd6978b14004af，正式源artifacts/gx-getdirect3d-stack/com-acceptance-package，直接既有exe --fh-crash-evidence candidate/外240秒。完整回收artifacts/gx-getdirect3d-stack/candidate；监督PID10068、子2236、native TID4604。actualNative为C:\CodingAgentSandbox\Tasks\7b6833b1032b4acbb9bd6978b14004af\Native\wpfgfx_cor3.dll，base1832452096/6D390000，Size4055040、文件3995648；SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53。testSHA04902AA6E7992CB3F0265EFFAD3A951DB26618B0AA32817A7562B1DA986C1E16，MVID9e5e0e0a-65b1-4e69-b7cc-cb01171890ac，.NET10.0.13/X86/指针4。包preflight与候选内preflight Verified=true，实际PE方法存在，不以定义常量文字代替预检。

同task/TID/F：caller S7849772/P7849924，经1351EB CALL→135573认证F7849764=S-8、[F]=P、[F+4]=1833718256/6D4C51F0。135573在push edi/esi/ebx/sub44之前，固定分配底F-50；两push之后参数基准F-58=7849676，实际一致。

|点|RVA/EIP|ESP（十进制）|相对F|相对CALL前|说明|
|---|---|---|---|---|---|
|1|1355A1/1833719201|7849676|-58|0|CALL EDI执行前，两DWORD已压栈|
|2|1355A5/1833719205|7849684|-50|+8|CALL返回，MOV EDI,EAX已执行但不改ESP；ADD尚未执行|
|3|1355A8/1833719208|7849692|-48|+16|ADD ESP8执行后，较点2再+8|

三点EBP=F；EAX183477144→0→0，EBX7850380保持，ECX7849684→0→0，EDX7849676→2293760→2293760，ESI183769856保持，EDI1854533856→0→0，EFLAGS514→582→514。全部通用regs/DR见hardware第29/37/45行。点2不是刚返回A3边界，但中间唯一8BF8 MOV不改ESP，故返回清8结论有效；没有断言原生RET8机器指令已读取。

this183769856=[点1ESP]=ESI，this原字节001BF40A；对象所指vtable183781980（5C4AF40A），slot6地址183782004，值1854533856=点1EDI。out参数7849748=F-10，点1参数槽Hex14C77700，out局部00000000→C029F00A→C029F00A，HRESULT0。actual槽位所属C:\Windows\SysWOW64\d3d9.dll/base1853620224/Size1548288/SHA F4757445D75D619BC5695294EE5CA6BFA0AD8763C6604A8E884739F605A6DF28；入口32字节8BFF558BEC51515356578B7D088BCFF7D96A001BC98D470423C8518D4DF8E8E1。不是factory QI slot0身份套用。

三点[ESP-16,ESP+32)均48/48成功/Error0，依次原hex：
1：7F0000000000803F00000000A1554C6D001BF40A14C77700A1554C6D24C7770098A3EF0A00800000CCC6770000000000。
2：24C77700A3554C6D001BF40A14C77700A1554C6D24C7770098A3EF0A00800000CCC67700000000000000000000000000。
3：001BF40A14C77700A1554C6D24C7770098A3EF0A00800000CCC677000000000000000000000000000000000000000000。
三点固定[F,F+8)均8/8/Error0、C4C77700F0514C6D；caller返回槽[F+4]4/4/Error0、F0514C6D；this/out各4、入口32、point指令16均实读完整/Error0。实际原字节门含135590连续窗，hFile1292读取3995648/3995648/Error0，Volume2759389505/Index8388608:294920/大小/写时前后稳定，位置0恢复成功，hFileSHA=pathSHA，映射PE稳定字段和指令门通过。文件句柄关闭事件见native-events retained-file-disposed。

### 原因链及边界
已实证：合法slot6调用前F-58→真实返回F-50（清8）→调用方ADD再到F-48（多清8）。结合GM连续原字节、GU/GV/GW同SHA helper前后事实：后续三push变成F-54而非F-5C；helper ECX=F-50记录写[ECX+4]=F和[ECX]=返回地址，正好覆盖out/riid。该候选callsite清栈错误→后续参数区上移→helper合法记录写覆盖实参的原因链成立。当前GX不额外监视QI，不冒称同一task再次取得GV的QI快照；历史关联保持同SHA且明确task不同。SDK basetyps.h 28亲读__stdcall，d3d9.h slot6签名亲读；绑定PE pointer/ref重载与calli静态认证支持Cdecl不匹配，但现存包非历史输入hash认证，未据此裁定全部Silk/编译器bug。FE不是default，历史源checksum/精确compiler仍缺，不无限等待归档、不把当前版本字符串当历史身份。

### 异常、退出与清理
复用严格slot0 DR6/EIP/phase/readback，1351EB→135573→A1→A5→A8，每个命中Continue一次65538；无EIP/ESP/参数/frame改写，无INT3/机器码注入。三点后DR0/1/2/3/6/7全0、Set/Get成功/Error0、Verified=true，Owned=false。原崩溃继续：1351F0第一机会AV，EBP0/ESP7849772=S；第二AV6D56E4D1，最终同处第二机会，均NOT_HANDLED2147549185。两context各122行全读，非法EBP/EAX/EDX/ESI等0字节/Error299明确不当有效内容。
EXIT_PROCESS Kind5已收，子Exit3221225477/C0000005，TimedOut=false，SecondChanceExceptions1；监督Exit1/CleanupConfirmed=true。cleanup全4行Detach=true/Error0/ExitEventtrue、WaitForExit0/Error0、线程0x3B8/进程0x194关闭成功；正常异常退出不需Terminate，不冒称调用过Terminate。无完成TRX，不造测试计数；沙箱工具仅回报回收完成，无独立工具退出码可抄。

### 修改与验证
仅FH显式诊断新增GX编译条件三点采样与必要只读绑定PE角色；ignored artifacts/GX.props嵌入现存绑定供纯C#元数据读取。生产COM/DRDS断言/ExitDiagnostics/NativeRun其它入口/VisualCache/historical/捕获/打包/CF/CX/CY未改。生产修复0，原失败DLL未重建。候选预算1/1；额外正常控制0、固定PE窗重取0（复用原件）、full/COM变体0。独立preflight及只读gx-binding各一次不是业务候选。
正式x86 Release自包含10.0.13发布exit0，独有obj/bin/publish/package、显式PublishDir，Native/Contracts/aot.sha256/MTP/runtime按正常依赖图打包，上传只用com-acceptance-package，回收独立candidate根。最后源码修改后默认Release/x86自包含publish exit0，normalbuild/obj/bin与normal-publish独立，未传FH/DR/GX/FV常量，默认#if隔离保持；两发布60行日志全回读，但stdout各前18920/18913字符工具截断，不宣称完整stdout或精确警告数；现存CA/MSTEST分析器警告仍在，无NoWarn/关闭分析器。

### 下一最小修复与开放风险
下一步只评审并实现正确COM调用约定适配（GetDirect3D及相邻同因调用），不增大frame/挪输出/改refs/手补ESP；用当前实际默认调用点核相邻同因风险，不能称单点必全修。新候选必须x86/x64 Release DR、生命周期、BWCK绑定实际SHA验收。当前轮仅x86诊断分支实际运行，x64、默认业务、GU旧分支/其它FH控制分支未运行；历史精确编译来源仍未认证。Available/HW消费恢复/full四格/x64Event0开放，完整替换未完成。

### 阅读覆盖与最后登记
规则27行、完整方案120行分段全读；GU报告GV/GW与GS/GT最新核心节已读，历史长段未宣称全文。GM原件37行全读；本轮hardware全51行（长行17/24/37/44等补单行）、两context各122行、fileidentity6、cleanup4、preflight/identity/dr6全读；native-events1–3、700–760及关键退出已读，不称760行全文。绑定只亲读GetDirect3D相关1–12及714–719，全部其它方法standalone输出未读不冒称全库认证。报告全文和三共享本轮最新节需写后回读，历史保留，不反复拖延千行。

## GY独立终态：三点动态认证成立，交接完成
GY仅专用读取/文档写入；新实验、提取、构建、发布、沙箱、脚本、源码修改及生产修复均0。起始handoff已登记独立核证，现以本终态覆盖；GX/GW等旧阶段保留历史，不再是待执行状态。

### 独立认证与纠正
亲读GX candidate/Results/fh-candidate/hardware.jsonl全部51行（截断长行已单行补齐）、两份context-av各122行、file-identity全部6行、cleanup全部4行、dr-candidate全部6行、actual aot-identity及preflight；native-events亲读1–3、699–760（717起补齐截断），不是760行全文。task7b6833b1032b4acbb9bd6978b14004af、监督10068/子2236/native TID4604，actualNative路径与上方一致，base6D390000/Size4055040/file3995648；hFile1292身份前后稳定、完整读取、恢复位置0且SHA匹配1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53，699行关闭成功。test04902AA6E7992CB3F0265EFFAD3A951DB26618B0AA32817A7562B1DA986C1E16/MVID9e5e0e0a-65b1-4e69-b7cc-cb01171890ac/.NET10.0.13/X86；未把apphost当test DLL。
三点29/37/45行同TID/F7849764，A1/A5/A8的EIP1833719201/205/208匹配异常地址及DR0；Get成功/Error0/flags65559，DR6=4294905841，屏蔽保留位后仅slot0，DR7=1、TF未置。phase-hit-raw/phase-hit/gx-point是同次事件不同记录，不是三次重复命中。每次换点Set仅flags65552调试寄存器组并Get读回Verified，未改应用ESP/EBP/EIP或参数；native-events五次硬件异常各Continue一次65538。
GM正确原件artifacts/gm-saved-write/readonly-result/result/fv-pe.txt全部37行亲读，SHA一致、384字节slice F6EF760C…；首次误试gm-callee-frame路径不存在，已定位原件，不冒称缺件即已读。连续原字节1355A1是FFD7（CALL EDI），不是FF51??；1355A3=8BF8只MOV EDI,EAX；1355A5=83C408；1355A8=8D4DB0。故CALL前ESP7849676(F-58)→ADD前7849684(F-50)是实际返回已清8，ADD后7849692(F-48)再清8。点2不是刚返回A3但唯一中间MOV不改ESP，边界有效。无需历史compiler源码才能承认这个callsite重复清8；未读取原生最终RET8指令，不能伪称已读。
this183769856/ESI经真实参数槽→vtable183781980→slot6地址183782004→target1854533856认证；out地址7849748=F-10，保持同址，内容0→C029F00A，HRESULT0。[F,F+8)持续C4C77700F0514C6D、EBP=F，保存P7849924/返回6D4C51F0不变。目标为实际SysWOW64/d3d9.dll/base6E7C0000/Size1548288/SHA F4757445D75D619BC5695294EE5CA6BFA0AD8763C6604A8E884739F605A6DF28，不挪用QI slot0身份。各点48字节相对ESP窗、F8/ret4/this4/out4/入口32/指令16均请求=实读/Error0，未把失败读取当有效内容。
两AV context GetThreadContext各716字节、对齐0、请求/返回65543、有效成功且句柄关闭。第一EIP6D4C51F0/EBP0/ESP7849772；第二EIP6D56E4D1。EBP/EAX/EDX/ESI等按各份实际BytesRead判定，0字节/Error299无有效内容；context的StackBalanceAuthenticated=false不推翻单独hardware三点窄认证，也不升级成完整unwind。三点后DR0/1/2/3/6/7清零并读回，Phase5/Owned=false。真实AV均NOT_HANDLED；EXIT_PROCESS Kind5、子C0000005/监督1/cleanuptrue/nonTimeout/SecondChance1成立，不是产品通过。未需Terminate；Results无TRX，不造执行/通过计数或工具exit0。

### 因果层级及责任边界
SDK10.0.26100.0 d3d9.h429–444/basetyps.h20–31本轮亲读，slot6=this/out、COM Stdcall明确。现存binding/gx-binding.txt1–4及714–719亲读，calli110002CA签名0102080F1182580F0F11823C确为Cdecl两参数；但HistoricalIdentityAuthenticated=false，不能作为历史FE精确输入证明。已证的是同SHA机器caller的Cdecl式ADD8与实际native清8不兼容；历史源checksum/PDB输入与精确compiler版本仍限制“源声明还是codegen责任”，不限制动态结论。不能说整个Silk库bug、d3d9 bug，不能说当前default已同样动态实测。
本轮亲读GU报告GV/GW核心及GW52–85完整分层；引用GW既有独立核证：同SHA GV task97a2…helper前后ESP F-54不变，ECX F-50，正确riid F-24/out F-14被写成返回地址/F；连续helper47A写[ECX+4]=F、47D写[ECX]=入口返回地址，静态写区与两槽完整重叠。与本轮GX多清8、GM后续三push应F-5C实为F-54组合，形成“重复清8→outgoing参数上移8→helper记录写覆盖riid/out→错误QI实参”分层因果链。不是同task新采样QI或动态首坏写监视。后续保存P→0及错误out零写的GR/GQ关联明确引用GW既有独立核证，GY未亲读GQ/GV原件，不伪称本轮全链全核；精确runtime helper符号布局未认证，不正式命名。

### 默认隔离、既有修复与生产实现门
源码只读亲核FhCrashEvidence全部六处GX条件区（using、Capture、三点切换、原字节门、gx-binding角色、Arm地址）及Get/Set/DR匹配机制；外层FH_CRASH_EVIDENCE、NativeRun FH分派、ComAcceptance.csproj、本地Directory.Build.props/targets与ignored GX.props已核。GX只在显式FH诊断内采样及PEReader元数据读取，不加载绑定/候选执行业务；GX.props显式嵌入绑定和FH/DR/GX常量，不由默认项目导入。生产COM未因GX改变；当前InteropDeviceBitmapExports.GetAdapterLuid仍device->GetDirect3D/QueryInterface/GetCreationParameters/GetAdapterLUID及finally Release，不是旧FE已修默认。相邻GetUpdateMethod仍GetDeviceCaps/QI/Release/GetDC/ReleaseDC；不能单slot6承诺全修。Core/Direct3D9TextureDescription已有Stdcall slot6/10窄实现保留，不扩全库。CF/CX/CY及EF既有修复/默认trace隔离按既有独立核证保留，本轮不重验其业务，更无修改；无版本差分API不虚构仓库全量diff认证。
既有最新正常发布日志60行本轮全读：明确默认Release win-x86自包含.NET10.0.13、独有normalbuild/normal-publish，实际exit0，未传FH/DR/GX/FV常量；现存CA/MSTEST警告仍在，stdout38913字符前18913不可见，不能造精确警告总数或称零警告。该日志不是本轮新发布，也不是default业务通过，更不是当前生产COM已修。默认隔离安全与完整替换安全是不同结论。
下一唯一可执行生产任务：在当前InteropDeviceBitmapExports本类已认证有限COM闭包，逐项对SDK核slot/this/参数宽度/返回值/所有权，以明确unmanaged[Stdcall]声明修正调用边界，覆盖GetAdapterLuid及相邻同因必要调用；复用既有正确模式，保留CF/CX/CY/EF、引用计数与finally，禁止手补ESP、加frame、挪helper、SuppressGCTransition或泛全库替换。历史归责归档可并行，不再作为承认动态事实或进入此最小源边界实现的前置阻塞。此为具备实施门而非批准保留：必须正常分析器、官方/候选x86与x64 Release正式DR循环及refs收敛、生命周期和BW1/CK10，回读TRX/JSON/identity并绑定actual newSHA；不通过精确撤回本轮新适配，不能拿1893370旧失败SHA充新通过。

### GY最后覆盖登记
规则27及完整方案120行分段完整读取；GX原报告56行分段完整读取，GU/GW仅以上最新核心及52–85，早期历史未全文；GQ/GV原件未本轮亲读明确标记。仅此GX报告与handoff/session-summary/issues四文档改变，不另建报告。写后回读范围为本报告全文（历史56行与新增GY分段）及三共享最新受改节/本末次补记，三共享长历史保留未全文。Available、HW消费恢复、full最终四格、x64Event0独立线全部开放；生产修复0，完整替换未完成，GY独立核证与交接已完成。


