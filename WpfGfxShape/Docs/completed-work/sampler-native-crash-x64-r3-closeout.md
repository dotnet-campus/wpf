# sampler-native-crash-x64-r3-closeout

后续[r4报告](sampler-native-crash-x64-r4-closeout.md)：原证据不覆盖。精确PDB已下载，但DbgHelp身份未认证、函数Unknown；预算耗尽而非确切平台符号能力阻塞。root最终5000ms等待已编译未运行，不能回写本r3的rootReap258。

状态：CloseoutCompleted / OriginalSecondChanceCaptured；原官方sampler取证目标达到，根因与完整替换仍Open。实质草稿先于执行创建。[逐次回执](../../artifacts/sampler-native-crash-x64-r3/execution-receipt.md) / [精确调用JSON](../../artifacts/sampler-native-crash-x64-r3/invocations.json)。

## 执行与预算
正确normal COM build1/最多2，明确绝对csproj、SamplerNativeCrashR2、win-x64、net10.0-windows，exit0，146warnings/0errors，7.35秒。64行可见日志全部读取；stdout114429字符中前94429被工具截断，非完整原始stdout。无需第二次编译。旧r2 null误根solution exit1/TraceWpp+fxc缺失仅历史调用错误，不是COM阻塞，不改SDK。
正式COM publish1/1 exit0，包含隐式build，不代替前述独立build；完整11行日志已读。显式selfcontained true、RuntimeFrameworkVersion10.0.13、ComAcceptanceSandbox=true及632F候选AotDllPath。专用配置保持SamplerNativeCrashR2名称，但publish/clean改独立r3绝对目录；aot.sha256与runtimeconfig21行、package-files622行全部回读。未运行publish根。
唯一Windows sandbox1/1：外300秒，内120秒、超时泵送至135秒及清理最多15秒。Nativepublish/full/其它测试/终端脚本/再委派均0；生产/SDK/shader命令未改。成功build后不能挪剩余build去再次运行；本轮没有重试。
沙箱真实return仅MSTest v4.0.1 win-x64 .NET10.0.13 banner及回收路径，未报告父OS exit，父退出Unknown；不能将工具正常返回当父exit0。task从实际identity为f1a166f650014e1c9fb94f41ffaeb669。子进程exit有debug EXIT事件与exit.json双证：3221225477/0xC0000005，5.3948478秒，timedOut=false。

## 原case、初始化和PID
唯一专用父入口 --sampler-native-crash Results/sampler-native-crash-r3；创建同exe --sampler-native-child，MTP builder/AddSelfRegisteredExtensions/Build/RunAsync，准确FQN WpfGfxShape.ComAcceptance.D3DImageTests.WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain，无DataRow、无额外manual test。
DEBUG_PROCESS CREATE_PROCESS PID9876/TID10096；historical-role.json确认同case PID9876、JH_HISTORICAL_ROLE分支直接运行已初始化scene，不二次隔离。stages首行ClassInitialize completed pid=9876；official=True/geometry=False/cache=False/colorFF00FF00/expected0000FF80。35行stages全部回读，末尾依次native shader、bind effect、target、close、commit returned HRESULT=0x00000000；没有consumer after/ClassCleanup completed，不伪造正常清理。

## 真实异常现场
证据根：`artifacts/sampler-native-crash-x64-r3/readback/Results/sampler-native-crash-r3/`。

|字段|实际值|
|---|---|
|异常|0xC0000005，firstChance=false，parameterCount=2|
|PID/TID|9876 / 168（同fixture/case PID）|
|ExceptionAddress/RIP|0x7FFC17C6B65F|
|RSP|0x78A963EF00|
|访问|operation0（读），inaccessibleAddress0x0|
|actual loaded official|task根wpfgfx_cor3.dll，非Native候选|
|module base/SizeOfImage|0x7FFC17C60000 / 1978368|
|faultOffset|0xB65F|
|official SHA256|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|
|official version/commit|10.0.13-servicing.26504.104 / 4099f83977f4d29604a9aeb971fb0a7bc1d5f699|
|candidate SHA256|632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671（包身份，不当故障模块）|
|test SHA/MVID|F4ED7D7DF752E46D519BCE914E15396EC89CD9845A067D449F23DA3357DA120A / 486533a6-d589-4ab5-a127-95f23d6b9262|
|context|captured=true/contextError0，1232字节rawContext Base64已保存|
|dump|second-chance-9876-168.dmp，written=true/error0，1652255字节|
|dump SHA256|1851A7517FFE42FB688BD098EBF463624F59180BF183AA71ABD90E872451B85C|

exception861行（含完整模块表）、events628行（含moduleLoad及末行）、context9行、dump sidecar6行、identity15行、historical-role、aot-identity、hw-module10行、exit9行、stages35行均已分段亲读；超长阶段/清单返回后补读尾段。child stdout/stderr实际为空；精确child.trx名称搜索无结果，未生成TRX，不报测试Passed。异常前仅loader breakpoint80000003、firstchance04242420、firstchance000006B5；没有先E0434352。events583/586为同地址同TID first/second C0000005，均NOT_HANDLED并Continue成功。它是原崩现场，不是旧未初始化异常。

## 调试器加固与尚存缺陷
证据Record I/O失败写stderr并保存错误清单；模块hash/PE读取失败不改变异常续行。异常状态预设NOT_HANDLED，finally Continue；API错误立即取值；超时/异常清理仅处理自有debug树，Terminate后最多15秒泵送事件，再Stop/零超时reap；不停止别人、不改全局WER。此次全部Continue及已记录关闭error0，evidenceErrors=[]，没有触发超时/Terminate分支，不能声称这些失败分支运行已证。
实际尾事件624 processExit9876=C0000005；626 Continue EXIT成功；627 rootReap=258/WAIT_TIMEOUT（零毫秒）而非signaled，628自有线程/进程句柄关闭成功。因此只能证明EXIT调试事件已收到续行，不能证明最终内核进程signaled/全部worker reap。具体缺陷是正常EXIT后最后WaitForSingleObject使用0导致退出竞态；后续如再授权执行应在最终close前共享总deadline有界等待并保留wait/LastError，非本轮偷改未验证产物。
当前原始dump为MiniDumpNormal且exception stream=false，context单独保存；stackDecoded=false，没有可靠native符号解码，不编造函数或栈。一次目录级文本正则意外扫描.dmp并返回片段，违反dump不文本读约束，已如实记账；片段未用于推断，之后仅精确JSON读取。debug loader/module hash/exception暂停会改变时序，必须披露，但本次确已复现。

## 下一源码定位与计划终态
可靠定位键为官方commit4099f839…、SHA AD1C…、RVA0xB65F、PID9876/TID168 read-null；不可把当前仓库任意版本当准确官方符号。下一先用匹配官方二进制/符号解码RVA及dump；然后对照D3DImageTests.VisualCache.cs 250–300的BindTwoInputEffect、target92字节、close→commit后异步消费，与官方shader sampler命令字段/size/句柄跨模块来源和释放次序。已知commit成功并不能证明后台消费安全，尚不能指定fixture根因或修改业务。
本轮步骤：原FQN/初始化链核验完成；诊断器错误路径加固完成并编译；正式独立build完成；正式publish/clean身份完成；唯一沙箱原C0000005现场完成；关键证据回读与报告/八入口同步完成。取证切片闭合，不等同生产修复。
原full x64 1823/1803/20fail、x86 1823/1820/3fail及23红保持；显示/HW/target独立backend、真实loss、实际音频、自然资源趋势/全部worker退出、final full仍Open，memoryGreen=null，完整替换Open。
