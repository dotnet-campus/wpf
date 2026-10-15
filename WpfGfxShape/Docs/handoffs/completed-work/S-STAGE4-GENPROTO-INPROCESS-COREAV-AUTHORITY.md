# Stage 4 GENPROTO：进程内 core/AV 实施与真实差分

## 裁定

本轮为实际代码进展，不是 Stage 4 Closed。未改原 WPF、生产代码、冻结文件、golden、TFM 或 doneRule。没有执行终端、Perl、脚本、自行启动程序或沙盒。所有运行均使用专用测试接口。

## 新代码

`Tests/WpfGfxShape.GenProto.Tests/` 新增：

- `TemplatePreprocessor.cs`：完整读取 gencode.pl 后移植其 section/visibility/struct/enum/type replacement/include/header/version 状态规则；保留 IncludeHeaderFile 额外丢弃首个非 // 行行为。它是测试专用移植，不是 OriginalPerlExecuted，也不执行通用 C++ 预处理。
- `CoreAvPreprocessingTests.cs`：从真实 include 输入生成隔离副本，排除 processed，输出独立 .h/.cs；记录输入 SHA256、入口身份、输出 SHA256、行映射、全文及 token 差分。严格全文不等仍失败，不拼接许可证头假装再生一致。
- `CoreAvLayoutTests.cs`：三份独立目标模型报告，实际测试宿主均 X64；显式标记 nativeLayoutExecuted=false，不伪报三架构执行。
- `Stage4ClosureTests.cs`：全局 ID/引用/schema version/count metadata 检查；Closed 必须具备 Passed 且 executed 的 required evidence，Planned/Deferred 不得闭合。
- `HistoricalAuthorityTests.cs`：当前 SDK/CSP 配置来源和历史缺口独立报告。

## 执行结果

完整 GenProto.Tests 最新运行：退出码 1，13 项，11 通过、2 失败、0 skipped。失败是 core 和 AV 严格预处理差分；不隐藏差异。

固定归档：

- core：`WpfGfxShape/Investigations/GenProtoEvidence/preprocessing-wgx_core_types-4bbf830f2bcf46ca8771adb247c714ec/report.json`，SHA256 `0F6B14E09887884D206CF87AFE46335F4144A6FC706AA4E724C6C12A528C3E39`。
- AV：`WpfGfxShape/Investigations/GenProtoEvidence/preprocessing-wgx_av_types-b7b4d1727d3642a1bbe350364b066190/report.json`，SHA256 `5804A988C606825D6D6E69A2D0D3FB20C49B2E2512A7D8AF0BE2E7A655907BBD`。
- 初次独立 Ledger validation：`WpfGfxShape/Investigations/GenProtoEvidence/closure-validation-eeb6b2f515db4b37b5adb87c1b121cf0/report.json`。已回读：errors=[]，实际 records45/groups9/evidence71/workPackages10，recordCount=45，stage4Closed=false。该报告保留验证时快照，不冒充后续 Ledger hash。
- 三目标模型：`layout-model-X86-e20ea69870ac413ea1edcb1d930e0d63`、`layout-model-X64-e5ef555dcf2a4bde84e5d58f8517e5af`、`layout-model-Arm64-542b288dff8c4f69ab1e1fcf561bdbce`，均位于上述 GenProtoEvidence。不是 native 编译证据。

## 已实际判读的差异

AV 的 .h/.cs tokens 均一致；全文不等，因为冻结 processed 的文件许可证头不在 .w section 输出。仍然严格失败，未批准注释白名单。

core native 第一个 token 差异：冻结 processed 为 `struct MilMatrix3x2D`，当前 Generated/wgx_misc.h:955-963 为匿名 `typedef struct ... MilMatrix3x2D`。该 include 在 CPP_ONLY 中原样导入，非 TranslateCSLine 导致。

core managed 第一个 token 差异：当前 wgx_core_types_compat.cs:187 的 `MIL_RT_DISABLE_DIRTY_RECTANGLES = 0x00010000` 不在冻结 processed 的对应 enum 中。该 include 在 CSHARP_ONLY 中原样导入，非翻译器主动增项。其余差异保留全文/token 数组，尚未逐项全部裁定，不能宣称差分已满足。

## 布局和 counted payload

源：wgx_av_types.w:76-83、mediaeventproxy.cpp:297-377。模型字段 offsets=0/4/8/12/16，size20，align4，WCHAR[1]，variablePayloadStart16。消费者真实分配为 sizeof(AVEventData)+2*(typeLength+paramLength)，长度单位 UTF16 code units；无字符串时直接 RaiseEvent sizeof(eventData)。不存在冻结 managed AVEventData，保持 CPP_ONLY，不造对端。

源：wgx_core_types.w:275-300、336-348。DECLARE_MIL_HANDLE 定义为指针；HMIL_OBJECT/RESOURCE/CHANNEL 是 UINT32。MIL_CHANNEL/player/connection/transport 目标模型 x86=4、x64/Arm64=8。protocol pack(push,1) 不用于这些区段外的指针 typedef；Generated/wgx_misc.h 存在独立局部 packing。没有 native compiler/PDB 观察，所有目标维持 BlockedMissingNativeLayout。完整 core 逐字段 native/managed 检验仍未交付。

## 历史 fingerprint authority

workspace global.json SDK=10.0.107，rollForward=latestFeature。当前 tools/csp/csp.csproj 明确 net48、PlatformTarget=x86；mcg.proj 用子 MSBuild 构建 CSP，但没有固定历史 Configuration/CLR build/配置。当前配置修正了先前未掌握 bitness 的调查，不等于冻结文件的历史身份。

src 中未发现 CLR String.cs。没有已固定 CLR 源码 commit、独立 knownvectors、历史冻结生成输入清单或实际历史 runtime/configuration authority。NuGet public API 本身不能证明私有 hash 实现/条件编译。未移植猜测算法、未拟合 seed、未替换 frozen MIL 200184C0 / DWM 0BDDCB2B。历史身份仍是具体闭包障碍。

## Ledger 与未完成项

EVID58-63 保留全部 required，回填真实已执行 core/AV 差分路径和 report hash；布局仍未执行 native comparison，历史再生仍 Deferred；不伪造 Passed。schema metadata 加入四类实际数量和独立引用验证状态。refs 无悬空不等于完整 family generated/mapping/consumer 记录与合法 sourceSymbols 已完成。

本轮未进行正式 AOT publish、ordinary full、COM full，因此无本轮发布序号、DLL SHA 或 RunID，不能借旧报告宣称最新验收。原 resourcegenerator 专用测试项目尚未运行。mapping/stable/workitem/remaininggap 联合一致性尚未完成，Stage 5 未开启。

监督后续应继续整体闭环：完整判读 core 差异和 family records；取得固定历史 CLR/输入 authority 及独立 native 三架构布局 authority；执行其余专用项目、发布/ordinary/COM 并以固定 RunID/SHA 收口。保留原 doneRule，不以模型通过替代 native observation。
