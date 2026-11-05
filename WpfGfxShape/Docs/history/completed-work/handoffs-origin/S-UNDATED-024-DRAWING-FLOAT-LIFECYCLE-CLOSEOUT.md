# DrawingImage FLOAT 边界与旋转生命周期：本轮事实收口

> 本轮完成事实归档；日期未可靠取得，采用 UNDATED，不推断发布或全量执行时间。文档收口只读取文件，不修改生产/测试，不启动程序、不运行发布、测试或覆盖率。生产方法族仍为 Partial，未 Closed。

## 证据来源与原生范围

原生合同范围沿用既有归档与映射中的 `core/resources/ViewportAlignedIntermediateRealizer.cpp`、`DeviceAlignedIntermediateRealizer.cpp`、`core/common/BaseMatrix.cpp`、`core/sw/swlib/bilinearspan.cpp`。本次监督文档收口不新增原生探索或编写代码；不将既有源码版本与安装运行时默认视为同版本。

事实来源分开记录：当前生产/测试文件和校准 JSON、当前固定 TRX 已在本次静态读取核实；前置/逐次发布成功、source15 定向、最终全量身份及新增50项通过来自本轮执行者已验证交接。全量 TRX 曾已读取，但固定路径现被后续联合定向覆盖，不能重新由该路径核实全量字段。未保存的退出码、时间、逐次独立 SHA 不补猜。

## 实质生产推进与保留状态

- `Code/WpfGfxShape/Core/GeneratedImageTransform.cs`：`TryDecompose` 使用 FLOAT 分量、MathF.Sqrt、FLOAT 逆尺度与 rest；`Prepend`/`TryInvert` 仍有 double 运算，不能宣称完整原始 FLOAT 矩阵坐标链等价。
- `Core/GeneratedVisualRenderer.cs`：保留 brush/world 两次 FLOAT 尺度应用、理想网格、brush rest 与返回映射；`Core/GeneratedImageMask.cs` 与 `Core/SoftwareBrushRealization.cs` 继续传递原始消费者 world bounds。局部 tile brush 变换不取代消费者 bounds。
- `Core/SoftwareImageRenderSession.cs`：目标锁读取后校验非空 buffer 指针、stride/有效 size、宽高/行字节与总面积的 int 表示边界，失败返回 InvalidArgument；初始化失败由 Open 的 finally 释放候选。`Core/Direct3D9SoftwareImageRenderer.ImageMask.cs` 也校验 tile 指针、stride 和有效读取长度。这里只确认这些入口，不泛称全部缓冲消费者已闭合。
- 直接 FLOAT rest 传递候选已撤回。实际格式分区提前字节量化曾造成 source15 三项回归；最终恢复 `AddVisibleTileMask` 的浮点分区生成与累加，即便目标为 Pbgra32 也先合并分区贡献再交既有消费者量化。当前代码中的 `_floating == false` 重定向及 FLOAT 分量累加已核实，source15 回归消除由定向结果支持。

## 消费者生命周期与测试语义

`Tests/WpfGfxShape.ComAcceptance/CompositionTests.DrawingLifecycle.cs` 新增旋转无回调基准、decoder 读取重入关闭/替换及实际填充锁冲突释放后重试组合；共同承载在 `CompositionTests.ActiveRender.cs`。锁冲突方法现在为 `WhenRotatedDrawingFillLockConflictsThenReleaseAndRetryPreservePixels`，不再把填充锁失败误称 decoder 首错注入。真正 decoder 首错/失败重试仍未证明。

`CompositionTests.DrawingTileIdentity.cs` 的完整 float surface 周期不变性增加非空和至少一个 alpha>0 的断言，避免两个透明表面相等造成假通过。新增50项通过是本轮交接事实，不等于完整生命周期或整个方法族关闭。保持 Stdcall、HRESULT 首错、拥有/借用引用、失败逆序释放及无锁回调约束；现有代表性验收不替代所有失败路径证明。

## 发布与验证身份

所有下列发布的目标项目均为 `WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj`，配置 Release，RID win-x64；完整命令均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`。产物均为 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。本次文档收口未重新执行这些命令。

| 发布序号 | 结果/身份 | 对应证据与限制 |
|---|---|---|
| 前置发布 | 成功；SHA256 `CCD2C01F350C9E8BA1F3D3DFA2F0E945BFE9BD9C3D5E39AFCE40602162197D1D` | 前置1111通过、1失败、总1112；历史身份见 S-20261010-023-ORIGINAL-FLOAT-DRAW-BOUNDARY.md。 |
| 最终发布1 | 成功（执行者已验证交接） | 逐次独立 SHA/TRX 与完整诊断未单独取得，不把最终发布2的身份倒填到发布1。 |
| 最终发布2 | 成功（执行者已验证交接）；最终 SHA256 `2029569D9CC020464FC4F74701663E97DCD47EADAD2382047CC16C955CD22F41` | 当前校准 JSON 的 AotDllSha256 已读到相同身份；未重新对 DLL 计算 hash。 |

最终身份下的验收：

- source15 定向279/279，Run ID `0d421bbc-e893-4898-a19d-4b70ad19f229`（执行者已验证交接，固定 TRX 已覆盖）。
- 最终全量1161通过、1失败、0跳过、总1162，Run ID `5b1ce6cd-92df-459b-8340-4a0340d07c11`（已验证交接及曾读取的全量记录，当前不能从固定 TRX 重核字段）。
- 最终联合定向1059/1059，Run ID `10abdcef-a7fa-48c0-82eb-fc0ad4488b85`；当前 `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx` 的 Run ID 和1059项计数已静态核实。此为后续定向，不是全量。
- 校准报告项目相对路径：`Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-11e376bcedaf49c08e472d8652a7e19e.json`（工作区路径前加 `WpfGfxShape/`）。读取确认 Compared450、Mismatches57、DirectCompared180、DirectMismatches0、FormatVerdictDifferences57。
- 唯一失败仍为 `CompositionTests.WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements`。float投影57/450不同：source17 affine3 31/45、source18 26/45（分组细项来自已验证交接）；Pbgra32 0/180。source17/18 差分未减少，没有 float 等价结论。

## 未闭合边界与后续合并目标

真实 float/HDR DrawingImage golden 未建立；原版内部 DrawBitmap 可达/存储为 float 不代表真正 scRGB/HDR 绘制，既有 sRGB 限幅探针不能用作 golden。固定参考为 `Tests/WpfGfxShape.WpfReference/TestResults/original-wpf-e07cbceaf453471dacb4cb12c99bdbc2.json`，SHA256 `98D740A895372A31DAF4EB234F3942E31A8C99A15D278F121DA84C36B585CE58`；当前校准报告 ReferenceFileSha256 与其一致。禁止按目录最新样本替换或用 AOT 回填。

下一轮合并同版本原版 float 真实目标绑定/颜色源合同、旋转 intermediate tile→mapping→span 分阶段证据、原始 FLOAT rest 坐标链、真实消费者 decoder 首错/失败重试与 mask 作用域，完成生产修复和严格测试后再发布、定向、全量收口。不能仅定位/计数/补测交付。

不宣称零警告、覆盖率、清洁重编译或 UI 验收；不扩展未授权 ABI/UCE/层栈等阶段。阶段1–3 Closed、4 Active、5–7 Pending；Ledger `RepairRequiredPartial`，不能替换原库。机器 JSONL 已存在行完整性损坏，不在本轮修复或晋升；当前映射事实保留 Partial。
