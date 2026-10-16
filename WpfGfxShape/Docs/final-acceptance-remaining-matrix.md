# 最终验收剩余矩阵

## 最新范围裁决（优先于本页全部旧矩阵）

[DEC-0005](decisions/DEC-0005-x86-x64-acceptance-scope.md)已生效：正式仅 **x86/x64**；ARM64编译/布局/运行/工具链/实机均为 **NotApplicable-UserScopeDecision**，不是Passed，不计阻塞或外部必需项。下方原ARM64缺口仅历史。精确10.0.12为历史路径绑定，未证用户指定，当前patch口径待澄清，详见[单一版本审计](windowsdesktop-version-provenance.md)；不据此索取永久外部包，不抹除10.0.1原版Green或历史失败。

| 当前范围 | 裁决 |
|---|---|
| x86 / x64全部正式配置 | Debug/Release、布局/ABI/调用约定、资源/失败/线程/callback/关闭、七阶段/A/B/C、E2E-00–09及最终同身份strict/full全保留，有限通过不外推 |
| C5 / CLOSURE | 原bilinearspan项目来源及完整实际physical-record闭包未满，Blocked；不再含ARM64阻塞 |
| REGEN-DIFF / D4 | 历史binding未证，Blocked；既有有限Satisfied不变 |
| 两required / parent | Blocked，stage4Closed=false，总门Red；Ledger原ARM64字段仅由决议显式NA覆盖，不改Satisfied |

下方机器数字/NA=0均为原审计风险表统计，不是本次架构适用性裁决。

## 最新正式矩阵：覆盖以下旧轮状态

| 范围 | 最终实测/证据 | 裁决 |
|---|---|---|
| 当前C5源码 | Release build 0 warning/error；X64 audit run；17,377报告行完整回读逐行比较 | 已执行，无未测尾改；不是46/COM重跑 |
| 正式入口/风险 | 6 physical+3 evidence；333=13已证build声明+92 build未知+227 include未知+1缺项目，NA=0 | 319未知保留风险原表；原group readiness/doneRule不要求保守333穷尽，不设额外门 |
| CLOSURE / C5 | 当前P/R/F追溯、20 evaluation、12配置16 TU及imports已证 | 有限子项保持；bilinearspan原项目及完整实际物理记录闭包未足，ARM64未证，Blocked |
| REGEN-DIFF | D1–D3当前378/275/378及身份保持 | D4历史binding缺，Blocked |
| CONSUMER-DECLARATIONS | 原声明/签名/既有断言 | Satisfied保持，不等于所有命令执行 |
| 下一动作 | 无已证必需生产方法修改 | 请求bilinearspan原项目/依赖或权威生成合同后有界复核；不继续候选扫描 |
| 外部与总门 | authorityhistorybinding、ARM64toolchain/realenv、完整10.0.12、官方player协议及HW可观测权限缺 | 父级Blocked、stage4Closed=false、总门Red；原七阶段/ABI/寿命/E2E不减 |

唯一正式依据/逐ID充分性与gap：[C5详档及唯一表](completed-work/GenProto-C5-final-adjudication.md)、[Ledger](../Ledger/mediaown-required-finite-acceptance.json)。生产3F291未改，未生产publish/COMfull、未重跑46，DLL不卸载；风险未删除、未假清零。

## 以下旧矩阵为历史，旧105/227及穷尽下一动作不再有效

> 最新状态覆盖以下旧C5矩阵：[唯一机器总裁决](completed-work/GenProto-C5-final-adjudication.md)。9正式入口；17,377原物审计记录已落盘回读（不是正式边数）；105 build条件、227 include指令适用性未证及1原bilinearspan项目缺失逐行有限列明。未能证明最小精确适用边全集，自主全审计未完成，不将332候选强写external或生产修改。独立audit源码最后build及沙箱运行成功，不是testfull。C5/两个required/父级Blocked、stage4Closed=false；生产/COM/strict不变。

## 当前 C5 原物有界映射已证，完整原条件未闭

依据：[activeincludes最终清单](completed-work/GenProto-C5-activeincludes.md)、[唯一基线](handoffs/current-stable-baseline.md)、[既有Ledger](../Ledger/mediaown-required-finite-acceptance.json)。本次只读对账与文档同步；此前源码GenProto46/46与既有CL运行分开，无新增源码/诊断项目/全量重跑或机器总裁决报告。

| 原条款/子范围 | 当前实际证据 | 裁决与真实余项 |
|---|---|---|
| C1–C4/D1–D3 | 当前输入、生成器、生成映射、消费者及隔离再生/差分身份 | 原有限Satisfied保持；不外推D4或所有命令执行 |
| C5固定入口/evaluation | 6 physical+3 evidence；18 native+2 generator的既有evaluation | 子项Satisfied；888候选/333审计行及全WPF图不扩分母 |
| C5所选active TU | 原12配置16必要TU CL预处理exit0，v143/14.44.35207/SDK26100；本次检索16份.i并读selected/result/hash | 所选active子范围已证，不再记未采集；不是完整生产编译/链接 |
| C5既有record/assertion映射 | core/render全部16份到达；resources/uce的factory/resources/data及factory正文映射既有P/R/F | 详档所列有限边已裁决；未到达不能Excluded，未证明全部333原边已裁决 |
| C5仍可自主 | 选定project/import/属性/include边的适用条件→已有record/assertion逐项审计 | 审计未完成，不一律blockedexternal；复用现有原物，不重采TU/全量重跑 |
| C5确证缺物 | wpfgfx四配置原bilinearspan引用GetTargetPath实际MSB3202 | 阻断完整物理引用记录；需原项目及依赖或权威生成合同。不是header失败，不拼项目/删引用 |
| C5 ARM64 | 6配置缺toolchain既有实证 | 工具链与真实环境外部缺；不能由x86/x64外推 |
| D4及其他外部 | 历史authority、合法full10.0.12、官方player协议及DWM/HW可见control前提 | 按唯一补件页验收；当前生成与active头不证明历史binding |
| MediaOwn/transport | 既有引用平衡/API功能有限证据 | 两packet/独立discard未证不增原生成required门，不批准完整等价 |
| 总门/生产 | 原七阶段、ABI/寿命/E2E保持；生产未改、无publish/COMfull，DLL驻留unload=N/A | C5、两个required及父级Blocked，stage4Closed=false，总门Red；不能报告整体完成或自主余项零 |

四短入口、总计划、矩阵、唯一外部页与独立清单同步；具体缺物交付只维护于[外部页](all-completion-external-prerequisites.md)，身份只维护于基线。下列旧节仅历史。

## 历史 native-only packet 有限审计完成，父级Blocked

最新依据为[native-only独立详档](completed-work/GenProto-nativeonly-packet-execution-finite.md)、机器report/verification及独立Ledger；身份和短数字集中见[唯一基线](handoffs/current-stable-baseline.md)。本轮真实修测试路径、build target/full GenProto，原始四Results全部机器解析、raw hash及同任务identity/TRX/RunID绑定；既有COM full仅读核，没有重publish/COM full。

| 条款 | 当前实证 | 具体未证 / 保留边界 |
|---|---|---|
| bitmap / create / delete / duplicate | 候选真实writer→consumer、像素与WIC owning生命周期断言绑定 | 不是原版原生packet trace，不提升完整ABI |
| media | 公开SendCommandMedia真实解码颜色、caller release、替换与失败重试 | failure/discard最后media引用计数未独立测量 |
| transport flush / destroy | API barrier/close/cleanup功能成立 | 候选直接barrier/State.Destroy不是transport packet执行；两项packet仍未证 |
| VisualCreate | 真实node.cpp ProcessCreate RIP/E_UNEXPECTED | 分类已闭，不新增legacy producer，不以ordinary create顶替 |
| 有限Ledger | childSatisfied=true，报告与源码/assembly/full身份已绑定 | parentBlocked=true、stage4Closed=false |
| 正式required | consumer declarations及既有有限子项保持Satisfied | EVID-GENPROTO-MILCMD-CLOSURE、EVID-GENPROTO-MILCMD-REGEN-DIFF按ID保留，具体项见工作项 |
| 外部与总门 | 已闭x86/enum/padding结构不重领，生产不变、DLL无unload | authority binding、ARM64真实环境、完整合法runtime、player协议及HW会话仍缺；七阶段/A/B/C/E2E全目标持续，总门Red |

[工作项](handoffs/current-work-item.md)仅保留正式required有限差集，不再笼统领取native-only全部执行。media与transport未证不假解除，不开无界故障门。本轮Docs整节同步完成，无未编译源码或待读测试报告。

以下旧轮状态全部为历史，以本节、独立详档及唯一基线覆盖。

## 历史 x86 CLR wire有限执行已闭，父级Blocked

最新依据为[x86 CLR独立详档](completed-work/GenProto-x86-clr-wire-finite.md)；执行身份、回归及全部短数字集中见[唯一基线](handoffs/current-stable-baseline.md)。本次仅读取既有证据并同步Docs，不代码、不改Ledger、不构建、发布、测试或终端脚本；此前执行不冒充本次新跑。

| 条款 | 当前有限结论 | 保留边界 |
|---|---|---|
| x86 CLR布局 | 真实win-x86 CLR执行，独立Ledger Satisfied，不再是推导或未执行 | 仅SizeOf/Pack/OffsetOf及enum观测；不证明传输、生产执行或完整WindowsDesktop runtime |
| x86 Shared | 先前独立发布/加载/有限资源生命周期合同保持原裁决 | 与本次CLR wire布局分开，不外推完整ABI/E2E或ARM64 |
| enum差集 | 独立compiler映射保持Satisfied | DBG声明含diagnostic项，非全部retail执行，不重新领取 |
| padding | 结构padding/offset洞/尾size marker映射Satisfied | managedNamedOffset仍null；native padding不是managed同名语义字段，不证明padding内容/传输等价 |
| VisualCreate | native RIP/E_UNEXPECTED、范围内无typed producer的分类审阅Closed | 不支持legacy packet不等于已执行；ordinary create不可替代，不新增producer |
| native-only | producer/consumer来源及版本精确历史typed case绑定保持有效 | packet-specific合法执行/传输/失败关闭仍逐项未证；delete/destroy/flush缺runtime binding；不造managed对应或原版packet trace |
| required/physical与ARM64 | x86有限子项完成不提升父级 | 两个MILCMD required及父级Blocked、stage4Closed=false；逐ID剩余明细继续，ARM64 compiler与真实运行仍缺，不能由x86/x64外推 |
| 生产/正式总门 | 生产未改，无新publish/COM full；DLL驻留无unload | 四外部具体物及正常可观测HW会话前提仍缺；同SHA历史COM/strict未重跑，七阶段、七项正式条件、全required/physical、A/B/C与独立E2E不减，总门Red |

下一自主项只领取[当前工作项](handoffs/current-work-item.md)中的既有native-only packet-specific合法执行与required/physical剩余明细，不再领取x86 CLR或已闭映射，不扩无界能力。外部缺项仅由[唯一页](all-completion-external-prerequisites.md)维护。本次文档整节同步完成，用户全目标尚未完。

以下旧轮节原文冻结，其中未执行、下一模块及当轮最新等描述仅属于历史；当前状态以本节、最新独立详档与唯一基线覆盖。

## 历史 wirecompiler有限验收

[有限详档](completed-work/GenProto-wirecompiler-finite-acceptance.md)、交付报告关键段与最新Ledger已实际读回；最新既有回归、TRX/RunID/source/assembly/报告身份及测量分母、独立生产历史基线只见[唯一状态源](handoffs/current-stable-baseline.md)。本次仅Docs，无代码、Ledger修改、构建、发布、测试或终端脚本。

| 条款 | 已证有限范围 | 真实剩余 / 验收边界 |
|---|---|---|
| compiler / managed布局 | Win32/x64真实compiler观测及managed字段模型对照完成，独立观测/差异审阅Satisfied | 不能由布局观测推出wire字节、消费者或生产执行等价；不再列完整组未做 |
| partial模型修正 | 测试侧优先Generated字段声明，旧错误观测保全；生产decl、expected、required未改 | 不是生产布局修复；不能覆盖旧报告或凑size |
| padding字段映射 | VisualSetGuidelineCollection、GlyphRunCreate显式padding缺名字段及native offset保留 | managed同名field为null，差异仍Open；相同size不证明padding内容或传输等价 |
| enum authority | 已找到独立native键的成员原值比较，逐项enumAudit保全 | 未证native键/alias具体Open；不以命名猜测或managed自生期望填空 |
| native-only来源与分层 | XML显式UnmanagedOnly、wire与consumer来源分层；MediaPlayer显式Update及真实发送/分派源码路径已证 | native-only生产执行未证；VisualCreate直接producer未找到，RIP/E_UNEXPECTED保留，不能用ChannelCreateResource替代 |
| 下一唯一自主模块 | [剩余enum keys / field mapping / VisualCreate producer精确分母闭包](handoffs/current-work-item.md) | 冻结逐ID去重分母及独立authority映射，不重领已测观测，不拆成重复测量或单补测 |
| 架构 | Win32/x64 native compiler与X64 CLR固定宽度观测身份分开 | x86 CLR未执行；ARM64 compiler/runtime具体Open，不以推导或交叉编译冒充执行 |
| generation / build authority | 历史固定边及production conservative分类独立，已有manifest不重索 | 逐TU条件/SDK/VC及历史authority binding未全证；未到达不等于排除，保守全WPF图非正式分母 |
| Ledger / required | 最新Ledger仅独立compiler观测与差异审阅Satisfied，既有consumer/factory有限Satisfied不降级 | 两个MILCMD required及父级Blocked，stage4Closed=false；完整physical/生产执行与失败清理断言仍Open |
| 正式总门 | 生产未改，同SHA历史COM full/strict非本次新跑；DLL驻留无unload | 四外部authority binding、ARM64、完整runtime、官方player protocol/oracle及HW可观测成功前提仍实际缺；七阶段、七项正式条件、全required/physical、A/B/C及独立E2E不减，总门Red |

外部前提只见[唯一页](all-completion-external-prerequisites.md)，不把全部失败归环境。本次Docs已收口，无新测试待办，不宣称父级全闭或替换资格。

以下逐轮节均为历史，不覆盖本节。

## 历史 GenProto factory physical 有限验收

[独立详档](completed-work/GenProto-factory-physical-finite-closure.md)与[基线](handoffs/current-stable-baseline.md)记录最新源码build、Factory1/1、全GenProto35/35及完成后归档1/1；原始TRX/RunID/hash、源码hash、报告和Ledger已读，无回归后源码尾改。

| 门 | 实证有限范围 | 保留条件 |
|---|---|---|
| Factory独立子组 | 97 IDs/76 cases/21 rejects，GlyphRun例外；source→generation→producer/wire→consumer物理声明→既有真实AOT结果；errors=[]、Satisfied | 非新生产能力，非全physical/历史generation/native layout；parentRequiredSatisfied=false |
| C/阶段4 | Ledger有限record已绑定report/run/hash | C及历史binding/ARM64仍Blocked；stage4Closed=false |
| 下一自主正式门 | GENPROTO-MILCMD现有ID去重入口为6 physical records + 3 required evidence共9项，随后冻结递归文件清单 | 完整实现超出factory的generated/mapping/producer/wire/consumer逐记录闭包与实际证据；不是hash存在即闭合，不编造未枚举数量，不把所有工作归外部 |
| 生产基线 | 3F291D3E不变；既有full1616/1583/33/0、strict Missing2/Extra0 | 本轮未改生产/重publish/COM full；33失败不全归环境，player不stub |
| 已闭范围/总门 | 多Dispatcher有限Closed不重领；GenProto35为验证数量 | 四类外部真缺项及真实可观测HW会话前提见唯一输入页；全required/physical、完整ABI/HW/七阶段/E2E仍Open |

[外部唯一页](all-completion-external-prerequisites.md)不新增重复清单。DLL驻留不卸载，用户总计划继续，正式总门Red。以下逐轮节为历史，不覆盖本节。

## 历史多 Dispatcher E2E-09 有限验收

[最新完整报告](completed-work/PresentationCore-multidispatcher-E2E09-finite-acceptance.md)与[状态源](handoffs/current-stable-baseline.md)绑定当前3F291D3E生产；旧57CE仅历史。无新增代码/扩范围/最终验证后源码尾改。

| 门 | 本轮真实覆盖 | 正式剩余 |
|---|---|---|
| E2E-09有限场景 | original/fixed-candidate各两轮3STA、joined6；36帧各4096字节threshold0全字节相等，有限Closed | 不泛化任意concurrency、callback静止、异常/OOM、长时/全架构；完整E2E-09仍Open |
| shared lifecycle / affinity | async connection与root/service-channel有限共享关闭正确，普通channel affinity仍守 | 非全面取消线程约束，不代表全部线程合同 |
| 6B full身份 | 同SHA RunID/identity/Counters核对，真实1616/1583/33/0 | 33HW/control失败保留，正常original/native可观测会话成功前提仍缺，不追绿、不全归环境 |
| 同SHA strict | 最新PublicAbi已运行且报告已读；Missing2/Extra0，PEExportContractMismatch，有限life通过 | PeContractPassed=false、AllAbiPassed=false；MilPlayer_Create/Process缺官方协议不stub，不借旧报告 |
| 普通 / GenProto | 新源码各一次4224pass/1fail/0skip与34/34；GenProto原始TRX/RunID已存 | 普通10.0.12 DLL或依赖8007007E；工具未提供TRX/RunID如实记载，不伪造、不重复跑补ID |
| 正式总门 | 有限模块Closed，父级可继续其它自主门 | 全required/physical、完整ABI/HW/cache/shader、七阶段及E2E仍Open |

外部authority历史binding、ARM64 toolchain+realenv、完整10.0.12、官方player protocol/oracle按[唯一输入页](all-completion-external-prerequisites.md)保持Open。DLL进程驻留不卸载，用户总计划持续，正式总门 **Red**。本轮无strict未跑/报告待读/文档待办；以下逐轮节仅历史，不覆盖当前身份。

## 历史 async drain / owning 有限验收

[最新独立完整报告](completed-work/async-drain-owning-finite-acceptance.md)收口已有AsyncDrain fixture；生产未改未重发、候选12/12、原版正常6/6。最终fixture full本轮真实一次 **1616 total / 1583 pass / 33 fail / 0 skip**，同任务Results完整回收，identity与实际DLL SHA核对；8TRX/180JSON完整解析，summary及36条drain记录已读，Added/Removed失败集合均空。不再使用旧1594分母或“1598未跑full”作为当前缺口。

| 受影响门 | 逐项有限Closed | 正式剩余 / 非必需增强边界 |
|---|---|---|
| 5B / transport owning | 真实WIC/外部stream，open/closed/committed；disconnect后显式flush或直接destroy drain，最终处置一次；原版正常六协议独立通过 | nativeDisconnect仅释放connection，不是独立cancel；撤去本模块错误cancel必需待办，不虚构cancel完成 |
| 5B / callback与worker | 候选12项sibling close callback返回0；源码规定case的非调用线程释放和deinitialize后OS worker退出 | 真实worker异常未证，不能泛化所有失败线程；具名正式线程/callback静止合同仍必需 |
| 5C / sticky失败 | 候选合法完整空source触发0x88980406 sticky首错，六状态最终释放一次 | 失败协议原版未对照；OOM/QI/中途替换失败未证，属增强风险，不无依据增加无界门；正式可达失败逆序清理仍保留 |
| 5B / typed generation | 原typed154/154、owning/generation15/15有限Closed不重复领取 | 不泛化全部方法族，不设COM非规范强refcount等值门 |
| 6B / 回归身份 | 最终full1616/1583/33/0已取得、失败集合不变，同SHA未重publish | 同SHA strict、完整ABI及33HW/control失败仍Open，不重追Green、不全部归环境 |
| C / 正式总门 | GenProto34/34、普通4224/1/4225为前轮实测复用，非本轮新跑 | 全required/physical、四外部真正缺项、完整架构/配置/HW/持续媒体、七阶段与E2E仍Open |

精确路径/RunID/SHA见[状态源](handoffs/current-stable-baseline.md)及独立报告。正常原版可观测HW会话成功前置仍缺，未证明新增必需用户硬件包；[唯一外部输入页](all-completion-external-prerequisites.md)的历史binding/ARM64真实执行/full10.0.12/player合同最小缺项不变，已有生成器/manifest不重索，无新材料不解除。DLL驻留不unload。本有限模块Closed，用户总任务持续，总门 **Red**；下文逐轮节仅历史，不覆盖本节、不重新派已Closed范围。

## 历史 ProcessCreate / GlyphRun 有限验收

[最新详档](completed-work/ProcessCreate-GlyphRun-finite-acceptance.md)已落盘；本次无新代码、运行、构建、发布、测试或脚本，不需要再测。当前 SHA、独立分母与最终 RunID 见[唯一状态源](handoffs/current-stable-baseline.md)。真实 factory 76 合法 / 21 拒绝、ordinary GlyphRun 按 native 忽略普通 create / 保留显式创建有限 Closed，3 Red → related 183/183；最终 full 1586 total / 1553 pass / 33 fail / 0 skip 已核，同前轮 AddedFailures / RemovedFailures 均为空，33 failure 仍未解，不能全归环境。

同 SHA primitive4096 byte exact / Blur14 zero / image10 zero / media4 frame8192 byte exact 四组独立 compare 已读；普通4224/1/4225仍为 runtime10.0.12 DLL 或依赖8007007E；GenProto34为本轮实际日志，非新TRX。[Ledger](../Ledger/processcreate-factory-evidence-20261015.md)追加有限 source → GeneratedValueResources / GeneratedProtocol dispatch → consumer → 真实报告，不改原始 record，不标全 requiredSatisfied 或 stage4Closed。

| 受影响门 | 本次有限覆盖 | 正式剩余 |
|---|---|---|
| C / 阶段4 / 5A | 76 case / 21 拒绝、ordinary GlyphRun 特例及有限签名/物理映射 | 全 required/physical、独立布局/authority、完整 producer/consumer；不能以 GenProto34 代满足 |
| 5B / 5C / ProcessCreate | factory/dispatch 有限错误与接纳释放；最终 related/full 已取得 | typed / partial-batch 首错与失败逆序清理、引用世代及关闭 owns 完整拥有链 |
| 阶段3 / 7A / 有限媒体 | 上述同 SHA 四组独立 compare，原有限 software effect 不重领 | 全格式/输入/生命周期、真实 HW/持续媒体、正式目标与架构矩阵 |
| 6B / 最终门 | 最终 full 身份绑定、失败集合审计 | 同 SHA strict 仍缺；33 failure 未解，完整 ABI/七阶段/E2E 条件不减 |

唯一下一自主生产模块为 **ProcessCreate typed / partial-batch 失败清理、引用世代与关闭 owns**，不是 factory 重领、software effect 或文档 WIP。四外部条件仅见[唯一页](all-completion-external-prerequisites.md)，DLL 驻留不卸载；正式总门 **Red**。
以下逐轮执行更新为历史绑定，不能覆盖上述当前覆盖/剩余；后续正式矩阵的完整门槛保留。

## 历史媒体E2E-08执行更新

[完整方法族报告](completed-work/PresentationCore-media-E2E08-finite-closeout.md)：最新构建及original/candidate/compare独立进程均已执行、JSON/pixels/module identities已核读。两PCM AVI四帧8192字节零阈值全字节相等；Opened/Ended/Failed、Position/HasAudio/尺寸、引用替换/并行callback Close有限Closed。系统媒体服务与候选UCE/VideoDrawing/RTB双身份合法；自有worker/callback清理不外推全部系统线程退出。最新full一次1446/33/1479、0skip，精确RunID与同SHA identity仅见状态源；未改生产、不重复publish，普通/GenProto沿用真实历史。

正式E2E-08 HW媒体消费者/长时持续/全架构配置及完整E2E-09仍Open；PRERELEASE record player官方协议与两缺export不是MediaPlayer缺口。四外部输入、七阶段及总门Red不变。下文FloatHDR执行更新为上一轮历史，不代表当前轮次。

## 上轮FloatHDR执行更新

最终源码已build/publish并完成full一次1446/33/1479、related1017/1017，Results内TRX及同9FDF…identity已回读，精确RunID见[状态源](handoffs/current-stable-baseline.md)。[FloatHDR详档](completed-work/FloatHDR-public-path-finite-closeout.md)给出公开Prgba128Float Clear/Draw真实raw、版本与HRESULT。180组合消费者独立线性关系、候选float Draw保真、Clear锁失败恢复与释放后存活有限Closed；原版公开Draw本身限幅/量化，source17/18同格式HDR golden继续Open，完整refs/核内fault/全格式shader合同不豁免。33失败包括原版HwndSource背景control，不是float回归，旧32不撤销。GenProto最新34/34原RunID778433c4-c65a-4025-b026-e896facfb993；普通4224/1/4225保留8007007E，未变项目本轮未重跑。

四外部authority binding/ARM64/full10.0.12/player protocol继续缺，正式总门Red。下文旧SHA/分母仅历史证据，不作为当前实测。

## 历史执行更新

本轮实际修改并发布唯一COM验收fixture，HW专用STA GetMessage持续pump及WM_PAINT不覆盖已落地。最新该fixture42/9pass/33fail/0skip，RunID 9449dc8e-0699-45b1-859e-3fa76e18b89c，identity同生产3F4B424…；旧58/26/32独立保留。原生backbuffer正确，display frontbuffer与公开原版target均背景，故A/B仍Open，需可观测交互桌面会话建立成功前提，不凭duplication超时判不支持。软件格式29/29不代FLOAT/HDR原版oracle。GenProto最新34/34，RunID a2a273cb-9a8f-4027-afec-f0bcc7d9c6ce；Pen有限模型不关闭C/5A。见[HW独立报告](completed-work/HW-dedicated-STA-presentation-current.md)、[Pen档](completed-work/GenProto-pen-producer-consumer-finite.md)。下文旧COM全量和旧GenProto28仅对应原历史运行，不是最新独立模块计数；正式条件不变。

本页是[总计划](remaining-gap-closure-plan.md)的门禁审计视图，不是新恢复点、运行状态源或生产交付。审计仅 Docs/只读代码与证据，无构建、测试、发布或环境变更；代码由并行开发者独占。本页只枚举既定生产 Ledger/原生映射与正式合同，不把所有未实现 WPF 功能无限纳入，也不缩减门槛。当前身份/分母只见[基线](handoffs/current-stable-baseline.md)；下列覆盖绑定所引原物，不自动绑定后续 SHA。

当前DropShadow更新：有效Red E_NOTIMPL→final compare06固定28帧零差，有限合同Closed；最终同SHA Blur14/nested16/image10/primitive双角色四比较零差。五JSON已完整回读，source/frozen/actual及COM related/full identity与TRX已核；精确分母/身份只见[状态源](handoffs/current-stable-baseline.md)，原物、RunID/Counters、算法与images见[完成档](completed-work/PresentationCore-dropshadow-acceptance.md)。

最终同SHA related985/985，full一次1427pass/32fail/1459；32真实失败保留control premise与未进入cache验证边界，不全归环境。普通4224/1/4225与GenProto28/28只按lifetime-managed-06真实原TRX列示，不冒充最终AOT新跑；最终strict报告缺证据不填历史。生命周期36帧与目标锁12项原合同Closed保留；失败calls=0，候选描画/核内fault与完整refs/close仍Open；固定28不外推全输入/并发/OOM，不自造核内OOM门。父级继续委派A/B HW image/visual/cache真实消费者及失败恢复拥有链完整模块，不重领已闭softwareeffects。

**结论：当前输入与证据下总计划不能完成，正式总门 Red，无兼容替换资格。** 自主生产工作仍可继续；模块 Closed 不等于全任务结束。四类外部条件中已有材料不再重索取，真正缺物及最小获取途径仅见[唯一外部页](all-completion-external-prerequisites.md#本次只读复核与最小获取途径)。

## 七阶段、子门与 A/B/C

“自主”表示开发者可继续实现/运行/补证，不授权本审计者生产操作；“混合”表示只将具体依赖交外部，不能停掉整阶段。实=实现，运=运行，资=资料/身份/记录。5A/B/C、6A/B/C、7A/B/C为既有正式子门，不新增门ID。

| 正式门 ID | 明确验收条件（完整条件见权威链接） | 已证覆盖（不重领） | 真正剩余：实 / 运 / 资 | 依赖 | 自主/需外部 |
|---|---|---|---|---|---|
| 阶段1 | [七阶段](remaining-gap-closure-plan.md#七阶段正式完成条款保留不以导航或有限green代替)：设备/manager/资源拥有寿命、失败恢复、线程及规定原生差集 | 既有限直接设备/生命周期；[HW r31](handoffs/completed-work/HW-r31-observable-session-gate.md)仅历史可观测会话 | 实：尚缺消费者及失败恢复；运：成功/设备失败/恢复/关闭因果与跨会话稳定性；资：逐原生可达差集 | 阶段2/5消费者；合法可观测会话 | 自主；未证明需新增用户硬件包 |
| 阶段2 | 规定内容/格式/采样/shader/cache成功失败消费者全链 | WIC及cache有限条款、固定ImageBrush十场景不撤销 | 实：完整generated内容接线与B政策；运：真实HW像素/失败重试/引用寿命；资：内容族差集 | A/B、阶段1/4/5 | 自主 |
| 阶段3 | 规定SW像素/变换/裁剪/格式/寿命及PresentationCore消费者 | primitive/miter历史、历史image/nested/Blur逐项有限Closed；DropShadow固定28及最终四回归有限Closed，详见[独立档](completed-work/PresentationCore-dropshadow-acceptance.md)；geom12/tile192及media8按原档 | 实：完整格式/消费者/动画/callback；运：source17/18同版同格式原版HDR oracle、完整失败/refs关闭（有限180组合与公开Clear/Draw已Closed）；资：正式范围映射 | A、阶段4/5、7A | 混合；目标runtime见外部页 |
| 阶段4 | producer/consumer、独立field/enum/pack/size、physical/required完整闭包 | [GenProto分层](handoffs/completed-work/GenProto-finite-closure-audit.md)声明/布局/签名有限证据 | 实/资：逐physical record及required依赖与签名、完整消费者；运：独立布局/真实目标执行，satisfied与stage4Closed正式满足 | C、5A；历史来源及ARM64 | 混合 |
| A | [主题A](plan-module-contracts.md#a-frozengraphrealize)：整树两阶段冻结、pure owning capture、callback、ownlease、后验key/version、首错/逆序释放、HW+SW旧新帧 | 已有devicekey、失败保旧及WIC本机五callback有限验证 | 实：Image/Visual/Cache完整可达消费与拥有链；运：CopyPixels/重绘/失效计数、候选失败旧键恢复重试、真实HW+SW寿命 | 阶段1/2/4/5、可观测呈现 | 自主；不重做已证有限条款 |
| B | [主题B](plan-module-contracts.md#b-cacheshader-失败)：generation/per-device、通知/fallback/OOM、sampler/constants、malformed/type、padding/Ddx/inputscope/HDR及compiler合同 | 已有底层及有限pass-through不撤销 | 实：政策真实消费与完整shader/cache消费者；运：真实成功/失败寿命及规定有限表；资：原生政策逐项对应 | 阶段1/2、A；独立呈现前提 | 自主 |
| C | [主题C](plan-module-contracts.md#c-genproto-required)：历史authority、跨架构独立布局、全族physical/required与ProcessCreate/InitializePen | 当前生成输入/快照manifest已存在；托管布局/声明/wire有限证据 | 实：ProcessCreate错误/引用/关闭、InitializePen ABI/dash/stride及全族消费者；资：历史关联与逐required；运：ARM64独立布局/真实执行 | 外部页历史/ARM64；5A | 混合 |
| 阶段5 | connection→channel→batch→dispatch/scheduler→Compose/Present/target及失败/取消/线程/callback/逆序释放 | [UCE r28](handoffs/completed-work/UCE-final-integration-20261014-r28.md)已Closed子合同 | 实/运：仅剩5A/B/C未满足差集，不能把有限UCE重新全领 | 阶段4、A/B、5A–C | 混合 |
| 5A | generated/resource布局及producer/consumer物理记录完整一致 | 独立声明/有限布局、有限命令传输 | 实/资：未闭physical/required、全族映射与依赖签名；运：完整目标布局 | C、阶段4 | 混合：历史/ARM64 |
| 5B | 引用/批次/失败/取消/初始化/线程/callback逆序释放全寿命 | FIFO、首HRESULT、channel owning、failed transport释放、drain/OS worker退出、跨channel关闭、双init有限Closed | 实/运：独立取消竞争；线程启动/CoInitialize/OOM/Compose/释放callback异常首错、唤醒与清理；正常drain不冒充cancel | 原生取消合同、5A、生产消费者 | 自主 |
| 5C | 真正命令/资源/target集成与像素因果，相关失败恢复 | 有限FIFO公开WIC像素、bitmap/media传输、软件media8 | 实/运：完整命令族及HW/SW消费者、批次中途失败、target恢复及并行/关闭 | 5A/B、阶段1–3、6A | 自主；目标矩阵受外部依赖 |
| 阶段6 | 完整创建/桥、导出语义/strict、DLL二进制/初始化/资源/退出/架构边界 | 有限方法族、HWND登记/DWM提示、x86 Shared有限运行 | 实/运/资：6A/B/C全部余项，不以PE存在替代语义 | 阶段4/5、6A–C | 混合 |
| 6A | meta/backend选择、factory/bitmap/stream/WIC/codec/media等生产创建/外部桥及失败拥有寿命 | 已有可达WIC、传输及主要对象有限交付 | 实：尚缺创建/桥可达分支与选择政策；运：成功/边界/失败/释放；资：原生文件/Ledger映射 | 阶段1–5 | 自主 |
| 6B | 完整公开导出/调用约定/参数/布局/HRESULT/异常/ownership与严格PE required集合 | 历史strict仅缺player2；geometry/tile等有限差分 | 实：player真实协议dispatch/所有权，其他方法族完整差集；运：最终same-SHA本族/related/full/strict；资：当前严格集合待核，见下节 | 6A、外部player合同/oracle；架构矩阵 | 混合；非“只补两个名字即全ABI完” |
| 6C | DLL资源/version/symbol/PE、startup失败与shutdown、资源/线程/callback静止及全部目标架构/配置 | [x86实证](investigations/audits/x86-shared-unload-actual-verification-20261014.md)Release Shared发布/真实x86/八轮资源寿命 | 实/运：x86全ABI/Debug/资源/失败/线程与callback、x64余项、真实ARM64；资：固定工具链及资源身份 | 6A/B；ARM64工具链+实机 | 混合；DLL卸载/重载/热替换NA，非待批准 |
| 阶段7 | 7A/B/C、目标全矩阵及原最终§9全部满足才有替换资格 | 当前primitive/image/nested/Blur/DropShadow有限PresentationCore证据；历史miter不拼当前SHA | 实/运/资：逐独立E2E补齐、全矩阵、Ledger及总验收；建设版不等于目标版 | 阶段1–6及下表 | 混合 |
| 7A | E2E-01–05同未改PresentationCore、原/候选隔离差分完整满足 | 10.0.1建设版当前primitive/image/nested/Blur/DropShadow逐项有限Closed；目标矩阵仍Open | 实：余下生产链；运：01–05完整输入/输出/失败/身份、目标10.0.12差分 | 6A–C、完整目标runtime | 混合 |
| 7B | E2E-06–08真实HW/HWND、D3DImage及媒体完整合同 | HW/WIC、dirty/callback、media8同步软件有限实证 | 实/运：真实HW恢复、D3DImage完整寿命、异步/HW媒体通知→sample→composition/target→重试/release/并行关闭 | 阶段1/2/5/6；真实组件/可观测会话 | 自主；ARM64矩阵需外部 |
| 7C | E2E-09多Dispatcher/重复关闭/退出与全部架构/配置/能力，完整Ledger总验收 | 有限UCE线程退出与x86资源循环；不是全矩阵 | 实/运：初始化失败、并发/取消/关闭、所有后台线程与callback静止；资：完整矩阵及生产Ledger闭环 | 7A/B、6C、历史/ARM64等必要输入 | 混合 |

## E2E 独立门（不得以测试名、低阶Green或模块Green代验收）

各行完整权威：[可执行合同](e2e-executable-contracts.md)、[矩阵与证据](testing-comparison-and-acceptance.md)。均须独立输入/输出、原/候选实际路径与SHA、HRESULT/异常/事件/退出及目标架构/配置/能力记录；当前有限覆盖不是整项Closed。

| 正式门 ID | 明确验收条件 | 已证覆盖 | 真正剩余：实 / 运 / 资 | 依赖 | 自主/需外部 |
|---|---|---|---|---|---|
| E2E-01 | 版本正/误、真实factory COM、QI/vtable/AddRef/Release/空参/ownership/双runtime | probe及factory有限机制 | 生产完整边界与目标矩阵差分，probe不代生产 | 6A/B/C、7A | 混合：runtime/ARM64 |
| E2E-02 | Same/CrossThread连接/channel/marshal/commit/close/destroy/disconnect、错序/重复/部分init/线程身份 | UCE有限拥有/关闭/双init | 其余错序/失败/线程组合及真实PresentationCore目标矩阵 | 5B/C、6、7A | 混合 |
| E2E-03 | create/addref/duplicate/send/release、payload/malformed/back-channel、32位ID/packet/router/factory/错误码 | 命令传输及有限FIFO | 全规定资源/命令族及失败引用清理、独立packet与生产目标矩阵 | 4/5A–C、6、7A | 混合：C资料/目标环境 |
| E2E-04 | 最小真实SW bitmap/target、确定几何/颜色/变换与规范化像素原版差分 | primitive、geom/tile有限结果 | 规定SW范围其余格式/裁剪/混合与失败寿命，完整矩阵 | 3/5C、7A | 混合：目标oracle/ARM64 |
| E2E-05 | 独立WPF DrawingVisual→DrawingContext→RTB→CopyPixels，填充/线几何/透明/变换，原候选独立进程 | 历史primitive/image十场景/nested八组两帧/Blur七组两帧及生命周期36帧、目标锁12项有限Closed；DropShadow最终固定28零差有限Closed；候选描画/核内fault及完整refs/close仍Open，历史miter按原档 | 完整规定范围及目标版本；白背景opaque不外推透明；callback/动画/格式按正式消费者合同 | 3/5/6、7A | 混合：完整10.0.12/ARM64 |
| E2E-06 | 真实HWND/HW bitmap、GPU/driver/RDP、create/lost/合同内恢复/fallback/present/close | HWND登记/DWM提示与历史可见会话有限 | 真实生产命令→呈现像素、失败恢复/关闭及跨会话因果；不捏造Reset/ResetEx | 1/2/5/6、7B | 自主；全架构需ARM64 |
| E2E-07 | D3D9 surface/frontbuffer callback/version/dirty/detach/SW fallback/device loss及线程/Dispatcher、detach后静止 | dirty像素/callback/替换有限 | HW color-source/adapter通知/bounds/availability/loss-recover、并发注销/替换丢包/引用重试；detach不冒充GPU loss | D3DImage主题合同、7B | 自主；全架构需ARM64 |
| E2E-08 | 受控资产/系统组件；open/event/frame/position/stop/close/shutdown/exit、apartment/state/event thread/timeout/callback | media8同步软件及真实传输两模式有限 | 持续异步/HW通知与真实sample消费、失败替换重试/并行关闭完整矩阵；不假指针 | 媒体主题合同、7B | 自主；目标runtime/ARM64依外部页 |
| E2E-09 | 多Dispatcher、重复创建关闭、finalizer/SafeHandle/进程退出、失败注入、Release/channel/player/线程/callback静止及全矩阵 | UCE/x86有限生命周期 | 完整资源/线程/callback、初始化失败/取消/并发与真实player关闭；进程退出不代逐对象清理 | 5B、6C、7A/B、player合同 | 混合；卸载/重载NA且不待决策 |

## 总最终条件与 P0（原始最终条款，非新门ID）

[原总规划§9](history/plans/migration-master-plan.md#9-完成标准)与[现行承接](plan-module-contracts.md#全迁移完成标准)要求**同时**满足：

| 正式条款/原P0 ID | 验收与已有覆盖 | 真正剩余 / 依赖 / 归属 |
|---|---|---|
| §9 Ledger与逐文件追溯 | 所有生产范围项最终状态/证据、无未记录重构/算法替换；已有机器记录不等于全部Accepted | 补齐正式生产分母、多视图/schema/JSONL与Markdown一致、全部未知blocker及逐文件证据；[Ledger§11](ledger-records-and-acceptance.md#11-完成定义)；自主为主，依未闭阶段 |
| §9 ABI/二进制兼容 | 架构/配置ABI/layout/COM/callback/resource/version/symbol；有限PE与方法族已证 | 6A–C完整矩阵；混合依赖，不以PE集合代全部语义 |
| §9 规定差分及真实PresentationCore全矩阵 | 固定有限Green保留 | 阶段1–7及E2E-01–09全部适用矩阵；依目标runtime/ARM64及player等；混合 |
| WP-00C / x86 P0 | 已有真实Release x86 Shared发布/加载/资源八轮，不再声称x86绝对不支持 | stdcall/完整ABI、Debug及真实调用方/E2E矩阵仍自主，ARM64仍外部；不删除Win32 |
| WP-00E / COM P0 | 有限IUnknown/factory机制与生产COM可达证据 | 全对象vtable/IID/refcount/GC root/apartment/失败寿命及目标矩阵；自主，依6/7 |
| WP-00F / unload P0 | [DEC-0004](decisions/DEC-0004-process-lifetime-module.md)已生效：进程驻留，不卸载/重载/热替换，NA而非通过 | 无新增批准或实现项；资源Release/channel/player关闭/线程退出/callback静止仍由6C/7C验收 |
| §9 P0与兼容替换资格 | 所有适用P0关闭或用户明确批准范围例外后，且其余§9全满足才可称兼容替代 | 当前不能授予资格；不得以报告、有限模块结束、外部包到位或修改golden/skip/required代替整任务完成 |

## 两类差集必须分账

- **严格PE/公开ABI**：[历史player/HWND档](completed-work/Player-Hwnd-production-closeout.md)的Missing=[MilPlayer_Create, MilPlayer_Process]、Extra=[]绑定历史SHA858C…，不是[当前基线](handoffs/current-stable-baseline.md)所列新候选的实测strict。**缺最新same-SHA strict验证**；当前完整Missing/Extra及AllAbiPassed不能从旧报告推断，更不能宣布全ABI已装完。名字存在还须全部方法语义与寿命。
- **历史32呈现/control失败分账**：该轮同SHA full32失败见[DropShadow完成档](completed-work/PresentationCore-dropshadow-acceptance.md)，不抹除、不全归环境；下述为历史独立身份追溯。[ImageBrush验收档](completed-work/ImageBrush-presentationcore-integration-acceptance.md#已有回归-trxrunid-与失败边界)绑定B293…；原生control前提黑像素/conditionSatisfied=false与未进入cache验证边界保留，逐项根因尚未确诊，不能全归环境或候选。独立于**资源方法族差分**：创建/命令/引用/清理/失败重试/协议布局及消费者自身合同；前提失败不证明cache正确或错误，资源差分Green也不抹掉32失败。历史r31一次full Green不是B293本轮通过或跨会话稳定闭环。

## 有限Closed不重领清单（仅原身份/原合同）

- [primitive/miter及最新ImageBrush](completed-work/ImageBrush-presentationcore-integration-acceptance.md)：固定十image场景、同SHA primitive和18 miter控制已收口；不追加范围拖延有限验收，不外推透明/完整格式/动画/callback。
- [WIC](handoffs/completed-work/S-20261014-004-WIC-MODULE-VALIDATION.md)、已有devicekey/cache失败保旧等有限证据保留；[UCE r28](handoffs/completed-work/UCE-final-integration-20261014-r28.md)各Closed行不重开；A/B与UCE独立取消/异常不因此关闭。
- [geom12](completed-work/Geometry-complete-method-family-run5-20261014.md)、[tile192有限Green及边界](completed-work/Four-exports-integration-closeout.md)、[media8](handoffs/completed-work/Media-r30-deadline-contract-closure.md)、[x86有限](investigations/audits/x86-shared-unload-actual-verification-20261014.md)不重领；不跨SHA拼接为最新定向或完整方法族验收。

## 审计收尾与防漂移

本页维护总计划的可核验剩余视图，与四短主Docs及独立完成档同步实际条款，不修改原始运行事实或冻结档，不创造第二套当前SHA。每次生产交付由开发者更新唯一基线及独立证据，门禁者按本表与原合同回读；门未满足时写具体剩余而非顺延口头承诺。唯一下一自主生产模块为ProcessCreate typed / partial-batch失败清理、引用世代与关闭owns完整拥有链，不重领已闭factory与softwareeffects；A/B HW消费者仍为正式Open差集，不作为并列当前派工。当前可自主推进的生产族不因四类条件停止；外部真正缺物到位后仍须实现及原始运行验收，才能重新判断**整个任务**完成。
