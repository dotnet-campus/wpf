# postfailure 最小修正第一验证门：build 与四个定向单元

## 终态

本阶段完成并停止：一次显式正常 Release build 成功，一次定向测试实际执行 4 项全部通过。仅证明托管真实 scheduler 有限合同；不是 Native/COM ABI 修复验收完成，也不是完整替换完成。下一门必须另授权。

本次只使用实际文件、构建、测试工具，无终端、脚本、再委派。Native 发布0、COM发布0、沙箱0、显示/full/x64验证0、重试0。build后生产与测试代码零修改；此报告及当前事项是证据文档，不冒源码尾改后的验证。

## 代码审核与构建前唯一补正

亲读上轮 replacement-postfailure-attach-minimal-fix-static 全文（拆段补读）、恢复入口、执行规则、基线/事项及连续性交接协议；亲读生产三个文件的相关完整路径与 PartitionSchedulerTests 全部130行、生产/测试csproj、隔离props/targets及global.json。SDK声明10.0.107，生产/测试net10.0，nullable启用；不改项目/SDK/依赖/构建规则。

确定问题：Register TryAdd返回false之后，scheduler原无条件Destroy；ReleaseResources原无条件Remove(_channel)，有移除不属于该对象表项的风险，重复Register同一对象失败还会销毁已有有效对象。

仅在原生产范围补正：
- SameThreadChannel：新增内部 IsRegistered、私有setter，仅TryAdd成功后置true；ReleaseResources仅在持有注册时移除表项并置false。
- PartitionScheduler：Register失败仅在对象尚未注册时销毁，避免重复注册失败回滚摧毁已有channel；真实首次注册失败仍回收本对象。
- UceChannelExports、四个单元均未追加编辑。全部生产编辑在唯一build之前完成并回读。

其余审核结论（静态支持，不冒异常路径已运行）：
- CreateChannel先保留全局表项再Submit；Submit未接受时调用方移除本表项/销毁本对象；已接受后失败由worker回滚，再由调用方移除。未接受路径持Gate，Disconnect不能在该临界区改变连接状态，不能顺手Release仍活跃连接引用。
- 异步失败移除后仅Disconnected且该连接已无channel才Release连接持有的一个manager引用；manager按总引用计数到零才Dispose共享scheduler，不因单个partition失败而回收其它连接持有的scheduler。保持原实现。
- Destroy消费/释放批次并真实Destroy；忽略CommitSubmitted返回的既有业务Failure，但执行异常仍映射为本次HR，外部等待路径返回该HR。composition线程重入仍不等待自身队列，返回接受结果，不宣称其完成HR已验。
- 仅Register/Destroy completion按本次管理结果；业务Commit/SyncFlush首Failure及zombie公开映射保持，不清Failure、不新加共享错误。
- 不扩大修复到构造中途OOM、诊断I/O、COM释放异常、重入/竞争架构；不改公开expected/skip/阈值，不改KN resize或显示实现。

## 实际构建账

目标：`WpfGfxShape/Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj`，configuration Release；项目引用同时编译生产。未构建COM、未把publish当build。

- 显式 build 调用1，Exit0，成功；3711 warnings、0 errors；21.52秒。
- 未抑制warning，保留日志所见CA1416、MSTEST0044等诊断，不把高warning数说成零警告，也不擅自修其它问题。
- 测试前已亲读构建工具保留日志1–32、33–62。工具记录原stdout共3110989字符，只保留末20000字符，前3090989字符已被工具截去；因此不能声称已亲读全部原始诊断。构建汇总3711/0及Exit0明确可见。

## 实际测试账与断言边界

一次现有测试调用，目标同上，filter：`FullyQualifiedName~WpfGfxShape.Tests.PartitionSchedulerTests`。类名与四个TestMethod均由源码确定，未跑full/发现/追跑。

现有入口无configuration/no-build参数；实际日志明确产物为 `artifacts/bin/WpfGfxShape.Tests/Debug/net10.0/WpfGfxShape.Tests.dll`。故计数必须为**显式Release build 1 + 测试内隐式默认Debug构建1 + 测试调用1**，不能声称只发生一次编译或四项在Release下运行。

实际结果：Exit0；匹配1个测试文件；4 passed、0 failed、0 skipped、total4；33ms。日志无逐方法明细，以下名称来自已读源码；类filter及实际总数4与源码四项一致，不捏造逐项TRX/testId/架构。

|测试方法（类前缀 WpfGfxShape.Tests.PartitionSchedulerTests）|源码断言边界|
|---|---|
|WhenFailedPartitionRegistersLateChannelThenRegistrationSucceedsAsync|真实未知wire命令经router产生88980406后，late Register completion为0|
|WhenLateRegistrationSucceedsThenFlushRetainsFailureAsync|late注册不清原失败，late flush等于已确认88980406|
|WhenSourceIsDestroyedThenSharedChannelSurvivesWithFailureAsync|source Destroy completion0，late flush保88980406，共享registry仍有1项|
|WhenFailedPartitionClosesThenIndependentPartitionRemainsHealthyAsync|同scheduler独立partition在失败partition关闭后flush0|

FailAsync另断言Send/CloseBatch为0及首次flush为88980406；非直接写Failure、非mock、非结果副本。finally清理返回值未逐项断言；重复内部Register/TryAdd失败所有权、真实注册异常回滚、公开ABI/Native资源闭合不在这四项覆盖内。WaitAsync5秒不证明Dispose.Join永不阻塞。

测试日志亲读1–30、31–61；原stdout1555903字符，工具仅保留末20000字符，前1535903字符截去。保留段有CA1416/MSTEST0044 warning，隐式构建warning总数不可从现有保留日志确定；无独立build error汇总，但成功生成测试程序集并实际4项通过、Exit0。不得套用Release的3711到Debug，也不虚报Debug零warnings。未指定TRX记录器，未声称本轮有TRX明细。

## 身份与不可外推边界

本轮未生成新Native身份或认证新PE/SHA。旧candidate20B3/40D0未包含本轮改动。原postfailure official通过/candidate20B3 Create88980406/output0、unknownRegistration=true、cleaned=false红证据继续有效保留，不能用本轮单元冲销。历史isolation、direct/public/WGC/KS/KN和Ledger全部分账保持；未改历史静态报告或红原件。显示冻结、KN修复保留，完整替换Open。

业务修复仅作为待原生验收候选；仍需新Native x86 + 原official/candidate postfailure复验 + isolation有限回归通过后才可保留，不宣称已修复完成。

## 下一唯一门：另授权 x86 新产物有限原生复验

沿用静态报告§下一唯一执行计划的原公开合同，不扩矩阵。所需预算/产物（本轮均未执行）：

1. 从当前冻结源码发布 `Code/WpfGfxShape/WpfGfxShape.csproj` Release/win-x86 NativeAOT ≤1，独立新输出目录；交付新 `wpfgfx_cor3.dll` 的实际SHA256、PE x86身份与发布日志，绝不复用20B3/40D0认证。
2. 正常COM Release/win-x86 build≤1（本轮只构建Tests及生产，未构建COM），COM self-contained publish≤1；显式AotDllPath绑定新Native、AotRuntimeIdentifier=win-x86。独立新publish/clean，按现有白名单/缺件/manifest/根摘要规则交付test SHA/MVID、runtime与实际Native身份。不得为方便改包规则/SDK/项目。
3. 原postfailure两角色各≤1：先 `WpfGfxShape.ComAcceptance.UceChannelTests.WhenOriginalFailedPartitionAcceptsLateSharedChannelThenSourceClosePreservesFailure` 全部通过，才 `WpfGfxShape.ComAcceptance.UceChannelTests.WhenFailedPartitionAcceptsLateSharedChannelThenSourceClosePreservesFailure`。Create0/真实非零L、两次L flush88980406、真实Destroy与全部正常清理必须保持原断言。
4. 再原isolation两角色各≤1：先 `WpfGfxShape.ComAcceptance.UceChannelTests.WhenOriginalAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives` 通过，才 `WpfGfxShape.ComAcceptance.UceChannelTests.WhenAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives`；同一新clean，精确FQN等值过滤。
5. 最多4个沙箱角色体，每个内120秒/外180秒；失败即停，不修后重试。交付完整发布/build日志、actual identity、TRX、business JSON及监督/清理证据，逐段回读；无发现/full/x64/显示/KN17/额外错误矩阵。新单元重跑预算0，已有本门成功账不重复消耗。

以上预算须由主代理取得另授权；本执行子智能体已收尾，不自动发布/进入沙箱。
