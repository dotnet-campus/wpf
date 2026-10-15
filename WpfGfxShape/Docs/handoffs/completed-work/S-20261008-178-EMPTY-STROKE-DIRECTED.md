# S-20261008-178：发布#2定向验证

发布#2见177归档；没有再次发布或修改生产/测试。

- 按ComAcceptance csproj，filter `FullyQualifiedName~WhenImageMaskSourceIsLockedThenLayerIsDiscardedAndRetryPreservesMapping`：80/80，0失败/跳过，102ms，退出码0。
- TRX `aa41bbd9-9be4-4a63-a1b8-d88b4bd262c4`，开始 `2026-10-08T19:25:59.5043078+08:00`。
- 四个Red场景全部转Green，包含ImageBrush与作用域实色、零厚度/float下溢/动画覆盖、锁失败首错及下一帧像素恢复。
- 此定向方法不输出DLL SHA，待后续全量从测试输出取得。同一次发布#2产物路径见177。
- 在全量覆盖默认TRX前保存本摘要，不是完整原始日志/TRX副本。
- 不宣称原生私有realizer effects路径已实现，完整模块仍未关闭。
