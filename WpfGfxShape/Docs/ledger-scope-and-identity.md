# Ledger：生产范围、工件与稳定 ID

[Ledger 规范](07-scope-and-ledger-schema.md)§1–4正式合同；运行状态只见[基线](handoffs/current-stable-baseline.md)，机器事实只认通过 schema 校验的 JSONL。

## 1. 目的

“整个 wpfgfx”生产完成分母是最终 `wpfgfx$(WpfVersionSuffix).dll` 完整生产闭包，不等于磁盘树或直接编译文件。包含声明、PCH、生成协议、资源、构建输入、供应槽、ABI、必要托管协议对端。
规范定义数据模型，不代项目求值、枚举、构建或 binary 证据。

## 2. 范围矩阵

| 类别 | 是否生产分母 | 规则 |
|---|---|---|
| core/dll/wpfgfx.vcxproj 最终聚合、自有源码/def/资源/构建输入 | 是 | 全建账，候选 AOT 等价承载 |
| 其余23个已加载 WpfGfx StaticLibrary | 是 | 项目哨兵及全部参与文件 |
| core/sw/bilinearspan/bilinearspan.vcxproj 供应槽 | 是 | 实际源码/运输库未取得时 Blocked-ExternalSupply，不凭头补实现 |
| src/Microsoft.DotNet.Wpf/src/Shared/OSVersionHelper/OSVersionHelper.vcxproj 生产行为 | 是 | 外部直接记录，按调用点近似直译/最低互操作 |
| src/Microsoft.DotNet.Wpf/src/Shared/cpp/dwriteloader.cpp 及声明 | 是 | 真实来源，不冒充 core/common 自有 |
| processed/Generated、MilCodeGen 模型/输出、WPP、shader、rc/res、def→i、version/ETW | 是 | 输入/生成器/输出/消费者/冻结基线逐项 |
| PresentationCore/Common MilCore PInvoke、共享布局/命令生产方 | Contract/Evidence，不是待翻译生产文件 | 只读，用于 ABI/协议/E2E |
| core/control/dll/milctrl.vcxproj | 否 | Sidecar 哨兵，记共享 control/util 消费 |
| exts/exts.vcxproj（wpfx）、DbgXHelper | 否 | Sidecar 哨兵，生成布局一致性证据 |
| DirectWriteForwarder、PresentationNative、D3DCompiler redist | 否 | 不迁入，记录共存/加载/E2E 环境 |
| 原 WPF C++/C# 产品项目源码 | 否且只读 | 不引用/链接/复制进新生产项目，只作事实与差分 |
| 托管单元、发布后 PInvoke、差分/E2E harness | 不计生产项目数量 | 独立承载，进入证据/工作包，不创建 Tests/NativeCaller |

发现文件实际进入最终闭包，先新增记录及更新范围表再翻译；移出生产分母须用户明确批准。

## 3. 工件位置与格式

根 `WpfGfxShape/Ledger/`：schema-version.json、records.jsonl、groups.jsonl、work-packages.jsonl、evidence.jsonl；派生 views/projects.md、batches.md、blocked.md、status.md。
- records：物理项/非物理哨兵唯一事实源，一行一个 JSON object。
- groups：生成族/类型族/循环组/ABI协议合同，不能代物理记录。
- work-packages：范围、前置、停止点、状态。
- evidence：ID/hash/路径索引，不内嵌大二进制。
- views 派生只读，冲突以通过 schema 的 JSONL 为准。
- 首版 schemaVersion=1.0.0；破坏语义升 major，新增可选字段升 minor。
- UTF-8 JSON 无注释；路径 `/`，仓库内相对 OriginWpf 根。

## 4. 稳定 ID

### 4.1 Record ID

`REC-<scope>-<kind>-<normalized-path>`。
scope：WPFGFX/SHARED/MANAGED/SIDECAR/EXTERNAL/GENERATED/BUILD。
kind：IMPL/HEADER/PCH/INLINE/RESOURCE/BUILD/ABI/PROTOCOL/PROJECT/SUPPLY/EVIDENCE。
ID path 小写，非字母数字压为单个 `-`；nativePath 保留真实大小写。
创建后不可随 C# 路径、批次、重命名复用给另一项；重命名用 aliases。

### 4.2 Group ID

GENPAIR-* 成对输出（GENPAIR-CORE-TYPES）；GENPROTO-* 协议族（GENPROTO-MILCMD）；TYPEFAMILY-* 手写/生成类型族；CYCLE-* 循环组；ABI-* 导出/COM/callback；PROJECT-* 完成哨兵。

### 4.3 Work package ID

沿用 WP-00A…WP-14x；拆包不改 Record ID。一个 Record 可历经多包，同一时刻只有一个 ownerWorkPackageId。
