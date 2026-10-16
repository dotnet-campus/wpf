# GenProto native-only packet 有限执行审计

## 本次实际交付

通过文件内容查找定位 `CMilVisual::ProcessCreate` 到 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/node.cpp:415`，实际读取实现为 RIP / E_UNEXPECTED。修复 NativeOnlyExecutionAuditTests 错误的 resources/visual.cpp 引用与 consumer 标签；未改原生源、生产、required、golden 或 skip。

最新测试源码已实际构建并专项、全量运行。首轮 full 的 archive 检查发现先前 wirecompiler 报告绑定旧测试 assembly；保留失败 TRX，重新运行真实 wirecompiler 测量生成当前 assembly 报告后 full 通过，未修改校验规则。最终 full 后再次专项读取原始 Results 并重建报告，无源码尾改。

身份和短数字集中在[唯一基线](../handoffs/current-stable-baseline.md)。[独立机器报告](../../Investigations/GenProtoEvidence/nativeonly-execution/report.json)、[verification](../../Investigations/GenProtoEvidence/nativeonly-execution/verification.json)及[有限 Ledger](../../Ledger/nativeonly-packet-execution-finite-acceptance.json)已落盘并回读。测试完整解析 baseline/existing/target/full Results 下全部 JSON 和 TRX，而非仅摘要；保全每份 raw SHA256。identity.WorkingDirectory 与全部 TRX TestMethod codeBase 同任务绑定，报告记录 taskDirectory、RunID、identity hash、TRX hash。原始四目录不改写。本次没有再 publish 或再跑 COM full；full 原始失败逐名记录，不归零、不全归环境。

## 八项 writer → consumer → 实际断言

以下是候选 AOT 的真实执行与来源映射，绝不冒充原版原生 packet trace；原生来源在报告 sources 中独立哈希。

| native-only ID | 写包 / 字段 | consumer / 真实断言 | 有限裁决 |
|---|---|---|---|
| MILCMD_BITMAP_SOURCE | apifunc SendCommandBitmapSource / UceChannelExports.SendBitmapSource；Type,Handle,Bitmap | bitmapres ProcessUpdate / SameThreadChannel.Commit；TransferredBitmapChanges 的像素因果、TypedBitmapBatchStops 的失败前缀保留与最终 stream 释放 | 候选 packet 执行已证；合法 WIC owning 路径，不是 fake probe |
| MILCMD_CHANNEL_CREATERESOURCE | htmaster / CreateOrAddRef → WriteChannelCreateResource；Type,Handle,resType | GeneratedProtocolProductionContext.CreateResource；GenerationOwnership 创建 Bitmap95 后通过该 typed handle 传输 | 候选 writer/consumer 已证 |
| MILCMD_CHANNEL_DELETERESOURCE | htmaster / Release → WriteChannelDeleteResource；Type,Handle,resType | GeneratedProtocolProductionContext.DeleteResource；MasterNonfinal deleted=0，MasterFinal=1，DeleteResourceReleasesFinalGeneration；别名独立持有 | 候选 writer/consumer 及实际最终释放已证 |
| MILCMD_CHANNEL_DUPLICATEHANDLE | htmaster / DuplicateTo → WriteChannelDuplicateHandle；Type,Original,TargetChannel,Duplicate | GeneratedProtocolProductionContext.DuplicateHandle；GenerationOwnership 别名替换、旧 stream disposal、两种关闭顺序 | 候选 writer/consumer 已证 |
| MILCMD_MEDIAPLAYER | apifunc / 公开 SendCommandMedia → SendMedia；Type,Handle,Media,NotifyUceDirect | videoslave ProcessUpdate / managed video resource + 真实媒体 provider；SolidFrame 解码颜色、caller release、替换、target lock 失败重试 | 真实像素成立；失败/discard 最后 media 引用计数未独立观测，不能标完整 owning |
| MILCMD_TRANSPORT_SYNCFLUSH | 原生 connection SynchronizeChannel 有 packet | composition Transport_SyncFlush；候选直接 commit/barrier，router default success 不是实际 transport 消费断言 | API 功能层已测；packet 执行未证 |
| MILCMD_TRANSPORT_DESTROYRESOURCESONCHANNEL | 原生 connectioncontext CloseChannel 有 cmdDestroy | composition Transport_DestroyResourcesOnChannel；候选 State.Destroy / scheduler destroy；stream 释放证明 API 清理 | API 功能层已测；packet-specific 执行未证 |
| MILCMD_VISUAL_CREATE | 冻结声明存在；限定来源未找到合法 typed producer | node.cpp CMilVisual::ProcessCreate RIP / E_UNEXPECTED | 不支持分类；不强造 producer，不用 ordinary create 顶替 |

## 有限满足与正式剩余

childSatisfied=true 仅指完整八项审计及上述候选有限执行归档；parentBlocked=true、stage4Closed=false。原版原生 packet trace 未证；native-only 不造 managed 对应；DBG MilCmdValidateStructureOrder 不进入 retail 分母。x86 CLR 与先前 Shared 生命周期、enum/padding 结构映射保持既有有限裁决，不重领。

正式 required 按 ID 保留 `EVID-GENPROTO-MILCMD-CLOSURE`、`EVID-GENPROTO-MILCMD-REGEN-DIFF`；已满足 consumer declarations 不降级。该两 ID 下本模块未证仅为上述 media failure/discard 最终引用、两项 transport packet 真实执行与完整物理/历史绑定，不能以 API 功能等价或 hash 顶替；不新增无界故障门。

历史 authority binding、ARM64 实际工具链/环境、完整合法 WindowsDesktop、官方 player protocol/ownership/oracle 及可观测 HW 会话仍依[唯一外部页](../all-completion-external-prerequisites.md)验收。七阶段、A/B/C、独立 E2E 和用户全目标持续，总门 Red。生产未改，DLL 进程驻留无 unload。
