# Stage14 新 NativeAOT 实际 ABI 验收

状态：LimitedNativeAbiProtectionValidated / CloseoutCompleted（仅本轮证据工作）/ OverallOpen。
权威执行账：Docs/completed-work/stage14-execution-ledger.md。主代理亲自执行与回读，无终端脚本、再委派、新runner或额外process；只使用既有exe监督入口。未修改生产代码，第二publish后未改测试/生产源码。

## 实际执行与身份
第二沙箱task806b4189fce0413fabb8be3b9d41d741；整任务回收WpfGfxShape/artifacts/independent-media-stage14-r2/readback。内部Results/stage14-native-r2；supervisor12980/child3632。2026-11-01T02:11:24.0993143Z至02:11:25.0920437Z，filtered内限120秒、外限300秒，非full；无timeout/cancel/kill/drain故障，child exit0。工具原return为ForegroundTopology及结果已保存路径，无独立数值exit字段，不能混同child exit。
预注册三个精确FQN（均namespace WpfGfxShape.ComAcceptance，公开无参TestMethod）：
- IndependentNativeAbiTests.WhenStage14ProductionNativeIsLoadedThenFactoryGateAndEventAbiAreReal
- UceChannelTests.WhenFailedPartitionAcceptsLateSharedChannelThenSourceClosePreservesFailure
- UceChannelTests.WhenAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives
合法FullyQualifiedName=精确FQN以|连接，完整参数已在ledger。expected3 / TRX total3 executed3 passed3 failed0 error0 timeout0 aborted0 notExecuted0。stdout693ms；三方法TRX耗时123.0766/74.6089/101.9336ms（对应上列顺序）。不是全量1841测试通过；1841只是discover数量。
唯一Native输入为WpfGfxShape/artifacts/independent-media-stage13-final-native-x64/wpfgfx_cor3.dll，来自既有stage13-r2默认生产NativeAOT；本轮Nativepublish0。aot.sha256及真实loaded module均F156DB69EFF6128768E4280070F65D3C40824D51135BA05E63052B2355D8F86A。
requested=actual=C:\CodingAgentSandbox\Tasks\806b4189fce0413fabb8be3b9d41d741\Native\wpfgfx_cor3.dll；PE Amd64，runtime10.0.13 / X64 / pointer8；module base140720497033216。两channel业务也记录同一actual/hash/base，pid3632，同进程candidate证据，无source-linked业务替身。
测试DLL SHA256 EB75B5DB948668071E16C309A4D1A6BB618FDAE744498AEB7A87081166295ED9，MVID5f884cbe-0e05-437b-91fb-e68ab610086a；exe SHA256 B31DBA03701128368AFA83F8D94E1259912980AAB50080CDB0474FDF6CC0EA35。businessSourceLinkUsed=false。
ABI前进程wpfgfx名称匹配列表仅本任务exe与测试DLL；后列表仅新增Native/wpfgfx_cor3.dll（完整路径/hash/base见business153行）。未见root official wpfgfx或loader；不把仅文件存在视为加载，不把未来预存root模块默认归因于生产回载。PostFailureAttach.loadedWpf=[]是其特定筛选字段，不能抵消ABI模块实证。

## 强门实测
|门|实际结果与边界|
|---|---|
|真实factory创建安全门|MILCreateFactory S_OK/非零1778247368464；MILFactoryCreateMediaPlayer canOpen0及1均0x80004001 E_NOTIMPL/playernull；proxy前后AddRef2/Release1。nullOutput E_POINTER80004003，nullFactory E_INVALIDARG80070057/outputnull，opaque client1 E_NOTIMPL/null且未触碰。不是媒体已支持。|
|真实event COM|MILCreateEventProxy S_OK/1778248944064；IMILEventProxy IID342efd8b-669a-4d16-b163-d75f5ffd1a10与IUnknown均QI0/同一地址，每次配对Release1。|
|packet/HRESULT|callback2次；packetHex0800000002000780000000000000000000000000，20bytes保持，Failed event8/packet HRESULT80070002；两次RaiseEvent均透传callback HRESULT80004005。|
|RaiseEvent在途ref|creator1+extraOwner1+inflight1=3，callback释放extraOwner返回2；dispatch后AddRef2/Release1，最终proxy.Release0、factory.Release0、descriptorDisposals1。23条ABI操作；不是callback最终Release/媒体内部refs全0认证。|
|生产导出|exports.json完整650行：106个Contracts/wpfgfx.def导出全部found=true/address非零；存在不等于每个业务已实现。|
|诊断导出缺席|准确生产符号WpfGfxShape_RuntimeHeapDiagnostic_v1、WpfGfxShape_NativeAotAbiProbe_v1均found=false/address0/expected=false；已核RuntimeHeapDiagnostic.cs17与NativeAotAbiProbe.cs21条件编译定义。不是猜测符号，也不泛称所有可能诊断。|
|postfailure late attach|completed/cleaned=true；A6失败88980406后Create.L(source6)=S_OK/L7非零；L首次/源A销毁后均88980406；顺序16<17<19<20。A/L两次Destroy0、Disconnect0/Deinitialize0；created=destroyed=[6,7]，stillKnown=[]、unknownRegistration=false、primarynull/cleanupErrors[]。23条调用。|
|partition isolation|A2/S3同分区均88980406；独立B4在关闭前后新的alpha update Send/CloseBatch/Commit/Flush均0，alpha0.5→0.25；三channel destroy0，Disconnect0/Deinitialize0；created=destroyed=[2,3,4]、primarynull/cleanupErrors[]。42条调用。|
|负向断言控制|late attach的5个复制结果拒绝及isolation的2个复制结果拒绝均true；仅断言自检，不是生产故障注入或额外TRX。|

## 首次失败与发布预算保留
首次task0953e6a51f8442b2a31e2954b7a0ece1真实child2/TRX1failed保留；原因测试漏算production RaiseEvent AddRef/finally Release，在callback期望1而真实2。按源码EventProxy.cs145–166修测试期望2并加dispatch后2/1验证，未修改生产追绿。首次cleanup proxy0/factory0/descriptor1；旧wrapper F7568B4D3038AFB1CA23903142285597FC9D98E913762E6FA30310EA10A28A94/MVIDd3b127b8-08e3-4e34-a7bb-e3afcf8b5c3c，不混同最终wrapper。
最终实际normalCOMbuild0 / targetedTests0 / COMpublish2 / sandbox2 / Nativepublish0 / implicitcompile2。上限2/1/2/2/0，不再使用未消费normal/tests额度，不额外publish/Native/沙箱。累计沙箱TRX4 executed=首1failed+末3passed，不抹去首失败，不称4passed。
第二publish日志1–60已亲读exit0，source=independent-media-stage14-r2/clean；stdout63246字符仅最后20000可见，前43246不可见，aggregate warnings Unknown。原工具逐字return不可恢复Unknown，以实际日志命令/exit作为发布证据；第一publish stdout677816/前657816不可见单列。两次隐式编译不算独立normalbuild。

## 证据完整回读
最终readback：TestResults/child.trx38行、supervision40、stdout12（中文解码有替代字符，数值与TRX交叉核）、stderr空、aot-identity1、ABI identity14/business153/exports650、PostFailureAttach business267、PartitionFailureIsolation business402、Diagnostic/log_261101021124197.diag87行均分段实读至尾；截断范围补读。首次exports650及diag82也完成尾读，原首次business/TRX/监督读数保留ledger。binary未作文本泛扫。
所有当前交接入口已同步本Stage14最高节；历史阶段保留为历史，不覆盖真实失败/额度。

## 真正剩余生产任务与下一可执行步骤
完整替换仍OverallOpen，未创建真实公开MediaPlayer、未执行原mediafactorypositive，不能从E_NOTIMPL安全门推导媒体播放支持。
已亲读SystemMediaSource.cs：Create仍output0/E_NOTIMPL；Resolve恒null；Subscribe/Unsubscribe抛NotSupportedException。MediaExports.CreateMediaPlayer仍调用此Create。IndependentMediaIdentity已有内部Create/Resolve与AttachEvents，但未接入此资源绑定链路，不能重复声称这些底层方法不存在，也不能把它们已有等同公开factory可用。
下一会话先设计并实施明确的internalCreate/Resolve/Subscribe/Unsubscribe桥接，把既有identity绑定真实独立player/owner与资源通知；使用已有生命周期/事件ref机制，失败回滚/退订/最后Release/Shutdown/Exit断环与cleanup错误均有合同。保持公开Create的E_NOTIMPL/null gate，内部桥接不自动开启公开factory，不新增生产测试export、不恢复原loader/委托/fallback；合法外部provider边界保持。
公开factory安全启用时点：上述真实独立创建/资源解析/订阅链路完备，且控制、错误、生命周期、音频/AV同步、取消等所需公开合同经真实Native与WPF消费者验证之后，显式决策放行；不能凭Stage14或静态布尔gate声称已可启用。
Volume/Balance真实audio应用与同步MF强cancel仍缺，不能默认返回S_OK或仅存变量假实现；需明确unsupported HRESULT或先实现真实backend能力。继续补独立Exit→最后Release、cleanup fault、MP4/音轨/AVsync、x86/x64及WPF/full回归。
继承未解决：audio endpoint0、AVI音轨gap16、旧full红、nativeRefs/buffer.Release1/lease底层最终refs Unknown、resourcesUnknown、MemoryDescriptiveOnly/memoryGreen=null。Stage14不改写为通过。
