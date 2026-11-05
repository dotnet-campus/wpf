# S-UNDATED-069：PBGRA32目标颜色/裁剪Clear部分实现

原完整模块仍未完成，本轮未交付Compose或前缓冲消费者，无新增外部阻塞，不将Clear独立宣称为大模块闭环。

## 原生证据与修改

- core/sw/swlib/swsurfrt.cpp:247-380：先获取写锁，裁剪后按行填充预乘sRGB，最终释放锁；空颜色不绘制。
- core/common/AliasedClip.h：BOOL IsNull + float LTRB布局。
- core/common/rtutils.cpp:54-127、257-287，rtutils.h:109-125、fix.h：aliased裁剪使用28.4舍入及inclusive top-left。
- include/Generated/wgx_misc.cs:197-203：原生颜色是RGBA；当前托管MilColorF构造参数是ARGB，因此边界显式转换。
- BitmapRenderTargetExports.Clear：PBGRA32真实写锁、颜色转换、有限矩形/空/NaN裁剪、逐行填充、finally释放锁。
- 复用Direct3D9SoftwareRenderTargetSurface.ConvertToSrgb，仅扩大到internal，不新增公开API。
- PRGBA128Float普通颜色/裁剪仍返回E_NOTIMPL；既有全透明无裁剪路径保留。不能声称完整软件栅格器或浮点格式Clear已翻译。

## 验证

新增5项真实COM Clear像素测试：全区域、局部区域、半整数边界、外部空交集、NaN。先在旧DLL失败，再发布后定向全部通过。

Release/win-x64 AOT发布成功；全量106项105通过/1失败/0跳过。唯一失败仍缺WgxConnection_SameThreadPresent。

SHA256：E01F105CF4BC211DC071C74084B986EF599C14DB9DC51E902A7AE26A99B32118。
DLL：artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。

## 未完成与下一轮

本轮只改Clear及对应测试。factory显示集、通用IWGX wrapper、IRenderTargetInternal、manager注册、DrawingContext、Compose与双缓冲前缓冲消费仍未闭环。下轮仍需整体完成消费者和多帧像素/失败/释放验收，不能用本轮目标Clear像素代替前缓冲E2E。阶段4保持Active。
