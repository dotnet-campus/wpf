# GU：QI helper 栈变化定位

## 进行中
本轮只读预算最多1次，同SHA候选硬件采样最多1次；尚未消费。先核执行规则、完整方案、GT/GR原件及FV/FH隔离设施。候选SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53；helper RVA1DA460，CALL1355E6，返回1355EB。不得预先归责helper或全库ABI。

只允许FV固定白名单只读扩展、FH隔离诊断及必要角色、本文与三共享文档。生产修复0；不重建候选、不改COM参数、DRDS、ExitDiagnostics、VisualCache、历史捕获、打包合同、CF/CX/CY。Available/HW恢复/full四格/x64Event0全部开放。

## 证据登记
目前仅已读最新handoff GT节；没有本轮task、提取或动态实证。历史结论不是本轮新证据。

## GV最终补记：唯一候选已执行，ESP保持而参数槽被覆盖
以下为最新事实，覆盖上方GU进行中登记；GU历史原文保留。GV已先在handoff登记接续，无新重复报告。GU同SHA只读原件readonly-final/result/fv-pe.txt全21行已读；GU报告当时尚未回填该提取及正式包事实，本节补齐，不把GU提取当GV新运行。

### 身份、预算和实际承载
唯一一次实际候选：正式source为d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\gu-qi-helper\com-acceptance-package，含Native、Contracts、aot.sha256及自包含runtime；非裸publish或回收根。直接既有WpfGfxShape.ComAcceptance.exe --fh-crash-evidence candidate，外预算240秒；FH既有BL式调试父/DR子分层，合法DR首个Create，无channel/Present，不重跑只读/控制/full/COM变体。候选预算1/1消费，原DLL未重编译、阶段间未重建；源码与生产修复均0。
实际task97a2f688101e40959ca6b9b31da6c98d，远端根C:\CodingAgentSandbox\Tasks\97a2f688101e40959ca6b9b31da6c98d；完整回收本地artifacts/gv-qi-helper/candidate。actualNative绝对路径C:\CodingAgentSandbox\Tasks\97a2f688101e40959ca6b9b31da6c98d\Native\wpfgfx_cor3.dll；映像base6D390000/1832452096，Size4055040，文件3995648字节。
Candidate SHA256=1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53；test DLL SHA256=0325C47F422B57F80633F393B58A6578DCEC349E1626B826AACAB72710E5E4E8，MVID51da9a05-9163-44c2-b600-e5a88949e0bd；.NET10.0.13/X86/指针4。监督PID3052、子PID10308、采样native TID11128。包预检及此次内置preflight均Verified=true/CandidateLoaded=false，身份一致；exe apphost SHA EA7BEC34…不是test DLL SHA，不混用。
句柄1232同步读取3995648字节，Error0；前后Volume2759389505/Index8192000:294837/大小/写时一致，文件位置0恢复成功，hFileSHA与actual path SHA一致；candidate-authenticated成功意味着PE I386/PE32、映像大小、稳定头字段及五处原字节门全部通过。retained-file-disposed IsClosed=true。没有门失败或借用旧F。

### 动态F认证与前后寄存器
1351EB实际call字节E880030000，caller S=50446604/P=50446756；135573入口EBP=ESP=F50446596=S-8，[F]=P50446756，[F+4]=1833718256=6D4C51F0，认证成立。
两次硬件命中均PID10308/TID11128/Phase3：前EIP1833719270=6D4C55E6，后1833719275=6D4C55EB；F=EBP50446596、ESP50446512=F-54、EDI1854358368=6E874360完全相同，真实ESP差0，不由普通RET静态推定。
寄存器前→后：EAX1836129484→192940504；EDX12→50446512；EFLAGS647→518（随后清DR读回516）。EBX50447212、ECX50446516=ESP+4=F-50、ESI193199584、EDI1854358368、EBP与ESP均保持。FullStackProven=false，不声称完整unwind或整个库ABI认证。
EDI两次均等于factory vtable slot0，this192955968=[F-10]，vtable1853631624；actual d3d9为C:\Windows\SysWOW64\d3d9.dll/base1853620224=6E7C0000/Size1548288/SHA256 F4757445D75D619BC5695294EE5CA6BFA0AD8763C6604A8E884739F605A6DF28；入口32字节8BFF558BEC83EC0C53568B75088BCEF7D9571BC98D460423C86A00518D4DF4E8。

### 真实参数、固定窗口及有效读取
两次[ESP,ESP+20)均Requested32/BytesRead32/Succeededtrue/Error0（这里20为hex）。原字节：
前4046800BE0C00103F0C00103D809800B00800000ACC001030000000000000000；
后4046800BEB554C6D04C10103D809800B00800000B0C001030000000000000000。
this保持192955968；riid50446560=F-24→1833719275=6D4C55EB（helper返回地址）；out50446576=F-14→50446596=F。前参数正确，后参数错误；不是ESP偏移，也不是准备阶段已经错误。
前riid读取16/16/Error0为41721702FC690C408FF193A44DF6861D；前out读取4/4/Error0为00000000。后actual-riid读取16/16/Error0为FFD78BF883C40C8D4DB0E8974E0A0085（CALL代码）；后actual-out读取4/4/Error0为A4C10103（固定F仍保存P50446756）。固定原GUID及原out槽未改变，变的是传入指针。
两次固定[F-60,F+8)地址50446500/Requested104/BytesRead104/Succeededtrue/Error0，完整hex：
前CC1C716DCC1C716D04C101034046800BE0C00103F0C00103D809800B00800000ACC0010300000000000000000000000000000000000000000000000041721702FC690C408FF193A44DF6861D000000004046800B6CC3010300000000B8C30103A4C10103F0514C6D；
后CC1C716DCC1C716DEB554C6D4046800BEB554C6D04C10103D809800B00800000B0C0010300000000000000000000000000000000000000000000000041721702FC690C408FF193A44DF6861D000000004046800B6CC3010300000000B8C30103A4C10103F0514C6D。
固定窗变化DWORD：F-58原F→返回6D4C55EB（CALL返回槽）；F-50原riid→返回6D4C55EB；F-4C原out→F；F-40原50446508→50446512。this、[F]保存P及[F+4]返回未变。
GU helper窗口128字节SHA0DBF1449537D7AC27ACFB4583C051C88481A59769B92251390F44CEDA18D0F16，RVA1DA460开头A1AC153910648B152C0000008B048205180000008B142489410889690489118D542404C7410C00800000895110894830C3。普通RET无立即数、无显式ESP修改不代表无参数写入：窗口的MOV [ECX+4],EBP、MOV [ECX],EDX及LEA EDX,[ESP+4]/MOV [ECX+10h],EDX与本次ECX=F-50、返回地址和F覆盖riid/out的动态值精确相合；MOV [ECX+8],EAX及[ECX+C]等其余写不必数值改变。窗口不是完整函数边界认证，不由此扩判全部helper/全部库/系统COM责任。

### 状态机、异常和退出
1351EB→135573→1355E6→1355EB仅同认证TID的四次0x80000004各Continue一次65538；各阶段Set及读回Verified=true。helper前将slot0从E6改EB；后DR0/1/2/3/6/7全0，cleared-readback成功，qi-clear-confirmed，Owned=false后不伪造线程销毁读回。未改实参/Ex/Release/生产逻辑，未插机器码INT3、外部调试器或新Host。
真实异常：1351F0第一机会AV EBP0/ESP=S50446604；context-av-1两段全122行有效，EIP1833718256，空EBP/EAX/EDX/ESI读0字节/Error299。第二context全122行有效，EIP1834411217=6D56E4D1、ESP50445332、EBP194288112；空/非法EAX4294967164、ECX156、EDX3读取0/Error299，其余按各原字段记录。两次first AV及最终second chance AV均Continue NOT_HANDLED2147549185，仅一次/事件，没有吞异常续跑假成功。
实际EXIT_PROCESS_DEBUG_EVENT Kind5已收，子Exit3221225477=0xC0000005，TimedOut=false，SecondChanceExceptions1，DumpBytes0；监督supervisor-result ExitCode1/CleanupConfirmed=true。cleanup全4行：Detach=true/Error0/ExitEventtrue，WaitForExit0/Error0，线程句柄3CC与进程3C8关闭成功。专用执行返回只确认结果回收，未给原始工具码，不造工具0。此为FH诊断非完成MTP验收，无完成TRX、不造测试计数。

### 默认隔离验证及唯一下一动作
执行后仅默认Release publish一次，目标现有ComAcceptance.csproj，显式PublishDir=d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\gv-qi-helper\normal-publish\；未传GU/FH/FV/DR DefineConstants、未导入GU Build.props、未改TFM/SDK/NoWarn/分析器。退出0，默认局部构建隔离校验通过；正常CA/MSTEST分析器仍报现存警告，不宣称零警告。60行专用日志1–30及31–60均回读；stdout原38764字符，前18764字符已工具截断，不能声称完整原stdout或准确警告总数。发布不执行任何业务，不重建固定候选。普通发布公开API检查DrInteropAbiTests为0（该类本为public，支持DR隔离）；FH为internal，公开API过滤0不单独当作其不存在的证明，源码#if FH_CRASH_EVIDENCE与GU嵌套、默认无专用常量的构建路径共同确认隔离保持。
下一唯一根因动作：先离线关联本SHA helper写transition-frame合同与1355DE LEA ECX,[EBP-50]这一frame布局/生成来源，核实为何transition记录区F-50与已压入QI riid/out槽重叠；优先确认该callsite的编译器/GC transition帧布局，不再回到准备阶段前、不重跑本预算，也不据此直接修全部库/归责d3d9。只在取得该生成合同后讨论最小修复。
生产修复0；生产COM/DRDS/NativeRun其它入口/ExitDiagnostics/VisualCache/historical/捕获/打包/CF/CX/CY全部未改，FH/FV默认隔离保持。Available/HW恢复/full四格/x64Event0仍开放，完整替换未完成。
本报告全文与三共享最新受改节在写入后回读；长历史保留未全文。最终补记作为本节终态，不以历史进行中或默认发布冒充产品通过。

## GW独立终态：连续写合同已证；生成合同认证仍缺
本轮只读及文档，已先登记handoff进行中。没有运行、提取、构建、发布、脚本、生产或测试代码修改；不新增报告。以下补充覆盖“helper内部未知/只相合”的历史判断，但不覆盖原始实跑事实。

### 独立参数变化与证据覆盖
亲读GV task97a2f688101e40959ca6b9b31da6c98d的hardware全57行（长段截断后缩小补齐10、16–18、24、31–36、41、48–57等）、context-av-1/2各122行、file-identity6行、cleanup4行、preflight1行、aot-identity1行、dr-candidate6行。native-events亲读1–3、625–630、704–728（719–728补截断）、745–757；未称757行全文。actual模块LOAD_DLL及hFile记录不是预期路径替代：base6D390000/4055040，SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53；test0325C47F422B57F80633F393B58A6578DCEC349E1626B826AACAB72710E5E4E8/MVID51da9a05-9163-44c2-b600-e5a88949e0bd/.NET10.0.13/X86。
同PID10308/TID11128，动态F50446596、P50446756、S50446604经1351EB→135573认证。1355E6前到1355EB后EBP=F、ESP=50446512=F-54、ECX50446516=F-50、EDI1854358368不变；this192955968不变，riid F-24→6D4C55EB，out F-14→F。参数32字节和固定[F-60,F+8)104字节完整/Error0，覆盖所有参数槽、全部五个frame DWORD及保存F/ret；前后完整hex保留上节。固定GUID和原out局部未变，指针槽实际改变，非ESP错位。快照是执行断点取样，不是参数写监视事件，不认证全局首坏写或排除其它线程。
实际DR全清及读回、四次命中Continue各一次、三次真实AV NOT_HANDLED、EXIT_PROCESS Kind5、子C0000005/监督1/CleanupConfirmed=true/非timeout均亲读；无完成TRX，不造通过数。GH报告30–46行关联同SHA外层CALL返回EBP0/ESP相等，但其本轮所试原件路径不存在，不能冒称GH原件亲读。GR报告1–26及87–119、GQ hardware24原件亲读：write4后F保存P→0/ret不变、OtherThreadsExcluded=false，系统边界仍未认证；该后续零写与本轮指针覆盖不是同一个事件。

### 同SHA128字节连续解码与写覆盖表
GU readonly-final/result/fv-pe.txt全21行亲读，输入同SHA，RVA[1DA460,1DA4E0)128字节，slice SHA0DBF1449537D7AC27ACFB4583C051C88481A59769B92251390F44CEDA18D0F16；未重提取或重算SHA。GM readonly-result/result/fv-pe.txt全37行亲读，callee384字节同SHA/slice F6EF760CA4ADA31AAE1C4E4F33F010A70528FB3C319396FD6735CB551A4356F0。以下地址/偏移均hex，从实际direct-call目标前向连续解码至RET，不反向猜边界。
1DA460 A1AC153910：MOV EAX,[103915AC]（映像首选VA，运行时重定位绝对项）；465 648B152C000000：MOV EDX,FS:[2C]；46C 8B0482：MOV EAX,[EDX+EAX*4]；46F 0518000000：ADD EAX,18；474 8B1424：MOV EDX,[ESP]。至此EAX来自TLS索引寻址再加18，EDX为helper调用返回地址；不凭形状给TLS对象命名。

|写指令RVA/原字节|实际语义/数据来源|ECX=F-50时目的与相交|GV前→后DWORD|
|---|---|---|---|
|1DA477 / 894108|MOV [ECX+8],EAX，TLS寻址结果|F-48；不交三参数槽|D809800B→D809800B（同值写）|
|1DA47A / 896904|MOV [ECX+4],EBP，当前EBP=F；不是加载[F]保存的P|F-4C；完整覆盖out指针槽|F-14→F|
|1DA47D / 8911|MOV [ECX],EDX，来自helper入口[ESP]的返回地址|F-50；完整覆盖riid指针槽|F-24→6D4C55EB|
|1DA483 / C7410C00800000|MOV DWORD PTR[ECX+C],8000|F-44；不交三参数槽|00008000→00008000（数值hex）|
|1DA48A / 895110|MOV [ECX+10],EDX；47F的8D542404已LEA EDX,[ESP+4]|F-40；存helper调用前SP=F-54|50446508→50446512|
|1DA48D / 894830|MOV [EAX+30],ECX；EAX为上述TLS寻址结果|不是ECX相对栈写；发布F-50到EAX+30|未读该目标，不造快照值|

1DA490 C3为普通RET，无立即数。ECX本身不改；EDX从入口[ESP]改为入口ESP+4；ECX相对写集合[0,14)，共20字节。this槽[F-54,F-50)与该集合不交，riid[F-50,F-4C)、out[F-4C,F-48)各完整相交。helper入口ESP=F-58，CALL自身把6D4C55EB压入F-58；不是把这个CALL压栈写误算helper记录写。记录的savedEBP=F不同于[F]=P。静态次序先覆盖out（47A），后覆盖riid（47D），与前后值逐项对应；这不是实际逐指令写事件认证，更不是进程首坏写。
128字节同时包含后续入口：491 8B5108，494 C7423000000000，49B 833DB0B4381000，4A2 7501，4A4 C3，4A5 E966180000（到1DBD10），4AA–4AF为CC填充；4B0开始另一序言。本轮不扩展这些目标或借此认证全函数边界；剩余字节不是1DA460已走RET路径，不自造helper名称。

### callsite及动态重叠机制分层
1355D0 F0是前一条MOV ECX,[EBP-10]的位移尾；1355D1 8B11取表、D3 8B3A取slot0。D5 8D55EC为LEA EDX,[EBP-14]，D8 52压out；D9 8D55DC为LEA EDX,[EBP-24]，DC 52压riid；DD 51压this。DE 8D4DB0确为LEA ECX,[EBP-50]；E1 BA0C000000设置EDX=C；E6 E8754E0A00直达1DA460（1355EB+0A4E75）；EB FFD7才CALL EDI，1355E8在E8立即数内。没有把D5/D9地址误说成PUSH指令地址。
序言575653/83EC44使正常固定分配底为F-50，三push理应使ESP=F-5C，参数位于[F-5C,F-50)，与记录[F-50,F-3C)相邻不交。实际进入QI helper前ESP=F-54，比此静态基准高8，三参数变成[F-54,F-48)，于是后两个槽落进记录。不能把ECX=F-50本身直接判成frame尺寸不足；当前已证的是动态outgoing布局上移后重叠。
前一slot6序列13558F 8D4DF0取F-10、135592 51压F-10、135593 56压ESI、13559C同helper、1355A1 CALL EDI，1355A3 8BF8取返回值，1355A5 83C408为ADD ESP,8，再1355AB调用1DA491。若目标依COM Stdcall已清8而生成调用方又ADD8，恰好解释此处净上移8；但前序实际返回/生成calli合同及适配源checksum未认证，不能将该条件推理写成已证双清栈。该helper从入口到RET不修改ESP（除CALL/RET配对），入口EDX=C被覆盖而未用于参数大小；它不是可凭EDX猜测的清栈补偿器。

### 生成合同认证层级、修复评审与唯一后续
L1原件身份、L2连续机器指令读写语义、L3同线程端点参数与重叠：已证。L4精确runtime helper符号/字段布局、L5同SHA原始IL/calli签名及生成源checksum/编译器版本：未证。本工作区src、WpfGfxShape及本机.dotnet对RhpPInvoke/PInvokeTransitionFrame有限内容检索无命中；TransitionFrame/ILCompiler名称检索无结果，WpfGfxShape无project.assets.json/.map/wpfgfx_cor3.pdb。GU范围只有测试侧PDB等，不能当候选native helper命名证据。没有读取到匹配该候选编译runtime版本的真实实现。现有global.json的SDK10.0.107及当前net10.0项目不证明旧FE使用的NativeAOT编译器版本；监督进程.NET10.0.13同样不证明编译器版本。官方NuGet公开托管API也不能替代runtime私有x86汇编及codegen合同。故本轮只称“RVA1DA460 helper，具有TLS关联frame记录初始化语义”，不正式命名为PInvoke transition frame初始化。
责任边界：候选二进制caller与frame helper在实际QI点不兼容已证；究竟适配源声明/calli错误、NativeAOT调用约定降低/清栈生成错误，还是更早frame/栈生命周期破坏，尚未认证。不能怪d3d9或整个Silk库。FE为已撤失败适配旧产物，非当前default；没有其源checksum绑定，不能把当前源码按旧机器码直接生产修复。
最小评审方案（不是本轮实施）：只针对同SHA前序slot6至QI的生成边界，先核Stdcall/调用方清栈与transition frame存活区；若证实源calli声明不符COM则只修该边界并补相邻调用审查；若源/IL正确却生成多余清栈则修复/更换经认证的编译器生成路径，不用扩大frame、挪ECX、SuppressGCTransition、手补ESP或泛全direct掩盖。评审不批准直接改默认。后续任何候选须x86/x64 Release独立DR/生命周期/BWCK回归及实际SHA身份，业务HW恢复与full仍独立。
下一唯一可执行认证动作：恢复同SHA FE的“生成合同证据包”——绑定源文件checksum/PDB来源、原始方法IL及各calli签名，精确NativeAOT compiler/runtime包版本/提交及x86 helper与frame/codegen对应片段，限定前序slot6清栈→QI frame布局这一边界。该包当前缺失；不重复helper前后采样、不继续泛全direct、不另跑崩溃或COM变体。源/IL与生成汇编绑定后才能选择生产边界修复或编译器修复。

### GW写后与历史覆盖登记
规则27行、完整方案120行均分段亲读；GU报告原47行含GV完整读取，新增GW逐段写后回读；三共享只回读最新受改节/最后补记，历史保留未全文。GH仅报告关联、原件缺失明确登记，GR按上述范围及GQ关键原件核证，不冒称历史全覆盖。仅本文及handoff/session-summary/issues四文档变化；生产COM/FH/DRDS/NativeRun/打包/CF/CX/CY均不改。默认隔离保持、生产修复0，Available/HW恢复/full四格/x64Event0开放，完整替换未完成。
