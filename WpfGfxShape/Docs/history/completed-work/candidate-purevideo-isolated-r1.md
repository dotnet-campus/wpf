# Candidate purevideo 独立包 r1：已构建、真实负控；视频仍未运行

## 范围与实现

新增验收辅助项目 `Tests/CandidatePureVideo`（csproj、Program.cs、MediaHelpers.cs）和 `Tests/CandidateVideoSupervisor`（csproj、Program.cs）。生产修改0；普通 ComAcceptance.csproj/Package.targets/full门禁修改0；expected/DPI/flags/像素阈值修改0。未运行original/full/compare，未复制或加载原版gfx，未使用终端或脚本。

独立入口只接受两个精确参数 `candidate purevideo-public`，其他参数exit64；不引用普通验收项目、生产项目、full/glyph reference。直接链接原 PureVideoPublic.cs；CopyMedia/WaitMedia保留原合同，AVI辅助仅保留该场景无音轨asset0分支。场景及Dispatcher清理放入NoInlining ExecuteWpf；认证和InstallResolvers方法不引用WPF类型。认证按摘要存在、SHA、原生PE/无CLR/Amd64顺序，文件只读租约覆盖后续作用域。启动及安装后有模块基线与wpfgfx预加载拒绝门。resolver只将确切wpfgfx_cor3[.dll]映射到认证绝对candidate路径，其他含wpfgfx名称抛错；其他系统库返回0不是gfx fallback。

**未完成执行接线**：ExecuteWpf尚不可达；认证后安装resolver，再明确抛出PreBusinessIsolationUnproven。此为阶段性安全停止，不是完成的视频入口，不是永久阻塞方案，也不是安全保证已闭合。尚未实现成功场景后加载身份核对、finalizer回收及consumer成功收据。不能删除该拒绝后直接运行。

## 固定包和审计

专用publish固定self-contained win-x64 NETCore/WPF 10.0.13；ComputeFilesToPublish后排除runtime原版wpfgfx，发布后检查根目录不含gfx。仅Native目录复制已授权candidate，并从新包重新计算aot.sha256。发布不依赖宿主已装WPF。没有修改SDK/TFM/语言版本或普通门禁。

`artifacts/candidate-purevideo-r1/final-readback/Evidence/runtime-audit.json` 保存每个包DLL的SHA、PE machine、是否managed和全部managed PInvoke；runtime-summary.json汇总目标。最终包candidate SHA复核为 FBD35CDD609DE2D67EB494B41A07FDB9240DC955B3F54D681D48F2423C8A4959。PresentationCore SHA 0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31，媒体MILMediaOpen/Close/GetPosition及MILRenderTargetBitmap等导入名确为wpfgfx_cor3.dll。DirectWriteForwarder SHA AEC60CD1A89ECE8F998DD7029BA9E4FD51211A20DB1A6F940B16E5AED39CECD7。

审计辅助只使用PEReader/MetadataReader和字节串，不加载WPF/Native。字符串标记不是本机导入表解析或显式加载路径证明。尚未解析本机普通/延迟导入、未反编译目标PresentationCore模块初始化与显式LoadLibrary调用链、未认证其递归OS依赖。DllImportResolver仅作用安装的程序集PInvoke，不拦截native LoadLibrary或其他程序集，Assembly.Load也可能早于resolver触发初始化。NoInlining解决场景JIT边界，不解决Assembly.Load自身初始化。包内无原版和模块基线不能独立证明不会向系统路径请求原版。因此真实WPF继续0，不把static audit写成加载预防证明。

## 专用执行结果

专用publish共6：candidate首轮1失败（缺System.IO，已修正）；candidate后续2成功；supervisor3成功。sandbox共3：首轮负控、只读审计、最终源码负控。Native发布0，dotnet test0，真实WPF/Native加载/视频/full0。

最终task `d7de687d187b45f399e0951e14f76854`，原件 `artifacts/candidate-purevideo-r1/final-readback/Evidence`：

|负控|PID|exit|失败阶段|timeout/reap/drain|
|---|---:|---:|---|---|
|missing-digest|3560|1|digest-required / FileNotFoundException|false/true/true|
|wrong-sha|12768|1|sha-validation / InvalidDataException|false/true/true|
|wrong-architecture|1220|1|pe-architecture-validation / BadImageFormatException|false/true/true|

各项保存独立stdout、stderr、result.json及process-results.json；stdout为空也是原始输出。错架构仅在沙盒修改candidate副本COFF machine并重算摘要，不执行该DLL。最终harness SHA 39331A61CEB335A654A6E8B84B8DE1EB625B6285B42ACDC79A61E68687D6E32A；MVID 465bdf1d-1362-4426-b41f-7d8447fcc00e。原始JSON有baseline/finalModules/requests。监督运行30秒、kill后reap10秒、drain10秒分别有界；监督exit0表示完成采集，并非独立断言所有合同通过，必须核对child结果。监督尚缺自动判断失败阶段/模块污染、失败时非零总退出码，需审查后补足。

## 独立审查下一步

1. 核对发布排除目标和最终包全部gfx条目、原始负控模块数组及摘要，补全监督自动断言和退出码。
2. 在只读辅助中解析固定二进制本机导入/延迟导入、模块初始化/显式加载调用链；不要用字符串缺失当安全证明。
3. 闭合原版禁令后才移除阶段拒绝并将ExecuteWpf接入；补成功前后candidate实际路径/SHA与请求证据、清理/最终模块验证。在有界监督内运行原纯视频consumer，不能降低任何现有断言。

当前结论：IsolatedPackagePublished / BoundaryCompiled / NegativeControlsExecuted / NativeSHARechecked / NativeLoadIsolationOpen / CandidateVideoNotRun / OverallOpen。用户授权无新次数上限；不是预算或缺授权阻塞。
