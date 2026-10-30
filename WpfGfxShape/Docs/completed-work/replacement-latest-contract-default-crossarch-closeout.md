# 最新合同默认双架构验收正式收尾

## 终态
CloseoutCompleted / DefaultFullFailed / OverallOpen，memoryGreen=null。本次仅亲自读写既有证据与文档，无再委派；源码/build/test/publish/sandbox新增执行全部0。下列预算为此前已执行轮次，不是本次新增调用。

|架构|task|total/executed/passed/failed|监督child exit|
|---|---|---|---|
|x64|c71948f3b6d546549ebc844d9936ca67|1823/1823/1821/2|2|
|x86|22e93b4d50af424893275cfbd5c2d23f|1823/1823/1820/3|2|

Counters原件分别为 `../../artifacts/latest-contract-default-x64-r1/readback/Results/latest-contract-default-x64/TestResults/child.trx:14076–14080` 与 x86同结构 `14086–14090`；其余Counters均0。full已真实执行但并非通过，不能以未运行或0失败描述。

## 原件、身份与监督分层
[完整5失败索引](replacement-latest-contract-default-failure-index.md)保留全部FQN、参数、testId、executionId、Results/Definitions准确行范围、原Message、角色、已达阶段与cleanup边界，本文完整引用该索引而不另造映射。[执行回执](../../artifacts/latest-contract-default-execution-receipt.md)保留两次publish及两次full的实际调用、参数、日志截断与监督边界。监督全文1–38此前已读，本次不重复无限读。

- x64 testSHA `7071E5C82D8F7E8527652D737A6E5717DBECA0F97F3DB22C4F5ACAD37B243A4D`，MVID `4b66cea4-b29b-4a4d-9700-a9e6beaf4908`。
- x86 testSHA `CC384959AC4CCE4C98B8AFC988ED2AC5E01CF7A31B90627B388CA359E23C5753`，MVID `d587b9b3-8d28-4f49-bfc7-a59a59ad57d8`。两者均Release/default、.NET10.0.13，实际X64/X86。
- official实际为各task根 `wpfgfx_cor3.dll`：x64 `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`；x86 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`。版本 `10.0.13-servicing.26504.104`，commit `4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。
- Native pack输入/clean aot.sha256：x64 `6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403`；x86 `B07C9073F75278ED61C44638EB3644547B413A281E2859B25211CEAAC3D1466C`。包身份不冒每测试actual；system d3d9 loss与native fresh不认证candidate模块，未提供实际模块字段处仍Unknown。
- 两监督均Reason=exited、HasExited/OutputCollected=true；timeout/cancel/kill/drain均false，SupervisorError=null。独立工具OSexit未提供。监督正常不认证全部业务资源cleanup。

## 全部5失败与有限结论
1. x64、x86共同official `WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive`：native563412前置通过、CreatePixelShader HR0；初始cache消费 expected `0x0000FF80`、actual `0x00000000`，resize分支尚未到达。根因Unknown，不写成resize已验证或candidate回归确证；独立cleanup Unknown。
2. x64、x86共同 `WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover`：初始exclusive foreground资格失败，`FailedNotTriggered`；system d3d9控制，真实loss未触发，不冒产品consumer loss恢复。stdout device/factory/window complete不扩大成全业务清理。
3. x86独有 `WhenNativeOwnerThreadSubmitsRedBlueRedThenFreshDesktopContentIsRequired (3)`，swapEffect UInt32=3：首红backbuffer FFFF0000、present0，但duplicated/front FFF3F3F3；后二帧accepted=true不抵消首帧失败，整体false。这是native独立desktop旧内容失败，非cache业务证明，actual模块/cleanup Unknown。

O6 `async-wic-drain-True-6-False` 两arch本轮Passed，完整业务与合同解释沿用前代理已读原件及[O6正式报告](replacement-official-wic-release-thread-contract-closeout.md)。本次有限补读业务JSON65–110确认callbackExited1/disposals1及deinit阶段，releaseThread x64=9952、x86=12140，caller分别13200/6384，均worker，不是旧caller2204同线程现场复现。candidate线程强门不变，不擦旧O6失败。

## 合法writer旧32/malformed专项核证
合法0x8D writer现28，非法32负控仍源码保留，本次不改不重跑。限定核对两份父TRX全部Failed集合及对应stdout文本，另在两结果目录以 `0x88980403|88980403|BitmapCache…length` 检索未命中；malformed名称命中的是诊断中的测试名称，不是Failed同根证据。stdout可读文本有编码替换，阴性检索不作全二进制日志审计。

因此仅结论：**本轮无旧合法32-byte writer同根malformed失败**。各1823 executed是真实TRX记录，不等价所有cache业务覆盖；早期native gate挡住的用例仍不得冒充业务验证。历史42/23/40等红项各自独立保留，不相减、不改expected/skip/阈值，不以>=0吞S_PRESENT_OCCLUDED。

## 实账与整体阻塞
此前执行轮COMpublish2（均exit0）、正常implicit compile2（x86可增量复用）、sandbox2；normalbuild/Native publish/其它tests/源码修改0。本次文档收尾新增执行0，未消费新额度。

HWbackend、official真实cache消费、product consumer loss、audio、natural resources及完整最终full矩阵仍Open；根因Unknown，完整替换未通过。公开28-byte packet产品fix已有公开cache通过，只构成partial/ClosedLimited，不能据此声称当前official cache失败已修或整体绿。

## 下一主任务：两条明确选项，仅建议未执行
1. **推荐：当前official cache exact resource/消费条件与真实WPF发送序列对照。** 固定本轮official实际SHA及test身份，以现有28-byte产品fix公开cache通过为partial对照，逐项对齐真实WPF产生的资源类型/handle、创建更新引用顺序、source像素可用时机、cache绑定与失效、root/DPI及提交/SyncFlush序列；形成精确packet/资源消费差异，再针对确定差异最小修正并以原FF80像素门验证。不得回退exact20 decoder或泛静态重选门。
2. 可选：将已证外部foreground竞争纳入可控独立desktop的测试执行条件验证。直接C#使用CreateDesktop/SetThreadDesktop/SwitchDesktop，在建窗口前切线程desktop，逐次记录权限/API返回/GetLastError、实际desktop与foreground、真实loss序列；finally恢复原desktop并关闭handle，拒绝时明确Blocked。不用脚本、随机global设置或操作不明外部owner；这是独立受控条件实验，不能宣称默认full本应隔离，也不能替代默认full验收。

## 同步范围
本正式报告、failure-index、execution-receipt与next-session-handoff、current-work-item、current-stable-baseline、replacement-issues-and-plan-status、final-acceptance-remaining-matrix、wpfgfx-complete-replacement-validation-plan、default COM baseline/failures同步为本轮终态。入口顶部声明旧“当前”均历史，原历史证据不删除。同步文档逐份回读，不把文档写入当新增运行。
