# ManagedStreamWrapper 生产 COM 导出与 AOT 验收

## 原生合同

读取 core/api/exports.cpp:531-890（descriptor、wrapper、创建）、MILIStreamWrite；common/shared/milcom.cpp:20-105（QI/计数）；include/InternalGUIDs.h:64-66、wincodec_private.h:159-169。

支持 IUnknown/IStream/IManagedStream；不支持 ISequentialStream QI。QI 空输出 E_INVALIDARG，未知 IID 清空输出。Descriptor 按值复制，最终引用释放调用 Dispose。Write/Seek/CopyTo 提供可空输出临时存储，Read 不作同类替换。

## TDD 与生产差集

先编写 ManagedStreamTests，共 17 项展开测试。实现前运行：旧生产 DLL SHA256 E52D24672DC6C1146B5C37EB628A706F1C81DF485EEC97AC96CC8C4DEB07A86B 缺 MILCreateStreamFromStreamDescriptor，初始化共同前置 Red；不声称逐行为断言先失败，不声称本轮重新发布过实现前源码。

编码前登记映射及 EVID-MANAGEDSTREAM-COM。新增 Abi/ManagedStreamExports.cs：稳定 unmanaged 对象内嵌 descriptor、进程生命周期16槽 vtable、原子计数、13个委托方法、两个真实导出 MILCreateStreamFromStreamDescriptor/MILIStreamWrite。使用单一对象地址统一 IUnknown identity（不复制 C++ 多继承子对象布局）；所有权与 QI 合同保持。HRESULT 槽设置托管异常屏障；调用方 unmanaged callback 不得抛出托管异常。无效函数指针、释放后裸指针和异常 dispose 不作为受支持输入。

## 验证

Release/win-x64 Native AOT Shared 发布成功；ManagedStreamTests 定向成功；唯一 ComAcceptance 项目全量45通过、0失败、0跳过，130ms，其中 EventProxy28项与流17项。没有修改既有 EventProxy 测试。

DLL：WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

Green SHA256：9A1248D757EAB6E3E44D00D75A0FDDBD53383605E75BA6BCD41025250F215148。

标准 TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx；run ID 74e05c2d-03ca-4a99-aba6-5b3b836b62b1。成功用例记录路径和哈希；后续会覆盖同名 TRX，本归档保存本轮身份。

覆盖创建失败所有权、QI/refcount/identity、descriptor 副本、全部13槽 S_OK/S_FALSE/E_FAIL 原样传递、64位偏移与长度、指针/输出、可空输出、额外引用与最终dispose、MILIStreamWrite真实vtable转发。

## 未覆盖

Clone 当前仅验证回调输出转发（回调返回 null），没有真实克隆对象；CopyTo 当前验证参数和计数转发，未执行双流复制。尚未验证完整 STATSTG 内容、真实 System.IO.Stream 来源、跨线程/跨 apartment、低资源、原生差分、进程退出、其他架构。无 Host、无新测试项目、无 slnx 修改。exports.cpp 文件族仍 Partial，阶段4 Active，不能替换 wpfgfx。

下一轮完成双生产流读写/定位/CopyTo/真实Clone/释放端到端闭环，继续先测试后必要修复。
