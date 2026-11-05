# 当前 Stage19 默认 full 双架构收尾

状态：ExecutedFailedAndTimedOut / EvidenceIndexed / CloseoutCompleted / OverallOpen。不是完整替换通过。

## 纠偏收尾（本轮零运行）
91实例索引已完成：`Docs/completed-work/replacement-independent-media-current-crossarch-failure-index.md`。原件child.trx L15083 Counters亲读：total/executed1843，passed1752，failed91，其他项0；L15084..15090 RunInfo明确exit2。确定fixture12（架构硬编码2、受信original-wpf.json缺失10），已完成契约+源码确认的生产根因0，能力/归因未证79，互斥合计91。后两类不代表生产没有问题，更不得改Pass。完整FQN/参数/testId/第一Message/业务栈与阶段均有原件行号；非巨大stdout全抄。

第三工程`.coding-agent/evidence-reader`及其四文件明确退役，不编译执行、不作为证据、不改根构建；前一执行者normalbuild3/sandbox2的违规投资单独记录在`.coding-agent/stage19-crossarch-correction-receipts.md`，不把0warnings当业务绿。本轮生产/测试源码修改0，normalbuild/tests/Native/COM/sandbox均0。索引工作量已大，不展开修复分支；授权后续normal≤2/tests≤1/COM≤1/sandbox≤1/Native0本轮完全未用，不为花预算新跑full。

修改before→after：原草稿“Counters待核/91待索引/x64最后节点待提取”→当前Counters原件核实、91索引落盘、x64边界核实。仅文档变化，回撤可删本节及恢复下列原草稿语句；不得删TRX/原件或旧full记录。没有改expected/Skip/golden，没有以candidate生成reference。

## 独立运行
- x64 task dd603767c95a4d4c971174604c968c95，PID7560，监督2880；2026-11-01 08:47:37.584998Z 至09:12:37.6483025Z。supervision.json 20..38：1500秒 timeout、KillAttempted/Returned=true、child exit -1/FFFFFFFF。不是正常cleanup，最终TRX分母Unknown。
- x86 task 9e6f1257c0ff433ea553d70ce4a18db5，PID10312，监督12336；09:13:55.7758962Z至09:23:08.7801333Z。supervision.json 20..38：exited，child exit2，无timeout/cancel/kill。child.trx L15083 Counters已直接复核1843 executed/1752 passed/91 failed，不从旧统计相减。
- 原件根：WpfGfxShape/artifacts/stage19-final-crossarch/readback-{x64,x86}/Results/stage19-final-default-{x64,x86}/。已直接读取两监督全文和两空stderr，未文本读取binary。
- x64测试SHA8EA50EC2F7FE20657DB9F90C6D89C61B9C6A41F2298925827ECD87AB35C4698F，MVID465f2658-cbfe-47e9-9c31-3bd834bf630a；x86测试SHAD85BE1EC239D8A50DF98789B99BA0ED650446D9CF5A4E43F8FEB16C3559C0062，MVID2847f8ee-5043-4166-8796-aaf9a1f773f2；两宿主.NET10.0.13。不是相同测试二进制。
- x64 diag 14501已见实际Native D9AA1E0ED5E99B29F10AC2DA4635D7F014B0F433C05DF2E5B18F2028836CD09C。x86实际加载身份由child.trx L1504 PresentCondition直接复核：Native/wpfgfx_cor3.dll SHA F4D4F13DD2F6DD8EFB47B3455C23921FA8F1B87D1F2494E27A765DE6D29DA68D；不是仅package sha。

## x64现场边界
stages.jsonl 825..843已读至尾：08:48:25.9888101Z visual cleanup complete（仅该visual场景）；不能据此解释09:12监督kill为正常退出。diag L14811..14815末尾明确InProgress/Executing `D3DImageTests.WhenFreshNativeGatePassesThenOfficialAndCandidatePublicTargetsAreCompared`，无完成记录。Results/bj-public-stages.jsonl仅L1 `native-gates-passed-official-start`；bj-fresh-1.json及bj-fresh-3.json L1..9分别passed=true且测试SHA/MVID与本次x64一致。已越过native两门、进入official调用，但没有official-passed/candidate-start，candidate公开比较未证。

源码D3DImageTests.CaptureFreshFrame.cs L60..76先两native门再official；D3DImageTests.cs L298..325经PresentationWindowThread.Invoke进入HwndSource创建/RootVisual，然后才执行10秒rendered.Wait；D3DImageTests.WindowThread.cs L76..92的completed.Wait()本身无界（ready.Wait也无界）。这是静态可能等待边界，不是运行栈。无dump/线程栈，实际停于Invoke/HwndSource/其他具体调用仍Unknown，不能断言deadlock；1500秒监督kill不是正常cleanup。下一直接工作是用现有Win32 debugger针对自有正式进程取得officialWindow所有者线程现场/等待栈，设置有界获取和退出；不新runner、不无限等待、不泛选门。refs/resources/OS顶层exit仍Unknown。

## 已确认首个x86失败
child.trx 9..19：testId19316999-ff78-8902-bffd-45b2336e366a，WpfGfxShape.Tests.MediaFoundationResourceTests.WhenOriginalAviIsReadBySystemSourceReaderThenAllVideoFramesReachEosAndClose，断言8 != IntPtr.Size(4)，发生在实际source-reader验证前。必须审结构与ABI后参数化，不能Skip。

## 本轮预算
旧草稿授权normalbuild<=3/run_tests<=2/COMpublish<=1/sandbox<=2，前执行者第三工程normal3/sandbox2违规投资不冲销。本纠偏轮先0运行；索引后条件授权normalbuild<=2/run_tests<=1/COMpublish<=1/sandbox<=1/Native0，实际全部0，新full0。旧轮normal0/tests0/Native1/COM2/sandbox2独立保留，不冲销。

## 保留门
r5公开视频/MP4能力拒绝/Seeking仅有限通过；含轨拒绝不等完整有声播放。audio endpoint0、AVI16样本缺口、AVsync、强cancel、HW target/consumer loss、自然结束资源full仍Open/Unknown。禁止原版wpfgfx loader/副本/委托/fallback；合法外部provider不破坏。失败索引91/91已闭环；根因修复仍Open，不把91全部归环境，不改expected掩盖生产bug。旧full按各自源码/身份/结果独立保留，旧TRX无Counters不造分母。
