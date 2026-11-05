# DG 清洁输入双架构 COM full 实跑

## 执行边界
本轮只写本文及独有 artifacts/dg-clean-full/；不改生产、测试、入口、项目和共享文档，不新增工程，不执行脚本或终端。仅复用 DE 两个从未作为回收根的精确 clean package 输入；回收与输入分离。不重发扰动候选 hash。

## 当前进度
已亲读最新交接/执行规则、完整方案120行、DD/DE含DF补正、DB全篇，以及 NativeRun 入口与 ExitDiagnostics 参数/执行监督。已递归枚举两精确输入（结果未截断），核对manifest首段、Runtime段和末段，实际列表没有Results/TestResults/PDB/obj/历史/跨平台/异架构coverage；保留Runtime/WPF/MTP、Contracts、ReferenceData两静态输入和Native。manifest分别421/420行。总大小采用DE专属审计JSON原件157451706/147783111字节，非本轮独立重算。aot.sha256源原件为x64 F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7、x86 7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA；不是actual加载结论。

本轮正式请求：sourceDirectory分别为根下artifacts/de-upload-review/clean-x64-02/com-acceptance-package、clean-x86-02/com-acceptance-package（绝非smoke/readback/PublishDir）；exe=WpfGfxShape.ComAcceptance.exe；Arguments=[--supervise-exit,--full,Results/dg-full-<arch>,1500]，外预算1800秒，outputRelativePath留空整任务回收，localOutputDirectory=artifacts/dg-clean-full/results/full-<arch>。入口--full使Filter=null，子参数无--filter，绝对TRX由监督Root构造；timeout整树kill及清理/排空机制不变。每架构一次，不重发、不改代码。

## x64 首次正式请求结果
专用执行返回客户端-1073740791，内部HttpClient.Timeout=100秒、TaskCanceledException/TimeoutException/IOException/SocketException995，栈在ExecClient.ExecuteAsync/HTTP。未返回taskID、上传字节/进度、监督或业务结果；不能确定上传/执行/回收阶段，不能认定长执行通道根因或被测崩溃。已先保存精确请求及错误摘要到artifacts/dg-clean-full/request-and-transport-x64.json。不重复x64 full、不扩大输入、不异步绕过；继续已授权的x86唯一正式请求，非x64重试。

## 双架构正式提交最终结果
x86唯一正式请求返回与x64相同的HTTP客户端错误；已保存artifacts/dg-clean-full/request-and-transport-x86.json。本轮没有额外smoke、重试、发布、脚本、第三工程或业务改动。工具错误没有提供上传阶段日志/成功字节/远端taskID，不能捏造明确上传阶段证据；尤其不能据100秒断定长执行根因。没有再上传回收根、PublishDir或父目录。

以下根R为 d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape。

|项目|x64|x86|
|---|---|---|
|实际sourceDirectory|R/artifacts/de-upload-review/clean-x64-02/com-acceptance-package|R/artifacts/de-upload-review/clean-x86-02/com-acceptance-package|
|总字节（DE原件）|157451706|147783111|
|manifest|source/package-files.txt，421行|source/package-files.txt，420行|
|正式提交次数|1|1|
|实际请求Arguments|--supervise-exit --full Results/dg-full-x64 1500|--supervise-exit --full Results/dg-full-x86 1500|
|外预算|1800秒|1800秒|
|localOutputDirectory|R/artifacts/dg-clean-full/results/full-x64|R/artifacts/dg-clean-full/results/full-x86|
|taskID及远端绝对TRX|工具未返回，未知|工具未返回，未知|
|监督JSON/子Exit/TimedOut/Kill/最后阶段|未回收，未知|未回收，未知|
|parentTRX Counters/RunInfo/失败项|未回收，未知，不能记0|未回收，未知，不能记0|
|stdout/stderr/Results业务|未回收，未知|未回收，未知|
|本轮actual loaded模块/test SHA/MVID/runtime/WPF/官方来源|未取得，不移绑smoke|未取得，不移绑smoke|

本地精确直读两条监督路径：R/artifacts/dg-clean-full/results/full-x64/Results/dg-full-x64/supervision.json、对应full-x86/Results/dg-full-x86/supervision.json均不存在。两条parentTRX精确直读：R/artifacts/dg-clean-full/results/full-x64/Results/dg-full-x64/TestResults/child.trx、对应full-x86/Results/dg-full-x86/TestResults/child.trx均不存在。这里只证明未回收至本地，不证明远端缺失或停止。预定远端TRX模板为 C:\CodingAgentSandbox\Tasks\<本轮未知taskID>\Results\dg-full-<arch>\TestResults\child.trx；没有真实ID，不能声称已记录实际绝对TRX。

## 身份证据层级
输入aot.sha256源原件已亲读，完整值见上文。DE原审计JSON头部已亲读：x64 test SHA673EAA4260DCFD2233E391F00668264DB6EF391211457F0BA9C631AA49139A93/MVID21821cb2-22f7-4c5e-8376-ef1812e9e4bb；x86 SHA198989C80F41518CCD6B41559A8ACA67A7391BB47A4942C779D326762E9D6AE8/MVID1e3c5e42-4f06-47bc-86bc-20fc903ab14d；DE runtime .NET10.0.13、WPF为各DE任务根PresentationCore.dll。这些全部是输入/历史smoke证据，不是本轮full实际加载。未取得本轮actual identities，无法交叉核父identity是否被子角色覆盖，不能把文件SHA冒充COM实际消费身份。

## 失败分类与下一最小切片
- 已证执行/结果观测阻塞：两个精确清洁包的正式请求均返回客户端异常；是否属于上传阶段尚无日志证据。客户端-1073740791不是子进程ExitCode，不报Exit2或原生崩溃。
- 真实产品/夹具：本轮没有可读full失败项，不能分类具体缺陷、sampler首写坏或最后Destroy；不据末用例归因。
- 输入缺失：实际列表含必要Runtime/Contracts/Native/ReferenceData，未证full依赖闭包完整，也未证某缺失导致失败。不随意删依赖或改runtime。
- 硬件前置/未覆盖：没有本轮backend、GPU、loss/resize、新帧、media、资源/线程/callback/通知证据；均保持未闭，不归环境通过。
- 新fill原44/10：本轮full无可读证据，不能裁定复现或关闭；CX/CY有限通过不拼接为本轮full。

下一最小可执行切片是由执行基础设施持有者提供这两次精确请求的上传/执行/回收分阶段日志及持久化taskID，再按既有任务完整回收。先确认上传实际字节和成功边界，不能未经证实改走异步或归因为长执行。拿到ID后记录真实绝对parentTRX并亲读监督、完整Counters/RunInfo/Failed、stdout/stderr、业务actual identities与官方来源，再选一个最小产品/夹具/部署修复。当前不越界修改入口/打包/生产，不增加重跑；没有被证实可修的部署根因。

## 最终裁决
本轮已按授权完成双架构各一次清洁输入正式full提交尝试，但没有完成态parentTRX或监督回收，**两个full均不能认定完成**。未获两个taskID是明确交付缺口，不用旧DB任务名/结果补填。完整替换与最后Tests/ComAcceptance × x86/x64 Release四格仍未交；本报告为已完成有界尝试的阻塞事实，不是等待结果却宣称完成的初稿。
