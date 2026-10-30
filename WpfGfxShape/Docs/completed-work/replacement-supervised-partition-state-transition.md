# IC：partition 状态转换官方合同验证

## 最终裁决：真实状态未变，双架构广播缺陷成立；不留生产补丁

保留IA初态快照修复，不恢复HY target HRESULT映射；IC生产修改0、NativeAOT发布0。先读执行规则/完整方案及IA/IB与HW/HX报告，最新handoff只读受影响节，历史不覆盖。

## 官方合同审计
composition.cpp:45–47初始NoDevice；1243–1289新加入监听集合时AddRef、读取同partition字段old==new、NotifyHelper广播全部已登记者。重复enable不发送；disable移除并Release。2225–2289状态变化才发送old/new，lost/released使cache失效，occluded另调度。
RenderingStatusFromHr:885–910把S_OK映射Normal、S_PRESENT_OCCLUDED映射Occluded、其它失效映射NoDevice；但调用层不能省略。rendertargetmanager.cpp:409–450仅失败Render调用状态处理，成功Render不构成Normal依据；615–655仅TYPE_HWNDRENDERTARGET的Present经过HandlePresentErrors（468–474无条件处理状态）。无target/WIC软件bitmap成功不能冒充Normal。这里是HWND呈现路径，不等于必须真正HW设备；软件HWND呈现也不能伪装HW消费。本轮没有证明一种可靠合法的非显示真实状态变更，故选择唯一同partition双listener有限矩阵，状态转换未覆盖，不制造环境错误或loss。
官方connectioncontext.cpp:588–646以source channel复用pCompDevice并AddRef；apifunc.cpp:228–260将真实源channel转为内部句柄，支持同partition证明。候选SameThreadChannel.cs:117–135只向新增channel投递快照，是待核广播差异；_deviceState只有NoDevice初始化，无动态更新。

## 实施与执行
新增Tests/WpfGfxShape.ComAcceptance/IcPartitionStateTests.cs，仅IC_PARTITION_STATE编译。两真实message-only HWND/两个真实channel，B由A作为source创建；合法命令3，逐stage flush、两队列drain、窗口barrier。独立C# ABI和64字节原始数据，不引用生产状态模型/硬编码1/1追绿。窗口wake与backchannel消息分开记录，窗口注销后仍观察队列；监听中销毁B后再注册A。无target命令/无Event0。
正式ComAcceptance自包含.NET10.0.13正常依赖/分析器，独有artifacts/ic-partition-state/{x64,x86}/obj/bin/com-acceptance-package，显式publish/{arch}。clean白名单由既有Package.targets生成；只上传com-acceptance-package，运行回收独立readback；未改打包项目，不上传publish或历史父目录。复用IA Native候选。
- x64 task c7326c5073374edc86e954d79dc0f959，父1执行1失败，监督Exit2；official11044/candidate9968。
- x86 task 5fe48015961042b5b7a25820ffb7a250，父1执行1失败，监督Exit2；official2752/candidate4560。两架构监督均exited、OutputCollected=true、TimedOut/Cancelled/KillAttempted/OutputDrainTimedOut=false、SupervisorError=null。四role各1执行1通过/Exit0，timedOut=false；role Passed仅采集与清理，不是差分通过。
- 精确父filter FullyQualifiedName~WhenIcPartitionGroupRunsThenSiblingBroadcastMatchesOfficialAsync；角色filter FullyQualifiedName~WhenIcPartitionRoleRunsThenTwoListenersRecordSnapshots。监督120秒，外层180秒。父/role TRX均由MTP实际生成，路径见各supervision/exit.json绝对task子路径，不补造工具Exit。
## 双架构逐操作合同（A=channel0，B=channel1）
|操作|官方消息目的地|候选消息目的地|
|---|---|---|
|A-enable|A|A|
|B-enable|A、B|B|
|B-repeat-enable|无|无|
|A-disable / A-repeat-disable / B-disable|均无|均无|
|B-reenable-alone|B|B|
|A-reenable-both|A、B|A|
|A-disable-before-windowless-broadcast|无|无|
|A-enable-B-windowless|A、B（B无窗口）|A|
|B-disable-final|无|无|
|B-enable-before-destroy|A、B|B|
|A-disable-after-B-destroy|无|无|
|A-enable-after-B-destroy|A|A|
|A-disable-final|无|无|
每架构官方11条、候选7条。全部实际保存64字节，type5@0、reserved0@4、old1@8/new1@12、16–63全0，完整hex逐条保存在business observations；64为输入/保存长度，peek retrieved是BOOL不是字节数。没有0/0或不同old/new，没有target命令；**实际状态始终NoDevice，状态转换及渲染后注册未覆盖**。这是静态快照广播差异，不是Normal/HW/Available通过。
窗口证据：x64官方HWND121505024/44172908、候选21693624/25887564；x86官方10487176/102958568、候选103024104/1180700。消息33105/33106各自对应窗口，w/l=0。B-enable官方两个wake/两个队列，候选仅B wake/队列；必要偏序send→wake与flush→barrier→drain，wake和flush之间不要求全序。官方window-unregister后A再enable仅A wake却两backchannel消息，故窗口callback不等于payload消息数。窗口注销不是partition disable；没有任意迟到或无限未来exactlyonce结论。

## 实际身份与核心回读登记
证据根 artifacts/ic-partition-state/readback/{x64,x86}。父TRX为Results/ic-{arch}/TestResults/child.trx；四role的Results/ic-roles/{official,candidate}/{role.trx,exit.json,business.json}。两父/四role TRX、两supervision及四exit已读；business身份、全部observations原始消息、同partition source与窗口首段、B-enable偏序、windowless与销毁后/cleanup核心段已亲读。四business其余中段重复send/peek事件未逐项全文核验，不冒全文审计；官方1170行/候选1073行各架构保留。源码及API离线核已确认IC类型及两个方法确实编入两显式包，非零测试；默认build产物查无IC公开类型。
- actual candidate x64 SHA531E096706C3E3AC4E9AE84C6A0158354382FC2B97A9018CF689B64289B09913；x86 SHA21B396AE6701CFD0E7D036B63ABE8308EDD5555874582E194D5930EA765EDD60。
- actual official x64 AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1；x86 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834，均10,0,1326,50504 commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。官方源码是仓库设计依据，不冒servicing逐行同源。
- test x64 SHA5DAEE22BF8F10C1F120A6D858FAABB69F768B88DB5F5DBF060CB1E1804E6705A/MVID0667b24e-1d60-4db3-b2e0-c46410367223；x86 SHA3DB877147A7396D06C8C58B40B3CDA8C7044A1B46B61453934746B8BF54087BF/MVIDfb14e6e6-8d86-4821-b189-2f6ae7754f63。runtime.NET10.0.13，X64/8、X86/4。
- 四角色B监听中destroy成功，后A重新enable成功；最终destroy/disconnect/deinitialize均HRESULT0、cleaned/joined=true/windowError=null。官方AddRef/Release只由源码解释，不声称直接测得ref归零或长期资源收敛。目标实际消费为0（本轮本就无target），没有HW消费证据。

## 最小修复评估与最终源码
本轮不改生产，IA初态快照与既有COM/fill原样保留；不恢复HY映射。最小可证修复是RegisterNotifications新加入时把同一真实partition快照发送给全部_notifications，而非只给新增channel；重复enable/disable、窗口唤醒与关闭锁不得顺带改。当前两架构旧红已成立，但本轮没有为新生产SHA执行双架构默认HY/通知/注销/关闭兄弟门，故不留下未核生产行为，也不宣称已修。
下一唯一有依据生产门：把该广播旧红收为默认正式回归→最小广播修复→新双架构actual SHA旧红新绿及初态HY/通知锁/HW有限注销/关闭必要兄弟→normal default build。动态状态更新仍须另有合法HWND Present/真实状态控制与渲染后快照实证，不能从单target HR猜聚合。
最终normal default build已执行，Exit0、104warnings/0errors、3.38秒，分析器开启；IC显式诊断未编入默认公开类型。测试源码自最后修改后已两架构发布实跑，未在实跑后改诊断。包全集合/逐文件hash与RuntimePackAsset求值未独立审完，不声称完整包审计。未改NativeRun/ExitDiagnostics/DRDS/VisualCache/historical/公共捕获/项目打包/CF CX CY。真实HWtarget/nonzeroSyncModeStatus/Available/长期资源/loss/mediaresources/full四格/x64Event0继续开放，完整替换未完成。

最后补记（IC）：本报告全文及handoff/session-summary/issues最新IC受改节已写后回读；保留隔离诊断与双架构旧红，无本轮生产补丁。未逐项全文的历史/重复事件及包审计范围如上登记。

## 历史进行中快照
报告先创建、handoff先置IC进行中；当时生产/构建/发布/运行均0。后仅新增诊断，两次正式测试发布及两架构各一次沙箱；未重跑显示/full/COM。
