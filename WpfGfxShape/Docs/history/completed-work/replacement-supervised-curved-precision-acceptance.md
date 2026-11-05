# CH curved precision 独立验收与交接

## 1. 裁决

**CF有限修复保留，CH独立有限验收通过；完整替换未完成。** 已先建立本报告并持续更新。执行规则27行和完整方案120行已读，CE历史保留。只用专用工具及Windows沙箱正式纯C#现有exe；不新增Host/项目，不改生产/测试/阈值/skip，不重发生产。授权补完CF和CG报告、前置三共享文档最新节。

## 2. CF原件独立亲核

根 `artifacts/cf-curved-precision/accepted-<arch>/Results/cf-<arch>/`：两父TRX各32行、两监督各38行；cd-baseline/{baseline,candidate,official,official-repeat}/ 八子TRX各26行、八监督各38行、八actual identity各14行全部全文亲读；两comparison各224行完整分段亲读。父各2/2/2/0、子各1/1/1/0，十监督Exit0/exited，均非timeout/cancel/kill/drain timeout、无SupervisorError。不是只引用CF报告。

四角色同架构同一测试程序集，actual=requested：旧Baseline、新Native、官方/repeat为task根；.NET10.0.13，X64/8及X86/4，software RTB。官方版本10,0,1326,50504，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。

|架构/task|实际新候选SHA256|旧候选SHA256|
|---|---|---|
|x64 ca93d384103441168360f9e4223fd1a4|36353DDE3BA720B642A6F12CEE8AB0EDDA501BD7DC49F35270063480B8A0532A|F45485F1C747361D89E31FDB667A2C8C1C4920CE6BD6982A6AE93532C2E5A03C|
|x86 fb214890d0f84d12acbb3bb66ed917ed|C4D7D4699F6334AC164DE95573D382807803ED65E4FE42AA05A9559FEA7F44EB|09CD6C360304E02AEFF873BFDCB082CEC8CFC55D0736238C81E378EF1D81FABB|

显式测试SHA/MVID：x64 `4E8686B0A9EDEA962E396D64C6B341E100F9C6BA1799D01758580720BE888F26` / `b1c7a71f-d165-4759-bae8-72426523b652`；x86 `63A80F201DD04A0CD3245A0DBC54DE9FE8D40A10846C7D23465DEBA605232536` / `487d72f5-1ef4-4a56-8b31-98d5aa5a4320`。

官方SHA：x64 `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`；x86 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`。旧身份对应CE/CD当前基线，新身份与默认包实际运行一致；未认证全仓修订hash或独立重算全部二进制。

完整comparison和比较器源码共同核实：18场景均非空/官方repeat0；窄rect64→0、ellipse32→0，候选完整像素SHA等于官方；其余16场景oldNewDifference0且baselineSha=candidateSha，不只是对官方差数相同。nested ellipse/rounded各18、stroke162/128与106/60仍差，exact=false、threshold0、evidenceOnly=true、hardwareVerified=false。

## 3. 源码审查及具体限制

全文亲读GeneratedNativeStroke56行、SoftwareGroupStrokeContours129行、SoftwareCurvedStrokeContours120行（长返回截断处补读），并核GeneratedVisualRenderer805–846、GeneratedImageContentBounds调用搜索及NativeShapeReader24–75。TryCaptureFill传true，普通TryCapture/Group/Curved默认false；stroke调用不传true。只局部primitive float乘加，不改NativeShapeReader整数/sharedstroke。

group/combined仍沿用double parent组合/Apply；rounded先double构点再float；大曲线/极值/强抵消及fill+pen通路未被18例证明。不得外推完整float/group/圆角/HFD已解决。CD_CURRENT_FILL_DIAGNOSTICS显式隔离；BW正常默认TestClass/TestMethod保留。未认证整个工作区diff。

## 4. 默认实际定向回归

复用CF已有default-{x64,x86}/com-acceptance-package，aot.sha256及BW actual identity核新候选。筛选BW正常默认测试；再先list发现（x64 task8cbb55dc484f4a4884a99ba92cb26d0c，所选Geometry/Fill/Stroke范围307项，不是完整发现总数）后运行Br整数oracle五行与PublicGeometry十二行。未盲full。

以下结果全目录回收到 `artifacts/ch-curved-acceptance/`；每行监督38行全文、BW父TRX26行全文、兄弟TRX计数/部分结果已亲读，不声称兄弟全部stdout逐字复读。全部Reason exited、Exit0、非timeout/cancel/kill/drain timeout，无SupervisorError。

|架构/场景|task|本地下目录及父计数total/executed/passed/failed|
|---|---|---|
|x64 BW|0cd630afcfef4ba59c41479dd57f2f60|default-x64/Results/ch-default-x64，1/1/1/0|
|x86 BW|61f052b5470e40f182f5992452b1cd09|default-x86/Results/ch-default-x86，1/1/1/0|
|x64兄弟|c9aa1c3f8bae4ff0b972143585155f6b|siblings-x64/Results/ch-siblings-x64，17/17/17/0|
|x86兄弟|0f26ad1ad51c43c9b8bdfc756e489e7b|siblings-x86/Results/ch-siblings-x86，17/17/17/0|

远程父TRX均为 `C:\CodingAgentSandbox\Tasks\<task>\Results\ch-<default或siblings>-<arch>\TestResults\child.trx`，监督Arguments已核为该绝对路径。传相对结果根后由监督Path.GetFullPath固定，完整回收没有依赖相对搜索猜测。首次尝试以绝对outputRelativePath回收被工具拒绝，未执行；随后改为全任务回收，未丢结果。

默认x64测试SHA `C4F93D7C435E30A536024DF9157ED4C5B6FBD1F548BE616CD3D485764D6A9FCC` / MVID `3d9d716e-e219-4378-9759-c5cc40b68a35`；x86 `C8B0703A432D6F4F5C078B6082BAADB88E9496936F42857285F9E5C4DDEE1A9E` / `23f288d5-f1c7-4501-a72a-e3ed3b3f5007`。两架构BW三角色identity各14行及comparison31行全部亲读：同默认测试程序集/.NET10.0.13、对应指针宽度，candidate为上表新SHA、独立官方/repeat为上述官方SHA，rect/ellipse均0差非空、exact=true。实际不是显式取证包，也不是官方代候选。

兄弟17例通过是有界补充；Br自含整数算法不证明生产接通。更广默认原44/10未跑本轮，不宣称已关。无新失败，未撤CF行为或越权修复。

## 5. CG现有证据补审

不新跑硬件。亲读三监督38行全文、三父TRX计数：两控制task66f4376505d04a8f990af4a1abf0df46为2/2失败Exit2；最终只读efa7d62d8b9f43349bef9870f38cc113为1/1失败Exit2；默认选择谓词0142efbb93d8447088ef6c66f637bf61为4/4 Exit0，只证明筛选。最终只读业务JSON726行已全文分段亲读补齐三帧，详细QPC及字段写入CG报告。

red/blue/red backbuffer正确、Present0，三帧各10次Acquire WAIT_TIMEOUT，无像素；paint1/erase0前后相同仅离散观测，不证明因果。DWM全局timing不是目标surface消费事件。依赖官方AD1C9DBB…不是候选，x86未跑，BJ历史成功反证保留。旧两控制JSON本轮未重新全文亲核，沿用CG审阅记录，不能把旧数据移绑只读最终测试SHA。

## 6. 交接与开放项

保留CF三文件有限修复；独立有限验收通过，但整体exact=false且完整替换未完成。原44/10、BD旧x641740/1699/41 Exit2及x86full无父完成TRX保持历史、不绑新SHA。GPU/loss恢复新色、持续媒体、长期CPU/GPU资源线程callback关闭、通知target同步/非零SyncModeStatus/迟到消息、圆角、合法大shader、同步Event0、E_HANDLE及首次堆损坏均开放。最后稳定源码Tests/ComAcceptance×x86/x64 Release完整适用四格还需交付。

本轮六文档：CH、CF、CG及next-session-handoff、session-summary、issues-and-plan-status。更新后的全文分段回读单独执行；文档闭环或小组绿不作为完整替换。
