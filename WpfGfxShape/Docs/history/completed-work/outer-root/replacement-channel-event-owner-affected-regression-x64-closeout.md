# x64 event-owner 受影响有限回归收尾

## 状态：CloseoutCompleted / 八角色有限回归 Passed

证据根：`WpfGfxShape/artifacts/event-owner-affected-regression-x64-r1`。本轮仅补 O8 readback、C8 和正式文档，不重复前八任务。八角色均通过；纯发现另计一次。累计沙箱 9/9，本接续只新增 C8 一次，前八任务不重跑；Native/COM publish、项目 build、资源趋势运行均 0。本次执行者未再委派。

## 身份与结论边界

candidate actual SHA256 `D3F5E40D2ABC0782D678C843537210EC203987A74C84616E20164703B3895B82`；official actual `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`。test SHA256 `6D4998A543844169FB893FB3D08E8EE0D01A14D745FBA78AC0E99FC53E7DFDB8`；MVID `b134506e-23ca-4f47-b543-5e293682213b`；.NET 10.0.13 / X64 / pointer8。初始化 identity 中 candidate file 身份不是 official 实际加载身份。

所有通过只属于新 x64 D3F5 受影响有限回归，可保留事件 owner 修正本合同；不等于完整替换通过。x86 旧 15A545 尚未含修正；KN/媒体旧 SHA 证据不迁移。资源趋势仍 PendingInvestigate：Handles 约 +2/轮，PrivateBytes 未改善，不标 Green。完整替换 Open。

## 角色矩阵与证据归属

01–07 原执行及业务全文核验归前代理；本次已读取全部对应 readback，不冒称重新亲读其原业务全文。原调用、原返回与详细读取范围保存在各编号文件。01–08 task 均未重新执行；C8 为本次执行。各角色 TRX total/executed/passed 均1/1/1、failed0，诊断同各自UID Passed/session0；01–07按前代理readback有限范围引用。

|角色|task / UID|合同与真实清理|读取范围/归属|
|---|---|---|---|
|postfailure official|b67fb76b5c234e99ad910166f3973e07 / 1492e97c-4a0e-8e0a-b34c-d1512cc86f05|late Create0；A/L 前后88980406；16<17<19<20；Destroy A/L、disconnect/deinitialize0|前代理 business1–267、TRX1–26、supervision1–40、identity1、stdout1–12、stderr空、diag1–65|
|postfailure candidate|1220b2aaf4c9490684f31837e709bdf8 / 1c192168-0326-8ff6-be42-a78cea1f8ece|D3F5 actual；lateCreate0；A/L故障保持；真实Destroy/断连/反初始化0，无cleanupErrors|前代理 business1–267、TRX1–26、supervision1–40、identity1、stdout12、stderr空；diag49–65，前48不冒全文|
|isolation official|aa802a689c7f4127901c037d7d535e2d / 1e3a533f-c51b-8855-b58a-2f84cc5b1ade|A/S故障；B前后新Send/CloseBatch/Commit/Flush0；三channel销毁及断连/反初始化0|前代理 business1–402、TRX1–26、supervision1–40、identity/stdout12/stderr空；diag54–65|
|isolation candidate|b432fbcdba4a43b382ce9e921edd1922 / 18b58946-d629-8139-8233-224beea22724|D3F5 actual；A/S故障隔离，B健康；三channel销毁/断连/反初始化0|前代理 business1–402、TRX1–26、supervision1–40、identity/stdout12/stderr空；diag54–65，前部截断保留。原04返回不含task，从监督/identity/business一致核定|
|O6|ef086c81093a41bd8faf493c66ce852b / 1e4c35fb-589a-8de1-838e-4845fd1028f4|official；callback count1/exit1，非caller；Destroy.Drain/Sibling/Disconnect/Deinitialize0；官方正常关闭不是candidate重入oracle|前代理 business1–132、identity全文、TRX1–26、diag54–65；无独立OS exit监督/分离stderr|
|C6|4c240ebfabea4c6183b9043704b79444 / 1d757743-a95d-86bc-ad1a-035db6190dac|D3F5；Callback.DestroySibling0/reentry0；count1/exit1，非caller；Destroy.Drain/Disconnect/Deinitialize0|前代理 business1–118、identity全文、TRX1–26、diag54–65；无独立OS exit监督/分离stderr|
|O8|9bccb15088db4656b938c2cf2da12c4a / 173013d6-ba45-853f-8195-7f9c30f57a50|official；callback count1/exit1，11152非caller8616；reentry sentinel；Destroy.Drain/Sibling/Deinitialize0，Disconnect.ReferenceOnly0|本次亲读08 invocation/原返回、business1–140、identity1、TRX1–26；diag50–65同UID Passed/session0；前49不冒全文。未重跑；无独立OS exit监督/分离stderr|
|C8|d099f979d3db472fbd44c6f3e0c68622 / 19a3b855-228b-8fad-a67a-cfae47a754f2|D3F5 actual，queueState8/system=false/retainExternal=false/completed=true；callback count1/exit1；7908非caller524；Callback.DestroySibling0/reentry0；Destroy.Drain/Disconnect.ReferenceOnly/Deinitialize0|本次亲自执行唯一一次，外180；business1–126全文、identity1全文、TRX1–26全文、diag50–65同UID Passed/session0；无独立OS exit监督/分离stderr|

纯发现 task `f5949213b2a2490d97838241e7bc8c21`：18参数节点、测试体0；前代理分段读取关键节点及diag115 session0，不冒称诊断全文。C6/C8/O6/O8 UID源于当前包独立发现。

## 执行、原件与有限退出边界

先创建本正式文档实质草稿（七角色已核/C8待执行），再去重检查09/C8与.coding-agent记录；只见历史Discovered/体NotRun，无已执行C8原件。09调用参数执行前落盘，原工具返回执行后立即保存，均已回读。回收目录 `WpfGfxShape/artifacts/event-owner-affected-regression-x64-r1/readback/c8`；新增08-O8-readback.md及09-C8-invocation/tool-return/readback.md。原工具返回中文乱码按原样保留，非完整分离stdout/stderr档案。

C8 business没有UID字段；通过同task路径、identity的filter-uid参数、business参数(8)/candidate及TRX/diag关联，不伪造同UID字段。C8 PID9244；TRX duration0.0823857秒，MTP session0.4535456秒，工具摘要426ms，三者口径不同，外限180秒不是实测耗时。无独立OS process ExitCode/监督/分离stderr；只认证MTP session0，不以回收成功冒充OS exit监督。

亲读 `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/UceChannelTests.AsyncDrain.cs`160–215、216–260：queueState8的failed=false/directClose=true/state2。既有231–232行在 !retainExternal && !system && (!failed || (directClose && state < 2)) 条件下断言反初始化后5秒内 !DrainThreadExists(releaseThread)，适用于C6/C8；246–250行仅查当前进程线程ID。Passed包含这个精确释放线程退出断言，不扩大为全worker/全部资源退出，整体WorkerExitNotProven保留。官方正常关闭分支不运行candidate重入/退出oracle。源码初读超长截断及一次把文件当搜索目录失败已保留于09 readback，后续切片补齐相关边界；未改源码。

## 实际预算及收尾同步

整个受影响回归：8角色各1 + 纯发现1（体0）=沙箱9/9；前代理8、本次1。Native publish0、COM publish0（隐式编译0）、normal build0、源码改动0、新趋势0、重试0、终端脚本0、本次再委派0。此前channel-message趋势阶段Native/COM publish各1与task29821d9d04454b49b8295cc96f65e6a4属独立旧账，不计本轮。

五状态入口同步本轮有限Passed与预算，旧内容保留为历史而非当前下一门；各入口顶部及本报告分段回读后提交。历史原返回/诊断截断、不全量重读范围及独立退出证据缺口均不抹除。

完成后唯一建议：残余 owner 静态定位。不自动新增任何运行。
