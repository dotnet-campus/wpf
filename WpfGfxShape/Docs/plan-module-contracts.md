# 总计划主题合同：A/B/C、消费者与独立 E2E

本页承接[总计划](remaining-gap-closure-plan.md)原“必须关闭的A/B/C与真实消费者差集”“自主依赖与外部前置”的完整条件；不是运行状态源。旧有限结果的适用范围见[基线](handoffs/current-stable-baseline.md)。

## A FrozenGraph/Realize

- root outer/content、pure owning capture、整树两阶段冻结。
- child/brush/geometry callback、Image/Visual/Cache 可达消费、冻结 ownlease。
- CopyPixels/重绘/失效计数、失败候选旧键恢复/重试、首 HRESULT。
- 后验 key/version、逆序释放、真实 HW+SW 旧/新帧。
- 已有 devicekey 和失败保旧不能反复当新交付，也不能代替完整条件。

## B cache/shader 失败

- generation/per-device cache、non-OOM/BadShader 通知、fallback/OOM 原生有限表。
- 真实成功/失败消费者寿命。
- 协议解析、A 或有限 pass-through 不替代 sampler/constants、malformed/type。
- padding/Ddx/inputscope/HDR、Auto/Nearest/null/white/DEF 及 compiler 完整合同。

## C GenProto required

- 历史 CLR/冻结输入 authority；正式布局/执行仅x86/x64。ARM64按[DEC-0005](decisions/DEC-0005-x86-x64-acceptance-scope.md)不适用，非Passed、不阻塞。全部其他required/physical条件保留；精确runtime patch口径见[单一审计](windowsdesktop-version-provenance.md)。
- core managed field/enum/pack/size、X86 runtime 或固定宽度独立推导。
- generated/mapping/consumer physical record 闭包；ProcessCreate 错误/引用/关闭及 InitializePen ABI/dash/stride，全族物理 records 依赖签名和逐 required 更新。
- 有限全量测试不替代 physical/required、satisfied 与 stage4Closed 的正式关闭。

## 消费者差集（原计划逐项保留）

### UCE

既有有限 FIFO 像素、首错、disconnect/channel owning、failed transport 释放、FIFO drain/OS 退出、跨 channel callback/initref 证据不自动关闭独立取消及全部异常注入。命令传输 PE 闭包不等于所有失败引用清理。

### DoubleBufferedBitmap / D3DImage

双缓冲两帧有限路径不关闭全格式、替换/失败/重入清理。
D3DImage 必须真实 HW color-source 复用、manager adapter 通知、interop bounds、availability/fallback、device-loss/recover、无锁重入/并发注销/替换丢包、surface/device 引用及重试。
既有 dirty 像素/callback 有限证据不重复领取；Detach 或 SW 快照不替代 GPU loss。

### 媒体

软件媒体、generated 软件生命周期/替换及真实 SystemMedia 传输两模式的有限证据，不关闭持续异步/HW 通知→sample→composition/target→替换/重试→release→并行/关闭。
不以 fake provider/GCHandle 替代真实原生指针。

### source17/18 DrawingImage FLOAT/HDR

同版同格式 oracle、FLOAT 坐标链、旋转 intermediate tile/mapping/span、Visual mask、有界超大网格与失败寿命全部保留。
历史 Pbgra32 零差异不能覆盖历史 FLOAT 差异，不全 8bit 降 float，撤回候选不计完成。

### glyph

有限非空整面消费、coverage quantize 原生半向上、DrawGeometry padding、RGBA wire 修复不代替完整族。
严格原版未初始化 padding raw 保留；历史 raw/字段/release 证据不关闭复杂脚本/侧向 bidi、ClearType、动画/brush/仿射及通用异常/callback/OOM 全寿命。

### perf / 通知

- 同步 signalPresent=false、异步 signal bit2 保留。
- 旧 SHA 通知、detach、关闭/FIFO/worker/callback 及 0x88980406 sticky 证据保留原身份。
- 合法结构 72 bytes；原生 Release 对不足 72 字节读取存在 OOB，旧短包接受/通知数不是 oracle。
- 候选安全检查有发布不等于最终 same-SHA 本族/related/full/strict 已收口。
- 设备 resize/Present 失败恢复、OOM 及未证并发异常注入仍须验收；包验证失败不能冒充设备失败。

## 自主工作与外部依赖

[唯一外部清单](all-completion-external-prerequisites.md)维护具体包内容与验收。player 包头/dispatch/destroy 缺合同不猜布局。
其余生产消费者、政策消费/恢复、UCE/资源失败清理、呈现失败因果/恢复、glyph 完整寿命、异步 perf、GenProto 可自主部分与 E2E 接线不得因外部确缺全部停工。
[DEC-0004](decisions/DEC-0004-process-lifetime-module.md)明确驻留，unload/reload/热替换不适用（非通过）；Release、关闭、线程退出、callback 静止及初始化失败清理仍验收。
[x86 独立有限证据](investigations/audits/x86-shared-unload-actual-verification-20261014.md)不外推完整 ABI/配置/E2E，不删除 Win32。
阶段6依生产 meta/factory/bitmap/stream/WIC/codec 桥、真实导出与 DLL 合同；阶段7依完整命令/资源链及同 PresentationCore 隔离路径。

## 独立 E2E-01–09（逐项，不合并降级）

| ID | 必须验收的生产链 |
|---|---|
| E2E-01 | 版本/COM |
| E2E-02 | connection/channel |
| E2E-03 | 资源命令 |
| E2E-04 | 软件像素 |
| E2E-05 | DrawingVisual/RenderTargetBitmap |
| E2E-06 | HWND/真实 HW |
| E2E-07 | D3DImage |
| E2E-08 | 媒体 |
| E2E-09 | 多 Dispatcher、重复关闭、进程退出及资源/线程/callback 静止 |

各项均须[测试分层](06-testing-strategy.md)与[可执行证据契约](09-test-evidence-and-e2e-contract.md)规定的独立输入、输出、原版/候选身份、能力/架构/配置矩阵。E2E-00 probe 不替代任何一项。
最终替换资格是兼容原 WPF 库的交付验收，不是运行期换掉已加载 DLL。

## 全迁移完成标准

承接历史总规划§9保留合同，全部同时满足：
- 完整机器 Ledger 中所有生产范围项有最终状态和证据。
- 全部目标架构/配置 ABI、布局、COM、callback、resource、version、symbol 兼容。
- 所有逐文件实现可追溯，无未记录重构或算法替换。
- 原/候选规定差分全矩阵通过。
- 真实 PresentationCore E2E 全矩阵通过。
- 所有适用 P0 门关闭，或用户批准明确范围例外；DEC-0004 将模块卸载改为不适用而非测试通过，不免除资源清理、x86及其余门。
- 只有此后才可描述为 wpfgfx_cor3.dll 兼容替代；现代化、性能优化、架构重构另立计划与基线。

原权威追溯：[历史总规划§9](history/plans/migration-master-plan.md#9-完成标准)，仅查条件来源，不恢复其旧状态或下一包。
