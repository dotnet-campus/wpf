# 过程材料清理

入口为 .NET 10 单文件应用 `Cleanup.cs`，无第三方包。唯一可选应用参数为仓库根目录，省略使用当前目录；根目录包含 `WpfGfxShape` 子目录。

## 有限合同

只执行源码既有显式删除清单，不新增清理范围、路径安全框架、`../` 检查、确认、dry-run 或 catch。保留源生成 JSON 与固定六行中文输出。路径规范化只用于业务路径匹配和计数。

既有范围包括 GenProtoEvidence、Tests ReferenceData/可再生输出、PublicAbi projection JSON，以及已授权 Ledger 过程摘要/views、35 份 Docs 过程/WIP/竞争壳和清理工具临时 publish/Apply/obj。不是生产源码或独立输入清理，不搬家备份历史原物。运行前修复指向显式已删页面的 Markdown 导航；不替换历史 SHA。

用户合同唯一归 DEC-0004（进程驻留）、DEC-0005（Windows x86/x64）、DEC-0006（当前官方完整 runtime）。核心 records/groups/work-packages/evidence/schema 保留；evidence 只保留身份与 subject 合同，旧 run/hash/执行态剥离为 Pending，不伪造 Satisfied。WireCompiler 派生结果使用该轮 artifacts。

## 验证与输出

最终源码通过专用 NativeAOT 发布接口验证，win-x64 Release 退出 0，生成原生代码，无 IL3050/IL2026；不关闭 AOT、不抑制告警。通过 .NET MSBuild Roslyn 内置任务字节加载实际 Cleanup.dll，在真实工作区调用 `Cleanup.Run`，无 Exec/shell。使用 .NET MSBuild，不以 Framework MSBuild 加载 net10 程序集。

`WpfGfxShape/artifacts/process-cleanup/summary.json` 保存累计逐路径 deleted/retained，`currentRun` 仅本次；`cleanreport.json` 保留最近一次非零清理明细。发布与 Apply 临时产物本身也在既有清单内，回收计数与业务清理分账；最后连续两次空运行应均为 0 文件/0 目录/0 字节、迁移 0。eng 最终只留 Cleanup.cs 和本说明。

## 唯一结果与风险

精确批次账与实际验证见 [清理交付记录](../../WpfGfxShape/Docs/completed-work/process-cleanup.md)，不在本页复制过期总数。Docs/Ledger 前批为 109 文件、10 目录、30,681,691 字节，包含当时临时产物，不冒充本次新增删除。

GenProto 最小项目构建已成功，但未运行 full，历史 46/46 与 GenerationTests 3/3 均非当前通过。历史 GUID 原物依赖尚未恢复；不 skip、不补假证据、不再造审计工程或拷贝 Docs 树。没有生产 publish/COM 验收，整体生产仍未完成。
