# CB nested fill 旧新基线与限定修复

## 1. 本轮裁决与边界

本轮完成已有双架构执行证据核查与报告收口，**双架构像素基线仍未闭合；不恢复 curved fill precision 生产修复，也不恢复整数 fill**。原因是 x86 的真实旧归档获取失败，不是已证明与 x64 一致。不得以官方 DLL、其他候选或历史 BZ 结果替代缺失的旧模块。

本轮只修改本报告，未修改生产源码、测试源码、捕获、NativeRun、ExitDiagnostics、VisualCache、WindowQ、项目打包或共享文档；未使用终端或脚本，未构建或发布，未留下本轮未验证源码改动。以下都是既存归档执行证据，不认证当前源码 NativeAOT。相对证据根为本工程 `artifacts/cb-nested-fill/`。

## 2. 亲读范围与父 TRX

已直接读取以下原件，不用 stdout 代替 TRX：

- `run-final-x64/new/Results/cb-x64/TestResults/child.trx` 全文 26 行；同目录 `supervision.json` 全文 38 行。
- `run-x86/new/Results/cb-x86/TestResults/child.trx` 全文 39 行；同目录 `supervision.json` 全文 38 行、`stdout.txt` 全文 16 行。
- `run-x86/new/archive-acquisition.json` 全文 4 行及 `Results/aot-identity.json` 全文。
- `run-final-x64/new/Results/cb-x64/cb-baseline/comparison.json` 分段完整读取 1–223 行；四个角色的 `identity.json` 各全文 14 行。

注意：目录列举及名称检索没有列出 TestResults/child.trx，但按监督中明确记录的路径直接读取成功。不能将检索未列出写成 TRX 缺失。

|架构|任务|监督 UTC 起止（2026-10-20）|监督/测试 PID|父 TRX total/pass/fail/notExecuted|实际退出|
|---|---|---|---|---|---|
|x64|bfc211195bd2458eb29708bc2facc20d|07:29:39.281055–07:29:42.8155642|1184/7088|1/1/0/0，Completed|0|
|x86|c74260c6a4a04a2081753490c2540668|07:35:12.4757194–07:35:13.3202597|4280/7212|1/0/1/0，Failed|2|

筛选均为 `FullyQualifiedName~CbNestedFillBaselineTests`，监督上限 120 秒，父 TRX 使用任务内绝对 `new/Results/cb-<arch>/TestResults`。两份监督均 Reason=exited、HasExited=true、OutputCollected=true；TimedOut/Cancelled/KillAttempted/OutputDrainTimedOut=false，SupervisorError=null。x86 不是超时，也没有 skip 或假成功。

## 3. x86 准确失败与证据缺口

父 TRX 的失败是 `WhenArchivedModulesRenderThenNestedBaselineIsClassified` 在 CbNestedFillBaselineTests.cs:40 的 `File.Exists(source)` 断言：`Required original archive is absent; no substitute module is permitted.`

archive-acquisition.json 记录原始路径：
`C:\CodingAgentSandbox\Tasks\b5b7dd6aa2b0446aa688c50e6709f4f1\baseline-x86\Native\wpfgfx_cor3.dll`，`exists=false`。

测试在获取旧模块时即终止，尚未启动 official-old/old/official-new/new 的四角色渲染和比较。因此本轮没有 x86 的 comparison、四角色 actual module identities 或旧新完整像素不变证明；不能把 x86 填成 nested18 旧有、precision 已修复或 stroke 不变。

父监督测试身份：SHA256 `EDC596C2696260AEAA4611854DD193220D539F6FD76AB85FD7BD97B844F7E908`；MVID `b14a4a24-aa96-4244-8f9c-c7e44f768ece`；runtime `.NET 10.0.13`；Architecture X86。

`Results/aot-identity.json` 记录候选路径为当前任务 `new\Native\wpfgfx_cor3.dll`，SHA256 `DE4F55623582E2396383D38F4BC469990A7A4AAFAB52A343C6B266C1FB7E3D01`，PointerSize=4。此文件是候选配置/文件身份，不是四角色 Render 完成后的实际加载模块证明，更不是当前生产源码验收。缺失旧归档不可由此补齐。

## 4. x64 完整比较结果

比较设置 threshold=0、hardwareVerified=false、evidenceOnly=true。以下数值是全 Pbgra32 字节差数量，不是不同像素数量。完整 18 行场景均 nonempty=true、officialRepeatDifference=0。

|场景（省略 .pbgra32）|old 对官方|new 对官方|old 对 new|
|---|---:|---:|---:|
|ellipse-fill-identity|0|0|0|
|ellipse-fill-nested|18|18|0|
|ellipse-stroke-identity|162|162|0|
|ellipse-stroke-nested|128|128|0|
|ellipse（precision 窄反例）|32|0|32|
|group-fill-identity|0|0|0|
|group-fill-nested|0|0|0|
|group-stroke-identity|0|0|0|
|group-stroke-nested|0|0|0|
|rectangle-fill-identity|0|0|0|
|rectangle-fill-nested|0|0|0|
|rectangle-stroke-identity|0|0|0|
|rectangle-stroke-nested|0|0|0|
|rectangle（precision 窄反例）|64|0|64|
|rounded-fill-identity|0|0|0|
|rounded-fill-nested|18|18|0|
|rounded-stroke-identity|106|106|0|
|rounded-stroke-nested|60|60|0|

在此次 x64 同一测试程序集、相同参数的归档对照范围内：两个 precision 窄反例 new 非空且与独立官方零差；ellipse/rounded nested 的 18 字节差是旧有，旧新完整输出零差；八个 stroke 场景旧新完整输出零差。ellipse/rounded stroke 对官方的 162/128/106/60 历史差仍在。

nested 关键像素 SHA256：ellipse 旧新均 `59AC4CA00ADDCA88B512547303933E4F5B13DF12D805FC2D7663794EC361E223`，官方重复均 `04D1678628CC336289DB38A00D1F1E37431468BAD438952EF9F45B18998EC91C`；rounded 旧新均 `EA57D12FFAB7B2F9C9C44B4F24F42906A13E35650E82F7A07C3C1BD3D50B9C2E`，官方重复均 `D85CCE08DB5A13D0F11371168129AF746785A08B161DD7665AC8154BF1819805`。

这些是亲读完整 comparison 的结论，本轮未独立重算原始二进制像素。父取证测试 Completed/Passed 只代表收集与分类成功，**不代表 exact compatibility 通过**；两个 nested18 对官方 exact 继续失败，不降低阈值，也不要求同时修复所有历史差。

## 5. x64 actual identities

四角色 requested=actual，任务前缀均为 `C:\CodingAgentSandbox\Tasks\bfc211195bd2458eb29708bc2facc20d\`。old 实际路径 `baseline-x64\Native\wpfgfx_cor3.dll`，new 为 `new\Native\wpfgfx_cor3.dll`，两次官方均为 `new\wpfgfx_cor3.dll`。

|角色|实际模块 SHA256|版本|
|---|---|---|
|old（撤回旧基线归档）|8D48ADBB30D24BD9CEAF4505969126450B52034BCD3726204B15C6EF126FBB79|1.0.0.0|
|new（BZ 归档试验候选）|8DD2BF6272D1FE969AC409F920C90F2D9977647A44F3373945D6A65377278CE5|1.0.0.0|
|official-old / official-new|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|10,0,1326,50504 @Commit: 4099f83977f4d29604a9aeb971fb0a7bc1d5f699|

四角色测试 SHA256 均 `5B385438DBC1C3959AED3E95901D0BDBDF5C186FCE16316A46839F1F716165AA`，MVID 均 `132f0e28-8e53-47c1-8f17-ff49e3248047`，与父监督一致；runtime `.NET 10.0.13`、architecture X64、pointerSize=8、backend=`RenderTargetBitmap software`、hardwareVerified=false。new 的身份与 BZ 历史试验吻合；不是撤回后的当前源码 NativeAOT。

## 6. 当前测试接入审查与未闭项

已读 CbNestedFillBaselineTests.cs 的归档获取、四角色启动、comparison 生成、Render 及身份写出逻辑。该类当前仍有默认 TestClass/TestMethod 发现属性；archive-source.json 若指向过期远程任务会直接失败，如此次 x86。不能让默认产品验收依赖该历史远程目录，不能把旧 DLL 偷换成当前源码验收，更不能改成无归档 skip 或成功。

本轮没有修改测试发现方式；专属显式编译符号隔离历史取证仍待落实。隔离时应保留缺归档失败断言及历史产物，正常独立官方 precision 产品回归必须继续默认发现。当前 Render 通过反射调用 BzCurvedPrimitiveTests/BwCurvedFillPrecisionTests 私有 RenderContent；仅记录此耦合，不进行泛重构。

后续必须先取得可追溯 x86 原始旧归档并核对 SHA，用正式 C# exe 收回四角色 actual identities、完整 comparison、父/子 TRX 与退出监督，才能判定双架构是否满足恢复条件。若再恢复生产，只能从原始证据/代码精确重建最小 curved fill precision 修改，不整文件覆盖、不恢复整数 fill；用独有新子 root、正常依赖图发布当前源码双架构，默认独立官方 precision 回归、nested 相对旧基线不变及同程序集 stroke 全像素不变均须验证，关键失败应精确撤回本轮改动。

本轮最终源码改动：无。有限生产修复：未恢复、未保留。尚未完成：x86 旧归档与像素基线、历史取证默认发现隔离、默认独立官方产品回归、当前源码双架构 NativeAOT 验证。真实 GPU/HFD、完整替换和最终四格均不在本报告认证范围内。
