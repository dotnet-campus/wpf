# 完整替换首包：初始实跑与阻塞

## 裁决
本包未完成，不能宣称完整替换。已实际运行当前候选，但双架构完整基线、历史32项逐项裁决、生产修复及最终复验尚未完成。未修改生产代码、golden、阈值或 skip，未改写历史基线身份。全程使用专用发布及 Windows 沙箱；无脚本、原生探针或第三 Host。

## 项目与五维盘点（初步，非覆盖闭包）
正式测试项目仅 WpfGfxShape.Tests（net10.0，MSTest 4.0.1，引用生产）及 WpfGfxShape.ComAcceptance（net10.0-windows/WPF，同 exe 含 Pcore/PublicAbi/player 隔离角色）。生产为 NativeAOT shared library。global.json 要求10.0.107/latestFeature，本次发布实际 SDK 日志路径10.0.402。

| 维度 | 现有入口 | 本包证据边界及后续包 |
|---|---|---|
| 二进制 | AcceptancePaths 身份、PublicAbi、GeometryUtility、COM/vtable 夹具 | x64 full 已跑；x86 私有 context 夹具仍硬编码 x64，须独立 ABI 修复包 |
| 内容 | BitmapRenderTarget、CompositionEffects、Glyph、FloatHdr、ShaderSampler、Pcore | x64 full 1675通过/2失败；x86 已出现像素差异，零阈值保留，须最小化诊断包 |
| 时序 | AsyncUceChannel、AsyncDrain、CompositionNotification、EventProxy、Media callback/rebind、Pcore dispatcher | 已有测试入口，不代表所有关闭竞争及偏序闭包；需专门序列包 |
| 资源 | GenerationOwnership、TypedLifetime、ManagedStream.Lifecycle、BlurLifetime、Media.Lifetime、cache 双 device | 单元和部分真实消费者互补；预热后趋势、关闭后收敛仍需专包 |
| 恢复 | AdvanceFrame/Resize 单元、cache configuration retry、shader channel/device 重建 | 重建或注入 HRESULT 不证明实际 device-loss；真实触发、恢复新帧及连续 resize 仍未覆盖 |

## 实际执行
证据根相对 WpfGfxShape/artifacts/sandbox-readback/。

- replacement-package1-x64-full：沿用当前生产包（生产未改不重发）。完整现有 ComAcceptance 1677 executed，1675 passed，2 failed，0 notExecuted。已回读 Results/aot-identity.json 与 TRX Counters（12457行）。TRX：Results/Administrator_EN-DEV-01_2026-10-18_12_25_00.2248196.trx，RunId 72f27c47-c696-4eb5-bdb1-3418b45845b3。
- replacement-package1-x86-full：验收应用以 Release/win-x86/self-contained、RuntimeFrameworkVersion=10.0.9、ComAcceptanceSandbox=true、AotRuntimeIdentifier=win-x86 发布成功后执行完整入口。运行未完成，无最终 TRX/总计。控制台已到675通过/114失败，之后还有失败，不能将此进度当最终分母。已回读 identity。失败含：私有 context 仍用376/400/408等 x64偏移；InternalDrawBitmap/Float 直接断言 IntPtr.Size=8；原版 FloatDraw 黑色与预期不符；FloatSpan 零像素；source18/affine3 mask 有132/133等1字节差异；校准报告85条 differences。不能将其全部归环境或全部判生产错误。需隔离危险夹具、修正独立布局后重新完整运行。
- 托管 x64：Release/win-x64/self-contained/10.0.9 发布成功；沙箱实际输出4228总计、4228通过、0失败/跳过。首次 Results 回收失败；诊断运行回收根 replacement-package1-managed-x64-diagnostic。TRX 实际写到远端 C:/lindexi/WinRemoteShell/Results 而不在任务目录，故本地尚未回读 TRX，不能称完成证据验收。
- 托管 x86：相同配置发布失败；专用发布日志 MSB3021/MSB3027/MSB3491，明确“磁盘空间不足”，未执行测试。
- replacement-package1-x86-hw：尝试定向 WhenStage4/PublicHwndTarget/ShaderChannel/NativeGpuPresents/SecondSampler，回收因本地磁盘空间不足失败，无可读 Results；不记通过。

## 当前实际身份
| 架构 | 候选 SHA256 | 实际进程 |
|---|---|---|
| x64 | 2BFC4AC24A83EBAF45A4B375CDC5FE1F9C90BD9370D73BD590C259091C5BF58F | X64 / pointer8 / .NET10.0.9 |
| x86 | 3CC69D4F32E5411F81A2A50F6BF1CD23876E6F152524C44C52CCA6B94EF7220C | X86 / pointer4 / .NET10.0.9 |

x64 本次 blur/default 角色业务 JSON actualNativeModules 已回读：官方098D9CEFACA7B1E36460EB28F2725D7AFDB8E93855C29F8DD3627A1FB692AE8E，fileVersion 10,0,926,27113，productVersion 10.0.9-servicing.26270.113，commit901ca941248413c79832d2fdbd709da0c4386353；候选fileVersion1.0.0.0，productVersion1.0.0+b4de881638949d7fdbb653f9650fb21fd07bc685，4464640字节。路径 Results/pcore/blur/default/988a866ddaa0421098235d32eea3d2e4/Results/{original,candidate}/result.json。已读多帧4096字节/665、777 coloredPixels 及相同SHA，支持非空和更新，不外推其他场景。
x86 本次未取得官方实际模块版本/SHA角色证据；历史身份不补作本次身份。

## 历史32项 HW 重新裁决（尚非逐项关门）
x64 当前完整 TRX 仅2失败，其余测试执行通过；历史 cache15/PublicHwnd8/shader3/SecondSampler3 在本轮不再呈现原失败集合。但尚未逐项完成原始业务及官方独立路径审阅，因此不签署29项全部关键合同闭合。

NativeGpu3 中2个 swapEffect(1/3) 的 FrontBufferAndClientPixelsAgree 失败：staging=FF123456，frontPixel=FF123456，屏幕采样00563412；Desktop Duplication 创建成功并返回00000000。必须保留为未解释的捕获链/时序差异，不能据此改生产或放宽断言，也不能将原生控制失败算候选通过。x64 shader channel/device重建用例已回读 TRX Passed 及两个真实 HWND 的屏幕像素/production SHA，但这不是实际 device-loss 恢复证明。

## 根因与修改边界
本包没有生产修复或新增稳定回归。已确定若干测试仅支持x64：BitmapRenderTargetTests.Realizer/BitmapRealizer/InternalClip/InternalLayers 的固定context偏移，与生产 NativeContextPrefix 顺序布局在x86不同；尚未完成独立原版布局核实、calling convention审计及回归，不做猜测补丁。x86 1字节内容差异及捕获黑帧根因仍未知。

## 阻塞与建议后续包
1. 先恢复工作区可用磁盘空间；不删除历史证据、不扩大清理scope。本包未进行清理。
2. 在现有两个项目内解决结果目录相对宿主工作目录的回收问题，确保最终 TRX 在 ignored 任务路径且实际回读。
3. x86 独立 ABI/context/realizer 夹具诊断包，随后补全双架构四象限完整基线。
4. x86 source18/affine3 与 FloatSpan 最小化差分包；确定生产根因后修复并补稳定回归。
5. Desktop Duplication 帧有效性/呈现时序诊断及历史32项逐项证据审阅包。
6. 真实 resize/device-loss、新帧、关闭竞争与资源趋势包。
最后源码修改后必须重新实际运行适用双架构矩阵并回读身份、TRX及业务结果；本报告不是该最终验收。
