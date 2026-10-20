# 当前唯一恢复入口

## CO接管CN：安全撤回完成（最新优先）

**CN本轮未核fill生产挂接已精确撤回；不是集成验收通过，完整替换仍未完成。** 详见[CO](completed-work/replacement-supervised-fill-integration-acceptance.md)及已补正[CN](completed-work/replacement-supervised-fill-integer-integration.md)。NativeShapeReader恢复共享Flatten与Apply；path fillLocalFloat及传参撤除；CF的fillPrimitive/curved实现、SoftwareFillBezier与BW/CK默认回归保留。仅CN新探索测试受CO_FILL_INTEGER_EXPLORATION隔离，未Ignore/skip、未改并行或既有历史回归；移除separator/be豁免，原断言和差异输出保留。

撤回后生产/ComAcceptance项目级Release win-x64、win-x86四次专用build均成功（生产各1479warning/0error，最后ComAcceptance x86 98warning/0error）；工具未单列数值exit，不填造。首次solution RID构建NETSDK1134后已改项目级成功。无新生产发布/运行/四角色验收，不能移绑旧SHA。记录artifacts/co-fill-integration/withdrawal-validation.md。

CN旧x64默认父TRX 11/11、监督Exit0仅BW1+CK10，测试SHA291AEF12…/MVID d5014306…；CK sample0实际官方而非候选；BW业务身份当前回收缺失，不外推集成。原十帧/B-E/D-F/负半格/兄弟fill/stroke完整缓冲及nested18新集成矩阵未做，下一轮从保留CF基线重新有限接入，不能恢复整套未验收补丁。GPU/loss/media/resource/notification等开放项及最终双项目×双架构Release四格仍开。无终端/脚本、无full、新Host、禁改文件或打包修改。

## CM CK整数实验与CL所有权边界（最新优先）

**CK最后正式包双架构有限算法控制通过；未接入生产，完整替换未完成。** CM执行，监督者仅监督。详见[CM](completed-work/replacement-supervised-integer-ownership-review.md)与补完[CK](completed-work/replacement-supervised-fill-integer-reconstruction.md)。CF有限修复保留；本轮无代码修改/生产重发、无终端脚本、full或desktop重跑，以下历史保留。

- x64 task1b773132806d4a749087456fba3c2080、x86 task44924e3a4a2d415eb23ca3320966d340，BL监督各Exit0/exited，父TRX各10/10/10/0全文亲读。整任务回收 `artifacts/cm-integer-ownership/{x64,x86}/`；固定绝对TRX及测试SHA/MVID见CM，不再沿用最后修改前旧10/10。
- 7个官方cubic→重建整数折线样本每架构非空零差，负半格等3舍入通过；14份业务结果/实际身份末节已亲读，全部trace/像素未独立重算。实际.NET10.0.13/官方10,0,1326,50504；算法在测试程序集直接编译执行，官方模块不是候选。Native输入SHA不构成加载/消费证明。
- SoftwareFillBezier虽在生产目录但无消费入口，NativeShapeReader仍共享Flatten；默认oracle仅算法有限控制，不关闭原44/10、nested/stroke。未另认证源码包逐字节溯源。
- CL源级UAF独立核关键官方行：失败delete B后Commit跳过清槽，Destroy间接再次Commit可读悬空槽；再次free仅条件性。缺匹配servicing源码/编译配置、BN历史模块及dump，不能认定BN 0xC0000374根因。候选先dequeue/detach非同构，不修改候选。

**下一步基于当前通过算法补父设备矩阵/普通本地float阶段，再真实fill入口限定挂接**，同程序集旧新官方repeat非空零阈值与stroke不变；不能因旧BU归档丢失停工，不覆盖CF三文件。原44/10、nested/stroke、BD旧full及x86缺父TRX全部保留且不绑新身份；GPU/loss/media/resource/notification及同步Event0/E_HANDLE/首次堆损坏等合同仍开。最后稳定Tests/ComAcceptance×x86/x64 Release完整适用四格仍缺。五文档收口不代替替换验收。

## CH CF/CG独立验收（最新优先）

**CF三文件有限生产修复保留，CH独立有限验收通过；完整替换未完成。** 详见[CH](completed-work/replacement-supervised-curved-precision-acceptance.md)、授权补完[CF](completed-work/replacement-supervised-curved-precision-delivery.md)、[CG](completed-work/replacement-supervised-dwm-delivery-boundary.md)。主代理只监督；无代码/阈值/skip修改、无终端脚本、新Host/项目或生产重发。以下CE及更早均为历史。

- CF accepted双父各2/2 Exit0及八四角色子各1/1 Exit0、十监督、八actual identity和两完整224行comparison已亲读；同架构同测试程序集旧新完整像素比较，不只同差数。窄rect64→0、ellipse32→0，其余16完整输出SHA及oldNewDifference均未变，全部非空/官方repeat0。nested ellipse/rounded各18及stroke162/128、106/60仍差，整体exact=false。
- 新候选实际SHA x64 `36353DDE3BA720B642A6F12CEE8AB0EDDA501BD7DC49F35270063480B8A0532A`、x86 `C4D7D4699F6334AC164DE95573D382807803ED65E4FE42AA05A9559FEA7F44EB`。fillPrimitive默认false，仅TryCaptureFill传true；stroke调用未改变。group double组合、rounded先double构点、大曲线/极值和fill+pen未全面验收，不外推完整float/整数fill/group。
- 复用CF默认包BL监督实际BW：x64 task0cd630afcfef4ba59c41479dd57f2f60、x86 task61f052b5470e40f182f5992452b1cd09各1/1 Exit0；默认兄弟Br五行+PublicGeometry十二行：x64 c9aa1c3f8bae4ff0b972143585155f6b、x86 0f26ad1ad51c43c9b8bdfc756e489e7b各17/17 Exit0。BW actual新候选身份及默认测试SHA/MVID亲核，独立官方repeat零差非空；非显式取证替代默认。全任务回收 `artifacts/ch-curved-acceptance/`，绝对父TRX路径见CH。
- CG不新跑硬件；补亲读最终只读三帧完整726行JSON。两控制2/2失败Exit2、只读1/1失败Exit2、选择谓词4/4 Exit0仅筛选通过。paint/erase未变不证因果，30次Acquire全timeout无像素，官方依赖非候选，x86未跑，BJ成功反证保留；旧两控制完整JSON本轮按CG记录引用。

原44/10、BD旧x641740/1699/41 Exit2及x86full无父完成TRX不绑新SHA。GPU/loss新色/持续媒体/长期资源线程callback关闭/通知/圆角/合法大shader/同步Event0/E_HANDLE/首次堆损坏均开放。下一步以本次有限修复为保留基线继续定位nested/group分阶段精度及交付链，不盲撤修复。最后稳定Tests/ComAcceptance×x86/x64 Release完整适用四格仍需交付；六文档全文回读不是完整替换。


## CE nested / delivery 最终独立收口（最新优先）

**当前双架构软件基线已建立，无新增生产修复，完整替换未完成。** 主代理仅监督；CE仅专用读写亲核，不改代码、不构建发布或新跑，无终端及直接间接脚本。授权补完[CD报告](completed-work/replacement-supervised-current-nested-baseline.md)，独立范围见[CE审查](completed-work/replacement-supervised-nested-delivery-review.md)。本节优先，下文保留历史。

- CD亲核8TRX全文、8监督全文、6 actual identity及双架构完整18行comparison：父各1/1、三角色各1/1且全部Exit0仅是取证完成；18场景非空、官方重复0差，candidate exact=false，非产品/HW通过。
- 当前实际候选SHA x64 `F45485F1C747361D89E31FDB667A2C8C1C4920CE6BD6982A6AE93532C2E5A03C`、x86 `09CD6C360304E02AEFF873BFDCB082CEC8CFC55D0736238C81E378EF1D81FABB`，requested=actual且非官方。nested ellipse/rounded各18、窄rect/ellipse64/32、stroke162/128及106/60，两架构一致。
- CB_ARCHIVED_FILL_DIAGNOSTICS与CD_CURRENT_FILL_DIAGNOSTICS已亲核显式隔离；BW默认精准失败仍保留。默认发布日志各97warning/0error、CD显式各101warning/0error亲核；完整默认发现清单未取得，不报总数，不声称默认BW已跑。源码亲核清单及身份边界见CE/CD，不造整体git hash。
- CB x64历史旧新nested18不变、x86归档缺失按报告引用。当前本地 `artifacts/cd-current-nested/` 可复用，不再寻找消失归档。CC两假设2×2均失败、官方打包依赖非候选、未跑x86、父TRX缺口均按CC报告引用，非CE原件亲核，更非业务硬件通过。BJ历史成功保留。

**下一步用CD当前双架构old基线恢复限定curved精度补丁**，同架构同一新测试程序集old/new/官方重复，窄反例非空零差、nested及stroke完整输出无新增回归。nested18已是当前既有差，不能一见失败就撤补丁；不同时恢复整套整数算法。旧44/10、BD旧x64 full1740/1699/41 Exit2及x86缺父完成TRX保持，不移绑CD身份。GPU/loss/持续媒体/长期资源线程callback关闭/通知/圆角/合法大shader/E_HANDLE/堆损坏全部开。五文档写入全文分段回读只是文档收口，最后稳定源码Tests/ComAcceptance×双架构Release完整适用四格仍缺。


## CA precision / delivery 最终独立收口（最新优先）

**BW–BZ无合格生产修复；完整替换未完成。** 主代理仅监督，CA只用专用工具亲核原件及修改五文档，无代码改动、终端/直接间接脚本、生产重发或无信息full。已先补完并全文回读BZ独有报告；完整证据范围、identity和限制见[CA独立审查](completed-work/replacement-supervised-precision-delivery-review.md)，下文全部保留为历史。

- BW亲核双架构BR/BU rectangle64/ellipse32非空反例；同程序集八项stroke逐字节未变但仍对官方有差。x64补强五份oracle nonempty/零差、stdout5/5与监督Exit0；父TRX未找到，2/1/1和7/6/1计数只认stdout。
- BZ归档双架构窄rectangle/ellipse各0差；nested ellipse/rounded各18差，stdout各2/1/1、监督Exit2。父TRX本地未取得，不能冒称亲读；无旧新同程序集nested基线，18差不能判新增回归。当前fillPrimitive/ApplyFillLocal已撤回；只核NativeShapeReader及curved fill所读行为，不认证历史整文件hash。BzCurvedPrimitiveTests internal无发现属性、无显式入口，属未接入实验，不删改/skip现有回归。
- BX五父TRX亲核依次2/0/2、2/0/2、2/0/2、4/0/4、4/0/4，均Exit2。final-x64红backbuffer/Present0而前台仍GDI绿、duplication未fresh；诊断默认隔离与WndProc同步I/O撤回保留，非生产修复。BJ历史16/16成功保留，不断言Basic Render Driver永不能产帧。
- BY CA2208参数重载修复保留；四份日志各92warning/0error，双架构控制TRX各1/1监督Exit0亲核。未见本轮WarningLevel/NoWarn压制；SDK既有nowarn不混为新增。default1758无GDI/diagnostic1760含GDI为BY发现输出报告引用，CA无落盘清单可全文亲核。CA未新编译最后源码，旧成功不作当前全部编译认证，未见所读范围明确悬空引用风险。

**下一最小任务：先旧新DLL同程序集nested基线确认18差是否新增，再float分阶段、本地保存/group来源和整数fill集成，不盲恢复撤回。** 异步surface先真实fresh前置，再真正事件唯一所有权、重复内容/cleanup及正常退出。44/10、BD旧x64 full1740/1699/41退出2及x86缺父完成TRX保持；最终稳定源码双项目×双架构Release四格仍缺。真实GPU/loss新色/持续媒体/长期资源线程callback异步关闭/通知/GPU圆角/合法大shader、E_HANDLE和首次堆损坏仍开。五文档落盘全文回读不是完整替换完成。


## BV fill integer 最终独立交接（最新优先）

**BR–BU最终没有合格生产修复；完整替换未完成，44/10仍未生产修复。** BV仅专用工具读取及四文档更新，不改代码、不构建发布运行、无终端/脚本、没有新full。主代理仅监督。亲核路径、身份及限制见[BV最终审查](completed-work/replacement-supervised-fill-integer-final-review.md)；下文BQ及更早内容保留为历史。

- BR原始监督+TRX亲核双架构oracle各5/5、Exit0；双架构十帧comparison全0，separator指定切片B/E30、D/F8仍有判别力，real fill0。兄弟各8 fill0且非空，但8 stroke仍差128/122、108/120、190/200、104/94；没有同程序集旧/新DLL stroke基线，整体exact=false。BV没有重解码全部像素、核全部父退出或移绑最终源码。
- BT rectangle/ellipse float反例仅源码推导，未实跑。BU因有限验收未完成撤回BR生产挂接及fillFloat，不是成功修复。BV搜索Code/Tests无fillFloat/SoftwareFillBezier引用，生产SoftwareFillBezier.cs为空，snapshot在ignored artifacts。NativeShapeReader恢复本地控制点→共享double Flatten→输出Apply及TryCreateFigures；只认证所读行为范围，无历史字节hash/全仓diff认证，不能保证撤回未误及他人所有改动。
- BrFillIntegerTests自含整数实现、默认五例；BmFillControlPointTests仍显式同exe实验，16场景及非空/零阈值/exact保留。连同既有曲线、CaptureFreshFrame及BL/BN诊断保留，未发现引用空生产文件导致的悬空类型风险；BV未构建当前测试项目，不作全编译认证。oracle非空仅落盘仍是边界。
- BU双架构publish成功/Exit0仅引用BU报告；BV亲见DLL/pdb存在，但未取得归档发布日志，无业务回归/新actual身份。BS亲核x64一次30秒timeout kill，随后双架构各2/0/2、Exit2；backbuffer变色/Present0而前台旧色，未进入surface合同修复。BJ历史成功保留，不断言Basic Render Driver不能产帧。同步Event0 Present偏差、E_HANDLE及首次堆损坏仍开。

下一精确顺序：先将fillFloat完整传到curved primitive/每层group并实跑BT反例；同程序集旧新DLL stroke基线；之后整数fill有限双架构回归合格再整合。异步surface先稳定fresh门，再合法事件唯一所有权、重复内容/cleanup及正常退出。不能以进一步源码猜测代实跑，不再无信息full。

BD旧x64 full1740/1699/41退出2、x86无父fullTRX不变；最终稳定源码双项目×双架构Release完整适用四格仍缺。真实GPU/device-loss新色/持续媒体/长期资源线程callback异步关闭/通知/GPU圆角/合法大shader合同均开。四文档全文分段回读只完成交接，不代表生产已修。


## BQ curve / capture / exit 独立交接（最新优先）

**完整替换未完成；BI–BP 最终无已验收生产修复。** BQ只读审查及四文档更新，无代码修改、构建发布运行、终端或直接/间接脚本。亲核路径、actual SHA/MVID/版本/阶段及报告级限制见[BQ独立审查](completed-work/replacement-supervised-curve-capture-exit-review.md)；下文BH及更早文字保留为历史，不覆盖本节。

- BI：B/E官方差30，geometry匹配E、drawing/visual匹配B；D/F差8，nested匹配D而非本次F。原归档frame1/3/5各44、frame9为10仍失败。真实fill链经过NativeShapeReader，不能只改备用SoftwarePathCoverage；来源在type=2丢失，分阶段float与整数细分未实现。
- BJ：亲核x64父16/16及同任务native→official→candidate三个public阶段；x86 gate17/17，但public无父完成TRX，退出机制未知。Basic Render Driver实际red/blue/red推翻旧绝对不能产帧结论；非owner对照也通过，owner根因未证。无数值退出证据不冒填0，非物理GPU/候选内部backend验收。
- BL/BN：亲核官方sampler BL退出0xC0000374，非timeout/cancel/kill；BN双架构首次Commit E_HANDLE后DestroyChannel期间同状态退出。仅新复现，不回填BD历史full根因。同步Event=0合同偏差明确，首次堆损坏位置未闭。
- BM：试验双架构原十帧各1/1全零差，但自己geometry30/nested8失败；NativeShapeReader预量化补丁已精确撤回。只认证所读行为恢复，不是历史整文件hash或恢复后二进制回归认证，绝不写生产曲线已修。
- BP：无surface最小合同4/4退出0；真实sampler原生像素前置失败、尚未connect。BO未验收行为/新默认合同已撤回，BL/BN诊断及历史默认回归保留；最终同步Event=0问题仍在，撤回不是修复。BM保留16场景/非空/threshold0/exact及历史失败，改同exe显式--bm-fill-control-points；显式x86仍失败、无独立数值退出。最终双架构路径控制各1/1退出0、发布成功不等于sampler/full。

BD x64 1740/1699/41退出2、x86无父full完成TRX仍为旧快照基线；不拼BK分组、BC历史full或BP新控制成最终矩阵。保留的测试/诊断/实验清单及身份见BQ，不将BM试验候选SHA或BD旧候选移绑最终源码。

下一最小任务：正确fill分阶段float/本地保存/设备28.4控制点及整数细分，保留来源和兄弟路径；捕获先稳定原生fresh门，再真正surface异步Present事件所有权与双架构重复内容/cleanup/正常退出。无信息full不再跑；限定修复稳定后最后源码两个正式项目×双架构Release完整适用四格full仍须交付。

真实物理GPU、loss-reset新色、持续媒体、长期CPU/GPU/线程/资源/callback与异步关闭、通知有效target同步/非零SyncModeStatus/迟到消息、GPU圆角、合法大shader内容差全部未闭。BQ四文档更新后全文分段回读；文档完成不等于完整替换。

## BH baseline / curve / hardware 最终监督交接（最新优先）

**完整替换未完成；本轮仅审查/文档，无生产修复，无构建发布运行。** 独立范围、全部身份与原始路径见[BH审查](completed-work/replacement-supervised-baseline-curve-hardware-review.md)。BD–BG源码变更只有曲线实验挂接/新增文件及WindowQ夹具；归档曲线候选、BD候选、新测试程序集不同快照，不拼为最终矩阵。

- BD亲核x64原full **1740执行/1699通过/41失败、退出2**；x86 TestResults无父full完成TRX，中断原因、最终计数与退出码仍未知。不得把继承smoke或末用例当full/根因，不无限重跑。
- BF亲核双架构WindowQ定向各 **11/11**，仅修真实尺寸及红蓝绿黄夹具，exact保留；不改写为full40失败或全绿，软件RTB不是HW验收。
- BE报告已更新separator-v4续轮；BH亲读双parent TRX各 **1/0/1、退出2**，comparison仍frame1/3/5各44字节、frame9为10。双架构summary指定切片支持负半格A/B差18、真实geometry/drawing/visual匹配B；正半格C/D差8、真实nested匹配D。不是全图重算或通用算法闭合。最新v4测试SHA/MVID未取得，不挪用旧compact或BF身份；本地float保存再identity栅格、float矩阵合成替代分支及普通cubic/rounded算术未闭。
- BG亲核三父TRX **11/0/11、6/1/5、3/1/2，均退出2**；原始切片显示backbuffer新红、Present/query/DwmFlush完成但duplication WAIT_TIMEOUT/resource0，Microsoft Basic Render Driver。共同前置阻塞不证明BD37项全环境；BD原官方PublicHwnd成功的历史反证保留。

下一最小顺序：补BE最新程序集身份并排除float替代分支、审完普通cubic/rounded上游，证据充分才fill限定生产修复；硬件先稳定可呈现桌面原生red/blue/red fresh内容门，再同task对照官方/候选；x86中断只做有新信息的有界诊断。合法大shader差异仍开。最后稳定源码Tests/ComAcceptance×双架构Release完整适用四格full仍缺，不能拼BC历史绿和本轮定向。

Capture WAIT_TIMEOUT/resource0、真实HW/物理GPU、loss-reset新色、持续媒体、长期CPU/GPU/线程/callback/异步关闭、GPU圆角、通知有效target同步/非零SyncModeStatus/迟到消息等合同全部保留。BH仅新增审查及三份共享文档最新节，保留历史、不覆盖BD/BF/BE/BG报告；四文档落盘后全文分段回读。

## BC SwapChain修复及托管full（优先于下文历史）

**本包托管双架构full各4256/4256，完整替换仍未完成；不是ComAcceptance full或最终四格。** 已亲读BA原始full：x86为4254执行/4244通过/10失败且退出2，x64为4254/4254；两份actual identity全文已核，不再把BA称为进行中。BA scene44→45槽见其报告；不追认历史AX终止唯一根因。

BC先用BA现成x86包有界复现十项SwapChain异常（11执行/1身份通过/10失败），再仅在Direct3D9SwapChainTests修索引：固定偏移8与x86 AddRef vtable[1]重叠，移到原已分配14指针后的尾int；不扩大数组/分配，不改原断言。新增0/1两行最小回归；原DoNotParallelize与默认Workers4保留，无skip/阈值/fallback。常规依赖构建及自包含.NET10.0.13 Release发布成功；定向各13/13、Surface/SwapChainRetrieval/Lifetime/Interface兄弟族各95/95、无filter full各一次4256/4256。每次120秒上限，完整回收parent TRX与actual identity并亲读，不用console进度代替。

证据根 `artifacts/bc-swapchain-fixture/`；full在 `full-{x86,x64}/TestResults/bc-full-{x86,x64}.trx:25545-25548`，identity在同目录Results/av-ownership-assembly-identity.json。x86实际task=3cccc43545b242c58961b94bb6bb3604，x64=56231e885e534e938a76a331abb758a3；绝对任务TestResults位于C:\CodingAgentSandbox\Tasks\<task>\TestResults。Tests actual SHA x86=ED48CB1AD26CEB1993144EF70A4773E14BC34BE15C72EB55DDA857CD1E2EA0A8 / MVID a825679d-e07b-4df6-99f4-b62aeeb6bf82；x64=E31CB7FFCBE00D3F7DB6D1FA7438B5941F98C5E69DA5B82E329FC3FBEC6ABA36 / MVID b0221029-074f-4cbc-9010-fb2484052164。生产托管actual SHA/MVID与构建初次CS0006及纠正记录见[BC完整报告](completed-work/replacement-supervised-swapchain-fixture-fix.md)，不沿用BA身份。BC仅改该夹具及指定四文档，未改生产/入口/项目/曲线/其他测试，未重发NativeAOT、未清理别人产物，无终端或脚本。

下一最小顺序：先独立核验BC证据，再继续限定fill设备量化的float/真实WPF变换栈审计与官方零阈值回归；AY原44/10差和完整HFD仍开，AZ AP归档各14/14不绑定BC新SHA、不外推251/251。Capture WAIT_TIMEOUT/resource0、真实HW/duplication、loss→Reset→新色HW消费、持续媒体、长期CPU/GPU/线程/callback及异步关闭、GPU圆角、通知有效target同步/非零SyncModeStatus/迟到消息未覆盖均保留。最后稳定集成源码仍须Tests/ComAcceptance×x86/x64 Release四格完整适用full，不拼旧快照Green。

## BB AX/AY/AZ最终证据交接（优先于下文历史）

**本包五份文档闭环，不是新实跑；完整替换未完成。AX x86全量仍无完成TRX，最后集成源码两个正式项目×x86/x64 Release四格完整适用full仍未交。** 主代理只监督；BB仅专用读写搜索、读证据及写文档，未改代码、未构建发布运行，未使用终端/shell/直接间接脚本。先补完[AY报告](completed-work/replacement-supervised-curve-boundaries.md)，再创建[BB独立审查](completed-work/replacement-supervised-ax-ay-az-review.md)并更新本入口、问题表及汇总；证据路径、身份和亲核界限以BB审查为准，历史状态不再冒充当前结论。

- **AX亲核**：`artifacts/ax-lifetime-fixture-full-20260721/full-x64/TestResults/ax-full-x64.trx:25533-25536` Completed 4254/4254；两份full实际托管identity全文亲读，.NET10.0.13、X86/4和X64/8。x86 TestResults仅部署目录和diag，无parent完成TRX；diag:39310-39315末仅开始scene参数-2005530520，不证明根因、确定timeout或访问冲突。AX生命周期5槽修复及最小两次2/2、族各22/22为报告交接，不再沿用AW“ObjectsLifetime待修4槽”，也不外推最后源码full。
- **AY已补完，非仅初稿**：亲读现成final-audit-readback/output与readback/x86-final、x64-final的summary/TRX/identity。原44/10字节差保留，双架构各1执行/0通过/1失败、退出2；三种预量化干预均0差；负半格14/30差，显式非整数变换double70/object80/device0。无生产修复，float舍入/真实WPF变换栈/完整HFD未闭；显式变换实验不是实际WPF变换栈证据，GPU圆角仍未验证。
- **AZ亲核**：`artifacts/az-reference-revalidation-20260721/final-official-{x86,x64}` parent各2/2，final-revalidate各14/14；两份aot-identity和消费JSON身份/来源段已读，实际引用新官方参考、同版同架构.NET10.0.13。参考只在ignored artifacts，绝不写tracked ReferenceData或改golden/阈值/skip。候选绑定AP归档SHA，不是最终源码；原4 glyph+3 mask+6 RotatedDrawing这次目标组通过，不外推251/251。旧6 RotatedDrawing历史present失败这次未重现，不假称证明原历史根因或全归缺文件。
- **BA scene并行进行中/待独立验收**：只读时文件已出现结束报告文字，但本包未验收其TRX/身份；不把其自报终态当BB已核结果，不覆盖AX x86未完成现场。主代理须接收并核验BA最新结果；不能预判全绿。

### BB下一最小顺序

1. 接收并独立核验BA终态、处理仍失败项，保留原始失败、parent TRX及实际托管身份，不按末用例断根因或无限无信息full重跑。
2. 下一曲线生产实现限定fill设备空间量化，先核float舍入与真实WPF变换栈，再做负半格/非整数变换/原44和10官方零阈值回归；不得向共享stroke盲加，完整HFD无证据不宣称闭合。
3. 稳定集成后，最后源码、正常依赖图完成Tests/ComAcceptance × x86/x64 Release四格完整适用full；取任务内绝对TestResults、parent TRX、actual identity及内容/时序/资源/恢复/异常，不拼AX/AM/AR快照及AZ局部Green。

Capture WAIT_TIMEOUT/resource0、真实屏幕HW/duplication、真实loss→Reset→新色HW消费、持续媒体、长期CPU/GPU/线程/callback及异步关闭、GPU圆角、通知有效target同步/非零SyncModeStatus/迟到消息等剩余合同继续开放。软件RTB、构建成功及文档落盘均不关闭关键门。

## AW 最终监督交接（覆盖下文历史状态）

**本轮仅三份文档收尾；完整替换及最后源码四格full未完成。** 主代理只监督；AW仅专用读写搜索，不改代码、不构建发布运行、不用终端/shell/直接间接脚本，不改AV或其他报告。已读执行规则、完整方案及AS/AT/AU；AP证据沿用[AU独立审查](completed-work/replacement-supervised-ap-aq-ar-review.md)，非AW重新实跑。

- **AP有限修复已完成**：双架构producer各119/119、realizer各78/78、新托管各3/3；宽族各251执行/247通过/4失败，缺glyph-arial-original.json，不能记全绿。旧6 RotatedDrawing与3 original-wpf缺reference未复验，和新4 glyph缺参考分别保留。
- **AS确定夹具越界**：[AS报告](completed-work/replacement-supervised-x86-managed-diagnosis.md)的修复前4槽、vtable=memory+2、写vtable[2]即memory[4]确定越界（x86写偏移16..19，x64为32..39）。AW亲读原`artifacts/as-x86-managed-diagnosis-20260721/ownership-single-x86-1/TestResults/log_261019125747203.diag:85-91`，只见4worker及单测开始后文件结束；`query-x86/TestResults/as-query-x86.trx:83-84`为Completed 11/11。无完成不能推断access violation、确定timeout或AR唯一根因；AS旧包实际托管程序集身份未绑定当前源码。
- **AV已取得结束报告，不再猜进行中**：[AV报告](completed-work/replacement-supervised-ownership-fixture-fix.md)称最小x86两次各2/2（业务1+身份1）。AW亲读当前ownership源码已5槽，以及`artifacts/av-ownership-fix-20260721/interface-{x86,x64}/TestResults/av-interface-<arch>.trx:113-114`各Completed 16/16（业务15+身份1）。报告提供实际托管SHA/MVID，但AW未逐份核身份JSON，不外推最后源码full。x86无过滤full仍无完成TRX；AW亲读其diag:43753-43759末仅开始PixelShader Bool用例，不能以末名认定原因或报全量计数。当前`Direct3D9ObjectsLifetimeTests.cs:143-150`仍是4槽/+2/[2]的确定越界，已亲核，需文件所有者修复；ownership修复不代表唯一根因闭合。
- **AT质量门未过**：[AT报告](completed-work/replacement-supervised-curve-quantization-fix.md)仅现有生产源码双架构常规Release构建成功，无算法修复、无新正式回归；审计传输SocketException 10054失败，未取得审计运行输出。AQ限定实验零差不等于生产曲线通过；原44/10字节差继续开放，GPU圆角仍未验证。

### 下一最小任务

1. 先协调修复ObjectsLifetime同类夹具越界，再以新包定向确认后完成x86托管全量；保留默认并发和失败，取parent TRX、实际托管identity及可取得的终止状态，不盲目重复full。AM/AR旧x64 4249/4249只是快照，不绑定新源码。
2. 缩小曲线审计证据传输到原始像素、完整轨迹、identity及必要结果，重做独立审计；补负数半格及非整数变换顺序的真实官方差分，证据门过后才考虑有界生产修复。
3. 独立官方参考生成仅纯C#、同版同架构、仅ignored artifacts，记录actual identity/原始输出；不写tracked ReferenceData，不改golden/阈值/skip，生成后正式复验。
4. 最后集成稳定后，以最后源码、正常依赖图完成Tests/ComAcceptance × x86/x64 Release完整适用full；回收任务内绝对TestResults、parent TRX、actual identity及内容/时序/资源/恢复/异常证据，不拼旧full或累计局部Green。

Capture WAIT_TIMEOUT/resource0、真实屏幕HW/duplication、真实loss→Reset→新色HW消费、持续媒体、长期CPU/GPU/线程/callback、异步关闭，以及通知有效target同步/非零SyncModeStatus/迟到消息继续开放；软件RTB、构建成功与局部TRX均不能关闭这些门。

## AO 最终交接（优先于AL及更早历史）

**本包四份文档落盘回读；完整替换未完成，最后集成源码两正式项目×双架构Release完整基线仍未完成。** 主代理仅监督；AO仅专用读写搜索，无代码修改、构建发布运行、终端/shell或任何直接间接脚本。详见[独立Bitmap/曲线审查及逐项证据](completed-work/replacement-supervised-bitmap-curve-review.md)。AJ/AM Bitmap、AK/AN圆角报告已有实际完成章节，仅只读引用，未覆盖并行成果；报告完成不等于缺陷全部修复。

- **AJ亲核**：post-x86.trx:2710为191/160/31，post-x64.trx:2336为191/191/0；producer-x64.trx:1085为128/103/25。八份独立Bitmap JSON及actual identity全文亲读：双架构官方/候选非平移红色、平移首像素透明一致，16次S_OK及回调各16，matrix/texture48/164与80/208。“独立各4/4”计数来自AJ/AM报告，AO亲核的是八份业务输出，不冒充单独4项TRX。自然布局producer与consumer共用NativeBitmapBrushLayout，源码已同步；私有Exact尾部/候选手造兄弟夹具Green不能证明全部自产或官方兼容。
- **AM报告已完成、自产未闭**：最小夹具架构偏移修复；AO亲读新am-family-x86.trx:2336为191/191。新x64族191/191、Managed x64全量4249/4249、producer x64新119/103/16来自AM报告及执行摘录，未全部亲核TRX。新x86自产GetState/Ensure崩溃与Managed x86仅3566进度/无完成TRX保留。原x64 25项按报告分为16锁源present、6 RotatedDrawing present、3缺reference，不能全说缺文件；新119项不覆盖后九项。
- **AK软件失败亲核**：实际com-x64-final comparison帧1/3/5各44字节差，首696/(14,4)/36对40，帧0/2/4零差；双方identity实际.NET10.0.1，hardwareVerified=false。AK x86 runtime未启动来自交接，AO只确认该包无Results可读，未亲读启动错误原文。**软件曲线差异与新GPU圆角未验证分开**。
- **AN报告已完成、无生产修复**：边界控制点实验及.NET10.0.13自包含包见其报告；AO亲读AN双架构最终COM TRX各2执行/0通过/2失败。44差及新增10字节差细节沿用AN报告，未复算原始像素；不能混用AK10.0.1身份，也不把AN新运行追认成AK x86曾成功启动。GPU仍未覆盖。
- **AG/AI有限完成沿用AL亲核**：AG当前异步FrameTime响应、AI独立夹具有效/D归档缺陷已发现，不等于一般同步销毁/当前全Bitmap验收；AO未重新核全部AG/AI原包。真实HW、恢复、媒体、长期资源等继续保留关键门。

### 下一阶段（不可无限局部）

集成修复稳定后立即以**最终源码及正常依赖图**构建生产与 `WpfGfxShape.Tests`、`WpfGfxShape.ComAcceptance`，完成x86/x64 Release四格完整适用基线。每格使用任务目录内**绝对TestResults路径**并完整回收TRX、实际identity、业务原始内容/事件/资源/恢复及崩溃输出；原始失败逐项区分生产缺陷、夹具、缺reference/依赖、触发不足/未覆盖和未完成，不能用child TRX或进度计数顶替parent完成。不得拼旧full、累计局部通过率或设无谓hash前置阻塞，SHA只作归属。

真实屏幕HW/duplication帧、loss→Reset→新色HW消费、持续媒体、长期CPU/GPU/线程/callback与异步关闭仍关键，不降门；通知有效target同步、非零SyncModeStatus及迟到消息继续开放。四份交付路径及亲核/报告/缺失边界见AO审查。本节覆盖下文“AJ/AK尚无结果、AM/AN仅进行中”的历史状态，不删除历史失败。

## AL 最终交接（优先于下文AF及更早历史）

**五份指定文档已实际更新；完整替换未完成。最后源码两个正式项目×x86/x64 Release全量尚未跑，不是无需跑。** 主代理仅监督；AL只读代码/现成证据并写文档，未改代码、未构建发布运行、未用终端/shell/任何直接间接脚本。本次没有新实跑，不以旧full或累计局部Green关门。

- **AG限定通过，报告已完成**：[Present序列证据](completed-work/replacement-supervised-present-sequences.md)。亲核 `artifacts/replacement-present-ag-2026/final-readback/com-{x64,x86}/TestResults/ag-final-<arch>.trx:72` 各9/9/0，Managed `managed-{x64,x86}/TestResults/ag-managed-<arch>.trx:70` 各6/6/0；双架构六组FrameTime共12份comparison完整64字节hex一致。只适用于当前CrossThread HWND异步场景；refresh=60不是一般60Hz/VSync证明。抽样events中重复Present信号数不同，替代通道空不能证明旧通道无迟到消息；有效target同步、非零SyncModeStatus及完整销毁合同仍开放。
- **AI夹具有效、归档候选缺陷已发现，报告已完成**：[Bitmap ABI证据](completed-work/replacement-supervised-x86-bitmap-abi.md)。实际根 `artifacts/supervised-x86-bitmap-abi/readback/final-{x86,x64}`；x86 Bitmap4/2/2，官方两输入成功、候选全透明；x64 4/4，兄弟双架构各2/2。亲读八份业务JSON及独立Sequential声明：相对CMILBrush子对象matrix/texture为x86 48/164、x64 80/208，有官方出图/平移行为佐证，不只是布局自洽。D归档非当前生产复现，不代AJ验收。
- **AH未验收，AJ/AK正在修**：亲读现有圆角/AJ报告；AH最后bounds合并未构建，Managed coverage通过不代表GPU；软件RTB官方36/候选40、首差696来自报告交接，AL未亲读其像素原包。AJ Bitmap、AK圆角尚无可验收新结论，不猜结果。
- **保留缺口**：S Capture初始及10次Acquire WAIT_TIMEOUT/resource0；真实loss→恢复→新色HW消费、屏幕HW、持续媒体、长期CPU/GPU/线程/callback资源、广泛随机及异步关闭仍开放。上述旧证据沿用AF/AE/AC交接，AL未重新亲核其原包。

### 下一批监督安排

1. 接收AJ/AK当前候选的正式双架构结果、实际identity/TRX/业务像素及关闭证据；失败保留，不以D归档替代当前复现，不预判修复成功。
2. **本轮集成稳定后即准备最后源码 WpfGfxShape.Tests 与 WpfGfxShape.ComAcceptance × x86/x64 Release完整适用矩阵**；不要无限追加局部绿而延后全量。全量保留真实失败/超时/未覆盖，并回读内容、生命周期与恢复证据。
3. 通知同步/销毁、真实恢复与duplication帧、持续媒体及长期资源分别继续补齐，不以局部响应或RTB替代。实际SHA只用于产物/结果归属，不设为开始工作的前置阻塞，也不把旧结果绑定新源码。

AG/AI核心原始文件已找到，AI路径需含readback；AL没有创建独有新报告。未解码AG原始bin、未重算SHA或重新执行比较器；逐份comparison及八份Bitmap JSON的亲核与identity/events抽核边界均列在两报告。下文“下一包FrameTime/x86夹具尚待建立”等旧状态仅作历史，不覆盖本节。

## AF 最终交接（覆盖下文AE/AC及更早的待验收状态）

**文档收尾完成，完整替换未完成；最后源码两个正式项目×x86/x64 Release四格全量未跑。** 主代理只监督；AF只读证据并更新指定四文档，无代码更改、构建/发布/运行或终端/shell/任何直接间接脚本。完整路径和亲核边界见[AE/AF证据审查首节](completed-work/replacement-supervised-evidence-detection-review.md)。本次是独立回读，不是新实跑。

- **AD布局修复及有界回归完成，不再进行中**：`artifacts/replacement-present-ad-2026/readback/com-{x64,x86}/TestResults/ad-com-<arch>.trx:74`各8/8/0；`managed-{x64,x86}/TestResults/ad-managed-<arch>.trx:60`各7/7/0。四份异常/未执行计数0。现场actual identity、四角色官方/候选完整samples hex及两通知comparison亲读：每角色两条64字节，Reserved@4=0/result@8=2/refresh@12=60/time@16=123一致。SyncModeStatus HRESULT@8修复来自AD报告；非零值实际差分未覆盖。
- **不是V归档**：AD实际Native x64 `2BD7C2B9207ACF1E75182A8EC5C8B13B7B1B84E525045272733B158E8F069D5C`，x86 `255E699B643E0A6F5250F80C28AE7086038E5FDA6D9959F3EEF5AE1F0C40DA1F`。AD报告声明集成AA；新包两架构图像及矩形四comparison均亲读12帧/threshold0/exact=true。是RTB consumer有界验证，未独立重算全部像素或hash，不证明屏幕HW。算法、不同FrameTime/QPC、有效target同步及销毁迟到通知仍开放。
- **AE最终证据亲核**：`artifacts/replacement-evidence-ae-2026/final-readback/x64/TestResults/ae-final-x64.trx:143`与x86对应`:141`各12/11/1、退出2；真实loss未触发失败原样保留，首cooperative0、secondCreate0x88760868。Managed真实最终路径是 `final-readback/managed-{x64,x86}/TestResults/ae-managed-<arch>.trx:66`各8/8/0，身份为同目录 `Results/preflight-identity.json`，已亲读，不再用早期路径。
- **AE检测业务亲核**：两资源identity/comparison/detection-controls全文，16轮48帧/角色、threshold0、Failure=null；34项ObservationOnly观测变异（4 remaining-reference、28 event-count、2 event-order）全部拒绝。AE报告说明共用正式Detect；不是34个MSTest、不是原生double-free/生产注入。AE COM仍V候选，不归属AD新SHA。有限Release/同步callback不能证明全局引用、任意异步关闭或长期资源；AF未重读全部资源序列，不外推。

### 下一可独立派发包

1. 通知不同/重复FrameTime、有效target同步、非零SyncModeStatus及注销/在途销毁/迟到通知官方对照。
2. 非退化圆角合法实现及独立内容回归。
3. x86 Bitmap独立纯C# ABI/consumer夹具，不能由AA图像有限成功替代。
4. 真实恢复触发条件与lost→not-reset→同线程Reset→新色HW消费/释放；屏幕捕获仍WAIT_TIMEOUT/resource0，需真实duplication帧，不能软件替代。
5. 持续媒体、受控长期CPU/GPU/线程/callback资源及异步关闭；广泛随机仍开放。
6. 最后源码两个正式项目×双架构Release全量基线及现场identity/TRX/内容/生命周期回读，不拼旧full，不累计定向通过率关门。

下文为历史交接，冲突以AF节为准。


## AE 最新交接（覆盖下文 AC 的待亲核/检测缺口状态）

AA 顶层原始 TRX 亲核：双架构单元各2/2、窗口各4/4；实际托管/Native 加载 identity、两12帧 threshold0/exact comparison 与关闭事件已抽核，不称全量像素独立复算。AB final双架构亲核各1执行/0通过/1失败，四角色原始64字节hex及identity、官方exports布局亲读，V候选字段提前4字节。AE审查时AD尚未验收是历史状态；现已由AF亲核最终包，布局修复及有界回归完成，完整通知合同仍开放。

AE仅改Z独有ReplacementResourceCycleTests：正式同一Detect纳入事件唯一性与必要偏序，新增34项实际观测副本变异（4类非0 Release精确命中remaining-reference，28遗漏/重复事件，2移序），不做原生double-free。两项目双架构Release发布/定向实跑；COM最终各12/11/1，唯一真实loss未触发失败保留，Managed各8/8。产物独有ignored `artifacts/replacement-evidence-ae-2026/`，明确V归档，未重发生产、未改AD/通知/图像/捕获/ABI/入口/项目。详情与亲读/交接边界见[AE完成报告](completed-work/replacement-supervised-evidence-detection-review.md)。有限RTB/COM不外推屏幕HW、长期资源、真实恢复或full；最后源码四格full未完成。


## AC 最新只读审查交接（优先于下文 W/U/P 历史）

**完整替换未完成；最后源码两个正式项目×x86/x64 Release 四格 full 未跑。** AC只读证据并写本次指定四文档，无代码/项目修改、构建、发布、运行、终端/shell/直接或间接脚本；主代理仅监督。完整路径、行号与亲读/交接区分见[AC独有资源审查](completed-work/replacement-supervised-resource-review.md)。未修改AA/AB报告。

- **Z亲核**：`artifacts/replacement-resource-z-2026/final-readback/x64/TestResults/replacement-z-final-x64.trx:143`及x86对应`:141`均12执行/11通过/1失败、timeout/aborted/notExecuted0，退出2。唯一失败首设备cooperative=S_OK，secondCreate失败不等于首设备loss；没有Reset/恢复后新帧。两份session均Session1/UserInteractive=true/RemoteSession0，不能归为非交互根因。
- **有限真实资源证据**：两架构identity与comparison、四角色完整12点测量段及cycles/events抽样已亲读。V归档，两独立模块同进程先官方后候选；4预热+12测量，每轮红蓝/producer释放后consumer仍蓝，四类最终Release0、EventProxy Calls2/Disposals1。源码检查器真实槽调用、逐像素oracle与同一Detect可见；负控仅将实际观测副本Disposals改0/2，不能称真实double-free注入。非0 Release虽有拒绝分支，尚无针对性变异负控；事件偏序不由Detect验证。
- **资源未全闭**：x64候选private 40,898,560→41,730,048；x86候选managed 3,563,880→7,719,804持续上涨，句柄/线程本段稳定。保留帧/事件及序列化、GC、同进程顺序污染归因；既不能直接判候选泄漏，也不能自称长期资源/GPU收敛。AC未重算SHA或全量BGRA。
- **X/Y进行中**：X旧x86窗口TRX已亲核4/4，显式COM Stdcall与改善为关联证据，旧INVALIDCALL精确首个D3D操作未确认。Y旧COM两架构2/0/2已亲核，Managed5/5来自交接，官方未触发真正Present，通知合同未闭。审查时共享AA报告已声称新双架构有限通过、AB报告已声称异步Present触发及候选通知字段提前4字节；这些仅是亲读报告的后续交接，AC尚未亲核新原始产物，不猜其最终验收结论。AA/AB独立回读继续进行，不用Y旧计数覆盖AB新声明。

### 下一最小包

1. 问题线独立接收AA最终TRX/实际身份/图像比较/关闭证据；接收AB最终原始64字节通知、发送包、实际Present仪表与TRX/identity，确认布局差异后交生产所有者最小修复。不得用Managed成功冲抵COM失败，不编造旧INVALIDCALL首因。
2. 测试建设线独立补检查器针对性负控/必要偏序和受控长期资源归因；通知不同FrameTime/销毁合同仍需。恢复先确认首设备lost→not-reset→同线程Reset→新颜色真实HW消费及释放，不能用状态模拟或软件替代。
3. R/U仅软件固定seed/full-clip Clear及单一导入序列重复的限定成功，广泛随机仍开放；S Capture仍WAIT_TIMEOUT/resource0、读回前失败，不能由旧局部成功盖掉。真实屏幕HW、持续媒体及长期资源仍缺。

SHA仅归属已记录产物，Z/Y/AB的V归档不绑定AA/最后源码；源码未改不无故重发。最终full不得累加定向Green或拼旧full。四份文档已写入并须回读确认；本节不是新运行。

## W 最新交接：V 最终定向结果已核（下文 U/P 为历史）

**完整替换未完成。V已有最终归档，不再报告“T仍修复中/最终结果未收到”；最后源码两个正式项目×x86/x64 Release四格full未跑。** 主代理仅监督。W只读取/搜索现成证据并写四文档，未改代码/项目、未编译、发布、运行或执行终端/shell/任何直接或间接脚本；不新增独有报告。先读[规则](handoffs/current-execution-rules.md)、[方案](wpfgfx-complete-replacement-validation-plan.md)、[window-next W节](completed-work/replacement-supervised-window-next.md)和[会话汇总 W节](completed-work/replacement-supervised-session-summary.md)，后两份已有本包完整证据链。

### 最终证据与实现进展

- Q当前矩形GPU/rounded/animated分发已包括无brush/pen合法消费、截断校验及零轴退化矩形；非退化双非零圆角仍E_NOTIMPL。T修改SameThreadChannel/GeneratedTargetRegistry/GeneratedVisualTargetResources，补0x06 packed12字节路由、逐请求一次性订阅、成功Compose后通知及destroy退订，新增WindowPresentNotificationTests/WindowQEvidenceTests。V最终源码有target填VSyncUnsupported/60/最小非零FrameTime，无target填NoPresent。W不虚构V相对T逐提交diff，也不自动将Q旧失败归陈旧依赖。
- 根 `artifacts/replacement-window-v-2026/readback/`，直接核最终顶层TRX：`managed-final-x64/TestResults/window-v-final-x64.trx:94`与x86对应各10执行10通过0失败；`com-x64/TestResults/window-v-com-x64.trx:50`为4/4/0；`com-x86/TestResults/window-v-com-x86.trx:59`为4/3/1。四份error/timeout/aborted/notExecuted均0；COM x86 RunInfo退出码2，Image候选退出-1073740771。托管10项及COM身份/负控不冒充内容场景或full。
- 已核Results身份及全部八份窗口角色loaded-before-show。候选Native实际SHA与清单一致：x64 `7B491B79976CB1E1C8C24792E067CFD8948F0D47F6C174933C6B99F230D331E3`、x86 `508C123706484CF830C2448BEE34F3624AF3A74E02B9E8920A766AA296638A6E`。最终托管生产实际SHA两架构均 `4245E6F409E880D93ADE0341A6A89BDEB73434BB0D180417C0A0EECFD631BB64`，不与Native混同。官方x64 `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`、x86 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`；.NET/官方10.0.13、FileVersion10,0,1326,50504、commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。SHA来自记录交叉核，不是W重算。
- x64 Image与双架构矩形comparison均12帧/threshold0/exact=true，矩形partial各12非空帧。三cycle红蓝24×16→绿黄40×24，有close-drained、注销callback及有限producerAlive=false。W抽读原始官方/候选.pbgra32与report/events，读取二进制为有损文本，**未独立重算字节/帧SHA或逐字节重验**，相等仅引用正式测试报告。report明确软件RTB consumer，HWND屏幕像素/HW backend未验证、deviceLoss未触发；有限窗口状态序列通过不是硬件恢复或长期资源验收。

### 下一最小任务（不可被局部Green掩盖）

1. **问题线：x86 Image首次DrawImage INVALIDCALL。** 失败目录 `com-x86/Results/replacement-state-window/f2e72beb1a2541efa3443410b51a9f67/candidate/`，events仅cycle0/create，无候选frame/report。`uce-diagnostic.log:1–3`首HRESULT `0x8876086C`（D3DERR_INVALIDCALL）、channel0/thread2，packet `57696E646F772E447261772064657074683D35206B696E64733D44726177496D616765`为`Window.Draw depth=5 kinds=DrawImage`，随后Window.Visit→Targets.Render。仅诊断传播链，不是完整原生调用栈或原始DUCE包，不能猜具体D3D调用/环境。下一有界包只定位最早INVALIDCALL的具体操作/参数、device/surface/texture关联和x86布局，以正式C#官方/候选同输入隔离对照，不泛化改renderer，不改捕获/阈值/skip或fallback。
2. **测试建设线：0x06独立官方合同。** 两份managed-final Results/present-notifications.json均packet `060000007B00000000000000`，两条`0A00000001000000...`64字节消息，仅无target重复请求NoPresent；目标有target时VSyncUnsupported/固定60/minFrameTime三字段尚无官方行为独立证明。解析/退订/窗口局部通过不是合同全闭。用现有两正式项目C#角色独立记录官方/候选有target、不同FrameTime、重复请求/销毁的内容和必要偏序，不用候选算法生成预期。
3. **S捕获仍开放。** S双架构各18执行14通过4失败、TRX timeout0；初始及10次Acquire均WAIT_TIMEOUT/resource0，在读回前失败。不能归候选绘制或宣布环境根因；需要可实际交付duplication帧的交互桌面原样重跑。S官方10.0.9/L归档不绑定V。

R各9/9仅三固定seed各16步full/clip Clear，14负控为测试内部拒绝；U各3/3仅seed104729、7×8同序列在三DataRow重复，不是新随机覆盖/HW/角色进程隔离，R/U为O归档不绑定V。非退化圆角、真实HW、真实device-loss恢复、持续媒体与长期资源、广泛随机、VBlank必要偏序、x86 BitmapRealizer和历史1字节等仍缺。两线继续独立推进。**源码没变不无故重发生产**；修复后常规配置构建真实依赖、双架构受影响验证，最终两个项目双架构Release full，不累加局部或拼旧full。四文档W节已同步落盘；下文旧“窗口候选全部首帧前失败”等不得作为V当前状态。


## U 最新监督交接（覆盖下文 P 的当前状态，历史证据保留）

**完整替换仍未完成；最后源码两个项目×双架构Release四格full未跑。** 主代理仅监督；U仅独立验证R既有包和整理证据，未改生产/窗口/捕获/ABI/入口/项目，未重发生产，无终端/shell/直接或间接脚本。

- 测试建设线R：固定三seed各16步、最大16×16 full/clip Clear，归档双架构各9/9；14负控拒绝是每架构测试内部检查，不是额外14项。真实失败删除缩减至固定点已有，但初次stale-frame是夹具误报，不是候选缺陷。U已实际补外部JSON导入：双架构各3/3，同一7×8 seed104729序列重复三次；非三个新seed、非HW、非角色进程隔离。证据 `artifacts/replacement-u-2026/readback/{x64,x86}/TestResults/replacement-u-replay-<arch>.trx:60`，原始输入/identity/官方候选JSON/BGRA同包。详见[随机报告](completed-work/replacement-supervised-random-detection.md)U节。
- 问题线Q：有有限软件RTB红/蓝两帧，不能当HWND/HW首帧闭环；窗口仍0x06失败。T已在[window-next](completed-work/replacement-supervised-window-next.md)记录0x06未路由导致malformed packet的根因和x64原始两帧独立审计。T并行修复**进行中，尚无最终结果**，不得猜通过或把中途源码绑定旧归档。
- 问题线S：双架构各18执行14通过4失败，TRX timeout=0。证据 `artifacts/replacement-capture-s/final-readback/{x64,x86}/TestResults/capture-s-final-<arch>.trx`（Counters302/297）。初始及后续10次Acquire全WAIT_TIMEOUT、resource0，失败在读回之前；不能归候选绘制，也不能宣布环境根因已定。需要同会话可实际交付duplication帧的交互桌面，随后原样重跑原Present和交替帧control。[capture-next](completed-work/replacement-supervised-capture-next.md)已落盘。

R/U运行官方及.NET为10.0.13，S为10.0.9；各自官方SHA与候选归档不同，不能混同/外推。R/U候选沿用O归档，S沿用L归档，均非T/最后生产源码证明。真实device-loss、持续媒体、资源收敛、角色进程隔离、广泛合法随机、VBlank与x86 BitmapRealizer等仍缺。测试建设和问题处理继续两线推进；稳定后常规构建生产依赖，再两个正式项目双架构Release全量，不拼接局部结果。

U落盘范围：本入口、问题状态、唯一会话汇总、R随机报告；独有输出ignored `artifacts/replacement-u-2026/`。R final-bundle两个包根留有重放JSON，勿误当默认seed包。

## 当前任务与监督方式

**实现并验证 WpfGfxShape 能够完整替换原来的 wpfgfx.dll；当前未完成。** 主代理仅监督、拆包、协调与验收，不探索编码；子智能体承担探索、实现、差异修复及实际验证。测试建设与问题处理两线可独立推进，不能等所有问题关闭才补测试，也不能以局部Green代替完整交付。

仅两个正式项目 WpfGfxShape.Tests 与 WpfGfxShape.ComAcceptance，不恢复历史治理门或第三Host。Windows x86/x64 Release，纯C#/.NET，官方独立零阈值；核心为ABI、功能、时序、资源、恢复。DLL驻留不免除对象/线程/callback清理，真实device-loss不能按环境错误排除。SHA只作结果归属，不是测试建设首要阻塞；最终结果必须绑定最后源码实际依赖，不能拼接旧full。

## 必读顺序

1. [执行规则](handoffs/current-execution-rules.md)、[当前任务](handoffs/current-work-item.md)、[完整方案](wpfgfx-complete-replacement-validation-plan.md)。后两份中的早期起始状态不等于本轮当前结果。
2. **[唯一监督会话汇总](completed-work/replacement-supervised-session-summary.md)**：P 有界落盘，已纳入 O 证据；主监督者据此答复用户。
3. [最新问题/排程](replacement-issues-and-plan-status.md)、[独立审查](completed-work/replacement-supervised-independent-review.md)、[L捕获清理/输出预检收尾](completed-work/replacement-supervised-capture-cleanup-preflight.md)。
4. ABI/content、realizer、capture、state、window各supervised报告（链接在问题页）；window 已由 P 仅文档收尾，记录 E/G/J/M/O 修复链及当前失败。
5. [历史稳定基线](handoffs/current-stable-baseline.md)、[总计划](remaining-gap-closure-plan.md)、[最终矩阵](final-acceptance-remaining-matrix.md)，均不能替代最后源码四格实跑。

## 本轮真实进展与未闭

- context夹具部分已修、集成双架构各52项通过；Realizer thiscall/槽与失败传播、窗口0x33初始化/0x05处理存在实际生产修改，不能再声称本轮没有生产修复。D/H部分归档顶层计数仅交接，K未找到完整顶层TRX；不冒充独立复核。
- E/G 的 0x33/0x05 修复、J 首帧诊断及 M 矩形实现均是实际工作。M solid 曾因默认 Uv1 调 null `_texturedColorSource` 返回 UnsupportedOperation；O 在 GeneratedHardwareBitmapOperations 修 solid 无纹理映射与三角形实际 bounds。O 双架构发布成功（交付事实，P 未重发/独立核发布日志）；P 已核 x64 单元15/15、两架构窗口定向各3执行1通过2失败；x86单元未实跑。
- 官方 Image/矩形角色通过，但候选仍首帧前终止。矩形两架构首条诊断均为 DrawRoundedRectangle / E_NOTIMPL；候选 events 仅 create。首帧/更新/resize/关闭及 HW 像素恢复未通过验收；后续阶段不能假称已实跑。唯一窗口通过项为空白/旧帧拒绝负控；先前失败刻画单元通过也不是内容绘制通过。
- K独立BitmapRealizer：x86归档执行0xC0000005、无最终TRX，官方分支/精确故障点仍需隔离持久化证据；x64归档2/2，不外推x86或最新生产。首包原始1字节差异未找回，新鲜内容相等不代历史关闭。
- **L最新COM x86/x64各11执行9通过2失败**：四清理用例、四选择边界、身份通过；两呈现control内容帧等待失败，不能关闭捕获。TRX timeout=0，实际是断言失败。此前42/42与6/6只归旧包。
- **L Managed x86/x64各1/1输出预检**，均有包内TestResults顶层TRX和Results身份。N已直接回读四格Counters/identity；不是full。
- VBlank必要偏序、官方同场景HW/backend、真实device-loss与持续媒体/资源、合法随机/缩减、最后源码两个项目双架构全量均未完成。

L证据根：`artifacts/replacement-capture-cleanup-l/readback/`；COM `com/{x86,x64}/TestResults/capture-cleanup-<arch>.trx`及`Results/aot-identity.json`；Managed `managed/{x86,x64}-final/TestResults/output-preflight-managed-<arch>.trx`及`Results/preflight-identity.json`。

## O 原始证据与 P 操作边界

P 本包仅回读并更新这四份文档，未改代码/项目、未编译发布运行、未执行任何终端/shell/直接或间接脚本。O 根为 `artifacts/replacement-window-o-2026/readback/`：`tests-x64/TestResults/unit-o-x64.trx:108`、`package-{x64,x86}/TestResults/window-o-{x64,x86}.trx`（Counters 分别在58/54行）。三份 error/timeout/aborted/notExecuted 均0，窗口 RunInfo 失败码2。

矩形目录：`package-x64/Results/replacement-rectangle-window/cc27f5fe36ae440abd74f1683655f1fd/`、`package-x86/Results/replacement-rectangle-window/6274686093d943a2b4d9e473770b5f77/`。已回读候选 events/uce-diagnostic/loaded-before-show 及包内 aot-identity，官方业务 JSON 部分；未重验全部帧字节。

已回读、未重算的实际加载 SHA：x64 `AF310A91CF68711CCD53D17F89BC1C02A7E339662BFD025AB5B4D6F3907619D5`；x86 `5D212B5B42E661ADCF5A16C90CBE5AD565083ADC97DA8FA074D201B74E91322B`。仅结果归属，不作为测试建设前置或全量完成证明。详见[窗口报告](completed-work/replacement-supervised-window-fix.md)。

## 下一批有界任务

**先执行 O 后窗口有界包**：取得矩形路径独立真正非空/非旧帧内容证据与官方零阈值对照；圆角实际缺失能力最小复现、合法最小实现及回归，不跳过/假成功/fallback/放宽断言，不无边界扩展，再继续五维。

按问题页P0—P6分派：P1 x86独立Bitmap官方访问违规/合法布局；P2捕获超时和真实状态机负控；P3 VBlank合同；P4真实loss/持续媒体/资源趋势；P5合法随机/失败缩减。P0入口验证可并行探索，修改共享入口须协调。窗口从 O 当前失败继续，不再等待 M 交付；共享代码所有权仍须协调。每包交文件、根因、限定结果、实际身份、顶层TRX与原始业务证据、明确未闭。

**P6最后执行**：源码稳定后先常规配置构建最终生产依赖，再发布两个项目双架构Release全量。禁止终端、shell、脚本及间接脚本执行；仅专用工具。独有输出/包/回收目录，注意不同PublishDir仍可能共享obj/bin，串行集成。

## 已验证输出配方与入口审查转交

- COM：Release、对应RID、自包含、.NET10.0.9（本次环境非固定合同）、ComAcceptanceSandbox=true、AotRuntimeIdentifier匹配RID、**显式AotDllPath**。NativeRun在创建MTP前默认绝对`AppContext.BaseDirectory/TestResults`。
- Managed：EnableMSTestRunner=true、DefineConstants=SANDBOX_TEST_ENTRY_POINT、StartupObject=WpfGfxShape.Tests.SandboxTestEntryPoint；手写入口及附带预检类均受同一条件编译，不是常规构建无条件多测。L另用BuildProjectReferences=false，**只适合本次已有依赖输出预检，不能证明最后源码依赖**。最终先常规生产配置构建依赖，不能把sandbox属性传染生产后或沿用旧依赖算最终验收。
- 两入口均直接返回RunAsync，未见吞失败；COM TRX记录失败码2，Managed受控失败返回码仍需实跑。独立token形式`--results-directory <path>`不被覆盖；其他语法是否支持待入口负责人验证，不猜缺陷。
- 有界用`--filter`而非filter-method；全量去掉filter。显式`--report-trx`和独有`--report-trx-filename`，不以默认目录误当自动开启报告。回收**整个任务目录**，逐格回读Counters、identity、原始帧/事件/backend/资源。若中断或找不到顶层TRX，明确标交接/缺失，不造计数。

整体未完成，不能宣布已可完整替换。
