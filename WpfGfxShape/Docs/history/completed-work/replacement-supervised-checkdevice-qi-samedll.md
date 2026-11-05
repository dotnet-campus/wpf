# EZ：CheckDevice 单点 QI sameDLL 对照

## FA 最终回读闭环补记（最新优先）
四文档最终正文已全文小段回读至末：补记前EZ25行、handoff568行、session-summary624行、issues641行；超长截断的末行与余段均缩小补读，本补记随后再读。源码安全撤回、627行全文核验、Code指定标记零残留及正常分析器默认Release build已完成；当前default安全但未修。既有两task独立原件与direct精确TRX已核，generated精确TRX缺失不填计数；hash仅核历史记录未重算，构建stdout截断限制保留。四文档交付闭环不是产品修复/运行验收；下一唯一第二deviceQI任务仅另授权，本轮不执行。

## FA 安全收口与独立核证（过程保留）
已按 default-before-ez.cs.txt 与 ez-full-experiment.cs.txt 的实际条件块精确替换撤除 EZ 新增行为及辅助，未整文件覆盖。恢复后 Interop 627 行全文亲读；Code 检索 EZ_CHECKDEVICE/EzGenerated/EzTrace/ez-generated/ez-checkdevice/creationBoundary 及指定 EX/EV/ET 辅助零命中。默认 generated、已有 surface directQI、DT/DX 条件 trace（默认未定义）、EF DR 声明和 CF/CX/CY fill 保留；仅修改 Interop 与本轮四文档，未触禁止区域。
默认正常分析器 Release 生产 csproj build 一次 Exit0，1482 警告、0 错误、6.38 秒；未传 NoWarn/规则关闭/实验 props/RID，无 clean，属默认增量构建入口而非 AOT 发布或运行验收。可读日志61行已读至末，但原 stdout 1213353 字符仅保留末20000，前1193353字符已截断，不能声称全部警告明细已审或新增警告为零。未因警告扩大修复范围。
EZ source/restore-sha256.txt 精确路径不存在；ET既有 restore-sha256.txt 两行亲读均为 1C3E30CCB283A71FAE61286C582D7899A036911F474D3632614678F207FFFFB1，仅历史恢复记录，不是本轮重算认证；不自造当前 SHA。
独立亲读 direct task482d5bb6703440b0a2e403cc6675eaf5 与 generated task8f32bc2bc6e949d8810a6f5057b8f97e 原件：监督各39行、业务39/6、DT72/8、stdout12/2及两stderr空、各aot-identity和业务actual identity、direct后置21行、精确标记与实验源码选择代码。两actualNative均2CB6F77FD367B8841DB26B2849971C8D21D359A489DC7708F12A010619DC0D47；test均E587956E835B2A3BEB269DCA9073320B65B1BA0372F3B3D43155ADC9B83EF9E7，MVID均8ff939db-bbd7-4447-89bc-2f86f7bf791d，.NET10.0.13/X86/4。direct根标记不存在；generated根标记一行内容为 EZ_CHECKDEVICE_SAMEDLL CheckDevice device QI generated only；静态readonly一次选择仅切creationBoundary CheckDevice deviceQI，其他前序direct、后置generated固定，不能从日志名generated推实际选择。
direct精确本地 run-direct/Results/ez-direct/TestResults/child.trx 26行全文亲核 Completed/执行1通过1失败0，绑定task482…；三轮Create Hr0/12x3、QI同identity、AddRef2/Release1、GC dirty Hr0、Detach/Bitmap0及最终surface/device/factory0，监督Exit0。generated精确本地 run-generated/Results/ez-generated/TestResults/child.trx 直接读取不存在，无TRX计数；业务首次Create未返，DT末CheckDevice.generated.QI.before无after，监督Exit-1073741819/C0000005，均无timeout/cancel/kill/drain异常。
单点对照因果成立于其余前序direct、后置generated的固定背景；不认定唯一默认根因、栈破坏或全Silk库bug。EN三处不足不能抹去EL Creation在其独立背景中的因果，也不能把EL背景直接移绑EZ。Available0且native/consumer false，Ex第二Creation/LUID/extendedRelease未执行；HW消费/恢复、独立x64Event0、full最终四格保持未闭。当前default源码安全但缺陷未修。
下一唯一具体任务（须另授权，本轮不执行）：沿用EZ direct已成功背景，同DLL/正式DR/布局日志，保持CheckDevice deviceQI及其余前序direct、后置generated固定，仅runtime切GetUpdateMethod第二deviceQI精确指针重载direct/generated。该调用在direct三轮均到达且QI返回E_NOINTERFACE，已具成功基线；不再重复已证CheckDevice，不同时改caps/Release/日志。先direct三轮refs0/Exit0/精确childTRX控制成功，才唯一generated对照，最多两有效task、控制失败停；结果只解释该背景单点作用，不外推全库/默认唯一根因。本轮无新实验/发布/沙箱/full。
四文档最终全文小段回读进行中；完成后补记再读，不以新增段回读冒称历史全文闭环。

## EZ-4 generated单点失败，立即安全恢复
最后task8f32bc2bc6e949d8810a6f5057b8f97e，父4416/子4632，Exit -1073741819/0xC0000005，无timeout/cancel/kill/drain异常。业务6行首次Create未返，DT8行末CheckDevice.generated.QI.before；stdout2行/stderr空均亲读。两组实际Native/testSHA/MVID完全相同，只同包增ez-generated.input（内容已读），没有每组重编译。仅支持该精确QI重载替换在固定背景中的因果对照，不判唯一默认根因、栈破坏机制或整个Silk库bug。有效task2（direct1成功/generated1失败），不追加拆组/full。现在撤全部EZ生产分支并恢复默认，构建与剩余回读进行中。

## EZ-3 direct正控制成功（历史）
Task482d5bb6703440b0a2e403cc6675eaf5，父4660/子5992，Exit0/无timeout、cancel、kill或drain异常；精确远端绝对child.trx不可读，本地精确回收件26行亲读Completed/1执行1通过。业务39行与后置21行亲读三轮Create Hr0/12x3、QI同identity、AddRef2/Release1、GC dirty Hr0、Detach/Bitmap0，最终surface/device/factory0。Available0不作消费验收。实际Native2CB6F77FD367B8841DB26B2849971C8D21D359A489DC7708F12A010619DC0D47，testE587956E835B2A3BEB269DCA9073320B65B1BA0372F3B3D43155ADC9B83EF9E7/MVID8ff939db-bbd7-4447-89bc-2f86f7bf791d，.NET10.0.13/X86/4。根标记精确读取不存在；现在仅同包加标记，不重编译，准备最后generated任务。有效task1。

## EZ-2 已发布，准备direct控制（历史）
修改前及完整实验源码已由专用构建Copy保存在 artifacts/ez-checkdevice-qi/source；NativeAOT x86正常分析器发布成功，正式DR selfcontained10.0.13发布成功，均显式独有PublishDir，未改DR/DS/禁止区域。标记只有一个静态readonly初始化位置，默认不读；其余前序固定direct，后置generated日志保留。有效task0。
偏差保留：初次尝试以发布工具-t:EzCaptureDefault捕获，但实际执行默认Publish且缺RID失败；只产默认源码实验快照，无业务task。随后改BeforeTargets条件Copy并以PublishAot=false/独有capture目录成功保存default；不是NativeAOT控制组。发布日志输出受工具末20000字符限制，不冒称全部警告明细已审。后续唯一正式NativeAOT及DR各成功一次。

## EZ 进行中（历史）
仅显式 EZ_CHECKDEVICE_SAMEDLL 配置；固定其余前序 direct、后置 generated，仅同 DLL/同正式 DR 程序集以一次读取的小标记选择 CheckDevice 精确 device QI 指针重载。先 direct 三轮完整生命周期与 refs0/Exit0/精确 childTRX 正控制，成功才唯一 generated 对照，最多两有效 x86 task，控制失败停。不改 GetUpdateMethod.QI，不改输出地址/布局/日志/能力与释放语义。
当前正在读取规则、完整方案、EX/EY、EV、当前源码及 DR；尚未修改生产源码、发布或运行，有效 task=0。保存完整修改前和实验源码到 ignored artifacts/ez-checkdevice-qi，最终撤所有 EZ 行为/辅助，正常分析器默认构建。历史不覆盖；本轮不是默认修复，Available/HW恢复/media/resources/full最终四格及独立 x64 Event0 保持开放。
