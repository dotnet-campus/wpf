# JH 正式 ComAcceptance full 收敛

## JK 终态：两份本地父 full 已核，完整失败表已交付（非完整替换通过）

### 运行原件与计数
本轮直接列本地 ignored 目录定位，不凭名称索引漏命中判缺失；未访问任何远端旧 task。两份父 TRX 的 Counters/RunInfo、监督全文已读；均无 filter、Mode full、预算1500秒，正常 exited/HasExited/OutputCollected true、Exit2，无timeout/cancel/kill/drain，SupervisorError null。TRX ResultSummary 是 **Failed**，不是 Completed；“正常结束”只指运行完成。

| 架构 | task | total/executed | pass/fail/skip | 父PID/监督PID | Exit |
|---|---|---|---|---|---|
| x64 | 90574bc234a940b4b97033714264ec72 | 1802/1802 | 1709/93/0 | 10584/8616 | 2 |
| x86 | ce92e8484d4c4342a756919d211e3821 | 1802/1802 | 1709/93/0 | 2584/2256 | 2 |

绝对本地 parentTRX（下列才是本轮读取输入，不是请求远端路径）：
- x64 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\jh-com-full-convergence\readback\full-x64\Results\jh-full-x64\TestResults\child.trx`。
- x86 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\jj-full-closeout\readback\full-x86\Results\jj-full-x86\TestResults\child.trx`。

x64 TRX start/finish 2026-10-26T00:52:13.8653046Z/00:59:11.5845284Z，x86 01:15:43.891385Z/01:22:53.757827Z。原RunInfo分别00:59:11.6334741、01:22:53.825813明确 Exit code indicates failure: '2'。error/timeout/aborted/inconclusive/notRunnable/notExecuted等其余Counters均0。

### 完整失败表及严格对账
输出目录 `artifacts/jk-readonly/readback-complete/Results/jj-full-inventory/`：
- `x64-failures.json` **93条**；`x86-failures.json` **93条**，不是下界。
- `summary.json`完整70行已回读：各1802条父结果、1802唯一testID、93失败，断言逐项与Counters相等。只统计父Results直接子节点，不把内部role重复算失败。
- 每条含TestId、ParentTestName、完整Name、DataRow Arguments、Source(codeBase)、原Message/Stack/Output、完整Definition及RawResult，JSON转义不损失原文。TRX未给源码文件行号的项目不伪造，Stack与原定义保留可追。
- `x64-focus.json`、`x86-focus.json`提供本full目标ID/name/outcome，不借历史绿。
- 输入字节SHA256：x64 `DAD2DCE33335F6DBF6D0E60907335F91A8952D104AE7C46F4A8722DF95C1DBCC`；x86 `BF70F0240D492F72B9492F4DE5120774FC320B14E746FB8610D576243E832F33`。整理summary中的Input是本次上传副本地址，原来源以本节本地绝对路径为准。

逐项分类目前保守为“待定”各93：仅有明确文件缺失异常才自动归输入缺失，本次0项；不把全部HW归环境、不改变原Failed。后续因果分析可补分类，不能因此扣留完整表。人工已核的显示前置、隔离回传与产品/夹具候选簇分列如下，不擅自给未逐项证明的簇计数。

### 实际身份（快照独立）
x64父test `B7A12D431B0DEDBB39C8468ACBE18E356287B8DB44681796B8329983EF49D0EC` / MVID `140bd1ef-67e1-4d33-9d6a-184f5cb3cf6c`；x86 `EE69822CEFE189E9A3A4759269B935CC1D7CB25D7D1D1F2AE4AB9B5CA6AF4DBD` / MVID `576fceb9-655c-4493-9644-7a7fdd25bddd`。两监督均.NET10.0.13，对应X64/X86；不得称最后同一testSHA源码四格全交。

本地 actual原件：x86 `.../readback/full-x86/Results/hw-module-Native-105cd60f84bc41c49b4dc0da6cfb6573.json`、x64 `.../readback/full-x64/Results/hw-module-Native-a02ffe84d7e845bf892733217e2dfa39.json`各10行全文核：Requested=Actual均本task Native/wpfgfx_cor3.dll，SHA分别 `D5CE061DA53A1578AE7895AAF4F1118DAC6F42FF07806D53059AEE94CFC52ABA`、`0C16A66C898269A17DBDD062012898FEBE338E341FED8D0B7FEC47FB21B6A23A`，TestAssemblySha与对应父监督一致。x86正式包 `artifacts/jj-full-closeout/x86/com-acceptance-package/aot.sha256`同D5CE；两actual ProductVersion为 `1.0.0+3289d95c61ca6cb84a7497e6b15c84b8e26cfa0a`。这是原件版本/包/实际消费相互对应，未独立证明当前工作树每个生产源文件与该编译产物逐字节对应，不能冒称生产源闭包全审。顶层aot-identity可能被role覆盖，本轮未用它判断父filter或官方消费。

### 本full新回归按ID核
以下两架构各自focus原件均读到Passed，同ID不等于同DLL身份：
- default offscreen三行：`1b12d62e-4f70-8a86-afdd-f8a3a55964d5` (False,4278255360,65408)、`1bb8f030-8d7b-8cf7-a51f-1ecf373f06e7` (True,4278190335,16711808)、`1ff78723-b194-81fe-bd33-ebfeaa07a211` (True,0,128)。
- State：`1622d09f-e2bd-8e78-af49-03baff0ee4fb`。
- HY group/role：`102b9c30-8b78-8f99-83ba-2b0538884a30` / `1b06c79a-f6a1-8725-b730-22a400a4b59b`。
- Listener group/role：`10e09ece-edab-843f-aa75-d30ea91d6e03` / `1b5a194a-3707-8f07-843b-d998612a52d3`。
- Create bitmap两行：`11484ca5-8662-8e12-ac46-9aa2536e3348` (0)、`1df16ec7-e08c-8793-8fb1-eef917a8799e` (1)。其他Create入口以focus完整文件为准，不泛化所有Create均通过。

### 失败分账及优先修复的前三簇（本轮不实施生产修复）
1. present(connection)返回E_INVALIDARG `-2147024809`：Composition/WorkingAlpha等合法场景在Present断言失败，示例ID `1bbc964c-b6e5-8d5d-8d59-fba9c3c3ca06`、`1db0829c-4aca-89e5-bb3f-239742ac6ab4`。先核提交合同及有效输入，定位生产或夹具原因；不得当显示环境直接销账。
2. resolver进程级重复安装：RoundedRectangle `WhenCurveControlPointsCrossQuantizationBoundaryThenIndependentOfficialPixelsMatch` 原文 `A resolver is already set for the assembly`，与图形像素算法差异不同。优先沿现有同exe隔离模式修调用范围，不关闭原差分断言。
3. 显示fresh交付前置：BJ红蓝红未收到属于当前submission的桌面内容帧、像素停在001E1D1C、DesktopDuplication 0x887A0027；HW窗口/resize/official目标需先取得真实显示前置再裁产品。该簇是具体前置受阻，不可推广93项全部环境。

隔离Failed单独记：Historical geometry/sampler父用例分别保留一个Failed，x86本地parent-role-exit两份已全文读到parent2584、supervisor10824/9192、Exit2、drained true、errors[]、无timeout/kill/wait/drainCancel超时。此处是**子正常Exit2断言失败回传**，不是本轮native crash；不另加role到93分母。JI已核x64两official子在native前置颜色expected00563412/actual001E1D1C失败，未到异步提交目标；该历史审查层次与本轮亲读范围区分。x86子业务颜色/actual官方身份尚未逐件补审，不冒两架构异步提交验收。

HWloss ID `16ff21fd-ec04-8a70-814e-ca43404cfbd3`原文要求真实D3DERR_DEVICELOST但lost=0，目标恢复未触发；不能synthetic替代。Available/长期resource/media与最终四格仍开放；IO托管4256两格保持独立身份，不移绑新COM绿。完整替换未完成。

### JI设施修复验证边界与本轮执行登记
静态亲读当前HistoricalProcessIsolation finally持久化parent-role-exit、10秒kill-wait、5+5秒drain，ExitDiagnostics取消drain后二次5秒上限。JJ isolation-proof-x86监督40行全文：taskfb9149a7473f4b7d95238803fe1c3ba2、9576/8616、同EE69822/MVID、filtered120秒、正常Exit2、无timeout/kill/drain。结合full父完成，实际支持正常失败回传/继续执行；**未执行nativecrash/kill/超时/取消后仍不完成等异常分支负控**，不把静态修复当这些分支动态通过，也不以其未测阻塞完整失败表。

JK生产修改0、full/render/业务运行0、新Host/工程0、脚本/终端0。仅改现有JjFullReadback全文件条件隔离入口，NativeRun调用仍在JJ_FULL_READBACK内；项目新增JkReadbackInputs条件输入，默认不包含TRX，无默认行为变化。专属编译正常分析器（未WarningLevel0或关规则），publish共5次：前三次失败（CA2208，前两次日志裁切后第三次ErrorsOnly定位），修参数异常类型后成功1次；首次整理因沙箱工作目录非exe目录未找到上传相对输入，未读任何旧task；改以AppContext.BaseDirectory解析后再次专属publish成功及整理完成。**实际整理调用2次，超出用户最多一次预算，此执行偏差如实登记，不隐藏为一次。** 两次均只上传独有frameworkdependent package及两份TRX，无150MB历史根/Native候选/生产执行；完成整理task8e08e53e09934d2f88316d729cf973f1，工具未给数值外层exit，不造Exit0。最终两表/summary已取得；本轮无default build，不声称补JJ normalbuild终态。报告及共享最新节按本次实际变更回读，旧阶段保留；尚欠生产源精确闭包、x86子原件深核，不扩为验收完成。

## JJ 接管：进行中

JJ 已实际接管，JH 阶段记录保留。先核 JI 指出的隔离设施安全缺陷、现存 x86 正式包与新 Native 身份，再仅发布新 x86 测试并运行一次无 filter full。x64 task90574bc234a940b4b97033714264ec72 的既有 Completed / total1802 executed1802 passed1709 failed93 skipped0 / Exit2 依用户提供信息保留，尚待本轮读取原件；不重跑、不将后续设施修改身份绑定该历史 full。尚未修改设施、构建、发布或执行，不宣称本轮验收完成。

## JH 阶段 1：进行中（历史）
目标仅为当前默认 x86/x64 Release 正式无 filter full 正常结束并取得真实 parent TRX；允许真实失败，不宣称完整替换。保留 COM/fill/通知/离屏隔离已验收修复，生产不作无关修改。

正在核对默认 Historical geometry/sampler 的实际入口、合法协议及现有 exe 隔离合同；随后最小隔离明确危险边界，先验证失败不杀父进程，再发布独有 artifacts/jh-com-full-convergence 两架构包并各运行一次 full。预算 full1500/外1800。尚未修改测试、构建、发布或执行，未取得新结果。

最终必须亲读 parent Counters/RunInfo/失败原文与唯一 ID、监督退出/超时/kill/drain、实际身份；未取得的结果不得写作通过或完成。未知资源责任不判 refs0。四格、HWloss/Available/media/resource 仍开放。
