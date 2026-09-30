# S-20260929-062：UCE COM释放重入修复（大模块未完成）

## 修改事实

- UceChannelExports.DestroyChannel先移除ABI身份，阻止销毁期间再次查找到通道。
- SameThreadChannel.Commit拒绝递归提交；正在处理批次时Destroy只标记销毁，批次尾部不再执行生产命令但逐包释放引用，退出活动批次后才释放资源表。
- 清理开放命令时先从集合移除再调用外部COM Release，避免重入修改遍历集合。
- 分发和OOM错误不覆盖回调期间已记录的partition首错。
- GeneratedBitmapSourceResource.ProcessSource的通知与transport Release使用嵌套finally，NotifyChanged分配失败也不会漏掉发送方引用。

这是现有生产实现的生命周期缺陷修复，不是完整双缓冲模块交付。

## 测试

唯一ComAcceptance增加3项：释放回调重入Destroy、重入Commit、外部Destroy时释放回调再次Destroy。使用真实生产EventProxy及其descriptor dispose callback，不伪造COM位图、不调用内部托管实现。

前两项先于修复编写并执行，旧DLL缺WgxConnection_Create，共同前置Red；没有实际复现方法体断言Red。第三项在修复后补充。新源码尚未发布，不报告行为Green。

Release/win-x64构建：0错误、833警告。
旧DLL全量：75项，52通过、23失败、0跳过；14个既有入口前置及9项UCE缺连接导出。
TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，run 4f984a7e-be3e-458f-8cce-b98eab0b0ff8。
未运行publish，没有新DLL或新SHA256证明。Ledger保持Pending。

## 本轮继续核实的双缓冲原生合同

证据根为src/Microsoft.DotNet.Wpf/src/WpfGfx：
- core/api/exports.cpp:425-525：创建格式GUID、后缓冲引用/大小、MILRect转无符号坐标及保护入口。
- core/sw/swlib/doublebufferedbitmap.cpp:340-531：脏区空区域无操作、整图替换、包含去重、达到5项合并；复制先弹出脏区、锁前缓冲、获取stride/data、可选转换CopyPixels、逐次释放。Protect不能是成功空壳。
- common/scanop/writeprotectedbitmap.cpp:194-339：DWORD对齐、索引格式palette要求、VirtualAlloc多分配guard page、VirtualProtect真实保护。DBG/Release写保护时序存在差异，仍需完整锁合同对照，不能自行假定位图锁行为。

## 未完成

本轮未新增双缓冲对象、四导出或复制消费者。生产对象/锁/保护、格式转换、CopyForward资源及事件所有权、像素消费者仍是内部待实现工作，不是发布限制造成的阻塞。整体模块与阶段不关闭。
下一轮继续current-work-item.md中的完整闭环，不把本次修复或纯补测作为模块完成。
