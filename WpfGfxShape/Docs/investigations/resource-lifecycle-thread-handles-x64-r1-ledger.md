# Thread handles x64 r1 本次独立执行账

## 授权与静态冻结

本次授权修正阶段A CS1597并追加normalCOMbuild1；成功才继续COMpublish1及唯一沙箱1，任一阶段失败即停，不修后重试。历史stageA build exit1/0warnings/1error独立保留，不冲销。

实际源码修正仅ProcessHandleTypeSnapshot.cs EndQpc自动属性尾分号。趋势hook和纯分类tests只读未改。生产/配置/NativeRun零改，Native0，无终端脚本、无再委派。未执行或发现额外纯分类tests。

静态核：同一SAME_ACCESS自有duplicate，仅type==Thread调用GetThreadId→WaitForSingleObject(0)→GetExitCodeThread；finally仅一次CloseHandle自有duplicate，无原句柄关闭、不等待Event。即时保存失败error再QPC。259+signaled退出，timeout仅观察未终止，wait失败/缺失Unknown；退出查询错误保留Failed及关联Partial。非原子/TID和handle复用歧义保留。DTO仅数值/字符串/数组/记录/字典；CallbackCandidate不持业务/COM对象。hook仅同步投影已有CallbackThread为uint数组，不新增持续强根。callback只称释放worker候选，非scheduler start-end。相邻属性/Win32声明/DTO/纯分类tests已静态读取；分类tests未运行，不声称动态验证。

固定10点/role、原65samples、16轮4+12、无强制GC、DescriptiveOnly、round20/role360/父840/外900不变。生产Uce Acquire异常未修。PendingInvestigate/MemoryDescriptiveOnly/memoryGreen=null/WorkerExitNotProven/full Open不变。

## normalCOMbuild1

已调用1/1，真实exit0、145warnings、0errors，MSBuild elapsed 00:00:08.17，总墙钟Unknown。原工具返回：执行成功。完整日志共 64 行，可使用 read_last_log_lines 按行读取。已亲读日志1–32及33–64；原stdout114245字符，前94245字符工具截断，仅末20000字符可见，不声称完整原stdout已保存。关键原返回：4: 退出码: 0；5: 结果: 成功；60: 145 个警告；61: 0 个错误；63: 已用时间 00:00:08.17。源码自此冻结，不改分析器/NoWarn、不修警告重试。参数：targetPath=WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，configuration=ResourceLifecycleHandleTypesDiagnostic，runtimeIdentifier=win-x64，targetFramework=net10.0-windows。调用后即时追加真实返回，成功冻结源码；失败停止所有后续执行。

COMpublish预登记1/1（含隐式编译流程1另计），沙箱0/1，Native0，额外test/发现0。未产生新TRX/identity/动态句柄数据。

发布来源：此前handle-types-x64-r1/clean/Native/wpfgfx_cor3.dll目录精确存在，clean/aot.sha256亲读D3F5E40D2ABC0782D678C843537210EC203987A74C84616E20164703B3895B82；旧00/02发布账及正式报告绑定该clean与actual D3F5。本次采用该验收clean副本，不要求失踪原native路径。

唯一发布参数：dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c ResourceLifecycleHandleTypesDiagnostic -r win-x64 -f net10.0-windows --self-contained true -p:RuntimeFrameworkVersion=10.0.13 -p:WindowsDesktopRuntimeFrameworkVersion=10.0.13 -p:TargetLatestRuntimePatch=false -p:ComAcceptanceSandbox=true -p:AotRuntimeIdentifier=win-x64 -p:AotDllPath=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/resource-lifecycle-handle-types-x64-r1/clean/Native/wpfgfx_cor3.dll -p:PublishDir=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/resource-lifecycle-thread-handles-x64-r1/publish/ -p:ComAcceptancePackageDirectory=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/resource-lifecycle-thread-handles-x64-r1/clean/

