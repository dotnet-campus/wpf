# Command：SyncFlush close 首错有限收口（2026-10-14）

## 实际交付

`Abi/UceChannelExports.cs` 的 SyncFlush 原先忽略 CloseBatch HRESULT，partial command 下可能继续 Commit/Submit 并误报成功；现在遇 close 失败立即返回，不提交未完成批次。`Tests/WpfGfxShape.PublicAbi/CommandContract.cs` 增加公开 `MilComposition_SyncFlush` partial 断言。

原生依据：`core/uce/clientchannel.cpp` 520–550 CloseBatch 对 open command 返回 MISSINGENDCOMMAND；812–843 SyncFlush 传播 SynchronizeChannel 错误。本轮相关原生阅读未超过400行；未改冻结生成代码、私有 ABI、导出分母或 stub。

## 本轮验收

生产、PublicAbi、COM 包 Release publish 成功。生产 SHA：`4052EB085EDF404F4B8964033CCD524B4399E1279813B508AB95B78D5BABD27E`。

| 条款 | 裁决 |
|---|---|
| Begin/Append/End/GetRefCount 真实实现及有限公开调用 | 已实现，沿用 FE7D6A 基线；本轮公开生命周期再次通过 |
| SyncFlush partial/CloseBatch 首错向调用者传播 | Closed：原版/候选同步4轮、异步4轮均返回 0x88980411 |
| partial 错误后 Append/End，关闭/提交/释放/重建 | 有限 Closed：同一公开生命周期后续成功；不推广到服务端 malformed/zombie 恢复 |
| 真实分段命令→Commit→SyncFlush→render→固定像素因果 | Open：当前 bitmap clear/readback 不构成证明 |
| resource handle/ref 中途失败、传输引用清理及关闭无泄漏 | Open：尚缺成组公开证据，不以生命周期通过代替 |
| 本模块剩余真实资源 def 导出 | Open：SendCommandBitmapSource/SendCommandMedia 仍缺；本轮缺项未减少 |

## 已回读证据（相对 WpfGfxShape）

- 原版 `Investigations/Command-close-reference/Results/Results/cc0e9956a8d546b1b03d73156ecfc837/report.json`：10.0.1，SHA `94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`，Passed/LifecyclePassed=true。候选与原版 harness SHA 均 `1CF620A99863003E5E22C00B4E37B92F2E52E58C21B4B86A43A22645D7F7738F`。
- 候选 `Investigations/Command-close-candidate/Results/Results/c452c573eecd49eb804f316a8fd8e6bf/report.json`：生产同SHA；FlushPartial=-2003303407；LifecyclePassed=true；Missing40/Extra0；PeContractPassed/Passed/AllAbiPassed=false。严格失败保留，不把 exit1 改成成功。
- COM `Investigations/Command-close-full/Results/command-close-full.trx`：RunID `446658ec-46be-4186-8a40-0d6072a1df07`，第10866行 **1406总计/1374通过/32失败/0未执行**，exit2。
- 同目录 `aot-identity.json`：生产同SHA，Sandbox=true。32项呈现观测失败保留，未随机重跑追Green。
- 初次 COM 使用含路径的 report filename 未产生 Results，回收失败；纠正为 `--results-directory Results` 后只执行一次有效全量并回读。PublicAbi 回收虽指定 Results，本轮工具实际回收目录含运行依赖，不能声称实际仅回收JSON；有效证据仅上列 Results/Results 报告。

前序 FE7D6A 的 `Command-publicabi-production` RunID `a7487d8f1f0c40a08075c7a85af0cca3` 和 `Command-publicabi-reference` RunID `1d33a561b6114182914c346c95dfb055` 已回读身份；旧 COM RunID `c203af39-946a-46b1-960f-bc6a1003498b` 仅历史，不替代本轮结果。

缺40项完整清单见 [独立差集页](../investigations/command-publicabi-missing40.md)。命令族整体仍 Active，须完成上表 Open 后才进入下一族；未宣称本次完整用户合同已交付或具备替换资格。
