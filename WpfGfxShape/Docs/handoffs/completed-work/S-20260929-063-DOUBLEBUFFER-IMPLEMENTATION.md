# S-20260929-063：双缓冲对象与生产命令部分实现

## 修改

- Abi/SoftwareBitmap.cs：unmanaged对象头及真实IWICBitmap/IWICBitmapSource/IWICBitmapLock vtable；原子引用，读共享/写独占锁，锁持有位图；VirtualAlloc多一页只读guard page，VirtualProtect保护/写锁解保护，最终VirtualFree；支持BGR32、BGRA32、PBGRA32。
- Abi/DoubleBufferedBitmapExports.cs：四个原生导出、IUnknown计数、前后缓冲所有权、空/整图/包含脏区处理与最多五项合并、先弹出脏区再锁前缓冲CopyPixels；BGRA直接预乘为PBGRA。
- Core/GeneratedDoubleBufferedBitmapResource.cs：20字节update和16字节copy布局，接管source已有引用，替换/最终释放；CopyForward无论成功失败都SetEvent/CloseHandle。
- factory/router/table/UCE入队与批次清理：接入DoubleBufferedBitmap资源，检查命令长度；无效资源、partition首错、销毁时释放未消费source和完成句柄。

未修改原WPF或slnx，不增加测试专用生产导出，没有把IWICBitmap冒充IWGXBitmap。

## 原生证据

根目录src/Microsoft.DotNet.Wpf/src/WpfGfx：
core/api/exports.cpp:425-525；core/sw/swlib/doublebufferedbitmap.cpp:120-531；common/scanop/writeprotectedbitmap.cpp:194-339；common/scanop/bitmap.cpp:656-675,1047-1120；include/Generated/wgx_commands.h:458-473；core/resources/doublebufferedbitmapres.cpp:186-294。

## 验证

新增DoubleBufferedBitmapBehaviorTests，直接加载生产DLL：
1. owner释放后back/lock保活、真实GetDataPointer写入；实现前编写及定向执行，旧DLL缺创建导出共同前置Red。
2. 真实创建/写锁/标脏/保护→connection/channel/resource/update/copy→重复事件完成；实现后补充，不能称为实现前行为Red。此测试没有前缓冲像素断言，不等于端到端像素验收。

源码Release/win-x64 build成功；修复过一个CS0136局部变量冲突。最终增量build 0错误/0警告，未重新报告全量警告，不表示既有833警告已解决。
未publish。旧DLL全量77项：52通过、25失败、0跳过。TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，run b32a929e-6ae3-49de-a0f9-f3daba0e6177。新增测试未进入行为断言，无新DLL Green。

## 明确未完成

前缓冲生产消费者/像素结果、IWGX私有接口、非32位格式/索引palette/WIC转换、完整bitmap HRESULT和锁/保护行为差分、多帧与低资源失败仍待完成。BGRA预乘舍入未经WIC差分验证。SetEvent/CloseHandle失败目前未诊断记录。没有完成Present生产生命周期。

本轮已实际增加双缓冲生产代码，不再是仅通道修复，但模块仍未完成，不能替换wpfgfx。继续current-work-item.md中的完整闭环，不把此部分实现或事件通知当成像素验收。
