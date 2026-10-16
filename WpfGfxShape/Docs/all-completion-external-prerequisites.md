# 全部正式门：唯一用户外部输入交付清单

## 当前有效清单（覆盖下方历史清单）

架构仅 **x86/x64**，见[DEC-0005](decisions/DEC-0005-x86-x64-acceptance-scope.md)。ARM64工具链、编译/布局/实机及交付均为 **NotApplicable-UserScopeDecision**，不计阻塞、不是Passed，无需用户重新批准。

仍需确切来源：①原bilinearspan项目及实际依赖/来源或权威生成合同；②冻结输出与历史CLR/generator/输入/build的authority binding；③官方record packet player协议/ownership及匹配oracle。HW可观测会话/权限前提仍需核实，不把全部HW失败泛归缺环境；现有生成器/manifest不重复索取。具体原来源规格保留于下方历史材料，仅以上仍适用项生效。

**10.0.12不是已证用户强制版本，也不列永久必需用户输入。** 精确patch口径待澄清；完整同版同架构官方Microsoft.WindowsDesktop.App及依赖的加载与身份验证仍必要，优先复用已安装官方runtime，不是要求用户购买。历史10.0.12校准路径和10.0.1原版Pcore Green均保留，详见[唯一版本来源审计/实施方案](windowsdesktop-version-provenance.md)。

C5完整实际physical-record闭包、D4及x86/x64全部正式七阶段/ABI/寿命/E2E/配置/strict/full未满，父级仍Blocked，stage4Closed=false，总门Red。包到位不自动Satisfied。

## 以下为覆盖前历史核查与交付规格，不是当前ARM64/精确patch必需清单

历史“四类输入”、ARM64请求、10.0.12唯一目标及“其他版本不替代”的强制语句不再作为当前门；保留路径、MSB8020、加载失败和来源边界，不改Passed。

> 最新正式裁决见[唯一详档](completed-work/GenProto-C5-final-adjudication.md)：最终audit源码build/run及17,377行完整回读比较通过。333风险=13已证build声明+92 build未知+227 include未知+1缺项目，NA=0；下文105/227旧数字仅历史。319未知保留风险，不一律external，也不设必须穷尽的新门。下一有界补件仍为下列bilinearspan原项目/依赖或权威生成合同；历史binding、ARM64真实工具链/机器、full10.0.12、官方player协议及HW可观测会话/权限均不能凭空满足。C5及两个required仍Blocked。

> 最新C5状态见[唯一机器总裁决](completed-work/GenProto-C5-final-adjudication.md)：9正式入口未扩；105 build条件、227 include适用性仍未证，不能统称外部缺物。1原bilinearspan项目确缺（6配置item+1旧审计行不是7个缺项目）。以下四类输入及HW可见会话仍未收到；机器audit不改变这些事实。

本页仅维护外部前置与交付条件，不维护运行数字或阶段状态。当前状态只见[基线](handoffs/current-stable-baseline.md)，旧四项集成/HWND/tile均属历史独立证据。收到新交付必须回读来源与验收结果，不因局部Green删除正式条件。

## 当前新增确证缺物：原bilinearspan项目来源（与四类历史输入分开）

依据：[C5原物最终有界清单](completed-work/GenProto-C5-activeincludes.md)及工作区根 `artifacts/log/GenProto-C5/production-reference-error.txt`。原 `core/dll/wpfgfx.vcxproj:142–144` 显式引用、GUID `{3A6A5D23-CB65-4685-AAC9-0969054701EA}`；Debug/Release×Win32/x64四配置真实GetTargetPath均报 **MSB3202**。这是适用ProjectReference确缺，不是header失败：同次必要TU预处理成功，不能推断缺项目负责生成那些header。错误文件是标明身份的诊断摘录，不冒充完整控制台日志。

| 需用户补的精确物 | 最小交付与验收条件 | 不可替代/原条件 |
|---|---|---|
| `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/bilinearspan/bilinearspan.vcxproj` 原文件及其实际依赖 | 提供原仓库可追溯来源/版本、原目录结构，连同该项目引用的源、props、targets及其他实际依赖；附路径和SHA256清单。恢复后按原选定配置核对项目身份、引用/输出及既有record归属 | `core/sw/bilinearspan.h` 与 `core/sw/swlib/bilinearspan.cpp` 不是项目；不能复制拼造vcxproj、删引用或假Condition绕过 |
| 若该项目本应生成：有权威的原生成合同（作为上述原项目的替代交付方式） | 原仓库target/生成器、全部输入/依赖及版本、调用条件与预期路径/输出合同；须能实际重现该原项目并核对GUID/配置/依赖。当前未找到此规则，不能给不存在的target编名 | 原C5 group readiness要求全部实际物理输入/组件/冻结输出/消费者有record；当前缺来源使完整物理引用闭包无法满足，不由同名header或CL成功替代 |

收到补件仍需有界复核，不等于C5自动Satisfied。本次对这项来源已到真实资料边界；原333审计行里未逐条完成的适用性/record/assertion审计仍可自主，不统称用户缺物。四类历史输入与HW可见control前提保留如下；已有生成器和输入manifest不重复索取。

## 四类输入验收规格（已有材料不重复索取，实际缺项以下方复核为准）

可交付可读归档的绝对路径、只读共享路径或可访问来源/机器标识；下列为包内建议相对路径，不表示已收到文件。每包附 `manifest.json`（来源/许可或访问授权、版本/架构、文件路径+SHA256）；不要发送密码或密钥。未知历史身份须注明未知，不编造commit。未获得新增来源，本次不重试同一缺物路径、不宣称已自主解除。

| 输入与交付路径 | 包内容 | 验收条件 / 对应正式门 |
|---|---|---|
| **历史authority**：提供 `historical-authority/` 归档绝对路径或可访问源码来源 | `clr/`：固定CLR仓库/commit、String hashing源码及编译条件、runtime产品/build/位数、hash branch/seed/randomization/config；`generator/`：冻结生成器revision与完整递归输入manifest（含Version.xml、Resource.xml、Resources.rsp及SHA256）；`vectors/`：独立已知输入→输出和来源说明 | 可追溯冻结输出至当年生成环境，复核hash/协议指纹；C及阶段4/5布局/required authority。当前net48/x86、SDK、公开签名或候选自生向量不替代历史来源；精确历史版本尚未知。依据：[分层审计](handoffs/completed-work/GenProto-finite-closure-audit.md) |
| **ARM64工具链+真实运行环境**：提供 `arm64/` 清单及可访问构建机/真实Windows ARM64机器标识 | `toolchain/`：现NativeLayout.ARM64.vcxproj所用v145 ARM64，或真实ClangCL（aarch64-pc-windows-msvc）及MSBuild集成，配Windows SDK ARM64 linker/import libs；`machine/`：Windows/.NET版本、真实架构、GPU/驱动、会话及允许测试配置、授权访问方式 | 消除已实证MSB8020；记录编译器/声明/packing身份及PE/COFF machine=0xAA64、独立布局；另有真实ARM64托管/生产执行身份与原始结果。覆盖C与6C/7全架构矩阵；交叉编译不冒充nativeRuntimeExecuted，SDK headers不等于compiler，v145不是唯一品牌。依据：[实际工具链失败](investigations/audits/stage4-arm64-layout-final-path.md) |
| **合法完整WindowsDesktop10.0.12 runtime**：提供 `windowsdesktop-10.0.12/` 合法安装包或完整安装的绝对路径 | 匹配验收架构的完整runtime/native依赖、同版PresentationCore，版本/架构/来源/许可与SHA256；现普通历史证据路径 `C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\10.0.12\wpfgfx_cor3.dll` | 原版DLL及依赖真实加载，身份匹配，再按未改golden/skip差分及7A/E2E验收。历史普通4211/1/4212的唯一失败为0x8007007E且目录缺失；单DLL、其他版本、候选均不替代。历史沙箱其他版本独立参考亦不替代10.0.12。依据：[加载失败与路径](investigations/INCOMPLETE-HW-CACHE-CLOSEOUT/evidence-readback.md) |
| **官方record packet player协议与ownership**：提供 `record-packet-player/` 官方可读冻结归档或授权来源 | `protocol/`：MIL_REC_PACKET_HEADER/UCE_RDP_HEADER完整定义、packing/版本/包种类/长度/校验及真实有效无效向量；`implementation/`：CMilRecordPacketPlayer创建与ProcessFilePacketContents的正式语义/可获取源码；`ownership/`：HMIL_PLAYER拥有关系、destroy/release ABI、线程/取消/失败/关闭合同；`oracle/`：匹配PRERELEASE能力的可执行原版与依赖、版本架构SHA | 可追溯官方协议与所有权，真实dispatch/资源调度及寿命实现后成组原版差分，再最终同SHA strict/full；repo src仅有调用，Tests/Ledger未找到定义，现冻结authority配置不提供协议。媒体对象、猜包头或NotImpl不替代；对应6B和相关5/7门。依据：[原生与实测](completed-work/Player-Hwnd-production-closeout.md) |

## 全部正式门的外部输入最终核查

| 正式门 | 外部输入结论 / 仍须自主完成 |
|---|---|
| 阶段1–3、A/B、HW/cache/shader/D3DImage/媒体 | 未证实新增必需用户包；生产消费者、失败/恢复/寿命、可观测session仍自主Open。不能把当前呈现失败一概归环境或要求用户为全部失败补硬件；若后续证明具体能力不足，再附可复核依据请求具体环境 |
| 阶段4/C、5A/5B/5C | 上表历史authority与ARM64确缺；完整ProcessCreate/InitializePen、physical/required、UCE异常与逆序清理仍自主；有限GenProto通过不关闭未满足的physical/required/satisfied正式条件 |
| 阶段6A/6B/6C | 生产创建/桥、当前缺项按原报告成组、完整ABI、资源/初始化/静止/关闭仍自主；ARM64依上表。x86 Shared有限实测见下文，完整ABI/E2E仍自主；unload/reload/热替换按已生效产品合同不适用，不列用户输入或阻塞 |
| 阶段7A/7B/7C与替换 | 7A同未改PresentationCore隔离差分需完整10.0.12；7B真实HW/媒体、7C多Dispatcher/重复关闭退出及全配置能力矩阵仍自主，真实ARM64依上表；不得以交付包到位、退出或局部Green代替最终验收 |

**已确认范围与独立证据**：[产品合同](decisions/DEC-0004-process-lifetime-module.md)已生效，生产DLL进程寿命驻留且不被运行期替换；unload/reload/热替换不适用（用户明确无此需求），不是通过、不是能力缺失或待批准项，不阻塞总计划。[x86核验](investigations/audits/x86-shared-unload-actual-verification-20261014.md)已有固定工具链发布、真实x86加载及八轮有限资源生命周期证据；完整ABI/配置/E2E仍Open。COM Release、channel/player关闭、线程退出、callback静止、初始化失败清理和最终兼容原WPF库验收不减；四类外部输入不变。

## 本次只读复核与最小获取途径

下表收窄上表“请交付”的实际缺项：**四类必要条件尚未全部满足，不等于四包所有内容都不存在，也不等于只能等待用户或不可联网。** 已有源码/manifest由开发者直接复用，不要求用户重复提供；尚未取得的来源不得编造身份。外部获取与环境变更不在本次Docs审计执行范围。

| 输入 / 阻塞门 | 已实际核实的材料与真正缺项 | 最小实际获取途径（本次未下载、未安装、未执行） |
|---|---|---|
| 历史authority / C、4、5A及最终布局/协议 | **更正“生成输入全缺”**：repo已有`src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/`生成器、Resource.xml、Version.xml、Resources.rsp；[已有authority.json](../Investigations/GenProtoEvidence/regeneration-244271f94bfc430e9c3799604fb8487f/authority.json)记录逐文件原/拷贝SHA及Ledger revision（不等于历史生成commit）。ProtocolFingerprint.cs:298/308/315真实调用String.GetHashCode；已有输入快照不能证明历史CLR build/hash branch/seed/randomization/config或冻结输出的当年输入关联 | 先复用既有manifest/快照和再生结果，不再索取这些文件。历史资源所有者仅补：冻结输出→实际generator递归输入/revision的来源关联、当时CLR产品/build/位数/条件及独立已知向量。可从官方CLR仓库历史源码定位候选算法，但当年身份未知时不能选任意commit当oracle；若无归档关联仍需该精确历史材料 |
| ARM64 / C、4/5A、6C、7全矩阵 | 本次枚举`C:/Program Files/Microsoft Visual Studio/18/Professional/VC/Tools/MSVC/14.51.36231/bin/Hostx64`仍仅x64/x86。既有[探查](investigations/audits/stage4-arm64-layout-final-path.md)实证v145/ClangCL均MSB8020；缓存PublishAotClang是Linux包装，不是Windows ARM64 authority。未取得真实Windows ARM64执行身份，不声称遍历所有外部机器 | 工具链所有者可通过VS Installer的C++ ARM64组件，或正式LLVM Windows-MS-ABI工具链加SDK ARM64 libs/MSBuild集成补足；也可直接给已有可访问构建机，非必须v145品牌。另须资源所有者提供一台可授权运行的真实Windows ARM64机器标识与访问方式。交叉编译只能补独立布局，不代实机矩阵 |
| 完整10.0.12 / 7A、目标版本差分及最终矩阵 | 本次直接目录查询确认`C:/Program Files/dotnet/shared/Microsoft.WindowsDesktop.App/10.0.12`及`C:/Users/lindexi/.nuget/packages/microsoft.windowsdesktop.app.runtime.win-x64/10.0.12`不存在；其他已安装版本不是目标。最新普通TRX仅证明DLL或依赖加载失败，不能据其异常分辨两者；此处目录不存在是独立新核查，不改写历史运行事实 | 可先查微软[.NET 10官方下载入口](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)的Windows Desktop Runtime对应版本/架构；repo根`NuGet.config`已有dotnet-public/dotnet10等官方feed，后续可核对精确`Microsoft.WindowsDesktop.App.Runtime.win-x64/10.0.12`及所需架构包身份/依赖。**入口/feed存在不证明该精确版本当前可取**，本次未远程验证或下载。可取得合法完整包则开发者补资料而非必需用户上传；否则请用户仅提供该版完整合法安装归档/路径及来源身份，不要单DLL |
| player协议/ownership/PRERELEASE oracle / 6B、相关5/7及E2E-09 | 本次repo `src`全文检索MIL_REC_PACKET_HEADER/UCE_RDP_HEADER/CreateRecordPacketPlayer仅命中`core/uce/apifunc.cpp`；已读785–867：PRERELEASE调用CreateRecordPacketPlayer/ProcessFilePacketContents，else为E_NOTIMPL，无header定义或destroy/ownership。已有[查证档](completed-work/Player-Hwnd-production-closeout.md)的Tests/Ledger缺定义仍未被新增来源解除；普通发布版E_NOTIMPL不是所需oracle | 先复用公开repo调用签名；尚无已定位可读包/源码能补正式定义。请官方/历史资源所有者提供上表最小协议与所有权归档、来源revision及匹配PRERELEASE可执行oracle/依赖。可给授权只读源码/构建归档路径代上传；不能用普通WindowsDesktop runtime、媒体player或猜头替代，不虚构官方身份 |

这些路径核查仅证明指定位置与既有证据的边界，未断言全磁盘无物、网络不可用或四类永远无法自主获得。专用API/包元数据和现有官方来源可继续用于定位资料；公开类型列表或签名本身不证明私有协议/历史生成环境。只有材料内容、来源与适用身份核实后才能消除对应外部缺项。

**交付状态**：历史关联、ARM64真实执行资源、完整目标runtime及player正式合同/oracle仍未满足；已有生成器与输入manifest不再作为全缺外部项。具体用户补件集中本页，不在各报告复制交付清单。逐门剩余见[最终验收矩阵](final-acceptance-remaining-matrix.md)，权威条款见[总计划](remaining-gap-closure-plan.md#七阶段正式完成条款保留不以导航或有限green代替)。本次仅Docs与只读核查，无构建测试发布、下载执行、终端脚本或环境变更。总计划当前不能全部完成，但自主生产实现/接线/失败恢复与补证必须继续；不是无期限等待承诺，也不是交付包到位即Closed。
