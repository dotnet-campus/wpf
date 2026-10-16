# wpfgfx 剩余缺口关闭总计划

本页是唯一“当前剩余工作”清单，不另建竞争看板；运行身份只见[基线](handoffs/current-stable-baseline.md)。本轮Docs/Ledger清理及清理工具验证不计生产进度；历史有限Green须区分原物已清理与当前未复验。九包中的“剩余”含未证验收，不等于已定位九处生产缺陷；只按原合同及真实失败决定后续代码修改。

## 已生效合同

[DEC-0004](decisions/DEC-0004-process-lifetime-module.md)：DLL进程驻留，卸载/重载/热替换不适用，资源清理不豁免。
[DEC-0005](decisions/DEC-0005-x86-x64-acceptance-scope.md)：仅Windows x86/x64的全部Debug/Release正式矩阵；ARM64为NotApplicable-UserScopeDecision，非Passed、不再阻塞。
[DEC-0006](decisions/DEC-0006-installed-official-runtime-contract.md)：已安装官方完整runtime、同版同架构原版/候选独立对照。10.0.12是历史身份，不再待澄清或外部补包；历史golden/失败不改。

**整体未完成，且不只剩外部输入。** 两MILCMD required/parent仍Blocked，stage4Closed=false，总门Red。下表是原正式合同的有界合包，不新增“所有WPF”或任意故障门。

## 当前剩余工作：唯一有界清单

| 包 | 已完成可复用 | 真正仍需实现或验收 | 依赖 / 自主或用户 | 相对规模 |
|---|---|---|---|---|
| R1 runtime定位与普通回归 | 官方同版对照机制、历史golden及4224/1/4225原失败 | 修 `Tests/WpfGfxShape.Tests/SystemVideoSampleTests.cs:27–32` 直接加载历史JSON NativeDll；分离历史身份与当前官方runtime定位、校验架构/来源/实际模块、拒绝候选自身；专项后普通回归，不skip或改golden | 可自主工程修复；复用已安装官方runtime，不等用户10.0.12；本轮未改代码 | 小 |
| R2 x86/x64完整ABI与配置 | x86 Shared/CLR有限实测、当前同SHA strict与有限lifecycle、正式MTP smoke | Debug/Release×x86/x64逐入口调用约定/ESP、布局、COM/vtable/QI/refs/callback、PE/resource/version/symbol、生产真实加载；最终same-SHA related/full/strict；缺player单独记R8，不以PE集代行为 | 自主实现/验收；依R1与正式构建产物，R8缺口可分账不阻其它入口 | 大 |
| R3 可观测HW基线与33失败分账 | 原TRX、native staging成功及会话观察、原版/native control失败证据 | 先建立正常original/native可观测成功前置，再按[矩阵](final-acceptance-remaining-matrix.md#33项失败分账)逐类诊断；区分Present状态、桌面取帧、背景像素与候选消费者差异，修实际发现的工程/产品根因 | 优先自主诊断；确需x86/x64交互会话/GPU/权限时请求可访问验收环境，不采购ARM64、不盲重跑追绿 | 中 |
| R4 A/B真实HW内容与shader/cache | 已闭software primitive/image/nested/Blur/Shadow；有限冻结/owning/key证据 | 原A FrozenGraph/Realize整树事务、image/visual/cache消费者、两sampler/constants/malformed/BadShader/fallback、generation/per-device、失败保旧/恢复重试及逆序引用；真实成功/失败像素因果 | 自主实现或补正式验收；依R3有效control，按原A/B来源不另加333清零门 | 大 |
| R5 D3DImage/backend恢复 | 有限dirty像素/callback、双缓冲路径 | 真实HW color-source复用、adapter/availability/fallback、device-loss/recover、resize/Present失败、surface/device refs、无锁重入/并发注销/替换丢包与重试；不能用Detach/SW快照代GPU loss | 自主生产/验收；依R3真实HW环境及R4可达消费者 | 大 |
| R6 持续媒体与资源/线程失败寿命 | MediaPlayer四帧、真实事件/替换/关闭、typed owning及async drain有限Closed | 持续异步/HW通知→sample→composition/target→替换/重试→release/并行关闭；原可达UCE/初始化/worker/callback异常与refs清理；按合同保留FLOAT/HDR同格式oracle、glyph及perf尚未满子项，不重领已闭范围 | 自主实现/验收；依R1/R3。disconnect不是独立cancel；任意核内OOM/QI未知不自动升级必需门 | 大 |
| R7 GenProto C5/D4原闭包 | selected v143的12配置16TU预处理、C1–C4/D1–D3、378再生/275字节等价、consumer/factory/x86有限Satisfied | bilinearspan原来源及全部实际物理输入/组件/输出/消费者record闭包；历史CLR/generator/input/build authority binding及正式隔离差分充分性复核 | 有确缺来源需用户，收到后自主有界核验；6physical+3evidence不扩，333保守风险保留但不必机械清零 | 中 |
| R8 PRERELEASE record-packet player | 最新同SHA strict仅缺MilPlayer_Create/Process，Extra0；普通MediaPlayer有限Closed独立保留 | 官方包头/dispatch/destroy/ownership及匹配oracle到位后实现真实协议并验收；不能猜布局、造stub或用普通MediaPlayer代替 | 确需官方来源/权限输入；取得后自主代码与验收。详见唯一外部页 | 中 |
| R9 全项目正式集成与交付验收 | 软件有限E2E及两轮3STA/36帧零差、各joined6可复用 | 七阶段/A/B/C、E2E-01–09逐项完整x86/x64配置/能力矩阵；真实PresentationCore原/候选独立对照、持续HW/媒体、关闭/退出、完整Ledger最终状态及偏差；最终同身份full/strict不借旧run | 自主集成/验收，依R1–R8适用条件；来源缺项分账。不是仅文档打勾或再跑smoke | 大 |

## 最优立即顺序

1. 先R1运行时路径工程修复并专项→普通回归，再启动R2 x86/x64真实配置回归；不等历史patch。
2. 同时R3以正常original/native control建立有效HW观察，随后合包推进R4/R5；R6复用已有有限闭包补持续/可达失败寿命。
3. 并行只索取R7/R8真正缺的来源；到位后有界核验/实现，不恢复全仓候选穷尽扫描。
4. 用R9汇聚全部适用门；每次生产变更绑定最终产物与真实运行，不把40/40 startup作为full，不把C5 audit当GenProto46新跑。

## 七阶段正式完成条款（保留，不以导航或有限Green代替）

| 阶段 | 正式完成要求 |
|---|---|
| 1 HW backend | 设备/manager/资源拥有寿命、失败与恢复、线程边界及对应原生差集全部有生产实现与独立证据；有限Closed不豁免新可达差集 |
| 2 HW内容管线 | 全部规定内容/格式/采样/shader/cache消费者和成功失败寿命闭环；底层组件Green不能代完整内容链 |
| 3 软件呈现链 | 全部规定软件呈现、像素/变换/裁剪/格式与生命周期合同及消费者闭环；直接组件不能代PresentationCore |
| 4 generated protocol | A/B/C、独立field/enum/pack/size、producer/consumer、required及physical records完整闭包；有限验证不能关闭未满足条件 |
| 5 resources/UCE | connection/channel/resource/batch/dispatch/scheduler/Compose/Present/target全链及正式可达失败、取消、线程、callback、逆序释放；5A布局/5B寿命/5C集成各有证据 |
| 6 meta/API与DLL | 6A生产创建/外部桥、6B完整公开导出/ABI语义及strict集合、6C资源/初始化/退出/架构与支持边界；不得以有限lifecycle替全矩阵 |
| 7 E2E与替换 | 7A E2E-01–05、7B E2E-06–08、7C E2E-09及全部适用架构/配置/能力矩阵、完整Ledger与正式总验收满足后才取得交付替换资格 |

## 必须关闭的A/B/C与真实消费者差集

[主题合同](plan-module-contracts.md)的A FrozenGraph/Realize、B cache/shader失败、C GenProto required，以及UCE、双缓冲/D3DImage、媒体、FLOAT/HDR、glyph、perf/通知各具名条件是本计划组成部分；不是用九包摘要替代或扩大原条件。

## 自主依赖与外部前置

具体交付规格只在[唯一外部页](all-completion-external-prerequisites.md)维护。历史binding、bilinearspan来源与官方player合同确缺不等于其余生产消费者/恢复/E2E全停工。模块驻留不豁免Release、关闭、线程与callback静止。

## 完成定义逐条追溯

| 原条件范围 | 权威位置 |
|---|---|
| 七阶段正式门 | 本页七阶段表 |
| A/B/C与消费者、全迁移历史§9 | [主题合同](plan-module-contracts.md)，包括“全迁移完成标准” |
| 独立E2E-01–09，不以E2E-00代替 | [逐项合同](e2e-executable-contracts.md)、[证据规范](09-test-evidence-and-e2e-contract.md) |
| 测试完成定义 | [测试§12](testing-comparison-and-acceptance.md#12-完成定义) |
| 生产Ledger完成定义 | [Ledger§11](ledger-records-and-acceptance.md#11-完成定义) |
| 历史来源逐条承接/最高原则 | [追溯](governance-authority-traceability.md)、[章程](00-migration-charter.md) |

## 持续收口与最终门

完整族交付需原生合同、真实成功/边界/失败/关闭证据及最终同SHA相关/full/strict；不降required、raw、阈值、skip或golden。不造私有ABI/stub，不以SW或clear/readback代真实HW命令因果。独立分母不相加、不编完成百分比或人天。旧轮流水退出当前总计划，证据仍按[历史索引](history-index.md)原位保全。
