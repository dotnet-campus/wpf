# 唯一当前恢复入口

1. 先读 [严格对照合同](current-comparison-contract.md)。
2. 完成报告：[同冻结包严格双进程重复](../history/completed-work/replacement-strict-dual-process-first-render.md)。
3. [当前严格替换验收矩阵与后续顺序](current-validation-matrix.md)；[任务边界](next-code-task.md)仅保留任务5边界，历史按需查[索引](../history/handoffs/README.md)。

## 正式 strict 独立取证闭合（当前）

- 两正式测试项目组织及既有 build 完成状态承接，不重跑 FinishTestsCleanup。生产代码0改动；仅 NativeRun.cs 和 Comparison/StrictRenderSupervisor.cs 增加受限 observe、只读 audit；无新增工程或 Tools，无 oracle/阈值/业务调用修改。
- 最终证据 `artifacts/strict-merged-observed-verified-run/Evidence/`；冻结 harness SHA `D67B9C86B53B694FC44A628CD3145D577C983DCB668BC1E917764149A994B93F`，runtime 10.0.13。Native 从实际旧包 `strict-merged-readback-final/Native/wpfgfx_cor3.dll` 复用，未重发生产 Native；新包实际 candidate SHA `8B05632E37BC5BDA2241662C8D1A196E0FE77CA84630FAE26804503D63167A79`、official SHA `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`。
- 独立 audit 进程实际读取完整 raw/result/process/模块/manifest，不以 comparison 摘要代替。两侧各4096字节、逐字节相等，SHA均 `7F55E138C9605464AE90811BD28B5477871BB3E1AA71F5FD3A1C8B052E0C308B`；11契约全等；353条模块SHA核对通过、692包项核对通过；两侧各唯一所选 gfx、handle一致及同harness均通过。审计结果已完整读回（专用 sandbox 输出），审计包落在 `artifacts/strict-merged-observed-verified-audit/`。
- observer11872保存真实父4948 exit0、未超时且回收；业务子12664/9200 exit0、无超时、回收并drain。父stdout passed=true/differences0，stderr空，原件已读；父/observer前后模块快照均无 gfx/PresentationCore。最外层 observer/audit 的独立OS退出收据仍Unknown，不递归套框架。模块为采样非连续拦截；签名主体/文件SHA不是签名信任全链；cleanup不证明完整生命周期/无泄漏。
- 本次实际执行：专用 publish3均成功（包含编译，既存分析器警告保留）；sandbox6，其中observe3、audit3（前两次audit分别因失效旧任务绝对路径、当前目录解析失败；修复后重新发布冻结并observe+audit一次全过）。单独build/test0、生产publish0、清理/终端/脚本/委派0。未改输出机制、未清理重组artifacts；仅定向读取验收包。Dispatcher四项未跑、未绕过AssemblyInitialize；full/桌面/媒体未跑且旧故障不关闭。
- 当前仅 x64固定32×32 RTB **PassedLimited**，OverallOpen。下一可执行严格场景仍为预注册闭合PathGeometry（线段+Bezier），同输入/画刷/背景，32×32/Pbgra32/96dpi，官方独立稳定性再双进程完整raw零阈值及契约比较。

## 正式合并接续（上一任务历史账，非当前未决）

- 用户已执行 FinishTestsCleanup，本次串行核验：Tests 一级目录及递归实际 csproj 均仅 WpfGfxShape.Tests、WpfGfxShape.ComAcceptance；旧 StrictRenderR28、StrictRenderSupervisorR28、CandidatePureVideo 三目录已消失。slnx 实读仅生产1项目及正式 Tests 2项目。正式 Compile/Link、Content、ProjectReference、Import/targets、NativeRun 入口静态检查未见旧三目录依赖，不以历史 namespace 判断引用。迁入 Harness/StrictRenderBusiness、Scenes/StrictRenderGeometry、Comparison/StrictRenderSupervisor 与 DispatcherWorkersTests 仍在；StrictRender 发布配置固定 WindowsDesktop runtime 10.0.13。源码修改0。
- 前任务已亲读最终 `artifacts/strict-merged-readback-final/Evidence/comparison.json`（本次未重读）：两侧 exit0、4096字节、raw差异0、11项契约相等。尚须逐项读取 process/result/parent/manifest 及实际 raw；不能仅凭此摘要宣称完整取证通过。
- 本次不修改 harness 身份，不将历史绿色结果绑定新代码。正式四项 DispatcherWorkers 身份门未运行；安全托管相关小集 `AvCompositionNotifierTests` 已通过5/5、失败0、跳过0、exit0，不加载原版 Native，不代表视频完整验收。`IndependentVideoPlaybackOwnerTests.cs` 实际为 MediaFoundationResourceTests 的分部文件，未使用同名类空筛选。当前仍仅固定32×32 x64 RTB门，不是完整替换；OverallOpen。
- 前任务调用账（承接，非本次重跑）：合并代码 Release build1 exit0/4109 warnings；publish6（成功5、缺旧DLL失败1）；sandbox3。新 strict 运行 comparison 摘要差异0、11 contracts及两侧exit0原件已承接，但独立 raw、父 OS exit、全 module 取证未闭合，不能记为独立审计完成。
- 本次删除后验证调用账：专用 Release build2，ComAcceptance exit0、172警告/0错误，WpfGfxShape.Tests exit0、0警告/0错误（增量）；定向 test1 为 Debug 下 AvCompositionNotifierTests，5通过/0失败/0跳过、exit0。前任务4109警告不作本次计数，也不据增量结果宣称警告已消除；未修既存分析器警告。清理工具执行0、publish0、sandbox0、终端/脚本0、委派0；未重发 Native、未跑 full 或四项 Dispatcher 身份门。正常输出机制未改，artifacts 未盘点整理清理或定向读取。
- Tools 递归仅剩 TestsCleanup/FinishTestsCleanup.cs，旧 CleanupTests.cs/README.md 已删；工具保留自身正常且已完成使命，用户可删除剩余 Tools/TestsCleanup，不再运行删除工具。本次仅更新四份 current 文档并全文小段回读。strict 独立 raw、父 OS exit、完整模块证据审计仍未闭合，不借历史绿升级当前结论。

## 当前状态（以下为前阶段冻结证据）

- **OverallOpen**。任务5用run2完整留存发布快照完成两次新增独立双进程重复，有限32×32 RTB门通过；不是桌面/full/x86或所有行为等价认证。
- 599个非Evidence文件相同且第二次运行前后不变，历史402个业务包manifest项全部匹配。四个新子进程exit0、完整4096字节差异0，与历史run2 raw相同；11项业务契约及runtime身份复核通过。
- 第二次父PID804真实exit0、stdout摘要通过、stderr空，子12080/824正常回收；首次父退出仍缺独立收据。证据见 `artifacts/strict-render-r28/task5-supervised-repeat/Evidence/summary.json`、`parent-process.json`、`readback.json`。
- run1缺模块/runtime证据且harness不同，不计同冻结包重复；旧红与历史缺口不抹除。模块采样非连续拦截，未比较字段与清理证明边界见报告；官方SHA已核不等于签名全链，审计最外层真实exit仍Unknown。

## 历史执行账与本轮文档任务

任务5报告的辅助审计 publish1、sandbox2 属于历史业务证据账，不是任务6新增。任务6只改文档与DocumentationMaintenance专用辅助，生产/业务验收改动0；历史维护记录见[原字节台账归档](../history/handoffs/documentation-maintenance-legacy.md)，该归档只覆盖原索引标明的阶段，不冒充任务6最终verify原件，终端/脚本/委派0。

**文档治理进行中**：任务10已全文治理5份历史报告；文档工作不是产品运行结果，不表示代码全完成。当前strict渲染仍仅上述x64固定32×32有限门，状态不变；下一代码任务仍待按[矩阵](current-validation-matrix.md)扩展路径→变换→裁剪→透明。旧234项为整理前历史盘点，不作迁后现状；目录整理检查见[当前维护检查](repository-organization-check.md)。

当前文档维护职责、已知归档范围、缺件与待审名单统一见[当前维护台账与迁后检查](repository-organization-check.md)；旧台账现行原件缺失，不由空输入副本重建，不以历史已读统计认证迁后全覆盖。产品当前状态仍只维护本页；历史累计文件不作为并行恢复入口。
