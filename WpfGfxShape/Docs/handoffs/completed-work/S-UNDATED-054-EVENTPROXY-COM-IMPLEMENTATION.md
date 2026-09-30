# EventProxy 生产导出实现（未取得发布 Green）

先新增 callback 内 QI/平衡 Release 测试，运行现有 AOT 产物：24 项全部因缺 MILCreateEventProxy 在初始化失败。该产物不是本轮发布，不能代表当前源码新发布 Red。

新增 Abi/EventProxyExports.cs：真实 MILCreateEventProxy、稳定 unmanaged 对象头、类型关联生命周期的四槽 Stdcall vtable、QI identity/未知 IID 清空输出、AddRef/Release/RaiseEvent。对象头 GCHandle 保活现有 EventProxy，计数方法直接使用现有计数；最终释放由 EventProxy 统一回收 descriptor、GCHandle、对象头。原始 uint 长度入口避免 Span 长度截断。HRESULT 方法在 ABI 层封锁异常为错误码，计数方法无法表达 HRESULT 时 fail-fast，不伪造成功。

原生证据 core/api/exports.cpp:895-910、core/av/eventproxy.cpp:209-267、include/wgx_render.h:82。生产 Ledger 与映射在编码前登记，EVID-EVENTPROXY-COM-EXPORT 保持 Pending。

实际验证：生产 Release build 成功，830 警告、0 错误，日志含 CA1416；测试项目 Debug build 成功。本轮没有执行新 AOT 发布，没有发布 Green，不把 build DLL 替换到测试发布目录，也没有内部替身验收。

偏差：原生 count Interlocked 由已有 EventProxy monitor 计数承载；无效 descriptor 函数指针沿用内部参数校验；RaiseEvent 内临时引用保护沿用已有实现。进程退出 SEH、并发压力、低资源及当前源码新发布 Red/Green 未验收。GCHandle 仅作为新 COM 对象状态句柄，不传给旧 CMilSlaveVideo 调用方。

未新增 Host、测试项目或 slnx 修改。下一任务是发布本实现并运行全部 24 项，根据实际失败先测试后修复，完成真正 Green；不能宣称完整 COM 或媒体模块关闭。
