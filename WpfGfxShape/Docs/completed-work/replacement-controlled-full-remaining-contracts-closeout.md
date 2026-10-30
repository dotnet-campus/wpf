# ControlledDesktopFull 剩余合同核证正式收尾

## 后续原件更正：TRX存在并已精确路径亲读
[后续完整审查](replacement-official-cache-first-send-review.md)按本报告stdout/监督指定路径读取成功：artifacts/controlled-full-remaining-contracts-r1/official-original-readback/Results/remaining-official-cache-original-x64/TestResults/child.trx，全文67行（长堆栈分段补读）。1 total/1 executed/0 passed/1 failed，testId14e862b3-9b71-843a-9ff5-67f779272c33，executionb229983c-3c5d-4728-8145-f58610649d20，MTP2。actualAD1C/testE08B/10.0.13 X64，原生00563412/141414在connect前，结论仍Failed/cacheNotReached。下文“回传未见TRX/缺件”仅保留为当时枚举误判，不再是当前事实；不得以枚举遗漏认定缺件。无新运行。

## 终态
EvidenceCloseoutCompleted / NewCounterexampleObserved / FixtureUnchanged / OverallOpen。本轮没有确证修复，没有新增Passed。执行前已写草稿，现以本节替代。业务失败后未追绿重跑，candidate未执行，新full未执行。主要HWbackend/productconsumerloss/audio/naturalresources/finalmatrix均Open，memoryGreen=null。

## 原无参官方单例：明确可证伪的新现场
以受控full原包运行原case，不改源码/expected/配置/并行：task9343dc001fec44cb93f16c0b247994af，父6092→监督7968→MTP2036。精确实际参数与工具返回见 artifacts/controlled-full-remaining-contracts-r1/execution-receipt.md。
结果stdout总1/失败1/成功0，监督ExitCode2，MTP diagnostic末129行exit2。失败不是FF80/0：原生control前置expected00563412/actual00141414，在connect前退出。stages共21行全读，未创建channel/cache/world资源；两FF80、resize、mutation全部NotReached，不冒cache复现或修正。真实device/windows cleanup complete空错误；bitmap/surface仅空槽路径，不冒活资源释放认证。
这直接否定“该当前受控包official单例稳定全业务通过”的强假设，而不是再以单例/full泛环境差异解释旧红。无新依据不重跑。

readback根：WpfGfxShape/artifacts/controlled-full-remaining-contracts-r1/official-original-readback/Results。remaining-official-cache-original-x64下supervision40行、stages21行、stdout46行、stderr空、diagnostic尾129行、父desktop61行及两子journal已读到尾。部分长段工具4000字符截断，尾分段补读；不声称全部长行逐字显示。stdout/diagnostic宣称生成TestResults/child.trx，但本地递归枚举未见该文件，故不虚报亲读TRX或完整传输认证。失败依据独立stdout+同步diag+监督，原可用原件已保存，不将缺TRX写成Passed。

### 身份与配置
原ControlledDesktopFull（不是Release，也不是OFFICIAL_CACHE_FIRSTFRAME诊断配置）；重用clean，没有本轮fixtureSHA或MVID变更。实际fixture SHA E08B135C7AF6813C70E7909BDB7D4762DAB88A644AD5148F1104CEA2B766A387、MVID b3744fec-14e3-4271-94c6-ef5b0a6857fd、.NET10.0.13/X64。actual official任务根wpfgfx_cor3.dll=AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，10.0.13-servicing.26504.104/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。Native输入6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403仅file预检，不冒candidate实际参与。本轮B07C未运行，生产/Native均0改。

### 独立硬门
CreateProcess flags4、AssignJob成功、SwitchDesktop后Resume；父Default、MTP与owner均WinSta0/WpfGfxOwned_6092_dcd222fd1c6a42c1b18afcb7065f936f。窗口visible1/iconic0/cloaked0、foreground与cover同目标，cooperative0，不足以推出内容已交付。监督exited/timeout false/cancel false/kill false/output collected true/drain超时false/SupervisorError null。job-reap Total2/Active0/Terminated0，final query成功；恢复Default重新核名Confirmed true；五自有句柄关闭各true；final ChildResult2/Restored/Reaped/HandlesClosed true/Result2。工具自身没有独立OS exit字段；不混用监督exit。不是全部业务资源自然收敛证明。

## 历史cache=true官方究竟是否曾通过
是：Docs/completed-work/replacement-resize-e-unexpected-investigation.md 末53–60记录d35a30de53714ac0952ca17908b4e206，原无参official resize而非cache=false sampler：00563412、两FF80、64/48/32×24三帧FF80、两FF0080、cleanup全部通过。旧fixture C3D4F524400F5209C8DE44BD4621F2CA2431D1D7CF9BC0962FBE490FC57CD30F/MVID1989da39-bdf7-4b19-86b2-8decb666b6b9，不是当前E08B；当时actual official同AD1C，Native输入632F，不冒当前候选6BDF历史验证。该原目录当前枚举未发现，属于已归档认证记录复核，不冒本轮重读原TRX。

## official发送/消费契约：已排除的臆测及未闭合项
当前HardwareClosure.cs20–21精确RunVisualCacheConsumer(cache=true,colorFF00FF00,expectedFF80,geometry=false,resize=true,official=true)，非WIC：两interop源97/两visual39/drawing43、BitmapCacheBrush为shader sampler。VisualCache.cs official initialize0/connect0 CrossThread，candidate connect1 SameThread；同创建channel无reference；target/world DPI矩阵60byte，分离contentRoot39/transform66/SetRoot。官方initial Close/Commit→SyncFlush→逐image JhPresentImage→帧SyncFlush/DwmFlush/像素断言。不能把这些成功HRESULT冒内容epoch消费。
仓库官方源依据：PresentationCore/.../Generated/BitmapCacheBrush.cs375–390首次创建先InternalTarget.AddRefOnChannelForCyclicBrush，再UpdateResource；Visual.cs813–880注册channel/root计数后立即RenderForCyclicBrush；BitmapCacheBrush.cs102–140会Precompute更新bounds并Render，变化时ResourcesUpdated另安排异步render。Native BitmapCacheBrush.cpp ProcessUpdate持旧引用、GeneratedProcessUpdate后按target/cache变更RegisterCache；node.cpp1263–1290懒建CacheSet/RegisterNotifier/AddCache；BitmapCacheBrush.cpp125–143 realization主动PreCompute→UpdateCaches→GetBitmapSource。
因此“首次cache注册之后必需额外managed Invalidate/Commit”并非已证实命令合同，不能盲加。managed首次render在brush Update之前这一顺序已明确，而fixture早已有source drawing/content发送；尚未证明存在必需少发命令。CacheInvalidateThreshold、worldroot实际bounds、dirty传播、channel flags/policy及实际render epoch的完整绑定未完成认证；不将未知当确定根因。上述仓库源未逐字证明与实际AD1C二进制完全一致，不扩大结论。

## x86 native fresh：原件已证明逻辑跳跃，但不放宽内容
原full task5db5163c671d4958a6ce1f32a8859dd4，swap3 JSON60行和stdout28–49亲读：
- initial pointer-only LastPresent0/lastMouse41593198491787已剔除，不是误接pointer帧。
- callback门submissionStart41593198492096；原Present submitted41593198494928；dup LastPresent41593198668318；completion41593198889408；acquire QPC41593200282031。
- 因而submit < LastPresent < completion，且accumulated2/coalesced0/protected0。不能仅把门提高到submit修好：本帧已晚于submit。
- 首红backbufferFFFF0000/Present0/clientPixel000000FF，但screen00141414/front与dupFF141414；后二帧蓝/红都通过。三个独立桌面结果一致为旧灰；不能据此定位灰色由哪个surface/更新贡献，也不能颜色丢弃追绿。
helper DesktopDuplication.cs177–270只拿output时间门，没有dirty/move读取或source标识。WaitForDesktopContent83–96命名/报错称belonging current submission超出它实际可证的output时间谓词。
官方完整FRAME_INFO成员与remarks已通过网页搜索取到正文（两次直接Fetch504不冒成功）：https://learn.microsoft.com/ka-ge/windows/win32/api/DXGI1_2/ns-dxgi1_2-dxgi_outdupl_frame_info#1 。LastPresentTime明确为desktop image最后更新QPC；pointer-only LastPresent/AccumulatedFrames/Metadata均0；AccumulatedFrames>1为累积桌面更新。另官方Desktop Duplication API完整正文 https://learn.microsoft.com/tr-tr/windows/win32/direct3ddxgi/desktop-dup-api#1 ：dirty为更新区域、move为桌面内部搬移、coalescing可覆盖未改变像素、两列表可空，完整texture仍是当前desktop。这是output数据定义与旧灰实证联合反例，不是“没找到保证”推断。
结论：现有时间门不是目标swapchain同帧证明。dirtyrect与move可用现有纯C# COM调用直接取得，能证空间变化范围，但即使覆盖目标也不能单独证明source提交序号；coalesced尤不能。现有GetPixel/FrontBuffer/DwmFlush均未携带该present的帧token。需要独立observable：同一提交内另设不可复用frame marker，capture先按marker辨识帧、业务区域颜色仍独立断言；或真正携带目标source/present序号的消费者完成事件。前者原则上可纯C#现有D3D/Win32实现，但当前原case没有marker，不能无实施与验证宣称可直接取得；后者不能由当前普通Win32窗口/monitor/time API直接取得。不得用业务期望颜色自己充marker。未据此改断言、skip或标通过。

## before/after、回撤、预算与直接下一必要执行
Before：三个full红保留，缓存原因Unknown，时间门声称目标帧。After：代码完全不改；新增同包original单例在connect前灰色失败及精确x86 QPC关联反例，保留所有独立内容断言。回撤仅本轮文档/receipt，不涉及二进制与生产。
normalCOMbuild0/3、COMpublish0/2（implicit0）、sandbox1/4（单体外300/内120）、Native0、新full0、生产0；未花余预算追同业务失败。没有新测试或依赖，未调用终端/脚本/再委派。
下一必要执行不是再跑full或同单例：先在最小公共capture helper增加独立frame-token与业务像素分离的可观测合同，记录实际提交QPC、raw frame metadata、dirty/move/monitor与token；证明原参数首帧为何可识别后，原nativefresh定向一次仍逐项验业务与cleanup。另official需匹配当前AD1C CrossThread真实managed发送/commit消费epoch及source/cache dirty证据，确认少发项才最小补命令；先official原无参全业务通过再candidate。同包新现场未达到这两个执行前提，不开启下一staticphase或无依据构建发布。受控full原1823/1822/1与1823/1821/2不扣红；不是总体关闭。
