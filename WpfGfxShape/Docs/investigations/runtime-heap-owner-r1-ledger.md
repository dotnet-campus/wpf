# Runtime heap owner r1 执行账

本轮授权上限 normalbuild4 / 定向tests3 / Native publish3 / COM publish4 / Windows sandbox8，每任务≤900秒。禁止脚本、终端、委派。当前登记normalbuild1/4：COM RuntimeHeapOwnerDiagnostic，win-x64，net10.0-windows，目的验证独立诊断配置及测试编译；余normal3/tests3/Native3/COMpublish4/sandbox8。Native诊断源码随后独立publish编译。

## 预注册诊断

独立默认关闭 WPFGFX_RUNTIME_HEAP_DIAGNOSTIC，仅独立诊断属性启用；不复用生产输出目录。scheduler构造记录scheduler和Thread短弱引用，不记录强引用、不反射私有句柄、不改runtime。默认生产编译不含记录或导出。测试独立配置保留原16轮、65样本、业务硬门、采样与等待。轮末额外诊断顺序：BeforeHostGc→宿主Collect/Wait/Collect→AfterHostGc→DLL导出Collect/Wait/Collect→AfterDllGc。每点记录DLL弱存活数、DLL GC0/1/2、host GC0/1/2、同进程句柄采集器快照与模块路径/hash。只诊断、不输出memoryGreen、不作长期无泄漏证明。短弱引用证明收集后不可达；收集后仍活仅说明该时刻可达，不凭此推具体强根。GC/finalizer无限等待风险由已有role360/父840/外900约束。

先完成诊断完整闭环再评估生产Acquire异常最小回滚、x86 event-owner生产发布及受影响合同。未运行不声称已验证。历史D3F5/15A545证据不转绑新产物。
