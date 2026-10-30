# 原 Present 原始 ETL 第一阶段：执行前缩范围登记

本文件先于本轮源码编辑创建。当前用户新授权覆盖上一轮“编译前停止/显示诊断结束”的执行状态，但不改写旧 closeout 或把旧静态阻碍当 ETW 不可行。上轮源码/build/沙箱均0。

## 本轮合同

只实现默认关闭 OriginalPresentSystemTrace partial、VisualCache 最窄 scope、SwapChainObservation 原 Present 前后 QPC、非默认 OriginalPresentSystemTraceDiagnostic 配置及必要资源。仅采 Microsoft-Windows-D3D9（783aca0a-790e-4d7f-8451-aa850511c6b9），不启用 DxgKrnl/DWM，不声称完整 trace 认证。原始文件不是呈现成功、目标事件观察、合成或 scanout 证明；不实现 OpenTrace/ProcessTrace/TDH/事件关联/decoder 负控。

官方本机 SDK 10.0.26100.0 shared/evntrace.h 1238–1265、1731–1735、1893–1898、2016–2026及shared/wmistr.h 30–57已读。WNODE_HEADER 使用两个8字节 union 最大成员，自然Pack8；TRACEHANDLE/CONTROLTRACE_ID是64位，与指针宽度无关；LoggerThreadId为nint。预期 x86 properties120、LoggerThreadId104、LogFileNameOffset108、LoggerNameOffset112，x64 properties120、offset104/112/116；新增运行前布局守卫，但本轮不冒运行认证。

keyword/level 正式来源：本机 Windows Performance Toolkit/gpuview/log.wprp 504–529亲读，D3D9 GUID/Level6/keyword0xf；仅读取官方配置，绝不执行其脚本/WPR。采用该稍宽原始采集mask，不猜事件ID、不用filter数组。PresentMon D3D9生成头本轮网页抓取另证Present keyword8000000000000002/L0，但不是运行机schema，也不是本轮采用的过滤合同。EnableTraceEx2 Microsoft Learn正文已取得，确认level/keyword/timeout/NULL参数语义。

会话唯一随机名与独有文件，自有Start成功才有Stop所有权；仅按所得64位handle停止，不按名称停止他人，失败不重试。64KiB缓冲、min2/max8（OS可能按CPU调整）、顺序文件16MiB配置上限，2秒一次性watchdog请求Stop，原Present返回立即同步Stop。无消费线程/异步decoder；watchdog不调用渲染、不取消/改变原Present。计时回调调度和Stop API自身不承诺硬截止，独立监督仍需后续授权运行提供。权限仅临时SeSystemProfile，保存准确previous并恢复；失败保存真实API结果，不改ACL/不提升其他权限。

本轮预算：normal COM build恰好1，参数 targetPath=WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，configuration=OriginalPresentSystemTraceDiagnostic，runtimeIdentifier=win-x86，targetFramework=net10.0-windows。publish/沙箱/测试/发现/Native/外部CLI/终端/脚本/再委派全部0。编译前静态检查，构建失败不修改源码重试，不关闭分析器。成功只申请另授权publish1/沙箱1。

## 阅读与工具边界

已读最新decision 1–44（返回可见至44）、43–74（字符截断）、76–104；closeout1–55完整；恢复入口1–43、current-work-item1–47、plan1–45均字符截断，仅读取当前头部；execution-rules27行与continuity-recovery-and-closeout66行完整；baseline1–18。旧HP Control读取1–60（字符截断）、80–132、180–235、300–330，未冒全文审核或复制其x64消费者。源码VisualCache已读1–56（截断）、55–83、84–125；本轮只改已读原Present附近。SwapChainObservation47行完整；csproj1–60截断后55–76补读；resx17行完整；global.json35行完整。AGENTS名称查询无命中。一次把源码文件当搜索目录返回目录不存在，后直接读原范围纠正；一次C:/Windows/System32/winevt/Manifests查询不存在，不据此推导系统能力不足。工具读写参数均为文件/范围或查询，真实结果无执行exit；没有隐藏终端调用。

## 当前进度

执行前登记Completed；Implementation静态完成；ABI/cleanup/serialization静态审查Completed；唯一Build=Passed（1/1，exit0/134 warnings/0 errors/5.94秒）；Publish/Sandbox/Test=NotRun。本轮尚无新task/ETL/运行身份。原最终矩阵及完整替换Open，历史双架构有限绿保留、不移绑。

静态审查：collector356行，WNODE48/properties120，x86 HANDLE4故两个offset108/112，尾部Pack8补齐到120；x64为112/116；控制句柄ulong、BOOL MarshalAs.Bool、LUID8/TOKEN_PRIVILEGES16。HGlobal全零、字符串含NUL、总尺寸checked、finally释放；token实有句柄Close，GetCurrentProcess伪句柄不Close；只调整一个权限，用previous精确恢复，Count0无需改回；恢复失败即请求自有Stop，BOOL/LastError独立保存。Start失败owned=false不ControlStop；Stop仅一次按所得handle+NULL名称，锁串行watchdog和finally，回调disposed检查避免访问释放内存。Stop非零不冒清理成功、统计有StatisticsValid标志；不重试不按名找他人。

DTO逐字段审查：仅string/bool/byte/int/uint/long/nullable原语/Guid和封闭record列表；nint窗口/device/loggerThread全部字符串，session亦字符串，不传native struct至JSON。SchemaVersion1，unknown以nullable表示，Evidence明确边界；同一DTO在Start前序列化自检、setup.json预落盘；摘要失败独立Error后仍写controller，写盘失败向stderr输出API/error，stderr失败另Debug记录，不覆盖业务异常。未运行自检，不称执行通过。阶段QPC与原Present before/after/result/PID/TID是JSON内标记，不宣称ETL内有marker或目标事件已观察。result作用域及System.Threading.Timer限定已于build前静态修正，无编译重试。

修改工具记录：两次WriteFileContent创建登记档及collector均OK；四文件MultiReplace为4成功0失败；构建前静态MultiReplace为9成功0失败。collector回读1–84、96–162、252–323有4000字符截断，另回读78–100、145–180、176–251、323–356及hook/config。一次登记档替换因新文件未先读被工具拒绝，补读23行后再替换，无源码执行。完整写入参数可见，不把截断返回称完整回读。

## 唯一构建实际结果与终态

已调用一次normal run_build，完整参数即本文件合同中的四项，无额外参数/关闭分析器；真实exit0，134 warnings、0 errors，5.94秒。工具返回成功及64行日志，随后read_last_log_lines(1,35)、(36,64)回读全部可见存储日志。标准输出总106029字符，工具只保存尾20000，前86029不可见；不能恢复或假称完整stdout归档。可见warning有MSTEST0037/0045/0044/0017、CA2007/CA2014，含原VisualCache循环stackalloc；截断前警告来源不能逐项审定，不声称本轮新增warning=0。没有编译后源码修正或第二次build。机器记录落ignored artifacts/original-present-raw-etl-stage1/build-result.json，记录完整工具参数、exit/count/time/截断及执行账。

已实际交付5个源码/配置/资源文件：新增D3DImageTests.OriginalPresentSystemTrace.cs（356行），修改VisualCache.cs、SwapChainObservation.cs、WpfGfxShape.ComAcceptance.csproj、AcceptanceResources.resx。默认Debug/Release不定义ORIGINAL_PRESENT_SYSTEM_TRACE；未构建默认配置，不能把静态隔离称默认回归通过。scope只覆盖原PresentNativeControl，Stop在原slot17/121返回finally内、在原Assert/Complete及Diagnose/shader矩阵之前；落盘则在原Complete之后scope退出，原参数/Clear/断言/等待/线程/入口/公共捕获/SDK/TFM及生产不改。

运行边界：代码已编译，不等于x86原生API布局/权限/Stop/lost/序列化运行认证；2秒Timer使用线程池且与Start/Enable持锁串行，Stop可能受调度或API阻塞延后，明确非硬deadline。文件16MiB为ETW配置上限，不冒实际文件大小认证；min/max缓冲为请求值，OS可能调整。仅D3D9稍宽mask，无PID事件过滤（原ETL可能含其他进程，不能视全文件都是目标），无DxgKrnl/解码/关联，不认全trace、目标提交/呈现成功。原始QPC marker在controller JSON，不是ETL EventSource控制事件；collector从Clear之后、Present之前开启，Clear未纳入ETL。失败路径保留API原码和独立Errors，不自动重试或提其他权限。

当前Implementation=Completed.Static/Build=Passed；Publish/Sandbox/Test=NotRun，Readback/Closeout=Completed。新增build1，publish0/沙箱0/测试0/Native0/发现0/终端0/脚本0/再委派0；无新task、ETL、TRX、运行SHA/MVID或actualNative身份。旧HQ/HP及双架构有限绿保留，不转绑；原最终矩阵/完整替换Open、内容根因Unknown。

唯一下一动作：申请另授权COM publish1、Windows沙箱1（原candidate x86精确FQN WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive体1），复用既有native，正式同exe/新clean包核验，采本阶段D3D9原ETL并回读controller/API/Stop/恢复/lost/原业务失败；不自动执行，不扩provider或decoder，不重发Native，不额外build/发现/official/x64/full/重试。沿既有120秒监督/180秒任务边界，发布与沙箱参数须在实际授权时完整登记。
