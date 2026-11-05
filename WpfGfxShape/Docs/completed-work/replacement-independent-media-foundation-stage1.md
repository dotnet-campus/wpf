# 独立 Media Foundation 第一阶段：已执行证据与收尾

## 正式状态
Stage1ResourceFoundationValidated / CloseoutCompleted；独立播放器 Open / NotImplemented，完整替换 OverallOpen。不是媒体功能完成、NativeAOT认证或最终部署认证。本次仅亲读原件、修改并回读文档；源码/build/test/publish/沙箱/Native 新增均0，无终端脚本、再委派或新增运行。

用户要求“先删再实现”已按顺序执行：[删除原版委托](replacement-remove-original-media-delegation.md)先完成，再增加本阶段真实系统资源基座。生产禁止任何原版wpfgfx媒体后端，Windows公开媒体组件允许；无需再次询问独立后端许可。原版加载器和player/provider创建委托已删除、源码不可达；工厂仍 E_NOTIMPL (0x80004001)/null，4例负向通过只证明安全失败，不是播放通过。原委托媒体绿撤销其当前认证效力，仅保留历史，不迁移为独立媒体成果。

## 实际代码文件与所有权
- `Code/WpfGfxShape/Core/Av/MediaFoundationSample.cs`：internal、串行/线程亲和 owner，真实 MFStartup、MFCreateSample、MFCreateMemoryBuffer、AddBuffer、长度、Lock/write/read/Unlock、Close；未新增公开API。仅资源创建/写读/释放基座，不是播放器或SourceReader。
- `Tests/WpfGfxShape.Tests/MediaFoundationResourceTests.cs`：资源闭环行为测试；正式Tests及COM项目依既有source-link惯例使用相同internal生产源码。COM运行中implementation是WpfGfxShape.ComAcceptance托管程序集，不是实际生产Native导出。
- `Code/WpfGfxShape/Core/Av/SystemMediaModule.cs`、`SystemMediaSource.cs`及`Code/WpfGfxShape/Abi/MediaExports.cs`：承接上一删除阶段的退役加载器、不可创建占位及E_NOTIMPL/null工厂边界；本阶段未接通成功创建。
- `SystemVideoSample.cs`与`Code/WpfGfxShape/Core/GeneratedMediaPlayerResource.cs`现有lease适配不代表独立视频源/时钟lease已实现，不能将IMFSample指针冒充IWIC/MIL bitmap source。

|资源|取得|所有者与关闭|
|---|---|---|
|MF初始化|每个owner Create调用一次MFStartup；仅成功才标记started|成功Startup每owner匹配一次MFShutdown；不是进程全局只启动一次，重复Close不再Shutdown|
|IMFSample|MFCreateSample输出引用|owner先Release sample|
|IMFMediaBuffer|MFCreateMemoryBuffer输出引用|owner持有；AddBuffer另增sample自己的引用；先释放sample，再释放owner的buffer引用|
|Lock指针|成功Lock|方法内借用，不逸出；finally Unlock，包含超长写/短读返回路径|

成功/部分失败均有关闭路径；Close幂等并保留关闭HRESULT。基座仅调用方串行、同线程创建/使用/关闭，不在MF work queue初始化/关闭，不代替COM apartment初始化。无blanket Exception捕获；HRESULT显式返回，必要清理失败通过Marshal.ThrowExceptionForHR上抛。library P/Invoke入口有OperatingSystem.IsWindows guard，mfplat DllImport限定System32；不是跨平台媒体实现。事件代理及其AddRef/回调所有权本阶段未触碰；诊断只记录系统MF模块，不引入原版loader。

ABI契约来源为前阶段亲读Windows SDK 10.0.26100.0 `mfapi.h`（MF_VERSION=0x00020070/FULL=0）、`mfobjects.h`、`mfreadwrite.h`；internal stdcall、DWORD uint、LONGLONG long、HRESULT int。未引入包、工程或修改生成代码。本次核读MediaFoundationSample实现，不重复冒称重新阅读SDK；官方AddBuffer网页曾HTTP504，不记为已读。

## 已执行原件与身份
证据根（相对WpfGfxShape子目录）：`artifacts/mf-stage1-r1/readback/Results/mf-stage1/`。既有任务 `task2b496eab33774a56be96ed90acedeaf2`，单FQN `WpfGfxShape.Tests.MediaFoundationResourceTests.WhenSystemSampleOwnsBufferThenBytesRoundTripAndResourcesClose`。

本次亲读 `media-foundation-resource.json` 全76行、`supervision.json` 全40行及 `stdout.txt` 全12行。前代理已全文读TRX26行/监督40行/业务76行，记录见仓库根 `.coding-agent/mf-stage1-execution.md`；本次不重复宣称亲读TRX，当前目录列举未含TestResults原件，stdout虽有编码乱码仍保留其1总数/1成功/0失败/0跳过数字，结合既有TRX记录使用，不补造缺件。

- completed=true；既有TRX及stdout：1执行/1通过/0失败；监督HasExited=true、Reason=exited、child OS ExitCode=0。
- 无timeout/cancel/kill/drain timeout/SupervisorError。工具未直接给出父进程exit，不补造父exit0。
- testSHA256 `9D818E2EC95B34F546429642F69C5C7AC9859F518A8633A3109A23F7CF4EE51E`；MVID `173f93c0-0fd3-485a-8e84-8f4911a95675`；.NET 10.0.13 / X64；托管测试程序集WpfGfxShape.ComAcceptance.dll。
- 实际模块 `C:\WINDOWS\SYSTEM32\mfplat.dll`，版本 `10.0.26100.9278 (WinBuild.160101.0800)`。
- 创建/AddBuffer、初始长度、写入/长度、正常读、Release/Shutdown、idempotent-close、finally-close均HRESULT `0x00000000`。
- capacity=32，initial current/total=0/0；written current/total=6/6；input/output均 `ABF/gP7/`（6字节），真实写读一致。
- oversized-write=`0x80070057`，undersized-read=`0x8007007A`；UnlockCount=4，ReleasedReferences=2，ShutdownCount=1。
- SampleReleaseCount=0、BufferReleaseCount=0是两次IUnknown::Release返回的最终引用计数，不是“没有释放”，也不是HRESULT。业务聚合Release/Shutdown HR为0；不把计数伪写成逐次Unlock的独立HR原件。

## 实际执行账（已执行阶段，非本次新增）
详见根 `.coding-agent/mf-stage1-execution.md`，删除阶段账独立，不混加：

|类别|实际|边界|
|---|---|---|
|normal Release build|1（预算≤2）|Tests，exit0，3716 warnings/0 errors，43.17s；可返回日志62行已由前代理全文读；stdout3114805字符，前3094805截断|
|定向tests|1（预算≤1）|MediaCreationUnavailableTests，4 passed/0 failed/0 skipped，19ms；另有implicit Debug build1；日志61行只读头1–10/尾54–61，stdout1557811字符前1537811截断，不伪造warning总数|
|COM publish|1（预算≤1）|implicit build1；Release/net10.0-windows/win-x64/self-contained/runtime10.0.13/ComAcceptanceSandbox；exit0；日志60行只读头1–12/尾53–60，stdout57543字符前37543截断，不伪造warning总数|
|Windows沙箱|1（预算≤1）|既有单FQN监督120秒/外180秒，1 passed/child OS0|
|Native publish/full|0/0|无NativeAOT/双架构媒体或最终full新认证|

COM包clean/aot.sha256旧fixture为 `6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403`；旧Native仅默认fixture初始化身份，不认证已删除生产链的二进制部署，不绑定本阶段生产源码。日志截断部分未声称已落盘或已读，不补造完整命令回执。本次不执行任何验证命令，以既有原件和文档回读完成收尾。

## 当前可用性、未实现项及下一实际实现
可用：internal MF sample/memory-buffer资源create/write/read/close闭环，限定上述系统版本、x64托管source-link测试证据。不可用：普通MediaPlayer成功创建/播放、独立SourceReader、解码帧接入、Opened/Failed/Ended与seek/rate状态机、媒体时钟及clock lease、音频renderer/同步、完整取消flush/drain恢复、真实WPF/双架构Native集成。现工厂有效请求同步E_NOTIMPL/null，不伪发Opened或静态空帧成功。

下一直接实现SourceReader，而非再申请架构许可或重跑stage1：在现有项目建立专属COM MTA、串行owner，调用mfreadwrite.dll的MFCreateSourceReaderFromURL，先协商真实视频输出媒体类型（subtype、尺寸/stride、帧率及格式变化），实现同步IMFSourceReader::ReadSample（SDK slot9）并处理actual stream、stream flags、100ns timestamp、EOS/stream tick/null sample及错误；用真实文件验证帧内容和时间序列。reader返回sample是独立引用，按作用域释放，不冒用本阶段人工sample为解码成功；提供取消/Flush与在途读取退出，释放sample/reader/attributes后才同owner MFShutdown及COM反初始化，禁止回调内Shutdown。随后适配SystemVideoSample/GeneratedMediaPlayerResource真实帧与lease，再接时钟/事件/seek/rate、音频renderer和同步；完整正向验收前不切工厂默认成功。

所有历史full失败、硬门及身份独立保留，不扣减、不迁移：controlled full x64 1823/1822/1、x86 1823/1821/2；默认latest-contract x64 1823/1821/2、x86 1823/1820/3（executed/passed/failed），以及更早42/23/40等历史失败仍按原报告身份有效。HWbackend/productloss/audio/naturalresources/finalfull均Open；MemoryDescriptiveOnly/memoryGreen=null。阶段资源绿不能关闭这些门。

## 同步收尾
本报告及next-session-handoff、handoffs/current-work-item、handoffs/current-stable-baseline、replacement-issues-and-plan-status、final-acceptance-remaining-matrix、wpfgfx-complete-replacement-validation-plan已实际同步，均以本阶段状态覆盖旧预注册/待许可/资源未实现排程；旧段落仅历史。根执行账补记文档收尾，不新增执行次数，无待同步事项。
