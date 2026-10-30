# IV 进程隔离合法 official sampler 验证

## 初始进行中记录（历史）

先读取执行规则、完整方案、IU/IR/IT/DS合同及当前夹具和官方所有权实现，再决定合法场景能否发布及唯一实跑。仅现有 ComAcceptance 显式 IV_ISOLATED_SAMPLER；默认及生产不改。禁止旧同步 Event0 输入，不将新场景称为旧 IS 精确复现。

## 初始实际范围（历史，后续阶段见下文）

已读当前执行规则27行、current-work-item31行、IU及handoff前段（超长返回截断部分尚待分段读取）。尚未构建、发布或执行；official sampler任务0/1。尚未证明消费者内容、资源收敛或任何修复。

## 所有权设计门

必须区分调用者持有、已记录未提交、Commit转交但处理/丢弃未知、ProcessPresent确认/flush和关闭。Commit0不是处理完成。未知副本不能凭S_OK关闭；未知转交路径由现有监督结束整个自有子进程，OS回收必须记为非正常资源收敛，不计业务refs0。

## 关键设计与发布阶段补记

新增全文件 IV_ISOLATED_SAMPLER 的 D3DImageTests.IvIsolatedSampler.cs，仅新测试入口，不改共享夹具/生产/NativeRun/ExitDiagnostics/打包。独立24字节0xA、16字节Present、148字节VisualBrush、122字节effect、324字节shader及原92/72字节窗口协议，全部实际发送先落盘。原始event归调用者、副本归接收执行者；0xA单独AddRef转交、调用者保留原引用。记录前调用者拥有；进入Send调用即保守标记不确定（失败也可能已有录入），不擅关副本。正常须完成event+flush+真实内容；失败/超时保留event及COM持有账本，等待现有120秒监督结束子进程，不作业务refs0。正常Detach、解绑root/effect/data、末event、逐资源delete/flush、destroy/disconnect/deinit、逐COM refs0、原柄/window关闭。

已用正常分析器Release x64 selfcontained10.0.13发布独有obj/bin/publish/package，Exit0；日志60行全读，可见输出前26503字符截断，不声称全警告审计。包按原targets白名单重建，只含依赖；候选复用IS包Native（IP既有产物），不重发生产。离线公开API已确认目标方法存在；NativeRun源码确认--supervise-exit filtered角色；包runtimeconfig双framework10.0.13、Contracts两原件、运行依赖清单已核。FH未启用，不为取证增加危险变体。

## 唯一实际运行及清理阶段终态

任务5f9bc6bef1314af3904b182d12e53839，监督10536/子10756，UTC2026-10-25T08:56:49.9042624—08:56:52.9310151；filtered内120/外180秒。子Exit2/0x00000002，Reason exited；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut全部false，OutputCollected=true，SupervisorError=null。一次任务预算已用，不追加变体、前置正锚或重跑。

**原生显示前置失败，sampler内容根本未执行。** initialize0、D3D device0、原PresentNativeControl/DiagnoseNativePresent后DwmFlush0，原expected00563412、actual00F0F0F0。停止在connect之前；没有async channel、资源、event副本、0xA AddRef或sendbuffer。ledger Resources/Transfers均空，目录无bin文件是尚未Send，不冒已采sampler bytes。stdout真实1 failed/0 succeeded/0 skipped仅框架文本汇总；不是完成TRX亲读计数。没有AV/FH异常地址，不与旧IS第一次坏写绑定。

原始回收根（以下相对它）：d:\\lindexi\\Code\\dotnetcampus\\WpfLab\\WpfGfxShape\\WpfGfxShape\\artifacts\\iv-isolated-sampler\\readback\\Results\\iv-x64。supervision.json39行、iv-sampler/identity.json28行、ledger.json47行、stages.jsonl6行、stdout34行、stderr空已全部亲读。诊断前段读有长行截断，不称全文。预期绝对TRX C:\\CodingAgentSandbox\\Tasks\\5f9bc6bef1314af3904b182d12e53839\\Results\\iv-x64\\TestResults\\child.trx；stdout声称产生，但实际回收目录没有TestResults，递归名称查询无child.trx，未亲读完成TRX，不造TRX counters。

actual official根wpfgfx_cor3.dll SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1 / 10.0.13-servicing.26504.104 commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。candidate仅包依赖Native SHA0C16A66C898269A17DBDD062012898FEBE338E341FED8D0B7FEC47FB21B6A23A，不认该任务实际执行候选业务。testSHA B0211D283FF5F05CD51B3710F7B5D7734CDD5C4C5199E0BA887C5355E7B53104，MVID1e6dfe2b-7f35-4049-9757-e5ea41f3ffd6；X64/.NET10.0.13。actual PresentationCore SHA0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31，MVID6cb5dedb-eea3-4832-9a08-c0225e9ff82f，同10.0.13/4099f839。

清理实际只到前置失败分支：deinitialize0、device/d3d Release、窗口Destroy成功、无原event待关；阶段记录最终original events and window closed。该失败分支未记录Release返回refs，不认refs0。没有未知转交或监督kill，不能把OS回收套为正常sampler资源收敛。正常delete/flush/refcounts和未知转交监督回收均只是新夹具设计，**未实跑验收**；显式文件不得默认迁移。

最后default正常分析器Release/win-x64 build Exit0、117warnings/0errors、2.96秒。未设置NoWarn；普通增量，不是clean/full，日志64行只回读55–64，保留可见日志截断边界。新文件全受IV符号保护，默认不发现新增测试，共享VisualCache/Historical/SharedConsumerLifetime及生产既有修复均未改。未重发Native、未改DRDS/公共捕获/项目打包。

## 下一唯一最小门

先定位原生D3D9 Present→同一HWND GetPixel仍F0F0F0的显示/捕获前置链，不修改sampler参数/expected，不依据本次改生产COMpartitionfill。还需单独解决stdout宣称TRX产生但回收缺文件的证据完整性。前置修复获批后才有下一轮新预算；本轮不再启动native。新夹具的成功资源路径和未知转交保护尚未验证，不能声称已经安全验收全部合法协议。旧源码/旧原始IS包不再是新current输入的阻断条件。

最后补记IV：本报告与handoff/session-summary/issues最新节写后回读；唯一任务为前置失败而非sampler复现，历史范围保留。仅新增显式测试和文档，不交付默认迁移。

## 范围

保留 VisualBrush 两输入 shader 和 expected 0000FF80 / 00FF0080；不用geometry或纯色替代sampler。Available/HWbackend/loss/mediaresources/full四格仍开，完整替换未完成。
