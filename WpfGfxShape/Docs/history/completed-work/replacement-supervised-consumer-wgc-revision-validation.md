# WGC 修订稿静态门与有界验证

## 最新接续：KP/20B3 唯一真实 official 诊断 Failed，已停止

见[真实原件回读与分层裁决](replacement-supervised-consumer-wgc-kp20b3-official-readback.md)。任务55fb7bcd5e3142e6aaac0d19db6bb9be，Exit2，无超时；TRX执行1失败1，阶段与体内栈证明实际体进入。候选实际SHA完整20B3匹配；test SHA/MVID/UID及官方实际根路径/hash/version已记录，独立可信资产hash/PE/PresentationCore配套缺口仍披露。首独立红标尺WGC合格帧零色/零alpha、后续两DC红；两个HWND destroy检查失败，consumer真正创建提交/两色not observed，不冒产品结论。WGC累计build3（1败2成）、COM publish3（1败2成）、沙箱2、单独发现1、真实体1失败；本次未追加发现/编译/发布/修码/重试，KS不变。下一仅请求失败原件限定的标尺内容关联及销毁检查静态审查授权；下文旧待运行与失败历史全部保留。

## 最新接续：显式 KP/20B3 绑定，新包发布准备完成

见[独立发布准备报告](replacement-supervised-consumer-wgc-kp20b3-publish-readiness.md)。本次唯一COM publish成功Exit0，显式AotDllPath指向已查KP clean Native，全新consumer-wgc-kp20b3-bound输出，正式GetFileHash摘要完整20B3；C427旧包保留。源码/项目0改，独立build/Native/沙箱/发现/测试全0。WGC累计build3（1败2成）、COM publish3（1败2成）、沙箱发现1、真实体0；KS另列不变。

本次runtimeconfig/deps为NETCore/WindowsDesktop10.0.13、win-x86、SDK18362.57；新DLL公开API实际包含目标方法。无独立二进制SHA/PE、test SHA/MVID、新包UID或官方实际加载认证；后续同次授权任务用既有初始化/监督实证，不新增工具或代码assert。唯一下一步请求一次真实official诊断，当前不执行。以下Blocked是恢复前历史。

## 最新追加：Native身份漂移审计，真实运行前输入门Blocked

见[完整身份审计](replacement-supervised-consumer-wgc-native-identity-audit.md)。本阶段build/publish/Native发布/沙箱/发现/测试全0，仅文档；源码/项目/包0改。完整发布命令没有AotDllPath，现有默认解析为生产Release/win-x86位置，而此前KS明确复用KP的20B3F944…；这是丢失显式输入绑定，不是已证默认公式改变。C427438A…有旧EX同DLL实验x86历史身份及两次历史Native发布来源，不能继续只称无区别的“既有生产候选”，也不能把历史发布算成本轮隐藏发布。可读COM工程/targets不构建生产，当前无隐藏Native发布肯定证据；日志前段截断及缺少字节重算仍披露。

当前clean与readback根摘要文本同为C427，但未独立核源/publish/clean DLL SHA/PE，发现未执行AssemblyInitialize，故不能排除文本与副本字节不匹配，也不据摘要差异判产品缺陷。唯一准备目标为另授权正式COM publish显式AotDllPath复用KP clean Native的20B3完整身份，并用新独立输出关闭残留风险；不需要Native发布。本阶段禁止，故停止不修包。

发现原输出已亲读全部7行：目标显示名单独一行加总数1，支持实际唯一列出目标，不是只概括分母；没有UID/FQN节点行/数值exit，回收未取得节点诊断，不追加发现。official-only候选只需现有初始化文件验证，不需实际加载；不能因此跳过来源门。现有WGC官方handle可记录Actual路径/hash/ProductVersion/runtime，但CandidateLoadedByDiagnostic=false是常量，不证明全进程未加载。完整证据及最小恢复条件见新报告§5–8；以下成功发布/发现事实保留，不再视为真实运行前身份放行。

## 最新追加：runtime/projection 根因修正、发布成功、唯一发现完成

本节覆盖下文发布阻塞，历史失败与预算不删除。仅实际工具，无委派、终端、脚本、新 Host、第三方包。读取当前执行规则、章程合同、交接、最新报告、csproj/targets、局部 props/targets/global.json 与实际 SDK 文件。

### 根因及正式依据

本地实际 SDK 路径 `.dotnet/sdk/10.0.402`；本次 restore 的 project.assets.json:3310 runtimeIdentifierGraphPath 同样指向该 SDK，不将 global.json 的10.0.107/latestFeature冒实际版本。

`Microsoft.NET.Sdk.FrameworkReferenceResolution.targets:30–52` 用 WindowsSdkPackageVersion、TargetPlatformVersion 和支持平台表创建 Windows KnownFrameworkReference；:152 将全局 RuntimeFrameworkVersion 传给 ProcessFrameworkReferences。`Microsoft.NET.Windows.props:15` 实际隐式引用是 `Microsoft.Windows.SDK.NET.Ref.Windows`，不是只有旧无后缀名称。BundledVersions.props:826 将18362.0映射为10.0.18362.57；:111–136的WindowsDesktop/App.WPF运行框架为10.0.13。WindowsDesktop.props:48–49说明UseWPF单独启用的是App.WPF profile，runtime pack关联App。

官方SDK维护说明 https://github.com/dotnet/sdk/issues/2792 与 https://github.com/dotnet/sdk/pull/2887 明确优先级：FrameworkReference的RuntimeFrameworkVersion元数据 > 全局RuntimeFrameworkVersion属性 > 默认/最新补丁选择；TargetingPackVersion独立。全局10.0.13并非仅.NETCore运行时约束，会被没有专属元数据的SDK投影运行包采用；投影targeting pack仍为18362.57，两条路径请求同包不同版本，导致前次NU1505/NU1102。不能只设WindowsSdkPackageVersion就声称隔离全局运行包选择。

两次raw GitHub Fetch均HTTP404；不算完整任务源码认证。官方维护说明、可读取本地targets及本次实际deps/restore共同支持结论，不将main源码检索片段冒本地任务逐行实现。

### 最小修改及重新build

只在现有 `ComAcceptance.Package.targets` 增加 `PinConsumerWgcRuntimeFrameworkVersions`，诊断配置且在ProcessFrameworkReferences之前，对Microsoft.NETCore.App、Microsoft.WindowsDesktop.App及App.WPF执行Update元数据RuntimeFrameworkVersion=10.0.13。目标执行时隐式框架引用已经创建；不新增引用/包。发布不再传全局RuntimeFrameworkVersion，投影独立沿原18362.57映射；这是将固定要求从全局移到对应框架，不是删除10.0.13固定或靠最新补丁碰巧命中。没有升级包/TFM、禁warning、改生产/Tests/global.json/公共捕获/NativeRun/ExitDiagnostics；两partial和资源未改。

项目targets实际变动后使用新增正常build **1/1**：ConsumerWgcDiagnostic / win-x86 / net10.0-windows10.0.18362.0，Exit **0**，**133 warnings / 0 errors / 1.55秒**。日志64行，60–63汇总；输出105024字符，仅保留末20000，前85024字符截断；不声称完整warnings明细。可见MSTEST0044/0045/0037/0017、CA2007/2014，未抑制。此后没有源码/项目尾改。

### 唯一新增publish及同次clean包

新增COM publish **1/1成功**，Exit0。命令与前次相同配置、RID、TFM、self-contained true、ComAcceptanceSandbox=true、AotRuntimeIdentifier=win-x86，**不传全局RuntimeFrameworkVersion**，PublishDir和ComAcceptancePackageDirectory改为独立 `artifacts/consumer-wgc-runtime-fix/{publish,com-acceptance-package}/`。实际完整调用见本次发布工具日志第1行。59行日志，57–58记录诊断程序集与独立publish输出；52809字符仅保留末20000，前32809字符截断；无warnings/errors汇总，不虚构发布警告计数。原有PackageSelfContainedComAcceptance清理重建clean目录，无再次publish、无Native发布。

同次clean根runtimeconfig includedFrameworks明确Microsoft.NETCore.App **10.0.13**、Microsoft.WindowsDesktop.App **10.0.13**；deps目标 `.NETCoreApp,Version=v10.0/win-x86`，根dependencies同时列.NETCore和WindowsDesktop runtimepack **10.0.13**与SDK runtimepack **10.0.18362.57**。project.assets.json:3275–3296下载精确范围分别为上述版本，不再请求SDK.Ref10.0.13。SDK.NET程序集assembly10.0.18362.38/file10.0.18362.55，WinRT.Runtime assembly2.2.0.0/file2.2.0.48161；包版本18362.57与程序集版本不可混同。MSTest4.0.1，发现进程实际.NET10.0.13/x86。

根 `aot.sha256`：**C427438AF11B8B4616AC732120B32B94EC848A8AD6B05406381C2C8A781DB267**，正式target对同次publish的Native候选GetFileHash后写入；回收包根同值。候选为 `Native/wpfgfx_cor3.dll`，输入仍既有生产win-x86发布产物；根 `wpfgfx_cor3.dll` 属官方WindowsDesktop10.0.13运行包，deps fileVersion10.0.1326.50504。两个相对路径独立，未把candidate覆盖官方根。**未独立重算复制后候选SHA/PE或官方SHA，未执行任何模块加载身份测试；不能把生成aot.sha256当额外二进制身份审计。**

实际package-files共 **623行**：最后为Contracts两文件、Native候选、aot.sha256及清单自身。FullPath去重后MakeRelative并统一斜杠；已人工分段读取检查，无发现重复相对目标、绝对/父目录逃逸路径，根官方与Native候选不是重复。Microsoft.Windows.SDK.NET.dll与WinRT.Runtime.dll在414–415行，.NET/WPF runtime pack、MTP/MSTest及扩展、卫星资源、Contracts均列入；targets发布成功证明每项存在并复制。没有从旧包补件。长段读取存在4000字符截断，个别卫星资源段未逐行完整显示，因此此处为生成规则+人工检查，不冒机器化全清单唯一性/每文件哈希认证。未见静态缺件，允许进入唯一发现，发现成功不等于所有运行场景的动态闭包通过。

### 唯一沙箱发现与原始输出边界

sourceDirectory严格为上述同次 **com-acceptance-package**，不是publish根；exe为根WpfGfxShape.ComAcceptance.exe，参数仅 `--list-tests --filter FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialConsumerTwoMarkersAreObservedByGdiAndWindowCapture`，timeout180秒。沙箱task **ade9276e16874b578f20c7eb11ac2c80**；回收 `artifacts/consumer-wgc-runtime-fix/discovery-readback/`。工具原始返回逐字保存为同级 `discovery-tool-output.txt`（含原乱码），没有修饰为完整未乱码日志。

原始返回：`MSTest v4.0.1 (UTC 2025/10/14) [win-x86 - .NET 10.0.13]`；仅列 `WhenOfficialConsumerTwoMarkersAreObservedByGdiAndWindowCapture`，摘要报告1个测试，251ms，net10.0|x86。**没有输出节点UID或显式进程退出码**；包回收未发现stdout/log/节点JSON，不虚构UID，不以FQN充UID，也不增调用重查。真实唯一发现节点已按等值filter获得，唯一UID仍未取得。未执行测试体、consumer、标尺或监督入口，无功能/资源收敛结论。

### 累计预算及交接

本阶段新增正常build **1/1成功**；WGC累计build **3**（1失败、2成功）。新增COM publish **1/1成功**；WGC累计publish **2**（前1失败、本1成功）。保留的唯一clean包发现沙箱 **1/1已用**；WGC发现1、真正体0。Native publish **0/0**，测试体/consumer/标尺 **0/0**。历史KS build1/COM publish1、沙箱4/运行请求3/发现1/真正体1/A1/B1/native提交2完全保留；跨KS/WGC合计COM publish4、正常build4、沙箱5、发现2，但不混用预算。

当前终态：版本混淆根因修正已获发布及发现实证；功能、投影生命周期、真实consumer帧、加载模块身份尚未验收。**下一真实运行预算仍未授权**。后续如要求唯一UID、独立SHA/PE/官方身份补证，也须有受支持读取能力或另行明确预算，不能复用已耗尽发现调用或偷偷跑体。以下全部保留为历史。


## 验证前恢复快照

用户已授权非默认 ConsumerWgcDiagnostic 配置和 AcceptanceResources 键，旧报告关于无可选配置的阻塞已被本次授权及现有配置消除。已读当前规则/基线、consumer-wgc-draft-audit 全文及最新五文件。默认配置不定义 CONSUMER_WGC_DIAGNOSTIC；诊断配置 x86/win-x86，TFM net10.0-windows10.0.18362.0，局部 SDK 10.0.107/latestFeature。未改生产、Tests 项目、global.json、公共捕获、NativeRun、ExitDiagnostics 或既有业务方法，无新工程/第三方包，无委派/终端/脚本。

### 静态修订与结论

同步 STA 已替代后台队列：资源返回后调用方即拥有，不再有超时迟到结果无人持有或 teardown 与 worker 竞争。当前五文件保留前执行者修订：完整几何、标尺部分初始化回收、日志异常独立收集、首帧落盘及负控、资源字符串、依赖 FullPath 去重及相对 PackagePath。本轮进一步修正 capture Read/Observe/Drain/SelectFirst finally 不覆盖主异常；surface/native/marshaler 逐项释放，Close 与 projection Release 独立；channel delete Close 失败不再 Commit，Commit 失败不再 Flush；失败 send 的未转移 duplicate/额外 image 引用不再以 deinitialize 成功为回收前提，不能通过泄漏回避合同。

依据本地原件 clientchannel.cpp:329–365 SendCommand 仅记录/CloseBatch，无 Submit；520–578 CloseBatch 仅排队，Commit 才 Submit。失败 separate send 可留下 current batch，故不再 close/commit 该失败 channel。成功 send 的 duplicate 不由测试侧二次 close。

公开 API 实际查询 Microsoft.Windows.SDK.NET.Ref/10.0.18362.57 IWinRTObject 确认 GetObjectReferenceForType(RuntimeTypeHandle)、NativeObject；QueryInterfaceCache 非公开，不虚构公开枚举接口。CsWinRT 源实现检索确认该方法返回缓存引用，不新增调用方 AddRef。当前只显式释放诊断 IDisposable QI 缓存及 native reference；SDK 生成成员内部缓存完整收敛仍需实际版本实现认证，不把 API 清单或编译视为生命周期运行通过。两次 raw GitHub Fetch 为 HTTP404，不能记为实现认证；检索得到 commit 6e758e2cec4c7c03a09444ecb113691f86994033 的源片段，不冒当前解析版本。静态审阅完成，进入编译认证门；不是静态等价编译，也不是功能交付。

### 五文件临时回撤边界

仅 ComAcceptance.csproj 的局部版本 TFM/SupportedOSPlatformVersion/诊断 PropertyGroup；ComAcceptance.Package.targets 新依赖收集/去重/路径规则；两个 ConsumerWgc partial（默认关闭）；AcceptanceResources.resx 的 ConsumerWgc* 键。不得整文件覆盖以抹去既有内容；不回撤其他会话修改。此次实际源码增量仅两个 partial，项目/targets/resx 已审阅未再改。资源清理及投影运行证明未经测试，不宣称完成。

## 预算（调用前）

正常 COM build 0/1，COM publish 0/1，Windows clean 包 list-tests 沙箱 0/1。测试体/consumer/标尺更新 0/0，Native publish 0/0。下一次唯一 build：现有 ComAcceptance.csproj / ConsumerWgcDiagnostic / win-x86 / net10.0-windows10.0.18362.0。任何失败即停，不修后再次 build。成功后方可同配置 self-contained 10.0.13 独立目录正式 publish 一次，再核 clean 包依赖清单与身份根，仅一次完整 FQN 等值 list-tests，真实体预算仍 0。

历史 KS 保留：沙箱4、运行请求3、发现1、真正体1、A/B各1、native提交2，历史 build1/COM publish1；不挪用不清零。

## 实际认证结果

**终态：唯一正常 COM build 失败，立即停止；未通过编译认证门，草稿不能交付。**

实际目标现有 ComAcceptance.csproj，configuration ConsumerWgcDiagnostic，runtime win-x86，framework net10.0-windows10.0.18362.0。工具原件：最后构建日志64行，1–5行记录参数及退出码 **1**；60–63行记录 **133 warnings / 5 errors / 00:00:05.41**。输出原总长108365字符，工具仅保留尾20000字符，前88365字符已截断；不虚构完整133警告明细。

错误原件（文件均为 D3DImageTests.ConsumerWgcCapture.cs；日志55–59行）：

|位置|错误|根因|
|---|---|---|
|(102,138)|CS1686 rawItem|native调用取 &rawItem，同一局部又被finally的Release lambda捕获；C#不允许该取地址/闭包组合|
|(199,154)|CS1686 staging|CreateTexture2D取 &staging，清理lambda又捕获staging|
|(190,123)|CS1686 texture|GetInterface取 &texture，清理lambda又捕获texture|
|(129,30)|CA2219|activation finally中显式throw cleanup，现有分析规则按error处理|
|(235,26)|CA2219|Read finally中显式throw cleanup，同规则错误|

可见warnings含 MSTEST0044（DataTestMethod过时）、MSTEST0045（Timeout协作取消）、MSTEST0037（断言建议）、CA2007、MSTEST0017、CA2014；举例日志10 JmResolverIsolationTests.cs(14,6) MSTEST0045，14–17同文件35–38 CA2007，45 CompositionTests.DoubleBufferedFormats.cs(86,13) MSTEST0017，46 D3DImageTests.VisualCache.cs(136,33) CA2014。未关闭分析器或顺手修既有警告。

三个闭包错误在既有资源清理结构中存在；本轮新增异常保留分支仍触发Read finally的CA2219。静态门未发现这些编译缺陷，故不能记静态通过/完整安全所有权认证。后续修复方向是取地址变量与清理持有者分离、退出finally后统一传播错误，而非关闭规则或泄漏引用；**本轮失败后未实施任何源码修复，不重建**。

SDK/CsWinRT完整生成成员缓存释放与实际解析版本仍未完整认证；当前GetObjectReferenceForType(IDisposable)+NativeObject不足以实证覆盖所有生成接口缓存，不得声称资源合同已闭。包FullPath去重与相对路径仅静态审阅，未publish，未实际闭包/冲突/身份认证。

最终预算：build **1/1**（失败已耗尽），COM publish **0/1**（编译前置失败，不能直接使用），Windows list-tests **0/1**（同样被前置阻塞），Native publish **0/0**，测试体/consumer/标尺 **0/0**。未生成本轮clean包、package-files/deps/runtimeconfig/身份实证或真实唯一UID。无第二构建、无发布、无沙箱、无测试体。

历史KS账保持原数，整体完整替换未完成。恢复必须另获修复及新增正常编译预算；不能将剩余发布预算拿来代替编译或从旧包发现。

## 本次追加授权结果（覆盖上一节恢复阻塞，不覆盖历史事实）

用户追加修复五错和正常 build 1 次，允许成功后沿用 COM publish 1、clean 包唯一 list-tests 沙箱 1；真实测试体/consumer/Native publish 仍为 0。无终端、脚本、委派、新 Host 或探针。

### 修复及范围

已读取规则、交接、最新报告及五文件。实际源码只修改 `D3DImageTests.ConsumerWgcCapture.cs`：rawItem/texture/staging 不再被清理 lambda 捕获，清理遍历各自 native 指针值并捕获 foreach 独立持有者，native 输出仍先被局部接收；各自非零原生引用只在此路径 Release 一次。activation/Read 捕获主异常，逐项尽力清理，清理异常关联到原异常 Data；清理完成后在 finally 外通过 ExceptionDispatchInfo 传播原异常（保留类型/堆栈），无主异常则传播清理 AggregateException。Read 先保存结果，清理成功才返回。未 suppress、NoWarn、关分析器或吞掉清理异常。

兄弟 Drain/Observe/SelectFirst 已检查：用 WgcReleasePreserving 保留主异常，清理失败附加到主异常，无主异常则清理失败正常传播；不存在这三项取地址局部被清理闭包直接捕获的同类模式。诊断 partial 的外层仍逐项收集清理错误、finally 外传播。未修改原业务方法。

SDK 可确定边界：已直接查询 Microsoft.Windows.SDK.NET.Ref/10.0.18362.57 的 Direct3D11CaptureFrame/GraphicsCaptureSession/Direct3D11CaptureFramePool 公共 Dispose 和 IDirect3DDevice : IDisposable；采用官方公共 Dispose 的 IClosable 支持合同。移除了手动 GetObjectReferenceForType(IDisposable).Dispose 和 NativeObject.Dispose，不枚举、不破坏投影内部缓存。GraphicsCaptureItem 无 Close，surface 的 Close 由 frame 生命周期持有；诊断自建 marshaler 独立 Dispose，自有 native 指针独立 Release。**SDK 内部缓存实际回收策略未经实现认证；真实资源、线程及引用收敛仍待运行验收，公共 Dispose 合同和 build 不等于实测收敛。**

五文件临时边界仍为：ComAcceptance.csproj 的局部 SDK 目标/官方投影/非默认 ConsumerWgcDiagnostic；ComAcceptance.Package.targets 的必要依赖打包；两个默认关闭 ConsumerWgc partial；AcceptanceResources.resx 的 ConsumerWgc 键。本次其余四文件未改。生产、WpfGfxShape.Tests、global.json、公共捕获、原业务方法、NativeRun、ExitDiagnostics 均零改；无新增包或工程。源码最后修改后只 build 一次，之后未尾改源码。

### 正常 build：成功

目标 `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`；configuration `ConsumerWgcDiagnostic`，runtime `win-x86`，framework `net10.0-windows10.0.18362.0`。专用构建退出 **0**，**133 warnings / 0 errors / 00:00:02.48**。原日志63行：1–5参数和退出码，59–62汇总。输出104898字符，仅保留末20000，前84898字符截断；不声称完整警告明细。可见 MSTEST0044、MSTEST0045、MSTEST0037、CA2007、MSTEST0017、CA2014，未顺手修正既有警告。

WGC 正常 build 累计 **2 次：前次 1 失败（133/5）+ 本次 1 成功（133/0）**；新增 build 1/1 已耗尽，历史 KS build1 不清零、不并为 WGC。global.json 要求10.0.107/latestFeature，本地 `.dotnet/sdk` 实际存在 **10.0.402**；前者不是实际 SDK 精确版本证明。TFM/SupportedOSPlatformVersion 为 **Windows 10.0.18362.0**，对应 CreateForWindow 最低平台；不是 .NET 10.0.13 runtime 版本，也不是 Windows SDK 包版本10.0.18362.57。最低版本声明不证明沙箱 OS/捕获支持。

### 唯一 COM publish：restore 失败，立即停止

实际命令：`dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c ConsumerWgcDiagnostic -r win-x86 -f net10.0-windows10.0.18362.0 --self-contained true -p:RuntimeFrameworkVersion=10.0.13 -p:ComAcceptanceSandbox=true -p:AotRuntimeIdentifier=win-x86 -p:PublishDir=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/consumer-wgc-revision/publish/ -p:ComAcceptancePackageDirectory=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/consumer-wgc-revision/com-acceptance-package/`。

发布原日志24行已全读：退出 **1**；第8行 **NU1505**，重复 PackageDownload `Microsoft.Windows.SDK.NET.Ref [10.0.13]` 与 `[10.0.18362.57]`；第9–22行是同一 **NU1102** 的源列表展开，无法找到 `Microsoft.Windows.SDK.NET.Ref (=10.0.13)`，不是14个独立 error；第23行 restore失败（715毫秒）。日志无编译 warnings/errors 汇总，不能编造发布警告总计。显式 RuntimeFrameworkVersion=10.0.13 的本次调用同时导致投影下载出现10.0.13请求；这是发布参数/框架版本解析冲突，不是已证源码编译失败。未进一步改配置、未重发。

本地 `.dotnet/sdk/10.0.402/Microsoft.NETCoreSdk.BundledVersions.props` 可见 .NET10 的 LatestRuntimeFrameworkVersion=10.0.13。后续如获授权应先只读确认 SDK 的 Windows projection 与 runtime 版本选择，考虑去除全局 RuntimeFrameworkVersion、由正常 self-contained SDK 解析10.0.13；不能把 runtime 服务版本当投影包版本。此建议未经新发布验证，不宣布已修好。

发布预定正式现有 PackageSelfContainedComAcceptance clean 布局：独立 publish 和 com-acceptance-package，candidate 应为 Native/wpfgfx_cor3.dll（AotRuntimeIdentifier=win-x86 默认既有生产发布输入），根 wpfgfx_cor3.dll 应为官方 WPF runtime 角色；Native发布0。**restore 即失败，artifacts/consumer-wgc-revision 目录不存在，无本轮 clean 产物身份、aot.sha256、package-files、deps/runtimeconfig、candidate SHA/PE、官方 SHA或加载身份可认证。**

必要依赖静态规则为 Microsoft.Windows.SDK.NET.dll、WinRT.Runtime.dll、Microsoft.Testing.Platform/MSTest及其闭包、.NET/WPF10.0.13 x86运行资产、Contracts 和 Native candidate；targets 有 FullPath 去重、相对 PackagePath 和缺件报错。由于 publish 未到该目标，依赖实际清单、清单重复、大小写/相对目标路径冲突和哈希均**未验**，不引用旧包冒本轮包。

### 发现与下一门

沙箱 **0/1（被发布失败阻塞）**，没有执行 list-tests，更无真实唯一节点 UID/发现原件；不从源码或历史 KS UID 推定。下一门必须先另获 COM publish 预算且生成/核验 clean 包，再以该 clean 包为 sourceDirectory，根 `WpfGfxShape.ComAcceptance.exe`，仅参数 `--list-tests --filter FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialConsumerTwoMarkersAreObservedByGdiAndWindowCapture`，只发现不执行测试体；不得斜杠 filter、不得 publish 根、不得启动已知缺件包。当前不能调用。

以后如另获真实诊断授权，现有监督入口建议参数为 `--supervise-exit`、`Results/consumer-wgc-clean-x86`、`120`、`FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialConsumerTwoMarkersAreObservedByGdiAndWindowCapture`，沙箱外180秒；必须先绑定本轮唯一发现 UID、clean包身份和精确运行预算，回读监督/身份/TRX/JSONL/帧与清理原件。**当前测试体/consumer真实运行预算0，不执行此建议。**

最终预算：新增 build **1/1成功**（WGC累计2，前败本成）；COM publish **1/1失败**；Windows唯一 list-tests **0/1阻塞**；Native publish **0/0**；测试体/consumer/标尺 **0/0**。本次无沙箱、无发现、无真实诊断。历史 KS 沙箱4/运行请求3/发现1/真正体1/A1/B1/native提交2及其历史build1/COM publish1保持，不挪用。终态为**编译门通过、发布门失败、发现门未进入，功能与生命周期尚未验收，整体替换未完成**。
