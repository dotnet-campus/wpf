# S-20261008-184：真实空双缓冲资源 Red

在S-183本轮发布#1（SHA256 `5B8168E2FF94EEAAAAD833F7796620466BAB0299C92E97A52A6C67C3C2D1EB34`）上扩展现有clearSource恢复链，真实通道创建95/96类型资源，分别绑定未初始化BitmapSource/DoubleBufferedBitmap；不伪造对象、无测试导出。

定向filter `FullyQualifiedName~WhenImageMaskSourceIsLockedThenLayerIsDiscardedAndRetryPreservesMapping`：77通过/3失败，0跳过，111ms。三项绝对/相对/192DPI作用域几何在双缓冲资源Present处返回E_NOTIMPL（0x80004001），应为S_OK透明。TRX `244af144-580e-4fce-82a2-ac30937d9448`，开始 `2026-10-08T19:37:16.6430695+08:00`。日志可见既有MSTEST0044/MSTEST0037警告。

原生证据：imagebrush.cpp:112–152在mapping前检查ImageSource.HasContent；doublebufferedbitmapres.h:38–41以m_pDoubleBufferedBitmap非NULL判定。GeneratedImageMask当前仅识别空BitmapSource，兄弟资源遗漏相同合同。本文在覆盖下一次TRX前保存摘要，非完整TRX副本。