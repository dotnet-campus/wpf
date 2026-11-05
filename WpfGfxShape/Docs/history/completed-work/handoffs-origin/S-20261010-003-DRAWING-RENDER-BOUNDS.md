# DrawingImage 消费边界与原网格裁剪映射

## 交付边界

完成消费者 render bounds 在局部 tile 递归中的独立传递、ViewportAligned 消费边界裁剪、原网格 scale 与整数裁剪 offset 保留及超大网格识别修正。完整 intermediate 与严格原版等价尚未关闭；不得以本归档声明方法族或阶段完成。

## 原生证据及生产修改

原生证据均来自 src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/，每次读取不超过400行：

- ViewportAlignedIntermediateRealizer.cpp：380–465、525–602、660–825、826–993、1000–1074；理想 tile/render bounds、整数尺寸、裁剪、tile shift、flip、最大尺寸调整。
- DeviceAlignedIntermediateRealizer.cpp：271–338；整数边界、最大尺寸 cap 与 FLOAT 映射。

生产修改：

- GeneratedVisualRenderer.DrawingSnapshot 保留裁剪前 scale；裁剪只减去整数 texel 起点，不从裁剪后的内容 bounds 重新求 scale。
- 超大网格以未饱和 idealWidth/idealHeight 判定，避免 CalculateSurfaceSize 饱和到 int.MaxValue 后判断失效；保留有界局部 tile 的双精度子像素原点路径。
- ViewportAligned 使用消费者 render bounds 与目标 clip 的交集，保留非整数形状边界。
- GeneratedImageMask 的局部 tile 递归只修改 brush 变换，不再用该变换重新计算消费者 bounds；原目标 render bounds 独立传入子 tile。
- DrawingTileIdentity 新增旋转 float/Pbgra32 的 fill/stroke/mask 周期移动完整表面一致性6项。

## 所有权及错误边界

未修改 COM ABI、HRESULT 约定或拥有/借用关系。沿用 snapshot 捕获失败 finally Dispose、tile 失败逆序 Dispose、Realize 的 session/candidate/target 释放及无锁回调。既有生命周期测试仅作为回归，不宣称全部新增路径故障注入已完成。

## 逐次 Native AOT 发布

以下四次均调用发布工具成功，成功退出码0；完整命令均为：

`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`

目标项目 WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，配置 Release，RID win-x64。每次产物路径均为 WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll，会被后续发布覆盖。原始发布日志也已被后续测试覆盖，未独立保存完整警告，不能据此宣称零警告。

| 发布 | 修改/验证 | SHA256 | TRX/报告证据 |
|---|---|---|---|
| 1 | 起始全量1074通过、1失败，总1075 | 4D007AFE2A105E85ADE78F72747150B87DCACE99EB74B19F2589A15ACE00F6B3 | wpf-calibration-db2a81b619034d2ab54c3a8aaa55c74c.json；TRX Run ID未提取，固定文件已覆盖 |
| 2 | 原网格映射初版，全量1074通过、1失败 | 0E81D486AAEE100B4978638F4ACC5B01C60AB51DCAAFEACA40B3B2895A9F349D | TRX 46b95759-5614-49ec-8914-06c2c5c95a43；wpf-calibration-cdab962bebeb45359aa20311af42f4a0.json |
| 3 | 整数crop直接传递及render bounds接线；联合650通过、21失败，总671；source15的1e10/HDR回归 | 未取得，不以发布4身份代替 | TRX Run ID未提取，固定文件已覆盖；未运行校准报告 |
| 4 | 修复局部tile递归消费者bounds；最终联合707/707、全量1080通过、1失败，总1081 | 45ED41EF3E35057BAF981951D603265054666056BE64B49B5FC86A66B5F33C95 | 最终TRX 371d9fee-6052-4d0a-99db-bf04d43fbcec；wpf-calibration-3bc88f873e22455689b88bb0bdd1307b.json |

发布4先验证联合671/671，TRX f06a1394-78f1-4826-a0ac-c79779da2480；新增测试不修改生产DLL，旋转周期测试36/36通过。首次最终全量TRX fd9ca4ec-f329-497b-addc-78b9f260c197、报告 wpf-calibration-1285219be2854834a5684135a8f50677.json，同为1080/1。最后联合707/707后再次全量1080/1，持续2s、测试退出码1。TRX路径为 Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，固定路径会覆盖；JSON报告均在同一TestResults目录。

## 严格差分及未关闭项

唯一全量失败仍为严格原版校准：float投影66/450不同、直接Pbgra32 42/180不同、格式判定差24。BitmapSource与实色对照各180组一致。未修改golden，未放宽float/HDR、1e10或Vector4距离<1e-7。

固定原版DLL SHA256：501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A。
固定样本：Tests/WpfGfxShape.WpfReference/TestResults/original-wpf-e07cbceaf453471dacb4cb12c99bdbc2.json；SHA256 98D740A895372A31DAF4EB234F3942E31A8C99A15D278F121DA84C36B585CE58。

完整尺度分解/理想tile的FLOAT运算合同、双向映射、旋转目标格式span、Visual mask作用域及新增路径故障注入仍未关闭。当前没有原生证据缺失的外部阻塞，不缩小下一轮为补测或重复发布。阶段1–3 Closed、4 Active、5–7 Pending；Ledger RepairRequiredPartial，不能替换原库。
