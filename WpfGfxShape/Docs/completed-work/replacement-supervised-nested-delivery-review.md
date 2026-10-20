# CE nested / delivery 最终独立审查

## 1. 裁决及授权范围
**CD当前双架构软件nested基线已建立；没有新增生产修复，完整替换未完成。** 主代理仅监督。CE读执行规则全文27行、完整方案1–70/71–120、CD初稿、CB最终报告、CC报告及最新CA审查（均补读截断部分），仅专用读写搜索，不使用终端、直接或间接脚本，不新跑、不构建发布、不改代码。获授权补完CD报告，创建本报告并更新三共享文档最新优先节，保留历史，共五文档。

## 2. CD原件亲核，不以采集Passed冒充产品通过
详见[CD补完报告](replacement-supervised-current-nested-baseline.md)第2–5节，路径均相对 `artifacts/cd-current-nested/`：
- `run-{x64,x86}/Results/cd-<arch>/TestResults/child.trx`各26行全文；同目录supervision各38行全文。
- 同两根 `cd-baseline/{official,candidate,official-repeat}/TestResults/child.trx`六份各26行全文，supervision六份各38行全文，identity六份各14行全文。
- 两份 `cd-baseline/comparison.json`各1–75/76–152全文分段。

亲核合计8TRX及8监督：每份TRX Completed total/executed/passed=1/1/1，failed/error/timeout/aborted/notExecuted=0；监督均Exit0、exited，非timeout/cancel/kill/drain timeout，无SupervisorError。父各1/1及三角色各1/1仅为证据采集完成，**不是产品兼容、硬件或完整业务通过**。两架构18场景逐行非空、官方重复零差，但threshold0、exact=false、evidenceOnly=true、hardwareVerified=false。

当前两架构对官方完整字节差一致：nested ellipse/rounded各18，窄rectangle64/ellipse32，ellipse stroke identity/nested=162/128、rounded=106/60；其余10场景0。计数不是像素数。软件RenderTargetBitmap，不是物理GPU。未独立重算原始像素或哈希，不扩充亲核范围。

## 3. 当前实际身份与源码清单
actual=requested，候选在各task `Native/wpfgfx_cor3.dll`，官方在task根；不是官方冒充候选。

|架构|实际候选SHA256|test SHA256 / MVID|
|---|---|---|
|x64|F45485F1C747361D89E31FDB667A2C8C1C4920CE6BD6982A6AE93532C2E5A03C|10741D3A06A1D6EED2D4375886C2D77D9EEA200BB7B044AE370C92B4DC326E60 / 6922df89-9db3-4b2e-afe8-9c0e0f3f46ec|
|x86|09CD6C360304E02AEFF873BFDCB082CEC8CFC55D0736238C81E378EF1D81FABB|FDDA44A402D610745E58AB5361623C9F96B8E976DDAB524A89F6B563E93FA87E / 2916e482-c63d-482a-9ee2-6e7b98bdbcbb|

task x64 `96401b7596fa44908b0a6e0825eb6c49`、x86 `eba0acac405e4fa3888107a200bb4dd7`，.NET10.0.13，X64/8及X86/4，候选version1.0.0.0。实际官方SHA x64 `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`、x86 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`，版本10,0,1326,50504。官方版本commit不作本仓git修订，旧full不移绑。

准确亲核源码范围：`Tests/WpfGfxShape.ComAcceptance/CdCurrentNestedBaselineTests.cs`全文98行；CbNestedFillBaselineTests.cs 1–28/30–45；BwCurvedFillPrecisionTests.cs 1–65；`Code/WpfGfxShape/Abi/NativeShapeReader.cs:24–75`、`Core/GeneratedImageTransform.cs:1–45`、`Core/SoftwarePathCoverage.cs:266–405`。不是全仓dirty清单或整体修订hash。生产仍为共享Flatten后Apply、double变换组合及curved本地double构点的所读行为；未恢复分阶段float/整数fill。

CB整个历史测试以 `CB_ARCHIVED_FILL_DIAGNOSTICS` 显式隔离；CD以 `CD_CURRENT_FILL_DIAGNOSTICS` 显式隔离，默认不引入这些取证测试。CB缺归档仍失败，非改skip；BW默认TestClass/TestMethod、非空零阈值exact失败断言保留。默认两架构发布日志结尾亲核各97warning/0error，显式CD各101warning/0error，不称零warning。CE未取得落盘完整默认发现清单，不冒称总数/逐项核验；仅亲核源码发现边界。CD筛选没有执行默认BW，不以窄反例采集代填默认BW运行。

## 4. CB与CC：报告引用和亲核分开
[CB最终](replacement-supervised-nested-fill-baseline.md)全文已读；其中x64归档old/new同程序集nested18、八stroke完整输出不变及窄反例new0差属于**报告引用**，CE未重核其全部原件；x86旧归档缺失/父1失败Exit2继续保留。CD不是补造CB缺失归档。当前本地两架构基线与产物已可复用，不再追索消失远程归档。CB第6节默认隔离待做及必须找回x86旧归档是历史时点；后续以本审查当前状态为准。

[CC报告](replacement-supervised-present-delivery-controls.md)全文已读，以下均为**报告引用，非CE独立亲核CC原件**：x64 hDestWindowOverride目标/NULL与device创建线程测试/owner的2×2控制，四格12帧backbuffer red/blue/red正确且Present0，桌面/duplication未fresh，全部accepted/passed=false；两假设各自及组合均未恢复交付，仅限该会话，不代表所有驱动/架构已判定。CC打包依赖actual SHA是官方AD1C9DBB…，不是候选生产；未跑x86或PublicHwnd；final监督Exit2且非timeout，父child.trx未收回，不填正式Counters。CC_PRESENT_DELIVERY_DIAGNOSTICS显式隔离为其报告范围。不是“业务硬件通过”，不把缺父TRX移嫁给已亲核完整8TRX的CD。

CA历史结论和限制继续保留：BW/BZ父TRX缺口、BZ旧试验身份与当前源码分离、BX交付仍Red、BY CA2208修复保留；本轮未重核它们全部原件。BJ历史16/16成功不抹除，不无证归环境或称Basic Render Driver永不能产帧。CC下一定位为先补正式父TRX回收，再观测DWM重定向更新/duplication LastPresentTime；未执行，不当结果。

## 5. 下一最小修复与未闭
以CD当前双架构候选作old，限定恢复curved精度补丁，不同时恢复整套整数算法。每架构用同一新测试程序集执行old/new/独立官方及repeat，保留非空/threshold0，检查窄rectangle/ellipse零差、nested旧新完整输出及所有stroke完整输出无新增回归。**当前未修源码已存在nested18，不能一见nested对官方失败就撤掉精度补丁**；也不能以历史18掩盖新增差，必要时只撤回本包新增回归。保留原44/10和兄弟门，补丁合格后才继续更广fill集成。

无生产修复；原44/10未闭。BD旧x64 full1740/1699/41 Exit2、x86缺父full完成TRX保持原历史范围，不绑定CD SHA。真实GPU、loss/reset后新色、持续媒体、长期CPU/GPU资源线程callback/异步关闭、通知有效target同步/非零SyncModeStatus/迟到消息、圆角、合法大shader、E_HANDLE及首次堆损坏全部开放。文档收口不替代最后稳定源码Tests/ComAcceptance×x86/x64 Release完整适用四格，也不承诺未验证runtime版本兼容。

## 6. 文档交付
本轮仅CD报告、本CE报告、next-session-handoff.md、replacement-supervised-session-summary.md、replacement-issues-and-plan-status.md五文档；后三者前置最新优先节，历史不删除。五文档写入后已按短段无遗漏回读：CD报告1–74、CE报告1–43、交接1–274、会话汇总1–400、问题状态1–351；均读至文件结束，非只读最新节。收口结论始终是基线可复用、修复未交、最终验收未完成。
