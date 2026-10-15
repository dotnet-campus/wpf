# 阶段4 ImageBrush sampler 真实 realization 接续（Partial）

阶段4 Active，非完整通用sampler族完成，不具备替换资格。

## 生产修改与映射

原生 `core/resources/ShaderEffect.cpp:536–610` 两阶段realize/bind合同；托管 `Core/GeneratedHardwareShaderSampler.cs` 新增普通bitmap/DrawingImage的真实 `SoftwareBrushRealization.Capture → GeneratedImageMask → SoftwareImageRenderSession.DrawPath → CreateDrawingIntermediate → Direct3D9InteropColorSource.TryCreateFromBitmap → ProductionDrawBitmap`。复用现有tile/stretch/opacity/一般仿射及相对bounds；中间工作格式随目标A32B32G32R32f或A8R8G8B8。D3DImage原窄路径保留，tile/opacity/一般仿射改接上述真实realization，不再只返回拒绝。bitmap、target、session、uploaded均在失败路径释放，brush拥有捕获资源；全部secondary完成后现有effect才bind。

`GeneratedVisualRenderer.Capture` 对未执行的visual.Effect明确E_NOTIMPL，禁止忽略effect而误报成功；没有实现软件shader executor。

## 实际构建与两次发布

Release生产构建成功。发布1、发布2均实际执行完整命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。项目Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；均成功。第1次后补D3DImage通用路由，第2次为最终代码。发布日志60行：56生产编译、57Generating native code、58lib/exp、59发布目录；存在CA1416平台分析警告，不声称零warning。

产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。最终测试报告SHA `DBE21C3D0DDEC3B01A4DFB101614C3CE9E4FDEAB5B2A4ECDA9F876B721C11F21`；第1次未单独取得SHA，不借用第2次身份。

## 最终代码实际验证

- 精确筛选 `FullyQualifiedName~WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels|FullyQualifiedName~Sampler`：实际发现/执行13，13通过/0失败/0skip，2秒。定向固定TRX随后全量覆盖，未在覆盖前保存RunID，不编造。
- 普通full4197/4197，0失败/0skip。
- COMfull1299通过/1失败/1300，0skip；4m35s，无abort。RunID `a2ff1757-03c5-431a-a45c-463aecd97bf1`，2026-10-13 09:50:44–09:55:20 +08:00。
- 独立report `wpf-calibration-74b242199ce74ad3b7f4ef497f5b09ba.json` 实际回读第1–12行：同最终SHA，FLOAT57/450，直接Pbgra32 0/180。唯一失败WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements，既有严格FLOAT校准；无新增回归。
- 公开HWND本次可测且通过，不将历史锁屏当恒常阻塞。上述既有测试不证明新增第二sampler通用像素输出已验收。

## 独立glyph证据归档

已补完并回读 `Docs/investigations/stage4-glyph-regression-repair-plan.md`。交接同旧SHA BB8B9B9B504C8903275C4C98D4F28119359201B79F2A339D3CFF96457B967934，全量RunIDbc44d9dc-30ac-4c72-aa1a-b22d1f46633b、report11a7f2b729e843c4ad31c7f8865849d9，1299/1/1300；glyph26/26、联合30/30、原版1/1。固定TRX后来由联合Runac1edbe6-9d10-46fc-a3d8-3692e88ac7aa覆盖。fileSHA/face0/glyph36/em16/advance10.671875一致；displaygamma1.8/contrast.5，原生contrast+gamma-table0预测3072bytes差0。历史151未复现，不拟合gamma、不改golden、不声称根因修复。

## 未闭门禁

VisualBrush真实快照与BitmapCacheBrush身份/失效/替换/close/空target1x1透明仍未接sampler。新增ImageBrush全族尚无专门真实publicCOM非恒等第二输入/constants/paddingDdx固定像素及dependency删除/callerRelease/channelclose/失败重试验收。D3DImage通用路径依赖其可获取bitmap的真实内容，纯硬件内容通用映射仍需专项验证；不能拿原窄路径通过替代。软件effect productionexecutor独立后续方案未实现。继续按原完整方法族门禁推进，不关闭阶段4。
