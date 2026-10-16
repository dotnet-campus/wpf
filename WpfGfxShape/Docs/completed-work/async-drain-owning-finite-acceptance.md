# Async drain / owning 有限验收收口

## 身份、执行与原物

本轮只收口已有 `UceChannelTests.AsyncDrain.cs` 的证据及文档；未修改生产或扩大测试合同。复用候选12/12时的完整标准MTP已发布包，不重新发布生产/fixture、不改变其身份。无终端或脚本；使用专用沙箱执行一次最终全量。

生产 SHA256：`57CE56EA65057F8F609AD56E318B459A4248790C1231B5909DDD4DEA6E0A6F74`。候选定向与最终full的identity及逐case实际DLL SHA均已核对。原版测试的aot-identity仍记录候选输入，不能冒充原版身份：原版逐case JSON实际加载 `C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\10.0.1\wpfgfx_cor3.dll`，SHA `94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`；不替代目标10.0.12。

以下路径均相对于工作区 `.coding-agent/sandbox-results/`：

| 运行 | Results目录 | TRX / RunID | 实际结果 |
|---|---|---|---|
| 已发布候选定向 | `ee3e5074a48b4057a6bd5d1511af5bd4/Results` | async-candidate.trx / `5e9843d0-a1c1-4326-a69a-5a2cbfca553b` | 12/12 |
| 已发布系统原版正常协议 | `9972bdbe04e54af49f601915a33bca57/Results` | async-original.trx / `d898973b-1e5d-495a-9ff4-2867e974ebcf` | 6/6 |
| 本轮最终fixture full，一次 | `503b50cf41f744e1bdb62b1b383025ae/Results` | async-drain-final-full.trx / `527b6f33-8185-46d2-8f86-9807f5f14619` | 1616 total / 1583 pass / 33 fail / 0 skip |
| 最终完整解析审计 | `b5e306b4ab704f97b8a79f6a516cf808/Results` | audit.json、summary.json、drain-summary.json | 8 TRX / 180 JSON完整解析；AddedFailures=[] / RemovedFailures=[] |

全量运行使用候选定向任务回收包为sourceDirectory，执行 `WpfGfxShape.ComAcceptance.exe`，参数为 `--results-directory Results --report-trx --report-trx-filename async-drain-final-full.trx`；同任务Results完整回收。实际运行win-x64 / .NET10.0.1 / MSTest4.0.1，汇总状态Failed。full分母直接读最终TRX Counters，不以旧1594或静态1598推算。Results继承有旧async-candidate.trx，审计明确按文件名选择新full，旧文件不计作又一次full。

复用既有TypedLifetimeEvidence程序完整解析原物并保存每份JSON的内容与SHA、TRX每项结果/错误。首次审计因full目录含继承TRX而Single匹配歧义；改为精确选择最终TRX后成功，未重跑fixture。专用publish只用于证据解析器，不是生产发布。最终summary及36条紧凑drain记录已全部当场回读；audit.json保留完整内容，不以截断的原始文本代替完整解析。

## 逐条 finite Closed

真实链为外部callback stream → 系统WIC decoder/frame → CWIC wrapper → BitmapSource transport → channel资源；不是假引用或只清空队列。所有18个独立定向case与最终full的18个对应case都completed=true、disposals=1；候选reentry=0，原版不执行该回调重入（诊断哨兵值不作失败）。

| 状态 | 候选case | 原版case | 有限结论 |
|---|---|---|---|
| 正常，disconnect后显式SyncFlush，再destroy | 0 open / 1 closed / 2 committed | 0/1/2 | flush成功；disconnect/flush后外部stream仍被持有，最后channel关闭释放一次 |
| 正常，disconnect后直接destroy，无显式flush | 6 open / 7 closed / 8 committed | 6/7/8 | destroy自身完成drain，外部stream最后释放一次 |
| 合法完整空source命令触发首错，再传真实WIC，显式flush后关闭 | 3 open / 4 closed / 5 committed | 未运行 | 首次及后续flush为0x88980406，sticky失败不丢最终拥有引用，最终释放一次；仅候选有限证据 |
| 同上失败，disconnect后直接destroy | 9 open / 10 closed / 11 committed | 未运行 | 关闭排空/处置一次，不宣称失败协议原版等价 |
| 外部stream最终Release回调关闭sibling | 全部候选0–11 | 原版显式Destroy.Sibling | 候选回调关闭返回0、最终处置一次，无本次死锁；不泛化任意重入 |
| worker线程释放和OS线程退出 | 候选0/1/2/6/7/8/9/10 | 无退出强断言 | 源码对这些case断言释放线程非调用线程，并在deinitialize后轮询该OS线程不存在；其余失败case不据thread id推定完整worker异常退出 |

`UceChannelTests.AsyncDrain.cs:100–140`定义上述状态与断言。原154/154与owning/generation定向15/15已经有限Closed，不重新领取或相加制造新方法族分母；新full包含当前全部fixture。

## disconnect纠错与 required / 增强边界

原生 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/apifunc.cpp:212–227` 的 `WgxConnection_Disconnect`只检查handle并 `pConnection->Release()`。候选 `Code/WpfGfxShape/Abi/UceChannelExports.cs:112–126`移除connection入口，保留仍有channel时的scheduler，只有无channel时才释放partition manager。由源码与真实disconnect后继续flush/drain共同证明：**nativeDisconnect不是独立cancel/transport取消协议**。此前当前待办中的“必须再证明独立cancel”缺少对应入口依据，从本模块required清单撤去该错误门；不宣称已经实现或测试独立cancel。若其他正式API确有取消合同，按其具名来源单独验收，不转嫁本入口。

正式仍必需：全required/physical及authority关联、同SHA最终strict、完整ABI/消费者/失败逆序清理/引用/线程/callback静止、七阶段与E2E全部既定条件；真实HW/cache/shader及目标架构/配置/持续媒体尚未完成。资源和worker生命周期必须满足已有真实合同，DLL驻留不豁免这些条件。

**未证而不冒称Closed**：真实worker异常退出、COM QI失败、分配OOM、替换中途失败的完整拥有链；本轮空source sticky失败不等同这些异常。本模块没有新增这些注入。它们作为增强故障覆盖/风险保留；只有能绑定既有正式条款、可达入口和原生合同的具体失败路径才进入正式必需差集，不能笼统把任意核内OOM/QI/中途异常设为本有限模块无界阻塞门。失败协议原版对照未取得亦明确保留，不运行已知挂死畸形batch来制造oracle。

## 回归与总任务继续

33项HW/control失败完整保留且失败集合无增减，包括原版正常HW呈现与native可观测control失败；不能全部归环境，不能据原版失败替候选通过，不重复追Green。真实可观测HW会话的正常原版成功前置仍缺，按唯一外部输入页“不已证新增用户硬件包”的边界继续自主核验，不虚构会话成功。

生产未变且本轮仅证据解析/文档变更，GenProto34/34及普通4224 pass / 1 fail / 4225复用前轮实测，**不是本轮新跑**；普通的目标10.0.12 DLL或依赖0x8007007E保留。同SHA primitive/Blur/image/media有限比较沿用原档，不伪新运行。

真正外部缺项以[唯一输入页](../all-completion-external-prerequisites.md)的最新最小差集为准：历史authority绑定（已有生成器/manifest不再索取）、ARM64工具链及真实执行、合法完整10.0.12、官方player协议/ownership/PRERELEASE oracle。未收到新增材料，未下载或安装，不捏造解除。DLL进程驻留、不unload。

**当前async drain/owning有限模块Closed；用户总任务持续，总门Red，无全部完成或替换资格。** 当前页与四短页、矩阵和总计划同步，不留下“full未跑/文档待同步”的假缺项。
