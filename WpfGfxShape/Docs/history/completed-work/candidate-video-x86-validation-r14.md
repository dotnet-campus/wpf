# r14：独立 win-x86 NativeAOT 真实视频有限门通过；OverallOpen

## 结论与边界

已先读最新 next-session-handoff、r13、r6；没有继续 x64 桌面 nativeControl 白像素循环。当前生产项目在本机构建环境确实能够发布 win-x86 NativeAOT Shared DLL：一次专用生产发布成功，不是依据“.NET 10 应当支持”的推断。新 x86 Native 与新 harness 独立冻结；最终同身份包两次全新 Windows sandbox 运行通过原公开视频有限门。没有借用 x64 SHA 或绿色结果认证 x86。

本轮生产源码和生产配置修改 **0**，未改 TFM/SDK/PublishAot、oracle、expected、颜色/时钟阈值、DPI、flags、default async；未引入生产测试钩子或测试耦合。无终端脚本、无原版图形 DLL 加载/副本 fallback。共享恢复入口未更新。

本结论只认证现有 candidate-only 公开 MediaPlayer/VideoDrawing/RTB 视频验收，不是 HWND 桌面呈现、完整并发/故障交错、COM refs0 或 full 产品闭合。r13 桌面白像素仍未解决，full 缺可信 oracle 继续保持门禁，本轮没有调用正式 full 包或绕过其检查。

## 静态核查与本机 x86 编译支持实证

- `Code/WpfGfxShape/WpfGfxShape.csproj:4-16`：net10.0、Nullable enable、PublishAot=true、NativeLib=Shared、SelfContained=true、IsAotCompatible=true；未指定限制 x64 的 RID。项目本身没有额外 LangVersion。
- 根 `global.json:2-10`：SDK 请求 10.0.107、rollForward=latestFeature、paths 为 .dotnet 和 $host$。已查看宿主 SDK 目录，存在多个 10.0 SDK；不把目录存在等同于实际选用版本，不声称本次已经读到精确选用的 SDK 版本。
- `Directory.Build.props/targets` 原样核查：本地构建隔离和 artifacts 路径，无关闭 NativeAOT 的替代分支。本次生产发布单独指定 r14 obj/bin/native，未覆盖既有 x64 Native。
- 一次生产发布：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x86 --self-contained true`，额外仅设置专属 BaseIntermediateOutputPath/MSBuildProjectExtensionsPath/BaseOutputPath 和输出目录。退出码0，输出明确包含 `Generating native code` 和创建 native wpfgfx_cor3.lib/exp。未设置 PublishAot=false 或 managed acceptance。
- 发布的 `artifacts/candidate-video-x86-r14/obj/project.assets.json` 实际解析 Microsoft.DotNet.ILCompiler/10.0.13、runtime.win-x86.Microsoft.DotNet.ILCompiler/10.0.13。
- `obj/Release/net10.0/win-x86/native/link.rsp` 保留真实链接参数：VS 18 Professional / MSVC 14.51.36231 的 x86 lib，Windows SDK 10.0.26100.0 的 x86 ucrt/um，microsoft.netcore.app.runtime.nativeaot.win-x86/10.0.13 原生 bootstrapperdll/Runtime.WorkstationGC 等。该文件和 ilc.rsp/obj/lib/exp 是编译证据，不需再重复生产发布。
- 最终 sandbox `Evidence/runtime-audit.json:20622-20628` 核实候选 machine=I386、managed=false；bootstrap 同时严格拒绝 CLR header。实际 x86 进程成功加载此唯一 Native 并执行公开视频，不是托管模拟。

生产发布有既有 CA1416 等警告，不宣称零警告。发布工具返回的生产 stdout 为尾部截断（原615946字符、只显示尾20000字符）；已回读其中生成原生代码和成功输出，**不声称保留了未返回的完整编译 stdout**。本次不存在平台不支持/x86 工具缺失失败，故没有虚构此类失败日志。

## 验收端最小改动

仅修改两个已有独立验收文件：

1. `Tests/CandidatePureVideo/Program.cs`：进程架构校验由硬编码 X64/Amd64 改为严格 X86/I386 或 X64/Amd64 匹配，其它架构拒绝；记录 processArchitecture。SHA、PE、无CLR、候选预载、实际路径/模块、同名句柄和唯一图形模块检查原样保留。
2. `Tests/CandidateVideoSupervisor/Program.cs`：wrong-architecture 负控根据真实 PE machine 写相反架构（I386→Amd64，其他→I386），并重算负控摘要，保证 x86 不会误用原来的 I386→I386 无效变异。没有削弱拒绝断言。

首轮发现独立验收日志竞争，随后仅在 CandidatePureVideo 的 Resolve/Checkpoint 使用同一 Evidence 锁串行化 requests/证据序列化与文件写入。没有重试视频像素、改业务断言或改生产代码；新 harness 重新冻结。共用 PureVideoPublic.cs 与 MediaHelpers.cs 均未改。

固定私有 runtime：CandidatePureVideo 原项目已固定 Microsoft.NETCore.App 和 Microsoft.WindowsDesktop.App.WPF 10.0.13，本次以 `-r win-x86 --self-contained true` 发布；生成 runtimeconfig includedFrameworks 两项均10.0.13。Supervisor 同样 win-x86、自包含10.0.13。实际 coreclr/PresentationCore/WindowsBase 路径在任务 Package 下，runtime 自带 wpfgfx_cor3 被既有打包规则剔除；包内唯一路径为 Native/wpfgfx_cor3.dll。验收宿主是托管 WPF 程序不代表候选为托管：候选另有 I386/无CLR 认证及真实 native 加载证据。

## 冻结身份

证据根 `artifacts/candidate-video-x86-r14/`；路径均相对此项目根。

- 新生产 x86 Native：SHA256 **3D889C1817AF9CE80D8223437063FFA142E44CAF1DA69BC41138062A93050AD8**，4295168字节，I386，无CLR。
- 最终 harness：SHA256 **786F1C3632D1B7712A314C8B707D751A5F391C6A99C3B42466880173F0320EBC**，58368字节；MVID **f11f4b43-7658-4184-bafb-aa35cfc8714e**。
- 首轮失败 harness：1AB7AA40F8A66031FDA6F7AE7FA9A28EBD289EAC225C9CDB653237506015B9D6，MVID 8afc15d1-451b-4ccc-8489-fb4ab7fe57ea，仅历史失败，不与最终绿色混用。
- 原 x64 Native SHA 2E5DC1BC…5705AFEE 未作为 x86 认证输入。

生产输出 `native/`；初始包 `package/`、`supervisor/`；最终包 `final-package/`、`final-supervisor/`。每次运行先由 supervisor 保存 package-identity，再负控、再公开视频；子进程结果实际计算 Native/harness 身份并由 supervisor 比较，不只是文档手填 SHA。

## 首轮失败原件（不冲销）

`readback/`，task **06e13b3888de4efea0c89dfa9153a24f**。

- 三项负控 missing-digest/wrong-sha/wrong-architecture 均拒绝通过：子PID2344/2872/8916，exit1，timeout=false、reaped/drained=true、killError=null。
- 监督PID9224 exit1；视频PID11884 exit **-532462766**，timeout=false、reaped/drained=true、killError=null。
- 原 result 已写 stage=purevideo-completed，但不能据此称首轮通过。stderr 为 finalizer 的 SafeMediaHandle.ReleaseHandle→Resolve→Checkpoint 与主线程同时写 result.json 导致 IOException 文件占用。该异常发生在验收证据写入，不能据此判生产失败，也不能忽略真实进程失败。
- 原件包含 stdout/stderr、父子退出、身份、result、AVI、四份 Pbgra32，均保留。只修验收日志竞争后以新 harness 重跑。

## 最终两轮新增运行及原始结果

两轮均使用未再重发布的同一 final-supervisor 包。每轮先三负控、后业务。视频超时上限120秒、父监督180秒、回收/管道各10秒；外层sandbox上限300秒，未新增无限等待。

### 第一轮：final-readback

Task **e9396cb95b38429981919a1d88e6dab1**。

- 监督PID9392 exit0；视频PID168 exit0。均timeout=false、reaped/drained=true；视频killError=null、failures=[]，两份stderr为空。
- 三负控PID2432/2840/9304均exit1，拒绝门passed=true，timeout=false、reaped/drained=true、killError=null、failures=[]。
- package-identity、candidate-video.json 的 harness/MVID、新Native完全一致；processArchitecture=X86，actualNative 位于此task Package/Native/wpfgfx_cor3.dll，nativeBasenameHandleMatches=true。
- stage=purevideo-completed、exitCode=0；allWpfgfxModules 仅实际候选一项，finalModules 保留。baseline 为私有宿主且无提前加载图形模块；未见 original/fallback 图形库。

### 第二轮：repeat-readback

Task **f5c4db2dbd8c41a6a7026d768b6990a5**。

- 监督PID2392 exit0；视频PID9564 exit0。均timeout=false、reaped/drained=true；视频killError=null、failures=[]，两份stderr为空。
- 三负控PID6424/12160/8480均exit1，拒绝门passed=true，timeout=false、reaped/drained=true、killError=null、failures=[]。
- 身份与第一轮相同，processArchitecture=X86，actualNative 为新task自身 Package/Native/wpfgfx_cor3.dll，同名句柄匹配；stage=purevideo-completed。

已回读两轮父子退出、负控清单、身份、stderr、actualNative、关键视频记录；不是只看工具“执行完成”。两轮原始result均保留完整模块清单及runtime审计。

### 两轮共同真实业务证据

保留原公开视频场景全部断言：生成4秒2x1无音频AVI，公开MediaPlayer/VideoDrawing读取，seek到0.5/2.5得到红/蓝，原EOS；随后同步回到零后只Play一次、中途无Pause/seek，连续红蓝并要求第二EOS；同一MediaPlayer三轮相邻 Close(); Open(uri);，两调用之间无I/O/等待。

两轮均保存AVI与四份原始Pbgra32：purevideo-0.5、purevideo-2.5、purevideo-playing-0.5、purevideo-playing-2.5。result实测四次sample依次0000FFFF、FF0000FF、0000FFFF、FF0000FF。第一轮连续帧before/afterPosition分别0.8/0.8及2.8/2.8，paused=false；元数据HasVideo=true/HasAudio=false/CanPause=true/duration4秒/尺寸2x1。两轮reopenedState均cycle=2、opened=4、ended=2、元数据保持不变；events保留三个reopen-Close-Open-return及随后Opened，不以延迟重开替代。

这是实际系统MF解码与候选Native公开桥的运行结果，非x64替代或白像素截图认证。systemMediaModules也保留mfplat/mfreadwrite/mfcore等实际模块路径。

## refs、未闭合门与执行账

现有公开视频入口未保存逐对象最终COM Release计数，因此 **COM refs0=Unknown/未认证**。父子exit0、Close完成和系统模块清单均不等同于refs0；未新增生产测试接口或导出以伪造该证据。不挪用r10/r13释放记录，也不宣称任意callback admission/最终释放故障路径已证明。

本轮执行：

| 类别 | 次数 | 结果 |
|---|---:|---|
| 专用生产Native publish | 1 | win-x86 NativeAOT成功，有既有警告 |
| CandidatePureVideo publish | 2 | 初始/日志竞争修复后均成功 |
| Supervisor publish | 2 | 初始/最终均成功 |
| publish合计 | 5 | 5成功、0失败；没有无意义重复生产发布 |
| Windows sandbox | 3 | 首轮失败保留；最终同身份两轮通过 |
| 每轮拒绝负控 | 3 | 合计9次均按预期拒绝 |
| 独立build / dotnet test | 0 / 0 | 不虚构单元测试执行 |
| x64桌面 / 正式full业务 | 0 / 0 | 原门保持 |
| 生产源码/配置修改 | 0 | 无测试耦合 |

没有未回读的sandbox任务。交付给监督者的结论是：**独立新 win-x86 NativeAOT 交付构建能力已实证，冻结身份的有限真实视频门两轮通过；refs0、桌面nativeControl、可信oracle/full及r7并发故障边界仍Open。**
