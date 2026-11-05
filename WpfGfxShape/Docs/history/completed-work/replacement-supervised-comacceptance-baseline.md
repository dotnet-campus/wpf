# BD ComAcceptance 当前源码双架构 Release 基线

## 结论

**失败/中断基线：x64 无过滤full完整结束，1740执行、1699通过、41失败，退出2；x86 无过滤full中断，无父完成TRX，最终执行/通过/失败/未执行计数及退出码未知。不能宣称双架构full完成或完整替换。**

全程仅专用文件/发布与Windows沙箱正式ComAcceptance纯C# exe，无终端、shell、PowerShell、cmd、直接间接脚本，无新增Host或第三工程。源码及配置零修改，唯一文档变更本报告。所有新产物位于独有 `artifacts/bd-comacceptance-baseline/`。未改生产、曲线、共享交接、tracked ReferenceData/golden/threshold/skip，未清理他人产物。与并行曲线调查无写冲突。

## 配置与源码对应

已读交接入口/监督汇总当前优先段、执行规则及完整方案全文，slnx、项目配置、Directory.Build.props/targets、Package.targets清理逻辑、NativeRun/AcceptancePaths与身份/参考/硬件/审计方法。首次相对路径读取失败，改绝对路径；解决方案枚举目录失败，随后读实际slnx并获取ComAcceptance项目文件列表。

正式ComAcceptance net10.0-windows/UseWPF/Nullable/MSTest4.0.1/MTP NativeRun入口；项目global.json SDK10.0.107/latestFeature。生产net10.0/PublishAot/NativeLib=Shared。Package targets从WpfGfxShapeArtifactsRoot派生包目录并RemoveDir，故分架构独有root，未改共享targets。

AZ报告明确只绑定AP归档SHA，ProductVersion提交字符串不是dirty源码凭证；没有足够证据证明AP归档等于当前源码。因此没有盲目复用AP，也不是为扰hash重发：按正常依赖图发布当前源码各一次建立新快照。生产文件没有修改。

工作根 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\`；下文产物相对该根。

四次专用发布工具均成功：
- 生产Code/WpfGfxShape/WpfGfxShape.csproj，Release，分别win-x86/win-x64；WpfGfxShapeArtifactsRoot=`<工作根>artifacts/bd-comacceptance-baseline/{x86,x64}/`，输出各root/native，未绕过依赖或NoBuild。x86日志亲读退出0、Generating native code与独有native输出，既存CA1416警告且大日志被工具截断，不声称零警告。
- 正式ComAcceptance项目Release，分别win-x86/win-x64、自包含true、RuntimeFrameworkVersion=10.0.13、ComAcceptanceSandbox=true；显式AotRuntimeIdentifier对应架构，AotDllPath=`<架构root>/native/wpfgfx_cor3.dll`；独有root、输出publish，原目标生成com-acceptance-package。

这是本次专用发布从当时工作目录源码产生的候选；未取得完整源码hash/git dirty清单，不证明两个发布时间之间绝对没有外部修改，不绑定后续并行曲线修改后的源码。候选版本1.0.0.0/ProductVersion `1.0.0+b4de881638949d7fdbb653f9650fb21fd07bc685`，不拿版本字符串冒充源码凭证。NativeAOT无托管CorHeader，候选MVID不适用。

## 包与任务

先各一次身份/官方输入smoke：PublishedArtifactTests + GlyphReferenceTests + WhenTileFixturesAreRenderedThenOriginalWpfEvidenceIsRecorded，各3/3、Completed、其他计数0。已亲读两份TRX54-57和identity全文。真实官方生成参考只写ignored artifacts任务ReferenceData；完整回收目录作为full输入，不修改tracked参考。

| 角色 | x86任务ID | x64任务ID |
|---|---|---|
| smoke及官方输入 |0ff4064516b34076b216f4d033a6143c|10b025e4689740d2b1c4fd6657ab15a8|
| 无filter full，各一次 |8a27a4d36f404ad0ac0dc7fca974a14d|f66747198e934f309ac1385f0403c9ad|
| 有界中断诊断 |4379d65efef041088a749b87b7871591|不适用|
| 补充身份1项 |8c38319ea089434fae05efc3f0ee3f59|不适用|

远程绝对full根：
- x86 `C:\CodingAgentSandbox\Tasks\8a27a4d36f404ad0ac0dc7fca974a14d\`。
- x64 `C:\CodingAgentSandbox\Tasks\f66747198e934f309ac1385f0403c9ad\`。

完整本地回收绝对根：
- `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\bd-comacceptance-baseline\full-x86\`。
- `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\bd-comacceptance-baseline\full-x64\`。

同独有根保存构建x86/x64/native、publish、com-acceptance-package，以及smoke-x86/x64、diagnose-x86、identity-x86完整目录。沙箱调用未指定outputRelativePath，回收整个任务而非仅TRX。smoke上限300秒、full900秒、诊断120秒、身份60秒。只用现有正式exe与既有隔离角色，没有无限重跑或定向拼full。

## 实际身份

| 身份 | x86 | x64 |
|---|---|---|
| 候选SHA256 |E0D3F5EFEDFACFD5CC3491DD43B194E19028E8B541A462123C8F73C73D6E25B5|1C381539E7C82DDD75908C0563F0AC062E583166BDB23AEE066BDD21E838752F|
| 测试SHA256 |84190DE3614F5983BAB69CE9CE0F9FB031EC5AB1964D55482461D442E31FC31F|D64A04DE7F85C63176B16C18AAFC4B470FB2D4E8BD573B58BB939A42237B6FE4|
| 测试MVID |91091a82-fd75-4e42-b7f9-bc2939583436|ce5bf492-6283-4571-8ec8-563c6f1564c6|
| 官方SHA256 |5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|
| PresentationCore SHA256 |D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D|0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31|

实际.NET10.0.13、X86/4与X64/8。测试版本1.0.0.0/ProductVersion同候选字符串。官方FileVersion `10,0,1326,50504`，ProductVersion `10.0.13-servicing.26504.104`，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；PresentationCore10.0.1326.50504。自包含WPF及官方模块位于实际任务根、候选位于Native；均同版同架构。

x86 full内Results/hw-module-*.json亲读Requested=Actual官方/候选路径、SHA与测试SHA。x86测试MVID/WPF另取同包补充identity-x86/Results/az-reference-consumption.json（1项通过），不是full执行到身份测试的证据。x64取full-x64/Results/az-reference-consumption.json，已读身份与Glyph来源段，参考SHA校验通过。参考来源是真实WPF同目录官方模块，未使用候选生成预期。

重要限制：现有初始化会让同exe子角色覆盖顶层Results/aot-identity.json。两full文件已全文亲读，x86现存Arguments是source-clip参考filter，x64是curve候选filter；它们不是父full参数证据。父调用实际无filter；靠调用记录、实际模块JSON、测试SHA及x64父TRX绑定，不能把被覆盖JSON误称父身份。未擅改共享初始化。

## 原始full状态

x64父TRX远程绝对路径 `C:\CodingAgentSandbox\Tasks\f66747198e934f309ac1385f0403c9ad\TestResults\bd-full-x64.trx`，本地full-x64/TestResults/bd-full-x64.trx。13613-13621亲读：Failed，total/executed1740、passed1699、failed41，error/timeout/aborted/inconclusive/notRunnable/notExecuted等均0；RunInfo明确退出2。TRX未执行0不代表所有关键HW路径均覆盖。

x86 TestResults仅部署目录和继承的bd-smoke-x86.trx，没有bd-full-x86.trx；不是full3/3。未发现dmp/log。工具调用很快返回，没有独立退出码/timeout字段；不能制造访问冲突码、确定timeout或最终计数。console末见官方sampler仅用于选择诊断，不用于正式统计。一次同包官方sampler+身份有界诊断同样中断；diag/log_261020010009863.diag38-51亲读：发现1740、开始WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain后无完成。证明官方角色可独立复现中断，不证明原full唯一根因。继而仅身份1项成功，未把定向结果拼作full。

## 41项失败分组与最小下一步

x64父TRX失败段1460-2355、3511-3548、3717-3728；关键错误/官方对照段已亲读。

1. **37项硬件/呈现/恢复**：Stage4 geometry cache2、configuration recovery2、Wic cache2、LazyWic重入5、write-locked retry2、two-device2，共15；historical candidate5+official geometry/sampler2，共7；PublicHwnd D3DImage8+shader generation/malformed/channel close各1，共11；native desktop1+candidate/official resize各1+exclusive loss1，共4。多数初始原生呈现前置像素0x00CBAE87对0x00563412；Ex分支PresentNativeControl返回141953144非0；loss探针第一设备仍0，第二设备失败不是第一设备loss。后续mutation/resize/retry/recovery未到达或无内容证明，记未覆盖。**不能全部归环境**：同full WhenOriginalPublicHwndTargetDrawsThenWindowContainsHardwarePixels通过且像素正确，而候选多项失败，产品/夹具嫌疑保留。下一步同包隔离现有native control、官方/候选PublicHwnd与stage4，核同device/adapter/读回时序与呈现链；不引入软件fallback，不从Basic Render Driver外推真实物理GPU。
2. **内容差异2项**：rounded/curve。rounded comparison threshold0、hardwareVerified=false；6帧中1/3/5各44字节差，首差(x14,y4,ch0)官方36/候选40。curve aq-summary保留frame5=44、frame9=10，三种预量化干预0差。下一步交并行曲线负责人处理float/设备量化及真实WPF变换链；本包不改生产/实验/测试。
3. **合法大shader输入内容差异1项**：WhenShaderResourceCrossesOneMiBThenOriginalAndCandidateAgree，shaderBounds长度2939相同，索引320像素官方4294967295、候选4294901760，不是缺参考。下一步用现有public-abi角色报告缩小边界输入，核有效字节码及实际消费；涉及生产修复先报告，不越界改。
4. **明确审计夹具缺陷1项**：WindowQEvidenceTests所有resize帧强制24*16*4，实际frame-0-2-40x24.pbgra32被固定长度拒绝。源码先exact逐字节比较后固定长度，TRX3717-3728明确失败后者，不应归产品差异。下一步按每帧真实尺寸验证完整buffer，保留红蓝及逐字节独立断言；该文件非本包基线/打包必要修改范围，本包仅报告、不擅改、不skip。

x86中断分类：官方角色/ABI夹具或原生进程异常待定位，不能声称候选崩溃。下一步在既有正式exe增加有界阶段/退出证据，核官方ABI调用布局，必要时把现有高风险官方测试经已有同exe隔离；先由主线协调范围，再修明确最小夹具、定向验证后完整回归。当前无dump/退出码，不补造根因。

## 内容、时序、资源与恢复亲读记录

除console外已读两smoke父Counters、双full aot identity全文、x64父Counters/退出及关键错误段、x86实际官方/候选module JSON、双架构测试身份、x64rounded comparison/curve summary、资源comparison/cycles/events/resources片段、恢复session全文。完整业务JSON/原始pbgra/ppm/参考/时序/资源目录已全部回收；未运行新比较器重算全部二进制，不夸大人工核查。

x64 Results/replacement-resource-z/2aa77554df3e4cfbb62f28d7d1daf0f9/：comparison Failure=null、threshold0、16cycles、48frames/role、缺失/重复release负控被拒绝。cycles头部Target/Bitmap/Factory/Proxy release=0、Calls2/Disposals1并有原始帧；events头部创建→更新→frame callbacks→producer释放。候选cycle0→15 handles1398/threads23，private137752576→141475840，末13-15稳定141475840；官方末13-15 private136568832→137687040。只证明短循环观察，不声称长期CPU/GPU/线程/callback收敛，未逐字审阅全部事件/像素。

Results/replacement-recovery-z/f22f5cac9123463cab53d34dddacf04b/session.json：interactive/session1、InputDesktopOpened/Closed=true、RecoveryVerified=false，明确无post-reset像素oracle。full真实loss probe失败；实际loss→Reset→新色HW消费继续未覆盖。

x64父TRX2510后duplication原始序列为Microsoft Basic Render Driver，提交QPC→frame lastPresent→red/blue新色读回一致，不冒充真实物理GPU闭包。native staging正确而部分GetPixel错误，支持继续区分读屏/设备呈现，不关闭候选问题。x86中断后的资源/恢复/媒体后半覆盖未知，不从x64或历史补齐。

## 交付与边界

源码/配置变更：无；仅本报告及BD独有ignored产物。没有最终四格、没有双架构Green、没有完整替换结论。

本报告仅绑定本次发布/实跑快照。曲线后续修改后必须再做最终 **Tests/ComAcceptance × x86/x64 Release 四格完整适用full**，不得拼旧快照或定向Green。当前交付为失败及中断基线、证据限制与最小下一步，不是验收通过。
