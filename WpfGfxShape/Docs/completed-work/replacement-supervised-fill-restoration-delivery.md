# CX 当前源码 fill 精确恢复与有限验收

## 授权补完与范围

CY 已取得用户明确授权补完本初稿；主代理仅监督。原初稿“尚未修改生产或取得新运行证据”为运行前历史状态，现以本报告及 [CY 独立验收](replacement-supervised-fill-restoration-acceptance.md)更新。CY 不改源码/测试/项目，不使用终端、直接或间接脚本，不新构建、发布、运行，不重跑已完成矩阵。

## 已完成恢复及源码清单

仅恢复 `Code/WpfGfxShape/Abi/NativeShapeReader.cs`、`Code/WpfGfxShape/Core/SoftwareStrokeContours.cs`、`Code/WpfGfxShape/Core/SoftwareGroupStrokeContours.cs` 对应三段，不整文件替换历史归档。
NativeShapeReader Capture 使用 float 设备乘加、半像素偏移、28.4 TryRound，并实际消费 SoftwareFillBezier.Flatten；有限/范围失败返回错误。Stroke 普通 path 恢复 fillLocalFloat，默认 false，非 fill 仍走原 Add/transform.Apply，allowNonFinite/gap 合同保留。Group ordinary path 按 fillPrimitive 传参，group/combined 递归、rectangle/ellipse curved 分支保持 CF 实现。
`SoftwareFillBezier.cs`、`GeneratedNativeStroke.cs`、`SoftwareCurvedStrokeContours.cs` 为保留依赖，不是本次新增恢复文件；TryCaptureFill→fillPrimitive→group/curved 链保持。CY 未修改上述文件。

## 当前源码新运行（非历史归档移绑）

根 `artifacts/cx-fill-restoration/`，以下父 TRX 均以根为前缀；对应同目录 supervision.json 记录 exited、Exit0、输出回收，无 timeout/cancel/kill/drain timeout/SupervisorError。

|运行/task|父 TRX|结果与族|
|---|---|---|
|x64 task8c2daf9e393a4e6f85a3459974581884|run-x64-02/Results/cx-x64/TestResults/child.trx|23/23；CQ5+CR1+BW1+CK10+Br5+格式1|
|x64 geometry task385b819965644d109ac98685fc3b1b33|geometry-x64/Results/cx-x64-geometry/TestResults/child.trx|PublicGeometry12/12|
|x86 taskb10fd8bf380b455395eed2b09fb6f4a1|run-x86/Results/cx-x86/TestResults/child.trx|34/34；CQ5+CR1+BW1+CK10+Br5+PublicGeometry12|

CQ5 是四组 fill 父用例加 stroke 不变父用例，不是五个像素；fill206=frames10+device178+precision2+siblings16。stroke16 是有限旧新不变场景。Br5 和 CK10 是整数算法族，不等于 candidate DLL 新增产品门。额外格式1仅记录 Pbgra32 与 Prgba128Float 构造不支持，未取得 FLOAT oracle，不是产品新增验收门。

## 实际身份

|身份|x64|x86|
|---|---|---|
|当前 candidate SHA256|F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7|7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA|
|同包 Old/Native CF SHA256|36353DDE3BA720B642A6F12CEE8AB0EDDA501BD7DC49F35270063480B8A0532A|C4D7D4699F6334AC164DE95573D382807803ED65E4FE42AA05A9559FEA7F44EB|
|测试 SHA256|9BC9F796EA680A207B7E43999B895251E935343E55D63352BD5B36827490B2A5|AB92DF00DEBE205E06D3DAAFF1FA247DB47184CFE98185FDE56CFB845B004D68|
|测试 MVID|236b761c-ad2f-4292-9741-7a447b168465|f0813943-164f-4b7e-bc51-13e2a154e5e5|
|official/repeat SHA256|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|

实际模块根分别为 `C:\CodingAgentSandbox\Tasks\8c2daf9e393a4e6f85a3459974581884\` 与 `C:\CodingAgentSandbox\Tasks\b10fd8bf380b455395eed2b09fb6f4a1\`：candidate=Native/wpfgfx_cor3.dll，baseline=Old/Native/wpfgfx_cor3.dll，official/repeat=根 wpfgfx_cor3.dll；requested=actual。官方/repeat 不经 candidate，独立进程，同版同架构 .NET10.0.13，pointer8/4；官方版本 `10,0,1326,50504 @Commit:4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。software RenderTargetBitmap，hardwareVerified=false。geometry x64 同测试身份、同 candidate SHA，独立 task，不能把 aot 路径记录夸称每项业务 actual 模块枚举。

## 有限结果与交接

当前双架构 fill206 comparison 全文记录 threshold0/exact/discriminating、全 differences0/repeatDifferences0/nonempty/accepted；stroke87行全文16差0非空，unchanged=true、baselineIsCurrentSnapshot=false，actual old/new 不同。precision/BW 父通过，Br5关键结果均零差/threshold0/非空。原44/10在当前双架构这些有限场景已修复，当前源码保留，不因初稿滞后撤回。CY 的精确亲读层级、未独立重算边界见 CY 报告。

不能外推所有 HFD/clip 组合、大曲线、GPU、任意 runtime 或 full。Br 未提供候选 actual 模块身份，不从算法族通过推断 DLL 消费；PublicGeometry 大数组未独立逐项重算；像素/buffer SHA 未独立重算。旧 BD x64 full1740/1699/41 Exit2、x86 full无父TRX仅属历史，不绑定新 SHA。
下一最小步是在保留此修复的基础上建立当前双架构 ComAcceptance 基线、定位剩余失败；最终稳定源码 Tests/ComAcceptance×x86/x64 Release 四格完整适用 full 仍须做。本轮不执行。nested/stroke 其余差异、真实 GPU 内容、loss/resize 新帧、media、长期 resource/线程/callback、通知、同步 Event0、E_HANDLE、首次堆损坏、圆角/大 shader 等未闭合同全部保留，完整替换未完成。
