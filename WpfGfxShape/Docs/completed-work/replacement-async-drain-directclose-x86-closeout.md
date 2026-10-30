# AsyncDrain directClose：x86 C6/C8/O6/O8 有限门最终收尾

## 终态与预算分账

**x86 四项有限合同通过，收尾完成，停止执行。** 本接续先查artifact，确认O8已实际执行且原件已回收，未因上一委派Function failed重跑。本接续新增沙箱/测试体/发现/源码/项目/build/publish/Native/终端脚本/再委派/x64/full/显示执行全部0，仅补文档及取证说明。

|独立账|项 / UID|task|结果|
|---|---|---|---|
|既有canary沙箱1/体1|C6 / 1d757743-a95d-86bc-ad1a-035db6190dac|12d73d1e9c5b4c8f810c2cd86a3496ad|既有报告有限通过，本接续引用，不重跑|
|剩余授权第1沙箱/体|C8 / 19a3b855-228b-8fad-a67a-cfae47a754f2|a1b2a7ee9cfd4e7d82373fafbe847cb9|TRX1/1/1、同UID Passed、会话0，有限通过|
|剩余授权第2沙箱/体|O6 / 1e4c35fb-589a-8de1-838e-4845fd1028f4|e77273616fbf4a81a1aae9709b1890a1|TRX1/1/1、同UID Passed、会话0，有限通过|
|剩余授权第3沙箱/体|O8 / 173013d6-ba45-853f-8195-7f9c30f57a50|b0315153cd4749f18fff9243d89f87b0|既有执行确认，TRX1/1/1、同UID Passed、会话0，有限通过|

剩余门累计沙箱3/体3，无重试，授权已用完；与C6原canary1独立，不能写本接续新增4次。既有发现x86/x64各1、体0继续独立，不把adapter内部Found1822算新发现任务或1822个执行体。所有历史账保留，KN17、旧full、旧候选SHA及共享所有权账不冲销、不迁移。完整替换仍Open，整体不升，显示冻结。

## 原件与亲读范围

路径均相对工作区。C6证据根 `WpfGfxShape/artifacts/async-drain-directclose-canary-x86/`；本接续亲读[既有C6报告](replacement-async-drain-directclose-canary-x86-closeout.md)1–59，引用其完整认证，没有重新亲读C6原件或将其声明为本次新执行。

剩余三项证据根 `WpfGfxShape/artifacts/async-drain-directclose-x86-remaining/`（下称R）：

- 执行前授权：`R/pre-execution-budget.json`全文1–22；`c8-invocation.json`、`o6-invocation.json`为既有调用记录，`c8-readback.md`、`o6-readback.md`为前次即时回读记录，均已亲读。三份`c8-tool-return.txt`/`o6-tool-return.txt`/`o8-tool-return.txt`原可见返回全文均已亲读并原样保留乱码，不重写原件。
- C8：`R/readback/c8/Results/asyncdrain-c8-x86/child.trx`1–26；`Results/async-wic-drain-False-8-False.json`1–126；`Results/aot-identity.json`单行；`Results/asyncdrain-c8-x86/Diagnostic/log_261028013445334.diag`1–65。
- O6：`R/readback/o6/Results/asyncdrain-o6-x86/child.trx`1–26；`Results/async-wic-drain-True-6-False.json`1–132；`Results/aot-identity.json`单行；`Results/asyncdrain-o6-x86/Diagnostic/log_261028013718500.diag`1–65。
- O8：`R/readback/o8/Results/asyncdrain-o8-x86/child.trx`1–26；`Results/async-wic-drain-True-8-False.json`1–140；`Results/aot-identity.json`单行；`Results/asyncdrain-o8-x86/Diagnostic/log_261028014216692.diag`1–65。

以上C8/O6/O8原件**本接续全部亲读**，不只引用前次摘要。业务分1–80和81–末行，diag分1–25/26–48/49–60/61–65；O8初读49–65截断后已补齐49–60与61–65。三项均唯一准确UID/参数体，diag49 InProgress、56同UID Passed terminal、63报告完成、65会话exit0；TRX executionId不是UID。会话分别de495cff-b5aa-41a2-a1bd-a1704e9056b2、2617d5ad-5135-4701-aa78-08988822aec7、4346a690-9d7c-4c69-9e45-95af680a6174。

目录初查O8原件和工具返回已存在，但没有单独O8 invocation。本次补 `R/o8-invocation.json`，明确为**收尾时事后核对记录，非即时原调用**：实参由identity/diag证实；source、外180秒及全任务回收设置引用执行前预算和授权，不伪造缺失原调用对象。`R/o8-readback.md`为本次亲读结论。既有完整工具返回保持原貌；不能补称本次即时归档或新调用。

## 实际入口与身份

三项沿现有 `WpfGfxShape/artifacts/async-shared-ownership-x86-r1/clean`、正式 `WpfGfxShape.ComAcceptance.exe`，普通单UID入口；参数为`--filter-uid UID --report-trx --report-trx-filename child.trx --results-directory Results/asyncdrain-LABEL-x86 --diagnostic --diagnostic-synchronous-write --diagnostic-output-directory Results/asyncdrain-LABEL-x86/Diagnostic --no-progress`，LABEL各c8/o6/o8。外180秒，完整任务分别回收到R/readback/LABEL；无supervise-exit/FQN整组/list-tests，没有为补退出证据重跑。

三项business runtime均.NET10.0.13 / X86，test SHA `D8A5F57FE4A61CC28459A89DCB50DC75E0ADE80E1D0AEDC35D7E18AC5069A7BB`，MVID `e207b681-4c15-453e-b0a4-a1690fa3db78`，identity辅助pointer4，与既有C6绑定一致。

- C8 path=actualPath为其task下`Native/wpfgfx_cor3.dll`，candidate actual SHA `15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620`，version1.0.0.0。
- O6/O8各自path=actualPath为本task根`wpfgfx_cor3.dll`，official actual SHA均 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`，version `10,0,1326,50504 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。

actual依据既有实现NativeLibrary.Load handle与ProcessModule.BaseAddress相等定位路径，再hash磁盘文件；没有handle数值/GetModuleFileName或内存映像hash证明。官方两项的aot-identity15A545只是候选预检，不能替代official实际5EBD身份。未重新计算整个发布包hash。

## 真实生命周期与有限认证

三项均retainExternal=false、completed=true。C8/O8是健康committed directClose，O6是健康open directClose；6/8是场景编码而非队列状态枚举数。

|项|真实callback与caller|顺序/关闭与清理|
|---|---|---|
|C6（引用既有报告）|disposals1/callbackExited1，release2464非caller8112|callback enter1/exit3、closeReturned4/closeReturn5、deinit6/7；Destroy.Drain0/Callback.DestroySibling0/Deinitialize0|
|C8（本次原件亲读）|disposals1/callbackExited1，release5688非caller4260|enter1/callbackOrder2/exit3、closeReturned4/closeReturn5、deinit6/7；Destroy.Drain0/Callback.DestroySibling0/Deinitialize0|
|O6（本次原件亲读）|disposals1/callbackExited1，release168非caller9244|closeReturned1/closeReturn2、enter3/callbackOrder4/exit5、caller sibling6/7、deinit8/9；Destroy.Drain0/Destroy.Sibling0/Deinitialize0|
|O8（本次原件亲读）|disposals1/callbackExited1，release6812非caller11368|closeReturned1/closeReturn2、enter3/callbackOrder4/exit5、caller sibling6/7、deinit8/9；Destroy.Drain0/Destroy.Sibling0/Deinitialize0|

C8/O8 CloseBatch0/Commit0；三项Initialize、Connect.Async、Channel、Sibling、Create.Bitmap、Wrap、Transfer、Disconnect.ReferenceOnly及其所有正常result均0。每项三次Release.External references1是引用数而非HRESULT。externalReleaseReferences4294967295为未走分支哨兵；O6/O8 reentry=-2147483648也是未执行哨兵，不当失败HRESULT。候选reentry0由真实Callback.DestroySibling0支持，官方不在callback重入，caller关闭sibling只是独立控制，**不是候选重入oracle**。

本接续亲读源码`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/UceChannelTests.AsyncDrain.cs`210–255：候选健康分支Deinitialize后5秒内worker退出断言在completed=true之前。C8准确Passed/terminal/完整终态支持该次实际通过已有worker断言；C6按原报告引用。没有独立worker.exit事件/退出耗时，不认证所有线程。官方分支不执行该断言，**不认证candidate worker退出，也不宣称官方同worker退出等价**。finally忽略HR兜底不作为正常清理证明。

## 退出边界与唯一下一建议

三项TRX准确1/1/1 + 完整业务/identity + 同UID成功terminal + MTP会话exit0，满足既定有限合同认证。工具返回均没有独立OS数值exit，无supervision.json/监督退出证明，也没有分离stdout/stderr供独立认证；仅亲读已保存的合并可见返回，不能声称stderr空、OS Exit0、监督Exit0或全进程无异常。工具乱码原样保留，不靠重跑补件。

本轮x86门已收尾，无文档待办。唯一下一建议为**另授权x64同C6/C8/O6/O8四个单UID独立任务**，使用现有x64 clean和独立身份核验，首错停；本次不执行、不授予预算。x64四体仍NotRun。只关闭x86此有限关闭/真实callback/候选重入及该worker退出切片，非长期资源趋势、像素消费、HW/resize/real-loss/media/full；五大完整维度及完整替换仍Open，显示冻结与所有历史保持。
