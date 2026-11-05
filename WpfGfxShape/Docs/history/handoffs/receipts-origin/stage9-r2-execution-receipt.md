# Stage9 r2 即时执行账

## 文档收尾回读确认
正式stage9报告r2新增1–43行全部分段回读；next-session-handoff/current-work-item/current-stable-baseline/current-execution-rules/replacement-issues-and-plan-status/final-acceptance-remaining-matrix/wpfgfx-complete-replacement-validation-plan七份新增顶部全部逐份回读成功；本receipt全27行亦已回读。旧正文保留为历史，不声明旧几百行本轮全文重读。所有本轮新增文档均已写入并回读，无新增正文尾未读。

## publish1 实际返回
RunDotNetPublish完整参数为下文拟发布参数，实际已执行。原return：执行成功。完整日志共 58 行，可使用 read_last_log_lines 按行读取。日志1–58已亲读；exit0；stdout共615375字符，前595375截断，尾20000可见；warning总数Unknown（存在CA1416，未给汇总），error汇总Unknown/命令成功。生产ManagedAcceptance、Tests及COMAcceptance均Release编译并发布到预定publish。clean新生成aot.sha256实读仍6BDF全值，runtimeconfig全21行实读Core/Desktop均10.0.13。normal0/2 tests0/2 COMpublish1/2 sandbox0/2 Native0；publishCompile1另列。

## sandbox1 实际返回
参数sourceDirectory=WpfGfxShape/artifacts/independent-media-stage9-r2/clean；executableRelativePath=WpfGfxShape.ComAcceptance.exe；arguments=[--supervise-exit,Results/stage9-r2,120,FullyQualifiedName=WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenPublicComProxyRunsThenPacketsAndRealVideoControlsPass]；localOutputDirectory=WpfGfxShape/artifacts/independent-media-stage9-r2/readback；timeoutSeconds=180；outputRelativePath省略整任务。
return可见ForegroundTopology supervisor-before-child，caller1908/session1，远端task=1b55572c97374fbc8203b2bda9efce7e，exe位于C:\CodingAgentSandbox\Tasks\1b55572c97374fbc8203b2bda9efce7e\WpfGfxShape.ComAcceptance.exe；父10596 WinRemoteShell/session1；foreground AweSun1228；ModuleError=null。原return未给exit/warnings/errors，不猜外层exit。结果已保存到d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\independent-media-stage9-r2\readback。原可见完整return保留于本会话，未声明另有raw。normal0/tests0/publish1/sandbox1/Native0，publishCompile1。

## 最终系统回读与计数
TRX实际stdout精确路径Results/stage9-r2/TestResults/child.trx，虽目录列表漏列仍直接Read成功，26行全读：1executed/1passed/0failed/0skipped，duration2.6737862s。监督40行全读child2700/ExitCode0/exited/TimedOutfalse/Cancelledfalse/KillAttemptedfalse/OutputCollectedtrue/DrainTimedOutfalse/SupervisorErrornull；stdout12行全读（中文编码损坏保留）、stderr空、aot-identity全1行读。业务Results/independent-media-stage6/result.json共469行分1–55/56–165/166–275/276–385/386–469全文亲读；该stage6目录是本新任务生成，不是旧证据。diag48起显示截断，另63–65完整尾读到session exit0；不宣称整个diag全文。
Opened1/NewFrame13×21/Ended7，packet20bytes/HR0/两UTF16长度0。pauseWindowTicks5197853=519.7853ms，count6/PTS5000000不变；Seek25000000 epoch2后帧无旧epoch；红蓝像素0000FFFF0000FFFF与FF0000FFFF0000FF；Ended回调CloseHr0；stateClosed/Failure0/identity refs0/registryRemovedtrue/descriptorDisposedtrue。native refsUnknown；非空UTF16系统packet、真实missingfile Failed、回调内Release及Close/退订并发Unknown，未用协议80070002冒真实系统失败。
wrapper SHA3BD4B8220230E6206AAE810D151A0ED3C417B61F2A184DA2D913D6818F242CB6/MVIDeeb32c6b-38ef-4723-97a6-4c0ecf75f2ef；source-linked Tests SHAB6D152215FCC004C5FC843451A32E48C49BD5CA39EEDFAB709FE25F4F31085DA/MVID295a0604-b042-4e1f-a8e6-129e8dceb93b，分层；独立ManagedAcceptance SHA/MVID未输出Unknown。runtime.NET10.0.13/X64，实际mfplat/mfreadwrite/mfsrcsnk10.0.26100.9278、mfcore10.0.26100.1均系统路径，完整见正式报告。6BDF旧PE仅初始化非新NativeAOT。
本轮normal0/2 tests0/2 COMpublish1/2 sandbox1/2 Native0/0，publishCompile1/implicitDebug0；源码修改0。58pure仅继承上轮56+2，本轮新系统TRX1passed不加成59pure。无重复追绿。旧两publish失败及RemoveDir误删自有输出如实保留。factory仍E_NOTIMPL/null，原生产loader不可达；audio/AVsync/强cancel/NativeAOT/full/OverallOpen保持。WPF初始化顺序及9/10/11/12/13/14/15/17/21等准确HRESULT.Check裁定已完成，详正式报告；下一真实HasAudio流metadata及本地buffer/download→Scrub/UI帧/pre-open rate，不用假音量存储。

## publish完整实际命令（用于参数审计，非脚本执行）
dotnet publish d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -r win-x64 -f net10.0-windows --self-contained true -p:RuntimeFrameworkVersion=10.0.13 -p:PublishAot=false -p:WpfGfxShapeManagedAcceptance=true -p:ComAcceptanceSandbox=true -p:AotRuntimeIdentifier=win-x64 -p:AotDllPath=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/independent-media-stage8-r1/clean/Native/wpfgfx_cor3.dll -p:ComAcceptancePackageDirectory=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/independent-media-stage9-r2/clean/ -o d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/independent-media-stage9-r2/publish/

## 授权及发布前静态检查
新增预算 normalbuild<=2（仅源必要修正）、tests<=2、COMpublish<=2、sandbox<=2、Native=0。当前全0；禁止终端/脚本/再委派；程序仅Windows沙箱。
已亲读next-session-handoff顶部、stage9正式报告全部28行、current-execution-rules顶部及stage8 receipt全部16行。上轮两次publish失败保留：不存在fixture；publish=clean导致RemoveDir删除自有输出。上轮未运行系统，不能虚报原件。
规范化绝对publish路径：d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/independent-media-stage9-r2/publish/
规范化绝对clean路径：d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/independent-media-stage9-r2/clean/
静态关系检查（不区分大小写、统一/、末尾/）：两者不相等；publish不以clean为前缀；clean不以publish为前缀；是同一r2父目录的兄弟目录。ComAcceptance.Package.targets43–74已读：AfterPublish先检查publish Native再RemoveDir clean，后复制，以上路径不会相互清空。
实际列目录确认stage8-r1/clean/Native/wpfgfx_cor3.dll存在，clean/aot.sha256实读6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403；stage8执行账记录其为成功publish2同SHA副本。仅旧PE初始化，不是本轮NativeAOT或AV实现。新publish由GetFileHash生成旁车，不手写SHA。
源码实读FQN=WpfGfxShape.ComAcceptance.IndependentVideoPlaybackOwnerTests.WhenPublicComProxyRunsThenPacketsAndRealVideoControlsPass；先运行两个生产COM桥协议测试再真实AVI控制。ExitDiagnostics.Parse要求--supervise-exit root seconds filter；filtered上限120；外层180，整任务回收。
拟发布参数：绝对COMAcceptance.csproj，Release/win-x64/net10.0-windows，selfcontained=true RuntimeFrameworkVersion=10.0.13 PublishAot=false WpfGfxShapeManagedAcceptance=true ComAcceptanceSandbox=true AotRuntimeIdentifier=win-x64 AotDllPath=上述已存在stage8-r1/clean/Native/wpfgfx_cor3.dll ComAcceptancePackageDirectory=上述clean，-o上述publish。不从bin运行，不绕ValidateIdentity，不启用factory或原版loader。
