# Media ownership 与 required 集成重试

## 实际执行与合同

最新 Ownership 测试已编译并在 Windows 沙箱独立进程执行：candidate/reference 各三场景，6/6，0 fail/skip/AV。原始 TRX、逐操作 progress 与六份 JSON 位于 `Investigations/MediaOwnershipRetry/{candidate,reference}-ownership`；GenProto RequiredIntegrationAuditTests 完整读取六份 JSON、进度、TRX 并计算 hash，结果见 `Investigations/GenProtoEvidence/required-integration/report.json`。

原生依据（本轮读取的关键合同片段合计少于400行）：apifunc.cpp SendCommandMedia 仅验证 channel 并发送指针，不 AddRef/Release；MediaPlayerState.SendMediaPlayerCommand 独立 AddRef 运输引用；VideoSlave.cpp ProcessUpdate 接管并在 Cleanup Release pMedia，QI 获得的 provider 引用由 slave 保存并在析构释放；clientchannel.cpp Destroy 先 SyncFlush 再销毁并释放 channel。故未显式 commit 后 Destroy 不是 queue discard oracle。

caller root 从 factory 创建一直持有至最终 Snapshot 后，运输引用独立 AddRef：RejectedSend 为 E_INVALIDARG，调用方释放未交接的运输引用，1→2→1；两种成功场景运输引用由 native 消费，销毁后回到1。最后调用 IMILMedia Shutdown（wgx_render.h vtable slot22）再释放 root；channel 仅销毁一次，connection 仅 disconnect 一次，cleanup 使用清零后的句柄避免双 close。未释放 root 后再读取 COM/vtable。

factory 直接返回 IMILMedia*。本轮尝试 QI IID_IMILMedia 两端均 E_NOINTERFACE，源码 CWmpPlayer::HrFindInterface 两个独立 if 的 else 可解释该结果，故按直接 factory 合同使用返回指针，仅 QI IUnknown 校验身份；不是 fallback，不替换私有接口。保留源码注释解释原因。修正后重新编译并执行六例全部通过。旧 AV 本轮未复现，不声称已经取得旧崩溃堆栈或证明其具体根因。

生产 SHA256 保持 3F291D3E6EF3A74688B8FDCBF124B725DC115D165FAE94DB9B2010BFDCE2570D；仅构建/打包测试，不发布生产、不跑 COM full、不卸载 DLL。真实 runtime 为10.0.1，不冒充完整合法10.0.12。

## required 再生范围

审计完整解析 regeneration-941a112ea3d44800ad1527f53bf9528c 的 authority/invocation/results，逐 authority 文件 hash、逐输出全文与 diff全文、C# Roslyn token或换行等价核对。378 outputs，275 byte-exact，378 在所列比较范围等价。原 Resources.rsp 不含 ProtocolFingerprint，不能宣称其历史 hash 一致；当前 generator 正常执行不是历史 build authority。

最新专项1/1：GenProto Tests/TestResults/11_25_16 TRX；最新 full45/45：11_26_23 TRX（完整名称见唯一基线）。首次full44 pass/1 fail为 WireCompilerArchive 对旧 assembly hash 绑定，保留11_25_31红TRX；该轮真实 wirecompiler 测量刷新归档后再跑full全绿，未放宽断言。

## 有限关闭与真实余项

RejectedSend 与 committed/implicit-flush destroy 的 real reference balance 有限关闭，不是最终对象析构或queue discard证明。两个正式 required ID 保留 parentBlocked=true、stage4Closed=false；transport packet-specific执行、queue-discard独立oracle、完整required physical/assertion来源对账仍未全闭。外部历史authority binding、ARM64真实工具链/环境、full10.0.12、官方player protocol与HW可见会话control前提仍缺。总目标持续，总门Red。
