# HG 共享 consumer 合同迁移

## HH实际登记（历史过程，最终结论见末尾）：同task两前置均失败，未证单一夹具根因
唯一task4b901b8e7b094574bf51400213e9cb41已回收，子Exit2/nonTimeout/noKill/drain正常。HB原样nativefresh三帧backbuffer正确、Present0、GPU完成，但window/screen固定00CDB38B，front FF8BB3CD、duplication无本次fresh帧；旧前置随后独立资源仍expected00563412/actual00CDB38B。不能把owner提交或显式format单独认定为根因；不修改共享捕获/旧业务。已按真实回收绝对路径直接读到child.trx，目录枚举曾未列出不能判无TRX。继续亲读TRX尾与最后源码默认构建；不追加task。


## 修改前逐项适用性映射
Historical official geometry 精确方法 WhenHistoricalCaptureOfficialGeometryRunsThenExpectedUpdatesRemain，经 RunHistoricalCaptureControl(true,true,false,FF00FF00,0000FF00) 调用 RunVisualCacheConsumer(cache=true,geometryConsumer=true,official=true)。这是本轮首个旧 DQ E_HANDLE/Destroy 堆损坏验证门。

普通 surface sampling/geometry/historical（wicSource=false，且无配置恢复/回调/事务/多设备分支）有真实 InteropDeviceBitmap Present，需要 async connect(0)、partition 初始化、setup闭批/flush、初始化invalidate、每图独批 DuplicateHandle Present/commit/wait/flush。原12×1 surface、32×16 target、shader/geometry预期、色变和caller release断言保持。

WIC callback/configuration/transaction/multipleDevices 依赖 SameThreadPresent HRESULT/重入/锁失败合同，不能机械替换异步返回值。本轮不改变这些同步行为，亦不宣称其全部已验收。target flags12、窗口DPI/context、初始化invalidate仅在迁移后的真实async分支生效；不更改HB显式实现。duplicate成功交接后测试不close，失败不猜所有权补关。原cleanup不跳过。

## 终态：显示前置失败，未验收共享行为已精确恢复
前述映射为修改前设计，不是验收结论。曾仅修改 VisualCache 的普通 surface consumer 分支，添加partition初始化/async连接、真实窗口flags/context/DPI、初始化invalidate、独批duplicate Present与等待flush；同步恢复/回调分支不变。正式发布并执行首个旧official场景后，显示前置失败，按授权停止并逐项逆向撤回全部HG代码（12项及新增私有方法块），未整文件覆盖。VisualCache现恢复535行、原connect(1)/Event0 Present/原断言及诊断；Historical与HB、生产均未改。默认共享修复不保留，候选不重发。

### 唯一实际任务与失败边界
Task **43cf881c5a8947eea0b886a7b331617b**，official x64；正常分析器win-x64 Release self-contained .NET/WPF10.0.13发布成功，未NoWarn/禁规则。独有artifacts/hg-shared-consumer/{obj,bin,publish,com-acceptance-package}，显式PublishDir；仅clean package上传，readback/x64-official整任务回收与input分离。生产复用HD包内HA候选，aot.sha256核为8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0。

同exe --supervise-exit，120秒filtered，filter FullyQualifiedName~WhenHistoricalCaptureOfficialGeometryRunsThenExpectedUpdatesRemain，外180秒。监督PID6476、子9200，UTC10:00:54.5347686–10:00:56.4068341，Reason exited、子Exit2/0x00000002；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，HasExited/OutputCollected=true，SupervisorError=null；stderr空。工具未给外层数值Exit，非MTP监督无父TRX。

stdout实际汇总total1/failed1/succeeded0/skipped0，方法确实执行；失败在原生PresentNativeControl/DiagnoseNativePresent后、connect前的AssertCombinedPixel：expected00563412，actual00CDB38B，window/screen一致。stages24行从historical load至cleanup complete，无connect before，无interop/channel/async target/Present事件；设备/窗口cleanup到达但没有最终refs数值。没有原生退出，不能用cleanup最后阶段猜原生首因。consumer内容/事件/refs本轮均未覆盖。

stdout声明绝对childTRX C:\CodingAgentSandbox\Tasks\43cf881c5a8947eea0b886a7b331617b\Results\hg-x64-official\TestResults\child.trx，但整任务回收实际没有TestResults目录或TRX（递归名称搜索仅匹配TrxReport DLL）；因此不填TRX Counters/Failed/RunInfo，也不把缺TRX误报为原生崩溃。诊断文件回收未全文审。stdout汇总不升级为已读TRX。

### actual身份与证据
hw-module JSON10行亲读：actual官方任务根wpfgfx_cor3.dll，SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，10.0.13-servicing.26504.104 commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699，X64/.NET10.0.13。测试SHA744BB930F11749F7E8E3F80FC91C206150ED410B0ACA85BBFFB94B15D9AD533C，MVID414349cd-6e2c-4774-9181-acf1d8209c46。WPF实际模块路径/SHA/MVID未单独落盘，runtimeconfig .NET/WPF10.0.13仅包声明，不冒actual证明。候选仅包SHA，未actual消费。

回收根d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\hg-shared-consumer\readback\x64-official。Results/hg-x64-official下supervision39/stages24/stdout48/stderr空已读（stdout中长29行返回截断，不冒全字符审）；Results/hw-module-43cf881c5a8947eea0b886a7b331617b-8843454b01cc4b55a182953dacfa1dab.json全文。未回读所有包字节/清单，不能宣称完整包审计。

### 恢复与最终验证
撤回后default正常Release/win-x64 ComAcceptance build成功：98 warnings/0 errors，2.99秒，日志58–64亲读；未关闭normal analyzers。构建不是运行通过。HG实际任务official x64=1，candidate x64=0，x86 official/candidate=0，相关consumer/historical回归=0，full=0。不存在双架构新通过矩阵；HDHEHF只作历史，未重新绑定HG。

下一唯一门：先解决并独立确认旧共享场景原生显示前置真实内容可见，再重新迁移上述合同并复测同一旧official HistoricalGeometry；本轮不再重跑或改生产。旧official E_HANDLE/Destroy崩溃本轮未真正到达，仍开放，不能称修复。迁移不能只测显式HB，默认原场景与相关回归通过后才允许保留。

HG末次补记：报告全文及三共享最新HG节写后回读；历史保留，仅相关HD/HE/HF/DQ节已读，长返回截断处未补全不冒全审。

保留 HA 生产 COM 修复与 HB 独有实现。候选固定 x86 75D136F71223DB9EA0830BFBF9CDFC0EAAB748B5067397B682E42323B698EC30；x64 8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0。不重发生产。不删 Present、不降低原内容与 cleanup 断言。同步且不 Present 路径不机械迁移异步合同。

正式门：最小旧 official 曾 E_HANDLE 场景优先；正常分析器、x64 self-contained 10.0.13、正式 MTP/BL 监督 filtered<=120 秒/外180秒，绝对任务 TRX。official 正常 cleanup 后才扩 candidate/x86；首原生崩溃停止扩大。未验收共享行为必须精确恢复。HW backend/Available/loss/media/资源通知/full/独立 x64 Event0 仍开放；完整替换未完成。

## HH终态：两前置真实差异已实跑，单一夹具根因未证
仅新增Tests/WpfGfxShape.ComAcceptance/D3DImageTests.HhNativePrecondition.cs，整文件受HH_NATIVE_PRECONDITION显式符号约束。同一正式exe、同一task先原样CheckFreshDesktopDelivery(1,false)，释放后再独立创建旧VisualCache原生前置；捕获前置断言仅为完成第二项诊断，最终两失败都写盘并使测试失败，没有静默return。未调用connect/consumer/Event0，未修改生产/DRDS/NativeRun/ExitDiagnostics/共享捕获函数/旧VisualCache/HB/项目或打包定义。HA只作为Native包依赖，未actual使用，不称候选通过。

### 静态差异及适用合同
|项目|HB成功历史所用nativefresh|旧共享前置及本次复刻|
|---|---|---|
|窗口/style/owner/pump|CreateOutputWindow，exStyle8、style90000000，无owner HWND；专用STA GetMessage/DispatchMessage，无背景刷、WM_PAINT仅validate|同一方法/同一owner设施，只有title不同；本次实际style -1811939328、exStyle8|
|DPI/坐标/cover|同Create/Establish流程，client16,8→screen56,48；本次实际DPI96|本次window/thread raw context24593，DPI96，client0,0,32,16、rect40,40,72,56；cover=foreground=本窗口，visible1/iconic0/cloaked0；owner7932与采样8204同Default桌面。HB本次raw context/style未另落盘，只能据相同源码，不冒实际双测|
|D3D9创建|Direct3DCreate9(32)，adapter0/HAL1/behavior26，32×16/count1/windowed1/SwapEffect1，requested Format21，actual21|除requested Format0外相同，actual22（X8R8G8B8）；不能因格式差异直接判错合同|
|提交|Clear红/蓝/红，在owner7932线程Invoke；提交前读backbuffer；DXGI duplication先建立再回调提交|单Clear FF123456/Test线程8204，PresentNativeControl；不预建duplication，不读discard后backbuffer|
|Present目标/flags|device/window与override同一HWND，src/dst/dirty=NULL、flags等效0|相同NULL矩形与override，同flags；两个前置使用独立HWND与device|
|完成/采样|每帧提交QPC、CompleteNativePresent、Diagnose、window/screen/front、再fresh acquire|CompleteNativePresent后Diagnose再AssertCombinedPixel；HH在断言前附加只读front尝试与窗口trace，不改变提交；不是字节级无扰时序复现|
|flush/query/lifetime|共同CompleteNativePresent：EVENT query8/Issue1/GetData FLUSH1有界5s，泵消息、两次DwmFlush；逐帧后统一释放device/factory/window|同完成函数；旧device/factory最终refs0、owner DestroyWindow1；HB finally路径已执行（released记录），既有Release不输出最终refs，不能造其数值|
|expected/API通道|Clear D3DCOLOR AARRGGBB，GetPixel COLORREF00BBGGRR；红FF FF0000→000000FF、蓝FF0000FF→00FF0000|FF123456应为00563412，原值正确；actual00CDB38B对应R8B/GB3/BCD，native front FF8BB3CD为同一RGB，不是仅红蓝互换错误。不改expected追绿|

### 唯一实际任务、身份、内容及边界
Task **4b901b8e7b094574bf51400213e9cb41**，监督2880/子9500，UTC10:08:51.2696619–10:09:15.1273892，filtered120/外180秒；正确三参root、120、FullyQualifiedName~WhenHhNativePrerequisitesRunThenBothOriginalContentContractsAreRequired。子Exit2/00000002、Reason exited，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，OutputCollected/HasExited=true，SupervisorError=null；stderr空。外工具未单列数值Exit，非MTP监督无父TRX。正式child TRX Counters total1/executed1/passed0/failed1/notExecuted0/timeout0/aborted0；ResultSummary Failed，RunInfo Error明确exit2，方法确实执行23.1895615秒。两前置是一个测试内部两段，不造2个test计数。

实际官方task根wpfgfx_cor3.dll，Load句柄140720787554304与Process.Modules匹配，SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，10.0.13-servicing.26504.104 commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。test SHA9B1101DA2E0E9A53D3AD6B49E66C046C2FC7F9B0E6CF0510F8DFB60A83362C76，MVID457ab1fb-c99d-4459-a552-3448d4e8279c，.NET10.0.13/X64/ptr8。actual task根PresentationCore.dll SHA0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31，同servicing版本。候选仅包SHA8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0。

HB三帧backbuffer FFFF0000/FF0000FF/FFFF0000全等，Present各0，owner=submit7932；submit/completed QPC分别35712087834468/35712092502676、35712163871398/35712169344530、35712237816884/35712243555692。window/screen三次固定00CDB38B，front HR0/FF8BB3CD；DXGI后续Acquire 887A0027，不能取得属于本次submission的fresh帧，duplicated字段0是未取得有效样本而非黑色内容。第一initial只有mouse更新lastPresent0；三帧accepted均false。adapter Microsoft Basic Render Driver/vendor1414/device8C/LUID4E71/flags0，output DISPLAY1 bounds0,0,1024,768/monitor10001，adapter0/output0；只证明duplication设施，不证明consumer HW。旧HWND A50516、HB A40516；旧submit前35712303055266、完成记录35712307482885，client/screen同00CDB38B，严格原expected00563412失败；旧front tuple序列化为{}，虽调用取得但数值不可得，不补造。旧backbuffer与duplicationfresh刻意不加以保持旧提交合同；资源0/0、销毁1、consumerEntered=false。

本次原样HB控制也未复现历史成功，因此不能凭owner线程/format/采样顺序区别解释HG；已证失败层是正确producer backbuffer/成功Present与GPU完成之后，独立window/screen/front均不随三次提交变化且无fresh桌面帧。没有确证单一夹具合同错误，不以泛称环境不稳定代替定位，亦无授权理由修改共享捕获函数。旧consumer/E_HANDLE/Destroy仍未覆盖；失败只表明原生前置未证。

### 构建、证据亲读与最终安全
正常分析器x64 Release selfcontained10.0.13，独有artifacts/hh-native-precondition/{obj,bin,publish,com-acceptance-package}，显式PublishDir；初次发布因新增finally内多余throw触发CA2219，改用已有owner DestroyWindow封装根治后一次有效发布成功。共2次发布调用（1编译失败、1成功），未关闭分析器/NoWarn，没有变参运行。仅clean com-acceptance-package上传，readback分离；不重发生产、不做COM/full变体、不重跑。最后源码后default正常Release/win-x64 build98 warnings/0 errors，1.71秒，日志58–64亲读；诊断默认不编译/不发现，未留下未验收默认业务行为。

回收绝对根 d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\hh-native-precondition\readback\x64-official。task绝对TRX C:\CodingAgentSandbox\Tasks\4b901b8e7b094574bf51400213e9cb41\Results\hh-x64-official\TestResults\child.trx；stdout声称后直接按回收映射路径读到，不因目录枚举遗漏判noTRX。亲读监督39、业务hh-native10、native60、window2、stdout112、stderr空及TRX方法/错误/Counters/RunInfo；TRX stdout长段有返回截断，以另读stdout原件补业务，不冒全文逐字符审。diagnostic仅回收未审；未造stages文件。当前HH实际task1/official1/candidate0/x860/full0，旧HistoricalGeometry实际0；HB历史四格不重新绑定HH。

下一唯一允许重启共享迁移的门：先在同一正式任务、原expected和真实采样下同时证实HB nativefresh与旧共享原生显示前置内容可见、资源释放；若要修公共捕获必须先提交明确API/坐标/DPI/目标HWND根因并协调。当前不满足此门，禁止直接重启迁移。满足后才按HG适用合同复迁并优先复测同一official HistoricalGeometry，随后正常cleanup再扩候选/x86。Available/HWbackend/loss/media/resources/最终full四格/独立x64Event0继续开放，完整替换未完成。

HH末次核证纠正：对HG历史task43cf881c5a8947eea0b886a7b331617b亦按stdout绝对路径映射直接读取，实际Results/hg-x64-official/TestResults/child.trx存在！原HG及共享历史“无TRX/未知Counters”系枚举漏失，现明确撤销，不删历史。亲读TRX1–9、35–69：HistoricalGeometry outcome Failed，Counters total1/executed1/passed0/failed1/notExecuted0/timeout0/aborted0，RunInfo Error exit2。此为旧证据回读纠正，不是新增运行；HG connect前失败和未覆盖consumer裁决不变。HH报告全文1–72及三共享最新HH节已分段写后读，TRX stdout重复长段不冒逐字符全审；长历史仅有关节。

