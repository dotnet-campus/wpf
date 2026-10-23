# DH 独立审查 DG clean full 传输失败边界

## 最终裁决与执行范围

本轮只读审计及四文档更新，主代理仅监督；未使用终端或直接间接脚本，未探测、上传、构建、发布、运行或修改生产/测试/打包。已读执行规则、完整方案120行、DG报告全文、双架构请求原件、DE含DF补正及当前clean清单。本文是有界审查收口，不是full完成报告。

**DG请求选择了精确clean包，而非回收目录；两次full的上传完成、远端创建及执行/回收状态均未知。** 大上传问题必须优先定位，不能改判为长运行问题，也不采用异步绕过。本节覆盖三共享文档DF“下一轮clean full”和DC“改长请求/异步轮询”的旧排程：现在不再提交full或大包，优先定位和恢复既有任务。

以下路径相对根 R=d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape。

## 1. 请求选择与清洁输入核验

|项|x64|x86|
|---|---|---|
|已保存请求|artifacts/dg-clean-full/request-and-transport-x64.json|artifacts/dg-clean-full/request-and-transport-x86.json|
|sourceDirectory|R/artifacts/de-upload-review/clean-x64-02/com-acceptance-package|R/artifacts/de-upload-review/clean-x86-02/com-acceptance-package|
|manifest|该输入下package-files.txt，421行|该输入下package-files.txt，420行|
|总字节来源|DE原审计157451706|DE原审计147783111|
|exe|WpfGfxShape.ComAcceptance.exe|WpfGfxShape.ComAcceptance.exe|
|arguments|--supervise-exit --full Results/dg-full-x64 1500|--supervise-exit --full Results/dg-full-x86 1500|
|外预算/次数|1800秒/记录1次|1800秒/记录1次|
|回收位置|artifacts/dg-clean-full/results/full-x64|artifacts/dg-clean-full/results/full-x86|

两请求第2–3行精确指向clean输入，第8–11行保存参数、外预算、outputRelativePath=null（整任务回收）及独立输出根。没有选择smoke/readback/Results根、PublishDir或其父目录。本轮亲读两manifest全部范围（截断处补读）并递归枚举两个精确目录，均未截断；没有旧Results/TestResults/TRX、历史、obj、PDB、CodeCoverage、alpine/macOS/Ubuntu/arm64资产。保留Runtime/WPF、MTP、Contracts、Native及两份静态ReferenceData。System.Security.Cryptography.OpenSsl.dll等Windows runtime pack管理程序集不能仅凭名称判成跨平台原生污染。

上述只能证请求输入选择和当前本地清洁边界，不能证明DG当时实际发送完整内容或服务端接收成功。157451706/147783111亲核于 artifacts/de-upload-review/smoke-{x64,x86}-02/Results/dd-upload-boundary.json 第9行TotalBytes；DG字段名也明确totalBytesFromDEAudit（第6行）。DG及DH均未独立重算总大小，更不是本次网络发送字节或压缩传输量。原DB上传总量未知，不计算缩减百分比。

## 2. 大上传问题已修与尚未知

DD/DE已修的是输入边界：避免将回收根的Results/TRX/诊断/历史再次上传，避免递归PublishDir夹带跨平台/异架构/coverage资产；DG沿用这个精确clean输入，未证再次选错根。**约150MB仍不小，清洁不等于足够小，也不等于本次full已上传。** DE同包smoke成功是历史可用性证据，不是DG本次传输成功证据；本轮不重跑smoke。

可核实的大文件及同名项（大小仅继承DE审计）：
- System.Private.CoreLib.dll：x64 16033576、x86 15542056字节，审计第436–437行。它是runtime依赖，不能据体积直接删。
- 根wpfgfx_cor3.dll：x64 1956648、x86 1807144字节；Native/wpfgfx_cor3.dll：x64 4520448、x86 3995648字节。x64审计第1664–1677行，x86第1660–1673行；manifest x64第414/417行、x86第413/416行。同名但大小不同，官方/候选角色不同，未证字节重复或可删除。
- manifest x64第254–409行、x86第253–408行包含13种语言卫星资源，例如cs/PresentationCore.resources.dll、de/PresentationFramework.resources.dll、zh-Hans/WindowsBase.resources.dll。不同语言不是重复文件；没有full消费及本地化范围证据，不宣布不必要或直接裁剪。

本轮未证不必要巨文件或可安全去重项，不把未取到的最大文件排序伪装成已审结。后续缩包可行性来源是DE逐文件Path/Bytes审计、当前manifest及既有打包选择/依赖消费链；先只读定位可省内容与闭包依据，不盲删runtime、插件或ReferenceData。framework-dependent转换必须先有目标环境同架构runtime/WPF可用及实际加载实证，x64不能替x86；保持本包10.0.13实际基线/合同，不擅自换版本或以转换绕上传。完整方案没有固定全产品runtime版本，亦不外推未验证版本。

## 3. HTTP错误与关联标识审查

两个request-and-transport原件均已全文读取：第13行taskId=null，第14行error保存客户端-1073740791、HttpClient.Timeout=100秒及TaskCanceledException/TimeoutException/IOException/SocketException(995)，栈含HttpConnection.InitialFillAsync、SendAsync、HttpClient.SendAsync、ExecClient.ExecuteAsync、ExecCommand.RunAsync；第15行phaseEvidence明确未返回进度/字节/taskID。x86第16行readback记录本地监督/TRX未找到。它们是DG保存的请求及错误摘要，并非完整网络报文/服务端原始日志；不能扩大为已核原始HTTP响应头。

其中没有traceID、requestID、服务端关联ID、任务创建确认、上传起止或完成字节字段；995是Socket错误码，不是任务ID。artifacts/dg-clean-full当前递归文件列表仅这两个JSON，没有其他错误原文或关联元数据可供本轮恢复。DE审计SourceRoot中的21a05e928a394e2bb7ad4b4bc654539c / cce3c41fe7a4417b84f9406b8ae9e70f属于smoke，不能借填DG taskID；旧DB或DC诊断ID也不能借用。

100秒是客户端HTTP超时，不是HTTP 100状态码，更不是1500秒被测预算或1800秒外预算耗尽。-1073740791仅客户端状态，不是产品ExitCode/原生崩溃。未证上传、任务创建、exe执行或回收任一具体阶段失败；也未证远端仍运行、已停止、无任务或上传0字节。DG本地未回收只代表本地证据缺口。

## 4. 给基础设施持有者的最小取证清单

1. 用上述两份已保存请求的精确sourceDirectory、架构、exe、arguments、输出根和错误栈定位既有请求；请从原调用记录补提交时间/时区、客户端会话或其他已有定位信息。JSON没有时间戳，不能杜撰。
2. 提供每次上传开始/结束时间、客户端实际发送字节、服务端接收字节及是否完整接收/校验；区分包文件总量与线上传输量，不能填DE TotalBytes代替。
3. 提供是否创建任务、持久化taskID及已有traceID/requestID/服务端关联标识、执行实例和真实任务根；若未创建，给对应阶段原始日志。
4. 提供exe是否启动、实际参数、启动/结束时间、退出状态及监督timeout/cancel/kill/排空记录；无启动也须有明确日志，不从客户端退出推断。
5. 提供回收是否开始/结束、失败点、远端现存证据位置及已回收字节/文件；如任务已存在，优先恢复既有监督、parentTRX、stdout/stderr和业务/identity原件。

当前可用文件工具能读已保存JSON/清单和本地结果，**没有既有远端任务按ID查询/独立回收或HTTP服务日志读取能力**；现有执行工具需新的sourceDirectory/exe提交，不能拿它冒充只读恢复接口。本轮不调用新执行，不发明平台接口或命令。基础设施先提供既有记录，不要求改平台、延长HTTP或异步启动来绕上传；根因和缩包依据先定位，再按证据决定最小修复。

## 5. full未知字段与产品边界

双架构DG full均未知：taskID/真实远端绝对parentTRX、上传完成和实际字节、是否启动/完成、实际Mode/Arguments/filter、监督Exit/TimedOut/Kill/最后阶段/排空、TRX Counters/RunInfo/Failed、stdout/stderr和业务结果、actual test SHA/MVID、候选COM消费模块SHA、官方来源、runtime/WPF/架构/backend。未取得不得填0或复制DE smoke；请求参数不等远端实际执行参数。

保留CX/CY生产修复与有限独立通过：原44/10在已核有限场景修复，DG full未知不能反称修复失效，也不能外推full通过。旧BD full不绑定当前SHA；未知DB/DG full不互相填补。最终稳定Tests/ComAcceptance × x86/x64 Release四格未交，HW/GPU、device-loss/resize新帧、持续media、resources/线程/callback/通知、同步Event0、E_HANDLE、首次堆损坏、其余nested/stroke、圆角/大shader等适用合同仍开。

## 收口与实际下一最小行动

四文档仅交付已证边界、未知项与取证顺序，完整替换未完成。下一最小行动是将两请求原件交基础设施持有者关联既有上传/任务记录，先取上传实际字节与任务创建ID；存在任务即优先完整回收，亲读监督、父TRX及actual身份再判产品/夹具。并行可只读分析DE文件大小和消费链以识别有依据的缩包机会；目前无可安全删除项结论，不授权新full、大包、异步绕过或runtime转换。本轮无运行验证，文档最终全文分段回读至末尾后提交。
