# S-20260930-132：纯色遮罩颜色动画验收

扩展独立CompositionEffectsTests：绑定ColorResource，alpha=0即使RGB为红也应透明；仅更新alpha为NaN，Present失败且目标透明；恢复maskAlpha后准确8位Mask→Scale及浮点HDR像素；解绑颜色动画后更新旧资源为0，绘制仍使用静态maskAlpha。验证颜色/透明度槽独立。

定向及全量回归成功。本轮未修改生产、未发布，使用S-130 DLL。TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。方案已更新。

纯色遮罩内部生成位图，没有外部可锁像素源；资源读取失败和回调路径应随ImageBrush实现，不以NaN参数错误冒充。内部遮罩最终释放计数仍未独立验证，不关闭完整B或DrawBitmap。
