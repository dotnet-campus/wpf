# x64 official window 有界取证闭环

## 终态：ExecutedFailed / NotReached / CloseoutCompleted / OverallOpen
当前代理亲自静态核查、正常分析器构建、正式发布并在Windows沙箱执行一次。无终端/脚本/再委派/新Host/第三工程/解析程序。没有修改生产、expected、skip、NoWarn、SDK/TFM/依赖；没有关闭分析器，没有把completed.Wait改为超时返回。诊断收尾完成不等于原hang修复。

完整即时收据：[official-window-hang-bounded-receipts](../../.coding-agent/official-window-hang-bounded-receipts.md)。本页取代旧“After待填写”草稿。上轮normal2/COM2失败/sandbox0独立保留；前referencepublish显式RunAnalyzers=false不是分析器认证。

## 构建与正式包
- 本轮normal1：绝对d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.ComAcceptance\WpfGfxShape.ComAcceptance.csproj，OfficialWindowHangDiagnostic/win-x64/net10.0-windows，正常restore；exit0，168 warnings/0 errors，3.84秒。日志64行全读，stdout132119字符，前112119截断。
- COMpublish1：同配置，self-contained，NETCore/Desktop10.0.13，PublishAot=false/WpfGfxShapeManagedAcceptance=true/AotRuntimeIdentifier=win-x64/ComAcceptanceSandbox=true；exit0。日志58行全读，stdout1697792字符前1677792截断，warning总数Unknown，可见真实CA/MSTEST诊断，不冒称零警告。
- AotDllPath实际存在stage19-final-crossarch/clean-x64/Native/wpfgfx_cor3.dll。全新official-window-hang-bounded-r2/publish与clean互不包含；正式targets复制与GetFileHash产生sidecar，D9AA1E0ED5E99B29F10AC2DA4635D7F014B0F433C05DF2E5B18F2028836CD09C，非手写。runtimeconfig完整读取NETCore/Desktop均10.0.13。
- CA1838修正已正常分析器验证：[Out]char[32768]传Length，返回零或>=Length断言失败，按返回长度构造actualModulePath，仍按actual路径及Process.Modules实际文件hash比较。finally失败记录不throw与SARIF父目录修正已随正常编译发布成功。

## 一次真实执行
工具参数详见receipt。source=新clean，exe=WpfGfxShape.ComAcceptance.exe，args=--sampler-native-crash Results/official-window，外300秒；task421037ce806e4b708e6c7f8165ffd0ac。原件根：WpfGfxShape/artifacts/official-window-hang-bounded-r2/readback/Results。

专用入口DEBUG_PROCESS启动正式MTP4.0.1，精确WhenFreshNativeGatePassesThenOfficialAndCandidatePublicTargetsAreCompared，真实ClassInitialize/ClassCleanup。child PID11752/调用nativeTID12884/ownerTID11436，与debugtree创建/退出及窗口0x4A000B4身份关联。TRX130行必要全文补读：1执行/0通过/1失败/0skip/0timeout/0aborted；case23.0068439秒，MTP23.676秒。监督exit.json：child2，24.4813484秒，timedOut=false/secondChance=false/evidenceErrors空。工具父层无数值OSexit，Unknown，不能拿child2替代。

失败在第一个swapEffect1 native fresh-frame gate：三帧backbuffer红蓝红正确，submitThread=owner11436，Present0，但client/screen0x00F3F3F3、front0xFFF3F3F3，AcquireNextFrame反复0x887A0027/resource0直到业务期限。Basic Render Driver1414:008C，Default桌面session1，窗口可见未cloaked且foreground/cover自己。swapEffect3和official/candidate public阶段均未到。源码先两native门通过才写bj-public-stages，故未生成。结论严格为NotReached，不是NotReproduced。

## 身份、线程与cleanup边界
- testSHA69A1BBACF6D7C374A28D8BA8CDAB5BD98125139A5821D50144731A2C7F8A1DDB，MVID29274d43-5050-416a-9f0c-10cf87cbb1fb，.NET10.0.13/X64/pointer8。
- candidate文件D9AA全hash如上；official文件AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1。这是输入文件身份，不冒称本次actual已加载official；未达public加载，事件无wpfgfx模块加载记录。
- stages28行完整回读：queue-ready、enqueue、部分posted/start/end/completed及owner-loop-exit/ClassCleanup存在；7/13/16/18/21残片，不能称完整队列阶段证据。根因可静态确定为ExitDiagnostics.Stage多线程独立Append FileStream未同步；不是窗口死锁证明。
- 未到60秒，不触发DebugBreakProcess；无CONTEXT/dump/等待线程moduleoffset，HwndSourceRootContextIdle未到，不编造native栈。静态CONTEXT_ALL_AMD64/aligned1232、debug event暂停后Continue恢复，无显式Suspend/Resume不配对；异常路径有自有树Terminate/事件drain/reap有界，未实测异常分支。
- events692行首尾与关键线程检索已读，正常childExit2后Continue成功，rootReap0/rootSignaledtrue/exitDebugEventReceivedtrue；processHandle及根thread/process Close成功。未强杀，不把kill冒正常cleanup。ClassCleanup真实完成，但COM资源refs/各HRESULT未单独证明，仍Unknown。

## 下一直接修正与预算
可信结论仅native前置门未取得所属新桌面帧；其底层交付原因及旧official hang具体原因未证。不改expected、不旁路native门追hang、不在同包再跑。
下一直接代码修正：仅专用诊断阶段写入加锁覆盖open/write/flush，保留原件残片；随后在能满足原native门的真实桌面条件下同一正式case再取证。没有证据支持改窗口Invoke等待/Dispatcher/生产代码。本轮未做未验证源修正。
实际normal1/COM1/sandbox1/Native0/生产0/full0。第二normal仅确定编译错复验额度未用；第二sandbox仅确证修复验证，本次不满足，不消费。COM预算耗尽，任何未来日志修正后正式新包运行需另授权COMpublish1+有界sandbox1，禁止旧包冒fix。

旧x86 1843/1752/91不相减；旧x64 taskdd603767c95a4d4c971174604c968c95 1500秒timeout分母Unknown独立；旧有限8+2保留但referencepublish关闭分析器记录不洗白。audioendpoint0、AVI16gap、AVsync/cancel、HWloss、resources/defaultfull均Open/Unknown。整体替换未完成。
