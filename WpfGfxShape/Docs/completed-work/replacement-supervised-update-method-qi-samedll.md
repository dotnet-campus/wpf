# FB：GetUpdateMethod 第二 deviceQI sameDLL 单点对照

## FD 最终回读闭环补记
四文档历史正文已全文小段读至末，超长截断末行/余段已缩小补齐；写入FD终态后，新增段逐段回读、原历史未改，最后补记随后再读。恢复源码627行全文及FB辅助零残留、两task独立证据与上传txt纠正均已核。空预检文件保持空且无引用，交接不因此阻塞。无新实验/代码/build/发布/沙箱/full；default安全但未修，下一仅生产组合修复评审及双架构DR/refs/BW/CK保留门，所有开放项不变。文档闭环不代表完整替换。

## FD 独立核证完成：正确部署与第二 QI 因果，default 安全但未修（最新优先）
FD 仅专用读写/搜索，先标 handoff 核证进行中，再读执行规则及完整方案120行、FC最新结果和既有原件；无终端/脚本、无新实验/task/发布/沙箱/full、无生产或测试代码修改，本轮不重build、不重算hash。下方FB/FC过程及历史全部保留，旧“待核/待清退/下一再direct”由本节终态覆盖，不抹去当时失败。

### 精确原件与独立核证
有效两组实际回收根是 `artifacts/fc-update-qi/run-direct` 与 `artifacts/fc-update-qi/run-generated`，不是 `artifacts/fb-update-qi/run-direct`（后者是FB初始化失败）。两根均用 `Results/fc-direct/`：监督 `supervision.json` 各39行全文；业务 `dr-candidate.jsonl` 各39/6行全文；DT `Results/dt-interop-create/stages.jsonl` 各72/18行全文；各 `Results/aot-identity.json`、stdout12/2行及stderr空均亲读。direct后置 `Results/ez-checkdevice-qi/stages.jsonl` 21行读至末。目录/名称枚举漏列TRX不能判不存在，FD已直接读取精确路径。
- direct task `e8ddeccf18564b6ea2fe897bbcda7e79`：监督Exit0，精确 `Results/fc-direct/TestResults/child.trx` 全26行，Completed/1执行1通过0失败；三次Create Hr0、QI/AddRef/GC dirty/Detach及Bitmap最终Release0，末surface/device/factory各0、callback root freed/module resident、completed。后置generated GetDirect3D Hr0、factoryQI E_NOINTERFACE/null、Release2三轮成功；第二Creation/LUID/Ex Release未执行，不造覆盖。
- generated task `215ca9d08f7a43239226ea87c223ce73`：监督Exit-1073741819/0xC0000005，首次Create before后无after；DT末 `GetUpdateMethod.QI.before` 无after。精确 `Results/fc-direct/TestResults/child.trx` 直接读不存在，无完成TRX，不填执行/通过数、不造父TRX。stdout仅MSTest版本与诊断路径，stderr空，无故障栈。两监督均exited，无timeout/cancel/kill/drain异常。
两组业务actualNative与aot identity同为 `5FCDE90B9EFC43092841FF1D4DBA955E885492E89D1347CF547B4E4E76B82883`；test SHA `4C4CADA04FD59982B2CAE4C9CAA1E451783809258389CB3C1B416532A817E96C`，MVID `1030015f-5b3b-4b06-9160-191a5bfd28be`，.NET10.0.13/X86/PointerSize4、requested=actual任务内Native路径。FD核记录间一致，不冒称本轮重新计算文件hash。

### snapshot 选择与因果边界
归档 `artifacts/fb-update-qi/source/fb-full-experiment.cs.txt` 36–38行由标记presence初始化FbGenerated；180–235行固定GetDesc12/GetDevice3/Creation9及CheckDevice(device,true)；266–310行固定GetDeviceCaps7，只有第二deviceQI在同this/IID/out、slot0/Stdcall签名的direct与精确 `device->QueryInterface(&iid,(void**)&extended)` 间runtime切换。445–495行证明CheckDevice前序direct，trace里的generated只是固定标签，不代表选择generated。320–397行证明后置slot6/QI0/Release2保持generated且日志目录仍EZ。direct根标记直接读不存在，generated根 `fb-generated.input` 一行存在，内容与选择一致。
在固定其余前序direct、CheckDevice direct、后置generated及同DLL/同测试身份背景下，第二QI切换与成功/异常的外部因果成立；正常控制的后置generated确实成功。不能判唯一default根因、首坏指令、栈损坏或整个Silk库bug，也不能由此证明只改两QI或历史EN三处就足够。Available0、NativeInvocationCovered/ConsumerCovered=false，未覆盖通知消费；nonEx未执行分支不当通过。

### 最终上传合同与错误探测分层
最终权威预检为 `artifacts/fb-update-qi/upload-preflight-fc.json`（Files的Path/Bytes/Sha256与ActualFiles清单），精确sourceDirectory为 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\fb-update-qi\com-acceptance-package`。不是PublishDir=package、父目录或回收根。FC既有C#逐文件清单/长度/hash核验通过；FD独立读JSON身份、根输入和2070–2110最终关键记录，不冒称重新执行逐文件校验。Native/wpfgfx_cor3.dll=4071936字节/5FCDE90B…；测试DLL=1450496字节/4C4CADA0…；exe=132096字节/EA7BEC34…；aot.sha256=66字节/04EC240D…；package-files.txt=14209字节/0B77E849…。根wpfgfx_cor3.dll=1807144字节/5EBD18E0…是不同官方模块，不能与Native候选混同。
`upload-preflight-fc.txt` 早期“尚未批准/hash未算/有效0”已纠正为指向JSON及最终两task结果；JSON的GeneratedMarkerExists=false仅上传前状态，第二上传加标记不改写历史预检。FC复用同正式白名单包，实验未重编译。早期VSTest归档DLL缺ApplicationInsights探测失败不属正式exe业务；FB task60c6e3a930aa4ae1815fe4b5b884f188误传PublishDir缺aot.sha256、业务0另列，均不遮蔽上述有效正式exe任务。

### default恢复、空文件与下一唯一评审门
当前 `Code/WpfGfxShape/Abi/InteropDeviceBitmapExports.cs` 627行已分范围全文读至末，并核default-before-fb的Create/两QI对应段：默认generated、既有surface directQI、DT/DX条件trace保留；Code范围 `FB_UPDATE_QI|FbGenerated|FbTrace|fb-generated|creationBoundary` 零命中。FB Build.props仅ignored实验显式定义，不是默认配置。FD不改生产、EF及CF/CX/CY；default已恢复安全但缺陷未修。FB历史默认正常分析器Release build Exit0/1482警告/0错误/13.55秒，仅历史引用，stdout前1193353字符截断，不造FD新编译或全审警告结果。
`Tests/WpfGfxShape.Tests/FcUploadPreflightTests.cs` 直接读为空，Tests全文内容检索FcUploadPreflight零命中，无类型/测试属性/引用；空编译单元本身无默认测试发现或悬空类型风险，不冒称跑过发现/编译。无删除工具保持空，不为删除写脚本、建工程或拖延交接。
下一单一任务仅生产修复评审：结合EL Creation独立背景、EZ第一QI与FC第二QI及EX前序组合/ER窄链不足，说明本类有限已证精确调用点组合怎样覆盖剩余前序风险；明确GetDesc/GetDevice/Creation/CheckDevice状态调用/GetDeviceCaps与失败cleanup的必要范围、SDK签名/输出/引用守恒及非Ex/Ex未覆盖，不把单点因果误当最小充分集合，不泛换全库/无界逐点实验。生产修改只有通过正常分析器、官方/候选x86+x64 Release正式DR循环、精确身份/TRX及refs收敛、必要生命周期与BW1/CK10后才能保留；失败精确撤回。此门本轮仅交接不执行。保留CF/CX/CY与EF，独立x64Event0/E_HANDLE、HW真实消费/恢复、Available通知、media/resources及最后源码双项目双架构full四格仍开，完整替换未完成。

## FC-3 两组有效业务完成，安全核证/文档回读中
有效业务task2，部署纠正一次：direct e8ddeccf18564b6ea2fe897bbcda7e79 Exit0/TRX1执行1通过；generated 215ca9d08f7a43239226ea87c223ce73 Exit-1073741819/0xC0000005，首次Create未返，DT18行末GetUpdateMethod.QI.before，无QI.after，精确Results/fc-direct/TestResults/child.trx不存在，不填计数、不造监督父TRX。两监督39行均无timeout/cancel/kill/drain异常，generated业务6行/stdout2/stderr空已全文读。
两actualNative=5FCDE90B9EFC43092841FF1D4DBA955E885492E89D1347CF547B4E4E76B82883，与本地实算及aot一致；testSHA=4C4CADA04FD59982B2CAE4C9CAA1E451783809258389CB3C1B416532A817E96C，MVID=1030015f-5b3b-4b06-9160-191a5bfd28be；均.NET10.0.13/X86/PointerSize4。同一sourceDirectory上传两次，仅第二次增加fb-generated.input，固定监督参数/Results/fc-direct与既有EZ trace位置；未重编译实验包/未改生产。
限定结论：固定前序direct（包括CheckDevice）及后置generated背景，GetUpdateMethod第二deviceQI direct成功、generated异常，单点因果成立；不能说唯一默认根因、栈破坏或整个Silk库bug。CheckDevice trace名称含generated是既有固定标签，选direct由归档源码证明。Available0未覆盖通知/消费；HW恢复/full四格/x64Event0等仍开放。暂无下一新增实验；唯一最小后续应独立评审将已证精确deviceQI调用点作生产修复及受影响回归，不扩全direct/full。

## FC-2 direct正控制成功，准备唯一generated
本地纯C#临时Tests诊断1/1通过，逐manifest文件存在/长度/hash与完整实际清单一致；upload-preflight-fc.json落地于fb-update-qi，sourceDirectory精确为其com-acceptance-package。Native实算SHA与aot.sha256均5FCDE90B9EFC43092841FF1D4DBA955E885492E89D1347CF547B4E4E76B82883；Native/test均I386，testSHA4C4CADA0…/MVID1030015f…，marker不存在。未重编译实验包。此前本地VSTest直接读归档DLL失败（缺ApplicationInsights）不是业务结果。
direct task e8ddeccf18564b6ea2fe897bbcda7e79，监督Exit0/.NET10.0.13/X86，无timeout/cancel/kill/drain异常；精确Results/fc-direct/TestResults/child.trx全文26行1执行1通过。三轮bitmap0及末surface/device/factory0、Available0且NativeInvocationCovered/ConsumerCovered=false；actualNative同preflight及aot身份，testSHA/MVID同归档。源码恢复全文与文档历史回读继续；有效业务1，准备同一精确包仅增fb-generated.input，输出根保持Results/fc-direct以固定日志背景。

## FC-1 进行中：纠正上传源，尚未启动业务
FB 唯一 task 因误上传 PublishDir=package 缺 aot.sha256，未进 Create；有效业务0/generated0，不能算第二QI对照。FC 已获授权先核既有 fb-update-qi 正式 com-acceptance-package、归档源码与恢复源码；精确上传白名单包，不上传 PublishDir/父目录/回收目录。上传前本地记录清单与实际hash核验；可复用则不重编译。先 direct 三循环 refs0/Exit0/TRX1/1，成功才同包仅加 fb-generated.input；最多2有效业务task，真实业务控制失败停止。当前核验中，无FC新task/发布/生产修改。

## FB-4 源码安全恢复完成；对照未完成
已逐块撤全部FB条件/辅助，未整文件覆盖；Code检索FB_UPDATE_QI/FbGenerated/FbTrace/fb-generated/creationBoundary零命中，原default generated及既有surface directQI、DT/DX默认关闭trace恢复。未改DR/DS/NativeRun/ExitDiagnostics/VisualCache/historical/捕获/WindowQ/项目打包/EF/CF/CX/CY。
默认正常分析器Release build Exit0，1482警告/0错误/13.55秒；未NoWarn/关闭分析器/降WarningLevel。61行可读日志已至末，stdout1213353字符前1193353被截断，不能称全审警告；非clean/AOT/运行验收。快照保留artifacts/fb-update-qi/source/default-before-fb.cs.txt与fb-full-experiment.cs.txt。
直接回读正式打包targets确认：PublishDir=package仅为发布输出，PackageSelfContainedComAcceptance真正白名单输出为com-acceptance-package，aot.sha256/package-files.txt写在后者。本轮误上传前者导致初始化失败，执行错误不归产品。后者aot.sha256实读5FCDE90B9EFC43092841FF1D4DBA955E885492E89D1347CF547B4E4E76B82883，只是发布身份，不是失败业务进程actualNativeSHA。
亲读监督39行、精确本地childTRX49行全文（1执行0通过1失败，初始化失败）、stdout26行（中文编码失真，英文异常明确）、stderr空。业务trace/stages精确路径无文件，不造parentTRX；generated未启动、无标记内容与第二组SHA/MVID，不存在可交付的两组sameDLL因果证据。恢复源码关键调用已读，尚未完成恢复627行全文及四文档历史全文小段回读，不宣称完整收口。
下一唯一最小步骤：独立核既有正式com-acceptance-package白名单/身份与源码恢复，另授权从该clean源重启FB direct控制；本轮不追加任务，不重复CheckDevice，不追加full。Available/HW恢复/media/resources/full四格/x64Event0均开放；EL Creation因果仅原背景，本轮不提供新因果结果或完整替换声明。

## FB-3 direct控制失败，停止实验
唯一沙箱task60c6e3a930aa4ae1815fe4b5b884f188，父1852/子1496，监督Exit2/0x00000002，无timeout/cancel/kill/drain异常。精确childTRX已读失败原因：初始化缺包根aot.sha256，未进入DR业务，dr-candidate.jsonl精确路径不存在。实际启动x86task1，业务有效对照0，generated0；严格控制失败即停，不补标记、不重发/补跑、不为凑两组运行negative。尚无actualNativeSHA业务原件，不作sameSHA因果结论。
监督testSHA4C4CADA04FD59982B2CAE4C9CAA1E451783809258389CB3C1B416532A817E96C/MVID1030015f-5b3b-4b06-9160-191a5bfd28be，.NET10.0.13/X86。此为执行包准备失败，不是GetUpdateMethod指令对照结果；开始精确撤FB及默认正常分析器build。下一唯一步骤只能先核正式包白名单身份文件生成/回收合同，另授权后重新取得direct正控制，不能重复CheckDevice或推全库bug。

## FB-2 已发布，准备direct控制
独有props/obj/bin/显式PublishDir已完成NativeAOT x86与正式DR selfcontained10.0.13发布，正常分析器未关闭。完整快照source/default-before-fb.cs.txt及fb-full-experiment.cs.txt已保存。前置PublishAot=false capture仅保存默认快照，非业务task；正式NativeAOT与DR各发布一次，不逐组重编译。后置日志目录保留EZ名称以固定输出背景；仅FB条件读取fb-generated.input，第二QI same this/IID/out及Stdcall slot0签名不变。有效task0。

## FB-1 进行中
读取执行规则、完整方案、FA/EZ及EX/EY控制、当前Interop与正式构建/监督入口。固定EZ direct成功背景：CheckDevice与其余前序direct，后置slot6/QI0/Release2 generated，仅FB_UPDATE_QI_SAMEDLL条件内runtime标记切GetUpdateMethod精确deviceQI指针重载。default不读标记，不改ABI/HRESULT/null/释放/Ex分支。
有效x86 task0；先direct三轮合法surface完整生命周期、refs0/Exit0/精确childTRX，成功才同包仅加fb-generated.input运行唯一generated，最多2task，控制失败即停。不追加full，不改DR/DS及禁止区域。结束保存实验前/实验完整快照，精确撤FB恢复default及DT/DX trace，正常分析器默认build。
当前尚未修改生产、发布或执行；Available/HW恢复/media/resources/full四格/x64Event0均开放。历史保留，阶段即时补记；最终四文档全文小段回读。
