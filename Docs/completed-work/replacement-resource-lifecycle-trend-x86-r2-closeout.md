# 资源趋势 x86-r2 最终收尾

- 硬门：**PassedKnownOwnershipAndCallbacksOnly**。
- 软指标：**softDescriptiveOnly**（原 JSON 为 DescriptiveOnly，memoryGreen=null）。
- 本切片：**CloseoutCompleted**。完整替换仍 **Open**。
- WorkerExitNotProven、长期无界无泄漏 NotProven；不认证所有 worker 或 GPU 资源。

## 证据来源与本接续范围

原 task：07a15005a0e0469bbb4e7b4f3c37eaca。
证据根：`WpfGfxShape/artifacts/resource-lifecycle-trend-x86-r2/sandbox-results/Results/resource-lifecycle-trend-x86-r2/ResourceLifecycleTrend/ddaf998ea0f24f98a5ca26af11704444/`。
本接续亲读 comparison.json 全文：original PID12032、candidate PID11812，均 exit0/passed/completedRounds16、unexecutedRounds=[]、unfinishedRound=null、completed=true、error=null；总 failure=null。role 实际目录按 comparison 的 original/candidate 标识对应本根下同名目录，不移绑其他 task。

**前代理已读**：全部32轮 business 1–78及681–689、events22–39、callback全文；两role各65samples全文及父角色监督。真实WIC引用、callback 1 exit、A关闭前后保活、B更新、真实duplicate释放、已知对象账0、cleanup正常均已核，不重复调查，不冒本接续亲读。callback退出不是OS线程退出。

**监督提供 / 前代理已核，非本接续亲读**：父TRX 1 passed；父36.702763 s、体35.8718142 s；两role退出0、reaped且非强杀。actual candidate15A545…、official5EBD…，test1F53D830…、MVID526585b8-cf25-410f-961e-2e3c4656331c，.NET10.0.13 / X86 / pointer4。本接续没有重新枚举TRX或重新认证完整hash，不将前缀冒作完整hash。

## 描述性指标（不判内存Green）

两role descriptive.json头及以下汇总切片为本接续亲读；各65samples、forcedGC=false、workerExit=NotProven。固定4warmup+12measurement；下表仅 **BeforeInitialize、测量轮4–15** 的现成汇总，不混入其他phase，不自算斜率或首尾。baseline不是必然等于测量首值；firstLastDelta照录原字段，不用baseline反推尾值。

|指标 / 单位；斜率单位|original baseline / slopePerRound / firstLastDelta|candidate baseline / slopePerRound / firstLastDelta|
|---|---|---|
|PrivateBytes：byte；byte/轮|19472384 / -160431.44055944055 / -1777664|24526848 / -141340.64335664336 / -3035136|
|WorkingSet：byte；byte/轮|48984064 / -98805.25874125875 / -1179648|52137984 / -58332.195804195806 / -2121728|
|Handles：个；个/轮|292 / 0.0944055944055944 / 1|317 / 4.104895104895105 / 45|
|Threads：个；个/轮|9 / 0 / 0|11 / 0 / 0|
|ManagedBytes：byte；byte/轮|5855396 / 9835.174825174825 / 104404|3627124 / 48391.818181818184 / 422084|
|NaturalGC0：累计次数；次/轮|3 / 1 / 11|4 / 1.1223776223776223 / 12|
|NaturalGC1：累计次数；次/轮|3 / 1 / 11|4 / 1 / 11|
|NaturalGC2：累计次数；次/轮|3 / 1 / 11|4 / 1 / 11|

PrivateBytes现成窗口范围（byte）及中位数：

|角色|轮4–7 min / max / median|轮8–11 min / max / median|轮12–15 min / max / median|
|---|---|---|---|
|original|18518016 / 19472384 / 19349504|17416192 / 18587648 / 17815552|17694720 / 18092032 / 17922048|
|candidate|21512192 / 24526848 / 23539712|16896000 / 23273472 / 22018048|21491712 / 23265280 / 22298624|

PrivateBytes测量首/尾：original19472384/17694720 byte，candidate24526848/21491712 byte；WorkingSet测量首/尾：original49000448/47820800 byte，candidate52150272/50028544 byte，均直接照录deltas.value。其余指标首尾及未摘录phase/window范围本接续为Unknown（未读，不代表原件不存在）；不为补软指标重复读取全部样本或阻塞硬门。descriptive长返回在约183行被工具截断，后续仅精确读取所需字段；不声称全文亲读。未计算全phase合并范围、差分或新回归。

这些数值仅静态描述有限窗口；不推断内存无泄漏，不解释增长来自缓存，不将自然GC当强制GC，不从进程退出推导全资源归零。

## 执行独立账及保留项

本轮COM publish1（含编译1）/沙箱1/父体1/官方候选各16轮/Native0，来自前执行阶段；初stageA失败、后builds、r1 publish未跑分别保留，不合并冲销。

**本接续新增：源码0/build0/publish0/沙箱0/test0/Native0/终端脚本0/再委派0。** 仅静态读写文档。原始样本、旧正文、原失败及独立账全保留。raw日志缺失、截断与未逐字保存偏差如实保留，本接续不能补造或追认完整raw。

已完成本报告及五份状态文档同步；最新终态覆盖旧启动阻塞/NotRun的当前含义，旧正文只作历史，不冲销原账。显示Unknown，实际音频/HW/shaderresize/real-loss/full/完整替换仍Open。下一仅建议x64同门另立预算和授权；本接续不执行。
