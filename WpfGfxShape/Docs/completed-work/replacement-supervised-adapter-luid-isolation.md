# ES：后置 GetAdapterLuid 区间精确审计

ES最终回读：五文档更新后已全文小段回读至末：本报告41行、ER23行、handoff490行、session-summary581行、issues598行；handoff78–90及issues52–55缩小补齐截断。此补记随后再次读取，仅文档闭环，非运行门通过。

## ES-2 审计结论：未启动实验，默认未修
生产/测试/项目源码修改0、条件辅助0、发布0、新沙箱task0、有效新实验0；x64/官方新跑/DR/生命周期/BW/CK/full均0。没有ES新候选SHA、测试MVID、退出码或TRX。没有行为/辅助可撤回，也无实验source可保存；不把ER增量记录冒充完整实验snapshot。DT/DX默认关闭trace及EF/CF/CX/CY保持；未动用户禁止区域，未用终端、脚本、新Host/工程或机器码。
默认正常分析器Release方案专用build一次Exit0、0警告/0错误、1.18秒，19行日志全文亲读。普通增量非clean/rebuild、NativeAOT或运行验收，无NoWarn/关闭规则。

### 亲读范围与ER实际身份
完整方案120行、执行规则27行、基线20行已读完。ER原20行全文两次，EP报告分段至末；当前Interop重点亲读TraceCreate、BitmapState构造/Dispose、Create全部语句/finally、GetUpdateMethod/GetAdapterLuid、CheckDevice与依赖色源入口，不称全文627行已读。
ER task760f6da25cf04064812f96c9f99c642c的监督39行/业务6行/DT22行/aot-identity1行全文亲核；精确Results/er-candidate-x86/TestResults/child.trx再次读取不存在。父6200/子7004，Exit -1073741819/0xC0000005，Reason=exited，无timeout/cancel/kill/drain timeout，SupervisorError=null；完成Create0，无最终refs或Available调用计数证据。stdout/stderr未由ES亲读，不承袭ER记录称本轮已核。
Actual=Requested=C:\CodingAgentSandbox\Tasks\760f6da25cf04064812f96c9f99c642c\Native\wpfgfx_cor3.dll；NativeSHA2B85E1DB413D9EE61E736783D218F825D30049954B95E7A54BE16484CFB512DB，两identity一致。File1.0.0.0/Product1.0.0+2e2a3c7e64d74642cb46f64247b40e73b4431299；testSHA9E627594AD169BAE209BE822C9F91CF4FDA8C690DD7FC8C2146ADB2CAFE1F2C0/MVID1b478f87-fdd6-4aab-bf8f-c22e73acf388；.NET10.0.13/X86/4，PresentationCore10.0.0.0不是完整servicing身份。监督记录子TRX为该task绝对路径；未重新认证BL入口三参数，以上只是历史任务原件核证。

### 后置GetAdapterLuid逐句审计
1. factory=null、extended=null在try前初始化，finally可按非null释放。device Ex QI失败不等于factory Ex QI失败，二者接口能力不可互推。
2. GetDirect3D.before后调用device->GetDirect3D(&factory)，负HRESULT return0；成功按COM合同取得factory引用。当前未检查成功/null，必须依赖官方成功输出合同，不可用指针看似合理替代。失败/non-null仍会被finally释放，实验不得改变这一既有cleanup。
3. GetDirect3D.after仅传factory、未传HRESULT，Result是TraceCreate默认0；只能从已越过负值return推出非负，不能称实采Hr0。完整行也不能证日志尾声已返回。
4. iid=IDirect3D9Ex.Guid；factory->QueryInterface(&iid,(void**)&extended)负HRESULT return0。COM失败输出应null、成功增加Ex引用；本地初始null不证明callee遵约。现有成功/null无额外守卫，失败/non-null仍在finally释放；不额外AddRef或试探Release。
5. DeviceCreationParameters parameters局部未显式default；第二次device->GetCreationParameters(&parameters)负HRESULT return0，仅成功后读AdapterOrdinal。它与Create前序slot9同重载但独立输出缓冲；ER仅替前序，不能由前序Hr0认证后置字段/写满。
6. Luid luid局部未显式default；extended->GetAdapterLUID(parameters.AdapterOrdinal,&luid)负HRESULT return0，非负后以ulong读8字节位模式。官方LUID的LowPart DWORD/HighPart LONG合同不是“值合理”测试。flags/window并非GetAdapterLUID参数，仅供核对创建上下文。未跳过调用/返回固定LUID。
7. return求值后仍执行finally：extended非null先Release，factory非null后Release，各释放本方法取得的一份引用。返回0可来自任一失败支，非零也必须经过两清理调用；没有分段实测，不能选factory QI/slot9/LUID/Release为唯一故障。

### Create state构造与外层finally
Create先output/width/height清零，surface/device/instance=null、handle=default。前序GetCreationParameters成功、GetUpdateMethod及多采样检查之后，parameters.AdapterOrdinal已先作为BitmapState构造参数消费：保存Adapter并创建_adapterStatusListener委托；对象初始化器再赋UpdateMethod、调用GetAdapterLuid赋AdapterLuid。因此不是后置首次消费前序AdapterOrdinal。后置slot9写的是另一份parameters。DeviceType/hFocusWindow/BehaviorFlags在这些语句均未消费，不称内容正确。
进入GetAdapterLuid时BitmapState构造已执行，GCHandle/Instance尚未分配、Register尚未执行；缺Register.before不是分配/注册首坏证据。后置返回才分配handle/instance、赋vtable/ref/state/identity、Register并写output/尺寸，最后转移surface/handle。外层只catch OutOfMemoryException；finally依次Free(instance)、若分配则Free(handle)、device Release、surface Release。未转移surface仍由finally处理；原失败路径没有state.Dispose，实验不得随意增加Dispose/Release形成双释放。正常state.Dispose移除监听并释放所持资源。

### SDK与实际Silk重载依据
本机SDK10.0.26100.0亲读shared/d3d9.h339–357、2041–2067与basetyps.h20–35：STDMETHODCALLTYPE=__stdcall。F QI slot0(F*,Guid*,void**)->int32 HRESULT；F/FX Release slot2(typed this*)->uint32 ULONG；FX GetAdapterLUID slot21(FX*,uint32,LUID*)->int32。D GetDirect3D slot6(D*,F**)->int、GetCreationParameters slot9(D*,DeviceCreationParameters*)->int，slot实际IL亲核，SDK D表本轮引用EP而非重读。d3d9types.h1493–1503亲核字段顺序AdapterOrdinal/DeviceType/hFocusWindow/BehaviorFlags。EP既有布局x86 16/0,4,8,12、x64 24/0,4,8,16，本轮非新测。
实际artifacts/ep-com-binding/run-x86-final/Results/ep-com-binding.txt亲读1–91、155–177、200–220行：F QI06000920、F Release06000925、FX Release0600094B、FX LUID06000975分别取slot0/2/2/0x15，calli约定01=Cdecl，显式typed this，返回int32/uint32/uint32/int32。LUID Raw0103080F118240090F1180D9，精确uint32,Luid*；D slot6 token06000A78 Raw0102080F1182580F0F11823C；slot9 token06000A7E所读IL取9并calli D3020011，完整signature在EP表，本轮未将未读尾行称亲核。Silk原件SHA F08ACBB76DD7C60803F8F0D4D8212C77A85D3EF67B1FB42FBB18F3C15A8586E4/MVIDdeec18f5-2a98-450b-93cd-9117248c6163。元数据差异不是NativeAOT清栈实证或全库bug证明。

### direct成功与ER差异；未强跑的具体缺证
EL/EM报告记录同DLL direct三次Create成功及refs0；ES本轮只读该报告有关段和完整后置snapshot328–375行，未重认证EL原始TRX/退出。snapshot明确EK条件还替后置GetDirect3D6、factory QI0、第二slot9、LUID21、两Release2；ER source-change-record9行亲读明确整段未改。已有成功全链不能推出ER只差factory QI，也不应重复泛全direct。
候选假设是factory四种绑定构成后置必要差异，但缺factory QI实际HRESULT/null、第二slot9输出字段、LUID实际输出、两Release返回refs和日志尾声标记。仅替factory四种仍留后置device slot6/slot9未受控：失败不足区分上游污染、输出、日志尾声或factory边界；成功仍需约束AOT布局变化。当前静态不足形成明确定位假设，本轮不耗唯一有效实验。另缺本轮独立LUID布局/字段写满实测；不把此缺证说成官方合同不安全。
下一最小验证点：ER背景中区分factory QI调用前/返回与日志FlushDispose尾声、第二slot9输出和cleanup。保留的ER二进制不能仅靠外部新增日志文件取得内部标记。若后续实施条件源，先锁定并完整保存ER背景source，严格只factory差异及低扰动分段记录，保留能力/HRESULT/null/cleanup与原DR三循环原断言；正常分析器NativeAOT x86与DR10.0.13独有obj/bin/publish/package，BL三参数/绝对TRX/actual身份逐一亲核。未经双架构DR、生命周期/BW/CK门不默认保留。

### 日志边界与未闭门
TraceCreate用Utf8JsonWriter写对象/Flush并Dispose，再StreamWriter.WriteLine/Flush、stream.Flush(true)，方法离开时writer/stream依序Dispose。完整after行可早于尾部完成；缺行不能给出原生故障指令。本轮未加JSON反射或日志辅助。
Available0不等于通知/回调验收；HW消费/恢复/full最终四格及独立x64 Event0仍开。无本轮默认修复，不宣称完整替换或全库bug。

## ES-1 接管进行中（历史）
已完整亲读 ER 报告20行及当前执行规则27行；ER默认修改已撤回，历史task760f6da25cf04064812f96c9f99c642c为C0000005，GetUpdateMethod.after/GetDirect3D.after出现但Create无after。该日志不是唯一原生指令或根因证据；运行门未过。
先核完整方案、EP实际绑定依据、当前Interop全部相关局部语句和Create构造/finally。最多一次有可判别假设的x86条件实验；若静态依据或安全执行条件不足则只交付缺证审计，不强跑。不改DR/DS、NativeRun/ExitDiagnostics、VisualCache/historical、捕获/WindowQ、项目打包及EF/CF/CX/CY。历史保留。
当前无源码行为修改、发布或新沙箱任务；Available/HW消费/恢复/full及x64 Event0仍开。
