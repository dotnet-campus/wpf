# 删除原版媒体委托

## 删除前记录 / before-after 与回撤边界

用户已明确撤销“合法第二 wpfgfx 提供生产媒体”的全部许可及排程效力，无需再次架构许可。允许 Windows 公开媒体组件，禁止生产保留、加载或改名复制原版 wpfgfx 作为媒体后端。

Before：MediaExports → SystemMediaSource.Create → SystemMediaModule.Acquire → PresentationCore 同目录原版 wpfgfx → MILCreateFactory / MILCreateEventProxy / MILFactoryCreateMediaPlayer；包装对象继续转发原版 player/provider。

After（本次已实施）：删除原模块解析、加载、export 解析以及整个原版 player 包装委托。有效工厂请求返回 E_NOTIMPL (0x80004001)，输出为 null，不创建资源、不 AddRef 事件代理。普通 MediaPlayer 暂不可用；不是完整播放器、音频输出或视频解码实现。

回撤边界：不得恢复官方 fallback，不得移除 self-check 后递归，不得复制改名欺骗依赖。后续只能接入独立 Windows 公开媒体后端。测试专属 official oracle 保留，不得进入生产工厂。

## 当前验收

生产创建委托删除及托管调用实际导出入口的4例负向契约验证已完成；独立媒体创建、事件时钟、解码、视频 source、音频输出、取消及释放验收全部 Open / NotImplemented。历史委托模式绿数仅历史，不迁移为独立能力证据。完整替换 Open。

## 本轮预算

normalbuild ≤3（绝对 Tests/COM 项目 Release）；定向 tests ≤2（隐式 Debug build 单列）；Native publish 0，COM publish 0，Windows 沙箱 0。已执行 normalbuild 1/3：run_build targetPath=d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.Tests\WpfGfxShape.Tests.csproj，configuration=Release；exit 0，3716 warnings，0 errors，30.28s。日志全部可返回62行已回读，原stdout 3,114,805字符，前3,094,805字符被工具截断，不声称完整原stdout已保存。定向 tests 1/2：run_tests 同一绝对 Tests 项目，filter=FullyQualifiedName~MediaCreationUnavailableTests；exit 0，4 passed / 0 failed / 0 skipped，18ms。隐式 Debug build 1 次，未计为 normal Release build；可返回62行已全部回读，stdout 1,557,811字符、前1,537,811字符被截断；存在 CA1416/MSTEST0044 等 warning，工具尾无测试编译 warning 总数，不伪造0。验证是托管测试中以反射取得实际 UnmanagedCallersOnly 工厂地址再通过 unmanaged stdcall 调用，不是发布 PE/真实 WPF 播放器或跨架构运行认证。禁止终端、脚本、委派、新工程及依赖升级。

normalbuild 2/3：run_build targetPath=d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.ComAcceptance\WpfGfxShape.ComAcceptance.csproj，configuration=Release；exit0，146warnings/0errors，7.27s。返回日志64行，头1–8及尾56–64已读，原stdout114534字符、前94534被截断。新增COM工厂负向测试已编译，因Nativepublish0/沙箱0，本次不运行旧AOT冒充新源码验证。

## 修改文件与生产不可达边界

代码（均未改自动生成代码、项目、SDK/TFM/依赖、InternalsVisibleTo 或可见性）：
- Code/WpfGfxShape/Core/Av/SystemMediaModule.cs：删除81行模块路径/实体比较/NativeLibrary.Load/进程驻留缓存和PInvoke，现仅2行退役注释，无类型、无可调用fallback。
- Code/WpfGfxShape/Core/Av/SystemMediaSource.cs：删除原273行player/provider身份、原factory/export解析、COM转发、事件callback与原生owner；现不可实例化内部绑定占位，Create恒E_NOTIMPL/null，Resolve恒null。Subscribe/Unsubscribe无可创建实例，明确抛NotSupportedException，非可用后端。
- Code/WpfGfxShape/Abi/MediaExports.cs：先检查output并清零，再检查factory；有效请求走上述明确未实现契约。HRESULT E_POINTER=80004003、E_INVALIDARG=80070057、E_NOTIMPL=80004001；选择既有Abi.NativeAotAbiProbe常量及retail未实现约定，不再误报缺DLL。
- Tests/WpfGfxShape.Tests/MediaCreationUnavailableTests.cs：4例实际生产UnmanagedCallersOnly入口，canOpen0/1均明确失败且清输出；opaque非空factory/client=1不被解引用，故没有AddRef/回调/分配需要清理；另覆盖null factory/output。无mock生产。
- Tests/WpfGfxShape.ComAcceptance/MediaExportsTests.cs：新增实际发布DLL工厂负向2数据行，用真实factory并finally Release；编译通过，未运行。

已读SystemVideoSample全120行、GeneratedMediaPlayerResource全166行；后者手写绑定保持不改，前者是借入provider采样lease而非模块加载器。现工厂不产生任何media，不能到原版provider；它们不构成工厂fallback。MILMedia*仍按既有ABI对调用方提供的合法COM对象调用槽，不主动创建/解析/加载原版。测试代码可独立提供official对象作ABI oracle，这不是生产后端。

删除后在Code目录搜索 SystemMediaModule、NativeLibrary.GetExport匹配MILCreateFactory/MILCreateEventProxy/MILFactoryCreateMediaPlayer、PresentationCore.dll、wpfgfx_cor3.dll：0命中（AssemblyName wpfgfx_cor3仍正确保留，不是依赖）。结合完整删除的原加载器与factory实现，生产原版媒体创建链源码不可达。未声称对最终PE依赖表或任何已发布旧二进制做了本轮认证。

## 测试边界与仍未完成

测试专属官方oracle保持：MediaExportsTests.SystemMedia默认candidate=false，NativeLibrary.Load(AcceptancePaths.OriginalDll)独立创建官方对象，及SystemMediaSampleTests的official分支。它们仅在Tests，生产Code不存在此路径；未删除合理独立oracle。

正向独立媒体验收不删、不skip、不改成失败即完成。现有candidate=true的真实创建/采样/事件/持续视频验收仍要求真实媒体成功，按新源码将因E_NOTIMPL而未满足，全部明确Open / NotImplemented；没有运行它们，也不声称旧full仍通过。临时负向测试只认证删除安全契约，不能代替这些正向标准。此次没有修改全部旧正向测试的断言，避免把产品缺口永久降成负向成功。

最终执行总账：normalbuild2/3，定向test1/2，implicitDebugbuild1，Nativepublish0/COMpublish0/sandbox0/full0。无新Native SHA、实际加载模块身份、x86/x64 PE或真实MediaPlayer错误事件认证；无新音频输出。普通有效播放在创建阶段同步HRESULT失败，不返回S_OK假成功，不发虚假Opened、不输出空帧绿或静态图。新后端未在本轮实现；删除优先闭环与文档同步交付后将独立后端作为独立阶段，非再次架构待裁决。

## 文档同步与下一实际实现

已更新并逐份回读当前顶部：00-migration-charter、charter-implementation-contract、charter-evidence-and-continuity、完整validation-plan、next-session-handoff、current-work-item、current-stable-baseline、current-execution-rules、final-acceptance-remaining-matrix、replacement-issues-and-plan-status、remaining-gap-closure-plan、e2e-executable-contracts；核心报告dependency-deployment-audit、PresentationCore-media-E2E08-finite-closeout、real-media-resource-contract-review、continuous-media-independent-oracle-review。均保留历史原文，顶注明许可及排程被用户撤销。Docs文本搜索核对相关“合法第二wpfgfx/不能自行改成完全独立/是否撤销”仍留历史但不再授权；未文本读取binary、未泛替换所有历史报告。

下一实际工作：先核真实Windows SDK或现有CsWin32 metadata中MFStartup/MFShutdown、MFCreateSample/MFCreateMemoryBuffer与IMFSourceReader签名/owner契约；现有生产项目新增internal资源owner，成功与失败路径成对关闭/Release，现有ComAcceptance准备真实sample/buffer验证。后续接真实source reader、时钟事件与合法视频bitmap-source适配，再实现音频renderer/同步和取消释放；公开系统组件可用但不得回原版。没有完成上述功能前保持工厂明确失败，不把资源骨架切成默认可用。下一阶段运行须另有Windows沙箱/Native预算，当前0预算不外推。

所有旧媒体绿仅委托历史、旧full失败独立保留，MemoryDescriptiveOnly/memoryGreen=null，完整替换Open。
