# Runtime heap owner r1 正式收尾

## 已执行闭环：有限可回收性已证，非正式无泄漏

证据根 `WpfGfxShape/artifacts/runtime-heap-owner-r1/complete-sandbox-results/Results/runtime-heap-owner-r1-complete`；trend根 `ResourceLifecycleTrend/a70df5ce8da541168b5b19f5b0e75d1c`。最终task fd66cfc5590b461c8968cdf33d7e6e98。先官方同业务控制，再candidate独立诊断。官方无诊断导出不判失败。

前task792db958965f4236a903f4ca40938c45错误使用publish而缺aot.sha256，初始化exit2，已补持久失败账；复用targets已生成com-acceptance-package运行task52865bb56c344c6e8d5ec0c97011aa7c，通过但发现采集器IsPoint跳过三个末点，旧结果保留，不能冒齐全。只修测试侧条件白名单，在RuntimeHeapOwnerDiagnostic下开放round15三个既定末点；正式目标显式complete-clean重新COMpublish1，未改Native/包规则/初始化/硬门。最终一次补齐观测非追绿，不增轮、不延时、不重复GC序列。

## 精确结果

|candidate末点|tracked|scheduler/thread alive|DLL GC0/1/2|host GC0/1/2|Handles|Event|Thread|unknown|自有dup opened/closed|buffer opened/closed|
|---|---:|---|---|---|---:|---:|---:|---:|---|---|
|BeforeHostGc|16|16/16|0/0/0|18/17/16|316|109|28|71|245/245|246/246|
|AfterHostGc|16|16/16|0/0/0|21/20/19|313|107|27|71|242/242|243/243|
|AfterDllGc|16|0/0|2/2/2|22/21/20|281|91|11|71|210/210|211/211|

三heap JSON已完整亲读。三末点类型记录原行11–13的header与首记录亲读，超4000字符后工具截断，不声称Thread逐entry全文已核。均Partial，未知71，类型不等于owner，不能把所有32handles精确归属对象；DLLGC前后Thread -16、Event -16及短弱引用全归零，有限支持该16轮对象可回收、host与NativeAOT DLL不同堆的延迟回收。没有关闭私有Thread句柄。不得推广为长期无泄漏/生产已修/正式memoryGreen。

原16轮65sample/role不变。comparison完整53行：O13232/C13212各16completedRounds、未执行空、error空、exit0，PassedKnownOwnershipAndCallbacksOnly。末sample O/C第65行完整亲读，各callbacks entries/exits/disposals16、known CallerOwners/Channels/Duplicates0、ConnectionOwned/ManagerInitialized=false、WicDisposed=true；原业务验证器逐轮硬门通过。O/C r15 business1–74身份及关键cleanup亲读，其余31轮尚未逐份人工全文回读，不冒称已完成该项。r15均源关闭前后0，dispose1/callbackExited1，destroy2、release[2,1]、disconnect/deinitialize true。完整原件留存。末sample PrivateBytes O25337856/C30019584；Handles O275/C316（GC前）；不把post-GC值替代自然趋势。

父监督40行、两role监督各20行、TRX26行完整亲读：父08:33:47.3452762→08:34:23.4320243（36.0867481s），exit0，无timeout/kill/drain失败；body35.4101158s，TRXtotal/executed/passed1/1/1、failed0。两role Exited/exit0/reaped=true/forcedTermination=false，cleanupErrors空，role360/parent840/round20/外900。

## 身份和静态根边界

.NET10.0.13 X64/pointer8。testSHA `0634FA07665CC63BFFE5DBC41E938F6818B31F4CF834DA497429309506C28F8D`，MVID `9144c63a-2b9a-4b53-959e-549a04e89e03`。diagnosticNative actual及三个heap SHA均 `F4A67D183B8BF9DDD176DA9BD94A3457E5BA197278E666CF89C00131B0C55B74`，actual=`task/Native/wpfgfx_cor3.dll`、requestedEqualsActual=true、handle=actualBaseAddress。official actual `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`、版本10,0,1326,50504；candidate初始化文件不当official加载证据。

Native诊断独立目录，默认生产属性false，整个导出/记录受WPFGFX_RUNTIME_HEAP_DIAGNOSTIC包围。Track只短弱ref，不存业务强ref；Observe用IsAlive不取Target，GC之前无观测目标局部变量跨界。构造Track帧已返回；DLL收集先于IsAlive，不因先读Target形成JIT根。本次无Native源码变化，F4A身份不转绑后续新源码。生产D3F5历史证据保持，诊断F4A仅candidate观测。

## 额度与限制

本接续用normalbuild0/tests0/Nativepublish0/COMpublish1（隐式编译1）/sandbox2；余normal3/tests3/Native2/COM2/sandbox5。包括前轮的全r1累计normal1/4、tests0/3、Native1/3、COM2/4、sandbox3/8。工具参数/原返回实时账见investigations/runtime-heap-owner-r1-continuation-ledger.md及artifacts/runtime-heap-owner-r1/continuation-publish-receipt.md。发布exit0，日志60行已回读，stdout前37353/57353字符不可见，warnings聚合/耗时Unknown。

资源维持MemoryDescriptiveOnly/memoryGreen=null/WorkerExitNotProven/完整替换Open；已关闭的是本次有限heap可回收性问题，而非所有资源门。Uce Create在Acquire后发布异常漏Release独立未修。下一在剩余额度优先x86默认生产event-owner构建绑定及受影响回归；不将诊断导出带入生产。
