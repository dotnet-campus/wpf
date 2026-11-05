# AsyncDrain directClose：独立参数UID清单与单体canary入口

本表仅发现节点，不是执行结果。报告、原件路径、读取范围与能力边界见[发现收尾/静态审查](replacement-async-drain-directclose-discovery-closeout.md)。本阶段执行新增0；既有发现x86/x64各1，体0，所有历史账独立。

## 八项逐架构核对

FQN共同前缀 `WpfGfxShape.ComAcceptance.UceChannelTests.`；C方法 `WhenAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates`，O方法 `WhenOriginalAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates`。所有行参数System.Int32，retainExternal=false；6为健康directClose/open未提交，8为健康directClose/committed。下表行号均为各自diag，不因UID相同合并架构证明。

|架构|项|system / queueState|MTP TestNodeUid|该架构diag行|状态|
|---|---|---|---|---|---|
|x86|C6|false / 6|1d757743-a95d-86bc-ad1a-035db6190dac|66|Discovered；体NotRun|
|x86|C8|false / 8|19a3b855-228b-8fad-a67a-cfae47a754f2|74|Discovered；体NotRun|
|x86|O6|true / 6|1e4c35fb-589a-8de1-838e-4845fd1028f4|102|Discovered；体NotRun|
|x86|O8|true / 8|173013d6-ba45-853f-8195-7f9c30f57a50|110|Discovered；体NotRun|
|x64|C6|false / 6|1d757743-a95d-86bc-ad1a-035db6190dac|66|Discovered；体NotRun|
|x64|C8|false / 8|19a3b855-228b-8fad-a67a-cfae47a754f2|74|Discovered；体NotRun|
|x64|O6|true / 6|1e4c35fb-589a-8de1-838e-4845fd1028f4|102|Discovered；体NotRun|
|x64|O8|true / 8|173013d6-ba45-853f-8195-7f9c30f57a50|110|Discovered；体NotRun|

x86 diag：`WpfGfxShape/artifacts/async-drain-directclose-discovery/readback/x86/Results/async-drain-directclose-discovery/Diagnostic/log_261027130857726.diag`，task189899d1153b4b2fafeb97f3c8cb5baa。
x64 diag：`WpfGfxShape/artifacts/async-drain-directclose-discovery/readback/x64/Results/async-drain-directclose-discovery/Diagnostic/log_261027131030346.diag`，taskbdb8b2cc0ee449b4a45925b7d3945c65。

两份第115行分别会话exit0；没有独立外层数值OS exit。UID仅绑定这两份现有clean的发现，不推断重新发布/不同版本仍稳定；不能用TRX executionId替代。

## 唯一拟执行：先x86 C6 canary，尚未授权/未执行

未来现有沙箱入口字段（均相对工作区，无终端命令/脚本）：

|字段|拟值|
|---|---|
|sourceDirectory|WpfGfxShape/artifacts/async-shared-ownership-x86-r1/clean|
|executableRelativePath|WpfGfxShape.ComAcceptance.exe|
|outputRelativePath|Results|
|localOutputDirectory|WpfGfxShape/artifacts/async-drain-directclose-uid-canary/readback/x86-C6|
|timeoutSeconds|120|

arguments按下列顺序逐项传入数组，每个反引号项是一个token，不额外加shell引号：

1. `--filter-uid`
2. `1d757743-a95d-86bc-ad1a-035db6190dac`
3. `--report-trx`
4. `--report-trx-filename`
5. `C6.trx`
6. `--results-directory`
7. `Results/async-drain-directclose-uid-x86-C6`
8. `--diagnostic`
9. `--diagnostic-synchronous-write`
10. `--diagnostic-output-directory`
11. `Results/async-drain-directclose-uid-x86-C6/Diagnostic`
12. `--no-progress`
13. `--timeout`
14. `60s`

不加list-tests、方法FQN组、ignore-exit-code、retry或supervise-exit；一次一UID。CLI TRX/diag在上面结果子目录；业务JSON及aot-identity在Results根，不随CLI结果目录改变。必须回收整个Results，不能只回收TRX子目录；原工具返回另存本地独立txt，保持无数值exit的原貌。

该canary只预算一体一次，首错停，不默认续跑。通过有限认证后，x86 C8/O6/O8另预算三体，x64另预算四体；沿用相同参数模板，分别替换对应clean、单UID、角色/架构独立目录/TRX文件名。每UID独立沙箱任务，不合并多个UID一次执行，不把同UID跨架构当一体。

## 身份期望（引用既有绑定，待每体重新核actual）

|架构|candidate actual SHA期望|official actual SHA期望|test SHA / MVID期望|
|---|---|---|---|
|x86|15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|D8A5F57FE4A61CC28459A89DCB50DC75E0ADE80E1D0AEDC35D7E18AC5069A7BB / e207b681-4c15-453e-b0a4-a1690fa3db78|
|x64|54BC62FBBEF9A55EA75E49AF5EFDCEC70F7C8C2808BDF4E2C80B916CF92D5ADD|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|4A8356201F062B01161744C6B3BBF7D9AB7DB5909F6F12A9389806FFB6B6ED43 / d41adfe8-8904-4c9b-887d-8d1f5979ba46|

期望runtime .NET10.0.13，architecture分别X86/X64；pointer4/8仅由初始化file identity辅助，AsyncDrain业务不输出pointerSize。未来x64 sourceDirectory为 `WpfGfxShape/artifacts/async-shared-ownership-x64-r1/clean`。身份表来源为[先前静态选择报告41–56行](replacement-async-drain-directclose-static-selection.md)，本次未重新计算包SHA/MVID，不将期望冒为新实测。

AsyncDrain已经按NativeLibrary.Load返回handle与ProcessModule.BaseAddress相等定位actualPath，再hash该路径文件；没有GetModuleFileName调用或handle数值输出，不是内存映像hash。aot-identity仅磁盘候选预检，不替代official actual；不存在已证可用env补充记录的方案。取证检查和有限关闭/回调/worker合同、TRX/terminal/OS退出分级以收尾报告为准；独立OS数值exit缺口不伪造、不以无限新runner作为继续前提。
