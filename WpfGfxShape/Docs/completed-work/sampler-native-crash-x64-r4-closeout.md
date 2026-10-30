# sampler-native-crash-x64-r4：离线符号推进，根因未闭合

状态：BudgetExhausted / RootCauseOpen。不是已验证修复，也不是已确证平台符号能力阻塞。[实时执行账](../../artifacts/sampler-native-crash-x64-r4/execution-receipt.md)。原r3证据、23红和§1–10保留。

## 实际运行与身份
normal COM build2/2（工具均成功，149warnings/0errors，6.87秒、4.23秒）；COM publish2/2（准确绝对COM csproj、SamplerNativeCrashR2、win-x64、net10.0-windows、selfcontained true、RuntimeFrameworkVersion10.0.13、ComAcceptanceSandbox=true、632F AotDllPath、r4独立clean/publish），两次完整11行日志exit0。各含隐式build。normalbuild日志未全部回读，不冒完整stdout。Nativepublish0/full0/official场景0/candidate场景0/CLI脚本委派0。

sandbox2/总3，均离线且没有加载生产模块执行场景。第一task c73a4803573c4bad8cab401113b12828，MVIDfdcb65fd-6b88-4533-a665-f4d132eb6809；第二task f7a487f86bfd48a492c44244f69ed3e7，MVIDb06594e5-a2b6-4d61-b1b0-09396cde3429，PID8272/.NET10.0.13/X64。第二offline超原offline1角色分配，属执行偏差；并未超总沙箱数，未动角色重崩。父OS exit工具未返回，Unknown，不冒exit0/TRX。

## 匹配符号结果（不能编函数）
实际official SHA256 AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，SizeOfImage1978368，与r3相同。PE RSDS GUID8756f755-10f4-48be-9886-22c9a80c75b8 age1。初次小写key HTTP503；修为大写标准key后HTTP200：
`https://msdl.microsoft.com/download/symbols/wpfgfx_cor3.pdb/8756F75510F448BE988622C9A80C75B81/wpfgfx_cor3.pdb`
PDB20287488bytes，SHA256732FB582AF469CA52FD3B1C39520FFAD892EE6E9FAFDA1BC69A6066EF6212582，已随整包回收。原件`artifacts/sampler-native-crash-x64-r4/symbol-canonical-readback/Results/sampler-offline-symbols/symbols.txt`10行全文回读；PDB禁止文本读取。

SymInitialize true/error0，SymLoadModuleEx返回0x7FFC17C60000/error0，SymGetModuleInfo64 true/error0但SymType0、PdbSig70全零、age0、LoadedPdb空；Authenticated=false，SymCleanup true/error0。入口在此安全返回Unknown，未调用SymFromAddr/line，不得说RVA B65F已解码。下载key不是PDB内部GUIDage验证；没有证明PDB不匹配。没有独立MSF7 info读取/加载失败细分，当前结果不能归因于工具仅支持托管，也不能说微软server不可达。当前唯一缺口是该已下载PDB的内部GUIDage及DbgHelp实际加载/符号结果。现有C# API能继续取得；本轮预算耗尽是本代理实现/执行不足，不是确切符号能力阻塞。下一应复用此PDB，先MSF7 stream1验证GUIDage，记录DbgHelp显式强制加载与SymFromAddr错误，不需重崩或建调试器。

## 已亲核ABI及仍未知
- VisualCache250–307：Bind→92byte target→root→CloseBatch→Commit；commit S_OK不代表异步消费成功。原secondchance仍PID9876/TID168 read-null/RVA B65F，不归候选。
- BindTwoInputEffect543–600：shader/implicit/secondary/effect handles均先resource创建；shader20byte header+tokens；effect80byte header+2byte float register+16byte float values+16byte sampler info+8byte handles=122。字段offset40 shader、44=-1、48=2、52=16、72=16、76=8；sampler(0,1),(1,1)，依赖implicit/secondary非零。当前源码GeneratedProcessUpdate3563–3650/3750–3838按同一段序解码；ShaderEffect117–140按TYPE_BRUSH查句柄并RegisterNotifier。未发现足够独立确证的包尺寸错，不做猜测修fixture。
- shader CTAB input0/input1与声明s0/s1及ps_2_0 tokens可见；未用其证明当前commit官方消费完全等同本地源码。
- VisualCache131–159：bitmap AddRef通过vtable[1]再发送24byte ImagePacket，draw资源先建；d3dimage.cpp180–228明确偷取channel预先AddRef，software bitmap允许null。本地证据支持此引用转移模式；CInteropDeviceBitmap具体返回指针布局/实际崩指令尚未验证。
- InteropDeviceBitmap.cpp29–82九参数与fixture声明相符；尚未完成所有COM派生布局、brush nullable/render路径、target/官方匹配commit全部核验。不能把静态局部一致当排除全部fixture误用。

## 真实变更、回撤与验证边界
新增`SamplerOfflineSymbols.cs`，仅SAMPLER_OFFLINE_SYMBOLS编译；NativeRun独有--sampler-offline-symbols入口，未调用生产场景。csproj仅专用SamplerNativeCrashR2增加符号并改r4输出。原fixture、expected、阈值、skip、重试和生产均未改。离线能力两次实际运行，如上Unknown，不冒测试绿。
`SamplerNativeCrash.cs`最终rootWait由0改5000ms，新增rootSignaled/rootWaitBudgetMilliseconds/exitDebugEventReceived；仅诊断父最终等待，最多5秒，EXIT事件不等于reaped。两次编译通过但未运行此分支，原rootReap258事实不覆盖。回撤：去掉该等待字段改回0；删离线文件与NativeRun条件入口、csproj符号，专用输出恢复r3（不要覆盖原证据）。

## 终态
预算已无build/publish，不新增未编译改动、不运行未经确证的official/candidate。此次未达到用户“可验证修正或确切符号能力阻塞”目标，应明确失败而非虚报闭合。原x64 1823/1803/20fail、x86 1823/1820/3fail不减；显示/HW/target/backend、真实loss、音频、自然趋势/worker退出/final full与完整替换Open，memoryGreen=null。
