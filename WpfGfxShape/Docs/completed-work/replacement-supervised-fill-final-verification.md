# CT fill 最终验收

## 最终裁决
安全撤回 CR 新挂接，未获有限真实双架构最终验收。完整替换未完成，不是像素失败裁决。
CT 未完成 x64 同包 Old/Native 矩阵、双架构必要17兄弟、全部原始 comparison 及 float/负半格/有限性/步长终止完整独立审计。按用户预授权选择安全撤回，不遗留 CR 未验收行为。没有证据说明工具无法运行；不将执行未完成伪装为环境阻塞。

## 已亲核 x86 原件
根 artifacts/cs-fill-acceptance/final-run-x86/，task cc18c3b74fad4a7c94ef499607fd62e3。
父TRX绝对位置 C:\CodingAgentSandbox\Tasks\cc18c3b74fad4a7c94ef499607fd62e3\Results\cs-final-x86\TestResults\child.trx，本地同相对位置可读。已读结果段及末尾计数，长段曾截断，不冒称122行全文阅读。
监督 Results/cs-final-x86/supervision.json 全38行亲读：exited/Exit0，HasExited及OutputCollected=true，无timeout/cancel/kill/drain timeout/SupervisorError。TRX total/executed/passed=17，failed/error/timeout/aborted/notExecuted=0；stdout17通过，覆盖CQ5、CR1、BW1、CK10。
测试 SHA B04139CA3BAE532E722F090E6759C48E59B61870A2195D79305CD537B3EF694B，MVID9ca00196-7748-40a6-926c-dcb22a761fa5，.NET10.0.13/X86/4，software RenderTargetBitmap。
stroke/comparison.json 全87行分段亲读：16行 differences=0/nonempty=true，unchanged=true，baselineIsCurrentSnapshot=false。两actual identity各14行全文：requested=actual，旧为任务 Old/Native/wpfgfx_cor3.dll，新为 Native/wpfgfx_cor3.dll，同一测试程序集。
旧 SHA C4D7D4699F6334AC164DE95573D382807803ED65E4FE42AA05A9559FEA7F44EB 等于 CrFillExactIntegrationTests 明确声明；新 SHA FD8B32FAF8D55CF72001DE9ACFE18A57E0814D92C38F63C4FE1D7D45047CD522，与旧不同。不是官方代候选或同候选重复。
frames/scenes.tsv11行、siblings/scenes.tsv17行全文：10+16目标均差0、repeat0、非空。device/precision完整comparison未审完，不据父通过外推全部原始场景已核。
以上仅绑定撤回前候选，不绑定撤回后源码；未重新发布扰动旧身份。

## 撤回范围与保留
仅修改生产三文件：
1. Abi/NativeShapeReader.cs：删除CR TryRasterPoint/DevicePoint及SoftwareFillBezier入口，恢复本地double控制点共享SoftwareBezierFlattener.Flatten(...,transform)、输出再Apply。保留原有限性检查、ABI、clip、TryCreateFigures。
2. Core/SoftwareStrokeContours.cs：删除CR fillLocalFloat参数和float局部乘加分支，CaptureSegment恢复transform.Apply，保留allowNonFinite的Add行为。
3. Core/SoftwareGroupStrokeContours.cs：仅删除ordinary path fillLocalFloat传参；CF fillPrimitive参数、递归与rectangle/ellipse调用保留。
修改后 NativeShapeReader76行全文、Stroke修改段10–45及Group73–98亲读确认。没有历史整文件SHA认证。不修改SoftwareFillBezier、CQ及CrFillExactIntegrationTests的CS_FILL_ACCEPTANCE显式隔离；CF GeneratedNativeStroke/SoftwareCurvedStrokeContours保持原状。
仅补完授权CR/CS报告及CT报告。未动NativeRun、ExitDiagnostics、VisualCache、historical、捕获、WindowQ、打包或共享交接。未使用终端或直接/间接脚本。

## 构建与证据边界
撤回后专用构建 Code/WpfGfxShape/WpfGfxShape.csproj，Release win-x64及win-x86均成功；两份结尾已亲读，各0错误、1479警告（含CA1416）。构建不代替NativeAOT发布或运行验收。
没有撤回后渲染运行证据，不以撤回前SHA通过认领恢复后通过。更广fill exact差异仍未闭；保留CF有限修复、整数实现与CQ，不声称生产整数接通或任意HFD/GPU等价。
交付选择：安全撤回，而非双架构通过。
