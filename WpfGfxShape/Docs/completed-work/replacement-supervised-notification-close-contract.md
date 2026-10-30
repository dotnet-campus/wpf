# HS 非显示通知 / 异步关闭合同

## HU终态补记：角色隔离后的有限关闭偏序成立，不恢复立即释放要求

本节优先于下方HS历史裁决；历史失败保留，不覆盖、不重跑错误立即断言。生产修改/生产发布0。只改 UceChannelTests.AsyncDrain.cs 与HU文档，未改NativeRun/ExitDiagnostics/项目/打包/HP/HI/VisualCache/DRDS/CF/CX/CY；无终端、脚本、新Host、full、显示或Event0。

### HU设施与所有权审计
规则27行、完整方案120行、正式项目/SDK配置已读。HS报告53行分段已读；共享汇总/问题页仅读取首段后定点插入，未冒全文。SystemWicBitmap84行及MemorySource创建/descriptor根引用机制已读；DecodeFrame在返回frame前释放decoder/factory，调用方在transfer后释放wrapper/frame，仍保留stream跨close。
官方源码 apifunc.cpp662–716：frame不支持IWICBitmap时CreateBitmapFromSource使用WICBitmapNoCache，wrapper持有该bitmap；bitmapres.cpp ProcessSource以ReplaceInterface保留m_pIBitmap，transport引用另释放，析构158行释放m_pIBitmap。因此尚存transport/slave/wrapper/WIC源时仍可合法持stream，caller Release返回值不能当零门。此为所读源码所有权解释，未证明已安装二进制全部缓存/调用闭包。
HS_NOTIFICATION_CLOSE内原六DataRow现在仅启动正式同exe，环境HU_CLOSE_ROLE选择模块/状态；角色filter仅WhenHuCloseRoleRunsThenFinalStreamDisposalIsRecorded，不调用父方法、不递归。六角色各独立PID，不相互加载两模块。正常Release顺序未变；无GC/skip/关并行/异常吞没/资源cap。
原子事件覆盖closeReturn、externalRelease.before/after、callback.enter/exit、siblingDestroy.before/after、deinit.before/after，含阶段/thread/count；各合法阶段用CancellationTokenSource五秒等待callback.exit，超时记false但不改变清理顺序；最终必须exit已观察且disposals恰1。保留callbackOrder>closeReturned，删除立即1与caller线程要求；Release数值只诊断。当前阶段均未超时。

### HU最终源码发布与执行
最后源码编辑已在首次HU发布前完成，HS guard已生效；其后未改源码。两次显式发布均成功，正常分析器、无NoWarn，selfcontained10.0.13；独有artifacts/hu-close-order/{x64,x86}/obj/bin/publish/com-acceptance-package，显式PublishDir。AOT复用HS包中指定HA候选，未重发生产。上传package，回收readback，与源分离；未完成逐项clean白名单集合重算或源回收清理，不能宣称该项验收完成。
BL filtered120秒、外180秒，每架构恰一次，无设施重试。监督均Exit0/exited、非timeout/cancel/kill、OutputCollected true、drainTimeout false、SupervisorError null。工具未另列外层数值exit。
- x64 task ac140f1175de49d98ea9ce3464a7aa75：父6执行6通过0失败0skip；六role各1执行1通过，退出0。官方6/7/8 PID2704/2204/9932；候选10568/6236/1760。
- x86 task 92334e2157bf42498867952ee54561d1：父6执行6通过0失败0skip；六role各1执行1通过，退出0。官方6/7/8 PID3620/2732/1724；候选2256/2396/6060。
- x64 actualtest SHA B40B105B3641DA3FB5F715A1880AE7A75D70FAC723650DD21882EF4C2A5A197B / MVID e390fbdb-d72e-4a04-ab3a-0df3c974d712。
- x86 actualtest SHA 5D73579B0F70089B6C8475DA0F943A8156670E59912C71583DC16467BDDFC72D / MVID 8df3b107-c372-43a9-b4b2-b9ff21eab991。
均实际.NET10.0.13，对应X64/X86。actualPath由Process.Modules BaseAddress匹配加载句柄，非包hash替代。官方task根wpfgfx_cor3.dll：x64 AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，x86 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834；版本10,0,1326,50504/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。候选actual Native/wpfgfx_cor3.dll：x64 8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0，x86 75D136F71223DB9EA0830BFBF9CDFC0EAAB748B5067397B682E42323B698EC30；1.0.0.0。

### HU实际完成阶段、共同必要合同与限制
双架构全部12角色事件一致：closeReturned序号1、closeReturn事件2(count0) → externalRelease.before3(count0) → callback.enter4 → callbackOrder5 → callback.exit6(count1) → externalRelease.after7(count1) → 第一等待8/9完成 → siblingDestroy10/11 → 第二等待12/13完成 → deinit14/15 → 第三等待16/17完成。实际callback阶段均externalRelease，当前隔离样本内同步进入/退出；不是sibling/deinit触发。最终callbackExited1/disposals1/completed true。wrapper/frame Release各1，surviving Release本组均0；这些不是断言门。所有协议HRESULT0。
新观察与HS同进程官方立即0/finally1不同，支持模块状态隔离的重要性，但未证明历史差异唯一根因，也不能抹去历史反例。绝不提升成“所有官方必须同步立即callback”的要求。
可有限确认：合法BMP/WICframe/CWICWrapper/bitmap95的open/closed/committed，外部stream存续跨close不被提前析构；closeReturn后caller释放，再按既有sibling/deinit清理及五秒完成观察，最终恰一次。候选满足这项共同必要偏序和最终次数。仅三状态/各架构一次，不证明长期资源收敛、任意调度、全部通知合同；注销后迟到通知未覆盖。
证据回收根artifacts/hu-close-order/readback/{arch}。绝对父TRX为C:\\CodingAgentSandbox\\Tasks\\<上述task>\\Results\\hu-<arch>\\TestResults\\child.trx；六role绝对TRX为同task\\Results\\hu-roles\\{True,False}-{6,7,8}\\role.trx。两父TRX56行、十二roleTRX26行、十二exit.json、十二业务JSON207/211/215行及两监督39行均分段全文亲读；stdout72行也全读。未逐项读runtime文件/清单，不冒包集合审计。default normal Release build成功99warnings/0errors/1.70秒，非full；未通过API枚举另证默认类型表，源码guard仍在。
下一唯一任务：在既有正式exe内单独构造合法通知注销与在途回调的可控制交错，独立角色记录register/unregister/通知进入退出，先官方确定迟到边界，再候选核必要偏序；不继续追立即同步。有效HWtarget、非零SyncModeStatus、迟到通知、持续在途回调、长期worker/资源循环收敛、full四格/显示loss/mediaresources/x64Event0仍开放。HR/HT历史保留，完整替换未完成。


## 终态：真实新边界已执行，但立即释放假设被官方否证，合同未闭

生产修复0，保留HA Stdcall/fill及指定候选SHA。未改HR/HP/HItrace、NativeRun/ExitDiagnostics、VisualCache或打包工程。没有终端/脚本/第三Host/full/显示/Event0 surface。新增方法最终隔离为HS_NOTIFICATION_CLOSE，默认不引入未经官方支持的立即回调要求。

## 1. 选定合同和已有覆盖

完整替换方案1–120、执行规则27行已读；HO分段读取但部分长段截断，不冒全文。latesthandoff读取开头并保留HR。CompositionNotificationTests已有注册/重复/注销、广播、register队列wait/peek，混有错误输入；不能推出注销后无迟到。PresentNotificationContractYTests读取至150行，真实HWND/type46但零字段target不作为本轮合法HWtarget依据，未运行显示组。AsyncUceChannel已有SW WIC内容及失败包释放/重入；其无效命令分支不代合法传输。

UceChannelTests.AsyncDrain原166行已读：合法2×1 BMP→系统WIC frame→CWICWrapper→bitmap95，已有外部引用先释放再close。新边界反转外部引用顺序：stream保留跨DestroyChannel返回，再caller Release，检查close前0、Release后立即1且反初始化后不重复。官方/候选×open/closed/committed（6/7/8）六用例；没有空consumer、无效指针或surface。

生产审查范围：UceChannelExports175–240的Gate外等待destroy Completion；SameThreadChannel180–275通知队列锁/销毁检查；PartitionScheduler1–65队列所有权。符号索引无结果，文本定位，不冒全部调用引用闭包。

## 2. 发布、任务和实际身份

selfcontained10.0.13正常依赖图，独有artifacts/hs-notification-close/<arch>/obj/bin/publish/com-acceptance-package，显式PublishDir；只上传clean包，不上传历史父/源码。未关闭分析器/NoWarn。首次publish把DLL错选为Native子目录，未上传；枚举纠正为gz-stdcall-fix/publish/<arch>/wpfgfx_cor3.dll。共4次publish，1失败3成功；生产未重发。

全部filter：FullyQualifiedName~WhenAsyncCloseReturnsThenSurvivingExternalStreamReleasesExactlyOnce。BLfiltered120/外180秒。全部监督exited、子Exit2，timeout/cancel/kill/drainTimeout均false，OutputCollected true，SupervisorError null；工具未单列外层数值Exit。

|阶段|task|readback子目录|executed/passed/failed|test SHA / MVID|
|---|---|---|---|---|
|x64初组|72397aab4172480d81ef1795393c74d5|x64|6/3/3|8DAF148647A9418275438B8E92ACB5EE18CB679149EE330DC6A170A773FAAA69 / d1717020-1b1a-4993-8c78-e8224554e533|
|x64夹具纠正一次|64d0243220d14c9db089f586809f0160|x64-corrected|6/3/3|6335579441210CD56D812C7AD8B7242445F6B236CC7D65B2E0695DA86AFD81AC / cde3dda8-fe58-4f93-9e92-e190e601d00f|
|x86正常组|01ab104a6b8e4969920cf9d0ef739154|x86|6/3/3|33B783A401F21FB94ED8E6F64EA047C78AF1854A0020299C05F82933F4F02EBD / bf1ce427-16fd-4c43-8884-ca84ecf06048|

回收根artifacts/hs-notification-close/readback；TRX为各自Results/hs-<arch>/TestResults/child.trx，监督相邻supervision.json，含绝对task TRX参数。每组三个official失败、三个candidate通过；error/timeout/aborted/notExecuted均0。

实际module按Process.Modules BaseAddress匹配NativeLibrary句柄：
- candidate Native/wpfgfx_cor3.dll，x64 SHA8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0；x86 SHA75D136F71223DB9EA0830BFBF9CDFC0EAAB748B5067397B682E42323B698EC30；版本1.0.0.0。
- official各task根wpfgfx_cor3.dll，x64 SHAAD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1；x86 SHA5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834。版本10,0,1326,50504，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；实际.NET10.0.13 X64/X86。

## 3. 轨迹和裁决

初组错误要求IUnknown::Release返回0；official返回1而finally最终已disposals1。返回值不应作精确存活引用合同，纠正一次，保留原值及原失败。不是改golden追绿：纠正后仍失败，停止扩跑。

纠正组双架构全部official在“Release后立即disposals=1”实际0；finally最终1，closeReturned1/callbackOrder2。立即回调假设被否证，不能据此修生产。现有序号不能区分callback相对sibling destroy/deinit的细边界，也不能把finally最终1当作close同步排空。

candidate双架构全部completed true：close前0，Destroy.Drain0，closeReturned1→caller Release→callbackOrder2/disposals1；callback在线程caller执行断言通过；Destroy.Sibling0/Deinitialize0后仍1。真实候选加载成立，但官方不同步满足同一断言，不能宣称兼容合同关闭。

refs原件：wrapper.Release1、frame.Release1，stream.Release official1/candidate0仅诊断；唯一析构证据为callback，不用Release返回值判泄漏。无墙钟全序要求、无资源cap。

## 4. 读回及最后源码

纠正x64/x86共12份业务JSON（各69–85行）全文亲读；三份监督39行全文。最终TRX100行分段读取，x64末段长输出截断，Failed/具体断言/Counters均亲读；初组仅首段及85–100，不冒全文。完整任务目录已回收runtime/Native/Contracts/aot.sha256/清单，但未逐项重算包集合，不冒集合审计完成。

最后把新方法置于HS_NOTIFICATION_CLOSE；实跑二进制在隔离前包含方法。guard后的显式配置未重发/运行（预算停止），不是最终源码矩阵。default Release normal build成功99 warnings/0 errors/1.83秒，分析器正常；非full回归。既有DoNotParallelize未改。

## 5. 下一唯一动作与开放项

下一唯一动作：另轮先补official callback相对外部Release返回、sibling destroy入口/返回、deinit入口/返回的原子偏序和既有5秒callback完成等待，明确合法异步完成点；不要求同步立即回调、不追绿改时限，不猜生产语义。现有同进程分别调用还未满足完整方案的每角色隔离进程；复用正式同exe机制涉及共享角色时先协调。

本轮新覆盖是一个真实反例而非通过合同：close返回后最后外部Release立即callback不能作为官方要求。未确证生产缺陷，关键通知/关闭合同未闭。有效HWtarget、非零SyncModeStatus、注销/迟到消息、持续在途回调、worker/资源循环收敛仍开放。完整替换/full四格/显示Hardwareloss/mediaresources/x64Event0独立线均开放。HR并行节保留。
