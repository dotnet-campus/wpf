# HWND4 正式生产发布与 player 外部协议阻塞

## 已交付边界

HWND4 已发布并独立原版差分；player2 保持缺失，不新增 E_NOTIMPL 桩，不用 MILFactoryCreateMediaPlayer/媒体对象替代 record packet player。正式总门 Red，无替换资格。最后生产发布后未修改生产源码；独立测试最后修改后重新发布并完成候选/原版运行。

## 原生合同核验（每次读取不超过400行）

- `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/vt_api.cpp:1–103`：g_hwndMap + g_csCompositionEngine；Attach 只登记 HWND，重复 E_ACCESSDENIED；Detach 只注销，不存在 E_INVALIDARG。不检查 IsWindow、不要求 visual/content 参数，不 draw、不 AddRef/持有窗口、不自动响应窗口 destroy。当前 HashSet 与共享 CompositionGate 是该注册机制，不是假装图形绑定；不能把这四项通过外推成 HWND 实际绘制完成。
- `core/uce/rsapi.cpp:70–185`：Content 两项动态取得 DwmAttachMilContent/DwmDetachMilContent，实际调用 HWND 提示，忽略 DWM HRESULT，均返回 S_OK。候选实际调用系统目录 dwmapi.dll 对应入口，不是只维护字典。沙箱两入口均可取得；返回值不能证明 DWM 内部内容、呈现或 ownership 状态。
- `core/uce/apifunc.cpp:785–870`：MilPlayer_Create 在 PRERELEASE 调 CMilRecordPacketPlayer::CreateRecordPacketPlayer；Process 调 ProcessFilePacketContents，参数依 MIL_REC_PACKET_HEADER/UCE_RDP_HEADER。非 PRERELEASE 的 E_NOTIMPL 不是本任务所需真实能力。
- 对 `src/` 全内容查三符号仅 apifunc.cpp 调用/签名命中；文件名 player 枚举只有媒体类，没有 record packet player 实现。对 `Tests/` 与 `Ledger/` 内容查三符号均无命中；可读取 authority.globalconfig 只是分析器配置，没有官方 player 协议。未取得任何新增可读官方来源，不能声称遍历不存在的外部资料或网络查证。

## 正式发布记录

1. 生产发布#1（成功）：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。net10.0/Release/win-x64/NativeLib Shared，产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`，生产 SHA `858C182AB5A29348E311946C044CBEBECD1D8339B072F6AD45A318EACD9F4DA8`。
2. 独立 HWND 发布（成功，初始及修正后共4次）：`dotnet publish WpfGfxShape/Tests/WpfGfxShape.PlayerHwndAcceptance/WpfGfxShape.PlayerHwndAcceptance.csproj -c Release -r win-x64 -o WpfGfxShape/Investigations/Player-Hwnd/acceptance-package`。初始旧程序相对 Results 不在执行包目录，指定回收失败；全目录回收也没有报告，不算成功验收。修正输入/输出以 AppContext.BaseDirectory 为基准后最终重新发布，candidate/reference 均取得 JSON。
3. 严格 ABI 包发布（成功）：`dotnet publish WpfGfxShape/Tests/WpfGfxShape.PublicAbi/WpfGfxShape.PublicAbi.csproj -c Release -r win-x64 --self-contained true -o WpfGfxShape/Investigations/Player-Hwnd/publicabi-package`。不改 required/golden/raw/阈值。
4. COM 正式包发布（成功）：`dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=true`，包 `WpfGfxShape/artifacts/com-acceptance-package`。

所有执行通过专用发布/Windows沙箱能力，未执行终端脚本。以下路径以 `WpfGfxShape/Investigations/Player-Hwnd/` 为根。

## 实际验收与回读

| 项目 | 最终原始证据 / 结果 |
|---|---|
| HWND候选 | `candidate/Results/hwnd-report.json`，SHA858C…，21/21、failures=0 |
| HWND原版 | `reference/Results/hwnd-report.json`，WindowsDesktop10.0.1，SHA94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F，21/21、failures=0 |
| 严格公开ABI | `publicabi-final/Results/0f872c7d6bd8457bb112b32426ee1e1e/report.json`，同生产SHA；Missing=[MilPlayer_Create, MilPlayer_Process]，Extra=[]；LifecyclePassed=true，Passed/PeContractPassed/AllAbiPassed=false；FixedGolden1012Satisfied/NativeAotUnloadVerified=false |
| COM full | `com-final-full/Results/hwnd-final-full.trx`，RunID27a03b40-596f-4733-9231-1e0acd30f42f；1429执行/1397通过/32失败/0未执行，失败退出2 |
| COM身份 | `com-final-full/Results/aot-identity.json`，同SHA858C…，Sandbox=true，任务acae668ab25c4a41bbdd9a3c55a98909；Results校准JSON一并回收 |

两份 HWND JSON 全部21项已逐行回读且 actual 相同：null注册/重复/注销/重复注销、真实窗口冲突、content重复、跨thread注销与注册、destroy不自动释放注册、destroy后注销及重复注销、destroy后content提示。没有把已销毁句柄的登记当成合法窗口或拥有引用。

PublicAbiStrict沿用现有 PublicAbi 严格 PE required 比较程序，无另名 PublicAbiStrict 测试；上述名称指该门，不虚构额外测试通过。完整JSON已回收；本轮回读身份、Missing/Extra及最终门字段，未声称新建完整JSON逐项审计。TRX counters 与 identity已回读，全部32失败保留。

## 保留差异与完整完成条件

旧SHA627F273204BF6C247D6B3F09055AFA71ECAE1F5FE1D713D572BCB194EB985A35的[四项集成档](Four-exports-integration-closeout.md)不覆写：3D24+null1有限Green；glyph raw22/30、定义字段30/30、未初始化padding裁决Open；release10+null30有限通过；perf候选0/1/0对原版0/0/0 Open；full1397/32/1429。最新full仍32失败，不清零、不一概归环境。旧定向数字不拼成新SHA定向执行。

player完整关闭仍要求：官方冻结协议的header布局/packing/版本/包类型/长度/校验/有效与无效向量，Process真实dispatch/资源与调度语义，Create输出/失败、handle ownership/销毁ABI、线程/取消/重复处理/关闭清理及原版可执行oracle；到位后真实实现、成组差分、最终同SHA strict/COM/full及身份回收。不允许以non-PRERELEASE NotImpl满足能力。

总计划已遇不可自主凭空解除的确证依赖：此前三类外部输入，加本次player官方protocol+ownership，共四类，见[外部前置](../all-completion-external-prerequisites.md)。外部资料缺失不豁免自主Open项。七阶段权威条款、A/B/C及E2E-01–09完整原样保留于[总计划](../remaining-gap-closure-plan.md#七阶段正式完成条款保留不以导航或有限green代替)，不是全部完成。
