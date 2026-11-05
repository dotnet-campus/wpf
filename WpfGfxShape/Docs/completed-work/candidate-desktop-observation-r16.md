# r16：桌面观测机制诊断与原件回读 / OverallOpen

## 结论

**原 nativeControl 失败，真实 shader/resize 完成0；未通过验收。** 最新已认证 x64 E158 候选已实际加载，不使用旧2E5身份。生产源码/配置修改0、生产Native发布0。未改 oracle、expected、容差、DPI、device/window flags、默认async；未 original/compare/full，无原版图形加载/副本/fallback，无终端或脚本。共享恢复入口未更新。

本轮实际实施并证伪到的范围：在同一原 Present 后，原 GetPixel 与三个限定32x16客户区的 BitBlt 捕获在原采样点均为白，没有发现“GetPixel白而BitBlt能看见目标颜色”的分歧。**不能因此证明GDI捕获就是独立的DWM最终合成观测，也不能证明环境硬限制。** 不具备修改原验收观测机制的确切依据，原断言及失败传播全部保留。

## 1. 先读与假设

先读 next-session-handoff 顶部、r15当前源码身份报告、r13及r9/r10必要段落，再读原 HardwareClosure/VisualCache/OutputWindow/PresentDiagnostics/WindowThread 和独立入口/监督包。当前工作区缺旧r13、r14 artifacts目录；没有假称重新读取那些不存在的历史运行原件。r9/r10的GPU staging FF123456、Present0、GDI控制及release返回值仅为历史报告事实，不移绑本轮。

本轮假设 H1：GetPixel 的 GDI DC 观测不反映实际目标窗口内容；若同次呈现后的screen SRCCOPY/CAPTUREBLT包含原目标颜色而GetPixel仍白，可提供观测分歧证据。H2：采样点不在真实客户/可见范围，或另一可见layer/WM_PAINT/WM_ERASEBKGND在呈现期间覆盖。单次业务运行，不重复 device/chain Present、不retry-to-green。

## 2. 官方API依据与边界

已取得完整正文：

- GetPixel：https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-getpixel 。返回指定DC逻辑坐标RGB，裁剪区外CLR_INVALID，不是所有设备都支持。该契约没有保证读回D3D9重定向表面/DWM最终合成，也没有把非CLR_INVALID等价于成功观测D3D。
- BitBlt：https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-bitblt 。DC之间矩形颜色传输，CAPTUREBLT包含叠在上方的layered窗口；只裁剪目标DC，必要时格式转换，无ICM。它仍属GDI/DC路径，不是DXGI duplication/WGC独立最终合成契约。
- DWM bounds：https://learn.microsoft.com/en-us/windows/win32/dwm/composition-ovw 。EXTENDED_FRAME_BOUNDS用RECT读取；本轮只查询，不改属性。

API读取收据摘要：`artifacts/candidate-desktop-observation-r16/api-semantics.md`。GetPixel首读504后二读完整；GetDC/DWM overview仅标题，不冒称读到正文；另一个redirection URL404和overview504未作为依据。

结论：官方语义不足以证明原GetPixel oracle必错；捕获见任意颜色也不等于看到目标颜色，更不能宣称原门通过。

## 3. 实施范围

仅独立验收项目修改：

- CandidateResizeDesktop.csproj 绑定 r15 snapshot-final/Previous 中保留的E158 Native；保留固定私有.NET/WindowsDesktop 10.0.13与原版gfx排除；不新发布生产Native。
- CandidateFixture.targets 在仅ComAcceptance引用构建时，从已读共享原件生成R16Visual/R16Output编译输入，插入原Present前诊断起点、原AssertCombinedPixel前只读捕获、原WndProc消息记录。**共享原测试源码未修改，原断言未替换。** 生成文件不是手工编辑对象。
- ObservationDiagnostic.cs：GetClientRect/GetWindowRect/ClientToScreen、DWM frame bounds、GetClipBox/PtVisible/GetWindowRgn、按z-order列出与目标相交的可见窗口；记录目标消息。三次BitBlt仅复制目标客户区，保存BMP和SHA；不保存无关桌面区域。
- 原 DesktopEntry 保持 StartPresentationThread → WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive → finally StopPresentationThread。
- Supervisor仅更新包路径与硬编码E158身份校验。正式项目默认缺oracle门禁未修改，没有传RequireReferenceData=false；ProjectReference继续禁用无关正式打包，仅独立无oracle场景。
- 新 DesktopObservationReadbackR16 是只读BMP字节统计器，不加载Native、不访问当前桌面、不更换oracle；独立sandbox读取首次运行拉回的截图原件。

诊断本身有GDI读取同步/消息日志开销，因此结果明确称instrumented，不冒称未插桩正式通过。

## 4. 实际运行与身份原件

业务task `1183bd912638474ea12fa6f8eeba0129`。证据根：`artifacts/candidate-desktop-observation-r16/readback/`。

已亲读 Evidence/parent-exit.json、desktop-exit.json、identity.json、desktop.stdout/stderr、parent.stderr，Package/Evidence/result.json身份与finalModules，以及Results逐阶段日志/截图/模块收据。

- 父监督子进程PID11372 exit1；真实desktop PID10872 exit1。两者timeout=false、reaped=true、drained=true、killError=null。外层无参数启动器工具未返回数值exit，不编造它exit0。
- Native实际SHA **E158C6277E3B3043E25A98D808C9D7B5ACB81CFE0DE8EBE5D3187B8381C1073B**，4895744字节、X64；Requested=Actual=本任务Package/Native/wpfgfx_cor3.dll；nativeBasenameHandleMatches=true。
- harness SHA **123877CA87754FAF4205C4FC7DA8A14A458735C81DB152BDD4D0B60A59AC9BB7**，MVID **6d3b425b-acf6-4434-808b-2c25cad253ef**；fixture SHA **17EE0087377856971698BFD42CAB88BB21C69059B30ABCB797AA3623E85C3275**。
- finalModules所见唯一gfx为该Native；系统d3d9.dll正常存在；私有WindowsBase/PresentationCore/PresentationFramework路径和SHA已读。requests=[]不冒称公开WPF导入已执行，本目标直接调用候选导出。
- stdout确认专用STA owner与finally `original fixture cleanup joined`。stderr完整原断言栈是 HardwareClosure → RunVisualCacheConsumer → AssertCombinedPixel，actual00FFFFFF / expected00563412、原逐通道差<=1失败。connect、shader及resize未进入。

## 5. 窗口、可见区域、消息与截图回读

Results/desktop-observation-r16.jsonl：

- HWND42665046即0x28B0456；owner PID10872/tid5716，调用tid11100。target=cover=root=foreground；Default/session1、visible1/iconic0/cloaked0、cooperative0。
- client[0,0,32,16]、window和DWM extended frame均[40,40,72,56]，frame HRESULT0；ClientToScreen成功，origin[40,40]、原点[16,8]→screen[56,48]。clipType2、clip[0,0,32,16]、PtVisible1。GetWindowRgn返回0不解释成空窗口，未取得自定义region与API失败尚未细分。
- z-order相交可见窗口首项就是目标，ExStyle8，WindowFromPoint命中目标；其余相交窗口在目标之后。未出现能解释采样点被另一顶层layer覆盖的观测。这里不排除非HWND层/跨时刻合成问题。
- before-native-present直到捕获仅消息127两条（WM_GETICON）及13（WM_GETTEXT）；无WM_PAINT(15)/WM_ERASEBKGND(20)。捕获后清理有WM_DESTROY2与WM_NCDESTROY130。日志只覆盖诊断起点后的目标消息，不声称创建前全历史均已记录。
- window GetPixel/screen GetPixel均16777215；三个BitBlt返回1/error0，分别window SRCCOPY、screen SRCCOPY、screen SRCCOPY|CAPTUREBLT。截图均32x16、源仅客户区[40,40,72,56]，未收集无关桌面图像。

三份原BMP SHA完全相同：**89682291F1B7CCAF28D5564628B58249690933CE16B9D986EBA145C446457A88**。先尝试加载图片及独立多模态读取；后者明确拒绝BMP格式，不能当成功分析。随后真正发布并sandbox运行只读字节统计器，亲读 `bmp-audit-readback/Evidence/bmp-readback.json` 三张摘要：**各白283/512，其他颜色229，目标RGB123456数量0；原采样点RGBFFFFFF**。所以不能把预览中的有色图案说成D3D目标颜色，也不能把整图描述成纯白。三份SHA一致是逐字节原件一致，不是仅视觉相似。审计工具外层数值exit没有由工具提供，不编造父子回执。

## 6. 排除范围、refs与下一最小实验

本次可以排除/显著限制：坐标落到客户区外、DWM frame与client screen位置不一致、采样点被GDI裁剪、所记时段WM_PAINT/ERASE覆盖、普通更高顶层HWND遮挡、只有GetPixel白而这三条GDI捕获能看到原目标颜色、误用2E5认证当前源码。

不能排除：GDI GetPixel/BitBlt共同读取未更新的重定向内容；非HWND合成层/远程桌面捕获路径；D3D9→DWM提交/调度的其它原因。**尚无证据确证宿主硬限制或candidate生产resize缺陷。** 本次不再重做devicePresent/chainPresent，不把历史GPU回读代替真实consumer。

refs：本次保存了WM_DESTROY/NCDESTROY与fixture join实证；原Release不记录计数，未动态新增逐对象Release收据，因此**refs0仍Unknown**。不挪用r10释放计数，不以exit/reaped代替COM refs。

下一最小实验：保留同一原consumer/颜色/resize/硬门，在原Assert前增加**独立于GDI DC的Windows.Graphics.Capture HWND限定捕获**（仅目标window，记录捕获尺寸、时间与同点像素，失败也保留原断言）；如捕获API不可用，保存精确HRESULT而非回退/替代oracle。该实验能区分“GDI共同观测陈旧”与“独立合成捕获也不见目标色”。本轮尚未实现或执行WGC，不冒称真实最终合成观测已闭合。即使WGC见色也只构成诊断分歧，需先证明API观测契约后才讨论最小验收修正，不能追认本次原门通过。

## 7. 执行账

- 专用publish **4次：3成功、1失败**。candidate首次因CustomAfterMicrosoftCommonTargets传入生产引用导致诊断target错误读取生产目录中的测试文件，编译输入不存在，失败；加MSBuildProjectName条件限定仅fixture后candidate成功，随后supervisor成功、只读BMP审计器成功。未改生产源码，未发布新Native。成功发布存在既有分析器warning，不称零警告。
- sandbox **2次**：原业务一次失败；截图原件只读审计一次产生完整统计。无业务重试。
- standalone build/test **0**；生产源码/配置修改 **0**；生产Native publish **0**；original/compare/full业务 **0**；oracle/expected/阈值/DPI/flags修改 **0**。
- 所有新增运行结果已回读；没有“已跑未读”待办。OverallOpen，真实桌面resize未认证。
