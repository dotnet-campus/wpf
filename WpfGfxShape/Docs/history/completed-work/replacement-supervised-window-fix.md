# P 有界证据收尾：E/G/J/M/O 窗口修复链

## 当前裁决与本包边界

**完整替换未完成，候选真实窗口仍在首帧前失败。** P 仅回读现成证据并修改约定四份文档；未探索编码、修改代码/项目、构建、发布、运行或执行任何终端、shell、脚本。主监督者仅监督与验收，不把历史子包实施冒认为 P 实施。

## 修复链与证据等级

- E：已修正 0x33 TargetUpdateWindowSettings 在 HWND 初始化前不应提前创建 backend，保留 WindowSettingsBeforeInitializationTests 回归。
- G：已有 0x05 vblank 订阅、重复启用、通道关闭撤销、SyncModeStatus 和 DWM 路径生产修改；初始化前启用、多订阅者关闭及截断包有测试。**VBlank 真实偏序、精度配对和失败恢复合同仍未闭**，不能用单元通过代替真实合同。
- J：开展窗口首帧诊断，记录候选 Targets.Render() 的 E_NOTIMPL 及调用链调查；旧报告为实施中初稿，不能当最终通过证据。
- M：实际增加 GeneratedHardwareRectangle 及 renderer 分发；solid 矩形曾因默认 Uv1 映射调用 null `_texturedColorSource` 返回 UnsupportedOperation。先前失败刻画单元通过只证明捕获了预期失败，不证明内容绘制成功。
- O：在 GeneratedHardwareBitmapOperations 修复 solid 无纹理映射及三角形实际 bounds。双架构发布成功属于 O 交付事实（P 未重新发布，也未独立回读发布日志）；P 直接回读下列 TRX、identity、业务 JSON 与诊断日志。新首错已推进到 DrawRoundedRectangle，并不代表矩形真实像素已通过。

E/G 历史原始根为 `artifacts/replacement-window-e-2026`、`artifacts/replacement-window-g-2026`；其结果和 SHA 不绑定 O。

## O 顶层 TRX：P 已直接核对

公共根：`artifacts/replacement-window-o-2026/readback/`。

|范围|原始 TRX（相对公共根）|Counters 与边界|
|---|---|---|
|x64 单元|`tests-x64/TestResults/unit-o-x64.trx:108`|total/executed/passed=15，failed=0；限定单元，不是业务内容验收|
|x64 窗口定向|`package-x64/TestResults/window-o-x64.trx:58`|total/executed=3，passed=1，failed=2|
|x86 窗口定向|`package-x86/TestResults/window-o-x86.trx:54`|total/executed=3，passed=1，failed=2|
|x86 单元|未实跑|无可报告通过率，不从 x64 或发布成功推算|

三份 TRX 的 error/timeout/aborted/notExecuted 均为 0。两个窗口 TRX 的 RunInfo 均记录失败退出码 2。通过项是 `WhenFrameIsBlankOrStaleThenContentOracleRejectsIt` 负控；失败项是 Image 状态序列与 solid rectangle 状态序列，候选子角色退出值均为 -1073740771。不得把“一项通过”写成候选内容通过。

## 原始业务证据与首错

矩形业务目录（相对公共根）：

- x64：`package-x64/Results/replacement-rectangle-window/cc27f5fe36ae440abd74f1683655f1fd/`。
- x86：`package-x86/Results/replacement-rectangle-window/6274686093d943a2b4d9e473770b5f77/`。

两格均保留 `original/report.json`、`events.json`、12 个原始帧文件及 source；候选只有首个 source、身份与事件/诊断，没有完成帧报告，`candidate/events.json` 均仅为 cycle=0/create。官方 Image 与矩形角色通过，不等于包含候选的顶层测试通过；P 未逐字节重验全部帧。

Image 对应目录：x64 `package-x64/Results/replacement-state-window/193661d732424cdfbe7cb51d356ca391/`；x86 `package-x86/Results/replacement-state-window/801f12b24d1a4adf9d266341c0213945/`（本包由顶层 TRX 核其失败归属，未逐项重验 Image 业务帧）。

两架构矩形 `candidate/uce-diagnostic.log:1` 均为 HRESULT=0x80004001（E_NOTIMPL），packet 解码为 `Window.Draw depth=5 kind=DrawRoundedRectangle`，随后才是 `Window.Visit`、`Targets.Render` 的传播错误。当前日志支持的最新首错是 **DrawRoundedRectangle 缺失能力**；不将上层 Render 当唯一根因，不外推圆角参数语义已查清。

## 实际加载身份（已回读，未重算文件哈希）

`package-{x64,x86}/Results/aot-identity.json` 与上述矩形 `candidate/loaded-before-show.json` 的 SHA 一致：

- x64 / PointerSize=8：`AF310A91CF68711CCD53D17F89BC1C02A7E339662BFD025AB5B4D6F3907619D5`。
- x86 / PointerSize=4：`5D212B5B42E661ADCF5A16C90CBE5AD565083ADC97DA8FA074D201B74E91322B`。

loaded-before-show 的 ProductVersion 均为 `1.0.0+b4de881638949d7fdbb653f9650fb21fd07bc685`。这些是现成身份文件记录的加载归属，不是 P 重算 SHA、源码新鲜度证明或完整兼容证明。

## 未闭与下一包的硬边界

候选首帧、更新、resize、关闭闭环，以及 HW 像素/恢复均未通过验收；首帧前终止不能说后续各阶段已实际运行并失败。ABI 归档仅限定成功，x86 独立 Bitmap 官方角色崩溃、捕获新内容超时和 VBlank 合同仍独立开放；最终两个正式项目×x86/x64 四格全量未完成。

下一实施包先取得**矩形路径独立、非空且非旧帧的真正内容证据**，与官方独立零阈值比较；同时对圆角实际缺失能力保留参数/调用路径并给出最小复现，在现有层合法最小实现及回归，不以忽略圆角、假 S_OK、fallback 或放宽阈值绕过。不要因真实窗口牵出任意能力而无边界扩展；随后再推进 ABI、功能、时序、资源、恢复五维。测试建设与问题处理两线独立，SHA 只作证据归属，不阻塞测试建设；所有结果不能拼接成最后源码 full。
