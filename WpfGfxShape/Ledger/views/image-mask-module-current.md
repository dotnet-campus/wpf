# ImageBrush遮罩模块当前差集（S-20261008-154）

## 当前核实差集

恢复生产AOT发布全量208/208通过；已有基础层栈、NaN空层和浮点裁剪Clear不重新领取。本轮内部DrawPath/DrawInfinitePath接入共同空裁剪消费者，无交集跳过形状/画刷及目标锁，非空路径仍E_NOTIMPL。8项旧DLL Red→Green，正常发布后内部目标41/41、全量216/216，0失败/跳过，325ms。实际加载DLL SHA256 7A513FA425A2107EDEC1FAE2C831C5E83E543B67A9795F863872CAEEA5C723AE，TRX ID b591b8a5-feaf-4f01-b57a-ccd642f2804f。两次发布及证据边界见../../Docs/handoffs/completed-work/S-20261008-154-INTERNAL-PATH-EMPTY-CLIP.md。

完整非空形状/画刷/层消费者与联合Compose模块仍未交付，无明确外部阻塞；不晋升机器Ledger。下方数字仅为前次基线。

此视图记录已核实事实，不修复或晋升机器Ledger。schema-version.json仍为RepairRequiredPartial，evidence.jsonl存在同一行连接多个JSON对象的损坏，subject链接尚未修复；不根据测试数量计算生产完成百分比。

## 已有生产接线

- GeneratedImageContentBounds.TryClip调用GeneratedGeometryBounds，填充路径、二次/三次及poly曲线、Arc、矩形/圆角、椭圆和组累计端点/导数极值边界；光栅覆盖独立。
- ImageBrush Tile/Flip方法族及真实层位图共同DrawTransformed/DrawPath/scan消费链已接通。
- 真实IStream/系统WIC遮罩读取重入、通道/连接销毁、删除/替换、帧快照及最终流释放代表性用例已存在。

## 当前新增生产差集及验证

内部DrawInternalBitmap裁剪与目标无交集时在效果/源/目标锁消费之前返回成功，目标/源在效果Capture回调前保活。六项旧DLL锁冲突Red，修改后发布定向内部目标16/16、全量191/191，0失败/跳过，232ms。DLL SHA256 5A787A91E90806B7B6B6017E4C8B8FB1AD1284CA3E0DD40A258637522612808D，TRX ID 06702c47-dc6c-444a-a787-2310f4ceeefc。两次发布及证据边界见../../Docs/handoffs/completed-work/S-20261008-151-INTERNAL-BITMAP-CLIP-LIFETIME.md。效果回调释放调用者引用仍需专门验收，不晋升机器Ledger。

## 前次验证

两次Release/win-x64发布成功。初始全量182/182；新增QuadraticBezier/PolyBezier/PolyQuadraticBezier后定向22/22；最终全量185/185、0失败/跳过。DLL SHA256 F0A15A0916C2620D6D745A085DC2EED43EEA3C0C3FEC74D0075BC0037E44EE10，最终TRX ID 5866cba4-6cc0-4b2f-b278-cafaf8fc157b。完整命令、产物、发布关联和证据未保存边界见../../Docs/handoffs/completed-work/S-20261008-150-MASK-GEOMETRY-REGRESSION.md。

## 保留的未交付条件

CombinedGeometry和非退化/仿射曲线原生差分；平铺DPI、非整数/仿射、8位/浮点、预滤与接缝联合矩阵；真实读取回调与嵌套层失败恢复联合验收；原生内部目标CContextState/IShapeData/画刷消费者及完整ABI。模块仍开放，阶段4仍Active，当前不能替换原wpfgfx_cor3.dll。下一动作以../../Docs/handoffs/current-work-item.md为准，不把本视图视为新小切片计划。
