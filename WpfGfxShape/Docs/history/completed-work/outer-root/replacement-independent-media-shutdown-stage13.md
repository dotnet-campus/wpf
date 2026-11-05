# Stage13 Shutdown / ProcessExitHandler 分离

状态：LimitedShutdownExitCloseReopenValidated / CloseoutCompleted（有限slice）/ OverallOpen；本轮独立实测，不继承 stage12 Passed。

## stage13-r2 最终实测
本接续normal1 Release/win-x64/net10.0：3851 warnings/0 errors/19.40s；tests0（旧76pure对应修正前源码）。正式COMpublish1 Release/win-x64/net10.0-windows，selfcontained、NETCore+WindowsDesktop10.0.13，PublishAot=false/ManagedAcceptancetrue/Sandboxtrue；明确旧fixture Native副本存在。stage13-r2/publish与clean为兄弟，正常implicit compile，未no-build。
唯一task8d6b8b0a34a946b3957fdd78bebcf7f7，120s监督/300s外限；正式桥FQN=WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenStage13ShutdownExitAndReopenRunThenRealLifecycleCompletes。child0/TRX1passed0failed/TimedOutfalse，无kill、无输出drain超时。readback=WpfGfxShape/artifacts/independent-media-stage13-r2/readback；监督40/TRX26/stdout12/identity1/stderr空、diag50–65尾、业务JSON均实读。首次system失败完整保留，不修改预期追绿。
CloseReopen：2 Opened/2 NewFrame、refs0、registryRemoved/descriptorDisposed；两个周期真实事件。六个帧回调/外线程case各110行全文：callback-close/release/external-close/callback-shutdown/exit/external-shutdown，均accepted0且completion仍pending、callback退出后owner完成，6帧停止/250ms latePackets0/新lease拒绝80000013；已借lease关闭后像素0000FFFF0000FFFF并Dispose。opened-shutdown/opened-exit各53行全文：0帧但真实Opened callback-enter，阻塞期间接受请求，放行后owner完成；Exit瞬间owner仍运行。两missing各21行全文，Failed8/80070002，cleanup=null。
identity refs0/registryRemoved/descriptorDisposed/threadExited、readerFinalReferences0/MFShutdown0/CoUninitialize已证。六帧case sample/buffer releases各7；buffer.Release1、attributes.Release1、nativeReferences与lease底层最终refs Unknown，不能写全资源归零。callback进入/放行后返回/owner完成分别10s deadline；250ms仅late观察不充当握手。
系统强门通过后实际Nativepublish1：Release/win-x64/net10.0/selfcontainedtrue/runtime10.0.13/PublishAottrue/ManagedAcceptancefalse/heap诊断false/DiagnosticAbifalse，默认wpfgfx_cor3/Shared；成功回执61行，58 Generating native code/59 lib-exp/60产物目录。WpfGfxShape/artifacts/independent-media-stage13-final-native-x64/wpfgfx_cor3.dll与pdb实存。工具未公开数值exit，不杜撰；仅当前源码编译/产物来源认证，SHA/binary诊断导出缺席/aggregate warnings-errors Unknown，未实际Native媒体导出运行。

## 身份与有限安全审查
wrapper SHA=C16D92A1F53654574280FF701B2CF0FD64F082C2B138142805F1BB43FD21D2A9，MVID=add03c35-1483-4ee8-9dd6-f329ca09011e；source-linked业务SHA=5599CA20760B43506DCED932EFCF7E60A080D3B16527EC386518D45482C87CBF，MVID=2bb3f65e-07cc-4ebc-985b-c4e3cbae8a72；fixture SHA=6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403只初始化，不认证最新Native。runtime10.0.13/X64/pointer8。
Resolve提供托管强引用、self仅vtable存储；未证明合法持ref调用存在确定UAF，故不做尾段在途调用大重构。此为有限审查，不证明所有非法并发最后Release安全。cleanup异常全路径未注入验证，不作无异常保证。Exit只shutdown代理，不发MF停止；之后测试显式Shutdown才owner清理。Exit的禁止后续控制为替换安全策略，不将官方只代理Shutdown扩大为全部方法拒绝合同；Shutdown接受请求不等完成，也不保证同步MF强取消。
下一直接实现：以当前新Native产物作显式fixture的COM包身份/SHA与Native媒体ABI受控认证；先补独立Exit→最后Release清理及cleanup fault具体路径证据，审定终态方法合同，再设计同步MF阻塞强取消可行边界。公开factory/audio/AVsync/MP4/Native ABI双架构/WPF整体仍Open；不得先解除factory E_NOTIMPL/null或启用原loader/委托/fallback。

## 接续起始实际历史（用户交接，非本轮重测）
Release normal build：3851 warnings / 0 errors；定向 pure tests：76 passed，对应握手修正之前的源码。首次 system：1 failed，openedshutdown 在阻塞 callback 中等待回调返回后才发出的 Opened 信号，属于测试握手死锁，保留首次失败，不视作已证 product bug。callback-enter 等待修正已在工作树，尚未验证。
本轮起始剩额 normal1 / tests0 / COMpublish1 / sandbox1 / Native1。只在最终 system 强门通过后执行生产 NativeAOT；失败则 Native 实耗0。不新分支、不委派、不运行终端脚本。工具原回执与截断、task、退出码逐次记入 ledger；尚未执行的步骤不标绿。

## Before / After 与官方来源
Before：IndependentMediaIdentity table22=table23=table5，Close 永久关闭准入，不能代表完整 Shutdown/Exit。
官方 src/Microsoft.DotNet.Wpf/src/WpfGfx/core/av/WmpPlayer.cpp：Close 223–265 是停止卸载当前源请求；Shutdown 854–895 通过 WmpStateEngineProxy::CallMethod 异步调用引擎 Shutdown 断环，保留无反向环的引擎用于 dummy presenter；ProcessExitHandler 899–918 只调用 MediaEventProxy.Shutdown，不 Close、不等待系统媒体清理。
目标：Close 完成后可再次 Open；Shutdown 终态禁止控制/事件并异步等待 owner 与已准入 callback 安全完成；Exit 终态关闭代理准入，不启动/等待 MF 清理，之后最后 Release 才走 owner 停止。已借 lease 保持独立引用。回调内不 selfwait。外部引用释放不在 identity 锁内执行。

## 状态与 owner
|操作|源 owner|事件|后续 Open|
|---|---|---|---|
|Close|请求停止，Completion 等线程退出|取消队列，允许已准入返回|完成后允许|
|Shutdown|请求停止，Completion 等线程退出|终态退订/释放并 drain|ClosedError|
|ProcessExitHandler|不调用 MF，不等线程|shutdown + 退订，已准入合法返回|ClosedError|
|最后 Release|停止并延迟释放 identity storage|drain|registry removed|
同步 MFCreate/ReadSample/Seek 的强取消不保证；Exit 不是全 OS exit 资源测量。音频未触。

## 预算与回撤
新增授权 normalbuild<=3 / targeted tests<=2 / COMpublish<=2 / sandbox<=2 / Nativepublish<=1。按交接额度核算最终使用3/2/2/2/1，余额全部0；本接续实际1/0/1/1/1，implicit publish compile2独列，前序implicit Debug1另列，不能把隐式编译算新unit执行。失败保留真实证据，不删测试、不改旧 expected、不加公共测试 API/InternalsVisibleTo、不改 generated/SDK/TFM/依赖。factory 继续 E_NOTIMPL/null，原四 negative 保持。最终托管闭环冻结后才可正式当前 production NativeAOT x64 编译；编译不等媒体导出运行。

历史 stage12 Native 仅编译边界；旧 full 红/endpoint0/AVI gap16/naturalresources Unknown/MemoryDescriptiveOnly/memoryGreen=null 不冲销。公开 factory、音频 AVsync、强取消、新 Native ABI 双架构、完整替换仍 Open。
