# ET：后置 Adapter LUID 区间闭包运行对照

## EU 独立验收边界与文档闭环（最新优先）
EU最终回读补记：本轮四文档最终正文已全文小段读至末（补记前ET35/handoff509/session-summary597/issues614行）；所有截断均缩小补读，随后再次读取本补记。ET历史“共享全文未读”交付缺口现由EU关闭，历史过程不删除。仅文档与独立证据边界收口，不是替换完成；默认generated、CF/CX/CY保持，无新生产修复或新运行。SHA只核报告，未独立重算。下一仍仅另授权同NativeDLL运行时后置slot6/QI0/Release2 direct/generated固定布局日志对照。

EU仅只读核证与四文档收口：无源码修改、新实验、构建、发布、沙箱、终端或脚本；禁止full，默认无新生产修复。过程更新已执行，历史全部保留。
独立亲读 artifacts/et-adapter-runtime/run-candidate-x86 下监督39行、精确 Results/et-candidate-x86/TestResults/child.trx 26行、业务dr-candidate.jsonl 39行、ET stages.jsonl 21行、aot-identity.json 1行至末：task a9846dc31dd2457abd0c6b4e8612b9f0，父5456/子4076，Exit0，1 executed/1 passed/0 failed；三次Create Hr0、12x3，QI同identity、AddRef2/Release1、GC后Dirty Hr0、Detach及Bitmap最终refs0，最终surface/device/factory均0。监督无timeout/cancel/kill/drain异常。Requested=Actual为该task的Native/wpfgfx_cor3.dll；业务与aot-identity记录NativeSHA 1F11BFE00844E9FCD9F2404F35703EAA2316FBC3C78B43438CE1FEFCF2A78E9B，testSHA 7C8F41EECD5CA5806B4D88A8CB38127F0FFAC824F0CFA00B2DCBB7DF1E62F057/MVID11f29ca9-2390-4717-9e29-7653b2f2dbc2，.NET10.0.13/X86/4。
三次真实GetDirect3D Hr0/factory203552256，factoryQI E_NOINTERFACE(-2147467262)/extended null，factory Release2；实验快照312–357行的真实失败分支解释第二GetCreationParameters、LUID与extended Release未执行，不能填造字段或覆盖。Available Count0、NativeInvocationCovered/ConsumerCovered false，不是回调消费。完整日志行本身不证明日志Dispose返回；后续priorLogReturned与外部Create after共同约束返回边界。区间组合通过不是唯一方法根因，也不排除编译布局/日志差。
只读对照当前InteropDeviceBitmapExports.cs 283–305与default-before-et.cs.txt同段，均为generated调用及原能力/cleanup判断。恢复SHA一致值1C3E30CCB283A71FAE61286C582D7899A036911F474D3632614678F207FFFFB1仅核ET既有认证记录；现有专用工具无独立hash重算能力，本轮没有重算或全文件字节一致性重认证。残留检索严格限WpfGfxShape/Code/WpfGfxShape/Abi，正则EtTrace|creationBoundary|QueryCreationDevice|ReleaseCreationDeviceEx无匹配，不外推整个仓库。CF/CX/CY及既有EF/trace保持，本轮不改代码；ET默认Release build成功仅属历史记录，不是本轮构建或默认AOT验收。
下一唯一最小对照（须另授权，本轮不执行）：同一NativeDLL内运行时仅切换后置实际执行slot6/GetDirect3D、factory QI0及factory Release2的direct/generated，保持同编译布局、同日志与已核ER控制背景，以排除编译布局和日志差；不重做ER全背景，不绕真实能力判断。Ex/Available/HW真实内容与恢复/full四格/双架构DR及生命周期/BW/CK/x64 Event0仍未闭。
EU回读进度：正在对四文档最终全文小段回读至末，截断缩小补读；完成后补记并再次读取，不以历史回读代替本轮。


ET最后补记：报告24行已分段回读至末，三个共享文档新增ET段已回读；历史全文重读要求尚未满足，明确列为交付缺口。默认GetAdapterLuid已亲读恢复为generated，SHA一致与零辅助残留另已核；唯一业务任务不再重跑。

## ET-4 最终：非Ex区间闭包成功，源码已恢复，默认未修
唯一有效新task a9846dc31dd2457abd0c6b4e8612b9f0，父5456/子4076；监督记录子Exit0，Reason exited，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut均false，OutputCollected true，SupervisorError null。纯C#正式DR清洁包完整回收于 artifacts/et-adapter-runtime/run-candidate-x86；未full/未第二业务task/未官方新跑/未x64。
实际候选Requested=Actual=C:\\CodingAgentSandbox\\Tasks\\a9846dc31dd2457abd0c6b4e8612b9f0\\Native\\wpfgfx_cor3.dll；NativeSHA 1F11BFE00844E9FCD9F2404F35703EAA2316FBC3C78B43438CE1FEFCF2A78E9B，aot-identity与业务一致。File1.0.0.0/Product1.0.0+2e2a3c7e64d74642cb46f64247b40e73b4431299；testSHA 7C8F41EECD5CA5806B4D88A8CB38127F0FFAC824F0CFA00B2DCBB7DF1E62F057，MVID11f29ca9-2390-4717-9e29-7653b2f2dbc2；.NET10.0.13/X86/4，PresentationCore Assembly10.0.0.0不充当完整servicing身份。
BL三参数root/90/filter确实使用，root经Path.GetFullPath规范到任务绝对路径；监督Arguments中TestResults/Diagnostic均为该task绝对路径。精确 Results/et-candidate-x86/TestResults/child.trx 26行全文亲读：1 executed/1 passed/0 failed/0 skipped；stdout12行和空stderr、业务39/ET21/DT72/监督39/identity1均读至末，DT初次截断已分段补齐。未以完整JSON行单独证明日志Dispose已返回；后续priorLogReturned、Register及外部Create after共同界定已通过日志尾声与方法返回。
真实输出：三次Create Hr0、width12/height3，Bitmap203525520/192194576/192194672；QI同identity、QI平衡Release1、AddRef2/Release1、GC后Dirty Hr0、Detach前后均有、三次Bitmap最终Release0。最终surface/device/factory均Release0；completed已记录。Available Count0且NativeInvocationCovered/ConsumerCovered均false，保持未覆盖。
后置三次GetDirect3D实采Hr0、factory203552256；QI前factory不变/extended0，QI后仍extended0、真实HRESULT -2147467262(E_NOINTERFACE)，factory Release返回2。按既有失败分支退出，所以第二GetCreationParameters、GetAdapterLUID、extended Release均未执行；不存在可报告的后置AdapterOrdinal/DeviceType/FocusWindow/BehaviorFlags/LUID/Ex Release值。没有为填日志而绕能力判断或提前固定返回；前序参数字段本次未单独记录，这是字段覆盖缺口，不称字段已核。
成功说明ER十调用背景加入后置区间整体后，当前非Ex路径可完整Create及refs0；不能唯一归因GetDirect3D、factoryQI、Release或整个Silk库，也不能排除编译布局/新增日志影响。实际执行子集是slot6、factoryQI0、factoryRelease2；其余分支代码已闭包但运行未覆盖。
已从本轮修改前完整快照恢复唯一修改的Interop文件，恢复前实验完整source已保存。恢复源与当前文件SHA256均1C3E30CCB283A71FAE61286C582D7899A036911F474D3632614678F207FFFFB1；EtTrace/creationBoundary/QueryCreationDevice/ReleaseCreationDeviceEx全Abi搜索零残留。默认generated及原DT/DX/EF保持，CF/CX/CY未改。未修改DR/DS/NativeRun/ExitDiagnostics/VisualCache/historical/捕获/WindowQ/项目或打包源码。ignored辅助仅供归档，不被默认构建导入。
恢复后默认正常分析器Release整个方案build Exit0，0警告/0错误、1.74秒，19行日志全文亲读；这是普通增量build，不是默认NativeAOT运行验收。未默认保留direct补丁。
下一最小点：以此次完整ER背景/ET快照和已过非Ex路径为控制，单独约束实际执行的后置slot6/QI0/Release2及日志扰动；另需具备真实factoryEx能力的独立覆盖才能读第二slot9字段/LUID/Ex Release，不强行改能力或重复无信息组合。本轮不继续耗任务。双架构DR、生命周期/BW/CK、Available通知消费、真实HW内容/恢复、full最终四格与独立x64 Event0均未闭。
文档核对范围：本轮报告与新增共享ET段逐段回读；共享文档的全部历史长正文尚未重新全文核读，不承袭ES旧全文回读为本轮完成证据。保留该交付缺口，不能写成四文档历史全文已读。

## ET-3 唯一任务成功，开始撤回（历史）
Task a9846dc31dd2457abd0c6b4e8612b9f0：监督Exit0，无timeout/cancel/kill/drain异常，TRX 1/1 Passed。三次Create Hr0、12x3，QI同identity/AddRef2-Release1/GC后dirty/Detach及Bitmap最终Release0；surface/device/factory最终均0。后置GetDirect3D三次真实Hr0；factory QI均E_NOINTERFACE(-2147467262)/extended null，factory Release均2。真实能力分支使第二slot9/LUID/extended Release不执行，不造Creation字段、LUID或Ex Release结果；该成功仅认证非Ex路径区间组合，不定位唯一方法。业务39行、ET21行、监督39行、identity、TRX26行、stdout12行/空stderr已读；DT首段存在截断待补读。没有第二业务任务。

## ET-2 发布完成，准备唯一有效任务（历史）
ER背景13处唯一替换成功；先保存default-before-et与er-ten-direct-background完整源码，再保存et-full-experiment完整源码（ignored artifacts/et-adapter-runtime/source）。后置六调用种类改SDK Stdcall，保持输出/失败分支/cleanup；EtTrace采用Utf8JsonWriter无反射，HRESULT明确可空，不把before默认0记实采，后续priorLogReturned只证明前次日志已返。正常分析器NativeAOT x86与DR selfcontained10.0.13各正式发布成功；无NoWarn/关规则。最初仅快照专用发布因缺RID报RuntimeIdentifier required，补RID后成功，未启动业务task。尚无运行结论。

## ET-1 进行中（历史）
已接管：本轮仅在 ER 十调用 direct 背景加入 GetAdapterLuid 的 device GetDirect3D、第二 GetCreationParameters、factory QI/GetAdapterLUID/两 Release 的 SDK Stdcall 区间闭包。最多一个有效新 x86 task，不据组合结果归因单方法，不默认化修复。
已读执行规则、ER 报告；正在分段读取完整方案、ES、当前源码与正式 BL 入口。尚未修改生产/测试/项目源码，尚未发布或执行。
必须保存 ignored 完整源码快照、保留真实能力与 LUID/cleanup；原 DR 断言不变。结束撤回 ET 行为与辅助，保留既有 DT/DX/EF/CF/CX/CY，默认构建并核对实际证据。Available/full/HW/x64 Event0 未闭。
