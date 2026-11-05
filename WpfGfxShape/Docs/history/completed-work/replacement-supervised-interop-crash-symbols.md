# FP：精确崩溃产物离线符号/源码映射

## FR 终态：唯一离线任务未认证实际加载PDB，停止符号循环（最新优先）
### 实际执行与原件
FR先登记handoff进行中。只修改现有FpSymbolReadonly显式FP_SYMBOL_READONLY区域和文档，新增ignored artifacts/fr-symbol-auth/Build.props；未新增Host/项目/外部调试器/库，无终端或脚本。现有正式ComAcceptance net10.0-windows、SDK10.0.107、x64 Release self-contained仅诊断发布1次，专属obj/bin/package，显式命令StartupObject覆盖项目入口，正常analyzers保留，未NoWarn/压制；发布Exit0，日志含原有警告且前部显示被截断，不称零警告或完整日志认证。生产/DRDS/NativeRun入口/打包/CF-CX-CY未改，生产发布0、候选/COM/崩溃/full0。
唯一只读task 5e66e939b3a54d29b8974c317acecfb2；clean-input仅exe、原DLL/PDB及必要FE原字节快照，回收result/fp-symbols.txt，不上传历史父目录。exe74238948字节；DLL3995648/SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53，PDB18395136/SHA E9B41963580407519B1EC4A7B28ADFDF643ECFF9CF6509E89CCE78476E45ED6C，输入RSDS/MSF7 GUID e0a4d956-1efc-4509-83a3-b1664c206c82 age1重新实算匹配。原件19行已读至末。
### 实际加载认证及API边界
FR选项E1610=FQ E1210加EXACT_SYMBOLS，限定本地input原PDB，无环境符号路径/联网fallback。SymInitialize=True/0，SymLoadModuleEx返回请求base6E630000/0；随后SymGetModuleInfo64=True/Error0，但LoadedPdbName为空、PdbSig70全零/PdbAge0、SymType0、LineNumbers=False、PdbUnmatched=False/DbgUnmatched=False。这不是已认证加载匹配，两个False不能脱离空身份当Matched；ActualLoadedPdbAuthenticated=False。不从输入匹配推加载匹配，不猜未加载原因，不把注册模块成功当PDB加载成功。
IMAGEHLP_MODULE64 Sequential/ANSI/Pack8，实测Size1680；含DWORD/ULONG64、固定ANSI数组、GUID及4字节BOOL，Winapi默认DllImport/SetLastError；失败立即取last-error，成功记录0不是成功后last-error实取。IMAGEHLP_LINE64沿用指针宽度结构；本轮无行API实测。角色门在module认证失败后return4，finally SymCleanup=True/Error0。专用执行工具仅报沙箱完成及回收路径，未返回进程原始ExitCode或监督/清理层级字段：4是已读源码与完整原件对应的返回分支，不冒称工具实报Exit4；监督退出码/原始进程退出码/超时与清理确认独立证据不可得。没有第二次执行。
checksum动态检查SymGetSourceFileChecksum导出及同算法比较代码已加入（MD5/SHA1/SHA256，仅对照FE快照原字节），邻行用SymGetLinePrev64/Next64；但均在认证门后，未到达，不能称API不支持或checksum不存在，更不能称源字节匹配。导出可用性/算法/checksum/211相邻边界本轮实际不可得。快照虽作为最小输入上传，尚未实际执行哈希比较；保持未经源字节认证，不无限阻塞。
### 可信映射与唯一下一根因动作
FR没有新增函数/行映射，独立读取FP result-final全21行支持历史映射的证据来源：RVA1351F0在Create范围[134F70,135362)、+280，nearest211/13517F+71；RVA1DE4D1在UnwindStackFrame范围[1DE440,1DE5A6)、+91。该旧运行仍缺实际加载PDB认证；保留有界函数定位，不升级为FR加载独立认证、具体C#故障语句或首次坏写。nearestline仅辅助，继发unwind非首因。
下一唯一根因动作：另行明确授权既有正式纯C#调试角色，在已证Create函数范围收异常寄存器/局部变量上下文，以区分前序COM输出污染与BitmapState/GetAdapterLuid初始化消费坏值；不是再一轮泛符号需求。本轮只离线不采新崩溃，不执行该动作。
### 隔离及交接
完整源码仍全文件#if FP_SYMBOL_READONLY、无测试发现属性，默认csproj StartupObject=NativeRun，不修改正式项目或默认编译符号；Build.props仅专属诊断显式使用。Available/native通知consumer、HW内容/resize/loss恢复、media/resources/线程callback、最终双项目双架构Release full四格、独立x64Event0/E_HANDLE均开放，生产修复0、完整替换未完成。FR/FS分工回读闭环见下段，不继承FQ/FO旧闭环声明。

### FS 最终补读与交接闭环（仅文档）
按本次交接指定范围承接FR本轮已读前段：handoff 1–420、session-summary 1–398、issues 1–397；这是FR阅读归属，不冒称FS独立全文亲读。FS用连续小段补齐当前handoff 421–699、session-summary 399–714、issues 398–730至末，handoff第510行截断已缩段重读；另读本FP报告1–73至末，第27行截断已补读。FS回核三共享当前首节仍为同一FR终态，无更新的最新节；此前前段按FR本轮交接承接，FS另核读首节及部分前段，不把历史FO/FQ声明当本轮阅读。仅更新四文档最新节并作写后回读，至此FR已读与FS补读合并完成交接，不是FS单独全读三共享长文档。
FS源码修改/新报告/符号任务/构建/发布/运行/沙箱/full均0，只用专用读写工具。输入PDB匹配，但本次SymGetModuleInfo64没有实际PDB加载身份；提前return4是源码分支，不是工具实报退出码。未到checksum不等于API不支持，旧函数范围/nearestline只作辅助、不作源字节认证；停止泛符号轮次。当前该崩溃无生产修复，完整替换未完成，其他开放门不变。
下一唯一真实根因动作须另行明确授权：由既有正式纯C#调试角色在Create首次AV收寄存器、异常/局部变量上下文及实际模块偏移，以原异常VA 0x6E7651F0 / RVA 0x1351F0为验证锚点，核对实际模块身份和数据流，区分前序COM输出污染与BitmapState/GetAdapterLuid初始化消费坏值；不把继发UnwindStackFrame当首因，不预判故障语句或整个库bug，本轮不执行。

## FQ 核证交付：函数映射保留，源码字节/实际加载PDB及全文交接未闭（最新优先）

### FQ 最后补记（本轮实际边界）
最终符号原件21行、FP诊断源码138行、FF FE快照640行及本报告55行已分段读至末；三共享长文档已读最新节和历史多段及末尾（handoff690/summary708/issues724行），但中段存在未读和超长截断未全部补齐，不能声称四文档最后全文回读闭环完成。保留该用户要求的交付缺口，不用历史轮次的全文声明抵销。最新FQ新增段已回读，本补记写后再读；后续仍需补三共享全文缺段，不能结果已跑未读结束。FQ所有新增业务/符号执行/构建/发布为0；无源码修改。
FQ早期口头将最终E1210称不含SECURE有误，最终已纠正：SECURE=0x40000、NO_IMAGE_SEARCH=0x20000均在；仅EXACT_SYMBOLS=0x400不在。其余实际加载PDB路径/Matched与source checksum缺口不因该纠正消失。

### 最终原件与初稿纠正
FQ只读核验，无新增构建、发布、沙箱/符号执行、候选/COM/崩溃/full运行；生产、测试源码、入口、打包、DRDS、CF/CX/CY均0改。规则、完整120行替换方案、slnx三项目、当前基线、FP源码138行及专属props已读。以下FP旧结论保留历史，不再作为最新认证。
权威原件是 artifacts/fp-crash-symbols/result-final/fp-symbols.txt，21行已分段读至末；任务090658255e19408d8be9a83ba05ddbef，不是初稿0388863ff6ee4b5c9d97c23238271064/result。最终exe74237063字节，比初稿多1字节。最终目录和upload均列出exe、input/wpfgfx_cor3.dll、input/wpfgfx_cor3.pdb三个文件；最终原件声明三者74237063+3995648+18395136=96627847字节，只是已列包文件体积，不是传输字节/压缩体积。FQ未独立计算二进制hash，引用最终诊断实算输出。
最终SymOptions=0xE1210：LOAD_LINES(0x10)、FAIL_CRITICAL_ERRORS(0x200)、IGNORE_NT_SYMPATH(0x1000)、NO_IMAGE_SEARCH(0x20000)、SECURE(0x40000)、NO_PROMPTS(0x80000)。旧报告的NO_IMAGE_SEARCH/SECURE名称正确，但EXACT_SYMBOLS(0x400)未设置，LOAD_ANYTHING(0x40)也未设置；不能称严格exact模式或以选项替代实际加载路径核验。初稿实际0x1888210与最终不同，禁止混用。FQ早期将0x20000/0x40000误释为NO_PUBLICS/AUTO_PUBLICS已即时纠正，不能沿用该误释。
最终输入DLL SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53；PDB SHA E9B41963580407519B1EC4A7B28ADFDF643ECFF9CF6509E89CCE78476E45ED6C。PE RSDS与手读MSF7 info stream均GUID e0a4d956-1efc-4509-83a3-b1664c206c82/age1，IdentityMatch=True。注意这是输入文件身份匹配，不是DbgHelp Matched字段。SymInitialize搜索目录为任务input，SymLoadModuleEx实际传DLL路径而非PDB路径。源码和最终输出都没有SymGetModuleInfo64/LoadedPdbName/PdbUnmatched信息；故DbgHelp实际加载的PDB完整路径、其Matched/GUIDage状态未独立记录，不能把输入PDB路径冒称实际SymLoad PDB路径。现有诊断只解析MSF身份、不解析source checksums，不调用source-info API；本轮不扩工具、不重跑。保留此认证缺口。

### 最终可信映射及来源边界
FO原件 Results/fh-candidate/native-events.jsonl 497–499、548–558已亲读：actual base1851981824=0x6E630000/size4055040=0x3DE000，actual SHA与最终符号输入一致。0x6E630000+0x1351F0=0x6E7651F0，+0x1DE4D1=0x6E80E4D1；不以PE preferred base0x10000000计算实际VA。首次读AV地址FFFFFF68，继发FFFFFF7C，不据此猜寄存器/指令。
最终SymInitialize/SymLoadModuleEx/SymFromAddr/line API/SymCleanup均True或正确base、Error0（代码成功时写0，并非另取成功后的last-error）。load requested/returned/modulebase均6E630000。
- 首次RVA1351F0：InteropDeviceBitmapExports__Create+0x280，start134F70，Size1010=0x3F2，范围[134F70,135362)，地址确在其中；nearest line211/address13517F，displacement0x71=113字节。
- 继发RVA1DE4D1：CoffNativeCodeManager::UnwindStackFrame+0x91，start1DE440，Size358=0x166，范围[1DE440,1DE5A6)；nearest C++ line683/address1DE4C1，displacement0x10=16字节。该继发runtime展开异常不是首坏因。
两地址均在.text [1000,21D606)。最终行API仅给最近地址，不提供下一sequence point、inline记录或完整行区间；不能声称13517F到下一行边界已认证。优化/内联可能让构造与初始化共享粗粒度行号，但是否发生及范围未提取，不把可能性当事实。异常所在函数不是首次坏写位置。
PDB行路径是原构建 Code/WpfGfxShape/Abi/InteropDeviceBitmapExports.cs 的绝对路径；该路径当前文件已撤FE行为，不是不可变原构建源。FF归档 artifacts/ff-interop-acceptance/source/fe-production-experiment.cs.txt 共640行本轮分段读完，211–215为new BitmapState及UpdateMethod/GetAdapterLuid初始化。原构建native PDB路径仍指fe-interop-closure/bin/.../native/wpfgfx_cor3.pdb，但该路径不能认证当时C#字节。未取得PDB内source checksum/算法、source file表完整记录或原编译源哈希，故FF归档及当前源均“未经源字节认证”，不能仅凭211相同完成归因。当前default的GetDesc/GetDevice/Creation及cleanup为generated，FE快照相应为显式Stdcall/helper/Direct3D9Factory.Release；只比较明确读到的范围，不移绑当前默认失败根因。

### FE快照静态数据流（源码事实，不是运行到达证明）
1. Create169–177先查output/width/height/resource/callback非空，随后写三个out为0；dpi只拒负值，未拒NaN/Infinity。非空不证明指针可写或不别名，输出可能先于try写坏；FO输入111.5/123.25、Fallback=false已核，不用错误输入反例解释本次合法调用。
2. resource先解vtable QI0取得surface；初始surface/device/instance=null、GCHandle=default。仅Hr<0退出，未额外检success+nonnull；GetDesc slot12写未显式初始化的description，Hr成功后校验尺寸上限、格式、usage、pool。随后GetSurfaceDevice slot3、CheckDevice（QI0/Ex128或TestCooperative3）、GetCreationParameters slot9写parameters。success只在COM合同成立时保证out写全；布局/调用约定/有效指针不是这些guard独立证明的结果。
3. GetUpdateMethod先Caps9 slot7写caps，以短路Hr判断避免失败读取DevCaps2；成功路径消费DevCaps2/Caps2，第二QI成功后ReleaseEx2。多采样检查依赖其method。parameters.AdapterOrdinal在211传入构造，尚未校adapter范围/与真实device一致；没有本次实际参数值、Caps/Description逐字段记录，不能把源码可达当实跑。
4. BitmapState是托管class；Gate、Dirty列表、delegate及字段由CLR初始化，构造98–103复制surface/description/dpi/version/callback/fallback/adapter，不解引用surface；不能误判class为未初始化native结构。Identity初始0，到219才赋值；Register221在Identity赋值之后。构造本身不调用callback，不能凭行211归因callback或GCHandle。
5. 初始化UpdateMethod后GetAdapterLuid296–318先generated device slot6取得factory、generated factory QI0；成功才第二份parameters经FE slot9取得AdapterOrdinal，Ex GetAdapterLUID写luid再按ulong读，失败返回0。它不使用前一份parameters作第二次out；factory/extended初始null、finally逐一Release。没有success+nonnull guard；第二slot9/LUID实际走否未由FO业务日志证明，旧其他SHA非Ex结果不可移绑。
6. 初始化完成才GCHandle.Alloc216；AllocZeroed217/Instance字段218（x86按本地Sequential可推offset0/4/8、size12，但不是原二进制布局实测）→Identity219→Register→写成功outputs223→surface/instance/handle清空转移所有权。Alloc失败由OutOfMemoryException处理，不能仅因无显式null guard认定本次分配空指针。finally230起先Free instance、free已分配handle，再release device/surface；若Register期间抛出，源码存在未见显式Remove listener/Dispose state的所有权补偿风险，但需审listener实现/真实到达才能判断，不当本次根因。

### 最小候选与下一单一动作
未发现可凭当前证据确定的错误结构offset或已证未初始化字段。最小候选集合是：前序COM输出/ABI未认证导致Create局部值损坏，以及211粗行关联的构造/初始化（尤其后置GetAdapterLuid调用）对既有坏值的首次消费；两者均是待证候选，不断言具体fault指令、栈损坏或Silk全库bug。success但null/out未写、out别名、Register失败残留只是反例推演，不是FO事实；不建议泛替全部调用。
下一唯一可执行根因动作：在既有正式C#只读符号角色的一次有界离线取证中，针对同DLL/PDB输出SymGetModuleInfo64的LoadedPdbName/PdbUnmatched/PdbSig70/PdbAge，并用DbgHelp source-info API（若原PDB不提供则如实失败）取得该源文件checksum及211附近前后行边界，与FF快照原字节作同算法比较；不加载执行候选、不重发生产、不跑COM变体。它补当前无法把行号绑定历史源码的根本证据缺口；失败即保留未经源字节认证，不无限造工具。本轮未执行此动作。

### 执行次数、隔离与交接状态
现有FpSymbolReadonly全文件#if FP_SYMBOL_READONLY，无MSTest发现属性；默认csproj入口仍NativeRun。显式props定义此符号及StartupObject，但项目自身StartupObject会覆盖早期props，历史第一次误入口不能说已安全执行该角色。最终原件证明X64/8纯文件符号进程，分析I386/PE32候选；SymLoadModuleEx注册符号模块不是执行业务DLL。
纠正FP旧“第三次即最终”：有两个独立已回收结果task0388863ff6ee4b5c9d97c23238271064和090658255e19408d8be9a83ba05ddbef；加历史报告两次未回收提交，记录口径至少4次符号诊断任务而非3。早两次taskID/原始退出与完整调用历史未获独立证据，不造精确全局成功次数。5次publish成功只是FP报告历史记录，FQ无原始构建日志验证，不冒称本轮实跑5次或证明之后没有额外发布。FQ新增符号执行0/业务0/full0/生产修复0。
Available/native通知consumer、HW内容/resize/loss恢复、media/resources/线程callback、最终双项目双架构Release full四格及独立x64Event0/E_HANDLE全部未闭，完整替换未完成。三共享文档历史全文回读与截断补齐仍在进行，不能用FO旧声明代本轮完成；最终补记据实际记录。

## FP 已证实映射；源码快照认证与全文交接尚未闭环（历史，认证更正见FQ）

### 身份与执行
已独立读取 FO native-events.jsonl 497–499、548–558：实际模块 base1851981824=0x6E630000、Size4055040=0x3DE000、SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53。TID7508 初次 C0000005 first-chance 指令6E7651F0，ExceptionInformation[0,4294967144]，读取FFFFFF68；随后6E80E4D1 first及second-chance读取FFFFFF7C。后续不是最初损坏证据。
FE publish/native-x86 精确DLL静态复算SHA一致，3995648字节，I386/PE32，preferred imagebase10000000，SizeOfImage3DE000。原始DLL/PDB未重建或删除。FE native obj/link.rsp/ilc.rsp/obj均仍存在；link.rsp指向FE bin native输出和NativeAOT10.0.13 Runtime.WorkstationGC.lib。未使用ignored名称检索0推断不存在。
CodeView RSDS GUID e0a4d956-1efc-4509-83a3-b1664c206c82 / age1，路径 artifacts/fe-interop-closure/bin/WpfGfxShape/Release/net10.0/win-x86/native/wpfgfx_cor3.pdb。FE发布PDB为MSF7，18395136字节，SHA E9B41963580407519B1EC4A7B28ADFDF643ECFF9CF6509E89CCE78476E45ED6C；直接读取MSF info stream GUID/age完全一致。当前只核此一个原始native PDB候选，无拒绝候选或伪配。托管同名PDB未用作native符号。

### 两RVA（匹配原始PDB）
- first 1351F0 → wpfgfx_cor3_WpfGfxShape_Abi_InteropDeviceBitmapExports__Create +280，函数起始RVA134F70，Size1010十进制；PDB源路径 Code/WpfGfxShape/Abi/InteropDeviceBitmapExports.cs:211，最近line address RVA13517F +71。属于生产Create，不是runtime helper。
- secondary 1DE4D1 → CoffNativeCodeManager::UnwindStackFrame +91，函数起始RVA1DE440，Size358十进制；PDB源路径 D:/a/_work/1/s/src/runtime/src/coreclr/nativeaot/Runtime/windows/CoffNativeCodeManager.cpp:683，最近line address RVA1DE4C1 +10。属于NativeAOT runtime栈展开函数，不当成首坏根因。
以上位移为十六进制（Size除外）。DbgHelp SymInitialize/SymLoadModuleEx/SymFromAddr/SymGetLineFromAddr64/SymCleanup均成功Error0；请求及实际symbol modulebase均6E630000。本地已核PDB目录限定、EXACT_SYMBOLS/NO_PROMPTS/FAIL_CRITICAL_ERRORS/IGNORE_NT_SYMPATH/NO_IMAGE_SEARCH/SECURE，无联网fallback或LOAD_ANYTHING。两RVA均在.text，符号范围包含地址；行仅最近sequence point，不证明单条C#语句/内联范围。

### 当前阻塞与可信层级
符号/行身份为精确原版PDB强证据；历史源码字节认证尚未完成。FF source/fe-production-experiment.cs.txt第211行是BitmapState构造，211–215包含UpdateMethod/GetAdapterLuid初始化；当前default第211行亦相近，但前序及cleanup绑定不同，不能把当前文件当FE完整源码快照。未提取PDB source checksum/内联记录，故不宣称该行即确切出错子表达式。无native栈/dump/寄存器，不证明首次坏写或Silk全库bug。
只读正式ComAcceptance显式FP_SYMBOL_READONLY全文件入口，未改NativeRun/生产/DRDS/ExitDiagnostics/打包/默认行为。正常分析器，无NoWarn。独有fp目录；5次只读工具publish成功（非生产发布），头两次沙箱未回收文件：第一次入口被项目StartupObject覆盖，第二次single-file BaseDirectory抽取路径；改显式StartupObject、全内容自抽取并按ProcessPath写结果后第三次只读任务0388863ff6ee4b5c9d97c23238271064取得21行结果。前两次没有候选执行入口授权，不拿结果缺失当产品失败；本轮真实DR/COM运行0/full0。上传白名单仅exe74237062 + DLL3995648 + PDB18395136 = 96627846字节；源upload与result分离，不上传obj父目录或历史。原件 artifacts/fp-crash-symbols/result/fp-symbols.txt已全文亲读。
生产修复0，Available/HW恢复/full最终四格/x64Event0等仍开，完整替换未完成。下一唯一根因动作：先用匹配PDB的源checksum认证FF FE快照，再静态审Create内该sequence-point范围（BitmapState构造/初始化与其内联调用）的x86指针和ABI数据流；不重跑、不先改COM变体，runtime展开崩溃仅辅助。

## FP 进行中（历史）
仅定位 FO task e31d917197404d69843eeb1b92312279 的既有异常。目标候选 SHA256 1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53，优先 RVA 0x1351F0，辅助 0x1DE4D1。当前尚未独立核实事件原件、PE/CodeView/PDB 身份，不能声称已有符号映射。
不复现崩溃、不重建候选、不改生产或默认测试、不运行脚本/外部调试器。只允许独有 artifacts/fp-crash-symbols 输出与显式只读诊断。保留原始 DLL/PDB 和历史。
