# FT 首次 AV x86 上下文取证

## FU 最后回读补记
本轮独立小段读完四文档全部正文与历史，超长截断均缩段补齐，不借FT/FR/FS旧阅读声明：初读handoff714行、summary722行、issues738行、FT18行；只前置新增FU节，原历史未改，新增内容全部写后回读，末尾再核最终handoff719/summary726/issues742及本报告34行。两任务debug事件195/589行与全部context/cleanup/identity/业务本轮已全文读至末。全文读取分布于写前历史及写后新增，不虚称三共享在写后又重读一遍全部历史。本补记随后再次读取。文档交接闭环；前驱EBP根因仍未闭，未新增运行/崩溃或生产修复。

## FU 独立核证终态（最新；覆盖下方阅读待办，不覆盖产品开放门）
### 原件、执行层级与布局
FU 未新增执行、构建、发布、候选加载、崩溃或业务/full；只更新本报告及三共享文档，历史全部保留。已独立亲读 control task f6eb783074ba48508cc0434eb33b348d 的 context-control.json 全131行、native-events.jsonl 全195行、cleanup.txt 全4行；candidate task47e74fa5e2bb40a3a33720fb13a3a1c0 的两份context各122行、native-events.jsonl 全589行、cleanup4行、dr-candidate/stages各6行、aot-identity1行。原件均在下方既有绝对本地/远端根；控制没有DR业务/候选actual加载记录，不制造它们。事件本轮连续小段读至末，不再继承FT事件中段待读缺口。
层级严格分开：taskID为传输/沙箱任务标识，工具数字任务退出码和独立stdout/stderr未提供，不能填0。控制监督PID10160/result0、子7588/Exit0；候选监督10672/result0、子9208/ExitC0000005。候选监督0只认证预期崩溃取证合同，不是产品通过；源码的candidate成功条件检查退出/secondchance/无timeout，不强制context有效，故上下文可信度须由JSON独立判断。两cleanup均Detach true/Error0、Wait0、两自有Close成功，事件无timeout-kill/continue-failed；候选secondchance1，最后退出事件TID7456不改变首次AV TID3196。
亲读当前 FhCrashEvidence.cs 全521行并补齐截断。来源是文件65–84行注明winnt.h i386的手写SDK布局，不是本轮读取外部已安装SDK头或独立运行Marshal.OffsetOf。按Sequential/Pack4字段逐项算：flags及6个DR共28；FloatSave=7*4+80+4=112，结束偏移140；4段寄存器至156；EDI156/ESI160/EBX164/EDX168/ECX172/EAX176；EBP180(0xB4)、EIP184(0xB8)、CS188/EFlags192/ESP196(0xC4)/SS200，ExtendedRegisters从204(0xCC)起512，合计716。函数显式分配716+15并对齐16，sizeof门为716/112，不是544字节结构，也不是自定义截断前缀。三份原件ContextSize716/Alignment0、请求及返回flags65543=0x10007（i386|CONTROL|INTEGER|SEGMENTS），Open/GetThreadContext/Close均true/Error0。未请求FLOATING_POINT/DEBUG_REGISTERS/EXTENDED_REGISTERS，不认证这些未选组。GetThreadContext返回BOOL、不返回读取字节长度；不得将内存读取总量当context实际长度。
RequestedBytes544是ReadProcessMemory请求预算：ESP256+EIP32+EBP64+六个通用寄存器各32=544；控制另null16为560。首次实际成功256+32+EBX32+ECX32+EDI32=384；其余四块299/0，未把零填充当读回。次份成功256+32+EBP64+EBX32+ESI32+EDI32=448，EAX/ECX/EDX各299/0。控制成功ESP256/EIP32/EBP64/ESI32/EDI32合416，其他读失败299/0。每块Hex仅序列化BytesRead有效前缀。首次EBP0不是由读空地址失败反推，而是成功context整数/控制字段；布局推导、有效控制及异常地址相互支持，未发现错位造成伪EBP0的证据，但无独立第二调试器/原始context缓冲认证。
### 首次访问与有界静态数据流
候选module-loaded事件498行、DR actual及aot identity均为SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53，base6E630000/size4055040；不重新计算或移绑当前default源码。首次异常事件550行Address6E7651F0，context.EIP1853247984相同，EIP读取Address亦相同，32字节成功，RVA1351F0。控制初始INT3异常地址775D8B88而context EIP为后一字节775D8B89，属控制现场事实，不能把此+1套到AV。
首条8B B5 68 FF FF FF：mod10/reg110/rm101，MOV ESI,[EBP-0x98]，EBP0使32位有效地址FFFFFF68，与ExceptionInformation[0,4294967144]完全吻合。只解释故障访问，不能解释EBP何时或为何归零。继发事件553/556行为1DE4D1 first/second，EAXFFFFFF7C及8B08=MOV ECX,[EAX]，匹配[0,4294967164]；旧UnwindStackFrame范围仅辅助，单列继发而非首因。
已回收首次窗口仅从EIP起，无EIP之前字节。由已知首指令边界可有限连续手解后续静态字节：+06 8D4E04=LEA ECX,[ESI+4]；+09 8901=MOV [ECX],EAX；+0B 895104=MOV [ECX+4],EDX；+0E 33C9=XOR ECX,ECX；+10 894D98=MOV [EBP-0x68],ECX；+13 6A02=PUSH 2；+15 8D4D98=LEA ECX,[EBP-0x68]；+18 8BD6=MOV EDX,ESI；+1A E831F5F2FF=相对CALL；+1F仅剩8B，指令不完整即停止。该31字节静态序列没有EBP赋值/恢复；但首次MOV已故障，绝不声称后续已执行、证明调用顺序或COM call、恢复native栈。它不能向前证明EBP保存/恢复/赋值与COM关系。
本轮未进行新PE提取：亲核现有FH modules角色固定ntdll RVA118B88，并非候选任意范围；FP角色只输出PE/CodeView和符号，未输出Create字节且认证失败提前返回，不假装现成支持提取。未新建Host/工程/巨大库/机器码/外部反汇编器，也未为缺边界越未知指令猜解。Create[134F70,135362)仅旧辅助；同DLL source checksum未认证，nearest cs211不是具体fault表达式。本轮只可信到缺陷访问，不声称完成前驱EBP根因追踪。
### 默认安全、发布日志及唯一下一动作
已亲读最后默认无诊断发布日志全部可用60行：Normal.props的win-x86 Release/self-contained10.0.13发布退出0、成功，最终两输出路径为ft-first-av/normal/bin与normal/publish；存在MSTEST0037/0044/0045/0017、CA2007/CA2014等警告，不称零警告。日志记录stdout38916字符，前18916已不可见、仅尾20000保留；没有可见错误，退出0不等于完整警告审计或产品验收。它不是FT显式诊断首次发布日志（后者仅保留FT历史声明）。Normal.props只导入本地Directory.Build.props及独有输出，不导入FO双符号；FH全文件及NativeRun两专属分派均#if FH_CRASH_EVIDENCE，其他入口不改。诊断保留为显式，生产COM、DR/DS、其他分派与CF/CX/CY本轮均未修改；默认安全是隔离边界，不是现有产品缺陷已修，未无故重build。
下一唯一可执行定位动作：在既有正式ComAcceptance的显式只读PE诊断内补一个限定提取分支（不是当前已有能力），仅File.OpenRead/PEReader，先核SHA1893370…980E53及I386/PE32，再一次提取旧辅助Create[134F70,135362)原始字节及节表PointerToRawData/VirtualAddress/SizeOfRawData，按raw+RVA-sectionRVA并逐段检查文件后备范围，核1351F0六字节与现场一致；从已知函数入口有限手解，未知指令立即停，仅记录可证EBP写/保存恢复及call边界。不得运行候选或泛符号重试。此为后续实现/一次纯只读验收动作，本轮未实施，不声称已获得入口前上下文或无侵入call返回事件能力。现有debugger没有任意函数入口/返回事件、无SetThreadContext/机器码断点能力，不虚构可用手段；未证具体Stdcall失衡指令，不给生产修复方案。
Available/native通知consumer、真实HW内容/resize/loss恢复、media/resources/线程callback、最后源码Tests/ComAcceptance×x86/x64 Release完整适用full四格及独立x64Event0/E_HANDLE均保持开放，完整替换未完成。

## FT 控制通过，候选取证前登记
publish一次Exit0，正常分析器输出前18902字符截断。复用FO离线预检，双符号/DR TestMethod/FH Run/Main存在，精确候选SHA及包清单PASS，自包含10.0.13。独有artifacts/ft-first-av obj/bin/publish/package。
控制task f6eb783074ba48508cc0434eb33b348d：GetThreadContext成功/Error0，CONTEXT716/Float112/16字节对齐，TID7508/EIP2002619273在ntdll；ESP256字节成功；null失败299/0字节。子PID7588 Exit0，监督10160结果0，cleanup4行成功。context131行已读，事件中段待读。TestSHA0CB47F645005B02BFE1E018E4F676B1247E43849ADD6595E8BA7B438B5A52765/MVIDb0aedd26-bfef-4937-a5db-09d9182bc980。
控制本地根d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\ft-first-av\run-control\Results\fh-control，远端C:\CodingAgentSandbox\Tasks\f6eb783074ba48508cc0434eb33b348d\Results\fh-control。下一最多一次candidate。

## FT 唯一候选实证已回收（待收口回读）
三个缺参数工具调用Function failed，未返回任务；随后唯一完整candidate提交task47e74fa5e2bb40a3a33720fb13a3a1c0完成。子9208/TID3196，监督10672。first AV 6E7651F0/RVA1351F0 readFFFFFF68，后继6E80E4D1/RVA1DE4D1 readFFFFFF7C first/second均NOT_HANDLED。ExitC0000005/监督结果0仅取证命中，CleanupConfirmed true、cleanup4行全成功，无Terminate。无完成TRX，不造计数。
两份context各122行已亲读；GetThreadContext均成功/Error0/Close成功；每份请求544字节、ESP256和EIP32全部成功，首份EBP/EAX/EDX/ESI空地址读失败299/0，次份EAX/ECX/EDX失败299/0；其余小块有效长度见JSON。首份EIP1853247984、ESP51888132、EBP0/EAX0/EDX0/ESI0、EBX51888716/ECX51888024/EDI51888784/EFlags66118。段CS35/SS DS ES GS43/FS83。次份EIP1853940945/ESP51886860/EBP194680016/EAX4294967164/EBX194679820/ECX156/EDX3/ESI51887408/EDI51887360/EFlags66195，段同。
候选模块base1851981824/size4055040，DR actual与候选SHA一致；测试SHA/MVID及.NET10.0.13与控制一致。业务/stages各6行及identity已亲读，合法surface Hr0后Create before，无after，无channel/Present。
实际EIP字节首6为8B B5 68 FF FF FF。手工按x86编码：8B为MOV r32,r/m32，ModRM B5=mod10/reg110(ESI)/r-m101(EBP)，disp32 FFFFFF68=-152，所以该具体指令是MOV ESI,[EBP-0x98]；采得EBP=0，32位有效地址FFFFFF68，与异常read参数吻合。这是原始指令字节和寄存器的手工解码对应，JSON的InstructionDecoded=false表示采集器本身无解码器，不是外部反汇编结果。后继首字节8B08可手工解为MOV ECX,[EAX]，EAX=FFFFFF7C对应继发读地址。不把继发作为首因。EBP在首次指令前已为0已直接观测，但未证何时/为何变0，不认证ESP平衡、C#语句、局部变量、首坏写或整个库bug。旧PDB范围仅辅助，源checksum和实际加载认证仍未闭。
本地根d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\ft-first-av\run-candidate\Results，远端根C:\CodingAgentSandbox\Tasks\47e74fa5e2bb40a3a33720fb13a3a1c0\Results。stdout/stderr和工具数字退出码未独立提供，不编造。事件589行当前仅首2、548–558、582–589已读，全文历史交接亦未闭，不称已完成。

## FT 进行中（历史；初始日期登记误为10-17，实际工具UTC10-23）
2026-10-17：承接 FO 精确 FE 候选 SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53；只增强 FH_CRASH_EVIDENCE 显式诊断，不重建候选、不运行 COM 变体/full。必须先通过正常退出调试控制，再最多一次 DR Create 崩溃取证。寄存器不证明 ESP 平衡或首坏写；未解码不得推断指令。

已开始读取交接；结构查询首次工具失败，目录读取成功。当前无构建/发布/沙箱运行，未取得 FT 上下文或退出结果。完整规则/方案及历史阅读尚未完成。
