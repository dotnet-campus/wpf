# S-20261008-193：真实直线笔画 Red

使用 S-192 发布 #3 的生产 DLL，唯一 ComAcceptance 项目直接加载，无额外发布。定向 `FullyQualifiedName~WhenImageMaskSourceIsLockedThenLayerIsDiscardedAndRetryPreservesMapping` 87 通过/3 失败，110ms，跳过 0。三项新增用例：水平 Flat、反向负厚度 Flat、Square。均在 Present 取得 E_NOTIMPL `0x80004001`，预期 WIC 锁定 `0x88982F0D`；未改断言绕过失败。TRX `64a198e7-9b79-4e67-bc50-419dfe6923ce`，开始 `2026-10-08T20:09:58.3500710+08:00`，结束 `2026-10-08T20:09:58.8080709+08:00`。SHA 与 S-192 发布 #3 相同；固定 TRX 下次覆盖前保存此身份摘要，无完整副本。

生产差集：为 LineGeometry 保留当前动画值，将 Flat/Square 轨道多边形送入共同 SoftwarePathCoverage 量化扫描；真实 renderer 和 ImageBrush scope bounds 通过现有 GeneratedStrokeGeometry 消费。保持 fill→Widen→stroke，不新增导出或公开接口。一般曲线、Dash、Round/Triangle、零长度 Square、私有 ABI 未完成，模块仍 Active。Green 在后续独立记录中归档。
