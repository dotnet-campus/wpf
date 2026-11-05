# EK：GetCreationParameters 单点 ABI 认证与最小验证

## EM 补完：EK认证证据与EL同DLL结论

收口回读确认：五文档已全文分段读至末（更新前EL44行、EK31行、handoff450行、汇总560行、状态577行），超长截断已补读；最终状态补记再精确回读，生产未新增修复。

本节覆盖下方历史“x64待测/同DLL未闭”，原文保留。EM不新增实验、发布或沙箱任务，不改代码/入口/打包。完整审核见[EL/EM收口报告](replacement-supervised-creation-parameters-samedll.md)。

已亲读既有readonly/run-x64/Results/ek-abi.json全部297行：.NET10.0.13/X64/指针8，独立与Silk结构均24字节，offsets0/4/8/16；x86对应布局16字节、0/4/8/12已核。两重载slot9的calli0102080F1182580F1181A4为Cdecl、显式device/output指针、int32返回，managed HasThis不等于native HasThis；与SDK Stdcall合同不同。未直接观测ESP，未实现canary/设备字段对照；IL本身不是NativeAOT验收。

EL原件两组actualNative SHA C7E3EAADCAED3C5335B049074A2FF234BBDFF0451FCA11BA59EE7F2472956702、testSHA4172A58E5FE2D7A1594D253AC64609D23D948C85128E09EACBB092326A59C1EA/MVID39090b4c-4a11-4de9-8a32-27f5c2025332完全相同。保存源中File.Exists(BaseDirectory/ek-generated.input)仅切Create.GetCreationParameters；direct标记不存在、generated一行标记已亲读。direct task3f6221abb9414da08c96326c3e7373ab Exit0三循环refs0；generated taskdd182cd677f64ffd8e41d939a8e72804 C0000409、Create+0x388 NRE，GetCreationParameters Hr0/GetUpdateMethod.after均存在。受控同binary外部因果成立，不定唯一默认根因/首坏地址/整个Silk库bug。

TRX精确直读补正：direct Results/el-creation-samedll/TestResults/child.trx实际存在，26行全文核、1/1通过；撤销旧missing推断。generated同路径实际缺失，不造完成计数；父监督非MTP，不造父TRX。监督各39行、业务39/6、业务stages39/6、DT trace75/21、stdout12/2、stderr空/2及两actual identities均亲核。Available0未验证真实回调。

当前Interop627行全文只读核，Code检索EK_CREATION_PARAMETERS/EkRelease零残留，默认generated与DT/DX条件trace保留；CF/CX/CY、EF不改。ElEvidenceSnapshot.cs为空、Tests无引用。709行实验快照仍在ignored artifacts，不上传历史根。默认Release正常分析器方案build一次Exit0、3664警告/0错误、18.04秒，未压制；默认增量非clean/rebuild，具体项目增量命中不可由截断核。日志62行已读，stdout3075451字符仅尾20000、前3055451未显示；非发布/AOT验收。

下一限定GetCreationParameters正确Stdcall实现修复已具同DLL因果依据；当前default其它QI已知问题仍在，生产保留须正常分析器官方/候选双架构DR循环/refs、必要生命周期/BW/CK。无需泛化全direct对照或重跑full；本轮无默认修复。x64 Event0/EHANDLE另线，HW/loss/media/resources/通知/最终四格未闭，安全文档完成不是生产已修。

## EK-1 进行中：入口与约束核查（历史）

本报告按实际工具阶段即时更新，不伪造墙钟时间。已读取最新交接开头、执行规则全文、EJ/EI开头；超长返回不视为全文已读，正在分段补齐。尚未修改生产或测试源码，尚无新发布、沙箱任务或实验。

目标：核 Silk.NET 精确版本 GetCreationParameters API/IL/calli 与 SDK slot9 Stdcall；独立核 x86/x64布局与合法输出 canary。优先同 NativeDLL/testSHA 运行时单调用选择，其他调用全 direct 受控，最多两个有效 x86 组；先成功控制再 generated。不得以自尺寸代替独立预期，不以 Hr0 排除调用破坏。

仅现有正式项目、纯 C# 专属诊断及 Interop 必要条件实验；不修改 DR/DS/NativeRun/ExitDiagnostics/VisualCache/历史测试/捕获/WindowQ/打包，不新增 Host 或工程，不执行脚本或机器码探针。专属产物根 artifacts/ek-creation-parameters。正常分析器，无压制；未通过全部要求的行为精确撤回，保留 DT/DX 默认关闭 trace、CF/CX/CY 与 EF。

## EK-2 实测 IL 已取得（即时补记）

只读诊断放在现有 WpfGfxShape.Tests，EK_ABI_READONLY 显式编译，Utf8JsonWriter输出。首次发布因全局obj路径使两工程中间产物冲突失败（CS0246 Windows），未执行；随后专属Diagnostic.props按MSBuildProjectName隔离obj/bin发布成功，正常分析器未压制。纯C# exe task4f37a3c7591641589d684ef2a1775e87运行，297行ek-abi.json四段全文回读；只读任务不计有效NativeAOT对照组，DR新计数无。

.NET10.0.13/X86/指针4。Silk实际2.23.0.0，SHA F08ACBB76DD7C60803F8F0D4D8212C77A85D3EF67B1FB42FBB18F3C15A8586E4，MVID deec18f5-2a98-450b-93cd-9117248c6163。指针与ref重载均slot9，calli签名0102080F1182580F1181A4：01=Cdecl，参数02，返回08=int32，两个显式pointer参数。managed方法Standard,HasThis不是native约定；IL显式传device和输出地址，无native HasThis/ExplicitThis位。与SDK要求Stdcall不同；未观测ESP，不声称NativeAOT实际机器调用已认证，需同DLL对照闭合外部因果。

独立Expected与Silk实测x86尺寸16、偏移0/4/8/12；目前仅布局，尚无合法buffer canary/设备字段对照，x64待测。证据artifacts/ek-creation-parameters/readonly/run-x86/Results/ek-abi.json。生产行为未改；完整替换未完成。
