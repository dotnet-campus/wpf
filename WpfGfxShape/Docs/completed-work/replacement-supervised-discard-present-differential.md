# HL 同任务 DISCARD 原生 Present 差分

## 进行中：证据与合同核查
本轮只允许显式 HL_NATIVE_PRESENT_DIFFERENTIAL 专属诊断；默认行为不变。目标是在一个正式 task 内，以独立资源、相同窗口合同/格式/尺寸/颜色/读回前置，判别 D3D9 Present 与 D3D9Ex DISCARD PresentEx。最多两个主要假设，不从跨任务结果归因，不追 FLIPEX，不迁移 consumer，不修改公共捕获或生产。

初始登记时尚未修改测试、发布或运行；正式 task 计数 0。HJ/HK为历史证据，不计本轮实证。

## 设计/发布阶段：task前
H1：相同合法合同下基础9 Present与9Ex PresentEx有交付差异。H2：基础9提交线程为决定因素。三组顺序基础9测试线程、Ex owner、基础9 owner；独立新窗口与资源，同style90000000/exstyle8/坐标40,40/client32×16/Format22/DISCARD1/count1/interval0/behavior26/各自HWND override/NULL source-dest-dirty/flags0。创建均测试线程；仅提交按组改变。红蓝红FF123456，Clear→实际同格式staging读回→实际Present→event query flush与DwmFlush→window/screen/front/DXGI fresh，零阈值。
HJ为Ex factory20/Present121/state128、count2/interval1/owner提交；HK基础factory16/Present17、输入Format0实际22/count1/interval0/测试提交。共同behavior26、窗口helper与消息泵/DPI、override/dirty/完成函数；HB native基础9 Format21，HD成功不回填本轮。仅HI partial最小复用，HL专属唯一方法；基础9状态/LUID不适用，哨兵非S_OK，正HRESULT非零状态保留。非HL仍HI双effect/原三色。
正常分析器正式发布1次成功，未NoWarn/禁规则；双framework10.0.13 x64 self-contained，独有hl-present-differential obj/bin/publish/package、显式PublishDir、正常依赖。HA候选Native仅包依赖SHA8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0，不重建不消费。实际DLL公开API已核唯一精确filter FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenHlNativePresentDifferentialRunsThenApiAndOwnerBoundariesAreRecorded。已读NativeRun监督角色及filtered120合同。仅clean package上传，源/回收分离；task预算1/外180，普通像素失败不重跑。

## 实际结果阶段：唯一task已完成，停止追加
Task **c6f6968310184cdb985728cab0b7cbac**，监督PID2700/child2852，UTC11:19:55.7580061–11:21:22.3872021；filtered120/外180。Reason exited，子Exit2/00000002，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，HasExited/OutputCollected=true，SupervisorError=null，stderr空。外工具未给独立数值Exit，不造工具码；监督非MTP无父TRX。正式发布1/task1，无部署补跑、consumer/connect/Event0/full/COM变体。
绝对childTRX **C:\CodingAgentSandbox\Tasks\c6f6968310184cdb985728cab0b7cbac\Results\hl-x64-native\TestResults\child.trx**。搜索首次漏失后按本地精确映射直读成功，未重跑：1–12、221–246亲读，ResultSummary Failed，total1/executed1/passed0/failed1/notExecuted0/timeout0/aborted0，RunInfo exit2。12帧不是12test。整个task回收在 **d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\hl-present-differential\readback\x64-native**，源另为com-acceptance-package，publish/obj/bin完整保留。

### 三控制最早失败边界
全部12帧Clear/acquire/GetDesc/create staging/copy/lock/unlock均HR0，实际Format22/32×16/Type1/Usage1/Pool0/MS0/Q0；staging同尺寸格式Pool2，Pitch128，backbuffer分别FFFF0000/FF0000FF/FFFF0000/FF123456，非空、非旧色。每次staging/backbuffer Release refs0；Format22 X非alpha，RGB零阈值。
- 基础9测试提交：HWND42730674、创建/提交线程1408、owner3140；四次真实slot17 Present=0，GPU完成。随后window/screen均COLORREF00CDB38B，frontResult0/ARGB FF8BB3CD；DXGI没有属于本次submission的新内容帧，duplicated=0为未取得结果，**不是实际黑帧**。
- Ex owner提交：HWND42796210、创建1408/提交与owner3140，slot121四次实际返回141953144=08760878 **S_PRESENT_OCCLUDED**，前/后slot128亦同值。正HRESULT不是S_OK或成功交付；继续完成query仍完成，但window/screen/front为同一旧色，DXGI无fresh。最早异常为Present前明确occluded状态，内容失败在完成后可见交付层。
- 基础9 owner提交：HWND42861746、创建1408/提交与owner3140；四次真实slot17 Present=0，GPU完成；window/screen/front同旧色、DXGI无fresh。基础9未调用CheckDeviceState/GetAdapterLUID，字段int.MinValue且Applicable=false，不伪造S_OK或实际LUID0。
三窗口实际style -1811939328=94000000（establish后状态，不冒初始90000000）、exStyle8、rect40,40–72,56/client0,0,32,16/screen56,48/DPI96/monitor10001；所有建立与四次采样foreground/cover均自身，visible1/iconic0/cloaked0，Active/session1/Default。三次cleanup device/factory refs0、DestroyWindow1；前控制cleanup QPC35755011120684早于第二创建，第二cleanup35755300652114早于第三建立，第三cleanup35755590450554，资源/窗口独立生命周期。

### 逐帧submission / Present返回 / GPU完成QPC
各组帧次均红/蓝/红/原expected；window/screen/front上列共同旧色，所有fresh等待超时，未制造lastPresent。
|控制|帧|submission|Present返回|GPU完成|真实Present|
|---|---|---|---|---|---|
|9测试|红|35754732985815|35754732988822|35754736955716|0|
|9测试|蓝|35754805929963|35754806354481|35754811373840|0|
|9测试|红|35754877795876|35754878204735|35754883501811|0|
|9测试|原|35754949365656|35754949775603|35754954813487|0|
|Ex owner|红|35755021475229|35755021478869|35755026646639|08760878|
|Ex owner|蓝|35755095265017|35755095566966|35755100882882|08760878|
|Ex owner|红|35755166964658|35755167365900|35755172393977|08760878|
|Ex owner|原|35755238760988|35755239103597|35755244404705|08760878|
|9 owner|红|35755310989005|35755310994054|35755316204842|0|
|9 owner|蓝|35755385154741|35755385608203|35755390926620|0|
|9 owner|红|35755457247410|35755457689764|35755462744017|0|
|9 owner|原|35755528678116|35755529111856|35755534271027|0|
首DXGI初始帧lastPresent0/鼠标时间35754732864647，不能作fresh；后续AcquireNextFrame 887A0027等待，无成功内容样本。Ex count/stats=8876086C，基础9 chain QI=80004002，不把失败输出计数0称成功统计。适用状态SDK26100 shared/d3d9.h2017–2018已核S_PRESENT_OCCLUDED MAKE_D3DSTATUS2168；原生函数按已有合法SDK槽，不用Ex状态要求基础9。

### 实际身份、adapter/output与系统模块
actual测试任务根DLL SHA **56D19E4B31D650582D24F1D4DC7080C496758FB595BF9099577D3D9AE97147FB** /MVID **eda472f1-26c2-45a0-b837-0c5dce1aeee8**，.NET10.0.13/X64。actual PresentationCore任务根路径，SHA **0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31**，10.0.13-servicing.26504.104+4099f83977f4d29604a9aeb971fb0a7bc1d5f699。WinSta0/Default/session1/high SID S-1-16-12288/remote0/protocol0/DWM1。Ex GetAdapterLUID实际20081=4E71，adapterMonitor10001；独立DXGI Microsoft Basic Render Driver vendor1414/device8C/LUID00000000:00004E71/output DISPLAY1/1024×768/rotation1/monitor10001，基础9仅观察monitor匹配，不冒直接取到其LUID。
系统actual C:\WINDOWS\SYSTEM32：dwmapi.dll SHA434EDD020C6EB30AEE968004641FCE6B21569E53F21695250B5055C4A93904C2 v10.0.26100.8875；d3d9.dll SHA87E157E78536C66FC413ED202296E39053CC9E14DC8D8381F8A3D18F444A4C0D v10.0.26100.9278；dxgi.dll SHAB12AEBF0F077D6C1394B50E2EECDD5E16072C9A0C871F30887B5D170CBF2EAAE v10.0.26100.9444；d3d11.dll SHAAE2777002F5A0FF543B5B9BEC5124746559FD01D6C7C0C216D3194632F084502 v10.0.26100.9278。HA Native仅包依赖，未进入candidate/official consumer，不声称actual wpfgfx消费身份。

## 收口：两假设裁决及默认安全
H1本task三组均无fresh，**不支持归因Ex差异**；Ex正occluded而基础9 HR0说明返回状态不同，不证明Ex能解决交付。H2 owner-only不足以修复基础9，本task同API/同合同两线程均失败，拒绝“只搬owner即可”的充分性假设；不外推线程永不相关。HJ旧成功保留，但本次统一count1/interval0不是完整HJ count2/interval1成功配置，不能跨task认定描述符、格式或环境因果。
没有确定公共合同错误；公共捕获/GpuReadback/VisualCache/historical/生产/DRDS/NativeRun/ExitDiagnostics/WindowQ/打包/CF CX CY均未改，不提出生产补丁，不跳过旧路径，不回到失败consumer迁移循环。最后源码后新跑正常default Release/win-x64 build **98 warnings/0 errors、2.36秒**，无NoWarn/关规则，HL仅显式符号，默认无实验发现或行为变更。原断言/并行/阈值/golden/skip保留。
下一唯一可判别动作：另轮将**精确HJ成功合同count2/interval1的Ex owner**作为同task正锚，与**同count2/interval1的基础9 owner**成对；不加入新线程/格式因素，先证正锚真实S_OK且红蓝红原expected fresh，若仍occluded则停止、不进入旧consumer。只有正锚成功时基础9同合同结果才可判API家族；再与原count1/interval0对应，不能直接迁生产。此动作是补本轮缺失的成功正锚，非随机参数全排列，本轮不执行。
HA生产修复、HD/HE/HF四格成功与HJ成功反证全部保留；Available/HWbackend/loss/mediaresources通知/full四格/x64Event0仍开放，**完整替换未完成**。

## 原件亲读与范围登记
本轮核心hi-delivery.jsonl153行逐组逐帧分段亲读（截断23单行补读）、三个窗口各5行全读、supervision39行全读/stderr空；TRX先读1–12/221–246，收口前再补读13–220，现246行全文亲读；stdout独立文件仅读1–39相关开头/195–225结尾，截断处内容由TRX完整StdOut补齐，不声称独立stdout全文。largeDiagnostic日志log_261023111955887.diag仅完整回收并登记，不全文审。执行规则27行全读，完整方案120行分段补读，HD/HE/HF成功报告相关节及HB/HI/旧前置实现必要段亲读；历史未重新全审，不造历史覆盖。
