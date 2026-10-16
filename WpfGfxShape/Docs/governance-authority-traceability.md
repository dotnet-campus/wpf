# 文档治理：source → current 条款追溯

最新合同修订：[DEC-0005](decisions/DEC-0005-x86-x64-acceptance-scope.md)将下表/历史合同全部ARM64必需门覆盖为用户范围NA（非Passed），当前仅x86/x64完整矩阵；不是无证删除要求。原Ledger字段保留并显式解释覆盖，parent仍Blocked。[DEC-0006](decisions/DEC-0006-installed-official-runtime-contract.md)明确已安装官方完整runtime同版同架构独立对照；10.0.12历史身份不改，不再待澄清或补包，见[版本来源](windowsdesktop-version-provenance.md)。

只追正式条件，不恢复来源中的状态/下一包。按正文操作、结果、失败/寿命与矩阵核对，不以标题相同判等价；这是文本审计，不是生产验收。
来源：[历史总规划](history/plans/migration-master-plan.md)§9/批次7–14、[阶段独立审计](investigations/audits/S-20261014-008-independent-plan-audit.md)“有限分母与完成定义”、[GenProto冻结审计](handoffs/completed-work/GenProto-finite-closure-audit.md)、[原E2E编号入口](09-test-evidence-and-e2e-contract.md)。现行条款含其链接主题页。
旧七阶段未保留独立逐字快照；以现存总计划七行、冻结审计及历史批次交叉核对，不声称重建拆分前全文。原E2E旧章标题保留，但无拆分前版本可逐字diff；以下核对的是仍可读取的历史要求与现行完整操作合同。

| source 条款/必须保留的语义 | current 承载与核对结论 |
|---|---|
| 历史§9-1 完整机器Ledger、全部生产范围最终状态/证据 | [主题合同](plan-module-contracts.md#全迁移完成标准)第1项；[Ledger完成定义](ledger-records-and-acceptance.md#11-完成定义)六条件，不以直接编译数代分母 |
| §9-2 全目标架构/config ABI/layout/COM/callback/resource/version/symbol | 主题合同第2项；[ABI验收](abi-lifetime-binary-and-gates.md#13-验证门)逐entry九门及binary身份 |
| §9-3 逐文件追溯、无未记录重构/算法替换 | 主题合同第3项；[实现章程§3](charter-implementation-contract.md#3-最强实现约束逐文件近似直译禁止提前重构)保持分支/锁/释放顺序 |
| §9-4 原/候选规定差分全矩阵 | 主题合同第4项；[比较§6/10](testing-comparison-and-acceptance.md)exact/批准容差、架构/config/环境，不放宽golden |
| §9-5 真实PresentationCore E2E全矩阵 | 主题合同第5项；[T5](testing-levels-and-gates.md#t5真实-presentationcore-e2e)未改调用方、独立原/候选与像素 |
| §9-6 P0关闭或用户批准明确范围例外 | 主题合同第6项；[DEC-0004](decisions/DEC-0004-process-lifetime-module.md)仅卸载不适用，非通过；x86/资源清理不减 |
| §9-7 满足全部才称兼容替代 | 主题合同末项，替换为交付资格，不是热换DLL |
| §9末段 现代化/性能/重构另立计划基线 | 主题合同末项及章程§1/3保留 |
| 原阶段1 HW backend设备/manager/资源/线程/恢复 | [七阶段表](remaining-gap-closure-plan.md#七阶段正式完成条款保留不以导航或有限green代替)第1行；[A/B](plan-module-contracts.md)真实HW/SW、per-device失败寿命，有限Closed不豁免差集 |
| 原阶段2 内容/格式/采样/shader/cache成功失败消费者 | 七阶段第2行；A冻结事务、B sampler/constants/malformed/type/padding/Ddx/inputscope/HDR/compiler完整合同 |
| 原阶段3 SW像素/变换/裁剪/格式/寿命 | 七阶段第3行；[软件呈现](migration-coupled-domains.md#软件呈现)所有权、FLOAT/HDR消费者及E2E-04/05，不以组件代PresentationCore |
| 原阶段4 generated协议及历史批次7 producer/consumer | 七阶段第4行；[C](plan-module-contracts.md#c-genproto-required)、[生成协议测试](testing-comparison-and-acceptance.md#7-生成协议测试)冻结ID/布局/golden/router/factory/负例及隔离重放 |
| 原C ProcessCreate错误/引用/关闭，InitializePen ABI/dash/stride | 主题合同C本轮显式恢复；逐physical依赖签名/required更新，有限测试不关闭satisfied/stage4Closed |
| 原阶段5/8A–F connection至命令/消息闭环 | 七阶段第5行；[Resources/UCE](migration-coupled-domains.md#resourcesuce)本轮补明5A=8A、5B=8B–D、5C=8E–F职责及E2E-02/03，禁止未实现命令假成功 |
| 原阶段5 失败/取消/线程/callback/逆序释放 | 七阶段第5行；[UCE差集](plan-module-contracts.md#uce)正式可达失败/异常不由FIFO或PE闭包代替；disconnect不是独立cancel，不新增无来源取消门 |
| 原阶段6A/历史批次12 backend选择/创建释放/外部桥 | 七阶段第6行；主题合同自主依赖生产meta/factory/bitmap/stream/WIC/codec桥；[批次12门](testing-levels-and-gates.md#4-批次门禁)backend selection T3/T4 |
| 原阶段6B/历史批次13 全公开ABI与8/7差异 | 七阶段第6行；[ABI§11/13](abi-lifetime-binary-and-gates.md)差异裁决、真实语义/全部架构，strict有限集合不等行为通过 |
| 原阶段6C startup/shutdown/PE/资源/版本/符号/架构 | 七阶段第6行；ABI binary逐格记录、测试生命周期首次/并发/失败/退出；卸载仅依DEC-0004不适用 |
| 原阶段7A E2E-01–05 | 七阶段第7行及下列01–05独立合同，依生产6A–C与原/候选隔离，不以probe代替 |
| 原阶段7B E2E-06–08 | 七阶段第7行及06–08真实HW/D3DImage/媒体能力矩阵，不用SW或fake provider顶替 |
| 原阶段7C/历史批次14 E2E-09、全部文件状态/偏差/阻塞 | 七阶段第7行、主题合同全迁移标准及09；happy path不构成完成 |
| 原E2E-01 version/minimal COM | [01正文](e2e-executable-contracts.md#e2e-01-version--minimal-com)：正确/错误版本、QI/vtable/AddRef/Release/null/ownership/双运行时；spike不代真实factory，生产唯一MTP直接AOT |
| 原E2E-02 connection/channel | [02正文](e2e-executable-contracts.md#e2e-02-connectionchannel)：Same/CrossThread、marshal、close/commit/destroy/disconnect、错序/重复/部分初始化/线程身份 |
| 原E2E-03 resource command | [03正文](e2e-executable-contracts.md#e2e-03-resource-command)：create/addref/duplicate/send/release、payload/malformed/back-channel、32位ID/bytes/router/factory/失败码 |
| 原E2E-04 software pixels | [04正文](e2e-executable-contracts.md#e2e-04-software-pixels)：最小SW bitmap/target、确定几何/颜色/变换、规范化像素逐字节或批准规则 |
| 原E2E-05 真实PresentationCore bitmap | [05正文](e2e-executable-contracts.md#e2e-05-真实-presentationcore-bitmap)：DrawingVisual/DC/RTB/CopyPixels、矩形/线几何/透明/变换，同app隔离比较像素/HRESULT/异常/事件/退出 |
| 原E2E-06 hardware/HWND | [06正文](e2e-executable-contracts.md#e2e-06-hardwarehwnd)：真实呈现、GPU/driver/RDP、create/reset/lost/fallback/关闭；不猜无原生证据ResetEx |
| 原E2E-07 D3DImage | [07正文](e2e-executable-contracts.md#e2e-07-d3dimage)：D3D9 surface、version/dirty/detach/fallback/loss、composition/render callback/UI Dispatcher、detach后无新callback |
| 原E2E-08 media | [08正文](e2e-executable-contracts.md#e2e-08-media)：资产/组件、open/event/frame/position/stop/close/shutdown/退出、apartment/state/thread/timeout/callback；缺能力不是实现通过 |
| 原E2E-09 lifecycle | [09正文](e2e-executable-contracts.md#e2e-09-lifecycle-closure)：多Dispatcher、重复/显式关闭、finalizer/SafeHandle、退出、Release/channel/player/初始化失败/注入/线程/callback静止；卸载NA不再列门 |
| 全部E2E独立输入输出/原候选身份/能力/架构/config | E2E正文尾段与[run合同§4–8](evidence-run-contract.md)、[能力/失败§10–14](evidence-capabilities-and-failures.md)保留manifest/原始结果/独立比较/重放/超时；低级不代高级 |
| 历史§8 无Ledger/测试定义不得编码 | [编码门§4](migration-implementation-gates.md#4-编码前门禁)、[Ledger§5/8/10](ledger-records-and-acceptance.md)必须先有记录/映射/required，缺项先补，不以注明缺口放行 |
| 原A冻结/两阶段/回调/失败保旧/后验键/逆序释放 | 主题合同A逐项保留真实HW+SW旧新帧，不重复领取devicekey有限交付 |
| 原B generation/cache失败政策/OOM及真实寿命 | 主题合同B保留non-OOM/BadShader与原生有限fallback表，协议解析/pass-through不关闭完整compiler消费者 |
| 原C authority/ARM64/core managed/X86独立推导 | 主题合同C保留field/enum/pack/size、实机、physical/required；外部包内容只链接唯一清单 |
| 消费者双缓冲/D3DImage/媒体/FLOAT/glyph/perf | 主题合同六同名子节保留全格式、重入/并发/替换/loss、真实指针、同格式oracle、严格raw与OOM、合法72字节及最终同SHA验收；有限历史不外推 |

结论：上述可读取来源未发现其余正式条件被降级；两处显式缺项已恢复。冻结unload旧门只由既有DEC-0004覆盖，不新增决定；无历史逐字快照的部分不宣称字节级等价。路径/旧编号标题按文本核验，实际渲染边界见[最终报告](docs-governance-report.md)。
