# 过程清理交付记录

## 最终范围与合同

本次只完成既有清理集成、引用修复及最终源码验证，不扩删除范围、不增加待办。已授权 Ledger 18 文件（11 份过程摘要及 views 中 7 文件）和 Docs 35 份过程/WIP/竞争壳已删除；未重建兼容文件。用户合同归并既有 DEC-0004（驻留）、DEC-0005（仅 Windows x86/x64）、DEC-0006（当前官方完整 runtime），重复 scope overlay 与 remaining-wire-mapping JSON 不再作为合同或状态源。

现行入口、四短交接文档、历史索引、最终矩阵和分类索引同步；指向显式已删页的 Markdown 链接改指清理说明，矩阵旧 overlay 改指真实 DEC-0005。历史 SHA、失败和原运行身份不批量改写。历史索引明确已按用户清理，不承诺旧运行原物存在。

保留 Ledger `records.jsonl`、`groups.jsonl`、`work-packages.jsonl`、`evidence.jsonl`、`schema-version.json`，保留真实独立输入、正式源码/Tests/Tools、生产 manifest/资源声明。evidence 保留 ID/subject 合同，旧 run/hash/执行态已剥离为 Pending，不假 Satisfied。WireCompiler 派生结果写当轮 artifacts，不再写 Ledger。核心记录的历史 satisfiedEvidenceIds 不是当前执行认证。

## 实际删除分账

| 批次 | 文件 | 目录 | 字节 | 迁移 |
|---|---:|---:|---:|---:|
| 此前 Docs/Ledger 清理，含当时临时产物 | 109 | 10 | 30,681,691 | 0 |
| 本次首次最终源码验证回收 publish/Apply/编译产物 | 33 | 7 | 45,690,345 | 0 |

上述不是同一批，也不把 109 再计作本次新增业务删除。本次没有新增 Docs/Ledger 删除。后续最终验证产生的同类临时产物亦只按既有清单回收，精确逐路径累计以 `WpfGfxShape/artifacts/process-cleanup/summary.json` 的 deleted 为准；`currentRun` 只表示最后一次执行，最终连续两次为 0 文件/0 目录/0 字节、迁移 0。`cleanreport.json` 是最近非零回收明细，不与累计 summary 相加。

更早 GenProtoEvidence/Tests 清理的 48,100 文件、32,434 目录、3,366,505,814 字节，以及历史首批 34,903 文件、2,178 目录、约 7.35 GB，均为历史，不能计作本次成果。GenProtoEvidence、Tests ReferenceData/projection JSON 及授权的可再生输出此前已清理，不迁移备份。

## 最终源码验证

最终 `Cleanup.cs` 已通过专用接口执行 .NET 10 win-x64 Release NativeAOT publish，退出 0，日志含 Generating native code，无 IL3050/IL2026；其它样式警告不冒称全部无警告。保留源生成 JSON、六行中文、默认当前根目录合同，不加入路径安全框架、`../` 检查、确认、dry-run 或 catch；路径规范化仅用于业务匹配与计数。

通过 .NET MSBuild 内置 Roslyn 任务字节加载真实 Cleanup.dll，调用 `Cleanup.Run` 在真实工作区运行，无 Exec/shell。Framework MSBuild 曾因 net10 程序集加载不兼容失败，未计为通过；改用 .NET MSBuild 后执行成功。临时 publish/Apply/obj 均在既有清单回收，最终 eng/process-cleanup 仅 Cleanup.cs 与 README.md；标准 artifacts 仅 summary.json 和 cleanreport.json。所有文档编辑后再连续执行两次空运行，确认幂等且无新增业务删除。

GenProto.Tests 最小项目 build 已成功，仅证明编译；没有运行 GenProto full、没有新造 GUID 审计产物、没有生产 publish 或 COM 验收。

## 剩余风险

GenProto 完整审计对历史 GUID 原物的依赖尚未恢复。历史 46/46、GenerationTests 3/3 及 C5 有限审计不是本轮通过；不 skip、不造报告、不为保持旧 hash 恢复已删树。两个 MILCMD required/parent、C5/D4、完整 ABI/HW/refs/callback/E2E 等正式门仍未关闭，总门 Red，整体生产未完成。清理不降低原 readinessRule/doneRule 或七阶段目标。
