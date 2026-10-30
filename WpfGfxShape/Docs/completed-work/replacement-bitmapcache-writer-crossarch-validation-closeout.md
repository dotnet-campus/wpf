# BitmapCache writer 恢复执行与双架构验证

## 正式终态：CloseoutCompleted（执行与失败登记），验收Failed / 完整替换Open
恢复轮已结束，非磁盘阻塞；用户清理后真实build成功。本收尾阶段源码/build/test/publish/沙箱新增均0，无终端/脚本/再委派。以下旧“待测试/立即核对”等是执行中记录，已由本节终态替代，历史restore磁盘失败仍保留，不冲销。

|arch|task|total/executed/passed/failed|child退出|test SHA256 / MVID|
|---|---|---|---|---|
|x64|975ed574ad46431f8d7079466757a0fb|1823/1823/1821/2|2|99302E62E1A94C4DE7C5677A4EEC61844C4B2F942F41BC9A1CDEA555F29F324F / 72e3b206-39a6-47fe-9ee8-b78ee2ca9de6|
|x86|1e99f9304d9b4e88871dc63b200d0b80|1823/1823/1783/40|2|1639A355E25ECBA6A49654CF5C9ADEAAF1E6377AD06106B28237EC46DF8F0A25 / d4264b67-4a92-4c94-b37d-22ca54b5d91e|

两主TRX Counters分别14077/14431，其余计数字段均0；两supervision.json全文1–38亲读，.NET10.0.13/X64或X86，Reason=exited、HasExited/OutputCollected=true，TimedOut/Cancelled/KillAttempted/KillReturned/两OutputDrain超时=false、SupervisorError=null。x64 supervisor11972/child11400；x86 supervisor9884/child11788。工具自身独立数值OS退出仍Unknown，不把child2冒工具OSexit。没有用旧计数相减。

恢复轮实账：normal Release build1成功（146 warnings/0 errors），定向tests1（Debug隐式编译1、29/29 Passed），COMpublish2（各arch1，隐式编译2），default full沙箱2（各arch1，无重试），Nativepublish0。先前磁盘restore exit1独立历史；本收尾新增执行0。publish原始回执及此文恢复执行段保留；不是本收尾重新执行或重新hash。

身份分层：当前candidate Native x64 `6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403` / x86 `B07C9073F75278ED61C44638EB3644547B413A281E2859B25211CEAAC3D1466C`，official x64 `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1` / x86 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`。test身份如表，不能用Native或预检包hash代替test/每场景actual；独立d3d9与Unknown不冒候选。合法writer28通过编译和定向合同测试，不等每项HW业务已进入；非法32负控、0x84=36与生产exact28/payload20原样保留。

全部42跨arch失败实例（不是唯一方法数）已持久化[逐实例FQN/参数/testId/原Message/角色/已达阶段/证据行索引](replacement-bitmapcache-writer-failure-index.md)。x64两败official初FF80读0及exclusive foreground FailedNotTriggered；x8640败包含native初像素前置、fresh gate、9项S_PRESENT_OCCLUDED、official初native失败及新official AsyncDrain O6同线程。所有失败保留，不排除/冲销；不能将前置失败冒业务绿。x86两个historical official准确child原件均66行已核，1执行/1败/exit2，native初563412读F3F3F3，非沿旧crash归因。

清理边界：主监督退出/输出收集正常不是所有业务资源审计。x64 exclusive stdout明确device/factory/window complete；新x86 O6 callbackExited1/disposals1但completed=false，正常sibling/deinit断言没到，finally忽略HR不认证清理。其余失败独立cleanup字段Unknown，未宣称全部资源审计完成。未读所有Passed，巨长PresentOcclusionState内嵌栈/重复acquire并非逐字全读；详见索引覆盖限制。前执行段的build64行、test17行、publish manifest622/621行和源码writer范围为前代理持久化亲读记录，本收尾没有伪称重新读全部原stdout。

[新O6线程合同有界审查与最小设计](replacement-official-async-wic-release-thread-review.md)：当前证据未建立official最终Release必须异caller的保证；2204同线程回调退出且恰一次，与合法同步关闭相容，但精确最后owner/servicing符号未证明。下一直接任务是确认此线程合同并做最小fixture修正验证（或明确未证），保留candidate专有重入/worker强门，不改断言追绿，不重开旧前台/paint/ETW/捕获循环。本阶段不实施/不重跑。

最终矩阵：writer字节合同ClosedLimited；default full两arch Failed；HWtarget/backend实际身份与消费、consumer loss、audio、自然资源长期收敛、final矩阵与完整替换仍Open，memoryGreen=null。用户完整目标未完成。


## 恢复执行前快照
用户本轮明确已清理本地磁盘且空间足够；据此恢复实际执行，不再将容量作为未经证明的阻塞。空闲字节 Unknown，未查询，非代理自动删盘。
历史磁盘失败：绝对 COM 项目 Release/net10.0-windows（未传 RID）restore exit1，NuGet.targets(198,5) 本地 D: 临时文件空间不足，未编译；历史 normalBuild1/2、tests0/1、publish0/2、sandbox0/4、Native0。历史原件及旧默认 full x64 1823/1783/40、x86 1823/1786/37 不覆盖。

本轮授权独立账：显式正常 Release build最多2（恢复1+确定编译修正复验1）；定向 run_tests最多1（隐式Debug另记）；COM publish最多2，每arch1（隐式编译另记）；Windows sandbox最多2，每arch default full1，x64后x86，内1500/外1800；Native publish0。源码仅必要合法测试writer缺陷，不改生产、SDK、TFM、依赖或诊断配置。

亲读当前源码：
|合法writer|实际当前字段/长度|
|---|---|
|SoftwareShader modePacket|stackalloc/clear/send28；0x8D@0、handle@4、double1@8，余零|
|ActiveRender modePacket|stackalloc/clear/send28，同上|
|ActiveRender reused packet|buffer72/clear72/send28，同上；之后clear72复用0x84/36|
|Stage4Hardware BindHardwareCacheGeometry|array28，0x8D@0、mode@4、double1@8，余零|
|VisualCache transaction|array28，0x8D@0、hardwareMode@4、double2@8，余零|
|VisualCache configuration|array28，double1@8、BOOL@24=1，恢复写0及double2；未删字段|
|VisualCache secondary|array28，0x8D@0、mode@4、double1@8，余零；后续0x84/36保留|

animation@16、BOOL@20/24零初始化保留；0x84 Brush36及所有像素/expected/流程不改。亲读 BitmapCachePacketContractTests：独立Pack1定义28；非法长度0/8/24/27/29/32/36及非法BOOL/animation负控保留。生产 exact28/payload20 不放宽；Native源码/发布0。当前七处已经合法，不凭历史重复替换32。

## 实际执行记录
恢复build1：准确绝对COM csproj，configuration=Release、runtimeIdentifier=win-x64、targetFramework=net10.0-windows。工具原返回“执行成功。完整日志共64行”；已读1–64。exit0，146 warnings、0 errors、00:00:04.67。原stdout共114542字符，工具截前94542字符仅留尾20000；可见64行完整读取不代表原stdout未截。保留restore/分析器，可见MSTEST0044/0037/0045/0017、CA2007/2014。构建成功消除容量阻塞，不虚报空闲字节。本轮normal build1/2；待唯一准确定向测试通过后才发布两架构。定向tests1/1：准确绝对 WpfGfxShape.Tests.csproj；filter=`FullyQualifiedName~WpfGfxShape.Tests.BitmapCachePacketContractTests|FullyQualifiedName~WpfGfxShape.Tests.GeneratedProtocolTests`。先亲读类确认同根serializer/command。工具原返回“执行成功。完整日志共17行”，全部1–17已读，exit0，29 total/29 passed/0 failed/0 skipped，30ms。正常restore；隐式Debug编译1（输出Debug/net10.0），不冒Release测试；无warning/error汇总，日志未见warning/error。未跑root、未重跑。原可见stdout：正在确定要还原的项目；已还原Code/WpfGfxShape(167ms)，1/2最新；Core与Tests输出Debug；1测试文件匹配；已通过29。所有真实返回及截断边界逐次追加。两COM publish已正常成功（每arch1，隐式编译2），各回执位于 artifacts/writer-fixed-default-{arch}-r1/publish-receipt.md。新clean aot.sha256分别完整匹配x64 6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403、x86 B07C9073F75278ED61C44638EB3644547B413A281E2859B25211CEAAC3D1466C；来源旧认证clean/Native，不借diagnostic。两runtimeconfig亲读NETCore/Desktop10.0.13；manifest尾含Contracts/Native/hash，x64共622行、x86共621行。正式NativeRun --supervise-exit/ExitDiagnostics --full，1500且无filter；Release诊断编译guard不启用，不改skip/expected/并行/fallback/GC。预注册两sandbox：source=artifacts/writer-fixed-default-{arch}-r1/clean，exe=WpfGfxShape.ComAcceptance.exe，args=[--supervise-exit,--full,Results/writer-fixed-default-{arch},1500]，timeout1800，整任务回收至artifacts/writer-fixed-default-{arch}-readback，顺序x64后x86。一次内容搜索误及二进制coreclr.dll（已立即停止），不作证据，不继续扫描二进制；后续只精确文本路径读取。整体/HW backend/产品loss/audio/自然资源/finalmatrix仍Open，memoryGreen=null。
