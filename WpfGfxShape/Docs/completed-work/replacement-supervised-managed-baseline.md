# AR 托管快照全量与集成证据审查

## 最终状态

**证据交付完成；双架构完整托管基线未完成，最终源码验收未完成。** AM现成快照x64全量4249/4249通过，x86未取得完成TRX；两格实际加载托管生产/测试程序集SHA尚缺，不冒用NativeAOT或历史身份。

报告先创建、最后更新并回读。仅写本文及独有 ignored `artifacts/ar-managed-baseline-20260721/`。不改生产、测试、启动、项目或主交接，不构建/发布、不清理共享、不与AP/AQ争用文件。使用专用读写及Windows沙箱正式C# exe，无shell/终端/脚本。已读交接执行规则、工作项、完整替换方案、capture-cleanup-preflight、bitmap-layout-fix及AO bitmap-curve-review。

## 包选择、配置与身份边界

选定原包 `artifacts/am-bitmap-closure-20260721-a/managed-{x86,x64}/publish/WpfGfxShape.Tests/Release/win-{x86,x64}/`。正式exe实际存在；x86原包、x64回收runtimeconfig均亲读includedFrameworks Microsoft.NETCore.App=10.0.13。实际执行banner/诊断确认win-x86/win-x64 .NET10.0.13，回收包包含coreclr/hostfxr/hostpolicy，不依赖沙箱安装x86 runtime。

AP/AQ并行改源码，优先复用稳定AM包而不重发生产。这是**AM产物快照全量，不是最终源码验收**。AM记录EnableMSTestRunner=true自包含发布；未取得能证明最终dirty源码的完整依赖构建链。当前Tests csproj为net10.0、MSTest4.0.1及生产ProjectReference。L历史预检BuildProjectReferences=false有旧依赖风险，不能将该配方机械作为最终集成构建。

亲读当前SandboxTestEntryPoint.cs：全文件受SANDBOX_TEST_ENTRY_POINT保护，条件成立时入口补绝对TestResults并返回RunAsync；预检类对实际加载测试DLL及wpfgfx_cor3托管DLL计算SHA256，写Results/preflight-identity.json。**选定AM包实际无此JSON；不能据当前源码推断包具有此入口/条件符号。** 本包不改启动或测试源码补证。源码身份仅限命名AM包快照，未追溯到最终集成源码。

SHA来源说明：AR没有取得托管实际加载程序集SHA，没有自行重算文件hash，没有用内嵌ProductVersion代替dirty源码身份。实际包和回收程序集保留不等于加载identity。NativeAOT SHA不能替代托管DLL SHA。这项要求明确未完成，不能写成已绑定身份。

## 本次无筛选正式运行

所有运行无filter、无skip，timeoutSeconds=1800，正式WpfGfxShape.Tests.exe直接执行；完整回收实际生成任务目录，不改golden/阈值。证据根 `artifacts/ar-managed-baseline-20260721/`。

|格|实际任务|回读结果|
|---|---|---|
|x64 Release AM快照|3164556516454d0d8977824dba4e7743|`readback-x64/TestResults/ar-managed-x64.trx:25503-25506` Completed，total/executed/passed=4249，failed/error/timeout/aborted/notExecuted及其余非成功计数均0；执行输出2s943ms|
|x86 Release AM快照诊断|e369e0371f1e46e49c50f37eb4ebad28|`readback-x86-diagnostic/TestResults/log_261019125013119.diag`共37830行，无完成顶层TRX；不能报告执行总数/失败数或成功退出|

x64原始绝对TRX：`C:\CodingAgentSandbox\Tasks\3164556516454d0d8977824dba4e7743\TestResults\ar-managed-x64.trx`。x86绝对诊断：`C:\CodingAgentSandbox\Tasks\e369e0371f1e46e49c50f37eb4ebad28\TestResults\log_261019125013119.diag`。最终两次使用runner默认任务内TestResults，诊断确认解析为实际任务绝对路径。业务Results在对应任务BaseDirectory内，两格均实际回收 `Results/present-notifications.json`，x64内容已亲读；未回收不存在的identity。

### x86末事件及无需改源码的最小复现

诊断:1-10确认MTP2.0.1、PID4580、runtime实际位于任务包内、实际参数。末尾:37814-37830：最后完成且收到Passed的是 `WhenDeviceIsDisposedThenInterfaceBackedAccessIsRejected`；最后开始为 `Direct3D9DeviceInterfaceOwnershipTests.WhenBaseDeviceQueryForExtendedInterfaceFailsWithPointerThenTemporaryReferenceIsReleased`，时间2026-10-19T12:50:15.6500185+00:00，随后日志结束。

四个并行worker，末事件仅是线索，不证明该测试造成崩溃。执行接口未给数值退出码、超时状态、异常栈或dump，原因未明；不能擅称已证明崩溃/卡死或MTP timeout。完整Trace和包已保留，未将进度/事件数量当测试完成计数。

复现：上述AM x86原包正式exe，无筛选参数 `--report-trx`、`--report-trx-filename ar-managed-x86.trx`、`--diagnostic`、`--diagnostic-verbosity Trace`。真实参数见诊断:10。不用过滤/降并行局部通过代替完整基线，本包未作此类改动。

### 输出路径失败尝试原样保留

最初outputRelativePath含 `..\ar-managed-baseline-x86`，接口执行前拒绝，不能算运行。随后x86任务fd5fcf12d64742e695c122bc25fa7c69显式传任务外绝对 `C:\CodingAgentSandbox\Tasks\ar-managed-baseline-x86\TestResults`，完整回收仅覆盖实际生成任务，故任务外TRX未回收；该次仅banner/worker启动，不能提供完成计数。回收包保留readback-x86。最终改用上述任务内默认绝对路径诊断复现，仍无x86完成TRX。此处调用方路径错误不能归因产品，也未擅改入口掩盖。

任务号、实际参数、执行接口输出与限制另保存 `artifacts/ar-managed-baseline-20260721/execution-observations.txt`。本包未覆盖受控失败返回码传播验证；不从“结果已保存”推断成功。

## AM/AO只读复核（不是AR新COM执行）

- 亲读AM `readback-x86/TestResults/am-family-x86.trx:2333-2338`、x64对应:2334-2338，各191执行/191通过，failed/error/timeout/aborted/notExecuted均0。
- 亲读x64 `readback-x64/Results/aot-identity.json`：Native SHA256 `90FA6BCB10C53E9E42221C5BA1524C55936401165EB12F5106127E05B63ACBE2`，X64/pointer8。Arguments实际是source-clip子角色，不能冒充父191全族参数身份。x86 Native SHA `C422361F16B9870A0C8B3B2F4B6544E004CFAD6C55F131975D23B28B248D7A12`来自AM/AO报告，本包未逐份复核其actual。
- AM闭环是兄弟夹具固定x64偏移，191 Green不关闭生产自产realizer；独立两个输入不能替代31组合广泛官方对照。
- 亲读AM `producer-x64/TestResults/am-producer-x64.trx:907-915`：119执行/103通过16失败，未执行0，TRX退出2。x86自产GetState/Ensure NullReferenceException、无完成TRX来自AM原observations和AO复核，不是AR新跑。

## AN2/曲线只读现况

没有找到名称含an2的独立产物目录，也无可定位独立AN2报告；以下明确是AO所链接AN最终“两项失败”证据，不冒称取得另一个AN2包。

亲读 `artifacts/curve-an/readback/com-x86-final/TestResults/curve-an-x86.trx:40-53`、x64对应:45-57：各2执行/0通过2失败，error/timeout/aborted/notExecuted均0，TRX退出2。x86原始结果明确为RoundedContent和CurveControlPointsCrossQuantizationBoundary两项exact失败。

原帧1/3/5各44字节差（官方36/候选40）；实验帧9十字节差（171/179），细节来源AN报告及AO，不宣称AR重新解码全部BGRA。官方10.0.13不能混用旧AK10.0.1；量化/HFD因果未闭，无已证生产修复，新GPU圆角仍未覆盖。

## 完整失败分类与未闭项

表格覆盖本包实际运行和要求的AM/AO/AN现况，**不是未执行COM全量的逐项失败穷举**。

|分类|证据事实|下一动作/界限|
|---|---|---|
|托管全量未完成/终止原因不明|AM x86旧3566只有进度；AR亦无完成TRX|补数值退出状态/必要dump/实际加载SHA，按末事件定位但不猜根因|
|身份/依赖缺证|两格无托管preflight identity，编译常量和最终依赖链未追溯|后续有界输出机制包处理；不禁依赖构建后声称最终源码验收|
|调用方回收配置错误|首次任务外TestResults不能随任务回收，上级相对路径被拒绝|保留尝试，最终任务内日志实际回收，不改入口掩盖|
|夹具缺陷已闭|旧x86 bitmap31项固定x64偏移，现各191/191|不外推自产或全部官方组合|
|自产ABI/状态崩溃|AM x86 GetState/Ensure NullReferenceException，无完成TRX|AP/AQ并行处理，AR不抢文件、不宣称根因已证|
|功能/呈现差异|AM新x64 119中16个image-mask present(connection) HRESULT失败|不是缺reference；修后仍须内容与完整回归|
|历史过滤遗漏|AJ旧128中25=锁源16+RotatedDrawing6个present失败+VisualMask3个缺reference|AM119未包含另9项，不证明它们已修；分类来自AM/AO而非AR逐条穷举|
|缺正式reference|VisualMask3项缺ReferenceData/original-wpf.json|走正式官方生成，不改tracked golden追绿|
|曲线软件功能差异|AN双架构各2失败，原44字节及实验10字节差保留|量化/整数HFD因果未闭，不能猜修复|
|环境触发不足/关键能力未覆盖|capture无合格新内容帧、真实loss→Reset→新色HW消费未证、新GPU圆角未证|不全归环境，不以软件fallback冒充HW|
|时序/资源/更广集成未闭|一般VSync/同步、非零SyncModeStatus、在途销毁/迟到回调、持续媒体、长期资源和广泛随机|托管全量不补齐这些门|

集成稳定后仍须正常依赖图完成两正式项目×双架构Release最终适用矩阵，回读实际加载SHA、顶层TRX及原始业务证据。当前只有x64 AM快照全量完成；x86完整基线、双架构托管SHA身份、最终源码验收及COM/真实HW/恢复全验收均未完成。
