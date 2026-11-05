# MilGlyphRun_ReleasePathGeometryData：有限释放合同通过

最终同SHA627F2732…、测试身份与原始证据见[集成档](Four-exports-integration-closeout.md)。最新30次outline真实调用后，10次返回非空geometry均配对Release，HResult全部0；30次ReleaseNull全部0；DWrite face/factory等引用逆序释放，测试进程完成生命周期。

这是非空配对/null安全的有限通过，不宣称重复释放非法指针安全、OOM无泄漏、全部字体并发寿命或NativeAOT unload支持。outline raw差分及padding裁决[独立保持待定](Four-glyph-outline-closeout.md)，不能以release成功覆盖。没有新增生产或改变strict规则，验证后不再改源码。
