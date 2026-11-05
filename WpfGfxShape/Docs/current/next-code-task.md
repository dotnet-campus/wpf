# 任务边界：任务5已收尾

遵守 [严格对照合同](current-comparison-contract.md)，结论与证据见 [完成报告](../history/completed-work/replacement-strict-dual-process-first-render.md)。本文不分派下一智能体或新矩阵。

- run2完整冻结快照两次新增严格双进程重复通过现有RTB门；两次均为同一业务exe/DLL、runtime与固定输入。599项非Evidence清单一致，第二次前后不变，历史402项包清单匹配。run1不计同包重复。
- 第二次父/子真实退出与完整输出有收据，首次父退出仍Unknown；审计最外层真实exit仍Unknown，不继续套框架补证。
- 总体OverallOpen：比较器11项与4096字节raw通过，不代表全部行为等同。未比较字段、模块采样/cleanup边界、官方SHA非签名链，以及桌面/full/x86历史未决见报告。
- 生产/业务/冻结监督包修改0、业务重编/发布0、无新输入/矩阵、无fallback/补偿/阈值改动。任务5自身执行为辅助publish1与sandbox2，账目独立保留；原run2证据不覆盖。

## 当前正式 strict 取证结果

最终正式包及独立audit已实际执行并完整读回，见[恢复入口当前闭合项](current-recovery.md)。证据 `artifacts/strict-merged-observed-verified-run/Evidence/`：4096 raw完整逐字节相同、11契约相等、353模块SHA及692包项全匹配；父4948和子12664/9200真实exit0。只认证当前冻结harness的x64固定32×32有限门；旧10376父exit缺失不补造。生产0改动，正式验收2文件新增observe/audit，无新项目/Tools。

本次publish3、sandbox6（audit前2次路径错误，最终新冻结包observe/audit通过）；单独build/test0、Native发布0。Dispatcher四项未跑，full/桌面/媒体未跑。模块采样、签名链和完整生命周期边界不升级；最外层observer/audit OSexit无独立收据。下一执行为矩阵路径场景，不能把本次固定几何当路径覆盖。

## 正式合并后的限范围收尾（上一任务历史）

以上任务5为旧冻结包历史，不自动认证新正式入口。合并代码已 Release build exit0；新 strict comparison 摘要为4096字节差异0、11 contracts相等，并有两侧exit0原件，但独立 raw、父 OS exit、全 module 取证尚未闭合。正式四项 Dispatcher 身份门未运行，不记通过。

用户已执行 FinishTestsCleanup，本轮串行核验完成：Tests 一级目录与递归实际 csproj 均仅 WpfGfxShape.Tests、WpfGfxShape.ComAcceptance；旧 StrictRenderR28、StrictRenderSupervisorR28、CandidatePureVideo 三目录已消失。slnx 仅生产1项目与正式 Tests 2项目；正式 Compile/Link、Content、ProjectReference、Import/targets、NativeRun 入口未见旧路径依赖，迁入严格业务/监督/场景及 DispatcherWorkersTests 仍在，不以历史 namespace 报断引用。

删除后专用 Release build2：ComAcceptance exit0、172警告/0错误；WpfGfxShape.Tests exit0、0警告/0错误（增量）。前任务4109警告保留为历史，不据增量数字声称已消除。定向 test1：Debug 下安全托管 AvCompositionNotifierTests 5通过/0失败/0跳过、exit0，不加载原版 Native；四项 Dispatcher 身份门未满足，不运行、不绕过。strict 独立 raw、父 OS exit、完整模块审计仍未闭合，不挪历史绿。

Tools 实际仅剩 TestsCleanup/FinishTestsCleanup.cs，旧 CleanupTests.cs/README.md 已删，工具完成使命可由用户删除剩余 Tools/TestsCleanup，不再运行。源码/项目配置修改0，仅更新四份 current 文档并全文小段回读；正常输出机制不改，artifacts 不盘点整理清理或定向读取。清理执行0、publish0、sandbox0、终端/脚本0、委派0，未重发 Native、未跑 full。

本任务到此结束，不启动新的大审计、full、publish或辅助运行。
