# CS fill exact 最终有限验收

## 状态与范围
进行中，尚未认证。专属产物根 artifacts/cs-fill-acceptance/。仅专用工具及 Windows 沙箱正式纯 C# exe，不使用终端或直接/间接脚本。主代理仅监督。

## 接管核实
已全文读取当前执行规则27行、完整方案120行、CR报告16行、CQ报告76行（首段工具截断处通过后续读取补齐尾段；仍需补读47–52）。CR报告已有：x64 task375db6729b3c4e5cbffa991b2317bd98 CQ5/5 Exit0，候选AB5655A1A567CE586DF31F71FC132E461FE761B2E3573BE51577B8888C60073F。该数字目前仅为前轮声明，CS尚未独立认证。

## 已做/未做台账
CR已做：NativeShapeReader/SoftwareStrokeContours/SoftwareGroupStrokeContours生产接入，x64有限矩阵运行。同候选stroke重复不构成CF旧/CR新认证。
CR未做：当前源码x86独占正常依赖图NativeAOT与CQ实跑；双架构真正不同SHA旧新stroke；双架构默认BW1+CK10及必要17兄弟；生产算法边界独立审计；全部最终证据亲读。
CS已做：定位正式项目、规则、完整方案、CR/CQ报告和现有产物根。尚无CS新运行。

## 保留/撤回裁决
待关键门实跑决定。不得降低CQ合同；保留CF既有修复、CQ与整数实现。若关键门无法完成且仍存在未验收行为，仅精确撤回CR本轮生产挂接。

## 修改文件
本报告（新建）。后续逐项补充。不修改共享文档、NativeRun、ExitDiagnostics、VisualCache、捕获或WindowQ。
