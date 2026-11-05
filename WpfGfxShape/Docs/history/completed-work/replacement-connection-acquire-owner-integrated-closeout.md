# Connection acquire owner 集成验收 — CloseoutCompleted（有限范围）

## 正式终态（优先于下方历史草稿）
source-owner修正、既有15 managed及双架构六体有限ABI正常回归已完成；本项CloseoutCompleted，完整替换仍Open。此前四体未重跑，本轮仅两次x86 sandbox。新分支/源码/配置/build/tests/Native publish/COM publish/终端脚本/再委派均0。这里tests0指本轮不再运行managed，不抹除既有15 passed，也不把sandbox误计0。

### 六体最终证据矩阵
下面路径均相对WpfGfxShape/artifacts；每行TRX精确路径为 `回收根/Results/结果名/TestResults/child.trx`，业务路径为 `回收根/Results/结果名/业务族/UID/business.json`。远端任务根统一 `C:\CodingAgentSandbox\Tasks\taskID`。六体监督exit0、正常exited/no kill，TRX total/executed/passed=1/1/1，primary=null、cleanupErrors=[]。

|角色|taskID|回收根|结果名|业务族/UID|终态|
|---|---|---|---|---|---|
|x64 official postfailure|7825993f0d22446a83fca72c4ddf61b1|connection-acquire-owner-x64-r2/official-postfailure|connection-acquire-owner-x64-official-postfailure|PostFailureAttach/03dcce11eb114749a9e73aabfba246da|Passed，既有不重跑|
|x64 candidate postfailure|6fa38eb3caef42d0b69eda2fcde9fe08|connection-acquire-owner-x64-r2/candidate-postfailure|connection-acquire-owner-x64-candidate-postfailure|PostFailureAttach/ea037a1cba954cd891652c38b8639f49|Passed，既有不重跑|
|x64 candidate isolation|a010b4d627ad472884a1c2c520bbffe8|connection-acquire-owner-x64-r2/candidate-isolation|connection-acquire-owner-x64-candidate-isolation|PartitionFailureIsolation/4da583e1261c4010bdd61b1dc93f0546|Passed，独立候选有限回归|
|x86 official postfailure|9a1844180d8f46f1af7e99338243ce3e|connection-acquire-owner-x86-r2/official-postfailure|connection-acquire-owner-x86-official-postfailure|PostFailureAttach/770ef845ef10404a89a65841c31acaf4|Passed，既有不重跑|
|x86 candidate postfailure|83cffbd385644d3dab5a9a7a185c857c|connection-acquire-owner-x86-r2/candidate-postfailure|connection-acquire-owner-x86-candidate-postfailure|PostFailureAttach/bc684bbc2b77435a94c3c72722143e2c|Passed，本轮新增1次|
|x86 candidate isolation|de4fcd207bd148dba55cdacc7395e72b|connection-acquire-owner-x86-r2/candidate-isolation|connection-acquire-owner-x86-candidate-isolation|PartitionFailureIsolation/85923e43f8214584ba28df33a11c3896|Passed，本轮新增1次；独立候选有限回归|

### actual与test身份（各架构独立绑定）
- x64 candidate actual Native/wpfgfx_cor3.dll SHA632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671；official actual根wpfgfx_cor3.dll SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1。test E0BA8257526087597ED093A2F3102E6BB09B5773318E860A4BBF2DCD190F15F6 / MVID358caf7b-d362-445d-ae09-0da97456cf77 / .NET10.0.13 X64 pointer8。
- x86 candidate actual Native/wpfgfx_cor3.dll SHA147B8D862F315A9E5FB0F00A2BEE8F0ED1DFB258C870E69D3C32C5D3BE9F9304；official actual根wpfgfx_cor3.dll SHA5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834。test DA6E29882DD7FA104BCCC38AAEA10C76581C1C9591FCEA50A6D5589184A38C12 / MVIDa746e7f6-d5e4-4b9c-b835-22a4401d1e3c / .NET10.0.13 X86 pointer4。
两次新candidate均business requestedEqualsActual=true、模块handle/base与实际路径绑定、actual hash与包/初始化hash一致；初始化hash只做前置，不替代actual role。official加载根官方DLL，初始化候选SHA不冒官方SHA。
postfailure：lateCreate=0、L非零，source销毁前后0x88980406（-2003303418）；created/destroyed配平、stillKnown空、disconnect/deinitialize0、cleaned=true。isolation：A/S同失败、独立B在关闭前后分别全新alpha0.5/0.25 send/closebatch/commit/flush=0，三channel destroy及disconnect/deinitialize0。本轮没有official isolation；旧official隔离证据不移绑新hash，不能宣称新同包官方等价。

### 真实包来源与历史失败
r1 task51be0b73a3714d38a4e327c1a2448cb1缺根aot.sha256，初始化exit2、TRX1 failed/业务0，保留为错误包失败，不算业务合同失败。`-o`仅PublishDir；正式ComAcceptance.Package.targets生成ComAcceptancePackageDirectory与hash。
x64修复包来自唯一r2补COM publish，显式正式根connection-acquire-owner-x64-r2clean，独立PublishDir x64-r2publish；绑定原x64-r1/native而非旧clean。完整参数保存在artifacts/connection-acquire-owner-r2-execution.md历史“COM补包x64 #1”段（增量插入后不使用旧行号定位）。
x86复用已确认默认正式根WpfGfxShape/artifacts/com-acceptance-package，不重新发布。原x86 official调用source的现存记录为r2执行账历史sandbox #4段；supervision/aot-identity原件确认task9a184…及实际根/identity。原工具调用响应本身未另存文件，不能把恢复出的执行账描述为亲读原工具JSON。x86-r1/com-publish.md保留原发布完整参数：输入x86-r1/native/wpfgfx_cor3.dll，-o x86-r1/clean但未覆盖默认PackageDirectory，正式根因此仍为com-acceptance-package。本轮两次实际工具调用的source精确为此目录，非嵌套WpfGfxShape/WpfGfxShape/artifacts错误相对根。首次读取artifacts短路径不存在，已纠正为WpfGfxShape/artifacts，不计执行。

### 预算、日志与证明边界
- 历史r1 normalbuild1 Release exit0，3714warnings/0errors，28.03s；输出3113342字符只存末20000、丢3093342。
- 历史managed test1，隐式Debug build，15passed/0failed/0skipped，34ms；1557018字符丢前1537018，warnings总数Unknown，无配置TRX；不冒Release运行。
- 历史Native x64/x86各publish1 exit0，各615464字符只存末20000、截595464，warnings总数/耗时Unknown。COM历史各架构1（x86 11行完整、restore176ms），x64额外修包1（11行完整、restore181ms），warnings/errors未汇总，不编造0。
- r2 sandbox预算原剩5+增2=7，已有4、本轮2，共6/7；剩1不用。每体最多一次、120内/180外、精确FQN、全任务回收；无修后重试。本轮工具只返回保存目录，不含独立exit字段，exit以supervision为准。两次监督40行/stdout12行/空stderr完整亲读；TRX计数及业务identity/合同/清理分段读无截断，stdout中文解码乱码原样留存。
- source修正为Acquire成功后ownsScheduler=true、Connections.Add成功才转移所有权，finally在lock外Release；两条新增manager引用测试及已有定向集合共15通过。真实OOM和identity溢出未动态触发；result-copy negative controls不是产品故障注入。诊断export absence Unknown，false属性不替代binary认证。
- 旧heap仅有限可回收诊断；自然资源PendingInvestigate / MemoryDescriptiveOnly / memoryGreen=null / WorkerExitNotProven。display Unknown；HW/resize/device-loss、实际音频及完整替换Open。旧KN/媒体/full不得整体移绑新hash。撤回任何无证“工具不可用”或“生成状态图”说法，不计为运行或完成证据。

### 确定下一实作与优先级（非选门审查）
P0：在冻结的632F x64/147B x86产物上执行受影响主要ABI/COM回归，随后按完整方案§1–10最终适用集逐格绑定actual/test身份；旧全量不得顶替。P1：先按现有公开媒体最小门完成当前hash真实连续媒体/实际音频输出，再落实真实显示会话与HW/resize/device-loss恢复，不以软件fallback或重复无证paint/capture前置探针代替。P2：在无强制GC下证明正式自然资源收敛与全部worker退出；既有诊断不升Green。上述新增执行须另授对应预算，本项余1不挪用；本项所有文档收尾已完成，不再以Acquire“未修”或六体“待跑”为下一动作。

### 最终同步与回读凭据
已实际同步并回读next-session-handoff、current-work-item、current-stable-baseline、replacement-issues-and-plan-status、final-acceptance-remaining-matrix、wpfgfx-complete-replacement-validation-plan六份最新终态；正式报告1–70分段完整回读无截断，r2账增量1–9回读。两份新增TRX又读1–19，结合20–26已完整26行：postfailure runID724c901e-c2e8-4c77-9e69-033c73d54882/testId1c192168-0326-8ff6-be42-a78cea1f8ece，isolation runID1c25e0a5-2225-46e7-bf50-daf26aeacc9c/testId18b58946-d629-8139-8233-224beea22724；TestMethod精确class/FQN与task storage吻合。无剩余待读、未同步的本项收尾动作。

## 以下为历史执行草稿与即时回执（保留，不覆盖原账）

## 本轮增量草稿（执行前持久化）
原下文为历史草稿，所有“待核/执行中”保留原文，不作为最新终态。本轮不新建分支、不改源码、不build/test/Native/COM publish、不使用脚本或委派。已有四体不得重跑：x64 official 7825993f0d22446a83fca72c4ddf61b1、candidate 6fa38eb3caef42d0b69eda2fcde9fe08、isolation a010b4d627ad472884a1c2c520bbffe8；x86 official 9a1844180d8f46f1af7e99338243ce3e。
x86-r2目录当前仅official-postfailure；执行前继续核对任务与包身份。剩sandbox3，仅必要两次各最多1、保留1不用：candidate postfailure → candidate isolation，监督120/外180，整任务回收，首错停不修后重试。复用sourceDirectory=WpfGfxShape/artifacts/com-acceptance-package，exe=WpfGfxShape.ComAcceptance.exe；根hash147B8D862F315A9E5FB0F00A2BEE8F0ED1DFB258C870E69D3C32C5D3BE9F9304；实际candidate模块须业务独立认证，不能仅用初始化hash。
正式关闭范围仅source-owner修正、既有15managed及双架构有限ABI正常回归；真实OOM注入、identity溢出、诊断export absence不作已证。完整替换仍Open。执行后逐项记录原可见返回、task/TRX/业务UID与清理，并同步六份入口文档及回读。

## 本轮 #5 即时回执（已核）
参数sourceDirectory=WpfGfxShape/artifacts/com-acceptance-package；executableRelativePath=WpfGfxShape.ComAcceptance.exe；arguments=[--supervise-exit,Results/connection-acquire-owner-x86-candidate-postfailure,120,FullyQualifiedName=WpfGfxShape.ComAcceptance.UceChannelTests.WhenFailedPartitionAcceptsLateSharedChannelThenSourceClosePreservesFailure]；timeoutSeconds=180；outputRelativePath省略（整任务）；localOutputDirectory=WpfGfxShape/artifacts/connection-acquire-owner-x86-r2/candidate-postfailure。
原可见返回：沙箱执行完成。结果已保存到：d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\connection-acquire-owner-x86-r2\candidate-postfailure。工具返回没有独立exit/task字段；task83cffbd385644d3dab5a9a7a185c857c由supervision原件绑定，远端C:\CodingAgentSandbox\Tasks\83cffbd385644d3dab5a9a7a185c857c。
supervision 1–40完整：exit0、正常exited、无timeout/kill/drain error；TRX TestResults/child.trx 20–26：1/1/1。stdout 1–12完整（中文有解码乱码，保留）、stderr空。business PostFailureAttach/bc684bbc2b77435a94c3c72722143e2c/business.json 1–65、200–267：actual=requested Native/wpfgfx_cor3.dll、actual SHA147B8D862F315A9E5FB0F00A2BEE8F0ED1DFB258C870E69D3C32C5D3BE9F9304（非仅初始化hash）、testDA6E…/MVIDa746…/.NET10.0.13 X86 pointer4；lateCreate0、source2/L3，First/After=-2003303418即0x88980406；destroyed[2,3]与created配平、stillKnown空、disconnect/deinitialize0、cleaned=true、primary=null/cleanupErrors空。本次读取未截断。剩sandbox2，仅isolation1，另1不用。

## 本轮 #6 即时回执（已核）
参数sourceDirectory=WpfGfxShape/artifacts/com-acceptance-package；executableRelativePath=WpfGfxShape.ComAcceptance.exe；arguments=[--supervise-exit,Results/connection-acquire-owner-x86-candidate-isolation,120,FullyQualifiedName=WpfGfxShape.ComAcceptance.UceChannelTests.WhenAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives]；timeoutSeconds=180；outputRelativePath省略（整任务）；localOutputDirectory=WpfGfxShape/artifacts/connection-acquire-owner-x86-r2/candidate-isolation。
原可见返回：沙箱执行完成。结果已保存到：d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\connection-acquire-owner-x86-r2\candidate-isolation。无独立exit/task字段；taskde4fcd207bd148dba55cdacc7395e72b由supervision绑定，远端C:\CodingAgentSandbox\Tasks\de4fcd207bd148dba55cdacc7395e72b。
supervision1–40完整：正常exited/exit0，无timeout/kill/drain error；TRX20–26：1/1/1；stdout1–12完整（中文乱码保留）、stderr空。business PartitionFailureIsolation/85923e43f8214584ba28df33a11c3896/business.json 1–65、305–402：actual=requested Native/wpfgfx_cor3.dll，actual SHA147B8D862F315A9E5FB0F00A2BEE8F0ED1DFB258C870E69D3C32C5D3BE9F9304（独立模块证据），testDA6E…/MVIDa746…/.NET10.0.13 X86 pointer4；A/S=0x88980406，B全新beforeCloseAlpha0.5/afterCloseAlpha0.25更新及flush均0，created/destroyed[2,3,4]配平、disconnect/deinitialize0、cleaned=true、primary=null、cleanupErrors空。本次读取未截断。两次均单次成功，无修后重试；剩sandbox1封存不用。

## 已核事实与范围
生产/测试/配置冻结，本阶段追加源码0、Native发布0。既有Release build exit0（3714 warnings/0 errors），定向tests 15 passed；日志为摘要与截断记录，不是完整stdout。真实OOM及identity溢出未动态触发，source-proof不是公开故障注入。

## r1失败原件
亲读task51be0b73a3714d38a4e327c1a2448cb1的 Results/connection-acquire-owner-x64-official-postfailure/TestResults/child.trx（1–48；1–37工具4000字符截断，37–48另读）。total1/executed1/passed0/failed1，初始化ValidateIdentity缺根aot.sha256，业务0；exit2。不能认作业务合同失败。第一次误读直接child.trx路径不存在，已按stdout精确改读TestResults/child.trx，不重复该错目录。

## 根因及真实包
实际ComAcceptance.Package.targets 42–70：self-contained Publish后将文件复制到ComAcceptancePackageDirectory，并用GetFileHash产生aot.sha256；-o仅PublishDir，不是正式包根。默认正式根为artifacts/com-acceptance-package。已亲读该根aot.sha256=147B8D862F315A9E5FB0F00A2BEE8F0ED1DFB258C870E69D3C32C5D3BE9F9304、package-files.txt前18行及runtimeconfig .NET10.0.13，公开DLL元数据含三所需无参方法。不能凭旧clean命名作正式包；根包架构与来源仍待核明。

## 本次授权预算
原剩余sandbox5，明确增补2，总剩余7；六体目标各架构official postfailure→candidate postfailure→candidate isolation，各1，剩余1不自动盲重跑。COM补包每架构最多1，仅确无可复用合法包时使用，Native追加0、源码改0。监督120秒/外180秒，精确无参FQN。每角色前核前置/identity/合同，首错停，不绕ValidateIdentity，不手写hash或文本搬运二进制。

## 未关闭边界
候选isolation若本轮无official角色，仅独立候选回归，不冒同包官方等价。旧heap仅有限可回收，资源趋势PendingInvestigate/MemoryDescriptiveOnly/memoryGreen=null/WorkerExitNotProven；显示Unknown，HW/loss/音频及完整替换Open。default GC诊断二进制导出absence未核，Unknown。
