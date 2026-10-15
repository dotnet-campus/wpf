# S-20261008-202：private bitmap fill 接通，完整模块仍未关闭

## 原生依据与生产差集

读取 WpfGfx/core/api/api_brush.h:212-460（分段）、api_basebrushes.h:1-140、api_brush.cpp:355-448、common/shared/ResourceCache.h:125-169、milcom.h:65-110，以及 core/sw/swlib/bilinearspan.cpp:300-383。bitmap getter 非虚；sampling space 决定矩阵与 source parallelogram 是否乘 world-to-device；Extend 钳位边缘，不等于透明边界。

FillInternalShape 对 BrushBitmap 接入 FillInternalBitmapBrush：Release/x64 相对 secondary CMILBrush 的字段读取、sample/world 映射、四点 source clip 与真实 shape coverage 的 Intersect、Extend/Tile/FlipX/FlipY/FlipXY、opacity、MIL source AddRef/Release、共同转换/采样/合成。SoftwareImageDrawingContext 增加默认关闭的 ExtendThroughGeometry 和默认1的 AlphaScale；32bpp 与 float 采样均消费透明度，普通 DrawBitmap 默认行为保持不变。保留 realization→brush→effects 首错和外围 target 保活/锁逆序释放。修正旧测试 Extend 纹理外透明的错误预期，未删除或跳过用例。

## 发布与直接生产 AOT 验收

两次完整发布命令均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，目标项目如命令、配置 Release、RID win-x64。产物均为 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

- 发布 #1：成功，退出码0，全部11行日志已读；仅显示还原/托管产物/发布路径，未显示重新 native 编译。启动全量312通过/8失败/0跳过，320总计，352ms；8项均为 private bitmap 路径 E_NOTIMPL。TRX `5f935fc5-d092-4c34-a0c5-42a5642d2411`，start `2026-10-08T20:52:52.9331756+08:00`，finish `2026-10-08T20:52:53.6349518+08:00`。读取全部96行测试日志。
- 发布 #2：生产修改后成功，退出码0；全部可用59行已读。工具将415749字符截为最后20000字符，前395749字符不可见；可核实 CA1416、Generating native code、lib/exp生成及发布路径，不宣称无警告。首次定向7通过/1失败/0跳过，30ms，失败为 Extend 预期错误；TRX `7a56bee7-a0c0-4f74-8189-9971887b8659`，start `20:56:25.9377428+08:00`，finish `20:56:26.3665449+08:00`（同日）。仅修正测试预期，没有再次改生产或发布。
- 发布 #2 后定向 `FullyQualifiedName~WhenPrivateBitmapBrush`：8/8，0失败/跳过，21ms；TRX `5bdfe2c6-948b-4634-b9af-978c11957a9c`，start `2026-10-08T20:57:17.2045769+08:00`，finish `2026-10-08T20:57:17.6306487+08:00`；全部28行已读，存在 MSTEST0044/MSTEST0037。
- 同一发布 #2 后全量：320/320，0失败/跳过，344ms；TRX `75e9b920-a8ba-43bb-9791-ec1624d17a68`，start `2026-10-08T20:57:34.3383662+08:00`，finish `2026-10-08T20:57:35.0254254+08:00`；全部16行已读。

唯一测试项目 `Tests/WpfGfxShape.ComAcceptance` 直接加载生产 AOT DLL，未恢复 Host、未修改 slnx 或原 WPF、未新增测试导出。测试包含 effects 首错、锁失败、重试、Pop、像素、重新锁定与 finally 释放。固定 TRX 每次覆盖前读取身份摘要，但未复制完整历史 TRX；本轮两次发布均未取得 SHA256，不沿用旧哈希冒充新产物身份。

## 未关闭边界

本次是完整 ImageBrush/层/Compose 模块的生产内部进展，不是完整模块交付。边界对象字段布局通过不等于真实 MSVC 创建者 ABI 已验证。BrushBitmap 不再无条件拒绝；Border/wrap5 保持 E_NOTIMPL，一般 Widen 仍 E_NOTIMPL。非有限/奇异 bitmap 映射区分、prefilter完整语义、非轴 source clip 差分、真实读取重入保活和全部失败恢复还需验证。四点 clip 使用真实 polygon，不以 bounds 替代 Intersect。

阶段1–3 Closed、4 Active、5–7 Pending；Ledger仍 RepairRequiredPartial，映射与机器差集尚未同步验证。覆盖率、原生差分、PresentationCore E2E未运行。无已确认外部阻塞，不能替换原 DLL，不报告整体百分比。
