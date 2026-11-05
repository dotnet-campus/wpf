# r17 WGC首帧、关键时序与实际字节审计

## 结论：OverallOpen，原resize未通过

先落初版报告和handoff，再完成本次亲读与只读审计。生产修改0，原consumer/expected/容差/DPI/flags/采样点/业务窗口参数不变；无原版加载、复制fallback、终端脚本或委派。新增仅Tests/DesktopByteAuditR17两个文件，读现存BMP，不加载候选Native。

已亲读原 task **1c0fd0ee19bb48d786efed447f383a93** 的 readback/Evidence/{parent-exit.json,desktop-exit.json,identity.json,desktop.stdout.txt,desktop.stderr.txt,parent.stderr.txt}，以及Package/Results模块身份、desktop-observation-r16.jsonl全21行、wgc-r17.jsonl全16行。父4124/子5332均 **exit1**，各自timeout=false/reaped=true/drained=true/killError=null；父stderr空。原nativeControl实际00FFFFFF/expected00563412、原通道容差1失败，shader/connect/resize未进入；stdout最终original fixture cleanup joined。

真实Requested=Actual，唯一模块为该task Package/Native/wpfgfx_cor3.dll，SHA **E158C6277E3B3043E25A98D808C9D7B5ACB81CFE0DE8EBE5D3187B8381C1073B**。harness SHA C6D1BC92D9D774549F7847A6FC2732527154CA6994C3F5BE39008964F6618DAC；fixture SHA CDADF7AB2B82F78A21A4E1B2FBBBA9825BC1E3D829C70D5D4A9E9E356D1157D8。未重跑覆盖原失败。

## 时序：首帧在Present后，但不证明新目标内容已合成

亲读CandidateFixture.targets、ObservationDiagnostic.cs相关路径、原VisualCache、SwapChainObservation、OutputWindow及WindowThread。专用targets插入诊断但不替换原断言。实际顺序：CreateDevice → Clear(0xff123456) → 默认窗口链Present返回且HRESULT=0 → CompleteNativePresent（事件query完成、两次DwmFlush返回）→ DiagnoseNativePresent → R16Observe几何/GDI截图 → wgc-before → WGC device/item/pool/session → StartCapture → 第一张非空帧 → 原GDI断言。

同次收据frequency=10000000，QPC单位与SystemRelativeTime.Ticks的100ns一致：

|位置|时间|
|---|---:|
|wgc-before（已在Present及CompleteNativePresent返回后）|45954465867295|
|CreateForWindow完成|45954467131461|
|StartCapture调用前start|45954467255367|
|首帧SystemRelativeTime.Ticks|45954468437500|
|首帧出队日志QPC|45954468555842|
|wgc-after|45954468965666|

首帧比保守的Present完成后下界wgc-before晚 **257.0205ms**，比StartCapture调用前晚 **118.2133ms**。没有精确Clear/Present入口/返回QPC，不能伪造该精度；但调用顺序及同一时钟下界足以排除帧时间在Present之前。session也是Present后才启动。

WGC为CreateFreeThreaded单buffer，最多3秒TryGetNextFrame轮询（10ms Sleep），只取首张非空帧、不按颜色选择。没有订阅FrameArrived、没有新内容事件/逐帧拒绝记录，不能补造丢帧理由与时间。**时间晚于Present并不证明源surface已被DWM有效合成**；同样不能直接认定建池初始帧、环境硬限制或生产缺陷。

执行用户“已有首帧在Present后且有据则只做字节审计和交接”分支：本次WGC修改0、consumer重新发布/运行0，不做无目标重复。未执行新的事件门选帧，不宣称已验证FrameArrived。未来只有明确新假设才可运行：先订阅事件，以明确Present完成QPC换算100ns阈值，仅接受事件通知后第一张时间严格大于阈值且尺寸合法的帧；固定总截止时间，逐帧记录时间/事件序号/拒绝理由，超时即结束，绝不retry到绿。当前未确证需要修复的诊断时序bug。

## 交换链、覆盖及窗口泵

ActualSwapChain实证width32/height16/format22/count1/swapEffect1/windowed1，hDeviceWindow=目标0x94F04B2，sourceRect=NULL/destinationRect=NULL/hDestWindowOverride=同目标，DPI96。原CreateDevice flags0x26未动。client及DWM frame均32x16，bounds=(40,40,72,56)，样点客户端(16,8)/屏幕(56,48)。WGC CreateForWindow同一HWND、contentSize及surface均32x16，不捕获监视器、不另开预览窗口。

PresentCondition/WindowFromPoint的cover是目标自身，同PID5332/ownerTID10636，visible1/iconic0/cloaked0/inputDesktop Default/session1/cooperative0。其他相交窗口coversSample均false；没有捕获预览覆盖自身的证据。瞬时命中不能证明任意时刻整个窗口无遮挡，亦不能证明合成来源。

Present/WGC运行tid2984，窗口消息tid10636；PresentationWindowThread独立STA GetMessage/DispatchMessage。R16Observe直接在consumer线程执行、不在owner Invoke内等待，无证据支持“同线程等WGC阻塞窗口泵”。WM_PAINT只验证、无background brush；销毁有WM_DESTROY/WM_NCDESTROY及fixture join。不能从消息间隔推定死锁。

## 只读BMP字节审计：已实际执行并亲读

隔离DesktopByteAuditR17为net10.0/win-x64/self-contained，发布内容仅包含原readback四张BMP，验证BMP头/尺寸/长度、重算SHA、逐像素计数RGB/第四字节。不连接桌面、不加载Native、不改截图。专用publish **1次exit0**，完整11行日志已读；sandbox **1次**仅运行审计EXE、30秒外层上限。工具返回完成但没有数值进程exit，**不编造审计exit0**。已亲读 `artifacts/candidate-desktop-wgc-r17/byte-audit-readback/Evidence/bmp-byte-audit.json` 全506行，count4且均有完整结果。

四张BMP均length2102、offset54、width32/signedHeight-16、planes1/32bpp/BI_RGB、512像素。

|文件|重算SHA256|样点RGB|全图计数|
|---|---|---|---|
|r16-window-srccopy.bmp|89682291F1B7CCAF28D5564628B58249690933CE16B9D986EBA145C446457A88|FFFFFF|白283、黑0、RGB123456为0、其余229；第四字节255共512|
|r16-screen-srccopy.bmp|同上|FFFFFF|相同SHA和计数|
|r16-screen-captureblt.bmp|同上|FFFFFF|相同SHA和计数|
|wgc-r17-window.bmp|35D6B18A3D56BD28459E221A1D6596FAA1DA94AE6918300D0CC3E3321D4DAEE4|000000|黑512、白0、RGB123456为0；第四字节0共512|

**纠正：三张GDI图仅样点白，不是整图白**，有229个非白像素。WGC文件像素载荷确实全零，不是查看器背景或仅中心黑。源码经过Bgra8/AlphaMode.Ignore转换，第四字节0仅证明文件内容，不证明原GPU surface透明度。Clear的RGB123456对应原GetPixel COLORREF00563412；WGC目标检查B56/G34/R12也是同色，无色序放宽。

## 有限生命周期，不称refs0

原WGC日志release-frame/session/pool/winrt-device均owned=true；手动COM Release返回item-abi2、factory0、inspectable3、dxgi2、context0、device0；HSTRING删除HRESULT0。非零值保留，不汇总成refs0。源码finally显式Dispose/Release，surface/SoftwareBitmap using释放；没有GC后投影内部引用或逐对象final refs证据。原父子正常回收/drained、fixture join及窗口销毁仅证明此次有限生命周期，不证明COM refs0或异常释放路径完整性。

## 合并r16/r15有限结论

[r16公开媒体失败恢复](candidate-media-failure-lifecycle-r16.md)：最终task1f56123dba984ceb879b2e7234a8081c父1468/子13296 exit0、三负控exit1；缺失文件80070002、无效AVI C00D3E84、失败URL C00D0035三次MediaFailed，同player恢复/立即替换，opened6/failed3/cleanupDiagnostics=[]有限通过。首轮父exit1日志阶段竞态失败保留，不覆写。并非reader Flush/drain/MFShutdown故障注入，任意epoch/refs0仍Open或Unknown。

[r15双架构有限视频](candidate-media-current-source-independent-r15.md)：x64 E158…1073B沿用r14两轮、x86 9FE6044FCCEE5C5A3D6E9452E8FF2506D4A67716942D11732CC60611BDA3E7FE从冻结源码发布且两轮通过。各自原harness下seek/EOS/连续红蓝/立即CloseOpen有限结论保留，不移绑本次桌面harness，不冒称本轮复验或HWND/full通过。

## 最小未知边界及续办执行账

**现存首帧时间晚于Present完成后下界，仍无法区分初始化黑内容与实际不可见目标**；缺新内容事件及源surface到合成关联。原GDI样点白、WGC文件全零，根因未知；不能据此归责生产或环境。无确证诊断时序bug，停止无目标实验。原nativeControl通过后才可继续原shader/resize；full可信oracle及逐COM refs仍Open。

本次续办publish1成功/sandbox1完成，均仅只读审计；桌面consumer新运行0、WGC修改0、生产修改/发布0、独立build0、run_tests0、新视频运行0、终端脚本0、委派0。历史r17任务不计作本次新增运行。报告/入口均落文，原父子和帧/cleanup收据及全部新增审计输出已亲读，不留未读运行产物待办。**OverallOpen**。
