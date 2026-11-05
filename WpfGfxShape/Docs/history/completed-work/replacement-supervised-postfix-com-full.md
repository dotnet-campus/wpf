# JV：JR安全矩形与JN resolver修复后默认COM双架构full

## 状态：JY 分类与文档交付完成；JV 双 full 已结束且 Failed，完整替换未完成
JY只承接JX已落盘分类，不新调查/构建/运行。JX分类产物已完成；此前文档收口/写后回读声明不作为已完成事实，由本次JY补齐并回读关闭。下述JV/JW/JX阶段记录与已核身份按原报告范围保留，不冒称JY重新核证原始运行。
已接管JU准入后的正式默认COM双full。生产源码本轮不改；保留JN Rounded角色隔离、JD离屏隔离、JH危险official同exe子隔离和有界清理。不得用过滤运行拼full，不修改断言、DataRow、并行、阈值或tracked golden。

## 阶段历史
1. 输入/规则核查进行中：已读当前执行规则、latest handoff前部及JN报告前部；正在核完整方案、JT/JU与JH设施及包身份。尚无JV构建、发布、沙箱运行。

2. 构建/输入：默认self-contained Release .NET10.0.13双架构包专用发布成功，正常分析器，无探索DefineConstants。首次x64误写aot缺win-x64子目录导致MSB3030/Exit1，已按实际目录纠正；非运行失败。包aot.sha256分别1782797EA3271F9426BFEDFC67F30D42834858C778C3ECADF6A40C1AC692CF9A、1DFB33772ACC5FFCD3A5D9D865867ACA9F0EA33A6DD7A8F59113056098E744DF。未新编生产、未独立证明逐字节源码闭包，仅继承JR/JU已验身份。
3. 新官方输入：同本次默认exe x64 task454e28aaeb1241789460b13a5084570c、x86 task57dd1d187bfa41bfa1cac9dc536ee476，监督Exit0/无timeout/kill/drain；已从各自回收ReferenceData通过既有发布机制重建clean包，不上传readback。即将各一次default full，内1500秒/外1800秒。

## 阶段历史范围更正
上方“即将 full”及旧“结果尚不存在”仅为 JV 当时记录，不能作为当前状态。JW handoff 登记了独立核证进行中；JX 接续完成以下确定性分类和交接，不冒称 JW 未写出的完整终态已存在。JV 发布/官方输入历史按原记录保留，JX 未重做；本轮没有新运行、源码改动或新报告。

## JX 终态：实际双 full 与身份
本地根为 `artifacts/jv-postfix-full/`；以下路径均相对此根。父 TRX 原文在 `inventory/inputs/{x64,x86}.trx`，完整失败原文与 RawResult/Stack/Output 在 `inventory/Results/jj-full-inventory/{x64,x86}-failures.json`，不复制巨 stack。

|架构|task|total/executed|passed|failed|skip|父监督 Exit|ResultSummary|
|---|---|---:|---:|---:|---:|---:|---|
|x64|338d677c5e124d569bba7e296aec503e|1809/1809|1765|44|0|2|Failed|
|x86|711b804e5ab64d349dc4820eb9da75e6|1809/1809|1790|19|0|2|Failed|

两份 `readback/full-{arch}/Results/jv-full-{arch}/supervision.json` 全文亲读：Mode=full、参数无 filter、BL1500、.NET10.0.13、Reason=exited、HasExited=true、OutputCollected=true，timeout/cancel/kill/drain 全 false，SupervisorError=null。不是超时凑表、过滤拼 full 或 Green；本轮未运行沙箱，不虚构外层工具 exit。父 TRX 末尾 Counters/RunInfo 已直接读，x64 14301–14309、x86 14133–14141，与 summary 的 1809 唯一 ID/44、19 失败严格对账。

- x64 testSHA `37F6E1A18DC6703C68703B8901EBF3668B2B9098A29224B1917CE31A5457C129`，MVID `da53ddb5-53d7-4e1f-86d4-936469c8c28d`；actual Native SHA `1782797EA3271F9426BFEDFC67F30D42834858C778C3ECADF6A40C1AC692CF9A`。
- x86 testSHA `E5619D54D9C9CADADCA0FEE9E46B07F1726A67E44DFDB7D1DD650FD8DB2F2C75`，MVID `eb7ef964-937b-4d36-93cc-ecc9006bdb12`；actual Native SHA `1DFB33772ACC5FFCD3A5D9D865867ACA9F0EA33A6DD7A8F59113056098E744DF`。
- actual 不是仅 aot.sha256：直接读 `readback/full-x64/Results/hw-module-Native-42ebcacda300401b81eff07dff192502.json`、`readback/full-x86/Results/hw-module-Native-018dc2a4ef5c4f8ea9772a68cd635fc0.json`，Requested=Actual=`C:\CodingAgentSandbox\Tasks\<对应task>\Native\wpfgfx_cor3.dll`；版本1.0.0.0，ProductVersion `1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a`，同对应 testSHA/runtime。
- 官方 x64 实际模块由 sampler 子 TRX 11–18行直接核：task根 `wpfgfx_cor3.dll`，SHA `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`；x86 `hw-module-711b804e5ab64d349dc4820eb9da75e6-7236c9a6f4f645d78155463b1af1b05d.json` 实际根模块 SHA `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`。均 FileVersion `10,0,1326,50504`、ProductVersion `10.0.13-servicing.26504.104`、Commit `4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。不外推其它版本/所有角色。
- 整理输入 SHA 仅引用既有 summary：x64 `92E507F39594A430CBDB77D7CB2496A295791241FFF5AD2B74E158042F1FBC10`，x86 `E972C2A05D6133E4B76DC6C51125CC0FDD1B75CA3CD85F4C0F5791D3158CA6D8`；JX 未独立重算。输入总字节未知，不新运行补证。源码逐字节闭包继承 JR/JU 有限已核范围，未独立全审。

## 63 条逐条分类、数量与交差集
确定性本地产物已写在 `inventory/Results/jj-full-inventory/`：
- [jx-failure-classification.tsv](../../artifacts/jv-postfix-full/inventory/Results/jj-full-inventory/jx-failure-classification.tsv)：63条，每条 arch、完整 id/name/参数、原错误短摘、直接分类、根因簇/因果未证、证据层级、原件行链接。
- [jx-reconciliation.json](../../artifacts/jv-postfix-full/inventory/Results/jj-full-inventory/jx-reconciliation.json)：实际计数、完整交集19 ID、仅x64的25 ID、仅x86空集及下一前三任务。手工逐ID核对，无脚本/C#整理器。
- 原 [summary.json](../../artifacts/jv-postfix-full/inventory/Results/jj-full-inventory/summary.json)、[x64原文](../../artifacts/jv-postfix-full/inventory/Results/jj-full-inventory/x64-failures.json)、[x86原文](../../artifacts/jv-postfix-full/inventory/Results/jj-full-inventory/x86-failures.json) 保持不变。summary旧“全部待定”是整理器初始标签，以JX伴随分类为本次分类结论，不改原件。

|直接分类|x64|x86|合计|
|---|---:|---:|---:|
|产品内容|3|2|5|
|测试包布局或角色路由|2|13|15|
|显示前置|38|2|40|
|原生子崩溃|0|1|1|
|输入|1|1|2|
|待定|0|0|0|
|合计|44|19|63|

集合对账：19+25=44（x64），19+0=19（x86），并集44，记录数44+19=63。交集仅指ID，不是同错误/同因：11条WIC在x64停原生首帧，而x86到Command0xC错误；sampler在x64 Exit2前置失败，x86 AV；resize两架构失败帧亦不同。

分类是直接失败阶段，不是全部根因已闭或环境免责。L1=原始断言；L2=监督/子TRX；L3=原文结合业务JSON或源码。x64 21条 VisualCache 方法族 `Pixel 0x001E1D1C expected 0x00563412`（15缓存+5历史候选+1候选resize）直接对 `D3DImageTests.VisualCache.cs:80–104`：原生D3D Present/AssertCombinedPixel 在connect之前；业务后帧使用 expected参数（300–304行），因此归显示前置而非候选产品内容。9条141953144逐一读各自stack，均 PresentNativeControl；`SwapChainObservation.cs:23–31` 为原生D3D9/Ex Present结果精确0断言，直接归显示前置，未改HRESULT断言。其余8条显示前置为fresh gate/owner1、3/nativeGPU/官方geometry、sampler/官方resize/device-loss。不能由这些证明所有HW缺口均环境同因。

x64产品内容3条：公共HW窗口两条像素不符（1dc5baf7…、108742fb…），ShaderBounds一条。x86产品内容2条：候选resize `0x0000FF00 vs 0x0000FF80` 与ShaderBounds。仍保留真实内容红，不因前置簇掩盖。WindowQ `152af104…` 双架构 `current=null`：源码 `WindowQEvidenceTests.cs:53–62` 找本地保留Q输入祖先失败，归输入；未开始逐字节比较，不能称产品差分红或绿色。直接分类待定0不等于底层因果全部已知。

## 下一前三根因任务（仅交接，本轮生产不改）
### P1：x86 WIC 命令布局，11条原文精确覆盖
x86原清单前11条（3–150行）：WicCache两条 `1de8aecd…/197f02db…`；LazyWic五条 `11194225…/16d0ec26…/1872055b…/168b8cd4…/1fa69fb3…`；WriteLocked两条 `17eef5c8…/12f170ce…`；TwoDevices两条 `1a7331dd…/1ac32cda…`。完整ID和名称在TSV/交集JSON，11条均 `-2003303421 / Command 0xC, 16 bytes`，不是按方法名猜。
`D3DImageTests.VisualCache.cs:223–227` sourcePacket固定new byte[16]，写uint Type、uint Handle、nint wrapper；`Core/GeneratedBitmapSourceResource.cs:5–10` Pack=1同字段，SDK/ABI uint+uint+nint应x86 12/x64 16；`Abi/UceChannelExports.cs:329–330` strict Marshal.SizeOf不符直接UceMalformedPacket。后续应修测试包架构长度并有限验证，不放宽生产strictsize；本轮未实现、未宣称11项已修。对应x64不是包长失败，不推同因/修复数。

### P2：窗口sequence角色路由，双架构各2条
精确ID `170ff4ac-8c18-8c83-8b35-77294231a78e` image与 `1e7cbafa-f206-8321-a8c4-2c890e8cc69e` rectangle，两架构原文均 RunIsolated(...)=64。`ReplacementStateSequenceWindowTests.cs:22–27,49–54` 仅以WPFGFX_RUN_ROOT存在判断业务角色，传--results-directory值；full监督同样设输出根，但不是original/candidate。94–96行明确只认这两个角色，否则return64，尚未进入STA内容比较。后续使用专属角色/方法验证并保持无空过、不动公共路由；本轮只交接不改，不据此宣布sequence像素/回调通过。

### P3：ShaderBounds 真实产品内容边界，双架构各1条
精确ID `105b96c7-ae45-8225-b7ac-be58d2f59b51`，原文均shaderBounds长度2939、index320白4294967295 vs红4294901760。业务原件：`readback/full-x64/Results/shaderbounds/23da43c872d64de587069ad5c12eea69/Results/{original,candidate}/result.json`；x86对应 `8424563637f2461db66a3367d627bb7f`。两边290–300行亲读：1048576两边红，1048580在disabled=0官方白/候选红；不能写“1048576也已差异”。x64后续disabled1/-1/2均红、复置0再次官方白/候选红亦读。
`Pcore/ShaderBounds.cs:14–40` 在WPF初始化后调用真实开关，构造1MiB±4的NOP填充有效资源；44–58行SoftwareOnly+真实RenderTargetBitmap首像素；`ShaderBoundsComparisonTests.cs:13–17` 两角色成功后零阈值原JSON比较。后续定位资源字节上界/BOOL恢复语义，不扩大成HW显示簇，不改golden/阈值；本轮未定位到候选具体拒绝/接收实现，不伪称生产修复。

## 官方 sampler AV 单独账与已修回归
`1c6be0d5…`：x86隔离子原监督 `readback/full-x86/Results/jv-full-x86/historical-isolation/WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain/25ec0771d3794069a5030eac277afaee/supervision.json` 实际Exit -1073741819 / 0xC0000005、无timeout/kill/drain；父仍Failed，非skip，不冒充候选崩溃。x64同方法 `c8208390b3234b4a956a69ca7011f8ce` 子TRX 34–43行是原生首帧Pixel断言，正常Exit2，不是AV。官方geometry `f6253c17ef31444fa6e8e9535155bef1`同样子TRX直接核首帧失败。生产日志“production=Native”不是这些官方场景actual身份，actual须引用前述Requested/Actual模块原件。

父TRX直接核已修旧ID：mode7 `1bbc964c-b6e5-8d5d-8d59-fba9c3c3ca06`、mode8 `1308b204-c717-8327-88c1-71486c5f582a`，两架构950–951行均Passed；Rounded `17bea8a5-bb70-8ccd-b35b-5694c41d27d2`、Curve `13a2f345-685a-81b1-bcce-02ed89041f18`，x64 3865–3866/x86 3695–3696均Passed。仅核父原件，无需重读全部角色，不推算旧93减少为当前。

## 1802→1809：当前可证新增回归类，精确增量未闭
旧full1802/1709/93是历史，当前1809/1765/44及1809/1790/19是实际，不能减旧93凑绿。旧TRX/93表指定历史路径在先前核证缺本地件，本轮不无限寻档，不造完整ID diff。当前默认定义与父TRX可证七项回归：
|当前定义/参数|ID|本full|
|---|---|---|
|Jl safe fill (7,0,1)|19e156fe-40f7-8a3c-a812-fb04a35990bd|双架构Passed|
|Jl safe fill (8,0,1)|1b0de0d9-64d4-87c4-a216-39f72ba0cb2b|双架构Passed|
|Jl safe fill (7,4,1)|1b7b309a-a209-8a55-9359-f5835019b56f|双架构Passed|
|Jl safe fill (1,0,1)|1dad025a-f440-845a-b9b1-873cde6f965e|双架构Passed|
|Jl safe fill (2,3,1)|11b1bba7-0f92-850b-bca9-33f7fd65030a|双架构Passed|
|Jl safe fill (1,0,1e8)|1a6665d0-aca0-810e-b7fc-6f7ed1830bc1|双架构Passed|
|JM resolver重复族父状态回归|1db769bc-7295-84c4-9198-6119b49e4590|双架构Passed|

源码 `JlPresentInvalidArgumentTests.cs:13–24` 六DataRow默认有效；原JO独立旧红方法 `1567885e…` 被JO_PRESENT_REPRO guard包住，不在本默认清单，不能错算为第七项。七项父TRX位置x64 3458–3464/x86 3286–3292；确切旧集合差与可能替换/删除尚未闭，以上只称当前可证新增回归类，不以此拖延63条分类交付。

## 最终边界与核证范围
JX 逐条读完63条id/name/message，按需读stack、上述父Counters/修复ID及业务JSON关键段、双方监督/代表actual、关键源码；不是14309+14141行父TRX全文或所有角色/原始像素/异常路径全审。JX原文曾声明写后回读本报告全文、三共享最新节与分类/对账受改段；此声明属于历史记录，不能证明JX实际完成文档回读。本次由JY完成报告全文及三共享最新受改节的写后回读；历史只读恢复所需前部及引用段，不声称千行历史全审。初次分类按像素直接归产品，补读原生前置源码后21条更正为显示前置，另9条stack更正；以终态表为准，原失败原件未改。
JX生产/测试/项目源码修改0，新build/publish/沙箱/full/脚本/终端/工程/Host均0，仅既有JV报告、三共享最新JX节及本地JSON/TSV。源码无改不需新build。保留JR/JN/JD等既有修复与历史，旧独立managed4256不移绑。
**Available、HWbackend/loss、mediaresources、final四格仍开放，真实恢复/长期资源未闭，完整替换未完成。JX现有清单分类已落盘；文档交接由JY关闭，不是生产缺口已关闭。**

## JY 最终补记：落盘分类与文档闭环完成
JY首先读取既有 `jx-reconciliation.json` 与全部63条TSV记录，确认x64为3产品+2夹具+38显示+1输入=44；x86为2产品+13夹具+2显示+1原生子崩+1输入=19。共同19、仅x64 25、仅x86 0、并集44；同ID不自动同症状/同因。分类直接症状已完整交付，待定0不是全根因关闭。

完整失败原件路径（均相对项目根 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape`）保留不变：
- `artifacts/jv-postfix-full/inventory/Results/jj-full-inventory/x64-failures.json`
- `artifacts/jv-postfix-full/inventory/Results/jj-full-inventory/x86-failures.json`
- `artifacts/jv-postfix-full/inventory/Results/jj-full-inventory/summary.json`
- `artifacts/jv-postfix-full/inventory/Results/jj-full-inventory/jx-failure-classification.tsv`
- `artifacts/jv-postfix-full/inventory/Results/jj-full-inventory/jx-reconciliation.json`
- 父原件：`artifacts/jv-postfix-full/inventory/inputs/x64.trx`、`artifacts/jv-postfix-full/inventory/inputs/x86.trx`。TSV各条rawReference继续指向原失败JSON行号，不改raw内容或初始summary标签。

真实双full仍为各1809：x64 1765通过/44失败、x86 1790通过/19失败，均skip0、监督Exit2/Failed。实际task、父testSHA/MVID、Native/官方actual模块和运行条件沿用上文报告已核范围；JY只核分类及文档，不重读原始监督、TRX、模块或源码，不把代表身份扩大为全部角色/源码逐字节闭包。输入总字节未知，七项当前回归定义可证但1802→1809精确集合diff仍未闭，不影响当前完整失败清单交付。

旧误读以终态更正：JV“即将full”、JW/JX“进行中”是历史；旧93不得相减推当前；官方sampler x86 AV与x64正常Exit2分账；WindowQ缺输入不是零差通过；x64的38条显示直接前置不证明所有产品HW无缺陷；旧managed4256为独立账，不是最终四格全过。JX未完成的共享文档/回读不冒称完成，现由JY关闭。

下一优先分派两个可证夹具根因任务并行，均不重full：
1. x86 WIC：仅修测试sourcePacket架构包长（uint+uint+nint，x86 12/x64 16），保持生产严格size校验，后续以11条精确ID有限验证；不承诺对应x64显示失败恢复。
2. window角色路由：专属角色/方法隔离，不能把WPFGFX_RUN_ROOT或results-directory当original/candidate；保留非法角色64失败，不改公共路由、不空过；后续有限验证双架构各2条。
第三任务后续处理ShaderBounds真实内容边界（1048580、disabled0官方白/候选红；1048576两边红），不改golden/阈值，本轮不新跑。

JY仅修改本报告及三份共享文档既有文件；源码/测试/生产/项目修改0，新实验/build/publish/沙箱/full/终端/脚本均0，分类JSON/TSV及raw原件0修改，不新增重复报告。写后回读范围为本报告全文及三共享最新JY节/受改历史标题，不反复千行历史。**分类与文档交付已完成；产品根因、最终四格与完整替换仍未完成。**
