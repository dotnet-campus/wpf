# AY 曲线边界独立对照与审计

## 最终文档状态

BB 已用现成证据补完本初稿；本次仅专用读写搜索，没有修改代码、构建、发布、运行或使用终端/shell/直接间接脚本。已读当前执行规则与完整替换方案。**审计输出已回收，不再沿用 AT Socket10054 后“无输出”的旧状态；无生产曲线修复，正式零阈值比较仍失败。**

## 亲核路径与计数

以下均相对 `artifacts/curve-ay/`：

- `final-audit-readback/output/`：已读 audit.json 开头的原始差异/SHA记录、compact.json节选、x86/x64 的 current-readback.txt 全部62行及 readback-readback.txt 全部13行。边界结果来自 current，不把旧 readback 的空 boundaries 当成已覆盖。
- `readback/x86-final/Results/curve-an/8642401af8744806842ff281dc8c3be7/aq-summary.json` 与 x64 对应 `90afbd583d4f466fa1b13c201f0071e2/aq-summary.json` 全文已读；两角色 original/candidate 的 identity.json 共四份及两个 Results/aot-identity.json 均已读。
- `readback/{x86-final,x64-final}/TestResults/ay-curve.trx` 全文亲读：各 total=executed=1、passed=0、failed=1，ResultSummary=Failed；RunInfo 明确退出2。不是通过，也不是双架构全量。TRX中的任务、业务根与identity一致。

|限定软件场景|x86与x64共同结果（差异单位：字节）|
|---|---|
|原十帧比较|frame1/3/5各44；frame9为10；其余0，threshold=0、exact=false|
|原frame5/9 double路径|original/candidate两角色各44/10；frame6/7/8为0|
|三种干预|prequantized-double、integer-schedule-exact、integer在frame5..9两角色均0差|
|负半格 below|官方cubic为基准0；double及候选cubic各14差；device/object预量化均0|
|负半格 exact及above|官方cubic为基准0；double及候选cubic各30差；device/object预量化均0|
|显式非整数变换顺序|double70、object-prequantized80、device-prequantized0；候选cubic70；官方cubic基准0|
|small、large-clipped|各角色各模式均nonempty=True、0差；仅对应有限样本|

frame5首差offset696/(14,4)/channel0，官方36、候选40；frame9首差offset2432/(8,15)/channel0，官方171、候选179。以上是回读既有审计/summary结果，BB未运行比较器或独立重算全部原始像素、轨迹及SHA，不冒称逐字审计全部audit.json。

## 实际身份与归属

两架构均实际.NET10.0.13，官方 `10,0,1326,50504`，commit `4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。backend均明确 RenderTargetBitmap software contract control only，hardwareVerified=false。

|架构|最终任务|候选Native SHA256|官方根模块SHA256|
|---|---|---|---|
|x86|ec2217163dd44683b01cf9ff95c1fc6a|0FCA4287C3E44CB7A7B0801D297C6D4E85F4AD84402D8B9580D0725E8449741B|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|
|x64|023a787d5c8948e49f81a5ef2f884912|C8C35CE4872FA659B02F8731F34F1F0492775D30879DEC17E54779F06228BE01|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|

实际加载路径均为 `C:\CodingAgentSandbox\Tasks\<任务>\Native\wpfgfx_cor3.dll`（候选）和同任务根 `wpfgfx_cor3.dll`（官方）。旧readback任务x86 c50ee70e49bb45e285374eb6451c371a、x64 2b68ca94e10e4c54b8c05ad15848cde1，只作旧审计复读，不替代final运行身份。Native SHA不等于实际托管测试程序集SHA/MVID；后者本次未取得，不造值，不绑定最终生产源码。

## 结论、边界与下一生产切片

预量化三干预零差支持“有限样本的设备坐标量化值得验证”，不能证明完整共享算法/HFD正确。显式非整数变换实验是显式计算控制点/折线的顺序对照，**不是实际WPF变换栈等价性证明**。负半格结果不能直接推出所有float舍入规则；float中间精度/舍入、真实变换栈的层次和顺序、完整HFD调度/大曲线仍未闭。small/large-clipped有限零差不外推任意输入、stroke或GPU。

下一曲线生产实现应限定 **fill设备空间量化**：先核float舍入及真实变换栈，再决定最小修复；不得向共享stroke盲加量化或以正坐标实验替代负数半格/非整数变换官方回归。保留原44/10失败，不改golden/阈值/skip/fallback。没有生产修复及正式修后回归，不关闭曲线问题；新GPU圆角仍未验证。

本包只完成证据与文档闭环，完整替换、最终源码两正式项目×x86/x64 Release四格full均未交。亲核不足部分明确保留交接，不由构建成功或有限实验零差补齐。
