# DW NativeAOT device QueryInterface 执行记录

## 1. 裁决

实际完成三个 x86 NativeAOT 实验。**direct Stdcall 正常越过 CheckDevice QI，但首次 Create 仍崩溃；generated 对照在 QI 返回前崩溃。未达到默认修复门槛，已撤回 direct 行为分支，保留默认关闭的条件 trace。专项未完成。**

本执行者直接使用文件、发布、Windows沙箱专用工具执行，无委派阻塞，无终端、脚本、原生探针、新Host或新工程。

## 2. 范围和方法

首先创建本报告；读完执行规则、完整120行替换方案、DU/DT/DR/DV全文，截断尾段另读。核对解决方案、正式项目、Directory.Build.props/targets、global.json、打包白名单、BL监督参数、DR入口。AGENTS名称搜索无结果。产品net10.0、验收net10.0-windows，SDK配置10.0.107/latestFeature，Nullable enable、Silk.NET.Direct3D9 2.23.0；未改版本。

唯一生产改动文件：Code/WpfGfxShape/Abi/InteropDeviceBitmapExports.cs。试验以DW_INTEROP_DIRECT_QI仅将CheckDevice QI改为delegate* unmanaged[Stdcall]<nint,Guid*,void**,int>，取合法device->LpVtbl[0]，相同this、IDirect3DDevice9Ex.Guid及extended输出地址。记录before/after/HRESULT/out、状态检查和释放阶段。默认保持generated调用。

DR/DS文件只读；复用DR_INTEROP_ABI原测试、独立Marshal Create声明和原断言，合法surface，无channel/Present/batch。未完成首次Create，不算三次循环通过。

三变体direct-x86、generated-x86、final-default-x86各自独有obj/native、obj/test、bin/native、bin/test、publish/native、publish/test、package，均在artifacts/dw-aot-qi/。正常依赖图发布，每变体各一次NativeAOT和一次Release/win-x86正式自包含测试发布，共6次成功。WarningLevel=0只控输出，不代表零警告。既有PackageSelfContainedComAcceptance生成全新白名单包；仅上传package，回收另置run-*，未沿用旧SHA、未回投回收包。未逐项认证全包清单字节，不能声称完成额外全包审计。

三个任务均正式ComAcceptance.exe的BL --supervise-exit角色，精确filter=FullyQualifiedName~WhenDrCandidateInteropCreateRunsThenIndependentAbiAndLifetimeAreRecorded，内30秒、外75秒。未跑full。

## 3. direct x86

任务e6acc1e87194407ab3094554a8f19c9b，6032→9520，UTC2026-10-21 11:54:30.7902625–11:54:35.4714774。

actual nativeSHA：294085410FA94A0B0F1C91E55F30B156C0ECC0B57C01FECD58B65F33AB5BF8F5。
actual testSHA：9CB4C4B27443DEEABDC9A4EE4E50C6FCBDE9B04D8422BCB59179E6314C6D48A3。
MVID：0151d3b9-baf0-45b8-944f-1975091e61cb。

surface.QI/GetDesc/GetDevice均S_OK，device=188160256。direct QI明确取得after：HRESULT=-2147467262/80004002 E_NOINTERFACE，extended=0；IID地址179098632、out地址179098648。本轮没有另输出GUID字节，值按源码未变。随后TestCooperativeLevel返回0，CheckDevice.after、GetCreationParameters.after均有。最后GetUpdateMethod.before，无after，ExitC0000005。

QI失败没有Ex拥有引用，未进入extended.Release分支，不伪造其释放证据。Create未返回，无bitmap或surface/device/factory最终释放记录。最早未定边界缩至GetUpdateMethod内部：该方法先GetDeviceCaps，条件成立才generated QI；无内部阶段，不能声称已定位第二个QI。

## 4. generated x86对照

任务69f0001db7c04656b274c523d93b72de，9912→10272，UTC11:55:39.8252884–11:55:48.5914242。

actual nativeSHA：96B0CC245703E20F19D1CEC695CF32FD8B6ED8324F896CD8DCFBB01AB09F77D4。
actual testSHA：59DBF1D54B8B40DE4A06D30B929F94902F3978950972F0F70EA1D91048BB63FE。
MVID：8a864b4e-80b9-4b4e-b637-d6fb2a72282d。

同一源码，仅不定义DW_INTEROP_DIRECT_QI，仍定义DT_INTEROP_CREATE_TRACE。surface.QI/GetDesc/GetDevice均S_OK，device=200022560。最后CheckDevice.generated.QI.before，无after，ExitC0000005。是同形合法场景独立进程对照，不声称跨进程指针相同。

已证只改CheckDevice调用入口会改变可达阶段：direct正常返回而generated未返回。未证全部Create故障同因、故障指令或AOT实际约定适配。DU的IL Cdecl仅线索，不断言整个Silk库或NativeAOT全局bug。

## 5. 撤回和最终默认实跑

direct仍未完成Create，故没有扩大修复GetUpdateMethod/GetAdapterLuid。已删除整个DW_INTEROP_DIRECT_QI行为分支；最终仍device->QueryInterface，保留IID、HRESULT负值处理、null、TestCooperativeLevel/CheckDeviceState及finally Release语义。只保留DT_INTEROP_CREATE_TRACE条件阶段调用，默认关闭；不绕过Ex检测、不吞错、不改分配。

最后源码修改后独有发布final-default-x86，不定义产品诊断符号，并实跑默认行为。任务9a407a0b9dc24606a69c129864b9bd58，10424→5964，UTC11:57:06.3099547–11:57:12.5426834。

actual nativeSHA：BC9CB7665F889B71638DEC8644796ACD13A41269365A1841A82671987FD36C1E。
actual testSHA：9B909EA192FBE9853B702F78929D5057FA7A4D0C66B0BB2B8A3BAD30CC0C0144。
MVID：e57d36cb-a05e-451e-b872-feea1f6d5835。

默认关闭诊断，首次Create before后ExitC0000005；不把trace格内部阶段冒作此格观测。默认行为已实际核查仍失败，没有遗留未验证的direct默认替换。

未修改DR/DS、NativeRun、ExitDiagnostics、VisualCache/historical、捕获、项目打包、共享文档、CF/CX/CY fill；无新增测试文件。兄弟方法只读审查，未作未经证实的同因修复。

## 6. 监督和证据回读

三份supervision.json各39行已全文读：均Reason=exited、Exit=-1073741819/C0000005；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，HasExited/OutputCollected=true，SupervisorError=null。工具完成不当作测试Exit0。

三格均.NET10.0.13、X86/4、MSTest4.0.1。actual模块均各任务Native/wpfgfx_cor3.dll，DR断言请求路径等于GetModuleFileName实际路径；FileVersion1.0.0.0，Product1.0.0+2e2a3c7e64d74642cb46f64247b40e73b4431299。Product提交不是工作区修改清单替代。PresentationCore程序集Version10.0.0.0，不代表已认证WPF servicing文件SHA。

三份aot.sha256各一行已读并匹配actual nativeSHA。三份DR业务JSON各6行、direct内部trace17行、generated内部trace8行、三份stdout各2行已全文读。前两格TestResults递归仅Deploy无完成TRX；最终格结果目录递归亦无完成TRX。三格无完成parentTRX/child.trx，不填通过率。监督中的结果目录均绝对C:/CodingAgentSandbox/Tasks/<task>/Results/dw-{direct,generated,final-default}-x86/TestResults。

原始证据：artifacts/dw-aot-qi/run-*/Results/各dw-*/supervision.json、dr-candidate.jsonl、stdout.txt；前两格另有dt-interop-create/stages.jsonl。包源与回收分离，未扩大清理范围。

## 7. 计数与未闭

发布6次成功，沙箱任务3次，x86候选调用实验3次，完成测试0、完成Create循环0；三次首次Create异常。新官方0、x64 0、生命周期/BW/CK 0、full 0。direct没有完成Create，未满足默认修复及后续成功矩阵门槛，没有盲跑。

最终保留条件trace增强和本报告；撤回DW独立Stdcall行为试验。最终默认产物已重新发布并实跑失败，SHA见第五节。

GetUpdateMethod内首错及其是否与CheckDevice QI同因未定；无可交付默认修复。Available实际回调继续未覆盖，不能从Create前记录断言崩溃前回调次数；真实HW消费、恢复、完整生命周期未覆盖。已完成有判别力NativeAOT对照，不等于Interop专项或完整替换完成。
