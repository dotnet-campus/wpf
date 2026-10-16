# 最终验收剩余矩阵

本页按正式门验收，不另立任务清单；唯一有界九包见[总计划](remaining-gap-closure-plan.md)。身份/分母只见[基线](handoffs/current-stable-baseline.md)。本轮执行Docs/Ledger清理及工具验证，没有生产发布或GenProto full；下表有限成果和33失败分账属于历史，不冒充本轮回读原物。未证项表示须实际验收或依诊断修复，不预判均为生产缺陷。

## 现行适用性

| 条款 | 裁决 |
|---|---|
| Windows x86/x64 | 全部Debug/Release、七阶段、A/B/C、布局/ABI/COM/refs/callback/资源/失败/退出、独立E2E及最终同身份strict/full保持 |
| ARM64全部编译/布局/运行/工具链/机器 | **NotApplicable-UserScopeDecision**，不是Passed/Satisfied，不计阻塞或用户输入；DEC-0005覆盖冻结字段 |
| DLL unload/reload/热替换 | DEC-0004不适用；Release/关闭/线程退出/callback静止/初始化失败清理继续必需 |
| runtime版本 | DEC-0006：已安装官方完整runtime，同版同架构原版/候选独立对照；10.0.12历史身份不改，不再待澄清或外部补件 |

## 正式门与验收差集

| 原门 | 已证范围可复用 | 当前未满 / 关闭条件 | 归属 |
|---|---|---|---|
| 阶段1 HW backend | 既有设备/资源和有限生命周期证据 | manager/adapter、真实设备与资源拥有、线程边界、失败恢复全链；device-loss/recover与Present/resize失败不可由SW替代 | R3/R5，自主+有效HW环境 |
| 阶段2 / A+B | 有限冻结/key/owning、software效果已闭 | 整树冻结/realize事务、真实image/visual/cache、sampler/constants/malformed/BadShader/fallback、per-device/generation、失败保旧/重试及引用逆序 | R4，自主；依有效control |
| 阶段3 SW | primitive、image/nested/Blur/Shadow有限场景 | 全部既定格式/参数/消费者/失败寿命与同格式FLOAT/HDR oracle；不重领有限已闭，也不以8bit覆盖float | R6/R9，自主 |
| 阶段4 / C / CLOSURE | C1–C4、selected v143 12配置16TU预处理、consumer/factory/x86有限Satisfied | 原bilinearspan项目/依赖来源、完整实际physical input/generator/frozen output/consumer records；C5仍Blocked。6physical+3evidence不等于完整原闭包；333只是风险，不新增穷尽门 | R7，确缺来源+自主复核 |
| REGEN-DIFF / D4 | D1–D3、378再生/275 byte-exact及所列378等价 | 历史CLR/generator/input/build authority binding与正式隔离差分充分性；当前hash/active头不替历史来源 | R7，确缺历史来源 |
| 阶段5 / 5A布局/5B寿命/5C集成 | 有限typed owning、FIFO、async drain/关闭、x86 CLR | 全规定connection/resource/batch/dispatch/scheduler/Compose/Present/target与原可达失败/线程/callback/逆序清理。disconnect不是独立cancel，不造任意核内故障门 | R2/R6/R9，自主 |
| 阶段6A meta/API桥 | factory、WIC/bitmap/media等有限公开链 | 原规定backend创建/选择、生产桥及消费者完整成功/失败寿命；有限导出存在不等于全语义 | R2/R6，自主 |
| 阶段6B ABI/strict | 当前同SHA strict仅缺MilPlayer_Create/Process、Extra0；有限lifecycle真通过 | x86/x64全配置逐入口签名/调用约定/布局/COM/错误/所有权/资源/版本/symbol；PRERELEASE record-packet协议另记R8 | R2自主；R8需官方合同 |
| 阶段6C初始化/退出 | x86 Shared有限与正式MTP startup smoke | 全配置首次/并发/失败初始化、资源/线程/callback静止、显式关闭/退出真实证据；无模块卸载门 | R2/R6，自主 |
| 7A / E2E-01–05 | 真实PresentationCore有限软件像素与公开链 | 同版同架构独立原/候选、全部规定配置/格式/生命周期与普通回归；历史路径加载失败先修R1 | R1/R2/R6/R9，自主 |
| 7B / E2E-06–08 | 媒体四帧、事件/替换/关闭有限Closed | 持续HW、D3DImage、shader、媒体sample/composition/target、失败恢复与拥有链；原版/control前置未成功不能批准候选通过 | R3–R6，自主+环境 |
| 7C / E2E-09 | 两轮各3STA、36帧零差、各joined6 | 全适用架构/配置、持续并发/重复关闭/退出及完整资源/线程/callback合同；不重复领取同一有限场景 | R2/R6/R9，自主 |
| 历史§9/完整Ledger/P0 | 现有有限Closed/Satisfied、生产身份与原证据保留 | 所有生产范围最终状态/证据、逐文件追溯与偏差、全部适用ABI/差分/E2E/P0同时满足；才取得交付替换资格 | R9，不能靠文档更新关闭 |

两MILCMD required与parent仍 **Blocked**，`parentBlocked=true`、`c5Satisfied=false`、`stage4Closed=false`，总门 **Red**；用户范围以[DEC-0005](decisions/DEC-0005-x86-x64-acceptance-scope.md)为唯一架构决议；驻留与runtime分别归DEC-0004、DEC-0006。已清理overlay不再作为状态或合同来源。

## 33项失败分账

下表保留既有只读审计按 `artifacts/sandbox-readback/dispatcher-fixed-full/Results/dispatcher-fixed-full.trx` 类名/参数行、Failed条目及选定错误/调用栈的归并；本轮未重新回读历史运行原物或逐条复核33项。历史full为1616/1583pass/33fail/0skip，绑定当时3F291生产身份，不是当前full或本轮重跑：

| 类别 | 数量 | 既有审计层次 / 后续处理 |
|---|---:|---|
| Stage4 HW cache geometry/configuration/WIC/lazy reentry/write-lock/multi-device | 15 | 2+2+2+5+2+2；实际多为背景 `0x00F3F3F3` 对期望像素失败。尚不足认定候选15个产品bug，也不因control失败假通过；须有效原版/native基线后继续A/B差分 |
| PublicHwnd D3DImage | 8 | 两个非shared像素路径及六个shared/callback/shader路径；部分在 `PresentNativeControl` 断言状态失败，尚未证明进入候选shader/cache消费 |
| Shader generation replace / malformed fallback / channel-device recreate | 3 | 同样保留normal control前置及具体调用栈；不把尚未到达的fallback/recovery记已验或已证产品bug |
| SecondSampler Visual/Cache | 3 | 两输入期望 `0x00563412`、实际背景；真实消费者差分仍未通过，不能只凭已有协议解析关闭 |
| OriginalPublicHwnd | 1 | 原版公开HW可观测control失败，不是候选代码回归证据；必须先解决有效观测前置 |
| NativeGpu topmost / frontbuffer-client (swap1,swap3) | 3 | staging已读到正确色，但桌面仍背景，另有桌面duplication取帧调用失败；不是候选功能断言，也不能据此泛判所有失败环境原因 |

合计33，原“32个候选bug/全部环境”都不是合法结论。先分清运行前置、原版/native正常输出、测量链与候选差异，再做有界工程/产品修复；禁止碰运气重跑追Green。

## 完成标准与来源

[总计划七阶段](remaining-gap-closure-plan.md#七阶段正式完成条款保留不以导航或有限green代替)、[A/B/C及消费者合同](plan-module-contracts.md)、[E2E逐项合同](e2e-executable-contracts.md)、[测试完成定义](testing-comparison-and-acceptance.md#12-完成定义)、[Ledger完成定义](ledger-records-and-acceptance.md#11-完成定义)与[历史§9追溯](governance-authority-traceability.md)均保持，不因本矩阵合包降标准。
历史40/40仅startup smoke，不给旧full改run；GenProto46/46与后续C5 audit均非本轮通过。当前仅做GenProto最小构建及清理工具验证，不运行full；历史GUID依赖尚未恢复。历史原文/失败/golden见[隔离索引](history-index.md)，不全局替换。
