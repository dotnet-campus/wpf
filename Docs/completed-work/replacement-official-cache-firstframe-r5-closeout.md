# official cache=true r5 — ExecutedFailed / cache evidence NotReached / Overall Open

## 实施与真实执行
预注册 `WpfGfxShape/artifacts/official-cache-firstframe-r5/preregistration.md` 在代码/运行前落盘。新增 `D3DImageTests.OfficialCacheObservation.cs`、VisualCache 原调用处最小hook和默认关闭 `OfficialCacheFirstFrameDiagnostic` 正式COM配置，无新增TestMethod/fixture define。未修改生产/Native/expected/阈值/两FF80/resize/mutation/target flags；无等色/新增flush/ETW/dump/委派/脚本。

构建绝对项目路径 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.ComAcceptance\WpfGfxShape.ComAcceptance.csproj`，配置OfficialCacheFirstFrameDiagnostic、win-x64：exit0/146warnings/0errors。COMpublish同绝对项目/配置/runtime/self-contained成功exit0。独立clean绑定正式candidate完整632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671，included framework均10.0.13。未发生产AOT。

实际task `6f6cff9a40bf4accb4cf370b29e973ce`：原精确FQN `WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive`，监督120秒/外180秒；TRX1执行0过1败/0timeout，child/MTP exit2，无kill/超时/drain错误，cleanup空错。工具未独立提供顶层进程exit，不冒称OS0。完整回收在 `WpfGfxShape/artifacts/official-cache-firstframe-r5/task1-readback`；参数/返回/截断/实际回读范围详见 `receipt.md`。

## 真正信号与未达到范围
业务JSONL实际仅2条：QPC identity和原生前置pixel。HWND=0x2260146/PID6592/window owner thread7648；actual模块handle140720707338240，实际路径任务根wpfgfx_cor3.dll，SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1。版本10.0.13-servicing.26504.104；test F4AAEF955DB2945B5A0A9AC0B99CB5EE606A6B3398DECB67A3682A4461E64BFB/MVID12f205e0-042d-43a3-b75d-48dfef61d532，.NET10.0.13 X64 pointer8。

**实际失败发生在connect之前：原生前置expected00563412，actual00F3F3F3。** 同一HWND visible1/iconic0/foreground自身/cover自身/default desktop/session1/cloaked0/client32x16/dpi96；producer swapchain32x16/format22/windowed1/cooperative0，但输出不等色。不能归遮挡/设备lost/官方cache根因，也不能把producer身份冒作consumer硬backend。official channel/target/cachebrush/dirty/matrix根本未创建，因此resource属性/真实public消息均NotReached，不把提交意图当现场测量，不称空队列或无通知。

这与历史共享full native通过而FF80实际0是**不同失败阶段**，不能相互取代。本轮至少已写代码、构建、发布、执行原case，非静态报告；但未完成用户所需cache现场resource证据，不宣称本目标完成。

## 消息观察语义
源码clientchannel.cpp921–969确证PeekNextMessage出队。本hook只在原业务结果确定后、cleanup/destroy之前对同channel有限drain；原case无drain/后续业务观察者，不抢初帧或resize事件，不新增注册/flush。记录hr/retrieved/raw，Presented仅按官方union偏移8/12/16解释，0x10为BadPixelShader；不期待公开type6，不添加候选私有export。无实际调用因为channel未创建。未登记instrumentation，因此Presented/NoPresent/consumer device保持Unknown，不冒内容/backend证明。

## 判定、预算、下一具体动作
task1前置失败依预注册首错停，task2仅允许独立通过后的新shared-full上下文，故NotRun而非同条件重跑。未确定fixture根因，不实施无证修复。下一必须先定位原 `PresentNativeControl` 在现行window owner线程/实际Clear-Present-readback之间的精确合同，比较本次F3F3F3与历史前置通过的正式原件；只有确定缺失合同才小修。之后需新发布授权重新进入此同case，获取已经实现的cache resource/消息记录；本轮publish余0，绝不修改后跑旧包冒验证。

预算：显式normalCOMbuild1/2、COMpublish1/1（implicit build1单列，保守合计build2）、sandbox1/2；Native0/生产0。task2剩余不用于追绿。诊断默认关闭；此配置才实例化，无全局运行开关待恢复。原full每arch1823/1821/2和旧23红均保留不相减；lossNotCovered/HWidentity/audio/naturalresourceOpen/完整替换仍Open，memoryGreen=null。
