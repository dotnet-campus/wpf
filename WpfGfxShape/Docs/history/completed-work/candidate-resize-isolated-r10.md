# r10：SDK ABI 核实与一次 implicit swapchain Present 对照 / OverallOpen

## 结论

已先读 next-session-handoff 与 r9 报告，再读取专用 harness 与其最少共享代码。未发现本路径手工 COM 槽位、签名或结构布局的确切验收 bug；未修改生产。仅运行一次失败后独立诊断：**device Present 与 implicit swapchain Present 均 S_OK，GPU 同点正确，桌面双 DC 均白。原 nativeControl 仍失败，RunCycle 未进入，resize 完成0。** 不将这一结果扩写成宿主不支持呈现已获确证，亦不归因为生产 resize 缺陷。

## 权威定义与逐项审查

直接亲读本机 Windows SDK `C:/Program Files (x86)/Windows Kits/10/Include/10.0.26100.0/shared/`：

- `d3d9.h:429-452` 从 IUnknown 三方法逐个计数：device GetCreationParameters=9、GetSwapChain=14、Present=17、GetBackBuffer=18。Present 的显式参数为 RECT*、RECT*、HWND、RGNDATA*，无 flags；GetBackBuffer 为 UINT swapchain、UINT buffer、D3DBACKBUFFER_TYPE、surface**。
- `d3d9.h:882-896`：swapchain Present=3、GetBackBuffer=5、GetDevice=8、GetPresentParameters=9。其 Present 额外带末尾 DWORD flags，不能复用 device Present 函数指针类型。GetBackBuffer 不带 swapchain index。
- `basetyps.h:28` 定义 STDMETHODCALLTYPE 为 __stdcall，C 接口显式 this 在前。现有 unmanaged[Stdcall]、HRESULT int、DWORD/UINT/enum uint、HWND/指针 nint 与定义相符；本包为 win-x64，不存在 x86 栈清理不匹配证据。
- `d3d9types.h:1495-1501,1537-1562,1734-1764` 核实 creation、present parameters、surface desc、locked rect 顺序。实测 PresentParameters size64/window offset32/windowed offset40；creation size24；surface size32；locked size16/bits offset8，与 x64 自然对齐一致，BOOL 为32位而非 C# bool。RECT 本次始终 NULL，无按值 RECT 错传。
- SDK `um/WinUser.h:2847,2878,2888,2896` 核对 TOPMOST=8、LAYERED=0x80000、NOREDIRECTIONBITMAP=0x200000、COMPOSITED=0x2000000。实际 ExStyle=8，后三位均未设；窗口创建为可见 POPUP、class style0/background NULL，未见禁用重定向样式。仅能排除这个样式错误，不能据此证明 DWM 实际消费成功。

已成功抓取并亲读官方文档：

- https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3ddevice9-present
- https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3dswapchain9-present
- https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3ddevice9-getbackbuffer

DISCARD 下 source/dest/dirty 都必须 NULL；NULL source 是完整源，NULL dest 为完整客户区；非零 HWND override 指定目标客户区，零才采用 hDeviceWindow。当前 override 与实际 parameters.Window/cover/owner 目标一致。swapchain Present flags=0 是官方定义的阻塞调度语义，不代表已上屏。未在 BeginScene/EndScene 内调用。早期若干远程头文件 URL 返回404、扩展样式页504，未将它们当已读依据；槽位与布局最终以直接读到的安装版 SDK 为准。

## 唯一诊断改动范围

仅 `Tests/CandidateResize/NativeControlHeader.txt` 与 `NativeControlDiagnostic.cs`。不修改共享原测试、oracle、RunCycle、生产、csproj、DPI、flags、expected、容差或采样点。

Header 在原 AssertFailedException 后调用独立诊断，再 throw 保留原失败。诊断取得 implicit chain0，分别从 device/chain 取得 backbuffer 并 QueryInterface(IUnknown) 比较身份；GetDevice 验证 owner device；读取原 present parameters、函数地址所属模块及实际窗口样式/结构偏移。只在身份和 HWND 一致时重新 Clear 同色，再读 GPU 作证据，调用 chain slot3 Present(NULL,NULL,原HWND,NULL,0)，沿用 CompleteNativePresent 的 query 与双 DwmFlush 后采原双 DC。所有持有 COM 引用 finally Release。该路径发生在原 oracle 已失败之后，并不重跑或替代原 oracle，不存在用 producer 字节通过 consumer。

这是顺序的失败后诊断，不是独立创建第二个 device，也不是两个未插桩正式验收通过。r9 staging 插桩继续保留；诊断成功与否均不追认原验收。

## 实际执行与立即回读

本轮 publish2，均 exit0（harness 带6条 MSTest 分析器 warning，不称零警告）；随后 sandbox1，无重试。任务 `e6be04e754784ae7a58409435eb060a3`。原件：

`artifacts/candidate-resize-r10/sandbox-readback/Evidence`

已亲读父子 process、stdout/stderr、frozen identity、result actualNative/finalModules、consumer 逐阶段采样及完整 release 尾部：

- supervisor PID676 exit1，candidate PID12176 exit1；均 timeout=false/reaped=true/drained=true/killError=null。外层无参数启动器工具未提供数值 exit，不编造 exit0。
- Native SHA `2E5DC1BCC9CC7DEF60B0EE05C622954DC9BC7C2C3A57888FEE5767ED5705AFEE`，4853248 bytes；harness SHA `3CF053E1A5688D1F9626FACA038CB633447F2B7E5DD5841B99FCAA7FE0147C6E`，MVID `d282e8e7-ff7c-4164-879f-47e2663c7d6c`。actualNative 为本任务 Package/Native 唯一 gfx，basename handle 匹配；finalModules 已读，未加载原版 gfx。
- owner/current thread4280，HWND=41878614（0x27F0456），cover/focus/parameters.Window 均一致，client32x16，bounds(40,40,72,56)，采样(16,8)/screen(56,48)，DPI96，Default/session1、visible1/iconic0/cloaked0，cooperative0。
- `native-control-r9.jsonl:16-17`：原 Clear 后 actual format22、pitch128、pixel4279383126=0xFF123456，所有 staging HRESULT0；device Present result0。原双 DwmFlush 后双 DC 白，stderr 原 AssertCombinedPixel 容差1失败，expected0x00563412/actual0x00FFFFFF。
- `:52-53`：GetSwapChain/两 GetBackBuffer/GetDevice/两 QueryInterface/GetPresentParameters 全部0；DeviceBuffer=ChainBuffer=两 IUnknown identity=1947865247960；SameBufferIdentity=true、SameDevice=true。device/returnedDevice=1947865207264。实际 parameters32x16/format22/count1/multisample0/quality0/swapEffect1/windowed1/autoDepth0/depth0/flags0/refresh0/interval0。device 槽17/18/14 与 chain 槽3/5/9 函数地址全部可解析到 `C:/WINDOWS/SYSTEM32/d3d9.dll`，没有明显调用到其它模块/空槽的证据。
- `:54-56`：独立 chain Clear0，同点GPU0xFF123456（所有staging HRESULT0），chain Present0；NULL RECT/dirty、相同 override、flags0。
- `:57-79`：query 与双 DwmFlush 无断言故障，chain 后 windowDC/screenDC 仍16777215=0x00FFFFFF，同一 HWND/owner/cover/尺寸。不能以 chain S_OK 记为成功 consumer。
- `:82-89`：失败后 GDI 同点 written/read/双 DC 为5649426=0x00563412，仅诊断；`:94` DestroyWindow=1，WM_DESTROY/NCDESTROY 存在。
- stages18行，共16条 Release 记录；其中5条中间 refs 非零（4/3/2/3/1），对应仍持有的多份 surface/device 引用，**不是每次都 refs0**。随后同 surface 最后 refs0、chain refs0、device refs0、factory refs0，nativeControl-released；不扩大为所有内部对象均已证明无引用。
- 六份拒绝负控 assertions 逐份亲读：original/full/compare exit64/bootstrap，missingdigest exit1/digest-required，wrongsha exit1/sha-validation，wrongarch exit1/pe-architecture-validation；全部passed/noNative/noRequests=true。它们不是 original/full 真实业务通过。
- candidate-raw 只有 result.json、stages.jsonl、native-control-r9.jsonl；无 RunCycle frame/events/resources、无截图。failure stage execute-wpf-before，精确栈在原 nativeControl。consumer-not-completed 等三项失败保留。

## 停止点与账

本轮排除“device 取到的 backbuffer 不是 implicit chain0 那一个”及“明显槽位/参数/布局/目标 HWND 错误”的解释；没有找到可实施的确切修复。实际可见结论仍只是本沙箱当前 D3D9 桌面观测前置失败；尚无 DWM/驱动实际消费或其它独立桌面捕获证据，不能确证环境硬限制。停止更换 flags/DPI/驱动或重复实验。下一步若继续，应由新的明确消费边界证据驱动，不再重复本次同参数 device/chain 对照。

本轮 build0/run_tests0、publish2/sandbox1；生产修改0、Native构建发布0、original/full真实业务0、新视频0、RunCycle0。未修复因此没有运行三轮12帧及独立复验，不得将保留其源码当运行完成。r9累计 publish10/sandbox5 为交接历史，独立计账；加本轮为 publish12/sandbox6，不覆盖此前失败/超时记录。r6有限视频结论保留；OverallOpen，resize未闭合。
