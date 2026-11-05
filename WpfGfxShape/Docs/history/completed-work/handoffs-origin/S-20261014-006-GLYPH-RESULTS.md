# S-20261014-006：结果目录回收已修，glyph历史差异未确诊

## 实际修改与边界

本轮未修改生产代码。既有per-device Dictionary、ResourceManager销毁前释放target/移除键、weak owner及候选失败保旧版本仍对应生产SHA `95FE0E53423A755A92E022231364739A26A3A08C5C8A2726BE95440DD378BAD2`。不把前轮改动计为本轮新修。

AcceptancePaths新增仅Sandbox生效的模块初始化，将工作目录设为AppContext.BaseDirectory；MTP CLI相对结果目录因此落任务内。身份记录加入metadata原值、实际Results/工作目录/应用目录和CLI。AcceptancePathsTests新增相对TRX目录基准断言。没有改根runner、golden、阈值或skip。

## 回收Red/Green

修复前参数 `--results-directory TestResults --report-trx --report-trx-filename glyph-isolated.trx` 实际输出 `C:\lindexi\WinRemoteShell\TestResults\glyph-isolated.trx`，不在任务目录，回收包没有TRX；该次36/36仅有工具摘要，不提供未回收RunID。

修复后同一相对目录参数，full实际输出到 `C:\CodingAgentSandbox\Tasks\63c335daef7c4d348e9088ec7c82008e\TestResults\full.trx`。整个任务已回收至 `Investigations/S-20261014-006-GLYPH/full/`。读回身份显示Sandbox=true、CalibrationResultsPath=Results、ResolvedResultsPath及WorkingDirectory均在任务内；CLI TestResults与业务metadata Results各司其职，metadata并不控制MTP TRX。

## 同SHA实际测试

| 测试 | pass/fail/total | 已回收RunID |
|---|---|---|
| 本机glyph单独 | 26/0/26 | c65253d1-0bec-468a-9979-b0647518e026 |
| 本机COM full | 1383/12/1395 | 8c1f9054-6b61-4fb0-b9f0-626ad9b13556 |
| 沙箱COM full | 1387/8/1395 | f9fbebe3-6453-4d74-adf2-f37fc4b0d19c |
| 沙箱device+glyph+路径（新增断言后） | 39/0/39 | f8c37a8a-3ae5-4f3d-a19f-ac027e04b7e6 |

全部零跳过。本机普通实际4211/4212，唯一失败SystemVideoSampleTests加载 `Microsoft.WindowsDesktop.App/10.0.12/wpfgfx_cor3.dll` 缺失；GenProto实际23/23。普通/GenProto本轮未生成TRX，不编RunID。

本机full的12fail为原版runtime加载缺失；沙箱full的8fail为真实媒体notification deadline。两边本次full的glyph和device测试均通过。最终新增路径断言后仅重跑39项组合，不把此前full说成最终测试程序集full。

## glyph实际调查结论与仍开放合同

前轮device-full日志显示直接/间接glyph均在索引1302得到151而golden189。本轮同生产SHA单独及full均通过，因此没有证据将其判为本次devicecache生产改动的确定性回归，也没有证据宣称已修复根因。

GeneratedGlyphAlpha使用DirectWrite系统rendering parameters取得contrast，对纹理执行contrast变换；固定golden还包含FontSha256。历史失败尚未保全当时字体/contrast和原版权威同环境输出，不能确诊字体、显示设置或测试残留。未运行会覆写glyph-arial-original.json的原版记录测试。必须补独立非覆写oracle及同进程device清理前后显式顺序矩阵，保留当前golden断言；当前组合测试不保证清理前后顺序。该合同仍是我们可修的调查/测试工作，不能当外部假卡点。

## 本轮发布记录

以下命令均由专用发布入口执行；工作区相对路径。生产P1成功，产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`，身份同上述SHA。

1. P1 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，退出0。
2. P2/P3/P6（修改前/模块初始化后/新增路径断言后）各执行 `dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=true`，均退出0。沙箱仅上传 `WpfGfxShape/artifacts/com-acceptance-package`。
3. P4 `dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=false -t:Test -p:TestingPlatformCommandLineArguments="--filter FullyQualifiedName~GlyphPixelTests --results-directory d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/Investigations/S-20261014-006-GLYPH/local-isolated --report-trx --report-trx-filename isolated.trx"`，退出0，26/26。
4. P5 `dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=false -t:Test -p:TestingPlatformCommandLineArguments="--results-directory d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/Investigations/S-20261014-006-GLYPH/local-full --report-trx --report-trx-filename full.trx"`，测试Red退出1，编译成功，1383/1395。

专用普通dotnet test COM入口曾失败于.NET10 MTP/VSTest门禁，未改根runner；后续按包实际MSBuild Test目标执行。包产物在 `WpfGfxShape/artifacts/com-acceptance-package`，本机应用在 `artifacts/bin/WpfGfxShape.ComAcceptance/Release/net10.0-windows/`。

## 总计划

阶段4 Active，A/B/C未关闭。完整消费者事务/device寿命和glyph历史差异未达到全部模块完成条件；不是生产新模块交付或替换资格。runtime安装前置需外部恢复；oracle、状态保全、顺序矩阵与资源生命周期审查均可由工作区继续推进。
