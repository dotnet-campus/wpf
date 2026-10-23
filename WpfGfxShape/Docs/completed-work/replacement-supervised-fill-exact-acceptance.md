# CS fill exact 最终有限验收

## 状态与范围
最终裁决：有限双架构验收未完成，CT 已安全撤回 CR 新挂接，CF 保留。专属产物根 artifacts/cs-fill-acceptance/。仅专用工具及 Windows 沙箱正式纯 C# exe，不使用终端或直接/间接脚本。主代理仅监督。

## 接管核实
已全文读取当前执行规则27行、完整方案120行、CR报告16行、CQ报告76行（首段工具截断处通过后续读取补齐尾段；仍需补读47–52）。CR报告已有：x64 task375db6729b3c4e5cbffa991b2317bd98 CQ5/5 Exit0，候选AB5655A1A567CE586DF31F71FC132E461FE761B2E3573BE51577B8888C60073F。该数字目前仅为前轮声明，CS尚未独立认证。

## 已做/未做台账
CR已做：NativeShapeReader/SoftwareStrokeContours/SoftwareGroupStrokeContours生产接入，x64有限矩阵运行。同候选stroke重复不构成CF旧/CR新认证。
CR未做：当前源码x86独占正常依赖图NativeAOT与CQ实跑；双架构真正不同SHA旧新stroke；双架构默认BW1+CK10及必要17兄弟；生产算法边界独立审计；全部最终证据亲读。
CS已做：定位正式项目、规则、完整方案、CR/CQ报告和现有产物根。尚无CS新运行。

## 保留/撤回裁决
CT 已精确撤回 CR 的 NativeShapeReader 整数入口及 ordinary path fillLocalFloat 分支/调用；CF fillPrimitive、整数实现、CQ、CS_FILL_ACCEPTANCE 显式隔离测试保留。专用 Release win-x64/win-x86 构建成功。未完成的关键验收不能转述为实际像素失败。
CT 亲读 final-run-x86/Results/cs-final-x86/supervision.json 全文、父 TRX 结果/计数和 stroke/comparison.json 全文及两 actual identity：task cc18c3b74fad4a7c94ef499607fd62e3，17 executed/17 passed/0 failed，Exit0，无超时/取消/kill/drain 错误；测试 SHA B04139CA3BAE532E722F090E6759C48E59B61870A2195D79305CD537B3EF694B，MVID 9ca00196-7748-40a6-926c-dcb22a761fa5，.NET10.0.13/X86。随包 Old/Native 实际 SHA C4D7D4699F6334AC164DE95573D382807803ED65E4FE42AA05A9559FEA7F44EB 等于测试声明，候选 FD8B32FAF8D55CF72001DE9ACFE18A57E0814D92C38F63C4FE1D7D45047CD522，二者不同；16 stroke 全缓冲差0且非空，baselineIsCurrentSnapshot=false。
frames10/siblings16 的 scenes.tsv 全文均零差、repeat0、非空。未完成 device/precision 全原始 comparison、x64 同包真正旧新矩阵及两架构必要17兄弟，不认证最终通过。上述数字不绑定撤回后源码。详见 CT 报告。

## 修改文件
本报告（新建）。后续逐项补充。不修改共享文档、NativeRun、ExitDiagnostics、VisualCache、捕获或WindowQ。
