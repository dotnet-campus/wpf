# FK：恢复后的纯 C# 崩溃取证执行

## FO 最后补记：原件及四文档全文回读完成
本轮四文档正文和历史已小段读至末；FN遗留三共享历史全文与截断缺段已补齐，handoff416/491/539/614/674、summary30/132/247/386、issues29/55/85/109/623/709等截断尾段均缩段补读，新增FO终态及末页亦已读。报告补记前85行、handoff678行、summary699行、issues715行；不沿用历史回读声明。本补记写后再次读取。原始事件589/业务6/stages6/cleanup4/identity1已亲读至末，唯一真实DR任务及地址/身份/退出清理已核，无新任务或源码修改。生产修复0；下一仅精确SHA候选1351F0/1DE4D1离线符号映射。Available/HW恢复/full四格/x64Event0等开放不变；本次取证及交接完成，不是生产修复或完整替换完成。

## FO 最终交付：编译闭包纠正及唯一真实候选取证完成
FO原件native-events.jsonl589行已逐段亲读至末，541–565截断段549–557/558–565补齐；DR业务/stages各6行、cleanup4行及aot identity全文亲读，精确child.trx不存在，父TestResults不存在。全部Continue成功/Error0。初始first80000003/775D8B88/TID7508/ntdll+118B88单次CONTINUE，随后04242420/750EAED4/KernelBase+1486548 NOT_HANDLED，不计候选AV。实际候选路径 C:\CodingAgentSandbox\Tasks\e31d917197404d69843eeb1b92312279\Native\wpfgfx_cor3.dll。first AV读0xFFFFFF68；继发first/second AV读0xFFFFFF7C，只是读取目标地址，不是首次写坏位置。候选FileVersion1.0.0.0/ProductVersion1.0.0+2e2a3c7e64d74642cb46f64247b40e73b4431299；实际PresentationCore SHA D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D/Version10.0.0.0，具体servicing由自包含runtimeconfig及运行.NET10.0.13记录约束，不外推其他版本。
只修改独有ignored artifacts/fo-crash-evidence/Build.props与四文档；未改任何生产/诊断源码、DRDS、NativeRun、VisualCache/historical/捕获/打包/版本或CF/CX/CY。生产修复0，正常控制重复0/full0/变体0。正式白名单包实际目录为artifacts/fo-crash-evidence/com-acceptance-package，不是登记中简称package；上传仅此目录，源与回收分离。预检为Roslyn编译的纯C#只读PE元数据与文件SHA，非脚本/新Host；保留Build.props和offline-preflight.txt。最终publish Exit0，正常分析器未压制；日志stdout38788仅尾20000可读/前18788截断，不造总警告数或默认build结果。构建发布成功本身不作运行验收。

### FO 唯一实际任务与证据
唯一有效任务 e31d917197404d69843eeb1b92312279，回收 artifacts/fo-crash-evidence/run-candidate；监督PID7152、被调试DR PID7148/TID7508，业务6行证明identity→CreateDevice/RenderTarget/ColorFill均Hr0→第一次Create before，无Create after。实际候选Native加载base1851981824/size4055040，SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53，与DR actual及预检一致；TestSHA905D899878468269011A556D4FBB767571D52583E3FC878BFFB44228F910F66B/MVIDb988c06b-92b8-483d-a6fe-9fdf7d17dd20/.NET10.0.13/X86/4。
真实AV均TID7508：first-chance C0000005/0x6E7651F0/候选+1266160（0x1351F0），读地址4294967144；随后first-chance C0000005/0x6E80E4D1/候选+1959121（0x1DE4D1），读地址4294967164；同地址second-chance，均NOT_HANDLED且Continue成功，没有吞异常。EXIT_PROCESS后子ExitC0000005，监督Exit0仅代表取证合同命中，不是产品成功；TimedOut=false、SecondChance1、CleanupConfirmed=true。cleanup4行：Detach/Wait0/两个自有Close全部成功/Error0，无Terminate。没有完成TRX/父TRX或完成计数，不是零测试；已进入真实DR。无dump/native栈，不能推断首坏写或整库bug。参数缺失的工具调用先报Function failed，未产生任务；随后唯一完整参数调用成功回收，未重试实际任务。工具不提供数字任务退出码/独立stdout/stderr，不冒称网络错误或工具Exit0。下一单一定位行动：对该精确SHA候选的两个RVA做离线符号/源码映射，优先首次0x1351F0，再核0x1DE4D1是否异常处理路径；不重跑或推断根因。

### FO 预检与执行登记（历史）
纠正 FN 的编译闭包缺口：仅为正式 ComAcceptance 同时启用 FH_CRASH_EVIDENCE 与 DR_INTEROP_ABI，独有 artifacts/fo-crash-evidence/obj/bin/publish/package，正常分析器。先离线证明最终发布程序集的 DR 类型、candidate 方法与 FH 入口，核对包和 FE 失败适配候选来源；未通过不上传。通过后至多一次 candidate，不重复正常控制、不改状态机或生产。FO 预检已通过，尚无沙箱任务：发布共3次（首次成功；加入只读C#元数据预检后第二次因SDK引用路径MSB3755失败；修正路径第三次成功），均正常分析器，无生产发布。正式 com-acceptance-package 元数据60公开类型/DR匹配1；candidate为TestMethod非DataRow，RVA430247；internal FH Run RVA440844、NativeRun.Main RVA488424。offline-preflight.txt 471行保存全部清单文件存在性/SHA和闭包PASS，I386/TestSHA905D899878468269011A556D4FBB767571D52583E3FC878BFFB44228F910F66B/MVIDb988c06b-92b8-483d-a6fe-9fdf7d17dd20。两个编译符号实际类型均存在，runtimeconfig/deps证实自包含10.0.13/win-x86。候选SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53来自FM Native、即FG已证FE失败适配来源，不是当前default源码、官方或成功诊断DLL。下一只上传本次正式包，固定既有candidate入口/filter，外240秒（内120终止/130抛超时/失败清理5+5），不重复控制。最终补齐四文档历史全文小段回读。

## FN 最后补记：执行已结束，取证和交接未完成
本轮实际候选角色仅1次，子Exit8/监督Exit1/清理确认true；有效DR执行0，未取得候选异常地址或actual加载身份。四文档FN新增终态已回读，同一FK报告71行全文已回读；三共享长文档历史尚未全文读至末，不能宣称满足用户要求的交接闭环，不能沿用FI/FG/FM历史回读记录。此为未完成交付，不是生产修复或取证成功。下一执行者须先补三历史全文小段回读及截断缺段，再审诊断包DR条件编译缺口；不将本次无目标Exit8冒充产品退出或重复有效候选。末次补记写后再次读取。

## FN 终态：原样复用 FM 包的唯一提交未进入 DR，取证目标未达成
### 执行与包配置缺口
本轮仅专用工具及既有正式纯 C# exe；无终端/脚本/新 Host/工程/原生探针。先登记 FN 进行中并读取执行规则、完整替换方案120行、总计划、FM 终态、FhCrashEvidence 全420行和实际 FM 包。正常控制沿用 task1bd3444a0b2c44c5914d874caf15398f，不再控制或模块 probe。原样上传 artifacts/fm-debug-startup/package，执行 WpfGfxShape.ComAcceptance.exe 的 --fh-crash-evidence candidate，仅提交1次；无重发/代码更改/full/COM变体/第二次请求。内部代码实际120秒终止、130秒抛超时、失败清理排空5秒和等待5秒；外部240秒。
重要偏差：提交前未核实包中 DR 测试的编译发现身份。事后公开 API 检查该 DLL 共59公开类型，DrInterop过滤0；DrInteropAbiTests整个文件要求 DR_INTEROP_ABI，而 FM Build.props只增加 FH_CRASH_EVIDENCE，csproj和默认props未启用DR符号。故固定候选filter没有对应公开测试类型；原样FM包并非候选输入已全。子进程Exit8与无业务记录相符，但无独立stdout/stderr，不能将其文字原因伪称原件已读。本轮有效DR候选执行0、候选角色提交1；不把未进入目标称产品不再崩溃或通过，停止追加运行。
### 任务、候选与测试实际身份
任务由事件绝对根识别为6336be71fa8748e2a2f24194a9899b4d；远端 C:\CodingAgentSandbox\Tasks\6336be71fa8748e2a2f24194a9899b4d；回收 d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\fn-crash-evidence\run-candidate。专用执行工具响应原文：沙箱执行完成。结果已保存到：d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\fn-crash-evidence\run-candidate。未提供数字任务退出码或独立双流，不把监督码冒称工具码。
监督PID8556/子9832/初始TID7136/start9571904，flags134218754=0x08000402；实际子加载测试DLL（事件116/121）SHA CEDF1BB6F56E21B78E28E1E61ADAEE942613ECD8A9A52C534BBBBE6093796DBA，监督同SHA/MVID4585cdb0-c774-4593-93e7-2d051c64a8c2，.NET10.0.13/X86/4，与FM完全相同。apphost SHA EA7BEC3470DF7CA3D9FB5AAD67593753BB2C572AEBB4B15293C513FB9AC0A386；runtimeconfig两includedFramework均10.0.13。
候选文件绝对路径 C:\CodingAgentSandbox\Tasks\6336be71fa8748e2a2f24194a9899b4d\Native\wpfgfx_cor3.dll，实际监督计算文件SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53，与aot.sha256一致。精确来源为FM←FL←FK/FJ复用的FE失败适配Native；FG原件taskd71a90439db344ceb58e114fa643312d曾业务actual加载同SHA、首次Create before后AV。不是当前已撤回适配的default源码产物，也不是历史全direct成功DLL，更不是旧原默认7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA。本轮未能核取旧原包用于本次任务，未偷偷替换官方模块。FN全事件中Native/wpfgfx模块未加载、无DR identity/阶段，不能称candidate actual loaded；没有候选base/size或故障偏移。此前FM/FK称其“默认失败候选”仅指选中的失败包，不能升级为当前default源码身份。
### 事件、地址及清理
native-events.jsonl全462行及cleanup全4行已逐段亲读至末。仅2个first-chance异常：TID7136的80000003/0x775D8B88，ntdll base2001469440、size1830912、path C:\Windows\SysWOW64\ntdll.dll、SHA7E15BD30890E9BF93B47FC894A68B2445618EE7528EB39584A263B21E112F9DF，地址减base=1149832/0x118B88；受限initial门只接受一次，DBG_CONTINUE65538成功。随后04242420/0x750EAED4，同TID，KernelBase base1962409984、size2990080、path C:\Windows\SysWOW64\KernelBase.dll、SHA85AD413FF860898F9E511F2E63C0E30F7AED1275FD1FD44FA90801211D328253，offset1486548，明确DBG_EXCEPTION_NOT_HANDLED2147549185成功。两者均非候选故障地址；不猜第二异常名字或native栈。没有second-chance/AV。所有Continue成功/Error0；kind5 EXIT_PROCESS后Continue成功，completed子Exit8/0x00000008、TimedOut=false、SecondChanceExceptions=0、Ready=false、InitialBreakpointAccepted=true、DumpBytes0。candidate角色没有normal-ready协议；Ready=false不等于证明DR ready或运行通过。
监督结果Exit1/CleanupConfirmed=true。cleanup原件Detach=True/Error0/ExitEvent=True，WaitForExit=0/Error0，自有0x380和0x37C两句柄Close=True/Error0；没有多余Terminate。子已结束/清理确认，但不是候选崩溃捕获通过。secondchance/timeout/Continue失败/日志IO失败分支本轮未运行，不认证其安全；未新增MiniDump，无解析native栈/首坏写。本地Results递归只有上述2文件；无DR业务JSON、stages、aot identity或完成child.trx，无父TRX与任何测试完成计数。
### 构建、交接及唯一下一动作
生产修复0，诊断/NativeRun/DRDS/CF/CX/CY/VisualCache/historical/捕获/项目打包0修改；仅本报告及handoff/session-summary/issues更新。无新build/publish，准确沿用FM默认normal analyzer Release build Exit0/98警告0错/1.70秒，不称FN新构建。FM控制通过与FK失败记录均保留。
唯一下一动作：离线核清正式诊断包的条件编译闭包，同时包含FH_CRASH_EVIDENCE与DR_INTEROP_ABI并在执行前用公开类型确认DR方法存在；绑定经核来源的失败Native而不改调试状态机、生产或DR预期。本轮不实施该配置修正/再运行；后续独立授权才能进行有效合法Create取证并取得故障模块偏移。Available/HW内容与resize/loss恢复、media/resources/线程callback、full最终四格、独立x64Event0/E_HANDLE仍开放，整体未完成。四文档历史全文小段回读及末次补记状态另据实际记录。


## FM：启动门纠正，唯一正常控制通过（最新优先）
### 合同与最小诊断更改
此前 exact DbgBreakPoint 导出 RVA 门是诊断设施自行添加的条件，不是用户要求，也不是 Windows API 必需合同；FL 的内部符号无限前置不再作为下一任务门。无浏览工具，未声称查阅官方网页。依据可复核的 CreateProcess 调试创建、DEBUG_ONLY_THIS_PROCESS、WaitForDebugEvent/DEBUG_EVENT、ContinueDebugEvent、OUTPUT_DEBUG_STRING_INFO 与调试事件句柄所有权合同，并用本节实验核对：系统在进入应用前向创建调试器交付 initial breakpoint；合同不保证该地址等于某导出入口。
仅 FhCrashEvidence 修改，仍整文件 FH_CRASH_EVIDENCE 显式隔离；NativeRun/生产/打包工程/版本未改。目标仅本次 CreateProcess 得到的 PID，flags=0x08000402（DEBUG_ONLY_THIS_PROCESS|CREATE_NO_WINDOW|CREATE_UNICODE_ENVIRONMENT），须先 CREATE_PROCESS、初始线程匹配、对应已加载 ntdll 地址映射、尚无任何异常/未消费 initial break/未 ready，且精确 firstChance=1、STATUS_BREAKPOINT=0x80000003，最多一次 DBG_CONTINUE。不是所有 ntdll 内断点或第一个任意异常；其他异常 first/second chance 均 NOT_HANDLED，second chance 计数。没有宽泛 WOW64 fallback，本轮只观察到一个系统架构断点；其他架构码未实证，不扩大识别。未知首门先记录并 NOT_HANDLED，再停止进入 FK 失败清理。
正常子用既有 --fh-crash-evidence normal-ready 显式入口，从独立环境块接收随机令牌，经 OutputDebugStringA 送回；不增加 NativeRun 入口。调试 kind/PID/TID/阶段、模块身份、线程起始地址、异常地址/firstchance、每次 Continue 状态均用 Utf8JsonWriter 逐行写出；新增事件输出不使用反射 JSON 序列化。模块采集原角色未执行，其既有 JsonSerializer 分支仍有 AOT 兼容风险，不能把本次自包含 JIT 发布当 NativeAOT 认证。
### 构建与独立包
专有 artifacts/fm-debug-startup/Build.props 将 obj/bin/publish/package 分离，仅上传 package，回收另在 run-control。正常分析器显式 x86 self-contained .NET10.0.13 publish 共3次：前2次 Exit1，同一新增局部 result 与 cleanup 内 result 重名导致 CS0136；第二次 errors-only 文件取得精确原错，改名 supervisorResult 后第3次成功。非网络失败；未 NoWarn/关闭分析器。既有白名单目标原样生成418行 package-files.txt，含运行时资产、Contracts 两文件、Native/wpfgfx_cor3.dll 和 aot.sha256；未改工程打包。runtimeconfig 两 includedFramework 均10.0.13。源 props/obj/bin/publish/包/回收不混传。候选来源 FL 包 Native，不重新发布生产；aot.sha256=1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53，与实际监督文件哈希一致，但控制未加载候选，不称候选实际加载。
默认正常 Release build 1次成功，Exit0/98警告/0错误/1.70秒；保留原分析器，默认不定义 FH_CRASH_EVIDENCE。不是 full 或生产修复。
### 唯一正常控制原件
本轮控制恰好1，模块probe/full/候选崩溃均0。task=1bd3444a0b2c44c5914d874caf15398f，远端 C:\CodingAgentSandbox\Tasks\1bd3444a0b2c44c5914d874caf15398f；回收 artifacts/fm-debug-startup/run-control/Results/fh-control。native-events.jsonl 195行与 cleanup.txt 4行已全文小段亲读至末。实际 TestSHA=CEDF1BB6F56E21B78E28E1E61ADAEE942613ECD8A9A52C534BBBBE6093796DBA，MVID=4585cdb0-c774-4593-93e7-2d051c64a8c2，.NET10.0.13/X86/4；监督PID5248，子PID10824，初始TID6740，StartAddress12521024。
事件顺序：kind3 CREATE_PROCESS → kind6 主系统模块/ntdll（SHA7E15BD30890E9BF93B47FC894A68B2445618EE7528EB39584A263B21E112F9DF、base2001469440）及kind2线程 → kind1 初始断点80000003、firstchance=true、TID6740、0x775D8B88/ntdll+0x118B88，受限门接受，Continue DBG_CONTINUE=65538成功 → hostfxr/hostpolicy/coreclr加载 → 后续kind1 firstchance 0x04242420/0x750EAED4/KernelBase+1486548，LoaderBreakpoint=false，Continue DBG_EXCEPTION_NOT_HANDLED=2147549185成功 → 托管模块加载 → kind8 随机FM_READY_cf550b41eef144e38eb708925282c3bb读取成功/Ready=true → kind4线程退出 → kind5进程退出并继续成功 → completed子Exit0、SecondChance0、TimedOut=false → supervisor-result Exit0/CleanupConfirmed=true。所有195行中 Continue 均成功/Error0；不把正常 initial break 的继续称吞掉候选真实异常，也不把后续异常码未经核查命名为系统架构断点。
cleanup：Detach=True/Error0/ExitEvent=True，WaitForExit=0/Error0，0x38C及0x388两个自有句柄Close成功/Error0；无多余Terminate，子已结束。事件文件句柄既有finally即时关闭且 diagnosticError=0；事件进程/线程句柄按系统退出事件所有权管理，没有重复手动关闭。工具只返回执行完成/回收位置，未独立暴露数字任务退出码；Exit0来自原始监督结果及子退出事件，不伪称工具数字退出码。无独立双流文件、无TRX或测试通过分母、无dump/native栈。
### 未运行风险与独立下一任务
保留FK失败清理，不修改其Terminate/排空/Detach/Wait/Close逻辑。本次只验正常路径，未知门/timeout/Continue失败/日志IO异常/secondchance/用户断点/多架构系统断点分支没有本轮运行证明；默认候选DR生产缺陷未修。控制通过不认证候选异常捕获，也不证明全部异常路径安全。
下一独立任务仅默认候选合法DR Create崩溃取证1次，使用已安全通过控制的诊断设施、重新核实际包身份，取得真实异常地址/模块偏移/secondchance/清理；本轮明确不执行。Available/HW内容及resize/loss恢复、media/resources/线程callback、最终full四格、独立x64Event0/E_HANDLE仍开放。四文档全文小段回读在更新后执行，最后补记另读，不沿用历史回读声明。


## FL 启动断点认证：同SHA模块已取得，精确来源仍未证
直接执行，无委派、终端、脚本、反汇编或机器码探针。FK原始事件22行及cleanup9行本轮亲读至末。事件3为7336（源码kind3路径也记作thread-created，原件未保存kind/起始地址）；主exe、ntdll及系统DLL加载后另有6740/2744/2980线程，唯一初次异常在7336。只能证明与启动线程相同，不能把该先后关系当loader内部符号认证。
新增仅FhCrashEvidence显式modules角色：纯C# PEReader解析当前x86 ntdll文件节映射/全部命名导出、SHA和目标RVA附近33字节，ReadProcessMemory只读当前进程映射；没有创建调试子进程。现有事件另补DebugEventKind/InitialProcessThread/StartAddress字段，但本轮未实跑新控制，不能补造FK缺失的启动地址。未改变任何断点识别、DBG_CONTINUE或NOT_HANDLED规则；未加首次吞断点fallback。
两次模块采集（非正常子退出控制）均返回JSON并回收；首次单行JSON/工具未暴露taskID，回收artifacts/fl-startup-auth/run-modules；最终task262638d5b2f94c5eba30ab15b9d30ba8，远端C:\CodingAgentSandbox\Tasks\262638d5b2f94c5eba30ab15b9d30ba8，精确回收artifacts/fl-startup-auth/run-modules-final/Results/fl-modules/module-evidence.json。工具未返回数字应用退出码，不能冒称Exit0；无独立stdout/stderr落盘，JSON原件独立保存。没有调试子进程/事件循环或调试句柄清理可验收，也没有TRX。
最终actual TestSHA850F20A2B7A08267BEF258DBB15B302A55105FE8BD3FDB9C82CF6C35BF89F162，MVIDef71709a-45f3-417c-9344-3cfb5ea3cd0c，.NET10.0.13/X86/4。候选文件1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53，CandidateLoaded=false；不是actual加载候选证据。当前ntdll逻辑路径C:\WINDOWS\system32\ntdll.dll（x86文件系统重定向语义），PE I386/PE32、SizeOfImage1830912/base2001469440；SHA7E15BD30890E9BF93B47FC894A68B2445618EE7528EB39584A263B21E112F9DF，与FK SysWOW64原件一致，不以路径字面声称x64。目标RVA1149832/0x118B88对应fileOffset1146760，文件与当前映射33字节一致：F6FF85C0781B807DE70075158365FC00CCEB0733C040C38B65E8C745FCFEFFFFFF。读取成功33/Error0，字节相等不是启动来源证明，不声称调用栈或ESP直接观测。
实际导出DbgBreakPoint RVA507408、DbgUiRemoteBreakin846256、DbgUserBreakPoint507392、LdrInitializeThunk213552、NtWow64DebuggerCall506576；目标1149832不等于这些入口，导出没有提供内部函数范围/调用关系。Windows创建调试进程合同可预期initial breakpoint，但不能凭系统模块内first-chance或字节中断点值认证此内部入口。缺少同SHA模块可信匹配符号/PDB内部边界与启动路径关联；历史线程起始地址未保存、已终止不可回查。平台需提供该SHA的可信符号/映射来源证据，或后续经授权在保留未知门的控制中保存原始kind3/kind2起始地址及完整加载映射。当前不作loader/WOW64确切函数归属。
正常分析器显式x86 selfcontained10.0.13 publish两次成功，专属fl-startup-auth/obj、bin、显式PublishDir=fl-startup-auth/publish；按既有白名单生成clean com-acceptance-package，Native/Contracts/runtime/aot.sha256与源码、publish、回收分离，只上传package。未改打包工程、TFM、SDK、NoWarn或规则。默认正常Release build成功/98警告0错误。NativeRun及生产0修改，整份诊断仍在FH_CRASH_EVIDENCE默认隔离内。
FL新增正常子退出控制0、候选Create0、full0。FK控制仍不通过；其terminate/4continue/detach/wait0/两个Close成功只证明失败清理，非正常Exit0，数字父子退出码仍缺。唯一下一先补可信内部入口来源认证；控制通过后才可候选最小取证1，不能先Create。Available/HW/full/x64Event0仍未闭。四文档历史全文小段回读进行中，不冒称完成。

## FK 终态：控制断点来源未证，停止候选复现
实际提交正常控制一次，候选崩溃零次、full零次。taskID `1ff51558d7474901843843fd4054e56c`，远端 `C:\CodingAgentSandbox\Tasks\1ff51558d7474901843843fd4054e56c`；完整任务回收 `artifacts/fk-crash-evidence/run-control`。非连接失败：工具返回真实应用未处理 InvalidOperationException（Unverified startup breakpoint），没有返回数字控制进程退出码，不推造；stdout/stderr未独立落盘，工具返回文本可见上述异常栈，不能声称双流完整证据。

### 控制原件亲读
native-events.jsonl全22行及cleanup.txt全9行已缩段读至末。监督PID2708，子PID4920，初始线程7336。唯一异常为first-chance breakpoint `0x80000003`，地址 `0x775D8B88`，线程7336，`ntdll.dll` base2001469440、size1830912、offset1149832（0x118B88），路径 `C:\Windows\SysWOW64\ntdll.dll`、SHA `7E15BD30890E9BF93B47FC894A68B2445618EE7528EB39584A263B21E112F9DF`。未匹配精确DbgBreakPoint RVA，LoaderBreakpoint=false；真实异常仍由DBG_EXCEPTION_NOT_HANDLED继续，然后抛出安全门异常并清理。不臆认此地址是loader断点，不是候选AV，也不是首坏写或根因。没有second-chance或正常Exit0/completed记录；控制不通过，禁止第二次控制或候选复现。
清理TerminateProcess=True/Error0；4次CleanupContinue=True/Error0；Detach=True/Error0/ExitEvent=False；WaitForExit=0确认子已结束；0x384与0x380两个自有句柄Close成功。清理期间事件仅摘要，不记录子退出码，不能冒称正常退出/事件循环通过。无父TRX，无Create完成TRX和测试计数，无dump/native栈。

### 身份及构建
实际监督TestSHA `C2FE0245DB4028036CD531A79BA0385F065E0E89E7E6AB0DA873A9D8258C0B05`，MVID `10370b81-c352-490d-b0ad-2e1f29e81942`，.NET10.0.13/X86/4。候选文件SHA `1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53`，与历史默认失败候选身份一致，来源现存fj包Native，不是旧全direct成功DLL；本控制尚未加载候选，不能称actual loaded nativeSHA。包aot.sha256已读与实际文件哈希一致。
显式诊断正常分析器publish两次Exit0；首次未显式覆盖PublishDir，落到shared artifacts/publish，发现后第二次显式PublishDir纠正到fk/publish，未上传shared。obj/bin由专属props重定位、包根为fk/com-acceptance-package，仅该白名单包上传，未上传publish/父目录/回收根。未改项目打包/版本、未NoWarn或禁分析器。第一轮发布越出专属publish要求是本轮偏差，如实保留，不认证全程隔离无偏差。
最后默认正常Release build一次Exit0，98警告0错误，2.59秒；正常分析器未关闭。FH_CRASH_EVIDENCE仍仅显式定义，FhCrashEvidence整文件及NativeRun两分派默认不编译不触发，NativeRun未改。

### 诊断更改与未验收边界
仅FhCrashEvidence更改：事件hProcess/hThread由系统在对应退出事件继续后管理，不手动关闭；自行CreateProcess取得的两个句柄去重持有，CREATE_PROCESS/LOAD_DLL文件句柄即时finally关闭，异常/cleanup路径仍回收文件句柄；Continue失败升级错误并进入终止/有界调试排空/脱离/等待/关闭。已有独立Unicode子环境块不改父环境，无父环境恢复窗口。退出事件后先有界等待，减少多余kill竞态；控制成功必须有经过精确验证的初始断点。未识别首断点记录后抛错，不吞真实异常制造通过。本次仅验证了此安全门失败路径的结束/自有句柄关闭，不声称全部timeout/continue失败/日志IO失败路径均安全验收。
FJ历史正文与本次用户交接矛盾，保留但不计FK任务；不能将其声称的控制通过接续为本次授权。

### 唯一下一动作与开放门
先离线核对本控制同SHA ntdll中0x118B88的loader断点来源（需可信符号/静态模块证据及线程启动关系），完善纯C#精确识别；未证明前不再复现Create、不扩大COM替换。当前尚无候选异常地址/模块偏移，不能跳转生产根因结论。生产COM、DR/DS、fill、VisualCache/historical、捕获和项目打包未改；Available/HW恢复/full四格/x64Event0及其他开放门均未闭，未称生产已修。
文档最新节与报告将回读；历史全文回读状态另据实际记录，不沿用FI回读作为FK亲读。

## 控制完成，candidate待执行
正常分析器显式发布两次失败（CS0255/CA2219），修正后第三次成功；未抑制规则。独有obj/bin/publish/package，正式selfcontained x86 .NET10.0.13。控制task64f6dcb46ee94b7ba18de2b2c281165d，远端C:\CodingAgentSandbox\Tasks\64f6dcb46ee94b7ba18de2b2c281165d，本地artifacts/fj-crash-evidence/run-control。51行debug与5行cleanup亲读至末：ChildPid8592启动，Exit0、无二次异常/timeout，事件循环结束，Detach=True、WaitForExit=0、两个持有句柄Close成功。EXIT_PROCESS后即时wait竞态触发多余TerminateProcess=False/Error5，随后脱离/等待确认退出。初始ntdll断点未匹配精确DbgBreakPoint来源，保守NOT_HANDLED，未吞异常或计崩溃。工具未提供数字退出码，不冒Native码；无父TRX。控制通过，同包只运行candidate一次。

## 进行中（历史）
已接续 FI；本轮直接执行，不委派、不使用终端或脚本。先审执行规则、完整方案、FI 风险及现有显式入口，修复诊断清理后采用独有 FJ 产物。正常控制至多一次，通过后合法 DR 默认候选 Create 崩溃取证至多一次；仅确定部署错误允许一次纠正。当前尚未构建、提交沙箱或取得任务 ID，不将历史连接阻塞视为本次结果。

仅允许 FhCrashEvidence/NativeRun 必要诊断更改、专属测试和四文档；保留 FH_CRASH_EVIDENCE 默认隔离、正常分析器及原 MTP/filtered/full 语义。不得修改生产 COM/DRDS/CF/CX/CY、项目打包或版本。不修改全局配置。Available、HW 恢复、最终 full 四格、x64 Event0 等仍开放。
