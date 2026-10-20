# 唯一当前基线

## 合同
当前任务合同已按用户要求更新：仅 WpfGfxShape.Tests 与 WpfGfxShape.ComAcceptance；Windows x86 Release/x64 Release，全部测试驱动/夹具/探针/比较器纯 C#/.NET。DLL进程驻留，资源/线程/callback仍需清理；官方同版同架构独立对照，无固定runtime合同。ARM64/卸载不适用。目标是完整替换原来的 wpfgfx.dll，关键五维及完成条件见[新实施方案](../wpfgfx-complete-replacement-validation-plan.md)。下列实跑数字和身份没有因合同更新而改变。

## 最新有界实证
本轮确认 NativePlainPen/NativeRealArray 自然布局、x86 thiscall/x64统一ABI及最后两夹具 `8 + sizeof(nint)` 修正。最终运行后无源码尾改。
- x86：Pcore8/8（各default/false/true）、geometry14/14、dash/pen119/119；最后 SoftwareShader/FloatMaskSpan 精确49/49，0失败/跳过。
- x64：同源码Release生产及验收发布成功，geometry12+dash/pen119+两夹具49+Pcore8共188/188；geometry尾项2/2，合计geometry14。0失败/跳过，未运行桌面full。
- TRX/JSON/identity已回读；primitive双架构三模式4096字节byte-exact，BCL实读并强制白底/红像素，原始PNG非空。具体路径及审阅范围见[本轮报告](../completed-work/x86-stroke-current-validation.md)。

| 实际身份 | candidate SHA256 | original SHA256 |
|---|---|---|
| X86 / pointer4 | `3CC69D4F32E5411F81A2A50F6BF1CD23876E6F152524C44C52CCA6B94EF7220C` | `0CC79CC937B17827344B8652556A4CFD774CFF64B7FD2709EF5284C480894047` |
| X64 / pointer8 | `2BFC4AC24A83EBAF45A4B375CDC5FE1F9C90BD9370D73BD590C259091C5BF58F` | `098D9CEFACA7B1E36460EB28F2725D7AFDB8E93855C29F8DD3627A1FB692AE8E` |
实际.NET10.0.9，WPF `10.0.9-servicing.26270.113` commit `901ca941248413c79832d2fdbd709da0c4386353`。原始根 `artifacts/sandbox-readback/x86-stroke-current-{pcore,geometry-retry,dash-pen-targeted,fixture-verified}/Results`、`x64-stroke-current-{final,geometry-tail}/Results`。

## 历史与未闭
旧候选737CDD…的托管4228通过、原生1677总计/1645通过/32失败、ShaderBounds31观测及retail50差分属于[前轮报告](../completed-work/bounds-protection-current-validation.md)，不绑定新SHA或充当本轮full。retail维持Closed，私有协议不是阻塞。
HW cache15/PublicHwnd8/shader3/NativeGpu3/SecondSampler3为历史未闭，不笼统归环境，需在当前候选重新裁决。保护状态无生产消费者，额外原生硬化精确范围/异常条件unknown/Open，如实披露但不独立设为当前完成门。当前仍需关闭双架构 Release 的二进制兼容、完整内容、时序、资源及真实恢复能力；不要求 Debug 或纯 C# 无法可靠直接观测的 ESP/寄存器探针。持续HW/媒体及完整生命周期与全场景独立证据审阅仍未满，整体未完成；旧“x86未实跑/E_NOTIMPL未修”不再代表当前。
