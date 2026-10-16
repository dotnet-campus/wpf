# HW 当前独立报告：专用 STA 呈现窗口与原版对照

## 实施与验证

本轮实际代码/发布/沙箱测试，无终端或脚本。生产未改，SHA256 `3F4B424933C5F11076CAE3F90D6E216295EEC7D1335AE558D00CA8E99786990F`。新增原生代码0行。

审查发现旧 fixture 在 MSTest 线程创建窗口，仅在局部调用按 HWND 过滤的 PeekMessage；没有持续消息线程。原生 control 使用 EDIT，部分候选使用 STATIC，既有自有WndProc只覆盖cache/部分control。现统一自有窗口类：无背景刷，WM_ERASEBKGND返回1，WM_PAINT只ValidateRect，不写GPU像素。fixture专用STA持续真实GetMessage/TranslateMessage/DispatchMessage，窗口创建和销毁都回所属线程，ClassCleanup发WM_QUIT并Join。线程位于现有唯一COM项目，不新增Host、不引用生产托管程序集或测试ABI。

原生D3D9 control实际Clear→GetRenderTargetData成功读回FF123456。Present后EVENT query完成及DwmFlush，真实swapchain/client/monitor参数、ClientToScreen与遮挡证据保留。GetFrontBufferData使用1920×1080 A8R8G8B8系统内存，不以32×16 client staging冒充生产输出；前者读到FFF3F3F3，不是预期FF123456。未盲猜新坐标、未加skip、未改阈值/golden。

独立原版对照实际使用安装runtime的公开HwndSource/DrawingVisual，不用candidate命令ABI驱动原版；同一STA创建、Dispatcher到ContextIdle、DwmFlush后同样读到F3F3F3。此对照证明公开原版路径也未获正确桌面像素，但ContextIdle不等于GPU完成，不能宣称原版正常呈现前提已成功。初次尝试直接把候选协议复用到原版导致进程异常退出，未产生完整TRX，不计Green或完成；已替换为公开接口。

## 本轮原始结果（均已回读）

根目录 `Investigations/hw-contract-current/`；只回收Results，不回收执行器依赖副本。

| 证据 | 实际结果 | 原物 |
|---|---|---|
| 专用STA原生control定向 | 4 total / 1 pass / 3 fail / 0 skip | sta-native/Results/sta-native.trx |
| 公开原版独立定向 | 1 total / 0 pass / 1 fail / 0 skip | sta-original-target/Results/original-target.trx |
| 最终D3DImageTests整fixture | 42 executed / 9 pass / 33 fail / 0 skip，退出2 | sta-hw-final/Results/sta-hw-final.trx；RunID 9449dc8e-0699-45b1-859e-3fa76e18b89c；Counters行890 |
| 自主软件双缓冲格式族 | 29 executed / 29 pass / 0 fail / 0 skip | software-formats/Results/software-formats.trx；RunID 5acf18fa-8fe6-44e5-89b1-6f71638bf03e；Counters行192 |

最终HW和软件各自aot-identity.json已回读，均绑定上述生产SHA。MSTest4.0.1，win-x64/.NET10.0.1。42是该fixture分母，**不是**旧58项集合或完整COM分母。旧HW58/26pass/32fail RunID bb1f34da-0a43-46cb-a198-216b9acf3229仍在final-matrix/Results，不能覆盖或混加。

实际发布验收程序三次，均同一命令 `dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=true`，均成功；产物 `WpfGfxShape/artifacts/com-acceptance-package`。最后一次对应公开原版修订及最终42项、软件29项。未重新发布生产AOT。

## 判定与未闭

窗口owner与MSTest线程明确不同；Default/inputDesktop匹配、sessionState=0、cloaked=0、visible=1、iconic=0、WindowFromPoint等于目标，swapchain匹配HWND及32×16 client。D3D9 frontbuffer和GDI都读到背景，duplication创建成功但Acquire超时只是附加证据。Ex Present另返回141953144（S_PRESENT_MODE_CHANGED），保持失败不当作普通S_OK。

**当前沙箱仍未建立原版正常桌面呈现前提。** 不能把所有candidate失败归为生产bug或全部环境，也不能宣称此平台永久不支持。最小外部门禁是有可观测DWM输出的可交互桌面测试会话，在同一包先取得原生clear→display frontbuffer及原版target成功，再跑candidate/cache成功/失败保旧/恢复/逆序寿命。当前cache大部分止于control premise，后续语义仍未验证。不得继续相同超时实验追Green。

软件29是既有公开双缓冲命令/系统WIC格式消费者的本轮回归，不是新增FLOAT/HDR独立原版oracle闭包。下一自主大模块为FLOAT/HDR软件oracle与生产消费者成功/失败寿命，或C ProcessCreate真实消费者错误/引用/关闭；不依赖桌面观测，不重领已闭shadow。

GenProto最新为34/34，RunID a2a273cb-9a8f-4027-afec-f0bcc7d9c6ce，见GenProto-pen-producer-consumer-finite.md；不得再写旧28为最新。Pen仅有限producer/consumer模型，stage4Closed=false。七阶段/A/B/C、E2E-01–09与四已知外部输入不减，正式总门Red，整个任务未完成。
