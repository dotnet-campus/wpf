# IO 当前源码 Tests 双架构 Release 无 filter full

## IO 终态：当前托管 Tests 两格完成（以下初始范围及阶段记录保留）

本轮只验证现有正式 WpfGfxShape.Tests 当前源码 x86/x64 Release 两格；不执行 ComAcceptance，不修改生产或测试行为，不新增 Host/工程或脚本。先核规则、完整方案、IN 与旧 BC 证据及入口身份，再使用独立 artifacts/io-managed-full 依赖图发布、清洁输入实跑每架构一次无过滤 full，回读原始结果后分类。旧 BC4256 不绑定本轮身份；IH/IA/ID 与 COMfill 保留，IN 仅指定同步可见 HWND 路径不适用。完整替换及四格未完成。

当前：已核正式 Tests net10.0/MSTest4.0.1；现有 SandboxTestEntryPoint 仅在显式 SANDBOX_TEST_ENTRY_POINT 下启用，已有 full 内实际加载身份测试。历史证据保持不变。

## 阶段记录（追加保留）

1. x86 正常依赖图 Build Exit0；self-contained 10.0.13 发布 Exit0。原始发布含 NuGet 自动复制 coverage/异平台额外资产，未上传；专属 ignored MSBuild 打包配置从根层运行 DLL/JSON/PDB及正式exe选择 clean-x86，排除coverage原生执行器/子目录/异架构资产；保留实际插件托管依赖、不启用coverage。显式 PublishDir=package-x86，clean输入独立于回收；input-x86.txt及.sha256记录实际文件。追加清洁打包发布 Exit0。工具日志58行有stdout前部截断及CA1416/MSTEST0044，不称零警告。即将x86唯一一次无filter full，工具180秒，入口补任务内绝对TestResults。不预设分母。

2. x86 唯一full task ba2ad674489148f68ed51b597681d017 已完整回收；亲读parent TRX:25545–25547=Completed，4256 executed/passed，失败及全部其他计数0。diag:46886–46890自报session Exit0（不是工具独立数值进程Exit）。实际身份JSON已读：X86/4/.NET10.0.13，当前Tests/prod SHA/MVID已取得。无失败、不需定位复跑。x64正常依赖Build及清洁发布均Exit0，runtimeconfig已核10.0.13，即将唯一full。

3. x64 唯一full task 86ccebb9f76f4c4dabf2fd3ee966cf3b 已完整回收；parent TRX:25545–25547=Completed4256/4256，其余计数0；diag:46886–46890 session Exit0。实际Tests/prod SHA/MVID与预flight JSON均已亲读，manifest关键身份一致。两格全部失败集合为空，无定向定位或full复跑。停止测试阶段，进入最终报告/交接收口。

## 正式构建、包边界与记录限制

根：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape`，以下 artifacts 路径相对此根。已读执行规则、完整方案、latest handoff、IN报告、原BC swapchain-fixture-fix及AR/AX入口/依赖教训；共享长历史仅受影响顶部，不冒全文历史审计。

Tests原项目 net10.0/MSTest4.0.1/Nullable enable，既有LangVersion latest未改；global.json SDK10.0.107/latestFeature未改。每架构在独立 `artifacts/io-managed-full/managed-{arch}/obj/bin/publish` 正常ProjectReference依赖图Build，生产为托管wpfgfx_cor3.dll，不是NativeAOT产物。之后同root self-contained10.0.13发布，BuildProjectReferences=false仅复用此前正常构建依赖；EnableMSTestRunner=true、SANDBOX_TEST_ENTRY_POINT及StartupObject只用于Tests发布，生产Build无该宏。未修改任何源码、入口、项目或默认并行/断言/SDK/runtime/TFM。

专用发布调用共5次均Exit0：x86依赖Build、入口发布、附加clean打包发布；x64依赖Build、入口与clean打包发布。无失败构建。x86首Build未指定原始文件日志，其58行工具日志为截断尾段，不能声称原始stdout完整保存；其余有 `publish-x86.log`、`clean-publish-x86.log`、`build-x64.log`、`publish-x64.log` 及两个publish binlog。工具stdout均有前部截断，CA1416/MSTEST0044保留，无NoWarn。发布命令与参数见这些原始日志，正式explicit PublishDir分别 `artifacts/io-managed-full/package-x86/`、`package-x64/`。

clean上传输入只有 `artifacts/io-managed-full/clean-x86/`、`clean-x64/`，绝非项目根、obj、publish父目录或历史回收。专属 `IoCleanPackage.targets` 仅专用构建声明Copy/GetFileHash/WriteLinesToFile，无shell/脚本/C#任务/新执行器，未改共享打包。实际各233文件，完整清单 `input-x86.txt` / `input-x64.txt`，逐文件SHA `input-<arch>.txt.sha256`；尾部Tests/prod SHA与actual吻合。仅根层runtime/MTP/MSTest/Silk与其托管依赖、exe、deps/runtimeconfig/PDB；无Results/TestResults/历史/跨平台目录/coverage采集数据、原生coverage执行器或异架构instrumentation。保留自注册MTP coverage插件所需托管DLL，并未启用coverage。未传ReferenceData；本轮full完成未见缺数据失败。两包总字节：现有专用文件工具不提供文件大小/合计，未取得，明确未知、不编造压缩/传输大小。清单/逐文件hash可得不等于总字节可得。

## 两格唯一full及完整回收

每架构一次正式 `WpfGfxShape.Tests.exe`，工具timeout180秒；完全无filter，无ComAcceptance调度器，无额外监督进程。参数 `--report-trx --report-trx-filename io-full-<arch>.trx --diagnostic --diagnostic-verbosity Trace`；原入口自动补任务内绝对TestResults，实际完整参数已从diag第10行核读，不传不支持的full期限参数。默认Workers4/MethodLevel，原DoNotParallelize保留。

|架构|task|parent TRX（任务内绝对）|Completed与Counters|Exit层级/耗时|
|---|---|---|---|---|
|x86 Release|ba2ad674489148f68ed51b597681d017|`C:\CodingAgentSandbox\Tasks\ba2ad674489148f68ed51b597681d017\TestResults\io-full-x86.trx`|Completed; total/executed/passed=4256|MTP session Exit0；runner3s235ms|
|x64 Release|86ccebb9f76f4c4dabf2fd3ee966cf3b|`C:\CodingAgentSandbox\Tasks\86ccebb9f76f4c4dabf2fd3ee966cf3b\TestResults\io-full-x64.trx`|Completed; total/executed/passed=4256|MTP session Exit0；runner3s002ms|

两TRX均25548行，Counters:25546全部 failed/error/timeout/aborted/inconclusive/passedButRunAborted/notRunnable/notExecuted/disconnected/warning/completed/inProgress/pending=0。Failed/ErrorInfo/RunInfo文本搜索均无命中，不虚构RunInfo退出字段。实际Exit来源是两份diag:46886的session ended with exit code0及46888 exit code0；工具输出本身未提供独立数值进程Exit或监督JSON，因此不造监督Exit。启动/传输/回收没有报告错误，两实际完成TRX与身份已取得；无原生中断/超时，不能声称有独立kill/drain监督检查。

全任务回收到 `artifacts/io-managed-full/full-x86/`、`full-x64/`，原始TRX/diag/Results全部保留。diag文件分别 `TestResults/log_261025041417514.diag`、`log_261025041537210.diag`（各46902行）；`execution-observations.txt`保存专用执行参数及工具返回文本。失败清单：x86空、x64空；没有失败需堆栈/同因兄弟定位，不进行任何补跑。不将通过冒称真实HWconsumer/backend覆盖证明。

## 实际加载身份（两份JSON全文已读）

`full-<arch>/Results/av-ownership-assembly-identity.json`与`preflight-identity.json`由full内既有测试生成，不增加分母、不用预期路径代替actual。BaseDirectory/ProcessPath/Location均是上表实际task根下exe、Tests.dll、wpfgfx_cor3.dll；两个程序集FullName均Version1.0.0.0、PublicKeyToken=null。实际x86=X86/4、x64=X64/8，两者.NET10.0.13，diag runtime location指向各task内System.Private.CoreLib.dll，runtimeconfig includedFrameworks10.0.13。没有本轮WPF/官方NativeAOT加载证据，不能沿用IH Native hash充当本轮托管prod。

|架构/actual程序集|SHA256|MVID|
|---|---|---|
|x86 Tests|DC761366642DD27AAFB460E769D46702F2AF6E086DE4DE93DBFBA30077E4C45A|05f7e452-9404-48ea-8530-4680538ab779|
|x86 managed prod|5265E6245480D62109E49484022F7C98D42485AB2E2E259520332330B16D0612|3a212af5-2f32-4c53-8270-84089e67e87f|
|x64 Tests|E96D259EC72111CB5875C7885005A42CC9766634E1FA0859F77BF016C17FEDE9|afa3c3e5-c7e4-4699-b481-d42836deb709|
|x64 managed prod|7FE8B39C2246444F31D8BF5FAB56F4FC59011B94FB23E4E1F7E9B8930CA3291C|5e169c11-010c-4c9e-8d06-208a599f908a|

## 裁决、历史与下一唯一门

当前源码托管Tests两格完整full已完成；计数恰与旧BC4256相同，但本轮来自新构建和实际身份，独立分账，不移绑BC。生产/测试源码修改0、NativeAOT重发0、COM运行0、full仅两次，定向复跑0；不修测试以追绿。IH动态状态、IA初态、ID广播、COMfill保留。IN仅限定同步可见HWND协议源码不适用，不排除COM全项目、不skip历史失败。

下一唯一实际门：另行授权后，以当前源码、正式ComAcceptance清洁双架构Release包取得当前无filter COM基线并分类真实失败；若同旧DP原生中断，保留精确parent缺失及诊断进入最小根因修复门，不能拼分组代替full。本轮不执行。旧DP崩溃full、旧BD41失败身份继续历史，不因本轮托管绿关闭。HWconsumerbackend/loss/Available/media/resources/full四格/x64Event0仍开放，完整替换未完成。

最后补记（IO）：报告全文及handoff/session-summary/issues最新IO受改节写后回读，历史只保留、不反复全量审读；原始TRX Counters、diag参数/sessionExit、四份actual/preflight JSON已读，不以已跑未读交付。包总字节未知及独立工具数值Exit缺失如上明确披露，不造数值。
