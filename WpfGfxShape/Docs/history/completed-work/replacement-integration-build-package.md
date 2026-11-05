# 两包集成可构建性修复与定向复验

## 裁决与实际变更

本包仅修复两包合入后的测试项目可构建性。已读取两份 replacement 包报告、当前执行规则和 COM 沙箱运行合同。双架构 Release 发布成功；最后代码修改后的 x86/x64 定向回归各 52/52 通过。不是完整替换验收，不覆盖全量测试或 HW 捕获根因。

唯一代码修改：`Tests/WpfGfxShape.ComAcceptance/WpfReference/FloatReadbackTests.cs` 增加 `using WpfGfxShape.ComAcceptance;`。

根因：`InternalContextLayout` 是 `WpfGfxShape.ComAcceptance` 命名空间中的 internal 辅助类，官方 FLOAT 测试属于 `WpfGfxShape.WpfReference`，后者没有导入前者。项目文件清单确认 `InternalContextLayout.cs`、`InternalContextLayoutTests.cs` 和 `WpfReference/FloatReadbackTests.cs` 均由同一 ComAcceptance 项目编译；SDK 默认编译项没有将新增文件排除。因此这是跨兄弟命名空间的名称解析错误，不是缺少 ProjectReference、internal 可见性或架构条件编译问题。无需改项目、复制偏移或公开辅助类。

保留所有布局公式、断言、测试、golden、阈值、skip 以及 DesktopDuplication 诊断；生产源码未改，未重发生产 DLL。没有新增项目、原生代码、脚本，没有删除历史证据或清理旧目录。

## 实际构建

通过专用发布能力依次发布 `Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`：

- 共同参数：`-c Release --self-contained true -p:RuntimeFrameworkVersion=10.0.9 -p:ComAcceptanceSandbox=true -p:DebugType=None -p:DebugSymbols=false -p:ProduceReferenceAssembly=false`。
- x86：`-r win-x86 -p:AotRuntimeIdentifier=win-x86`；PublishDir 为 `artifacts/publish/replacement-integration-win-x86/`，ComAcceptancePackageDirectory 为 `artifacts/staging/replacement-integration-win-x86/`，实际传入对应工作区绝对路径。退出码 0，编译、发布和自包含打包成功。
- x64：对应 `win-x64`，目录后缀改为 `win-x64`。退出码 0，编译、发布和自包含打包成功。

未关闭分析器，仍输出既有 MSTest/CA 警告；本包不顺带修复这些警告。两架构都编译了新增辅助类、布局测试、官方 FLOAT 夹具及上包 HW 诊断代码。此次未发生磁盘不足，不将上包阻塞当作本轮失败，也没有为释放空间删除证据。

## 最后源码定向实跑

使用各自全新 staging 包的标准 `WpfGfxShape.ComAcceptance.exe`，MSTest 4.0.1 / .NET 10.0.9，自包含 Windows 沙箱执行，完整回收任务目录。

共同 filter（各项以 `FullyQualifiedName~` 匹配并用 `|` 连接）：`InternalContextLayoutTests`、`FloatReadbackTests`、`InternalDrawUsesFloatTarget`、`FloatSpanStretchesHdrGradient`、`InternalBitmapClipHasNoTargetArea`、`NativeZeroOpacityLayerDraws`、`NativeLayersEndOrAbort`、`NativeLayerNeedsAlphaFixup`、`PublishedArtifactTests`。另传 `--report-trx --report-trx-filename replacement-integration-x86.trx` / `replacement-integration-x64.trx`。

|架构|回收根（相对 artifacts/sandbox-readback/）|TRX counters（实际回读479行）|运行身份|
|---|---|---|---|
|x86|replacement-integration-win-x86|total52/executed52/passed52/failed0/notExecuted0；error/timeout/aborted0|X86，pointer4，任务5da42183fbb9457ab4fc49976c887a46|
|x64|replacement-integration-win-x64|total52/executed52/passed52/failed0/notExecuted0；error/timeout/aborted0|X64，pointer8，任务b093a358ba4641bfa29541c1f5b48fcf|

每个根的 `TestResults/replacement-integration-<arch>.trx` 和 `Results/aot-identity.json` 均在最后代码修改之后回读。布局测试已在两架构实际执行；不是仅构建通过或沿用历史 full。TRX 的 clip、FLOAT span 和 layer 条目亦有实际 Passed 记录。

本轮 identity 中候选实际文件位于各自 `C:\CodingAgentSandbox\Tasks\<任务ID>\Native\wpfgfx_cor3.dll`：

- x86 SHA256：`C113F60F19BD6129C3A14381E9ABF79F584EA3881A31DFEAC3C1697E4E2D909B`。
- x64 SHA256：`2BFC4AC24A83EBAF45A4B375CDC5FE1F9C90BD9370D73BD590C259091C5BF58F`。

这是本轮回读值，不沿用首包 x86 SHA；生产未重发，不能由测试重发推断生产身份发生变化。

## 官方 FLOAT 内容抽样与证据边界

本轮官方夹具生成的文件仅位于 ignored artifacts 下的沙箱回收目录，未写入 tracked ReferenceData：

- x86：`ReferenceData/original-float-draw-afe87a80c1ae42f9a470e4a59517556b.json`（width6），官方路径为该任务根的 `wpfgfx_cor3.dll`，SHA256 `0CC79CC937B17827344B8652556A4CFD774CFF64B7FD2709EF5284C480894047`。
- x64：`ReferenceData/original-float-draw-cf3b42503ebf40539bef67e0a022ded7.json`（width6），对应官方 SHA256 `098D9CEFACA7B1E36460EB28F2725D7AFDB8E93855C29F8DD3627A1FB692AE8E`。

实际抽样回读两文件身份和30–61行输出：包含蓝通道0.122138776、0.2663556、0.47932017、0.7605245、0.76815116及红1/绿0/alpha1，报告明确为原生采样经过 sRGB clamp 的诊断，不是 HDR/DrawingImage golden。两架构还生成了 width10 DrawBitmap 和原生 FLOAT 写入/readback JSON，由 TRX 附件索引保留；本包未逐项重新解析所有像素，不宣称独立全场景零差分。

已观察到官方 DrawBitmap JSON 的 Contract 文本历史写死 `Original Release/x64`，x86 运行也会输出该字面值；不能将此标签当作架构证据。架构以本轮 MTP 与 identity 的 X86/pointer4、X64/pointer8 为准。本包不扩改报告格式；这些 FLOAT 报告没有完整官方 fileVersion/productVersion 字段，不补造模块版本证据。

## 剩余项

- 本包关闭上述 CS0103 和两包最终测试源码的双架构可构建性缺口，取得限定布局/FLOAT/clip/layer 回归证据。
- 未执行全量回归；未执行 DesktopDuplication/HW control，本包仅证明其诊断代码随项目可编译，保留其诊断和原有失败裁决，不扩展到捕获根因。
- Realizer/BitmapRealizer 成员 ABI、其他兄弟夹具固定偏移、source18/affine3 mask 的1字节差异，以及完整官方角色版本/backend/生命周期与实际恢复证据仍不由本包关闭。
- 局部52/52不能外推为历史32项关闭、完整替换或未执行场景通过。
