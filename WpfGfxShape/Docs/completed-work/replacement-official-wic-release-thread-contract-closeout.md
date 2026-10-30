# official WIC release-thread 合同收尾

## 当前终态：合同修正 ClosedLimited，official O6 单体验证 Passed
本轮已取得具体双端最后Release可竞争的源码依据并实施验证，取代下文历史ContractEvidenceInsufficient终态；不是仅以“未找到保证”降门，不是完整替换通过。

### 源码合同裁决与范围
来源均为 src/Microsoft.DotNet.Wpf/src/WpfGfx/：
- core/uce/clientchannel.cpp:773–796 Destroy先SyncFlush后DestroyChannel；connection.cpp:308–346的SynchronizeChannel会SendCommand、CloseBatch、Commit并等待。故O6未显式Commit不代表pending bitmap没有执行；旧普通batch持有的channel引用在composition.cpp:499经SetChannelPtr(NULL)撤销。
- connectioncontext.cpp:297–346 CloseChannel的GetServerChannel仅借用（serverchanneltables.cpp:76–96无AddRef），关闭batch特意channel ptr=NULL，先SubmitBatch再caller RemoveChannelFromTable；162–179撤销表内server ref。Close批次自身没有额外channel owner。
- partitionmanager.cpp:330–394明确ASAP或延后处理，自己的Lock内入队唤醒后Unlock再返回；锁不覆盖调用者之后RemoveChannelFromTable。connection.cpp:264的connection锁不由DetachChannel获取；不是worker必须等caller撤表的同步契约。
- composition.cpp:1912–1945 Attach存储ref，1962–2018 Detach撤销通知ref（如有）、storage及GetAttached临时ref；203–212 close处理之后SetChannelPtr(NULL)对原NULL无新增引用。crossthreadcomposition.cpp:418–455顺序处理普通批次及close。caller先撤表则worker最后；worker先完成Detach则caller表ref最后。两条都是健康close合法顺序，不存在worker-finalref invariant。
- serverchannel.cpp:35–94每channel新建独立table并在析构Release，sibling共享composition而非同table；htslave.cpp:119–139/426–474释放同table资源；bitmapres.cpp:60–80将同一transport wrapper存为m_pIBitmap且撤销transport引用，析构156–160释放wrapper；bitmapwrappers.cpp:725–732同步释放WIC bitmap。common/shared/refcountbase.cpp:37–51及milcom.cpp:42–60在原子ref=0直接delete this，实际同步析构，不将此冒充所有IUnknown实现的通用必然性。
- apifunc.cpp:626–661只承诺slave持ref到用毕；SystemWicBitmap.DecodeFrame释放factory/decoder局部owner，fixture在close前释放frame/wrapper/stream。当前内置BMP/MTA及free-threaded wrapper使用范围不构成固定析构线程保证，也不外推任意STA/第三方codec。codec内部符号未知不阻断server最后owner裁决；无需证明旧2204具体最后owner。

### 实证与离线异常正式收尾
旧writer-fixed-default-x86-readback/Results/async-wic-drain-True-6-False.json全文1–96：caller/callback2204，callbackExited1/disposals1，Destroy.Drain0，completed=false，无调用栈字段；正常sibling/deinit未到，finally不能认证cleanup。旧失败保留。
原采集taskd3bc29c5365549969765b406ea5ee635 caller12612/callback10604、Destroy后callback。actual5EBD精确RSDS/PDB087c735f-43d9-448a-98ac-993527e505ec age1；原raw地址匹配worker→ProcessBatches→Detach→ServerChannel→table→bitmap→WIC wrapper→ManagedStream析构，不是完整unwind。离线taskbe350db878194b508cf2c095a65bb465在WindowsCodecs同GUID age3与RSDS age1不符处抛未处理InvalidDataException；严格拒绝正确，未生成offline-exit.txt、独立OSexit Unknown，绝非offline成功，不再下载。详见owner报告及offline-execution-receipt原件。

### before / after / 负控与回撤
唯一fixture文件 UceChannelTests.AsyncDrain.cs：原非caller硬门覆盖official和candidate；现加!system，只对candidate硬断言，official releaseThread仍完整记录（system=true为真实official路径）。新增afterClose callbackExited等待检查；callback exact1、close前活性、所有既有HR/正常cleanup保留，candidate非caller/reentrant0/workerexit与retainedExternal不放宽。生产/Native/TFM/SDK/包/expected/skip/负控均0改。回撤为还原该两行，不改历史结果。未新建负控，不宣称candidate已重验。

### 当前字节单UID验证与身份
独立Release/win-x86/net10.0-windows COM项目构建成功，146warnings/0errors/2.81s。绝对COM项目publish self-contained，两framework10.0.13、显式AotRuntimeIdentifier/双AotDllPath B07C已认证副本、ComAcceptanceSandbox=true、独立official-wic-thread-contract-x86-r1/clean；回读runtimeconfig和aot.sha256一致。build/publish工具返回成功但未提供独立数值exit。
先list-only taskcf7b80ab384a4be8b9d9ff3851b631b2，当前DiscoveryDiag:54确认official queueState6 UID1e4c35fb-589a-8de1-838e-4845fd1028f4，随后仅filter-uid运行taskdc59e26d409a4a8d97bce71b74beef11（外180秒、TRX Results、同步RunDiag分离）。TRX全文1–26：total/executed/passed=1/1/1、failed0/skip0；业务JSON全文1–153：system=true/queueState6/completed=true，callbackExited1/disposals1，全部记录HR0含Destroy.Sibling/Deinitialize。caller11588/callback2080、Destroy后callback，不是旧同步复现。
actual official SHA256=5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834；fixture SHA256=CC384959AC4CCE4C98B8AFC988ED2AC5E01CF7A31B90627B388CA359E23C5753，MVID=d587b9b3-8d28-4f49-bfc7-a59a59ad57d8，.NET10.0.13 X86。RunDiag:65 MTP session24d5c769-7e71-4bcc-9b76-c8d8b63fde6f terminal exit0；独立OSexit未提供。TRX+完整业务+MTP terminal有限认证，不外推所有资源、其它角色或全组。

### 回执、预算及下一主修正
即时参数/返回见 artifacts/wic-final-release-owner-x86-r1/contract-execution-receipt.md。normalCOMbuild1/2；COMpublish1/1（implicit build1）；sandbox2/2（发现1+单UID1）；Native0/full0/额外offline0，停止新运行。原owner采集build2/publish1/sandbox2、离线build2/publish1/sandbox1独立保留，不相抵。读取有4000字符上限的长栈/历史文档不冒全文，关键原件按小段回读；两次沙箱原返回无截断，乱码以TRX/JSON为准。
下一确定主修正为42索引G组真实HWcache Geometry首帧消费/官方与candidate像素差异，核合法0x8D writer后的真实硬件缓存使用和失效链，不新增O6同步追捕、不额外codec下载、不原样full。旧x64 1823/1823/1821/2、x86 1823/1823/1783/40及42失败实例全保留，不用单体绿减红。HWcache/原native输出/consumer loss/audio/自然资源/finalfull/完整替换仍Open，memoryGreen=null。

## 以下为上一轮历史终态与过程（已被上文取代）
CloseoutCompleted / ContractEvidenceInsufficient / FixtureUnchanged / ValidationNotRun。历史轮证据不足时未改未跑；不删除原判断与预算。

## 文档同步实际完成
两份 artifacts/writer-fixed-default-{x64,x86}-r1/full-receipt.md 已补最终TRX/监督/身份/cleanup边界并全文回读；writer-crossarch-closeout、writer-disk-recovery已追加已恢复并回读，旧BlockedDiskSpace及restore exit1未删除。next-session-handoff、current-work-item、current-stable-baseline、replacement-issues-and-plan-status、final-acceptance-remaining-matrix、完整实施方案及replacement-default-com-acceptance-failures均同步恢复轮事实并回读。failure-index的historical child Counters58已改59，重新读取两准确child的55–66行并回读修正：58为ResultSummary，59才是Counters。

权威默认full独立保留：x64 task975ed574ad46431f8d7079466757a0fb，1823/1823/1821/2；x86 task1e99f9304d9b4e88871dc63b200d0b80，1823/1823/1783/40，child均2。共42跨架构失败实例，不是42唯一方法。原失败索引、旧40/37红、O6同线程2204失败不扣除。主监督退出收集正常不认证所有资源；O6 callbackExited1/disposals1但completed=false，正常sibling/deinit未到，finally忽略HR不作为清理成功；其余独立cleanup Unknown，x64 exclusive明确complete。

## 静态核证来源及具体路径
以下为工作区官方源码，只读，不是actual5EBD精确符号/源码匹配证明。

1. core/uce/clientchannel.cpp 773–796：CMilChannel::Destroy先IGNORE_HR(SyncFlush)，调用connection DestroyChannel，再Release客户端channel。未显式Commit不等于没有worker处理。
2. core/uce/connection.cpp 262–293：DestroyChannel在caller调用栈释放client表引用，再调用connection context CloseChannel。
3. core/uce/connectioncontext.cpp 297–346：CloseChannel创建PartitionCommandCloseChannel，SubmitBatch后在Cleanup同步RemoveChannelFromTable。162–179明确同步释放pCompDevice及pServerChannel表引用。这建立caller同步撤销server-channel引用的实际代码路径，不是仅凭async名称推断。
4. core/uce/composition.cpp 1962–2018：DetachChannel由packet player处理，撤销notification引用（若有），再撤销storage及GetAttachedChannel两引用。它与caller表引用撤销存在不同执行端；但仅列出两端Release不足以证明当前O6全部额外owner均已撤销及哪次是最后一次。
5. core/uce/serverchannel.cpp 90–94：server-channel析构同步Release m_pServerTable及transport；157–160 GetChannelTable返回同一m_pServerTable。
6. core/uce/htslave.cpp 119–139：table析构调用ReleaseHandleTableEntries(NULL)，并BreakLinksForCyclicResources；426–474的NULL分支同步Release每个resource。
7. core/resources/bitmapres.cpp 60–80：ProcessSource将transport wrapper存入m_pIBitmap并释放transport引用；156–160析构同步Release m_pIBitmap。
8. core/uce/cmdbatch.cpp 100–122：batch/channel association还拥有channel引用，处理后撤销。该引用及其他owner不可从fixture的三次Release返回1直接穷尽。
9. core/uce/apifunc.cpp 626–661注释承诺slave取得source引用并在使用完释放，不建立异caller最终Release保证。

尚缺：wrapper到实际WIC decoder/frame/stream的所有权与线程合同完整链、当前健康O6在close竞态中server table/bitmap确为最后owner的充分证据、actual official5EBD servicing实现对应证据。本次未取得最终Release调用栈，也未把可能的caller析构链冒称已证明合法同步最终callback。未证明存在固定release-thread保证，也未证明不存在；结论是保证无法判定，而不是断言已错误。

## before / after / 回撤
|项目|before|after|回撤|
|---|---|---|---|
|official非caller硬断言|原fixture第210行适用条件下硬断言|原样保留；无合法同步路径充分证据，不降为记录|无源码变更可回撤|
|candidate非caller、callback重入HR0、workerexit|硬门|全部不变|不适用|
|retainedExternal|原生存与顺序门|不变|不适用|
|callback退出与全部cleanup改进设计|旧review中的待验证设计|未实施、未认证|不适用|
|生产/SDK/TFM/包/expected/skip/阈值|现有规则|0改|不适用|

因此没有新testSHA/MVID、actual runtime/file预检、新UID、TRX或MTP session结果；历史UID1e4c35fb仅旧失败标识，未套作当前发现。未发布official-wic-thread-contract-x86-r1/clean，未用旧clean冒新合同验证。candidate未参与本轮验证，不冒认证；独立工具OSexit无新值，Unknown。

## 真实预算及操作回执
normalCOMbuild 0/2；COMpublish 0/1（implicit build0）；sandbox 0/2（discovery0、singleUID0）；Native0；其它full0；源码0。不存在编译失败或新执行exit/log可披露，未调用根solution构建、终端、脚本、再委派或外部程序。停止原因是证据门未满足，不是文档未写。

本轮操作仅目录/文件检索、指定范围读取、文档替换及新建本报告。批量同步11项工具返回11成功/0失败；Counters修正返回OK。工具读取通常有4000字符上限：最初validation-closeout、failure-index、review、AsyncDrain大范围及部分长状态文档有截断，不宣称全文亲读；关键路径使用后续小范围补读，具体范围见上。完整两child55–66、两个fullreceipt、各新权威头段及Counters修正段均无截断回读。对单文件路径误用目录内容搜索返回“目录不存在”，未将其当文件不存在；随后通过直接读取/目录检索取得源码及child原件。新报告与状态终态再回读记录以实际读取为准。未逐字重新读取所有历史Passed/业务JSON/超长栈，未将旧代理记录冒本轮全量新审计。

## 下一确定行动及主要红
下一仅在已验收的official5EBD x86对应符号/源码下，闭合Destroy→RemoveChannelFromTable/DetachChannel→table→bitmap→wrapper→decoder/stream的最后owner与线程合同证据；不先build/publish/发现/UID测试，不重开foreground/paint/ETW/捕获循环。只有形成合法同步最终Release充分证据才可实施review中的official最小变化，并按原限额发现当前queueState6 UID后单UID验证；不能从“未查到保证”直接进入修正。该后续不是已完成的本轮验证，也没有新执行授权外推。

HWcache实际消费/原native输出/产品consumer loss/audio/自然资源长期收敛/finalfull及完整替换全部Open，memoryGreen=null。已默认双archfull独立保留，本文不升级任何主要验收门。
