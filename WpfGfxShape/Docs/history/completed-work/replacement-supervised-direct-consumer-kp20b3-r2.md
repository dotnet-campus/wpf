# KP20B3 direct consumer 修正与有界真实验收

## 结果与停止点
本次授权亲自实施，未委派、未终端/脚本。正常 build1 成功、COM publish1 成功、official 沙箱1真实体1失败，立即停止；candidate未请求，未修后重跑。此前direct build1失败保留，累计direct独立build2（1败1成）/COM publish1/沙箱1/official请求1/candidate请求0/真实体1失败。Native publish、单独发现、x64、full、resize均0。

## 独有源码修正与静态审计
仅修改 `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.DirectConsumer.cs`；本次项目/资源/生产/WpfGfxShape.Tests/入口/公共helper/Package.targets/TFM/SDK均0改。
- 进入资源阶段前验证并保存 PresentationWindowThread 与 ownerId；创建前核对共享owner引用，创建在保存owner上执行，验证 HWND owner与当前进程；销毁使用保存owner，实际销毁先执行，再核对原owner/实际owner/当前线程，避免单个断言阻断销毁。
- finally无显式throw表达式。detach、各resource release、close/commit/flush/destroy、disconnect/deinitialize、四项COM Release、callback记录、HWND销毁均逐项Cleanup尝试。捕获AssertFailedException/SystemException，保留主异常ExceptionDispatchInfo；finally外写汇总，将后续cleanup错误附主异常Data并原堆栈重抛，无主异常才抛AggregateException。灾难性native进程终止不承诺可恢复；不是故障注入全覆盖。
- 窗口DC释放同样纳入cleanup，不覆盖主像素错误。native输出变量的清理闭包使用值快照。
- 已读native apifunc.cpp SendCommand、clientchannel.cpp SendCommand和d3dimage.cpp ProcessPresent：separate send有Begin/End/CloseBatch多个失败点，失败HR不能证明未写入。ProcessPresent负责SetEvent/CloseHandle。duplicate在send尝试后不猜测主动CloseHandle；失败所有权记unknown，未决资源仅由隔离进程退出回收。本次send/commit成功且event到达；未实测失败注入，不宣称失败duplicate已逐柄认证。
- 两公开方法仍共用同一序列：无fresh-native前门；HD合法async connect(0)、packed92/flags12/DPI、separate PresentPacket16、duplicate event/commit/5秒等待/flush。严格红FFFF0000→蓝FF0000FF→绿FF00FF00，COLORREF255/16711680/65280，每色DwmFlush后仅一次window DC GetPixel(16,8)，首次失败即停。未加skip、容差、颜色调节、producer读回或backend断言。
- 共享PresentationWindowThread的ready.Wait、Invoke completed.Wait、Dispose Join均无局部截止；未泛改helper。本次沿既有监督120秒/外限180秒，实际无超时。

## build与publish
唯一正常build：ConsumerDirectDiagnostic/win-x86/net10.0-windows，Exit0，134 warnings/0 errors，2.74秒。日志64行已回读；stdout105693字符，前85693未显示，不称全部warning已审。正常分析器、无抑制。构建后无源码尾改。

唯一正式publish：同项目/配置/架构/TFM，self-contained true，ComAcceptanceSandbox=true，AotRuntimeIdentifier=win-x86；显式AotDllPath为 `d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/kp-resize-pixels/com-acceptance-package/Native/wpfgfx_cor3.dll`。新独立 `artifacts/direct-consumer-kp20b3-r2/publish/` 与 `com-acceptance-package/`，发布前根不存在。Exit0；发布日志60行，stdout53203字符前33203未显示，不虚构warnings总数；内部正常build不重复计独立build。

clean核验：正式aot.sha256=20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D；runtimeconfig两框架10.0.13，deps runtimeTarget win-x86且两runtimepack10.0.13。manifest621行，亲读首15、390–430、614–621，根官方wpfgfx_cor3.dll和Native/wpfgfx_cor3.dll分离，入口、测试程序集、Contracts、MTP依赖存在；递归枚举上限650未提示截断。未人工逐条重算全包hash；正式打包目标未修改。实际初始化进一步检验候选PE/架构与摘要，不将该初始化摘要冒充candidate加载证据。仅上传clean，不上传publish根。

## 实际沙箱、TRX与身份分层
任务 `c7993216b896464389746e0275b2229b`。完整回收 `artifacts/direct-consumer-kp20b3-r2/readback/official/`。
入口WpfGfxShape.ComAcceptance.exe，参数 `--supervise-exit Results/direct-official-r2 120 FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenDirectOfficialConsumerReceivesThreeColorsThenWindowDcMatches`，外限180秒；无额外发现。

监督 `Results/direct-official-r2/supervision.json`：filtered，Pid2224/Supervisor7112，exited，Exit2，TimedOut/Cancelled/KillAttempted均false，OutputCollected=true，无SupervisorError。02:00:15.2953203→02:00:17.3844907 UTC。stdout31行含编码替代字符，stderr空；裁决以UTF-8 TRX/JSON为准。

TRX `Results/direct-official-r2/TestResults/child.trx`：唯一真实UnitTestResult，total/executed/failed=1/1/1，passed/notExecuted=0/0。UID/testId `1c362925-9d12-8b9c-a7bd-4ddbf15cb32b`，executionId `f9c15e74-e85f-4e19-a113-3bc952d65039`，runId `2b0d276b-be79-493f-96d4-721e4c2a7099`。类/方法与精确FQN一致，duration1.2595187秒，Failed主异常为首红像素断言；非cleanup错误替换。

程序集实际身份：sandbox任务根 `WpfGfxShape.ComAcceptance.dll`；SHA256 `3EB8DEB93C0BECE79BE24C2882C6E8256D0D9FC638FB401FE6CB74A697F269DA`，MVID `57b2cf99-7ae6-4161-9b76-819de491536b`；.NET10.0.13/X86/PointerSize4，监督与业务identity一致。

实际加载官方：Requested==Actual=`C:\CodingAgentSandbox\Tasks\c7993216b896464389746e0275b2229b\wpfgfx_cor3.dll`；SHA256 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`；version10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。PresentationCore同版commit，SHA `D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D`。backend unknown。未增加独立受信资产证明，不用历史身份替本次加载。

候选仅 `Results/aot-identity.json` 初始化：实际读取任务根Native/wpfgfx_cor3.dll，SHA完整20B3，Sandbox=true/X86/PointerSize4；不是candidate已加载/已执行。

## 内容/HR/event/cleanup回读
完整direct-consumer.jsonl51行已读；stages尾段与监督、TRX一致。
- producer device/surface、InteropDeviceBitmap创建、partition初始化、async connection/channel、四consumer资源、image/draw/tree、setup close/commit/flush均HR0。target92/Present16/flags12，DPI context-2/scale1，owner5992，HWND22021110，32×16。
- frame0红：fill/dirty HR0；duplicate1820；separate send/commit HR0；event Signalled=true；consumer flush/DwmFlush HR0；window DC实际1973532，预期255，Accepted=false，立即失败。
- frame1蓝：未执行，无HR/event/内容；frame2绿：未执行，无HR/event/内容。禁止补齐为3次通过。
- detach完成，handles4/3/2/1依次Deleted1且HR0；delete close/commit/flush、channel destroy、disconnect、partition deinitialize均HR0。bitmap/surface/device/factory各Refs0。
- callback记录两项Available1/Version7，不升级为全部Available语义认证。
- HWND销毁Owner/ActualOwner/Current均5992，Result1。cleanup complete Errors=[]，Primary保留原像素断言；class cleanup后进程真实正常退出（测试失败码2），无kill。

## 裁决与交接
官方首红严格window DC观察失败，非candidate失败；不推断唯一产品/环境根因，不追加诊断、重跑或candidate。当前20B3有限三色consumer仍Open，无Passed；窗口DC不认证screen/scanout/HWbackend，不能外推resize/full/完整替换。

WGC账本原样保留：build4（1败3成）/publish4（1败3成）/沙箱3（发现1真实2）/真实2失败；撤后default build1成功另列。KS仍build1/publish1/沙箱4/请求3/发现1/体1/A1/B1/native2。通用ReferenceCopyLocalPaths/FullPath去重/PackagePath/Copy与manifest的引入前快照缺口保留，不修改打包规则或历史包。

本轮到达真实official验收门并按首错停止，剩余candidate任务未消耗且不自行使用。后续仅在新授权下决定是否静态审查官方首红不匹配；不得把本报告作为继续执行授权。
