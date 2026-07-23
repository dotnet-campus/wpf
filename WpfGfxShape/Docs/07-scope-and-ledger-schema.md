# wpfgfx 迁移范围与机器可读 Ledger 规范

> 状态：实施前权威 schema；实际 Ledger 由 `WP-00I-MACHINE-READABLE-LEDGER` 创建  
> 上位约束：[`00-migration-charter.md`](00-migration-charter.md)  
> 当前 WPF 源基线：`44615ed4b9f033922b3361ea02c02f173b8bf82e`（当前工作树 `.git/HEAD` 静态读取；工作区脏状态未验证）  
> 当前 Silk.NET 基线：`ed4b3c299a44e8b6936f6b1c61c0b119e82237cd`

## 1. 目的

本规范消除“整个 wpfgfx”与“整个 `WpfGfx` 磁盘树”的歧义，并规定后续机器可读迁移台账的唯一数据模型。它不创建 Ledger，也不替代实际项目求值、文件枚举、构建或二进制证据。

生产迁移完成分母定义为：**最终 `wpfgfx$(WpfVersionSuffix).dll` 的完整生产闭包**。该闭包不仅包含直接编译的 C/C++ 实现，还包含其声明、PCH、生成协议、资源、构建输入、外部供应槽、ABI 合同和必要的托管协议对端。

## 2. 范围矩阵

| 范围类别 | 内容 | 是否计入生产完成分母 | 处理规则 |
|---|---|---:|---|
| 最终 DLL 聚合项目 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.vcxproj` 及其自有源码、`.def`、资源和构建输入 | 是 | 全部建账；最终由候选 Native AOT DLL等价承载 |
| 已加载生产项目 | 当前解决方案中的其余 23 个 WpfGfx `StaticLibrary` 项目 | 是 | 项目哨兵与全部参与文件建账 |
| 缺失/运输供应槽 | `core/sw/bilinearspan/bilinearspan.vcxproj` 及其实际运输包库/源码来源 | 是 | 未取得真实供应物前标记 `Blocked-ExternalSupply`；不得凭头文件补写实现 |
| WpfGfx 外直接项目 | `src/Microsoft.DotNet.Wpf/src/Shared/OSVersionHelper/OSVersionHelper.vcxproj` 中被生产路径调用的行为 | 是 | 建立外部直接依赖记录；按实际调用点决定近似直译或最低层互操作 |
| WpfGfx 外直接编译文件 | `src/Microsoft.DotNet.Wpf/src/Shared/cpp/dwriteloader.cpp` 及配对声明 | 是 | 保留真实来源路径；不得冒充 `core/common` 自有文件 |
| 生成与资源闭包 | processed/Generated 类型、MilCodeGen 模型与输出、WPP、shader、`.rc/.res`、`.def -> .i`、版本/ETW 等 | 是 | 每个输入、生成器、输出、消费者和冻结基线均建账 |
| 托管 ABI/协议对端 | 当前 PresentationCore/Common 中的 MilCore P/Invoke、共享布局和命令生产方 | 不作为待翻译生产文件；作为强制 Contract/Evidence | 原则上保持只读；用于 ABI、协议与 E2E 验证 |
| 旁支控制 DLL | `core/control/dll/milctrl.vcxproj` | 否 | 建立 `Sidecar` 哨兵，记录共享 `control/util` 消费；不要求迁为同一 AOT DLL |
| 调试扩展 | `exts/exts.vcxproj`（`wpfx`）与 `DbgXHelper` | 否 | 建立 `Sidecar` 哨兵；生成命令布局可作为一致性证据 |
| 相邻部署组件 | `DirectWriteForwarder`、`PresentationNative`、D3DCompiler redist 等 | 否 | 不翻译进本项目；记录共存、加载和 E2E 环境依赖 |
| 原 WPF 项目/源码 | 全部现有 WPF C++/C# 产品项目 | 否，且只读 | 不引用、不链接、不复制进新生产项目；只作事实和差分基线 |
| 测试与 harness | 托管测试、native caller、差分/E2E harness | 不计生产项目数量 | 独立项目/承载；必须进入证据和工作包清单 |

若后续证据表明某文件实际进入最终 DLL闭包，必须先新增 Ledger 记录并更新本表，再开始翻译。任何移出生产分母的决定均需要用户明确批准。

## 3. 工件位置与格式

`WP-00I` 应创建：

```text
WpfGfxShape/Ledger/
  schema-version.json
  records.jsonl
  groups.jsonl
  work-packages.jsonl
  evidence.jsonl
  views/
    projects.md
    batches.md
    blocked.md
    status.md
```

规范：

- `records.jsonl` 为物理项和非物理哨兵的唯一事实源，一行一个 JSON object。
- `groups.jsonl` 保存生成族、类型族、循环组和 ABI/协议合同组；不能替代物理记录。
- `work-packages.jsonl` 保存工作包范围、前置、停止点和状态。
- `evidence.jsonl` 只保存证据索引与 hash/路径，不把大型二进制内嵌进 JSON。
- Markdown `views` 是机器清单的派生只读视图；若与 JSONL 冲突，以通过 schema 校验的 JSONL 为准。
- 首版 `schemaVersion` 固定为 `1.0.0`。破坏字段语义的变化提升 major，新增可选字段提升 minor。
- JSON 编码 UTF-8，无注释；路径使用 `/`，仓库内路径相对 OriginWpf 根目录。

## 4. 稳定 ID

### 4.1 Record ID

格式：`REC-<scope>-<kind>-<normalized-path>`。

- `scope`：`WPFGFX`、`SHARED`、`MANAGED`、`SIDECAR`、`EXTERNAL`、`GENERATED`、`BUILD`。
- `kind`：`IMPL`、`HEADER`、`PCH`、`INLINE`、`RESOURCE`、`BUILD`、`ABI`、`PROTOCOL`、`PROJECT`、`SUPPLY`、`EVIDENCE`。
- `normalized-path`：将路径小写化用于 ID，`/`、`.` 等非字母数字压成单个 `-`；记录中的 `nativePath` 仍保留仓库实际大小写。
- ID 创建后不可因目标 C# 路径、批次或文件重命名而复用给另一项；重命名通过 `aliases` 记录。

### 4.2 Group ID

- `GENPAIR-*`：成对生成输出，如 `GENPAIR-CORE-TYPES`。
- `GENPROTO-*`：协议族，如 `GENPROTO-MILCMD`。
- `TYPEFAMILY-*`：手写/生成文件共同实现的类型族。
- `CYCLE-*`：编译/实现循环组。
- `ABI-*`：导出或 COM/回调合同组。
- `PROJECT-*`：项目完成哨兵。

### 4.3 Work package ID

沿用总计划的 `WP-00A...WP-14x`。工作包拆分不得改变 Record ID；一个 Record 可历经多个工作包，但同一时刻只能有一个 `ownerWorkPackageId`。

## 5. `records.jsonl` 必填字段

| 字段 | 类型 | 规则 |
|---|---|---|
| `schemaVersion` | string | 当前为 `1.0.0` |
| `recordId` | string | 全局唯一、稳定 |
| `recordType` | enum | `PhysicalFile`、`ProjectSentinel`、`ExternalSupply`、`ManagedContract`、`BinaryArtifact`、`EvidenceOnly` |
| `scopeDisposition` | enum | `ProductionDenominator`、`ContractOnly`、`SidecarEvidence`、`AdjacentDeployment`、`ExcludedByApprovedDecision` |
| `nativeProject` | string/null | 原 `.vcxproj`，公共项可为空并写 `sourceOwner` |
| `nativePath` | string/null | 仓库相对路径；外部工件可为空 |
| `fileKind` | enum | `Implementation`、`Header`、`PCH`、`Inline`、`GeneratedInput`、`GeneratedOutput`、`Resource`、`Build`、`ABI`、`Protocol`、`External`、`Evidence` |
| `buildParticipation` | string[] | `Compile`、`Include`、`ResourceInput`、`GeneratedInput`、`GeneratedOutput`、`LinkInput`、`ProjectReference`、`TransportSupply`、`ManagedConsumer`、`SidecarOnly`、`Unknown` |
| `sourceAuthority` | enum | `CurrentConsumedSource`、`FrozenGeneratedOutput`、`HistoricalModel`、`HistoricalGenerator`、`DerivedCopy`、`BuildIntermediate`、`ExternalBinary`、`Unknown` |
| `originalResponsibility` | string | 只描述原职责，不描述拟议重构 |
| `keySymbols` | string[] | 类型、函数、导出、资源 ID等 |
| `pchAndDirectIncludes` | string[] | PCH 与直接 include；未调查使用空数组并在 blocker 说明 |
| `dependencies` | string[] | 只放 Record/Group ID，不放自由文本路径 |
| `platformDependencies` | string[] | 例如 `Win32`、`COM`、`D3D9`、`WIC`、`DWrite`、`EVR` |
| `generatedGroupIds` | string[] | 可为空 |
| `typeFamilyIds` | string[] | 可为空 |
| `cycleGroupIds` | string[] | 可为空 |
| `abiContractIds` | string[] | 指向 ABI manifest ID |
| `investigationBatch` | string | 调查工作包/批次 |
| `implementationBatch` | string | `0`–`14` 或子批 |
| `proposedCSharpPath` | string/null | 生产文件开始编码前必须非空 |
| `status` | enum | 见第 6 节 |
| `readinessBlockers` | string[] | 明确 ID或风险；不得写“稍后处理” |
| `requiredEvidenceIds` | string[] | 编码前定义 |
| `satisfiedEvidenceIds` | string[] | 只填真实已运行证据 |
| `knownDeviations` | object[] | 每项含原因、风险、验证和批准状态 |
| `ownerWorkPackageId` | string/null | 当前负责工作包 |
| `lastReviewedSession` | string | 会话归档 ID |
| `notesDecisionIds` | string[] | 指向 decisions 索引 |

附加必填语义：

- `ProductionDenominator` 记录不得缺少 `implementationBatch`、`status` 和 `requiredEvidenceIds`。
- `PhysicalFile` 必须有 `nativePath`；`ManagedContract` 必须有实际项目包含状态。
- 未发现某类输入时，项目哨兵应在 `checkedCategories` 中显式记录 `CheckedNoneFound`，不能以缺行代表已检查。
- 集中生成实现（如 `marshal_generated.cpp`）保持自己的物理记录；不得只把方法散到类型族后删除原文件追溯。

## 6. 状态机与允许迁移

主状态沿用章程：

`NotInvestigated -> Investigated -> Scaffolded -> Translated -> UnitVerified -> DifferentialVerified -> Integrated -> EndToEndVerified -> Accepted`

`Blocked` 是正交状态，记录为：

```json
{
  "status": "Investigated",
  "blocked": true,
  "blockerIds": ["RISK-P0-X86-NATIVEAOT"]
}
```

允许提升条件：

- `Investigated`：职责、依赖、路径映射、风险、测试合同和未决项齐全。
- `Scaffolded`：仅有语言承载所需的原名类型/签名/布局/薄入口；无假业务成功。
- `Translated`：按原文件近似直译完成，但验证不足。
- `UnitVerified`：规定 T1 证据通过。
- `DifferentialVerified`：规定 T3 原/候选差分通过。
- `Integrated`：规定 T4 组件闭环通过。
- `EndToEndVerified`：该文件参与的最高已就绪 T5 阶梯通过。
- `Accepted`：该记录要求的所有证据满足、偏差均记录并获批准、无未决阻塞。

任何状态提升都必须由 `evidence.jsonl` 中的已执行记录支撑。构建成功不能直接提升为 `UnitVerified`。

## 7. `groups.jsonl` 合同

每个 group 至少包含：

- `groupId`、`groupType`、`memberRecordIds`；
- `authorityRecordIds` 与 `consumerRecordIds`；
- `invariants`：ID、布局、顺序、生成关系或循环关系；
- `readinessRule`、`doneRule`；
- `requiredEvidenceIds`；
- `status` 与 `blocked`；
- `lastReviewedSession`。

生成组必须表达 `input -> generator/model -> frozen output -> consumer`。循环组必须列出允许的骨架成员，不允许以新抽象消除原回边。

## 8. 完整性校验规则

`WP-00I` 必须实现或提供可重复执行的校验，至少检查：

1. ID 全局唯一，所有跨记录引用存在；
2. 所有 `wpfgfx.vcxproj` 直接项目引用均有项目/供应记录；
3. 每个项目项、磁盘范围项、PCH/include、生成、资源、ABI 和外部供应视图均有记录或 `CheckedNoneFound`；
4. 每个 `ProductionDenominator` 物理文件只有一个主记录；
5. 每个实现文件在编码前有 `proposedCSharpPath` 和测试证据定义；
6. 原路径与 C# 主路径不得被多个无说明记录冲突占用；
7. ABI manifest 中的源码定义、托管调用方和实现文件均能回链 Ledger；
8. 所有 `Accepted` 项无 blocker，且 required evidence 全部满足；
9. 项目哨兵只有在各类别闭环后才能 `Accepted`；
10. 生产完成分母、已完成数、阻塞数和未调查数可由记录确定性计算。

## 9. 基线与可重复性

每次生成/更新 Ledger 必须记录：

- OriginWpf revision、工作区是否有未提交改动；
- Silk.NET revision（若参与）；
- SDK/MSBuild/Windows SDK/toolchain；
- configuration/RID 条件；
- 枚举方法和命令/IDE 操作；
- 生成时间与 schema version；
- 输入 hash 或来源 manifest。

当前规划轮次只确认 revision 文本，未验证工作区 clean 状态、项目求值或磁盘完整闭包。

## 10. 与工作包的关系

- `WP-00A` 只创建未来 Ledger 目录承载所需的最小边界（如确有需要）；不得伪造完整记录。
- `WP-00H` 提供原二进制 Evidence/Artifact 记录。
- `WP-00I` 创建并校验首个完整 Ledger，之后每轮先更新受影响记录再编码。
- 批次 1–14 的每个会话必须在结束前回写状态、依赖、路径、偏差和证据。
- `next-session-handoff.md` 必须列出本轮涉及的 Record/Group ID；若 Ledger 尚未创建，明确写 `None (WP-00I pending)`。

## 11. 完成定义

本规范完成不代表 Ledger 完成。`WP-00I` 只有在：

- 生产分母机器可计算；
- 多视图闭环通过；
- 首批工作包可从记录确定性派生；
- schema 校验无错误；
- Markdown 视图与 JSONL 一致；
- 所有未知项以明确 blocker 表达；

时才可关闭。