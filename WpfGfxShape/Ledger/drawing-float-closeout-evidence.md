# DrawingImage稳定化与decoder验收映射证据（非机器权威）

> 本文件实际更新本轮ledger证据导航，不替代records.jsonl/evidence.jsonl的schema校验，不修复或晋升机器状态。schema-version.json仍RepairRequiredPartial；records.jsonl读取仍发现同一行连接多个JSON对象，不能把本次成功测试写成机器ledger已修复。

## 本次事实

- 生产映射：../Docs/native-to-managed-file-map.md；历史归档：../Docs/handoffs/completed-work/S-20261012-MASK-ROLLBACK-DECODER-STABILIZATION.md。
- GeneratedVisualRenderer.CaptureMaskLayer恢复消费者scalingMode，Visual Capture撤回固定linear；原生drawingcontext.cpp:3895–4005最近邻只约束已物化层1:1合成，EndOpacityLayer保留其原生合同。没有新的全局采样候选。
- FloatMaskSpan恢复原生FLOAT端点与累计增量预期；保持HDR及Vector4距离<1e-7。三项新增mask引用固定原版Pbgra32全surface，不从AOT观测回填golden。
- CompositionTests.DrawingLifecycle六项真实系统WIC/IStream STG_E_READFAULT首错/失败来源/重试像素直接验收通过。完整方法名WhenRealWicDecoderReadFailsThenPresentReturnsFirstErrorAndRetryRestoresPixels，tiled false/true × scaling1/2/3。ActiveRender.cs:369–371及710–743为实际失败注入/首错/恢复观察链；不混称为填充锁冲突。
- 新增9项9/9，RunID 10b2feca-3299-4baf-a8f1-45e291777a3d；全量1170通过/1失败/0跳过/总1171，RunID c55ccb21-e83e-4fe6-acd1-dd7af339fdf5。已消除61失败候选状态，但不是全绿。
- 本次Release/win-x64 AOT发布成功，校准计算SHA256 760CB48F8365BA5004BEBDBE891364E20709A56ABF058CDEC7CD37F0451CC988；报告wpf-calibration-22ca2dde837b490ba3290a348374779d.json，float投影57/450、Pbgra32 0/180。唯一既有校准失败不隐藏。

## 未晋升的阶段4合同

- source17/18：真正同版本非限幅float/HDR参考、目标绑定/颜色源、FLOAT rest与旋转tile/mapping/span尚缺；sRGB限幅参考不能当float oracle。
- DoubleBufferedBitmap已有前缓冲两帧窄路径；全格式/源替换/失败生命周期未完整闭合。
- D3DImage：d3dimage.cpp:94–205的CInteropDeviceBitmap、software fallback与interop bounds不能以普通IWICBitmap替代；factory/消费者、真实D3D9 surface/device、callback/detach/device-loss及失败所有权验收仍缺。
- glyph真实字形来源与私有数据消费、media真实provider/sample/composition桥、shader真实bytecode/GPU/sampler消费者尚未全闭环。

阶段1–3 Closed、4 Active、5–7 Pending；文件族Partial，不可替换原库。本次安全边界为稳定化与decoder实际范围完整验收，未声称安全完成新消费者族。最新身份以稳定基线为准，后续机器台账修复须先恢复JSONL完整性与subject关联，不能直接导入本补充为整体Complete。
