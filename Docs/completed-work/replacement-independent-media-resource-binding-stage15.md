# Stage15 独立媒体资源桥

## R5最高当前：ManagedStrongGatePassed / NativeAotPublishPassed / OverallOpen
本接续亲自监督并完成r5既有运行链回读、最终新增puretests正常编译与定向运行、生产NativeAOT发布及文档闭环；无终端脚本/再委派，无生产尾改。以下R4终态为历史真实失败，不撤销。
- r5 task acb821e4057c426084da715467382be4 supervisor12860/child6736，filtered120秒/原外300秒，exit0；stdout准确TestResults/child.trx全26行 total1/executed1/passed1/failed0。无timeout/cancel/kill/drain故障；不是只认bridge头passed。
- bridge308行全读：40帧/DrawVideo红蓝；resource2→32，解绑32→32；listener11→11；同identity missing80070002且missingOwnerThreadExited=true；真实退订262.2815ms内11→11。43包末Failed=0800000002000780000000000000000000000000。lease跨Close/missing/最后Release仍红0000FFFF0000FFFF且Disposed，final0/registryRemoved/descriptorDisposals1；reader.Release0/MFShutdown0/CoUninitialize。顶层ThreadExited来自旧视频owner，missing退出有独立字段强断言。底层native/lease refs仍Unknown。
- controls真实480行，亲读1–95/90–190/430–480：Pause六帧位置5000000冻结、Stop/Seek25000000/rate2拒绝/Resume/Ended、21帧epoch2/Close0/refs0/registryRemoved/descriptorDisposed。scrub真实302行，亲读1–95/90–150/250–302：红蓝、禁用seek不发布、Ended callback Close、12帧epoch3/refs0/registryRemoved/descriptorDisposed。没有声称全读二者中间逐帧列表。
- wrapper SHA1B2E826558AAC8B93242D1D700B554850CFE87751E5B65959F7E9F2C3C642D23 MVID5faca150-a182-4209-a29b-0baf27031141；业务Tests SHA D7E2F9B4DFCDDC66BE5B0D5C8F3A48240A67E3175E32349970BB779C298D81E0 MVID2bcfc00a-f69e-4968-b30a-3ac079eed8c6，bridge/controls/scrub一致。NET10.0.13/X64/8，ManagedAcceptance/Tests/wrapper实际来自task，四MF模块System32。F156旧fixture identity仅初始化，不是新Native媒体执行。
- ObserveOpenAsync131–190完整复核：捕获owner先退订，锁外await Close，失败通知重新检查ReferenceEquals和closing；每次Open必new owner且Close完成才Closed，不需另加同owner epoch；回调Close/Release不selfjoin。cleanup Aggregate仅令Completion保留双错误，不替换Failure/Failed包原HResult，公开Open仍0异步合同。未发现确定新逻辑缺陷，不盲改。不是cleanup fault注入测试已覆盖的宣称。
- 最终normal绝对Tests Release/win-x64/net10.0 exit0，3883warnings/0errors；唯一剩tests实际OR六类（OpenFailure/ResourceBinding/Identity/MediaCreationUnavailable/GeneratedMediaPlayerResource/Lifecycle）50passed/0failed/0skipped/37ms。真实provider类不是不存在的MediaProviderTests；旧37/40不推算不累加。新增failurecompletion/reopen仅空白路径不调用MF；最终pureTests版本不是r5systemassembly，生产源码未变因此沿r5actual系统证据，不造新系统身份。
- sourcefreeze后生产Native发布实际exit0：默认NativeAOT Shared，Release/win-x64/net10.0/selfcontained/runtime10.0.13/两诊断false。独立artifacts/independent-media-stage15-final-native-x64/wpfgfx_cor3.dll及.pdb已核；日志明确Generating native code/生成lib exp；未运行或测hash，不伪称新Native媒体桥通过。准确参数、return、截断见即时账。
- R5累计normal2/3 tests2/2 COM1/2 sandbox1/2 Native1/1；最终余额normal1/tests0/COM1/sandbox1/Native0，未用额度不凑量。implicitDebug2/COMcompile1/Nativecompile1独列；本接续只normal1/tests1/Native1，COM0/sandbox0。正式细账 .coding-agent/stage15-r5-continuation-receipts.md。
下一直接任务：公开factory必需接口与完整controls/events/真实audio volume balance/AVsync/取消生命周期合同接入，并对本次新Native产物建立SHA/实际模块来源及真实Native桥WPF验收；未完成前factory继续E_NOTIMPL/null。原版loader/副本/委托/fallback永久禁止，合法provider保留。audio/AVsync/MF强cancel/Native双架构/WPF/full仍Open，旧full红/endpoint0/AVI gap16/resourcesUnknown/MemoryDescriptiveOnly/memoryGreen=null不冲销。


状态（R4正式收尾）：SourceRecovered / R4StrongGateFailed / EvidenceCloseoutCompleted / OverallOpen。执行与文档收尾完成，不是强门通过或产品完成；以下BlockedRestore为历史，不再代表当前。

## R4真实强验收终态
- task `e4f827ac95964dc9b584700a96736b0a`，整任务回收 `WpfGfxShape/artifacts/independent-media-stage15-r4/readback`。监督12748/child8836，filtered120秒/外300秒；child exit2，TRX43行 total1/executed1/passed0/failed1（组合FQN，不拆算子case），无timeout/cancel/kill/收集故障，stderr空。原参数/完整工具return即时保存 `.coding-agent/stage15-r4-sandbox-receipt.md`。
- 真失败：`IndependentMediaResourceBindingSystemTests.cs:105`，同identity missing-file重开完成后 `identity.Owner!.ThreadExited` 为false。源码146–153的ObserveOpenAsync catch仅解绑owner事件并OnFailed，没有CloseAsync/等待owner退出；owner Run在命令队列等待直至Close。这是下一直接生产修正点，不能删强断言、加无合同延时或宣称已修复。本次没有修改源码、追加编译或预算外重跑。
- 到达并通过的前序源码强断言：40帧、samples2、resourceChanges非零增长、正常播放listener退订后计数不增、3秒资源解绑后不增、首次Close旧owner退出、Close后Acquire拒绝、同identity随机missing Open返回0且Failure=80070002。原件bridge.json passed=false、comPackets43、resourceChanges2→32、listener11→11、资源解绑32→32；DrawVideo红0000FFFF0000FFFF/蓝FF0000FFFF0000FF。
- 真实finally：0.5秒借得的lease跨首次Close、missing失败、最终Close和identity最后Release后仍MILCopyPixels红0000FFFF0000FFFF，像素强断言已执行；leaseDisposed=true。记录final0、registryRemoved=true、descriptorDisposals1、旧视频owner Closed/ThreadExited=true、reader.Release0/MFShutdown0/CoUninitialize。bridge顶层ThreadExited来自局部旧视频owner，不是失败时missing owner；finally关闭missing owner并等待Completion。最后finally后的三个Assert因异常不会执行，不能称这些Assert Passed；数值清理由JSON证实。lease底层最后ref未输出，仍Unknown，buffer.Release1不冒充0。
- 未覆盖：missing ThreadExited强断言失败后的退订采样/250ms等待未执行；JSON missingAtUnsubscribe=0、missingAfterUnsubscribe=0为默认值，missingObservationMilliseconds=250为硬编码，不能作窗口通过。comPackets43只有总数，无Failed packet字节/同identity字段，真实80070002已证但Failed packet精确证据未记录，不补造。当前组合在桥失败终止，controls480/scrub302未执行；只能保留r3既有回归，不能冒称r4通过。

## 身份与模块逐项核对
监督wrapper SHA256 `9BA2EDAD337C92705F1D4327175527AA7E5832BE041D995DC43A7FF62135C118` / MVID `04384001-d308-44b2-9f6a-7260434eb6a3`；业务Tests SHA256 `920757C2D56C6CF244E1B1CF0778BCBC524A21886E8599C303C56412167C322F` / MVID `54b08e18-9f31-4fda-baa8-90a39d04a238`。runtime .NET10.0.13/X64/pointer8。实际模块WpfGfxShape.ComAcceptance、Tests、ManagedAcceptance均来自该task；mfplat/mfreadwrite/mfcore/mfsrcsnk来自Windows System32。identity请求Native/wpfgfx_cor3.dll SHA F156DB69EFF6128768E4280070F65D3C40824D51135BA05E63052B2355D8F86A；该fixture仅初始化PEhash，不是新NativeAOT/source-link Native桥认证，业务模块列表不列Native实际模块，不能以文件存在补加载结论。

## r3保留与执行总账
r3 task2be893398d6c4da0ab5eb762e0662df9唯一组合TRX1passed保持，不替代r4。controls480行尾核实21帧/epoch2/Close0/refs0/registryRemoved/descriptorDisposed；scrub302行尾核实passed=true/epoch3/12帧/refs0/registryRemoved/descriptorDisposed，二者业务身份06AD24C8…/dbb05f44…，不是r4身份。原账pause6帧冻结等保留不增算。
本轮实际normal2/3、targetedTests1/2=40purepassed、COMpublish2/2、sandbox2/2、Native0/0；未用normal1/tests1封存不用，publish和sandbox余额0。Debug隐式编译1/publish隐式编译2另列。历史Stage15 normal2/tests0/COM2/sandbox0/Native0保留；合计normal4/tests1/COM4/sandbox2/Native0，本接续新增仅sandbox1。r4publish准确命令/exit0/原return缺口与stdout截断账见ledger，不伪造raw；旧NU1101/生成器失败原样保留。
亲读本次TRX1–43、监督1–40、stdout1–20/空stderr、identity1、bridge1–260（超长分段补尾）、diag1–82（超长61–65补读）；源码新强断言77–156与ObserveOpenAsync/owner Close/Run核对。r3 controls/scrub关键及尾重核，不声称本接续重读其全部480/302。

## 下一直接生产接入工作与真实Open门
下次须另授权有界执行，直接在IndependentMediaIdentity.ObserveOpenAsync失败路径完成当前owner的受控Close与等待退出，明确失败通知/Completion/最后Release顺序，保留Failed=80070002及原失败原件；再验同identity missing失败退订250ms、Failedpacket和r4组合controls/scrub。不是泛选门，也不在本轮无验证地尾改源码。随后才推进公开factory完整合同接入与当前源码Native/WPF验收；当前公开factory仍E_NOTIMPL/null，合法provider保留，禁止原wpfgfx loader/副本/委托/fallback。真实audio、AVsync、MF强cancel、Native双架构、真实WPF/full均Open。旧full红、endpoint0、AVI gap16、natural resourcesUnknown、MemoryDescriptiveOnly/memoryGreen=null不冲销。

官方源恢复依据与最小修改见.coding-agent/stage15-source-recovery-execution.md；本轮normalbuild2 exit0、40purepassed、COMpublish2 exit0，r4强断言已编译。r3 task2be893398d6c4da0ab5eb762e0662df9单组合TRX1passed，资源/controls/scrub已证但不替代r4。r4唯一沙箱已实跑失败并封存，执行参数、发布命令与截断账见stage15-execution-ledger.md最高节。没有新的NativeAOT，fixture F156只初始化；公开factory E_NOTIMPL/null及禁止原loader/委托/fallback不动。

## 历史接续终态与来源核查（源恢复前，非R4当前）
已首先补齐第二publish实际账（20行全读，exit1/NU1101，非编译/ABI/业务失败）。新增绝对Tests Release/win-x64/net10.0 normalbuild一次，17行日志全读exit1，同样缺既有Silk.NET.Direct3D11/Direct3D9/DXGI；没有系统编译成功或测试运行。两个配置缓存目录无Silk包，不使用no-restore遮缺包、不改源追绿。
COM295 alias Tests ProjectReference在Stage3/Stage13已使用，不是可证实的Stage15误引；Tests17引用生产，生产35–37三个2.23.0包由现有Direct3D源码实际依赖。COM显式链接三个单文件，没有复制整个Tests的Compile glob；无Directory.Packages文件。未找到应回撤的本轮项目条件改动，不能凭猜测裁剪项目。
CsWin32当前生成props引用不存在的build/netstandard1.0路径，真实0.3.298缓存props在build目录且含NativeMethods AdditionalFiles；还原产物不一致已定位，但首次publish完整原件本接续未获得，首次生成失败根因仍未最终认证。AssemblyName切换/NoIncremental不是已证实修复，不手改生成文件。
本次未改生产/测试/csproj/库/源/SDK/TFM/native名称/default属性，未恢复loader；safeowner仅局部静态审查不等通过。Create失败finally Release与AttachEvents临时proxy归还、registry查找、双资源传入ref、Action订阅/锁外调用已读；在途并发/Dispose/失败QI/真实lease等尚缺动态证据。
本次实际normal1/3、tests0/2、COMpublish0/2、sandbox0/2、Native0/1；Stage15累计normal2/tests0/COMpublish2/sandbox0/Native0。无新SHA/MVID/task/TRX/count，不挪用Stage14身份当本桥证据。明细与参数/原return/exit见同目录stage15-execution-ledger.md。
下一先恢复既有依赖合法还原，再编译、pure精确回归、真实AVI桥资源闭环；托管闭环后才当前源码NativeAOT。公开factory仍E_NOTIMPL/null，内部Create源码存在不等于生产可用。

## 预注册与 before / after
Before：SystemMediaSource.Create 拒绝、Resolve 恒 null、订阅抛异常；资源识别 identity 后直接要求 Owner 非空并 BindIndependentPlayer，不能在 Open 前绑定，也没有 identity epoch 的资源订阅。
目标 After：内部 Create 校验参数并 QI/保留真实事件代理，失败回滚 identity；Resolve 只查自有 registry，不读 opaque 地址；资源 transport 接受自有 identity，订阅 identity 级通知并按当前 owner/状态取 lease。外部合法 provider QI 路径保留。公开 MediaExports.CreateMediaPlayer 另置拒绝门，opaque 输入不触碰。

## owner / 引用 / 退订合同
|对象|所有权与边界|
|---|---|
|identity|Create 返回一个 COM 引用；transport 消耗调用方提供的一个引用，资源解绑 Release；两资源各需独立引用|
|event proxy|Create 经 AttachEvents 一次 QI；不能每次订阅重挂 COM 事件；最后 Release/Shutdown 释放注册|
|resource subscription|托管订阅项；通知在锁外；退订阻止新 admission，已 admitted 回调允许完成；资源 listener token 再检绑定，禁止旧绑定污染|
|frame lease|在 identity 锁内取得当前 owner 的 lease，Close 后拒绝新借用；已借 lease 独立释放，不把 identity refs 当底层全零|
|source epoch|订阅不缓存 owner/frames，Close/失败不再提供旧帧，重新 Open 读取新 owner|

回撤：若桥失败，保持公开 factory gate；回撤内部桥及资源 transport 分支，不恢复 original module loader/委托/fallback，不添加秘密 export。

## 历史执行预注册（原额度，R4当前Native额度0以最高节为准）
授权上限 normalbuild3 / targetedTests2 / COMpublish2 / sandbox2 / Nativepublish1。实际初始全部0。publish 隐式编译独列。只使用实际工具，无终端/脚本/再委派；程序仅 Windows 沙箱。系统 MF 不在主机测试执行。
验证：先 Release/net10.0 绝对 Tests 目标构建；pure 测试实际函数指针覆盖 Create/Resolve/订阅/引用/失败回滚及原四 negative。系统经既有 COM supervisor、固定 AVI、真实 COM proxy、新 transport 桥、MF、DrawVideo 像素与退订窗口；其余 controls/scrub 生命周期按既有实际用例回归。不以空回调集合通过作为证据。
COM 发布 PublishAot=false/WpfGfxShapeManagedAcceptance=true、ComAcceptanceSandbox=true、selfcontained NETCore/Desktop10.0.13；fixture 显式 stage13-final-native-x64 F156DB69EFF6128768E4280070F65D3C40824D51135BA05E63052B2355D8F86A，仅 fixture 身份，不认证新 Native 媒体。publish 与 clean 兄弟路径。Native 仅最终托管闭环后。

## 历史当前证据（源恢复前，原失败保留）
此前normalbuild成功3858warnings/0errors仅覆盖当时源码；之后新增系统用例未编译成功。两次COMpublish失败（首次Windows namespace、第二次NU1101），本接续normalbuild还原失败。未运行测试或沙箱，无新count、SHA、MVID或Passed。
stage14 首次 ref expectation failed、旧 full 红、endpoint0、AVI gap16、resourcesUnknown、MemoryDescriptiveOnly/memoryGreen=null 保留。公开 factory/audio/AVsync/强 cancel/真实 WPF/Native 双架构仍 Open。
