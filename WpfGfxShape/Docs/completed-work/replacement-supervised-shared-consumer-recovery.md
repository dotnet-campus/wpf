# HV 旧共享 consumer 重验证与最小合同迁移

## 终态：原 native 前置失败，迁移未验收并已精确撤回

本轮实际执行了共享 VisualCache 普通 surface 合同迁移、正式发布及唯一旧 official HistoricalGeometry 任务，不是只跑 HB。原 native 真内容前置失败，按约定立即停止，不扩大矩阵。最终默认源码不保留本轮未验收行为。生产修复0、生产重发0；完整替换未完成。

## 入口与历史边界

先创建本报告并前置 handoff HV进行中；执行规则全文、完整方案分段、最新 handoff、HK终态/HG适用性、HD相关合同/内容及HE身份段、HR报告HT有效关联段和当前VisualCache/historical/HB关键实现已读取。长历史返回被截断的部分不计全文审计，不声称审完HD四格全部原件。本轮不覆盖这些历史。

HT唯一task0c1cdf3ad01e45e0bed28f5d51f47c00固定HM首红Present0/fresh，Dxg166真实HWND/PID/TID桥接成立；没有历史occluded原因证据，也不证明本轮旧D3D9前置可用。没有追加ETW、正锚重复或COM变体。

## 实际最小迁移与适用性

Historical官方geometry方法仍直接进入RunVisualCacheConsumer，cache=true/geometryConsumer=true；默认共享分支生效，不是独有HB绿色替代。原D3D9 Direct3DCreate9、DISCARD/count1/32×16/Format0及expected00563412不改，native前置在connect之前。

只对!wicSource && !configurationRecovery && callbackAction<0 && !transactionRecovery && !multipleDevices分支迁移：partition initialize/deinitialize、connect0、target flags12、窗口合法DPI context映射及实际DPI/96、setup flush及initial invalidate、初始化两图与mutation各独立DuplicateHandle/Present separate batch/commit/5秒event wait/flush。原事件由夹具持有，duplicate交命令不由测试补关，不冒native exact-once-close。WIC/事务/回调/配置/多设备SameThreadPresent合同不机械异步化。保留geometry、sampler、shader、颜色、后态、caller release和cleanup所有原断言。HB/HA生产修复只读。

迁移编辑13项，运行失败后13项逐项逆向恢复，私有HvPresent/HvFlush及DPI声明全部移除，没有整文件覆盖并行修改。VisualCache恢复535行，搜索HvPresent/HvFlush/asyncConsumer/partitionInitialized/surfaceImages无残留；未复跑恢复后的危险同步Event0。Historical未修改。

## 正式发布与包

正常分析器x64 Release/win-x64 self-contained10.0.13发布一次成功，无NoWarn/禁规则。独有artifacts/hv-shared-consumer/{obj,bin,publish,com-acceptance-package}，显式绝对PublishDir，正常依赖图及既有PackageSelfContainedComAcceptance clean白名单目标。复用HT包内HA DLL，不重发生产。

亲读aot.sha256为8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0；runtimeconfig NETCore及WindowsDesktop均10.0.13。419行package-files仅核头尾，MTP/Contracts/runtime/Native条目存在；没有独立全量集合/逐文件hash审计，不冒完整clean集合验证。上传仅新com-acceptance-package，非PublishDir/回收父；readback另置。撤回后不再publish覆盖迁移编译snapshot。

## 唯一实跑及完整TRX

Task **97cfbd69e48a460392bfa9d31e7dac65**，filter FullyQualifiedName~WhenHistoricalCaptureOfficialGeometryRunsThenExpectedUpdatesRemain；同正式exe --supervise-exit，filtered120秒/外180秒。

绝对TRX：C:\CodingAgentSandbox\Tasks\97cfbd69e48a460392bfa9d31e7dac65\Results\hv-x64-official\TestResults\child.trx。69行分两段全读，ResultSummary Failed，total1/executed1/passed0/failed1/notExecuted0/timeout0/aborted0；RunInfo为Exit2。监督非MTP，不造父TRX。

监督39行全读：PID7148/child9952，UTC14:04:29.5910959–14:04:32.0291207；Reason exited、子Exit2/0x00000002；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，HasExited/OutputCollected=true，SupervisorError=null，stderr空。外工具仅返回执行完成及回收路径，没有独立数值工具Exit；与子Exit严格分开。不是native crash或部署缺项，不追加纠正或重跑。

## 内容与实际身份

失败是原native AssertCombinedPixel：expected COLORREF00563412，window/screen actual001D1C1D。PresentNativeControl与DiagnoseNativePresent返回阶段均到达；实际swapchain32×16/Format22/count1/SwapEffect1/windowed1/DPI96，HWND0x11C004A，ownerPID9952/TID10756、测试提交TID6968。前台/cover均自身，visible1/iconic0/cloaked0，Default桌面/session1 Active；cooperative=0。不能据这些状态猜occlusion原因。

stages24行全读：到AssertCombinedPixel before后进入finally，没有connect before、partition、channel、async consumer Present或completion事件。cleanup complete到达；原Release未输出最终refs，不能报refs0。消费者初始/更新帧、实际backend、Available回调、duplicate关闭、目标后态均未覆盖。**没有进入consumer，不能声称旧E_HANDLE本次不复现或已修复。**

actual官方根wpfgfx_cor3.dll SHA **AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1**，10.0.13-servicing.26504.104 commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。actualtest SHA **3F418C8F98217499D3887DD15928804717DB672C66DFDAACE0AA655CFAEF48F2**，MVID **3a7bad28-9128-4bf9-a699-299b089148eb**，.NET10.0.13/X64。

当前旧夹具没有单独记录actual PresentationCore路径/SHA/MVID；包WindowsDesktop10.0.13不能充actual WPF模块证明，不借HT旧身份回填。本任务候选SHA仅依赖/PresentCondition文件身份，未actual consumer加载。x86既定HA SHA75D136F71223DB9EA0830BFBF9CDFC0EAAB748B5067397B682E42323B698EC30本轮未发布/未消费。

## 矩阵、回收及最终源码

x64 official=1任务/1执行/0通过/1失败；x64 candidate=0；x86 official=0/candidate=0；consumer实际覆盖0、兄弟0、full0。首前置失败早停，无四格新通过结论，不绑定旧HD/HE/HF到HV。

回收根：d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\hv-shared-consumer\readback\x64-official。supervision39/stages24/stdout48/stderr空/childTRX69均已读；完整task保留，Diagnostic未全文审。发布的迁移编译snapshot及obj/bin/publish/package保留，没有另造全量源码snapshot。

撤回后默认normal Release/win-x64 ComAcceptance build成功：99 warnings/0 errors，3.19秒，末段57–64亲读。没有关闭分析器，构建不代运行。未修改生产COM/CF/CX/CY/DRDS/NativeRun/ExitDiagnostics/公共窗口捕获/项目打包或并行HW注销测试。HA双架构生产修复及HB保留。

## 下一唯一门

定位并独立证明原旧D3D9 DISCARD native前置00563412真实可见，再重新最小迁移并执行原official HistoricalGeometry；不再泛native矩阵/重跑正锚/追Green。当前没有可证明的单一根因，不能调整expected、删前置、skip/阈值/golden/fallback或跳cleanup。HW backend/loss/mediaresources/full四格、Available及独立x64Event0继续开放。

最后补记：本轮未验收默认补丁已撤回，最终normal build已读；报告及三共享HV节写后回读，不将历史成功冒充本轮验收。保留HW共享节及所有历史。
