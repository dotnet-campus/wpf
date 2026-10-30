# JF/JG：0xA interop 传输引用所有权实际验收

## JG 终态：x86同合同两格通过；同版双架构有限验收成立

JG已停止，未追加第三任务。JF的x64历史两格与JG本次x86两格均为同一JB正常0xA合同、各架构同版10.0.13官方独立对照；只成立固定对象生命周期的有限双架构验收，不代表长期资源无泄漏或完整替换。原JB源码195行已读，原expected/refs/cleanup及成对AddRef/Release查询未改；未发现必须修正的x86硬编码声明。源码/生产/项目/默认隔离修改0，Native重发0，不触碰生产COMpartitionfill/DRDS/NativeRun/ExitDiagnostics/VisualCache历史/公共捕获/CF CX CY。

### JG构建与部署
先一次normal default x86 Release build：Exit0，0warnings/0errors，1.03秒；随后仅一次专属正常分析器Release win-x86 selfcontained10.0.13 publish，Exit0，无NoWarn/禁规则。发布日志60行，已核头尾；接口裁掉前29287字符，未声称完整告警全审。独有根`artifacts/jg-x86-ownership/`，obj/bin/publish均显式隔离，PublishDir显式指定该根publish。正常依赖图无新Host/第三工程，无共享runtime安装。

复用`artifacts/je-x86-isolation/com-acceptance-package/Native/wpfgfx_cor3.dll`，旧包aot与新包aot精确同SHA **63B91E6CD6B5C1E8D0095E1673933DD5BFC539744F7EC36DC06C8DCB074E3136**，候选实际加载身份再次确认。不是HA旧包，不重发生产。只启`JB_WINDOW_SAMPLER_CONTRACT`，离线编译DLL API已核两个精确方法，default DLL API无这两方法。runtimeconfig两includedFrameworks均10.0.13；现行targets白名单包418行manifest首尾已核，包含MTP/runtime/root官方/Native/Contracts/aot。只上传clean `com-acceptance-package`，未上传PublishDir/历史父/回收根；readback/official与candidate分开。无diskfull，剩余空间/包总字节未知，清理0，不声称容量完全恢复。

### JG实际任务、监督及精确TRX
|角色|task|监督/业务PID|真实计数|业务Exit|UTC监督时段|
|---|---|---|---|---|---|
|official|ce9e9447df814be1ada6809113f479f7|11236/9144|1执行1通过，失败/skip0|0|13:14:37.1039483–13:14:38.2954945|
|candidate|3630e1eecc554fa59a5ac6b2ede3fddd|8732/1256|1执行1通过，失败/skip0|0|13:15:53.489792–13:15:54.6890426|

日期2026-10-25。filtered BL120秒/沙箱外180秒，两39行supervision全文已读：TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut均false，HasExited/OutputCollected true，SupervisorError null。沙箱工具仅报告执行完成未提供数值工具Exit，不能拿业务Exit0冒工具码。监督非MTP，无parentTRX。
- official绝对TRX：`C:/CodingAgentSandbox/Tasks/ce9e9447df814be1ada6809113f479f7/Results/jg-official-x86/TestResults/child.trx`
- candidate绝对TRX：`C:/CodingAgentSandbox/Tasks/3630e1eecc554fa59a5ac6b2ede3fddd/Results/jg-candidate-x86/TestResults/child.trx`
- 精确filter分别`FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenJbOfficialInteropTransferIsClearedThenReferencesConverge`、`FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenJbCandidateInteropTransferIsClearedThenReferencesConverge`。官方核心全读通过后才执行候选一次；目录未列TRX但精确直读存在，不误判缺TRX。

### JG实际加载身份
各task根为`C:/CodingAgentSandbox/Tasks/<task>/`，不是包hash替代：official实际根`wpfgfx_cor3.dll` SHA **5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834**；candidate实际`Native/wpfgfx_cor3.dll` SHA **63B91E6CD6B5C1E8D0095E1673933DD5BFC539744F7EC36DC06C8DCB074E3136**。两test实际根`WpfGfxShape.ComAcceptance.dll` SHA **3B84CED39E47F91E51DE4906A9C132A4908707C77196326E1E0770639E81911F**，MVID **a32d2dec-8c9f-4389-8eae-9c619a2cf3c7**；WPF实际根`PresentationCore.dll` SHA **D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D**，MVID **2934bccd-58e4-4afe-8677-ec985888f34e**。两进程.NET10.0.13/X86；WPF/官方10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；test/候选1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a。

### JG引用、事件及cleanup核心原件裁决
两侧identity27行、ledger266行（分三段）、stages38行（分两段）、TRX35行、stdout12行、空stderr全部核心亲读。保存于`artifacts/jg-x86-ownership/readback/{official,candidate}/Results/jg-{official,candidate}-x86/`，不是只读Passed。真实D3D9 HAL device/surface/InteropBitmap各HResult0。

|阶段|official|candidate|
|---|---|---|
|caller baseline临时查询AddRef/Release|2/1|2/1|
|显式传输AddRef|2|2|
|0xA记录/close/commit/flush后查询|3/2|3/2|
|null 0xA同序后查询|2/1|2/1|
|resource97删除HResult/Removed|0/1|0/1|
|删除后close/commit/flush|0/0/0|0/0/0|
|destroy/disconnect/deinit|0/0/0|0/0/0|
|Detach后bitmap/surface/device/factory最终Release|0/0/0/0|0/0/0/0|

三次临时查询均配平；转交引用未被调用方额外Release。正常四对象最终释放后指针归零，DestroyWindow成功后完成标记。计数与官方具体生命周期吻合，未强行推广任意IUnknown Release必须0。Commit/flush不替代引用观测，正常退出不替代cleanup。

未转交duplicate：official原柄1344/副本1352，candidate原柄1332/副本1348。副本只关闭一次，原柄Wait(false)→Set→Wait(true)，原completion独立Dispose；仅证未交付取消，不证已交付event exact-once close。WindowConsumer/Present/NativeDisplayPrecondition/SoftwareFallback均false；存在设备HWND与STA线程不等窗口内容消费。未执行Present/桌面fresh/原sampler/full/COM变体。未接收、已记录未submit、submit未知失败保留且未覆盖，不诱发坏句柄/UAF、不盲补close/doubleRelease；unknown隔离回收不记正常refs0。

### JG阶段历史与末次补记
- 接手即handoff标JG进行中；读规则/方案/最新handoff/JF报告，保留JF/JB/JE历史。
- 小default build通过；一次正式publish与离线API/aot/runtime/白名单核对完成后，handoff更新official阶段。
- official唯一task核心全读，refs1→2→1及四Release0后更新handoff，才允许candidate唯一task。
- candidate唯一task同样全读通过，达到两任务上限即停止，无部署纠正或额外重试。报告及三共享最新受改节写后回读，不以文档更新当新运行。
- 下一唯一门：原窗口合法async sampler/VisualBrush/HardwareOnly/非零duplicate事件交付整合；必须另有本task显示前置通过才验内容，不借本轮无consumer合同冒消费已证。Available/HW恢复/长期资源/media/full四格/旧official sampler AV仍开放，完整替换未完成。

## 以下JF历史原样保留

## 终态：x64官方与候选正常路径各1/1实际通过

0xA持有/解除/最终引用责任已在本次合法正常路径实证。复用JB源码原样，无源码/生产/项目打包修改，无Native重发；未删除或减弱任何引用/cleanup断言。两格均为正式同exe不同任务独立业务进程，非第三Host，不扩本轮矩阵。正常default编译DLL公开API已核：不存在两个JB方法；专属编译DLL存在两方法且真实TRX各执行1，未验收诊断不进入默认发现。JD/JE离屏修复未动。

### 实际任务与证据

本地根 `artifacts/jf-interop-ownership/`（相对WpfGfxShape）。仅`com-acceptance-package`上传，publish/历史/readback未上传。正式obj/bin/publish/package独有；结果在readback/official与readback/candidate分离。包由现行targets白名单产生，419行manifest首尾已读，包含MTP、runtime、Contracts、root官方和Native候选；不是逐419文件手工验签。API离线核已确认精确类型WpfGfxShape.ComAcceptance.D3DImageTests及两个方法。normaldefault build1、专属publish1、沙箱2、official1/1、candidate1/1、full/显示fresh/COM变体/原sampler复跑0。构建发布无diskfull，不代表剩余容量完全恢复，容量/包总字节未知，清理0。

|角色|task|监督/业务PID|TRX计数|业务Exit|监督时段|
|---|---|---|---|---|---|
|official|58e22d7a99d24ced84024f8855ab7695|9676/3924|1执行1通过，失败/skip0|0|13:07:43.6226733–13:07:44.6315557 UTC|
|candidate|30f3ce99e20742608a82da2ac11a16e5|6220/7560|1执行1通过，失败/skip0|0|13:08:27.9432186–13:08:28.9400699 UTC|

两份supervision各39行：filtered120秒，外180秒，无TimedOut/Cancelled/KillAttempted/OutputDrainTimedOut，OutputCollected=true，无SupervisorError。沙箱外工具未给数值退出码，不造工具码。监督非MTP，无parentTRX；业务TRX绝对路径分别：
- `C:/CodingAgentSandbox/Tasks/58e22d7a99d24ced84024f8855ab7695/Results/jf-official-x64/TestResults/child.trx`
- `C:/CodingAgentSandbox/Tasks/30f3ce99e20742608a82da2ac11a16e5/Results/jf-candidate-x64/TestResults/child.trx`

精确filters为`FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenJbOfficialInteropTransferIsClearedThenReferencesConverge`及candidate对应`WhenJbCandidateInteropTransferIsClearedThenReferencesConverge`。监督参数`--supervise-exit Results/jf-{official,candidate}-x64 120 <filter>`，监督将结果根转换为上述任务绝对路径。

### 实际身份（两任务同一正式测试包）

任务根用上述task替换；official实际`<task>/wpfgfx_cor3.dll`，SHA **AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1**；candidate实际`<task>/Native/wpfgfx_cor3.dll`，SHA **0C16A66C898269A17DBDD062012898FEBE338E341FED8D0B7FEC47FB21B6A23A**。拟复用旧IS目录不存在，实际输入准确定位`artifacts/jd-offscreen-isolation/x64/com-acceptance-package/Native/wpfgfx_cor3.dll`，publish生成aot.sha256及运行assembly initializer均核SHA，未用HA旧实现。

两业务实际test`<task>/WpfGfxShape.ComAcceptance.dll` SHA **966B8661399CBA2D492FB67A2B8FF05E64053912E58E2ACB573BAE6044B11FAC**，MVID **c425a692-3341-4e9c-9bbe-34f375b16362**。实际WPF`<task>/PresentationCore.dll` SHA **0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31**，MVID **6cb5dedb-eea3-4832-9a08-c0225e9ff82f**。两进程.NET10.0.13/X64，WPF/官方10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；候选/test版本1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a。非JD共享10.0.1身份，不外推x86。

### 引用及事件原件逐阶段裁决

两任务`jb-ownership/identity.json`27行、`ownershipledger.json`266行、`stages.jsonl`38行、TRX35行、stdout12行与空stderr均核心全文亲读，不以Passed代读。D3D9 HAL device/surface/InteropBitmap create全部HResult0；无Present/无software fallback/无native显示前置/无window consumer（存在device所需HWND与既有STA窗口线程，不等于显示验收）。

|阶段|official|candidate|解释|
|---|---|---|---|
|bitmap caller baseline成对查询|AddRef2/Release1|2/1|诊断临时引用立即配平|
|显式传输AddRef|2|2|唯一交给0xA的额外引用|
|发送/close/commit/flush后成对查询|3/2|3/2|服务端持有+1实测，不由Commit0推定|
|null 0xA更新后成对查询|2/1|2/1|回caller baseline，解除实测|
|resource97删除|HResult0/Removed1|0/1|随后close/commit/flush各0，非单独释放证明|
|destroy/disconnect/deinit|0/0/0|0/0/0|各阶段独立记账|
|Detach→最终bitmap/surface/device/factory Release|0/0/0/0|0/0/0/0|具体测试对象正常最终释放观测|

未将任意IUnknown::Release返回0设为通用COM保证：返回计数通常仅诊断用途，此处仅这组已由同版官方实际验证的具体对象/固定生命周期采用该合同，不能外推聚合/代理/其他实现。baseline、转交后、null后共三组成对引用查询均配平，不额外Release传输引用。最终Release后指针归零，无失败盲补Close/doubleRelease；callback0未作验收依据。

两任务未记录/未交付duplicate均原柄1276/副本1280（跨进程数字相同不是同一对象），关闭副本一次后原柄Wait(false)→Set→Wait(true)，原completion独立Dispose。只证明未发送副本取消，不证明已交付event exact-once close；本轮根本未发Present，不重复DS无surface事件控制。

Send未接收、已记录未submit、submit未知失败仍独立未覆盖，不故意zombie/invalidptr/坏句柄诱发官方UAF。unknown分支仍保留账本/隔离等待监督结束，未进入本次成功路径；OS回收不计正常refs0。Commit/flush与事件/具体资源引用各自独立证据，不将其中之一代替生命周期全绿。

### 下一唯一门与末次补记

本轮两格成功即停止，不追加x86。下一唯一任务为**x86同一JB0xA合同**：复用对应现行IP x86 Native、同版官方独立进程先过再候选，不扩显示/full；通过后才原窗口合法async/VisualBrush/HardwareOnly/非零duplicate整合门，须另有本task显示前置通过才验内容。Available/HW恢复/长期资源/media/full四格/原officialsampler AV均开放，完整替换未完成。完整报告与三共享最新受改节写后回读，旧JB/JE等历史保留。源码本轮未改，最后适用包身份即上述实跑身份。

## 开始阶段历史（非终态计数）
已读执行规则、完整替换方案入口、latest handoff、JB所有权报告及JD/JE收口。优先复用JB显式诊断，先normal default build，再正常分析器x64 Release selfcontained10.0.13正式专属发布，复用IP Native，不重发生产。官方独立进程1格实际合同通过后才候选独立进程1格；不扩x86/full/窗口/DS矩阵。

当前JF build/publish/sandbox/official/candidate均0；无新运行证据。JE曾成功不代表容量完全恢复，实际余量未知。未知转交不补Close/Release，隔离回收不记正常refs0。Release返回值仅按所测具体对象与官方合同解释；flush/Commit及事件与引用阶段独立记录。Available/HW恢复/长期资源/media/full四格/原officialsampler AV仍开放，完整替换未完成。

## 阶段历史
- 已创建本报告；正在核对JB源码、官方0xA语义、正式发布与监督参数。
- normal default x64 Release build Exit0，0warnings/0errors/1.02秒。正式专属publish一次Exit0，正常分析器可见既有告警；日志60行亲读但前29295字符被接口截断，不声称完整日志全审。无diskfull，余量未知。JB源码未改。
- 已读JB全195行与官方d3dimage.cpp172–240：旧引用先Release，新传输AddRef由服务端偷取，null解除；失败和software临时引用独立。AddRef/Release观察成对。已用编译DLL公开API离线确认两个精确方法存在；包manifest419行首尾、aot完整SHA、runtimeconfig双10.0.13已读。目标Native从JD/x64/com-acceptance-package/Native定位，原IS路径不存在，不用旧HA。
- 官方开始前：仅上传独有com-acceptance-package，Results相对根由监督转任务绝对路径，BL120/外180，filter精确限定官方方法。候选尚未启动。
- 官方task58e22d7a99d24ced84024f8855ab7695，监督9676/业务3924：真实1/1 Passed、Exit0，无timeout/cancel/kill/drain。identity27行、ledger266行、stages38行、TRX35行及stdout/stderr全部核心亲读。bitmap成对查询remaining1→2→1；删除resource97/flush/destroy/disconnect/deinit/Detach后bitmap/surface/device/factory最终Release均0，duplicate未交付关闭一次且原柄可用。实际官方AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，.NET/WPF10.0.13/X64；已证该具体正常路径，允许候选下一格。目录列举未显示TRX但指定路径实际读取存在，不记缺TRX。
