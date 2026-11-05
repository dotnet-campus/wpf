# IK：默认异步 partition 动态状态产品回归

## IM 最终裁决：双架构默认独立动态回归保留，文档终态已收口

授权沿用本 IK 报告，不另建重复报告。IM 仅专用文件读取与四份文档补记；生产/测试源码修改 0，新 build/publish/沙箱/测试运行 0，无新 Host/工程。保留 IH 有限生产修复和 IA/ID/COMfill。未发现须撤回默认回归的具体风险；默认动态回归不再因报告缺尾悬挂待验收。下方 IK 进行中/开始阶段及共享文档 IL/IJ 旧下一门均为历史，当前以本节为准。

### 已跑证据定位与计数（不是 IM 新运行）

本地绝对根 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape`，以下本地路径相对此根。

| 范围 | 实际绝对 task 根 | 本地原件根 | 裁决 |
|---|---|---|---|
| IK default x64 | `C:\CodingAgentSandbox\Tasks\6185fdc6e5914b11b30a3c76d4714481` | `artifacts/ik-default-state/readback/x64/Results/ik-default-x64` | 父 child.trx 2/2 Passed、监督 Exit0 |
| IK default x86 | `C:\CodingAgentSandbox\Tasks\a9960230c2074fc0a7d0c2b290fb66df` | `artifacts/ik-default-state/readback/x86/Results/ik-default-x86` | 父 child.trx 2/2 Passed、监督 Exit0 |
| IK default 兄弟 x64 | `C:\CodingAgentSandbox\Tasks\43582650a24244889d4546da907f595d` | `artifacts/ik-default-state/siblings-readback/x64/Results/ik-default-siblings-x64` | 25/25 Passed、监督 Exit0 |
| IK default 兄弟 x86 | `C:\CodingAgentSandbox\Tasks\61c77e13916e49b694165c2735d9afe7` | `artifacts/ik-default-state/siblings-readback/x86/Results/ik-default-siblings-x86` | 25/25 Passed、监督 Exit0 |
| IL 补 HW x64（复用 II 显式包） | `C:\CodingAgentSandbox\Tasks\ac492da22f324fa79f56d8bb9c4c0335` | `artifacts/il-default-state/hw-readback/x64/Results` | il-hw-x64 父 1/1 Passed、监督 Exit0 |
| IL 补 HW x86（复用 II 显式包） | `C:\CodingAgentSandbox\Tasks\a2414bd326ed41a4a7d8c10945f9cad0` | `artifacts/il-default-state/hw-readback/x86/Results` | il-hw-x86 父 1/1 Passed、监督 Exit0 |

各父精确 TRX 为对应结果目录 `TestResults/child.trx`；监督为同目录 `supervision.json`。default 的 group UUID：x64 `9e79dd8df1db42658e67eb110b563b3a`，x86 `69abd821b5814a19b1c9b919e472b604`；其下 `partition-present-state/<UUID>/{official,candidate,official-repeat}/TestResults/child.trx` 六角色各 1/1 Passed，role-exit.json 均 Exit0/timedOut=false，监督无 timeout/kill/output-drain 失败。父两个测试分别为独立 group 与无 role 实际 candidate，后者不是空返回；direct UUID x64 `003f5f6b69c544b2a213af9efab73ff5`、x86 `3ff95a9a1b7a43fdb8b90bbc30fcde65`。

默认兄弟每架构严格为 3 通知/锁 + HY 2 + ListenerBroadcast 2 + 候选排空 12 + 官方排空 6 = 25。监督 filter 虽含 HU 名称，但本默认 TRX 未发现 HU，不能把过滤条件当执行或称 HU 默认覆盖。IL 补 HW 两架构 `Results/hw-roles/{official,candidate}/role.trx` 四角色各 1/1 Passed，exit.json 四个 Exit0；不是 IK default test 身份上的 HW 运行。HW 名为轮次，不等于硬件 backend：business.scope 明确 nonHW class5/message-only HWND，未触发 HW render target/SyncModeStatus。

### 实际模块、test 与 runtime 身份分账

下列 Native SHA 来自角色业务 actual/loaded，不用包声明替代实际加载。每个 task 内候选实际路径为 `<task根>\Native\wpfgfx_cor3.dll`，官方为 `<task根>\wpfgfx_cor3.dll`；测试为 `<task根>\WpfGfxShape.ComAcceptance.dll`。Native 未重发。

| 架构 | 实际候选 Native SHA256 | 实际官方 Native SHA256 |
|---|---|---|
| x64 / pointer8 | `EE3F1DE1BC3192766ABE5ACBC94B2548B9EEDD5107B5E7BA8BF8F465DB148189` | `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1` |
| x86 / pointer4 | `01CF2206C3AE4327B6057A18AFA084629B49409AC812CFBD099E22CB66105ABC` | `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834` |

| test 账本 | SHA256 | MVID |
|---|---|---|
| IK default/25兄弟 x64 | `253881259FFA69E987AFF0EB5D74CF01ABA3200F13D16F9D7CE192E3B2934C8F` | `cb759b13-0c14-46ff-a121-d2e7073cc2da` |
| IK default/25兄弟 x86 | `0A8D7A5574146D26A293B8190B681ED8232C5E6CCA43CD879ABFD987CC600BB1` | `d5dd13f1-0520-4c5c-9de3-6f41de83b077` |
| II 显式包 / IL HW x64 | `B84B35125789717EB88E32294944210B91D5E9CDFB01FE9862E7E3C6E88210A7` | `090b609d-35ed-4eba-a2be-7b5542a219b4` |
| II 显式包 / IL HW x86 | `400714294930D5322C96A2C8835F9AA52E499A37EB7AFD21D74A203A0F9F7457` | `e8ca1b93-a055-4b70-b58f-7e713482c081` |

实际 runtime `.NET 10.0.13`；WPF/官方 product `10.0.13-servicing.26504.104`，commit `4099f83977f4d29604a9aeb971fb0a7bc1d5f699`，HW 文件版 `10.0.1326.50504`。PresentationCore SHA x64 `0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31` / x86 `D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D`。来源边界：本包 self-contained Microsoft.NETCore.App/Microsoft.WindowsDesktop.App 10.0.13，runtimeconfig/deps 声明与实际角色相符；AcceptancePaths.OriginalDll 默认取实际 PresentationCore 同目录官方文件，角色 actual 已证该位置。RuntimePackAsset 加 Contracts/Native 正式白名单未改。不声称独立完成 NuGet/安装源复制链或包全文件 manifest/hash 全审，亦不另造该审计为此有限回归保留门。

### 内容与时序闭包、默认安全与旧红边界

IL 已核六角色实际三色 consumer，IM 亲读关键 identity、精确 TRX/监督与候选内容/消息/尾部，并按原件索引核六角色及 direct 的阶段存在；不冒充全部业务逐字全审。真实窗口 GetPixel 三色 `255 / 16711680 / 65280`、各帧 event=true，非 producer readback。64 字节 type5 初态 1/1；frame0 内容确认后恰一条 1/0；B 注册时 A/B 各一条 0/0，A 重注册时 A/B 各一条 0/0，共六条；后两帧 A/B StateMessages=0，禁用无新增。RealTransitions=1/Current=0；四 COM refs=0、窗口销毁/cleanup complete。具体转换在 setup 还是哪次 Present 未更细认证，consumer actual backend 未独立观察；B 在动态转换时未订阅，不能称多监听同时动态变化已测。

IM 已读 PartitionPresentStateTests 全文、IfPartitionRenderStateTests 观测器/guard 与 D3DImageTests.HbSurfaceConsumer 关键完整调用闭包：共享 private helper/观测器默认可编译，独立预期按原始64字节断言；IF/HB TestMethod 仍分别受 IF_PARTITION_RENDER_STATE/HB_SURFACE_CONSUMER 保护。默认无 IK_PARTITION_ROLE 实际取 candidate 并 RunHbConsumer(false,true,output)，有 group official/candidate/official-repeat 独立进程；observePartition 跳过大型历史 fresh desktop 诊断，但真实窗口可见前置、producer更新、consumer三色/event/清理硬断言保留。没有将 IF/HB 探索测试入口放入 default，也未修改 NativeRun、ExitDiagnostics、打包或生产状态机。未以未做的新反射检查冒充默认产物全 API 审计。

IL 补 HW 仅必要有界 sibling：四角色身份/退出与候选关键 callback 注销、保留队列及清理采样可核；callback 进入后 unregister 返回，callbackexit 后保留消息可 drain，禁用后 drain0，windowless 后快照，ownerJoined=true/callbacks=1/windowError=null。IM 非四份386行业务全文全审，其他兄弟也仅计数/监督及必要身份，不冒称全部业务全审。HU 以前 II 证据保持历史，不转记 IK 默认。

旧红只保留 IF 历史 x64 旧候选差异；IK 新 default 测试未实跑旧 DLL，不能称新 default 旧红或 x86 旧红新绿全矩阵。IH 最小合法 S_OK NoDevice→Normal 修复保持，当前无具体撤回证据。不据本回归承诺 Available、完整窗口错误状态机、loss/cache副作用或完整同步状态合同。

IK normal Release build Exit0/119 warnings/0 errors/3.53s 为已有报告记录；IM 未重 build。当前可读最后日志60行是 IK x86 publish 尾段（58-59行输出到 ik-default-state/x86/bin 与 publish），不是可重新认证 normal build 全日志，原截断限制仍保留，不称完整诊断审计。

### 下一唯一合法门与总体状态

下一仅在现有正式 ComAcceptance 做合法同步状态合同：先以官方同架构真实有效 HWND/consumer 路径确定同步调用是否以及何时更新 partition 状态、注册快照与消息时序，再据可重现差异决定最小修复和双架构验收。不能把异步1→0结论直接套 SameThreadPresent、不能仅 Render 返回0就改状态，也不预设同步必须广播。IM 不执行该门。

Available / HWbackend / 非零sync / 长期资源 / loss / mediaresources / full四格 / x64Event0 全部开放；完整替换未完成。旧 full 不绑定本次 Native/test SHA；不需要为这次文档终态再发布/沙箱/full。

核读范围登记：规则全文27行、完整方案120行分段读完；IL handoff 顶部及三共享最新历史受影响节已读，三共享其余数百/千行历史未全量复读且原样保留。报告原15行全文先读，补本终态后全文分段回读；三共享仅最新 IM 受改节与最后补记写后回读。最后补记（IM）：本报告和三共享终态均须完成写后读取才交付；此补记不构成新运行证据。

## 以下为 IK 原阶段记录（历史保留）

## 实施与运行进行中

已实现默认新组和无 role 实际 candidate 入口。共享私有消费核心与独立状态观测器默认编译，旧 IF/HB 测试入口仍 guard，不引旧档依赖。精确断言完整64字节type5、初态1/1、frame0一条1/0、B注册与重注册四条0/0、后两帧A/B零条、事件和四refs0。默认动态场景不重复大型fresh desktop诊断，真实窗口可见前置和三色consumer硬断言保留。

正常Release build Exit0、119 warnings/0 errors、3.53s；初次日志前73866字符截断，不称全诊断审计。随后仅新await改ConfigureAwait(false)，双架构默认self-contained10.0.13发布成功，无IF/HB符号，无NoWarn/关闭分析器。上传前反射确认新组与角色方法存在、默认D3D API无IF/HB探索入口；aot.sha256亲读符合IH。独有artifacts/ik-default-state/{x64,x86}/obj/bin/publish/com-acceptance-package，仅package上传，readback分离。现有正式RuntimePackAsset/Contracts/Native白名单未改。

已实跑x64 task6185fdc6e5914b11b30a3c76d4714481、x86 taska9960230c2074fc0a7d0c2b290fb66df，两主TRX新组及无role candidate均Passed、监督Exit0，无timeout/kill/drain失败；三个独立角色及业务仍继续亲读，不以摘要代核心。x64 testSHA253881259FFA69E987AFF0EB5D74CF01ABA3200F13D16F9D7CE192E3B2934C8F/MVIDcb759b13-0c14-46ff-a121-d2e7073cc2da；x86 0A8D7A5574146D26A293B8190B681ED8232C5E6CCA43CD879ABFD987CC600BB1/MVIDd5dd13f1-0520-4c5c-9de3-6f41de83b077。Native未重发。旧DLL新测试尚未实跑，不称新默认旧红。

## 开始阶段（历史）

已接手 IJ 后续默认回归门。保留已验收 IH 生产有限修复，生产 Native 不重发。首先核读执行规则、完整替换方案、IF/HB 依赖及默认 HY/ListenerBroadcast 角色框架，再实施仅必要共享 helper 与默认产品测试，并以正式独立进程双架构验证。尚无本轮构建、发布或运行结果，不继承历史 Green 作为本轮通过。

约束：不新增 Host/工程，不改生产状态机、NativeRun/ExitDiagnostics 或打包配置；显式 IF/HB 诊断仍隔离；默认无 role 必须实际运行 candidate。显示前置失败或首次原生崩溃停止扩大并保留证据。完整替换仍未完成。
