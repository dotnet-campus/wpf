# 无画刷 Pen：定向失败诊断

- 对 S-172 发布 #1 的同一 DLL 定向测试 ImageMask 方法：74 通过、1 失败、0 跳过，106ms。
- TRX `3efbe629-6303-4286-b360-09b393ca9952`，start `2026-10-08T19:17:46.9210668+08:00`；DLL SHA256 沿用 S-172 的 `06D06BAEE8977E032659F89FA27BAE09FA093FA84A21A8520667E751277F87AC`。
- 更正初始描述：失败参数 solidGeometry=false、scopedGeometry=false，为 ImageBrush 填充并叠加 Visual ImageBrush 遮罩，不是实色作用域场景。
- 相对 viewport 宽度 1 映射内容 bounds [1,2]，已有相同无 Pen 用例预期 0x40400000；新增 Pen 用例误写 0xffff0000。填充和遮罩各采样约半 alpha，预乘结果 0x40400000。
- 修改新增参数行至相同预期，并加入已有绝对 viewport 映射的无画刷 Pen 对照，仍要求不透明 0xffff0000；不改生产采样与颜色转换。
- 该测试仍覆盖源锁失败首错、层丢弃、解锁恢复、空/NaN 映射与下一帧恢复。
- 在再次测试覆盖 TRX 前保留此摘要，不是完整原始证据副本；未新增发布。
