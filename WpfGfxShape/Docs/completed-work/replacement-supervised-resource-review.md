# AC：Z 资源与 X/Y 进展只读独立审查

## 范围与裁决

本包仅使用专用读取、搜索和文档写入；没有代码/项目修改、构建、发布、测试、终端、shell、脚本或间接执行。主代理只监督。规则亲读 `../handoffs/current-execution-rules.md:1–27`、方案 `../wpfgfx-complete-replacement-validation-plan.md:1–90`。本报告不是新运行证据。完整替换未完成，最后源码两个正式项目×x86/x64 Release 四格 full 未跑。

Z 有真实模块与真实 COM/vtable 调用，支持有限显式对象所有权/同步回调闭环；不支持长期内存/GPU 收敛、异步在途关闭或真实 device-loss 恢复。疑点仅报告，未修代码或测试。

以下源码及文档相对 WpfGfxShape 根；产物根记为 `Z=artifacts/replacement-resource-z-2026/final-readback/`。行号为本次读取时位置。

## 亲读：正式计数与身份

- `Z/x64/TestResults/replacement-z-final-x64.trx:142–146`、`Z/x86/TestResults/replacement-z-final-x86.trx:140–144`：均 total/executed12、passed11、failed1，error/timeout/aborted/notExecuted均0，退出2。不是 full，也不是资源测试12项全通过。
- 两 TRX `:8–24` 唯一 loss 探针失败：firstCreate=0，1920×1080/format22；secondCreate=0x88760868；firstCooperative=0（S_OK）。该次探针未观察到首设备 loss，在 loss 断言停止，不是候选已证明恢复失败，更不是恢复通过。
- x64 资源根 `Z/x64/Results/replacement-resource-z/9955137b6c4c4fcda8e87520a5722a9c/`，x86 根 `Z/x86/Results/replacement-resource-z/b0c89db746ce4402bb6f785cb2b1c80f/`，下称 R64/R86。
- 两根 `identity.json:1–26` 均亲读：实际沙箱包根官方与 Native 子目录候选，两独立模块同进程、先官方后候选；.NET10.0.13，PresentationCore10.0.0.0，官方 FileVersion10,0,1326,50504/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699，指针8/4。记录与 V 归档相符，不是 AA 最新生产身份。

|架构|现场候选 SHA256|现场官方 SHA256|
|---|---|---|
|x64|7B491B79976CB1E1C8C24792E067CFD8948F0D47F6C174933C6B99F230D331E3|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|
|x86|508C123706484CF830C2448BEE34F3624AF3A74E02B9E8920A766AA296638A6E|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|

SHA 仅说明已记录产物归属；AC 未重算文件 hash，不据此证明最新源码、行为正确或 full 已跑。

## 亲读：真实 refs、内容及检查器有效性

`Tests/WpfGfxShape.ComAcceptance/ReplacementResourceCycleTests.cs:30–47,60–113,118–155`：直接加载官方/候选，真实 factory/CreateTarget/GetBitmap；target 槽4 Clear、bitmap 槽7 CopyPixels、对象槽2 Release、真实导出 MILCreateEventProxy 及 proxy 槽3 Raise。GetBitmap 得到独立持有的接口，target 释放后仍读 bitmap，再释放 bitmap/factory/proxy；不是用托管布尔值模拟引用。源码 `Code/WpfGfxShape/Abi/BitmapRenderTargetExports.cs:155–188` 的 QI/释放链与该所有权解释一致，但当前源码不能单独证明 V 归档逐字相同。

`Code/WpfGfxShape/Abi/EventProxyExports.cs:27–61` 明确真实 COM 函数表与 descriptor 拷贝/原生对象及 GCHandle；测试回调 `:147–155` 实际累计 Calls/Disposals/Sum。这里是同步 EventProxy，不是 WPF Present 通知，也没有异步关闭竞争。

两根 `comparison.json:1–9` 亲读 Failure=null、Threshold0、Cycles16、FramesPerRole48、两负控true。每角色4预热+12测量，每轮红→蓝→producer关闭后蓝。测试 `:123–131` 对四类 Release 非0、Calls非2、Disposals非1、Sum非65280、缺帧、长度非256和逐像素错误颜色直接拒绝；并逐字节比较官方/候选，不仅比共同空白。

抽读 R86 `candidate/cycles.json:1–22` 与 R64 `candidate/cycles.json:227–242`：首轮/末轮均四 Release0、Calls2、Disposals1、Sum65280，保留三帧 base64；R64 `candidate/events.json:1–70` 顺序含红蓝两次消费、producer Release0、关闭后消费256字节、其余释放0和处置1。AC 未重新解码/重算所有 BGRA，也未遍历四角色全部 cycles/events；全量逐像素成功引用正式测试及 comparison，不称独立逐字节复验。

负控 `:43–51` 把真实观测副本第4轮 Disposals 改0或2，走同一 Detect。有效性限于缺失/重复处置信号检测，**不是实际漏 Release 或 double-free 注入**。非0 Release 拒绝分支存在，但本包没有对应独立变异负控；events.json 是轨迹，Detect 不解析事件偏序。亦未验证额外 AddRef/QI 身份稳定、任意共享图、关闭后迟到回调。上述为覆盖边界/后续建设，不将有限成功取消，也不升级为已证生产缺陷。

## 亲读：全部测量样本，不以增长冒充长期收敛

R64/R86 的 `official/resources.json:40–146`、`candidate/resources.json:40–146` 四份完整测量段均已读，12个点包含 private/working-set/handles/threads/managed。句柄/线程 x64 368/18、x86 388/18在本段稳定。

|角色|private bytes 轮4→15|managed bytes 轮4→15|观察|
|---|---|---|---|
|x64官方|48,807,936→40,898,560|12,372,816→5,494,336|轮7下降，后增长|
|x64候选|40,898,560→41,730,048|6,084,560→2,466,944|private台阶上涨，managed轮15下降|
|x86官方|43,307,008→33,734,656|11,334,424→2,998,800|轮15下降|
|x86候选|33,734,656→33,976,320|3,563,880→7,719,804|managed测量段持续增长|

测试 `:109–113` 保留帧/事件/样本并反复序列化，GC.GetTotalMemory(false)没有强制收集，且两角色同进程顺序运行。因此无法将增长归因于候选，也无法宣布长期收敛或缓存合理。最终 Release0仅对应这些接口引用，COM Release返回值也不能充当进程所有内部资源的直接计数。须另做受控保留数据/分配归因与更长预登记重复观测；不随意加容差阈值追 Green。

## 亲读：会话与 loss 边界

`Z/x64/Results/replacement-recovery-z/bf2037f11a9b493295dd1ed185c52927/session.json:1–14`、x86 `.../812eef9e36bf47a6ba18b77ee3662452/session.json:1–14`：SessionId1、UserInteractive=true、RemoteSession0、RemoteControl0、InputDesktopOpened/Closed=true、RecoveryVerified=false。不能笼统归为非交互/远程，也不足以确定驱动或独占接管失败根因。首设备观测始终S_OK即本轮未loss。

Z报告 `replacement-supervised-resource-recovery.md:81–92` 交接现有探针也缺 post-reset 像素 oracle。下一恢复包必须先证 lost→not-reset→同创建设备线程Reset，再更新新颜色、真实HW消费、官方/候选身份及最终释放；单独 Reset/cooperative Green不能关恢复合同。

## X/Y 与并行 AA/AB：亲读和交接分开

- X旧包亲读 `artifacts/replacement-x-image-2026/readback/com-x86/TestResults/x-image-x86.trx:49–50` 为4/4/0。显式 COM Stdcall、签名问题及旧 INVALIDCALL具体首个D3D操作未确认，来自 `replacement-supervised-x86-image-fix.md:13–22,75–77`，AC未重做生产 ABI 审查。有限RTB窗口通过不证明屏幕HW/长期资源。
- Y旧包亲读 `artifacts/replacement-present-y-2026/readback/{x64-run,x86-run}/TestResults/present-y-<arch>.trx:51–52` 各2/0/2。Managed各5/5为报告交接 `replacement-supervised-present-contract.md:91–100`，AC未回读其原TRX；Y官方未真正触发Present，不能以空队列等价关合同。
- 用户交接时 AA/AB并行、尚待独立回读。审查中上述两个共享报告已更新：AA声称单位各2/2、窗口各4/4及新身份（x86-image-fix:24–65）；AB声称真实异步Present已触发并发现官方 Reserved@4/Presented@8 与候选字段提前4字节、最终COM各1/0/1（present-contract:5–46）。这是**亲读报告中的后续交接，不是AC亲读这些新产物**。不猜最终结论、不改AA/AB报告；本交接仍将独立验收列为进行中，通知合同未闭。若AB原始消息核实，应将触发不足升级为可复现布局问题，不再把Y旧2/0/2当最新AB计数。

## 下一最小包：两线独立

1. 问题处理线：接收AA独有最终包并直接核顶层TRX/实际loaded identity/图像comparison/关闭轨迹；不编造旧INVALIDCALL精确首因。接收AB最终原始64字节消息、发送包、仪表事件/身份/TRX，独立确认布局差异后交生产所有者最小修复与双架构回归。不得以Managed通过冲抵COM差异。
2. 测试建设线：在原两个正式C#项目中独立补资源检查器非0 Release/缺帧或错色等针对性负控及必要偏序、受控资源归因；异步通知不同FrameTime/重复/销毁/必要偏序仍需，不用候选算法生成预期。真实loss需先有可重复触发与恢复后HW内容条件。
3. R/U限定成功仅三固定seed full/clip Clear与单一导入序列重复；广泛随机仍开放。S Capture初始及10次Acquire WAIT_TIMEOUT/resource0，读回前失败仍开放，不能用旧局部成功覆盖。此二项来源为既有交接/报告，非AC重验。
4. 持续媒体、长期GPU/缓存、非退化圆角、屏幕HW及其他历史缺口不因Z局部成功关闭。最终两项目×双架构Release full须绑定最后源码及现场身份，旧full/不同归档不拼接。源码无改不无故重发生产。
