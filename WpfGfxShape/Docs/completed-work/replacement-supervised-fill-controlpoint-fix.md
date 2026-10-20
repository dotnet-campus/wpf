# BM NativeShapeReader 生产试验撤回记录

## 1. 裁决与生产撤回

试验未合格、未验收。首先创建本独有报告记录待撤回状态，随后读取当前 NativeShapeReader.cs，仅以两次唯一片段替换撤回 cubic 设备控制点预量化分支、删除新增 TryRasterPoint 方法，没有整文件覆盖。

撤回后按1–42、43–76行全文回读：已恢复本地控制点调用共享 double Flatten（transform 参与误差），对细分输出再 Apply 父变换的原消费行为。新增预量化及其范围拒绝逻辑不再存在；起点/直线/cubic有限性检查、figure ABI、clip与TryCreateFigures保持。核对依据包括 BI 报告114–133行记录的原消费顺序及现有 Flatten 签名。没有独立的修改前逐字节快照，因此不能把片段恢复夸大为历史整文件字节哈希认证。

本任务生产试验补丁已撤回；没有留下本次未验证的全局行为改动。未修改他人的 NativeRun、VisualCache、捕获或共享交接。本轮没有终端、脚本、发布、hash更新或新运行；没有补跑x86兄弟。未执行构建，已核范围是源码回读与既有证据，而非恢复后二进制回归。

## 2. 双架构原十帧证据与实际身份

路径均相对用户根 d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape。产物根 artifacts/bm-fill-controlpoint/，以下回收目录均在该根。

|架构|回收目录|业务根（目录内）|sandbox task|
|---|---|---|---|
|x64|run-experiment-x64|Results/curve-an/fa20fe46d0d546b7832778e30c439f65|45911528054c4b72b3509f8d3d9ef047|
|x86|run-experiment-x86|Results/curve-an/e666de6a04154ca9a26612e0d503eaad|b1f056d88f804764be671deb51dc1eb6|

亲读两份 bm-parent/parent.trx 全文各26行：TestRun分别8742c0d8-f117-46a3-b1c0-3fb4d46d71ba、66ed2653-4b14-47fe-a9fe-a68521f4fce3；原曲线parent均执行1、通过1、失败0，timeout/aborted/notExecuted均0。两份业务根comparison.json全文：threshold0、frames10、exact=true，frame0–9差均0，hardwareVerified=false。这只是已撤回试验的有限十帧通过，不是恢复后的通过或生产验收。

四份 original/candidate/identity.json 全文亲读，requested=actual path。官方实际路径为 C:\CodingAgentSandbox\Tasks\<task>\wpfgfx_cor3.dll，候选为同任务 Native\wpfgfx_cor3.dll。均.NET10.0.13、各自X64/X86；官方版本10,0,1326,50504，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；候选1.0.0.0。软件RenderTargetBitmap，不是窗口GPU证据。

|架构|官方actual SHA256|试验候选actual SHA256|
|---|---|---|
|x64|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|1C71D3B0BF08CCCBF0D06776DB0B34ACAE2AD28883877CDEE2BE566B24559492|
|x86|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|F26110CB17B6F44EEE5AADE1BF4FA3883A96D91B59F710DC749F8758EFA74AAA|

上述hash仅属于历史试验，不绑定撤回后源码。十帧identity未提供测试程序集SHA/MVID，不补造。

## 3. B/E、D/F真实失败

直接亲读上述两架构业务根 separator-summary.jsonl：x64第65–70、133–134行，x86第65–69、133–134行。不是160行全审，未逐像素重算二进制。初读误用 be-device-summary.jsonl 路径不存在，随后定位并读取真实 separator-summary.jsonl，不将缺失文件视为已审证据。

- separator-be-positive-0-geometry-fill：两架构官方B/E互差30，真实geometry对B30、对E0；候选real对官方30、line差44，非空。drawing-fill官方选B（对B0、对E30），候选real为0。
- separator-df-positive-0-nested-fill：官方D/F互差8，真实nested对D0、对F8、对C8；两架构候选real差8、line差28，非空。
- 同段stroke负控保留：geometry-stroke候选real140，nested-stroke候选real118，不能称stroke修复。

这些是BM候选自己的未合格结果，不是只引用旧BI失败。十帧exact没有覆盖这些真实合同失败，不能说原44/10生产已修。

## 4. x64 siblings、rounded与测试定位

亲读 BmFillControlPointTests.cs 全部内容（长段截断后补读）、正式csproj与acceptance.runsettings。注释说明fill consumer experiment，但它实际上是普通可发现的[TestClass]/[TestMethod]，没有实验类别或独立开关，SDK默认编译，runsettings没有排除。父方法对arc/rounded/ellipse/cubic × fill/stroke × identity/nested统一强制exact零差，确实把已知stroke差也设成了强制门。其用途应视为未合格诊断实验，不能作为新增绿色回归；现有普通测试发现行为仍在，未伪称已经技术隔离。

保留源码和断言原样：不skip、不放宽阈值、不改筛选或项目以隐藏失败。没有在本次仅撤回生产的任务中另行改变测试行为。

run-siblings-x64/bm-parent/parent.trx 全文41行亲读：task b01911419b2b49039d5d8fef8b3cedac；TestRun992898b5-61ab-4e87-a744-6ad17282cea1；执行1、通过0、失败1、退出2，timeout/aborted/notExecuted均0，失败在exact断言。

业务根为 run-siblings-x64/Results/bm-siblings/26772a5f563b40008ed5857ad25465c5/。comparison长单行读取被截断，随后精确文件名搜索补核：rounded-fill identity/nested均0且非空；rounded-stroke identity104、nested94且非空。已读前段arc-stroke128/122、cubic-stroke108/120。未声称完整offsets全审或重新计算像素；局部fill零差不取消父失败。

两角色identity.json全文亲读：actual=requested，task根官方及Native候选分别与上表x64 hash相同，版本相同；.NET10.0.13、X64、pointerSize8、软件RTB、hardwareVerified=false。测试程序集均为同task根WpfGfxShape.ComAcceptance.dll，SHA256为8F28D012F49B9CC30EA34FA05628A7CE560751A86D43117575AF17211395FFA8，MVID为f5bf814e-ae13-4c6a-b4ac-87147c733dd5。

siblings-x86已有发布目录不代表已运行，本轮不补跑、不编造TRX。失败测试不得宣称为新增绿测试。

## 5. 为什么有限十帧通过仍须撤回

试验将float设备变换后的四控制点先舍入28.4，再逆映射double交原double细分器。它能在有限十帧消除先double细分、只量化输出所致差异，但不是整数CMILBezier，也未证明整数HFD步长、舍入、范围与边界等价。

真实geometry要求本地float保存（分离器选E），drawing/visual父矩阵消费选B，nested要求本地保存后outer（选D而非本次F）。入口统一预量化无法修复上游float转换、本地保存及矩阵组合顺序，实际仍差30/8。Segment→NativeGeneratedPath type=2还丢失普通cubic/arc/rounded来源；无区分改变全部cubic会影响兄弟路径，有限rounded fill零差不能证明全局安全。

整数CMILBezier、完整来源传递和真实变换合同仍未实现。本轮只撤回，不另留猜测式生产改动，不宣称原44/10生产已修或完整替换完成。

## 6. 最终保留范围

本轮仅修改 Code/WpfGfxShape/Abi/NativeShapeReader.cs 的两处撤回及本独有报告。未删除、撤回或改写专属实验测试和产物。

保留 Tests/WpfGfxShape.ComAcceptance/BmFillControlPointTests.cs；既有 CurveDeviceQuantizationExperiment.cs、CurveCausalityExperiment.cs、RoundedRectangleContractTests.cs 均未动。保留 artifacts/bm-fill-controlpoint/ 下 experiment-x64、experiment-x86、run-experiment-x64、run-experiment-x86、siblings-x64、siblings-x86、run-siblings-x64 全部目录及试验DLL、包、TRX、失败比较、原始像素和身份文件。其他代理代码和共享交接未修改。

交付：生产试验补丁已撤回，实验及已知失败保留；没有恢复后的构建/运行认证。报告最后分段全文回读核对。
