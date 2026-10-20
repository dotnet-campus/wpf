# AD：呈现通知二进制布局修复

## 执行前登记

已阅读当前执行规则、完整替换实施方案、AB present-contract 报告与 PresentNotificationContractYTests、AA 图像修复报告。AB 已在 x64/x86 CrossThread 真实 HWND target、重复 FrameTime=123、commit/SyncFlush 消息泵下复现：官方 Reserved@4、result@8、refresh@12、time@16；旧候选字段提前4字节。

范围仅通知生产序列化/读取兄弟路径及其测试。保留最新工作区 AA 图像生产实现，不复用 V 归档候选。不得修改 GPU 图像、捕获、ABI、项目、入口、随机、golden、skip、阈值或 fallback。仅两个正式测试项目、纯 C#、双架构 Release；专用工具和 Windows 沙箱执行，不使用终端/shell/脚本。不清理其他产物。

## 执行顺序

1. 对照官方 exports.cs 及消息声明，检查通知序列化和读取的全部兄弟路径。
2. 最小修改生产偏移，并使 Managed 通知测试独立遵循官方布局。
3. 双架构 Release 常规构建及最新源码 NativeAOT 发布、正式 package。
4. 执行 AB 真实通知差分及现有通知/原窗口有界回归。
5. 回读实际 TRX、identity/SHA、原始通知包，落盘真实计数及未闭边界。

## 最终状态与实际修改

本包布局修复及有限回归已完成；完整通知合同未闭。独有 ignored 产物根为 `artifacts/replacement-present-ad-2026/`，下列路径相对此根。未清理其他产物，未改 AB 断言、项目、入口或 AA 图像代码。

生产仅修改 `Core/SameThreadChannel.cs` 四处负载偏移：Presented result 从4到8、refresh从8到12、time从12到16；SyncModeStatus HRESULT从4到8。依据官方 `include/exports.cs:323–341`，Type@0、Reserved@4、联合负载@8；官方 `rendertargetmanager.cpp:986–987` 同步状态写入 hrEnabled。无算法改动、无公共 ABI 新增。

兄弟路径检查：ChannelGraphicsCapabilities 已将能力负载写在8及以后；无负载的0x05/0x10消息保持零初始化；PostMessage队列、PeekMessage和导出读取直接复制原始字节，没有独立错误的Presented解包。未发现需修改的其他通知负载写入。

测试仅修改 `WindowPresentNotificationTests.cs`：两个完整64字节 NoPresent 预期采用独立官方result@8，新增同步状态完整64字节布局比较并finally撤销订阅和销毁。同步成功值0不能区分旧偏移，新增测试属于辅助覆盖，未声称已通过真实非零HRESULT独立差分。真实非零Presented字段由未修改的AB官方差分覆盖。

## 最后代码修改后的构建与正式运行

双架构 Tests Release 常规构建（含生产依赖）成功；双架构最新工作区 NativeAOT 发布成功；ComAcceptance self-contained Release 发布及正式 package 成功。生产为 `production/{x64,x86}`，正式包为 `package/{x64,x86}`，Managed包为 `managed/{x64,x86}`。不是V归档，不拼接旧full数字。所有运行均直接上传相应正式包或Managed包至Windows沙箱，没有脚本或额外Host。

|最终定向运行|执行/通过/失败|实际回读TRX|
|---|---|---|
|COM x64|8/8/0|readback/com-x64/TestResults/ad-com-x64.trx|
|COM x86|8/8/0|readback/com-x86/TestResults/ad-com-x86.trx|
|Managed x64|7/7/0|readback/managed-x64/TestResults/ad-managed-x64.trx|
|Managed x86|7/7/0|readback/managed-x86/TestResults/ad-managed-x86.trx|

已直接回读COM第74行、Managed第60行Counters：error/timeout/aborted/notExecuted均0。总计30/30通过，零失败/跳过；不是全量验收。COM各8项为AB真实呈现1、CompositionNotification3（异步/同步通知及递归锁）、发布身份1、原窗口回归3（图像更新/resize、矩形更新/resize、空白/旧帧负控）。Managed各7为通知6与身份预检1。子进程的单项控制结果不重复计入顶层分母。

## 新产物身份（已回读实际加载与包SHA）

运行环境.NET 10.0.13，MSTest4.0.1；x64/x86指针宽度分别8/4。官方 FileVersion `10,0,1326,50504`，commit `4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。正式包aot.sha256与AB candidate identity中的实际Loaded现场SHA完全一致。

|SHA256|x64|x86|
|---|---|---|
|实际NativeAOT候选|2BD7C2B9207ACF1E75182A8EC5C8B13B7B1B84E525045272733B158E8F069D5C|255E699B643E0A6F5250F80C28AE7086038E5FDA6D9959F3EEF5AE1F0C40DA1F|
|实际独立官方|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|
|Managed Tests.dll|E9BFEF5CBCA3EFF80FE7B559B63B226603EB786CE396F66072333DBF7E76B148|C6721F1D97D7C31DED2488589ADA53E9AF7CE081B10A15D40E2E41C1569A6310|

两个Managed实际产品SHA均 `0464F3455C9E5B684E91CFEF08902DFFB897ABB0A569FD8AE947E6E05BC1CA1B`；身份文件为 `readback/managed-{x64,x86}/Results/preflight-identity.json`。NativeAOT与Managed身份不可混用。

## AB真实通知原始回读

业务根：

- x64：`readback/com-x64/Results/present-contract-y/770b19e2fd014620b25c1c225d7da3e3/`
- x86：`readback/com-x86/Results/present-contract-y/5159efbe2dac444292820edda5cd271a/`

已回读两架构comparison.json中双方全部原始hex、identity、events及候选packets.json；原始compose-0.bin/compose-1.bin也全部随包回收。每架构官方和候选均返回两条64字节0x0A，原始字节完全一致（不只是归一化后一致）：前24字节 `0A00000000000000020000003C0000007B00000000000000`，其余40字节全0。明确验证Reserved@4=0、result@8=2、refresh@12=60、time@16=123。发送包含两份 `060000007B00000000000000`，另有在途销毁阶段第三份请求。

双方compose均实际收到 `WM_USER(0x400,123,456)`，各1次。官方再次flush仍有1次呈现仪表、销毁后阶段2次，候选分别0/1；仪表数量不是严格等价断言，也不能据此声称已闭销毁全序。双方before-close、compose-again、replacement-channel的通知队列均空。注册、commit、flush、注销、在途销毁、最终释放HRESULT均0。

AB代码和唯一DataRow(123,123)未改；比较仍零阈值，仅沿用既有对PresentationTime@16八字节的绝对QPC归一化。本次原始time本身也相同，未增加FrameTime变化或墙钟比较。

## 最新工作区AA集成有界窗口回归

两架构图像和矩形比较均已回读 `frames=12, threshold=0, exact=true`，本次新SHA保留AA生产修复且通过原窗口有限回归：

- x64图像：`readback/com-x64/Results/replacement-state-window/4491832266f2427c9eebdaa1b0006919/comparison.json`
- x86图像：`readback/com-x86/Results/replacement-state-window/39c93f3eabda4435a3e7f022047c5c38/comparison.json`
- x64矩形：`readback/com-x64/Results/replacement-rectangle-window/597da205e8c1466a8ad37f8c029a5dff/comparison.json`
- x86矩形：`readback/com-x86/Results/replacement-rectangle-window/7dd199a0df3d4bf88ef60ee8106bea10/comparison.json`

原始帧、子进程报告与身份均随完整任务目录回收。该窗口测试是RTB consumer，不能冒充屏幕HW图像验证。

## 未闭边界

1. 实际值算法仍未闭：生产目前按可呈现target返回结果2、固定refresh60及最小非零FrameTime；本轮只证明重复123的实际观察与布局一致，不证明刷新率、QPC、多个不同FrameTime或VSync/DWM一般算法。
2. 有target的同步呈现分支未做独立官方同场景覆盖；现有同步通知family及Managed无target不替代这一分支。
3. 销毁偏序未闭：替代通道队列空不证明已销毁通道无迟到消息，注销通知窗口不等价取消一次性0x06监听者；当前事件亦存在不同仪表次数，未增加必要QPC/销毁偏序断言。
4. SyncModeStatus非零HRESULT的实际独立官方字节差分未覆盖；本轮兄弟修复依据实际官方声明及生产源码，不能用成功值0自洽测试证明失败值路径已实测。
5. 不声称完整通知合同、全量替换、长期资源收敛、device-loss恢复或其他runtime版本已完成。

