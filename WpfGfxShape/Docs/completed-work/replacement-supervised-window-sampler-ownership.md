# JB：旧窗口 sampler 引用及事件所有权安全边界

## 终态

新增专属 D3DImageTests.JbWindowSamplerOwnership.cs，全文件 JB_WINDOW_SAMPLER_CONTRACT，official/candidate两个精确测试。不改默认、JA、共享helper、生产或项目打包。所选合同是真实D3D9 HAL surface/Interop bitmap的0xA转交→null更新撤销持有→resource delete/flush→destroy/disconnect/deinitialize→Detach→bitmap/surface/device/factory Release返回0；同时验证从未记录/交付的event副本关闭一次，原柄继续可用再Dispose。它不是DS无surface Present完成重复：本控制有真实bitmap，测服务端持有+1和清空后-1，不发送Present，不声称执行windowconsumer。

未验收成功生命周期：正式发布被本机磁盘空间不足阻断。official task0/1、candidate task0/1、sandbox0、full0、显示前置0、sampler内容0。没有JB task/PID/TRX/actual module SHA/test MVID/runtime/ledger/events/refs运行原件，不能造数。生产IP预期0C16A66C898269A17DBDD062012898FEBE338E341FED8D0B7FEC47FB21B6A23A仅作为拟复用输入，不是本轮actual。无重复native红帧、旧Event0、DS或随机变体。先创建本报告并前置handoff JB进行中，JA并行节保留。

## 实际发布与最后default

正式ComAcceptance Release/win-x64 selfcontained10.0.13，独有artifacts/jb-sampler-ownership/{obj,bin,publish}及WpfGfxShapeArtifactsRoot，显式PublishDir，正式targets的com-acceptance-package。AotDllPath指向既有artifacts/is-shared-consumer/x64/com-acceptance-package/Native/wpfgfx_cor3.dll，不重发Native。

发布调用4次均Exit1：前两次首次编译及ErrorsOnly诊断明确CS1686（局部函数捕获bitmap/channel/image又取地址），已改独立out局部变量。第三次正常分析器发布进入运行依赖复制，MSB3021/MSB3027磁盘空间不足、FileList写入失败；末42行全文亲读，输出7172862字符前7152862不可见，不冒完整审计。可见汇总4043warnings/783errors、11分12.89秒，不能当最后源码成功编译或源码错误数。ErrorsOnly只过滤诊断输出，没有NoWarn/关闭分析器。第四次尝试定向-t:Clean的发布调用仍进入编译并CS8104空间不足，不能当成功清理；未删除历史/共享/回收根，未运行脚本或终端。

最后default normal Release/win-x64 build Exit1：CS8104向默认artifacts/obj/WpfGfxShape.ComAcceptance/Release/net10.0-windows/win-x64/WpfGfxShape.ComAcceptance.dll写入时空间不足。首错亲读，64行未全审。最后默认源码符号隔离未变，但**default构建未验证成功**，不能沿用历史green。专属源码随后把调用CloseHandle前清零本地副本owner，避免关闭失败时finally盲重试；该最后修改未成功构建。

未完成发布，没有clean包/manifest/runtimeconfig/Contracts/aot全核或离线方法/符号闭包核证，不能声称已排除零测试，不上传残包/旧包冒JB。目录工具返回专属根无子项，与构建绝对路径不一致，仅记录观察，不断言机器无残件。可用专用工具没有完成白名单回收；不绕到脚本。

## 逐对象、逐动作持有者表（静态合同，不是本轮运行）

|边界|Completion原柄与duplicate|0xA额外引用/bitmap/surface|brush/effect/shader/visual/data/resource|
|---|---|---|---|
|创建/Send未进入|原柄SafeWaitHandle归调用者；duplicate仅调用者拥有，取消可close一次|调用者bitmap原引用；bitmap内部可持surface/device；额外AddRef待转交，未记录由调用者负责|master channel分配handle；create0不是服务端已处理|
|进入Send后返回失败|副本可能已记录，不凭HRESULT认未接收，不盲补Close|额外AddRef可能已记录，不凭失败Release同一份|可能部分记录，保存创建与动作清单|
|已记录未CloseBatch|原柄仍调用者；副本只是recorder内整数，非自动SafeHandle|待服务端偷取的传输引用；调用者保留原引用|图引用需实际解码建立|
|已Close未Commit|closed数组持批次内存，不表示副本已关闭|未证偷取，不先释放bitmap/surface|master free index不等于服务端删除完成|
|已Submit处理未知|部分批次可先成功；失败不回退caller-owned；保留原柄|同理，Commit0不证明resource持有|不能Commit0认图已构建/删除|
|已Submit丢弃/zombie|可能S_OK，只通知zombie/flush并delete batch；未执行Present就不能认signal/close|batchdelete不证明内嵌AddRef释放，未知转交隔离|服务端状态不确定，杀进程不认正常refs0|
|Interop ProcessUpdate执行|不处理Present副本|Release旧bitmap/software；偷取新interop AddRef；失败释放偷取引用；software转交临时引用末尾Release|image resource持bitmap；图依赖由对应resource/notifier持有|
|Present事件确认|官方Cleanup即使Present失败也SetEvent再CloseHandle；wait返回时Close可能尚未执行，禁止自行补Close|Present更新software source；不是内容或所有refs0证明|事件不证明window consumer像素/图删除|
|SyncFlush完成|单独flush可来自zombie通知，不能推副本已关；正常event+flush提供偏序，不是close计数|新控制额外测ref变化，不以flush独立认转交消费|正常解绑/delete/flush后destroy，flush不独立证明全部释放|
|Detach/delete/destroy正常|原柄独立Dispose；已转交副本不再手工Close|Detach不等于Release bitmap；清空0xA、delete/flush/destroy后逐原bitmap/surface/device/factory Release0才认所测收敛|target root=0、visual effect/data=0，逆依赖delete/flush，然后destroy/disconnect/deinitialize|
|Detach/delete/destroy失败|保留原柄/副本账本，不未知补Close，防doubleclose/句柄复用|未知转交和指针留自有子进程，监督结束不是refs0|成功动作不重放；未知在途不继续释放可能被使用对象；不认产品通过|

只要求一个合法正常路径可测闭合，不要求穷尽任意未来错误。失败未知责任是保存现场、禁止重复释放、自有独立子进程监督终止，不是宣称官方错误都会自动清理。新测试进入0xA Send前标记uncertain，转交后+1、清空后回baseline断言均通过才清除；未知阻塞等待现有120秒监督结束，OS回收不记refs0；该分支本轮未实跑。

## 真实缺陷与保证不足

SharedConsumerLifetime全68行仍IS显式保护，Dispose只处理原Completion，Submitted/Completed不足以编码部分提交/丢弃，duplicate失败责任无安全状态；不能启用为默认。这是测试helper状态设计缺陷，不是旧官方sampler AV首坏写定位。VisualCache仍connect1/旧Event0；不得机械转async或继续危险默认复跑。0xA预AddRef后send失败责任无完整账本，finally destroy/disconnect/Release不足以证明额外引用释放，是保证缺口，不是本轮证明每次正常必漏。shader/VisualBrush静态字段未发现可据以修改生产的错误；旧IS Commit0 EOF不定位候选生产缺陷。

官方亲读clientchannel.cpp329–369（separate Begin/End/Close）及520–586（closed数组/逐批Commit）；connectioncontext.cpp356–390（Submit失败也转交内存/查channel失败delete）；crossthreadcomposition.cpp113–177（zombie signal flush/delete/S_OK）；cmdbatch.cpp20–65（空析构）；d3dimage.cpp182–237及249–310（偷取/失败释放/event signal-close）。这是仓库源码对应，未绑同servicing反编译，未审所有基类/丢弃分支，不泛称每条失败必漏。

## 可提交共享正常路径修复设计（未应用）

1. 普通surface分流async生命周期，callback/WIC/same-thread恢复兄弟不机械迁移。复用IV原148字节VisualBrush、122effect、324shader及0000FF80→00FF0080和全部cleanup断言。
2. 每次0xA/Present强类型账本：CallerOwned→RecordingUnknown→Recorded→Closed→SubmittedUnknown→Confirmed→Cleared。原柄/副本独立、bitmap原引用/传输AddRef独立；跨调用前置状态，失败不回退、不补close。
3. Present非零duplicate、separate1、commit、wait、flush；成功0不能独立推进Confirmed。shader/brush/effect/data/visual/target完整创建清单。
4. 原内容首/后态通过后Detach，解绑target/visual/data/effect依赖，末合法Present完成，再清空image的0xA、逆序delete/close/commit/flush，destroy/disconnect/deinit，逐COM最后Release0、关闭原event/window；未知转交留独立子进程由监督结束，不走正常通过。
5. 本轮所选控制先测0xA+1/清空回基线/最终refs0，再整合IV合法窗口合同，无需穷举未来失败才能正常实跑。

## 下一唯一门

先恢复本机存储（仅既定自有白名单、不能共享回收根/脚本），重新正式发布**同一JB控制**，离线核两个精确方法/符号、runtime双framework10.0.13、Contracts/Native/hash/白名单依赖。official独立进程先亲读ledger 1→2→1及最终refs0，才candidate对照，各最多一次有信息组。BL<=120外180、绝对task Results/jb-*-x64/TestResults/child.trx；非MTP监督无parentTRX。本轮task预算未消费。

上述ownership控制实过后，下一原窗口整合门：独立同exe复用IV真实VisualBrush两输入HardwareOnly/非零duplicate/async；本task原native显示前置00563412真实通过才sampler0000FF80→00FF0080及全部cleanup原断言。前置不过只记内容未覆盖，不改显示参数、不SoftwareOnly替代、不producer读回/eventonly冒consumer；合法静态/无显示ownership控制不绑显示前置。

本轮所交为专属测试待验证实现、所有权表/正常路径设计与磁盘空间阻断，不是已验收修复。Available/HWbackend/loss/mediaresources/full四格及旧官方sampler AV仍开放，完整替换未完成。

## 阅读及末次补记

规则27、IV43、IU分段至112、IR含IT分段至96、方案首段（长返回截断）及80–120、handoff前段、SharedConsumerLifetime全68、VisualCache相关链与IV局部实现已读；不冒全库全文审计/历史全部原件重审。本报告与handoff/session-summary/issues自有JB短节写后回读，JA历史保留。具体阻断为本机存储，而非又以所有权未闭泛停工。
