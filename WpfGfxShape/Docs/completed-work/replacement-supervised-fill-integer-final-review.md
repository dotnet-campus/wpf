# BV：fill integer 最终独立审查与交接

## 1. 最终裁决与本轮边界

**BR–BU 最终没有合格生产修复；完整替换未完成。** BR 有限整数实验通过不认证全部 fill；BT 发现具体 float 阶段缺口；BU 因有限验收未完成选择撤回，不是成功修复。当前原44/10曲线问题仍未生产修复。

主代理仅监督，BV执行独立读取与四文档交接。仅专用工具；无终端、直接/间接脚本、代码/项目修改、构建发布、测试或新full。已读执行规则、完整方案1–120、BR/BS/BT/BU及BQ报告全文（长响应关键尾节补读）。本报告路径均相对用户指定根。AGENTS名称搜索无结果。不是全仓版本控制diff认证。

## 2. BR原始证据：有限通过，不移绑最终源码

共同根 `artifacts/br-fill-integer/`。亲读 accept-oracle-{x64,x86}/Results/br-{x64,x86}-oracle/supervision.json 全38行及 TestResults/child.trx:47–50：两架构均 **5执行/5通过/0失败、Completed、监督ExitCode=0**，其余异常计数0；非timeout/cancel/kill、双流完成、SupervisorError=null。

|架构|task|actual测试SHA256 / MVID|
|---|---|---|
|x64|eb8bff94758b4cb4a8e72d3d8d203987|447C1AB7D5D380C385DF9B881D0DCD62F235F520293CAE203AA1DF792B70CE8F / 5659e331-fffc-4da0-8444-60d6c1dcb3da|
|x86|3ccfdc140f884eb991d16b164e84c81a|4A5A8AA986C0791833D61B1B565A7038D9FD67B966B20EE9D37B2C219FE6DA73 / ad21e91b-2b33-4d87-b746-ccdb85268101|

监督actual runtime均.NET10.0.13。另亲读x64 oracle sample0 evidence:75–99，differences=[]、nonempty=true、官方版本10,0,1326,50504、commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699，测试身份与监督相同。这是测试侧整数折线对官方RTB cubic的oracle，不是生产父矩阵，也不是HW。

亲读十帧comparison全文：x64 `trial-run-x64/Results/curve-an/9b1fa19a674945d59127c22954983441/`；x86 `accept-curve-x86/Results/curve-an/d8425666dd6749c38ab7f15b3e94e163/`。均10帧count0、threshold0、exact=true、hardwareVerified=false。亲读两架构各自 separator-summary.jsonl:65–71、133–134：B/E仍差30，geometry选E、drawing/visual选B，real/line fill均0且非空；D/F仍差8，nested选D、对F8、real fill0。不是全160行审查、全部BGRA重解码或当前源码回归。x64 trial父1/1为BT已核记录；BV未新增核父TRX/数值退出，x86父完成/退出亦不填造。

亲读x64上述双方identity全文：task cf015b94bd184cbe8eb43c4eb0c66dc7；官方SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，候选SHA 2C7529F5BC1DB2BA851DFA58703F2FEDC0815E6FBD96604BE9050B009ACAB57C；requested=actual，.NET10.0.13、X64、RTB软件。

亲读两兄弟comparison全文：`accept-siblings-x64/Results/bm-siblings/8223cfc98c174a718c67465af55a7fd5/`、x86对应 `5cd2614966da4154bebf7a769a596f4a/`。arc/cubic/ellipse/rounded各identity/nested共8 fill均0且非空；8 stroke差依次128/122、108/120、190/200、104/94，两架构一致，整体exact=false。未与旧候选DLL在同程序集/同场景对照，既不能认定BR导致stroke回归，也不能认证不回归。未亲核兄弟父完成/退出。

亲读x86兄弟双方identity全文：task546f30b05d164a228a43778597ba022d，X86/4、.NET10.0.13，测试SHA/MVID同上x86 oracle；官方SHA5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834，候选SHA6A681983165861B1B5C2596EBC35A7C8A4832F5A938C41E4A6C74152185C38F4。其他角色身份沿BT报告，不冒称BV全亲核；所有SHA为实际JSON记录，未重算。

## 3. BT反例与BU撤回后的最终源码

BT Rect=(1,1,2^-20,1)、local M11=1+2^-24、Dx=-1、drawing parent M11=2^20：官方[0,1]对候选[1/16,17/16]是源码数值推导；ellipse右端点有同类缺口。**BT/BU/BV均未实跑该像素反例，没有最终BGRA差数。** rounded构点及多层group阶段仍需有界官方运行，不能用进一步源码猜测代实跑。

BV重新全文搜索 Code 与 Tests 的 fillFloat|SoftwareFillBezier 均无匹配；亲读生产空 SoftwareFillBezier.cs 确无内容。`artifacts/bu-fill-float/SoftwareFillBezier.cs.snapshot`存在并读首12行，保留类实现文本；根.gitignore:60的artifacts/规则覆盖该位置。空路径仍在，不称文件已删除。

亲读 NativeShapeReader Capture及尾段：起点/line保留本地float输入，以transform.Apply输出；cubic交共享 SoftwareBezierFlattener.Flatten(previous, first, second, lastPoint, transform)，输出再Apply；有限性、figure ABI、clip及TryCreateFigures保留。无TryDevice、BR整数细分调用。GeneratedNativeStroke全文确认TryCaptureFill入口保留但无fillFloat参数；group调用、递归、rectangle/ellipse分支和parent变换已读，仍共享double Apply后存float；SoftwareStrokeContours:1–65确认CaptureSegment恢复该顺序。curved primitive本地transform仍Apply，未被BU补成float合同。

BU报告列四个既有文件精确撤回及新增实现清空，BV所读调用链支持“BR新增挂接/参数已无效”。没有修改前整文件快照、历史字节hash或全仓dirty清单，**不能认证所有源码历史字节恢复，也不能无条件认证未误及他人**；仅认证上述行为范围及既有TryCaptureFill/共享stroke链仍在。BV未作任何代码修补。

BU双架构生产Release publish成功/Exit0、CA1416警告仅引用BU报告：本轮独占根无归档.log可读，BV没有独立取得发布工具日志。BV亲列 `artifacts/bu-fill-float/publish/{x64,x86}/wpfgfx_cor3.dll`及pdb存在；存在不独立证明退出0。无BU业务TRX/监督/actual module身份，不把BR旧SHA移绑BU产物，发布不是业务回归。

## 4. 保留测试、实验与编译风险审查

保留 BrFillIntegerTests.cs：BV全文197行已读，默认TestClass/TestMethod五DataRow；IntPoint/Basis/Flatten/Adjust均在测试内部，无生产SoftwareFillBezier引用。nonempty仅落盘、最终只断差数0，仍是测试有效性边界；STA Join超时不取消线程也保留，未无证指控5例挂死。

保留 BmFillControlPointTests.cs：internal显式实验；NativeRun先--supervise-exit，再--bm-fill-control-points，await成功才return0；原16场景、nonempty/threshold0/exact断言仍在。保留 CurveDeviceQuantizationExperiment、CurveCausalityExperiment、RoundedRectangleContractTests、CaptureFreshFrame、ExitDiagnostics以及VisualCache/historical的BL/BN Stage/HRESULT诊断。后者沿BQ清单，非本轮完整dirty清单。

Code/Tests全文名称搜索无fillFloat/SoftwareFillBezier引用，加上Br测试自含实现及ComAcceptance项目无生产源码链接，**未发现因空生产文件造成的悬空类型引用编译风险**；空.cs本身不是缺失类型错误。不等于BV编译过两个测试项目或全部配置；BU只发布生产，最终测试构建仍待验证。若后续发现真实编译失败即阻断，不猜修、不将归档包通过充作当前编译通过。

## 5. BS原生门失败：没有进入surface合同修复

BV亲读三份监督全文及两完整TRX结果：`artifacts/bs-surface-present/native-x64/bs-native/` task18d9a6b20f8444d8adfe5f11de8556a1，30秒timeout、kill attempted/returned=true、Exit=-1，无完成TRX记录沿BS；不是原生崩溃证明。`native-x64-complete` task5693314defb646fbab11ed92f55b56c1与`native-x86-complete` task3863b247efc74696b9b194e5cddd1c39均 **2执行/0通过/2失败、退出2**，非timeout/cancel/kill；TRX分别210–218、201–209。监督双流完成且无SupervisorError。

actual测试身份：x64 SHA3618466036359A53BC1030F6F3BB949B7BBEA8304E519A185EB41466B5441FBC/MVIDbb4fb7f5-a654-452a-9b3b-defec0e1a303；x86 SHA83B35C8205BF24B541FE663FA7BCBED3DB75ED3655FEB6B1089A35E93DB89DDD/MVID51a184a4-864c-4499-8282-c5e2c4e04db8，.NET10.0.13。这是BP既有测试包复跑，不是BU生产或BS新测试。

BV亲读x86 bj-fresh-1.json全60行和x64首24行：effect1 expected/backbuffer红蓝红、Present0；x86三帧duplicated0、client/screen13348487(00CBAE87)、front4287082187(FF87AECB)、accepted=false；x64首红同样。effect3及x64后两帧沿BS报告，未冒称四业务JSON全文亲核。原生backbuffer变色且Present成功不等于前台新帧。未进入sampler connect/异步surface合同修复或业务cleanup验收。保留BJ历史成功反证，不断言Basic Render Driver不能产帧，也不把系统原生门当物理GPU/候选内部backend证明。

最终VisualCache:77–86亲见原生前置→connect(1)，123–124及404–405初始/mutation PresentPacket仍无Event。同步Event0 Present偏差、E_HANDLE及首次堆损坏仍开；BL/BN退出细节沿BQ，BV未新读其原始dump/全阶段，不回填BD历史根因。

## 6. 下一执行序列与不变的未闭合同

1. 先将fill专用float阶段完整传到curved primitive（rectangle/ellipse/rounded构点和本地变换、每层group保存），以BT窄矩形/ellipse反例作真实官方对照，保留默认stroke；不是直接恢复旧BR挂接即交付。
2. 用旧/新候选DLL、同一实验程序集及同场景取得stroke基线，分清既存差与新回归。之后才作整数fill有限双架构回归：oracle、原十帧、B/E30、D/F8、负半格、步长边界、兄弟非空、边界/大曲线/HFD/clip及后续有效绘制；验收合格后整合，不以源码相似代运行。
3. 异步surface先稳定同任务原生fresh门、对照BJ成功与BS失败的窗口/桌面交付，再合法初始化、异步channel、事件原柄/duplicate成功失败唯一所有权、重复内容及在途cleanup；官方/候选双架构须正常退出0。不跳过DestroyChannel、不改阈值追绿。
4. 无信息full不再跑；限定修复稳定后最后源码对应产物的Tests/ComAcceptance×x86/x64 Release完整适用四格仍必须补。BD旧x64 **1740/1699/41、退出2**，x86无父full完成TRX保持BQ历史裁决，BV未新核BD原包，不拼局部绿/历史full。

真实GPU/物理GPU、真实device-loss→reset新色、持续媒体、长期CPU/GPU/资源/线程/callback及异步关闭、通知有效target同步/非零SyncModeStatus/迟到消息、GPU圆角、合法大shader内容差等合同全部仍开。overflow等未验收不能无证写成已发生算法错误。四文档完成只完成审查归档与交接，绝不是生产修复或完整替换完成。
