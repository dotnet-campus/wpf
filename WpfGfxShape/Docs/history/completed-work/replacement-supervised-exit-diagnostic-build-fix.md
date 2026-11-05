# BY：共享退出诊断构建修复

## 范围与进行中记录

根目录为 d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape。只修改 ExitDiagnostics.cs 和本独有报告；产物根 artifacts/by-diagnostic-build/。已读执行规则、BW/BX 最新报告、BL 监督说明、ExitDiagnostics、资源文件及 NativeRun 调用点。禁止终端和脚本；使用专用构建发布工具和 Windows 沙箱正式同 exe。

## 根因与修复

ExitDiagnostics.RunAsync 的参数数量校验使用 ArgumentException(nameof(args))，实际将 args 作为 message，触发 CA2208。改为 ArgumentException(message: null, paramName: nameof(args))：保留 ArgumentException 及无效参数数量的语义，明确参数名 args，使用框架本地化默认消息，不增加硬编码用户文案或资源。相邻 ArgumentOutOfRangeException(nameof(args)) 已使用正确参数名重载，不改。资源 AcceptanceResources.resx 不变；NativeRun 分派不变。

不压制分析器，不设置 WarningLevel/NoWarn，不修改 SDK/TFM、项目、生产、曲线、BX 捕获或共享交接。BZ 并行生产变化不视为旧候选内容；拟复用 BD 独有 full-x86/full-x64 中已发布 Native/wpfgfx_cor3.dll，不重发生产。

## 正常构建发布与快照

正式 ComAcceptance.csproj，Release、self-contained=true、ComAcceptanceSandbox=true；正常 restore/build/publish 图，无跳过依赖/构建。默认未设置 DefineConstants；诊断设置 DefineConstants=BX_DESKTOP_DELIVERY_DIAGNOSTICS，不改项目默认值。四格各自 BaseIntermediateOutputPath、BaseOutputPath、PublishDir、ComAcceptancePackageDirectory 指向独有根下 obj/bin/publish/package，使用既有 PackageSelfContainedComAcceptance，不覆盖公共包。

|独有目录|本地开始时间 2026-10-20|结果|亲读 build.log 汇总行|
|---|---|---|---|
|default-x64|14:56:44|发布成功，92 warning、0 error|2411–2412|
|default-x86|14:57:01|发布成功，92 warning、0 error|2404–2405|
|diagnostic-x64|14:57:13|发布成功，92 warning、0 error|2404–2405|
|diagnostic-x86|14:58:06|发布成功，92 warning、0 error|2405–2406|

日志保存在各目录 build.log，包含正常还原图、编译、发布及打包记录。不宣称零警告；无 WarningLevel/NoWarn/分析器开关。

AotDllPath 显式引用 artifacts/bd-comacceptance-baseline/full-x64 或 full-x86 的 Native/wpfgfx_cor3.dll，不编译发布当前生产。亲读 BD 原 aot.sha256 与 BY 默认包打包时重新计算的 aot.sha256，一致：
- x64：1C381539E7C82DDD75908C0563F0AC062E583166BDB23AEE066BDD21E838752F。
- x86：E0D3F5EFEDFACFD5CC3491DD43B194E19028E8B541A462123C8F73C73D6E25B5。

这是 BD 旧候选，不是 BW BR/BU，不绑定并行 BZ 新生产源码或其验收结论。

## 测试发现与默认边界

Windows 沙箱正式 package/WpfGfxShape.ComAcceptance.exe --list-tests --no-progress：
- default-x64 task 92df6b8d7fb04edb99f2ab5e88434ed5，发现1758；完整工具返回清单没有 WhenSameWindowGdiControlPrecedesNativeFramesThenBothDeliveryBoundariesAreRecorded。
- diagnostic-x64 task 06e2033251764cb98ebfb05280d6cf98，发现1760；含上述 GDI 方法 (1)/(3) 两项。
- 两份清单保留 owner gate (1)/(3)、fresh native gate、historical sampler/geometry 和 BW 精准失败 WhenNarrowPrimitivesAreTransformedThenIndependentOfficialContentMatches。归档 stroke 批量方法不在清单，与 BW 收口一致。
- 清单由工具直接返回；list-default/list-diagnostic 回收任务目录，但命令未自行写 stdout 文件，不冒称目录含落盘完整清单。未执行GDI，未加Ignore/skip或改过滤规则。选定默认控制实际 notExecuted=0；未运行全量，不能推断1758项全部无运行时skip。

## BL 同 exe 受监督路径控制

输入分别 default-x64/package 与 default-x86/package 正式exe；参数 --supervise-exit by-control 30 FullyQualifiedName~WhenSandboxPathIsContainedThenItResolvesUnderApplication；外层上限75秒。子参数含TRX、同步diagnostic、独有结果目录，未关闭并行。

|架构|task|UTC起止 2026-10-20|监督/子PID|TRX id|
|---|---|---|---|---|
|x64|998a892b1c1d43cab073ff442e079968|06:57:26.9212351–06:57:27.5744816|1248/9912|de00df00-0512-4989-9084-7c457ddfa8a1|
|x86|5037888ee0e545358f1f87a5a3101b89|06:57:31.1881222–06:57:32.0093461|8160/8836|0286103e-e002-49d3-91d2-eeb2b4be2c76|

本地产物 run-x64/by-control、run-x86/by-control。两份 supervision.json 全文38行、TestResults/child.trx 全文26行、stdout全文12行和空stderr已亲读。TRX均Completed，total/executed/passed=1/1/1；failed/error/timeout/aborted/notExecuted等其余计数全0；stdout同样1成功0失败0跳过。这是业务子会话TRX，不是监督器自身TRX。

监督均Reason=exited、HasExited=true、ExitCode=0/0x00000000、TimedOut=false、Cancelled=false、KillAttempted=false、KillReturned=false、OutputCollected=true、OutputDrainTimedOut=false、SupervisorError=null。数值退出来自实际监督JSON，不从工具“执行完成”推定。

## 实际程序集身份

监督JSON与TRX codeBase 指向同一任务根 WpfGfxShape.ComAcceptance.dll，runtime=.NET10.0.13，不从本地publish推定：
- x64 Architecture=X64，SHA256=124E4320DC9CB51FABEE2DB866CF894BB8D7C1D3E3D1FB75C29889E26E7427CE，MVID=68e585b4-bee4-4449-883c-499059521414。
- x86 Architecture=X86，SHA256=01BE18C31AD6922CE77D2AF073779BF88D0229519039AFCA8BA04F15394C76BA，MVID=e9e7ea67-6805-4d58-8142-481105efe287。

仅打包旧候选并运行路径解析，不是加载候选验证precision/呈现。未生成新生产身份，未将旧包称为当前源码快照。

## 交付与未覆盖范围

最终回读确认第24行正确重载；改动仅 Tests/WpfGfxShape.ComAcceptance/ExitDiagnostics.cs 单行和本报告。资源、NativeRun、项目SDK/TFM、BX窗口捕获、生产/曲线、入口分派和共享交接均未修改；无终端、脚本或额外Host。

CA2208阻塞消除；双架构默认/显式诊断正常构建发布，默认发现隔离和双架构最小控制有上述证据。未运行full、GDI捕获、historical sampler、BW/BZ precision或硬件业务，不将成功当作完整替换验收，不改变BW/BX原Red及BL堆损坏结论。
