# 全部正式门：唯一用户外部输入交付清单（一页）

本页仅维护外部前置与交付条件，不维护运行数字或阶段状态。当前状态只见[基线](handoffs/current-stable-baseline.md)，旧四项集成/HWND/tile均属历史独立证据。收到新交付必须回读来源与验收结果，不因局部Green删除正式条件。

## 请用户/资源所有者一次交付的四类输入

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

**交付状态**：四类输入未收到；本环境不能凭空产生历史authority、真实ARM64或合法完整runtime，无网络不可用断言。若新增可读来源，先核实再请求缺项；收到后按路径/来源/身份/内容初验，记录原始验收结果而非直接标Closed。七阶段权威条款及E2E-01–09保留于[总计划](remaining-gap-closure-plan.md#七阶段正式完成条款保留不以导航或有限green代替)，无替换资格。既有HWND发布与沙箱结果见独立档；本轮仅Docs同步，未构建测试发布，未用终端脚本，不宣称全完成。
