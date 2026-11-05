# r20：双架构真实 MF callback admitted Close/Flush/drain 有限验收

## 结论与边界

当前源码的托管生产程序集，在 Windows sandbox 的真实 X64/X86 进程下，原样调用 `MediaFoundationResourceTests.WhenRealReadCallbackIsAdmittedThenCloseFlushesAndDrainsAsync` 均通过。另原样运行同组跨线程 receive/Close 与 40 帧/EOS 两项，各架构共3项通过。**这是纯托管生产程序集 + 系统原生 MF 互操作，不含 app NativeAOT DLL/导出入口，不是新的 NativeAOT 验收。OverallOpen。**

生产修改0、现有测试源码修改0。新增 `Tests/CandidateMediaCallbackR20` 独立项目与执行器，链接原 ResourceTests 三份 partial，沿用现有 MSTest 4.0.1 断言和既有 friend assembly 名 WpfGfxShape.Tests；没有新增 InternalsVisibleTo、mock 或生产条件/钩子。使用生产中已经存在的 ReadCallbackAdmitted 观察点，不新增注入。调用的是公开测试方法，但被测 owner/reader 是内部 API，**不冒称新测了公开 app 媒体 API**。

未执行 original/comparison、未加载/复制原版 gfx、无 fallback、无终端/脚本。defaultasync/oracle/expected/阈值/DPI/flags 均未修改。测试名称中的 OriginalAvi 指该测试自身生成的40帧2x1无音频 AVI，不是运行原版 WPF。无 reference 数据依赖，不关闭 full 门禁。

## 实际调用链

测试 OpenAsync → CancellableVideoReaderOwner MTA 命令 → MediaFoundationSourceReader.Create → 系统 MFCreateSourceReaderFromURL。ReadAsync → BeginRead → callback.RequestRead → 真正 IMFSourceReader.ReadSample（slot9）。系统回调通过生产 unmanaged Stdcall vtable / UnmanagedCallersOnly OnReadSample，Enter 增活动计数并对借用 sample AddRef 后，既有 ReadCallbackAdmitted 暂停。测试此时 CloseAsync 两次返回同一 Task，且 Task 未完成；恢复回调后 read 取消且 delivered=0。

CloseAsync 请求 closing 并排入 MTA 命令，生产 SourceReader.Close 调真实 Flush（slot10，0xfffffffe），WaitDrain(10秒) 要求 `_flushed && _active == 0`，OnFlush 本身也 Enter/Leave；排空后 Release reader/callback、MFShutdown、CoUninitialize，FinishCloseAsync 等 owner 退出并 Join。此运行覆盖真实系统 OnReadSample/OnFlush，不是模拟回调。它没有证明源端 callback 尚未 admitted 的 pending、Flush失败/永不回调、所有竞态或 URL 创建强取消。URL同步创建不可取消仍 Open。

## 真实运行账（无隐藏重试）

独立 publish 共3次：首次 x64 exit1，漏链接原 PropVariant partial 导致 VerifyPropVariantStorage 不存在；日志同时发现 MSTest 自动入口会覆盖监督 Main。只修独立 csproj：补原 partial、StartupObject=Program、GenerateTestingPlatformEntryPoint=false。第二次 x64 exit0、第三次 x86 exit0，Release/self-contained，PublishAot=false、WpfGfxShapeManagedAcceptance=true，项目引用当前生产源码。未重发 app Native。独立 build0、run_tests0、sandbox2（各架构一次），测试方法共6次，无业务失败。未执行其他测试。

首次发布工具原输出625035字符，仅保留最后20000字符/60行，已读保留日志，不能说全编译输出已读。成功发布日志36/37行均已读；存在 CA2007/MSTEST0001/MSTEST0037 等警告，首次生产编译保留尾部 CA1416，未隐藏或放宽警告。

父监督同架构启动自身 child 参数，不经过 shell。业务进程150秒期限，超时整树回收后等15秒，管道各等15秒，sandbox210秒；不是所有同步系统调用可硬实时取消的证明。本次没有触发超时/kill。执行器直接调用原测试方法，保存真实 `result.json` 而非 MSTest 框架运行，因此**没有 TRX，不伪造 TRX**。父 stdout 是 sandbox 返回的一行退出摘要，父无主动 stderr 输出；本轮未额外持久化父管道文件。child stdout/stderr 独立保存。process-results 的 parentExit 是父最终返回值字段；最外层工具没有数值 exit，不补造。

|架构|sandbox task|父 PID/exit|子 PID/exit|核心/重入/正常耗时 ms|
|---|---|---|---|---|
|X64|c310524d61fe44ffa30ed4c3c753ecaa|12316/0|10292/0|201 / 14 / 21|
|X86|8c99d0848125440ba225942e40d77f04|4720/0|9304/0|218 / 128 / 27|

两侧 timeout=false/reaped=true/drained=true/killError=null，child stderr 空，stdout各6行START/END，result exit0、3项 passed=true/error=null。结果目录 `artifacts/candidate-media-callback-r20/readback-{x64,x86}/Evidence`。

## 身份与资源原件

.NET 10.0.13，实际 ProcessArchitecture X64/X86、pointerSize8/4；self-contained apphost 不等于 NativeAOT。两侧生产托管实现 SHA **CD75E6EF0A6490AA6C696B8B7DA08A89B036DE398EB7F2ADD55E34025AEE330C**，MVID **a9abd46a-f179-4d7c-839e-60d825cdb21e**。同一 AnyCPU 实现由两个架构的运行时执行，不把同 SHA 误称两份机器码。

- X64 测试 DLL SHA D979F5AA9160952E43CDA209C9613593302B8D7CDF84FA5C10606073E2E7E807；MVID dca636ff-6289-497e-83f8-4ada74bd7663；apphost SHA 30CB77C0790E6C5703542284FBCB9EA5DC5C39F0E55046946F4B15AD22FF002C。
- X86 测试 DLL SHA 1554A1DAE3E0F4DA7D203CE3D92E8136E7E45589E63CA945FCEEF8E9EF8ADD95；MVID 3abc4d0e-88af-4742-9521-5b828b4a80db；apphost SHA 1F5868704C59DBACB01BFC3900D8A98FFABE2709B63FC3BABD2E780F3BBAE114。

两侧核心 `cancellable-reader.json`：delivered0，ReadCallbacks1/FlushCallbacks1，SamplesRetained1/SamplesReleased1，Drained=true，FlushResult0，ReaderFinalReferences0/CallbackFinalReferences0，ShutdownCount1/UninitializeCount1，ThreadExited=true。时间线顺序都是 admitted → cancel accepted → close incomplete while held → callback released → read cancelled → close completed。cleanup保存真实 Flush0、OnFlush+callbackDrain、reader.Release0、callback.Release0、MFShutdown0、CoUninitialize；attributes.Release1 如实保留，不宣称所有中间COM引用均0。

重入用例 delivered1、ReadCallbacks1/FlushCallbacks1、reader/callback最终0、shutdown/uninitialize1、ThreadExited=true。正常用例 frames40/ended=true/previous39000000、ReadCallbacks41、SamplesRetained40、SamplesReleased0、SampleReleaseCount40/BufferReleaseCount40/UnlockCount40；SamplesReleased0是回调把引用移交给 reader 后由 reader 释放，不是40次未释放。两侧cleanup最终均 Flush/OnFlush+drain/reader0/callback0/shutdown0/uninitialize。

resources.json 保存两侧退出前16个系统线程快照，不当作逐线程源头归属或全局无泄漏证明；owner ThreadExited 来自原测试，最终进程正常退出。模块快照含实际 WpfGfxShape.ManagedAcceptance.dll 与系统 mfplat/mfreadwrite/mfcore/mfsrcsnk，无 wpfgfx_cor3.dll。核心收据系统 mfplat/mfreadwrite 版本10.0.26100.9278，枚举路径报告 SYSTEM32（包括x86进程），不凭该字符串推断实际机器码，架构以进程pointer与runtime为证。

上述两侧 identity、process-results、result、child stdout/stderr、cancellable-reader、reentrant、normal全部143行、resources全部99/103行已亲读。生成的 AVI 为输入产物而非动态结果，未另做字节审计；没有已运行未回读的结果任务。

## 既有 Native 身份复核与保留门

x64已亲读 r19 原件 `artifacts/candidate-desktop-baseline-r19/readback/Package/Results/hw-module-Native-1797e099a86148f6b330d57b3159c5ea.json`：Requested=Actual 指向task327226847cc7449ab047aa184b21b3dc/Package/Native/wpfgfx_cor3.dll，Architecture X64，SHA **E158C6277E3B3043E25A98D808C9D7B5ACB81CFE0DE8EBE5D3187B8381C1073B**。这是既有运行收据复核，不是本轮重新计算该 Native 文件。

x86报告 r15/r16一致记载 SHA **9FE6044FCCEE5C5A3D6E9452E8FF2506D4A67716942D11732CC60611BDA3E7FE**、4333056字节/I386/无CLR。r16指向的 final-readback 当前不存在，artifacts按candidate-media目录仅找到本轮r20；全工作区该SHA文本搜索只找到历史文档。故只能确认报告一致，**不能宣称已亲读x86原始收据或重新hash Native**。不假设该文件仍在，不拿本轮托管结果替它认证。

桌面nativeControl原门仍失败，r19基线后单次Present6秒无新帧，停止捕获派生。full缺original-wpf.json/glyph-arial-original.json的既有阻塞保持，不重建oracle、不关闭RequireReferenceData。没有发现需要本轮修复的生产缺陷，不扩大生产修改。下一步应由真实需求选择公开 app NativeAOT 取消覆盖或取得缺失可信reference；源端pending、异常Flush与URL取消均不得据本轮标完成。
