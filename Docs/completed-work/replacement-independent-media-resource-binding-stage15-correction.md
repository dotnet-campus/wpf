# Stage15-r5 失败路径修正正式闭环

状态：ManagedStrongGatePassed / NativeAotPublishPassed / EvidenceCloseoutCompleted / OverallOpen。不是产品全完成。R4 task e4f827ac95964dc9b584700a96736b0a 的missing owner未退出强断言失败永久保留。

## Before / After
Before：ObserveOpenAsync失败只解绑并OnFailed，missing owner仍等待命令，没有Close/退出。
After（既有r5实现，本接续未盲改）：捕获owner解绑；锁外await owner.CloseAsync；重新校验ReferenceEquals(_owner,owner)及!closing后设置Failure/Failed并用捕获proxy通知。每次BeginOpen必new owner，Close完成才Closed可重开，因此owner引用即Open归属，不缺同owner重用epoch。WaitForExitAsync等待异步closed后Join；Close/Release返回异步完成，不在callback自身同步join，不在identity锁内等待Close完成。
cleanup异常Aggregate保留原error及closeError于Completion，Failure和Failed包仍使用原error.HResult；不将Aggregate HResult映射到原失败公开合同。正常Open仍返回0/异步Failure合同。没有新增cleanup故障注入验证，不扩大覆盖。

## 实际验证
r5 task acb821e4057c426084da715467382be4：监督exit0，stdout准确TestResults/child.trx 1–26全读 total1/executed1/passed1/failed0；bridge1–308分段全读，missingOwnerThreadExited=true、Failure80070002、Failedpacket0800000002000780000000000000000000000000、退订11→11真实262.2815ms。lease跨Close/失败/lastRelease红像素并Dispose，final0/registryRemoved/descriptor1；reader.Release0/MFShutdown0/COMuninit。controls480与scrub302关键及实际尾已核，业务身份一致；底层native/lease refs仍Unknown。
本接续新增failurecompletion/reopen已实际normal绝对Tests Release win-x64 net10.0 exit0，实际六类OR 50passed/0failed/0skipped。含合法provider GeneratedMediaPlayerResourceTests与factory negative；不是不存在MediaProviderTests零命中，非37/40推测计数。新增两测试仅空白路径不调用WindowsMF。生产未变，system沿r5真实assembly，新增pureTests非同systemassembly；无必要COM/sandbox复跑。
随后唯一生产NativeAOT实际exit0，默认Shared/Release/win-x64/net10.0/runtime10.0.13/selfcontained/两诊断false，独立artifacts/independent-media-stage15-final-native-x64 DLL/PDB已核。未执行新Native或认证其SHA；F156只是旧fixture初始化。

## 预算与后续
R5累计normal2/3 tests2/2 COM1/2 sandbox1/2 Native1/1，余额normal1/tests0/COM1/sandbox1/Native0；implicitDebug2/COMcompile1/Nativecompile1。本接续normal1/tests1/Native1，COM0/sandbox0，无终端脚本/再委派。准确参数/原return/截断/读取范围见.coding-agent/stage15-r5-continuation-receipts.md，正式报告与ledger同步。
下一直接公开factory必需接口、真实audio volume/balance/AVsync/强cancel完整合同与本次Native产物实际桥WPF验收。factory仍E_NOTIMPL/null，合法provider保留；原loader/副本/委托/fallback永久禁止。audio/AVsync/Native双架构/WPF/full及旧full红/endpoint0/AVI gap16/resourcesUnknown/memoryGreen=null保持Open，不把有限闭环作产品放行。
