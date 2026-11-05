# GS factory QI CALL前真实参数取证

## GT独立终态（覆盖下方GS初稿；生产修复0）
本轮独立读取GS taskab5dcfc36f6d475e84ed452136d3da95既有产物，不重新运行候选；先登记handoff进行中。错误实参已在1355EB CALL EDI之前形成：this正确匹配factory，riid指向调用点机器指令字节，out指向固定savedP槽F。不是d3d9将正确out改为F；ABI/失衡的形成机制仍未证明，不由Cdecl元数据推定全库bug。

### 原件、实际身份和读取边界
工作区相对原件根：artifacts/gs-qi-arguments/candidate-result；以下Results/fh-candidate/下hardware.jsonl共40行（分段读取，长行10/17/37另单行补读）、file-identity.jsonl全6行、context-av-1/2各全122行、cleanup.txt全4行、dr-candidate.jsonl全6行，另Results/aot-identity.json全1行已亲读。native-events.jsonl仅1–3、633–636、710–726可见异常段、741–754终态；710–728请求在727附近超长截断，未称754行全文。没有独立名称actualidentity或writecontext文件，不造文件。GS执行工具原始返回码不在已读文件中，不能把“沙箱完成”或监督码当工具码，也不臆造0。
监督PID2748/子6764/受监视TID10712，.NET10.0.13/X86/PointerSize4；testSHA E29748F01C15705C4464DCC8B5BADFBFC0ECC01927FDEF37C017F403589F7EDD，MVID779c1487-85f4-488e-ad3f-1a21e9b51577；PresentationCore程序集10.0.0.0。候选actual路径C:/CodingAgentSandbox/Tasks/ab5dcfc36f6d475e84ed452136d3da95/Native/wpfgfx_cor3.dll，base1832452096=6D390000，Size4055040，actualSHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53；不是使用当前默认build的测试身份代替GS包身份。
LOAD_DLL句柄1180：Volume2759389505/Index2686976:310846/Size0:3995648/Write31285785:3967683863前后一致；同步请求/实读3995648成功、Error0；pointer0保存恢复0，handle-sha同上。此为独立读记录，未重新计算DLL散列。CreateDevice/CreateRenderTarget/ColorFill均Hr0，Create before后未完成业务。

### 动态CALL前参数（RVA/偏移/字节按hex，裸地址数值按十进制）
1351EB调用前P=49989540、S=49989388；135573序言后实际EBP=ESP=F=49989380=S-8，[F,F+8)=A4C7FA02F0514C6D，保存P与return6D4C51F0认证。断点转换1351EB→135573→1355EB均同TID，DR6低四位1、slot0，Set/Get读回一致。
1355EB实际EIP1833719275=6D4C55EB，EBP=F、ESP49989296=F-54，EDI1854358368=6E874360。CALL前32字节原件：E04A6A0BEB554C6D04C7FA02E897690B00800000B0C6FA020000000000000000。严格按x86 stdcall入call前参数，而非把CALL将压入的返回地址算已有参数：

|实际槽|DWORD|独立读取及比较|
|---|---|---|
|[ESP]|191515360=0B6A4AE0|this，与[F-10]49989364处E04A6A0B的factory相等|
|[ESP+4]|1833719275=6D4C55EB|riid不是F-24=49989344，而是CALL指令地址|
|[ESP+8]|49989380=02FAC704|out=F，不是F-14=49989360|

pointer-read各请求/实读4，Succeeded=true/Error0；this所指vtable=1853631624=6E7C2C88，slot0=1854358368，factory独立读取同表/同slot，与EDI一致。
actual-riid-GUID16请求/实读16成功/Error0，内容FFD78BF883C40C8D4DB0E8974E0A0085，是调用点代码而非GUID；actual-out-before-DWORD请求/实读4成功/Error0，A4C7FA02仍为P49989540，尚未清零。
固定完整窗[F-40,F+8)起49989316，请求/实读72成功/Error0：B0C6FA0200000000000000000000000000000000000000000000000041721702FC690C408FF193A44DF6861D00000000E04A6A0B6CC9FA0200000000B8C9FA02A4C7FA02F0514C6D。其[F-24,F-14)可见预期GUID原字节41721702FC690C408FF193A44DF6861D，但实际riid并未指向它。actual-call-stack请求/实读32、EDI-entry请求/实读32均成功/Error0；不能用请求长度冒充实读或把AV中Error299零字节读当内容。
actual-EDI-entry：8BFF558BEC83EC0C53568B75088BCEF7D9571BC98D460423C86A00518D4DF4E8。EDI所属实际C:/Windows/SysWOW64/d3d9.dll，actualbase1853620224=6E7C0000、Size1548288，SHA F4757445D75D619BC5695294EE5CA6BFA0AD8763C6604A8E884739F605A6DF28；hardware模块记录和native-events第635行互证，不将候选SHA套给系统模块。

### 同SHA静态准备及helper：已核与未核
亲读artifacts/gm-saved-write/readonly-result/result/fv-pe.txt全37行；输入SHA同候选、I386/PE32、.text RVA1000/raw400，callee[135570,1356F0)384字节/file offset134970，slice SHA F6EF760CA4ADA31AAE1C4E4F33F010A70528FB3C319396FD6735CB551A4356F0。只是现有原件认证链，不是本轮重提取/独立重算SHA。
135570 PUSH EBP；135571 MOV EBP,ESP；135573 PUSH EDI/PUSH ESI/PUSH EBX及SUB ESP,44，形成静态局部基线F-50。此前GetDirect3D：135593附近LEA ECX,[EBP-10]/PUSH ECX/PUSH ESI，经13559C helper后1355A1 CALL EDI，1355A5 ADD ESP,8，1355AB又call helper；这段没有helper内部返回约定证据，不能把其后ESP无条件等同F-50。
1355CE MOV ECX,[EBP-10]、1355D1 MOV EDX,[ECX]、1355D3 MOV EDI,[EDX]；1355D5 LEA EDX,[EBP-14]，1355D8 PUSH EDX(out)；1355D9 LEA EDX,[EBP-24]，1355DC PUSH EDX(riid)；1355DD PUSH ECX(this)。三次push使ESP净减C，静态操作数明确是factory/F-24/F-14；1355DE LEA ECX,[EBP-50]，1355E1 MOV EDX,C。
1355E6 E8 754E0A00：有符号rel32为+0A4E75，下一RVA1355EB；逐位加算1355EB+0A4E75=1DA460（低位55EB+4E75=A460，无低16位进位；高位13+A=1D）。1355E8在该CALL的四字节立即数内部，不是指令边界。
该direct CALL另压入1355EB返回地址，ESP再减4；正常返回须消耗此返回地址，是否RET imm16、是否搬移实参/保存返回地址，现有384字节不覆盖1DA460内部，现有FV角色只支持既定callee/create窗而非任意helper窗。本轮不改只读角色、不执行提取，也不认证PDB名称。可证层级是“三参数已按上述LEA/PUSH准备，然后进入helper；helper返回后、COM CALL前实际栈已是factory/callsite/F”。不能据此断言helper自身改坏参数；更早GetDirect3D及helper返回后的ESP同样未采。
1355EB FFD7将再压1355ED；真实QI为this/riid/out三个DWORD，共C字节，正确stdcall返回通常消耗返回地址4及参数C。1355ED MOV EDI,EAX、1355EF ADD ESP,C、1355F5 E8 974E0A00→1DA491也已见，但QI实际RET立即数和两helper真实返回约定未读，不能单凭ADD ESP,C推出双清栈。局部基线F-50减C为F-5C，而实际F-54差8，仅是相对名义基线的候选偏移；不能将此前8字节COM调用与ADD ESP,8直接宣布为根因。

### 事件、退出、清理层级
采完后hardware35–38显示DR0/1/2/3/6/7全部0且读回一致、qi-clear-confirmed Captured=true/ContinuedRealPath=true；未改实参或跳业务/cleanup。后续thread-destroyed-not-readback注明WasOwned=false，不冒充额外清零读回。
context-av-1有效GetThreadContext：1351F0返回处EIP1833718256、ESP=S、EBP0，异常读取FFFFFF68；context-av-2 EIP1834411217/RVA1DE4D1、EBP192785368/ESP49988116，随后同址second-chance，读取FFFFFF7C。两context全读；无效地址读取均Error299/BytesRead0，InstructionDecoded/StackBalanceAuthenticated/FirstBadWriteAuthenticated仍false。GS不是write4任务，不冒充新首坏写实证；GQ写F事实独立保留。
native-events异常Continue三次NOT_HANDLED均成功；Kind5 EXIT_PROCESS_DEBUG_EVENT已收。completed：子ExitCode3221225477=0xC0000005、TimedOut=false、SecondChanceExceptions1、ExitEventCollected=true、DumpBytes0。子并非正常退出0；这是未改路径最终崩溃，不是“正常控制”执行。supervisor-result实际ExitCode1/CleanupConfirmed=true，不是0，也不是产品通过。FH源码1313–1319的监督门含hardware.Complete，本轮不为改结果而修门；没有把1抹为取证成功0。
cleanup：Detach=True/Error0/ExitEvent=True；WaitForExit=0/Error0；CloseHandle0x3D0和0x3CC均Success=True/Error0。工具执行状态/工具返回码、监督结果1、子C0000005是三个层级，原始工具码未读到则不报数字。未见完成TRX，不造测试数。

### 默认隔离build与交接
当前FhCrashEvidence.cs首行#if FH_CRASH_EVIDENCE；NativeRun14–20及DrInteropAbiTests中相应入口也受条件隔离；默认项目net10.0-windows/Nullable enable/global SDK10.0.107、Directory.Build.props/targets已核。本轮没有源码修改，生产COM、DRDS、CF/CX/CY、ExitDiagnostics/VisualCache/historical/捕获/打包/版本不改；不新增Host/项目/脚本/机器码或外部调试器。
仅一次正常Release build正式ComAcceptance.csproj，无NoWarn、无关闭分析器、无诊断Build.props/DefineConstants注入、无publish。结果exit0，98警告/0错误，2.97秒；保留CA2007/MSTEST及CA2014等现有警告，不顺手改其它入口。最后构建日志共64行已1–64回读；工具原始stdout77103字符仅保留末20000字符，前57103字符不可见，日志第9行从片段开始。不是77103字符全文日志，也不称零警告。新候选执行/候选重建/COM变体/提取/full均0。
下一唯一有根据动作：针对同SHA RVA1DA460实际调用前后context（含ESP和三参数/返回槽）或认证该helper真实返回约定，补齐“准备→helper→CALL前”缺口；不泛替整个COM闭包，不先修生产。Available/HW恢复/full四格/x64Event0全部开放，完整替换未完成。
阅读登记：规则27行、完整方案1–120（首段截断后55–89及90–120补读）、当前基线20行；GL报告GR节关键1–48，长段截断如实保留，不称全117行本轮重读。三共享只读/更新最新节，长历史保留未全文。GT最后补记：报告写后1–49行已分段全文回读；handoff1–7、session-summary1–7、issues1–7最新受改节（含最后补记）已回读。共享长历史保留未全文；本补记随后单独回读。原件不复制到新产物目录，本轮未生成提取产物，不把源包和回收目录互相覆盖。

## GS进行中初稿（历史保留，不代表当前执行状态）
已读执行规则27行、完整方案120行、GR/GQ报告关键原件定位及同SHA callee原件37行。唯一候选预算1，当前执行0；生产修复0。不得将静态参数位置或GQ系统栈形状升级为动态CALL前实证。
同SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53原件：artifacts/gm-saved-write/readonly-result/result/fv-pe.txt，callee [135570,1356F0)，384字节，slice SHA F6EF760CA4ADA31AAE1C4E4F33F010A70528FB3C319396FD6735CB551A4356F0。1355D5 LEA EDX,[EBP-14]后PUSH EDX；1355D9 LEA EDX,[EBP-24]后PUSH EDX；1355DD PUSH ECX；1355DE LEA ECX,[EBP-50]；1355E1 MOV EDX,C；1355E6 E8 754E0A00到1DA460；1355EB FFD7 CALL EDI。1355E8是前条CALL的立即数内部，不是独立指令边界。真实栈位置必须由动态ESP采样，不以上述PUSH推定helper返回后的参数位置。
计划沿已验证slot转换：1351EB→135573→1355EB；只读观察，不修改实参，不跳Ex或cleanup。尚在检查FH实现及正式打包配置；没有发布/运行/cleanup结果，不宣称机制已证。Available/HW恢复/full四格/x64Event0保持开放，完整替换未完成。
