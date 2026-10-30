# 完整替换主要缺口收敛审查（文档阶段）

## 裁决与唯一下一主门

**完整替换仍 Open。下一主门只推荐：将既存 KN 的 baseline/resize 生命周期两个合同迁移到当前 x86 候选实际 SHA；先发现参数 UID，再分别运行 false/true 两体，首错停。** 不是重跑旧 KN17，不新增测试代码，不发明微 ABI/错误门，不启动显示探针。x64 同门另预算；本阶段一概不运行。

本阶段源码/项目/测试代码修改、build/test/publish/Native 发布/沙箱/脚本/终端/再委派全部 0。只读文档、当前 clean 的 aot.sha256 及程序集公开 API 元数据，写文档。没有读取生产/测试源码，没有新执行或重新散列二进制。下文旧运行数据均按具名报告引用，不冒本次亲读历史全部原始 TRX/业务文件。

恢复入口和共享状态以本报告及其顶部摘要为当前优先级；各长文下方连续“下一门/NotRun”段均是历史快照，不再逐段追随或重复全读。不删除历史报告或原件。

## 1. 身份与已闭有限切片

|身份|x86|x64|
|---|---|---|
|当前 candidate actual（既有执行报告）及本次 clean 清单值|15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620|54BC62FBBEF9A55EA75E49AF5EFDCEC70F7C8C2808BDF4E2C80B916CF92D5ADD|
|当前 COM test SHA|D8A5F57FE4A61CC28459A89DCB50DC75E0ADE80E1D0AEDC35D7E18AC5069A7BB|4A8356201F062B01161744C6B3BBF7D9AB7DB5909F6F12A9389806FFB6B6ED43|
|test MVID|e207b681-4c15-453e-b0a4-a1690fa3db78|d41adfe8-8904-4c9b-887d-8d1f5979ba46|
|official actual（独立角色历史）|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|

当前有限证据：双架构 postfailure attach/isolation、健康 async 同 partition bitmap/Visual duplicate/source close/目标更新/最终所有权、directClose C6/C8 和 official O6/O8。runtime 为 .NET 10.0.13、对应 X86/pointer4 和 X64/pointer8。

依据：source-rebuild-x86/x64-closeout、async-shared-ownership-x86/x64-closeout、async-drain-directclose-x86/x64-closeout。最后两份报告全文已分段读取；其四体均 TRX1/1/1、callback 恰一且退出、正常关闭，candidate 真实 callback 重入及本次已有 worker 退出断言通过。official caller 关闭 sibling 不是重入 oracle。directClose 只有 MTP 会话 0，没有独立 OS 数值 exit/监督/分离 stderr；不倒签 Exit0。ownership/partition 的监督账独立，不补 directClose 退出证明。

普通健康 create/duplicate/close 并非完全没验：共享 ownership 已在同一新 SHA 覆盖异步两 channel、真实 Bitmap95、独立 Visual39、源关闭后目标新更新；C6/C8 已覆盖健康 open/committed Destroy。**不再为这些已覆盖点重跑同合同，也不新增另一个健康管理微门。** 当前值得迁移的是另一个既存公共资源/target 生命周期合同，而不是扩大错误矩阵。

## 2. 包 1–8 主要缺口工作表

“可本地”只表示已有输入/入口可以有界推进，不表示本阶段授权执行或无需真实设备。

|包|当前候选直接证据|历史其他 SHA（不可迁移）|真正未覆盖/外部前提|可本地推进与排序|
|---|---|---|---|---|
|1 当前完整基线|上述有限合同与身份/来源链|IO managed 各4256；JK COM各1802；KD COM各1814；KN各17|当前最后源码对应双架构完整回归未有证据；历史 HW32 不是当前失败总数|先完成本报告唯一受影响2体，最终再授权正式完整矩阵；不拼旧计数|
|2 二进制兼容|管理 completion、共享表项/实际 WIC ownership、正常关闭与 callback 有限切片|旧 PublicAbi/COM/布局/refs、KN producer refs/snapshot/通知|当前 SHA 全适用 ABI 及调用后状态未闭；Register OOM/异常仍未证，但不造注入器/环境错误等价门|优先既存 resource46/producer 引用与关闭合同迁移，即下一门|
|3 内容/真实调用方|ownership 中目标 Visual 新更新与 flush，不等于 bitmap 像素消费|旧曲线/fill、E2E、HD/HE/HF、旧 shader 内容；KN 无 consumer 像素|当前真实 WPF producer/consumer 内容闭包、HW backend、组合/格式/媒体新帧缺口；显示能力外部|显示冻结；不可拿下一门 snapshot/refs 当内容或 HW 成功|
|4 时序|postfailure 管理与业务结果分流；ownership 必要偏序；C6/C8 candidate 重入/本次 worker|旧通知/注销/HU/HW/HX/HY，其他 SHA|长期交错、广泛在途关闭与当前所有必要偏序未闭|下一门观察正常 target 释放/通知；不再扩等待/线程探针|
|5 资源|真实 source/duplicate 最终释放、callback 和正常 cleanup 的有限证据|KN producer refs/末四 Release；旧资源与媒体序列|预热后 CPU/GPU/对象/线程样本趋势、持续媒体资源未闭|下一门迁移另一类 producer surface/target 生命周期；不冒长期稳定|
|6 恢复|没有当前 resize/loss 内容恢复直接证据|KN 20B3/40D0有限原地生命周期；KD/KP像素失败；旧 loss 触发未成立|真正 resize 后内容、policy/generation/cache、真实 loss 后新帧及持续媒体；需设备/显示/媒体能力|下一门只缩小 resize 生命周期缺口；像素/loss 必须独立保留，不能绕过|
|7 未知差异/有效性|这些有限合同不是合法随机图/序列或趋势闭包|旧局部负控/固定场景|合法约束随机差分、seed/缩减/负控及循环资源完整证据未闭|固定受影响回归先行；不在本门追加 fuzz/新代码|
|8 最终交付|没有最终完整替换资格|全部旧 full 各自有效历史|最后源码实际产物的双架构 Release 全适用矩阵及官方独立支持证据、原始内容/生命周期审阅|保留最终候选/官方四格；条件未齐不跑 full 追绿、不按通过率关门|

## 3. 历史 HW32 与旧 full：身份、分母不混用

完整方案 §8.3 当前原文仍为“历史32项”，本次未发现应替换为其他数字的最新定义。其可追踪名单见 `Perf-sync-compose-contract-20261015.md` 与 `replacement-x64-hw-capture-package.md`，不是现在仅剩32个失败：

- #1–15：cache geometry2、configuration recovery2、WIC replacement2、lazy WIC reentry5、write-lock retry2、two-device2。
- #16–23：PublicHwnd8；#24–26：shader generation/malformed fallback/channel-device recreate3；#27–29：系统 native GPU topmost/front-buffer3；#30–32：second sampler3。
- 四项集成旧 full：候选627F2732…、official94C064…/10.0.1，1429执行/1397过/32败。之后 perf 候选202A8C9F…、同旧 official 的1429/1429使该名单各项在当次 Passed；不证明所有根因关闭，更非当前候选证据。
- 首包 x64 2BFC4AC2…/10.0.9：COM1677/1675/2；HW32专题有30条当次 Passed、两条 native FrontBuffer(1/3)失败，不能将30条当独立 official 同场景/backend闭合。x86 3CC69D4F… full 未完成，675/114只是中途进度，非分母。

|后续历史账|实际结果与身份|不可作何推断|
|---|---|---|
|IO managed|各4256/4256；managed prod x86 5265E624…、x64 7FE8B39C…，不是 NativeAOT；test DC761366…/E96D259E…|不得当15A545/54BC62 managed/Native当前通过|
|JK COM full|各1802执行/1709过/93败/0skip；actual x86 D5CE061D…、x64 0C16A66C…|93均有完整失败表，但不是当前失败数，也不全归环境|
|KD/KE COM full|x64 1814/1774/40；x86 1814/1810/4；actual x64 E331526A…、x86 7E0936D2…；test0CE3CC0D…/8AF9EEB3…|44条架构记录/40唯一ID不是HW旧32同一个集合；不能减掉KN/其他局部通过推新full|
|KN/KO|各17/17，actual x86 20B3F944…/x64 40D09398…；test45656891…/6BB784E9…|17=生命周期2+BW1+CK10+软件snapshot2+generic refs2；不是17 resize像素，更非当前候选|

KD的40/4分类仍有价值：x64 首帧前置23、新提交桌面帧3、隔离首帧失败2、公共target像素2、native非零Present状态9、loss未触发1；x86 AV1、候选resize后像素1、official resize前shader1、loss未触发1。候选 x86 FF80→resize64×24→FF00 与 x64未过首帧不同；official x86未到resize，不能称“双方resize后坏”。真实 loss firstCooperative0 未满足预期丢失状态，恢复未覆盖。KP20B3原resize ID在native前置即停，不抵销更旧已到resize的内容问题。

旧 DG transport未知、旧CZ执行前full阻塞、JK/KD已实跑亦分别保留；不得挑某个旧“尚未full”覆盖后续实跑，或拿任何旧full移绑当前候选。KN原件报告所引 `WpfGfxShape/artifacts/kn-inplace-resize/...` 本次精确读取目录/父TRX未找到，故只引用具名KO审阅，不声称重新验原件、也不重跑补历史。

## 4. 为什么选既存 KN 最小2，而不是KN17/再造门/直接像素探针

修改合同见 `replacement-postfailure-attach-minimal-fix-static.md`：Register/Destroy completion只报告管理工作自身结果；Create先保留全局表项、接受前/接受后各归其所有者回滚；Destroy等待不再丢弃真实注销结果；SameThreadChannel Register检查插表。其交叉风险包括正常 channel 仍能注册、提交resource46、在尺寸更新后保持 producer引用、正常Destroy实际释放target/通知并完成连接收敛，而不是只有错误分支可用。

KN false/true使用同一正常公共 channel/target/producer 生命周期，分别给创建/关闭基线与32×16→64×24更新后关闭。它检验资源46与真实D3D producer surface/device持有，和已有 Bitmap95/Visual39 async共享所有权不是同义重复；false是true的必要无resize对照。旧KN是同样合同在20B3/40D0成功；迁移到15A545/54BC62提供修改后的跨资源族证据，有依据地回归，不受“禁止盲重跑旧17”误伤。

它不是 async 管理completion的新全面证明（原KN合法初始化采用connect1），也不是 resize后shader/桌面像素验证。已有async两合同/ownership/directClose负责其有限路径；两者互补，不互相冒充。snapshot非空/producer HAL均不证明consumer HWbackend或shader像素。

## 5. 可直接执行的后续授权规格（本阶段 NotRun）

### 5.1 复用与精确项目

两 clean 路径均相对工作区，且均已通过公开API元数据读取确认方法存在：

- x86 `WpfGfxShape/artifacts/async-shared-ownership-x86-r1/clean`
- x64 `WpfGfxShape/artifacts/async-shared-ownership-x64-r1/clean`
- executableRelativePath：`WpfGfxShape.ComAcceptance.exe`，不是源码入口、publish根或历史readback。
- 准确 FQN：`WpfGfxShape.ComAcceptance.D3DImageTests.WhenKnTargetResizesInPlaceThenProducerLifetimeSurvives`。

|精确参数体|当前clean存在依据|为什么必要|当前身份运行状态|
|---|---|---|---|
|(System.Boolean false)|两DLL公开签名bool resize；KO/退役覆盖表明确false/true两行|创建、不resize、正常Destroy producer持有/通知基线|没有已认证的当前SHA该体结果，按NotRun处理|
|(System.Boolean true)|同上|真实尺寸改变后target/producer引用、通知和末释放；本次所有权/销毁改变的交叉合同|同上；不能把旧KN17迁移成已通过|

元数据只证存在和签名，不证明 DataRow属性、唯一UID或实际成功。本次没有新 list-tests，既有AsyncDrain发现diag仅筛其两方法，不能借其UID给KN。**false/true 当前真实参数UID未知，必须先发现，禁止猜Name/Id或用FQN执行整方法。**

### 5.2 唯一下一主门的预算与字段

建议另授权 x86 一组有限门：build0、COM publish0、Native0、测试代码0；沙箱最多3次（发现1/体0，随后false与true各1独立任务/各1体），无重试，执行体上限2。发现无确切两参数节点即结束，不改代码/重打包。每体前亲读上体全证据；本组结束即停。x64不在该预算，x86两体有限通过后才另授同规格。

发现调用：sourceDirectory为上面x86 clean；arguments数组顺序为 `--list-tests`、`--filter`、`FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenKnTargetResizesInPlaceThenProducerLifetimeSurvives`、`--results-directory`、`Results/current-kn2-discovery-x86`、`--diagnostic`、`--diagnostic-synchronous-write`、`--diagnostic-output-directory`、`Results/current-kn2-discovery-x86/Diagnostic`、`--no-progress`。timeoutSeconds=120；outputRelativePath=null；localOutputDirectory=`WpfGfxShape/artifacts/current-kn2-x86/readback/discovery`。完整保存工具原返回及实际调用对象；从diag核方法、System.Boolean false/true、各真实TestNodeUid及Discovered状态。若0行、多余行、只有方法节点、缺参数/diag、超时/启动失败，即停，不跑体/整方法/help。

发现后每体直接同clean正式exe，arguments为 `--filter-uid`、`该次发现中与false或true对应的真实UID`、`--report-trx`、`--report-trx-filename`、`child.trx`、`--results-directory`、`Results/current-kn2-LABEL-x86`、`--diagnostic`、`--diagnostic-synchronous-write`、`--diagnostic-output-directory`、`Results/current-kn2-LABEL-x86/Diagnostic`、`--no-progress`；LABEL依次baseline、resize。timeoutSeconds=180；outputRelativePath=null；localOutputDirectory分别 `WpfGfxShape/artifacts/current-kn2-x86/readback/baseline`、`.../resize`。UID无实值前不能提交体请求。现有supervise-exit只传--filter，不能塞UID或新增runner；直接UID运行的退出证明按实际工具/diag层级报告，未给OS exit就不声称OS Exit0。

### 5.3 回读、停止和有限完成标准

每任务即时归档完整可见返回/准确参数及全任务回收；先读原件再下一体。必须准确单UID TRX total/executed/passed=1/1/1、同方法/参数、同UID Passed terminal、会话0；正常业务和cleanup全部完整，不能只读摘要。actual candidate必须是该task Native路径及15A545完整SHA、test D8A5/MVIDe207b681、X86/pointer4、实际runtime/WPF。不能用aot文件预检替代实际消费；如既有业务输出不足绑定actual或完整cleanup则报告证据缺口并停，不临时造探针。

有限断言沿现有测试/历史合同，不改阈值/expected：refs1→2→2、snapshot成功且非空/临时引用正确释放、正常设置期间无提前unavailable；true有真实尺寸更新32×16→64×24；正常Destroy/disconnect/deinitialize成功，销毁通知Available0/Version701及末bitmap/surface/device/factory Release0/0/0/0、窗口及cleanup完成。若当前产物确有target/device/manager与真实GetDevice/QI/GetDesc诊断输出，按原件另记；没有就保留该层未证，不能把旧KI_RESIZE_TRACE默认移绑或为了诊断重发Native。producer device不冒target device。

任一失败/身份不符/参数不唯一/多跑/零跑/缺原件/超时/崩溃/cleanup异常首错停，保留失败，不补跑、不加snapshot/generic/17整组、不改显示/等待/线程/取色，不转x64。两体通过只完成“当前x86公共target/producer创建和resize生命周期受影响回归”切片；不是完整resize内容恢复或完整替换。x64另预算并独立绑定54BC62/test4A835620/MVIDd41adfe8，不能借x86。

### 5.4 official 的必要性

本门不追加official：当前clean中公开KN方法是candidate合同，无对应同名official参数对照；既有KK official是另一夹具/历史test，不能把其重跑当KN同义合同。旧KK/KO official5EBD只作合法初始化/producer寿命先例，不冒当前新双边实测。当前directClose O6/O8也不是KN official。下一门是在当前候选迁移必要回归，并不授予新的official等价或全ABI资格。若失败，先按实际失败/身份报告，不临时追加official探针追绿。

## 6. 后续优先顺序与外部完成条件

1. 另授权上述x86发现→false→true；失败即本门收口并回报具体原件/受影响节点，成功也停止，不自动扩大。
2. 另预算x64同两体；完成后将有限ABI/资源/resize-lifetime格更新为当前身份证据，仍不关闭包1–8。
3. 显示/真实HW内容不由本门解决：需要能力提供方给出可交互真实桌面/目标适配器、可用真实producer→consumer呈现和原始内容证据的执行环境；按既存原shader resize合同授权，不再启动native-only/WGC/等待/线程/取色前提探针。原候选FQN `WpfGfxShape.ComAcceptance.D3DImageTests.WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive`；official对应 `WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive`。旧UID仅索引，当前也须按实际节点核定。标准是实际模块/架构、真实目标HWbackend/adapter、resize前后独立非空新内容及原FF80像素断言、真实尺寸和cleanup；前置失败保留失败/未覆盖，不以本门snapshot或generic冒像素。没有可用环境就外部Blocked，不要求主代理重新探索或再跑前置探针。
4. real-loss由能力方提供能记录旧device确实丢失而非仅新建失败的设备操作/环境；随后既存loss合同验证恢复、新内容、再次更新及清理。旧firstCooperative0不达标。媒体需真实可用源/解码能力、持续播放与新帧、恢复/关闭序列及资源样本趋势；单次通知或一次最终Release不达标。这些未有本轮能力证据，保持Open，不凭文档批准执行。
5. 最后源码不再变化且关键能力条件齐备后，另授最后产物的双架构Release完整适用矩阵与候选/官方独立支持证据审阅。原始失败/未覆盖/阻塞全部可追溯、无关键未解释差异，才可按方案§9裁决替换；不同SHA、不同分母和局部通过永不相减成新full。

本审查已经作出选门裁决，无需再开一轮无限静态审计。Register异常/OOM未知保留但不独立扩门；retail Closed、PRERELEASE私有player非目标、驻留/非卸载边界不重开。direct/public/WGC/KS/Ledger、旧HW32、IO/JK/KD/KN、旧20B3红/40D0隔离及所有预算账全部独立保留。显示冻结、真实loss/media/长期趋势/final full持续开放。
