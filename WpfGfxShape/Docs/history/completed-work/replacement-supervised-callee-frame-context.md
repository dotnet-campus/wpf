# GJ 四阶段 callee 帧上下文取证（GK续证）

## GK最终结论
控制设施错误已修复并正常验证；唯一候选四阶段全部命中，首次动态确定callee固定保存槽从正确P变0，ret不变，随后pop恢复EBP=0。仅确定写坏区间，不确定首次写入指令/内部call/写入线程；生产修复0，产品仍AV。下文GJ终态为历史，不再代表最新状态。

## 终态：控制在 Arm 前失败，候选未执行
四阶段动态取证未完成。仅修改 FhCrashEvidence.cs 的 FH_CRASH_EVIDENCE 隔离代码；实现有序阶段1351EB→135573→13569E→1351F0、slot0复用/最后启slot1、阶段2固定实际F及后续固定[F,F+8)读取，并增加候选入口/尾部完整连续原字节文件/映射认证。正式发布成功；唯一正常控制在指令认证门失败，未Arm。遵守控制不过禁止候选及禁止控制重试，候选运行0，四阶段命中0；P/S/F/保存槽/ret本轮无观测，不预填GH数值。生产修复0，完整替换未完成。

## 接手历史
先创建报告与handoff GJ进行中，再读取执行规则、方案、GA GH/GI与FW FZ关键节、FH关键代码。原始状态是尚未修改/构建/运行，预算未消费；现以本终态覆盖状态但不删除共享历史。方案首次长读截断，后续90–120已读；未声称方案/GA历史/FH全文亲读。没有新增Host/第三工程、终端、脚本、INT3改写、机器码生成/注入、系统代码修改。

## 实现与局限
HardwareSample 使用有序points及Phase；只匹配当前启用地址、同TID、firstchance、EIP/ExceptionAddress、DR6唯一位和DR7。中间slot0地址替换并读回，最后清slot0地址、启slot1；禁用旧地址不会被识别为当前点。Write沿用仅写DR组及完整读回门。初始P/S固定，阶段2frame=实际EBP固定；阶段2以后读取固定frame8字节与当前ESP8字节，不随损坏EBP改固定地址；Read保留错误/实际有效长度/Hex，不把失败当零。
记录F==S-8、ESP==F、EBP==F；槽内容及ret保留原始Hex，尚无自动数值解码认证。FullStackProven=false，配对非全栈证明。入口完整558BEC57565383EC44与尾部8BC68BD78D65F45B5E5F5DC3加入实际认证；原实际文件句柄SHA/前后身份、PE稳定字段、CALL/return、DR合法Create前PID/TID门保留。仅编译验证，新增成功/异常转换未动态验证。
控制和候选入口先执行离线preflight，核FH Run/CandidateReady、HardwareSample Arm/Hit、准确DR TestMethod与aot.sha256。默认入口不变，原每事件一次Continue、非自有异常NOT_HANDLED、真实AV记录及clear/terminate/wait安全路径不改。

## 发布与隔离
ignored artifacts/gj-callee-frame/Build.props、Normal.Build.props仅配置，非工程/脚本。独有obj/bin/publish/com-acceptance-package，显式绝对PublishDir，win-x86 Release selfcontained Core/Desktop10.0.13，FH+DR；正常分析器，无NoWarn/关闭规则。诊断发布2次，首次后增加自动preflight再最终发布；只运行最终包。沿用白名单PackageSelfContainedComAcceptance，原候选复用gg-handle-runtime/com-acceptance-package/Native，不重建生产。上传clean package与control回收目录分离，不清历史父目录。
package aot.sha256全1行、runtimeconfig全21行已亲读，SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53，Core/Desktop10.0.13。最后源码后normal无FH/DR独有目录正式发布Exit0，日志1–6/58–61亲读；既有MSTEST警告保留。这是默认隔离发布验证，非默认运行/产品通过。

## 唯一正常控制真实身份与失败
实际task eb989a5547334b668c82603f6c22e1dc，监督8616/子9436/ready TID2284；整个任务回收至artifacts/gj-callee-frame/control。actual test SHA E44D8F33C34460A207CEEAE3BA874288C48FB5D6665BD2F54488D2EEC237549C，MVID b37e8106-9ae0-4298-b609-6be263f28144，.NET10.0.13/X86/PointerSize4。preflight全1行Verified=true/CandidateLoaded=false，候选仅磁盘核SHA，未加载/Create。
系统C:\Windows\SysWOW64\kernel32.dll，SHA BAFD9E061964642802D62F1EB694E7E4AF60DE739A8DF1A492F90006DC5680F3，base1987641344，GetCurrentProcessId=1987771888/RVA130544；canonical路径和local/remote RVA一致、系统目录门通过。hardware全4行亲读：预设64A1180000008B4020C3，实际64A120000000C3CCCCCC。实际为MOV EAX,FS:[20h];RET，不是预设两MOV再RET。
根因是本轮控制设计错误：未经该系统版本实际连续指令证据就预设实现。严格字节门Arm前抛InvalidOperationException: Control instructions differ，工具响应FH/NativeRun栈亲读。实际两个指令边界不足以在单次调用证明slot0复用再启slot1，不能临时猜任意RVA。没有绕门或追加控制。
Arm0/阶段命中0/转换0/清DR读回0，因为从未拥有设置DR，不伪称已验证清DR。无正常子Exit0，无EXIT_PROCESS数值或supervisor-result；不能把Terminate参数1、Wait0或工具异常猜成实际子/监督数字Exit。监督异常非取证成功0，无TRX/测试计数。

## 清理与原件阅读
native-events共248行仅亲读1–3及235–248关键门，不称全文。末次ready ReadSucceeded=true/BytesRead51/PID9436/TID2284，Kind8恰一次Continue65538成功/Error0。cleanup.txt全13行：TerminateProcess=True/Error0，8次CleanupContinue=True/Error0，Detach=True/Error0/ExitEvent=False，WaitForExit=0/Error0，0x374/0x37C各Close成功/Error0。证明终止后等待及自有句柄关闭，不证明正常业务finally或DR清零路径成功。hardware/preflight/cleanup全文已亲读，实际无candidate fileidentity/context/四阶段原件。
预算实际控制1 task，候选0；无加载控制/full/COM变体/额外控制。没有用旧GC门代新增转换控制。

## 未测与下一唯一动作
新增slot复用、slot1后启、最终清DR成功路径未动态验证。候选四阶段、固定槽值/ret变化、真实AV、入口矛盾核查、卸载/timeout/Get/Set/readback失败等本轮均未测试。当前代码仅编译验证，不称已定位首坏写。
下一唯一动作：另授权修正正常转换控制，先只读认证现有正式合法native函数的完整连续真实指令与至少3个可达可靠边界，再只执行一次slot0复用→slot1后启→清DR/子Exit0真实控制。通过前严禁候选，通过后才按另授权预算一次同SHA四阶段采样。本轮不重试。
GH/GI端点EBP归零历史仍成立，但保存槽仍仅条件推导；首坏写/具体内部call/ABI归因未定，不归咎整个Silk。Available/HW消费恢复/full四格/x64Event0独立线仍开放。生产COM/DRDS业务断言/ExitDiagnostics/VisualCache/historical/捕获/打包项目/CF-CX-CY均未改，生产修复0，完整替换未完成。

## 写后覆盖
本报告全文分段回读，三共享仅最新受改节及末次补记回读；历史中段未全文且未改，不冒称全历史核证。核心四阶段原件不存在，不以已跑未读结束；本轮实际失败控制原件已按上述范围亲读。文档不是新动态证据。

## GK 进行中：控制设施已修正且唯一正常控制过门
亲读GJ hardware/preflight/cleanup原件，确认错误预设，不是硬件不支持或产品缺陷。实际64A120000000C3只含MOV EAX,FS:[20h]与RET，偏移0/6；旧“至少3边界”要求撤销。仅FH修改Arm显式repeatedControl分支，points=[target,target,target,target+6]，受控纯C#函数指针三次调用，逐调用记录实际PID/TID。候选points/固定F/字节/线程/PE身份门不变；无写入或执行新代码字节。
正式独有gk-frame-context FH+DR包发布成功，normal analyzers，无NoWarn；候选从gg原包复制，未重建。正常控制唯一task a6f705f78b4849a583495161f357716e，监督7180/子11260/TID7796；testSHA5D031BA34D6457D7E7C200A5308B948D7768584937937EF89F5B58597B8408D1/MVIDab2e7482-fece-43ce-bb6f-9e5c9e7144fc，.NET10.0.13/X86。preflight真实Verified=true/未加载候选。映射实际字节64A120000000C3，kernel32 SysWOW64/SHA BAFD9E061964642802D62F1EB694E7E4AF60DE739A8DF1A492F90006DC5680F3/RVA130544，路径/canonical/local-remote RVA已核。
同TID三次入口EIP1987771888，DR6=4294905841/DR7=1；两次同址slot0转换清DR6读回成功，第三次后DR0=0/DR1=1987771894/DR7=4读回成功；末次EIP1987771894/DR6=4294905842，最终DR0–3/6/7均0读回，Phase5/Paired=true。control-calls三次ActualPid=ExpectedPid11260/TID7796。EXIT_PROCESS实际0/secondchance0/监督0/CleanupConfirmed=true，cleanup全4行Detach/Wait0/两Close成功。控制证明断点转换机制，不证明候选ABI。
即将执行同最终包唯一candidate；尚无GK候选四阶段事实，不预填。预算control1已用/candidate0。控制hardware38行按小段关键原值已读，两个长内存行首次截断不冒称全文；native-events仅identity/终态等关键范围。原件artifacts/gk-frame-context/control。

## GK唯一候选实际结果：固定保存槽覆盖已确定
实际task448644f7c5b74a67870c0aed5619d3f6，监督11096/子6476/TID6076，base6D390000（1832452096），testSHA/MVID同上述最终控制包，候选SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53未重建。上传仅clean com-acceptance-package，源与control/candidate回收分离。preflight全1行确认FH/HardwareSample/准确DR TestMethod PE元数据、aot.sha256及未加载候选；runtimeconfig全21行Core/Desktop10.0.13。实际LOAD_DLL复制句柄1280，DISK1，前后Volume2759389505/Index3866624:296295/Size0:3995648/Write31285785:3967683863一致；同步3995648/指针0恢复0/句柄SHA匹配。稳定PE及文件/映射CALL、序言、尾声、return严格认证通过，不放宽ImageBase等门。

|阶段/RVA|实际EIP|当前EBP|当前ESP|固定F槽8字节|
|---|---:|---:|---:|---|
|1 CALL1351EB|1833718251|P=51888484|S=51888332|尚未建立F，残留0C00000003000000不算savedP|
|2 序言后135573|1833719155|51888324|51888324|64C11703F0514C6D|
|3 pop EBP前13569E|1833719454|51888324|51888324|00000000F0514C6D|
|4 return1351F0|1833718256|0|51888332|00000000F0514C6D|

阶段2实际F=51888324=S-8；固定F不随阶段4坏EBP0改变。小端64C11703=0x0317C164=51888484=P；F+4的F0514C6D=0x6D4C51F0=真实return，阶段2/3/4保持。阶段2→3固定[F]从正确P到0，而EBP/ESP均仍F；第三阶段实读5DC3，第四阶段ESP恢复S/EBP0。这证明callee建立后至pop前固定保存槽已被覆盖，解释随后的EBP归零；不是入口未保存、不是把坏EBP0当固定槽，也不再只是GH残留推导。仍不能定位首次写入、具体内部call、写入线程或完整栈平衡，不归咎整个Silk。
四阶段同TID6076/firstchance SINGLE_STEP；前三DR6=4294905841/DR7=1，末次DR6=4294905842/DR7=4。slot0依次1833718251→1833719155→1833719454，每次Set/清DR6/完整读回Verified=true；最后DR0=0/DR1=1833718256/DR7=4，末次全DR0–3/6/7=0读回/Phase5/Paired=true。四次Continue65538均成功。硬件原件36行全文小段亲读。
每阶段主ESP256、EBP16、EIP16、初始P16、S-32窗口48、caller固定192均保存原始Hex/Requested/BytesRead/Error；阶段2以后另固定F8及当前ESP8。除阶段4当前EBP0读取16失败/Error299/BytesRead0/空Hex外，所有上述阶段读取均成功/有效长度等于请求/Error0。初始[P]16四次E8C117031C10B70C004B8A0B00000000不变。第四阶段当前ESP8为387B020F08C11703，不误当固定F槽。原始窗口均在hardware中，不用失败零填槽值。
清DR后真实AV在1351F0读取FFFFFF68，随后1DE4D1 first/secondchance读取FFFFFF7C；Continue2147549185=NOT_HANDLED均成功。两个context-av各122行全文亲读，首个EBP0/EIP1833718256/ESP51888332，后者EIP1834411217/ESP51887060/EBP194884032，失败内存读取保留299/0/空Hex。实际EXIT_PROCESS子0xC0000005/SecondChance1/未超时，监督取证Exit0/CleanupConfirmed=true，不是产品通过。cleanup全4行Detach=True/ExitEvent=True/Wait0/两Close成功；无Terminate，无正常业务finally完成证据。DR原件只到Create before，未channel/Present；stages/dr-candidate各6行全文已核，无TRX或虚构测试计数/工具数字exit。

## GK收口：隔离、范围与下一动作
最后源码后normal无FH/DR独有normal obj/bin/publish/package正式x86 selfcontained10.0.13发布Exit0，日志1–6/56–60已核，正常分析器既有MSTEST警告保留，无NoWarn/关闭规则。默认入口仍条件隔离，本轮未运行normal，不移绑默认候选。仅FH源一文件修改，ignored新增Build.props/Normal.Build.props与运行证据；未改生产COM、DR业务预期、CF/CX/CY、ExitDiagnostics、VisualCache、historical、捕获或打包项目。无Host/第三工程/脚本/机器码注入/INT3/外部反汇编。
预算正常control1+candidate1已全部消费；无只读采集额外请求，无full/COM变体/旧加载控制/重试。任务已实际执行且原件亲读后收口，不是初稿。异常clear、timeout、Get/Set失败、readback失败等分支未动态测试，仍是诊断设施风险，不假称均已验证。
下一唯一根因动作：只读核同SHA callee135573至13569E区间的现有原字节、内部调用与独立ABI/输出指针范围，查找为何输出或清零可能别名固定savedP槽；先静态缩小覆盖路径，不追加候选、不直接生产修复。首次写入指令/具体call尚未确定。
Available/HW消费恢复/full四格/x64Event0仍开放，生产修复0，完整替换未完成。
阅读边界：GJ报告原34行已全文亲读；GK候选hardware36/fileidentity6/cleanup4/preflight1/dr6/stages6及两AV context各122行全文亲读；native-events757行仅identity/阶段/异常/终态关键范围，未冒称全文；FH只读相关实现关键范围，GA旧历史未全文。本报告及三共享最新修改节写后回读，历史未改且未全文。
