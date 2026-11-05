# CF curved precision 有限生产交付

## 1. 最终事实（CH授权补完）

**保留限定生产修复；CH独立有限验收通过，不是完整替换通过。** CF独占产物根 `artifacts/cf-curved-precision/`（以下B）。仅保留生产三文件 `Core/GeneratedNativeStroke.cs`、`Core/SoftwareGroupStrokeContours.cs`、`Core/SoftwareCurvedStrokeContours.cs` 的fill primitive精度通路；CH没有修改代码、阈值、skip或重发生产。

CF此前记录：读取CE/CD/BZ/BW后调查实际fill命中路径，双架构accepted父各2/2 Exit0。CH现亲读这些父及全部四角色TRX/监督、八份actual identity、双架构完整comparison，补齐事实而不把取证绿当作全产品绿。完整独立范围见[CH验收](replacement-supervised-curved-precision-acceptance.md)。

## 2. 源码边界与风险

`TryCaptureFill`唯一显式传 `fillPrimitive:true`；普通 `TryCapture`、Group.TryRead及两种Curved.TryRead均默认false。该标志递归传到group/combined内部，但仅rectangle/ellipse局部变换改用 `ApplyFillLocal` 的float乘加。stroke调用在GeneratedVisualRenderer及GeneratedImageContentBounds不传true；pen存在时仍走原TryCapture。NativeShapeReader当前仍共享Bezier Flatten后Apply/SoftwarePathCoverage.TryCreateFigures，未恢复整数fill或修改sharedstroke。CH审阅的是所列调用/实现，不伪造全仓diff或历史字节hash。

具体未闭：group parent.Prepend/parent.Apply仍为既有double组合，不是完整分阶段float；rounded控制点仍先double构造再转float；float局部乘加在极值/强抵消/大曲线下的有限性和舍入未全面证明。fill+pen分支沿用stroke快照，不外推该组合已覆盖。仅18场景与默认窄图元回归合格，不宣称所有曲线/group/rounded或HFD已修复。

## 3. CF accepted原件与身份

B/accepted-<arch>/Results/cf-<arch>/ 下父 `TestResults/child.trx`各32行、`supervision.json`各38行；cd-baseline/{baseline,candidate,official,official-repeat}/ 下子TRX各26行、监督各38行、identity各14行均CH全文亲读。父2/2/2/0，八子各1/1/1/0（total/executed/passed/failed）；全部Exit0/exited、非timeout/cancel/kill/drain timeout、SupervisorError=null。四角色同架构使用同一测试程序集，actual=requested。

|架构/task|测试SHA256 / MVID|旧候选SHA256 → 新候选SHA256|
|---|---|---|
|x64 ca93d384103441168360f9e4223fd1a4|4E8686B0A9EDEA962E396D64C6B341E100F9C6BA1799D01758580720BE888F26 / b1c7a71f-d165-4759-bae8-72426523b652|F45485F1C747361D89E31FDB667A2C8C1C4920CE6BD6982A6AE93532C2E5A03C → 36353DDE3BA720B642A6F12CEE8AB0EDDA501BD7DC49F35270063480B8A0532A|
|x86 fb214890d0f84d12acbb3bb66ed917ed|63A80F201DD04A0CD3245A0DBC54DE9FE8D40A10846C7D23465DEBA605232536 / 487d72f5-1ef4-4a56-8b31-98d5aa5a4320|09CD6C360304E02AEFF873BFDCB082CEC8CFC55D0736238C81E378EF1D81FABB → C4D7D4699F6334AC164DE95573D382807803ED65E4FE42AA05A9559FEA7F44EB|

新候选实际在各task Native/wpfgfx_cor3.dll，旧在Baseline，官方及repeat在task根。旧身份与CE/CD current基线一致；来源为可复用CD旧包，不是失踪CB历史归档。新候选version1.0.0.0；runtime.NET10.0.13，X64/8与X86/4，software RenderTargetBitmap。

官方SHA x64 `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`、x86 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`，version10,0,1326,50504 / commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。它是官方版本信息，不是本仓源码修订。不外推其他runtime版本。

## 4. 完整输出比较，不只看同差数

双架构cd-baseline/comparison.json各224行已全文分段亲读；比较器源码核实读取全缓冲区、长度相等、逐字节old/new及哈希、四角色alpha非空和官方repeat。18场景全部nonempty/repeatDifference0/noRegression；窄rectangle baselineDifference64→count0、oldNewDifference64，ellipse32→0、oldNewDifference32，candidateSha等于officialSha。其余16场景oldNewDifference0且baselineSha=candidateSha，**完整输出未变**，不是仅对官方差数相同。CH未另行重算所有原始二进制哈希。

nested ellipse/rounded各18；ellipse stroke identity/nested162/128、rounded106/60仍对官方不同；整体exact=false、threshold0、evidenceOnly=true、hardwareVerified=false。不能把CD采集测试Passed说成这些差已关。

## 5. 默认产品回归（CH新增实际运行）

复用B/default-{x64,x86}/com-acceptance-package，未构建发布。aot.sha256与实际BW candidate identity均对应上述新候选，不用官方代候选。BL同exe监督将结果目录规范为各task绝对路径，完整任务目录回收至 `artifacts/ch-curved-acceptance/`。

|架构/任务|本地子目录/结果目录|实际计数与退出|
|---|---|---|
|x64 0cd630afcfef4ba59c41479dd57f2f60|default-x64/Results/ch-default-x64|BW 1/1/1/0，Exit0|
|x86 61f052b5470e40f182f5992452b1cd09|default-x86/Results/ch-default-x86|BW 1/1/1/0，Exit0|
|x64 c9aa1c3f8bae4ff0b972143585155f6b|siblings-x64/Results/ch-siblings-x64|兄弟17/17/17/0，Exit0|
|x86 0f26ad1ad51c43c9b8bdfc756e489e7b|siblings-x86/Results/ch-siblings-x86|兄弟17/17/17/0，Exit0|

BW筛选 `FullyQualifiedName~BwCurvedFillPrecisionTests`，窄rectangle/ellipse官方及repeat均0差非空、exact=true；不是显式CD取证补数。兄弟为默认Br整数oracle五行及PublicGeometry十二行；oracle自含算法，不冒称整数生产已接通。监督/计数原件已亲核，所有四次非timeout、正常退出。默认测试SHA/MVID与显式不同，详细绑定见CH。既有原44/10及更广默认回归没有因此变绿。

## 6. 未闭及交付

GPU/loss新色、持续媒体、长期CPU/GPU资源线程callback关闭、通知、圆角、合法大shader、同步Event0、E_HANDLE及首次堆损坏均开放。BD旧x64 full1740/1699/41 Exit2和x86无父full完成TRX不绑定新SHA。最后稳定源码Tests/ComAcceptance×x86/x64 Release完整适用四格仍待交付；文档或小组绿不等于完整替换。
