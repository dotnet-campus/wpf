# EL：接续 EK，同 DLL GetCreationParameters 对照与安全交接

## EM 最终安全收口与独立原件审核

收口回读确认：五文档已全文分段读至末（更新前EL44行、EK31行、handoff450行、汇总560行、状态577行）；汇总超长截断已补读。最终状态补记再精确回读，不将文档完成当生产修复。

本轮仅文档收口、只读源码/原件审核及一次默认 Release 方案构建；新实验、发布、沙箱任务、代码/入口/打包修改均为0。按实际工具阶段记录，不伪造时间。已完整读执行规则27行、完整方案120行及当前Interop627行（超长段已补读）。

### 真实构建（不等于运行验收）

专用构建目标 WpfGfxShape.slnx，仅 configuration=Release，Exit0、3664警告/0错误、18.04秒；未使用NoWarn、EnableNETAnalyzers=false或WarningLevel压制。默认增量，未clean/rebuild；截断使各项目实际增量命中/重编译清单不可核，不称全量重建。返回日志62行已全文读取；stdout共3075451字符，前3055451未显示，仅尾20000可读，不声称持有完整原始输出。正常分析器构建不是NativeAOT发布、双架构DR或最终矩阵验收。

### 两份独立原件亲核

根为 artifacts/el-creation-samedll/run-direct 与 run-generated。两份 Results/el-creation-samedll/supervision.json 均39行全文核，Reason=exited、未超时/取消/kill、SupervisorError=null；运行 .NET10.0.13/X86/指针4。

- direct：task3f6221abb9414da08c96326c3e7373ab，Exit0；dr-candidate.jsonl39行、业务stages39行、dt-interop-create/stages75行、stdout12行及空stderr均全文核。三次Create Hr0、12×3、bitmap最终refs0，surface/device/factory最终refs0，callback根释放、模块驻留。Callbacks=0且NativeInvocationCovered=false/ConsumerCovered=false，不能称Available/通知已验证。
- generated：taskdd182cd677f64ffd8e41d939a8e72804，Exit=-1073740791/0xC0000409；业务6行、stages6行、dt trace21行、stdout2行、stderr2行均全文核。首次Create无after，stderr明确InteropDeviceBitmapExports.Create+0x388未处理NullReferenceException，非监督设施异常。GetCreationParameters.after Hr0与GetUpdateMethod.after均存在，不把最后before/after当首坏地址，也不以Hr0排除前序ABI破坏。

两份实际身份均亲核业务identity与Results/aot-identity.json，而非仅预期文件：actualNative分别位于 C:\CodingAgentSandbox\Tasks\<上述task去task前缀>\Native\wpfgfx_cor3.dll；SHA均 C7E3EAADCAED3C5335B049074A2FF234BBDFF0451FCA11BA59EE7F2472956702，版本1.0.0.0/Product1.0.0+2e2a3c7e64d74642cb46f64247b40e73b4431299。实际测试程序集为各task根WpfGfxShape.ComAcceptance.dll，SHA均4172A58E5FE2D7A1594D253AC64609D23D948C85128E09EACBB092326A59C1EA，MVID均39090b4c-4a11-4de9-8a32-27f5c2025332；WPF PresentationCore10.0.0.0。

### 精确TRX读取补正

直接读取 run-direct/Results/el-creation-samedll/TestResults/child.trx 成功，26行全文核：WhenDrCandidateInteropCreateRunsThenIndependentAbiAndLifetimeAreRecorded，Completed，total/executed/passed=1，failed/error/timeout/aborted=0。与direct stdout1成功/0失败/0跳过及监督Exit0一致。覆盖历史handoff“回收目录缺该文件”的错误推断；不是仅枚举missing。

直接读取 run-generated/Results/el-creation-samedll/TestResults/child.trx 返回文件不存在；只有监督退出/业务/stderr失败证据，没有完成TRX计数，不虚构失败测试计数。父监督非MTP，不应生成或补造父TRX。

### 单开关因果与ABI证据边界

已亲读保存实验源Snapshot/InteropDeviceBitmapExports.cs第190–245行：EK_CREATION_PARAMETERS下仅Create内通过File.Exists(Path.Combine(AppContext.BaseDirectory,"ek-generated.input"))选择generated binding或slot9显式Stdcall，其他受控direct不变。direct精确路径input不存在；generated精确路径input一行“EL: select only Create.GetCreationParameters generated binding.”已读；trace分别EK.direct/EK.generated。相同NativeSHA、testSHA/MVID排除本次两组重编译/代码布局差异，足以成立此受控同binary外部单开关因果；不宣称唯一默认根因、首坏地址或整个Silk库bug，不据此把全direct留作默认修复。

只读x64原件 artifacts/ek-creation-parameters/readonly/run-x64/Results/ek-abi.json297行四段全文读完：独立/实际均24，offsets0/4/8/16；x86对应原件布局独立/实际16、0/4/8/12已核。Silk2.23.0.0 SHA F08ACBB76DD7C60803F8F0D4D8212C77A85D3EF67B1FB42FBB18F3C15A8586E4、MVID deec18f5-2a98-450b-93cd-9117248c6163。pointer/ref重载slot9，calli签名0102080F1182580F1181A4，01=Cdecl、两个显式指针、int32返回；managed Standard,HasThis不是native HasThis，与SDK COM Stdcall合同不同。IL/布局本身不等于AOT机器调用验收；没有直接ESP、合法输出canary及字段对照验证，不补造。

### 源码安全状态与下一门

EL既有19块精确撤回由当前只读全文及Code文本检索复核：EK_CREATION_PARAMETERS/EkRelease零残留。当前Create/GetAdapterLuid仍默认generated；DT/DX条件只读trace保留，CF/CX/CY与EF不改；其它QI已知问题仍在，未新增默认修复。ElEvidenceSnapshot.cs精确读取为空，Tests文本检索ElEvidenceSnapshot零命中，无悬空引用发现，无需改代码。完整709行实验源已保留ignored artifacts/el-creation-samedll/snapshot/Snapshot/InteropDeviceBitmapExports.cs（既有SHA AB55BB1A58DFB1AF735043893DA92C8FAA8E695662E0FD145ADECA62DE79C566，本轮未重算）；根.gitignore含artifacts/，不上传历史根。

下一动作：现有同DLL因果足以进入限定GetCreationParameters正确Stdcall实现修复，无需回到泛化全direct对照或重跑full。正式生产保留必须经过正常分析器、官方/候选双架构DR循环/refs与必要生命周期/BW/CK回归；当前default仍有其它QI问题，不把单点证据外推全部恢复。x64 Event0/EHANDLE崩溃另线；HW/loss/media/resources/通知/最终四格均未闭。本轮文档/安全收口完成不是生产已修或完整替换完成。

## EL-1 接续进行中（历史）

接续 [EK 认证报告](replacement-supervised-creation-parameters-validation.md)。按工具实际阶段记录，不伪造时间。已更新唯一交接入口；正在读取完整方案、当前 EK 条件源码和实际正式 package。尚未新增发布、沙箱任务或修改生产行为。

约束：最多两个有效 NativeAOT x86 任务，先 direct 成功控制（三循环 refs0）再 generated；使用同一 NativeDLL 和测试程序集，仅专属 ek-generated.input 标记选择 Create.GetCreationParameters，其他受控 direct。初始化失败不计有效组，不上传裸 publish 或回收根目录。先核 aot.sha256、Contracts、Native、MTP/runtime 与候选身份。

IL Cdecl 与 SDK Stdcall 差异仅为已有只读证据，不替代同 DLL NativeAOT 因果验证。x64 JSON 必须补读全文。canary 若未实现不新增实验变量。结束保存完整实验源至 ignored artifacts，精确撤 EK_CREATION_PARAMETERS 与专属辅助，恢复 default generated 和 DT/DX 只读 trace，保留 CF/CX/CY 与 EF，不改变业务断言。
