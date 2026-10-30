# 最终候选主要回归 r1 正式收尾

## 正式终态：CloseoutCompleted / PassedLimited；完整替换 Open

本次只亲自回读、编辑及核验文档，不再运行、不再委派。源码/build/tests/Native publish/COM publish/沙箱/终端脚本新增均0。原 r1 双架构八任务已执行完毕，无首错、无重试，预算余额0，不再将 x86 标为待执行。以下原草稿、即时裁决、原件及所有历史失败/预算/截断完整保留；旧草稿中“执行中/待执行/剩余”仅为当时快照，以本节终态为准。

### 当前候选与实际加载身份

|架构|actual candidate SHA256|test SHA256 / MVID|运行环境|
|---|---|---|---|
|x64|632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671|E0BA8257526087597ED093A2F3102E6BB09B5773318E860A4BBF2DCD190F15F6 / 358caf7b-d362-445d-ae09-0da97456cf77|Release self-contained .NET 10.0.13 X64，pointer8|
|x86|147B8D862F315A9E5FB0F00A2BEE8F0ED1DFB258C870E69D3C32C5D3BE9F9304|DA6E29882DD7FA104BCCC38AAEA10C76581C1C9591FCEA50A6D5589184A38C12 / a746e7f6-d5e4-4b9c-b835-22a4401d1e3c|Release self-contained .NET 10.0.13 X86，pointer4|

actual 与 clean/file 预检分层，不用文件 hash 冒充已加载模块。媒体 actual official x64=AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，x86=5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834；candidate 的系统媒体 service/decoder 仍复用相应官方模块，不称独立全媒体栈。

### 已执行矩阵与退出证据

|架构/范围|sandbox task|结果与证据边界|
|---|---|---|
|x64 media|f377f889b4d349f9b2229d7eb503efba|PassedLimited；TRX total/executed/passed=1/1/1，监督 OS exit0、无 kill|
|x64 discovery|a3461c97f65c498aab0ed48ce6dbb61b|取得当前 false/true UID；仅发现，不计正式体|
|x64 KN baseline false|e5ad8187325743aba726c089ace55abe|PassedLimited；TRX 30664c52-0846-4c39-91a7-53fa92dd7ff4，1/1/1；MTP session e85bc651-3171-4d26-9b3f-93c76526de94 exit0|
|x64 KN resize true|7ed2001feaa041079745894eed3fdb51|PassedLimited；TRX d843fdf5-3bd4-489e-a3d3-d46158de9fd6，1/1/1；MTP session 2edddc9a-0b14-4bbe-95e2-ea4381fce499 exit0|
|x86 media|d3380a2b84dc4b9c8ec9da550f6ee122|PassedLimited；TRX b107bb3c-8979-4072-a443-3ef342bd2820，1/1/1；监督 OS exit0、无 timeout/kill|
|x86 discovery|fe2477257f404741b62d797e00ebd117|取得本架构当前 false/true UID；仅发现，不计正式体|
|x86 KN baseline false|23a19ff22feb45a8b02cbab577e1014e|PassedLimited；TRX adc91101-ba92-4498-88eb-b5ea967d53fd，1/1/1；MTP session d1d13530-65b4-49a6-bcf3-91ba9fe6ebc4 exit0|
|x86 KN resize true|f7cf0d095cc242009ef4dfff5e1cc1d6|PassedLimited；TRX be9493b5-d4ff-462a-a3a4-9f27ed696400，1/1/1；MTP session 2b7c054d-8327-435e-8e32-a4839900410e exit0|

KN 两架构均独立发现；false UID=10baaeb0-3ba8-84e6-8830-7058e6fca4ac，true UID=1f3e7305-18e0-806d-82fd-affa104bcbb4。值恰相同不表示沿用另一架构发现。四个 KN 体没有独立 OS exit 监督证据，OS exit=Unknown；仅认证 TRX Passed 与 MTP session 终态，不冒充 OS 监督。程序集发现总数不作执行分母。

### 有限验收内容

- 每架构媒体在同一次 media 沙箱任务内 official oracle → candidate oracle → independent comparator 各1，顺序先官方语义通过再候选再比较；不是三个沙箱。当前候选四全帧各2048、合8192 bytes、threshold0、equal=true。官方独立 dominant-color/事件时序验证不是输入 exactRGB oracle。opened/ended 各2及 parallel/expected-failed-close 合同成立；HasAudio/HasVideo 不证明实际音频输出或显示/HW。
- 当前候选 KN baseline/resize producer lifetime 迁移通过：初始化/target settings/send/close/commit/flush/compose、producer HAL/surface/interop、前后非空 snapshot/cooperative 均0，caller refs1、surface refs2→2，Invalid=false、Resize分别false/true；无 early callback，正常 cleanup callback available0/version701，destroy/disconnect/deinit、detach、bitmap/surface/device/factory release0、window destroyed/cleanup complete。ConsumerPixelsObserved=false、DesktopCaptureExecuted=false。
- 只迁移本次已核媒体及 KN baseline/resize 生命周期合同；旧17项不整体迁移，旧15 managed/此前 acquire-owner 六体按原报告有限继承，不冒本轮新增执行。shader像素、target独立 GetDesc/GetDevice/backend、HW、真实device-loss/恢复、实际音频、资源趋势与 full 均未由此关闭。
- 默认 diagnostic exports absence=Unknown；heap forced-GC 只证诊断可回收，非生产无泄漏。资源继续 PendingInvestigate / MemoryDescriptiveOnly / memoryGreen=null / WorkerExitNotProven；display Unknown，完整替换 Open。

### 预算最终实耗（与本次零执行分开）

r1 累计 COM publish=2/2（每架构1，均包含隐式编译流程，不冒 normal build0 代表没有编译）；sandbox=8/8（每架构 media1 + discovery1 + KN baseline1 + KN resize1），正式体=6（media2 + KN4），发现=2。normal build=0、定向 managed tests=0、Native publish=0。媒体角色每架构 official1/candidate1/comparator1 是任务内部执行层级，不能混作沙箱计数。历史上限 normalbuild≤1、managed tests≤1 未使用，不自动转为新授权。无新增运行、无首错、无重试；所有旧失败预算单列保留，不抵销或倒签。

### 亲读范围、原件路径与本次必要复核

路径以下均相对 WpfGfxShape。正式草稿修改前已读原1–11行；执行总账 `artifacts/final-candidate-major-r1-execution.md` 原1–23行；本次亲读 `artifacts/final-candidate-major-{arch}-r1/{media,baseline,resize}-verdict.md` 全文：x64分别1–4/1–3/1–4，x86分别1–4/1–5/1–4。下列完整原件读取范围是即时裁决记载的前轮亲读，不冒本次全量重读：

- x64 media verdict记载监督40/TRX26/stdout12、两oracle各27/compare34行及业务关键身份/事件/四帧/清理；baseline记载TRX35/identity24/lifetime320/diag75；resize记载相同四份及aot identity1行。
- x86 media verdict记载监督40/TRX26/compare34/两oracle各27/identity1，业务关键365–507及candidate436–514；baseline记载TRX35/identity24/lifetime320/aot1，diag45–75截断后已补66–75；resize记载TRX35/identity24/lifetime320/aot1，diag45–75分段核UID及session终态。原截断、stdout乱码及历史缺失不抹除。
- 本次只必要关键复核：`artifacts/final-candidate-major-x86-r1/media/Results/pcore/media/default/9aebaa48c4534c94b6cddd3d20ee20fb/Results/diff-media/comparison.json` 1–34，确认Green/threshold0/四帧各2048及逐帧SHA一致；`artifacts/final-candidate-major-x86-r1/resize/Results/final-candidate-major-x86-resize-r1/child.trx` 1–35，确认真实task、true UID、Passed及1/1/1；`artifacts/final-candidate-major-x86-r1/resize/Results/kn-inplace-lifetime/resize/identity.json` 1–24，确认actual/test/MVID/runtime/X86/Resize true和像素未观察。未全量重读既核lifetime或重跑。其他不可得日志边界保留Unknown，不无限阻塞已核合同。

### 主剩余门的下一具体执行项（本次不执行）

P0：在这两个冻结候选上，按完整方案§1–10最终适用集逐格绑定actual/test身份，先执行尚未被本次媒体/KN生命周期及既有acquire-owner有限证据覆盖的主要ABI/COM回归，逐格留TRX/业务/清理/退出证据；不得以旧17或旧full整体移绑。下一执行前须另授明确build/publish/沙箱预算，本项余额0，不重复本次六体、不再停在选门审查。随后P1验证实际音频输出、真实显示/HW shader像素、target独立GetDesc/GetDevice/backend及真实resize/device-loss恢复；P2无强制GC自然资源收敛及全部worker退出。未闭这些关键门，不得声称“全部完成/全替换通过”。本项仅正式收尾完成，不是用户完整目标完成。

## 历史原稿（原文保留，不再作为当前状态）

# 最终候选主要回归 r1 正式收尾（执行中草稿）

本轮接续原 r1，不新建选门。x64 四任务已有通过有限证据，不重跑；x86 publish 最后日志11行亲读：exit0，Release win-x86 self-contained，runtime10.0.13。本阶段源码/normalbuild/tests/Nativepublish/COMpublish/脚本/委派均0。累计COMpublish2/2、sandbox4/8，剩余只用于x86媒体、KN发现、false、true，首错即停。

冻结 candidate x64=632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671；x86=147B8D862F315A9E5FB0F00A2BEE8F0ED1DFB258C870E69D3C32C5D3BE9F9304。clean aot.sha256及NETCore/Desktop runtimeconfig10.0.13亲读；file预检不替代运行实际模块身份。

x64 media task f377f889b4d349f9b2229d7eb503efba；发现a3461c97f65c498aab0ed48ce6dbb61b；baseline e5ad8187325743aba726c089ace55abe；resize 7ed2001feaa041079745894eed3fdb51。即时原件位于 artifacts/final-candidate-major-x64-r1；旧hash、所有失败/有限证据保持历史，不自动绑定新候选。

x86待本轮执行回读。媒体只证明官方语义先绿后的四帧8192bytes零阈值比较；实际系统媒体服务与official/candidate模块分层，音频未证。KN仅producer lifetime；target独立GetDesc/GetDevice/backend、非resize像素仍未知。UID执行若无独立OSexit，仅MTP session终态有限认证。

完整替换Open。软资源趋势PendingInvestigate/MemoryDescriptiveOnly/memoryGreen=null/WorkerExitNotProven；HWshader、真实target/device-loss、实际音频及full不升Green。
