# 当前工作切片

> 本文只维护唯一下一动作，不累积历史。执行约束见 current-execution-rules.md。

## 当前方向与最近完成

阶段 1～3 Closed，阶段 4 Active。MediaPlayer 接收仍在 provider QI 成功后返回 E_NOTIMPL，释放传输及临时引用，不保留 source identity；注册、帧通知和真实媒体消费未完成。

本轮按用户要求完成 CMilSlaveVideo 可行性探索，未修改生产代码、未运行测试或构建。证据、状态机、错误规则与路线比较见 [`completed-work/S-UNDATED-044-CMILSLAVEVIDEO-FEASIBILITY.md`](completed-work/S-UNDATED-044-CMILSLAVEVIDEO-FEASIBILITY.md)。历史验证仍为上一生产切片的定向 11/11、主测试 4174/4174、ABI 8/8、Debug 增量构建 0 警告 0 错误；测试重新编译有既有 MSTEST0044 警告。

## 唯一下一动作

完成真实媒体创建/provider—composition 注册—帧就绪通知—确定性释放的生产闭环。**执行前先完成路线决策关口**：推荐保持单一 C# / Native AOT 项目，联动迁移 AV 调用方；若要求复用旧原生播放器，必须先获得原生桥架构批准并验证私有类型的构造/链接可达性。不得默认用户已同意新增原生生产项目。

## 明确阻塞与范围边界

- 旧 CompositionNotifier 直接调用 CMilSlaveVideo 非虚 NewFrame/InvalidateLastCompositionSampleTime；COM 包装、GCHandle、仿制布局及普通子类覆写均不能拦截。
- 只翻译 videoslave.cpp 或新增托管 notifier 不会接管旧 CWmpPlayer/MediaInstance 的真实通知源。纯 C# 路线需要 AV 创建端及通知链联动迁移，不能把这个前置藏在“注册”接口后。
- renderer BeginRender 还依赖 CD3DDeviceLevel1*；软件 null 路径有原生证据，但不能当作硬件互操作完成。
- 本轮明确缩小为用户指定的静态探索；下一轮先取得影响生产边界的决策，再合并回完整生命周期，不连续拆成仅布局、仅计数或空注册任务。

## 下一轮执行与验收计划

1. 与用户确认路线：纯 C# 联动迁移（推荐）或允许原生桥。若未作决策，保持 E_NOTIMPL，不创建两套猜测性实现；纯 C# 路线不以旧 provider 注册成功为可达目标。
2. 根据已选路线登记生产 Ledger、原生到托管映射与测试定义；核对媒体创建入口、provider 来源、state engine/presenter/sample scheduler/event proxy、composition owner 和关闭链，明确可独立验收的真实生产闭环，不能用假 provider 替代缺失的媒体来源。
3. 近似直译 VideoSlave/CompositionNotifier 与 composition video 方法族。保留非拥有唯一列表、注销与在途回调互斥、direct=false 的 UI 事件、outstandingUIFrame 合并；确认完整锁序及 flag/sample time 跨线程可见性，禁止推测 scheduler。
4. 接通 ProcessUpdate 与释放：direct flag 先更新；同 provider 指针不重复注册，异源 E_INVALIDARG；先保存 provider，再 composition 注册、provider 注册；两级失败保持原生可析构状态。析构先 EndComposition，再 provider 注销、composition 注销、provider Release；失败后同源更新不得擅自增加重试。
5. 接通真实帧路径：NewFrame 只请求 pass；Begin 先 End 上轮、锁定 renderer/sample，syncChannel=!direct；仅 frame ready 才 NotifyChanged。Begin/End 按原生在清理后返回 S_OK，失败 frame-ready=false；不得机械套用通用首错传播。覆盖 UI 通知、sample invalidation、多 slave 最后 End 和 sample 归还。
6. 经 VideoDrawing、DrawVideo/DrawVideoAnimate 验证消费者通知、依赖替换、跨表 duplicate/delete、最后引用释放、注销并发、owner 关闭及注册失败。真实 provider 路径必须有证据；失败注入仅作为补充。若先验收软件取帧，明确硬件 device 桥仍未完成，不上调整体媒体替换资格。
7. 生产代码变更后运行定向、主测试、ABI 和 Debug 构建，建立独立归档并同步基线、映射和阶段状态。只有真实闭环通过才移除对应 E_NOTIMPL；架构或真实来源仍阻塞时保持未完成，记录阻塞与下一完整交付边界。

## 禁止扩展

不修改原 WPF 源码、SDK fingerprint、命令编号或未经审核的生产 ABI；不新增 Reset/ResetEx，不用 Task.Run 伪造 composition scheduler，不将 renderer caller 的身份用途泛化为 provider 可接受任意 token。只接收可信同进程有效 COM 指针；发送队列未消费引用、32 位运行及完整 PresentationCore 媒体 E2E 均未验收。
