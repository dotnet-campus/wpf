# postfailure 新Native x86：零执行来源恢复与下一门裁决

## 后续终态索引

本篇保留零执行恢复时的历史边界。[后续source-rebuild收尾](replacement-postfailure-source-rebuild-x86-closeout.md)已在新独立根持久化build1/Native publish1/COM publish1并完成候选x86两个有限合同，限定新来源闭合；不补造本篇缺失的原日志，不倒签原15A545源码。唯一下一建议为另授权x64同冻结源码两合同验证，不执行。旧官方仅同测试/运行/夹具条件的历史对照，整体Open。


## 终态与范围

本阶段生产/测试/配置代码修改0，build/test/publish/沙箱新增0；无终端、脚本、DLL执行、hash执行器或再委派。仅通过现有文件读取、目录/名称/内容搜索及最近日志读取恢复证据并修改文档。工作区根 W=`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape`；P=`WpfGfxShape/`（W下嵌套目录）；R=`WpfGfxShape/artifacts/postfailure-new-native-x86/`。下列路径均相对W，不把W与P混为一层。

已先读最新恢复入口、独立审计、新Native快照、first-validation、执行规则/基线/当前事项/完整方案和适用项目配置。新增发现：Native本轮专属PublishOutputs、ILC/link响应文件、正式clean包621行清单；不是发现Native原始publish日志。停止无界历史指纹追索，零执行恢复阶段完成。

两道门分开裁决：
- **门1：足够复用已运行产物的有限合同——成立。** 保留独立审计认证的15A545…四角色真实x86通过及清理，不降为未执行。只绑定当时实际加载身份和两个有限ABI合同。
- **门2：可复现当前源码发布来源——未闭。** 可恢复部分路径/工具链元数据，不能证明当前全部源码字节等于当时构建输入。生产修正保持源码待验候选，整体/新x64基线不升，完整替换Open。

## A：直接日志、命令、返回值

### A1 本轮COM publish（不是Native，也不是normal COM build）

最近保留日志共60行已读，命令为：

`dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -r win-x86 -f net10.0-windows --self-contained true -p:RuntimeFrameworkVersion=10.0.13 -p:WindowsDesktopRuntimeFrameworkVersion=10.0.13 -p:ComAcceptanceSandbox=true -p:AotRuntimeIdentifier=win-x86 -p:AotDllPath=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/postfailure-new-native-x86/native/wpfgfx_cor3.dll -p:ComAcceptancePackageDirectory=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/postfailure-new-native-x86/clean/ -o d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/postfailure-new-native-x86/publish/`

日志2–4行：目标`.`、退出码0、成功。第7行明确stdout原52785字符，仅保留末20000，前32785不可恢复。第58行生成`P/artifacts/bin/WpfGfxShape.ComAcceptance/Release/net10.0-windows/win-x86/WpfGfxShape.ComAcceptance.dll`；59行输出R/publish。保留段有warning，不推断133warn/0err汇总。本报告保存命令/退出与截断事实，作为新建的审阅摘录，不冒完整原stdout归档或原工具持久日志。

### A2 四角色运行沿用已完成独立审计

原件完整索引和全部合同认证见[独立审计](replacement-postfailure-execution-handoff-independent-audit.md)，本轮不假装重新逐段认证四份business。原件根`R/readback/<角色>/Results/<角色>/`，各有supervision.json、stdout.txt、stderr.txt、TestResults/child.trx；business子路径：

|角色|task|business子路径|
|---|---|---|
|postfailure-official|6edf1e54cb4d4d33acc0ea81ee5b78c6|PostFailureAttach/7c4248e2567349f3bec021548c917482/business.json|
|postfailure-candidate|06db04ddad8f4f149f42368e8449d05f|PostFailureAttach/9634aa34b15b4f1599e49e6215026d93/business.json|
|isolation-official|9e6ff003bce14ec494beb87175f0ec69|PartitionFailureIsolation/ffcc9a89995f4ae094fd9a7269655ec1/business.json|
|isolation-candidate|1d0a27b241834cbd8d1ed759596edb87|PartitionFailureIsolation/3833db728df441cba0901102ff1ff646/business.json|

四个TRX均1/1/1通过，监督Exit0/正常退出、业务清理通过；candidate实际SHA `15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620`、版本1.0.0.0；official实际SHA `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`、版本10,0,1326,50504。共同test SHA `1C56E8CB2F0679FFF67EB217708EFD1DAF969200201E4973802D6927963759BD` / MVID `cc7cfbfe-6b94-4400-814c-a299f138970d` / .NET10.0.13 X86 pointer4。

本轮另亲读postfailure-candidate监督40行与Results/aot-identity.json：开始`2026-10-27T07:56:15.2197255+00:00`、结束`07:56:16.1727027+00:00`，PID11964，task06db…，内120秒，无timeout/kill/drain异常，实际加载根对应该远端任务。四任务UTC顺序07:55:32–33、07:56:15–16、07:57:08–09、07:57:59–07:58:00来自既有审计；这些是运行时间，不是Native构建时间。远端`C:\CodingAgentSandbox\Tasks\<task>`是历史字符串，不等于本地可访问日志目录，也不认证外层180秒调用参数。

### A3 Docs发布命令的证据级别

`P/Docs/com-acceptance-sandbox-operations.md:3–10`给出相对P的生产示例`dotnet publish Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`及默认COM包发布命令。它明确是文档说明，且架构/包方式不同；不能套成本轮x86实际命令。本轮新Native快照只记录Release/win-x86/独立输出的执行前授权，不含实际完整Native命令或返回值。first-validation中Release build1/隐式Debug build1/4unit通过保持历史报告层级，未用COM日志替代。

## B：恢复到的构建元数据与正式复制链（不等价原日志）

令N=`P/artifacts/obj/WpfGfxShape/Release/net10.0/win-x86/`，C=`P/artifacts/obj/WpfGfxShape.ComAcceptance/Release/net10.0-windows/win-x86/`。

|实际读取原件|恢复内容及边界|
|---|---|
|N/PublishOutputs.ef1095c83f.txt（全文1行）|精确记录W下R/native/wpfgfx_cor3.dll；这是本轮独立输出路径的直接元数据，不是publish命令/Exit/源码指纹|
|N/native/wpfgfx_cor3.ilc.rsp（1–25、165–202、210–263，部分段工具截断）|输入N/wpfgfx_cor3.dll、输出N/native/wpfgfx_cor3.obj；NativeAOT win-x86 10.0.13库，--targetos:win、--targetarch:x86、-O、--nativelib、--export-unmanaged-entrypoints、RUNTIME_IDENTIFIER=win-x86。未声称全263行完整亲读|
|N/native/link.rsp（全文48行）|输入上述obj，/OUT指P/artifacts/bin/WpfGfxShape/Release/net10.0/win-x86/native/wpfgfx_cor3.dll，/DLL、/DEBUG、/INCREMENTAL:NO、/OPT:REF、/OPT:ICF；ILCompiler/NativeAOT10.0.13，MSVC14.51.36231 x86库，Windows Kit10.0.26100.0。响应文件不证明链接器已执行成功|
|N/WpfGfxShape.csproj.FileListAbsolute.txt（全文13行）|托管bin/obj DLL、PDB、ref/refint及生成配置路径；不能把托管DLL路径冒最终native DLL；本轮未重算任何本地二进制哈希|
|N/WpfGfxShape.AssemblyInfo.cs（全文25行）|Release，Assembly/FileVersion1.0.0.0，InformationalVersion1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a；只读未编辑生成文件|
|N/WpfGfxShape.sourcelink.json（全文1行）|W通配映射到dotnet/wpf提交3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a；不是本地改动冻结证据。native/wpfgfx_cor3.sourcelink存在，单行读取截断，只确认可见runtime映射，不声称全文认证|
|N/WpfGfxShape.GeneratedMSBuildEditorConfig.editorconfig（全文28行）|TFM net10.0，AOT/trim分析开启，ProjectDir精确指W下P/Code/WpfGfxShape/|
|P/artifacts/obj/WpfGfxShape/project.nuget.cache、WpfGfxShape.csproj.nuget.g.props|restore success、精确源csproj；ILCompiler10.0.13/ILLink10.0.13/CsWin320.3.298等。restore成功不等于publish成功|
|P/artifacts/obj/WpfGfxShape/WpfGfxShape.csproj.nuget.dgspec.json:1–80（输出截断）|项目/输出路径、net10.0、SdkAnalysisLevel10.0.400。根global.json声明10.0.107且rollForward=latestFeature；不能把最低声明版本冒实际执行SDK，10.0.400是元数据线索，不足独立认证本轮实际SDK版本|
|C/PublishOutputs.51e36aeeda.txt:1–16|明确R/publish/exe、testhost.x86、Contracts、Native/wpfgfx_cor3.dll和测试DLL/runtimeconfig；不是R/native生产发布记录|
|C/WpfGfxShape.ComAcceptance.csproj.FileListAbsolute.txt（全文28行）、AssemblyInfo.cs:12–29|COM Release/net10.0-windows/win-x86 bin/obj链；版本1.0.0.0，同提交信息，Sandbox=true，AotDllPath=Native/wpfgfx_cor3.dll。不能识别独立normal build调用次数或133warn0err|
|R/clean/package-files.txt:1–28、500–567（截断）、563–575、606–621|实际正式文件名不是package-manifest.txt。共621行，末段有Contracts两文件、Native/wpfgfx_cor3.dll、aot.sha256、package-files.txt。未逐文件认证全部621项，不把文本清单当每文件内容hash|
|R/clean/WpfGfxShape.ComAcceptance.runtimeconfig.json（全文21行）|includedFrameworks NETCore与WindowsDesktop均10.0.13；实际运行.NET10.0.13/X86另有监督支持|

R/native精确枚举仅DLL/PDB。N/native还精确列出def/obj/响应文件/sourcelink。根/嵌套普通枚举会漏artifacts/obj，已用精确路径成功读取；不以漏项推出不存在。目录工具未暴露文件创建/修改时间、大小或原始工具调用ID，不编造时间戳。PDB/obj/cache等二进制未主动作为文本读取或修改；一次目录内容搜索工具意外返回DLL/PDB命中，未采信其乱码/字符串为证据，随后只按已知文本路径读取。

### 正式项目与复制链

当前`P/Code/WpfGfxShape/WpfGfxShape.csproj`全文40行：net10.0、PublishAot=true、NativeLib=Shared、SelfContained=true、AssemblyName=wpfgfx_cor3，诊断ABI默认false，DebuggerSupport默认false。Silk.NET2.23.0、CsWin320.3.298；含仓库src下Shaders.rc和ShaderEffectsVS.fx两个资源输入。`P/Directory.Build.props`及targets全文确认本地隔离obj/bin，不导入WPF Arcade；不因此认证历史所有求值参数。

当前`P/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj:1–50`明确net10.0-windows、UseWPF、MSTest4.0.1、Sandbox=true时将显式AotDllPath作为Native/wpfgfx_cor3.dll复制到publish，内部元数据使用相对路径。`ComAcceptance.Package.targets:42–70`全文亲读：AfterTargets=Publish、Sandbox/self-contained条件；先验证publish/Native存在，清理指定clean目录，再白名单复制publish依赖/Contracts/Native，正式GetFileHash对publish/Native算SHA256，写clean/aot.sha256及package-files.txt。不是手工凭文件存在推链，也不是此次新执行hash。

可支持的链：Native obj/ILC/link与本轮PublishOutputs路径 → R/native → 已认证COM publish显式AotDllPath → publish/Native → 正式self-contained打包目标 → clean/Native及aot.sha256 → 四task运行身份/actual加载15A545…。A层命令/运行身份与B层正式目标/清单相互支持，但obj可能被共享后续构建覆盖，不能把这一链提升成当时源码字节冻结。

本轮当前源码静态摘录：PartitionScheduler.cs:145–165管理Register/Destroy与业务提交分支；SameThreadChannel.cs:338–342 IsRegistered仅TryAdd成功后true；UceChannelExports.cs:157–193先保留全局表项、Submit、未接受回滚，再等待注册结果。这是当前快照，上一独立审计另已完整审修正/四单元。没有当时完整源码归档/内容校验，将当前摘录补档也不能倒签当时构建输入。

## C：限定范围内未恢复项与最小补证

实际检查W下`.coding-agent`（无可见子项）、`.coding-agent/sandbox-results`（报告不存在）、P下`.coding-agent`（报告不存在）；只暴露最近build/test日志，没有可见持久日志文件路径。R仅clean/native/publish/readback；R内.log名称搜索、P/artifacts内.binlog名称搜索无命中。上述只是本轮可见范围未定位，不推断全磁盘/工具后台不存在，也不再次用目录漏项作不存在证明。未访问/要求未暴露工具后端。

未恢复：独立Native publish完整命令/原stdout/返回值/实际SDK/准确起止与次数；normal COM build独立日志/133warn0err；first-validation独立build/test原日志（历史自述数字保留）；当时输入源码及嵌入资源的冻结身份。FileList、PublishOutputs、SourceLink提交、程序集版本、PDB存在或时间先后均不能补成冻结证据。缺失历史日志不可用新执行伪造补回，只能为当前源码建立新证据。

**normal COM build原日志缺失是历史审阅边界，不是必须重跑四绿的产品阻塞。** 已有COM publish Exit0和真实四体通过，不以缺normal build原日志否定二进制可运行或运行合同。第一门4unit原日志同理，不暗中追加单元重跑。真正阻塞门2的是当前源码与Native发布输入无法绑定；若需要认证当前修正的发布来源，应做一次有来源冻结的新Native发布，而非无期限索取旧日志。

## 唯一推荐下一门：另授权当前源码x86来源重建及最小定向绑定

本轮只推荐、未获执行授权，以下预算不在本阶段消费：代码/项目/依赖/SDK调整0，生产Native Release/win-x86 publish≤1、normal COM Release/win-x86/net10.0-windows build≤1、COM self-contained publish≤1；candidate postfailure≤1、isolation≤1，默认沙箱≤2，每体精确等值FQN、--supervise-exit、内120/外180，失败即停/重试0。Native发布先成功；normal build仅在新门建立当前构建审阅记录，不倒填旧133warn0err；随后COM显式绑定新Native及两framework10.0.13、新独立publish/clean，不覆盖R。

发布前保存本次真实输入快照（生产源码/项目、本地props/targets、global/NuGet配置、嵌入src资源及必要生成输入；构建工具实际SDK/参数/返回和可见输出），采用现有文件能力/正式产物身份链，不新增hash执行器、不为了凑指纹扩工程。每次构建/发布后立即归档当次可见日志，区分工具截断段；冻结前后不得尾改源码。新记录解决的是新来源，不回溯认证15A545构建时刻。

精确candidate方法（类WpfGfxShape.ComAcceptance.UceChannelTests）：
1. WhenFailedPartitionAcceptsLateSharedChannelThenSourceClosePreservesFailure
2. WhenAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives

必须完整回读TRX/监督/business/identity/stdout/stderr，绑定actual模块SHA/handle=BaseAddress/requested=actual、test SHA/MVID、runtime/架构、真实Destroy/disconnect/deinitialize与正常cleanup。postfailure Create0/非零L、两次flush88980406；isolation A/S失败、健康B关闭A/S前后新更新全0。

**official是否复用的具体条件与预算：** 已有两个official不是旧推测，而是同testSHA1C56…/MVIDcc7c…、.NET10.0.13/X86/pointer4、实际官方5EBD…的两个合同及完整清理已认证；仅Native候选改变不会改变它们的预期。因此新包确认上述测试字节/MVID、runtime、official模块身份和角色/参数/Contracts/测试依赖保持对应一致时，直接引用这两次official，无须再跑。仅版本号相同不够，测试字节变化、runtime/official身份或实际夹具/合同变化即停止默认两任务计划；需另授权受影响official各≤1并先绿再相应candidate（两项均受影响时总沙箱≤4）。这不是为了normal build日志而重跑official。新publish改变测试SHA时不能自动把旧official绑定新测试身份。

**hash裁决：** 不要求每次构建保持同hash，也不要求必变。若新Native hash不同，建立新身份并运行上述两个candidate，新绿只绑定新身份，旧15A545四绿仍保留。若新hash相同，先用新发布来源记录和正式身份链证明新产物身份；可引用既有相同二进制的有限行为证据，但不能据同hash把当前源码认证回溯到旧构建。推荐仍保留最多两次candidate预算用于新clean/新来源链定向确认；不把相同hash作为再次跑official的理由。

若复用条件不满足或首次步骤失败，停止并报告，不擅自使用四任务条件预算。无需full/x64/显示/KN17/WGC、发现或4unit重跑；本门完成后停止，再据来源/身份/有限合同交付判断，不宣布完整替换。

## 账本与计划状态

零执行恢复：已完成；来源层级A/B/C：已交付；门1：有限运行可复用；门2：待新授权来源重建，不再停在“请提供日志”。本阶段所有执行新增0。原四绿、20B3 Create88980406/output0与unknownRegistration=true/cleaned=false旧红、40D0旧身份、历史双架构isolation、direct/public/WGC/KS/KN/Ledger各独立账不冲销。真实注册异常回滚、composition线程重入Destroy、极端资源异常及其它原未覆盖保持未证；显示冻结，生产源码待验边界及整体Open不变。本文覆盖旧文档中“新Native必变/必须新SHA”的字面断言：须真实测定身份，不预设hash必变或必同；旧断言只保留为历史。
