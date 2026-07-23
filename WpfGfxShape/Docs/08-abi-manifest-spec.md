# wpfgfx ABI Manifest 规范

> 状态：实施前权威 schema；实际 manifest 由 `WP-00H`/`WP-00I` 及后续 ABI 工作包建立  
> 上位事实：[`02-abi-and-managed-callers.md`](02-abi-and-managed-callers.md)  
> 测试规则：[`06-testing-strategy.md`](06-testing-strategy.md)

## 1. 目的

ABI manifest 是 `wpfgfx_cor3.dll` 原实现与 Native AOT 候选之间的机器可比较合同。它必须把以下四层分开保存，禁止用任一层代替其它层：

1. `.def` 静态声明；
2. 原生 prototype/定义与构建条件；
3. 当前项目实际包含的托管 P/Invoke/COM/协议调用方；
4. 原 DLL与候选 DLL的实际 PE、调用和生命周期证据。

当前静态基线为 106 个 `.def` 名、109 个托管声明出现、107 个唯一 native EntryPoint、99 个交集、托管独有 8、`.def` 独有 7。上述数字必须由后续机器 manifest 复核，不能当作不可变常量硬编码到校验器。

## 2. 建议工件

```text
WpfGfxShape/Abi/
  schema-version.json
  entries.jsonl
  managed-declarations.jsonl
  types.jsonl
  callbacks.jsonl
  com-interfaces.jsonl
  binaries.jsonl
  baselines/
    original/<configuration>/<rid>/manifest.json
    candidate/<work-package>/<configuration>/<rid>/manifest.json
  generated/
    wpfgfx-abi.h
    exports.expected.txt
```

- `entries.jsonl`：以唯一 native EntryPoint/数据符号为单位。
- `managed-declarations.jsonl`：保留 109 个声明出现及其实际项目包含状态；多个声明可指向同一 entry。
- `types.jsonl`：结构、enum、handle、union、packet、固定数组和布局。
- `callbacks.jsonl`：函数指针/descriptor/线程/寿命。
- `com-interfaces.jsonl`：IID、vtable、所有权、apartment。
- `binaries.jsonl`：具体原/候选 PE 工件与验证结果。
- `wpfgfx-abi.h` 只能由已批准 manifest 生成或人工维护后反向校验；不得成为脱离事实源的第五份签名真相。

## 3. Entry ID 与状态

### 3.1 ID

- 函数：`ABI-FUNC-<exact-export-name>`。
- 数据：`ABI-DATA-<exact-export-name>`。
- COM 接口：`ABI-COM-<interface-name>`。
- callback：`ABI-CB-<owner>-<name>`。
- 类型：`ABI-TYPE-<qualified-name>`。

ID 保留精确大小写名称；文件系统不安全字符只在路径别名中编码。

### 3.2 来源存在状态

每个 entry 分别记录：

- `defPresence`：`Present`、`Absent`、`Conditional`、`Unknown`；
- `nativeDefinitionPresence`：`Present`、`DeclaredOnly`、`NotLocated`、`Conditional`；
- `managedDeclarationPresence`：`IncludedCurrentProject`、`SourceOnlyNotIncluded`、`Absent`、`Conditional`；
- `originalBinaryPresence`：`VerifiedCode`、`VerifiedData`、`AbsentVerified`、`NotInspected`；
- `candidateBinaryPresence`：同上；
- `runtimeReachability`：`VerifiedCalled`、`VerifiedExternalOnly`、`VerifiedUnreachableForScope`、`NotTested`。

禁止用 `SourceOnlyNotIncluded` 推断二进制不存在，也禁止用托管无声明推断可删除导出。

## 4. `entries.jsonl` 必填字段

| 字段 | 规则 |
|---|---|
| `schemaVersion` | 首版 `1.0.0` |
| `abiId` | 稳定唯一 ID |
| `publicName` | PE 导出精确大小写文本 |
| `kind` | `Function` 或 `Data` |
| `domain` | COM/factory/media/UCE/geometry/HW/WIC/diagnostic 等 |
| `defSource` | 路径、行/条目、条件、静态属性 |
| `nativeDeclarations` | prototype 路径、符号、calling-convention 宏、条件 |
| `nativeDefinitions` | 定义路径、符号、配置条件、Ledger ID |
| `managedDeclarationIds` | 指向 0..n 个声明记录 |
| `candidateExportMethod` | C# 路径/方法；未实现为 null，不创建假 stub |
| `returnAbi` | native 类型、位宽、有符号性、HRESULT/BOOL/void 语义 |
| `parameters` | 有序数组，见第 5 节 |
| `callingConventionByArchitecture` | x86/x64/ARM64 分开记录 |
| `nameDecorationByArchitecture` | 公开名、内部名、是否已实测 |
| `ownership` | 创建、借用、转移、AddRef、释放、配对入口 |
| `threading` | affinity、锁、callback/thread/apartment 约束 |
| `initializationPrerequisites` | 首次进入/全局状态要求 |
| `errorSemantics` | 参数校验、out 初始化、HRESULT/BOOL/void、异常映射 |
| `resourceSemantics` | HMODULE、resource type/ID、借用指针等 |
| `configurationBehavior` | Debug/Release/PRERELEASE/架构条件 |
| `differenceClass` | `Common99`、`ManagedOnly8`、`DefOnly7`、`ExtraBinary`、`ResolvedDecision` |
| `ledgerRecordIds` | 实现/声明/生成/资源记录 |
| `requiredEvidenceIds` | ABI、布局、caller、差分、E2E |
| `status` | `StaticMapped`、`OriginalBinaryVerified`、`CandidateExported`、`CallerVerified`、`DifferentialVerified`、`Accepted` |
| `decisionIds` | 差异/例外裁决 |

## 5. 参数字段

每个参数必须按原顺序记录：

- `index`、`name`；
- `nativeSpelling` 与 ABI canonical type；
- `direction`：`In`、`Out`、`InOut`；
- `pointerDepth`；
- `nullable` 与空值失败码；
- `widthByArchitecture`；
- `signedness`；
- `passMode`：value/pointer/reference/by-value struct；
- `boolKind`：`CppBool1`、`Win32BOOL4`、`NotBool`；
- `handleKind`：`DuceResource32`、`ClientPointerSized`、`COMPointer`、`Win32Handle`、`FixedUInt64ProtocolSlot`、`OpaqueToken`；
- `stringKind`：BSTR/LPCWSTR/tail UTF-16/none；
- `arrayElementType`、长度参数与长度单位；
- `structTypeId` 或 `callbackId`；
- `ownershipIn`/`ownershipOut`；
- `mustInitializeBeforeFailure`；
- `managedDeclarations` 中的 marshalling 元数据；
- 每架构 native/managed probe 证据。

## 6. 托管声明记录

每个 `[DllImport(DllImport.MilCore,...)]` 声明出现单独记录：

- 文件、类型、方法、源码行；
- 当前 `PresentationCore.csproj`/共享编译链是否实际包含；
- 实际 `EntryPoint`（显式或方法名推导）；
- `CallingConvention`、`CharSet`、`ExactSpelling`、`SetLastError`、`PreserveSig`、`BestFitMapping`、`ThrowOnUnmappableChar`；
- 参数/返回的 `MarshalAs`、SafeHandle/IntPtr/unsafe pointer；
- 上层直接调用方与当前可达性；
- 对应 `abiId`；
- `SourceOnlyNotIncluded` 的原因和项目证据。

历史默认元数据本身是合同输入。不得为了统一风格修改声明来让候选 DLL更易实现。

## 7. 类型与布局 manifest

`types.jsonl` 每项至少包含：

- 精确 native 名与 managed 名；
- 来源文件、生成组、Ledger ID；
- kind：scalar/enum/flags/struct/union/handle/packet/tail-array；
- underlying type、pack、align、size、field offset；
- x86/x64/ARM64 分值；
- `bool`/`BOOL`、pointer/`size_t`、fixed `UInt64` 说明；
- C++ 与 C# 字段有意非对称；
- native `sizeof/offsetof` 与 managed/AOT probe 证据；
- golden bytes 与负例；
- 状态。

首批强制类型：

- `CStreamDescriptor`；
- `CEventProxyDescriptor`；
- `AVEventData`；
- generated command/resource/render-data 结构；
- `MilPathGeometry` 相关结构；
- back-channel union/message；
- GUID/RECT/WICRect；
- 三类 MIL handle 与 fixed 64-bit protocol slots。

## 8. Callback manifest

每类 callback 必须单独记录：

- native typedef 与托管 delegate/function pointer；
- 调用约定、返回/参数、descriptor offset；
- 同步或长期保存；
- 调用线程/apartment；
- delegate/context 的创建者、保活者、释放者；
- CoreCLR token 是否只作不透明往返；
- detach 顺序：阻止新进入、等待在途、释放 context；
- callback 内异常策略；
- process-exit/unload 行为；
- x86 栈平衡与各架构运行证据。

强制四类：stream、event proxy、geometry、D3DImage。后续发现的新 callback 先建 manifest，再编码。

## 9. COM interface manifest

每个对外或被消费的 COM 接口至少记录：

- IID、继承链、vtable slot 顺序和方法签名；
- x86 `STDMETHODCALLTYPE` 与 x64/ARM64 ABI；
- QueryInterface 支持矩阵；
- AddRef/Release 线程与原子语义；
- create/getter/QI 的所有权；
- native-facing object/vtable 内存布局；
- 托管 GC root 与 native refcount 闭环；
- apartment/marshalling/agility；
- final release 与并发调用；
- C++ caller、PresentationCore 和差分证据。

`WP-00E` 的受控 IUnknown-like object 必须先证明机制，不能直接把 factory/media 标为可行。

## 10. Binary manifest

每个原/候选二进制格必须记录：

- `artifactId`、role（original/candidate/probe）；
- source revision、configuration、RID、PE machine；
- 绝对采集路径和仓库存储/外部存储位置；
- SHA-256；
- file/product version、OriginalFilename、Debug flag；
- imports/delay imports/dependencies；
- exports：name、kind、ordinal、RVA、forwarder、decoration；
- resources：type/name/language/size/hash；
- PDB/symbol identity；
- toolchain、SDK、OS；
- loader 回读路径；
- 已执行测试与结果。

未经实际检查时字段必须为 `NotInspected`，不能从 `.def` 或项目属性填成“已验证”。

## 11. 8/7 差异处理

- `ManagedOnly8`：保持未决，不因声明存在自动导出；先核实项目包含、原二进制、真实调用和历史条件。
- `DefOnly7`：保留兼容候选，不因当前 PresentationCore 无声明删除；检查 native/import-library/诊断调用方。
- 差异只能通过 Decision ID 改为 `VerifiedKeep`、`VerifiedConditional`、`VerifiedAbsentInShippingBinary` 或用户批准的例外。
- `MilPlayer_*` 原配置中真实 `E_NOTIMPL` 分支是待直译行为，不是批量 stub 先例。
- Debug `g_fNoMeterChecks` 作为 `ABI-DATA-*` 独立登记，不混入 106 函数数目。

## 12. 生成 C header 的规则

若生成 `wpfgfx-abi.h`：

- 只从状态至少为 `StaticMapped` 且签名完整的 manifest 项生成；
- 精确使用固定宽度 C 类型、Windows ABI 宏和 opaque handle typedef；
- 不暴露 C# 类型、SafeHandle 或托管 marshalling；
- 每个声明带 ABI ID，不带“已完成”暗示；
- header 与原 native prototype 差异必须由校验器报告；
- 生产 caller 只能使用与目标 manifest revision 匹配的 header。

## 13. 验证门

一个函数 entry 只有在以下证据齐全后才可 `Accepted`：

1. 原/候选 PE 名称和 kind 对照；
2. 每架构调用约定、参数/返回和栈/寄存器验证；
3. 所有引用类型的布局验证；
4. success、null、boundary、failure、exception 封锁；
5. 所有权、重复释放、失败中间状态；
6. callback/COM/thread/resource 的专项证据（如适用）；
7. 原实现与候选差分；
8. 参与的最高已就绪 E2E；
9. 无未决 8/7 或其它差异。

## 14. 当前未验证

本规划轮次未预处理 `.def`、未构建原 DLL或候选 DLL、未枚举 PE、未执行布局、caller、COM、callback 或 unload 测试。本文只定义后续事实的存储和验收方式。