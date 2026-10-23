# EJ：EI creation-parameter 最小对照独立核验与安全收口

## 执行范围与源码最终状态

执行子智能体直接完成，主代理仅监督。先将handoff标为EJ进行中并链接[EI](replacement-supervised-interop-repro-progress.md)，历史保留；已读执行规则与完整120行实施方案。EJ不追加实验：新task/沙箱执行/发布/full均0，仅默认Release构建1次。

完整亲读当前Interop实验源711行后，先确认snapshot目录不存在，保存至`artifacts/ei-interop-repro/snapshot/InteropDeviceBitmapExports.cs`。为完整文本快照（换行归一化），不声称字节相同或提供未测SHA。随后19处唯一匹配替换全部成功/0失败，未整文件覆盖生产源码：恢复各原#else generated调用，撤EI_INTEROP_GROUP/EI_GENERATED_CREATION诊断及EiRelease辅助。Code目录检索上述行为标记、EiRelease、EI.direct/EI.generated零匹配；改动区再次亲读确认原GetDesc/GetDevice/GetCreationParameters、两device QI、Caps/Ex/GetDirect3D/LUID及Release路径。DT/DX默认关闭只读trace、Ex/HRESULT/null/refs原逻辑保留。EF DR char*与CF/CX/CY fill未触碰，未改其它生产、测试、项目或配置；本轮无新生产行为修复。

默认正常分析器Release方案build Exit0，1482警告/0错误，30.93秒，无NoWarn或关闭规则。日志记录`artifacts/ei-interop-repro/ej-closeout-build.log`；工具可读62行，原stdout1213693字符仅尾20000可读，不能称完整原始日志。普通增量build非clean/rebuild、NativeAOT发布或两架构运行，不能绑定旧实验SHA为当前默认运行通过。

## 独立亲核EI原件（非新增运行）

路径均相对项目根`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape`，公共证据根`artifacts/ei-interop-repro/`。

control：taskb7691bc80334453a9e6226027f000bb2，UTC03:44:31.3615329—03:44:32.4197659（2026-10-22），父2980/子5372。亲读`run-control/Results/ei-control-x86/supervision.json`39行、`dr-candidate.jsonl`39行、stdout12行、空stderr、`TestResults/child.trx`26行与`run-control/Results/aot-identity.json`全文。filtered/30秒、无timeout/cancel/kill，OutputCollected=true、SupervisorError=null、Exit0。三次Create Hr0、12×3、IUnknown同identity、AddRef2/Release1、GC dirty0、Detach、bitmap最终refs0；surface/device/factory最终refs0。TRX total1/executed1/passed1/failed0，为监督子测试TRX，不是父TRX。

control实际native SHA `A9D1201BF9901FBFA557649B88CD99E27E3083D7EAF8906784A58AE4151B85C8`；test SHA `C2A12EBC97815B542C0F0D2C03AB0F134B9196D2ED49266B934D819C4E2FAF1B`；MVID `afc74971-3bc6-44fa-a262-1d5dc803a0bd`。Requested=Actual=`C:\CodingAgentSandbox\Tasks\b7691bc80334453a9e6226027f000bb2\Native\wpfgfx_cor3.dll`。

单恢复：task4236dabcf8664380b9f4937032a0b4c8，UTC03:46:02.0198959—03:46:06.97284，同日，父10296/子3032。亲读`run-creation/Results/ei-creation-x86/supervision.json`39行、`dr-candidate.jsonl`6行、stderr2行、stdout2行、`run-creation/Results/dt-interop-create/stages.jsonl`22行及aot-identity全文。filtered/30秒、无timeout/cancel/kill，OutputCollected=true、SupervisorError=null，Exit-1073740791/0xC0000409。stderr明确未处理System.NullReferenceException，候选InteropDeviceBitmapExports.Create +0x346；不是监督设施异常，不凭退出码断ABI。首次Create仅before，无完成TRX，不编造失败0或父TRX计数。

单恢复实际native SHA `0E48BA986291577C2AA0728A6094DB8A7F2A9AFB7BE4B179D6F148D2F031EEA5`；test SHA `DBA22222AFEA84D930C69EA1A20186E8BD9AA844160F774C2E314BB8B8193123`；MVID `d2b49352-d54e-4b28-a33d-7659a5b883bb`。Requested=Actual=`C:\CodingAgentSandbox\Tasks\4236dabcf8664380b9f4937032a0b4c8\Native\wpfgfx_cor3.dll`。

两格均X86/指针4/.NET10.0.13、WPF PresentationCore10.0.0.0；native FileVersion1.0.0.0/ProductVersion1.0.0+2e2a3c7e64d74642cb46f64247b40e73b4431299。业务identity与aot-identity、监督一致；官方新对照不适用。trace中的CreationParametersSize16（不要混淆测试identity ParametersSize56），SurfaceDesc32/Caps304。

## 最小对照因果边界

已亲读单一实验源条件结构：EI_INTEROP_GROUP下，两包只有Create内GetCreationParameters受EI_GENERATED_CREATION切换direct/generated；其余三早期调用及两device QI、后续和Release保持相同direct诊断框架。DR源本轮未改；此为源码条件差异核验，不是两个NativeAOT产物字节相同证明。

失败trace明确EI.generated.GetCreationParameters后after Hr0，GetDeviceCaps Hr0、第二QI E_NOINTERFACE/null且GetUpdateMethod.after已返回。不能将最后before当崩溃位置，也不能用after排除前序栈/内存破坏。最小对照只支持该单调用或其编译代码影响与成败关联：测试程序集SHA/MVID实际不同，native重新编译及代码布局影响不能排除。不是唯一默认根因证明，不是首次错误写地址定位，更不是Silk全库bug。

项目级`artifacts/eg-remaining-com`现存control/early/late及两run目录，撤销EH“原件缺失”推断。EG监督/业务/TRX/stderr本轮未亲核，仅引用EI-2的恢复裁决（control三轮refs0/Exit0，early Create+0x3b2 NRE/C0000409）；不能转写为EJ亲核。

## 下一最小执行点与未闭门

下一仅认证GetCreationParameters实际绑定的IL/calli，与SDK x86 Stdcall slot9逐项对照；核输出结构16字节、AdapterOrdinal/DeviceType/hFocusWindow/BehaviorFlags字段大小偏移以及合法输出地址/写入边界。EJ未实施该认证或修复。确认根因后才做单点修复，保留前必须正常分析器双架构DR、生命周期及BW/CK验收，不泛替全部COM。

Available/Callbacks0不是通知验证：NativeInvocationCovered=false、ConsumerCovered=false；x64 Event0线独立。full、GPU真实消费、loss/resize、media、resources、通知及Available真实callback仍未闭。EJ安全撤回与交接文档完成不等于完整替换完成。
