# Official direct consumer：固定两时点观察（执行前快照）

## 冻结范围与合同
本门已授权，仅修改现有 DirectConsumer 独有默认关闭 partial 与文档，沿 ConsumerDirectDiagnostic；原两个三色方法和严格断言语义保留。共享 helper、生产、WpfGfxShape.Tests、原业务、入口、Package.targets、SDK/TFM 均不改。禁止终端/脚本/委派，执行仅 Windows 沙箱。

新方法 FQN：`WpfGfxShape.ComAcceptance.D3DImageTests.WhenDirectOfficialConsumerReceivesRedThenFixedWindowIsObserved`。仅 official 一次合法红提交，沿现有初始化、独批 Present/event/consumer flush/首次 DwmFlush。首次 GetPixel 原值无条件持久保存，不因颜色错误中止。以首次 GetPixel 前 QPC 为基准，截止=+5秒；等待只按时间，不读取颜色、不再提交、不重复 DwmFlush，原独立 owner 消息泵正常运行。到期仅一次 DwmFlush 和一次 GetPixel。总颜色读取2；任何 HR/event 失败立即转清理，不续观察。首次采样记录调用前后QPC；第二次同样记录；调度有效窗口预定为第二 DwmFlush 开始不早于截止且第二采样结束不迟于截止+250ms，首次读取自身耗时也须≤250ms。迟到如实 timing-invalid，不延长、不跳帧。5秒仅经验观察窗口，不是 D3D/WPF 延迟保证。

记录两个原始 COLORREF、QPC/frequency/实际间隔、target/HWND/owner、可见/iconic/中心点遮挡辅助信息；分类两者匹配/首错末对/两者不符/首对末错，与 cleanup/timing-invalid 独立。最终颜色错误必须 Failed；首错末对最多观察门自洽 Passed，不冒首次通过或三色/产品通过。任何结果后停止，不追加探针。

## 预算与身份
先静态审查主异常保留、逐项清理、finally 无 throw、取地址局部变量不由闭包捕获，再正常 COM build1；失败即停。成功后 COM publish1，self-contained .NET/WindowsDesktop10.0.13/win-x86，AotDllPath 显式 KP clean20B3，独立 `artifacts/direct-consumer-fixed-window/` publish 与 clean。包根角色、manifest、runtime、入口核验后唯一 official 沙箱1，精确 FQN 等值 filter，`--supervise-exit Results/direct-fixed-official 120`，外180秒。发现/candidate/Native发布/重试/resize/x64/full/原三色执行全部0。

候选文件预期 SHA256=20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D，仅包文件身份，不冒实际加载。亲读 TRX/监督/log/业务JSON 后填 test SHA/MVID/runtime/真实UID、actual official身份、HR/event、refs/owner销毁/cleanup和实际等待。

## 历史账本与证据边界
此前 direct build2（1败1成）/publish1/official1失败；本次另列并累计。历史 HD/HE/HF 最终成功未保存首次值，不是本门已复现。WGC build4（1败3成）/publish4（1败3成）/沙箱3（发现1真实2）/真实2失败，撤后default build1成功另列；KS build1/publish1/沙箱4/请求3/发现1/体1/A1/B1/native2不变。通用打包退场残项保留，完整替换仍 Open。

## 执行结果
**唯一official已执行并停止：两次均旧色，真实Failed，另独立timing-invalid，不追加探针。**

### 实施与静态审查
仅现有DirectConsumer partial及文档；一个新guarded TestMethod，私有RunDirectConsumer增加默认false参数，复用全部合法初始化/红提交/清理，无协议复制。原三色方法调用、期望和首错断言不变。新路径一帧，两次调用采样函数；等待仅QPC/Thread.Sleep，不取色/重发/按颜色判断，原owner独立GetMessage继续正常进展。原值先持久写JSON，最终再断言。

清理逐项捕获托管异常并保留主异常EDI，后续错误附主异常Data；event Dispose/临时DangerousRelease纳入Cleanup，finally无显式throw。native输出变量清理用值快照；取地址deleted为闭包内部局部，不捕获被取地址变量。DC、detach、各资源/批/通道/连接/partition、四COM refs、callback与owner销毁均尽力清理。不宣称native灾难性终止或故障注入全覆盖。共享ready/Invoke/Join无局部截止仍保留，外监督120/180。

日志标签缺口：入口仍复用DirectConsumerContract旧三色资源，stages首行描述不准确；本次未新增资源键，运行后未尾改源码/补发。真实FQN、TRX仅新方法、JSON Submissions1/ColorReads2明确实际合同，旧标签不代表运行或通过三色。

### 构建发布与clean
正常COM build1成功：ConsumerDirectDiagnostic/win-x86/net10.0-windows，Exit0，134warnings/0errors，4.98秒。可读日志63行已全读，stdout105819字符前85819截断，不宣称全部warnings已审。分析器正常。此后源码无改。

COM publish1成功，self-contained true/ComAcceptanceSandbox=true/AotRuntimeIdentifier=win-x86；显式AotDllPath为KP clean的Native/wpfgfx_cor3.dll，独立fixed-window publish/com-acceptance-package（创建前根不存在）。发布日志60行已读末9行，无另计独立build，Native发布0。clean aot.sha256确为20B3完整值；runtimeconfig两框架10.0.13，deps win-x86/runtimepack10.0.13；manifest621行核首18、390–430、614–621，入口/测试/MTP/Contracts/根official/Native candidate分离存在，未逐条重hash全包。离线公开API确认新方法；NativeRun/ExitDiagnostics源码确认监督/等值filter，无运行发现。只上传clean，未上传publish根。

### 唯一运行与TRX
任务3d53aad485524c258da5402289f91e79；回收根artifacts/direct-consumer-fixed-window/readback/official/，下级Results/direct-fixed-official/。supervision.json全40行、child.trx全53行、direct-consumer.jsonl全57行、stdout全32行及空stderr亲读；stages读首15及45–58。stdout中文存在替代字符，以UTF-8 TRX/JSON裁决。

监督2480/子11108，02:33:42.3290992→02:33:49.4378314 UTC，exited/Exit2；TimedOut/Cancelled/KillAttempted=false，OutputCollected=true，drain超时false/SupervisorError=null。TRX total/executed/failed=1/1/1，passed/notExecuted=0/0；唯一新方法FQN正确，UID/testId=12af8f6c-d70b-8ecd-87a5-0f07906181a9，executionId=bf27e748-71b3-4411-82e2-b5c7186a1483，runId=93baa6e9-4713-4716-9e21-bc12f79ccf6e，duration6.2665556秒。主失败second.Pixel=1973532!=255，非首值或cleanup异常替换。

### 身份
- test SHA=A56E6981D0289B0AE19755C0275A168EC6EA863BEA4F1F32B37BA130A0769C05，MVID=4dd062be-2383-4722-9e1a-b0f539884b74；业务/监督一致，实际.NET10.0.13/X86/PointerSize4。
- actual official Requested==Actual为任务根wpfgfx_cor3.dll（C:\CodingAgentSandbox\Tasks\3d53aad485524c258da5402289f91e79\wpfgfx_cor3.dll），SHA=5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834，10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。
- actual PresentationCore SHA=D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D，同10.0.13 servicing。
- candidate仅Native包文件/初始化校验20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D，未运行candidate，不冒actual加载身份、不判候选缺陷。backend unknown，无producer readback。

### 原值与真实时间（QPC frequency=10000000）
|字段|首次|末次|
|---|---:|---:|
|原始COLORREF|1973532（0x001E1D1C）|1973532（0x001E1D1C）|
|期望|255（0x000000FF）|255（0x000000FF）|
|读取前QPC|38894951057355|38895003747381|
|读取后QPC|38894953688394|38895003747659|
|读取自身秒数|0.2631039|0.0000278|

固定截止38895001057355，第二DwmFlush开始38895001187919，晚截止0.0130564秒；两次读取前实际间隔5.2690026秒，末次结束晚截止0.2690304秒。首次读取自身已超250ms，末采样亦超截止+250ms，TimingInvalid=true。首采样结束至第二flush开始4.7499525秒，包含首值持久记录和纯时间等待，不冒单独Sleep精确耗时。无延长/跳帧/补读。JSON分类both-mismatch，ProductAcceptance=false/Submissions1/ColorReads2。

初始化/producer fill/dirty/separate send/commit/consumer flush/两DwmFlush均HR0，event=true；duplicate1816沿原native转交约定，失败send所有权未实测，不称逐柄exact-close认证。target4/HWND23528798（0x167055E）/owner3860，业务native线程9200；两次Visible1/Iconic0/Foreground==HWND，屏幕点56,48、Cover==HWND。辅助可见性不充内容/scanout/HWbackend凭据。

### 清理与停止裁决
detach执行；资源4→1均Deleted1/HR0，delete close/commit/flush/channel destroy/disconnect/deinitialize均HR0；bitmap/surface/device/factory真实Release各0；callbacks两项(1,7)。窗口销毁Owner/ActualOwner/Current均3860，Result1。cleanup complete保留末值主断言，Errors=[]；DC/event无收集错误，但未独立逐柄核验。

**两次均旧色，此固定延迟不足以恢复可见红色。** timing-invalid也意味着不是严格预定调度窗口内的有效时间对照，不能据此排除所有时序假设，更不建立5秒API保证。未复现历史最终颜色成功，不称首次通过、三色通过或产品通过。有限consumer、当前候选与完整替换仍Open。

本次build1成功/COM publish1成功/official任务1请求1真实体1失败；与此前累计direct build3（1败2成）/publish2/official2真实体2失败。candidate/发现/Native发布/重试/resize/x64/full/原三色本次执行均0。WGC/退场残项/KS账本原样保留。

## 唯一下一门（需另授权，本次不实施）
停止执行。仅有界静态源码/现有原件复核producer→interop→target更新与窗口DC观察关系，定位可证实内容关联缺口并保留本次timing-invalid限制；只读源码/原件及文档，build/publish/沙箱/新探针0。无确定新证据前不重跑、不扩大等待、不恢复追色循环、不重开native/线程/WGC、不运行candidate/三色。此为调查，不以历史成功代当前复现。
