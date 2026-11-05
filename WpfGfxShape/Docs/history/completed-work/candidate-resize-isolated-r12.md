# r12：独立 RTB-lifecycle 有限门通过；桌面组合仍 Open

## 原件亲读

任务 `27cdd919191b4d11832095f43cbfb5dd`；原件 `artifacts/candidate-resize-r12/verified-readback/Evidence/`。本次已亲读 rtb-lifecycle-process.json、rtb-raw-verification.json、candidate-raw/events.json、resources.json、supervisor-exit.json。路径中的 Evidence 与 task 编号是目录和任务标识，不是同一个目录名。

父 PID12864 exit0；RTB 子 PID4328 exit0，timeout=false/reaped=true/drained=true/killError=null。三周期各4帧，共12帧，24x16→40x24，Pbgra32 全内容 exact。每周期 close-drained closed=1，callbacks=callbacksAtUnsubscribe=9，退订后无增长；producerAlive 三项均 false。仅 RTB 内容与窗口生命周期有限门通过，不是 HWND 桌面呈现、nativeControl、COM refs0、full 或 x86 认证。原 RunCycle 不测 COM refs，不可把弱引用释放改称 COM 释放。

认证身份（历史回读，不冒充新计算）：Native SHA256 `2E5DC1BCC9CC7DEF60B0EE05C622954DC9BC7C2C3A57888FEE5767ED5705AFEE`；harness SHA256 `582D09AA1B027BCC035346787DA546157F7F91E7D3C9F689115BAF1C4B9260A3`，MVID `718c065d-349d-4392-aac9-26fbf72b8a78`。前账 publish4（3成功1失败）、sandbox2。拒绝 full 的 exit64 是拒绝负控，不是 full 业务执行。

## 正确桌面入口纠正

`D3DImageTests.HardwareClosure.cs` 的 `WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive` → `RunVisualCacheConsumer(true, 0xff00ff00, 0x0000ff80, false, resizeWindow:true)`。此 shader 就是原目标，撤回“shader必定误绑”的旧判断。原链包含 SetWindowPos 64/48/32x24、SendOutputWindowSettings、close/commit/present、DwmFlush、GetDC/GetPixel，保留逐通道容差1。

原 fixture `D3DImageTests.WindowThread.cs` 的 ClassInitialize 首先创建 PresentationWindowThread，STA GetMessage/DispatchMessage，ClassCleanup Dispose/join。CreateOutputWindow 强制要求 windowThread 非空；早期直接调用缺少此初始化，不能当场景无关或环境限制证据。原 VisualCache 自带 nativeControl 硬门，失败即不进入 connect/consumer，不得跳过。

桌面后续执行及当前剩余项见 next-session-handoff 的最新顶部条目；生产修改0，不改 oracle/expected/threshold/DPI/flags，不加载 original，不使用复制 fallback。
