# ProcessCreate：工厂分母实测与未关闭合同

## 本轮实际交付

串行生产 Release/win-x64 NativeAOT publish、标准 COM 包 publish 成功；没有终端脚本、stub、skip 或预期修改，没有新增原生代码。继承的移除泛用兜底修改已实际编译运行。SHA256：`F7F6AD93369D910801C7E98B6D3C43E3327E451506D6785D178394284A125780`。

根因：GeneratedResourceFactory 原泛用基类兜底把没有原生工厂 case 的类型当作成功。当前只保留显式类型以及原生合法 EtwEventResource；RenderTarget 抽象类型45不合法。原生证据为 `core/uce/generated_resource_factory.cpp:19–350`，default 返回 WGXERR_UCE_MALFORMEDPACKET，成功分支 AddRef；不是依据 hash 推断 ABI。

## 有限完整分母

范围1–97共97项：76个原生 factory case +21个非case。0和98及更大值不属于合法资源类型。

非case：2,5,6,10,12,15,19,21,26,28,35,44,45,58,60,67,74,76,79,86,93。

合法case：1,3,4,7,8,9,11,13,14,16,17,18,20,22,23,24,25,27,29,30,31,32,33,34,36,37,38,39,40,41,42,43,46,47,48,49,50,51,52,53,54,55,56,57,59,61,62,63,64,65,66,68,69,70,71,72,73,75,77,78,80,81,82,83,84,85,87,88,89,90,91,92,94,95,96,97。

新增 FactoryDenominator 测试逐项走公开 CreateResource→Close→Commit→ReleaseResource(deleted=1)→Close→Commit，共76例全部通过。该项证明候选工厂接纳/释放，不证明每类全部绘制消费者，也不证明42符合原生延迟创建语义。

21个非case真实候选提交均返回88980403；4个长度及3个碰撞/删除错误测试也通过。旧Red来自用户交接，旧f32e3原TestResults未保全，本轮不声称已取得旧TRX。

## 运行绑定与本地证据

回收路径均相对于工作区 `.coding-agent/sandbox-results/`，每任务显式 `--results-directory Results`，TRX和identity同目录。

| 任务 | RunID | 结果 |
|---|---|---|
| 7fa70b5f7e704ae59ef718ebd47684f9 | 9ce0256d-3f3e-40ac-8d58-26e4c789b429 | related 76/76 |
| 882ab6021e6e46c6950a5ff75ce753a7 | 3057f88f-181b-466f-a864-79807d819cec | full一次1507，1474 pass，33 fail，0 skip，退出2 |
| 70f9d1258d284cb38a6d208fa4f440bb | 5ce8e5dc-d026-40ab-a163-ebcfdb72ec67 | 增加76分母测试后related152/152，0 skip |

三份identity均读出上述生产SHA；full和最终related Counters已读取，但TRX所有内容尚未逐段完整读取，不冒称完整核读。full发生在新增分母测试之前，生产相同但不是最终测试fixture全量。33项HW/cache/shader、原版/native control真实失败保留，没有盲重试，不全部归因环境。

普通Tests实际执行4225：4224 pass、1 fail，指定10.0.12 wpfgfx DLL或依赖8007007E，非改预期消除。专用测试入口没有生成本轮普通TRX，不能提供其RunID。GenProto实际34/34，RunID `38095045-66a0-4934-aca1-b7d97f40e484`，原TRX位于该项目TestResults的2026-10-15_22_58_21文件。普通/GenProto为托管测试，不是假称加载此AOT SHA的测试。

现有 `artifacts/publicabi-final-package` 独立reference运行：任务a3a963573a9d4c18b72eab2c4e21c3c2，报告Results/dd9d3122f8ca403191ec97afbd5f0f3f/report.json。10.0.1原版SHA94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F；HarnessSha7C4CBDA9F7CAAF71458DCA1CA40B95E507A2FAB14F767A48DB39867EEF97D346。Passed/LifecyclePassed=true，Error=null，AllAbiPassed=false，FixedGolden1012Satisfied=false。运行现有合法协议，不向原版发送unsupported命令。新版PublicAbi publish因缺引用的历史projection golden报告失败；现有包不能冒充新构建或本轮候选差分。

## 原生dispatch、所有权及有限裁决

`htslave.cpp:33–108`：AllocateEntryAtHandle→factory→Initialize→InitializeHandle；失败DeleteHandle并ReleaseInterface，成功将本地引用转给调用者，表持独立引用。候选Create先检查handle/type/重复，factory失败不插表；ReleaseAll及typed引用释放全链尚未成组独立裁决，不能只凭finally宣称无泄漏。

`composition.cpp:1470–1476`明确普通Channel_CreateResource对GlyphRun直接忽略。`1712–1778`显式GlyphRun_Create才按空表创建并ProcessCreate，已有对象要求正确类型。候选ProductionContext.CreateResource直接table.Create，提前创建42，与原生不同；本轮只识别，没有未经验证尾改。真实producer/显式失败路径、typed首错/partial cleanup必须作为下一连续生产块，不能将42从合法factory分母删除掩盖差异。

source：上述工厂与特殊dispatch已直接读；wire：现有4长度测试有限通过，field/enum/pack完整独立ABI尚未裁决；consumer：21拒绝及76接纳/释放已实测，但GlyphRun特殊路径未关闭；physicalmapping：没有本轮独立完整映射裁决。GenProto34通过不替代原required逐条关闭，也不以hash模型代原生ABI。

结论：仅工厂分母/21拒绝及已有COM边界有限交付；完整ProcessCreate、PcoreImageBlurMedia最终候选回归、同SHA最终strict/最终fixture full及独立完整原始报告核读均未交付，不标Closed。四外部authority历史绑定、ARM64 runtime、完整10.0.12、player协议缺项保留。DLL驻留不unload；七阶段与正式总门Red，整个计划继续。
