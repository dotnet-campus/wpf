# 当前会话执行约束

> 本阶段只改Docs并只读来源审计，禁止终端脚本，不改生产/测试/配置/Ledger，不构建发布测试。下方构建运行表述均为既有轮次，不是本次执行。架构按[DEC-0005](../decisions/DEC-0005-x86-x64-acceptance-scope.md)仅x86/x64；ARM64编译/布局/实机不适用、非Passed、不再阻塞或请求。精确10.0.12未证用户需求，待澄清口径，详见[单一审计](../windowsdesktop-version-provenance.md)，不得改golden或擅选版本。x86/x64全部正式门不减，两required/parent仍Blocked。

## 最新覆盖规则

本轮只修改现有C5审计器的裁决元数据与完整报告回读校验，已最终build 0警告/错误、自包含audit发布、沙箱运行及17,377行完整回读比较通过；之后仅Docs/Ledger。未新增项目/测试/扫描范围、未生产publish/COMfull、未重跑46，DLL不卸载。

333风险原表保留：13已证build声明、92 build条件未知、227 include未知、1原项目缺失，NA=0。原group readiness/doneRule要求完整实际生成追溯及隔离差分，不要求保守候选穷尽；下文旧“继续全部逐边清零”不再是工作指令。不无证排除未知、不造历史authority或ARM64执行、不删除正式required。C5/两个required/父级Blocked；下一动作及确切补件只见当前工作项、唯一详档和外部页，无必需生产改动实证则不虚派生产任务。

## 以下旧轮描述仅为历史，当前以以上规则为准

本文只维护执行纪律；身份和分母见[状态源](current-stable-baseline.md)，下一模块见[当前工作项](current-work-item.md)。

## 当前 C5 原物对账执行边界

本次复用现有selected输入、12配置16必要TU的.i/result/hash及原MSB3202摘录，仅只读对账与文档更新；不新增源码、诊断项目、解析器或额外全量重跑。此前源码build/target/full46/46不是本次CL或本次新跑，身份只见[唯一基线](current-stable-baseline.md)。

[activeincludes清单](../completed-work/GenProto-C5-activeincludes.md)保留已实际到达路径、固定入口映射及具体未知。CL预处理成功不是完整对象编译/链接；bilinearspan显式引用四配置GetTargetPath失败不是header失败。不得拼项目、删引用、造生成合同、将未到达直接Excluded；原333审计行及888候选不扩正式6 physical+3 evidence分母。

仍可自主做原C5适用边到既有record/assertion的逐项审计；确缺原项目来源与D4历史binding属于真实资料边界，不与未完成审计混写。补件集中唯一外部页；ARM64缺工具链不以host替代。两transport packet/独立discard未证不自动新增原生成required门。

生产未改，无publish/COMfull或终端脚本；DLL驻留unload=N/A，资源/引用清理不豁免。C5及两个required/父级Blocked、stage4Closed=false，总门Red；原七阶段/ABI/寿命/E2E继续。以下旧轮描述均为历史，不覆盖本节。

## 历史 native-only 实际执行边界

本轮已修复审计真实原生源路径并实际构建、专项和full GenProto回归；机器报告完整解析四组原始Results、绑定同任务identity/TRX/RunID/raw hash。独立Ledger及详档已回读，短数字只见[基线](current-stable-baseline.md)。本轮不改生产、不再publish/COM full，无终端脚本、DLL无unload。

候选bitmap/create/delete/duplicate的writer→consumer有限实证成立，media公开SendCommandMedia真实像素成立但failure/discard最终引用未证。transport SyncFlush/Destroy的API功能与packet执行严格分层；后者未证。VisualCreate在node.cpp RIP/E_UNEXPECTED，不造producer。后续仅按[工作项](current-work-item.md)两个正式required ID有限剩余推进；原版native trace、外部authority、ARM64、完整runtime、player与HW前提不假解除。父级Blocked、stage4Closed=false，总门Red。

以下旧文档集成边界是历史，不能覆盖本轮已实际执行状态。

## 历史文档集成执行边界

本次只读取既有report、verification、独立Ledger及文档并编辑Docs；不代码、不改Ledger、不构建、不发布、不测试、不终端脚本。此前真实执行及回归身份与全部短数字仅见[基线](current-stable-baseline.md)，最新裁决见[x86 CLR独立详档](../completed-work/GenProto-x86-clr-wire-finite.md)，不将此前运行冒充本次新跑。

x86 CLR真实布局执行独立Satisfied，不再列未执行或下一任务；先前x86 Shared有限资源生命周期为另一合同，不能与CLR wire布局混用，也不外推ARM64、完整ABI或E2E。enum差集及padding结构映射保持Satisfied；padding同名null和字节内容未证边界不变。VisualCreate仅不支持分类审阅Closed，ordinary channel create不可替代，debug声明非retail执行。

下一真实自主项按[工作项](current-work-item.md)仅为既有native-only producer的packet-specific合法执行/传输/失败关闭证据及required/physical剩余明细；不重领已闭映射或x86 CLR。历史固定边与production conservative分类独立，未知不Satisfied、未到达不等于排除，全WPF保守图非正式分母。两个MILCMD required及父级Blocked、stage4Closed=false；既有有限Closed/Satisfied不降级。历史页原文冻结，由当前导航覆盖，不作当前任务来源。本次文档同步完成。

## 证据与验收纪律

- full / related / compare 各有独立分母与身份，不相加，不推算总体完成百分比。
- full 与前轮 AddedFailures / RemovedFailures 均空不代表 33 failure 已解；不能全部归环境，不盲重跑追 Green。
- native factory → GeneratedValueResources / GeneratedProtocol dispatch → 有限消费者 → 原报告，只能形成有限签名/物理映射；不得标全部 requiredSatisfied 或 stage4Closed。
- 同 SHA strict 未取得不得以历史代填；既有有限 Closed 不因此被重新打开，也不豁免正式余项。
- 必要 JSON/TRX/log/SHA/identity/pixels/生产 DLL 原物保全、只引用，不制造重复大包，不删除唯一证据或冻结 authority。
- [四外部输入](../all-completion-external-prerequisites.md)唯一页，不重复索取已有材料；10.0.1 建设版不冒充完整 10.0.12。

## 后续生产执行纪律（不是本次执行授权）

- 按[工作项](current-work-item.md)及总计划继续真正正式差集；不重领已闭typed154/15、async drain、factory、ordinary GlyphRun或software effect，不派无源码依据的独立cancel，不把增强核内故障作为无界门。
- 以完整生产方法族合并实现、真实消费者、成功/失败/关闭及证据；单参数、单补测、纯复验、文档 WIP 不作默认独立交付。
- COM 验收只使用 `Tests/WpfGfxShape.ComAcceptance` 标准 MTP 项目直接加载生产 NativeAOT DLL；禁止新增/恢复 ComAcceptance.Host，不创建或修改 .slnx，不恢复已删旧方案。
- 不以生产托管程序集、内部实现或自定义子进程冒充公开 COM 验收；完整方法见[测试约定](../com-nativeaot-testing-method.md)。
- 原生证据从 `src/Microsoft.DotNet.Wpf/src/WpfGfx/` 枚举读取，每次最多 400 行；不修改原 WPF/wpfgfx 源码。
- Silk.NET ABI/类型存疑时读取本地 2.23.0 源码；不猜 vtable、包头、glyph 私有模型、Reset/ResetEx、WOW64 或 ScrollBlt。
- 保持 Windows Stdcall COM ABI、HRESULT 首错、引用计数、失败逆序清理、确定性释放与释放后保护；不新增无原生依据的必需核内 OOM 门。
- 禁止 stub、skip、改 expected/golden/required、阈值放宽、SW 冒充 HW、clear/readback 冒充真实命令因果。
- 每轮真实生产交付新增独立 completed-work 档；不得追加或迁移旧 progress-completed-work.md。真实发布需记录序号、完整命令、项目/配置/RID、结果、路径及对应 SHA/TRX；未执行不得编造。
- 无原生依据不得扩展协议/ABI、设备/层/scene 生命周期；旧域限制不是永久停工门，已有完整合同的生产族可按总计划推进。

## 寿命、存储与正式总门

[DEC-0004](../decisions/DEC-0004-process-lifetime-module.md)：DLL 进程寿命驻留，不卸载/重载/热替换；不豁免资源、引用、线程、callback 与初始化失败清理。
已有 dummy back-buffer、target/scene 及有限失败清理按原档范围使用，不反复领取。
存储回收仅限可确证本轮可再现副本，先保全原证据并回读核验；禁止删除未知文件、正式 publish、生产 DLL 或唯一证据。无测量不报告回收容量；本次不执行回收。
七阶段、A/B/C、[独立 E2E](../e2e-executable-contracts.md)及最终兼容原 WPF 库验收不减；[总计划](../remaining-gap-closure-plan.md)与[矩阵](../final-acceptance-remaining-matrix.md)正式总门 **Red**。
有限模块完成不等于全任务完成，后续继续自主生产模块，不宣称替换资格。
