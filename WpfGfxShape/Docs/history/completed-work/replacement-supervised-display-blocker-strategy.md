# HO 独立审查：显示阻塞的处理路线与迁移前置裁决

## 1. 结论与权限边界

- 【推断/建议】推荐“双轨解耦”：显示侧先做一次有明确判别力的观测者隔离对照，不再重复 HM/HN 红帧；迁移侧先完成合法协议的静态设计及无 surface 合同验证，不让桌面前置阻塞 ABI、通知、媒体生命周期、托管 Tests 和最终 full 的准备工作。显示失败不授予跳过原内容断言的权利，也不要求撤销已经独立证成的非显示合同修复。
- 【报告引用】HD/HE/HF 已有 x64/x86 official/candidate 四格、consumer 红蓝绿真实可见通过，必须保留；其 actual consumer backend 未知，不能升级为硬件完成。HA 有限 Stdcall 和既有 fill 修复保留，不因本轮显示失败撤回。
- 【本轮事实】本轮仅专用文件/API工具读取与文档写入；构建、发布、测试、沙箱、终端、脚本均为零。没有新运行结果，没有生产、测试、窗口或默认行为修改。只写本报告和 next-session-handoff 的 HO 自有短节，不改 session-summary/issues，不覆盖 HP。
- 【推断】HN 的“下一唯一行动必须 ETW”和 HG/HK 的“必须原生前置通过才可开始迁移”是当轮调查/风险控制顺序，不是完整替换方案授予的全仓前置合同。可以调整调查顺序，不能据此删除旧用例、skip、改 expected、用事件或 producer backbuffer 顶替 consumer 内容。

## 2. 证据账本：观察与不能推出的结论

下列路径相对于 WpfGfxShape；仓库官方参考源码路径相对于工作区根。标签“原件”只指本轮直接读取的范围，“报告引用”不冒充本轮重新实跑或原件全文复核。

|编号/等级|定位与观察|结论边界|
|---|---|---|
|E1 原件|artifacts/hj-present-control/readback/x64-native/Results/hi-delivery.jsonl 1–6、13–17：task80257012…，Format22/count2/interval1/behavior38，首红 Present0/state0，frame accepted=true，backbuffer/duplicated/front 为 FFFF0000，client/screen255|至少一次同类 Ex DISCARD 链真实成功，非硬件永久不可用；本轮未重审全部三帧/fresh stdout|
|E2 原件|artifacts/hm-positive-anchor/readback/x64-native/Results/hi-delivery.jsonl 12–21：首红 backbuffer正确、stateBefore/Present/after=141953144，GPU完成，frame rejected，清理 refs0/0、Destroy1、基础对照未执行|08760878 是 S_PRESENT_OCCLUDED，不是 S_OK；GPU完成不证明桌面消费；22行系统模块长尾截断不计全审|
|E3 报告引用|completed-work/replacement-supervised-display-source-state.md，尤其 active 路径/session/精确首红和结尾：CCD active、UNLOCK、Default、屏保0、LUID/monitor对应，仍 occluded，无 DXGI fresh|配置及采样时会话状态已证，不证明扫描、电源或目标重定向面消费；不能把全部失败泛归环境|
|E4 原件+报告引用|artifacts/he-candidate-consumer/readback/x64-candidate/Results/he-x64-candidate/hb-consumer.jsonl 35–39、45–61：三 consumer pixel=255/16711680/65280 且 accepted，事件后 flush/内容及 detach。HD报告37–68、100–160补充 HD/HE/HF身份、四格和refs|消费者真实内容历史成立，不只是事件成功；四格汇总主要引用 HD 报告，未重新全读四份TRX/原件|
|E5 原件|Tests/WpfGfxShape.ComAcceptance/D3DImageTests.VisualCache.cs 55–100：普通 Create9、DISCARD/count1；native expected00563412 在 connect前；connect(1)。120–151：PresentPacket未设Event即入批。205–250：92字节target清零，context/DPI/flags未填写，仅alpha设1|前置确实先于任何consumer命令；共享夹具存在可静态指出的合同缺项，不是显示恢复就自然修复|
|E6 原件|Tests/WpfGfxShape.ComAcceptance/D3DImageTests.HbSurfaceConsumer.cs 78–116、155–205：先独立native窗口，再consumer窗口；partition/async；每次duplicate/separate Present/commit/wait/flush，随后独立consumer颜色断言|native前置与consumer不是同一HWND/surface；前者是观测条件控制，不是后者协议的必要命令|
|E7 原件|src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/InterOp/D3DImage.cs 650–729：duplicate事件、独立batch，注释说明批次迟交可能导致Lock死锁|独批/事件存活有实际时序依据；不能把“返回即完成”作为等价替代|
|E8 原件|src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/d3dimage.cpp 235–295：Present后取得software bitmap，替换引用；Cleanup即使失败也SetEvent并CloseHandle|有效事件及所有权有依据；事件完成不证明Present成功。局部 pISoftwareBitmap 无初始化是源码风险线索，不证明安装servicing二进制具有同一UAF或此次显示失败由它导致|
|E9 报告引用|replacement-supervised-shared-consumer-contract-fix.md 1–34及32–43；shared-consumer-revalidation.md 1–37：迁移试过后因原native颜色前置失败撤回；HK未进入connect|本轮旧consumer未覆盖，既不能宣布迁移有效，也不能说迁移实现被动态否证；旧 E_HANDLE/Destroy风险未闭|
|E10 报告引用|replacement-supervised-interop-stdcall-fix.md 1–47：HA双架构有限DR/生命周期/兄弟及fill相关通过；明确SharedSurface、Ex等未测分支|保留本类Stdcall修复，不外推全部COM、x64Event0、HW或恢复|

【推断】HM/HN 在真实 Present 前就已报告 occluded，削弱“只有像素比较器错”这一单因解释；但仍不能排除窗口/创建线程生命周期或捕获初始化影响。HJ与HM跨task，不能把差异归于某一个字段。HL的基础9 Present0与Ex occluded同样不能证明基础9消费正常。

## 3. 是否遗漏生命周期、窗口、设备合同

### 3.1 已有依据、应修而不是继续随机调参

- 【原件 E5/E7/E8】普通surface共享分支沿用同步connect、Event0及合批Present；目标协议参数还保留零值context/DPI/flags。应按适用路径做合法协议迁移，而不是先通过所有 native 模式才允许分析这些缺项。
- 【报告引用 HD 37–52】真实窗口target需合法 DPI context映射、GetDpiForWindow/96、flags0xC、setup flush、初始化invalidate。HB显式实现已修这些输入并取得四格内容成功；旧共享代码并未因此自动修复。
- 【原件 E8】duplicate是交给native的独立引用，原event需由托管持有；成功交接后测试不可再close同一个duplicate。发送失败的所有权必须单独确认，不能补一个猜测性的CloseHandle或重试。exact-once-close如未观测则继续披露，不把扩展hardening矩阵另立无限新门。
- 【推断】普通surface迁移与WIC/configuration/callback/transaction/multipleDevices的同步重入/HRESULT合同是两件事；不可机械全改async而悄悄丢掉原同步断言。每个受影响兄弟调用应按是否真实发送D3DImage Present分类。

### 3.2 有遗漏可能，但还没有因果证据

- 【原件】HiNativeDelivery.cs 123–176：HWND在WindowThread拥有，factory/device在测试线程创建，Clear/Present由owner调用；HM“owner提交”并不等于“device和focus window全生命周期均在owner”。VisualCache55–100也在测试线程CreateDevice。
- 【推断】仍有价值的审查点是CreateDevice/Reset/TestCooperativeLevel/最终释放相对focus HWND和WndProc的线程合同、重入、是否有同步跨线程窗口调用；0x26含MULTITHREADED并不能替代逐项认证。需先核匹配SDK/官方文档与实际helper完整调用链，当前未读到SDK原文，不能宣称已违反某条文档强制要求，更不能立刻再跑一个“换线程变体”。HJ相同类设计成功，已反证它是无条件充分故障原因。
- 【报告引用 HM】HJ duplication initial为HR0/鼠标初帧，HM initial为887A0027，后续也无fresh；这是明确外部状态差异，不是“初始化顺序已完全等价”的证明。
- 【推断】比再取一遍active/unlock更有收益的是区分“捕获初始化/观测者参与导致或暴露occlusion”和“在没有捕获时也没有桌面消费”。现有多轮都把DXGI先初始化/drain嵌入提交时序，尚未有独立无duplication观察者对照能裁决这一点。
- 【结论/推断】未发现可凭当前原件直接宣布已定位的窗口/设备生产根因。不修改HA、fill，也不把参考源码潜在UAF升级为servicing已证因果。

## 4. 前置门合法性：分开“协议修复能开始”和“内容可验收”

|历史要求|裁决及依据|
|---|---|
|必须有非空、新帧、真实consumer内容及必要时序/释放|保留，完整方案§3/4/6/9；事件、flush、producerbackbuffer均不可替代|
|旧native00563412断言、原geometry/sampler/shader/mutation/cleanup必须保留|保留原测试，E5及完整方案禁止删现有失败；它失败时该旧路径仍记阻塞/未覆盖|
|旧native前置通过才允许声称本次原consumer验收通过|合法：未进入connect就无consumer证据；禁止拿HJ、HB历史跨task代旧场景通过|
|先让旧native前置通过才可以静态设计、修正协议或验证无surface控制链|代理自设顺序，不是API因果前置；可以另建有界验证任务，不能将它报作原测试通过|
|HM首正锚失败就不做后续同类参数变体|当轮合理止损，继续保留该事实；不是禁止另一个具有独立终点和新信息的诊断控制|
|FLIPEX必须过才可迁移普通surface|无依据，HK已明确DISCARD/普通Present；FLIPEX不作本路径额外门|
|必须ETW完成才能推进任何验收|无依据；ETW是内部因果证据的升级手段，不是ABI/无窗口通知/托管Tests的依赖|
|必须补完所有性能/hardening再继续|无依据，完整方案§2/9；但有效输入崩溃、无限等待、资源不收敛仍是关键缺陷|

【建议】以后将“原场景完整内容验收仍阻塞”与“某个协议更正可独立保留”分列，而非每次显示失败就把全部合法更正逆向撤回，再重复相同迁移劳动。当前任务无改代码权限，本报告不执行该变更，也不追认HG/HK撤回为错误操作。

## 5. 旧同步 Event0：可以先做什么，何时允许保留代码

- 【原件 E5/E7/E8】Event0不是官方SendPresent协议；源码明说同步channel不应走ProcessPresent，native又无条件SetEvent/CloseHandle。测试夹具必须先约束自己发送合法输入，不能要求候选以容忍非法夹具来证明兼容。
- 【报告引用 HA】已有SW target通道的创建/dirty/Present/Detach内容通过可复用为设计与回归依据；它不证明新的async HWND迁移，也不证明独立x64 Event0问题已关闭。D3DImageTests.cs 595–623显示已有duplicate首帧后又Event0的兄弟用法，需按分支审查而非全局搜索替换。
- 【建议】可先交付静态协议表，再经单独授权在现有ComAcceptance隔离角色验证 partition→async connect→channel→无surface D3DImage资源→有效duplicate独批Present→commit→有界event/flush→删除→disconnect/deinit。无surface分支只验证控制协议、事件和释放，不伪称原surface消费、Available、backend或显示成功；先确认该资源状态是合法输入，不强造空consumer冒验收。
- 【建议/保留门】若仅修改无surface协议路径：需要官方/候选同版同架构有界证据，身份、失败/关闭路径及相关既有回归通过，所有内容测试仍在且fail可见，方可作为有限协议修复保留。若改到普通surface共享路径：无surface通过不够，至少须覆盖实际surface交接/dirty/Present/Detach等受影响合同；HWND内容仍未覆盖时只能显式隔离候选迁移，不能默认启用并称验收完成。
- 【风险】async改变完成点、回调线程、关闭在途生命周期；可能出现event已signalled但内容没到、duplicate泄漏或错误释放、send失败后所有权不明。保留有界超时和cleanup，首崩溃/死锁停止扩大，不能用删除Present或close猜测压掉问题。是否让尚未获实际surface证据的更正进入默认路径由主代理协调，不由HO越权决定。

## 6. 推荐路线及两个备选

|路线|新证据收益|复杂度/权限|风险与完成条件|
|---|---|---|---|
|推荐：显示观测者隔离 + 协议/非显示并行|区分capture耦合与独立桌面停滞；迁移不再绑住全部验收|中低；现有exe C#调用系统API、交互desktop及同进程窗口权限；不需内核trace|对照本身不算consumer通过。完成条件：一次成对结果能选下一分支；非显示按各自合同交付，不降低原内容门|
|备选1：经合同审查后的focus HWND同线程全生命周期对照|仅当SDK/实际调用链发现明确线程缺项时，判别生命周期合同是否决定状态|中；新显式隔离角色，不改参数组合|不得因为“owner”听起来正确就跑。先有文档依据和唯一变量；通过仅该合同，原旧consumer仍需复测|
|备选2：有权限的目标关联DWM/DxgKrnl短trace/换已证显示端|定位source抑制、目标surface未latch/discard或scanout/电源等内因|高；管理员或明确trace授权、匹配manifest及解码能力，必要时Hyper-V宿主协作|采集缺失事件不能证明没有提交；只围绕单次固定合同。无匹配目标/时间关联即停止，不扩ETW平台或随机重启|

【推断】推荐不是“再跑旧红帧”：保留设备参数不变，新增独立GDI可见内容标尺并控制DXGI观测者的生存期，目标是反事实区分，不追绿色。推荐与HP若触及相同测试文件，主代理先分配文件所有权，HO本轮不抢写。

## 7. 下一最小可执行任务（供主代理另行授权，不在本轮运行）

**题目：同一交互会话内，一次无duplication/有duplication的隔离进程成对对照，带独立GDI桌面消费标尺。**

1. 文件/API：只在现有ComAcceptance增加显式诊断partial并复用NativeRun/监督角色；读HiNativeDelivery.cs、WindowThread及duplication helper完整实现和SDK合同，不改公共helper或旧用例。固定HM的Create9Ex DISCARD/Format22/count2/interval1及全部窗口/Present字段。现有包HA依赖原样保留，不重发生产。
2. 权限：能在同一WinSta0/Default创建和销毁自己的窗口、调用D3D9Ex/DXGI/GDI；无需管理员，不改电源、会话、registry、驱动或显示配置。运行前批准成对诊断总预算和有界超时；本任务当前不提供执行许可。
3. 角色A无duplication：不创建DXGI duplication；创建独立小GDI窗口，通过owner WM_PAINT/InvalidateRect/UpdateWindow绘制两个不同非黑标记，保留原native窗口位置尺寸且不相互覆盖。采window GetPixel和对应screen GetPixel，记录QPC、HWND、PID/TID。DwmFlush仅作顺序辅助。按固定合同创建native设备，记录Present前/后CheckDeviceState、Present返回和window/screen/front内容、释放，不用GPU完成替代内容。
4. 角色B有duplication：同exe另隔离进程，先初始化/drain一次duplication，再执行相同GDI标记和native提交；GDI内容也用屏幕区域像素与LastPresent>标记submission对应，排除鼠标帧。记录实际模块身份及LUID/monitor；保留所有断言和失败结果，两个诊断角色独立终点，A内容失败不伪装为B的跳过理由，崩溃/超时则停止扩大。
5. 预期区分：H观察者耦合预测A可见/非occluded而B失败；H共同消费停滞预测A/B的GDI与native均不消费。若GDI两边可见但native两边occluded，则反证“整个桌面不可更新”，转向D3D生命周期/重定向或API合同，不直接归环境。若GDI可见但dup无fresh，仅限制duplication观测链；native窗口颜色独立保留。若两边native均成功，只证当前能力恢复，不解释历史原因，下一轮才原旧official consumer。
6. 停止条件：各角色一次，不改参数/重试追绿；首崩溃、超时、跨进程会话/LUID不一致、窗口互遮或无法建立独立GDI标尺即停止并记不具判别力。两边均失败不再红帧重跑；输出成对原件/identity/监督/TRX与一张分类表，择合同审查或trace升级。角色A没有duplication是诊断变量，不得被标作正式HW验收或替代旧fresh门。

【推断】即使此对照不能唯一定位DWM内部原因，它至少回答“没有DXGI观察者时该桌面及该D3D目标是否仍被拒绝”，这是HN active/session查询没有提供的新信息。两角色顺序执行仍有时间/外部状态混杂：A成功B失败只支持观察者耦合假设，不单凭一对就证明因果；两边失败也不唯一证明环境故障。GDI成功不证明D3D HW backend或scanout；仅证明该时间段独立桌面内容可更新。

## 8. 确需环境能力时的最小条件与验证

- 【建议】需要的是一个可实际消费目标内容的交互显示端，而非笼统“activeconsole”：同一目标HWND关联到CCD的LUID/source/target，外部显示客户端/虚拟机控制台持续连接并能观察提交前后两个标记；会话不切换、无锁屏/屏保，窗口未最小化/遮挡；有实际可用D3D设备。该条件须由成对内容/API事实验证，不能只由操作者称已打开控制台。
- 【建议】若操作者确有证据表明控制台断连或显示端暂停，只改变该已证缺项，采改变前后时间及连接状态；固定合同验证一次，不能把重启或切换RDP当碰碰运气的通用疗法。HN没有证明这个缺项，故当前不下达该修复。
- 【建议】若低成本对照仍不足，授权C#直接系统ETW API或已有专用采证能力，限定一个固定提交窗口；要求PID/TID/HWND、LUID/source/target、QPC映射，Present token/surface→DWM latch/consume/discard/occlusion→目标scanout/flip/电源/session。事件号与原因字段按该Windows manifest确认，不杜撰。内部原因不可得就标未知，不能用全局DWM帧数补齐；普通权限不够就交外部授权采集，不写脚本替代。

## 9. 可并行推进的小任务（不消耗本轮运行预算）

|任务|现有文件/证据入口|可交付与边界|
|---|---|---|
|二进制与所有权|HA报告及InteropDeviceBitmapExports相关独立ABI/DR夹具；两正式项目既有导出/QI/布局测试|盘点未测兄弟，独立声明、callback/COM身份/合法buffer边界；不要求ESP、不把HWND失败作为所有ABI运行前置|
|通知/托管Tests|Tests/WpfGfxShape.Tests/AvCompositionNotifierTests.cs 1–38已直接读：注销锁与重入；AvVideoFailureTests、BoundsCheckProtectionTests待专人核适用性|可优先无设备/无窗口单元合同；不冒称替代真实Available、HWloss或持续媒体|
|媒体生命周期|ComAcceptance/MediaExportsTests.Lifetime.cs 1–48直接读：provider引用和callback关闭；Ownership/CallbackRebind/RuntimeBinding待逐项检查|provider/QI/关闭/注销与在途回调准备及独立运行；需解码/时钟/文件能力的项单列，不把无HWND等同无所有外部依赖，持续视频内容仍保留|
|合法协议|本报告§5，D3DImage.cs与d3dimage.cpp原件|无surface控制合同和发送失败所有权审查；不使用空consumer抵扣surface/内容项|
|最终full准备|完整方案§8–10、两个正式项目现有发现/分包/identity/监督与比较流程|预先核实际模块/运行时匹配、两架构依赖、TRX和业务原件回收、零测试负控、超时收敛；可运行的无显示项无需等待显示根因，但不能以filtered结果冒full|

【建议】每个小任务有独立文件所有权和有界结果，不改共享默认测试基础设施；生产未改不重发扰SHA。最后源码修改后，仍须 WpfGfxShape.Tests / WpfGfxShape.ComAcceptance × x86/x64 Release 的最终完整适用full，包含真实内容、时序、资源、resize及实际device-loss。旧full或本报告不能绑定新SHA，不能把失败改不适用以关门。

## 10. 本轮阅读范围与收束

- 【本轮事实】current-execution-rules 1–27全文、current-stable-baseline 1–20全文；完整替换方案通过1–29/30–65/66–89/90–120补读覆盖全文。handoff只读最新HN/HM/HL/HK/HJ及本轮HO短节，未读500余行历史全文。
- HN/HM已读取开头及末节；最初大范围返回有超长截断，HN中部部分只按可见内容与报告引用处理，不冒全字符审阅。HG/HK相关开头与末节、HD 1–35/37–68/100–160、HA 1–47（开头长返回有截断）为聚焦阅读；HD其余历史和HF末尾未全文重审。
- 原始证据本轮只读§2列明片段；HE业务最初误定位Results根返回不存在，按报告正确run子目录已读到原件，不把误路径说成原件缺失。HJ/HM原件未全审TRX、stdout与大Diagnostic。官方源码是仓库参考，未与servicing反汇编绑定。
- 源码只读VisualCache 1–55可见片段、55–100、120–151、205–250、290–340；HB 1–66可见片段、78–116、155–205；HI 1–42、123–176；D3DImageTests.cs595–623；官方D3DImage.cs650–729、d3dimage.cpp235–295。未宣称完整生产调用图或SDK原文审计。
- 【最终裁决】显示关键阻塞仍在，完整替换未完成。实质处理是收敛到一个可判别的低成本对照、撤销无依据的全仓串行前置、合法协议分层保留并并行推进非显示项；不是新增性能/hardening完成门，也不是把所有HW失败归环境。
