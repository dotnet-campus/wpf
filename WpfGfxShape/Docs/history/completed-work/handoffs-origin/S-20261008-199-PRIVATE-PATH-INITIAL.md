# S-20261008-199：私有路径恢复时的 Red 证据

本文件归档模块内部诊断，不代表完整 ImageBrush 模块交付。

## 发布 #1

- 命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。
- 项目 WpfGfxShape，Release/win-x64，退出码 0。
- 读取全部可用 59 行诊断；工具已将 414927 字符截断到尾部 20000 字符，前 394927 字符不可取得。可核实 CA1416、Generating native code、native lib/exp 创建及发布路径；不声称完整编译诊断无警告。
- 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- 全量直接 AOT 验收 304 通过、3 失败、0 跳过，总计 307，336ms；测试退出码 1。
- TRX：`685458f1-d77c-4a5f-b1c7-3198750f6e45`，start `2026-10-08T20:34:24.6297997+08:00`，finish `2026-10-08T20:34:25.3132450+08:00`。
- 测试输出 SHA256：`E651637F8D666D97FF22861DE1AE76479717E938B88EF98DB600C037951B0889`。
- 覆盖固定 com-acceptance.trx 前保存以上身份及结果摘要，未复制完整 TRX。

## 诊断

前轮未重新发布的修改本轮已真实加载。新增 infinite realizer 的三项像素验收预期红色 `0xffff0000`、实际蓝色 `0xff0000ff`，不是旧 DLL 的 E_NOTIMPL。

原生 `include/Generated/wgx_misc.cs:196-203` 明确 MilColorF 为 RGBA；托管 `Direct3D9SurfaceRenderTarget.cs:8` 为 Alpha/Red/Green/Blue。新增 FillInternalShape 将原生内存直接解释成托管颜色，是颜色通道错位根因。原生 `core/api/api_brush.h:32-85` 和 `api_brush.cpp:95-101` 确认 solid 保存 MilColorF；`core/sw/swlib/swrast.cpp:533-596` 确认 EnsureRealization → GetRealizedBrushNoRef → GetRealizedEffectsNoRef → null 检查及借用合同。

一般 Widen、bitmap 私有映射与真实创建者 ABI 验收尚未关闭；这不是模块完成记录。
