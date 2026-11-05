# CM：CK整数实验验收与所有权边界收口

## 1. 裁决及代码状态

**CK最后发布包双架构有限算法控制通过；无新增生产挂接，完整替换未完成。** CM执行、监督者仅监督。先读规则27行、完整方案120行、CK初稿、CI82行、CL87行、CH59行；长返回截断处补读。已授权补正CK过时初稿。仅专用工具与Windows沙箱正式C# exe，无终端/直接间接脚本、新Host、full或desktop重跑。

亲读SoftwareFillBezier183行、CkFillIntegerTests120行和csproj。生产目录代码是**未接入实现**：Code范围文本查找仅声明，NativeShapeReader24–75仍共享SoftwareBezierFlattener→transform.Apply，没有整数消费入口。ComAcceptance通过Compile Link直接编译同一实现；默认7个oracle和3个舍入DataRow没有skip/放宽阈值。本轮未改任何代码、CF三文件、NativeShapeReader或stroke，未重新发布生产/测试。复用CK最后正式包；未另取构建溯源清单或独立反编译认证源码与包逐字节对应，证据绑定下列实际测试身份，不造全仓修订hash。

## 2. 实际运行、退出与TRX

来源 `artifacts/ck-fill-integer/tests-{x64,x86}/com-acceptance-package`。现有exe参数为 `--supervise-exit Results/cm-ck-<arch> 120 FullyQualifiedName~CkFillIntegerTests`；未改BL监督，内部Path.GetFullPath固定子results绝对目录。完整任务回收至 `artifacts/cm-integer-ownership/{x64,x86}/`。

|架构|task|total/executed/passed/failed|退出|
|---|---|---|---|
|x64|1b773132806d4a749087456fba3c2080|10/10/10/0|0 / 0x00000000|
|x86|44924e3a4a2d415eb23ca3320966d340|10/10/10/0|0 / 0x00000000|

两份supervision各38行、stdout各12行及父TRX各80行均分段全文亲读；Reason exited、HasExited/OutputCollected true，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut false，SupervisorError null。TRX Completed，error/timeout/aborted/notExecuted均0，非stdout替代TRX。

远端精确父TRX：
- `C:\CodingAgentSandbox\Tasks\1b773132806d4a749087456fba3c2080\Results\cm-ck-x64\TestResults\child.trx`
- `C:\CodingAgentSandbox\Tasks\44924e3a4a2d415eb23ca3320966d340\Results\cm-ck-x86\TestResults\child.trx`
本地精确对应 `artifacts/cm-integer-ownership/x64/Results/cm-ck-x64/TestResults/child.trx`、`artifacts/cm-integer-ownership/x86/Results/cm-ck-x86/TestResults/child.trx`，此次无父TRX缺口；不反向补认CI/历史x86缺失TRX。

## 3. 实际身份与独立范围

.NET10.0.13，MSTest4.0.1，X64/8及X86/4，software RenderTargetBitmap，hardwareVerified=false。监督身份与各业务样本身份一致：
- x64测试SHA `950CF353047F5C710DB71B96187DC579583CF001953D0280C5B285397B60C50D`，MVID `10df52bf-a1eb-4059-85cb-e373dab5b9f1`。
- x86测试SHA `8F089C4C3070CA9451FE7C909885A8E2AAF6F6BDC7E1ECDCFEB3FDC5CDEF1D10`，MVID `4bdf53ae-139a-4d20-af9f-b0c6b8f3474a`。
- 实际加载官方模块均为各task根 `wpfgfx_cor3.dll`，版本 `10,0,1326,50504 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。x64 SHA `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`；x86 SHA `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`。
- 两份aot-identity.json亲读的Native候选输入SHA为x64 `0BDE710F6A91E7A9ACE744AF3B162B012C31370C865B1C271B780F29A232ED13`、x86 `95638A310F0B73B8DC9F4A8192B43A93B849F68B9B6B57095F6DBBFE84F4F464`。这是包输入记录，**不是CK实际加载候选消费路径身份**，不能把官方模块或oracle通过冒充候选DLL通过，也不把CH旧候选SHA自动移绑该包。

14份 `Results/ck-fill-integer/oracle/{0..6}/evidence.json` 的结果/身份末节全部直接读精确路径：differences=threshold=0，cubicNonempty/linesNonempty=true，40×32/stride160。原始两缓冲和controls/trace随全任务回收；本轮没有逐字节独立重算全部缓冲，也没有全文读完全部trace，不能声称完整业务轨迹公式证明。

样本覆盖窄曲线、平缓步长、负坐标及负半格控制点、较大控制点、正负1/32坐标、零曲率终止、wide大曲线。TRX独立确认-1.5→-1、-0.5→0、0.5→1三个舍入断言均通过。默认新增oracle只支持这些有界算法控制，不证明父设备矩阵、普通Path本地float阶段、clip优化、全范围溢出或生产闭包。

## 4. 官方算法与CL独立边界确认

亲读官方bezier.cpp180–250/264–320及495–630、bezier.h250–320、aarasterizer.cpp2945–2975、real.h680–708：28.4转换先float平移-0.5再scale16，half integers shifted up；narrow输出后先终止，wide先重建low再推进high、low步进后输出。CK终止保护和无clip入口与所读有界合同相容；未完成全部公式、宏与边界审计，不外推普遍正确。

独立读CL全文及官方clientchannel.cpp550–580/773–825、connection.cpp310–335、connectioncontext.cpp350–385、samethreadcomposition.cpp104–137、composition.cpp475–512：Submit失败可已消费并delete B，Commit的IFC失败跳过closed槽置NULL与Reset；Destroy→SyncFlush→SynchronizeChannel→Commit可再次GetFreeIndex解引用该悬空槽。**当前仓库同线程源码UAF路径确定；再次delete仅条件性后继，非已经观测到double-free。** 候选SameThreadChannel404–455先TryDequeue/DetachBatches换队列，非同构；本轮不修候选。

缺BN历史原始模块、匹配servicing源码/编译配置及dump/首次free与第二次use地址栈；CL记录的BN E_HANDLE/0xC0000374仅按报告引用。当前官方同SHA实际加载记录也不能证明仓库源码与servicing编译一致。不能认定已证BN崩溃根因、首次堆损坏位置或排除早先ABI/资源破坏。CL宏/其他路径审计按CL报告引用，不冒称CM全部原件亲核。

## 5. 交付与下一步

五文档：本报告、补正CK、next-session-handoff、session-summary、issues-and-plan-status；历史保留。文档回读只做收口，不能代替替换验收。

下一步基于本次通过的当前算法，增加父设备矩阵及普通本地float分阶段验证，再做真实fill入口限定挂接；同程序集旧/新/官方repeat零阈值非空对照并保留stroke完整输出无新增回归。不能因BU/旧归档丢失停工，不能整覆盖CF三文件。CF有限修复保留，nested/stroke既有差不判本轮新增回归。

原44/10、nested/stroke、BD旧x64 full1740/1699/41 Exit2及x86缺父完成TRX保持历史，不绑CK身份；GPU交付、真实loss恢复新色、持续媒体、长期CPU/GPU资源/线程/callback关闭、notification target同步/非零SyncModeStatus/迟到消息、圆角、合法大shader、同步Event0/E_HANDLE与首次堆损坏均开放。最后稳定源码Tests/ComAcceptance×x86/x64 Release完整适用四格仍缺。**有限算法验收通过，生产替换验收未完成。**
