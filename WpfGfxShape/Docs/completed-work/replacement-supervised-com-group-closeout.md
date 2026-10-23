# EH：COM 分组实验与强制安全收口

## 进度（持续更新）

已读取执行规则、完整替换方案、EG remaining-com-binding 与当前 Interop 源码前部。EG 报告仍为初稿，不能代表实际运行账本；当前源码存在 EG_INTEROP_COM_CONTROL / EG_GENERATED_EARLY / EG_GENERATED_LATE 实验分支。

当前工作区未找到 `artifacts/eg-remaining-com`，指定 all-direct taskebf9a136ffe74b94b87ff1bf58c73df6 和 early task3eae50f5bc544823b46150e570ce9b6c 亦未发现。不得据此虚构亲核、计数、退出、SHA、MVID、runtime 或 parent TRX。后期唯一允许实验尚未运行；缺少实际发布 candidate 和执行器输入时不重新发布、不猜 filter、不补跑 full。

以上为初始执行账，以下为最终结果。全程无终端、直接间接脚本、新Host、full或沙箱新实验。

## EG 已做 / 未做与原件边界

已亲核：Interop撤回前完整857行源码含三标记与EgTrace/EgRelease。已在 `artifacts/eh-com-closeout/snapshot/InteropDeviceBitmapExports.cs` 保存完整文本；行尾不作字节级身份保证。根.gitignore的artifacts/覆盖此输出。

未亲核：用户提供all-direct taskebf9a136ffe74b94b87ff1bf58c73df6、early task3eae50f5bc544823b46150e570ce9b6c。名称递归查找无匹配，根.coding-agent为空，项目根无既有artifacts。不能补造all-direct39行业务三循环refs0、监督、绝对parentTRX，亦不能写测试0或Exit0；其实际计数/退出/SHA/MVID/runtime均未知。EG旧报告初稿不是执行原件，已补最终状态并保留历史。

Early stderr原文缺失：C0000409仅为待核交接线索，不能区分设施异常与被测进程native fail-fast，更不能仅凭码断定ABI。未满足原件核证要求，明确保持未完成，而非冒称已读。

## 后期唯一实验：准确未运行

未找到交接指定 `artifacts/eg-remaining-com/late/com-acceptance-package`，不可核确切发布符号、实际candidate、测试身份及BL执行器三个参数，故没有提交新task、没有猜FQN/filter或产生Exit8。后期0次新实验；未重新发布，不引用回收根作为输入，不虚构绝对任务TRX/actualSHA/MVID/runtime。

源码分组亲核：早期四调用为surface.GetDesc(slot12)、surface.GetDevice(slot3)、CheckDevice.device.TestCooperativeLevel(slot3)、Create.device.GetCreationParameters(slot9)。两处device QI分别CheckDevice和GetUpdateMethod，EG控制中始终direct。后期由EG_GENERATED_LATE控制GetDeviceCaps、Ex状态/释放、旧系统GetDC/ReleaseDC、GetDirect3D、factory QI、LUID参数/查询和finally释放。后期不是默认修复；没有执行结果，不能建立新因果差分。

## 强制安全收口：已完成

先完整读取并保存快照，再用19处唯一文本替换精确撤回（5+9+5），未整文件覆盖生产文件。仅修改 `Code/WpfGfxShape/Abi/InteropDeviceBitmapExports.cs`：删除三标记行为、EgTrace/EgRelease，恢复原默认generated调用。Code目录正则检索三标记及辅助无匹配。DT/DX默认关闭只读TraceCreate及调用、Ex检测、HRESULT/null/refs、原surface direct QI保留。

未修改DR/DS、NativeRun/ExitDiagnostics、VisualCache/historical、捕获/WindowQ或项目打包；未触碰EF DR char*与CF/CX/CY fill。此次是撤回诊断行为，不是产品根因修复。

## 默认构建与日志边界

撤回最后源码后专用正常build：`WpfGfxShape/WpfGfxShape.slnx`，Release，退出0，1482警告/0错误，13.65秒。未传实验常量、NoWarn或关闭规则；项目net10.0且AOT/Trim/PInvoke分析器显式开启。未变TFM/SDK/语言版本。

本次普通增量build，非clean/rebuild、非NativeAOT发布、非双架构运行矩阵。读取日志1–62行；stdout原1213693字符，工具仅保留末20000字符，前1193693字符不在日志。计数和退出可核，不能把截断前内容称完整回读，也不能以此宣称无既有警告或最终运行通过。构建之后没有生产源码修改。

## 因果裁决与下一最小切片

all-direct成功三循环只是用户交接待核，不移绑历史B或当前默认源码；early故障性质未核；late未运行。没有证据支持全部生成调用均错或唯一根因。第二QI在既有DX交接已返回E_NOINTERFACE/null，不能再写未返回；返回不排除前序栈/内存破坏。

先恢复指定既有task的监督、early stderr原文、39行业务、精确parentTRX和actual身份，再仅单拆实证失败组的四调用，不泛替全部、不盲full。若原件不能证明early是native失败，不先将其当已证ABI失败组。后期未覆盖，不自动追加实验。x64 Event0继续独立；Available实际callback、HW消费、loss/resize恢复、资源/线程与最终四格均未完成。完整替换未完成。
