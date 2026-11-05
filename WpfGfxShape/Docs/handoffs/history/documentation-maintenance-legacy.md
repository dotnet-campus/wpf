# 文档维护台账（串行专项任务1–4）

最新任务4实账见文末：首份2484行累计入口已原字节归档并缩为10行。任务1–3保留为历史快照，不把其归档0、待改或读取断点误当最新进度。全库正文审核及其余长档归档仍未完成。

## 范围与口径

治理根为 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape`，不是其父级 WPF 上游工作区。已递归枚举本根全部 Markdown、Docs 全部文件、Ledger 全部文件以及 .txt/.rst 名称。根无 README，项目入口是 [Docs/README](README.md)。父级 README 为上游复制文档，仅检查本项目导航，不把上游产品状态当本项目状态。

排除父级 src/Documentation/eng 等第三方或上游复制文档、artifacts/bin/obj 生成产物。Docs 内 .txt/.log 原始证据仍登记保留，不删、不冒规范。Ledger 的5个 JSON/JSONL 是机器证据/合同数据，不改。Code/NativeMethods.txt、Tests/CandidateResize/NativeControlHeader.txt、SceneHeader.txt 是代码或输入素材，不改。

目录枚举不等于正文审核，标题搜索不计全文已读。全文读取被截断的文件标为部分。任务1未完成的文件总数复点已由任务2的D0补齐，见文末；全库正文审核仍未完成，禁止宣称整体完工。

## 不遗漏的目录责任索引

以下按路径集合覆盖全部 Docs 文件，子目录递归包含；逐文件例外以本轮记录优先。

| 路径集合 | 分类 | 实际审核状态 | 下一责任 |
|---|---|---|---|
| Docs/*.md | 未审（规范/设计/累计状态混合） | 已枚举，按下表逐文件推进 | D2：规范一致性；D3：长档保全 |
| Docs/decisions/** | 未审（决策规范） | 仅枚举 | D2：逐条核与当前用户合同冲突 |
| Docs/handoffs/*.md | 未审（当前规范/状态/累计历史混合） | 当前入口2份已全文读；其余按下表 | D1/D3 |
| Docs/handoffs/history/** | 历史实证/索引 | README全文读；其余未审 | D3：累计历史保全与链接 |
| Docs/handoffs/completed-work/** | 历史实证 | 已枚举，未逐篇审 | D5：按文件名排序分批，每批最多15篇 |
| Docs/completed-work/** | 历史实证 | 已枚举，未逐篇审 | D4/D5：有限结论、身份、role边界，不改原事实 |
| Docs/history/** | 历史实证/设计说明 | 已枚举，未审正文 | D5：历史链接、覆盖关系 |
| Docs/investigations/** | 未审（设计说明/历史实证/兼容指针混合） | 已枚举，未审正文 | D6：含 active/audits/closed/superseded、GlyphProbe、INCOMPLETE-HW-CACHE-CLOSEOUT、RotatedFloatContract.Tests、TwoInputShader.Tests 全子树 |
| Ledger/** | 历史实证/机器合同数据 | 5文件已枚举，未审内容 | 后续仅文档侧解释，不改数据 |

## 本轮初始读取快照（下方收尾实账覆盖待改状态）

| 文件（相对Docs） | 分类 | 实际状态 | 操作/下一责任 |
|---|---|---|---|
| handoffs/current-comparison-contract.md | 当前规范 | 全文已读，待定点完善 | D1：桌面前置与role差异边界 |
| handoffs/current-recovery.md | 当前状态 | 全文已读，待改 | D1：保留唯一状态，分清本任务执行0与历史账 |
| handoffs/README.md | 当前状态/索引 | 全文已读，待改 | D1：去掉过时任务2调度 |
| handoffs/status-index.md | 过期误导 | 全文已读，待改 | D1：修复旧恢复路径与历史冒当前 |
| README.md | 过期误导/冗余待合并 | 部分读取，待续读 | D1：全文读后保留导航、不复制状态 |
| testing-comparison-and-acceptance.md | 当前规范 | 全文已读，待改 | D1：严格全像素、不开放容差 |
| handoffs/history/README.md | 历史实证/索引 | 全文已读，待改 | D1：明确未完成长档保全任务 |
| handoffs/current-execution-rules.md | 冗余待合并/过期误导 | 部分（1–24响应截断） | D3：完整分段读，原样归档回读后短指针；不得截断重写 |
| handoffs/current-stable-baseline.md | 冗余待合并/过期误导 | 部分（1–24响应截断） | D3：同上 |
| wpfgfx-complete-replacement-validation-plan.md | 冗余待合并/过期误导 | 部分（1–20响应截断） | D3：完整读后提取正式验证合同，历史保全 |
| next-session-handoff.md | 冗余待合并/过期误导 | 仅标题定位，未审正文 | D3：最高优先单文件保全任务 |
| handoffs/current-work-item.md | 冗余待合并/过期误导 | 仅标题定位，未审正文 | D3：独立单文件保全任务 |
| final-acceptance-remaining-matrix.md | 冗余待合并/过期误导 | 仅标题定位，未审正文 | D3：独立长档任务 |
| remaining-gap-closure-plan.md | 冗余待合并/过期误导 | 仅标题定位，未审正文 | D3：独立长档任务 |
| replacement-issues-and-plan-status.md | 冗余待合并/过期误导 | 仅标题定位，未审正文 | D3：独立长档任务 |

## 下一串行批次（尚未委派）

- D1（本轮）：入口与短合同，修改前全文读、修改后全文回读，新链接逐个核存在。
- D2：`00-migration-charter.md`、`charter-implementation-contract.md`、`charter-evidence-and-continuity.md`、`06-testing-strategy.md`、`testing-levels-and-gates.md`、`evidence-run-contract.md`、`evidence-capabilities-and-failures.md`、`e2e-executable-contracts.md`、`continuity-recovery-and-closeout.md`、`10-session-continuity-protocol.md`。交付：逐条修旧禁令/容差/恢复来源/执行授权，不更改历史结果。
- D3：按上表7份累计长档顺序一份一任务，另含完整验证计划；每份必须逐段无截断读完，原样复制并逐段回读，确认原文无丢失才把原路径变为短指针。归档中的规则明确受当前合同覆盖；不能完整处理就保留原文并登记精确断点，不声称已归档。
- D4：`completed-work/replacement-supervised-window-role-routing-fix.md`、`completed-work/two-test-projects-consolidation.md`、`completed-work/candidate-geometry-native-validation-r27.md`、`completed-work/replacement-next-feature-validation-r26.md`、`completed-work/candidate-resize-desktop-r13.md`。交付：历史事实保留，有限结论和role专属业务补偿不作为现行授权；链接可恢复性单列。
- D5：完成档与history目录剩余文件按名称顺序，每批15篇；每篇交付来源/结论范围/实际链接审核记录，不批量把旧红改绿。
- D6：investigations全部子树，逐兼容指针验证目标，区分设计与实跑，原始证据保留。

## 本轮收尾实账与未完成项

本轮全文已读既有项目文档18份：下面15份已改，以及 `handoffs/next-code-task.md`、`06-testing-strategy.md`、`testing-levels-and-gates.md`（后两者仍有旧基线/容差，未改）。部分已读4份：`handoffs/current-execution-rules.md`、`handoffs/current-stable-baseline.md`、`wpfgfx-complete-replacement-validation-plan.md`、`governance-authority-traceability.md`。其余均未审正文，不因搜索命中标题而计已审。新增台账1份不计既有已读分母。已归档0、删除0、代码修改0、执行0。

15份既有文档已实际修改（不只是新增导航）：

| 文件 | 分类/完成程度 | 实际治理 | 下一责任 |
|---|---|---|---|
| 00-migration-charter.md | 当前规范；本批已处理 | 历史E_NOTIMPL不冒当前；官方独立对照不是生产委托；新恢复来源 | D2复查主题实现章程 |
| README.md | 当前索引；部分治理 | 新旧状态分层，修旧基线冒当前与历史数字认证；历史摘录仍保留 | D3完整保全历史摘录后短化，不丢原文 |
| handoffs/current-comparison-contract.md | 当前规范；本批已处理 | 桌面前置失败归因边界、role仅身份/目录/取证 | 后续只集中维护合同 |
| handoffs/current-recovery.md | 当前状态；本批已处理 | 任务5历史账与本轮执行0分开 | 后续唯一产品状态源 |
| handoffs/README.md | 当前索引；本批已处理 | 去掉过时任务2分派 | 后续保持短导航 |
| handoffs/status-index.md | 当前索引；本批已处理 | 旧规则/基线/工作项不再冒现行 | D3保全目标长档 |
| handoffs/history/README.md | 历史索引；本批已处理 | 累计档案归档0、明确专门保全任务 | D3 |
| testing-comparison-and-acceptance.md | 当前规范；本批已处理 | 完整像素/业务严格差分、撤当前容差、旧矩阵不冒当前 | D2与其它测试页对齐 |
| evidence-run-contract.md | 当前规范；本批已处理 | 同冻结双进程、禁role业务差异/补偿、前置失败与比较器边界 | D2组织措辞仍须与两项目裁决核对 |
| charter-evidence-and-continuity.md | 当前规范；本批已处理 | 专用授权有效、文档轮执行0、新职责表 | D2清理其它规范旧职责 |
| continuity-recovery-and-closeout.md | 当前规范；本批已处理 | r6历史化、恢复顺序/职责/收尾不再写旧基线 | D2 |
| continuity-failures-and-decisions.md | 当前规范；本批已处理 | 当前值来源和桌面前置失败归因 | 后续核其引用来源 |
| 10-session-continuity-protocol.md | 当前规范；本批已处理 | 修唯一恢复链接 | D2 |
| completed-work/status-index.md | 历史索引；本批已处理 | 当前状态/任务边界链接修复，role补偿非授权 | D4/D5 |
| history-index.md | 历史索引；本批已处理 | 不再声称全库引用已修，历史build非本轮执行 | D5 |

以上本批已处理不代表每页全部深链/历史来源均重新认证。修改后逐份回读；末次追加修改另回读变更段。新增/变更导航目标已由目录枚举或实际读取确认存在；保留的父级历史链接另核 `../Docs/completed-work` 目录，Stage15/执行账/real-loss目标确实存在，不随意改成项目Docs下不存在的同名路径。

### 精确盘点范围与尚缺总数（任务未全部满足）

已经枚举：Docs根原有42份Markdown（新增本台账后43）；handoffs根16份；decisions目录7份；history递归6份；investigations递归28份Markdown及1份log；handoffs/history为1份README。完成档两个大目录也已完整递归列出，但尚未完成其独立计数复点，因此本轮**不能提供可信全库确切总数或待审总数**，此项仍未完成，禁止以估算补数。上述目录合计100份原有Markdown（42+16+7+6+28+1），不含两个completed-work大目录；不是全库总数。Docs外另有3份.txt代码/测试输入，不属文档正文；未找到.rst/.html/.pdf。

下一串行任务D0先补精确总数：只用目录工具重新列 `Docs/completed-work/` 和 `Docs/handoffs/completed-work/`，逐项复点Markdown及txt/log，再与上述100份核并；记录全库总数、已读18/部分4/其余未审数量，不调用脚本。父级上游README仅读到52行且截断，不计本项目全文已读或本轮修改。

### 下一批可直接交付的精确文件清单

D2下一批8份：`charter-implementation-contract.md`、`06-testing-strategy.md`、`testing-levels-and-gates.md`、`09-test-evidence-and-e2e-contract.md`、`evidence-capabilities-and-failures.md`、`e2e-executable-contracts.md`、`governance-authority-traceability.md`、`com-nativeaot-testing-method.md`。先全文读取；修严格容差/原版禁令/同进程加载/role补偿/旧当前来源；有界有限结论不扩张。governance页有疑似已删除audit目标、表格粘连，逐目标核查不猜。

D3逐文件顺序明确为：`next-session-handoff.md` → `handoffs/current-work-item.md` → `handoffs/current-execution-rules.md` → `handoffs/current-stable-baseline.md` → `final-acceptance-remaining-matrix.md` → `remaining-gap-closure-plan.md` → `replacement-issues-and-plan-status.md` → `wpfgfx-complete-replacement-validation-plan.md`。每份一独立保全交付，尚未完整读取，不声称已归档；前三个部分读取长档从第1行重新按字符容量分段，不沿用被截断响应末行作为已读断点。

## 任务2：D0精确复点与D2实际收尾

### D0目录实账

本轮重新枚举Docs各目录、两个完成档目录及Code/Tests的Markdown名称；Code/Tests无Markdown，Ledger仅5份JSON/JSONL，项目根无Markdown。排除上游父目录与生成产物。目录数量不是正文审核数量。

| 路径集合（互不重叠） | Markdown | 其他文档/原始证据 | 文件合计 |
|---|---:|---:|---:|
| Docs根（含本台账） | 43 | 0 | 43 |
| Docs/handoffs根 | 16 | 0 | 16 |
| Docs/decisions | 7 | 0 | 7 |
| Docs/history递归 | 6 | 0 | 6 |
| Docs/investigations递归 | 28 | 1 log | 29 |
| Docs/handoffs/history | 1 | 0 | 1 |
| Docs/completed-work | 397 | 2 txt | 399 |
| Docs/handoffs/completed-work | 342 | 2 txt + 3 log | 347 |
| 合计 | 840 | 8（4 txt + 4 log） | 848 |

两个完成档目录无下级目录。completed-work完整枚举末项为`x86-stroke-current-validation.md`，398项上限枚举停在`typed-lifetime-finite-20261015.md`并提示尚有结果；剩余末项已按名称单独核存在。handoffs/completed-work完整枚举末项为`wpf-reference-tiled-direct-build-blocked.md`，346项上限停在`wpf-reference-tiled-base-realization.md`并提示尚有结果；剩余末项单独核存在。均已取得更大上限完整列表，不把截断列表当总数。

非Markdown逐项：completed-work的`candidate-media-late-drain-ownership-r23-test-receipt.txt`、`replacement-next-feature-validation-r26-test-log.txt`；handoffs/completed-work的`S-20261008-163-publish-01.log`、`S-20261008-165-directional-evidence.txt`、`S-20261008-166-validation-evidence.txt`、`S-20261008-188-publish-01.log`、`S-20261008-188-tests-01.log`；investigations/GlyphProbe/Results的`fixed-evidence-validation.log`。全部保留原件，正文未审。Docs外3份txt仍为代码/测试输入，不计文档；Ledger5文件另计机器数据，不计上述848。

审核统计采用可核口径，不把“读过”说成“全链认证”：既有Markdown839份（840减新增台账1），累计全文已读24份=任务1的18份+本轮新增全文6份；部分3份（current-execution-rules、current-stable-baseline、完整替换验证计划）；完全未审812份。即24+3+812=839。含台账时为24份既有全文、3份部分、812份未审、1份维护记录。原始8份txt/log未审，另列不混分母。任务1的15份修改仍只是其已记录范围；累计既有修改23份=15+本轮8，归档0、删除0，不等于23份全深链认证。

### D2逐文件审核、问题与解决

以下8份均实际存在，逐份分段全文读取后定点修改，并分段全文回读；本轮未修改任何历史完成报告或失败原件。

| 文件（相对Docs） | 全文审核发现 | 实际解决 | 剩余边界 |
|---|---|---|---|
| charter-implementation-contract.md | 删除媒体委托时的E_NOTIMPL与全部Open冒当前；旧baseline/执行规则入口；独立PublicAbi项目歧义 | 历史状态明确历史化，改当前恢复/任务边界；两正式项目与同冻结双进程严格差分 | 不重新认证删除报告的运行证据 |
| 06-testing-strategy.md | 旧发布SHA/4224/1676及41fail冒当前；旧baseline权威 | 保留数字但明确历史快照；保护失败不擅自升级；当前结果/规则集中链接 | 累计矩阵与保护详档未全文复审 |
| testing-levels-and-gates.md | 批次9及编码合同开放tolerance；禁止子进程与独立PublicAbi旧口径 | 完整像素/业务严格差分；明确同冻结双进程与同exe角色；probe仅历史 | 批次表不是本轮实跑结果 |
| 09-test-evidence-and-e2e-contract.md | 旧baseline与并行权威措辞、旧承载标签 | 定位为证据主题导航，链接集中总合同/唯一恢复；辅助角色归两项目 | 主题深链不等运行认证 |
| evidence-capabilities-and-failures.md | 缺能力与前置故障可混归环境；exact/tolerance；旧能力未决清单与probe授权 | 检测证据/最早阶段/Unknown/NotReached正文规则；严格差分；维度不冒当前未决状态 | 原始能力检测未重新执行 |
| e2e-executable-contracts.md | 像素允许批准规则；媒体全部未实现与有限Closed冲突；组件缺失直接归环境 | E2E-04严格完整字节；媒体两个历史快照均有限；前置失败不无证归因 | 不新增媒体能力结论 |
| governance-authority-traceability.md | audit断链；批准容差；无依据整体审计结论；疑似表格粘连 | 撤销失效链接，保留原路径及缺失事实；修严格门与结论范围。E2E-03/04分段读取实际为独立两行，未改正常表格 | audits目录为空且本根同名搜索无结果；缺失原件与拆分前快照不能复核，不杜撰目标 |
| com-nativeaot-testing-method.md | 唯一运行器可能误解为同进程双实现；旧baseline/current-work-item；加载失败混业务失败 | 两独立业务进程与禁止双加载/委托/fallback/只测官方/补偿/生产测试耦合；更正恢复来源与前置状态 | 配置/包版本为既存文档声明，本轮只用文档工具未核源码/未执行 |

本批正文无旧次数预算可继承，也未新增预算、授权确认或第三项目。执行限制仍由当前合同集中维护；本任务的执行0不是撤销后续专用授权。

链接核查：8页保留/新增Markdown文件目标已与本轮Docs根、handoffs、decisions、history及完成档枚举逐项对照；目标存在不代表其正文已审。E2E编号锚点及本批分层锚点由全文读取核对；其余目标页深层锚点未全部重新读取，列为未认证，不能声称全库断链清零。唯一已核实缺失audit路径保留为非链接历史文本，不将history-index冒充该审计原件。没有新建重复入口。

### D3下一批（尚未执行、未委派）

仍按单文件保全顺序：`next-session-handoff.md` → `handoffs/current-work-item.md` → `handoffs/current-execution-rules.md` → `handoffs/current-stable-baseline.md` → `final-acceptance-remaining-matrix.md` → `remaining-gap-closure-plan.md` → `replacement-issues-and-plan-status.md` → `wpfgfx-complete-replacement-validation-plan.md`。每份从第1行按字符容量分段完整读，原文保全并逐段回读确认无丢失后才改原路径为短指针；历史旧禁令/预算/状态受现行合同覆盖。任务1的README历史摘录也仍待保全，不能遗漏。D4–D6保持原责任索引；decisions及其它未审规范仍须后续审核，不以本批8份代表D2全域完成。

本轮实际修改共9份：上述8页及本台账。源码修改0；build/test/publish/沙箱/程序/终端/脚本执行0；委派0。Overall产品状态不在台账更新。

## 任务3：首份累计文件保全未完成（非归档成功）

先全文读取本台账及 `handoffs/current-comparison-contract.md`，再小行段读取 `next-session-handoff.md`。原文件实测2484行：连续完整取得1–185，另完整定位读取1000–1002、1500–1501、2314–2320、2410–2484。未读区间为186–999、1003–1499、1502–2313、2321–2409。111–135响应在134行中途截断，134–145重读补全；不以截断响应重建内容，未发现确证不可读取的超长单行。100000/3000/2500行定位为空不计正文已读，末尾2484行实际取得。

本次没有完成全文读取、原文写入与归档逐段回读，故保留原件不写，未创建legacy归档；原路径2484→2484，缩短0行；归档0、删除0。本次并非已证明工具无法继续读取，而是全文保全工作尚未完成。专用读取单响应上限4000字符，15–25行仍可能截断；应按实际字符量缩小段，截断行单独重读。现有工具无直接复制/移动和可靠字节校验；后续若逐段文本复制，必须完整回读所有段、顺序/首尾/2484行，只能声称文本逐段核对，不声称byteexact或编码/换行/末尾字节一致。本轮无归档核对成功可报告。

实际修改仅本台账及 `handoffs/history/README.md`，独立索引登记读取覆盖与缺口，不向原件混入说明。Docs文本锚点搜索命中 `replacement-issues-and-plan-status.md:1633`；读取1629–1634及原文2314–2320确认其AW历史引用指向 `### 下一最小任务`，原件未迁移所以不改历史链接。不把本次文本搜索等同全库深链认证，也未批量改历史证据。

续办优先完成 `next-session-handoff.md` 的上述未读区间及唯一原文归档，再全段回读后才允许短兼容指针。之后下一独立单文件 `handoffs/current-work-item.md` 本次未读、未归档。不新增重复累计入口。任务2盘点840 Markdown+8其他=848保持；既有全文已读24、部分4（原3加本文件）、完全未审811，新增台账1，24+4+811+1=840；8份原始txt/log仍未审。绝大多数未审，不宣称整体完成。

本轮源码修改0；build/test/publish/沙箱/程序/终端/脚本执行0；委派0。写后回读两份新增记录，不冒称原文已保全。

## 执行边界

本轮代码修改0；build/test/publish/沙箱/程序/终端/脚本执行均0；不继续委派。后续专用执行授权有效，旧预算不移绑。当前状态仅维护于 [current-recovery](handoffs/current-recovery.md)，规则仅维护于 [current-comparison-contract](handoffs/current-comparison-contract.md)。本台账只记录文档工作，不是第二个产品状态源。

## 任务4：首份2484行旧入口实际归档完成

本任务采用独立固定文件C#保全程序 `Tests/DocumentationMaintenance`，不委派、不运行终端或脚本，不改生产/业务验收逻辑。辅助程序完整读取发布输入，严格UTF-8解码、行数校验、原字节Copy、完整读回及SHA256/逐字节比较。归档先回传真实目标并读首尾，再重新发布工作区尚未替换的原文、已落地归档及首阶段audit；确认两份全文一致且原文hash未变，才回传短指针。程序不删除文件，不接任意目录操作。

| 文件（相对Docs） | 实际完成量 | 字节与行数 |
|---|---|---|
| next-session-handoff.md | 累计正文迁出，现行规则只链接current-recovery/contract；已全文回读短文件 | 2484→10行，减少2474行；现477字节 |
| handoffs/history/next-session-handoff-legacy.md | 原字节完整归档；程序全文校验，人工首尾回读；不是全文语义审查 | 2484行、614775字节、UTF-8无BOM |
| handoffs/history/next-session-handoff-audit.json | 实际程序审计，phase=pointer，sourceUnchangedSinceArchivePackaging=true；已全文回读 | 20行 |
| handoffs/history/README.md | 真实归档导航、旧档剩余责任、相对链接基准与诊断副本边界 | 本次重整并回读 |
| replacement-issues-and-plan-status.md | 只修1633行AW历史锚点到legacy，不改其历史结论，未审其余正文 | 1处链接 |

原文及归档SHA256：`48D417B980439C99DD4DE736CCD0A85F5507B7DC9292425942C4F5BA5C8BF868`。
短指针SHA256：`1D3F413A929701144A902308A51FD641E009EAEBD26C25365DC126D71DB21CF6`。
hash边界：针对完整发布输入副本和沙盒归档读回；不声称直接在原位工作区计算hash。第二阶段重新打包真实工作区原文及已落地归档，与首阶段源hash和彼此完整字节比对，确认串行打包间原文没有变化。最终再次发布工作区短指针、归档和audit，verify实际返回verified=true；收据 `artifacts/documentation-maintenance/final/verification.json` 已全文回读，确认短指针10行/477字节及归档2484行/614775字节的hash与audit一致。

导航实查：短入口四个目标均实际读取；AW唯一搜索命中的旧锚点改到归档2316行 `### 下一最小任务`。归档内部链接保持原字节，仍以原Docs位置为基准解读；索引明确此边界，不伪称全库链接清零。归档完整读取保全不计为助手逐条语义审核，既有正文审计分母不因此虚增。

执行实账：专用publish5次；Windows沙盒8次（前2次因要求10.0.13而沙盒只有10.0.1未启动；后6次实际运行：archive3、pointer2、verify1）；build/test0；终端/脚本0；委派0；生产及验收代码修改0。辅助项目运行时改为沙盒现有10.0.1。首次失败原始错误：`FileNotFoundException: 沙箱执行完成，但拉取的任务目录中不存在指定结果：Output`；诊断回传确认.NET运行时缺失，不是原文不可读。

回传工具具体落点确认：localOutputDirectory下保留完整outputRelativePath，因此最初archive回传额外产生 `Docs/Output/handoffs/history`、`Docs/handoffs/Output/handoffs/history`，每处legacy+audit两文件，未删除；均是诊断副本，不是第二权威入口。修正程序输出层级后，返回明确为真实 `Docs/handoffs/history`；随后pointer回传明确为真实 `Docs/next-session-handoff.md`，audit亦明确真实路径。未覆盖其它Docs文件。原848文件盘点作为历史基数，本轮新增正式legacy Markdown1、审计JSON1，以及诊断副本Markdown2/JSON2，共新增6文件；不冒称原848全部治理完成。

下一批：`handoffs/current-work-item.md` → `handoffs/current-execution-rules.md` → `handoffs/current-stable-baseline.md`，本次均未归档。其后仍为final-acceptance-remaining-matrix、remaining-gap-closure-plan、replacement-issues-and-plan-status、wpfgfx-complete-replacement-validation-plan及README历史摘录；不扩展到未审规划正文。首份完成，不代表全库完成。
