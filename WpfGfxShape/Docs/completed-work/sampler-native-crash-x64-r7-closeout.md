# sampler-native-crash-x64-r7：可信调用链与 worldstack 契约收尾

状态：CloseoutCompleted / EvidenceReviewed / FixtureContractIdentified / ValidationPending。正式报告与九份基线/失败索引/恢复计划入口已逐份回读；本状态只关闭r7证据与静态契约切片。本轮只读源码、读写报告；代码0，build/publish/sandbox/业务/下载/脚本/再委派0。r7既有执行为normal COM build1（147warnings/0errors，exit0）、publish1（exit0，含隐式编译）、offline1；并非本轮新增执行。生产、fixture、expected、SDK、TFM均未改。完整替换仍Open。

## 1. 正式结论

确定的fixture缺项是 **HWND渲染根visual缺少显式world/DPI MatrixTransform绑定**。这不是UpdateWindowSettings中的一个漏填矩阵字段：该命令根本没有worldTransform字段。真实WPF在HwndTarget.CreateUCEResources中创建/引用_worldTransform资源，并以CompositionNode.SetTransform绑定_contentRoot，随后SetRoot；96 DPI也发送非空的identity矩阵资源。现有RunVisualCacheConsumer直接SetRoot到visuals[0]，只发image/content/effect与target/window settings，没有等价根变换。

已认证故障路径是HWND target → CDrawingContext::Render → DrawVisualTree → CGraphIterator::Walk → PreSubgraph → PushImageEffect → PushEffects → PushLayer → SetupEffectTransform → DecomposeMatrixIntoScaleAndRest → GetScaleDimensions。原故障RCX=0。结合匹配PDB行与本地稳定结构，最窄根因契约为：effect层要求非空world matrix指针，而无root/visual transform、无offset的原始DUCE树没有建立transform stack实体。普通绘制允许空栈代表identity，effect的按引用取顶却返回null；这解释普通image可绘、effect崩溃。

此结论确证fixture与官方managed初始化序列的缺项，不宣称私有DUCE协议有公开的“所有根必须带transform”规范，也不宣称已证明官方对所有合法空栈树都无bug。修正是恢复已见官方使用合同，不是改候选生产容错或golden。缺项补齐能否消除崩溃、像素是否一致尚未运行验证。

## 2. 身份、可信链与停止边界

原件：`../../artifacts/sampler-native-crash-x64-r7/readback/Results/sampler-offline-symbols/`。offline task a1fa3407521f4d8596d9afedb370bc36，PID13176，MVID ea0f6397-9ae6-4a91-931e-d0ee447c0e37，.NET10.0.13 X64。offline-exit.txt明确ApplicationExit=0、BusinessRuns=0、NetworkRequests=0；父工具没有OS exit，仍Unknown。

AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1；PDB SHA732FB582AF469CA52FD3B1C39520FFAD892EE6E9FAFDA1BC69A6066EF6212582，RSDS GUID8756f755-10f4-48be-9886-22c9a80c75b8 age1，MSF7及DbgHelp SymType3/Machine8664匹配。原dump SHA1851A7517FFE42FB688BD098EBF463624F59180BF183AA71ABD90E872451B85C。候选仍632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671。

|帧|RVA|认证函数|
|---|---|---|
|0|B65F|CBaseMatrix::GetScaleDimensions+F，原RCX0|
|1|B72F|DecomposeMatrixIntoScaleAndRest+2F|
|2|FFC53|CDrawingContext::SetupEffectTransform+D3，PDB3722|
|3|FECE9|PushLayer+C9，PDB3084|
|4|FE3EF|PushEffects+2FF|
|5|FDA1E|PushImageEffect+2E|
|6|1017C9|PreSubgraph+579，PDB4970|
|7|10C88B|CGraphIterator::Walk+6B|
|8|1009E2|DrawVisualTree+1F2，PDB4424|
|9|102FDB|Render+3EB，PDB6007|
|10|105DFE|CSlaveHWndRenderTarget::Render+26E|
|11|10442F|CRenderTargetManager::Render+5F|
|12|F36E8|CComposition::Render+28|
|13|F3559|ProcessComposition+1B9|
|14|F3666|Compose+46|
|15|B017E|CPartitionThread::RenderPartition+3E|
|16|B02FF|CPartitionThread::Run+6F|

帧0–16采用原CONTEXT、dump-first读取、匹配image函数表/展开信息及单调SP认证，不是symbols.txt的NotUnwound扫描候选。step17返回7FFC74E5CD87，UnknownModuleImage，Blocked=True后停止，无合成帧。无需扩展全栈系统模块。不得把unwind后volatile寄存器当caller参数；当前未测matrix stack count，不伪造其现场数值。

## 3. fallback风险审评

r7实现放行整节只读.rdata，超过原限定header/.pdata/.xdata的授权，是诊断范围偏差，明确保留。该image实际没有独立.xdata节：UNWIND_INFO在.rdata，例如RVA18924C。可信帧的函数范围来自匹配image .pdata，prologue/epilogue辅助字节来自.text，展开规则来自.rdata；实际返回地址、saved registers和栈槽来自原dump，原RCX来自原CONTEXT，名称/行号来自匹配PDB。.rdata不是堆、不是matrix对象重建，不能据其恢复原堆状态。

审核接受本次链作为有限诊断依据：相同SHA/RSDS、只读节、RVA/raw边界、dump优先、失败即停、源台账均存在。但同SHA磁盘只读节不能概括为任意加载后地址数据都完全等价（重定位/运行期修改仍需考虑）；不批准整节fallback作为后续通用堆补全。未来若改诊断，只限实际RuntimeFunction引用的unwind记录/链式记录及必要text，不能无声扩权。本阶段不改代码、不为审评再运行。

映射可用范围（RVA半开区间，非“已实际逐字读取范围”）：.text [1000,153C00)，.rdata [154000,1AAC00)，.pdata [1B7000,1CBC00)，按max(VirtualSize,RawSize)及文件边界；实际.pdata目录为[1B7000,1CBB5C)，84828字节。已核首个.rdata读为VA7FFC17DE924C、4/20字节，末段例VA7FFC17DE7CF0、4/16字节。没有将整节全部读过冒成事实。

本报告实际亲读unwind.txt行范围（包括截断后补读；不声称1589行逐字全读）：1–20（第20行显示截断）、30–38、60–68、163、240–254、348、445、532、620–631、722、812–819、912–919、1008–1015、1105、1188、1268、1358、1438、1517、1570–1587（尾行显示截断）、1587–1589。另通过文本索引核所有UnwoundFrame/Stop行位置。symbols.txt亲读1–22、27–47、100–106；offline-exit.txt1，receipt1–11；未声称全部重复ReadMemory行读完。

## 4. 源码契约核对

本地参考前缀：`src/Microsoft.DotNet.Wpf/src/`。

- WpfGfx/core/uce/drawingcontext.cpp：Initialize91–134只建brush/iterator/bounder，不压identity；BeginFrame153–201只切target/状态，EndFrame219清栈。PreCompute5655–5704委托另一个CPreComputeContext，不初始化此drawing context的worldstack。Render5900–6011与DrawVisualTree4367–4444只做precompute/dirty/cache/clip及Walk，没有根矩阵兜底。
- PreSubgraph4840–4885：只有非零offset或非空m_pTransform才PushOffset/PushTransform；root无变换不会调用“PushTransform(null)”重载。PushTransform2331–2358确实可把显式null调用转换identity，但不能误以为无visual transform会执行它；矩阵重载2384–2403和PushOffset4126–4145才实际压栈。
- MatrixStack.cpp170–233：Top(empty)返回identity值，GetTopByReference(empty)返回null；Push1–95支持首个矩阵直接入栈。故“数值上等价identity”不等于“引用不为空”。
- PushLayer3035–3095取得GetTopByReference并传入SetupEffectTransform3695–3740；后者无null分支直接Decompose，与认证PDB3722一致。可信帧6是PreSubgraph，帧8是DrawVisualTree，无DrawRenderData帧；仓库文本检索未见UpdateTransformStack/DrawRenderData命名，不编造该函数的调用。该版对应职责在PreSubgraph/PushTransform及render-data执行器。
- hwndtarget.cpp175–199：BeginFrame→Render(root)→EndFrame，无DPI矩阵压栈。HwndTarget.cs250–280创建DPI MatrixTransform；735–808 AddRefOnChannel→CompositionNode.SetTransform(contentRoot,hWorldTransform)→SetRoot→UpdateWindowSettings，是缺项的官方managed使用依据。
- MILCMD_HWNDTARGET_CREATE（wgx_commands.h354–369）仅DPI awareness/scale和target参数；flags12=PresentImmediately4|PresentRetainContents8（Resource.xml579–588），不是“初始化worldstack”开关。UpdateWindowSettings结构379–394没有worldTransform；SendOutputWindowSettings73–85发送72字节windowRect/alpha/renderingEnabled，不能替代visual transform。
- RunVisualCacheConsumer：VisualCache.cs125–165构建无offset/transform的image visual；250–284绑定effect后直接SetRoot。target32×16、DPI来自GetDpiForWindow/96；draw宽32高16均非零。BindTwoInputEffect543–604：隐式输入brush存在，secondary为VisualBrush/BitmapCacheBrush引用另一visual，而非直接bitmap-effect输入。可信PreSubgraph→PushImageEffect证明当前崩在主HWND树建立effect layer之时，不是secondary离屏context的空初始栈；4955–4977的RenderCompatible分支调用PushImageEffect；离屏层矩阵Push在PushLayer后续3405–3436才发生，不能回补此前解引用。
- VisualBrush命令（wgx_commands.h1030–1053）Viewport先、Viewbox后；fixture偏移16/48为各rect起点，宽高32/40和64/72均1，Units108/112均1。此包未见高层viewbox/viewport互换或零尺寸触发；这些rect也不是worldstack初始化命令。effect122字节局部一致只说明此局部，不能推成完整初始化序列合法。

精确官方commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699源码本轮未获取，不把本地checkout冒成该commit。依据强度是原AD1C/PDB行认证加本地稳定结构、managed实际发送序列；不是精确版本源码哈希证明。

## 5. 最小合法修正表及下一预算（尚未执行）

|对象|最小更正|保持不变|
|---|---|---|
|fixture主root visuals[0]|在首次SetRoot/commit前创建TYPE_MATRIXTRANSFORM，发送MILCMD_MATRIXTRANSFORM，将M11/M22设DPI scale（本case96 DPI即1），其它项0，动画handle0；发送MILCMD_VISUAL_SETTRANSFORM绑定root|不靠非零offset、改shader或禁effect避崩|
|命令声明/生命周期|使用独立官方协议声明核type/size；创建→矩阵更新→绑定→SetRoot→close/commit；资源按已有channel所有权规则释放|不猜handle/type数字、不提前释放、不改生产|
|oracle|官方先跑原精确FQN，候选随后跑同包同输入|expected、像素阈值0、sampler/缓存/输入均不改|

下一可直接授权：只做上述测试fixture最小修正，normal COM build≤1、COM publish≤1（含隐式编译）、隔离沙箱≤2（official原FQN1，official不再崩且正常完成才candidate相同FQN1）；Native0、offline0、full0、新dump平台0，外层每角色≤300秒。只验证当前x64切片，不声称覆盖x86或23红集。首个失败即停，不重试、不轮换开关。保全模块SHA、命令sequence、ClassInitialize/原FQN、TRX/像素原件及cleanup/exit；若仍同一空矩阵崩溃，唯一下一现场量为PushLayer入口该context的transform stack count及root.m_pTransform（与同PID/TID绑定），不得再无限静态选门。此预算是建议，不是已授权执行。

## 6. 基线、偏差与终态

r7既有build日志原115165字符仅留末20000、前95165被截断；receipt记64行回读，不虚称完整编译输出。publish11行记录成功。symbols中symsrv加载失败与SymFromAddr成功但旧LastError487照实保留，成功按返回值/身份认证，不抹诊断噪声。symbols.StackDecoded=False为扫描阶段字段，可信链见独立unwind.txt，不能混用。

全部r2误根build/触达Native、r3 rootReap258、r4第二offline角色超配、SDK API403/404、r6漏image读取、r7整节.rdata超授权，以及receipt空参数/未回读编辑被拒记录均保留。rootWait5000ms仍未新增运行验证。诊断默认关闭，只取证不等于生产通过。

原full：x64 1823/1803/20fail；x86 1823/1820/3fail，共23红不减。memoryGreen=null，父OS exit Unknown，worker退出未证，显示/HW/target/loss/音频/自然资源/final full与完整替换Open。本轮证据收尾和根因契约调查完成；fixture修正及业务验证Pending，不以文档关闭完整替换。
