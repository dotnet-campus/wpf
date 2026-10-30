# HK 共享 consumer 适用合同迁移与旧场景定向验证

## 进行中：执行入口
已读取执行规则、最新 handoff、HG/HH 与 HI/HJ 报告相关开头；完整方案及相关报告后续节仍待回读。历史结论不覆盖。

本轮先核 HistoricalGeometry/Sampler 实际 SwapEffect/Present 合同；HJ task80257012a4c640019c6eaf83b1b34024 的 DISCARD fresh 成功是已有证据，FLIPEX INVALIDCALL 仅另一诊断控制，不自动成为迁移门。HB 独有实现与 HA 生产 Stdcall 修复只读，候选沿用指定双架构 SHA，不重建。

仅普通 surface consumer 按适用性最小迁移，保留全部旧内容、状态、cleanup 断言；WIC/配置/事务/多设备的 SameThreadPresent 合同不机械替换。旧危险 Event0/E_HANDLE 官方提交不直接复跑。

运行门：迁移后精确旧 HistoricalGeometry filter，正常分析器 x64 self-contained 10.0.13 official 一次；同 task 原生前置若不 fresh 即停止、不归候选。仅其全部原断言/cleanup/refs通过才依次 candidate x64、official/candidate x86；各格最多一次，确切部署缺项允许一次纠正。未验收行为必须精确撤回并正常 default build。

## 适用性及迁移阶段
源码已核 Historical controls 两个 official 方法均进入 RunVisualCacheConsumer；原生 device 为 Direct3DCreate9/SwapEffect1 DISCARD/count1/32×16/Format0，PresentNativeControl 使用普通 Present，非 FLIPEX/PresentEx。本轮 FLIPEX 不适用，INVALIDCALL 登记待查但不改不跑。
仅 VisualCache 普通 surface 分支迁移 partition initialize、connect0、target flags12/context/实际DPI、setup flush及invalidate、initial两图和mutation独立DuplicateHandle/单独batch/send/commit/event wait/flush；WIC/configuration/callback/transaction/multipleDevices仍同步。所有原geometry/shader/颜色/后态与cleanup路径保留；HB只读。新增私有助手，无公共抽象。
正常分析器x64 Release self-contained10.0.13发布一次成功，独有hk-shared-consumer/{obj,bin,publish,com-acceptance-package}，显式PublishDir；aot.sha256亲读为8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0，复用HI保存HA候选。runtimeconfig双framework10.0.13及MTP清单头已读。即将唯一official filter FullyQualifiedName~WhenHistoricalCaptureOfficialGeometryRunsThenExpectedUpdatesRemain，BL120/外180；原native expected00563412不改且前置失败立即止于connect之前。
## 实际运行及停止阶段
唯一task **0432ac5ad9df4e78b41573f06a62edea**。监督PID2864/child2056，UTC11:11:00.382301–11:11:02.2537596，filtered120秒/外180秒。Reason exited，子Exit2/0x00000002，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，HasExited/OutputCollected=true，SupervisorError=null，stderr空。外工具无独立数值Exit；监督非MTP，不造父TRX。
精确filter实际执行 WhenHistoricalCaptureOfficialGeometryRunsThenExpectedUpdatesRemain 一次。绝对child TRX为 C:\CodingAgentSandbox\Tasks\0432ac5ad9df4e78b41573f06a62edea\Results\hk-x64-official\TestResults\child.trx；69行已分段全读，ResultSummary Failed，total1/executed1/passed0/failed1/notExecuted0/timeout0/aborted0，RunInfo Error exit2。不是native crash或部署缺项，不追加纠正或重跑。

原native前置失败：Clear FF123456，普通D3D9 slot17 Present返回0（现有Assert.AreEqual(0,result)通过，之后CompleteNativePresent返回；没有另行序列化数字HR，非consumer Present）。原expected COLORREF00563412，window与screen实际00CDB38B。实际swapchain32×16/Format22/count1/SwapEffect1/windowed1/DPI96，HWND **0xAC0516**，owner10068/测试提交线程7384；screen56,48，client32×16，foreground/cover均自身，visible1/iconic0/cloaked0，Default/session1 Active。没有本次独立fresh duplication证据，不拿HJ成功替代本task前置。DiagnoseNativePresent后AssertCombinedPixel明确失败，24行stages无connect before、无partition/async/Present consumer事件，cleanup complete到达。未进入consumer，不能归候选或声称Event0/E_HANDLE本轮不复现。

### actual身份与覆盖边界
actual官方任务根wpfgfx_cor3.dll SHA **AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1**，10.0.13-servicing.26504.104 commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699，.NET10.0.13/X64。测试SHA **5C19F1367487F82A39225962DCCDD40ABDC76D1DD8B7BE56BA886061D80C3BDB**，MVID **07a7457d-19b0-46e2-915e-09d01735cad0**。runtimeconfig双framework10.0.13是部署声明，actual PresentationCore路径/SHA/MVID本次旧夹具没有记录，不回填HJ数值冒actual。
候选仅包依赖SHA8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0，未actual消费。x86既定75D136F71223DB9EA0830BFBF9CDFC0EAAB748B5067397B682E42323B698EC30未发布/运行。consumer内容/事件/refs/后续mutation均未覆盖；device/window原cleanup阶段完整到达，但原Release不输出最终refs，不能报refs0。Available/duplicate native exact-once-close/actualconsumerbackend均未证。

## 精确撤回及最终默认验证
本轮14项迁移编辑逐项反向恢复，新增私有助手块全部移除，VisualCache恢复535行；原connect1/Event0 Present/完整geometry与sampler/shader/后态/cleanup及diagnostic保留。搜索asyncConsumer/SharedConsumer/surfaceImages无残留。未直接复跑恢复后的危险官方旧提交。HB独有实现、HA生产Stdcall修复、NativeRun/ExitDiagnostics/DRDS/公共捕获WindowQ/项目打包/CF/CX/CY均未改；Historical文件未改。
撤回后正常default分析器Release/win-x64 ComAcceptance build已新跑成功：**98 warnings/0 errors，2.27秒**，日志57–64亲读；未NoWarn/关闭规则。构建不代运行，未验收共享行为最终不保留。
正常发布1次/正式task1；x64 official=1失败于native前置，x64 candidate=0，x86 official/candidate=0，related historical consumer=0，full=0，FLIPEX=0。未扩大或随机追绿。

### 原件、snapshot与审计范围
根 d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\hk-shared-consumer 保留独有obj/bin/publish/clean com-acceptance-package以及整task readback/x64-official，input/output分离，未上传历史父目录。正式包保留本轮迁移已编译snapshot（上述test SHA/MVID）及Native、Contracts、MTP、runtime、aot.sha256；撤回未重新publish覆盖它。没有另造源码全量snapshot；迁移明细与逐项撤回操作可追溯，不能把编译snapshot称源码snapshot。
监督39/stages24/stdout48/stderr空/childTRX69行已全读；stdout29长行单独返回可见，诊断Observation关联71fec76e2a5c4f92a70ceaa215b16ea8。Diagnostic大日志及所有包文件字节未全审，不冒完整包审计。package-files419行只核头、runtimeconfig与aot.sha256已全读。历史仅执行规则全文、方案主要节及结尾、HG/HH相关节、HI/HJ相关节、HDHEHF报告相关证据节及HB核心实现；长返回截断未补全部分不计全历史审计。

## 终态与下一唯一门
**迁移已尝试、唯一原official场景前置停止、共享行为已撤回；完整替换未完成。** DISCARD适用，FLIPEX INVALIDCALL登记待查但不作本轮门。下一唯一门是定位旧D3D9 DISCARD native前置为何Present0/GPU完成却无预期00563412（HJ另D3D9Ex DISCARD成功不等于旧前置已修）；保留原内容断言，只在前置真实通过后重新迁移并运行同一旧official consumer，不新增泛native模式矩阵或重跑HB代旧验收。Available/HWbackend/loss/media/resources通知/full四格/独立x64Event0继续开放。

