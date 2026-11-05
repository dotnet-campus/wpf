# BB：AX / AY / AZ 独立证据验收与最终文档

## 范围及总裁决

主代理只监督。BB仅专用读写搜索、只读证据及规则方案、写五份指定文档；未改代码/项目/参考数据，未构建发布运行，未使用终端/shell/直接或间接脚本。先补完[AY报告](replacement-supervised-curve-boundaries.md)，再建立本审查与三个入口的优先状态；AX/AZ/BA报告未改。

已读 `Docs/handoffs/current-execution-rules.md`、`Docs/wpfgfx-complete-replacement-validation-plan.md`、连续性协议、当前三个入口及AX lifetime-fixture-full、AY curve-boundaries、AZ reference-revalidation。**完整替换未完成；AX x86全量仍未完成，最终集成源码Tests/ComAcceptance × x86/x64 Release四格完整适用full仍未交。** 以下是旧产物的独立回读，不是BB新实跑。

## AX：亲核全量及实际托管身份

证据根 `artifacts/ax-lifetime-fixture-full-20260721/`。

- `full-x64/TestResults/ax-full-x64.trx:25533-25536`亲读：ResultSummary=Completed，total=executed=passed=4254，failed/error/timeout/aborted/notExecuted及其余非成功计数均0。只绑定AX快照，不拼为最后源码full。
- `full-x86/TestResults/`亲列只有部署目录与 `log_261019134903890.diag`，无parent `ax-full-x86.trx`。亲读diag:39310-39315，末时间13:49:06.5670573+00:00，仅开始 `WhenBeginSceneFailsThenHResultIsPreservedAndSceneIsNotCommitted`，参数-2005530520；末用例不是根因证明。没有完成TRX，不报全量通过/失败数；未取得数值退出码、异常栈/dump或明确timeout标志，不能把1800秒上限写成已观测超时或访问冲突。
- 两份 `full-{arch}/Results/av-ownership-assembly-identity.json`共42行全文亲读：实际.NET10.0.13，X86/4、X64/8，ProcessPath与程序集Location均位于各实际任务根；不是预期发布路径代替加载身份。

|架构/实际任务|程序集|SHA256|MVID|
|---|---|---|---|
|x86 / 366113f411974057a8bfa13e2189f7d3|WpfGfxShape.Tests.dll|871338D4ABDC4A5D1703BC22940293F4F68216B28760F3667506462C0200DC01|ac2724df-e5a7-483b-b531-fbde8037e8de|
|同上|wpfgfx_cor3.dll（托管）|5DCF389A383DDA8F93B431337DF73F63806C1A62C5EB41CFEFED52F281DC6011|24804a9c-7e6a-476e-bab8-3510a846cbe0|
|x64 / 40c7e59342bc483e846fa30fca372746|WpfGfxShape.Tests.dll|F7EFBF113CEBA57B950FCDBAD0E21B78654CE2DAEFA78D2B8CDF07DDF08B5EC2|deb10e96-de94-4c4b-b1d7-53952a8f4a80|
|同上|wpfgfx_cor3.dll（托管）|1E2044AFE0D94BE358447C52CD27DF999DA7E427B5DEF215D6D383304A35D47D|1f681ee5-4a6d-40c4-9d3b-d2751a0906dd|

AX报告的生命周期5槽最小修复、x86最小独立两次2/2及双架构lifetime/interface族22/22为报告交接，本次未逐份亲读其TRX或重审源码；不能用这些局部结果替代x86full，也不能继续沿用AW“ObjectsLifetime尚4槽”的旧当前状态。

## AY：审计输出已取得，无生产修复

证据根 `artifacts/curve-ay/`。已补完初稿，不再仅写进行中或沿用AT传输失败。亲核路径、四份角色identity、两个aot-identity、完整summary及双架构失败parent TRX见[AY最终报告](replacement-supervised-curve-boundaries.md)。

- `final-audit-readback/output/x{86,64}-current-readback.txt`全文：原frame1/3/5各44字节、frame9为10；预量化三干预（prequantized-double、integer-schedule-exact、integer）对frame5..9两角色均0差。
- 负半格below为14差，exact/above为30差；显式非整数变换double70/object-prequantized80/device-prequantized0。small/large-clipped有限样本nonempty且0差。
- `readback/{x86-final,x64-final}/TestResults/ay-curve.trx`全文：各1执行、0通过、1失败，退出2。软件RTB，hardwareVerified=false；无生产修复，原44/10继续开放，GPU圆角未验证。
- 实际.NET10.0.13；候选SHA x86 `0FCA4287C3E44CB7A7B0801D297C6D4E85F4AD84402D8B9580D0725E8449741B`、x64 `C8C35CE4872FA659B02F8731F34F1F0492775D30879DEC17E54779F06228BE01`，不混作AZ AP归档候选或最终源码身份。

BB亲读既有审计结果，未运行比较器重算全部原始像素/轨迹/SHA；完整HFD、float舍入、真实WPF变换栈仍未闭。显式变换顺序实验不是实际WPF变换栈证明。下一生产切片限定fill设备空间量化，先核float舍入与真实变换栈，而非向共享stroke盲加。

## AZ：新官方参考的生成与真实消费

证据根 `artifacts/az-reference-revalidation-20260721/`。

|亲核parent TRX|位置|结果|
|---|---|---|
|final-official-x86/TestResults/az-final-official-x86.trx|40-43|Completed 2/2|
|final-official-x64/TestResults/az-final-official-x64.trx|40-43|Completed 2/2|
|final-revalidate-x86/TestResults/az-final-revalidate-x86.trx|101-104|Completed 14/14|
|final-revalidate-x64/TestResults/az-final-revalidate-x64.trx|101-104|Completed 14/14|

四份Counters均total=executed=passed，failed及所有其余计数0。生成TRX虽随消费包保留，不累计为额外运行。14项构成为原4 glyph + 原3 DrawingImageVisualMask + 原6 RotatedDrawing + 新身份1，分类来自AZ报告；BB亲核parent总数及实际filter，不冒称本包逐项重新执行。

亲读两份 `final-revalidate-{arch}/Results/aot-identity.json`全文及 `az-reference-consumption.json:1-105`（身份与GlyphRaw来源）：

|架构|最终消费任务|AP归档候选SHA256|官方SHA256|
|---|---|---|---|
|x86|44610a69bb9e491bb764349540c4cf0c|AB8EEB23982411FB5F74CB0F65ECB7D1E0804A542B8066FB3309ED4DE89B81BF|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|
|x64|0ef14cb62d344054b13c30af97f36b12|6F8307E80245F2E84026608613816F5E6962730520833108A8788CA5507AD33F|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|

实际.NET10.0.13、X86/4、X64/8；候选在消费任务Native子目录，官方在任务根，官方版本10,0,1326,50504。测试程序集实际SHA x86 `0303591504F4FC8370FF6ED0EC649A79CA065E9C7D820C86E6AC9D83DECA3BAA` / MVID `e3ec87d7-c42f-40d3-9eae-7403da780270`，x64 `44A593E3F049D20F06A036C73E6FA409DE11390FFB4F006640234BD43CC30138` / `f9ac1a94-0a2d-45f2-9bc4-fe9f4e28ca56`。

实际消费绝对路径为 `C:\CodingAgentSandbox\Tasks\<消费任务>\ReferenceData\glyph-arial-original.json` 和 `original-wpf.json`。Glyph参考SHA x86 `DE16BA19135234E1D99E7DBC380076FC28A3CBB481F1D2A74324968CC0A7DC52`、x64 `2C229560BECDA1E5DFF8FCD7CBC61D61F61603887FFC5757A031D91A1CB50273`；drawing参考SHA x86 `3B56FAEC09BC0C53F2970F95641C1F81F16BAD8BAF19D9100AC6903458138FBB`、x64 `F2B1FE085EF90147F4DECDDB6376EEAC8E0166D025DC1AA366BB33DFCB9ADC49`。GlyphRaw来源分别仍指生成任务04429ec5a1ef476ab6d3003accd0fc15、df8452791ab746ceb7aa9e6618e0eead，OriginalLibrarySha256与消费官方一致；不是候选生成预期或仅检查文件存在。Drawing全部450样本及原始像素本次未逐字亲核，生成/拷包流程和分组断言沿用AZ报告，不造独立重算结论。

根`.gitignore:55`有artifacts/规则，参考仅在ignored产物中生成/消费，不写tracked ReferenceData，不改golden/阈值/skip。AP归档不是最终源码凭证。原6 RotatedDrawing这次在AP归档复验通过，不能继续笼统写从未复验；也不能假称已证明历史present失败根因或将其归为缺参考。原4 glyph/3 original-wpf缺文件的历史失败不改写；本次目标组有界通过，**不外推251/251或最后源码四格full**。

## BA并行状态与下一阶段

BA scene诊断按本包验收范围仍标**进行中/待独立验收**。只读时BA文件已出现结束报告文字，声称scene44→45槽、x86 full有10失败及x64通过；BB未亲核其原始TRX/身份，不将该并行报告纳入本包已验收结果，不覆盖AX x86无完成TRX的历史现场，也不修改BA文件。主代理应接收并独立验收BA最新结果，不能以“进行中”否认文件已出现结束文字，亦不能预判全绿。

1. 对BA终态做独立证据验收并处理仍失败项；保留默认并发、真实失败、parent TRX及实际托管身份，禁止无限无信息full重跑。
2. 曲线先核float舍入/真实变换栈，生产修改仅限定fill设备空间量化，再做负半格/非整数变换/原44及10零阈值官方差分；共享stroke和完整HFD无证据不盲改。
3. 稳定集成后，以最后源码和正常依赖图交付Tests/ComAcceptance × x86/x64 Release四格完整适用full；任务内绝对TestResults、parent TRX、actual identity及内容/时序/资源/恢复/异常证据齐备，不拼AX/AM/AR快照或AZ局部Green。

Capture WAIT_TIMEOUT/resource0、真实屏幕HW/duplication、真实loss→Reset→新色HW消费、持续媒体、长期CPU/GPU/线程/callback及异步关闭、GPU圆角未验证和通知有效target同步/非零SyncModeStatus/迟到消息等剩余合同继续开放。软件RTB、局部计数、构建成功和本包文档闭环均不能关闭这些关键门。
