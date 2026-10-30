# Exclusive foreground 与 cache 证据正式收尾

状态：**CloseoutCompleted（仅本次既有证据与文档闭环）**；完整替换仍 Open。

## 1. 范围、来源与执行账

本次主代理仅亲自读写文档，不新增探索任务、不读无关源码、不重复读取 14074 行 TRX 或 32 轮样本，不再委派。下列运行、源码及 cache 事实引用前代理已完成的关键读证范围，**不是本次重新亲读运行原件或源码**。本次亲读范围是本报告草稿、七入口及旧 present-occlusion 报告；修改后回读另列于提交结果。

- 本次新增代码、构建、发布、sandbox、测试及其他执行均为 **0**，没有新 task。
- 既有修正代码范围仅 ExclusiveFocusLoss/resx；不把它记作本次改动或生产修复。
- 既有首次 build 因 CS0103 Failed；第二次 build 为 146 warnings / 0 errors；COM publish 2、sandbox 2。失败及后续成功均保留，不合并、不隐去首次失败，也不伪称只有 publish implicit build 成功。
- production 632F/147B 不变。consumer HW backend、产品实际 loss 恢复、audio、natural resources、最终 full 与完整替换仍 Open，memoryGreen=null。旧独立分母及失败历史不相减。

## 2. present-occlusion 原件存在性与两份 full

前代理已核实：present-occlusion-state 原件真实存在于绝对路径下的嵌套 artifacts 中。旧报告及入口所称“不存在”“仅用户交接/用户认证”“EvidenceUnavailable”是代理误判，**不是用户认证，也不是原件不存在的事实**。旧正文保留历史，当前以本报告与旧报告顶部更正为准；本次未重新搜索或读取这些原件。

|既有任务角色|task|total / passed / failed|
|---|---|---|
|default|df9e6a43f21f48cab95ce47a6f40e634|1823 / 1821 / 2|
|diag|3ab6be85d5ed4e5f837cc852ab33bb04|1823 / 1821 / 2|

上述两份 full 的九项 occlusion 未复现，不证明 occlusion 根因已定位。两项失败应分层记录：official 初始 expected FF80、actual 0，native 前置通过；旧 loss 在 foreground 前置 Failed，尚未构成真实 device loss。official actual 身份为 AD1C。不能把这两份 full 写成全绿，也不能用后续 system-control 单项通过抹去旧失败。

## 3. 新 loss 系统控制：ClosedLimited，不是产品恢复

引用前代理已读的新 loss task **0f364…**（此处保留已知前缀，不补造完整 task ID）及其系统日志/TRX：

- 实际 SetForegroundWindow 三次返回 1，双向 AllowSetForegroundWindow 均返回 1，session=1。
- 真实 DEVICELOST 0x88760868 → DEVICENOTRESET 0x88760869 → Reset 0。
- 新/旧实际颜色分别 FF123456 / FF654321；不再沿用旧“只有强断言、没有实际颜色记录”的边界。
- OS exit=0，TRX Passed。

这证明该系统 D3D loss 控制链有限闭环，仅 **system control / ClosedLimited**；不证明产品 HWND consumer 的设备就是该设备，不覆盖产品 consumer loss、恢复和新内容呈现。旧 foreground 前置失败及旧代码缺口属于历史，不能继续表述为新任务仍未取得合法前台控制。

## 4. official cache：cacheReached 已有，不是永久 NotReached

引用前代理已亲读 **official-cache-8836-7970572.jsonl 的全部 16 行**及对应源码合同关键范围；本次不重复读日志或源码。

- cacheReached；connect=0；两份 source 均 12×1；dirty HR=0。
- mode 11、scale=1；brush 9 → mode 11 / visual 6。
- target 12、root 13、transform 14、child 3，world 为单位矩阵。
- flush=0；expected FF80、actual 0。
- publicPeek retrieved=0 只表示事后该次窥探未取到消息，不证明先前没有消息、consumer 成功呈现或内部缓存有效。
- 资源类型必须区分：BitmapCache=94、BitmapCacheBrush=83、HwndTarget=46；不得混用 69/73/74，也不得把资源类型与命令号、handle 混为一谈。

这些证据纠正当前状态中笼统的“cache 未达、无法取证”；不改写其他旧任务确实 NotReached 的历史。未发现可确定的 layout / scale mode / root 缺项；不得凭 actual 0 或事后无消息就补协议、判 shader/硬件错误或归因环境。

candidate 同源码 case 通过，不保证 official source 位图可用。官方内部 GetBitmapSource 路径存在，也不等于 public ABI 可直接调用；当前合法公开 COM 接口能否观察相应状态或内容仍 **Unknown**，不能虚构可用 getter 或接口。

## 5. 未定根因与真实下一必要证据

official actual 0 的根因仍未确定；既有图、提交成功与 candidate 通过均不足以证明 official consumer 获得可用 source 位图或最终输出成功。

下一仅建议先在**最小范围核实可观察的合法 COM 接口**及其公开 ABI/调用合同，确认能否观察 official source 位图可用性或对应 consumer 内容状态。未证明的接口和能力保持 Unknown；先核合法观察能力，再决定是否有必要另行申请最小取证。本报告不强行开启诊断、不新排 full、不要求重跑旧变体，也不把内部 GetBitmapSource 当成现成 public API。

## 6. 文档闭环与验收边界

本报告为正式收尾，不再是草稿。七入口为 next-session-handoff、current-work-item、current-stable-baseline、replacement-issues-and-plan-status、final-acceptance-remaining-matrix、wpfgfx-complete-replacement-validation-plan、remaining-gap-closure-plan；均以新增顶部摘要覆盖旧当前结论，旧正文保留历史。旧 replacement-present-occlusion-bounded-closeout 追加顶部更正。

**CloseoutCompleted 只指本次证据与文档工作完成，不表示产品 consumer loss、official cache 根因、最终 full 或 wpfgfx 完整替换完成。**
