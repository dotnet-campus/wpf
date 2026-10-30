# 阶段A源码冻结 / build前复核

已先落盘并回读变更合同。源码现冻结，之后不再修改源码/配置：
- ProcessHandleTypeSnapshot.cs：同一owned duplicate，仅Thread增加TID→zero wait→exit查询，每次即时last-error及QPC；finally关闭记账。schema2、数字handle、distinctTID/每TID handle数、逐轮候选路径/匹配round、非原子及复用歧义、关联Partial与新增query秒数。
- UceChannelTests.ResourceLifecycleTrend.cs：仅10固定点传已有state.CallbackThread数组，索引为原轮次；当前轮在states未加入时append一次。原samples不改。QPC/pid/thread复核来自各round callbacks.jsonl，business.json保留callbackThread；关联时不新增文件读取扰动。
- ProcessHandleTypeSnapshotTests.cs：新增纯分类/错误/DTO自检（含259两种wait、WAIT_FAILED、TID0、未知权限、50不是权限、transition、重复TID和numeric handle）。只编译不执行。

静态检查：本次无Dispose方法改动；既有AsyncSharedOwnership逐资源清理独立收集异常、primary.Throw优先保留，未改变。没有native指针被新lambda捕获：ObserveThread仅同步使用duplicate，InBudget只闭包snapshot/start；DTO没有IntPtr/delegate。finally不新增throw、不抑制CA2219或其他分析器。Dictionary<uint,int>为System.Text.Json支持的数字key，已加序列化自检但尚未执行。未声称动态验证API权限/关闭/序列化通过。

唯一待执行normalCOMbuild1：targetPath=WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj；configuration=ResourceLifecycleHandleTypesDiagnostic；runtimeIdentifier=win-x64；targetFramework=net10.0-windows。失败即停不修后再试，成功也停；test/publish/Native/沙箱0。后续仅真实日志账及正式收尾/恢复文档更新。
