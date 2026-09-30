# S-20260930-131：纯色遮罩动画更新和失败恢复

扩展独立CompositionEffectsTests的8位/浮点遮罩场景：绑定opacity DoubleResource=0，静态值1被覆盖；仅更新动画为NaN，Present明确失败且目标仍透明；仅更新为1恢复Mask→Scale准确像素。沿用后续解绑Visual mask和PushOpacityMask/Pop恢复断言。

定向及全量回归成功；本轮仅修改测试和方案，未改生产、未发布，使用S-130 DLL。TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。

这是参数捕获失败，不是遮罩像素读取失败；颜色动画及最终资源释放计数尚未独立验收。方案B继续开放，ImageBrush属于后续C。没有新增测试导出或fake COM。
