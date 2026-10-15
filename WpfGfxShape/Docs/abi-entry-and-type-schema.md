# ABI Manifest：来源、entry、参数与类型

[ABI 规范](08-abi-manifest-spec.md)§1–7正式合同，不维护实现进度。

## 1. 目的

原 `wpfgfx_cor3.dll` 与候选的机器可比较合同；四层分别保存，不能相互代替：def 静态声明、native prototype/定义/构建条件、实际项目包含的托管 PInvoke/COM/协议调用方、原/候选 PE及调用/寿命实证。
历史静态调查为 def106、托管声明109/唯一107、交集99、managed-only8/def-only7；后续机器复核，不硬编码为校验常量或当前完成数字。

## 2. 目标工件结构

根 `WpfGfxShape/Abi/`：schema-version.json、entries.jsonl、managed-declarations.jsonl、types.jsonl、callbacks.jsonl、com-interfaces.jsonl、binaries.jsonl。
baselines/original/<configuration>/<rid>/manifest.json；baselines/candidate/<work-package>/<configuration>/<rid>/manifest.json；generated/wpfgfx-abi.h、exports.expected.txt。
- entries：唯一 EntryPoint/数据符号。
- managed-declarations：每声明出现及项目包含状态，多项可指一 entry。
- types：struct/enum/handle/union/packet/fixed array/layout。
- callbacks：指针/descriptor/线程/寿命；com-interfaces：IID/vtable/ownership/apartment。
- binaries：具体原/候选 PE与验证。
- header 从批准 manifest 生成或人工维护后反校，不成第五签名真相。

## 3. Entry ID 与状态

### 3.1 ID

ABI-FUNC-<exact-export-name>、ABI-DATA-<exact-export-name>、ABI-COM-<interface-name>、ABI-CB-<owner>-<name>、ABI-TYPE-<qualified-name>。
保留大小写，文件系统不安全字符只在路径别名编码。

### 3.2 来源存在状态

- defPresence：Present/Absent/Conditional/Unknown。
- nativeDefinitionPresence：Present/DeclaredOnly/NotLocated/Conditional。
- managedDeclarationPresence：IncludedCurrentProject/SourceOnlyNotIncluded/Absent/Conditional。
- originalBinaryPresence、candidateBinaryPresence：VerifiedCode/VerifiedData/AbsentVerified/NotInspected。
- runtimeReachability：VerifiedCalled/VerifiedExternalOnly/VerifiedUnreachableForScope/NotTested。

SourceOnlyNotIncluded 不推出 binary 不存在，托管无声明不授权删导出。

## 4. `entries.jsonl` 必填字段

| 字段 | 规则 |
|---|---|
| schemaVersion/abiId/publicName | 首版1.0.0、稳定唯一、PE精确大小写 |
| kind/domain | Function或Data；COM/factory/media/UCE/geometry/HW/WIC/diagnostic等 |
| defSource | 路径/行条目/条件/静态属性 |
| nativeDeclarations | prototype路径/符号/calling-convention宏/条件 |
| nativeDefinitions | 路径/符号/配置条件/Ledger ID |
| managedDeclarationIds | 0..n声明记录 |
| candidateExportMethod | C#路径/方法；未实现null，不造stub |
| returnAbi | native类型/位宽/符号/HRESULT或BOOL或void |
| parameters | 原序数组，见§5 |
| callingConventionByArchitecture | x86/x64/ARM64分开 |
| nameDecorationByArchitecture | 公开/内部名及实测状态 |
| ownership | 创建/借用/转移/AddRef/释放/配对入口 |
| threading | affinity/lock/callback/thread/apartment |
| initializationPrerequisites | 首次进入/全局前置 |
| errorSemantics | 参数校验/out初始化/返回/异常映射 |
| resourceSemantics | HMODULE/resource type与ID/借用指针 |
| configurationBehavior | Debug/Release/PRERELEASE/架构条件 |
| differenceClass | Common99/ManagedOnly8/DefOnly7/ExtraBinary/ResolvedDecision |
| ledgerRecordIds/requiredEvidenceIds | 实现/声明/生成/资源；ABI/布局/caller/差分/E2E |
| status | StaticMapped/OriginalBinaryVerified/CandidateExported/CallerVerified/DifferentialVerified/Accepted |
| decisionIds | 差异/例外裁决 |

## 5. 参数字段

每参数按原顺序记录：
- index/name、nativeSpelling/canonical ABI type、direction（In/Out/InOut）、pointerDepth。
- nullable/空失败码、widthByArchitecture、signedness、passMode（value/pointer/reference/by-value struct）。
- boolKind：CppBool1/Win32BOOL4/NotBool。
- handleKind：DuceResource32/ClientPointerSized/COMPointer/Win32Handle/FixedUInt64ProtocolSlot/OpaqueToken。
- stringKind：BSTR/LPCWSTR/tail UTF-16/none。
- arrayElementType/长度参数/单位，structTypeId或callbackId。
- ownershipIn/ownershipOut、mustInitializeBeforeFailure、managedDeclarations marshalling 元数据、每架构 native/managed probe证据。

## 6. 托管声明记录

每 DllImport(MilCore) 出现单记：文件/类型/方法/行、PresentationCore.csproj或共享编译链实际包含、实际 EntryPoint、CallingConvention/CharSet/ExactSpelling/SetLastError/PreserveSig/BestFitMapping/ThrowOnUnmappableChar、参数返回 MarshalAs/SafeHandle/IntPtr/unsafe pointer、上层直接调用及可达性、abiId、SourceOnlyNotIncluded 原因/项目证据。
默认元数据也是合同，不能为统一风格改声明迁就候选。

## 7. 类型与布局 manifest

types每项：精确 native/managed名、来源/生成组/Ledger ID、kind（scalar/enum/flags/struct/union/handle/packet/tail-array）、underlying/pack/align/size/offset、x86/x64/ARM64分值、bool/BOOL/pointer/size_t/fixed UInt64解释、C++/C#有意非对称、native sizeof/offsetof及managed/AOT probe、golden bytes/负例、状态。
首批强制：CStreamDescriptor、CEventProxyDescriptor、AVEventData、generated command/resource/render-data、MilPathGeometry、back-channel union/message、GUID/RECT/WICRect、三类MIL handle与fixed64 protocol槽。
