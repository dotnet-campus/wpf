# S-20260929-065：索引palette/子字节格式及IWGX适配

## 源码实现

- BitmapFormatConverter.ClonePalette创建真实WIC palette并InitializeFromPalette，位图持有独立副本，不持有调用方可修改palette作为状态。SetPalette失败释放旧palette，最终释放及初始化失败清理副本。
- SoftwareBitmap接受索引1/2/4/8位和BlackWhite/Gray2/Gray4；按位宽算DWORD stride和末行长度，CopyPixels以MSB优先对齐矩形内容。
- 非对齐锁使用DWORD对齐临时缓冲，Read初始化像素，Write最终Release仅写回锁定区域位，保留相邻像素。字节对齐锁保留直接指针访问；读共享、写独占、lock保活不变。
- SoftwareBitmap.Mil.cs：独立IWGXBitmapSource/Bitmap和IWGXBitmapLock接口地址、独立枚举格式槽，WIC继续返回GUID；QI回到同一个IUnknown，共用计数与生命周期。
- 实现15槽IWGXBitmap表及7槽锁表。无原生resource-cache注册时按原生分支不保存局部dirty列表，uniqueness变化返回false要求全量更新，相同token返回true/空列表。没有伪造缓存注册，也不意味着完整CMILResourceCache支持。

## 原生证据

根目录src/Microsoft.DotNet.Wpf/src/WpfGfx：include/wgx_render.h:93-99,879-994；common/scanop/bitmap.cpp:80-193,237-314,555-655,1399-1542；common/scanop/systembitmap.cpp:140-200。palette拷贝与失效顺序、非对齐lock临时数据和最终写回均依据这些原生合同。

## 测试

新增PackedBitmapTests六项参数化用例：索引1/2/4位调色板快照、非对齐两行写锁及邻居保留，IWGX枚举格式/QI统一身份/锁转WIC；BlackWhite/Gray2/Gray4非对齐CopyPixels与dirty token状态。
前三项在生产修改前编写并执行；旧DLL缺双缓冲创建导出共同前置Red。后三项在实现后补充。没有fake COM，palette来自真实WIC，位图来自被测DLL。

Release/win-x64 build成功。唯一测试项目全量86：52通过、34失败、0跳过，六项新测试均缺MILSwDoubleBufferedBitmapCreate，未进入行为断言。未publish，未获得新AOT行为Green。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，run 74be0433-1c62-4425-a717-fb0e4a3d566c。

## 边界

两项请求的生产源码路径已实现，尚未真实DLL验收，不能称为完全验证。并发palette/元数据变更、低资源注入和完整原生缓存容器不在本次验收范围。前缓冲生产消费者仍未接通，整体双缓冲闭环保持未完成。下一动作见current-work-item.md。
