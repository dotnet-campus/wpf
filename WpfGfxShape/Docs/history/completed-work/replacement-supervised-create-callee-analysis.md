# FW callee RVA135570 可靠边界与有界只读分析

## FZ 最新终态：FX尾段实际核证完成；动态采样仅设计
FZ仅用专用文件工具核既有原件及源码、更新本报告与三共享；没有新提取、实验、构建、发布、沙箱、运行或代码修改。FY“进行中”在本次交接中关闭为由FZ接替并交付，不声称FY已核证或全文读完。下方FW/FX/FY旧状态均为保留历史，不再作为下一动作。

### FX诊断身份、原件与固定范围
实际FX task为155dbcf5b66f4d199ace2507fc72f40b；远端根C:\CodingAgentSandbox\Tasks\155dbcf5b66f4d199ace2507fc72f40b。FZ第一步亲读artifacts/fx-callee-tail/result/result/fv-pe.txt全部1–37行；其记录诊断exe74236254字节、SHA256=36FE288FC2AF84528A4AC24A1CFE72D5987A7D913354FA5513C0AFDAC602C387、MVID=90c1efa6-58e5-432b-9f5b-5566c70808ce，实际.NET10.0.13/X64/PointerSize8/FV_PE_READONLY/CandidateLoaded=false。这是提取器身份，不是被调试x86进程身份。
原件输入DLL3995648字节，SHA256与ExpectedSHA256同为1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53；I386/PE32，ImageBase10000000、SizeOfImage3DE000。.text RVA1000/raw400/rawSize21C800，offset=400+135570-1000=134970；[135570,1356F0)为0x180=384字节，文件区间[134970,134AF0)。原件12行片段SHA256=F6EF760CA4ADA31AAE1C4E4F33F010A70528FB3C319396FD6735CB551A4356F0，13–36行共24组16字节hex连续覆盖该窗口。
已读FvPeReadonly.cs全部1–83行：精确--fw-callee-window分支40–43行固定上述范围；10/27–37行固定输入、整文件SHA拒错及I386/PE32；53–79行唯一raw backing、.text/384白名单、文件/节边界、ReadExactly、片段SHA及逐16字节hex输出与原件一致。此为既有日志与实现交叉核对，不是本轮重算DLL/bin哈希或独立二进制比对。原件37行RoleResult=0是角色字段，不造工具数字退出码、TRX或动态成功；FunctionRangeAuthenticated=false、WindowIsNotCompleteFunction=true仍成立。

### 从FW已知13566E接续的连续静态尾段
FW原手解从135570推进至13566E时只有837D而停止，有明确缺字节，不是已证RET。FX原件29–31行补足；以下RVA/立即数均hex，按指令长度连续推进，不进入CALL目标：
- 13566E:837DEC00 CMP DWORD PTR [EBP-14],0；135672:740D JE135681；135674:8B4DEC MOV ECX,[EBP-14]；135677:8B11 MOV EDX,[ECX]；135679:8B5208 MOV EDX,[EDX+8]；13567C:E8CF4BEDFF CALL（5字节，下一681）。
- 135681:837DF000 CMP DWORD PTR [EBP-10],0；135685:740D JE135694；135687:8B4DF0 MOV ECX,[EBP-10]；13568A:8B11 MOV EDX,[ECX]；13568C:8B5208 MOV EDX,[EDX+8]；13568F:E88C4BEDFF CALL（5字节，下一694）。
- 135694:8BC6 MOV EAX,ESI；135696:8BD7 MOV EDX,EDI；135698:8D65F4 LEA ESP,[EBP-C]；13569B:5B POP EBX；13569C:5E POP ESI；13569D:5F POP EDI；13569E:5D POP EBP；13569F:C3 RET（无imm16）。31行连续hex为8C4BEDFF8BC68BD78D65F45B5E5F5DC3，结合前两行形成连续依据，不是搜RET模式猜尾声。
结合入口55/8BEC/57/56/53/83EC44，令callee入口ESP=S且帧EBP保持S-4、保存槽未污染：LEA后ESP=S-10，四次POP依次从[EBP-C]/[EBP-8]/[EBP-4]/[EBP]取保存值并恢复EBX/ESI/EDI/旧EBP，RET取[S]，最终ESP=S+4。这里S-10为hex，即S-0x10；这是有前提的静态账本，不是实测。EAX/EDX输出赋值和无立即数RET已见，但具体返回类型/ABI仍未认证。
窗口还含1356A0后另一字节段、1356C6的C3、填充与1356D0形似序言；未从已证控制流认证其入口/用途，不据此圈完整函数或PDB范围。主尾声存在不证明所有分支/内部call效果、保存槽不变、动态到达、首坏写、EBP归零点或物理清栈失衡；原间接call cleanup差异仍不足以定ABI错误。

### 下一有界动态采样最小扩展设计（本轮不实施）
已读FhCrashEvidence.cs全部1–521行。其#if FH_CRASH_EVIDENCE/x86角色、Pack4的716字节X86Context含DR字段；RecordContext目前flags10007、OpenThread仅GET_CONTEXT(8)，无SetThreadContext。现有模块事件采路径/基址/size/文件SHA；异常只在启动loader断点及最多两次候选AV采context；事件finally负责ContinueDebugEvent，120秒超时终止/130秒上限，最终5秒drain/等待/关句柄。已有结构可扩展，不等于已有CALL前后采样能力；现有模块SHA按路径读取也不是防换件认证。
最小范围仍在同正式ComAcceptance的显式纯C#调试角色，不新增第三Host，不用INT3/WriteProcessMemory改代码、机器码注入或原生探针。另授权后先做正常控制，控制未通过不进入候选。控制应在同exe已有normal-ready角色中增加纯C#、禁止内联且预JIT的正常方法调用场景，在已认证暂停握手点布置硬件断点，证明命中/上下文/清除/继续/正常退出；控制地址单独核其实际JIT代码与call/return，不把候选RVA移用于控制。现有normal-ready只有token后return，不能冒充已有控制覆盖。
1. 地址与线程安全门：仅Windows原生x86调试器/子进程，检查结构大小/对齐/flags/API；用CREATE/LOAD事件的文件句柄及完整路径、文件SHA、I386/PE32/SizeOfImage与加载基址B绑定同一映像，防路径替换；核内存CALL及返回字节，任何重定位仅按已验证PE重定位规则处理。候选只有上述精确SHA时，才可用B+1351EB与B+1351F0，不能用首选ImageBase代B。FZ另亲读既有FV原件1–13、44–57行，同SHA的51行包含E880030000，下一RVA1351F0加380=135570，52行返回指令为8BB568FFFFFF；此仅静态地址合同，不是新加载认证。
2. 目标TID须由受控调用线程的独立阶段标记与debug事件互证，不能凭初始线程猜。在该线程暂停debug事件期间打开GET_CONTEXT|SET_CONTEXT(0x18)句柄，读取原DR0–3/6/7及控制/整数上下文（请求10017包含DEBUG_REGISTERS）；发现已有硬件断点/TF等非本角色状态则拒绝，不抢占其它调试器。绑定PID/TID/模块加载代次，一次调用对；TID退出、模块卸载即失效。
3. 装配DR0=B+1351EB、DR1=B+1351F0，DR7仅本地L0/L1，RW0/RW1=00执行、LEN0/LEN1=00（相应控制位清零），不用全局断点。先只启用DR0（无其它状态时L0=1）；SetThreadContext后GetThreadContext回读认证。硬件执行异常发生在指令执行前，收到EXCEPTION_SINGLE_STEP(80000004)仍须核first chance、PID/TID、EIP、ExceptionAddress、DR6.B0及DR7与已装地址，非本角色异常保持DBG_EXCEPTION_NOT_HANDLED(80010001)，不可吞掉AV或应用单步。
4. pre命中读取EIP/EBP/ESP/EFlags及全部整数寄存器、DR6/DR7、模块身份和有界原始内存，持久化后禁DR0、清本次DR6状态、启DR1（本地L1=4），回读成功才以DBG_CONTINUE(10002)继续。执行断点不改EIP；禁用命中槽避免重触发，不靠通用TF单步；如需RF仅由已审合同设置并记录前后，不能盲改其它EFlags。post仅接收同一次pre、同TID、B+1351F0、DR6.B1，采样后清本角色DR0/DR1/DR6/DR7并回读，再继续；重入/递归/非预期栈位置判本次配对无效并清除停止，不追无限事件。
5. 不能只看post EBP0：pre令P=EBP、Q=ESP，同时读取并标注有效长度的[P]旧帧、[P+4]caller返回槽、[P-C..P)三saved槽、[P-98]局部、[Q-0x10..Q+0x10)及模块CALL字节。静态预期callee入口S=Q-4，CALL压入返回地址B+1351F0于[Q-4]，callee保存pre EBP于[Q-8]、EDI/ESI/EBX于[Q-C/Q-10/Q-14]；这些槽pre时尚未写入，只能标将写地址，不能当已存在证据。post在同一暂停点读取相同绝对地址与寄存器，比较ESP是否Q、EBP是否P、callee-saved是否保持，检查残留[Q-4]/[Q-8]及saved槽，记录实际返回地址/读失败/模块偏移。需要时扩展一次固定0x30字节覆盖[Q-0x20,Q+0x10)，不跟坏EBP扫描。残留槽可被覆盖，只是佐证；两个端点即使相等也不证明内部未曾坏写或全路径栈平衡。
6. 全程每对最多pre/post各一次，沿用8MiB事件上限，采样每次最多12块/1024字节且检查32位地址溢出、RPM实际有效长度；候选最多一次授权调用，不以重试追成功。超时/任意API失败/AV/未配对/取消时，在已暂停的拥有线程上清调试寄存器并回读；若运行中超时不能假称可直接SetContext成功，须有审过的SuspendThread/ResumeThread成对暂停清除路径（记录原暂停计数且仅撤销自身增量），否则终止受控子进程而不detach让其继续。清除失败时同样终止、drain事件/确认退出/关句柄，逐项记录真实结果；进程已死则记“随线程销毁”，不伪造清零回读。每个pending事件恰一次Continue，成功自有单步用10002，其它异常80010001；清理结果、子退出与诊断结果分层，未采到post不得算成功。
设计可行性仅到Windows x86系统调试接口与现有布局/事件循环可承载；须新增SetThreadContext、拥有线程生命周期/状态机、硬件正常控制和失败清理后才能验证。当前没有已运行硬件断点能力、成对context、首坏写定位或产品修复；不得由本设计直接改生产调用约定。

### FZ阅读范围与交付边界
写前本报告旧1–40行已全部读完（截断24行已重读）；FX原件1–37与FV源码1–83、FH源码1–521（439–445缺段已补）、当前执行规则1–27均亲读。三共享本轮写前实读handoff 1–40及725–748、summary 1–32及726–740、issues 1–30及736–756；不继承FY任何未提供的阅读区间。预算内先交实际核证与设计，三共享中间历史未全文覆盖，不宣称全文读完，也不将其设成交付先决条件。写后已分段回读本报告1–71全部（16行措辞修正后另读15–16），三共享FZ新增节全部及末段已读；又补读当前handoff49–60、summary41–52、issues39–50。按前置8行换算后，本轮实际累计覆盖当前handoff1–60及733–756、summary1–52及734–748、issues1–50及744–764；未覆盖历史分别为61–732、53–733、51–743。以上是写前旧文与写后新文/补读的合并覆盖，不是三共享写后全文重读。本段登记随后再读。
源码default未改，CF/CX/CY/EF保留。Available/HW内容与resize/loss恢复、media/resources、线程callback、full最终四格、x64Event0/E_HANDLE及完整替换均未闭。无新运行或生产修复，不新增重复报告，不造数字退出码、source hash/PDB函数范围认证。

## 提取与有界分析已交付，文档闭环未完成
三共享历史全文本轮尚未连续读至末，不继承FV/FU阅读声明，不声称FW整体交接完成。

## 边界核证
已读执行规则27行、完整方案120行、项目37行/SDK配置10行/局部props11行、FV源码69行及FV/FT/FU关键证据。FP result-final/fp-symbols.txt21行本轮全读，只查询1351F0和1DE4D1，没有135570记录。原RSDS/MSF7 GUID e0a4d956-1efc-4509-83a3-b1664c206c82 age1匹配仅认证输入。FR记录LoadedPdbName空/GUID0/age0/SymType0，不能把旧SymFromAddr当实际加载完美认证。本轮无135570可核函数range/map；不再无限PDB加载，不重建候选/PDB，不指定GetUpdateMethod或其它方法名，不套当前FE源码/nearestline211，source checksum未证。
采用fallback：1351EB E880030000的下一RVA1351F0加380=135570，提供静态直接CALL目标边界，不认证动态执行入口或函数范围。[135570,135670)仅256字节唯一.text真实raw窗口，不是完整函数，不扫描RET模式猜结尾。

## 实际任务、包及身份
唯一只读task3dfca776fe764ab5be90226473d9525f，远端C:\CodingAgentSandbox\Tasks\3dfca776fe764ab5be90226473d9525f。回收artifacts/fw-create-callee/result/result/fv-pe.txt29行已全部亲读，另有callee-window.bin。上传package目录核仅正式WpfGfxShape.ComAcceptance.exe与input/wpfgfx_cor3.dll，无PDB/父目录/历史/obj，不故意删运行依赖，single-file包含自提取内容。
exe74236256字节，SHA B3CAF74E373BBCD32CD9542E0F6CC6CB2214835B3B8043C014EAE9371A3BA470，MVID613dbc89-16d0-4c6b-950b-98e0ca094726；实际.NET10.0.13/X64/PointerSize8。DLL3995648字节，实算SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53，非当前default产物；包必要文件合计78231904字节。CandidateLoaded=false，不提供本轮候选/PDB实际加载身份。
I386/PE32，ImageBase10000000/SizeOfImage3DE000/headers400。.text RVA1000/VirtualSize21C606/raw400/rawSize21C800。offset=400+135570-1000=134970，长度100，末offset134A70（hex），唯一.text raw/文件长度/image范围检查通过。bin SHA6538477605DFF8F7B10DA9949C8AA3219FCCE3BE8C877E9BF13BE741859BE7AF，逐16字节RVA/offset/长度/hex均见原件。
发布一次实报Exit0，正式net10.0-windows/Release/win-x64/self-contained，独有artifacts/fw-create-callee/obj/bin/publish/package；SDK配置10.0.107非工具独立实报版本。正常分析器无NoWarn，有既存MSTEST/CA警告；stdout38912前18912字符不可见，不称零警告或日志全审。沙箱工具只报完成及回收路径，数字进程ExitCode/独立监督码未提供；RoleResult=0为程序输出与源码return0对应，不冒称工具Exit0。非MTP，不造TRX/业务计数/业务清理证据。

## 连续有界x86手解（静态，非动态已走路径）
仅从直接CALL目标135570线性推进，不进入call/分支目标。RVA及立即数均hex；每组所列首尾以指令长度连续推进。
- 135570:55 PUSH EBP；571:8BEC MOV EBP,ESP；573:57 PUSH EDI；574:56 PUSH ESI；575:53 PUSH EBX；576:83EC44 SUB ESP,44。入口ESP=S时，旧EBP存[S-4]，新EBP=S-4；EDI/ESI/EBX存[EBP-4/-8/-C]，ESP=EBP-50（仅局部静态账本）。
- 135579:0F57E4 XORPS XMM4,XMM4；57C:0F1165C4、580:0F1165D4、584:0F1165E4为MOVUPS [EBP-3C/-2C/-1C],XMM4，写范围[EBP-3C,EBP-D]在分配帧内，不直接覆盖saved EBP/三saved寄存器槽。
- 135588:8BF1 MOV ESI,ECX；58A:8B0E MOV ECX,[ESI]；58C:8B7918 MOV EDI,[ECX+18]；58F:8D4DF0 LEA ECX,[EBP-10]；592:51 PUSH ECX；593:56 PUSH ESI；594:8D4DB0 LEA ECX,[EBP-50]；597:BA08000000 MOV EDX,8；59C:E8BF4E0A00 CALL1DA460；5A1:FFD7 CALL EDI；5A3:8BF8 MOV EDI,EAX；5A5:83C408 ADD ESP,8；5A8:8D4DB0 LEA ECX,[EBP-50]；5AB:E8E14E0A00 CALL1DA491。
- 1355B0:85FF TEST EDI,EDI；5B2:0F8CA6000000 JL13565E；5B8:BFCC1C3810 MOV EDI,10381CCC；5BD:837FFC00 CMP DWORD[EDI-4],0；5C1:0F859D000000 JNE135664；5C7:0F1007 MOVUPS XMM0,[EDI]；5CA:0F1145DC MOVUPS [EBP-24],XMM0；5CE:8B4DF0 MOV ECX,[EBP-10]；5D1:8B11 MOV EDX,[ECX]；5D3:8B3A MOV EDI,[EDX]。
- 1355D5:8D55EC LEA EDX,[EBP-14]；5D8:52 PUSH EDX；5D9:8D55DC LEA EDX,[EBP-24]；5DC:52 PUSH EDX；5DD:51 PUSH ECX；5DE:8D4DB0 LEA ECX,[EBP-50]；5E1:BA0C000000 MOV EDX,C；5E6:E8754E0A00 CALL1DA460；5EB:FFD7 CALL EDI；5ED:8BF8 MOV EDI,EAX；5EF:83C40C ADD ESP,C；5F2:8D4DB0 LEA ECX,[EBP-50]；5F5:E8974E0A00 CALL1DA491；5FA:85FF TEST EDI,EDI；5FC:7C60 JL13565E。
- 1355FE:8B0E MOV ECX,[ESI]；600:8B7924 MOV EDI,[ECX+24]；603:8D4DCC LEA ECX,[EBP-34]；606:51 PUSH ECX；607:56 PUSH ESI；608:8D4DB0 LEA ECX,[EBP-50]；60B:BA08000000 MOV EDX,8；610:E84B4E0A00 CALL1DA460；615:FFD7 CALL EDI；617:8BF0 MOV ESI,EAX；619:8D4DB0 LEA ECX,[EBP-50]；61C:E8704E0A00 CALL1DA491；621:85F6 TEST ESI,ESI；623:7C39 JL13565E。
- 135625:8B4DEC MOV ECX,[EBP-14]；628:8B55CC MOV EDX,[EBP-34]；62B:8B01 MOV EAX,[ECX]；62D:8B7054 MOV ESI,[EAX+54]；630:8D45C4 LEA EAX,[EBP-3C]；633:50 PUSH EAX；634:52 PUSH EDX；635:51 PUSH ECX；636:8D4DB0 LEA ECX,[EBP-50]；639:BA0C000000 MOV EDX,C；63E:E81D4E0A00 CALL1DA460；643:FFD6 CALL ESI；645:8BF0 MOV ESI,EAX；647:83C40C ADD ESP,C；64A:8D4DB0 LEA ECX,[EBP-50]；64D:E83F4E0A00 CALL1DA491。
- 135652:85F6 TEST ESI,ESI；654:7C08 JL13565E；656:8B75C4 MOV ESI,[EBP-3C]；659:8B7DC8 MOV EDI,[EBP-38]；65C:EB10 JMP13566E。其后顺序字节65E:33F6 XOR ESI,ESI；660:33FF XOR EDI,EDI；662:EB0A JMP13566E；664:E8E5FC0900 CALL1D534E；669:E959FFFFFF JMP1355C7。无条件跳转后文本仅为线性字节，不说同一路径执行。13566E只余837D，缺disp8/imm8，指令不完整立即停止，不猜尾声。

## 数据流与ABI局限
Create1351E8 MOV ECX,[EBP-14]后CALL135570，无为该直接调用新增stack参数；入口保存ECX到ESI并取函数表，支持寄存器输入的静态意图，不认证名称/managed ABI。Create返回后字节消费EAX/EDX，但本窗口未跟到RET，不能认证双寄存器返回/RET立即数/callee-saved恢复/最终ESP。
窗口直线除序言MOV EBP,ESP外无显式EBP寄存器写，见入口保存，但不证明内部call/间接输出不会改变帧或寄存器。局部地址-10/-14/-24/-34/-3C及transition记录-50只是静态地址，不证明callee真实写范围。
精确差异：第一/第二/第四间接call后ADD ESP,8/C/C，第三615 CALL EDI后没有立即ADD ESP,8；helper仍指向1DA460/1DA491。缺实际各vtable目标/合同及helper栈效应，不能把不同cleanup形态直接判为Stdcall/cdecl错误或物理清栈失衡。没有完整return与所有尾声，本轮未发现可静态认证ABI违约。
动态仍仅FT首次1351F0 EBP0/readFFFFFF68；静态正常邻接1351EB→135570→1351F0不证明动态必正常返回。无call前后context/返回槽，不指定首次坏写/EBP归零点，不因EBP0只认此callee破坏；继发unwind不是首因。

## 下一唯一动作及实现门
交可审最小窗口，停止本次任务。下一仅另授权现有FV只读角色将固定白名单窗口延至1356F0（总384字节），从已证13566E补全首条，追主合流/恢复/RET立即数，未知即停；仍核sameSHA/I386/PE32/.text raw及bin/hex/SHA，不加载代码。这针对明确缺失尾声，不是泛全direct/PDB循环。只有补齐后出现具体栈矛盾并与精确原版vtable合同/元数据互证，才有局部实现修复门；当前无此证据不修。现有FH无任意入口/返回事件，不虚构动态采集能力，不注入机器码/原生探针。

## 修改与开放门
仅既有全文件#if FV_PE_READONLY的FvPeReadonly.cs新增精确参数--fw-callee-window，拒其它参数、固定输入路径/256字节白名单/.text检查，无参数保留FV旧行为；其余为专属ignored Build.props及本报告/三共享。未改NativeRun/项目/打包/生产COM/DRDS/ExitDiagnostics/VisualCache/historical/捕获/WindowQ/CF/CX/CY。未定义FV默认全文件排除，默认行为不变。生产修复/生产发布/新崩溃/COM变体/full均0。Available/HW内容及resize/loss恢复/media/resources/线程callback/full最终四格/x64Event0/E_HANDLE均开放，完整替换未完成。

## 阶段历史
先创建本报告并前置handoff FW进行中；后核边界未认证，转.text真实raw限定窗口；唯一任务返回后即更新handoff提取完成/边界未认证，随后分析，历史保留。四文档全文回读尚未闭，不能以本报告初次成稿冒充交接结束。
