# DrawingImage 重复 tile 实现路径收敛与验证收口

## 交付及边界

本轮完成正常尺寸 DrawingImage 重复 tile 的实现路径收敛，不宣布完整 intermediate 方法族关闭。移除前置 FitsSingleTile 判定及对应单分区快照旁路，让 DrawingSnapshot 已有的整数网格裁剪、负 tile shift、奇偶 flip、尺寸调整和 PixelToDevice 映射共同决定实现；普通 BitmapSource 分区逻辑不变。

保留超过 int.MaxValue 的理想网格保护。实验移除该保护导致 source12/13 三项透明像素回归且全量耗时增至一分钟；恢复保护后回归消失。这是本轮实测的风险边界，不是原生完整等价证明。后续应完成超大网格有界实现，不能仅删除保护。

新增周期完整表面验收 12 组，覆盖非均匀尺度、旋转以及 float/Pbgra32 和 fill/stroke/mask 的代表组合。既有 90 组 viewport 完整表面及 decoder 生命周期共同回归。测试增长不代表完整方法族完成。

## 原生证据及生产调用者

- src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/ViewportAlignedIntermediateRealizer.cpp：CalculateSurfaceSizeAndMapping1D（677–959）、CalculateTileShiftForPixel、AdjustSurfaceSizeAndMapping1D（990 起）。整数溢出保护、含滤波邻居的 inclusive 像素边界、同 tile/相邻 flip tile 裁剪、奇数 tile 平移修正、2048/2046 上限。
- DeviceAlignedIntermediateRealizer.cpp：271–338，设备空间 floor/ceiling 和独立 2048 上限及映射缩放。
- 生产路径：GeneratedImageMask.TryCapture → GeneratedVisualRenderer.DrawingSnapshot.TryCapture/Realize → Direct3D9SoftwareImageRenderer.ImageMask 的实际 fill/stroke/mask 消费者。
- 修改：GeneratedImageMask.cs、CompositionTests.DrawingTileIdentity.cs。没有修改原版 WPF 源码、ABI、golden、容差、目标框架或方案文件。

## 所有权和错误语义

移除正常尺寸 DrawingImage 分区后复用既有 snapshot 所有权和消费者 AddRef，不新增 COM 所有权或接口。借用/拥有身份、HRESULT 首错、finally 逆序释放及无锁 decoder 回调保持原实现；联合生命周期回归通过，但不据此声称新增路径全部故障注入已完成。

## 本轮逐次发布

三次均使用完整命令：

`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`

目标项目 WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj；配置 Release；RID win-x64；TFM net10.0。产物统一为 WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

### 发布 1：起始基线

- 退出码 0，成功；11 行日志，增量发布没有输出警告。
- SHA256 C391C75C74879B142D26FDD0F4BC99A1B94EB21DDDA81D32E4C14B517E5049F5。
- 全量 1004 通过、1 失败、0 跳过，共 1005，2 秒，测试退出码 1。
- 报告 Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-0e9443c37d90456b927d0e6c7e332762.json；对应 TRX 已被后续测试覆盖，未在覆盖前提取 Run ID。
- 首次尝试测试 Code/WpfGfxShape.ComAcceptance 路径返回目标不存在，随后按实际 Tests 路径执行；不算一次已执行的测试。

### 发布 2：移除全部 DrawingImage 分区候选

- 退出码 0，成功；32 行日志，CS8600/CS8602/CS8604、CA1822 警告，均位于既有其他实现；未抑制这些警告。
- SHA256 A82CD76BD7CE545B6BBDC6F7C1A35D995F91DC35AB2CCF24FF49ECFC4B728E9C。
- 全量 1001 通过、4 失败、0 跳过，共 1005，约一分钟，测试退出码 1。
- 严格校准加 source12 Tile、source12 FlipXY、source13 FlipXY 三项透明像素回归。该候选不作为稳定基线。
- 报告 Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-d69b0d5ea1fa4f74afccca059c0db869.json；TRX Run ID 未在覆盖前提取。

### 发布 3：保留超大网格保护，正常尺寸统一快照

- 退出码 0，成功；32 行日志，保留 CS8600/CS8602/CS8604、CA1822 警告。
- SHA256 DF1FE340AA55762EDC1416735EAACD4A443EE67A400E4E582A194D5C739AD66D。
- 定向命令语义：指定 COM 验收 csproj，filter 为 FullyQualifiedName~WhenCroppedDrawingTileMovesByOnePeriod|FullyQualifiedName~WhenDrawingViewboxExpands|FullyQualifiedName~WhenDecoderReadsDuringRender。365/365 通过，0 跳过，166ms，退出码 0；TRX Run ID 632f27c9-6aa9-4d8c-9c10-5a7f9ef0aabe，已被全量覆盖。
- 最终全量：指定 WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，1016 通过、1 失败、0 跳过，共 1017，2 秒，退出码 1。
- TRX 路径 WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx；Run ID e1c87e8e-a7f4-494e-a045-c0c50dec6178。
- 最终报告 Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-8cec233faf1a4160b570a466bc9ba83a.json。

## 差分结论

唯一失败仍为严格原版校准；float 投影 66/450 不同，直接 Pbgra32 42/180 不同，格式判定差 24。普通 BitmapSource 和实色对照各 180 组一致。周期与 viewport 的等价 fixture 通过不能代替原版差分。

固定原版 DLL SHA256 501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A；固定原版样本 original-wpf-e07cbceaf453471dacb4cb12c99bdbc2.json，SHA256 98D740A895372A31DAF4EB234F3942E31A8C99A15D278F121DA84C36B585CE58。保持 float/HDR、1e10、Vector4 距离 <1e-7，不从 8 位投影反推 float golden。

## 上一轮未归档状态的恢复说明

起始代码已含独立空间尺度分解、饱和整数尺寸、完整 viewport 透明留白、snapshot/消费者尺寸对齐和 90 组 viewport 测试。本轮起始发布验证这些现存修改，但不把它们误称为本轮新增。上一轮摘要提及最终 Run ID a4722c0e-9823-45c6-882f-aafe82db7819 和报告 wpf-calibration-a637247e09db49558f7e4510c6dd5199.json；本轮没有重新取得上一轮每次发布完整日志，因此不补造其逐次发布证据。

## 未关闭与后续合并目标

完整 ViewportAligned/DeviceAligned 方法族、source17 旋转差分、24 组格式判定差、Visual mask 作用域合同、超大网格严格有界等价及新路径故障注入未关闭。下一轮仍以实际目标格式 span、双向映射和生命周期合并闭环为交付，不只做定位或补测。

阶段 1–3 Closed、4 Active、5–7 Pending；Ledger RepairRequiredPartial；不能替换原库。
