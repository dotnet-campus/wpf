# 无画刷 Pen：定向验收恢复

- 发布：本项未再次发布；使用 S-172 发布 #1 的 Release/win-x64 生产 Native AOT DLL。
- 修改：仅 ImageMask.cs 的新增无画刷 Pen 参数行，修正相对映射预期为已有无 Pen 对照的 0x40400000，并增加绝对 viewport 不透明红色对照；没有修改生产算法或 ABI。
- 定向：76/76、0 失败/跳过、101ms；退出码 0。可见既有 MSTEST0044、MSTEST0037。
- TRX `377cea53-4a66-422e-bebc-8e028bd3c539`，start `2026-10-08T19:19:05.0062002+08:00`。
- DLL SHA256：`06D06BAEE8977E032659F89FA27BAE09FA093FA84A21A8520667E751277F87AC`，对应 S-172 发布 #1。
- 在全量覆盖默认 TRX 前保存摘要，不是完整 TRX 副本。
- 原生复核：WpfGfx/core/resources/pen.cpp:30-110；core/uce/drawingcontext.cpp:886-994、1009-1138。GetPen 保存各 cap/join/miter、动画厚度和 nullable brush；DrawShape 先填充，再获取 Pen 与可空 stroke brush。前轮生产状态保留及无画刷 Pen 消费已随 S-172 发布验证。
- 有画刷 Pen、Widen、真实私有 realizer 创建者仍未实现；完整模块未关闭，不能以本回归替代完整路径翻译。
