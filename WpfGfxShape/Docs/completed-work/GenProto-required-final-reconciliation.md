# 两个 MILCMD required：有限分母及逐 assertion 最终对账

> 最新正式裁决仅见[唯一C5详档及表](GenProto-C5-final-adjudication.md)：当前audit最终build/run、17,377行完整回读比较完成；333风险=13已证build声明+92 build未知+227 include未知+1缺项目，NA=0。原group readiness/doneRule不要求穷尽保守333；风险原表不删，不判排除。下文旧105/227与全部逐边审计恢复任务已撤销，原C1–C5/D1–D4及required不删；C5/两个required/父级仍Blocked，stage4Closed=false。当前selected CL/imports不再记未采集。每个正式ID的必要证据、当前充分性及缺口由唯一详档维护。

> 最新C5唯一总表见[机器总对账裁决](GenProto-C5-final-adjudication.md)：9正式入口不变；17,377是原物审计记录而非正式边。旧333候选已逐行记录105 build条件未证、227 include适用性未证、1缺项目；不能声称最小精确适用边全集已证。下述旧C5状态仅历史，C5/两个required及父级仍Blocked。

> C5当前裁决由[activeincludes最终有界清单](GenProto-C5-activeincludes.md)覆盖本页旧C5未evaluation/未采集描述；此前evaluation报告保留原身份，最新身份只见[基线](../handoffs/current-stable-baseline.md)。12配置16必要TU预处理及所列实际边归属已证，全部适用原边逐record/assertion审计仍未完成；bilinearspan项目四配置MSB3202确缺，不是header失败。正式6 physical+3 evidence不变，原权威门未足；以下“本轮”指本页原历史执行，不是本次文档对账。

## 权威与本轮实际动作

已读两个原 required 全行：`Ledger/evidence.jsonl:58–59`；追溯原条件见 `Docs/investigations/superseded/stage4-ledger-and-last-contract-closure-plan.md:68–82` 及 `Ledger/groups.jsonl:2`。CLOSURE 要求完整生成输入/输出/消费者及映射；REGEN-DIFF 要求隔离再生和语义/全文差分，不覆盖冻结输出。group readiness 为物理输入、生成器组件、冻结输出及消费者已有 Ledger record；doneRule 是隔离再生差分满足组条件。后续报告的 remainingRequired 是证据调查结果，不自动具有新增原 required 的权限。

本轮未修改生产或测试源码。只执行现有 WireCompilerArchiveTests：11_34_07 TRX，RunID `e86fd073-7638-4ca9-8684-4e7cf5200667`，1/1、0 fail/skip；完整机器解析旧/current report及summary，核对sources/managed assembly/test assembly，复制并hash核对既有最新full。未重跑45Green，未生产publish或COMfull，无终端脚本。原生读取合计306行（connection 71、connectioncontext 101、composition 101、clientchannel 33），小于400。

## 固定分母

正式入口固定9个ID：6 physical + 3 evidence。仅取group既有 member/authority/consumer/required 去重集合，不再保守遍历全repo。递归5301文件/4617边/921未解是审计包络，不是新增正式required或必须翻译分母。build-contract明确仅三个正式project roots及其显式references/imports/items和按序include闭包；固定历史721边与production conservative分类不得相加。116 command declarations/dispatch、108 managed wire、8 native-only、97 factory IDs是各自独立合同分母，不能相互冒充完成数。

证据索引（以下SHA均为已有机器绑定或本轮测试输出，不冒称人工重新计算）：

- **P** `Investigations/GenProtoEvidence/milcmd-physical-537d4ba0f0394ad3ab3d9456ec3ec84c/report.json` SHA `1F9E5741A7A0039E97862CC07C9CEFF65B69D0C7150E9CB30C21E1D1110FA1E8`；manifest SHA `755B16AFA2FD876A325D216787EFDC3866AA48709F07D3E0EA7C5D40E7C81663`。实际run及sourceStable=true，逐dispatch原文/签名/输出绑定保存；不是全执行证明。
- **B** `Investigations/GenProtoEvidence/milcmd-build-contract-b6261bbffd564a0998b8cc208ac28724/report.json` SHA `B7E69AD79704EB74AB49F34BBDF97814D43D2F687711B9197108D447F5CFEF4C`。实际分类历史721边；production分类1729 resolvable、332 undetermined、2 conditional、1 realMissing、125 SDK、7 generated；未给出的evaluated selection不伪造。
- **R** `Investigations/GenProtoEvidence/required-integration/report.json`：测试source SHA `FCF10C08E3B6BA7E239305B5388EF20D8FFA3E421F74E72D22727C07CDD83C83`，assembly SHA `0DA89E410A1AFA4109D223C874EBE1AA89395542ADABB343D4FCB38D31E7A41B`。再生authority SHA `581ABA44495F75B1BC36319519DB07A0047F49700C5118CE479142EE6509E49B`、invocation SHA `B3E9BB6EAFBC5073A007E355CD24F77BC8D1BC75C6018FBCD8C4ED5219016C8C`、results SHA `8D89523EDDD451B4D73D7C9ACECEC880B8834CD1BA4123A80B4B846C53822972`，原目录 `regeneration-941a112ea3d44800ad1527f53bf9528c`。实际专项/既有full中的RequiredIntegrationAuditTests逐输出hash、全文、diff及Roslyn token/换行检查，378输出/275 byte-exact/378所列范围等价。
- **W** 本轮完整归档 `Investigations/GenProtoEvidence/wirecompiler-delivery-5a29d838ee904436b59dcdb05b9ee63f/report.json` SHA `7F88D59055EF8F904E1EDBAA8BE798823CE49E3C3715EACE1DBCBEBE735104A1`；current测量 `wirecompiler-4daf2f02e22d42af9e4bdf770f5ded73/report.json` SHA `11670359498C6601FE98F98CA767E3EF268600DA8F8D87DE7E57AF577E27F688`，完整读取12078 leaves及summary75 leaves。当前测试assembly同R，MVID `88de9ccc-17f9-4814-8a4f-e0a2db373422`。
- **F** factory report `factory-physical-ef1f18b2c6ee4c67905564648db4434f/report.json` SHA `6EEE7ECBE54DBB0BF511F72D3A03070B204BBFA0A3C8FA3BE19A7B91D9AD39EB`；97 IDs、76 concrete/21 rejection逐项绑定public AOT。原AOT RunID `262f277c-5f57-44d7-80a2-1ece70cba1e0`，TRX SHA `78573F4B8BAFD9B1B29BAD686D28745DD453F93545A054441DB5507E42A9B3A0`、identity SHA `393A122FF410B071B331CCDB0B68F690D63DFF6372027D2D44A89E037F1F84EE`；本轮未重新执行AOT。
- **N** `Investigations/GenProtoEvidence/nativeonly-execution/report.json` 的sources含native/候选逐文件SHA；历史独立Ledger保留其当时报告身份，不将可变report的后来内容仍称旧hash。固定候选生产SHA见下；实际bitmap/create/delete/duplicate断言及四组raw Results机器读取保留。
- **M** R内六份Media JSON/进度均完整读取并逐份SHA；candidate TRX SHA `A0EDB4C0531B3EFDE6B0B70740C9AB3DD25806DA97B2A70BA2EAA6DBD04DE82D`、RunID `810bdfd8-0396-45b6-b259-116275ef325e`；reference SHA `744ACEFDCAA366FE9388AB7966EAD7C9683D9EA04A3B87DAB6E48E2E24BA8FD6`、RunID `116349a8-9937-4f79-9dd4-ffcb0b63ecbc`。各3成功断言，拒绝E_INVALIDARG、transportAddRef=2、最终root存活时before=after=1、Shutdown后root释放进度。

## 原 required 逐 assertion 表（冻结，不随缺证新增行）

| ID | 原权威条件 | source / SHA / 实际run与assertion | 满足或剩缺；责任 |
|---|---|---|---|
| C1 | CLOSURE：生成输入及schema可追溯 | Resource.xml/Resource.xsd，P清单，R authority逐文件hash | 当前源来源满足；历史版本binding另见D4，不能以当前运行顶替 |
| C2 | CLOSURE：生成器组件及构建入口可追溯 | mcg.proj，P、R invocation/authority、实际Resources.rsp执行 | 当前生成器输入/调用满足；完整选用build条件仍见C5 |
| C3 | CLOSURE：冻结输出及生成映射完整 | R 378逐项全文/diff及SHA；F 97资源、76构造/21拒绝，P 116dispatch原文 | 所列当前生成映射满足；不宣称ProtocolFingerprint历史一致 |
| C4 | CLOSURE：实际消费者及实现分别追溯 | resources.vcxproj/uce.vcxproj/wpfgfx.vcxproj；P consumer签名，F逐项实际AOT，N四packet，M六例 | 现有consumer declarations独立Satisfied保持；不是每个命令执行已全覆盖，不能按同名Passed推断 |
| C5 | group readiness：全部实际物理输入/组件/冻结输出/消费者有记录 | 正式6 physical roots，P清单；B限定三个native project roots、显式items/imports/include | **未完整满足**：选定ClCompile/import/include/defines及SDK/VC版本header来源未确证，332 unknown/1 RealMissing的适用性未定。可自主做范围内evaluation/归属对账；ARM64/外部历史选用材料缺失部分外部。不能以5301包络全有hash代替 |
| D1 | REGEN-DIFF：隔离执行 | R authority/invocation/results三SHA，原Resources.rsp run | 当前源隔离再生满足；未覆盖冻结输出 |
| D2 | REGEN-DIFF：逐输出全文与语义差分 | R逐项exact/newline/token断言，378/275/378 | 所列比较范围满足；token等价不是ABI/执行全等价 |
| D3 | REGEN-DIFF：生成/差分/冻结输出身份绑定 | R每输出generated/baseline/diff hash及全文；W当前assembly/full快照 | 当前证据绑定满足；旧assembly失配明确保留，未放宽断言 |
| D4 | 原冻结历史输出的generation/build authority绑定 | HistoricalAuthorityTests及R明确rsp不含ProtocolFingerprint | **未满足，外部**：原历史CLR/hash/输入与build版本authority；不能用当代generator或当前378输出填补 |

### 6 physical + 3 evidence 入口逐ID归属

| 既有ID（REC前缀不变） | 对应事实与缺口 |
|---|---|
| REC-GENERATED-PROTOCOL-CODEGEN-MCG-XML-RESOURCE-XML | C1；R逐输入hash，当前来源已证 |
| REC-GENERATED-PROTOCOL-CODEGEN-MCG-XML-RESOURCE-XSD | C1；R逐输入hash，当前来源已证 |
| REC-BUILD-BUILD-CODEGEN-MCG-MCG-PROJ | C2/C5；当前执行已证，完整selected build归属未闭 |
| REC-WPFGFX-PROJECT-CORE-RESOURCES-RESOURCES-VCXPROJ | C4/C5；消费者声明已证，selected items/imports归属未闭 |
| REC-WPFGFX-PROJECT-CORE-UCE-UCE-VCXPROJ | C4/C5；consumer/dispatch已证，selected items/includes归属未闭 |
| REC-WPFGFX-PROJECT-CORE-DLL-WPFGFX-VCXPROJ | C4/C5；最终项目入口已列，selected SDK/VC/条件来源未闭 |
| EVID-GENPROTO-CONSUMER-DECLARATIONS | 保持既有Satisfied，不重新领取 |
| EVID-GENPROTO-MILCMD-CLOSURE | C1–C5，父级Blocked；不能由增强执行要求替代C5实缺 |
| EVID-GENPROTO-MILCMD-REGEN-DIFF | D1–D4，父级Blocked；当前差分已满足而历史binding未满足 |

## transport / discard 裁决：不降低原required，也不增新门

原生公开路径确有consumer：connection.cpp SynchronizeChannel构造SYNCFLUSH、CloseBatch/Commit并等待；composition.cpp Transport_SyncFlush持有channel引用并延迟signal；connectioncontext.cpp forced close构造完整destroy+flush批次并转移所有权；clientchannel.cpp Destroy先IGNORE_HR(SyncFlush)再DestroyChannel。对应native source SHA见N/W：connection `F99FDFE027926B53D0C658C944BDB6F29565FE5A7C3537F0670E9717D71C4F8A`、connectioncontext `588D68386CBE8EE61176F463F6387CEB0A73E9C9FC895B085BFC7CAD26578271`、composition `DD06C94AF98084B6D0A0530BD24ED15688C0922430B8924C8C1EF80850946AD8`。

候选UceChannelExports SHA `85D946579B90675B3C7F021C2197E0989140C3DCBC80E9106747B69AC77A82A9`：SyncFlush使用CloseBatch/Commit或scheduler barrier；Destroy使用State.Destroy或scheduler destroy。通用SendCommand明确拒绝两种transport packet（E_NOTIMPL），不存在已消费包的实证。此结论不能写成packet execution Satisfied。正式目标优先外部ABI/生命周期等价，但章程禁止未经证明的线程/控制流重设计；因此已有API行为证据只满足已测层，**不据此正式批准完整transport等价**。原两个生成required没有强制候选内部必须同样传包；故也不据“包未执行”自动派生产实现。若后续固定公开断言发现返回值/顺序/callback线程/失败清理差异，才按该具体差异修生产并执行正确publish+related/full sameSHAidentity；不发stub空包凑证。

独立queue-discard oracle不是C1–D4中的新增条款。M证明公开拒绝/成功/隐式flush销毁引用平衡；Destroy先drain，不能自行构造“未commit=discard”。异常/zombie路径、最终对象析构和原生packet trace仍未证；这限制生命周期等价外推，不把无公开可达合同的独立故障注入变为关闭生成required的新门。VisualCreate原生RIP/E_UNEXPECTED的分类保持，不新增producer。

## 最新 full 与 stale assembly 完整归档

11_25_31原红TRX保留，RunID `77950b13-1af1-4275-83f9-b805ef0589c6`；WireCompilerArchiveTests行46绑定旧assembly `84173E305C5F7B772854A20B454EB0B917745366D4F1BCFB05E57CAB7F47F918`，实际当前为R的`0DA89E...`，44 pass/1 fail，不忽略。

W归档的current measurement绑定当前assembly及isolated ManagedFields SHA `D229052F4E3226D71E9CBD15B70513AC539120EA8B69FECB84D957D7AC226D2F`。full原始11_26_23，RunID `25c5868b-d0e0-47ce-b89d-430d6c87c721`、SHA `B67D6711376FCF87B5E657B676D82CE49DB74C522F674FBFB414F2E8F575CCF8`，Counters实际45/45/0/0。W存其完整原TRX副本且逐字节SHA相等；W.targeted仍为历史10_52_20测量单测，不冒充本轮归档专项或测量重新运行。当前测量含232行（116×Win32/x64），已有enum/padding后续独立裁决保持，不把旧null或派生x86误述当最新结论。

## 明确停止点与自主条款

本次完成原required有限对账报告及最新full真实归档；**没有完成两个required的全闭包**。仍可自主且正式必须的工作是C5：固定6 physical入口下选定build依赖的evaluation/记录归属与适用性裁决，不是遍历全repo，也不是预设必须改生产。当前证据不能断言“再无可自主正式工作”，同样不能证明“必须新增transport生产包实现”；两者均不冒报。

生产保持 `3F291D3E6EF3A74688B8FDCBF124B725DC115D165FAE94DB9B2010BFDCE2570D`；sameSHA历史COM1616/1583/33/0、strictMissing2/playerExtra0均保留，非本轮新跑。历史authority、ARM64、full10.0.12、官方player协议与HW可见会话control前置不编造、不阻合法自主C5。parentBlocked=true、stage4Closed=false；用户总目标持续、总门Red。源码未改且已由本轮专用测试构建；本轮归档TRX/report关键身份及断言已回读，文档已落盘，不留未编译源码或文档待更。
