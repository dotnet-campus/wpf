# BG 硬件呈现最小调查（阻塞交付，非完整验收）

## 结论

**未取得可重复的“官方有效新内容成功、候选有效新内容失败”最小硬件业务差。** 本轮三个有界任务证实当前原生呈现/桌面读回前置阻塞；未证实候选生产根因，因此不修改生产或共享夹具，不重发生产扰 hash，不运行 full。BD x64 1740/1699/41、退出2仍然有效；本轮不从41中减项。BD x86中断根因仍未知。不能将37项全部归环境，不能宣布 resize、cache 或 device-loss 恢复成功。

唯一 tracked 新文件为本报告。产物独占 `artifacts/bg-hardware-present/`。未修改曲线、BF WindowQ、项目、入口、打包、共享交接、golden、阈值或skip，未新增Host/第三工程/原生探针；全程专用文件工具与Windows沙箱正式ComAcceptance纯C# exe，无终端或直接/间接脚本。没有涉及共享文件的写入。

## 阅读与取样

已阅读当前执行规则、完整替换方案的各节、BD baseline和BF baseline-review；读取正式项目文件清单以及相关现有呈现测试和生产调用。长文件工具返回存在截断，不声称逐字核验所有历史业务或所有生产分支。

亲读 BD `artifacts/bd-comacceptance-baseline/full-x64/TestResults/bd-full-x64.trx` 13613–13621：Failed，1740执行、1699通过、41失败，timeout/aborted/notExecuted均0，退出2。BD官方PublicHwnd成功与候选失败的历史不对称必须保留；不能拿本轮共同前置失败抹除它。BF x86在sampler业务前失败不能确定BD中断根因，WindowQ软件RTB通过也不是硬件证据。

最小切片选择：现有 `WhenNativeGpuAlternatesFramesThenEachDuplicatedPixelMatchesItsSubmission`（DISCARD=1、COPY=3），对照实际官方PublicHwnd与候选PublicHwnd；另以现有官方/候选resize同一任务复核。前者原生独立于候选绘制算法，能同时观察backbuffer新色与桌面fresh-frame，适合先检验读回链，不必新增探针。

## 实跑与父TRX

复用BD正式Release自包含10.0.13双架构包：`artifacts/bd-comacceptance-baseline/{x86,x64}/com-acceptance-package/WpfGfxShape.ComAcceptance.exe`。生产与测试均未改，故不重新发布，也不声称这些旧快照验证并行BE改动后的当前生产源码。专用沙箱工具完整回收每个任务目录，不仅回收TRX。

| 本地产物子目录 | task | 过滤范围 | 结果 |
|---|---|---|---|
| probe-x64 | cef3941706404bb899086f8571e123f8 | native alternating两行 + 官方PublicHwnd + 候选PublicHwnd八行 | 11执行、0通过、11失败，退出2，17.717秒 |
| probe-x86 | c93baf312d2d4d0284c408073bb261c8 | native alternating两行 + 官方PublicHwnd + 官方/候选resize + PublishedArtifactTests | 6执行、1通过、5失败，退出2，16.353秒 |
| resize-x64 | 169cb09aa19c415db1cb6977a726ab5c | 官方/候选resize + PublishedArtifactTests | 3执行、1通过、2失败，退出2，2.491秒 |

父TRX分别为各目录 `TestResults/bg-hardware-x64.trx`、`TestResults/bg-hardware-x86.trx`、`TestResults/bg-resize-x64.trx`。亲读Counters/RunInfo行307–315、203–211、109–117：三者timeout/aborted/notExecuted均0，退出2均有父RunInfo明确记录。两项通过仅为发布产物检查，不是硬件内容通过。单次上限120/90/60秒，无full重跑，无重试到绿。

## 实际身份与backend

亲读各任务Results/aot-identity、resize角色hw-module JSON及父TRX stdout。实际requested=actual模块路径，官方在任务根 `wpfgfx_cor3.dll`，候选在任务根 `Native/wpfgfx_cor3.dll`。`PresentCondition.production`固定打印候选路径，即使官方角色也如此，不能据此误判官方加载了候选；模块身份以hw-module为准。

| 项目 | x86 | x64 |
|---|---|---|
| runtime / 指针 | .NET10.0.13 / X86 / 4 | .NET10.0.13 / X64 / 8 |
| 候选SHA256 | E0D3F5EFEDFACFD5CC3491DD43B194E19028E8B541A462123C8F73C73D6E25B5 | 1C381539E7C82DDD75908C0563F0AC062E583166BDB23AEE066BDD21E838752F |
| 官方SHA256 | 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834 | AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1 |
| 测试SHA256 | 84190DE3614F5983BAB69CE9CE0F9FB031EC5AB1964D55482461D442E31FC31F | D64A04DE7F85C63176B16C18AAFC4B470FB2D4E8BD573B58BB939A42237B6FE4 |

官方FileVersion `10,0,1326,50504`、ProductVersion `10.0.13-servicing.26504.104`、commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。候选1.0.0.0/ProductVersion1.0.0+b4de881638949d7fdbb653f9650fb21fd07bc685；不拿版本字符串冒充dirty源码凭证。本轮未重新提取PresentationCore hash/MVID，不把BD表格伪称本轮重新观测。

DesktopDuplication实际记录 **Microsoft Basic Render Driver**，vendor1414/device008C，LUID00000000:00004E71，DISPLAY1，1920×1080，monitor与windowMonitor均10001，create=0。这是实际D3D接口backend证据，不是物理GPU或候选目标设备身份的完整证明。候选硬件业务未到达，不能声称已验证候选backend消费新帧；无软件fallback。

## 原始内容、完成时序与窗口证据

native两种swap effect、两架构均：

1. staging实际读到 `FF123456`；随后Clear红色并读到 `FFFF0000`，断言使用本次color变量。日志中的 `NativeGpuStaging expected=FF123456` 是旧的固定诊断文字，不是本次红色预期；没有修改它，也没有据此判失败。
2. Present返回0，`CompleteNativePresent` 已经过EVENT query Issue/GetData（最多5秒）和两次DwmFlush；日志有提交前、完成后QPC，postPresentBackbuffer仍为 `FFFF0000`。
3. DesktopDuplication提交之后约5秒持续 `887A0027`（WAIT_TIMEOUT）、resource=0，没有属于当前提交的桌面内容帧。x64 DISCARD最初一次frame仅lastPresent=0、lastMouse非0，鼠标帧不被接受；COPY和x86初始即timeout。不能把backbuffer正确或query完成当成桌面新色消费成功，更不能把mouse-only帧算内容帧。
4. 两架构各两种swap均在第一红帧超时，蓝色及之后红色未执行。没有完整red/blue/red桌面消费序列。

窗口证据：client32×16、DPI96、visible1、iconic0、cloaked0、Default输入桌面、session1/state0；cover HWND与target相同，foreground为目标，coverClass=WpfGfxShape.HardwareOutput；显示范围包括目标坐标(56,48)。x64第二候选窗口位于(120,80)，读点(136,88)，仍失败（实际CCB18C而非预期563412）。这排除了这些观测时刻的简单遮挡、最小化、零客户区、offscreen猜测，但不能证明整个呈现期间桌面驱动/合成链正常。

原生前置Clear的期望COLORREF为00563412；多数GetPixel和screenPixel却是00CBAE87。官方真实WPF PublicHwnd本轮双架构同样得到00CBAE87而非目标颜色。官方与候选resize同task、同原生窗口夹具、同32×16原生swap-chain参数均在该前置失败；未连接channel、未创建业务资源、未实际resize，不能称候选resize内容失败或两者恢复等价。

x64候选PublicHwnd非Ex两行实际继续执行到interop selection，随后独立原生GetPixel断言失败，尚未create InteropDeviceBitmap或connect/channelCreate。Ex六行在PresentNativeControl的result=141953144非0失败，尚未完成前置。该值仅作诊断，不在未核清设备状态时改为接受成功，也不拿错误码不等当业务兼容门。

原始像素数值、提交/完成QPC、DXGI错误、窗口状态、堆栈、实际模块均保存在父TRX stdout及Results JSON；完整包一并保留。失败在读回前置，未产生有效桌面帧缓冲、业务事件/资源循环或新色resize图像，因此不能交付不存在的二进制帧或GPU资源收敛证据。finally路径有Release/DestroyWindow，只有源码清理路径，不足以证明实际GPU资源最终收敛。

## 生产路径核查（未认定bug）

- `Abi/HardwareRenderTargetExports.cs` Target.InitializeHardware：MonitorFromWindow匹配adapter，创建硬件manager/device，COPY/A8R8G8B8/指定HWND的additional swap-chain renderer，随后Resize。
- Target.Render：EnsurePolicy、Clear、GeneratedHardwareVisualRenderer.Render，再Renderer.Present；存在显式软件策略分支，本轮未用其替代硬件验收。
- `Core/Direct3D9SurfaceRenderTarget.cs` Present/PresentCore：device guard、实际swap-chain Present、失败状态处理；不是桌面fresh-frame oracle，返回成功不能证明桌面已消费。
- Resize：结束layer、清除valid contents、释放旧surface/swap-chain、更新尺寸、重建additional swap-chain/backbuffer、清除失效矩形、更新renderingEnabled。仅阅读不证明cache正确性。
- 实际上层 `Core/GeneratedVisualTargetResources.cs` CreateHwndTarget调用HardwareRenderTargetExports.Create；UpdateWindowSettings遇非零新尺寸时创建新的target后释放旧target，不是直接修改原Target只读Width/Height；Render再调用HardwareRenderTargetExports.Render。因此不能只看到Target.Width/Height readonly便预认resize bug，也不能跳过上层target替换/per-device cache生命周期。

本轮原生控制阻塞发生在这些候选业务调用之前，没有证据支持修改InitializeHardware、Render或Present/Resize。未为掩盖问题新增等待、放宽阈值、改golden或skip。

## 分类与尚缺具体证据

已证实：本轮双架构、DISCARD/COPY原生backbuffer新色正确；query/DwmFlush完成但无桌面fresh-content；官方PublicHwnd同轮也无法读到目标颜色。属于**共同呈现/捕获前置阻塞**，而非已定位到具体驱动或夹具根因。尚不能区分驱动/远程桌面状态、合成提交、捕获观测时序的更深原因。

未覆盖：候选目标新色硬件消费、官方/候选有效内容零差、业务resize、cache失效、后续shader/sampler、真实device-loss/reset后新色消费、GPU资源收敛。没有触发device-loss，不称恢复成功；没有当前源码双架构生产修复验证。

下一步所需证据（非盲重跑）：在可呈现的交互桌面/backend上，先同task确认原生red/blue/red、两种swap每次fresh lastPresent与实际像素；再同task执行官方/候选相同场景，保存目标实际adapter/device/swap-chain身份、全尺寸新色buffer与窗口resize前后事件；若官方稳定成功且候选失败，再在同一最小切片区分候选backbuffer、Present请求、桌面消费，才改有证据的生产节点。resize须实际40×24等新尺寸新色，不接受32×16前置。device-loss另须第一设备真实lost、Reset成功、reset后新色被硬件消费，缺一均未完成。

本报告是有限阻塞调查交付，不是最终四格，不是完整替换，不宣称BD x86中断已修复。保留BD历史不对称与全部未闭业务给主线继续处理。
