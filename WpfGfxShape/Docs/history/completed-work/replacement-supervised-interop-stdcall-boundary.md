# ER：Create 私有 Stdcall 窄边界修复与验收

ES最终回读：本报告更新后23行全文、ES41行、handoff490行、session-summary581行、issues598行已小段回读至末，截断补齐；本补记随后再读。只认证文档读写闭环，ER运行门仍未过。

## ES 回读补记：运行门未过
ES已亲读本报告原20行全文至末（两次），并独立亲读保留任务监督39行、DT22行、业务6行及aot-identity1行；精确child.trx再次读取不存在。仅这些范围为ES亲核，不追认ER历史四文档全文回读或stdout/stderr的ES亲读。Exit C0000005、Create完成0，非修复验收。GetDirect3D.after行的Result为TraceCreate默认0，不能当实际保存的GetDirect3D HRESULT；只能由代码负值分支已越过推非负。完整JSON行不证明随后的writer/stream Dispose或Flush(true)已返回。保留历史收口记录，不替其扩大认证范围。

## ER-3 最终裁决：默认变化已精确撤回，修复未验收
最终四文档已全文分段回读至末：ER18行、handoff482行、session-summary577行、issues594行；超长截断段已缩小补读至完整末行。本补记随后再读，不把文档闭环当生产验收。
具体私有边界：S.GetDesc12/SurfaceDesc*→int、GetDevice3/Device9**→int、Release2→uint；D.QueryInterface0/Guid*,void**→int、TestCooperativeLevel3→int、GetCreationParameters9/DeviceCreationParameters*→int、GetDeviceCaps7/Caps9*→int、Release2→uint；DX.CheckDeviceState128/nint HWND→int、Release2→uint。全部显式typed this及delegate* unmanaged[Stdcall]。SDK d3d9.h432–443/1625–1643/2267–2276与EP精确绑定表用于核slot；未以猜测选slot。两device QI复用private QueryCreationDevice、两Ex释放复用private ReleaseCreationDeviceEx；仅Create给CheckDevice传creationBoundary=true，原Present默认false路径不改。上述实现已全部撤回，非当前默认代码。
十三处逆向唯一替换13成功/0失败；仅撤本轮表达式、两个private helper和CheckDevice参数，不整文件覆盖。Code搜索creationBoundary/QueryCreationDevice/ReleaseCreationDeviceEx零残留；默认generated恢复，EF char*、CF/CX/CY及已有trace保留。未修改DR/DS预期、NativeRun/ExitDiagnostics、VisualCache/historical、捕获/WindowQ、项目/打包/版本/TFM或共享默认测试行为。
撤回后正常分析器默认Release方案build Exit0，1482警告/0错误，8.31秒。普通增量非NativeAOT验收。实际本轮发布2次成功（NativeAOT x86、DR x86各1），沙箱正式候选任务1失败，完成Create0；额外诊断0、x64/官方新跑/生命周期/BW1/CK10/full均0。x86前门失败，不把未跑项记通过。未重认证历史官方模块；DR历史同版官方身份仅报告输入，不冒充本次对照。
保留产物：artifacts/er-stdcall-boundary/Build.props、obj/bin/publish、清洁com-acceptance-package、run-candidate-x86和source-change-record.md增量快照。清洁包通过既有白名单生成，上传仅包目录；结果单独回收，未上传PublishDir父目录或回收根。未保存完整源码快照，增量精确记录及13处逆向工具结果用于最小撤回审核。
已亲核aot-identity.json与业务actual SHA相同；候选FileVersion1.0.0.0/Product1.0.0+2e2a3c7e64d74642cb46f64247b40e73b4431299，Requested=Actual任务Native路径。没有最终surface/device/factory释放证据，不声称正常refs。Available native调用和consumer仍未覆盖，不由未返回快照称回调次数0。fill既有验收保持；x64 Event0另线，HW消费/恢复/最终四格仍开。
下一最小任务：另授权仅对保留同SHA候选的GetAdapterLuid内部返回区间作分阶段定位，先区分factory QI调用/日志尾声、输出、GetCreationParameters及cleanup，不改后置调用或泛全direct，不重跑full。当前事实只支持“十重载窄边界仍不足，观测推进到后置区间”，不证明唯一方法因果或全Silk库错误。EL单点因果仍限其控制背景，EP元数据不是清栈实证。默认生产未修复。

## ER-2 x86 正式门失败，准备精确撤回（历史）
13处唯一匹配替换全部成功；十种重载按SDK/EP slot改显式Stdcall，仅Create调CheckDevice时启用，Present原generated路径保持；后置GetAdapterLuid不动。正常分析器NativeAOT x86与正式DR自包含10.0.13各发布一次成功，没有压制警告。Native stdout607332字符，仅末20000可见，不称完整警告明细。
正式task760f6da25cf04064812f96c9f99c642c，父6200/子7004，Exit -1073741819/C0000005；无timeout/cancel/kill/drain timeout，SupervisorError=null。BL三参数正确，child结果目录为任务绝对路径。业务6行、DT22行、监督39行、stdout2/stderr1已全文亲读；精确child.trx读取不存在。新完成Create0，不造引用释放或TRX通过计数。
新边界：GetUpdateMethod.after已出现（Software=0），随后GetDirect3D.before/after均出现；首次Create仍无after。观测已越过原EN最后边界，不能称窄链充分，也不能把随后崩溃精确归给factory QI/slot9/返回等任一调用；没有故障地址或栈。第二device QI E_NOINTERFACE/null，Ex成功分支不执行。后置GetAdapterLuid仍generated且超出授权替换范围，不继续泛替；额外诊断组0。
actual candidateSHA2B85E1DB413D9EE61E736783D218F825D30049954B95E7A54BE16484CFB512DB；testSHA9E627594AD169BAE209BE822C9F91CF4FDA8C690DD7FC8C2146ADB2CAFE1F2C0，MVID1b478f87-fdd6-4aab-bf8f-c22e73acf388；.NET10.0.13/X86/4，WPF Assembly10.0.0.0不是完整servicing身份。不扩x64/官方新跑/生命周期/BW1/CK10/full，立即撤回本轮默认实现，保留原trace与既有修复。

## ER-1 接管进行中
已读取最新交接、执行规则、EP/EQ 与 EN；继续逐段读取完整方案、DR、当前 Interop 和正式执行入口。尚未修改生产源码、发布或运行任务。
范围严格限首 Create 至 GetUpdateMethod 返回及失败 cleanup 的十种已核重载。保持 direct surface QI、检测、输出、HRESULT、引用计数及后置路径；不作全库替换。先正常分析器 x86 NativeAOT 与正式 DR 10.0.13；关键门未通过则仅撤回本轮实现。保留 EF/CF/CX/CY、已有 trace 和历史证据。
