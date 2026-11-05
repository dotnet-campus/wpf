# DA：当前默认 COM full 验收边界独立预审

## 1. 范围与裁决

本轮仅专用工具读取和新增本文；无终端、直接/间接脚本、构建、发布、执行或 full 重跑。未修改生产、测试、配置、CZ 包/报告及共享交接。CZ 正在进行的双架构运行结果不属于本文已知事实。根目录为 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape`，下文路径相对此根；测试简称 T=`Tests/WpfGfxShape.ComAcceptance/`。

**默认正式入口仍是 NativeRun → MTP；默认源码未启用 CQ/CS/CB/CD 探索符号，BW/CK 回归仍可发现。监督入口存在确定的 120 秒内部硬上限，但没有本轮耗时证据证明当前 full 必定超过它。不能把外层 900 秒当内部 900 秒，也不能预认工具阻塞。当前 CF/CX/CY 仅有限修复验收，完整替换未完成；当前默认 full 必须实际消费当前候选，最后稳定 Tests/ComAcceptance × x86/x64 Release 四格仍须真实验证。**

## 2. 权威与亲读范围

亲读 `Docs/handoffs/current-execution-rules.md:1–27`、`current-work-item.md:1–31`、`current-stable-baseline.md:1–20`；完整方案 `Docs/wpfgfx-complete-replacement-validation-plan.md:1–120` 分两段读完。规则23–25、方案34–43/89–120要求最后源码/实际身份、原始内容、真实恢复和最终矩阵；历史基线页的 .NET10.0.9/SHA 不能覆盖 CY 最新有限证据。

指定报告：CY `replacement-supervised-fill-restoration-acceptance.md:1–38` 全文；BD `replacement-supervised-comacceptance-baseline.md:1–93` 中1–56长输出截断于56，56–87截断于87，均以56–87、87–93补齐；BN `replacement-supervised-x86-sampler-abi-diagnosis.md:1–86` 首段截断于51后以51–86补齐；CL `replacement-supervised-batch-failure-ownership.md:1–87` 首段截断于49后以49–87补齐。CX delivery 实读1–40，41–42未读；不称全文。状态汇总页仅读最新1–43返回至43截断，不以其历史段作证。

项目枚举确认仅生产及两个正式测试项目；已枚举正式 ComAcceptance 文件。工作区名称查找未发现 AGENTS，不声称检查工作区外祖先规则。源码索引未用于推断未读定义。以下均为此次读到的行号，不保证并行后快照不变。

## 3. 默认入口、超时及最小建议

- `T/WpfGfxShape.ComAcceptance.csproj:1–37` 全文：net10.0-windows、UseWPF、Nullable，Exe，StartupObject=NativeRun，禁生成测试入口，MSTest4.0.1。`T/global.json:1–10` 全文：SDK10.0.107/latestFeature、MTP。`Directory.Build.props:1–11`、targets:1–12全文，无上述探索 DefineConstants；csproj无该设置。未读取外层全局注入或 CZ 实际构建日志，故这只是仓库默认配置结论，不能代替实际包证明。
- `T/NativeRun.cs:1–53`：Main 14–15只在首参数精确为 `--supervise-exit` 时进入监督；21–28为已有隔离角色，其余29–34交 MTP，未提供 results-directory 则自动添加 exe 基目录/TestResults。默认 MTP 无本文件设置的 full 总期限。RoleAsync 37–51给 PublicAbi/Pcore 等子角色独立3分钟期限，非父 full 总预算；超时会杀进程树。这与监督总上限不能混同。NativeRun 65–85另读，未称其余比较用例全文审计。
- `T/ExitDiagnostics.cs:1–163` 全文（首段1–64截断，64–125、126–163补齐）：RunAsync 24要求去掉开关后恰好3参数，即 root、整数 seconds、filter；25将 root 转绝对路径；26–27只允许1–120，0、负数、121、900均拒绝。38始终注入 `--filter`，无省略 filter 的专门分支。不能假设空字符串或任意通配表达式等于无过滤 full；须以实际发现列表/参数和父 TRX 核实，不把小组过滤命中数拼成 full。
- 38给子 MTP设置 root/TestResults/child.trx 和同步诊断目录，40–41给阶段与运行根；不会把所有业务输出统一迁入 root，AcceptancePaths.Results 仍由程序集元数据解析。
- 74–108：内部期限到即标 timeout 并杀整树，等待退出10秒、输出排空5秒；监督退出取消130、timeout/drain timeout124，否则返回子退出码。外层运行工具延长到900或1800秒不会改变 seconds 的120秒检查或 CancelAfter。其他异常路径110–124可再清理；不能把120+15当绝对可证明的全部系统调用墙钟上限。

**上限是否使 full 不可完成：** 若当前父 full 实际超过120秒，当前监督方式必在完成前终止，确定不适配；BD full 给了外层900秒而未给可据以证明超过120秒的耗时（BD44、64–66），不能据此断言这次必不可完成。当前默认串行（`T/TestSettings.cs:1` 的 DoNotParallelize）且包含多角色等待，120秒不是已有证据支持的完整预算。若 CZ 传了900给现有监督，会在启动测试前参数拒绝，不是工具阻塞；若 JSON 显示TimeoutSeconds=120且TimedOut=true，是内部期限证据。若直接正式MTP自然退出，则没有这个内部120限制。

给后续 CZ 的最小有据建议（本轮不实施、不打断已运行任务）：

1. 现有源码可直接走正式 MTP、无 `--filter` 的 full，明确给绝对 `--results-directory`、独有 TRX 文件名、同步诊断；外层先采用已有 BD full 的900秒预算作为有界参考，而非成功保证。专用执行能力最大1800秒；需留回收/退出余量，不能无限延长或定向拼 full。直接入口缺监督退出字段时如实报告，不从控制台猜退出码。
2. 若必须同时具有监督退出字段和真正无过滤 full，最小协调修改应限现有 ExitDiagnostics：保留三参数兼容，为 full 明确定义“不注入 --filter”的语义，放宽有验证的上限（例如900秒，外层960秒或更大且≤1800），维持取消/杀树/排空及记录。900来自既有full操作预算，不是已测足够耗时。不要只把调用参数改900、不要去掉所有期限、不要改生产或阈值。本轮仅建议，不能冒称已修复/已验证；CZ应先交付当前运行事实再由协调方裁决是否需要。

## 4. parentTRX、完整回收与退出判读

parentTRX 是报告中的父 full 文件概念，不是 NativeRun 自有参数。直接 MTP 应以沙箱中真实任务或包根下的绝对目录作 results-directory；监督模式其 root 必须定位到可记录的沙箱绝对位置，所得父文件为 `<root>/TestResults/child.trx`，不是孙角色同名TRX。不可把本机 d:工作路径、旧task路径当新沙箱有效目录。无新taskID时不能预造绝对任务路径；最终据执行记录补全，且同时给本地绝对回收路径。

AcceptancePaths 模块初始化13–16在sandbox切CWD，但注明MTP更早解析CLI相对目录；因此不能依赖后切目录修正相对parentTRX。亲读 `T/AcceptancePaths.cs:1–91`（首段截断后73–91补齐）。建议回收整任务，至少 parentTRX、Diagnostic、stdout/stderr、supervision、stages、Results、角色各自TestResults/业务JSON、像素/事件/资源、artifacts/bw等非Results输出、ReferenceData及包身份清单；只回收监督root可能漏掉以AppContext基目录写出的业务证据。

监督record137–161需逐项亲核：Started/EndedUtc、SupervisorPid/Pid、Executable/Arguments、TimeoutSeconds、Runtime/Architecture、TestAssembly/SHA/MVID、Reason、TimedOut/Cancelled、KillAttempted/Returned、HasExited、ExitCode/ExitHex、OutputCollected/OutputDrainTimedOut、SupervisorError。Reason=exited并不等于通过；Exit2可以是完整失败full，0xC0000374是进程异常而非测试计数。Save在启动前及finally写入；仅有初始文件不证明结束。参数验证在Save之前失败可能没有监督JSON。父TRX必须读结果状态、全部Counters、RunInfo并核发现范围；子角色1/1不是父full。缺父TRX只能记中断/未知，不能借继承smokeTRX报full3/3。

## 5. 默认发现与实际身份

实读文件开头：`T/CqFillExactMatrixTests.cs:1–18` 为 CQ_FILL_EXACT_DIAGNOSTICS，`CrFillExactIntegrationTests.cs:1–25` 为 CS_FILL_ACCEPTANCE，`CbNestedFillBaselineTests.cs:1–14` 为 CB_ARCHIVED_FILL_DIAGNOSTICS，`CdCurrentNestedBaselineTests.cs:1–14` 为 CD_CURRENT_FILL_DIAGNOSTICS。四者从第1行隔离，默认未定义不发现；不能因此把CY小组门说成默认full覆盖。Cr显式模式仍有Old/Native基线查找逻辑，不应靠历史远程task补默认依赖。文本搜索另见 CO/BX/CG/CC 探索条件，不展开无关历史调查。

`T/BwCurvedFillPrecisionTests.cs:1–105`实读：12–17无编译隔离的TestClass/TestMethod，32–55独立official/candidate/repeat零阈值及非空断言，65–99子角色监督与实际加载模块身份。`T/CkFillIntegerTests.cs:1–120`全文：默认7个像素数据行+3个round数据行；61–96使用链接SoftwareFillBezier与官方WPF比较，证据自己明确“no production entry hookup”。**CK10可发现但不是候选DLL消费证据；BW必须核candidate实际模块才是链路证据。** csproj21链接生产文件不等于调用已发布候选。

AcceptancePaths19–20：候选环境变量优先，否则元数据Native/wpfgfx_cor3.dll；官方环境变量优先，否则PresentationCore同目录wpfgfx_cor3.dll。51–69校验候选本地PE架构、无CorHeader、SHA与aot.sha256。AssemblyInitialize78–89写顶层Results/aot-identity，记录文件与命令行，但子角色可覆盖它（BD60）；不能单凭它证明父参数或实际加载。

`T/PublishedArtifactTests.cs:1–20`只哈希文件/检查长度，不证明加载。`T/D3DImageTests.HardwareClosure.cs:1–64`实际返回截断于65：LogHardwareModuleIdentity23–42以已加载句柄GetModuleFileName并断言Requested=Actual、记录实际SHA/版本/测试SHA。BW84–98也以解析器加载及进程模块地址核actual。当前默认full应亲核这些与真实业务输出绑定，而不是只拿包清单、ProductVersion提交字符串或候选路径作证。

CY原件本轮只抽核两份 `artifacts/cx-fill-restoration/{run-x64-02,run-x86}/artifacts/bw-curved-fill/precision/candidate/identity.json:1–14`全文：requested=actual，分别F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7与7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA，测试SHA/MVID与CY23一致，.NET10.0.13、X64/8及X86/4、software、hardwareVerified=false。没有重算DLL/像素SHA，没有再审CY全部comparison或官方原件，不把这一抽核改称full当前身份；CZ必须独立绑定其实际测试包/候选SHA。

## 6. ReferenceData及官方版本依赖

csproj27–34规定sandbox参考为包内ReferenceData，校准输出Results，候选Native；非sandbox为绝对构建元数据路径。`T/ComAcceptance.Package.targets:1–42`全文（首段截断于37以36–42补齐）：普通包只取ReferenceData/*.json，self-contained包取publish树并排除Results/TestResults/CodeCoverage/PDB；生成aot.sha256。12/31 RemoveDir会重建包目录，两个架构必须独有root，DA不触碰它。目录名、包存在不证明参考完整。

`T/ReferenceRevalidationEvidenceTests.cs:1–68`全文：23–28读取包内glyph-arial-original.json、original-wpf.json，43–56记录候选/官方/PresentationCore/参考SHA版本，62–66断言两个参考来源SHA均等于当前官方文件SHA。必须核这些真实输入，不能写tracked ReferenceData或用候选生成oracle。BD27先生成官方输入再用于full是历史方法，不是本轮自动存在的事实。`acceptance.runsettings:1–25`含HW诊断、blame与TRX配置，但不据配置推断实际收到了dump。

BD56、BN78报告官方10.0.13-servicing.26504.104、commit4099f839，属历史身份；当前需以CZ官方actual及PresentationCore版本/SHA重新绑定。无固定runtime合同，不把单servicing验证推广全部版本，也不把版本目录当实际身份。

## 7. full完成/中断后优先亲核清单

以下旧分类取BD64–87、BN5–9/53–86、CL5/19–27/29–69/79–87，**不是DA重新核过旧原件，也不是CZ当前结果**。

|优先级/类别|当前产物应亲核|禁止的越界结论|
|---|---|---|
|P0 完成性与身份|父参数是否真正无过滤；父TRX状态/计数/退出；监督全字段；测试SHA/MVID、候选actual、官方actual与参考来源；缺项逐项标未知|发现数固定1740、旧1699通过可移绑新SHA、小组门拼full|
|P0 异常/中断|最后同步stages、首个失败HRESULT、stdout/stderr/诊断及已有dump；区分正常非零、timeout、cancel、kill、排空失败及进程异常|无TRX即工具阻塞、控制台末项即唯一根因、官方角色中断即候选崩溃|
|P1 官方sampler/清理|BN曾首次Commit=0x80070006后finally Destroy未返回，最终双架构0xC0000374；核当前是否同形态、同步channel/Event0、事件所有权及实际模块|Destroy末阶段即首次写坏位置；E_HANDLE消失即堆损坏根因修复|
|P1 旧37项HW/呈现/恢复|Stage4 15、historical 7、PublicHwnd及shader/channel 11、desktop/resize/loss 4；逐项读前置、adapter/backend、官方成功对照、实际新色/新帧、mutation/resize/retry/loss到达性|全部归环境、软件fallback当HW、第一设备未loss却用第二创建失败证明loss；Basic Render Driver外推物理GPU|
|P1 内容2项|rounded旧帧1/3/5各44差、curve旧frame5=44/frame9=10；当前原始buffer/尺寸/变换/repeat与零阈值|因CY206有限fill通过清零所有nested/stroke/rounded/full差异|
|P1 合法大shader1项|旧WhenShaderResourceCrossesOneMiBThenOriginalAndCandidateAgree，长度2939/索引320差；当前合法输入、实际报告与消费链|称缺参考或错误输入而排除有效内容差异|
|P2 WindowQ夹具1项|旧40x24帧被固定24*16*4拒绝；当前逐帧真实尺寸、完整buffer、既有exact/红蓝断言及失败位置|直接归生产、不读当前便称已修；通过skip/放宽像素阈值解决|
|P2 通过项抽核与未覆盖|真实producer/consumer、HW更新、loss→恢复→新帧、持续media、资源/线程/callback关闭和收敛；短循环看完整趋势/事件|TRX未执行0即所有关键路径覆盖；短16cycles通过即长期资源已证|

CL当前仓库同线程失败路径的源级闭环是：失败batch已delete，Commit错误跳转保留closed槽，Destroy的SyncFlush可再次Commit解引用；再次delete仅为有条件后续。CL同时声明未取得对应servicing精确源码/编译信息、首次free地址/栈及第二次访问/free证据。**因此不能将仓库源码UAF冠为实际10.0.13-servicing已证根因，不能照此无证修改候选。** 候选源级非同构也不是全部失败释放安全或旧DLL安全证明。

## 8. 交付边界

未获得CZ当前完整执行记录、当前实际构建符号/发现清单、parentTRX、监督退出与业务输出，故本文不报新运行数字，不判双架构full完成/失败原因。现有工具能力可接受有界正式应用执行，不能先称不可用。报告建议供CZ后续证据收口，不要求重跑已完成CY、不替CZ改报告或共享交接。当前修复应保留；最终结论仍须稳定源码四格Tests/ComAcceptance、双架构Release的全部适用关键证据，不能按通过率关门。
