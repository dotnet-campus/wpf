# CO：CN fill 集成接管与安全撤回

## 最终裁决
**CN本轮未核生产挂接已精确撤回；非有限集成验收通过，完整替换及最终四格仍开。** CO执行，主代理仅监督。仅专用工具，无终端、直接/间接脚本、新Host、full或desktop重跑。已读规则27行、完整方案120行、CN初稿、CM关键身份与边界、当前CN修改；不冒称完整CM/BW/CK所有原件均亲核。

## 根因与修改
CN报告称未改生产，与源码不符，已授权补正。NativeShapeReader已挂float设备阶段和SoftwareFillBezier整数细分，path局部float经group传入，尚无完整有限验收。Cn测试把separator/be诊断无条件accepted，Passed不能冒充exact，原式保存在独占产物记录。

仅三文件相关文本撤回：NativeShapeReader起点/line恢复transform.Apply，cubic恢复共享SoftwareBezierFlattener.Flatten再Apply，删除TryDevice/整数消费；SoftwareStrokeContours删除fillLocalFloat及局部float运算，CaptureSegment恢复transform.Apply，Add仍传allowNonFinite；SoftwareGroupStrokeContours仅删除path的fillLocalFloat传参。CF fillPrimitive递归/rectangle/ellipse传递保留。不整文件覆盖，未认证历史字节完全一致。

SoftwareFillBezier、GeneratedNativeStroke、SoftwareCurvedStrokeContours未改。Code搜索fillLocalFloat无匹配，SoftwareFillBezier仅声明；fillPrimitive仍在CF三文件。NativeRun/ExitDiagnostics/VisualCache/historical/捕获/WindowQ/项目打包未改，旧归档未覆盖。

CN新增测试以CO_FILL_INTEGER_EXPLORATION编译隔离，已有BW/CK/历史回归不隔离，无Ignore/skip或并行修改。删除separator/be豁免；普通fill要求count0，stroke仅旧新完整缓冲changed0，nested探索要求changed0或官方count0，不以差数相同冒充完整无回归。原断言和差异输出保留。未新增独立正式exact测试或完整B/E D/F判别断言，因此选择撤回，不宣称验收。

## 实际构建
最终撤回后生产csproj Release win-x64/win-x86均专用build成功，各1479warning/0error，分别33.43s/14.97s；ComAcceptance csproj两RID均成功，最后x86日志98warning/0error、2.48s，x64未另保存尾部计数不填造。工具未单列数值进程exit，不造Exit0。首次solution RID构建NETSDK1134，改项目级后成功。无新生产publish或沙箱业务运行，不扰归档hash。记录artifacts/co-fill-integration/withdrawal-validation.md；构建不是像素验收。

## 旧CN证据及限制
亲读default-x64-final/Results/cn-default-x64/supervision.json全38行：task0ea1ab7df0fa44d1a49d8ece2953f398，timeout120、exited/Exit0/0x00000000，双流完成，无timeout/cancel/kill/drainTimeout，SupervisorError=null。

父TRX分段读11/11/11/0、Completed，其他异常0，只BW1+CK7oracle+3舍入。长响应49–76截断后补读69–86，尚不宣称全部TestDefinitions逐行亲核。固定绝对父TRX：C:\CodingAgentSandbox\Tasks\0ea1ab7df0fa44d1a49d8ece2953f398\Results\cn-default-x64\TestResults\child.trx。测试SHA291AEF12A9690D02BDDE781D476BF3327E317A00E8AD49F0643BCCA7FE0BCCAB，MVID d5014306-2036-42b3-acbb-47324b0727d8，.NET10.0.13/X64。

CK sample0 evidence末节亲读：differences/threshold0、双方非空；实际官方模块SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，版本10,0,1326,50504/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；测试身份同监督。这是直接编译算法+官方栅格化，不是候选消费；全部7份本轮未核。

BW父Passed，但回收根递归查bw-curved-fill/identity.json无BW业务身份，仅aot-identity输入记录；不冒称BW角色模块亲核或11/11认证集成。CN baseline-x64/x86、candidate-x64目录存在，本轮未复用四角色执行，不造新旧SHA/角色TRX/comparison。

## 自审与未闭
专用搜索确认整数消费/float参数消失，NativeShapeReader全76行分段回读，CF选项保留，最终双架构生产与测试编译成功。此为CO自审，不冒称另一审查代理或全仓字节溯源。

五文档为CN、本CO及三共享入口，最新优先并保留历史。CP已补齐五文档全文回读：此前已核CN25行、本CO32行、handoff305行、summary433行及issues第1–370行，本次补读issues第371–399行并确认第399行为末尾；未发现实质矛盾。此为文档交接核对，不改变既有运行证据、验收边界及未闭结论。

原十帧逐帧0、B/E D/F有判别真实变换、负半格、BW窄rect/ellipse、必要arc/rounded/ellipse/cubic/line/group fill、历史stroke旧新完整缓冲不变、nested18官方改善或完整无回归，仍待同一新程序集old/new/官方repeat验收。本轮不留未核生产挂接；下一轮从保留CF基线重新有界接入，不因旧归档丢失停工、不整套恢复。GPU/loss/media/resource/notification及最后稳定Tests/ComAcceptance×x86/x64 Release四格仍开。
