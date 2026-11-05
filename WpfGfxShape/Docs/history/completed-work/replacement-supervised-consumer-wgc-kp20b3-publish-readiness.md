# WGC KP/20B3 显式输入绑定与发布准备门

## 最新接续：唯一真实诊断失败并停止

本页发布准备事实保持历史。[真实回读报告](replacement-supervised-consumer-wgc-kp20b3-official-readback.md)：已用另授权沙箱1、精确请求1、真实体1，Exit2/TRX1失败，无超时；候选实际SHA完整20B3匹配，test SHA/MVID/UID已记录。独立红标尺首合格WGC帧为零色/零alpha，提交后窗口/屏幕DC均红，两个HWND销毁检查失败；consumer真正创建提交和两标记not observed。未重试、未发现、未改包/build/publish。WGC累计build3（1败2成）/COM publish3（1败2成）/沙箱2（发现1+真实1）/单独发现1/真实体1失败；KS另列不变。以下“待授权/体0”为执行前历史，不再是当前动作。

## 裁决与范围

**发布准备门通过；可请求一次真实 official-only 诊断授权，尚未执行或验收功能。** 本轮唯一 COM publish 1/1 成功 Exit0；独立 build0、Native publish0、沙箱0、发现0、测试体/consumer/标尺0，源码/项目0改。无终端、脚本、委派、手写摘要、拼包或二进制写入。主代理仅监督。旧 C427 包 `artifacts/consumer-wgc-runtime-fix/` 未覆盖，发布后亲读其 clean 根摘要仍为 C427438AF11B8B4616AC732120B32B94EC848A8AD6B05406381C2C8A781DB267。

本次用户一次 publish 授权覆盖旧文档“本阶段禁止发布”的阶段性限制，不扩大为运行授权。不是产品完成，不把临时 SDK/诊断配置当正式业务能力；默认配置仍不定义 CONSUMER_WGC_DIAGNOSTIC。

## 先读来源与身份

先读 native-identity-audit、revision-validation、章程00及两主题页、当前执行规则/工作项/稳定基线/交接；读 ComAcceptance.csproj、Package.targets、global.json、AcceptancePaths、监督参数和 WGC 官方身份段。历史定向读 KP resize-pixel-revalidation §实际前置及 KS lifecycle-thread-scope 最新身份节、发布参数节，不冒称重审全部历史。

输入为仓库根相对 `WpfGfxShape/artifacts/kp-resize-pixels/com-acceptance-package/Native/wpfgfx_cor3.dll`。发布前亲读 KP 权威 package-files.txt 首尾（418行，末段列 Native、摘要及根官方不同角色），根 aot.sha256 为 **20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D**，列 Native 目录确认 DLL 存在。KP 报告记复用 KN native-x86、生产发布0；KP actual 同摘要；KS 正式初始化历史为同 SHA、X86/PointerSize4、无 CorHeader/I386。历史认证是来源证据，不冒称本次源文件或新 clean 字节已独立重验。没有猜用 KN 当前共享 x64 clean 或生产默认位置。

新根 `artifacts/consumer-wgc-kp20b3-bound` 发布前枚举不存在，故 publish 与 clean 均全新独立，不受旧 PreserveNewest 副本残留影响。

## 唯一正式发布

实际完整调用：

`dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c ConsumerWgcDiagnostic -r win-x86 -f net10.0-windows10.0.18362.0 --self-contained true -p:ComAcceptanceSandbox=true -p:AotRuntimeIdentifier=win-x86 -p:AotDllPath=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/kp-resize-pixels/com-acceptance-package/Native/wpfgfx_cor3.dll -p:PublishDir=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/consumer-wgc-kp20b3-bound/publish/ -p:ComAcceptancePackageDirectory=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/consumer-wgc-kp20b3-bound/com-acceptance-package/`

专用发布日志全部11行已读：1完整命令，3 Exit0，4成功，7–8还原最新，9 ComAcceptance 诊断配置程序集输出，10新 publish 输出；没有截断标记、没有逐文件 Copy 详细日志，也没有 warning 汇总，不虚构数量。仅一次发布，无重试。未传 NativeAotPath、全局 RuntimeFrameworkVersion、NoBuild、NoDependencies 或生产项目。publish 内部正常构建不另记一次独立 build。

输入属性是 csproj 已支持的 AotDllPath。既有 PinConsumerWgcRuntimeFrameworkVersions 在诊断配置下、ProcessFrameworkReferences 前只更新 NETCore/WindowsDesktop/App.WPF 的 RuntimeFrameworkVersion=10.0.13；本次不修改该目标、不全局固定投影运行框架。

## 复制链及正式摘要

1. 日志第1行显式选择上述 KP clean Native 文件。
2. csproj 的 None/Link=Native/wpfgfx_cor3.dll、CopyToPublishDirectory=PreserveNewest 将该输入放入新 publish/Native；发布后列目录确认 publish Native 文件存在。
3. 现有 PackageSelfContainedComAcceptance（AfterTargets=Publish，自包含沙箱条件）清理新 clean 根，从 RuntimePackAsset、ReferenceCopyLocalPaths、入口、Contracts/Native 等显式集合去重、计算相对 PackagePath、检查依赖存在，再 Copy 到 clean。
4. 同目标对 **publish/Native/wpfgfx_cor3.dll** 执行正式 GetFileHash(SHA256)，将结果写 clean 根 aot.sha256。本次亲读结果恰为 **20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D**，不是手写。

这是完整发布参数 + 可读正式目标 + 成功结果 + 新文件/摘要的链路证据，不是日志逐文件 Copy trace。现有工具无非执行二进制 SHA/PE 检查，故没有独立重算源/publish/clean三份或 test DLL SHA/MVID；正式摘要认证 publish Native 的计算结果，不冒称 clean 复制后已独立认证。后续同一次授权任务中的既有 AssemblyInitialize/ValidateIdentity 可实际 File.OpenRead/PEReader 验无 CLR、I386、SHA 匹配包摘要并写 Results/aot-identity.json；无需加载 candidate，不为不可用工具无限阻塞，不绕初始化、不改摘要。

## 依赖、相对清单与官方角色

新 clean 递归枚举上限700未提示截断，入口、测试 DLL、Contracts、Native、官方根、SDK.NET、WinRT.Runtime、MTP/MSTest及资源均存在。正式 manifest623行；亲读首24行、399–428、600–623及 runtimeconfig 全21行、deps 首38与955–990。manifest413是根官方 wpfgfx_cor3.dll，414–415是 SDK.NET/WinRT.Runtime，619–623是 Contracts/Native/摘要/清单，相对路径保留。未逐行人工重审全部623行，也无独立机器化全清单冲突/逐文件hash核验；不将文本搜索误扫 DLL 的字符串当 PE/版本认证。

runtimeconfig includedFrameworks 明确 Microsoft.NETCore.App、Microsoft.WindowsDesktop.App **10.0.13**；deps目标 win-x86，两 runtimepack **10.0.13**、官方 SDK runtimepack **10.0.18362.57**。SDK.NET assembly10.0.18362.38/file10.0.18362.55，WinRT.Runtime assembly2.2.0.0/file2.2.0.48161。根官方 wpfgfx 来自 WindowsDesktop runtime pack，deps file10.0.1326.50504；Native 子目录为 KP candidate，两者不可混同。版本声明与存在不等于加载认证。

## 新测试发布身份与发现边界

通过非执行公开 API 读取新 clean 的 WpfGfxShape.ComAcceptance.dll，确认公开类型 WpfGfxShape.ComAcceptance.D3DImageTests 及方法 WhenOfficialConsumerTwoMarkersAreObservedByGdiAndWindowCapture 实际存在。已核新发布配置/路径、deps/runtimeconfig、方法名；**未取得新 DLL 的 SHA/MVID、metadata 的独立二进制展开或 MTP UID**。源码在 sandbox metadata 固定相对 Native/wpfgfx_cor3.dll，不含绝对 KP 路径。

亲读既有 discovery-tool-output.txt 全7行：旧 C427 包 .NET10.0.13/x86，目标方法独立列出、总数1；历史完整FQN等值filter见 revision-validation。可引用相同 FQN 的已有发现，不称新20B3包 UID 已实测，不追加发现。后续监督器记录实际 test SHA/MVID，真实 TRX/诊断输出核节点和执行计数；不能搬用 KS UID。

## 官方加载身份仍待真实测试

静态复核 WGC identity：NativeLibrary.Load(requested) 后按 handle 基址从 Process.Modules.Single 取 Actual，计算实际 SHA/ProductVersion/runtime/test SHA/MVID。**代码没有 Requested==Actual 断言，也无预置可信官方 SHA 比对**；本阶段不改代码补 assert。未来须回读 identity，规范化 Requested/Actual 并确认均指同任务 clean 根官方 DLL，核实际 SHA/PE/产品版本与同架构受信官方10.0.13资产及 PresentationCore 配套。当前不声称已保证相等或受信版本已认证。

CandidateLoadedByDiagnostic=false 为常量，不是全进程“从未加载”实测。official-only 不调用 candidate 导出；初始化只认证 candidate 文件。若要求全时段未加载负证据，仍须受支持模块/加载事件证据，不拿常量替代。功能、WGC可用性、帧关联、资源清理均未实测。

## 唯一待授权任务（本阶段不执行）

- sourceDirectory：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\consumer-wgc-kp20b3-bound\com-acceptance-package`
- executableRelativePath：`WpfGfxShape.ComAcceptance.exe`
- arguments依次：`--supervise-exit`、`Results/consumer-wgc-kp20b3-x86`、`120`、`FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialConsumerTwoMarkersAreObservedByGdiAndWindowCapture`
- 外超时180秒；outputRelativePath留空回收完整任务；建议 localOutputDirectory=`WpfGfxShape/artifacts/consumer-wgc-kp20b3-bound/official-readback`。
- 请求预算：仅沙箱1/监督运行请求1/该 official 测试体至多1；按既定方法 consumer两色各1与独立标尺两色各1，无额外 native-only、candidate、线程、shader、resize、full、x64或单独发现。build/publish/Native发布/代码包修改/重试均0。初始化/发现零命中/模块身份/清理或内容失败即停；不重试追绿。
- 回读监督、stdout/stderr、TRX/节点、Results/aot-identity.json、WGC evidence.jsonl/帧/清理。新包 UID 只能在真实任务后据原件记录；如无法取得则明确未知。

## 预算与交接

WGC累计 **build3（1败2成）**；COM publish此前2（1败1成）+本次1成功 = **3（1败2成）**；沙箱/发现累计1、真实测试体0；本次除COM publish1外执行增量全0，Native发布累计本链0。KS另列：历史build1/COM publish1、沙箱4、精确运行请求3、发现1、真正体1、A1/B1/native提交2，不清零、不算WGC。

唯一下一动作是请求上述一次真实 official 诊断授权，当前不运行。输入选错门已通过显式绑定与正式摘要恢复；复制后字节PE、实际test身份/UID、官方实际路径与受信版本、功能/生命周期属于待授权任务的实证门。整体替换未完成。
