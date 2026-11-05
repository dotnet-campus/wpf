# S-20261009-232：稳定基线文档去重与交接整理

## 本轮交付

本轮为用户指定的文档维护，不修改生产代码、测试代码、项目配置、原 WPF/wpfgfx 源码或解决方案，不增加生产能力、不关闭生产阶段。

- `current-stable-baseline.md` 保留当前阶段、稳定能力摘要和最新一份验证；删除重复的多轮“当前权威验证”及冻结历史摘要，历史仍查询既有独立归档，旧冻结文档不拆分、不迁移、不追加。
- `next-session-handoff.md` 明确最近验证基线必须整节替换，不再追加历史。
- `current-work-item.md` 清除已失效的“当前按内容 DIP 分配”背景及重复 hash/TRX；保留 DrawingImage/TileBrush 未关闭合同，规划下一整块生产闭环与验收条件。

## 起始发布记录

1. 命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\WpfGfxShape.csproj -c Release`。目标为上述绝对路径、Release，未指定 RID；失败，9 行日志，诊断 `MSBUILD : error MSB1009: 项目文件不存在。`。没有本次发布产物，没有对应 SHA256/TRX。读取交接及测试方法文档后确认生产项目位于 `Code/WpfGfxShape/`。
2. 命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。目标项目 `WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj`，Release/win-x64；成功，11 行发布日志。产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。未据增量发布成功宣称既有编译警告全部消除。

## 发布后全量测试

唯一验收项目：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`；直接加载第 2 次发布的生产 Native AOT DLL。

- 全量 686/686 通过，0 失败、0 跳过，耗时 2s，退出码 0。
- 测试输出确认加载路径：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64\wpfgfx_cor3.dll`。
- SHA256：`CD2BCD99F8E0EE4AB75B6C8798D149BEFA05508A8B2EF5BC6AD919ADA26AC801`，与 S-231 相同，符合本轮未修改生产代码的边界。
- TRX：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`；Run ID `50515a34-a07e-463f-a406-7690810007c5`，记录时间 2026-10-09 15:45:22（+08:00）。该固定文件后续运行会覆盖，不承诺永久保留原 TRX。

## 下一轮与未扩展边界

下一轮继续完整 DrawingImage/TileBrush 生产闭环：原生 clip/尺寸/mapping 合同、目标裁剪、绝对 viewbox 子区域、跨 tile/flip 接缝、非均匀/旋转及资源遮罩失败释放联合实现和真实 COM 验收。具体步骤与完成条件仅维护于 `../current-work-item.md`，不以本轮文档整理替代生产推进。

阶段 1–3 Closed、阶段 4 Active、阶段 5–7 Pending；Ledger `RepairRequiredPartial`。目标 clip/viewbox/跨 tile 合同、完整 MSVC ABI、曲线拓扑、完整层栈、UCE 与 PresentationCore E2E 均未因本轮整理而关闭，不能替换原库。
