# CU fill 矩阵独立最终审查与交接

## 1. 裁决及执行边界

**CT 因关键验收未齐安全撤回 CR 新挂接；CF 有限修复仍在，完整替换未完成。** 不是已证实像素回归，也没有证据可归为工具或环境故障。主代理仅监督；CU 仅用专用读取、搜索及文档写入工具，无终端、直接或间接脚本，无新构建、发布或运行，无生产/测试/项目变更。

已读当前执行规则全文27行、完整方案全文120行、当前基线全文20行，以及 CQ（76行，截断补读）、CR（21行）、CS（20行）、CT（28行）全文。目标仍为两个正式项目、x86/x64 Release、五项关键维度及最终稳定源码适用四格；文档交付不等于替换验收。

## 2. 当前源码独立核查

- `Code/WpfGfxShape/Abi/NativeShapeReader.cs` 全76行：float 输入有限性检查保留；cubic 进入共享 `SoftwareBezierFlattener.Flatten(..., transform)`，输出再 `Apply`；没有 CR 的 DevicePoint/TryRasterPoint/SoftwareFillBezier 整数入口。
- `Core/SoftwareStrokeContours.cs:14–41`：TryRead 无 fillLocalFloat 参数，CaptureSegment 使用 transform.Apply 后保存 float；allowNonFinite 行为仍在。`Core/SoftwareGroupStrokeContours.cs:75–95` ordinary path 仅传 preserveGaps，没有普通 path float 分支。
- **CF fillPrimitive 全链仍保留**：GeneratedNativeStroke.TryCaptureFill 显式 true，TryCapture 默认 false；Group 的参数、group/combined 递归及 rectangle/ellipse 调用传递；SoftwareCurvedStrokeContours 的 ApplyFillLocal 及 primitive 分支仍在。不能将 CR 撤回说成 CF 撤回或整文件历史字节恢复。
- `SoftwareFillBezier.cs` 保留28.4整数实现；生产 Code 范围文本检索仅命中其类型声明，没有生产消费入口。算法实现/测试控制通过不等于生产接通。
- CQ 显式 `CQ_FILL_EXACT_DIAGNOSTICS` 保留；逐字节比较长度、threshold0、official/candidate/official-repeat 非空及 repeat0，父失败断言、B/E与D/F可判别检查均在。真实目标选择没有 separator/be 无条件豁免；stroke 与 fill exact 分开。
- CR 测试仍以 `CS_FILL_ACCEPTANCE` 显式隔离，验证 Old/Native 真实旧 SHA、old/new 不同，再调用完整缓冲 stroke 比较；未 Ignore/skip 或降低合同。CU 未改任何测试。

## 3. CS x86 原件：可复用且不要求无理由重做

本地根 `artifacts/cs-fill-acceptance/final-run-x86/`；task `cc18c3b74fad4a7c94ef499607fd62e3`。父绝对 TRX 为 `C:\CodingAgentSandbox\Tasks\cc18c3b74fad4a7c94ef499607fd62e3\Results\cs-final-x86\TestResults\child.trx`。

CU 亲读监督全文38行及父 TRX 末尾 Counters：total/executed/passed=17，failed/error/timeout/aborted/notExecuted=0；监督 exited/Exit0，OutputCollected=true，无 timeout/cancel/kill/drain timeout/SupervisorError。筛选覆盖 CQ5+CR1+BW1+CK10；**不是 Br/PublicGeometry 必要17兄弟**。TRX 结果段抽读存在工具截断，不声称122行全文均亲读。

stroke/comparison.json 全87行已分段亲读：16个完整缓冲 differences=0、nonempty=true，unchanged=true、baselineIsCurrentSnapshot=false。两 actual identity 各14行全文亲读，同一测试 SHA/MVID，requested=actual：

|身份|实际 SHA256|
|---|---|
|旧候选 Old/Native/wpfgfx_cor3.dll|C4D7D4699F6334AC164DE95573D382807803ED65E4FE42AA05A9559FEA7F44EB|
|新候选 Native/wpfgfx_cor3.dll|FD8B32FAF8D55CF72001DE9ACFE18A57E0814D92C38F63C4FE1D7D45047CD522|
|测试程序集|B04139CA3BAE532E722F090E6759C48E59B61870A2195D79305CD537B3EF694B|
|官方（frames/official/identity.json）|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|

测试 MVID `9ca00196-7748-40a6-926c-dcb22a761fa5`；.NET10.0.13/X86/pointer4，software RenderTargetBitmap，hardwareVerified=false。官方版本 `10,0,1326,50504 @Commit:4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。旧新 SHA 确实不同，非官方代候选，非同候选重复。

frames/scenes.tsv 全11行与 siblings/scenes.tsv 全17行亲读：10+16目标均差0、repeat0、非空；两组 candidate identity 全文确认同一 FD8B 候选/测试身份。CU 未重算原始像素 SHA，也未全文审计 device178/precision2 的 comparison/参数/trace，因此**不声称206个场景全部亲核完毕**。以上通过只绑定撤回前 FD8B，不绑定撤回后源码；保留既有 x86 通过作为后续设计和复用证据，不要求重做已核 x86。未来若实际源码/产物改变，按影响范围补证，不移绑身份。

## 4. CQ 旧矩阵与 CR x64 先前通过：分层保留

CQ 两最终根 `artifacts/cq-fill-exact/final-run-{x64,x86}/`：CU 直接读取父 TRX 末尾与监督20–38行，两架构均5 executed/2 passed/3 failed、Exit2，无超时/取消/监督错误。task 分别 `c83487e1b462477ea949c036bae4083c`、`834ce6996c8c40af85e3f47f00e8ffb1`。这是旧 CF 包真实 red，不是新运行或当前撤回源码回归结论。

每架构正式 fill206=frames10+device178+precision2+siblings16；stroke16另计。CQ报告记 frames/device/siblings 失败、precision/stroke通过，旧 frames 差数 `0,44,0,44,0,44,0,0,0,10`；CU 已核其父计数及当前比较合同，206逐行结果及旧身份表本轮引用 CQ 报告，不冒称所有旧 comparison、角色TRX、原始缓冲再度全文亲核。原件 scenes.tsv/comparison/diff/identity 仍可复用，不删除失败。

CR x64 `artifacts/cr-fill-exact/local-run-x64/Results/cr-local-x64/`：监督全文38行、父 TRX末尾、frames/candidate identity全文直接读取，task `375db6729b3c4e5cbffa991b2317bd98`，5/5/0 Exit0。实际候选 `AB5655A1A567CE586DF31F71FC132E461FE761B2E3573BE51577B8888C60073F`，测试 `F9E12FCE8E2180D356264B4545CED16CB600015E6F55E70F78FED2E79151F80D`，MVID `29e503e5-4495-45dd-83cf-215f5bd2483e`，.NET10.0.13/X64/pointer8/software RTB。stroke/comparison 起始字段 baselineIsCurrentSnapshot=true 已亲读：该次通过不是 CF旧/CR新对照，也不是 x64 最终 CS 同包验收。完整206原始审计未在 CU 完成。

CR x64与CS x86先前通过均保留为真实历史能力证据，**撤回前通过不能绑定当前源码**。CT记撤回后双架构专用 build 成功仅作构建历史引用，CU 未新构建或认证运行。

## 5. 未完成项与下一执行重点

1. 先独立审查接入设计与矩阵覆盖：普通 path 本地float保存、geometry/drawing/group/device 各阶段精度、28.4量化及负半格、有限性、范围和细分步长终止；明确 primitive/stroke 不受污染。有限206不是任意HFD/极值/组合闭包。
2. 优先补 **x64 同包真实 CF旧/CR新矩阵**及实际 old/new/official/repeat 身份、完整缓冲/监督/父TRX；不能以 CR旧5/5的同候选stroke替代。先复核已有产物，缺的再有界执行，不让已核x86进入无信息重跑循环。
3. 补必要 Br5+PublicGeometry12 的17兄弟最终证据并核身份；CS父17不是这组。CH已有CF旧基线两架构17/17仍可复用，但不能移绑CR或当前新产物。device/precision原件审计可继续读存档，不必为阅读缺口先重跑。
4. 再依据审查结果有针对性恢复最小生产挂接，保留CF有限修复、零阈值和失败档；源码稳定后按实际影响补必要证据。禁止泛化“再跑全部直到Green”，也不能先恢复整套未核补丁。

当前生产原44/10问题仍开（撤回后无新渲染运行，不冒称重现），nested/stroke既有差异仍保留。BD旧x64 full1740/1699/41 Exit2、x86 full无父完成TRX仍是历史，均不能绑定最终稳定源码。最后稳定 Tests/ComAcceptance × x86/x64 Release 四格 full 缺口仍在。

GPU真实内容/backend、实际device-loss/resize恢复新帧、持续媒体、长期CPU/GPU资源/线程/callback关闭、通知target同步/非零SyncModeStatus/迟到消息、圆角、合法大shader、同步Event0/E_HANDLE及首次堆损坏因果等继续开放；不能因有限fill证据消项。retail Closed、私有PRERELEASE非目标等既定边界不变。

## 6. 文档交付

新增本报告；三共享文档前置CU最新节，所有历史保留。四文档写后按小段全文回读至末尾，工具截断处缩小范围补读；此项为文档核验，不产生新运行证据。结论始终为：**CF有限修复保留、CR挂接已撤回、关键验证缺口尚存、完整替换未完成。**
