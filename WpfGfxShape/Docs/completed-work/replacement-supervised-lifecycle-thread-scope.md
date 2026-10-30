# KS 唯一生命周期线程作用域对照：A/B 均未交付 consumer，停止线程方向

## 最新授权 clean package 唯一实跑：身份成立、两侧像素失败、清理收束

本节覆盖下文历史准备门与下一运行建议。用户明确纠正过严准备门：不新增hash工具/执行器，不要求不可用的非执行PE检查；允许正式AssemblyInitialize/AcceptancePaths.ValidateIdentity在本次唯一任务内认证Native SHA/PE/架构，监督器记录实际test SHA/MVID。没有绕过初始化、复制、新staging、修改包或使用publish输入。主代理只监督，无委派/终端/脚本。

### 输入、预算与实际身份

- 唯一沙箱task **52c18aa8497149ee845a5ef2625cc47f**；sourceDirectory精确为 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\ks-thread-scope\com-acceptance-package`，executableRelativePath `WpfGfxShape.ComAcceptance.exe`。
- 原始参数数组依次 `--supervise-exit`、`Results/ks-clean-package-x86`、`120`、`FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenKsLifecycleThreadScopeIsComparedThenOwnerConsumerMustDeliver`；外部180秒。监督ChildArguments为该FQN等值filter与TRX/diagnostic参数，没有扩过滤器。
- 启动前以一次递归目录枚举（上限600，未截断）核manifest及包内路径存在，读根aot.sha256、runtimeconfig；根exe/test DLL/Contracts/Native DLL/官方根DLL可见，无明显身份冲突。不把清单文本或目录存在当二进制认证，不声称完成逐文件hash/大小/闭包测试。
- 根aot.sha256为 **20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D**；初始化成功，`Results/aot-identity.json`记录同一实际SHA、Sandbox=true、X86/PointerSize4、任务根WorkingDirectory/BaseDirectory、candidate为任务根Native/wpfgfx_cor3.dll。结合已读ValidateIdentity源码，成功返回意味着无CorHeader、Machine I386及实际SHA与根清单匹配；不是工具独立输出PE表。KS体再次SHA断言成功。
- 监督与KS identity实际test SHA **D56FB20357A5D367B8D79A0C7A0262181DB4C8E55DFF254A6A3CE8723568C51F**、MVID **d5082b0d-734d-49ed-8121-822710749ba8**，与预期一致；实际.NET10.0.13/X86，MTP2.0.1、MSTest4.0.1。UID **1bae82f4-99fb-89b2-a4a5-309f92a81329**准确匹配。
- 本轮沙箱1/1、精确运行请求1/1、KS体1/1、A1/1、B1/1、单Clear/Present各一次、native提交总2/2；build/publish/发现/代码及包修改全部0。无shader/resize/full/x64/official扩展，无修包或重试。

### 亲读结果与实际角色

回收根 `WpfGfxShape/artifacts/ks-thread-scope/readback/clean-package-x86/`。亲读 `Results/ks-clean-package-x86/TestResults/child.trx`（73行，长段分读）、supervision.json（40行）、stdout（52行，截断后补段）、stderr（空）、stages.jsonl（36行）、`Results/aot-identity.json`及`Results/ks-thread-scope.jsonl`全部36行；另读diag首段和退出/失败段，不冒称全文读完。角色JSONL包含B记录，TRX/stdout没有完整转录B，不拿控制台缺行否认B；stages的A/B window至cleanup及verdict/owner joined明确证明测试体进入，非仅TRX executed1推断。

TRX run **5d2cc5d3-c8cc-42f1-9c5a-1b5c9be30a24**，total/executed1、passed0/failed1、skip0；方法耗时3.2467545秒。最终失败为 `Assert.IsTrue(b)`：KS owner consumer failed；不是身份/初始化/API/清理异常。原始verdict a=false/b=false/hypothesis=inconclusive/productAcceptance=false。

| 实测项 | A（test线程生命周期） | B（owner线程生命周期） |
|---|---|---|
| native/managed线程 | tid2584 / managed8；HWND owner1072 | tid1072 / managed10，等于HWND owner1072 |
| HWND / PID | 35325324 / 10488 | 35390860 / 10488 |
| 原参数 | adapter0/HAL1/behavior0x26；32×16、format0、count1、swap1、windowed1；Clear FF123456；client(16,8)、screen(56,48)、dpi96 | 相同参数；独立重建窗口 |
| client/screen提交前 | 均1973532 = 001E1D1C | 均1973532 = 001E1D1C |
| client/screen提交后 | 均001E1D1C，未变为00563412 | 均001E1D1C，未变为00563412 |
| baselineValid / consumer | true / false | true / false |
| CreateDevice / Clear / Present | hr0 / 0 / 0 | hr0 / 0 / 0 |
| event query | create/issue/complete hr0，polls13、flushFlag1 | create/issue/complete hr0，polls3、flushFlag1 |
| DwmFlush | 两次均hr0 | 两次均hr0 |
| query/device/factory实际Release | refs各0 | refs各0 |
| DestroyWindow / cleanup | result1 / clean=true | result1 / clean=true |

会话两侧为1/0/Default且过程会话断言未失败。A原始PresentCondition记录visible1/iconic0/foreground和cover均目标窗口、cloaked0、cooperative=0、readOnly=True/Ex=False；实际swapchain format22，32×16/count1/swap1/windowed1，source/destination NULL、目标HWND override。**B的cooperative/实际swapchain扩展文本未落在回收stdout/TRX中，角色JSON也无这些字段，故B此项not observed**，不得从A复制。两侧query/flush证据独立存在；API成功不等于consumer交付。

### 身份与 actual 加载必须分层

根 `wpfgfx_cor3.dll` 是官方WindowsDesktop runtime资产，Native/wpfgfx_cor3.dll才是candidate，未互换/修改；根官方文件本次未做独立PE/SHA认证或官方角色运行。初始化及日志production路径/SHA只认证candidate文件，**不证明候选被加载**。

测试末尾针对wpfgfx/d3d9的实际进程模块枚举仅记录 `C:\\WINDOWS\\SYSTEM32\\d3d9.dll`，SHA **F4757445D75D619BC5695294EE5CA6BFA0AD8763C6604A8E884739F605A6DF28**；candidateStatus=not loaded，枚举未列官方wpfgfx。此为末尾限定名称模块快照，不是全进程/全时序模块审计，也不是candidate产品执行认证。

清理层级：两侧真实query/device/factory Release返回0，真实DestroyWindow返回1、clean=true；其后owner joined=true。已读既有Dispose：PostThreadMessage WM_QUIT成功后无超时Thread.Join返回，再记录joined；不只是子进程退出推断线程收束。最后监督HasExited/OutputCollected=true，Reason=exited，业务Exit2/0x00000002；TimedOut/Cancelled/KillAttempted/KillReturned/两种OutputDrain超时均false、SupervisorError=null。监督PID1116、child10488，UTC12:14:48.0274799–12:14:52.0880998。工具仅报告执行完成，未给独立数值exit，不冒业务成功。

### 累计账与裁决、唯一下一门

KS历史累计Windows沙箱 **4次**：①8ffcb912…零命中/Exit8（运行请求1、体0）；②efe0ea70…仅发现1/Exit0（体0）；③584e44bb…缺根aot.sha256初始化失败/Exit2（运行请求1、框架executed1但体0）；④本次52c18aa8…真实体1/A1/B1/提交2、Exit2。累计精确运行请求 **3**、独立发现 **1**、框架executed结果 **2**、真正KS体 **1**、A/B各 **1**、native提交 **2**。历史前两类运行设施失败没有被本轮新预算抹除，也不记产品失败；KS初始历史build1/COM publish1/Native publish0原样保留，本轮均0。此前静态审查阶段沙箱/测试均0。

**两侧都真实未交付目标像素，按本次授权停止线程方向，不再以改到owner线程作为下一修复/实验。** 这不能逻辑证明线程在所有场景无影响；固定A→B、独立重建窗口混杂仍存在，原始hypothesis仍inconclusive。不是产品通过，不把system d3d9观察变成候选或官方wpfgfx验收，完整替换仍未完成。

**唯一下一门：另行授权仅静态审查既有非线程证据，确定一个可证伪的consumer可观测性前置缺口及其最小验证合同；本轮到此停止，不自动执行该门。** 不预授权新构建/发布/沙箱/发现、修包、shader/resize/full/x64/official或其他变体。下文所有再次KS运行/非执行PE准备要求均为历史，已被本次授权与终态取代。

---

以下保留此前初始化失败及准备审查历史。

## 最新：身份前置静态审查完成，找到既有 clean package；未改包、未运行

本轮只读文本/目录并更新文档。构建0、发布0、沙箱0、测试0、发现0、A/B0、native提交0；委派/终端/脚本0。生产/测试源码、公共helper、NativeRun/ExitDiagnostics、项目、打包targets及所有二进制均0改。没有手写aot.sha256，没有用文本工具读写二进制，没有复制或创建staging。下述包选择纠正此前“继续用既有publish目录”的建议，不覆盖历史真实执行记录。

### 根因与身份合同（本轮已核源码/文本）

`task584e44bb09b9484785e6b1b029d2352d` 的输入是 `artifacts/ks-thread-scope/publish/acceptance-x86`，不是同次生成的 `artifacts/ks-thread-scope/com-acceptance-package`。直接读前者根aot.sha256和package-files.txt均返回不存在；后者两文件存在。原TRX异常段本轮再次直接读取，确为任务根aot.sha256缺失，非产品/像素失败。list不触发AssemblyInitialize，不能验收包执行闭包。

`AcceptancePaths.ValidateIdentity` 的完整直接输入只有解析后的candidate文件、进程架构和（Sandbox=true时）AppContext.BaseDirectory下aot.sha256：先打开candidate，以PEReader要求无CorHeader且Machine匹配进程（本KS要求I386），再从头计算实际SHA256，最后与根身份文本Trim后OrdinalIgnoreCase比较。aot.sha256是单独64个十六进制字符的SHA256摘要文本，可有末尾换行；不是sha256sum的“hash 空格 文件名”、不是JSON、不是路径或test程序集摘要。它绑定candidate，不绑定官方DLL、test DLL或system d3d9。执行走到缺清单读取，说明该历史进程此前已经完成candidate文件打开/PE检查/摘要计算，但没有打印其值，不能反推其摘要必为预期值，更不证明candidate已加载。

`AotDll`优先WPFGFX_CANDIDATE_DLL，否则sandbox程序集metadata `Native/wpfgfx_cor3.dll`相对AppContext.BaseDirectory解析；`Sandbox`取metadata `ComAcceptanceSandbox=true`。Results同样由metadata解析为包根Results，初始化成功后写 `Results/aot-identity.json`，这是文件身份输出，不是actual模块加载证明。ModuleInitializer在sandbox把工作目录设为BaseDirectory。环境覆盖必须排除或明确核准，不能靠改变环境绕过角色/清单。

`OriginalDll`优先WPFGFX_ORIGINAL_DLL，否则取实际PresentationCore.dll所在目录的wpfgfx_cor3.dll。自包含包根的 `wpfgfx_cor3.dll` 是WindowsDesktop官方runtime资产；`Native/wpfgfx_cor3.dll` 才是candidate，二者不得互换/覆盖。ValidateIdentity不读official、Contracts或package-files.txt；这些属于包闭包/白名单合同，不能误称它们也会在本次初始化逐个验hash。KS本身只做system d3d9生命周期对照，并未加载候选生产模块；初始化和KS开头均校验candidate字节，末尾枚举wpfgfx/d3d9模块，不应预填candidate loaded。

### 权威来源、发布与KP合法包组成

已读现行 `ComAcceptance.Package.targets` 全部55行及项目metadata声明。自包含目标 `PackageSelfContainedComAcceptance` 在Publish之后执行：从PublishDir按明确清单复制正式exe/DLL/deps/runtimeconfig、MTP/MSTest依赖、RuntimePackAsset（含.NET/WPF本体及本地化资源）、两Contracts、Native candidate和显式ReferenceData；对PublishDir/Native/wpfgfx_cor3.dll执行GetFileHash，再把摘要写到 **ComAcceptancePackageDirectory/aot.sha256**，并生成该目录的package-files.txt。这两个生成文本不写回PublishDir。因此“发布成功”不等于PublishDir就是合法上传包；本次是sourceDirectory选错，不是要修改初始化/打包逻辑。

package-files.txt为每行一个包根相对文件路径、子路径统一 `/`，无摘要、大小或角色标签；包含自身和aot.sha256，官方根DLL与Native DLL各列一项。`AcceptancePathsTests`按Ordinal排序比较递归实际文件与manifest，排除Results/TestResults，并对ReferenceData只纳入manifest列明项。该测试未在本轮执行；不能把清单文本当逐字节认证。本轮分段读完KS清单418行：包含完整运行资产、两Contracts、双DLL角色和两个根文本，没有ReferenceData条目。KS方法不读ReferenceData或Contracts，不能为本精确KS捏造必需golden或挪用full闭包结论。

此前KP合法clean输入为 `WpfGfxShape/artifacts/kp-resize-pixels/com-acceptance-package`，不是KP publish或readback。当前该目录根aot.sha256亲读为 `20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D`；其清单尾列同样的官方根DLL、Contracts、Native及身份文本。本轮亲读KP历史 `readback/x86/Results/aot-identity.json`，Sandbox=true、X86/4、Native任务内路径及该SHA一致；KP报告记载曾实际加载该candidate、正式test SHA 4C522922…/MVID 0e30de26…。这些仅历史认证，不证明当前磁盘二进制未变化。KP默认test不提供已认证的KS guard入口，不能直接拿KP包跑KS，也不允许替换其中test DLL“拼包”。

### 现有正确sourceDirectory选择与准备度

应选既有 **`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\ks-thread-scope\com-acceptance-package`**，exe相对路径 **`WpfGfxShape.ComAcceptance.exe`**。本轮已确认根exe/DLL/deps/runtimeconfig、Native DLL、官方根wpfgfx DLL、两个Contracts及身份文本存在；runtimeconfig声明.NETCore/WindowsDesktop 10.0.13。根aot.sha256亲读与KP清单及KS源码常量一致。按名称递归检索Results无命中，未混入历史结果的关键路径已核；此前递归枚举180条截断，**没有声称418个文件均已逐项存在、无额外文件或总字节已核**。

这是现行打包合同认可的输入目录选择，不是已完成当前字节验收的运行授权。现有工具没有安全二进制复制、SHA256计算、PE/MVID读取及完整文件大小清单专用能力；不通过执行程序/脚本补做，不以API类型清单冒这些验证。故保持所有包原样，不新建staging，不把读到相同SHA文本说成二进制一致。KS clean中的test程序集是否与曾执行publish的D56FB203…/MVID d5082b0d-734d-49ed-8121-822710749ba8相同，当前未重新认证；clean candidate是否仍为20B3F944…也未重新认证。**准备度：布局/文本来源成立，当前二进制与完整逐文件验收未完成，暂不可直接重跑。**

### 一次性启动前静态验收清单（必须全部关门，禁止逐个缺件试跑）

| 检查项 | 必须满足的完整条件 | 本轮层级 |
|---|---|---|
| 输入根 | 仅上述KS clean package；不得上传publish父目录、KP默认包、源码/obj/bin、readback或回收Results | 正确目录已定位，未上传 |
| 完整文件集合 | 对清单418行逐项确证存在、为普通文件、路径不越根/无重复；与递归实际文件集合对账，审查额外文件、总字节和架构资产，无历史Results/TRX/diag/PDB/异平台额外资产 | 清单全文已读；未完成全目录对账/大小核验 |
| 正式入口 | 根WpfGfxShape.ComAcceptance.exe与同名DLL/deps/runtimeconfig配套；不可指向Native DLL、旧Host或KP默认exe | 四项存在；当前二进制配套待核 |
| 测试身份与入口 | KS_THREAD_SCOPE正式发布，test SHA D56FB20357A5D367B8D79A0C7A0262181DB4C8E55DFF254A6A3CE8723568C51F，MVID d5082b0d-734d-49ed-8121-822710749ba8；唯一KS UID 1bae82f4-99fb-89b2-a4a5-309f92a81329 | 此身份及UID是此前publish实跑认证；clean DLL需非执行元数据/字节核验，不再申请list |
| runtime闭包 | x86自包含.NETCore/WindowsDesktop10.0.13，exe/native hostfxr/hostpolicy/coreclr等架构一致；全部RuntimePackAsset、WPF托管/原生/本地化、MTP/MSTest/JSON依赖按清单齐全，deps对应本包 | runtimeconfig已核；具体PE/逐文件闭包待核 |
| candidate角色 | 根下Native/wpfgfx_cor3.dll，native PE无CorHeader、I386；实际SHA20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D，与KS常量及根aot.sha256一致 | 路径/文本/常量已核；当前PE/实际SHA工具无法核 |
| 根身份来源 | 使用同次生成clean包的aot.sha256；只含candidate摘要，来自GetFileHash绑定Native字节，不从报告手抄、不放在Results/Native子目录代替根 | 目标及两包文本已核；当前字节绑定尚欠独立复核 |
| official角色 | 根wpfgfx_cor3.dll保持WindowsDesktop runtime官方资产，与PresentationCore同版同架构；禁止用candidate覆盖；OriginalDll环境覆盖必须明确 | 双路径存在/规则已核；当前official版本/摘要/PE无法核，未来actual另记 |
| 其他静态输入 | Contracts/wgx_sdk_version.h、wpfgfx.def按清单保留；本KS无ReferenceData需求，不捏造golden，也不外推full | Contracts存在；KS静态调用不读golden |
| 路径与环境 | sandbox metadata为true、AotDllPath=Native/wpfgfx_cor3.dll、CalibrationResultsPath=Results；BaseDirectory为任务根且工作目录与之相符；核查WPFGFX_CANDIDATE_DLL/ORIGINAL_DLL/RUN_ROOT/EXIT_STAGES继承值无越界/串角色 | 源码合同/历史初始化行为已核；clean编译metadata、未来环境待核 |
| filter与参数 | 只用完整FQN等值filter，不用斜杠filter/旧ID/通配扩大；监督参数和输出路径见下文；不绕过初始化 | filter已有历史精确命中，不需新发现 |
| 输出非输入 | 初始不预置aot-identity/模块加载/像素/refs证据；将来保留Results/aot-identity.json、KS jsonl、监督/stages/stdout/stderr/TRX/diag，全量回收至独立readback | 本轮无输出/运行；actual仍not observed |

最小后续受支持操作是提供**非执行**二进制SHA256、PE Machine/CorHeader、托管MVID/assembly metadata和递归路径/大小核对能力，对既有KS clean完整验收；不需要构建/发布，也不需要复制。若必须另建专属staging，只能由受支持的二进制安全复制能力按既有manifest整体复制并前后hash对账；不能由文本工具搬DLL或手写摘要。本轮上述能力不可用，明确留阻碍而非声称“修包完成”。

### 下一次唯一运行预算申请（有条件、尚未授权）

先完成上述全部静态门，再由监督方取得新授权。申请构建0/发布0/Native重发0/发现0，沙箱1、外180秒/监督120秒，精确KS测试体至多1，固定A→B各至多1、native提交总至多2。实际sourceDirectory使用上述KS clean；executableRelativePath为根exe；参数数组依次为 `--supervise-exit`、`Results/ks-package-identity-verified-x86`、`120`、`FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenKsLifecycleThreadScopeIsComparedThenOwnerConsumerMustDeliver`，回收完整任务目录到独立 `artifacts/ks-thread-scope/readback/package-identity-verified-x86`，不把回收根当下次输入。再次初始化或身份失败即停，不现场补件重试。监督输出必须重新核test SHA/MVID/runtime；身份文件不是actual加载证明。A/B的线程/API/像素/Release/DestroyWindow/owner收束仅依新原始证据裁决。本轮申请不消耗预算，也不把设施失败冒产品失败；线程假设及完整替换结论仍开放。

以下为历史执行/发现，保留原值；其中“下一唯一门/既有publish输入”建议已被本节取代。

## 最新授权真实执行门：UID/程序集身份匹配，AssemblyInitialize失败，A/B未进入

本次只用既有 `artifacts/ks-thread-scope/publish/acceptance-x86`，唯一Windows沙箱任务 **584e44bb09b9484785e6b1b029d2352d**。沙箱 **1/1**，外180秒、监督120秒；代码/项目/入口/打包修改0，build0、COM publish0、Native publish0、发现0、委派0、终端/脚本0。失败后停止，没有修包或重试，没有原ID/shader/resize/x64/official/full。

### 实际参数与身份

sourceDirectory为 `WpfGfxShape/artifacts/ks-thread-scope/publish/acceptance-x86`，executableRelativePath为 `WpfGfxShape.ComAcceptance.exe`。原始参数按顺序严格为：`--supervise-exit`、`Results/ks-entry-verified-x86`、`120`、`FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenKsLifecycleThreadScopeIsComparedThenOwnerConsumerMustDeliver`。监督实际ChildArguments为该完整名称的 `--filter`，随后TRX/diagnostic参数，没有斜杠filter。

监督实际根程序集SHA **D56FB20357A5D367B8D79A0C7A0262181DB4C8E55DFF254A6A3CE8723568C51F**、MVID **d5082b0d-734d-49ed-8121-822710749ba8**，两者与用户给定历史身份完全一致。实际 `.NET10.0.13 / X86`，MTP2.0.1、MSTest4.0.1。TRX唯一testId **1bae82f4-99fb-89b2-a4a5-309f92a81329**，类型/方法全名与已验证入口及本次授权一致；不是零命中，不再做发现。身份由运行后监督记录确认，不宣称运行前独立完成hash校验。

### 实际失败与预算

TRX run **2585f1dd-c0b1-4ce1-82c9-d5ba9e059c18**，Counters total1/executed1/passed0/failed1，duration0。此executed1是框架结果计数，**不是KS测试体进入**。实际错误为 `AcceptanceInitialization.Initialize` 的Assembly Initialization调用 `AcceptancePaths.ValidateIdentity()`，读取任务根 `aot.sha256` 时抛出 `System.IO.FileNotFoundException`（AcceptancePaths.cs:67/80），中止测试执行。发现模式未触发该初始化，故此前list成功不证明执行前置完备。本次未进一步查包、补清单或改初始化绕过合同。

精确运行请求 **1/1**，唯一框架失败结果 **1**，KS体 **0/1**，A **0/1**、B **0/1**，native提交 **0/2**。这是初始化前置失败，非A单纯像素失败，无需也不可继续B。线程假设仍未裁决；既非“两侧失败”，也非“仅B通过”，没有产品通过或强因果结论。

业务child **Exit2 / 0x00000002**，Reason exited。监督PID2832/child984，UTC **2026-10-26T12:03:35.3307178–12:03:36.1353465**。TimedOut、Cancelled、KillAttempted、KillReturned、OutputDrainTimedOut、OutputDrainCancellationTimedOut均false；HasExited/OutputCollected均true、SupervisorError=null。工具仅报告“沙箱执行完成”，**未给独立数值工具exit**，不把工具完成当业务成功。

### 原始回读与证据层级

回拉根：`WpfGfxShape/artifacts/ks-thread-scope/readback/entry-verified-x86/Results/ks-entry-verified-x86/`。

- 亲读supervision.json全部40行（1–25、26–40），stdout.txt全部26行、stderr空。
- 亲读 `TestResults/child.trx` 全49行（1–22、23–49）。目录枚举未列TestResults，但按stdout实际路径直接读取成功，不能称TRX缺失。
- 直接读取 `stages.jsonl` 返回不存在；没有角色诊断产物，stdout/TRX无KS角色输出。没有用静态清理路径替代实测。
- 亲读 `Diagnostic/log_261026120335476.diag` 的1–65、66–79、79–80、81–100行。66–80首次合读因4000字符限制在79行截断，已单独补读79–80；不冒称最初截断输出是全文。日志唯一InProgress/Failed UID、初始化异常及最终session Exit2与TRX一致。

| 要求 | 本次实际证据及边界 |
|---|---|
| A/B线程与固定A→B | 两角色均未进入；没有owner/test线程ID，不能用监督/child PID或框架线程替代 |
| 原设备/窗口/颜色参数 | 未建立角色，0/HAL1/0x26、32×16/count1/swap1/format0、FF123456仅既有设计值，不是本次实测 |
| consumer前后client/screen严格00563412 | 四点及前后采样均 **not observed**；不能判A/B像素失败或通过 |
| cooperative/query/DwmFlush | 均 **not observed**；未发生角色设备诊断或完成调用 |
| 真实device/factory Release | **not observed**，没有引用计数返回值；不得写refs0 |
| DestroyWindow/owner线程收束 | **not observed**，没有HWND销毁返回值/owner Join证据；child退出及输出收集只证明进程监督层收束 |
| candidate actual | **not loaded / not observed**：初始化在测试体前失败，没有候选加载认证/角色模块清单；不是全进程模块审计，不拿包hash或test程序集hash代actual |
| system d3d9 actual | 路径/hash **not observed** |

### 下一唯一门

**保持停止。** 唯一下一门是监督方另行授权审查并纠正既有包的 **AssemblyInitialize身份清单前置（缺aot.sha256）**，明确清单权威来源及与真实candidate字节绑定；不得伪造hash、跳过初始化或借此开展新发现/其他变体。本次不实施、不自行查包补件或重跑。清单门满足且再次授权后，才可考虑同一精确KS有界执行的新预算（沙箱1、外180/监督120、测试体1、固定A→B各1、native总至多2；不预授权build/publish）。线程方向没有两侧结果，不能按“两侧均失败”关闭；若未来两侧真实失败即终止该方向，若仅B通过仍保留固定顺序/窗口重建混杂。完整替换仍未完成。

以下入口发现成功及更早零命中记录原样保留，均属历史，不覆盖本次初始化失败终态。

## KS 入口修正门：仅发现完成，未执行测试体

本阶段按新授权仅静态审查和既有诊断包一次 Windows 沙箱发现；代码/项目/入口/打包修改0，构建0、发布0、Native发布0、A/B执行0、native提交0、委派0。沙箱 **1/1**，task **efe0ea705ac442a6a6b70d9bfa50d034**，外部上限120秒。未使用终端、脚本、新Host或执行器，没有不支持list后的降级或第二次尝试。下列旧KS实验终态仍有效：task8ffcb9123d93489984049f05e654e53b实际执行0，不能裁决线程假设。

### 静态与二进制入口证据

- 适用规则已读 current-execution-rules/current-work-item/current-stable-baseline 全文及最新恢复入口顶部、原KS报告；AGENTS.md/instructions名称检索无适用结果。根及项目global.json已读，项目SDK10.0.107、TFM net10.0-windows、MSTest4.0.1；不改配置。
- `D3DImageTests.KsThreadScope.cs` 全文件受 `#if KS_THREAD_SCOPE` 控制，方法为public实例无参void，行177有 `[TestMethod]`；同partial主声明有 `[TestClass]`、`[DoNotParallelize]`，为public sealed类型。WindowThread的ClassInitialize/ClassCleanup是测试生命周期，不是NativeRun启动时直接调用。
- 亲读现存最后发布日志60行：命令明确 `-p:DefineConstants=KS_THREAD_SCOPE`，Exit0，行38/40/41有KS文件内部分析器警告，行58/59为KS独有bin/publish路径；日志自身声明前33994字符被截，不冒称完整原始构建输出。
- 对现有 `artifacts/ks-thread-scope/publish/acceptance-x86/WpfGfxShape.ComAcceptance.dll` 使用公开API元数据读取，`WpfGfxShape.ComAcceptance.D3DImageTests` 确有 `public void WhenKsLifecycleThreadScopeIsComparedThenOwnerConsumerMustDeliver()`。这证明不是guard未进入发布程序集；发现日志进一步证明该方法被MSTest识别，不仅是1817总量。
- NativeRun.cs行24–25只对首参数`--supervise-exit`进入监督，其他普通MTP参数在补results-directory后交给CreateBuilderAsync/AddSelfRegisteredExtensions/BuildAsync/RunAsync。ExitDiagnostics.Options.ChildArguments固定产生`--filter <值>`及TRX/diagnostic参数，Parse恰收三个参数，没有list透传。因此本次不用监督器，直接调用既有正式exe已知list模式；没有改入口或默认运行。

### 无执行模式确认与唯一实际参数

执行前亲读微软官方CLI文档 `https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-cli-options`：`--list-tests` 明确“Lists the available tests without executing them”；`--filter-uid`自1.8.0可用。已有旧诊断确认当前MTP2.0.1。没有使用仅2.3起支持的`--list-tests json`。另读 `https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-running-tests`，其MSTest exe过滤示例为`--filter "FullyQualifiedName~UnitTest1|TestCategory=CategoryA"`，不是路径式树表达式。

本次sourceDirectory：`WpfGfxShape/artifacts/ks-thread-scope/publish/acceptance-x86`；executableRelativePath：`WpfGfxShape.ComAcceptance.exe`。参数逐项为：

1. `--list-tests`
2. `--filter`
3. `FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenKsLifecycleThreadScopeIsComparedThenOwnerConsumerMustDeliver`
4. `--results-directory`
5. `Results/ks-entry-discovery`
6. `--diagnostic`
7. `--diagnostic-synchronous-write`
8. `--diagnostic-output-directory`
9. `Results/ks-entry-discovery/Diagnostic`
10. `--no-progress`

工具原始输出列出该方法，摘要找到 **1** 测试、254ms。部分本地化字节显示损坏；以原始diag的测试节点为权威。工具返回结果位置指向Results，但实际回拉目录还包含程序集等包文件，不把枚举差异当缺失或另行执行。

### 原始发现输出回读与精确身份

亲读原始文件全部47行：`WpfGfxShape/artifacts/ks-thread-scope/readback/entry-discovery/Results/ks-entry-discovery/Diagnostic/log_261026115854864.diag`。

- 行1：MTP **2.0.1+2eb19d535ed152853999db4feb491c7afc44cc6b**；行5 PID5716；行6 **win-x86 / .NET10.0.13**；行9正式程序集为 `C:\CodingAgentSandbox\Tasks\efe0ea705ac442a6a6b70d9bfa50d034\WpfGfxShape.ComAcceptance.dll`；行10完整保留list及等值filter参数。
- 行28：MSTest4.0.1。行39：源程序集发现 **1817**；此数字本身不证明KS。
- 行42：唯一TestNode，UID **`1bae82f4-99fb-89b2-a4a5-309f92a81329`**；DisplayName **`WhenKsLifecycleThreadScopeIsComparedThenOwnerConsumerMustDeliver`**；Namespace **`WpfGfxShape.ComAcceptance`**；TypeName **`D3DImageTests`**；MethodName同上；MethodArity0，ParameterTypeFullNames空；源码KsThreadScope.cs，Line177（零起点）。状态明确 **`DiscoveredTestNodeStateProperty`**，不是Passed或测试体执行结果。
- 完整测试名：**`WpfGfxShape.ComAcceptance.D3DImageTests.WhenKsLifecycleThreadScopeIsComparedThenOwnerConsumerMustDeliver`**。
- 行47：session `1a0cef51-c2ef-4e5e-8b12-1aa4f56fa544` 以 **Exit0** 结束，0.2765758秒。此为原始MTP日志数值，不声称工具另给独立数值exit。日志中的TestRequestExecutionTimeInfo及runsettings“Execution”通用措辞不代表测试体运行；实际请求是list、节点为Discovered，没有MSTestExecutor.RunTests记录。

**精确filter及差异：** 已真实发现验证单项的MSTest语法是 `--filter "FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenKsLifecycleThreadScopeIsComparedThenOwnerConsumerMustDeliver"`。原错误值是 `/WpfGfxShape.ComAcceptance/D3DImageTests/WhenKsLifecycleThreadScopeIsComparedThenOwnerConsumerMustDeliver`，被ExitDiagnostics原样传给`--filter`；该参数不是`--treenode-filter`，不会自动把斜杠转换成命名空间/类型/方法。新值改为属性`FullyQualifiedName`、等号精确比较及点分全名。另一合法精确选择语法为 `--filter-uid 1bae82f4-99fb-89b2-a4a5-309f92a81329`（平台版本支持、UID真实回读，但本次没有另做UID过滤调用，不冒称已经实测该参数）。

### 发布包身份与证据边界

本次直接使用上述既有KS正式publish目录，无重建/重发/修改。deps.json及runtimeconfig.json确认win-x86、自包含NETCore/WindowsDesktop10.0.13、MSTest4.0.1；实际发现日志也确认同架构/runtime/runner。旧task监督记录的根程序集SHA **D56FB20357A5D367B8D79A0C7A0262181DB4C8E55DFF254A6A3CE8723568C51F**、MVID **d5082b0d-734d-49ed-8121-822710749ba8**是该包的**历史认证身份**；本次直接list没有监督器hash记录，元数据工具也未提供SHA/MVID，故**本次未重新独立认证当前字节hash/MVID**，不能把旧记录写成本次actual。没有创建hash执行器补证。

Native预期20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D仅是源码/历史包身份。本次发现不运行KS内SHA或模块采集，**没有候选实际加载认证或完整模块审计**；不以发现Exit0当consumer通过。线程假设仍不确定，A/B0、native提交0，没有新增像素/线程/资源清理结论。

### 下一精确参数与新授权最小预算

当前停止，不调用以下运行。可以独立复核的无测试发现参数就是上列10项；若要再独立复核需另授**沙箱1、发现1、测试体0、A/B0、native提交0、build/publish0**，不可复用本阶段已耗尽的沙箱预算。

若监督方转入真正KS对照，既有exe下一精确监督参数逐项为：`--supervise-exit`、`Results/ks-entry-verified-x86`、`120`、`FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenKsLifecycleThreadScopeIsComparedThenOwnerConsumerMustDeliver`。监督器会转成已由list单项验证的`--filter`，并重新记录实际根程序集SHA/MVID。需明确新授权最小预算：**沙箱1（外180秒）、精确测试体1、固定A→B角色各1（共2）、native提交各1（共2）、COM build/publish0、Native publish0**。只用当前既有包，实际SHA/MVID必须与历史身份比对；任何身份差异或测试入口/前置失败即停，不自行重跑、修码或重发。完整替换、原ID shader/resize及full均不在此授权内。

---

以下为前次KS实验原始终态（历史保留）。

## 终态

**未取得 A/B 像素实验。精确测试身份门失败，已停止，无重试。线程假设不确定，既不支持也不反驳；不是两侧失败，更不是产品通过。** 原 ID shader/resize 未执行、未获本轮授权。未把零测试或观察自洽当 Passed。

本轮阅读最新 next-session-handoff、KR API 合同审计关键段、KQ/KP 主要终态、current-execution-rules/current-stable-baseline/current-work-item全文，以及正式 csproj、局部 props/targets、NativeRun、ExitDiagnostics、WindowThread、OutputWindow、SwapChainObservation、PresentDiagnostics/GpuReadback相关链。长文工具截断未冒称全文逐字审阅。AGENTS/instructions名称检索无适用结果。初次结构查询误用不存在的 .sln 路径返回失败，随后以实际 csproj取得文件索引，没有改工程。

## 修改与静态可行性

唯一代码新增 `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.KsThreadScope.cs`，全文件 `#if KS_THREAD_SCOPE`，默认关闭；没有修改生产、原方法、公共helper、NativeRun/ExitDiagnostics、项目或打包，没有新增Host、脚本或委派。

设计固定 A(test)→B(owner)：各自创建原参数窗口及普通Device9，分别释放和销毁；B单个既有owner Invoke包住Establish、初始化、提交、完成、诊断、采样及最终Release。Invoke同线程分支直接执行；窗口过程仅ValidateRect/DefWindowProc，无等待测试线程路径。partial内完成逻辑保留原query/消息泵/两次DwmFlush顺序次数，并打印query/flush返回值，不额外完成、不producer读回。像素失败仅作为布尔观察，A不因像素断言阻止B；基础API/清理/会话失败则抛出停止。

严格consumer门client/screen均00563412，提交前任一点已为目标则baselineValid=false，不变色救绿；参数0/HAL1/0x26、32×16/count1/swap1/format0、FF123456及单Clear/Present保持。最终断言要求B真实consumer通过，不允许两侧失败的观察报告变成测试Passed。一次固定顺序与重建窗口必有时序/桌面混杂，即使未来owner成功也不构成强因果证明。

**静态入口判断不足：** 根据NativeRun的MTP注册链及既有guard/DefineConstants模式判断可执行，但本轮传入的路径形 `--filter /WpfGfxShape.ComAcceptance/D3DImageTests/WhenKsLifecycleThreadScopeIsComparedThenOwnerConsumerMustDeliver` 实际零命中。未在执行前取得该二进制真实测试UID，属于本轮入口验证失误；不能宣称静态可行性已经被真实精确执行证实。日志发现1817个测试不等于此方法已命中，也不足以区分filter语法问题与特定方法发现问题。按授权失败即停，没有另行发现/执行任务来修正。

## 构建发布及唯一任务

- normal COM Release win-x86小build **1**，成功，133 warnings/0 errors，4.90秒；guard关闭。
- COM Release win-x86 self-contained **10.0.13** publish **1**，Exit0；命令以既有 `-p:DefineConstants=KS_THREAD_SCOPE` 启用，仅独有 `artifacts/ks-thread-scope/{obj,bin,publish/acceptance-x86}`；复用KP Native，没有项目/打包修改或跳过分析器。发布60行日志标明53994字符、前33994被截，未称全日志逐字核实。
- Native publish **0**。AotDllPath指向KP包的Native/wpfgfx_cor3.dll；预期20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D。因测试未进入，代码内SHA检查没有执行，**本轮没有实际加载hash认证，不用包路径或历史hash冒actual。**
- Windows沙箱 **1/1**，task **8ffcb9123d93489984049f05e654e53b**，BL120/外180。监督2652/child3424；UTC 2026-10-26 11:53:28.0167454–11:53:28.7355408。
- 精确测试尝试 **1**，实际执行 **0**，角色 **0/2**，native提交 **0/2**；没有x64/official/shader/resize/full或后续任务。

## 亲读实证及证据边界

本地根 `artifacts/ks-thread-scope/readback/x86/Results/ks-x86/`：已亲读supervision.json全部40行、stdout.txt全部12行、stderr空、TestResults/child.trx全部22行；直接读stages.jsonl确认不存在，未以枚举遗漏断言缺TRX。Diagnostic搜索确认MSTestDiscoverer发现1817tests，但未证明指定方法执行。

TRX run **33c20579-d3e5-4ed0-9e40-54da705ebc0c**：Results/Definitions/Entries为空；Counters total/executed/passed/failed均0，ResultSummary Failed，RunInfo明确Exit8。stdout同为总0，部分本地化文字编码损坏但数字及路径可读。业务child **Exit8/0x00000008、Reason exited**；监督源码正常分支返回child exit，但工具仅报告执行完成，**没有独立数值工具exit**，不混同二者。

TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut/OutputDrainCancellationTimedOut均false，HasExited/OutputCollected=true，SupervisorError=null。没有timeout/crash证据，停止原因是精确身份门失败。

实际运行根测试程序集路径 `C:\CodingAgentSandbox\Tasks\8ffcb9123d93489984049f05e654e53b\WpfGfxShape.ComAcceptance.dll`，监督记录SHA **D56FB20357A5D367B8D79A0C7A0262181DB4C8E55DFF254A6A3CE8723568C51F**，MVID **d5082b0d-734d-49ed-8121-822710749ba8**，runtime **.NET10.0.13 / X86**。这证明测试根身份，不证明诊断方法被执行。

**candidate actual：not loaded（没有诊断角色进入或候选加载证据；零测试任务未生成模块清单，不能当作完整进程模块审计）。** 系统d3d9实际路径/hash也未采集。没有A/B窗口/线程ID、参数实测、client/screen前后像素、cooperative/query/flush返回值、真实device/factory Release计数、DestroyWindow或owner线程Join实测。未创建对象不写refs0；静态参数与清理代码不冒充实际观测。

## 下一唯一门

先保持停止。由监督方审查并另行授权**精确入口身份修正门**：须从已发布诊断二进制取得真实MTP测试UID/发现证据，证明精确入口只命中该单测，再决定是否重新授予有界A/B预算；本轮不自行追加发现、修filter重跑、发布或任务。线程方向未获得任何像素结果，不能以本轮宣布“两侧失败停止该方向”，也不能用同义改名重跑绕开授权。若后续获准且两侧真实失败，终止此线程方向；若owner真实通过，仅提出原方法最小合同修正及另行验收门，不直接改业务。完整替换及原shader/resize仍开放，历史结论不覆盖。
