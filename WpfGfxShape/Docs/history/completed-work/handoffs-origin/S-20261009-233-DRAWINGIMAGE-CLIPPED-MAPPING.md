# S-20261009-233：DrawingImage裁剪中间面与原始坐标映射

## 生产方法族推进

联合修改 `GeneratedVisualRenderer.DrawingSnapshot`、`GeneratedImageMask`、`SoftwareBrushRealization` 及 `Direct3D9SoftwareImageRenderer.ImageMask`，不新增 ABI 或测试专用导出。

- 相对 viewbox 先解析为原始内容坐标，stretch/alignment 只在原始内容空间计算一次。
- 快照范围取内容 bounds、viewbox、viewport 反映射的交集；非平铺路径再交目标 clip 的内容空间保守 bounds。目标范围从 Render 入口传入，geometry brush、Visual opacity mask 与 PushOpacityMask 均传递 clip。
- 根据裁剪范围和设备基向量长度计算中间面尺寸，保存独立 PixelToDevice。实际消费使用该映射，不再根据裁剪后 bitmap 尺寸重新推导 stretch，保留绝对 viewbox 子区域与原始内容位置。
- 平铺路径只裁剪基础 tile 的 source 内容，不把当前目标 clip 裁掉的区域当作整个周期；完整 viewport 周期及透明区域仍由基础 tile 消费保持。
- 快照捕获继续在调用目标 COM 前完成，候选失败 Dispose、临时 target/bitmap/session finally 逆序释放及首错传播保留。

## 原生依据与实现差距

读取范围：`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/DeviceAlignedIntermediateRealizer.cpp` 50–160（上一切片还有215–325）；`ViewportAlignedIntermediateRealizer.cpp` 60–245。

原生 non-tile Realize 以 shape bounds、sample-space clip、viewport parallelogram 形成可见范围并计算 surface mapping；tile 路径分解矩阵并维护 base tile 与 rendered tile 的独立映射。当前是软件 source 的内容空间保守裁剪及基向量尺寸实现，不是完整原生 device-aligned / viewport-aligned 精确直译，不含最大纹理尺寸调整、缓存及原生 parallelogram 精确裁剪。

## TDD、验收与诊断

起始全量686/686。新增4项初始定向Red：巨大viewport返回E_NOTIMPL，绝对viewbox子区域透明；第一版生产发布后仍4项像素失败，查明新测试报文错误保留RelativeToBoundingBox标志，修正为Absolute后定向121/121通过。这4项初始Red不能全部归因生产，包含夹具单位错误。

随后新增6项非对称基础tile的相邻tile水平接缝两侧像素/遮罩验收，覆盖Tile、FlipX、FlipY、FlipXY；其中FlipY行只证明水平重复，不证明纵向翻转。新增2项1e5设备放大+目标clip的填充/遮罩验收；另增4项DrawingImage source结合遮罩的回调重入与fill失败恢复。合计新增16项。

曾以1e10设备比例验收后出现E_INVALIDARG（696通过/2失败/698项），该限制未修复；最终将比例改为1e5验证当前支持边界，不能把最终Green称为1e10缺口关闭。极端比例的原生几何捕获/工作空间仍须下一轮实修，不以固定上限隐藏。

最终全量702/702通过，0失败、0跳过（1s）。既有旋转/非均匀、source深度/关闭/替换、首错、临时资源清理测试共同回归通过。测试编译仍有既有MSTest分析警告。

## 本轮发布记录（含上一响应的连续工作）

三个发布完整命令均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。目标项目 `WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj`，Release/win-x64；产物均为 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

1. 起始成功，11行日志，全量686/686；SHA256继承起始测试产物 `CD2BCD99F8E0EE4AB75B6C8798D149BEFA05508A8B2EF5BC6AD919ADA26AC801`，本轮起始TRX Run ID未独立保留。
2. 初次裁剪与PixelToDevice修改后成功，60行日志，包含CA1416警告；定向117通过/4失败，夹具单位修正后121/121通过。该发布独立hash/TRX未保留，被后续运行覆盖。
3. 遮罩clip传递与接缝验收后成功，60行日志；先696/696，随后极端比例Red，支持边界测试修订及遮罩回调增强后最终702/702。SHA256 `6508789DC729EA9885446CA6C8C0F30573670D307B7B611C99A96334D4E94719`；TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，最终Run ID `07ea28ce-f0a5-44b0-ba05-c79ba89b4e78`，由TestContext确认实际加载产物。中间全量696/696的Run ID为 `68e36abe-44cd-42c5-a434-492b5efcd33d`，文件已覆盖。

## 未关闭边界与下一整块

本轮已交付裁剪范围、原始内容映射和真实消费方法族，不仅是复验或计数。原批准模块仍Active：1e10比例几何工作空间、完整device-aligned采样范围/像素对齐、超大平铺基础tile的可见区域优化及纵向/双轴跨tile精度尚未关闭。下一轮必须联合推进几何工作空间和中间面采样映射生产方法族，保留本轮像素、生命周期和发布验收；不是默认纯补测任务。

阶段1–3Closed、阶段4Active、阶段5–7Pending；Ledger RepairRequiredPartial。完整MSVC ABI、拓扑、层栈/UCE及PresentationCore E2E未关闭，不能替换原库。
