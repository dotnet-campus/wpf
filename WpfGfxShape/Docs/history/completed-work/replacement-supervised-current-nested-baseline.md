# CD 当前源码 nested 基线：执行完成，产品差异未闭

## 1. 范围与过程记录
原初稿记录：隔离 CB 历史归档取证；不修改生产、NativeRun 或项目。产物根 `artifacts/cd-current-nested/`。x86 已消失旧归档不再追索；归档 x64 与当前源码证据严格分离。原目标是建立当前撤回后源码的本地 x86/x64 候选基线，未执行完不声明修复或业务验收。所有执行仅专用工具，无终端或脚本。

CD 已执行当前源码双架构基线但未补报告；本最终稿由获授权的 CE 独立亲核既存原件后补完。CE 不构建、发布、运行或修改代码。结论是**当前双架构软件内容基线已建立，precision 未修复，完整替换未完成**；不是恢复历史 BZ 修复或整数 fill。

## 2. 原件亲核范围与执行身份
以下路径相对上述产物根。令 R 为 `run-<arch>/Results/cd-<arch>/`：CE 全文读取 R 下父 `TestResults/child.trx`（26行）、`supervision.json`（38行），以及 `cd-baseline/{official,candidate,official-repeat}/` 各角色同名 TRX/监督（各26/38行）、`identity.json`（各14行）。共 **8份TRX、8份监督、6份actual identity**；两份 `cd-baseline/comparison.json` 均以1–75、76–152完整读取。不用stdout代TRX，不因目录检索漏列TRX就报缺失。

|架构|远端task|父监督UTC起止（2026-10-20）|父监督/测试PID|角色PID official/candidate/repeat|
|---|---|---|---|---|
|x64|96401b7596fa44908b0a6e0825eb6c49|07:59:12.9955334–07:59:15.9310722|1276/6496|8556/3356/4236|
|x86|eba0acac405e4fa3888107a200bb4dd7|08:00:00.0367986–08:00:03.7915716|6532/10316|8276/7716/6336|

八份TRX均 Completed，total/executed/passed=1/1/1，failed/error/timeout/aborted/notExecuted=0；八份监督均 Reason=exited、ExitCode=0、HasExited/OutputCollected=true，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false、SupervisorError=null。父上限120秒，角色45秒，筛选 `FullyQualifiedName~CdCurrentNestedBaselineTests`，TRX落在对应任务内绝对TestResults。**父1/1及角色各1/1仅证明采集完成，不是产品exact通过，也不是8个业务场景通过。**

同一架构父及三角色的测试身份一致：
- x64 SHA256 `10741D3A06A1D6EED2D4375886C2D77D9EEA200BB7B044AE370C92B4DC326E60`，MVID `6922df89-9db3-4b2e-afe8-9c0e0f3f46ec`。
- x86 SHA256 `FDDA44A402D610745E58AB5361623C9F96B8E976DDAB524A89F6B563E93FA87E`，MVID `2916e482-c63d-482a-9ee2-6e7b98bdbcbb`。

## 3. 实际加载，不是官方冒充候选
六份identity均 requested=actual，runtime `.NET 10.0.13`，X64/8或X86/4，backend=`RenderTargetBitmap software`，hardwareVerified=false。实际候选为各task根 `Native/wpfgfx_cor3.dll`；官方及repeat为各task根 `wpfgfx_cor3.dll`。不是仅依据AotDllPath或目录名判断。

|架构/角色|实际模块SHA256|版本|
|---|---|---|
|x64 candidate|F45485F1C747361D89E31FDB667A2C8C1C4920CE6BD6982A6AE93532C2E5A03C|1.0.0.0|
|x86 candidate|09CD6C360304E02AEFF873BFDCB082CEC8CFC55D0736238C81E378EF1D81FABB|1.0.0.0|
|x64 official及repeat|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|10,0,1326,50504|
|x86 official及repeat|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|10,0,1326,50504|

官方identity版本还含 `@Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699`；这是官方版本元数据，**不是本仓整体git修订hash**。候选实际SHA与各自官方不同，也不同于CB/BZ历史候选，不移绑历史full。

## 4. 两架构完整18场景
两份comparison均 threshold=0、evidenceOnly=true、exact=false、hardwareVerified=false。每一行 nonempty=true、repeatDifference=0。下表是完整Pbgra32的不同**字节数**，不是像素数；两架构值相同。

|场景（省略.pbgra32）|x64对官方|x86对官方|
|---|---:|---:|
|ellipse-fill-identity|0|0|
|ellipse-fill-nested|18|18|
|ellipse-stroke-identity|162|162|
|ellipse-stroke-nested|128|128|
|ellipse（窄precision）|32|32|
|group-fill-identity|0|0|
|group-fill-nested|0|0|
|group-stroke-identity|0|0|
|group-stroke-nested|0|0|
|rectangle-fill-identity|0|0|
|rectangle-fill-nested|0|0|
|rectangle-stroke-identity|0|0|
|rectangle-stroke-nested|0|0|
|rectangle（窄precision）|64|64|
|rounded-fill-identity|0|0|
|rounded-fill-nested|18|18|
|rounded-stroke-identity|106|106|
|rounded-stroke-nested|60|60|

两架构nested ellipse候选SHA均 `59AC4CA00ADDCA88B512547303933E4F5B13DF12D805FC2D7663794EC361E223`，官方 `04D1678628CC336289DB38A00D1F1E37431468BAD438952EF9F45B18998EC91C`；rounded候选 `EA57D12FFAB7B2F9C9C44B4F24F42906A13E35650E82F7A07C3C1BD3D50B9C2E`，官方 `D85CCE08DB5A13D0F11371168129AF746785A08B161DD7665AC8154BF1819805`。

这是亲读比较器输出，CE未独立重算二进制像素或DLL哈希。结合CD测试源码的逐字节比较、三方alpha非空和官方重复断言可确认本基线的取证含义；不能将跨轮相同计数代替未来同程序集旧新完整输出比较。

## 5. 默认隔离、构建及源码边界
CE亲读 `Tests/WpfGfxShape.ComAcceptance/CdCurrentNestedBaselineTests.cs` 全文98行：整个文件受 `CD_CURRENT_FILL_DIAGNOSTICS` 隔离；采集后只记录exact，不断言exact=true，非空及official repeat仍强断言。反射调用Bz/Bw的RenderContent，实际模块由进程模块路径取证。CB文件开头已受 `CB_ARCHIVED_FILL_DIAGNOSTICS` 隔离；30–45行仍保留缺归档失败及SHA检查。CB报告第6节“尚未隔离”是其历史时点，不再作为当前默认状态。BW源码1–65仍有默认TestClass/TestMethod、非空零阈值及Assert.IsTrue(exact)，精准失败没有删除或skip。

默认ComAcceptance发布日志 `default-x64/build.log:2417–2423`、`default-x86/build.log:2418–2424` 亲核各97warning/0error；发布输出行分别见日志发布路径记录。显式CD日志 `experiment-x64/build.log:2419–2425`、`experiment-x86/build.log:2419–2426` 各101warning/0error。不是零warning，不认证最终双项目四格。`list-default-{x64,x86}`目录存在，但CE未取得落盘完整发现清单；不声称亲核默认总发现数或全清单。**CD本次筛选不是默认BW运行；不得声称默认BW已跑。**

可准确列出的相关源码核验清单：CdCurrentNestedBaselineTests.cs全文；CbNestedFillBaselineTests.cs 1–28及30–45；BwCurvedFillPrecisionTests.cs 1–65；`Code/WpfGfxShape/Abi/NativeShapeReader.cs:24–75`、`Core/GeneratedImageTransform.cs:1–45`、`Core/SoftwarePathCoverage.cs:266–405`。后三者仍见共享Flatten后Apply、double Prepend/Apply及ellipse/rounded本地double构点/group递归。清单是亲核范围，不是完整dirty清单、全部生产文件快照或整仓git hash；CE无生产修复、无代码改动。

## 6. 与CB历史及下一步
[CB最终报告](replacement-supervised-nested-fill-baseline.md)的x64归档旧新nested18且完整输出未变为报告引用；x86旧归档缺失事实继续保留。CD建立了当前x86/x64本地基线，不伪造缺失归档，不再把找回消失归档设为继续工作的前提。本地 `native-{x64,x86}`、`experiment-{x64,x86}`、`run-{x64,x86}` 产物可复用，复用时再次核对应实际SHA。

下一包以CD当前双架构DLL作为old，同架构同一测试程序集运行old/new/独立官方及repeat，恢复**限定curved精度补丁**；要求窄rectangle/ellipse非空零差、nested及stroke新旧完整输出无新增回归。nested18已存在于当前未修源码，不能一见对官方失败就撤回精度补丁；也不能据此豁免新增变化或宣称nested已修。不同时恢复整套整数算法。原44/10、BD旧x64 full1740/1699/41 Exit2及x86缺父完成TRX保留，不绑定本次身份。

真实GPU、loss/reset新帧、持续媒体、长期资源线程callback/关闭、通知、圆角、合法大shader及E_HANDLE/首次堆损坏仍开；最后稳定源码Tests/ComAcceptance×x86/x64 Release完整适用四格仍须完成。本报告收口只完成文档与当前基线取证，不替代最终验收。
