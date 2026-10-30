# postfailure 执行交接独立审计

## 后续终态索引（不改本篇历史审计结论）

[新独立source-rebuild收尾](replacement-postfailure-source-rebuild-x86-closeout.md)已以本次输入快照/持久build与两publish/新候选两合同闭合限定新来源。旧原日志缺失与本篇四绿认证仍保留，不宣称历史日志恢复。仅当前x86两个合同可保留修正，整体不升；下一仅建议另授权x64同冻结源码两合同，不执行。


## 裁决：部分认证，来源链收尾仍 Blocked

本审计独立亲读实际文件，不以任何前代理文字为用户认证或执行事实。本阶段源码/build/test/publish/沙箱新增均0，无终端、脚本、程序执行、执行器或再委派。只新增/更新文档。

“原件目录不存在”不能作为当前结论：精确路径实际可读，四个声明 task 的监督、TRX、业务原件均已找到。另一方面，原新Native报告确实仍为21行执行前快照，恢复入口与计划也确有滞后；不能因找到Passed而自动补齐所有构建来源和预算。选择“只找到部分，逐项独立认证”：有限运行事实可认证，Native publish完整来源、独立COM正常build日志及次数仍未证，整体基线不升级。不是认定未执行或伪造。

## 实际调查范围与读取陷阱

工作区绝对根 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape`，以下 R 为根下 `WpfGfxShape/artifacts/postfailure-new-native-x86`。所有找到的本地原件均在根内。

根及 `WpfGfxShape` 的普通目录枚举没有显示artifacts，后者精确路径却有native/clean/publish/readback；内容正则搜索四task和新SHA未命中，名称搜索task/SHA亦未命中。这些负结果不证明原件不存在。Results枚举未显示TestResults，但精确 `TestResults/child.trx` 四份均可读。不沿用先前的缺件推断。

实际检查根 `.coding-agent`（无可见子项）、根 `.coding-agent/sandbox-results` 与 `WpfGfxShape/.coding-agent/sandbox-results`（工具报告目录不存在）；未找到这两处的归档。没有检查全磁盘或未暴露归档。JSON中的 `C:\CodingAgentSandbox\Tasks\<task>` 是远端历史执行路径，不在当前工作区根下；此次未访问该远端路径，依据是本地回收副本。

先读真实恢复入口、first-validation报告（含末段）、21行新Native快照、基线/事项/计划/规则，再核源码和精确原件。名称搜索artifacts内.log未命中，不能据此断言所有日志不存在。

## 四角色原件索引与分层认证

每行本地根为 `R/readback/<角色>/Results/<角色>/`。该根下 `supervision.json` 全40行、`stdout.txt` 全12行、`stderr.txt` 空内容、`TestResults/child.trx` 全26行均亲读。business按不截断小段完整读取（postfailure各267行，isolation各402行）。stdout中文有解码乱码，不将乱码修复为额外事实；结构化JSON/TRX为主要依据。

|角色|task|business相对路径|TRX testId|
|---|---|---|---|
|postfailure-official|6edf1e54cb4d4d33acc0ea81ee5b78c6|PostFailureAttach/7c4248e2567349f3bec021548c917482/business.json|1492e97c-4a0e-8e0a-b34c-d1512cc86f05|
|postfailure-candidate|06db04ddad8f4f149f42368e8449d05f|PostFailureAttach/9634aa34b15b4f1599e49e6215026d93/business.json|1c192168-0326-8ff6-be42-a78cea1f8ece|
|isolation-official|9e6ff003bce14ec494beb87175f0ec69|PartitionFailureIsolation/ffcc9a89995f4ae094fd9a7269655ec1/business.json|1e3a533f-c51b-8855-b58a-2f84cc5b1ade|
|isolation-candidate|1d0a27b241834cbd8d1ed759596edb87|PartitionFailureIsolation/3833db728df441cba0901102ff1ff646/business.json|18b58946-d629-8139-8233-224beea22724|

四TRX各total/executed/passed=1/1/1，failed0，方法与执行前快照四个精确FQN一致，storage/codeBase分别绑定该task。业务UID不同于TRX testId，不混用。监督各Exit0、exited、无timeout/cancel/kill、outputCollected=true、无drain超时或SupervisorError；内限120秒。外180秒是原计划/声称，单凭本地监督不认证外层调用参数。

时间按监督UTC：07:55:32–33、07:56:15–16、07:57:08–09、07:57:59–07:58:00，日期2026-10-27。支持四次顺序独立进程，不能据四个归档证明不存在额外未归档执行。

共同测试SHA `1C56E8CB2F0679FFF67EB217708EFD1DAF969200201E4973802D6927963759BD`，MVID `cc7cfbfe-6b94-4400-814c-a299f138970d`，.NET10.0.13/X86，business pointerSize4，业务PID与监督一致。相同测试字节与历史测试复用不等于旧运行：本轮task、时间、business UID及新candidate实际加载身份独立可辨。

两个candidate的requested=actual均为各task下Native/wpfgfx_cor3.dll，moduleHandle=actualBaseAddress=1851981824，实际SHA均为 `15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620`，版本1.0.0.0。两个official实际加载各task根DLL，handle=BaseAddress=1854210048，SHA `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`，版本10,0,1326,50504。初始化候选文件校验不冒充official角色加载。

postfailure两角色：23次调用完整；A失败88980406后才Create L，Create0且L非零（official73054400，candidate3）；L关闭source前后两次flush均88980406；Destroy A/L、disconnect、deinitialize均0；completed/cleaned=true，unknownRegistration=false，stillKnown=[]、primary=null、cleanupErrors=[]。五个结果副本负控true，仅断言检查，不冒生产故障注入。

isolation两角色：42次调用完整；A/S flush88980406；独立B在关闭A/S前后真实Send/CloseBatch/Commit/Flush全0；三通道Destroy及disconnect/deinitialize0；completed/cleaned=true、primary=null、cleanupErrors=[]；两个副本负控true。

由此独立认证：指定新SHA的x86 postfailure与isolation有限ABI运行证据存在且通过。不能外推真实注册异常回滚、composition线程重入Destroy、全部资源极端异常、KN17/resize、HW/device-loss/media/full或新x64。

## 产物与构建来源：哪些仍不能认证

`R/native`精确枚举有wpfgfx_cor3.dll/pdb；`R/clean/aot.sha256`亲读为完整15A545…；clean runtimeconfig全文21行，NETCore与WindowsDesktop均10.0.13。`R/readback/postfailure-candidate/Results/aot-identity.json`全文亲读，同SHA/X86/pointer4。当前AcceptancePaths.cs:51–70、76–82亲读：AssemblyInitialize先验证无CLR、Machine.I386及实际SHA与根摘要相同再写身份。因此PE x86身份有既有初始化链的间接支持，不声称此次重新计算本地DLL哈希/直接读取PE字节。

最近保留build/test日志全60行亲读：实际是本轮COM publish，Exit0/成功，命令显式AotDllPath指向R/native，AotRuntimeIdentifier=win-x86、新R/clean与R/publish、self-contained及两framework10.0.13，末两行产物输出成功。原输出52785字符，仅末20000保留，前32785不可读。它不是独立COM build，也不是Native publish，不能套用133warn/0err汇总。日志未提供这两项历史独立调用原件。包来源未完整逐文件审计；猜测的clean/package-manifest.txt精确读取不存在，名称搜索manifest无命中，不据此声称正式发布未生成任何清单。

first-validation报告声明显式Release build1/3711warnings0errors、测试内隐式Debug build1、定向调用1/4通过，且无TRX记录器、stdout前部截断。本审计亲读该报告及四单元源码，但当前最近日志已是上述COM publish，未定位第一门独立build/test原日志，故仅保留其“历史报告记载”，不把报告自述的亲读冒此次原日志认证，更不以COM日志认证4unit。

## 当前源码静态核实

亲读 `Code/WpfGfxShape/Core/PartitionScheduler.cs` 管理分流/失败回滚：Register/Destroy不继承业务Failure；失败注册仅未持有IsRegistered时Destroy。`Core/SameThreadChannel.cs:330–352,640–670`有TryAdd检查、成功置IsRegistered、仅持有注册时移除。`Abi/UceChannelExports.cs:139–242`有先保留表项后Submit、未接受回滚、worker失败后移除及断连引用闭合、Destroy外部等待completion HR。当前三文件确有修正，未修改源码。

`Tests/WpfGfxShape.Tests/PartitionSchedulerTests.cs`四公开MSTest确实存在：late注册成功、flush保错、关闭source后共享存活、独立partition健康；helper的未知wire command经真实router触发88980406，无直接赋Failure。完整130行已读（首段输出截断后的108–130另读）；未将源码存在当成运行通过。没有可用源码构建指纹将当前所有字节与新Native构建时刻完全锁定，静态一致不补足来源缺口。

## 账本与阻塞恢复

|项目|前执行声称|此次可复核|
|---|---|---|
|Native publish|1成功、新15A545…|产物路径/摘要及实际加载身份成立；独立publish日志/调用次数未证|
|正常COM build|1成功133warn0err|未定位独立原日志，不认证该汇总|
|COM publish|1成功|最近日志认证一次成功；不认证全局只有一次|
|四task|四体全通过|四套监督/TRX/business完整认证；不认证全局无额外任务|
|托管第一门|Release build1+隐式Debug build1+4unit通过|报告及源码可读，原build/test日志此次未定位|
|本审计新增执行|0|0，无预算消费|

旧红原件 `WpfGfxShape/artifacts/postfailure-attach/readback/candidate/Results/postfailure-candidate/PostFailureAttach/77900378d8e646d69c8c221994d24cf0/business.json` 首25行本次亲读，绑定旧task c9f330bdff8a479297b58a460383cf7c/20B3、completed=false/cleaned=false/unknownRegistration=true；旧Create88980406/output0的完整历史报告和原件原样保留，不被新SHA通过冲销。全部历史账、旧双架构isolation、direct/public/WGC/KS/KN/Ledger保留。

最小恢复门仍是证据来源闭合，不是直接x64扩张：优先零执行恢复原Native publish、独立COM build及第一门原日志/正式清单来源，并绑定当时冻结源码、新SHA和四任务；若确不可恢复，由外层重新明确授权有界x86来源与验收预算后执行，不得使用本阶段0预算补跑。原边界为Native≤1、COM正常build≤1/publish≤1、同clean四精确角色各≤1、内120/外180、任一步失败即停，无重试/full/x64/显示/KN17；是否及如何替代缺失第一门日志须另行明确授权，不暗中重跑4unit。

生产修正仍是待完整Native ABI来源验收的候选；承认已亲读有限ABI绿，但不升整体基线/不宣称完整修复已最终保留。此次只完成交接审计和文档同步，完整替换Open。
