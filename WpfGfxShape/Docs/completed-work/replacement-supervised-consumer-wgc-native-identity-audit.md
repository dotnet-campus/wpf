# WGC clean 包 Native 身份漂移审计

## 最新接续（保留原审计）：唯一正式恢复发布已完成

见[KP/20B3发布准备报告](replacement-supervised-consumer-wgc-kp20b3-publish-readiness.md)。新授权仅一次COM publish已成功，显式AotDllPath绑定KP clean Native、全新独立输出，正式目标摘要为20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D；旧C427不覆盖。本次Native/build/沙箱/发现/测试0、源码项目0改。输入选择门恢复，复制后PE/SHA与官方实际模块/受信版本、新test SHA/MVID/UID仍待真实任务；不冒独立认证，不为不可用非执行工具无限阻塞。以下禁止发布及Blocked为原审计阶段事实，真实运行仍未授权。

## 1. 唯一裁决：输入身份门 Blocked，先恢复明确复用的20B3候选，不运行

本阶段仅静态读取与Docs Markdown修改；build、COM publish、Native publish、沙箱、发现、测试体、consumer、标尺全部新增 **0**。无终端、脚本、委派、源码/项目/包/清单/hash文本修改。没有非执行二进制哈希或PE解析能力，未重算任何DLL摘要，也未用文本工具搬运二进制。

**当前C427包不获得真实测试前身份放行。** 最强证据解释是：此前KS显式选择KP候选，本次WGC发布省略AotDllPath，重新落入生产默认发布位置，选到了不同的已有候选；不是已证本阶段隐藏Native发布，也不是由hash差异证明产品缺陷。C427不是没有历史的随机新摘要，它与旧EX同DLL实验候选的历史实际身份相同，但该历史不是默认KN/KP候选的验收，更不是当前WGC字节的独立证明。不能靠official-only跳过输入门。

唯一准备目标是通过现有正式发布入口显式绑定已验收来源的 **20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D**，并关闭复制后身份门；不要求重发Native。当前禁止任何发布，故停在文档裁决。若未来希望改为接受C427，须先补齐本节列出的来源/字节证据并另作明确复用身份变更裁决，不与恢复20B3并列为当前可执行路线。

## 2. 读取依据与权限边界

读取最新revision-validation与next-session-handoff顶部；章程00、charter-implementation-contract、charter-evidence-and-continuity、current-execution-rules、current-work-item及current-stable-baseline；局部Directory.Build.props/targets、根Directory.Build.props、global.json及ComAcceptance/global.json；ComAcceptance.csproj、Package.targets、AcceptancePaths.cs、NativeRun.cs、ConsumerWgcDiagnostic关键身份段、生产csproj；KN/KO、KP、KS、EX/EY历史报告；最新59行发布日志的参数/结果及尾部与定向搜索；当前包/回收identity文本、runtimeconfig、deps关键条目与发现原始输出。AGENTS.md及局部instruction名称检索未取得额外适用指令。大历史文件只读相关段，不冒称全文重审。

以下路径除特别标注外相对于仓库根；`WpfGfxShape/artifacts/`下文件为现有ignored证据，不是本阶段创建。

## 3. 发布完整参数与候选选择链

最后发布日志第1行完整调用为：

`dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c ConsumerWgcDiagnostic -r win-x86 -f net10.0-windows10.0.18362.0 --self-contained true -p:ComAcceptanceSandbox=true -p:AotRuntimeIdentifier=win-x86 -p:PublishDir=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/consumer-wgc-runtime-fix/publish/ -p:ComAcceptancePackageDirectory=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/consumer-wgc-runtime-fix/com-acceptance-package/`

第3–4行Exit0/成功；57–58仅列ComAcceptance程序集和独立publish输出。原stdout52809字符，前32809已截断。没有AotDllPath、NativeAotPath、全局RuntimeFrameworkVersion、额外生产项目、NoBuild或NoDependencies参数。不能将未保留stdout冒完整构建图/审计日志。

实际消费属性是 **AotDllPath，不是NativeAotPath**。局部源码/文档检索未发现NativeAotPath作为此流程选择入口；不能建议传这个未支持的名字来修复。

- `Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj:18–19`：AotRuntimeIdentifier未传时win-x64；本次显式win-x86。AotDllPath仅在空时默认到`$(MSBuildThisFileDirectory)../../artifacts/publish/WpfGfxShape/Release/$(AotRuntimeIdentifier)/wpfgfx_cor3.dll`。
- 本次默认解析位置为`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x86/wpfgfx_cor3.dll`。已列目录确认该DLL和PDB存在，未重算其SHA。
- ConsumerWgcDiagnostic只设置诊断常量、Optimize、DebugType、x86/win-x86；不会选择KN/KP Native。局部Directory.Build.props确定artifacts及普通COM输出根，PublishDir命令行值覆盖普通输出选择。测试项目没有生产ProjectReference；None文件项复制不是生产构建依赖。
- csproj:37将AotDllPath作为None，Link=`Native/wpfgfx_cor3.dll`、CopyToPublishDirectory=`PreserveNewest`。因此链条是默认生产位置 → `consumer-wgc-runtime-fix/publish/Native/wpfgfx_cor3.dll` → clean同相对路径。源路径不是publish根官方同名DLL，也不是KS/KP包。
- Package.targets:48–77清理的是clean输出目录，从publish的显式资产集合复制，随后对 **publish/Native** GetFileHash写clean根aot.sha256。不是直接重算clean副本，也不是重新发布Native。publish本身未在该target清理，PreserveNewest及现有publish残留的可能性不能仅靠clean输出命名排除。
- csproj:42在沙箱程序集metadata写固定相对`Native/wpfgfx_cor3.dll`，不是构建机绝对输入。运行时另有WPFGFX_CANDIDATE_DLL覆盖，见§6。

此前KS报告:232–234明确AotDllPath指向`artifacts/kp-resize-pixels/com-acceptance-package/Native/wpfgfx_cor3.dll`、Native发布0；KP报告:30明确复用KN native-x86。**已证本次调用丢失显式复用绑定；没有证据表明默认路径公式本身被本轮改了。** 环境属性/历史文件复制及publish输入完整快照未保留，默认解析是按可读工程及完整命令成立的高置信解释，不冒称取得当时MSBuild求值快照。

## 4. 两种身份及C427历史来源

|身份|已取得证据|不能外推|
|---|---|---|
|20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D|KN/KO报告的x86实际Native身份、17/17及路径；KP复用记录；KS正式初始化I386/无CorHeader记录；本次亲读KP clean根aot.sha256同值且Native文件存在|不是当前WGC Native字节；KN共享clean根当前文本为x64的40D09398…，不能凭目录名选KN共享包作为x86源|
|C427438AF11B8B4616AC732120B32B94EC848A8AD6B05406381C2C8A781DB267|当前WGC clean与discovery-readback根aot.sha256均同值；EX/EY报告:13、29记旧两task实际Native同SHA、.NET10.0.13/X86/指针4|摘要文本相同不等于本次已独立核验二进制、PE、来源和默认源码一致|

C427可追溯历史文档是`Docs/completed-work/replacement-supervised-prefix-samedll-control.md`：EX-2记录生产实验NativeAOT发布 **两次**，第一次未补后置日志未运行，补日志后的第二次作为同DLL对照；正式DR自包含发布一次。实验是前序十重载运行时direct/generated选择、后置slot6/QI0/Release2固定generated，留日志和真实Ex分支。EX-3 direct task `d505891df5534c42ba2eff70bf112fdb`，Native C427、测试SHA0273A1DE5529ED292A2D062C90FD05E241CDB9479FF427B3F0DD91A940A7F2D5、MVID e6656377-7943-4d3b-9d8e-b7400971a272，三轮refs0/Exit0/1条Passed。generated task `b4e9509fac5d42fdb1264c21fb219e87`同DLL，退出C0000005。后续EY记录恢复默认源码且只做正常增量build，**不是默认NativeAOT重发**，故旧实验DLL仍可存在；源码已恢复不能洗白已有实验二进制。

本次精确读取`WpfGfxShape/artifacts/ex-prefix-samedll/run-direct/Results/aot-identity.json`不存在，artifacts顶层枚举也未列EX目录；因此C427的权威历史链来自既有审核报告中的已核actual原件记录，本次没有重新取得原始JSON/生产发布完整命令、SDK/编译常量全表或源码快照与PE的独立绑定。不能把报告推成当前默认生产构建认证。

原因分类裁决：

1. **选到另一既有candidate：有强证据支持**，省略AotDllPath且C427历史早已存在；实际当前默认源与publish/clean三份字节一致性仍未独立核。
2. **默认源路径改变：是有效输入从KS显式KP变回默认生产位置，不是已证路径公式改写。**
3. **包hash文本不匹配：未证实也未排除。** 两处文本一致及正式GetFileHash只证明生成规则/记录，不能代复制后SHA/PE；发现不触发AssemblyInitialize，不提供hash匹配实证。
4. **已有另一产物/残留publish Native：有历史候选来源解释；具体文件搬迁及PreserveNewest选用需字节/时间/发布输入快照补证。**
5. **隐藏重建发布：当前无肯定证据。** 可读项目无生产依赖，正式target仅Copy/GetFileHash/写清单，可见发布日志只COM输出；日志前段截断和环境未归档意味着不能证明所有进程历史绝对不存在。旧EX两次Native发布是历史，不扣本轮Native0预算，也不能拿来声称本轮新发。

因此：零Native发布声明与本次COM复制另一既有DLL并不矛盾；但若要求“复用此前已验收20B3身份”，本包没有满足连续身份要求，前轮“既有生产x86候选”描述不足且遗漏EX实验身份风险。不是产品功能缺陷裁决。

## 5. 官方根DLL与candidate角色不得混同

clean根`wpfgfx_cor3.dll`由Microsoft.WindowsDesktop.App.Runtime.win-x86/10.0.13的RuntimePackAsset进入publish及clean；deps:963–965给fileVersion **10.0.1326.50504**。Native子目录同名DLL是AotDllPath输入。清单:621另列Native，根官方与Native不是重复目标。runtimeconfig includedFrameworks为NETCore/WindowsDesktop10.0.13，deps目标win-x86，SDK投影10.0.18362.57；它们是声明来源，不是二进制重算或运行实际加载身份。

官方选择`AcceptancePaths.OriginalDll`默认以实际PresentationCore（Visual类型程序集）目录拼根官方DLL，WPFGFX_ORIGINAL_DLL可以覆盖。WGC:111–117用NativeLibrary.Load(requested)后，以返回模块基址从Process.Modules.Single取 **Actual** 文件路径，对实际路径计算SHA256并记录ProductVersion、测试SHA/MVID、RuntimeInformation.FrameworkDescription/ProcessArchitecture/PointerSize。导出从这个handle取，不借candidate调用。

该代码能提供未来实际模块路径/hash/产品版本/.NET runtime，但 **没有显式断言Requested==Actual、没有预置官方可信SHA比对、没有记录完整模块集合或PresentationCore的hash/版本**；源码与deps只能确定设计，当前没有执行后identity输出。将来应回读evidence.jsonl的identity，规范化比较Requested/Actual均在任务根官方位置，核实际hash/PE/产品版本与同架构官方10.0.13受信资产及PresentationCore配套，结合runtimeconfig/deps和监督进程版本；不能只读deps声称官方已验证。

## 6. official-only的candidate门及“未加载”证据

`AcceptancePaths.ValidateIdentity():51–69`在AssemblyInitialize中对candidate **File.OpenRead + PEReader**，拒绝CorHeader/不匹配Machine，SHA256与包根aot.sha256比较；Initialize:79–89写Results/aot-identity.json。没有NativeLibrary.Load或调用生产导出。WGC体只需官方模块，**不需要为了认证输入而实际加载candidate**，更不应为此引入一次候选运行改变official-only合同。

这道门验证“文件与包自带摘要一致且架构正确”，不验证它是否为20B3或来自已验收源码。即使将来初始化通过C427，也不自动获得复用身份授权。WPFGFX_CANDIDATE_DLL可改变解析文件，但仍须与当前C427摘要相等；不能拿它指向20B3后手改aot.sha256绕门。WPFGFX_ORIGINAL_DLL覆盖也必须明确记录。

WGC identity中的`CandidateLoadedByDiagnostic=false`是 **写死的意图声明，不是全进程模块枚举证明**。基址Single只定位官方handle，没有检查所有同名DLL；当前list无候选初始化/加载业务输出。静态代码支持“本方法无显式candidate Load”，但本次不能声称“进程实测从未加载candidate”。未来若要求动态负证据，应取得现有受支持的进程模块/加载事件记录，覆盖官方加载前后及结束，路径与hash区分根官方和Native候选；阶段快照只证明时点，绝对“从未加载”需加载事件覆盖。不能用false常量或KS另一task的模块快照替代。当前无该证据，不追加执行、不改源码。

## 7. 已有list足以证明什么，UID边界

本次亲读`artifacts/consumer-wgc-runtime-fix/discovery-tool-output.txt`全部7行：MSTest4.0.1 / win-x86 / .NET10.0.13；独立节点行 **WhenOfficialConsumerTwoMarkersAreObservedByGdiAndWindowCapture**；同task程序集路径、摘要找到1个、251ms。结合既有调用记录的完整FQN等值filter，它 **不只是概括总数**，支持目标显示名确实被唯一列出，不是旧KS发现1817但零命中的情形。

输出没有类名/FQN节点行或UID，因此严格的二进制节点FQN/UID身份未独立落盘。不能说它已有UID级唯一认证，也不能反过来说没有实际列出目标。原工具文件没有显式数值exit。回收根json名称检索只取得deps/runtimeconfig，log/diagnostic检索只有框架DLL，TestResults目录读取无子项，精确Results/aot-identity.json不存在；没有取得可读节点诊断/UID。名称枚举不是穷尽不存在证明，但当前无可取UID证据，不追加发现，FQN不能充UID。list不执行AssemblyInitialize/测试体，不等于候选PE/hash门通过、official已加载或功能验证成功。

## 8. 最小恢复准备与停止点

1. 保留现有C427包和历史原件不动。恢复目标选 **KP clean Native**：`d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/kp-resize-pixels/com-acceptance-package/Native/wpfgfx_cor3.dll`；本次文件存在与根摘要已核，当前字节需受支持非执行SHA/PE读取，预期20B3完整值、I386/无CLR。可交叉核KN独立`publish/native-x86`，不得误用KN共享clean（当前x64）。
2. 当前WGC clean的编译metadata固定相对Native路径，单纯改变发布参数不会追溯改现有包。通过 **现有正式流程** 改AotDllPath并生成新的干净WGC包，须另授权 **一次COM publish**，配置/RID/TFM及局部框架元数据固定沿本次成功组合，显式`-p:AotDllPath=<上述KP绝对文件>`，使用全新独立PublishDir与ComAcceptancePackageDirectory以避PreserveNewest残留；不传NativeAotPath，不全局RuntimeFrameworkVersion，不发生产Native，不添加新Host/脚本/补件清单。
3. 不是算法上只有publish才能复制DLL：如果将来另授权受支持二进制安全复制与hash校验，可讨论整体包身份迁移。但本阶段无此能力/授权，不能用文本工具搬DLL、只改aot.sha256，或把旧KS程序集当当前WGC替代。现有正式参数路线确实需要新COM publish；当前budget0，**本阶段不执行**。
4. 新包出厂后须核源/publish Native/clean Native三份实际SHA/PE、根摘要与路径清单，独立保留根官方SHA/版本/架构/受信来源、测试SHA/MVID/诊断配置与runtime闭包；有非执行能力则直接读取，无则明确缺口及另授权条件，不为重算hash偷偷执行程序。
5. 现有发现显示名+数量证据可保留，UID仍unknown。新publish后测试字节可能变，不能自动继承旧UID/测试SHA。若真实运行合同要求UID，须另授权受支持节点输出能力；当前不再list。真实运行本身也必须另授权，且先满足输入门；之后回收初始化aot-identity、官方实际identity、监督/TRX/业务内容及候选未加载证据，层级不得混淆。

总体影响仅是WGC真实验证前的输入来源阻塞，不推翻KN/KP各自身份上的历史结果，不判C427功能缺陷，不给默认产品或完整替换放行。最小新增执行需求是另授权正式COM发布和相应身份读取能力，**Native publish仍为0需求**；本阶段到此停止。
