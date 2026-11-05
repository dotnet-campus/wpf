# r27：candidate-only 公开 WPF 几何/画刷 RTB 有限门通过 / OverallOpen

## 结论与范围
实际重新冻结源码、发布 x64 生产 NativeAOTShared，使用真实 PresentationCore → 认证 candidate → RenderTargetBitmap.Render → CopyPixels。既有 Pcore Render 场景及原白/红两个固定字节断言在独立几何入口通过，同一最终包在第二个独立 Windows sandbox 再次通过。不是仅托管13项，不是仅加载成功，不是桌面或 full 通过。生产修改0；没有终端/脚本、原版 gfx 加载/复制/fallback，没有修改 defaultasync、oracle、expected、阈值、DPI、flags 或生产验收钩子。

先读 next-session-handoff、r26、r22 包隔离方法和 r13 属性传播纠正；未使用已不存在的 r22 产物或旧 SHA 认证。工作开始时已存在 GeometryR27.cs、GeometryR27 项目条件及 SceneSelection 分支；本次读取核准并从冻结快照实际发布，不冒称本次新写这些现存文件。本次实际源码编辑仅 Tests/CurrentSourceAuditR15/{Program.cs,CurrentSourceAuditR15.csproj}：增加已拉回证据的只读 geometry-r27 模式，不参与 Native/consumer/监督器构建。其编辑发生在冻结及两轮业务之后，因此不在本次源冻结清单内；不得用冻结 SHA 认证后修改的审计辅助。

## 原门复用与隔离
原 Tests/WpfGfxShape.ComAcceptance/Pcore/Program.cs:228–271 Render：32×32、96 DPI、Pbgra32；白底、红矩形、半透明蓝矩形、平移后的绿色路径/黑描边和黑线，RTB.Render、CopyPixels、输出原始像素/PNG。GeometryR27.cs 原样复用绘制和两个断言，仅增加证据字段；没有直接调用项目托管 geometry 实现。RTB 消费就是原门语义，不借桌面 HWND 通过。固定预期来自原源码：offset0 为 FF FF FF FF；(3,3) offset396 为 00 00 FF FF。红点远离边缘且不在半透明覆盖区。没有用 candidate 生成结果作为 oracle。

这是新隔离的有限门 geometry-public-r27，不是正式旧 full。透明叠色、路径、变换、线条进入消费场景，但没有额外解析断言，不能宣称其每个像素已获认证，也未声称裁剪功能通过。

复用 CandidatePureVideo 和 CandidateVideoSupervisor，不新增框架。GeometryR27=true 编译时仅接受 candidate geometry-public-r27；original/full/compare 参数不匹配直接64。MediaHelpers 和 PureVideoPublic 在该构建排除。Main 无 WPF 类型，独立 NoInlining ExecuteWpf/RenderGeometryR27 在 digest、PE 架构及认证预载之后调用。固定 self-contained .NETCore/WindowsDesktop 10.0.13，runtimeconfig 已读；runtime gfx 在发布项排除，只保留 Native/wpfgfx_cor3.dll。认证前排除已加载 gfx，认证后 SetDefaultDllDirectories/AddDllDirectory 限定包和系统依赖；验证 basename 同句柄、实际路径/SHA、进程中唯一 gfx。PresentationCore 请求由 resolver 返回认证句柄。

本项目无正式 ComAcceptance ProjectReference，未传 ComAcceptanceRequireReferenceData=false 或其他关闭正式门属性；遵循 r13 不将独立无 oracle 场景的属性传播到正式包。普通 full 缺两份可信 JSON 的门禁仍保留，未执行 original/comparison。

## 冻结及发布
证据根（相对 WpfGfxShape）：artifacts/candidate-geometry-r27/。
CurrentSourceAuditR15 SourceBoundR22=true 发布到 freeze，再在 sandbox 调用 freeze。source-receipt/Evidence/freeze.json 与 source.sha256 给出818文件，UTF-8 无BOM排序清单 SHA256：
E87F368A049522EB5C0AFDD12008FAF80FA8782159B5D404AC6E60090D1CE770。

覆盖 Code 全文件（排除 bin/obj）、Tests 的 cs/csproj/targets/resx（排除 bin/obj）、本地 Directory.Build.props/targets、仓库 global.json/NuGet.config/.editorconfig、Shaders.rc、ShaderEffectsVS.fx。清单逐文件路径/长度/SHA落盘，不冒称818文件全文都已人工阅读。不是完整供应链快照：外部 SDK/NativeAOT/VC/Windows SDK、NuGet 缓存内容及用户配置、系统 DLL、操作系统和环境未冻结。快照后构建生成的 artifacts 不在原清单中。

实际项目绝对路径：
d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\candidate-geometry-r27\freeze\Source\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj。
实际通过专用发布工具执行（路径相对工作区 WpfGfxShape 上一级）：
`dotnet publish WpfGfxShape/artifacts/candidate-geometry-r27/freeze/Source/WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 --self-contained true -p:PublishAot=true -p:NativeLib=Shared -o WpfGfxShape/artifacts/candidate-geometry-r27/native -bl:WpfGfxShape/artifacts/candidate-geometry-r27/native.binlog -flp:logfile=WpfGfxShape/artifacts/candidate-geometry-r27/native.log;verbosity=minimal`
这只是参数记录，不使用 shell 执行。发布 exit0，实际 Generating native code 和快照输出路径已读。生产 csproj net10.0/Nullable enable、诊断ABI和heap默认false，未覆盖。发布有既有 CA1416 警告；工具793499字符输出仅保留最后20000字符，前773499未返回，不声称完整人工审计。native.log/binlog作为持久化构建记录，未解析 binlog。

其余发布项目/参数（均 -c Release -r win-x64 --self-contained true）：
1. 工作树 Tests/CurrentSourceAuditR15：-p:SourceBoundR22=true，输出 freeze。
2. 冻结 Source/WpfGfxShape/Tests/CandidatePureVideo：-p:GeometryR27=true，-p:CandidateNativeDirectory=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/candidate-geometry-r27/native，输出 package。
3. 冻结 Source/WpfGfxShape/Tests/CandidateVideoSupervisor：-p:GeometryR27=true，-p:CandidatePackageDirectory=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/candidate-geometry-r27/package，输出 supervisor。
4. 工作树审计辅助 -p:GeometryReadbackR27=true，输出 readback；首次出现 nullable 警告，修正文件名键的 null guard 后同参数再发布，最终无该警告。没有再次发布 Native 或业务大包。

Native：4895744 bytes，PE Amd64、managed=false；SHA256 8EE1C6895313DB7AB9F95B90D45EF1D0249695229ABE227D1F73C725A8D9FC6D。
Harness CandidatePureVideo.dll：19456 bytes，SHA256 25B95706ABA45117B6E68A217867CF713CB860D013DC3B0836EF7165CC8DB01E；MVID 80b59b9e-e4e4-493a-b2f5-4f6c369bea01。
PresentationCore.dll：8324904 bytes，SHA256 0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31；版本10.0.0.0。两轮原件和 package-identity 对应同一身份。

## 监督与真实执行账
专用 publish6次（freeze1、Native1、consumer1、supervisor1、只读审计2）；sandbox4次（freeze1、业务2、只读审计1）；独立 build0/test0。所有发布日志都在下一发布前亲读；无删除用户文件。业务使用同一个 supervisor 输出，不重新 publish。两次调用 CandidateVideoSupervisor.exe verify-video，由外监督器启动正常父，再依次完成三负控及业务子；原始结果保存在 run1/Evidence、run2/Evidence。

|项|run1|run2|
|---|---|---|
|sandbox task|f8e348c33d1544d18ca8425ec5686620|911a4c1599ff4b69b6080a3737d3c516|
|正常父 PID / OS exit|12212 / 0|4660 / 0|
|业务子 PID / OS exit|7984 / 0|2032 / 0|
|missingdigest / wrongsha / wrongarch PID|4208 / 10428 / 12268|8716 / 10536 / 2836|
|负控 OS exit|各1，正确拒绝|各1，正确拒绝|

父子 timeout=false、reaped/drained=true，业务与负控 killError=null。父原始 stdout 为三负控收据，stderr 空；业务 stdout/stderr 空；负控 stdout 空。三份负控 stderr 分别为 FileNotFoundException（aot.sha256/Main41）、InvalidDataException sha-validation（Main44）、BadImageFormatException pe-architecture-validation（Main55），精确堆栈保留并已读。两轮负控都在 Native 边界前拒绝，requests0、final gfx模块0，无 candidate/preScene/caseId业务边界越过，harness SHA/MVID一致。不能把负控exit1误记成业务失败或抹为exit0。

现有监督上界：负控每个30秒主等+10秒reap+10秒drain；业务120+10+10秒；外父180+10+10秒；sandbox工具300秒。无本次timeout/kill分支触发，不声称硬实时或已验证kill恢复。外监督器本身工具没有单独 OS exit/PID收据，不能补造；supervisor-exit-probe记录的是其子即正常父的真实exit。

actualNative 两轮分别位于 C:\CodingAgentSandbox\Tasks\<task>\Package\Native\wpfgfx_cor3.dll，SHA如上；nativeBasenameHandleMatches=true，finalModules中gfx恰一项。各有36个resolver请求，亲读 PresentationCore Version10.0.0.0、flags2050、selected为该candidate。包gfxFiles仅这一项，runtime-audit实际Native条目 Amd64/managed=false；imports=[]不是没有系统导入的证明。preScene/final模块清单为原始收据，独立只读审计又枚举其gfx项。

## 真实像素复核
run1、run2 均完成 geometry-public-r27-completed、originalAssertionsPassed=true、exitCode0；原始 candidate-raw/pixels.pbgra32 各4096 bytes，SHA256均为：
7F55E138C9605464AE90811BD28B5477871BB3E1AA71F5FD3A1C8B052E0C308B。

只读辅助重新读取拉回的二进制全部字节算SHA/颜色计数，按原固定位置逐字节比较 offset0=FFFFFFFF、offset396=0000FFFF，而非相信业务json中的布尔值。结果 readback-result/Evidence/geometry-r27.json passed=true/runs2 已回读；两份整文件相同SHA是重复性证据，不是把首轮结果设为oracle。颜色计数白651、红78、绿色23等仅诊断，不追加为验收阈值。PNG也保留，但通过依据是原始Pbgra32的固定断言而非图片目测。未声称对每个边缘/所有4096字节存在解析golden。

原件已读：两父真实exit探针、业务真实exit收据、两父stdout、所有业务/负控/父stdout与stderr内容（只读审计也完整收录）、package identity、result关键身份/场景/像素/cleanup字段、actualNative/PresentationCore请求、NativePE条目、只读像素审计。大型runtime/初始化审计JSON没有全文逐行人工读完，不将其静态扫描当完整动态供应链证明。没有已执行但未回读的业务实验。

## 保留的 Open 与恢复
桌面 nativeControl 白像素不重跑；桌面呈现/resize不通过。full两份可信JSON未提供，原参考门不关闭；完整替换、所有几何/画刷像素、x86本门、clip、文本glyph、负Flush通用有界恢复等继续Open。本次不盲修生产，不弱化oracle。下一步如扩展应从现有固定解析公开场景选择新的独立有限门，不将本门冒充full，也不借r22旧SHA或重复桌面白像素。
