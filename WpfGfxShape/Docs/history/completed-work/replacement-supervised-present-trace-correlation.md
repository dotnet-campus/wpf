# HR 固定首红 Present 短 ETW 关联

## HT终态：唯一部署纠正后，目标Blit桥接成立；occlusion根因未证

### 旧HR补核（不是重跑）
旧task `20b167c954c04869aa8083bf6eb37f32` 原件位于 `artifacts/hr-present-trace/readback/x64-native/Results/`。精确读取 `hr-x64-native/TestResults/child.trx` 全46行（目录枚举未列出但按stdout实际绝对路径对应相对路径读取成功），1执行/0通过/1失败，Exit2；异常栈定位CreateOutputWindow，手动new D3DImageTests没有触发MSTest ClassInitialize，owner为空，Present0。旧监督39行、stdout23行、HI3行、control32行全读：无timeout/kill/drain失败；ETW Start/三Enable/Open/STOP/Process/Close均0，control1/graphics0/lost0/joined、权限恢复成功。它是窗口设施错误，不是目标bridge失败，也不能抹掉ETW自身正常成功。旧actualtest BFD689C1ADB6772ABA6461B0D33E9DD08F495D4C8E0DDB80EEE1B2643F6915C6 / MVID389640e5-032d-4651-957f-5debc35e4065 / .NET10.0.13 X64。

### 最小修复及部署
唯一源码修改为 `HpGraphicsTraceTests.Control.cs` 的HR回调接到已经准备好的 `HrRunWithWindowOwner()`，不再直接调用测试体。该入口创建既有PresentationWindowThread（STA、GetMessage队列ready后返回），try运行固定测试，finally调用既有StopPresentationThread（Dispose/WM_QUIT/Join/清空）。窗口创建和Present经owner Invoke；窗口关闭复用既有DestroyWindow路由，实跑destroyed1。异常同样走测试资源finally、owner finally、ETW finally；未改共享owner/公共窗口捕获/NativeRun/ExitDiagnostics/生产/打包/consumer/DRDS/CF CX CY，也未动HU stream文件。没有改变Present参数、expected、阈值或规则。
一次正式专用发布成功，正常分析器，无NoWarn/规则关闭；独有 `artifacts/ht-present-trace/obj/bin/publish/com-acceptance-package`，显式绝对PublishDir；正式ComAcceptance自包含win-x64，NETCore及WindowsDesktop均10.0.13。既有PackageSelfContained目标clean package，白名单419项实际集合419，missing/extra空；Contracts双文件齐全。HA native SHA `8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0` 仅打包依赖，没有目标加载或consumer使用；未重发生产。上传源仅clean package，回收另置readback。

### 唯一HT task及身份
唯一task `0c1cdf3ad01e45e0bed28f5d51f47c00`，正式exe经既有--supervise-exit运行精确HR方法；没有第二次正锚、没有full或旧task重跑。回收根 `artifacts/ht-present-trace/readback/x64-native/`。
实际test SHA `6AAB005D0C31B7C8015F9C18C00A8E4AC2838A93C5573997001416FEB5A21CE1`，MVID `f7cb0948-3929-4ef4-9d2c-1abb31db1a14`；.NET10.0.13 X64，PID5296，测试TID10488，STA owner TID1448；PresentationCore SHA0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31，10.0.13-servicing.26504.104。监督39行与child TRX44行全文读取：total1/executed1/passed1/failed0/notExecuted0，子Exit0，非timeout、无kill、输出收集完成、drain未超时、stderr空。外层工具未给独立数值Exit，不另造；监督不是MTP，无父TRX计数。

### 真实调用、窗口及内容
窗口HWND9110896（0x8B0570），ownerPid5296/TID1448，Default桌面，visible1/iconic0/cloaked0、前台且cover自身，32×16，monitor65537。创建HR0；Ex owner DISCARD/Format22/count2/interval1/behavior0x26，实际swapchain descriptor逐字段吻合，没有参数纠正。配置路径DISPLAY1/LUID20081/source0/target5527112，active/Available1只是配置，不是scanout。
仅首红FFFF0000：clear/读回HR0；实际PresentEx一次，submitted QPC35813091641028，returned35813091645984，present=0，stateBefore=0/stateAfter=0，GPU完成35813092500522。backbuffer/duplication/front均FFFF0000，client/screen COLORREF255，accepted=true；duplication fresh lastPresent35813093775148、accumulated11。countResult/statsResult均-2005530516，不把零统计当Present0。refs设备0/工厂0、staging/backbuffer refs0、destroyed1；complete allAccepted=true/consumerEntered=false；owner finally正常返回后ETW流程继续，子正常退出。一次成功不覆盖HM/HN/HL旧occluded或无fresh历史，不外推候选/恢复/一般HW可用。

### ETW控制、TDH及目标桥接
自有session `HR-502827896283482da356da6ab51c691b`。独立setup预算5秒、capture请求1.5秒、STOP等待4秒与finally join独立预算；Start QPC35813090063989、STOP请求35813105067442、完成35813105121300，总1.5057311秒<=2秒。Start/三个窄EVENT_ID Enable/Open/Stop/Close/Process均0；过滤control[1]、DWM[67]、Dxg[166,501]，无fallback。控制marker35813090133236，实际event PID5296/TID10488/QPC35813090134125，raw Int64精确吻合；TDH1168如实保留，未声称控制TDH解码成功。controllerEnd controlObserved=false是STOP排空回调尚未送达，最终control1已消费，不当丢失。
实际graphics仅DxgKrnl166一条，88bytes；metadata sizing122/read0，全部17个标量TdhGetPropertySize/GetProperty status0：hwnd9110896、pDmaBuffer18446714982421196864、PresentHistoryToken0、hSourceAllocation18446714982206120720、hDestAllocation18446714982206130784、bSubmit1、bRedirectedPresent0、Flags1、Source/Dest各(0,0,32,16)、SubRectCount1。事件PID5296/TID1448/QPC35813091645422；真实HWND精确等于目标且同owner PID/TID，事件落于实际Present调用区间。这是字段身份桥接，不是仅时间相邻：**目标Present到DxgKrnl Blit桥接成立**。Token0，DWM67/501均未回调，不能再串接token、合成或scanout。无occlusion事件/原因证据；本次状态0不解释历史08760878根因，也不证明问题已修复。
清理：callbacks2/control1/graphics1/decoded1/payload96，callbackErrors0/budget0/lost三项0，STOP完成、ProcessTrace0/joined、CloseTrace0，临时SystemProfile恢复true/error0、CloseToken true，仅自有session，无其他session操作。

### 最终验证、审阅边界及下一步
最后源码后默认normal Release build成功：99 warnings/0 errors，2.25秒；默认DLL公开类型查询HpGraphicsTrace为0，HP诊断未泄漏默认构建。未关闭分析器。HT发布1/task1/生产修复0/full0；HR旧task仍是历史1失败，不拿HT覆盖。
实际审读：旧TRX46/监督39/stdout23/HI3/control32；新TRX44/监督39/stdout12/stderr空/control34（第29行单独完整重读）/HI27（尾26-27补读）/窗口2行全部读取。Diagnostic原件回收未全文审读；本结论不依赖其未读部分。报告和handoff/session-summary/issues仅更新自有HT节，保留HS/HU及历史内容；下方“进行中”是原始HR记录，task0仅当时状态，不是当前结论。
不追加ETW或失败正锚。桥接虽在Blit层成立，历史occlusion因果/scanout仍开放；若后续需要检验观察者耦合，唯一HO备选为同正式exe有/无duplication隔离对照及独立GDI两内容标尺，**未运行**，不得宣称已跑。Available/HWloss/媒体资源通知/x64Event0/最终Tests与ComAcceptance×x86/x64 Release四格均保持开放，完整替换未完成。


## 进行中
仅本轮 HR：先复核规则、完整方案、HQ/HP、HM、HN 和控制代码，再在 HP_GRAPHICS_TRACE 隔离诊断中补真实 TDH 字段读取及独立采集 deadline，最多一次固定 HM Ex owner count2/interval1 首红。目标 bridge 必须有真实字段，时间邻近不作归属。无桥接即停止 ETW 扩张，转 HO 具体备选。

限制：不改生产、共享捕获、项目打包、NativeRun/ExitDiagnostics；不运行 full/consumer/Event0/COM 变体，不增加工程、Host 或脚本。保留并行 HS 节。当前发布调用2次均成功，尚未上传运行；生产修复0/task0。新增HP条件内HR拓扑partial、复用HI固定Ex owner首红；正常分析器，独有obj/bin/publish/package和显式PublishDir，自包含双framework10.0.13，HA SHA8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0仅包依赖未消费。预检HR方法存在；正式运行前继续核字段布局和独立STOP，不能把构建当运行。
