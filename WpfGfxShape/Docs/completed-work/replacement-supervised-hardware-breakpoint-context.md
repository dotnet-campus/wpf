# GA x86 硬件执行断点成对上下文取证

## GI 独立终态：GH动态证据成立；候选保存槽仅条件推导；下一采样仅设计
GI先登记handoff进行中，再亲读原件与当前代码；不是继承GH口头核证。仅更新本报告及三共享，无新报告、源码/诊断/生产修改，无build/publish/测试/沙箱/新运行。GH预算加载1+候选1保持历史，不追加崩溃。以下覆盖旧“下一只读分析”动作，不抹除历史。

### 独立身份、有效读取与配对
亲读artifacts/gh-candidate-context下load-control与candidate的file-identity全部6行、hardware分别3/22行、cleanup各4行。load task1f3f12ae65fb4b53bda90d2580d4a5a5/监督3168/子8052/TID7532/base6E740000；candidate taskdcfc9dd1d98c458d8675617a159b6575/监督3828/子9956/TID2960/base6D390000。两执行identity实际test SHA6D8DA8EB2CCD0F69A9FF5298FF6C701EFE868391A14D9DB8CA16BFBBCA242366、MVID0cd67192-981b-4022-ad18-33fabbe306cc、.NET10.0.13/X86/PointerSize4相同；candidate aot-identity及DR identity另核actual任务Native路径/SHA，不移绑default。
两实际复制句柄load1056/candidate1224的DISK1/GetFileInformation成功/Error0，Volume2759389505；Index分别3407872:296093及4980736:294667；Size0:3995648、Write31285785:3967683863，前后逐字段一致。同步Requested/BytesRead均3995648，pointer保存/恢复0，句柄SHA均1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53。不是{}或默认零身份。权限证据是这些API实际成功，不外推所有访问掩码/失败路径均验证。原hFile1076/1264关闭成功；复制在认证结束Dispose/IsClosed=true（非原生Close错误码回读）。代码357–469核有界全读、前后身份、路径SHA、稳定PE字段、合法ImageBase及两处文件/映射完整字节比较；authenticated事件证明此门返回，不是假定包SHA代实际身份。本轮未重新计算二进制哈希。
当前FH Raw/Log逐字段写context；Context先清结构再Get10017，失败即抛，OpenThread0x1A，布局716/16字节对齐检查；Write仅10010调试寄存器组，不写应用EBP/ESP/EIP/EFlags。Read按RPM实际BytesRead截取Hex，而非把零填充数组整段序列化。load hardware外层Tid0/Phase0仅表示未Arm，实际NativeTid7532在Data，不能误作线程认证0。
精确token输出116字节、事件PID9956/TID2960与已登记线程/模块/路径绑定后才Arm。CALL1351EB=E880030000、return1351F0=8BB568FFFFFF：Phase1/2分别EIP6D4C51EB/6D4C51F0、DR6=4294905841/4294905842、DR7=1/4，均Get成功/Error0。EBP50054196→0、ESP50054044→50054044；caller阶段P/本次S固定，初始快照没有被post覆盖。Hit核Owned/TID/firstchance/EIP/ExceptionAddress/DR6/DR7/phase；slot0禁用并清地址后启slot1，最终Paired=true且全DR清零读回成功/Owned=false。SameCallStack/PairEstablished是状态机加同ESP判定，不是独立展开全调用栈或排除同线程重入的证明；本原件仅记录一对，不能泛称所有调用都安全。

### 内存覆盖、异常与清理的真实层级
令call前S=50054044、P=50054196（P=S+152）。主ESP256有效范围[50054044,50054300)=[S,S+256)，不含S-8或S-4；初始[P]16范围[50054196,50054212)也不含两槽。额外initialESP-minus32的48字节实际有效[50054012,50054060)=[S-32,S+16)，明确包含S-8=50054036及S-4=50054040。caller-frame-storage192实际有效[P-160,P+32)=[50054036,50054228)=[S-8,S+184)，也包含两槽，不能与主ESP256混称。上述成功读均Error0、BytesRead等于Requested。
S-8四字节由0C000000变00000000，S-4由03000000变F0514C6D（返回地址6D4C51F0残留）；额外48与caller-frame首8互相吻合。主ESP256前后相同，[P]16前后均B8C4FB021C10940C603F670B00000000（[P]=02FBC4B8）。post当前EBP0的读失败299/BytesRead0/Hex空，与Get成功得到EBP0严格区分，不是缺失字段默认0。[P]是caller自己的保存链，保持不变不能证明callee自己的保存槽未被改。残留不是执行中槽观测，不能由零值直接证明保存槽覆盖。
两个原始AV context各122行全文亲读：首个EIP6D4C51F0/EBP0/ESP=S有效，第二EIP6D56E4D1/EBP192581992/ESP50052772有效；失败内存读有299/0字节，成功块有实际长度。return清DR读回后，两硬件异常各一次Continue65538成功；首AV readFFFFFF68及后续RVA1DE4D1 first/secondchance readFFFFFF7C均Continue2147549185=NOT_HANDLED成功，未吞原AV。最终子C0000005/secondchance1/未超时，监督0仅满足崩溃取证合同，非产品通过、非工具数字exit。load子0/secondchance0/监督0。两cleanup Detach/ExitEvent=true、Wait0/两Close成功，无Terminate；不证明业务surface/device/callback finally完成，DR日志止首次Create before，无完成TRX。

### 同SHA有界静态对照与唯一下一采样设计（本轮不实施）
FX原件artifacts/fx-callee-tail/result/result/fv-pe.txt1–37亲读：输入同SHA/I386/PE32/.text raw，[135570,1356F0)384字节/offset134970，片段SHA F6EF760CA4ADA31AAE1C4E4F33F010A70528FB3C319396FD6735CB551A4356F0；不是完整函数边界。GH实际call字节rel32=380，1351F0+380=135570。入口连续55(135570)、8BEC(571–572)、57/56/53(573–575)、83EC44(576–578)，所以135573是MOV EBP,ESP之后、push EDI之前的可靠边界；135579是完整分配帧后的边界，不凭RET模式猜入口。
用本节S为call前ESP（不同于FW历史以callee入口ESP命名S）：正常CALL使ESP=S-4且[S-4]=ret，PUSH EBP使ESP=S-8且[S-8]=P，MOV建立F=EBP=S-8。继续三push/sub44后ESP=F-50h。尾段从已连续手解13566E接续到135694:8BC6、696:8BD7、698:8D65F4、69B:5B、69C:5E、69D:5F、69E:5D、69F:C3；LEA ESP,[F-C]后恢复EBX/ESI/EDI，pop EBP前ESP应F，RET从F+4取ret，最终ESP=S。这只是该序言与正常尾声且帧保持前提下的账本。GH未在入口观测F/ESP，故S-8只能称“按该序言及正常路径推导候选槽”；异常/跳转/内部call全路径未核，不能升格为已证savedEBP覆盖或栈失衡。动态EBP归零区间成立，但首坏写/具体内部call未定位，不归咎整个Silk库。
下一唯一任务是同SHA、同正式FH显式角色的一次分阶段帧保存/恢复硬件取证，需另授权实现及运行；现有HardwareSample只有before/after两阶段，不能直接通过改两个RVA假称已支持。沿用既有身份/线程门，先1351EB记录P/S；命中后复用slot0到B+135573，slot1保持B+1351F0禁用。135573执行前采F/ESP、绝对[S-8,S+4)及[F,F+8)、入口原字节；这里应F=ESP=S-8，[F]=P、[F+4]=B+1351F0。随后slot0复用B+13569E，slot1仍禁用；命中在pop EBP之前采真实EBP/ESP及初始F槽、当前[EBP]/[ESP]、F+4返回槽，核ESP=EBP=F与[ESP]=P。再清slot0并启slot1，返回位置采恢复EBP/ESP及同绝对槽，清全读回；每阶段严格PID/TID/代次/first/EIP/address/slot/phase，保留同次锚点并拒意外重复/卸载，不能把未到尾声/异常路径强配成正常返回。新增位置必须再核实际加载内存入口135570–578及尾部135694–69F完整连续字节，不仅核磁盘；地址用实际base，不固定GH旧base。
判别边界：post序言已不符P/S表示入口保存/建立前或此过渡已异常，不指定首写；入口正确而尾声固定F槽变说明两采样间槽值改变，但仍未定位首次写；F槽保持而尾声EBP/ESP偏离提示帧寄存器/恢复路径异常；尾声各项正确而返回EBP错误则把疑点限定pop/ret至返回上下文区间。短暂污染后恢复、非正常尾声或未命中均如实未决，不补造观测。一次快照每阶段固定有界读取/实际长度，失败不当零；继续沿用仅写DR、每事件一次Continue、非自有异常NOT_HANDLED与清DR/终止退出分层。
不重跑既有GC基础DR控制或GH加载控制冒新证据；但新四阶段槽复用/锚点读取从未实测，另授权实施时须先做一次只针对新增状态转换的正常控制（现有exe角色，非第三Host，原始Get/Set/readback/正常退出及清理）；不通过即不候选，不能拿旧GC单点控制代替。候选仅一次，不附带多个ABI/SDK泛任务；本轮控制和候选均0。不用INT3、代码写入、机器码、外部反汇编器或生产修改。

### GI阅读登记与终态
规则27及完整方案120全文、FW/FZ报告71行分段读（21–25补首次截断）；FX原件37全文。旧GA163行分段覆盖（GH顶部长段截断以16–20补，旧历史保持），当前FH重点120–345、357–469、884–929、970–1000、1008–1042、1100–1116亲读，首次长段截断的关键Layout180–205/923–934等补读，不称FH1116全文。candidate events751仅1–3、548–570、625–630、697–725（714–725补截断）、744–751；load events277仅1–3、244–277（274–277补截断）。硬件/文件身份/cleanup/两个AV context/DR6及aot-identity全文，主配对两长行完整可见；没有另一个load AV context，不能虚构。FV旧提取路径本轮读取不存在，未声称亲读旧FV83行；调用点以GH实际映射认证和现场EIP原字节独立核，静态callee用实际FX原件。共享仅最新受改节，七百余行历史中段未全文。写后本报告全文及三共享最新节/最后补记将分段回读；覆盖实绩以末次补记为准。
默认FH隔离、CF/CX/CY/既有修复保持；Available/HW消费恢复/full四格/x64Event0/E_HANDLE等仍开放，生产修复0，完整替换未完成。

GI最后补记：本报告写后188行已按1–9/10–17/18–23/24–30/31–39/40–49/50–64/65–79/80–94/95–109/110–124/125–139/140–154/155–169/170–188全文回读，无截断；三共享GI最新受改节handoff1–6、summary1–6、issues1–6已回读，旧中段历史未全文且未改。FW/FZ21–25已补读，现71行覆盖；FH初次120–205、206–282、283–356存在截断，已补关键Layout/配对/认证门，不把范围声明冒充逐行全文。无新运行或构建；下一采样仅设计。本最后补记随后单独回读。

## GH 终态：修正版加载控制通过；唯一真实候选CALL/return成对命中，EBP在调用区间归零，产品仍崩溃

### 唯一候选实证与因果边界
候选task **dcfc9dd1d98c458d8675617a159b6575**，监督3828/子9956/阶段及两命中TID2960；actual test SHA **6D8DA8EB2CCD0F69A9FF5298FF6C701EFE868391A14D9DB8CA16BFBBCA242366**、MVID **0cd67192-981b-4022-ad18-33fabbe306cc**、.NET10.0.13/X86/PointerSize4。实际Native模块base **6D390000**（1832452096）/4055040，SHA **1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53**，非default。原LOAD_DLL hFile1264关闭成功，duplicate1224持有到认证结束再Dispose/IsClosed=true。前后Volume2759389505/IndexHigh4980736/IndexLow294667/SizeHigh0/SizeLow3995648/WriteHigh31285785/WriteLow3967683863一致；DISK1/身份成功、同步3995648字节、pointer0恢复0/SHA匹配。合法surface CreateDevice/RenderTarget/ColorFill三步HRESULT0，iteration0/version2443313159/fallback=false/dpi111.5/123.25；精确token事件PID9956/TID2960/base/SHA及模块/线程登记匹配，既有stablePE/合法ImageBase及CALL1351EB=E880030000/MOV1351F0=8BB568FFFFFF全部成功才Arm。
CALL前EIP **1833718251=6D4C51EB**、EBP **50054196**、ESP **50054044**、DR6 **4294905841**/DR7 **1**；返回位置EIP **1833718256=6D4C51F0**、EBP **0**、ESP **50054044**、DR6 **4294905842**/DR7 **4**。同线程/同ESP/phase严格匹配，PairEstablished/Paired=true。slot0命中后DR0=0、DR1=1833718256/DR7=4读回；return后DR0..3/DR6/DR7全0读回Verified=true/Owned=false，两单步分别恰一次Continue65538成功。EIP/EFlags应用组未由诊断写入；flags582→580为系统读回，未改TF/RF。
初始EBP地址50054196的16字节保存槽前后均 **B8C4FB021C10940C603F670B00000000**，saved caller EBP小端 **02FBC4B8**，没有观察到此槽被清零。ESP256字节前后完整相同；initialESP-32的48字节前 **1500000001000000010000000000000000000000000000000C00000003000000387BE10ED8C3FB020100000000000000**，后 **4DF6861D00000000E04C670BFCC5FB020000000048C6FB0200000000F0514C6D387BE10ED8C3FB020100000000000000**；其中initialESP-4返回残留 **F0514C6D**（6D4C51F0），initialESP-8为0，但仅为返回后残留，不自动认证为特定callee保存槽。caller-frame窗口initialEBP-160的192字节首8由0C00000003000000变00000000F0514C6D，后184字节相同。返回时读当前EBP0失败Error299/0字节，读初始EBP仍成功，不是日志丢失。
动态证据将EBP非零→0限定在CALL前至返回位置的执行区间，并与后续MOV [EBP-98h]读FFFFFF68相符；不能据此确定首个坏写、callee内部哪个指令、异常展开/回调是否参与，不能把ESP端点相等夸大为全程栈平衡或其它调用全部安全。下一唯一根因步骤：只读核对该SHA的callee135570及其实际返回/EBP恢复路径与当前成对窗口，定位保存/恢复合同；本轮不修改生产、不追加运行。

### 真实异常、清理与预算
firstchance AV在6D4C51F0/TID2960/read **FFFFFF68**（4294967144）；随后6D56E4D1 firstchance及secondchance readFFFFFF7C。三个真实AV均Continue **2147549185=DBG_EXCEPTION_NOT_HANDLED**成功/Error0，未吞异常。最终子EXIT_PROCESS **3221225477=0xC0000005**、secondchance1/TimedOut=false；监督结果 **0/CleanupConfirmed=true**仅取证合同满足，不是产品通过。cleanup4行全文：Detach=true/ExitEvent=true、Wait0、两Close成功/Error0，无Terminate。DR日志止首次Create before，业务finally未完成，不能称surface/device/callback正常释放；进程与自有调试句柄已回收。未产生child.trx，不造MTP完成/TRX或工具数字exit。
原件artifacts/gh-candidate-context/candidate/Results/fh-candidate；hardware22/fileidentity6/cleanup4/DR6/context-av-1和2各122行全文亲读。native-events751行仅1–3、548–570（非门前段曾截断）、625–630、697–725、744–751亲读；关键认证/两个命中/三个真实异常/终态段均完整，未称事件全文。预算实际load-control1+candidate1，无full/COM变体/能力跳过或额外重试。未改任何源码/生产/default/DR断言/禁改区，无新增Host/工程/脚本/INT3/机器码。复用GG正常分析器最终包与GG已完成无FH/DR的normal发布，不冒新build；本轮build0，生产修复0。Available/HW消费及恢复/full四格/x64Event0仍开放，完整替换未完成。

### GH最终阅读与回读登记
本报告旧143行按1–23、24–43及40–43补截断、44–55、56–67、68–79、80–91、92–107、108–123、124–143覆盖；新增GH1–22写后全文回读，旧历史未改，不反复全长历史。规则27/工作项31/方案120分段读，方案40–79及80–120补首次截断。FH亲读硬件状态机、fileidentity/PE/moduleidentity、事件认证/异常/清理与实际testSHA/MVID/runtime路径；79–150、341–400、406–450、581–632等长段有截断，已补关键认证/配对/异常门398–405、449–495、985–995、1108–1116，但不冒称FH1116行全文。DR173行全文，NativeRun1–65，GG最终preflight1/aot.sha2561/runtimeconfig21/Build.props9/Normal.Build.props8全文；准确输入为既有GG clean com-acceptance-package，package-files418行只读1–30及395–418，不称中段全文。source/package与两次回收结果目录分离，未上传父目录/回收/PublishDir，未执行clean或发布。此前normal Exit0只引用GG报告，不假称GH亲读缺失的旧构建日志。三共享最新受改节已写后回读，历史中段未全文；本最终补记随后回读。后续不追加执行。

### 加载控制与执行过程（已完成，保留进度记录）
复用GG最终包未改源码/未重建；close-final-preflight全文核Verified=true/CandidateLoaded=false，test SHA6D8DA8EB2CCD0F69A9FF5298FF6C701EFE868391A14D9DB8CA16BFBBCA242366/MVID0cd67192-981b-4022-ad18-33fabbe306cc，FH+DR方法齐全，Core/Desktop10.0.13/X86，aot.sha256匹配FE失败适配SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53，非default。此前根artifacts查空是路径前缀误差，实际WpfGfxShape/artifacts存在，未据此停止或重建。
唯一load-control task1f3f12ae65fb4b53bda90d2580d4a5a5：监督3168/子8052/阶段TID7532/base6E740000/映射4055040。原hFile1076复制1056；DISK1/身份成功Error0。前后Volume2759389505、IndexHigh3407872/Low296093、SizeHigh0/Low3995648、WriteHigh31285785/Low3967683863逐项相同；同步ReadFile3995648/3995648 Error0，pointer保存0/恢复0，直接句柄SHA匹配。既有稳定PE/合法ImageBase/CALL E880030000/MOV8BB568FFFFFF及模块/线程阶段门认证成功，无Create/Arm。原句柄Close成功，复制认证后Dispose/IsClosed=true，阶段Continue一次成功；正常子EXIT0/secondchance0/未超时，监督0/CleanupConfirmed=true，Detach/Wait0/两Close成功。工具仅报完成，无工具数字exit。原件artifacts/gh-candidate-context/load-control/Results/fh-load-control；identity6/hardware3/cleanup4全文，events只1–3及244–277（270–277补截断）亲读，未称全文。
GG旧task3c4a0c60d1694aeeb0fa2a3bff02a7ef身份认证实际成功，仅原值日志{}；GH现已取得修正版原值，不重写为旧认证失败。加载证据门通过，接续同包一次合法DR候选；未执行前不预填call/return。生产修复0，禁改区未动。

## GG 终态：唯一实际加载控制正常退出，原始 fileID 日志缺失使严格门未闭；候选0
先登记handoff GG进行中，再最小增加FhCrashEvidence显式load-control/load-ready。同一正式exe子进程NativeLibrary.Load原候选，只运行合法loader，不调用导出、不Create、不Arm、不卸载驻留DLL；同一debugger LOAD_DLL复制所有权、AuthenticateCandidate同步ReadFile/SHA/PE/映射指令及Dispose路径，不另造假认证。只改FH条件文件；NativeRun/生产/项目打包/DR业务及断言/ExitDiagnostics/VisualCache/historical/捕获/CF-CX-CY未动，生产修复0，无第三Host/脚本/机器码/INT3。

### 实际任务、身份与正常终态
唯一加载task **3c4a0c60d1694aeeb0fa2a3bff02a7ef**，监督7964/子2508/阶段TID7416，实际.NET10.0.13/X86/PointerSize4。执行test SHA **8EA6083827262D634ADF4D126A0A181DBF8DE8E19E005053C6DCA3CFB6D3599E**，MVID **639cd396-ee79-4934-80b0-068231678b96**。actual模块为该task Native/wpfgfx_cor3.dll，base **6E740000**（1853095936）、映射SizeOfImage **4055040**，实际SHA **1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53**。这是GG独立实测，不回填GE同数值base旧任务；FE失败适配非当前default。
原件：artifacts/gg-handle-runtime/load-control/Results/fh-load-control，完整回收native-events.jsonl277行、file-identity.jsonl6行、hardware.jsonl3行、cleanup.txt4行。事件247记录原hFile **1096/0x448**、duplicate **1100/0x44C**；249原hFile Close成功/Error0，250该LOAD_DLL Continue成功。257复制句柄Dispose后IsClosed=true，259阶段事件Continue一次成功。复制未入逐事件handles；实际原/复制关闭顺序与GF生命周期修复一致，未观察重复原生关闭；运行版using+显式Dispose会重复调用幂等Dispose，并非第二次原生Close，最终源码移除using仅保留finally Dispose，未再加载验证。
同一duplicate GetFileType=1/DISK、GetFileInformationByHandle成功/Error0；文件长度 **3995648**，pointer保存0，同步ReadFile单次请求/返回 **3995648**、成功/Error0，恢复0/Expected0成功/Error0，同句柄字节SHA匹配。认证走既有I386/PE32/SizeOfImage、稳定映射header及CALL1351EB=E880030000、MOV1351F0=8BB568FFFFFF严格检查并成功；没有放宽ImageBase或指令门。正常EXIT_PROCESS **0**、TimedOut=false、SecondChance=0；supervisor-result **0/CleanupConfirmed=true**。cleanup全4行：Detach=true/ExitEvent=true、Wait0、两自有Close成功/Error0。工具仅报沙箱完成，不提供数字进程退出码，以上两个0分别来自子事件与监督原件，绝非产品通过。

### 精确停门及后续修正（不得回填）
**严格加载身份取证门未通过**：file-identity before/after的Identity均为 **{}**，原因FileIdentity内部字段不被System.Text.Json IncludeFields输出。源码确实核volume/index/size/write-time前后一致且未抛错，但缺实际 **Volume、IndexHigh/Low、SizeHigh/Low、WriteHigh/Low** 数值原件，不能把成功布尔当完整身份原始证据。没有加载API/权限/同步读失败，不伪报句柄不可读。加载控制运行版hardware标签沿用“DR first legal Create before”，实际子arguments=load-ready且无DRCreate/Arm；此为错误标签，不是真实Create。已最小改匿名对象显式身份字段和load-only标签，且移除重复Dispose调用；这些最后修改仅编译/离线预检，**没有新加载原件**，不补造旧fileID。
预算加载1/候选0，无部署错误纠正，无候选重试/full/COM变体/全direct，无新TRX；严格门缺证后停止，不以监督0越门Arm。CALL前/返回前原始EIP/EBP/ESP/DR6/DR7及保存caller/返回槽成对证据本轮 **未取得**，未证EBP归零根因。

### 包、最后源码验证、风险与下一唯一动作
独有artifacts/gg-handle-runtime/{Build.props,obj,bin,publish,com-acceptance-package}，显式PublishDir，原正式依赖图，x86 Release selfcontained Core/Desktop10.0.13，FH+DR，正常分析器未NoWarn/关闭规则。原候选复用GF正式包，未重建；clean package上传与load-control回收目录分离，未清理本机源码或历史。包aot.sha2561行/runtimeconfig21行全文已读。执行前离线preflight Verified=true/CandidateLoaded=false，FH Run/CandidateReady/HardwareSample Arm/Hit及准确DR TestMethod元数据齐全，执行SHA/MVID同上。
修日志后final-preflight SHA3FFFFFCBE82E3F7499ABA92B32CAB44F15FE1A79583462CCF43DF626F5A080D6/MVID27ccc634-beb5-4bdd-8644-cd4be1728e4e；去重复Dispose后close-final-preflight SHA **6D8DA8EB2CCD0F69A9FF5298FF6C701EFE868391A14D9DB8CA16BFBBCA242366**/MVID **0cd67192-981b-4022-ad18-33fabbe306cc**，均Verified=true/CandidateLoaded=false/.NET10.0.13/X86，已全文回读，绝不绑定首次加载结果。共3次不加载候选离线预检，仅一次加载。
最后源码后FH+DR发布成功；无FH/DR的Normal.Build.props独有normal/{obj,bin,publish,com-acceptance-package}正式发布 **Exit0**，日志1–6及55–60回读，正常分析器既有MSTEST警告保留。默认入口未改、整文件#if隔离；这是无符号发布验证，不是默认运行测试或产品通过。未测读取失败/restore失败/卸载/timeout/设置与读回失败清理分支，SafeFileHandle IsClosed不是原生Close错误码回读；不宣称全分支关闭成功。
下一唯一动作：**另授权一次最终显式身份日志版本load-control**，先取得前后volume/fileID/size/time原值及生命周期/正常退出完整证据；严格门全部通过后才另按授权的一次真实候选Arm成对采样。本轮停止，生产修复0，Available/HW恢复/full四格/x64Event0均未闭，完整替换未完成。
阅读登记：执行规则27行/工作项31行、方案分段（首次1–92截断，补40–89及90–120）已读；GA旧报告主要分段已读但1–23、46–72首次截断未全部补齐，不冒称历史全文。当前原件file-identity6/hardware3/cleanup4全文，native-events只亲读1–3、244–277（尾段截断后273–277补齐），不称277行全读；全部文件原样回收。四文档最新受改节写后回读，历史中段未全文，旧GF/GE历史保留。

## GF 终态：已修确证的跨事件句柄提前关闭；GE收口，未追加候选
GF最终补记：本报告写后1–123已分段全文回读，19–35首次截断的34–35已单独补读；handoff1–9、summary1–7、issues1–7最新受改节已回读。最终normal日志Exit0与GF preflight全文已核；共享历史中段未全文读。旧两次异常栈/工具响应缺件未补造；本轮停止，未追加候选。本补记随后单独回读。
先在本报告与handoff登记GE真实进度/GF进行中。仅修改FhCrashEvidence.cs的FH_CRASH_EVIDENCE显式诊断；生产修复0，其它入口/DR业务/断言/ExitDiagnostics/VisualCache/historical/捕获/打包项目/CF-CX-CY不变。无新Host/工程/脚本/INT3/机器码。本轮候选/DR业务/崩溃/control/full运行0，句柄诊断运行0；只执行一次不加载候选的离线preflight。未伪造TRX。

### GE实际原件及调用层级
GE至少存在5份独立执行结果：auth-readonly、preflight、candidate、corrected-candidate、final-preflight；不能再称GE仅一次候选。原始工具响应/调用流水未保存在此目录，故不据结果目录猜完整工具调用总数或数字退出码。
只读认证task4306f2a1c8b34d339c4918436fc2bce9，auth-readonly.json全文已读：I386/PE32/Size4055040/base6E740000，Read364字节成功，ArmAttempted=false。raw/mapped仅十进制offset310/311（0x136/137）有字节差异，属于完整ImageBase字段offset0x134的10000000→6E740000；稳定字段一致。GE已将整头比较改为稳定字段比较，仍核实际SHA、精确CALL/MOV映射字节与线程门，不回填GD旧base，也不把此只读加载算候选DR运行。
候选1 task9a580414c3c34bb0956a6d471a527d56：监督6840/子4740/阶段TID5564，test SHA4C3AAC207FBBCC34CC8574487BACAF76AC5D8BC6618A9D6165610A9CA4AA96B1/MVIDfda779d1-29e1-43ae-9460-075268a1c708。候选2 taskd1a5175d4de948ba821992d9c94330dc：监督6548/子2368/阶段TID2264，test SHA1F0956CE04B0A6E918A323914604364475FF6DB3586F636EC70EA307904ADB23/MVID0e42d734-336b-407a-bdce-9a260cc58c59。均.NET10.0.13/X86，actual LOAD_DLL base6D390000/size4055040；各实际task Native路径与aot-identity、DR identity一致，SHA均1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53。不要混用只读任务6E740000或GD6E630000。
两次hardware各仅1行candidate-authentication-input；各DR日志6行全文已读，合法CreateDevice/RenderTarget/ColorFill HRESULT全0，首次Create前暂停，未Arm/未命中/未配对/无clear回读。两份事件各1–3与548–560已读，最后阶段文本的base/SHA/TID对应，Kind8各恰一次Continue65538成功/Error0；不称560行事件全文。各cleanup21行全文已读：Terminate成功，16次清理Continue成功，Detach成功/ExitEvent=false，Wait0，两自有Close成功，Error全0。没有正常EXIT_PROCESS值、监督结果或工具数字exit，不把Terminate参数1/Wait0当exit，不称DR业务finally正常释放。
用户交接分别为RandomAccess不支持seeking与ReOpenFile Error6；当前两份hardware未写错误，目录无error/工具响应原件，未亲读到两次完整异常栈。此异常类型按交接保留，不假称已独立核栈。缺的最小原件是这两次原始工具错误/栈（含API及行号）与工具退出元数据；不再用重复候选补旧证据。GE preflight与final-preflight各1行全文已读，均CandidateLoaded=false/Verified=true，前后test SHA/MVID分别对应上述两版；它们证明离线内部方法及DR TestMethod元数据门，不证明句柄可读。

### 精确源码根因与最小修复
LOAD_DLL_DEBUG_EVENT的hFile属于调试器进程句柄表；现有DuplicateHandle(-1,file,-1,...,DUPLICATE_SAME_ACCESS)使用GetCurrentProcess伪句柄，源/目标均当前调试器，不应换目标进程。API成功未记录实际权限，不能声称已实测读取权限或异步标志。原hFile在加载处理finally显式Close一次，无SafeFileHandle双重所有权。
确证缺陷：复制retained既加入retainedFiles字典又加入handles；逐事件finally遍历handles的非PROCESS_INFORMATION成员，LOAD_DLL当次即关闭复制句柄，字典仍留数值到后续OutputDebugString/Arm。故认证拿到已失效或被复用的数值；这足以解释身份所有权失效，不凭ReOpenFile6猜当时被复用为何种对象，也不把RandomAccess seeking异常当真实DLL为不可seek文件的证明。之前并非有效同文件读取API选型实验。
现复制句柄只由独立SafeFileHandle(ownsHandle:true)字典拥有，不加入逐事件集合；取出后移交局部using，认证完成或抛错后Dispose一次；未到认证时UNLOAD_DLL或外层异常/终止/timeout finally统一Dispose并清空。事件原hFile仍按原事件finally显式Close一次。Continue仍在既有finally；事件与复制身份所有权分离，不再缓存已关闭值。
移除ReOpenFile/RandomAccess链。对同一复制打开文件GetFileType必须DISK、GetFileInformationByHandle核volume/index/length/write-time；64MiB有界；SetFilePointerEx保存当前位置、归零、ReadFile同步合法读取全文件并回查身份，finally恢复共享文件指针。SHA直接取这些字节，另核路径SHA一致，路径不是身份来源；PE稳定字段/实际SHA/映射CALL与MOV/实际线程认证不放宽。读取失败保留真实Win32错误并安全拒绝，恢复失败Trace并在正常读取路径抛错，不覆盖原读取异常。没有按路径fallback掩盖句柄失败。

### 最终验证与下一门
独有artifacts/gf-file-identity/{Build.props,obj,bin,publish,com-acceptance-package}，显式PublishDir，既有正式白名单PackageSelfContainedComAcceptance，源包与回收preflight分离。未清本机源码或GE原件。FH+DR x86 Release selfcontained10.0.13发布成功；包aot.sha256及runtimeconfig21行全文核Core/Desktop10.0.13。正常分析器，无NoWarn/关闭规则，既有MSTEST警告保留，不称零警告。
GF唯一preflight使用现有exe纯C#离线PE元数据读取，无Native加载/DRCreate/Arm；回收artifacts/gf-file-identity/preflight/preflight.json全文核Verified=true/CandidateLoaded=false，test SHAECB5459D85E2360519C9A6ACD7DD524CEE5EAF04ED1502B54BB43E8A04F26A1B/MVID3a4e6f69-0919-4617-b3a0-6787d42b9a73/.NET10.0.13/X86，包SHA仍1893370B…；工具只报完成无数字exit，不造监督或MTP结果。
最后源码后normal独有Normal.Build.props/normal/{obj,bin,publish,com-acceptance-package}无FH/DR定义，正式x86自包含10.0.13发布，最终日志1–4明确退出码0及57–60产物路径已回读。源码整文件#if FH_CRASH_EVIDENCE及NativeRun既有条件分派保持默认隔离；未运行默认业务矩阵，不拿构建当产品通过。未修改生产DLL。
下一唯一候选重试目前不能宣称可过门：源码提前close根因已修、发布与离线元数据门已过；真实LOAD_DLL复制句柄的DISK/权限/同步读取/seek与恢复、失败清理实际API结果仍未实测，设置/读回失败/timeout/卸载/同步读取失败分支未测。若要求候选前实证，只能另行单次现有FH纯C# LOAD_DLL只读句柄诊断（无DRCreate/Arm），记录原/复制数值、type/identity/read及close序列；不是无限API替换或追加候选许可。当前不执行该诊断，不放宽认证直接候选。Available/HW消费与恢复/full最终四格/x64Event0全部开放，完整替换未完成。
阅读边界：规则27行/工作项31行/完整方案120行已读；GA报告全文回读与四文档最新受改节/最后补记按本轮实绩登记，不继承历史覆盖；共享七百行历史中段不全文重读。

## GD 最新终态：安全候选Arm已接入，唯一候选在PE/映射认证门拒绝，未命中未配对
GD最终补记：新增报告1–14及三共享最新受改节已写后回读，旧报告87行已写前逐段全文覆盖，未改旧历史；不冒称整份99行写后重读。共享七百余行中段未全文读。normal最终日志4行Exit0已回读，final-preflight原件已读。唯一候选认证拒绝，停止追加运行；本补记随后回读。
先在handoff登记GD进行中；不重复GC控制，不新增报告。本轮代码仅FhCrashEvidence.cs与DrInteropAbiTests.cs中FH条件诊断调用。NativeRun/生产COM/DRDS断言/ExitDiagnostics/VisualCache/historical/捕获/打包/CF-CX-CY均未改。合法DR的首次Create前，FH独有随机token+实际模块base+文件SHA通过OutputDebugString提供debug暂停事件PID/TID；仅确认已CREATE_THREAD/CREATE_PROCESS登记的该事件TID，不拿LOAD_DLL TID猜执行线程，不盲设其它线程。无FH或无专属token完全不进入握手，原surface与能力分支/预期/cleanup不改。
安全Arm实现：实际LOAD_DLL hFile规范路径保存base/SizeOfImage/SHA，阶段用独立实际路径重读SHA，核I386/PE32/SizeOfImage/地址界限、映射头与文件、两处文件及映射CALL E880030000/MOV8BB568FFFFFF；全部通过才调用Arm。实际路径重读未持有原LOAD_DLL文件句柄到Arm，存在换件竞态剩余风险，不称强身份闭合。硬件只写debug组，DR0启用/DR1保留；call匹配后DR0地址清0、DR1保留并启用；return按DR6/EIP/TID/phase/DR7及DR0=0匹配，清全并回读。增加caller-frame原始192字节窗口；同线程且返回ESP同初始才Paired，未匹配真实异常NOT_HANDLED，Continue finally保留。设置/读回失败和超时未实测。
唯一候选task4865eace578847c3b85ca9a7723e7318，原件artifacts/gd-candidate-arm/candidate/Results/fh-candidate。监督4208/子10808；实际test SHA1AD1802DA97A32379C4491C0C69AD2E9276A3CA71D7670DE1EDEE53A39A0C40C/MVIDafba1e34-89e3-44e3-9b7b-0d45b1d4cf1a/.NET10.0.13/X86。模块实际路径C:\CodingAgentSandbox\Tasks\4865eace578847c3b85ca9a7723e7318\Native\wpfgfx_cor3.dll，LOAD_DLL base1851981824=6E630000/size4055040/SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53；非当前default文件输入猜测。合法CreateDevice/CreateRenderTarget/ColorFill HRESULT全0，首次Create参数原件完好；阶段Kind8的PID10808/TID2552/base6E630000/SHA匹配，初始断点已接受，确为真实DR线程阶段，不仅managed Thread2。
认证中AuthenticateCandidate抛BadImageFormatException，尚未hardware.Log/Arm；hardware.jsonl不存在、无任何DR set/命中/清零回读，CALL前/返回前EBP/ESP/保存槽/返回地址全部未取得，不能预填旧FT EBP0，未证动态返回、首次坏写或栈失衡。原版异常未标具体校验项，不能从此通用异常断言究竟PE属性/映射头/文件或映射指令哪一项失败；没有API拒绝权限的证据，不能伪造权限阻塞。停在安全门，不追加第二候选/控制/full/COM变体。
事件559阶段原文、560该Kind8唯一Continue65538成功/Error0已读。cleanup21行全读：Terminate=true/Error0、16次CleanupContinue=true/Error0、Detach=true/ExitEvent=false、WaitForExit0、两Close成功/Error0。没有正常EXIT_PROCESS退出值、没有supervisor-result，工具只报告未处理异常与回收路径，无数字进程退出码；不能用终止参数1或Wait0填exit。Arm前无DR自有状态，不虚报clear。目标Create未继续完成，业务finally未完成；监督强制终止回收进程，不称DR对象正常释放通过。
构建：独有artifacts/gd-candidate-arm/{Build.props,obj,bin,publish,com-acceptance-package}，显式PublishDir，正式既有白名单自包含x86/Core+Desktop10.0.13，复用GC正确候选未重建候选。首次编译CS0136局部assembly遮蔽已修；最终显式发布和normal无FH/DR独有obj/bin/publish发布Exit0。正常分析器，无NoWarn/关闭；ErrorsOnly仅过滤显示、不压分析。正式包aot.sha2561行与runtimeconfig21行全文已读，Native/Contracts/MTP/runtime由原打包依赖图提供；仅上传clean package，源和回收分离，不删除本机源码。
非候选preflight只读角色运行了两次，不加载候选：实测反射核FH Run/CandidateReady/HardwareSample Arm/Hit与准确DR TestMethod齐全，并验证aot.sha256。不是离线PE元数据全方法认证，不能称完全满足该离线门。首次preflight绑定上述执行SHA；拒绝后只补具体认证错误原始值/caller窗口与配对完成门，未再候选。最终只读preflight.json已回读：SHA9B9B449756EFF492EC357DB9DA2FB92B805EE1421DBB4AA7D789A0BF5277F731/MVID0ce203d7-fc04-4706-b93f-949272ecb50e/.NET10.0.13/X86/Verified=true，不把最后诊断exe身份绑定旧候选结果。
下一唯一动作：在同FH只读设施补真正离线元数据与具体PE/映射认证差异输出，先确认拒绝的字段及合法loader变化，保持实际SHA/精确原字节/线程阶段安全门；另授权才允许新的候选Arm成对取证。本轮交付可编译实现和真实拒绝原件，目标成对取证未完成，不绕门强跑。
阅读登记：当前规则27行、完整方案120行已读；GA旧报告87行分段全文已读；FH硬件状态机/事件/清理与DR合法surface已审，未声称FH全文件全文。共享只读顶部受改节，七百余行历史未全文，不继承历史覆盖。候选native-events只读1–3、495–499、548–560，不称560行全文；DR6行/cleanup21行/preflight1行已全读。Available/HW消费/恢复/full最终四格/x64Event0/E_HANDLE/media/resources均开放，完整替换未完成。


## GC 最新终态：原始硬件控制通过；候选安全Arm设施未接入，候选零任务
本节覆盖GB的下一动作，不覆盖历史。已先在handoff登记GC进行中。仅FH显式诊断源码修改：Utf8JsonWriter逐字段记录EIP/ESP/EBP/EFLAGS/DR0..3/DR6/DR7，保留Get/Set成功及Error、before/set/readback/hit/cleared阶段；不依赖IncludeFields暴露internal。事件清DR置于try，Continue置于finally，使Clear抛错也执行该事件一次Continue，再向外传播进入终止/等待清理；未吞异常伪通过。原DR匹配、真实异常NOT_HANDLED、默认隔离均不变。未修改NativeRun其它入口、ExitDiagnostics、VisualCache/historical/捕获/WindowQ、项目打包、生产COM/DRDS或CF/CX/CY。

### 唯一实际正常控制
任务9cce32f3eebd40d6b1f4742c116997ad；原件artifacts/gc-hw-context/control/Results/fh-hw-control。工具只返回沙箱完成及结果目录，无数字进程退出码；下列0来自实际EXIT_PROCESS和监督自报，不能代替工具退出码或产品通过。
监督9212/子2664/唯一认证TID3048，.NET10.0.13/X86/PointerSize4，实际test SHA C1F4C0B663DDFFC2C9D3A3CE9B7CE534DE1EB7F26C7F73CDCC7706828FA7DCD4，MVID d889b708-f616-4f9d-8155-377069b7fc7c。实际系统模块C:\Windows\SysWOW64\kernel32.dll，base1987641344/RVA130544/target1987771888=767AFDF0，SHA BAFD9E061964642802D62F1EB694E7E4AF60DE739A8DF1A492F90006DC5680F3；本地句柄规范路径与remote一致。
before-get：EIP1963896532/ESP47703168/EBP47703260/EFLAGS530，DR0..3/DR6/DR7全0。set及set-readback-raw：Succeeded=true/Error0，DR0=1987771888、DR1..3=0、DR6=0、DR7=1，Verified=true。
命中：firstchance80000004/TID3048/address767AFDF0，EIP1987771888/ESP47704076/EBP47704896/EFLAGS582/DR0同target/DR1..3=0/DR6=4294905841/DR7=1；DR6按原掩码E00F仅slot0命中。ESP256字节、EBP16字节、EIP16字节、initialEBP16字节及initialESP-32的48字节均读取成功/Error0/实际长度齐全；原指令64A120000000C3CCCCCCCCCCCCCCCCCC，非修改字节。[EBP]原字节30ECD7022D35C3080000000000000000；不将系统控制样本当候选栈证据。
clear：Set=true/Error0，cleared-readback Get=true/Error0，DR0..3/DR6/DR7全0，EIP1987771888/ESP47704076/EBP47704896/EFLAGS580；Phase3/Paired=false/Owned=false。命中事件恰一次Continue65538成功/Error0。EXIT_PROCESS实测0，TimedOut=false/SecondChance0；supervisor-result0/CleanupConfirmed=true。cleanup4行全读：Detach=true/ExitEvent=true、Wait0、CloseHandle38C及358成功，Error全0。此正常硬件控制原始证据门通过，不追加控制。

### 构建与包
独有artifacts/gc-hw-context/Build.props/obj/bin/publish/com-acceptance-package，明确PublishDir，正式白名单PackageSelfContainedComAcceptance生成clean包，x86自包含10.0.13，FH_CRASH_EVIDENCE+DR_INTEROP_ABI，正常分析器无NoWarn/关闭规则。发布成功；只上传正式包到全新任务，不上传源/obj/历史Results，未清理本机源码或回收目录。实际aot.sha256全文1行=1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53，控制identity仅认证包内候选文件，未认证候选actual LOAD_DLL，不能用它替代实际模块身份。
最后源码后独有normal/obj/bin/publish无FH/DR符号发布Exit0；日志1–7/58–60实读，有既有分析器警告，前18920字符被工具截断，不声称零警告或全日志已读。未运行默认产品测试。

### 候选停门与准确设计（未实施、未运行）
candidate任务0/full0/COM变体0。现有candidate没有安全Arm入口：未把实际LOAD_DLL文件句柄SHA、映射PE/指令字节及目标DR线程进入阶段绑定，不能仅借控制通过运行现有旧candidate模式。FH+DR内部类型/方法离线完整认证亦未完成；公开API查询不能认证internal FH，不能冒充已核齐。本轮按用户允许的安全退路交付控制和设计，未猜地址或盲设全部线程。
下一一个根因动作：在同一FH显式诊断补安全candidate Arm认证，不改DR预期/生产：先离线用同项目纯C#元数据读取确认FH Run/HardwareSample与DR测试实际类型/方法齐全；从LOAD_DLL实际句柄持有只读身份，SHA必须1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53，验证实际路径、I386/PE32、SizeOfImage、base+RVA不溢出、映射PE及1351EB/1351F0原字节与对应文件窗口完全一致，不用包hash替actual。
线程门需同一合法DR surface在调用入口前发出独有OutputDebugString阶段标记；debug事件PID/TID与实际阶段/模块一致才Arm该TID，不从LOAD_DLL线程猜DR执行线程、不设其它线程。DR0=实际base+1351EB，DR1=实际base+1351F0，先仅DR0 enable；命中须firstchance/EIP/address/TID/DR6/DR7/phase全匹配。采原context、[EBP]saved caller EBP、有限ESP窗口/返回槽、实际字节及读取Error/长度。先清slot0并读回再启slot1；返回前清slot1并读回。现有Write在Phase2仍保留禁用DR0地址，若严格要求slot地址清零还须同步状态机匹配，禁止无审计改门强跑。
后续最多一次真实候选，真实异常保持NOT_HANDLED，secondchance记录，缺CALL前或未配对就明确未配对；旧AV1351F0/EBP0/readFFFFFF68仅历史对照不预填。静态相邻位置/静态RET不代表动态返回，不宣称首次坏写或栈不平衡已直接证明。读回失败保留Owned并抛出，由Continue finally释放事件，外层尝试clear/Terminate/Wait；设置失败、读回失败、超时及暂停清理分支本轮未执行，不能声称全错误组合已验证。

### 阅读及开放门
规则27行/工作项31行/完整方案120行已分段亲读；GA报告写前1–61分段全文已读。FH已读所需context/硬件状态机/事件finally及外层清理，未称当前828行全文；共享只覆盖顶部受改节，handoff首次1–35截断、summary1–12、issues1–10，中段七百余行历史未全文读，不继承前人覆盖。控制hardware1–16（11行截断后单独补11–16）及cleanup1–4全文实读；native-events只读1–3、180–208，不称208行全文。报告全文及四文档最新受改节写后回读、最终补记另读。
Available/HW消费/resize-loss恢复/full最终四格/x64Event0/E_HANDLE/media/resources/线程callback均开，完整替换未完成；正常控制0与监督0不算产品通过。

GC最终补记：写后本报告1–85分六段全文已回读；handoff1–6、summary1–8、issues1–8最新受改节已回读。默认normal发布日志Exit0已核。共享中段历史未全文读；本轮仅一个正常硬件控制任务，候选0/full0，停止追加运行。此补记随后单独回读。

## GB 最新终态：认证设施已修，正常控制有命中与清零判定，原始寄存器日志门仍开
本节覆盖下方GA历史的“下一动作/未运行”状态，不覆盖历史事实。GB先登记handoff进行中，独立读GA native-events全部185行（末段截断补读182–185）及cleanup全部13行；未候选、未full、未改生产。

### 精确诊断与最小修复
一次必要诊断task 0ff94ee9a3094ba4b7da33af0511b982，原件artifacts/gb-hw-control/diagnostic-control/Results/fh-hw-control。握手PID5880/TID3388，ReadProcessMemory=true/51字节。target1987771888，remoteBase1987641344，RVA130544；localExport与localBase分别相同。远端实际路径C:\Windows\SysWOW64\kernel32.dll，本地Process.Modules.FileName为C:\WINDOWS\System32\KERNEL32.DLL，Environment.SystemDirectory为System32。当前FH原606–608行的第一及第三路径比较不等，触发InvalidOperationException；不是RVA/ASLR失败，也不是转发导出失败的证据。GA旧原件缺这些输入，因此此诊断精确证明同实现的新控制失败条件，不倒造GA缺失字段。
仅改FhCrashEvidence显式FH分支：File.OpenHandle(local.FileName)并GetFinalPathNameByHandle取得实际路径；64位OS下严格核SystemX86目录，仍比较所属模块路径与RVA，目标仍来自子进程握手，不拿本进程绝对地址直接布子DR。规范化后双方均SysWOW64/kernel32.dll；保留初始系统断点一次与非自有异常NOT_HANDLED，未泛吞single-step。
诊断未Arm；cleanup13行全读：Terminate=true、8次CleanupContinue=true、Detach=true/ExitEvent=false、Wait0、两Close成功，Error均0。无实测正常exit0，不虚报clear。

### 唯一纠正控制及证据边界
纠正task e5802dd1e2214191bd2077c9bdd72c29；原件artifacts/gb-hw-control/corrected-control/Results/fh-hw-control。工具仅回“沙箱执行完成”，未提供数字进程退出码。identity：监督3628/子1360/TID5940/.NET10.0.13/X86/PointerSize4，test SHA A23E72FF2B809D7557E16F93CA7D27099472AD7B4234B99C63F646353959C767，MVID62828de0-5104-407e-9d3d-63fa384a156e。
hardware.jsonl全9行已读：认证kernel32实际路径/基址/RVA及SHA BAFD9E061964642802D62F1EB694E7E4AF60DE739A8DF1A492F90006DC5680F3；DR0=1987771888/DR7=1的set-readback Verified=true，armed，before-hit，再DR0/DR1/DR7=0的set-readback Verified=true，clear-confirmed。事件189–190行EXCEPTION_SINGLE_STEP=80000004/firstchance/TID5940/address767AFDF0，191行恰一次Continue65538成功；207行EXIT_PROCESS实测0/无超时/secondChance0，208行supervisor-result0/CleanupConfirmed=true。cleanup全4行：Detach=true/ExitEvent=true、Wait0、两Close成功/Error0。没有失败后重试，没有候选加载；candidate SHA1893370B…只是包内文件身份。
重要缺证：HardwareSample的X86Context字段为internal，System.Text.Json IncludeFields只包含公开字段，实际Actual与Context均序列化为{}。所以源码严格执行了DR6/EIP/TID匹配和写回校验，日志有Verified/命中/清零结论，但缺实际DR6/EIP/DR0/DR7数值原件，不能满足“全部原始寄存器证据才说控制通过”的严格门。本轮控制执行行为成功，但完整控制证据门未闭，不追加控制蒙混；后续必须先修显式日志字段可见性再另授权有界控制，不能直接候选。

### 隔离、构建、风险与下一门
专属artifacts/gb-hw-control/Build.props/obj/bin/publish/com-acceptance-package；明确PublishDir，正式x86 selfcontained10.0.13/FH+DR，正常分析器发布Exit0。默认无FH/DR的normal/obj/bin/publish独立发布成功；未运行默认测试，不能当产品通过。未改项目打包、NativeRun其它入口、COM/DRDS/CF/CX/CY，无新增Host/工程/机器码/INT3/脚本。没有清理本机source或回收目录；只上传正式package到全新task，未上传旧Results。
既有清理分支审查：Arm前Owned=false无需clear；Set后Owned先置true，失败进入外层clear或Terminate/Wait；事件finally内Clear抛错仍可能跳过Continue，但外层会尝试清DR/终止并等待。未实跑设置失败/读回失败/超时/暂停清理，不能保证所有错误组合；默认FH排除，不留默认行为改变。未新增补丁以掩盖这些未测分支。内部入口离线认证未完成，实际运行identity证明进入FH，不代替离线门。工具响应见本会话；未伪造完整工具响应磁盘副本、TRX或数字退出码。
下一唯一门：先补原始寄存器可见性及失败清理审计，另授权控制满足DR数值原件/命中/clear/exit0/清理全门后，才设计同SHA真实加载模块+RVA/TID认证的CALL1351EB前与1351F0返回前成对采样。本轮不候选，未证EBP归零/栈失衡根因。Available/HW恢复/full四格/x64Event0/E_HANDLE及完整替换均未闭，不称生产修复。
阅读实绩：规则27行、工作项31行；方案1–89初次截断，90–120完整，中段未补读，不称全文。GA报告旧1–38分段全读。当前FH重点80–280、300–350、390–595、593–757已读，其余不称全文；三共享只读顶部（handoff初次1–34截断、summary1–12、issues1–14），未读七百余行中段历史，不继承前人全文声明。最终四文档报告全文与最新节回读登记随后补记。

最终补记：本报告写后1–59全文已分段回读；handoff1–10、summary1–8、issues1–8的GB最新节已回读。normal日志1–8核exit0，有既有警告且前18894字符不可见；显式日志只读1–30，不称全日志。实际工具响应已保存artifacts/gb-hw-control/tool-responses.txt。纠正事件只读1–6、180–208（截断末段已补203–208），未称208行全文。控制原始寄存器门未闭，停止本轮执行；本补记随后回读。

## GA 以下为历史

## 终态：正常控制失败，候选未执行，功能未交付通过
先创建本报告并前置handoff GA进行中；发布前、唯一控制返回后均实时更新handoff，历史保留。本轮按首次控制失败停止，不追加控制、不运行候选、不改生产。不能把发布Exit0、API存在或调试监督启动当硬件控制通过。

## 唯一实际任务与原件
control task fa2383c92c8e4176bf83512047d7bb71；远端根 C:\CodingAgentSandbox\Tasks\fa2383c92c8e4176bf83512047d7bb71。
本地原件 WpfGfxShape/artifacts/ga-hw-breakpoint/run-control/Results/fh-hw-control：native-events.jsonl全部185行、cleanup.txt全部13行已逐段亲读，截断161–185补读完成。工具回传监督未处理System.InvalidOperationException（FhCrashEvidence.Run）；没有工具数字退出码，不能填0或任意推定退出码。
监督PID6860，子PID4464，初始/握手事件TID2864，实际.NET10.0.13/X86/PointerSize4。actual test DLL SHA256=F367084600431A18FF2975305D521259CFB75FCA38B22CA5A11D60DE416D951F，MVID=88913d2a-9f37-4edd-ab39-d60f49abda89；子模块加载事件116/121行重复加载均同SHA。apphost模块SHA=EA7BEC3470DF7CA3D9FB5AAD67593753BB2C572AEBB4B15293C513FB9AC0A386。这些是已执行版本，不绑定后续仅补日志的重发布版本。
包内候选文件SHA与aot.sha256实读均为1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53；控制事件没有加载Native候选，不伪造候选actual模块。未重建该DLL。

## 失败现场、命中与清理
59–61行初始ntdll断点80000003/firstchance/TID2864/775D8B88被一次接受并DBG_CONTINUE；89–90行CLR firstchance04242420按NOT_HANDLED继续，没有泛吞异常。最后184–185行为Kind8握手事件及成功Continue，Ready仍false；随后监督抛InvalidOperationException。目录没有hardware.jsonl，无armed/set-readback/before-hit/return-hit/clear-confirmed证据。
依源码执行顺序，失败在握手状态/模块Single选择/路径和RVA认证、首个hardware.Log之前；不能精确断言具体哪个判断。原实现没有在判断前保存原始握手文本及双方模块认证输入，这是本轮可确定的诊断可观测性缺陷。已补debug-output-input及control-export-validation-input，未重跑；不能将猜测的SystemDirectory或转发导出问题当已证根因。
清理原件：TerminateProcess=true/Error0，8次CleanupContinue=true/Error0，Detach=true/Error0/ExitEvent=false，WaitForExit=0/Error0，两自有CloseHandle均true/Error0。正常退出0未达成；没有EXIT_PROCESS退出值、没有supervisor-result，不把终止请求参数1当实测子退出码。没有设置DR的证据，因此不存在本轮DR清零回读证明；不能借进程退出假称clear通过。候选task=无/执行0次。
CALL前与返回前成对EBP/ESP、[EBP]保存槽、残留callee保存槽、返回地址证据全部未取得。既有CALL1351EB→callee135570→返回位置1351F0仍只是静态关系，不证动态正常返回，更不证内部执行或首坏写。EBP归零根因未闭。

## 代码范围与未运行风险
仅修改 Tests/WpfGfxShape.ComAcceptance/FhCrashEvidence.cs，全文件仍#if FH_CRASH_EVIDENCE；NativeRun现有条件分派未改，无新增Host/工程，无INT3改写/注入/脚本/WER/registry操作。
增加SetThreadContext、SuspendThread/ResumeThread与专属HardwareSample；控制角色hw-control/hardware-ready使用GetProcAddress等效NativeLibrary.GetExport所得GetCurrentProcessId真实系统导出地址和OutputDebugString握手，未生成机器码。只对握手事件TID布DR，不盲设新线程。请求10017读取、10010仅写debug组；716/112布局及DR0=4/DR7=24/EBP180/EIP184/EFlags192/ESP196/extended204运行门；本轮未到该门，不声称运行验证过，也未亲读外部安装SDK头。
状态机拟按DR6低命中及BD/BS/BT、DR7启用/类型/长度、地址、EIP、TID、firstchance、phase严格匹配；仅本角色匹配单步可CONTINUE，禁命中槽并回读，保持EIP/EFlags不变（不猜改RF/TF）。采样窗口记录实际有效字节及失败，无反汇编器。
所有新增DR设置/命中/转换/clear/暂停清理路径均未运行。candidate旧模式未接入硬件Arm，DR0/DR1成对状态机只有内部代码，不是已可用候选取证入口；未实现候选LOAD文件句柄SHA绑定/内存CALL字节认证/独立目标TID阶段标记。模块当前仍按路径取SHA，不能声称防换件认证。异常期间clear自身抛错可能中断事件finally中后续Continue，及清理错误与主错误的组合处理仍需专门审查，不能承诺所有失败路径已安全闭合。这些风险均在默认排除代码内；严禁直接重启candidate冒充GA能力完成。

## 构建、包与预检
独有artifacts/ga-hw-breakpoint/obj/bin/publish/com-acceptance-package，显式PublishDir；正常分析器无NoWarn/WarningLevel或EnableNETAnalyzers压制。首次显式发布因CA2219失败，修正finally直接throw后成功；控制后仅补输入日志再次显式发布Exit0。最后源码后normal无FH/DR符号x86自包含10.0.13发布Exit0，独有normal/obj/bin/publish；是默认构建验证，不是产品运行通过。
正式包由既有PackageSelfContainedComAcceptance生成，只上传com-acceptance-package，不上传历史、obj、回收根或裸PublishDir。aot.sha256全文1行，runtimeconfig全文21行（Core与Desktop均10.0.13）、清单首25和末406–418已读，MTP/Contracts/Native/runtime均列在正式依赖图。未称清单418行全文审计。
离线公开类型/方法核DrInteropAbiTests及目标TestMethod存在；FH为internal，公开API工具未找到不表示被裁剪，也不能当离线内部方法存在证明。没有完成要求的内部类型离线预检，这也是本轮不足；但实跑栈和identity证明已进入FH，不是旧Exit8漏DR。不得以该事实替代后续正式离线门。
显式成功发布可用日志61行已读，stdout38786字符的前18786不可见，既有MSTEST/CA警告未消失；最后normal日志可用60行另核退出与输出，非零警告声明。诊断非MTP，不造父TRX、完成TRX或业务计数。

## 下一唯一动作与交接范围
下一仅审查并收敛正常控制握手认证诊断：依据新加的判断前输入日志设计一次另授权正常控制定位，先闭模块/导出/路径/地址认证和异常清理，再谈真实DR命中/clear/exit0；禁止当前直接跑候选或修改生产ABI。此轮不追加试跑。
已读完整方案120行、当前规则27行、工作项31行，FW/FZ报告71行及FT/FU报告37行全文；原FH521行分段阅读主要路径，208–210附近未补齐，不虚称原源码全文。共享历史只读受改顶部，未继承FZ/FU全文阅读声明，不无谓全读历史。新增四文档受改节写后回读，报告全文回读；最终补记另读。
Available/native通知消费、HW内容及resize/loss恢复、media/resources/线程callback、full最终四格、x64Event0/E_HANDLE仍开放；默认FH隔离保持，生产COM/DRDS断言/VisualCache/historical/捕获/打包项目/版本/CF/CX/CY未改，完整替换未完成。

## 过程历史
初始：创建报告与handoff进行中，尚未构建/运行。
控制前：发布成功，登记未通过控制且候选未接入；随后唯一控制失败。
控制后：立即handoff登记失败/停止候选，回读185行事件及13行清理；仅补失败前输入日志和构建，不重试。
