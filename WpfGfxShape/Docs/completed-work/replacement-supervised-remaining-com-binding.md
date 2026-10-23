# EG：剩余 COM 调用有界分组定位

## EH 最终补正（覆盖以下初稿进度，历史保留）

EG 已留下三标记分组实验源码，本轮完整读取 857 行并在 `artifacts/eh-com-closeout/snapshot/InteropDeviceBitmapExports.cs` 保存全文（文本内容保存，非字节/hash认证）。现有工作区没有 EG 发布包和指定 task 回收原件；以下“尚无构建或执行”仅为旧初稿，不能作为 EG 最终执行账本。

|组|已有交接信息|EH 亲核及裁决|
|---|---|---|
|all-direct|taskebf9a136ffe74b94b87ff1bf58c73df6；用户要求核验39行业务、三循环refs0、监督和精确parentTRX|未找到原件，不能补造实际计数、Exit、身份或称已亲核三循环；不是本轮task|
|early generated|task3eae50f5bc544823b46150e570ce9b6c；用户要求核明stderr C0000409性质|stderr/监督/TRX不在当前工作区，无法确定是设施异常还是native fail-fast，不仅凭码断ABI|
|late generated|交接称正常分析器发布于artifacts/eg-remaining-com/late/com-acceptance-package|该目录未找到，实际candidate/符号/输入身份不可核，EH准确未运行，未另发布或补跑|

早期组四调用为 surface.GetDesc(slot12)、surface.GetDevice(slot3)、CheckDevice内device.TestCooperativeLevel(slot3)、Create内device.GetCreationParameters(slot9)。两处device QI为CheckDevice与GetUpdateMethod，实验控制中均direct；后期组为其它受EG_GENERATED_LATE控制的caps、Ex检查/释放、GetDC/ReleaseDC、factory/LUID及finally释放路径。分组是诊断变量，不是默认修复。

EH 已逐块精确撤除 Interop 中 EG_INTEROP_COM_CONTROL、EG_GENERATED_EARLY、EG_GENERATED_LATE 及 EgTrace/EgRelease，Code目录检索均无命中，保留原DT/DX条件trace和原默认generated调用。其它生产/测试/项目未改，EF DR char*与CF/CX/CY未触碰。撤回后默认Release方案专用构建Exit0、1482警告/0错误、13.65秒；无NoWarn/关闭分析器或实验常量。日志工具仅保留stdout最后20000字符（原1213693字符），不能宣称全日志逐项核验或干净重建；这是正常增量build，不是NativeAOT重新发布或运行验收。

第二QI在既有DX交接明确已返回E_NOINTERFACE/null，不能写未返回；返回后仍不能排除前序栈/内存破坏。当前未证唯一根因，不能泛替全部生成调用或归咎整个Silk库。下一最小切片先恢复既有监督/stderr/actual身份/精确父TRX，再仅拆实证失败组；x64 Event0独立，Available实际callback、HW消费、恢复、full均未完成。详见[EH收口](replacement-supervised-com-group-closeout.md)。

## 执行约束与进度

本轮只执行显式 EG_INTEROP_COM_CONTROL 分组诊断，不提交默认行为修复。先读取规则、完整方案、EF/ED/EB/EC 报告、实验快照和当前源码，再建立正常分析器下 all-direct + 原 stack + 原语义控制；控制成功才进行至多两次单组 generated 恢复。最多三次有效控制/组运行；不运行 full、旧 Event0 或脚本。

产物目录：`Code/WpfGfxShape/artifacts/eg-remaining-com/`（须核实 ignored 规则和既有正式执行器）。

待执行：规则和证据核对；显式控制与必要只读诊断；正式 x86 NativeAOT 正常依赖图独立构建发布及 BL 监督；保存完整实验源码快照；精确撤回 EG 分支与辅助代码；默认正常构建；四份文档最终全文回读。

## 已知事实（用户提供，待对照原证据）

- EE 未运行。
- EF 正常分析器条件下，两处 device QI Stdcall 均返回 E_NOINTERFACE，但 Create 仍 C0000005；此前默认行为已撤。
- DR CA1838 的 char* 声明修复已官方 x86 运行验证，必须保留。
- 旧 all-direct + stack 三次 Create 成功曾受分析器验证限制，不能假定本轮重现。
- Available/HW 消费/full/四格与 x64 Event0 堆损坏继续开放，不能推定与 x86 同根因。

## 本轮实测

尚无构建或执行，task=0，有效控制/组=0。不得将历史运行计入本轮。
