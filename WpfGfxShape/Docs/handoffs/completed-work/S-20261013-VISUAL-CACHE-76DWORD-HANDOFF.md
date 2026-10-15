# Visual/Cache 76 DWORD 双输入交接归档

状态：Partial；阶段4 Active。此文先保存用户提供的真实验收身份，不冒充本轮重新执行或独立报告回读。

## 交接身份

- SHA256：B9069161364DE6F0DC97759EC5CE9C3B084817AB7278D7C76E5E05EF5B8CB338。
- COM full：1302 Passed / 1 既有 FLOAT Failed / 1303 Total。
- RunID：292900b2-8201-46b9-bdd3-a16fe0af1efc。
- report 身份：ef578f0dba45464da8d0eba73a613334。
- 76 DWORD 两输入真实 Visual/Cache、sourceMutation、null cache 原独立 expected 已通过（用户交接）。不再将 27 DWORD 创建失败作为当前阻塞。
- 本轮尚未重新 publish；交接未提供发布命令、产物路径、普通 full 或定向实例计数，不补造。
- 本轮按文件名搜索 report 身份及按内容搜索 RunID 尚未找到工作区证据文件；独立文件回读状态待验证。

## 有限剩余完成条件

1. root Visual outer 的 transform/offset/clip 与 Cache root content 分别验收真实像素。
2. handle delete / channel close 引用寿命与失败 realization 后重试。
3. 首个外部 callback 前所有 sampler 参数及 effect/shader/version 引用冻结。
4. Cache source change version key 复用已通知状态；以真实重绘次数证明未重复重绘，而非只检查像素。
5. 原生 BadShader policy 的实际通知、真实有效 fallback 输出及 OOM 首错传播；公开 malformed command → render → notify/fallback，无 dummy。
6. 软件 effect 真实执行；不以 S_OK 或忽略 effect 宣称 Closed。若有技术阻塞，记录具体符号及独立探索问题。
7. 最后发布 SHA 与全部新增精确定向、普通 full、COM full、RunID/report 对应，并同步 mapping/ledger/stable/workitem/remaininggap 后回读；不保留未测生产修改。

不扩展到未来 cache model，不改变独立 expected，不吞错或将失败转换为假通过。

## 本轮实际复验（无生产修改、无新发布）

- 首次筛选 FullyQualifiedName~VisualCache 未发现任何测试：明确不是 Passed 验收。
- 改用完整方法名 WhenSecondSamplerUsesVisualOrCacheThenNonIdentityShaderCombinesBothInputs：3 Passed / 0 Failed / 0 Skip。读取 D3DImageTests.VisualCache.cs 确认 76 DWORD 字节码及三个原独立 expected，未修改。
- 普通 full：4197 Passed / 0 Failed / 0 Skip。
- COM full：1302 Passed / 1 Failed / 1303 Total / 0 Skip；运行日志正常结束，无 abort。
- 本轮 full TRX 回读 RunID：45e06e99-7f6a-47fb-b83a-b411c6c9b567。
- 本轮独立报告：Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-6a14f127c9d94aebacebc51fa483ebb4.json。
- 报告实际回读 AotDllSha256：B9069161364DE6F0DC97759EC5CE9C3B084817AB7278D7C76E5E05EF5B8CB338；FLOAT 57/450，Direct 0/180；没有新增回归。
- 默认 AotDllPath：artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。本轮仅复验已有发布，不声称源码重新发布或新功能完成。

## 具体生产缺口与独立探索问题

- GeneratedHardwareShaderEffect.RenderFrozen，约46–55行：TryCreate bytecode / TryCreate pixel shader 的失败直接 return hr；Code 目录搜索 BadShader 无命中。原生 core/resources/PixelShader.cpp:205–220：E_OUTOFMEMORY 或 D3DERR_OUTOFVIDEOMEMORY 原错传播，其他硬件创建错误设置 SetPendingBadShaderNotification 后创建真实 pass-through。必须接线 composition backchannel 并按 bytecode/version/device 拥有 fallback 引用；不能在此直接将错误变成0。
- GeneratedVisualRenderer.Capture:531–532：visual.Effect != null 返回 E_NOTIMPL。GeneratedHardwareShaderEffect.RenderFrozen:35–36：SoftwareOnly 返回 E_NOTIMPL。独立探索问题：按原生 CPixelShaderCompiler/GetSwPixelShader 的合法指令范围及采样/常量合同实现实际软件 shader 执行，并接入 DrawingSnapshot/VisualSnapshot；不以硬件偷偷替代 SoftwareOnly 或忽略 effect。
- GeneratedHardwareShaderEffect.RenderFrozen:73–78：顺序调用 SoftwareBrushRealization.Capture。effect/shader/sampler资源 AddRef 不等于全部 sampler 值冻结；前一个来源外部 COM callback 可以改变后面的来源。需明确两阶段不回调参数快照/依赖租约，再实际 realization；现状不得认定全冻结已验收。
- GeneratedBitmapCacheBrushResource.AcquireCacheBitmap:295–303 / 327–334：key 含 BrushChange、TargetChange，且在 candidate realize 后校验版本才保存。source pending change 在 realization 内通知导致首次 candidate 不缓存、次帧重绘的风险必须用真实来源调用次数验收后修复，不能只凭 pixels 一样判断复用正确。
- root outer/content、delete/close refs 与失败重试新增正式门禁尚未实现。本轮没有修改生产代码；有限闭环仍未完成，阶段4 Active。
