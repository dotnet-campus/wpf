# C5 selected production active-input：真实 CL 与确证缺项目

> 最新机器裁决见[唯一详档](GenProto-C5-final-adjudication.md)：当前已测源码333风险=13适用build声明已证+92 build未知+227 include未知+1缺项目，NA=0。下文105/227及332全部审计下一任务仅历史；原group未规定保守333穷尽额外门。319未知保留风险，不排除；已证适用bilinearspan及历史binding/ARM64缺口仍Blocked。原12配置16 TU原物不重采，不冒充完整生产build。

> 当前裁决已由[机器总对账裁决](GenProto-C5-final-adjudication.md)及其唯一JSON总表接管。以下为历史采证：现已实际机器对账17,377记录；正式入口仍9；旧333行分流105 build条件未证、227 include适用性未证、1缺项目，不把333作为正式边分母。自主全部适用边审计未闭，不统称external。

## 实际执行与原物

本轮只添加受控 MSBuild 调用入口 `Investigations/GenProtoEvidence/c5-selected-evaluation/ActiveIncludes.proj` 与 `ActiveIncludes.targets`，不新增扫描器、测试、正式门或生产实现。入口向原 resources/uce/wpfgfx 项目注入目标，只调用其已注册的内置 CL task；Debug/Release × Win32/x64 共12组合。每项目 precomp.cpp，另 uce/generated_resource_factory.cpp，共16次 `/P /showIncludes`。原defines、forced includes、ordered includes、SDK与VC来自原项目当前evaluation；强制核对 v143 / 14.44.35207 / 10.0.26100.0，不使用NativeLayout/v145。仅为展开头将PCH改NotUsing，非生产对象编译或链接，不冒充完整生产build。

最终执行退出 **1**：CL预处理全部退出0；原wpfgfx的bilinearspan引用在四配置均实际报 **MSB3202**。前置首次环境参数转义错误已修复后重跑，不作为缺源证据。未执行终端脚本、Exec、生成器、全WPF Build或链接。

标准原物目录：`artifacts/log/GenProto-C5/<project>.<configuration>.<platform>/`：`selected.txt`保存properties、TU items、defines/forced/ordered includes及project/source/target SHA256；`*.i`是CL直接产出、保留#line；`preprocessed-sha256.txt`保存原物SHA256；`result.txt`保存CL退出码。12份result已检索读回，16份.i已列目录；关键#line、factory正文与hash已读回。控制台1.65MB输出被工具截断，不声称完整showIncludes栈已保全；以实际.i为原物。`production-reference-error.txt`为明确标记的实际错误摘录，不冒充完整日志。

读回例：uce Debug x64 generated_resource_factory.i SHA256 `2379E0625BAFED23C3D52EB5F06EDD9B3DCF5923C081CDFDE0F9EFF57205E4CE`；precomp.i `2A1C992A35D53527235BC5864E2EBFEAB66825F06ED0A150DED7533D8AAB88FA`。uce.vcxproj `C6D6BE3E62F989C7518429B65EB44B722223FC152DA0927F801E00C0F36BD370`；factory.cpp `FB661D72575A02E316ECA420E974D4CF0039D2596A13DBEBE274B577C3235322`。其余组合身份在各原物旁，不手工伪造hash。

## 原边与固定入口裁决

正式仍为 Resource.xml、Resource.xsd、mcg.proj、resources.vcxproj、uce.vcxproj、wpfgfx.vcxproj 六physical，及原 CLOSURE/REGEN-DIFF/CONSUMER-DECLARATIONS 三evidence。原evidence.jsonl:58–59及group已实际读取。原888候选、333审计行均不升级active或required。

| 已实际到达原边 | 原物与既有归属 | 有限裁决 |
|---|---|---|
| uce factory.cpp → precomp.hpp → wgx_core_types.h | factory.i:359368，实际来自 artifacts/obj/uce/x64/Debug/wgx_core_types.h；uce physical consumer | active，但不是源码Include同名文件优先；历史生成authority仍未证 |
| precomp.hpp → wgx_render_types_generated.h | factory.i:363315，原Include/Generated；既有Resource.xml/xsd→mcg生成映射 | active；复用既有生成/签名证据，不据此提升D4 |
| precomp → generated_resource_factory.h | factory.i:657082；uce物理consumer，原factory97 IDs断言 | active声明输入，不等于97 case本轮全执行 |
| precomp → resources_generated.h/data_generated.h | factory.i:666905/666939；resources物理owner/uce consumer，既有生成映射 | active；不证明全部原333边已逐条裁决 |
| factory.cpp → CResourceFactory::Create | factory.i:677510后实际正文、TYPE_MEDIAPLAYER等case；既有signature/factory断言 | 当前预处理正文已证，不是runtime/packet执行 |
| wpfgfx → core/sw/bilinearspan/bilinearspan.vcxproj | 原wpfgfx:142–144，无局部Condition；原sln:74及eng/WpfArcadeSdk/tools/WpfProjectReference.targets:77同指此路径 | **实际适用ProjectReference、GetTargetPath真实MSB3202**，不能删除绕过；不与bilinearspan.h混同 |

## 最终有界对账清单（本次读原物，不是新CL运行）

原物位于**工作区根** `artifacts/log/GenProto-C5`，不是 `WpfGfxShape/artifacts`。本次专用文本检索覆盖16份.i，以下是物理行号，不是#line中的源行号。路径与selected.txt、result.txt及旁置hash交叉读取；未新增解析器或生成机器总裁决报告，不把文本检索冒称完整条件栈解析。

| 原物组合（D/R=Debug/Release，32/64=Win32/x64） | precomp.i：core/render | precomp.i：factory/resources/data | factory.i：core/render/factory/resources/data |
|---|---|---|---|
| resources D32 | 359391/363338 | 656050/666650/666684 | 不适用：未选该TU |
| resources D64 | 359361/363308 | 656020/665843/665877 | 不适用：未选该TU |
| resources R32 | 359391/363338 | 656065/666665/666699 | 不适用：未选该TU |
| resources R64 | 359361/363308 | 656035/665858/665892 | 不适用：未选该TU |
| uce D32 | 359391/363338 | 657105/667705/667739 | 359398/363345/657112/667712/667746 |
| uce D64 | 359361/363308 | 657075/666898/666932 | 359368/363315/657082/666905/666939 |
| uce R32 | 359391/363338 | 657120/667720/667754 | 359398/363345/657127/667727/667761 |
| uce R64 | 359361/363308 | 657090/666913/666947 | 359368/363315/657097/666920/666954 |
| wpfgfx D32 | 359378/363325 | 本次检索未到达，不裁决排除 | 不适用：未选该TU |
| wpfgfx D64 | 359348/363295 | 本次检索未到达，不裁决排除 | 不适用：未选该TU |
| wpfgfx R32 | 359378/363325 | 本次检索未到达，不裁决排除 | 不适用：未选该TU |
| wpfgfx R64 | 359348/363295 | 本次检索未到达，不裁决排除 | 不适用：未选该TU |

core均实际选 `artifacts/obj/<project>/<platform>/<configuration>/wgx_core_types.h`；render选原 `Include/Generated/wgx_render_types_generated.h`；factory选 `core/uce/generated_resource_factory.h`；resources/data选 `core/resources/` 同名生成头。物理来源优先于同名候选；不能从#line出现推断完整父子include栈或原历史生成身份。

| 固定正式入口（REC前缀全名见原条件表） | owner / signature / generation / consumer / assertion对账 | 本次能确定的范围 |
|---|---|---|
| Resource.xml、Resource.xsd 两physical | 当前输入owner → 既有R authority及378输出差分 → render/resources/data/factory生成映射 → 所选resources/uce消费者；C1/C3/D1–D3 | 复用现有逐输入hash和生成映射，新增实际active consumer路径；未重跑生成器，不提升D4 |
| mcg.proj physical | 当前生成入口 → 原Resources.rsp invocation → 上述生成输出；C2/C5/D1 | 当前生成合同既证；obj/core头的原历史版本绑定不能由本次CL补足 |
| resources.vcxproj physical | 所选四配置precomp → core/render及uce factory声明、resources/data声明 → 既有consumer/signature映射；C4/C5 | 四配置均有具体.i位置；不把包含uce声明改称resources拥有uce实现 |
| uce.vcxproj physical | 四配置precomp及factory.cpp → 同上声明 → CResourceFactory::Create实际正文 → 既有F 97 IDs/76构造/21拒绝断言；C3/C4/C5 | factory D64正文677510以后已读取签名与case；97 IDs执行来自既有F，不是本次CL执行97断言 |
| wpfgfx.vcxproj physical | 四配置precomp → core/render；显式ProjectReference → bilinearspan GUID；C4/C5 | 头active已证，项目引用适用且缺物已实证；不是header失败或生成头依赖失败 |
| EVID-GENPROTO-CONSUMER-DECLARATIONS | 既有P签名、F factory断言及上述consumer路径；C4 | 既有Satisfied保持；预处理不新增runtime断言 |
| EVID-GENPROTO-MILCMD-CLOSURE | C1–C4有限证据 + 本表C5实际输入 | C5所选active子范围已证；全部适用原边逐record/assertion裁决尚缺，父级Blocked |
| EVID-GENPROTO-MILCMD-REGEN-DIFF | 既有R隔离再生、逐输出全文/token/diff与身份；D1–D4 | D1–D3有限Satisfied保持，D4历史authority未满足；不以CL替代再生差分 |

本表合并展示两输入physical但分母仍是6，不新增required。P/R/F的实际报告、hash和断言身份见[原条件表](GenProto-required-final-reconciliation.md)，不复制成一次新执行。

### 真正剩余：按原条件分开，不一律blockedexternal

1. **C5可自主审计余项**：原333审计行中尚未与本次所选TU对应的project/import/属性及include边，仍需逐条核对适用条件、实际路径、既有record与assertion；本表只裁决上列实际到达边。未到达、include guard抑制或条件未选都不能直接标Excluded。现有evaluation与.i允许继续有界对账，这部分不是已证外部缺物，也不是333个新required；本次未完成全量逐边机器裁决，不能报告自主余项零。
2. **C5真实资料边界**：原wpfgfx显式适用bilinearspan引用的来源/项目输出合同无法满足group readiness“全部实际物理输入/组件/冻结输出/消费者已有record”。确证缺项目且没有权威生成合同，不能自主制造来源。用户补件后仍须核对其原依赖与输出归属，不能收到文件即闭门。
3. **D4真实资料边界**：当前生成器、378差分及active头不能证明历史CLR/hash/版本和冻结生成来源；须原历史authority。ARM64缺工具链/真实执行独立保留。没有新证据把原333其余未知全部归这两类。
4. **总体非C5余项**：原七阶段、ABI/寿命/E2E及可见HW control合同仍按总计划逐门保留；两transport packet与独立discard未证只限制等价外推，不新增生成required门。本轮不据此改生产或重跑COM。

因此：对bilinearspan来源及历史binding已经到真实资料边界；对现有原物的逐边审计尚未到资料边界。可自主做的正式动作是上述C5适用性/record/assertion对账，不是再采集同一16 TU、重跑全WPF或重领已闭x86/consumer证据。

## 准确外部补物与停止边界

最小确证缺物：`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/bilinearspan/bilinearspan.vcxproj`，GUID `{3A6A5D23-CB65-4685-AAC9-0969054701EA}`，以及该原项目实际引用的源/props/targets。名称检索和原core/sw目录读取均证实项目子目录缺失；eng引用映射/ShippingProjects仍列它；原codegen搜索未发现生成该项目的规则。现有 `core/sw/swlib/bilinearspan.cpp` 与 `core/sw/bilinearspan.h` 不构成该缺项目的生成合同，不能擅自复制/拼装项目。

需用户补原项目及其依赖，或提供原仓库有权威的生成该项目的target、输入及输出合同。当前不能给不存在的合同命名。CL必要TU本次成功，**没有证据证明它依赖缺project生成header**；缺物阻断的是所选生产项目引用闭包，不谎称C1083。真实错误已足以停止自主补造来源；原333未到达的边仍不标Excluded/Satisfied。

C5真实CL原物缺口已推进，但整体仍Blocked：缺引用来源、全部适用原边到既有owner/assertion的完整裁决尚不成立。不声称自主C5全闭或自主余项为零。两个required及父级Blocked，stage4Closed=false；既有有限childSatisfied只指原范围。四外部历史authority、ARM64真实工具链/环境、合法full runtime、官方player protocol及可见DWM/HW control前置仍缺。生产3F291D3E未改，不publish/COMfull；测试源码未改，历史46/46不是本轮新跑；DLL驻留unload=N/A。
