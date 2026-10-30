# WIC final release owner x86 本轮执行记录

状态：CloseoutCompleted / ClosedLimited。原owner采集与离线身份认证已收尾；WindowsCodecs PDB age拒绝导致离线未处理异常，不是offline成功。后续独立源码合同裁决、最小fixture修正及official O6单UID验证已完成，详见[正式合同收尾](replacement-official-wic-release-thread-contract-closeout.md)。旧caller2204现场无调用栈，不再追不存在现场；新运行仍worker callback，不声称复现旧同步。下文预算、InProgress与假设均为历史过程记录。

## 离线最终事实
亲读 artifacts/wic-final-release-owner-x86-r1/offline-execution-receipt.md 全文及 artifacts/wic-owner-offline-x86-r1/readback/Results/wic-owner-offline/wic-owner-symbols.txt 关键小段。taskbe350db878194b508cf2c095a65bb465：actual official5EBD与RSDS/PDB087c735f-43d9-448a-98ac-993527e505ec age1匹配；raw返回地址匹配worker→ProcessBatches→DetachChannel→ServerChannel析构→HandleTable→SlaveBitmap→CWICWrapperBitmap→CManagedStreamWrapper。不是可信完整unwind。WindowsCodecs SHA匹配，但同GUID d2541541-c321-cd02-9ddd-a66d905d9de3 的PDB MSF age3不等PE RSDS age1，严格拒绝正确。InvalidDataException未捕获，offline-exit.txt缺失，独立OSexit Unknown，未认证codec符号；不继续下载。离线独立预算normalbuild2/2、publish1/1（implicit1）、sandbox1/1已耗尽；原采集build2/publish1/sandbox2独立保留。本次合同轮build1/2、publish1/1、sandbox2/2、Native0/full0/额外offline0。

## 本轮执行前来源与独立预算快照
已有运行taskd3bc29c5365549969765b406ea5ee635，caller12612/callback10604；Destroy返回先于callback。原始wic-final-release-owner.json亲读头部：pointerSize4，callbackQpc41541630602679，destroy.after41541630592434；是异线程异步路径，不是旧caller2204分支。用户说明先前代理已全读TRX业务部分，本轮不将其冒充亲读。来源目录：artifacts/wic-final-release-owner-x86-r1/official-o6-readback/Results。actual official SHA256=5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834，runtime10.0.13 X86；fixture SHA前缀A6FEF0B4、MVID924710fd-f085-454a-9c81-4e73cf8ca2c5须与模块元数据分别核对，不能由file初始化身份代替actual映像验证。39个RtlCaptureStackBackTrace原始返回地址不具备完整unwind承诺。

新增授权独立计账：normalCOMbuild<=2、COMpublish<=1、offline Windows sandbox<=1（外300秒）；当前本轮均0。Native0、原O6重跑0、full0、显示探针0、生产源码0改。前轮已执行normal build2成功/146warnings、publish1、sandbox2（发现+O6），与本轮预算不相抵。离线目标最多actual official和WindowsCodecs两模块，先SHA和RSDS/MSF GUID+age认证再解析，缺失只能Unknown。禁止近似映像、忽略GUID、CLI或脚本、委派、新采集。缺失文档更新作为实际待办，而非停工理由。

## 预注册（运行前）
假设H1：Destroy内部同步flush后，worker已撤销batch/attached/storage引用，caller在RemoveChannelFromTable撤销server最后引用，经table→bitmap→wrapper→WIC→stream同步最终Release。竞争假设H2：最终释放由worker完成；一次非caller观测不排除H1。现有静态链尚未闭合wrapper/WIC及actual5EBD映射，不授权修改official非caller断言。

先建立caller stream/frame/wrapper、channel/table、pending batch、sibling及Close引用账；若仍缺环，在原OnDispose内默认关闭、预分配有限缓冲捕获RtlCaptureStackBackTrace、QPC/TID，退出后映射模块路径/hash/base/RVA。raw捕获不冒充完整native unwind，不下载近似PDB。callback不IO，不改变Release顺序及引用计数。一次official O6，不为复现caller重跑。

预算：normalCOMbuild<=2，COMpublish<=2，sandbox<=3（纯发现计一次），Native0/full0/生产0。当前已执行build1/publish1/sandbox0。normal build准确绝对COMcsproj，WicFinalReleaseOwnerDiagnostic/win-x86/net10.0-windows，无Aot参数；工具成功，日志64行exit0、146 warnings/0 errors、7.42秒，stdout114565字符前94565截断。publish同配置self-contained true、显式AotDllPath及NativeAotDllPath=cache-packet-fix-x86-r1/clean/Native/wpfgfx_cor3.dll；工具成功，日志60行exit0、stdout57584字符前37584截断，已读1–10及55–60；无warning汇总。独立clean/aot.sha256=B07C9073…1466C，runtimeconfig两个framework均10.0.13。publish隐式编译1，Native0。发布必须独立wic-final-release-owner-x86-r1/clean，self-contained .NET10.0.13 win-x86，显式B07C9073F75278ED61C44638EB3644547B413A281E2859B25211CEAAC3D1466C候选并核验副本。新fixture先实际发现UID，后仅--filter-uid official O6，结果目录与同步诊断分离，外部180秒。不使用FQN执行整方法。

发现沙箱task=e44949862fb14cfeadf5964be971024a，原返回MSTest4.0.1 win-x86/.NET10.0.13，发现6参数，无测试执行；独立数值OSexit未提供。DiscoveryDiag/log_261030040406614.diag第54行实证O6 UID=1e4c35fb-589a-8de1-838e-4845fd1028f4（新字节下实际发现，非套用旧值）。sandbox1/3，外部180秒，整个任务已拉回discovery-readback。下一仅该UID。

旧official caller2204失败保留；x64 full1823/1821/2、x86 full1823/1783/40及42跨架构失败实例不相减。HWcache/productloss/audio/naturalresources/fullOpen，memoryGreen=null。
