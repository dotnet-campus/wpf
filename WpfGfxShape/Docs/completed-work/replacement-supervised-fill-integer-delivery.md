# CI fill 整数细分交付

## 1. 最终裁决

**本轮未交付整数生产修复；保留 CF/CH 已验收有限行为。** 调查及基线运行已完成下述有界部分；受本轮执行预算及证据缺口限制，选择用户允许的“不改生产、准确记录阻塞”分支。没有新增整数挂接、float 参数或备用算法，因此没有待撤回的本轮生产行为。原44/10、nested18、完整替换及最终四格仍开放。

本报告先创建后更新；独占产物为任务根 `artifacts/ci-fill-integer/`。仅使用专用文件工具与 Windows 沙箱现有正式纯 C# ComAcceptance.exe；无终端、直接/间接脚本、新Host、第三工程、原生探针。未编辑 NativeRun、ExitDiagnostics、VisualCache、历史代码、捕获、WindowQ、项目打包或共享交接。

## 2. 文档与源码核查范围

已读执行规则全文27行、完整替换方案全文120行、CH全文59行、CF全文49行、BR implementation全文15行、BT全文79行、BU全文47行。next-session-handoff 已读最新CH/CE/CA与BV相关段，历史长文未全文读完；不声称完成整个历史交接阅读。长返回截断处CH/CF/BT补读末节，未将文件元数据“已读完”冒充返回内容完整。

生产项目net10.0、Nullable enable、NativeAOT Shared；根global.json所读SDK10.0.107、rollForward latestFeature；任务根无global.json，未修改SDK/TFM/语言版本。

### 快照缺失（实际阻塞，不是算法错误）

- `Core/SoftwareFillBezier.cs` 已读为空。
- 任务根 `artifacts/bu-fill-float/` 与工作区根同名路径均不存在。
- 工作区递归名称搜索 `SoftwareFillBezier.cs.snapshot` 无命中。BU报告所述133行快照本轮无法亲核内容；没有把报告中的“存在”复述为本轮事实，也没有用测试侧算法伪装原生产快照。
- 当前任务根artifacts仅见BX/BY/BZ/CB/CC/CD/CF/CG/CH等目录及其他既有项，未取得BR/BU原件。CF正式默认双架构包可复用，已用于下述实跑。缺快照不证明不能重新移植，但重新实现需要额外审查及完整验证预算。

### 当前保留行为

GeneratedNativeStroke.cs全文56行：TryCaptureFill唯一显式fillPrimitive:true，普通TryCapture默认false。SoftwareGroupStrokeContours.cs全文129行：标志递归传到rectangle/ellipse；Path分支仍调用原SoftwareStrokeContours.TryRead；group/combined parent.Prepend及最终点/控制点parent.Apply仍double。SoftwareCurvedStrokeContours.cs全文120行：fill专属ApplyFillLocal float乘加保留；rounded仍double构点后局部float，未擅改来源协议。

NativeShapeReader.cs已读主要实现及末段：本地点先double transform.Apply，cubic共享SoftwareBezierFlattener.Flatten后Apply，再TryCreateFigures；尚无fill整数消费挂接。SoftwareStrokeContours.cs所读起始段及CaptureSegment仍本地double Apply后保存float，无fill专属参数。普通Path本地保存与设备矩阵分离仍是将来修复点，不能仅接整数末端便声称阶段已正确。未修改共享stroke flattener，未认证全仓引用闭包或历史整文件hash。

BrFillIntegerTests默认TestClass/TestMethod五DataRow保留，76–80行亲核cubic与lines各自非空断言及零差断言，未删除、skip或调整阈值。测试侧自含整数算法只支持有界官方oracle，不证明生产接通或一般HFD正确。

## 3. 官方算法有界审查

亲读官方源码（路径相对工作区）：

- core/sw/swlib/aarasterizer.cpp:2945–2975：先float translation减0.5，再矩阵scale16；1270–1337：float乘加、否定式有序范围检查拒绝NaN/无穷，范围由26-c_nShift决定，再CFloatFPU::Round。
- common/shared/real.h:680–707：Round合同half integers shifted up；负半格-0.5→0、-1.5→-1，而非AwayFromZero。将来不能用普通banker round替代；应在独立实际对照覆盖负半格与实际父矩阵。
- core/geometry/bezier.cpp:180–250、264–320：narrow空间/误差选路、lazy shift及输出后先判剩余步数，再halve/double/step；不能允许steps=0进入倍增循环。
- bezier.h:250–350：wide fraction28、high/low误差常量、32失败才64、末端不保证等于原控制点且调用者需补端点。
- bezier.cpp:495–625：wide高层重建low控制点、可见区优化、high先推进及low输出/步长调整次序。

这不是本轮完整逐公式HFD审计：基变换/反变换全部公式、溢出严格界、全部移位、clip外行为及终止的生产实现验证尚未完成；未据文档相似性宣布正确或错误。BT旧报告给出的范围分析和历史五oracle/十帧只按历史证据引用，不移绑CF或不存在的CI新候选。

## 4. 本轮真实基线运行与身份

复用CF `default-{x64,x86}/com-acceptance-package`，不重发生产、不覆盖公共包。Windows沙箱执行现有exe的BL监督角色，120秒内部/180秒外部；筛选BW及BrFillIntegerTests，默认并行未改。结果全任务回收到本独占根baseline-x64 / baseline-x86。

|架构|task|监督与stdout|
|---|---|---|
|x64|3566ea897e384dcd8b19866529972552|ci-baseline-x64/supervision.json全文38行、stdout全文12行；ExitCode0，stdout 6总/6成功/0失败|
|x86|c58624d96dc6493a9b2836867117c4a7|ci-baseline-x86/supervision.json全文38行、stdout全文12行；ExitCode0，stdout 6总/6成功/0失败|

两监督Reason=exited，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut均false，HasExited与OutputCollected均true，SupervisorError=null。不是将沙箱工具“完成”当作进程退出0。

实际测试程序集身份（监督与BW各角色identity全文亲读一致）：

- x64 SHA C4F93D7C435E30A536024DF9157ED4C5B6FBD1F548BE616CD3D485764D6A9FCC，MVID 3d9d716e-e219-4378-9759-c5cc40b68a35。
- x86 SHA C8B0703A432D6F4F5C078B6082BAADB88E9496936F42857285F9E5C4DDEE1A9E，MVID 23f288d5-f1c7-4501-a72a-e3ed3b3f5007。

BW candidate/official/official-repeat六份identity各14行全文亲读：requested=actual，runtime.NET10.0.13，X64/8及X86/4，RenderTargetBitmap software，hardwareVerified=false。候选Native/wpfgfx_cor3.dll实际SHA：

- x64 36353DDE3BA720B642A6F12CEE8AB0EDDA501BD7DC49F35270063480B8A0532A。
- x86 C4D7D4699F6334AC164DE95573D382807803ED65E4FE42AA05A9559FEA7F44EB。

独立官方及repeat实际为task根wpfgfx_cor3.dll，版本10,0,1326,50504，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；x64 SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，x86 SHA 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834。官方不是候选代替，未外推其他runtime。

两架构 `artifacts/bw-curved-fill/precision/comparison.json` 各31行全文亲读：rectangle与ellipse candidate各count0/nonempty=true，official-repeat同为0且非空，threshold0、exact=true。原始rectangle/ellipse.pbgra32与场景JSON随目录回收，但本轮没有逐字节重新解码全部原始缓冲或亲读全部场景轨迹，不虚称完整像素独立重算。Br五例本轮只认stdout及监督结果，其各份业务JSON尚未逐份读完。

## 5. 父TRX回收缺口（尚未纠正）

监督Arguments与stdout给出固定任务内绝对路径：

- C:\CodingAgentSandbox\Tasks\3566ea897e384dcd8b19866529972552\ci-baseline-x64\TestResults\child.trx
- C:\CodingAgentSandbox\Tasks\c58624d96dc6493a9b2836867117c4a7\ci-baseline-x86\TestResults\child.trx

已请求回收整个任务而非仅Results；但本地监督目录列举没有TestResults，独占根递归名称搜索.trx仅命中TRX组件DLL，未取得实际父TRX。stdout声称生成不能代替亲读。当前无法判定是远端文件缺失、回收过滤还是其他原因，不无证归因；也没有把两次6/6冒称父TRX认证。此缺口需后续正式exe证据搬运/回收验证解决，不使用脚本、不修改禁止的监督文件。

## 6. 变更及未完成验收

本轮仅新增/更新本报告；产物写入独占根。生产、测试、golden、threshold、skip、fallback、默认并行及共享交接均未修改。无新候选CI DLL，无当前源码双架构独有正常依赖图发布，不将CF旧包测试宣称当前源码发布验收。

下列尚未执行或未闭：完整公式与边界审计；fill专属整数真实消费挂接；普通Path本地float分阶段；原十帧及有判别力B/E、D/F、负半格；线段/close/arc/rounded/ellipse与真正大曲线有界兄弟；同程序集CF旧/CI新/独立官方完整像素比较；stroke旧新完整像素不变；nested18是否改善；父TRX完整回收与业务轨迹全部亲读；最终适用四格。既有nested18不判作本轮新增回归，历史44/10不关闭。

**交付结论：CF已验收有限修复原样保留，BW双架构基线再运行的有界零差成立；CI整数生产修复与完整证据验收未完成。** 后续优先找回/重建并审计snapshot、解决父TRX回收，再实施小补丁和双架构旧新官方验证，不整覆盖CF三文件，不留未核整数补丁。
