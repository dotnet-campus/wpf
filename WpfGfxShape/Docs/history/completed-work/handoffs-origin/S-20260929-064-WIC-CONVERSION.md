# S-20260929-064：WIC转换及非32位布局（部分缺口修复）

## 修改

BitmapFormatConverter使用原生CLSID_WICImagingFactoryWPF及IWICImagingFactory创建格式转换器，Initialize后持有到双缓冲销毁。DoubleBufferedBitmapExports按MilPixelFormatInfo.HasAlphaChannel选前缓冲格式，CopyForward改走转换器CopyPixels，删除手写BGRA预乘。初始化失败与最终释放均释放converter，converter保有的source引用不提前失效。

SoftwareBitmap按MIL GUID前15字节及末字节范围识别格式；接受现有bits-per-pixel表中MIL范围字节对齐非索引格式。DWORD对齐stride，buffer size为stride*(height-1)+rowBytes；CopyPixels和锁按真实bytes-per-pixel寻址。不接受索引/子字节格式，未虚构palette实现。

## 原生证据

src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wincodec_private.h:60-151（工厂CLSID与GUID映射）；core/sw/swlib/doublebufferedbitmap.cpp:138-192（工厂/converter/前缓冲格式）；common/shared/pixelformatutils.cpp:820-855（buffer size）；core/api/exports.cpp:50-101,140及core/api/api_factory.cpp的render-target入口定位（尚未实现消费者）。

## 验证

新增BGR24、Gray8、Gray16行填充/紧密CopyPixels测试3项，测试直接加载生产DLL并初始化COM apartment。测试写于生产改动之后，不声称TDD行为Red。
Release/win-x64 build成功，未publish；旧DLL全量80项52通过、28失败、0跳过。新增测试缺双缓冲导出，未进入行为断言。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，run 010350af-12d9-410a-9ffe-d277923c3615。
没有新DLL身份/运行证据，不声称WIC路径已运行通过。

## 尚未补齐

索引palette、1/2/4位位偏移、IWGX身份、前缓冲生产消费者、准确像素和多帧失败生命周期验收均未完成。COM工厂未注册/未初始化等HRESULT直接回传，不隐式CoInitialize改变调用方apartment。没有用测试getter替代消费者，也未把真实WIC接线等同于完整格式支持。

用户要求的全部剩余缺口尚未关闭，整体模块不完成。下一动作仍为current-work-item.md完整生产闭环。
