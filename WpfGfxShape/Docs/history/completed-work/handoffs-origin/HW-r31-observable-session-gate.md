# HW r31：实际可观测会话门禁与未关闭边界

## 实际实现

仅修改 Tests/WpfGfxShape.ComAcceptance/D3DImageTests.PresentDiagnostics.cs，补充运行线程 ID/线程桌面、WTS session 连接状态、DWM DWMWA_CLOAKED 和 desktop DC 的 screen GetPixel 诊断。WTS 查询内存及桌面 DC 均释放；保留原生 clear/present、窗口 DC GetPixel 和原断言。未添加 ABI、未以客户端 staging 替代生产输出、未改生产 DLL、媒体、阈值、skip 或四导航。

标准测试包 publish 成功（37 行构建日志）；最后源码修改在 publish 及以下执行之前，没有未编译尾项。生产 DLL 无需重发，仍为 r30：6FDF81D7F8CA98588DB4C5CCB3FAFB4C2904C4225B89D4FC725D7B07CD3F239D。定向方法族/full 的 aot-identity.json 已回读并一致。

## 实际执行链

所有执行只上传 artifacts/com-acceptance-package，并只回收 Results。

| Investigations 证据目录 | 执行 | pass/fail/total | skip |
|---|---|---|---|
| HW-r31-native-before/Results/native-before.trx | 未修改包的原生 GPU 四项 | 1/3/4 | 0 |
| HW-r31-session/Results/session.trx | 补录 session 的原生呈现可见性 | 1/0/1 | 0 |
| HW-r31-family/Results/hw-family.trx | D3DImageTests 完整方法族 | 41/0/41 | 0 |
| HW-r31-full/Results/hw-r31-full.trx | 一次全量 | 1406/0/1406 | 0 |

full RunID：6b3c5cde-c2d6-493e-9a06-573e1d4ffd44；50.949 秒。原始 TRX 10638–10642 已回读：executed=1406、passed=1406、failed=0、notExecuted=0。媒体三色、重复、并行等原始 Passed 行已回读，r30 八项没有回退。

## 已证明与未证明

第一次原生定向：staging=0xFF123456 正确；普通 D3D9 Present=0、DwmFlush=0，但窗口 DC 黑色；swapEffect=1 和 3 的显示尺寸 frontbuffer 均为 0xFF000000。由此不能将当次消费者黑屏直接归罪 cache/shader，也不能把 backbuffer 成功当作呈现成功。历史 r30 另见 D3D9Ex rawPresent=0x08760878（S_PRESENT_OCCLUDED），非 S_OK，不能将其视作已呈现。

补录后的实际观测：session=1、WTS 查询成功、sessionState=0（WTSActive）、currentThread 与 HWND ownerThread 一致、threadDesktop=Default、inputDesktop=Default、cloaked=0；screenPixel 与窗口 DC 均为 0x00563412。方法族先 Green，再执行一次 full；full 内原生 swapEffect=1/3 的 frontbuffer 均为 0xFF123456，窗口 DC 为 0x00563412。生产消费者仍直接读取其公开 HWND 呈现输出，没有借用原生 control 像素。

这是当前真实可观测会话下全量 Green 的证据，不是生产呈现根因修复声明。新增诊断不主动连接/切换 session，不建立长期会话 lease，不阻止运行途中桌面状态变化。首次失败没有 WTS/CLOAKED 字段，无法从新日志倒推首次 session 是否 disconnected；日志读取额外时间也可能影响时序，尚未排除。没有实现通用生产 GPU 输出读回能力，也没有充分验证 FLIPEX 语义。

旧 r30 全部32失败尚未逐项完整读完（读取工具长行截断）；不能宣称完成全部失败根因审计。已确认程序集及 D3DImageTests 均 DoNotParallelize，但这不证明异步 scheduler 无竞争。不能将本次一次全量 Green 当作跨会话稳定闭环。

## 后续仍 Open

需要继续在失败会话获取同样 session/DWM 字段，建立消费者之前、同一合法生产呈现路径的强制原生前置校验及可观测会话生命周期保证；或基于权威正常公共接口实现生产真实 GPU 输出读回。必须保留失败，不以 skip/降低阈值/客户端 GPU 像素替代。跨会话可靠性、32项根因族完整闭环未关闭；本文件只归档已编译实现和真实门禁，不改导航或阶段状态。

本机 run_tests 的 VSTest 路径被 .NET 10/Microsoft.Testing.Platform 拒绝；实际验证使用已成功 publish 的标准 MSTest 可执行包在 Windows 沙盒执行，见上述 TRX。
