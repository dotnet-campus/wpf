# FLOAT 工作合同未修改复验（未完成修复）

## 边界

本轮读取 stage4-rotated-float-contract-repair-plan、迁移章程、当前执行规则和测试证据契约，审查 DrawingSnapshot.Realize、GeneratedImageMask.RealizeDrawingSource、RealizeTransformed/RealizeFloat、BlendSpan/BlendAffineSpan、AddImageMask 和 SoftwareBitmapMask.Prepare。

没有修改生产代码、固定 reference、严格断言、mapping 或 Ledger；没有发布。下列 SHA 是实际测试加载的既有产物身份，不是本轮新发布身份。没有执行终端、脚本、自定义程序或沙盒执行器。

## 实际证据

- 精确定向严格校准：发现1、失败1、通过0。报告 `Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-dfc8e425b5874fc088f51298f04d6dce.json` 已回读。
- 450 fixture 中57差；source17 affine3为31/45，source18 affine3为26/45；Direct Pbgra32为0/180，solid target control为0/180，AotBitmapMismatches为0。
- 独立 `Docs/investigations/RotatedFloatContract.Tests/RotatedFloatContract.Tests.csproj`：3/3通过。其分类测试读取既有 candidate Evidence，不冒充对本轮新报告重新分类，也不构成候选内部阶段诊断。
- FLOAT/HDR及viewport定向筛选运行成功；本轮没有保留该次日志中的精确发现数，不报告猜测数量。
- 普通 full：4197通过、0失败、0跳过。
- COM full：1339总计、1327通过、12失败、0跳过。RunID `a6d9b98d-a31e-48b9-bf91-f47c813f181d`，`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx` 已回读。
- COM full 严格校准报告 `Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-27710ff7cd324152b8281b49420c4c18.json` 已回读，仍57/450，Direct0/180。
- 实际候选 SHA256：`AED6B7D1FF8EA57F79849EF8CD78D77E1274CEB037DC5DCA49E1016F90BAF09F`。
- 固定 reference SHA256：`98D740A895372A31DAF4EB234F3942E31A8C99A15D278F121DA84C36B585CE58`。
- 原版 DLL SHA256：`501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A`。

## COM 失败分类

1项严格校准失败，8项公开 HWND/D3DImage 像素失败，3项第二 sampler Visual/cache 窗口像素失败。窗口 GetPixel 实际为0；第二 sampler 的76 DWORD CreatePixelShader 返回 S_OK。本轮没有独立桌面遮挡/锁屏证据，不能将11项窗口失败认定为系统锁屏，也不能把它们作为通过。用户此前提供的1338/1不是本轮实测结果。

## 实施状态

已确认代码仍以目标 `_floating` 同时选择 tile、color-source、mask 和 span 运算。首次 alpha 分叉尚未取得合法候选 tile/contentToSurface/surfaceToDevice/final-span 实证；没有提交未经证明的末端舍入或全目标8bit workaround。

完整合同分离、真实格式 metadata 接线、57→0、正式 publish、同发布 SHA 定向/full 归档、mapping/Ledger/stable/workitem/remaining-gap 同步及 shader 生命周期 freeze/per-device fallback 接续均未完成。阶段4保持 Active，不具备关闭或替换资格。本文件仅记录实际复验，不作为实现交付或模块完成证据。
