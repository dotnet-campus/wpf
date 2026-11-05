# r18：session-before-Present 的有界事件诊断 / OverallOpen

## 状态与范围

整体 **OverallOpen**。生产修改0；未改变原业务像素 oracle、expected、通道容差1、采样点、DPI、设备/交换链/Present flags；无原版gfx加载/复制/fallback，无producer/RTB替代桌面consumer，无终端或脚本，无新增捕获框架，无retry-to-green。本次是插桩隔离子进程的**非原验收执行**：原fixture的Clear/Present及后续原AssertCombinedPixel保留，但先后时序被诊断改变，不能认证原组合通过。

已先读r17恢复入口和报告，审查VisualCache、SwapChainObservation、WindowThread、诊断targets及WGC实现。仅修改Tests内诊断路径与本报告/handoff。监督者未分派执行或采样。

## 实施与选择规则

专用targets生成R16Visual/R18Swap/R16Output，原文件未编辑。生成源码已亲读：原CreateDevice调用仍adapter0/type1/flags0x26，原Clear仍(0, NULL, TARGET=1, FF123456, 1, 0)，原非Ex Present仍(NULL,NULL,同HWND,NULL)。Clear成功断言、Present result断言、CompleteNativePresent、DiagnoseNativePresent、原GetPixel断言全部保留。

顺序：实际设备只读审计 → CreateForWindow → CreateFreeThreaded池（诊断buffer2）→ 订阅FrameArrived → StartCapture返回 → 一次原Clear/Present → 事件队列首个合法Present后帧 → 清理 → 原GDI oracle。没有按颜色筛选；不再次Present。事件回调仅在事件通知时TryGetNextFrame，不轮询找颜色。回调QPC、事件序号、帧SystemRelativeTime、阈值、尺寸、选择/丢弃理由写JSONL。帧要求事件QPC和SystemRelativeTime均严格大于实际Present返回QPC（Int128换算100ns），并且contentSize符合目标。

事件等待上限3000ms、最多32个出队帧、队列容量32；每张copy单独1000ms上限并超时取消；保存未做AlphaMode转换的Bgra8字节和BMP。停止时解除订阅，剩余排队帧写丢弃理由并保存，不重新考虑。容量溢出分支写明确理由后释放，不声称此分支也保存了像素；本次未触发。初始化/原Present是同步API，本地3000ms并不涵盖其调用自身阻塞，外层子120秒/父170秒及sandbox210秒兜底；不宣称全部API均有本地可取消超时。HWND owner独立STA，初始化/Present/等待均不在owner Invoke回调内。异步surface copy使用有界状态等待，不是FrameArrived替代轮询。

## 发布与唯一运行：全部已回读

publish共3次：首次编译完成但打包exit1，旧snapshot-final/Previous候选输入已不存在，未执行consumer。仅将诊断csproj输入改为现存r17 Package/Native候选；这不是系统原版。第二次consumer publish成功；第三次supervisor publish exit0。初次及第二次大stdout只保留尾20000字符，不声称亲读被截去的全部编译警告。监督发布11行日志已全读。未单独build/test，实际运行验证1次。

sandbox task **ee9e61bae03d44abb003a5ca6ad3197c**，产物根 `artifacts/candidate-desktop-present-events-r18/readback`。立即亲读Evidence/{parent-exit.json,desktop-exit.json,identity.json,desktop.stdout.txt,desktop.stderr.txt,parent.stderr.txt}，Results内模块身份JSON、desktop-observation-r16.jsonl全25行、wgc-r18.jsonl全23行；已加载实际WGC BMP原件，非仅读文档结论。

- 父监督子PID12392、桌面PID11168，各exit1、timeout=false、reaped=true、drained=true、killError=null。parent.stderr空。工具未单独提供最外层数值exit，不编造。
- Requested=Actual为该task Package/Native/wpfgfx_cor3.dll。
- Native SHA **E158C6277E3B3043E25A98D808C9D7B5ACB81CFE0DE8EBE5D3187B8381C1073B**。
- harness SHA **0AF455CE15AB7C2F6A71AD6B77E3F93C6D451C950962AC5F5870CA6404A4963E**。
- fixture SHA **4612F1FE74305C93C3A9210D33392B8C6CE4CF9079E0C2ED7ECC5656F81D8505**。
- 原nativeControl GetPixel actual00FFFFFF/expected00563412、原通道容差1失败；shader/connect/resize未进入。stdout最终original fixture cleanup joined。没有把WGC结果变成原门。

## 原始事件因果证据

QPC frequency=10000000，与本次SystemRelativeTime的100ns单位一致。

|位置|QPC / ticks|
|---|---:|
|订阅FrameArrived完成日志|45964556703020|
|StartCapture已返回日志|45964556722304|
|Clear前|45964556732124|
|Present前（Clear成功之后）|45964556750154|
|Present实际返回，HRESULT0|45964556756989|
|唯一帧SystemRelativeTime|45964557343750|
|FrameArrived sequence1 arrival|45964557375924|
|原control返回（含CompleteNativePresent）后日志|45964560047135|

session启动返回比Clear前早0.9820ms，比Present返回早3.4685ms。帧时间比Present返回晚58.6761ms；事件arrival晚61.8935ms。事件回调在managed tid6，控制/选择在managed tid2；原生HWND ownerTID3992，控制TID9724。owner在控制返回后也继续收消息，末尾WM_DESTROY2/WM_NCDESTROY130及join均可见；没有同线程等帧阻塞消息泵的证据。

唯一事件/唯一出队帧，32x16，按双时间阈值首次即被选择，无被拒帧，所以不伪造丢弃记录。selected-complete elapsed354ms，未触发等待超时、队列容量或copy超时。时间先后证明的是通知及帧时间在Present后，不证明像素来源已经关联到该Present。

## 帧与原像素

Results/wgc-r18-1.bgra是未转换原始Bgra8载荷（紧密复制2048字节），另有wgc-r18-1.bmp。记录明确width32/height16、sample(16,8)、RGB000000、targetCount0、zeroBgraCount512。

- raw SHA **E5A00AA9991AC8A5EE3109844D84A55583BD20572AD3FFCD42792F3C36B183AD**。
- BMP SHA **35D6B18A3D56BD28459E221A1D6596FAA1DA94AE6918300D0CC3E3321D4DAEE4**，与r17全零BMP相同；实际BMP已加载，载荷全零。
- 三张GDI捕获SHA仍为 **89682291F1B7CCAF28D5564628B58249690933CE16B9D986EBA145C446457A88**，样点白，不能写成全图白；本轮没有另做逐字节独立审计。原GDI业务oracle独立失败。
- client/frame/bounds均32x16、bounds(40,40,72,56)，DPI96。窗口可见、非iconic、cloaked0，cooperative0，样点cover为目标自身。只证明记录时状态，不证明任意时刻无遮挡。

## D3D creation flags / adapter / Caps审查

只读权威本机Windows SDK 10.0.26100.0：

- shared/d3d9.h:1832–1865：FPU_PRESERVE=0x2、MULTITHREADED=0x4、SOFTWARE_VERTEXPROCESSING=0x20；故0x26恰为三者组合，无PUREDEVICE0x10、DISABLE_PRINTSCREEN0x8000等位。
- shared/d3d9types.h:1298–1306：HAL=1、REF=2、SW=3、NULLREF=4。**NULLREF是device type，不是名为D3DCREATE_NULLREF的behavior flag**。
- Microsoft Learn https://learn.microsoft.com/en-us/windows/win32/direct3d9/d3dcreate 明确SWVP是软件顶点处理，Windows10后不推荐主要是效率，并不定义为禁渲染；消息线程与D3D线程分离要求MULTITHREADED，原值已含。

实际GetCreationParameters HRESULT0：adapter0、deviceType1(HAL)、focus同HWND、behavior38(0x26)。GetSoftwareVertexProcessing=1。GetDeviceCaps HRESULT0：capsType1、capsAdapter0、DevCaps1814512；保存304字节原始 `r18-device-caps.bin`。GetDirect3D/GetAdapterIdentifier均HRESULT0：driver **d3d10warp.dll**、description **Microsoft Basic Render Driver**、deviceName DISPLAY1；保存1104字节原始identifier。remote GetSystemMetrics(SM_REMOTESESSION)=0，当前session1，原会话查询state0、Default桌面。不能把remote0等同物理GPU，实际adapter已明确为Basic Render Driver，也不能据此推导不呈现。

未发现确定的NULLREF/禁渲染验收配置错误，因此没有改验收flags/type/SWVP，没有参数差分实验，也没有依据修改生产。

## 清理与限制

已亲读unsubscribed(events1)、release-session/pool/winrt-device及cleanup-complete(invoked=true,selected=true,events1,observed1)。frame/surface/SoftwareBitmap为using；session/pool/显式ABI释放在finally。手动Release原件为item2/factory0/inspectable3/dxgi2/context0/device0、HSTRING HRESULT0；audit factory Release2。保留非零计数，**refs0 Unknown**，不是逐对象最终引用归零证明。原fixture窗口销毁和join、父子回收均正常。本次没有动态覆盖超时/异常/队列溢出清理分支。

## 结论与下一最小项

本次直接证实：即使session和事件订阅先于一次完全相同参数的Clear/Present，首个双时间门后事件帧仍全零。因此“只有在先前Present之后才启动session”不能单独解释全部现象；但**StartCapture返回不是首帧ready确认**，本次唯一事件仍可能是session初始化帧。不能据此宣称已排除初始化黑、确认DWM实际未接收、宿主硬限制或生产gfx缺陷。

下一最小项不是无目标重采，也不是改flags：在同一现有WGC诊断路径加入严格有界的**首次FrameArrived基线就绪握手**，先记录/保存启动基线帧，再触发一次同参数Clear/Present，以实际返回QPC筛首个新事件帧，保留基线/拒绝/超时证据。它针对本轮新确认的缺口（session虽启动但未曾交付基线帧），不是按颜色等绿；若无新事件则直接记录有界超时，不追加Present或重跑。执行前需明确此假设，仍不算原验收执行。当前没有运行该后续项，也没有悬而未读的已运行任务。

r15/r16有限媒体结论按原身份保留；桌面原门、shader+resize、full/refs0继续Open。
