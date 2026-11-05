# ID：同 partition 新增 listener 全监听快照广播

## IE最终裁决：保留ID一行广播修复，有限合同验收成立

IE独立核证，不重跑已完成组；本轮仅修改本报告及handoff/session-summary/issues四文档，生产/测试修改0、发布0、沙箱执行0，最后正常Release build一次。下方原“进行中”内容为ID历史，数字不代表最终状态。

### 源码与安全边界

SameThreadChannel.cs:121–131只在_notifications.Add(channel)成功后，将原64字节partition snapshot向_notifications全体PostMessage；IA NoDevice字段、重复enable不发、disable仅Remove均保留。官方composition.cpp:1241–1285先Add/AddRef、取m_deviceState old/new、NotifyHelper；2282–2292按集合下标遍历全部监听者。候选HashSet无官方数组顺序保证；本合同只要求各channel收到快照，不要求跨窗口全局顺序，测试按channel排空而非验证HashSet枚举顺序。不得宣称插入顺序或任意跨线程全序等价。

PostMessage(byte[])在_messages锁中检查_destroyed、入队；只有队列原空且HWND非零才PostMessageW，随后Set事件；无HWND仍入队并唤醒。SetNotificationWindow与PeekMessage也锁同一队列；Peek只复制数组到调用者，不改共享snapshot，本次广播数组构造后无修改，队列强引用维持消息寿命。PostMessageW为异步窗口投递而非直接用户回调，不能把wake数当payload数。HashSet保存托管channel强引用，不等于直接测到官方AddRef/Release归零。

已实读UceChannelExports Commit/SyncFlush/Destroy及Notifications：同步提交/销毁走Gate；异步工作由PartitionScheduler单worker执行，destroy排入同队列，离开Gate后才等Completion；RegisterNotifications命令在提交执行路径，ReleaseResources先标destroyed并移除_notifications，再清窗口/队列、唤醒及释放资源。本行不新增集合修改或锁嵌套，单次枚举内PostMessage不反入集合。此为已查调用路径及有限运行无回归结论，不是全线程安全证明；_destroyed写入及其它未穷举路径不因静态审查获普遍正确性背书，不新加锁或状态算法。

### 旧红必须分账

证据根artifacts/id-listener-broadcast/readback。baseline-x64 task a7c04b4caeb74b52aa713e5e686222d3：candidate PID6404，Native D74F79F56146FC544FAAD4AD8ABFA97A83AF328E25257C207C536B281BB27DE3，test604268D2694942BD1876D53D7FD042493E1EF1F46DD9B8818F29A08CB2E5A0A6/MVID3206ee59-db2e-46a8-be3f-00a73b7c6e38；A-enable已1/1内容正确，B-enable仅B、A expected1 actual0，role TRX确Failed/Exit2。它是当前IA行为的真实广播旧红；不同构建SHA不偷换成IA旧报告531E…。

old-x64 task9c068be6a2fd4ae386049f25c5606751加载2BFC4AC24A83EBAF45A4B375CDC5FE1F9C90BD9370D73BD590C259091C5BF58F，A-enable已经old/new0/0、hex index17失败；仅误包初态失败，不能当广播基线。两原件均保留。IC双架构旧红另见partition-state-transition报告，本轮不冒重新全文复核IC全部事件。

### 默认新绿：独立进程与实际身份

|架构|task|父TRX/Exit|official / official-repeat / candidate PID|
|---|---|---|---|
|x64|acfbc84543834ad4bda9d93ed1108069|2执行2通过/0|2228 / 9676 / 4624|
|x86|992ccc01c6804d468464bb9cb7f1b2fb|2执行2通过/0|7652 / 10476 / 2456|

父filter FullyQualifiedName~ListenerBroadcastTests；六role均1执行1通过、exit.json Exit0/timedOut=false。四监督（含下方guard）均Reason exited、Exit0、OutputCollected=true，TimedOut/Cancelled/KillAttempted/OutputDrainTimedOut=false、SupervisorError=null。父默认direct不是空跑：PID7932/2368另有listener-broadcast-direct business，实核同11条/15阶段及cleanup。

Native实际SHA：x64 1AFFBE40F24D35DC317FE05D3D1F64C62AC136700F5D8D50B41A9B7252D68863；x86 C3D357371E070F3A81CCA3F955F50DC45A1A1F56A46680AE118930432C424AF7。默认test x64 FFF06900ACA7FEA34F3140678977FBED69AF19743C784529E6EA57391C98612D/MVID2a79bf46-cba3-4ad5-8453-8336b1a16f79；x86 233DB2024F4E88694396723942CC76A4497DF360D485E983FD1A34CADB035466/MVID3226e120-82ba-47c4-a4b4-3987b2cb33a5。business及aot-identity/监督交叉亲核，runtime.NET10.0.13、X64/8与X86/4。

官方实际加载task根wpfgfx_cor3.dll，候选加载Native子目录，两者不转发；官方SHA x64 AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，x86 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834，版本10,0,1326,50504 commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。test-obj assets/cache亲读Microsoft.WindowsDesktop.App.Runtime.win-{x64,x86}10.0.13来源记录；guard实加载PresentationCore同10.0.1326.50504（x64 SHA0E605CCC…/x86 D2A9C8B6…）。这是包来源/实际版本链，不是重新下载验签或servicing源码逐行同源证明；此处所有SHA均原运行记录亲读，未伪造重新hash。

### 消息内容、数量与注销销毁

六business的observations完整亲读，每个11条64字节：type5@0、reserved0@4、old1@8/new1@12、16–63全零；全部128个hex字符逐条核，不只读type。15阶段counts(A,B)依次为：A-enable(1,0)，B-enable(1,1)，B-repeat-enable(0,0)，A-disable(0,0)，A-repeat-disable(0,0)，B-disable(0,0)，B-reenable-alone(0,1)，A-reenable-both(1,1)，A-disable-before-windowless-broadcast(0,0)，A-enable-B-windowless(1,1)，B-disable-final(0,0)，B-enable-before-destroy(1,1)，A-disable-after-B-destroy(0,0)，A-enable-after-B-destroy(1,0)，A-disable-final(0,0)。全部与独立官方及repeat一致；retrieved是BOOL，不是返回64字节长度。

六角色windowless事件69–84已亲读：注销B窗口hr0后，A再注册仅A收到33105/w=l=0 wake，但A/B均收到队列消息。候选两架构销毁监听B hr0、之后A重新注册正常；六角色最终window-clear/destroy/disconnect/deinitialize均hr0、cleaned/joined=true/windowError=null。没有强制wake与flush全序（官方可先flush后wake，候选可反向）；必要flush→窗口barrier→drain成立。重复中段事件未逐项全文，官方销毁中段以observations与通过断言/末端cleanup交叉，不冒全部事件全文审计。targetCommandsSubmitted=0、actualStateTransitionCovered=false：仅未渲染静态注册快照广播合同，绝非真实状态变化或渲染后snapshot正确。

### Guard适用兄弟：30不是30个默认产品闭包

x64 task0a99217da78d4635b55f7a6661b12369、x86 task473002d2898a496389dc279d4f9b1b6c：父各30执行30通过、监督Exit0。两父TRX分段回读结果/定义/entry/计数，首轮截断的定义/entry尾部已补读；并核监督完整filter。组成3通知/递归锁+2HY初态+12候选异步排空+6官方排空+1HW有限注销group+6HU关闭；含显式探索包，不冒30项皆默认闭包或硬件验收。

Guard test x64 2DEB6B2561ED6994B26E08665889DFBABEBED2E28007DCEDBAB2069A75AF195F/MVID2f0b75b2-910a-4283-9af0-d5736916d434；x86 BD56BDE456979584C18E3DD103D036A223F33DE829A909747FC4C00798AD307C/MVID412c215f-5e6e-40e7-acfc-e594f51a62a0。HW候选与HU关键原件实际Native均与默认新SHA相同，非沿用IA旧绿。

HW四角色关键事件亲核：dispatch进入→unregister返回→callbackexit；窗口注销不撤已排队消息，retained1、新windowless backchannel1、最终disabled0，callbacks1/ownerJoined=true及destroy/disconnect/deinitialize hr0。此HW命名组scope明确nonHW class5、没有HWtarget/SyncModeStatus。HU两候选False-6完整207行业务原件亲读：closeReturn时disposals0，surviving external release触发callback.enter/exit后disposals1、refs0；siblingDestroy/deinit后各等待完成且仍1，子Exit0。其余关闭变体以父TRX通过为证，没有逐个全文业务核读；不外推长期资源或无限未来exactly-once。

### 包审计与最后构建

实读Package.targets白名单、自包含RuntimePackAsset来源、默认两包package-files首45/末30行（419/418行）；Contracts及Native/官方根DLL明确分离。输入com-acceptance-package与readback/Results不混。未完成四包全集合差分、逐文件重算hash/签名及RuntimePackAsset完整求值审计，不能宣称完整包审计通过。

IE最后只执行一次默认正常分析器Release solution build：Exit0、3772warnings/0errors、9.16秒；未传NoWarn/关闭分析器或规则、未publish。日志工具62行全部回读，但stdout原3160257字符仅保留最后20000，前3140257字符已丢弃；这些不是可继续回读的完整日志。计数/Exit来自可见尾部与工具元数据，不宣称全警告审核；增量build不替代NativeAOT或双架构运行。

### 最终交接与唯一下一状态门

保留已实证的一行广播修复及IA初态、COM/fill、旧红原件；没有具体新回归或核心失败，不因未来状态门开放而撤回已证合同。本轮未动target聚合/GeneratedTargetRegistry/DRDS/NativeRun/ExitDiagnostics/VisualCache/historical/捕获/打包项目/CF CX CY，未创建Host/工程或运行终端脚本。

下一唯一状态门：先在官方独立进程合法驱动同partition真实状态转换（审计指向HWND Present/HandlePresentErrors，不能WIC/no-target成功冒Normal），记录同源old/new及渲染后注册snapshot，再据此最小实现权威状态更新；不得从单target HRESULT猜全局。动态转换、渲染后snapshot、有效HWtarget非零sync、Available、长期资源、HWloss、media、full四格/x64Event0全部开放，完整替换未完成。

最后补记（IE）：本报告和三共享最新IE节写后回读；历史进行中和旧下一门保留为历史，不覆盖原运行结果。

## ID历史进行中

已开始执行。保留 IA 初态权威字段及 IC 双架构旧红证据；目前生产修改0、构建/发布/执行0，未取得新验收。先核执行规则、完整方案、IC测试与官方注册/队列合同，再建立独立默认产品回归；只允许最小通知注册修复及必要回归和报告。不得将 NoDevice 1/1 静态快照称为真实状态转换。

IC历史 task：x64 c7326c5073374edc86e954d79dc0f959；x86 5fe48015961042b5b7a25820ffb7a250。历史官方11条/候选7条仅作待核证据，不作为本轮绿。

最终保留生产行为须经新双架构默认回归及适用有界兄弟验证；若核心门失败且本轮无法修复，则撤回本轮广播行为，保留 IA 及历史证据。Available/HWbackend/loss/mediaresources/full四格/x64Event0仍开放，完整替换未完成。
