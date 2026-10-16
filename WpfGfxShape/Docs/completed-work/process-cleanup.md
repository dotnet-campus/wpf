# 过程清理交付记录

## 已实际完成

本轮只收口过程材料，不代表生产整体完成。正式 `src` 与 `WpfGfxShape/Ledger` 保留，生产代码未改、不 publishProduction、不跑 COM full，不重跑 GenProto 全量。

历史首批实际删除 **34,903 文件、2,178 目录、约 7.35 GB**：10 个 Investigations 子树、根 hw-present-diagnostic.txt、7 个一次性 PresentationCoreE2E Acceptance.proj。该数字是历史事实，不与本轮临时产物回收重复累加。`unused` / `WpfGfxShapeShape/unused` 实际未找到，不声称删除。详细逐路径事实与一次性迁移清单见唯一机器摘要 `WpfGfxShape/artifacts/process-cleanup/summary.json`。

已创建可运行入口说明 `eng/process-cleanup/README.md`；入口为 .NET 10 file-based `Cleanup.cs`。已有迁移源不存在会跳过，不要求重搬 oracle。直接按有限清单删除，无确认、dry-run、路径安全框架或 catch。最终专用 publish 编译成功，编译程序集经内置构建任务实际调用成功；桥接从字节加载程序集，避免删除被加载 DLL 时锁文件。临时 publish/obj/toolstaging/Apply.proj 由入口回收，不交付桥接工程。

## 剩余大批：GenProtoEvidence（未删除）

最终实际枚举：**45,136 文件、26,551 目录（含根目录）、2,503,071,496 bytes**。这是下一单独大批治理，不是“全部 Investigations 已删除”。本轮不复制整个证据包改名，不为清理而扩大重构。

唯一摘要 `remainingInvestigations` 保存精确规模；`genProtoReferences` 保存逐文件/行号/原文命中。扫描范围为 Code、Tests、Ledger、Docs 和清理入口，扩展名 cs/csproj/proj/props/targets/md/json；排除 bin/obj/artifacts/Investigations/.git/.coding-agent，不宣称字符串扫描能证明全部动态依赖。

硬依赖包括：GenProtoX86Clr.Diagnostics 的 GUID ManagedFields.cs/布局输入、GenProtoC5Audit.Diagnostics 的 evaluation XML/report 与源暂存、Stage4Layout.Diagnostics 的 delivery 输入，以及 GenProto.Tests 的固定 milcmd/wirecompiler/regeneration/delivery 基线、目录模式最新选择和 GUID 输出。文档历史引用与测试写入位置不混称编译输入。下一工作项先分类固定输入、动态选择和可再生产物，再有限解除硬依赖，不能直接删除整树。

## 正式最小输入与工具

- `Tests/ReferenceData/MediaOwnership/{candidate,reference}-ownership/Results/`：各一份 aot-identity.json，及 RejectedSend/UncommittedDestroy/CommittedDestroy 的 required-media 与 required-progress JSON（每侧六份）；供所有权/进度回归，不保留整运行包。是否存在历史 TRX以磁盘为准，不能由迁移意图断言存在。
- `Tests/ReferenceData/FloatHdr/previous.trx`：此前 FloatHdr 对照输入。
- `Tests/ReferenceData/HardwareAcceptance/sta-hw-final/{aot-identity.json,sta-hw-final.trx}`、`sta-hw-full/aot-identity.json`：历史硬件身份与可用结果；不虚构 full TRX。
- `Tests/WpfGfxShape.PublicAbi/ReferenceData/projection-glyph-reference.json`：glyph 投影 golden。
- `Tests/ReferenceData/GenProto/EmptyClass1.cs`、`PresentationCoreE2E/{pixels.pbgra32,result.json}`：已存在正式最小测试输入，不是本轮复制整个证据包。
- `Tests/Tools/PixelDiff/{Program.cs,PixelDiff.csproj}`：像素比较；`TriangleControls/{Program.cs,TriangleControls.csproj}`：三角对照；`ShadowPixelDiagnostic/{Program.cs,ShadowPixelDiagnostic.csproj}`：阴影诊断；`TypedLifetimeEvidence/{Program.cs,TypedLifetimeEvidence.csproj}`：寿命证据；`FloatHdrEvidenceAudit/{Program.cs,FloatHdrEvidenceAudit.csproj}`：FloatHdr 审计；`NativeCallerStartup/{Program.cs,PeAudit.csproj}`：PE/启动历史诊断源码。源码保留不等于恢复旧 caller。
- `Docs/history/{nativecaller-startup.md,Color-ABI-regression-execution.md,Glyph-outline-execution.md}`：迁移的历史说明，不是当前调度入口。

`MediaOwnershipRetry`、`PresentationCore-render/Results`、根 hw-present-diagnostic.txt 与 hw-present-log 的不存在核验见摘要 `checkedAbsent`。冻结文档中旧 Investigations 路径是已按用户授权删除的历史位置，不再宣称最新可运行。只更新恢复资料与索引，不全局伪造旧正文。

## 验证边界

此前受影响六项目 Tests/Tools、PublicAbi、GenProto、ComAcceptance、PresentationCoreE2E、Staging 的 Release build 成功为接续事实；本轮未重新构建六项目，不冒充新执行。

本轮实际机器完整解析已保留 `Tests/WpfGfxShape.GenProto.Tests/TestResults/lindexi_DESKTOP-51A5UGG_2026-10-16_16_08_57_net10.0.trx`：RunID `f1dd9225-fc28-441f-954a-6f43b388b047`，Counters total/executed/passed=46，failed=0；逐项 46 个 UnitTestResult 全 Passed，完整名单在摘要 `genProtoTrxValidation`。未重跑、未新增 GUID 证据树。

COM 本轮没有执行新 smoke；既有路径/identity 标准入口及 MediaOwnership 专项可有限复用，不把旧 40/40 startup smoke 写成本轮结果，不以 smoke 代 full。生产有限 Closed 保持，不重复领取；C5/D4 与全生产正式剩余门仍未满。本轮完成的是过程清理交付。
