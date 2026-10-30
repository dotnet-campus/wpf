# AsyncDrain directClose：x64 C6/C8/O6/O8 有限门最终收尾

## 终态及预算

**四项均有限通过，收尾完成，停止执行。** 按C6/C8/O6/O8顺序实际执行4个独立Windows沙箱进程、4个精确参数体，无重试；每项即时保存完整可见工具原返回及准确调用对象，再亲读全部结果后才进入下一项。授权4/4用尽。源码/项目/包修改、build、publish、Native、发现、x86重跑、full、显示、终端/脚本、再委派新增均0。adapter内部Found1822不是1822体执行或新增独立发现任务。

|项|准确UID / 参数|task|有限结果|
|---|---|---|---|
|C6|1d757743-a95d-86bc-ad1a-035db6190dac / (6)|19de9d1326ea4787838fd53d1308dfe3|TRX1/1/1、同UID Passed、MTP会话0|
|C8|19a3b855-228b-8fad-a67a-cfae47a754f2 / (8)|02f35e3819bd48e8af839be1a04d09f9|TRX1/1/1、同UID Passed、MTP会话0|
|O6|1e4c35fb-589a-8de1-838e-4845fd1028f4 / (6)|6c30d489262c456b9a0ae7fee1d04afe|TRX1/1/1、同UID Passed、MTP会话0|
|O8|173013d6-ba45-853f-8195-7f9c30f57a50 / (8)|7976f17ad8b04b2b82c776a346b830f6|TRX1/1/1、同UID Passed、MTP会话0|

结合[x86既有四绿](replacement-async-drain-directclose-x86-closeout.md)，双架构此有限切片关闭；x86 C6原canary1与余三项3独立保留，本轮不重跑、不合并为新增8。两架构既有发现各1/体0、两个partition合同、双架构共享ownership、旧KN17/direct/public/WGC/KS/Ledger/HW/media/full及所有旧SHA账独立，不移绑40D0或x86 official5EBD。完整替换Open，整体不升，显示冻结。

## 执行前静态核对

已读最新恢复入口、x86 closeout、discovery closeout/UID manifest及current-execution-rules。直接读取当前clean/aot.sha256为54BC62完整值、runtimeconfig为net10.0及两includedFrameworks10.0.13，定位根正式exe并读既有03-preflight；未重算整个发布包。实际x64发现diag `WpfGfxShape/artifacts/async-drain-directclose-discovery/readback/x64/Results/async-drain-directclose-discovery/Diagnostic/log_261027131030346.diag` 的64–75及100–110已直接读取，四项UID/方法/Int32参数分别66/74/102/110正确，不以x86替代。

执行前快照 `WpfGfxShape/artifacts/async-drain-directclose-x64/pre-execution-budget.json` 固定case、分类、顺序、身份、参数模板、停止条件与预算。沿现有 `WpfGfxShape/artifacts/async-shared-ownership-x64-r1/clean`，根 `WpfGfxShape.ComAcceptance.exe`。普通入口参数准确为：`--filter-uid UID --report-trx --report-trx-filename child.trx --results-directory Results/asyncdrain-LABEL-x64 --diagnostic --diagnostic-synchronous-write --diagnostic-output-directory Results/asyncdrain-LABEL-x64/Diagnostic --no-progress`，LABEL分别c6/c8/o6/o8，UID见表；外层180秒，不附CLI timeout。outputRelativePath=null，完整任务回收而非仅Results。无supervise-exit、FQN整组、斜杠filter、默认run、publish根或补跑。

## 原件、实际参数及完整亲读索引

证据根R=`WpfGfxShape/artifacts/async-drain-directclose-x64/`。每项 `R/LABEL-tool-return.txt` 均在该任务返回后立即保存整个可见合并返回，乱码原样保留；`R/LABEL-invocation.json` 同时记录实际完整调用字段、准确arguments数组、task及回收路径，不是事后推测。`R/LABEL-readback.md` 为继续下一项前的亲读判定；O8判定为停止。所有完整任务分别在 `R/readback/{c6,c8,o6,o8}`。

各Results目录递归枚举均只有identity、一个业务JSON、TRX及一个diag，全部亲读，无业务遗漏：

|项|各自readback/LABEL/下业务文件 / 亲读行|各自Results/asyncdrain-LABEL-x64/Diagnostic/日志|
|---|---|---|
|C6|Results/async-wic-drain-False-6-False.json 1–118|log_261028020315429.diag 1–65|
|C8|Results/async-wic-drain-False-8-False.json 1–126|log_261028020433276.diag 1–65|
|O6|Results/async-wic-drain-True-6-False.json 1–132|log_261028020549393.diag 1–65|
|O8|Results/async-wic-drain-True-8-False.json 1–140|log_261028020705554.diag 1–65|

每项 `Results/asyncdrain-LABEL-x64/child.trx` 1–26全文、`Results/aot-identity.json` 单行全文均已亲读。业务分1–75/76–末；diag分1–25/26–48/49–60/61–65，工具实际输出无截断。真实合并返回在调用时亲读并完整落盘，不声称存在额外stdout/stderr原件。TRX testId与所选UID一致，executionId/TestRun id另列，不当作业务UID；业务JSON本身无UID，用角色/queueState/retainExternal与独立task绑定。

diag每项49行准确UID InProgress，56行同UID Passed terminal，63行TRX artifact，65行MTP会话exit0。会话分别为C6 `01cefaa4-9e12-49d5-a98a-c07420e16113`、C8 `565f7478-804d-4053-bdcb-ac6f9ed96aed`、O6 `b0505370-a032-47fb-b772-ed0d135dde71`、O8 `74b41ea1-a54d-46dc-9b1f-de52335b42d6`。

## 每体actual身份重新绑定

四体runtime均.NET10.0.13 / X64，test SHA `4A8356201F062B01161744C6B3BBF7D9AB7DB5909F6F12A9389806FFB6B6ED43`、MVID `d41adfe8-8904-4c9b-887d-8d1f5979ba46`；初始化identity辅助PointerSize=8且实参数组匹配。

- C6/C8业务path=actualPath为各自task `Native/wpfgfx_cor3.dll`，actual SHA `54BC62FBBEF9A55EA75E49AF5EFDCEC70F7C8C2808BDF4E2C80B916CF92D5ADD`，version1.0.0.0。
- O6/O8业务path=actualPath为各自task根 `wpfgfx_cor3.dll`，亲读actual official SHA均 `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，version `10,0,1326,50504 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。不是x86 5EBD；official初始化identity的54BC62只是候选磁盘预检，不能替代actual。

亲读现存源码 `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/UceChannelTests.AsyncDrain.cs` 110–175、230–255，既有NativeLibrary.Load返回handle与ProcessModule.BaseAddress相等定位actualPath，再对该路径文件SHA256；不是只凭预检。没有输出handle数值，没有GetModuleFileName、内存映像hash或抗磁盘替换认证，不借ownership更丰富字段补造。

## 真实生命周期、重入及worker退出的有限结论

四项retainExternal=false、completed=true；6为健康open未提交directClose，8为健康committed directClose，非queue枚举6/8。主channel由caller Destroy。candidate真实最终stream释放callback中重入Destroy sibling；官方显式跳过callback重入，caller正常Destroy sibling，是独立关闭控制，不是官方重入oracle，也不是候选worker等价。

|项|释放thread / caller thread|真实顺序|正常关闭|
|---|---|---|---|
|C6|5452 / 1488|callback.enter1/order2/exit3；closeReturned4/closeReturn5；deinit6/7|Destroy.Drain0 / Callback.DestroySibling0 / Deinitialize0|
|C8|11544 / 11524|callback.enter1/order2/exit3；closeReturned4/closeReturn5；deinit6/7|CloseBatch0 / Commit0 / Destroy.Drain0 / Callback.DestroySibling0 / Deinitialize0|
|O6|872 / 11444|closeReturned1/closeReturn2；callback.enter3/order4/exit5；caller sibling6/7；deinit8/9|Destroy.Drain0 / Destroy.Sibling0 / Deinitialize0|
|O8|3708 / 2236|closeReturned1/closeReturn2；callback.enter3/order4/exit5；caller sibling6/7；deinit8/9|CloseBatch0 / Commit0 / Destroy.Drain0 / Destroy.Sibling0 / Deinitialize0|

四项disposals=1且callbackExited=1，真实enter/exit完整、非caller释放，不能用dispose先计数代替callback退出。所有正常calls的result均0，包括Initialize/Connect.Async/Channel/Sibling/Create.Bitmap/Wrap/Transfer/Disconnect.ReferenceOnly；每项三次Release.External references1为引用数非HR。externalReleaseReferences4294967295、official reentry-2147483648为未走分支哨兵，不当失败HRESULT。candidate reentry=0由真实Callback.DestroySibling0支持。

源码232–233在Deinitialize后、completed=true之前，SpinWait最多5秒断言该releaseThread对应OS线程不再存在。C6/C8本次准确Passed、terminal及完整业务支持这两次已有断言实际通过；无独立worker.exit事件或耗时，不认证所有线程或长期趋势。官方不执行此断言，不据其Passed认证候选worker退出。finally忽略HR的兜底不作为正常清理证据，本结论依正常calls。

## 退出边界、范围及下一主线

TRX准确1/1/1 + 完整业务/identity + 同UID Passed terminal + MTP会话exit0，满足既定有限合同。外层原返回无独立OS数字exit，没有supervision.json/监督证据，没有分离stderr；不称OS Exit0、监督Exit0、stderr空或全进程无异常退出。NativeRun返回MTP值只是源码预期，非OS观察。无工具调用错误、无重试/补件运行。

本工作无收尾待办，已停止；成功只覆盖所选健康关闭、真实callback、候选重入及这两次候选worker退出，不覆盖像素/长期资源趋势/HW/resize/real-loss/media/full。保留两partition合同和双架构ownership及所有历史独立账，完整替换与五大维度仍Open。

**下一主线建议：零执行梳理完整计划包1–8剩余主要缺口、当前SHA适用覆盖和真实外部能力**，优先内容/真实WPF producer-consumer、HW旧32失败的当前身份裁决、循环资源收敛、resize/real-loss/持续媒体及最终双架构Release适用矩阵。若有明确受影响路径，再另授权最小现存回归；不再发明微型ABI门、不借此授权full或显示。恢复入口/current-work-item/plan-status/覆盖矩阵已最小更新并回读，旧段落仅历史。
