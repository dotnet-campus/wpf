# CW 已回收 fill gap 证据独立审读

## 1. 裁决与边界

**已有归档有限运行门足以支持进入当前源码三文件精确恢复阶段；不是当前生产已修，更不是完成替换。** 不以未读文档新增泛化运行门。CW仅专用工具审读CV/CS已回收原件及更新五文档，无终端、直接/间接脚本，无构建、发布、运行，无生产/测试/项目修改。主代理仅监督。
已读执行规则27行、完整方案120行、当前基线20行、CV初稿9行、CU报告（截断另补40–60）及CR原修改清单21行。保留历史失败和CT撤回事实；CF有限curved修复仍在。

## 2. CV x64 同包原件

根 `artifacts/cv-fill-x64/run-x64-02/`，task `0ac894bf47e44a7e984a683fec57d11d`。父 `Results/cv-x64/TestResults/child.trx:100–103` Counters total/executed/passed=12，failed/error/timeout/aborted/notExecuted=0；不是必要17兄弟。父监督1–38全文：exited/Exit0、OutputCollected=true，无timeout/cancel/kill/drain timeout/SupervisorError。
四组fill各official/candidate/official-repeat，以及stroke baseline/candidate，共14角色：各TRX末23–26亲读1/1/1/0（frames official为全文26行）；各监督25–38亲读同测试SHA/MVID、exited/Exit0、无timeout/cancel/kill/drain timeout/error。没有声称14份TRX及监督每一行均全文审计。角色identity各14行全文亲读，requested=actual。

|身份|SHA256|
|---|---|
|CF旧x64 Old/Native|36353DDE3BA720B642A6F12CEE8AB0EDDA501BD7DC49F35270063480B8A0532A|
|接入候选x64 Native|AB5655A1A567CE586DF31F71FC132E461FE761B2E3573BE51577B8888C60073F|
|official及official-repeat|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|
|测试程序集|FD10C8155A236ED845D20965F1BBDD604D539470811AFF0A8F325F6E5780C309|

测试MVID `da2444a0-5ea1-429b-9074-ab706faed74c`，.NET10.0.13/X64/pointer8。官方版本 `10,0,1326,50504 @Commit:4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。software RenderTargetBitmap，hardwareVerified=false，不代表HW覆盖。

完整comparison亲读范围（非scenes摘要）：stroke1–87；frames1–40/41–100/101–108；precision1–28；siblings1–60/61–120/121–168；device1–55、56–105、106–155、156–205、206–255、256–305、306–355，随后356–415起连续60行分段直到1676–1735，最后1736–1788。无跳段，178行场景全部differences=0/repeatDifferences=0/nonempty=true/accepted=true；四组threshold=0/exact=true/discriminating=true。合计fill206=10+178+2+16。stroke16完整缓冲差0且非空，unchanged=true、baselineIsCurrentSnapshot=false，实际旧新SHA不同；不是CR早期同快照重复。只核记录，不声称逐像素重算或独立重算buffer SHA。

## 3. CS x86补读与复用

根 `artifacts/cs-fill-acceptance/final-run-x86/`，task `cc18c3b74fad4a7c94ef499607fd62e3`。CU已核父17/17 Exit0（CQ5+CR1+BW1+CK10）、stroke16全文及frames10/siblings16；本轮复用，不重跑也不冒称全部重新审计。
device comparison1–1788按1–60、61–120起连续60行到1681–1740，最后1741–1788全文亲读；178场景差0/repeat0/非空/accepted，threshold0/exact/discriminating。precision comparison1–28全文：ellipse/rectangle均差0/repeat0/非空。candidate ellipse.json/rectangle.json各1–30全文：rect(1,1,2^-20,16)、local M11=1.0000000596046448、OffsetX=-1；parent M11=1048576、OffsetX=8；32×24/stride128及alpha记录非空。未解码重算alpha或每像素。
device candidate与stroke baseline identity各14行全文亲读：旧 `C4D7D4699F6334AC164DE95573D382807803ED65E4FE42AA05A9559FEA7F44EB`；新 `FD8B32FAF8D55CF72001DE9ACFE18A57E0814D92C38F63C4FE1D7D45047CD522`，实际不同。测试 `B04139CA3BAE532E722F090E6759C48E59B61870A2195D79305CD537B3EF694B`/MVID `9ca00196-7748-40a6-926c-dcb22a761fa5`，.NET10.0.13/X86/pointer4。官方 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`（CU及Br原件），同上述官方版本。结合CU已核范围，206有限comparison阅读缺口已补，不移绑撤回后源码。

## 4. 双架构必要17兄弟

CV根 `siblings-x64-03` task `dc925678e7034d4497714c4837a12a3e`、`siblings-x86` task `e0da0b00e4af4ea885ae8c5dca32aa7c`。各 `Results/cv-siblings-{x64,x86}/supervision.json`1–38全文、父TRX110–122亲读：17/17/17/0、Exit0/exited、输出回收、无timeout/cancel/kill/drain/error。父结果8–24分段补读识别Br5及PublicGeometry12：normal/empty/degenerate/nonfinite/curves/complex/winding/transform/stroke/relative/callback-reject/null-callback，均Passed；不是CS的17。
Br各0–4的evidence关键末节亲读：sample0/2/4为87–99，sample1为39–51，sample3为595–607；x64 sample0另全文1–99。differences=[]、threshold0、nonempty=true，各自测试SHA/MVID和官方SHA如上。算法在测试程序集执行；Br没有输出候选模块actual身份，不能从aot-identity推断Br消费Native，也没有独立official-repeat身份，不能造出。未全文复核所有trace，更未重算像素。
PublicGeometry各12业务JSON身份通过原件搜索逐文件定位6/11行，均官方AD1C9…/候选AB5655…（x64）或官方5EBD18…/候选FD8B32…（x86）；aot-identity各全文确认架构与候选路径SHA。normal业务原件亲读x64 1–60、490–525，x86 1–35、490–525，flatten闭合矩形点(0,0),(10,0),(10,10),(0,10),(0,0)，arc末数值10,10，receiver-error HR0/value0，callback为void Stdcall本地拒绝记录。其余业务大数组未逐数值全文再审：17通过属于父断言及记录身份下既有有限运行门，不夸称24份业务全数组独立重算。路径/版本/SHA记录不等于每个业务新增实际加载模块枚举；缺少的actual-loaded身份不能从路径凭空推断。此审计层级限制不是新增泛化重跑门。

## 5. 当前源码与精确恢复范围（只读，未修改）

CR原清单及当前代码亲核：
1. `Code/WpfGfxShape/Abi/NativeShapeReader.cs`全文1–76：当前有限float输入检查在，cubic第59–60行共享double Flatten后Apply。恢复范围仅Capture中的DevicePoint/TryRasterPoint及28.4 SoftwareFillBezier入口、line/cubic设备量化与有限/范围失败处理，替换CR对应段，不覆盖整文件、不影响stroke共享算法。
2. `Code/WpfGfxShape/Core/SoftwareStrokeContours.cs`第14–41行：当前TryRead无fillLocalFloat，CaptureSegment先double Apply再float。仅恢复默认false的fillLocalFloat选项及普通path在geometry局部float保存/乘加的分支；非fill保持既有stroke、allowNonFinite和gap语义。
3. `Code/WpfGfxShape/Core/SoftwareGroupStrokeContours.cs`第75–78行：ordinary path仅传preserveGaps。仅恢复该调用按fillPrimitive选择fillLocalFloat。现有fillPrimitive参数、group/combined递归及rectangle/ellipse传递保持，CF curved修复不得回退。
SoftwareFillBezier既有实现保留，不列为此次恢复文件；GeneratedNativeStroke与SoftwareCurvedStrokeContours的CF既有修复保持。恢复前应以CR原变更精确核对应段，避免把测试控制或整个旧文件覆盖回来；本轮未执行恢复。
CrFillExactIntegrationTests.cs全文1–59：整个类在CS_FILL_ACCEPTANCE显式隔离；TestInitialize仅找Old/Native或祖先cr-fill-exact/baseline-x64归档并校验36353D…，缺失Assert.Fail；default不变。历史档依赖仅实验，不让正式验收依赖远程task；后续正式包应本地显式携带同架构已核baseline，并保留真实old/new不同SHA断言、非空、repeat及完整缓冲零阈值。

## 6. 下一最小动作与未闭

可以进入三文件源码精确恢复，不需为已核归档矩阵重新跑一遍。本轮没有把归档通过归给当前源码。恢复后的源码/产物身份发生变化时，才按实际影响补相应双架构有限接入证据；禁止直接把AB5655/FD8B历史TRX绑定新产物。现有CV回收本地TRX确实存在；沙箱绝对路径当前不可直接读取不等于原件丢失。
原44/10问题在当前撤回源码仍开（无新运行不称已重现）；nested/stroke既有差异、最终稳定Tests/ComAcceptance×x86/x64 Release四格full，以及真实GPU内容、device-loss/resize新帧、持续media、长期资源/线程/callback、通知/Event0/E_HANDLE等历史关键缺口不因有限fill通过关闭。保留历史、没有新设泛化门。五文档更新并分段全文回读至末尾；文档核验不是新运行证据。
