# CMilSlaveVideo 实现可行性探索（仅调查完成）

## 本轮范围与结论

按用户要求优先探索上一轮 AV 阻塞；本轮仅修改文档，不新增生产实现，不解除 E_NOTIMPL，不将媒体生命周期标记完成。阶段 4 Active，替换资格不变。

**CMilSlaveVideo 的业务状态机可以用 C# 近似直译；不能将该托管实现直接交给旧原生 IMILSurfaceRendererProvider。** 原生 provider 的 COM 调用约定只是入口，参数实际要求能被旧模块内部 C++ 代码直接调用的对象。需要联动迁移调用方，或经架构批准采用真正兼容的原生承载方案。

## 原生证据与最小状态机

以下路径相对于 `src/Microsoft.DotNet.Wpf/src/WpfGfx/`。

| 证据 | 已确认事实 | 实现要求 |
|---|---|---|
| `core/resources/VideoSlave.h:25-93` | 继承 CMilSlaveResource；NewFrame、InvalidateLastCompositionSampleTime 非虚；持有 composition、provider、current renderer、direct flag、sample time | 保留资源 identity 与通知依赖；不能靠覆写虚表转发两个回调 |
| `core/resources/videoslave.cpp:35-99` | 初始 renderer/provider 空、direct=false、sample time=-1；NewFrame 仅请求 composition pass；析构先 EndComposition，再 provider 注销、composition 注销、provider Release | 不把 NewFrame 当 NotifyChanged；owner 必须活到注销结束 |
| `core/resources/videoslave.cpp:114-237` | Begin 先 End 上轮，再获取 renderer 并 Begin；syncChannel=!direct；GetSurfaceRenderer 返回已锁定 renderer 的新增引用 | 一次 pass 使用同一 sample；失败、重入下一轮及销毁均须配对清理 |
| `core/resources/videoslave.cpp:245-300` | direct flag 先更新；接收 master 的传输引用；QI 后比较 provider 指针；首次保存 provider 后依次注册 composition、provider | 同源不重复注册，异源 E_INVALIDARG；注册失败保留可析构状态，不擅自改为即时事务回滚；传输及临时引用必释 |
| `core/av/internal.h:34-88` | provider 注册和 renderer Begin/End 使用 CMilSlaveVideo*；BeginRender 使用 CD3DDeviceLevel1* | 不能把这些参数统称为普通 COM 指针 |
| `core/av/CompositionNotifier.cpp:63-217` | 非拥有唯一列表；注册、注销、回调遍历均持 m_lock；直接调用两个非虚方法；锁外发 AVMediaNewFrame；outstandingUIFrame 合并 UI 请求 | 注销与在途回调互斥；不能简单改为锁外快照遍历后立即释放资源；direct=false 与无资源 UI 请求不可丢失 |
| `core/uce/composition.cpp:525-585,1082-1124` | 非拥有 video 列表；Begin 返回 frame ready 才 NotifyOnChanged；End 遍历清理；注销允许未注册 | source 更新、调度请求、帧就绪是三个不同事件 |
| `core/uce/crossthreadcomposition.cpp:215-218`、`samethreadcomposition.cpp:171-174` | 跨线程交给 partition manager；同线程 ScheduleCompositionPass 是 no-op | 不能用 Task.Run 或任意同步回调伪造 scheduler |

### 必须修正上一轮计划中的笼统错误规则

ProcessUpdate 仍按原生传播 HRESULT。**CMilSlaveVideo::BeginComposition 与 EndComposition 则在清理/断言后返回 S_OK**，防止媒体失败使 composition 无响应；End 即使 renderer End 失败也 Release。Begin 的原生失败跳转可能不写输出，而 CComposition 调用前初始化 false；C# 应确保失败时 frame-ready 为 false，记录这一安全表达差异。不得将通用“首错传播”机械套到这两个方法。

首次注册失败后，provider 已存入 slave；同指针后续更新不会自动重试两级注册。这也是原生状态，不能不经差分证据增加自动重试。

## 为什么不能只包装 provider

真实创建链为 `CMILAV::CreateMedia → MediaInstance::Create → ChoosePlayer → CWmpPlayer::Create/Init`（`core/av/milav.cpp:77-166`、`MediaInstance.cpp:32-127`、`WmpPlayer.cpp:68-134`）。MediaInstance 内含 notifier 与 media event proxy；notifier 借用 MediaInstance，不独立 AddRef。

`CWmpPlayer::RegisterResource/UnregisterResource` 直接委托其 MediaInstance 的 notifier（`WmpPlayer.cpp:597-633`）；GetSurfaceRenderer 委托 state engine。只在 C# 侧新增一个 notifier 不会截获旧 player 的真实通知。

`SampleScheduler` 的 mixer/scrub 路径会使 sample time 无效并请求 composition（`samplescheduler.cpp:385-417`）。`CompositionNotifier` 的锁只证明其注册列表与回调遍历互斥，不证明 flag/sample time 的所有跨线程访问已安全；迁移前仍须核对 scheduler/state engine 的完整锁序，不凭局部源码指定新的同步机制。

`EvrPresenter::AVSurfaceRenderer` 另有独立 compositing-resources 列表与 m_compositionLock（`evrpresenter.cpp:2785-2862,3005-3041,3581-3638`）。在已读取的方法中 caller 用作唯一身份，而 notifier 确实解引用调用；因此 renderer 的身份用途不能反推 provider 参数可用任意 token。最后一个 caller End 后才进行 sample cleanup。

硬件绘制还有第二道边界：`evrpresenter.cpp:2894-2978` 的 BeginRender 保存/引用 CD3DDeviceLevel1 并传入媒体 buffer；它不是 IDirect3DDevice9*。原生明确允许 null 表示软件路径，但不能以始终传 null 冒充硬件兼容。

## 实现路线比较（建议，不是已批准架构变更）

### A. 保持单一 C# / Native AOT 生产项目，联动迁移 AV 调用方（推荐长期路线）

- 按原文件边界迁移 VideoSlave、CompositionNotifier、MediaInstance 与 composition video 生命周期；内部使用真实托管对象调用，不模拟 C++ 内存。
- 同时打通媒体创建/provider/state engine/presenter/sample scheduler/event proxy 的真实通知来源。只有创建端和消费端都属于新实现，内部 C++ 类型边界才可消除；系统 COM 边界仍保持原 ABI。
- 先登记 Ledger、映射与验证定义。原生 IMILMedia/私有 provider 的对外可达性需审计，不能擅自改变既有接口或接受任意外来 provider token。
- 代价是跨阶段 4/5/6 的前置较大，不是只翻译 videoslave.cpp 一轮即可完成。纯托管通知单测可以是子证据，但不是旧 provider 兼容或真实播放验收。

### B. 保留旧原生播放器，加入原生桥（需要用户明确批准及进一步可链接性验证）

- 普通代理、ComWrappers、GCHandle、对象 pinning、仿制布局均不解决非虚调用；普通派生类覆写也无效。
- 桥必须提供旧调用代码实际认可的 CMilSlaveVideo/CComposition 承载，或重建调用方使其显式转发到稳定 C 回调。另一个 DLL 内放置同名 C++ 方法不能截获旧模块已绑定的直接调用。
- 前者依赖私有类、构造/工厂及链接边界，当前没有已验证的可用创建路径；后者涉及原生构建/逻辑边界，超出现有单一 C# 生产项目及不修改原 WPF 源码的约束。
- 还须处理 callback context 生命周期、注销等待在途回调、模块卸载、x86/x64、异常封锁、renderer/device/sample 引用与 UI event 转发。不能声称“加一个小 shim”即可投产。

### C. 仅托管假 provider 或 bitmap 注入

适合失败注入/单元测试，不是解除阻塞的生产路线。现有 `Direct3D9VideoDrawPipeline.cs` 仅承载 bitmap draw operations，不包含 AV sample/provider 生命周期。

## 本轮托管差集与验证

读取并核对 `Core/GeneratedMediaPlayerResource.cs`：当前 QI 成功后返回 E_NOTIMPL，finally 释放 media/provider，未保留 source identity；保持原样。

本轮只做静态源码调查与文档交叉核对；未执行构建、主测试或 ABI 测试，不产生新的通过数。上轮 11/11、4174/4174、8/8 仍只是历史基线。没有新增生产文件，因此不新增生产映射或将任何映射状态上调。

## 后续决策及验收边界

下一轮唯一任务与验收步骤见 `../current-work-item.md`。优先明确是否继续纯 C# 联动迁移；未经批准不得增加原生生产项目。E_NOTIMPL 只能在真实创建—注册—调度/UI 通知—sample 锁定—消费者通知—注销释放闭环通过后移除，不能凭模拟 provider 测试通过移除。
