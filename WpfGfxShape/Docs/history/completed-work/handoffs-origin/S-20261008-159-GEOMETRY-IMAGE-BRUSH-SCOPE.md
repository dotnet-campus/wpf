# S-20261008-159：几何 ImageBrush 填充恢复与作用域联合验收

## 交付边界

这是完整 ImageBrush 遮罩映射、层消费与 Compose 生命周期模块的内部进展，不是完整模块交付。本轮恢复并验收前次尚未归档的生产填充实现，新增三项真实作用域联合用例；没有新增生产 ABI、测试导出、Host 或修改原 WPF 源码/解决方案。

## 原生证据与生产差集

本轮读取 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/drawingcontext.cpp:878-952`：DrawGeometry 取得当前 IShapeData，存在形状时调用 DrawShape。此证据不授权把 C++ 形状或 CBrushRealizer 当作 IUnknown。

恢复核实的既有实现：`Core/GeneratedVisualRenderer.cs` 捕获无笔画 ImageBrush DrawGeometry、局部内容 bounds、设备覆盖及持有来源引用的画刷快照；`Core/GeneratedImageContentBounds.cs` 消费几何填充 bounds；`Core/Direct3D9SoftwareImageRenderer.ImageMask.cs` 复用普通/tile 物化消费完整 RGBA；`Core/SoftwareImageRenderSession.cs` 接通 DrawBrush。Render 在目标 COM 回调前捕获，失败时释放会话，再逆序 Dispose 画刷快照和释放图像来源，最后释放根 Visual。本轮没有修改这些生产文件。

`CompositionEffectsTests.ImageMask.cs` 新增三项 DrawGeometry + PushOpacityMask + 嵌套 PushOpacity/Pop 联合用例：相对单位作用域 bounds、半透明预乘来源、192 DPI + FlipXY + 来源清空/恢复。各用例沿真实连接/通道/资源/Present 消费，检查来源锁失败首错、失败不输出、解锁后恢复、零 viewport 透明及下一帧恢复。作用域用例显式选择最近邻采样，半透明来源与遮罩两次 alpha 消费预期为 0x40400000。

## 发布流水

两次发布完整命令均为：

`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

目标项目为上述 csproj，配置 Release，RID win-x64。产物：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64\wpfgfx_cor3.dll`。

1. P1 开始发布成功，工具报告日志 11 行。恢复全量 261/261、0 失败/跳过、254ms，TRX `c00ee0ce-4341-497e-bca7-75f07c8dbb28`；测试记录实际加载 DLL SHA256 `D5EC6C7285AE216D4910098081FBABABC64291E0CC6CA9D98FF308EED77821C2`。P1 原始日志在测试覆盖前未逐行保留，不宣称无诊断。首次测试传目录时返回目标不存在，随后改传 csproj 成功，该路径错误不计作测试失败。
2. P2 最终发布成功，读取完整 11 行日志：退出码 0、还原已最新、生产 DLL 输出至 artifacts/bin/WpfGfxShape/Release/net10.0/win-x64，发布目录如上；该次日志没有警告或错误。随后全量 264/264、0 失败/跳过、267ms，TRX `97c3fc79-7ae8-4a77-b665-869205bac230`；实际加载 DLL SHA256 与 P1 相同。本轮仅修改验收源码，生产产物未变化。

## 测试证据限制

P1 定向初次 53 通过/3 失败、69ms，失败是新测试未固定采样模式而产生边缘插值衰减，不是生产 E_NOTIMPL。修正测试采样设置后 P1 定向 56/56、68ms，TRX `d684f313-4fe5-4bc0-b01a-c8b360fb04bf`。不将其表述为生产缺陷同断言 Red→Green。测试重新编译出现既有 MSTEST0037/MSTEST0044 警告，未扩展修复范围。

统一结果文件 `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx` 会被覆盖，目前保留 P2 最终全量结果；恢复/定向 TRX 的上述标识和已读取结论在本文件保留，但未复制完整 XML。发布原始日志由后续测试覆盖，此处保存 P2 可核实摘要，不宣称完整原始日志长期归档。前次填充实现的发布流水和 Red 原始证据未独立保留，本轮只报告恢复验证，不补造历史。

## 剩余范围

原生内部非空有限/无限 fill/stroke、IShapeData/IFigureData、CBrushRealizer/CMILBrush、CPlainPen/CContextState 私有 ABI 消费尚未完成；一般曲线组合 bounds/非轴保持操作数、原生边缘差分及完整回调保活矩阵未关闭。没有执行 Ledger 修复/验证、覆盖率或 PresentationCore E2E，也没有确认外部阻塞。阶段 4 Active，Ledger RepairRequiredPartial，当前不能替换原 wpfgfx_cor3.dll。下一轮唯一计划见 ../current-work-item.md。
