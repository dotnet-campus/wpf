# BZ curved primitive precision：实际执行补完（CA授权收口）

## 1. 最终裁决

原十行是进行中初稿，本节以本次专用工具亲读原件补完。有限验收失败；没有合格生产修复。fillPrimitive/ApplyFillLocal 试验已不在当前 Code 搜索结果中；撤回不等于修复，不认证整文件历史 hash。CA仅补报告，不修改代码、不重发生产、不重跑 full。

## 2. 双架构实际执行

证据根 `artifacts/bz-curved-primitive/`。亲读 `run-final-x64/Results/bz-final-x64/{supervision.json,stdout.txt}` 全文38/18行及 x86 对应文件全文38/16行。筛选均为 BwCurvedFillPrecisionTests|BzCurvedPrimitiveTests，监督上限120秒。

|架构|task|UTC起止|监督/子PID|stdout total/pass/fail/skip|实际退出|
|---|---|---|---|---|---|
|x64|6a745f1ff0274348865e6d5662205c9f|07:08:44.616438–07:08:47.9489366|4244/7340|2/1/1/0|2|
|x86|1d2aa7ee2a154da2bb1535ecf623d4f3|07:09:16.8436318–07:09:21.1535192|8816/8456|2/1/1/0|2|

日期2026-10-20。两份监督均 Reason=exited、HasExited/OutputCollected=true，TimedOut/Cancelled/KillAttempted/OutputDrainTimedOut=false，SupervisorError=null。失败为 sibling fillsExact 断言，不是监督超时。stdout声明生成 Results/bz-final-<arch>/TestResults/child.trx，但本次递归搜索整个 BZ 根未找到 child.trx，两份 run-final 搜索 .trx 仅命中报告扩展 DLL。**父TRX未取得，计数只来自stdout，不冒称亲读TRX。**

## 3. 业务原件与差异

两架构 `run-final-<arch>/artifacts/bw-curved-fill/precision/comparison.json` 全文1–19：threshold=0、exact=true，rectangle/ellipse 各count=0、nonempty=true、hardwareVerified=false。只说明归档试验候选修复这两个有限反例，不能绑定撤回后的当前源码。

两架构 `run-final-<arch>/Results/bz-siblings/comparison.json`：fillsExact=false、threshold=0、hardwareVerified=false。ellipse-fill-nested=18；rounded-fill-nested=18；rectangle/group identity及nested fill、ellipse/rounded identity fill=0；均非空。ellipse stroke identity/nested=162/128。没有同一测试程序集对旧/新DLL的 nested sibling 基线，不能将18差认定为新增回归，也不能认证不回归。未逐字节重算原始pbgra32；这里亲核的是实际比较JSON及身份，不是独立二次像素运算。

## 4. 实际加载身份

亲读两架构 sibling official/candidate identity.json 各全文1–13，与父监督测试身份一致。requested=actual，远端前缀 `C:\CodingAgentSandbox\Tasks\<task>\`；官方在根 wpfgfx_cor3.dll，候选在 Native/wpfgfx_cor3.dll。runtime=.NET10.0.13；x64 pointerSize8、x86 pointerSize4。

|架构|候选SHA256|测试SHA256 / MVID|
|---|---|---|
|x64|8DD2BF6272D1FE969AC409F920C90F2D9977647A44F3373945D6A65377278CE5|61C65566033AD99CF676CB7CD3EBA9136EE588A03D1A51ED3ECDADD40E184480 / 58aa7c92-970d-40be-8d94-efffd454568d|
|x86|DE4F55623582E2396383D38F4BC469990A7A4AAFAB52A343C6B266C1FB7E3D01|618D8BDCE17F0D071EC4B08BC6BEC5FAA6756729C8AB9456877B50739FCA4411 / bf5b1935-514d-45fb-95fa-152a2e2ebc11|

官方SHA x64 AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，x86 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834；版本10,0,1326,50504 @Commit:4099f83977f4d29604a9aeb971fb0a7bc1d5f699。候选版本1.0.0.0。均是归档试验身份，不是当前源码认证。

## 5. 当前实验接入与后续

亲读 `Tests/WpfGfxShape.ComAcceptance/BzCurvedPrimitiveTests.cs`：internal类及internal方法，无TestClass/TestMethod发现属性；Tests内名称搜索仅该文件命中，没有显式调用入口。源码虽保留角色启动逻辑，但当前是**未接入实验**，不能以历史2项执行推定当前发现或验收。CA未删改、跳过任何现有回归。当前编译状态另见CA独立报告，不从旧程序集推定。

下一最小任务先用旧新DLL、同一程序集、相同nested场景确认18差是否新增，再做float分阶段及整数fill集成；不得盲恢复已撤回试验。原44/10、真实GPU、loss、资源/媒体/通知/大shader及最终四格仍开。报告补完不是完整替换完成。
