# 双缓冲位图模块：生产入口 Red 与跨模块阻塞（未完成）

## 结论

未实现生产双缓冲位图，模块保持未完成。本轮完成原生依赖核查、可执行入口前置测试和发布验证，不能作为模块Green或替换资格。按工作计划的明确技术阻塞条款停止扩大到完整UCE/compositor，不以内部调用、测试getter或仅后缓冲实现降低验收标准。

## 具体证据

- core/resources/doublebufferedbitmapres.cpp:137-170：GetBitmapSource通过内部资源访问前缓冲或格式转换后缓冲，不是公开COM取前缓冲槽。
- 同文件ProcessUpdate接管命令携带的CSwDoubleBufferedBitmap引用（发送方已AddRef），直接reinterpret_cast为C++类；不能把GCHandle或新对象裸指针交给旧wpfgfx消费者。
- 同文件ProcessCopyForward检查source和UseBackBuffer，调用CopyForwardDirtyRects；无论成功失败SetEvent并CloseHandle，必须接管重复完成句柄。
- core/uce/generated_process_message.inl:1849-1910：两种命令从CMilSlaveHandleTable查找TYPE_DOUBLEBUFFEREDBITMAP，再调用资源方法。
- core/uce/apifunc.cpp:129-141：WgxConnection_SameThreadPresent调用CMilConnection::PresentAllPartitions，并非孤立执行一个复制函数。
- 同文件802-859：MilPlayer_Create/Process只在PRERELEASE启用；普通分支E_NOTIMPL，不能作为绕过channel的通用生产测试入口。
- core/sw/swlib/doublebufferedbitmap.cpp:110-219：依赖CWriteProtectedBitmap、CWGXWrapperBitmap、CSystemMemoryBitmap及可能的WIC格式转换。前缓冲格式根据alpha选择PBGRA32/BGR32，不能只实现固定32位后缓冲声称全格式。
- 同文件380-493：超过5个脏区合并；复制逐项弹出脏区、锁前缓冲、经可选格式转换CopyPixels、逆序清理。
- 当前Code内导出定义仅EventProxy、ManagedStream与ABI probe；无connection/channel/resource/target生产导出。DoubleBufferedBitmap只有编号与消费者类型判断，无具体资源实现；SameThreadPresent也无实现命中。

## 必须补齐的真实依赖

创建/断开connection → 创建/销毁channel → 创建/释放资源handle → SendCommand/CloseBatch/Commit → 同步composition处理 → 双缓冲资源CopyForward → image消费者/可读目标观察前缓冲。

这至少涉及阶段5 UCE生产入口/生命周期与阶段6消费者出口，不能视作四个bitmap导出的薄接线。具体前缓冲观察出口仍须核对factory/target/renderer实际调用链，未在本轮证明可用，不凭空指定一个getter。

## 已运行验证

新增唯一测试项目内DoubleBufferedBitmapTests.cs：14个原生wpfgfx.def已有导出的存在性前置测试，包括四个bitmap导出与10个connection/channel/resource提交路径入口。不添加导出占位，不跳过测试。入口全部存在也不等于行为合格，后续必须增加像素和生命周期断言。

当前源码Release/win-x64增量AOT发布成功。全量66项：52通过、14失败、0跳过；失败均为所需生产入口不存在。没有52个既有回归退化，没有生产代码修改。测试套件当前明确为Red。

SHA256：9A1248D757EAB6E3E44D00D75A0FDDBD53383605E75BA6BCD41025250F215148。
TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx；run ID 5f57cc41-aa77-4526-8154-fd62534db996。旧产物哈希虽相同，本轮重新发布并由测试核实。

## 后续恢复

保留原双缓冲模块目标，将最小同线程UCE提交与可观察消费者链作为整体前置纳入依赖评估；禁止把本次定位/入口计数认作用户要求的大模块交付。下一步须先解决生产可达性而非继续增加出口名测试。阶段4仍Active，其余阶段不变。
