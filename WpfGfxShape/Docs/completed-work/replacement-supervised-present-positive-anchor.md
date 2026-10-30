# HM 精确成功合同正锚与基础D3D9对照

## 进行中：先核历史原件与完整合同
初始登记正式task=0，发布=0，尚未执行测试。仅允许HM_NATIVE_POSITIVE_ANCHOR显式诊断；默认行为及生产、公共捕获、旧consumer、项目打包保持不变。
先核HJ task80257012a4c640019c6eaf83b1b34024 DISCARD成功原件与HI源码完整合同，不将count2/interval1两个字段冒充全部合同。一个正式task先Ex owner正锚红蓝红及FF123456，要求实际Present S_OK、非空正确内容及lastPresent晚于submission的fresh；正锚任何失败即停止，基础9对照记未执行。只有正锚通过才执行同共同合同的基础Direct3DCreate9 owner，独立同style/坐标/生命周期窗口。禁止FLIPEX/consumer/connect/Event0/full/随机换参补跑。
HL taskc6f6968310184cdb985728cab0b7cbac count1/interval0三控制均无fresh，仅为历史失败对照，不支持归Ex或线程。历史HJ成功保留；本轮未取得实证前不作新结论。

## 离线合同与发布核证（task前）
HJ原件精确路径 artifacts/hj-present-control/readback/x64-native/Results/hi-delivery.jsonl 1–17亲读：Ex成功创建0/count2/format22/interval1/behavior38，红帧真实Present0/state0/0/内容正确；其余成功帧与QPC见HI报告原表。窗口原件及duplication输出仍需分段核证，不用缺索引冒原件缺失。HI源码同一方法复用，无公共设施修改。
合同：SDK32/adapter0/HAL1，测试线程创建factory/device，0x26，32×16/X8R8G8B8=22/DISCARD1/count2/interval1，MS/Q/AutoDepth/DepthFormat/Flags/Refresh=0，windowed1/fullscreen NULL。原CreateOutputWindow同title HI native delivery、40,40、32×16、style90000000/ex8/owner消息泵，建立后实际94000000；独立窗口生命周期。Clear/同格式SYSTEMMEM staging读回均owner，随后记录submission，再PresentEx slot121/NULL source-destination/目标HWND override/dirty NULL/flags0；event query flush及DwmFlush；window/screen/front；每帧独立DXGI先初始化并drain初帧再submission回调，等待真实fresh。顺序与HJ保留。
对HL真实差异：count1→2、interval0→1，去掉基础9测试线程组，以Ex owner先作正锚，再基础9 owner；其余共同合同不变。HJ原三色加FF123456是旧前置内容检查，不是调参追绿；先失败即停止。Ex与基础9的factory16/20、Present17/121、状态/LUID/chainEx可用性属API不可比差异，基础9保留Applicable=false与未执行哨兵。
正式发布1成功，正常analyzers，无NoWarn/关闭规则。独有hm-positive-anchor/{obj,bin,publish,com-acceptance-package}，显式PublishDir，selfcontained win-x64/.NET双framework10.0.13。clean白名单419行末与runtimeconfig/aot.sha256亲核；Native依赖固定HA x64 SHA8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0，不重建不冒consumer。离线公开API核HM精确方法存在、无HI/HL诊断方法，防零测试。正式task上限1，filtered120/外180，源package与readback分离，整task回收。

## 实际停止阶段：唯一task正锚首红失败，不运行基础对照
Task395b5f3c45ae43d8881e3386118788e7，发布1/task1，监督6524/child6364，UTC11:32:19.0837671–11:32:27.3010073。子Exit2/00000002/exited，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，HasExited/OutputCollected=true，SupervisorError=null，stderr空。外工具只报告执行完成/回收路径，未提供独立数值Exit；监督非MTP无parentTRX。
绝对TRX C:\CodingAgentSandbox\Tasks\395b5f3c45ae43d8881e3386118788e7\Results\hm-x64-native\TestResults\child.trx 已按本地精确映射1–65全文亲读：total1/executed1/passed0/failed1/notExecuted0/timeout0/aborted0，ResultSummary Failed，RunInfo Exit2。业务26行/窗口2行/监督39行全部亲读。整task回收 artifacts/hm-positive-anchor/readback/x64-native，源仅com-acceptance-package，未上传publish父/历史根。
Ex实际descriptor HR0：32×16/Format22/count2/MS0/Q0/DISCARD1/HWND14747118/windowed1/AutoDepth0/DepthFormat0/Flags0/Refresh0/interval1，与创建输入逐字段一致。behavior26/SDK32/adapter0/HAL1，测试创建7108、owner提交9500，state/LUID适用，chainEx QI0。Clear/acquire/GetDesc/staging/copy/lock/unlock全HR0，backbuffer FFFF0000；backbuffer Type1/Usage1/Pool0/MS0/Q0，staging同32×16/Format22/Pool2/Pitch128，读回refs0/0。
最早拒绝状态是首红Present前CheckDeviceState=141953144=08760878 S_PRESENT_OCCLUDED；随后实际slot121 PresentCalled=true/同值OCCLUDED（不是S_OK，不是INVALIDCALL），后态亦同值。submission35762176274836、Present返回35762176279785、GPU完成35762181412495。没有以GPU完成冒consumer consume。window/screen=00CDB38B，frontResult0/front FF8BB3CD，duplicated0为未取得内容的初始化值，非黑帧；没有lastPresent可报。独立DXGI初始Acquire=887A0027，qpc35762176078676，回调起点35762176080010，随后10次887A0027直到35762239348751，真实无fresh；不是初始鼠标帧冒fresh。count/stats均8876086C，lastCount/stats0为失败输出而非零提交。
首帧accepted=false后立即停止剩余蓝/红/FF123456，清理device/factory refs0/0，DestroyWindow1（qpc35762239605481）；明确comparison not executed（35762239658706）。基础9无创建/提交/帧，必须记未执行而不是基础API失败。未跑FLIPEX/consumer/connect/Event0/COM/full，不迁共享、不重启生产。

## 实际身份与外部显示证据边界
actual test任务根DLL SHA EA2108B46639172A09D96B50BC01C070CFE8DBE176D1A58F72F2422037404C52，MVID46cc82ba-d59e-4529-9875-4b660f023dc8，.NET10.0.13/X64。actual任务根PresentationCore SHA0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31，version10.0.13-servicing.26504.104+4099f83977f4d29604a9aeb971fb0a7bc1d5f699。Native HA包SHA8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0仅依赖，consumerEntered=false，无actual wpfgfx消费身份。
actual系统模块均C:\WINDOWS\SYSTEM32：dwmapi.dll v10.0.26100.8875 SHA434EDD020C6EB30AEE968004641FCE6B21569E53F21695250B5055C4A93904C2；d3d9.dll v10.0.26100.9278 SHA87E157E78536C66FC413ED202296E39053CC9E14DC8D8381F8A3D18F444A4C0D；dxgi.dll v10.0.26100.9444 SHAB12AEBF0F077D6C1394B50E2EECDD5E16072C9A0C871F30887B5D170CBF2EAAE；d3d11.dll v10.0.26100.9278 SHAAE2777002F5A0FF543B5B9BEC5124746559FD01D6C7C0C216D3194632F084502。
现场session1 Active0/WinSta0/Default/High SID S-1-16-12288/remoteMetric0/protocol0/DWM enabled1；窗口14747118/owner9500/测试7108，style94000000/ex8/DPI96，rect40,40–72,56/client32×16/screen56,48。建立/采样两次visible1/iconic0/cloaked0/foreground与cover均自身；Ex adapter LUID4E71/monitor10001匹配独立DXGI Microsoft Basic Render Driver vendor1414/device8C/LUID00000000:00004E71/DISPLAY1/1024×768/rotation1/attached1/monitor10001。不能据窗口表层可见推翻API明确occluded。
HJ同窗口原件4行与首帧duplication原始stdout1–24已补核：当时initial HR0/lastPresent0鼠标帧，随后fresh lastPresent35744705642302；本轮initial HR887A0027且无后继内容。初始化调用顺序未改，但外部实际初始帧就绪状态不同，是观测差异而非已证根因；禁止伪称完全相同外部显示状态。HJ其余原件未重新全文审，历史报告保留；HL报告首次长返回部分截断未全复读，不冒历史全文审。

## 裁决、默认安全与下一唯一动作
精确共同创建/Present/读回/窗口合同恢复后，Ex正锚仍在首红明确occluded且无fresh；因此本轮不能比较基础9与Ex，不能归候选产品错误，也不能把count2/interval1说成充分修复。HJ成功保留，不以HM覆盖；没有新的生产修复建议或共享迁移。
最后源码后default正常分析器Release/win-x64 build98 warnings/0 errors、2.31秒；默认产物公开API亲核无HM/HI/HL诊断方法，显式符号未泄露。仅改HI partial及本报告/三共享交接，无公共GpuReadback/窗口、生产COM/DRDS、NativeRun/ExitDiagnostics、VisualCache/historical/WindowQ/HB/打包/CF CX CY改动。未关闭规则/NoWarn/放宽阈值/golden/skip；候选未重发。
下一唯一必要动作不是再换参数跑同门：取得与该HWND/adapter LUID/monitor和submission QPC关联的**外部DWM/DxgKrnl显示源/合成可呈现状态证据**（显示源是否激活/电源或会话切换是否抑制呈现，以及目标重定向surface的occlusion原因/是否被合成），以判别“窗口表层前台但present source被显示栈判occluded”的具体来源。现有Active/foreground/uncloaked/attached快照不能证明显示源实际扫描或DWM消费，当前未提供该证据，不以泛环境不稳定代替。先获取/解释这项证据或官方CheckDeviceState/PresentEx对当前显示源occlusion判定的适用合同，再决定如何恢复旧路径；不机械再改count/interval/flags，不跳过旧前置。若上层无法提供外部显示证据，此门保持阻塞而非新增有效task。
Available/HWbackend/loss/media资源通知/full最终四格/x64Event0全部开放，完整替换未完成。

## 最后核证范围
本轮业务26行/窗口2行/监督39行/childTRX65行/stdout44行全文亲读，stderr空；大Diagnostic/log_261023113219211.diag完整回收但未全文审、不冒Event0/full。报告及三共享最新HM节写后回读；旧历史保留未覆盖，无新历史运行计数。任务计数最终1，不追加执行。
Available/HWbackend/loss/media资源通知/full最终四格/x64Event0开放，完整替换未完成。
