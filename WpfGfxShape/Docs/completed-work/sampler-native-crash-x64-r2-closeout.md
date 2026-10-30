# sampler-native-crash-x64-r2-closeout

状态：CloseoutCompleted / ExecutionBlocked；仅本轮记录闭合，原官方sampler取证目标未完成。实质草稿先于执行落盘，现按失败实况回填。[完整实际工具账](../../artifacts/sampler-native-crash-x64-r2/execution-receipt.md)。

## 事实与授权偏差
唯一构建调用实际参数null，导致工作区根dotnet build，而非COM csproj。exit1，1.47秒，0 warnings/2 errors：TraceWpp.exe与fxc.exe缺失。完整18行日志已全部读取、无截断。此为本代理调用错误，触达Native项目，违反Native0范围；不把它伪记为成功COM build或声称Native调用0。无已生成Native产物证据。不是允许的一次确定COM编译错误，停止后续执行，不借publish过门。

预算：全部build调用1（错误根目标）；正确normalCOMbuild0/最多2，剩余额度冻结；COMpublish0/1；沙箱0/1；full0/其它测试0；Native专用publish0，但错误根build触达Native项目。终端/脚本/再委派0。未创建r2 clean、无沙箱task、无新增异常地址/PID/TID/module offset/访问地址/context/dump/TRX。不能声称已复现，也不能声称未复现（根本未运行目标）。

## 启动链及真实case
r1 execution-receipt13行全文已读，旧E0434352因未经ClassInitialize直接调用，不是原C0000005。当前RunChildAsync使用MTP CreateBuilderAsync→AddSelfRegisteredExtensions→BuildAsync→RunAsync。原FQN由源码确认：WpfGfxShape.ComAcceptance.D3DImageTests.WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain；TestClass/DoNotParallelize/TestMethod，无DataRow。原scene参数official=true、geometry=false、cache=false、color=0xff00ff00、expected=0x0000ff80。

D3DImageTests.WindowThread.cs含ClassInitialize StartPresentationThread及ClassCleanup StopPresentationThread。JH_HISTORICAL_ROLE匹配方法名时HistoricalProcessIsolation直接执行原scene，不再启动supervisor；仍由MTP执行fixture，禁止直接调方法绕初始化。专用环境开关新增完成阶段PID记录；崩溃时ClassCleanup可能不执行，不能伪造完成。尚无实际初始化/cleanup运行证据。

## 本轮未验证源码
SamplerNativeCrash.cs：DEBUG_PROCESS取代DEBUG_ONLY_THIS_PROCESS，跟踪树内CREATE/EXIT事件，按PID区分模块和首次loader breakpoint；独立OpenProcess句柄绑定异常事件PID。second-chance输出异常地址、访问类型/地址、模块range/offset、x64 GetThreadContext raw/RIP/RSP及实际PID dump。普通first/second异常NOT_HANDLED，dump仅MiniDumpNormal，无exception stream，不声称已解栈。补充自有线程/进程句柄关闭及有界终止/reap/Stop尝试。所有改动未编译/运行验证，不是生产crash修复。

专用非默认SamplerNativeCrashR2配置绑定合法既有final-candidate-major-x64-r1/clean/Native/wpfgfx_cor3.dll；其aot.sha256已读632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671。runtime/desktop10.0.13、独立r2 publish和clean；正式根通过既有PackageSelfContainedComAcceptance GetFileHash生成aot.sha256，不手写hash、不运行publish根。默认生产和普通测试分支未改业务。

## 下一源码定位与执行顺序
1. 先复核SamplerNativeCrash.Run错误路径：待处理debug event终止/Continue/Stop/reap顺序，每个API立即保存LastError，模块文件与证据I/O失败不覆盖原异常，完整进程树退出有界；这是未验证代码的安全门。
2. 正确指定COM csproj、SamplerNativeCrashR2、win-x64、net10.0-windows先编译；不得再默认根目标，需重新明确授权后继续。通过后最多唯一publish、核clean清单/aot.sha256/runtimeconfig，再Windows沙箱唯一一次外300秒，内120秒+有界退出。
3. 亲读stages确认ClassInitialize和historical-role实际PID，核debug树覆盖该PID、实际loadedofficial路径/SHA/PE区间；官方root应为10.0.13。若second-chance C0000005，绑定fault thread RIP/context、exception address、module offset和读写访问地址到同一PID原始dump。调试事件/hash会自动暂停并改变原时序，须披露；未崩则真实未复现不重试。所有JSON/events/module/identity/TRX/子输出分段读完再结论，dump不文本读。
4. 有可靠module offset后再定位原native源码commit→flush/消费：D3DImageTests.CaptureHistoricalControls.RunHistoricalCaptureControl→RunVisualCacheConsumer的sampler资源创建/command字段及大小/句柄所有权/释放次序，对照官方消费实现；无符号不编造函数栈，不凭猜测修改fixture。

## 计划与矩阵终态
步骤1静态源码核完成；步骤2仅实现未验证；步骤3编译门Blocked（工具参数错误）；步骤4–5未执行；步骤6正式报告、baseline/failures、六状态导航及完整计划补记并回读。没有新证据可以关闭官方sampler根因。原full x64 1823/1803/20fail、x86 1823/1820/3fail及全部23红保留。显示/HW/target独立backend、真实loss、实际音频、自然资源趋势/全部worker退出、final full仍Open；memoryGreen=null，完整替换Open。绝不宣称默认full变绿。
