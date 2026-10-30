# sampler-native-crash-x64-r5 诊断收尾

状态：CloseoutCompleted / RootCauseOpen / BudgetExhausted。既有执行链及本轮文档收尾完成，不是生产修复，不是完整替换通过。

## 已认证的最窄定位

官方 DLL SHA256 AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1；匹配 PDB GUID 8756f755-10f4-48be-9886-22c9a80c75b8、age1、SHA256 732FB582AF469CA52FD3B1C39520FFAD892EE6E9FAFDA1BC69A6066EF6212582。MSF7 stream1 和 DbgHelp 实际加载身份均通过，RVA B65F 位于 CBaseMatrix::GetScaleDimensions+0xF（起始 B650、size170），PDB 行 BaseMatrix.cpp:767。SymFromAddr=True 时 LastError487 仍保留，不误当失败；symsrv load failure 未阻止本地匹配 PDB 加载。

原 r3 PID9876/TID168，RIP0x7FFC17C6B65F、RSP0x78A963EF00；原 AMD64 CONTEXT1232bytes、flags0x10001F、RCX0、RDX0x78A963EF60、R8=0x78A963EF98。B650 开始的实际代码在 offset15 为 F3 0F 10 01，即 movss xmm0,[rcx]，本次异常是读取空矩阵 this，不是写空输出指针。该解码是对已存字节的静态判断，raw 输出自身标 NotDisassembled=True。原 dump1652255bytes，SHA2561851A7517FFE42FB688BD098EBF463624F59180BF183AA71ABD90E872451B85C；ThreadList 和 MemoryList 可读，已验证5077个内存范围。不能以没有 exception stream 否定原现场，原 context/exception 独立保存。

## 实际执行与证据边界

[执行账](../../artifacts/sampler-native-crash-x64-r5/execution-receipt.md)保留逐次参数、日志截断与历史失败。累计 normal COM build2/2、COM publish2/2（各含隐式编译）、offline sandbox2/2；最后 build148warnings/0errors、exit0，publish exit0。Native publish/原 fixture 修改/official 或 candidate 业务重跑/full/原测试均0。本轮只亲自读写文档并静态核源码，新增构建/发布/测试/沙箱/脚本/委派均0。

离线1 task953001534d61475facf56edecb342065 首次完成符号认证；离线2 task52c4192684ce49f28393b5a5269cd0cd，PID11936、test MVID8c8019e8-ea74-4df9-97d0-2ba25921aaeb、.NET10.0.13/X64，复用 r3 dump/context，新增寄存器、指令字节和内存范围核验。原 raw 为 `artifacts/sampler-native-crash-x64-r5/readback2/Results/sampler-offline-symbols/symbols.txt`（109行）；本轮核关键段，不伪造逐字工具回执。父 OS exit 工具未给出，Unknown；无 TRX/业务通过。原 rootReap258 和 worker 退出未证事实保留，root5000ms等待分支没有新运行证明。

## 本地官方参考的静态判断

参考根 `src/Microsoft.DotNet.Wpf/src`，不是已认证精确 commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699 的源码快照。原 raw/blob/API 抓取404、用户指明的SDK API403历史和前轮全部违规/失败继续保留；本轮实际读到的 official-source-status.txt 为 GitHub API HttpStatus403，不混称SDK回执；不以抓取错误断言源码不存在或外部能力阻塞。

- WpfGfx/core/common/BaseMatrix.cpp:756–789 从 this 的 _11/_12/_21/_22 求尺度；:802–817 DecomposeMatrixIntoScaleAndRest 无条件调用 GetScaleDimensions，必须有有效矩阵对象。
- WpfGfx/core/uce/drawingcontext.cpp:3695–3740 SetupEffectTransform 的 pWorldTransform 为必需输入，先传给 GetLocalSpaceClipBounds，再无条件调用 DecomposeMatrixIntoScaleAndRest。:3070–3093 PushLayer 路径及 :4635–4646 DrawEffect 路径均来自 m_transformStack.GetTopByReference()，不是 EffectMapping 的托管 MatrixTransform 字段。MatrixStack.cpp:194–228 的 Top 在空栈时返回 IdentityMatrix，而 GetTopByReference 原样返回 nullable 指针；WatermarkStack.inl:280–293 明确空栈返回 NULL。因此空栈按值可表示 identity，但按引用送入此分解会有读空风险。这是具体静态假说，不是已证原现场根因，仍缺可信 caller 和实际栈状态。
- PresentationCore/System/Windows/Media/Effects/Effect.cs:68–74 的 EffectMapping 默认返回 Transform.Identity；单位空间映射缓存不等于 native world-transform 指针。ShaderEffect 的 scale-transform 参数也不能凭字段 nullable 推导允许在 SetupEffectTransform 传 null。需结合真实 caller 证明是哪条输入链失效。
- fixture D3DImageTests.VisualCache.cs:250–294 在 BindTwoInputEffect 后创建92byte target、SendOutputWindowSettings、设 root；未见在该片段显式建立 world-transform 的命令，但这不是必传矩阵缺失的证明：绘图上下文/target 初始化可负责 identity，nullable 资源句柄也不等于空 CBaseMatrix*。此前122byte shader effect包及句柄核对未证尺寸错误。下一静态细节见执行账；不修改 fixture、golden、阈值或 skip。

## 尚缺可信 caller；下一最小工作仅建议，未执行

raw 的 RSP+0x38 DecomposeMatrixIntoScaleAndRest+0x2F、+0x78 SetupEffectTransform+0xD3 等是 NotUnwound 候选；vftable 同样命中，恰说明线性代码地址扫描不能当真实调用栈。当前只确证空 this 的故障指令，不能确证 SetupEffectTransform 是实际 caller，更不能确证 fixture 应传却传 null 或某次生命周期释放为根因。

下一优先在现有 C# 离线入口接系统 DbgHelp StackWalk64（AMD64），用已存 CONTEXT 初始化 PC/SP/FP；ReadMemoryRoutine 复用已验证 dump 范围，FunctionTableAccessRoutine/模块 base 回调以实际模块范围及匹配 images 提供 .pdata/.xdata，保持回调生命周期并串行调用 DbgHelp。限定只取故障帧到第一个可信调用者及必要 effect/transform 上游；记录每帧 PC/SP、返回值、即时错误、内存命中/缺失及终止原因，检查 SP 前进和重复帧。不得把当前进程地址空间当原进程，不无限自造 PE unwinder。

StackWalkEx 可作为可用公开接口替代；RtlVirtualUnwind 仅在函数表和 image/VA 映射可靠时备用，并非可直接接受任意 dump-read callback 的同等替换。缺模块 image/栈内存或 API 失败则停并准确记录，不重崩、不猜栈、不改 fixture。建议下一独立授权上限：normal COM build1、COM publish1、offline sandbox1；Native/official/candidate/full/原测试0，失败即停。此预算不是本轮余量或已执行动作；先由外层授权再执行。

## 不变的开放项与收尾

当前 full23红全部保留：x64 1823/1803/20fail，x86 1823/1820/3fail，不以符号成功冲销。权限与显示 Unknown、真实 loss 未触发、音频、自然资源趋势/全部 worker 退出/final full 与完整替换仍 Open，memoryGreen=null。当前 diagnostic 实现仅用于取证、默认关闭，不替代生产验证。r2误根build触及Native、r4第二offline角色偏差和全部历史原件不覆盖。本报告、execution-receipt、baseline/failures及六个当前入口均已同步为 r5 已执行链收尾、根因开放；外层继续推进整体任务，不再留下“正式报告待写”的尾项。
