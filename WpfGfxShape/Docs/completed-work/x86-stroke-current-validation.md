# x86 stroke 当前最终验收

## 根因及 ABI 边界
`Abi/NativePlainPen.cs` 的 NativePlainPen / NativeRealArray 使用 Sequential 自然布局，依据独立原生 `cpen.h` / `dynarrayimpl.h`；指针及 InitialAllocation 为目标宽度，不得硬编码 x64 偏移。原生 C++ shape/figure 成员调用在 x86 使用 thiscall，x64 落入统一 ABI；managedShape 自有函数表保持自身调用合同，不把 COM Stdcall 或公开 packed Pen 描述误当 CPlainPen 布局。公开参数结构独立定义与内部 ptr-aligned 结构是不同边界。
最后两族测试夹具的资源指针包从固定 16 字节改为 `8 + sizeof(nint)`：x86=12，x64=16；修复的是夹具发送长度，不是放宽产品协议。

## 最终实跑与回读
以下路径均相对 `WpfGfxShape/artifacts/sandbox-readback/`，仅回收 Results；无新测试/工程，无阈值、golden、skip 修改，实跑后没有源码尾改。

| 架构/范围 | 最终结果 | 原始 Results |
|---|---|---|
| x86 Pcore | 8/8，各 default/false/true 共24角色对照组 | `x86-stroke-current-pcore/Results` |
| x86 geometry + WidenOutput/PathArc | 14/14 | `x86-stroke-current-geometry-retry/Results` |
| x86 dash/pen image-mask 参数族 | 119/119 | `x86-stroke-current-dash-pen-targeted/Results` |
| x86 SoftwareShader + FloatMaskSpan 精确方法族 | 49/49，0失败/跳过 | `x86-stroke-current-fixture-verified/Results` |
| x64 同源码 Release 发布后目标回归 | 188/188，0失败/跳过：geometry12 + image-mask119 + 两夹具49 + Pcore8（各三模式） | `x64-stroke-current-final/Results` |
| x64 geometry 尾项，与 x86 同范围 | 2/2，0失败/跳过；合计 geometry14 | `x64-stroke-current-geometry-tail/Results` |

x86 首次误选 publish 目录缺 `aot.sha256`，49项在 AssemblyInitialize 中止，记录保留于 `x86-stroke-current-fixture-final/Results`；改用现有完整验收包后49项真实执行通过，不改源码/身份门。旧过宽 Pen filter 96中27失败不再作为本轮最终结论；精确两族49项全部通过，相关失败已在该范围消除。没有跑1690桌面full，亦未把历史full绑定新SHA。

## 模块身份
两轮实际 .NET 10.0.9。TRX Counters、aot-identity、Pcore actualNativeModules 已回读（primitive三模式候选及default原版）：
- x86：ProcessArchitecture=X86、PointerSize=4；candidate `3CC69D4F32E5411F81A2A50F6BF1CD23876E6F152524C44C52CCA6B94EF7220C`；original `0CC79CC937B17827344B8652556A4CFD774CFF64B7FD2709EF5284C480894047`。
- x64：ProcessArchitecture=X64、PointerSize=8；candidate `2BFC4AC24A83EBAF45A4B375CDC5FE1F9C90BD9370D73BD590C259091C5BF58F`；original `098D9CEFACA7B1E36460EB28F2725D7AFDB8E93855C29F8DD3627A1FB692AE8E`。
原版 WPF `10.0.9-servicing.26270.113`，commit `901ca941248413c79832d2fdbd709da0c4386353`。新x64生产与验收项目均发布成功；x86沿用已发布最终修正包。各次运行身份来自实际回收文件而非预期路径。

## primitive 非空证据
现成 Pcore 主程序通过 BCL `CopyPixels` 实读4096字节，校验背景 `[255,255,255,255]` 与(3,3)红色 `[0,0,255,255]` 后才完成。已读双架构三模式候选 result 的4096字节及Green、独立 comparison 的byte-exact/threshold=0/Equal；所有像素 SHA 为 `7F55E138C9605464AE90811BD28B5477871BB3E1AA71F5FD3A1C8B052E0C308B`，default原版相同。另实际加载x86 default原始 candidate/image.png，具有可见非空内容；不是全透明相等。没有新增像素探针工程或测试尾改。完整逐文件全场景独立审阅不在此有限复核声明内。

## 未闭边界
旧“x86 E_NOTIMPL 未修/尚未实跑”不是当前事实。仅两个正式 Tests；无第三Host。保护状态额外硬化语义仍有限，原生额外检查/异常触发仍 unknown/Open，本轮不扩展。retail 已 Closed，私有 PRERELEASE 协议不是阻塞。Debug/Release全配置ABI/ESP、完整生命周期与真实HW仍未满；历史HW失败保留，不能以本轮有限双架构Green宣布整个总任务完成。
