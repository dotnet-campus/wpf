# EN：最小 COM 调用生产修复与真实验收

## EO 最终交接补正（最新优先）
[EO返回区间审计](replacement-supervised-interop-return-path-review.md)已亲核本报告task的监督39/业务6/DT19/stdout2/stderr1/actual identity及精确缺失child.trx。第二QI负Hr/null，成功Ex释放分支不执行；after缺失不能证明函数没返，尚含QI日志尾声、OS判断、返回/caller及外层日志内部。无异常地址/栈，不造根因。当前三处撤回且默认generated，EO无新生产/测试/条件行为或任务，旧trace/EF/CF/CX/CY保留。EO默认正常Release增量build Exit0、0警告/0错误、0.94秒，非AOT验收。剩余generated IL/calli表明确缺项，不能称全认证或全Silk库bug；EL单DLL因果不外推三处足够。EO已接续完成最终五文档全文分段回读至末：EO59/EN28/handoff463/session-summary568/issues585行，长截断已缩小补读；完成后补记再读取。以下EN历史待办由此闭环覆盖，非生产验收。

## EN-3 撤回完成与最终裁决（最新优先）
三处唯一匹配逆向替换3成功/0失败，恢复原generated；无新默认行为修复、无新helper/公共抽象。未修改DR/DS或其预期、NativeRun/ExitDiagnostics、捕获、WindowQ、项目/版本；EF char*、CF/CX/CY及原trace保留。专属Build.props仅ignored artifacts隔离设置，不是产品项目修改。

撤回后默认Release方案构建1次Exit0，1482警告/0错误，14.13秒；正常分析器未压制。stdout1213693字符仅尾20000可显示，不称完整原始日志；默认增量非AOT验收。x86正式候选新任务1/失败1、完成Create0；x64任务0；官方新任务0；额外诊断组0；BW1/CK10/生命周期回归未运行（x86前门已失败），不宣称合同已验收。发布调用4：native2成功，DR1失败后1成功；均只x86。正式失败包仍保留，不能绑定为最终默认源码产物。

精确读取child.trx返回不存在，stdout仅MSTest启动及诊断路径2行，无完成计数；父监督不是MTP，不造父TRX。actual aot-identity.json已亲读，SHA与业务identity一致。没有把名称搜索无命中当缺失。Available通知、HW消费者、device-loss/resize恢复未覆盖；x64 Event0/EHANDLE独立，不合并根因。

结论：所要求最小三处Stdcall组合不足以恢复当前NativeAOT x86调用链；本轮精确撤回，不交付未验收补丁。EL同DLL单调用因果仍有效，但不证明仅三处足够或整个Silk库bug。下一最小行动应先核剩余生成调用在此链的精确calli及现有失败边界，不泛化全direct，不跳能力检测/cleanup；需要新的有据单组诊断设计。本轮未取得故障指令/首坏写地址，未跑额外诊断。

阅读披露：执行规则全文已读；完整方案曾超长截断，尾90–120补读但中段不冒称逐字完整；EJ/EF/EL长段有截断，当前Interop仅相关上下文已核，未全文627行。最终四文档全文回读尚待完成，不把这项纪律要求标为已做。

## EN-1 进行中：规则与输入核查（历史）
已实际读取执行规则全文及 EK/EL/EM 最新结论；正在补读完整方案、EJ/EF 与当前生产实现。尚未修改源码、发布或运行新任务。仅计划改 GetCreationParameters slot9 与 CheckDevice/GetUpdateMethod 的 device QI slot0 为显式 Stdcall，保留其余调用和输出合同。正常分析器；先正式 x86 DR，成功后才扩 x64。未过完整门则精确撤回本轮行为，保留既有修复及 trace。实时证据按实际阶段追加，不把构建当运行。

## EN-2 最小组合真实失败（即时记录）
三处表达式改显式Stdcall，未改其它调用、检测、引用及trace输出。正常分析器 x86 NativeAOT 与 DR 自包含10.0.13发布成功；第一次DR发布因预期native路径不存在MSB3030，随后显式专属PublishDir纠正，未上传错误包。无分析器压制。

新任务aaf1acf656f9493a852c30f85ec90f30，BL正确三参数，子TestResults绝对任务路径；父8712/子5652，Exit -1073741819/0xC0000005，未timeout/cancel/kill，SupervisorError=null。.NET10.0.13/X86/4。actual candidateSHA BF98CB728DEF274674769CC84B36F21099A186DDAEB990F1BC1EC1479DC775C9；testSHA EAB5BEFA56FD62BEC368685F4BFFF64FFA824181C683DF26DD7184809B7ED712；MVID c43b0534-64a2-423d-a541-3d37c4f926a9。Requested=Actual=任务Native/wpfgfx_cor3.dll，版本1.0.0.0/WPF10.0.0.0。

监督39行、业务6行、DT19行、stderr1行全文亲读。CreateDevice/RenderTarget/ColorFill Hr0，首次Create无after。两device QI E_NOINTERFACE/null，TestCooperativeLevel、GetCreationParameters、Caps Hr0。最早可观测未返回边界：GetUpdateMethod.QI.after至GetUpdateMethod.after之间，非故障指令或首坏地址。旧generated trace标签按要求保留，不表示实际generated QI。stderr Access Violation，无本次NRE栈。

x86第一门失败，不扩x64、不full、不追加无据诊断（额外组0）。即将撤回三处表达式；保留原始失败包和证据 artifacts/en-interop-com-fix/run-candidate-x86。不能称生产修复成功。

