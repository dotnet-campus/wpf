# r22：源快照绑定媒体候选独立原件复核

## 当前结论
双架构公开失败恢复、正常视频 seek/连续红蓝播放/两次 EOS/立即 Close/Open 的有限门通过。OverallOpen。先读 latest handoff/r21，再亲读 r22 原件；不是仅复述前任务摘要。没有重发 Native。生产修改0；本轮只扩展既有 CurrentSourceAuditR15 只读辅助的帧审计模式并编辑文档，没有新项目。

证据根（相对 WpfGfxShape）：`artifacts/candidate-media-source-bound-r22/`。四次业务证据均在 `{recovery,video}-run-{x64,x86}/Evidence/`。本文“已核”指列出的收据及关键字段定点读取，不声称全部五万行 PE 审计、两万行 runtime 审计或814份源文件全文读完，不声称本轮重新 hash Native。

## 源冻结与发布输入
`source-receipt/Evidence/freeze.json` 原件给出814文件，manifest SHA256 `8B109FD9BBCA919FCD15B49F27DC7E87671F4712B249158DB31A7B72F06539A6`，配套 `source.sha256`。冻结由既有 CurrentSourceAuditR15 的 freeze 模式逐文件 SHA/长度及排序后的 UTF-8 无BOM清单产生；本轮已读其实现和项目覆盖规则。

覆盖：Code 下全部文件（排除 bin/obj）、Tests 下 cs/csproj/targets/resx（排除 bin/obj）、WpfGfxShape 本地 Directory.Build.props/targets、仓库 global.json/NuGet.config/.editorconfig、Shaders.rc 和 ShaderEffectsVS.fx。不是整个仓库冻结；不包含完整外部 SDK、NuGet缓存内容、Windows系统MF、VC/Windows SDK工具链、用户级NuGet配置或主机环境。冻结后的 Source 中另有构建生成 artifacts，不把这些后生成文件计入原814文件。

实际生产发布项目为：
`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\candidate-media-source-bound-r22\freeze\Source\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj`。
不是工作树生产项目直接发布后才补挂一个快照。`native-x64.log` 开头及1600–1608、`native-x86.log` 开头及1590–1609明确从此项目 Restore/Publish、生成 native，再复制该快照构建输出到各自 native-x64/native-x86。两份同名 `.binlog` 均已由目录工具确认存在；本轮未解析二进制 binlog，不冒称全文检查其参数。

已核有效配置/输出路径：Release/net10.0/win-x64 与 Release/net10.0/win-x86，项目 PublishAot=true、NativeLib=Shared、SelfContained=true，诊断ABI/heap默认false。快照 csproj 和本地props已亲读。NativeAOT rsp 显示快照 obj 输入及 NuGet Silk.NET2.23.0、NativeAOT runtime10.0.13；x86日志明确 ilcompiler10.0.13 和 VS18 Professional/MSVC14.51.36231 Hostx64/x86 link。global.json 请求SDK10.0.107/latestFeature，不等同于已独立认证实际SDK二进制。NuGet日志还显示用户/VS配置和多个外部feed。

**发布收据限制**：publish-01.log保存冻结辅助的完整11行专用工具日志（SourceBoundR22=true）；publish-02-return.txt/03-return.txt是前任务摘录，不是完整原始工具输出。02记823613字符截断/丢前803613，03记822622字符截断/丢前802622；02保留43–47末尾路径，03明确仅曾读1–8且未逐字保存20000字符。不可从“执行成功”补造未返回的命令文字、开关或诊断属性。native日志存在且已读发布成功段，但本轮没有找到独立逐字保存的两次完整 dotnet publish 命令行；上述配置来自实际路径、项目与日志，不冒充命令行原文。其他八份包/监督者 log/binlog也存在。r22加强了快照作为实际发布输入的追溯，不称可重现构建或完整供应链认证，也不反向认证r21的六次旧发布。SHA与r21不同不自动证明生产源变化。

## Native 与四包身份
|项|x64|x86|
|---|---|---|
|Native SHA256|A9B9F69FC56E48072B9FA05B44D526FAC03287A4C4C61A766C464AEBEC1F639E|93405EB4A14F50CB556972F19D749B2580302A0F70BBE1342763215C83B4F9F4|
|bytes / PE|4895744 / Amd64、managed=false|4333056 / I386、managed=false|

每个包的 Package/aot.sha256、package-identity.json、业务 candidate/actualNative 和 runtime-audit Native条目已亲核对应。恢复与正常包共享各自架构 Native，不共享 harness。

|业务包|sandbox task（业务路径原件）|CandidatePureVideo.dll SHA256|MVID|bytes|
|---|---|---|---|---|
|recovery-x64|0e07c8edbfbe423297fcd1c46bc73808|CC3A7391A30872EE8B9C72CBB7FBD41B848F7FFD2E5A8330776304D238ACDDE9|1aa70bb5-ed12-4663-beac-91faea2823c2|33280|
|recovery-x86|2c6edf6e76f9402ca81a53ad8ea8222e|1090330094EC02B284FA5FBD9B3CA21D3801186814828AE1E20C82D7F6B56ED2|457e00b6-ba57-4a43-a0fb-d7945a650695|34304|
|video-x64|045abfd96b4946bb9d09e2b0700ad52c|635A513DC6775935CDF32BFF8C4A083704C1ECB04D7BE0C884A8A5A534EB6C03|916e5d6f-d298-4c23-97a2-69fddb5b24e1|57856|
|video-x86|7efc2c0160714330992a4b552edd3ebc|A45F10AB1E511FE3A021BF0192EDED55B07C55C162FF1E349DBE22BFC48A70DF|78f68dc6-bf56-4c89-afb6-472653f81849|58368|

actualNative均为对应 `C:\CodingAgentSandbox\Tasks\<task>\Package\Native\wpfgfx_cor3.dll`，nativeBasenameHandleMatches=true。PresentationCore Version10.0.0.0 请求 wpfgfx flags2050、selected指向各自候选。正常视频 PresentationCore SHA：x64 `0E605CCC78D7929EB8DA4473CDCB6D7EF79DB07B3F69E780745AE1F2AA5A6E31`；x86 `D2A9C8B691A6AEB4E7417BB700212BD58FDBB9F8ACEB41A24EAC0327F98C170D`。package identity gfxFiles各只有候选；正常 allWpfgfxModules也各只有候选。runtime-audit imports=[]不是无OS导入证明，containsLoadLibrary=true不是原版gfx加载证明。

## 父子监督、三负控、输出
|业务|父PID/exit|业务子PID/exit|三负控PID（digest/SHA/PE）|
|---|---|---|---|
|recovery-x64|13296/0|3588/0|10328 / 5648 / 10000|
|recovery-x86|4476/0|9012/0|12976 / 2944 / 12604|
|video-x64|5940/0|2020/0|4540 / 13080 / 4248|
|video-x86|8828/0|12808/0|9808 / 4580 / 9368|

四份 supervisor-exit-probe.json 与 candidate-video-process.json均全文读取：timeout=false/reaped=true/drained=true/passed=true。子killError=null/failures=[]；父收据没有killError，不补造。三负控各自process-results.json全文读取，12个均预期exit1、passed=true/failures=[]、timeout=false/reaped/drained=true、killError=null。

12份负控业务JSON关键段也已亲读：missing-digest在digest-required/FileNotFoundException拒绝，wrong-sha在sha-validation/InvalidDataException拒绝，wrong-architecture在pe-architecture-validation/BadImageFormatException拒绝；requests均空（正常视频负控的requests在JSON末尾，已另行读尾确认）。不要混同“三种公开媒体失败”。

四份父stdout全文读完，均为三负控JSON；四份父stderr和八份业务stdout/stderr均亲读为空。normal-process-results不是正常业务子收据；业务使用candidate-video-process。没有触发超时，不能以这些正常退出收据宣称异常kill/tree强取消路径实测通过。

## 失败恢复有限门
双架构result原件已亲读三失败、恢复元数据、立即重开段、计数和cleanup：缺文件0x80070002、8字节无效AVI0xC00D3E84、localhost:1失败URL0xC00D0035。每次MediaFailed内Close返回，各自随后恢复HasVideo=true/HasAudio=false/2x1/4秒并playing-Close-return。最后三轮immediate-open-close-open、Opened和repeated-Close-return，最终Close返回。

publicCounts两侧opened6/failed3/ended0；最后Opened elapsed x64 9371ms、x86 9344ms；cleanupDiagnostics=[]，stage=purevideo-completed、exitCode0。有效素材SHA `F277087DA15898B6C96720AAEF17062A6E9D221856055CD2B51014FED323E7E4`/864字节；无效SHA `8A851FF82EE7048AD09EC3847F1DDF44944104D2CBD17EF4E3DB22C6785A0D45`/8字节。原件明确comFinalReferences=Unknown，cleanup空不等于COMrefs0。

## 正常视频、seek、连续播放与立即重开
双架构原件显示2x1无音频4秒、CanPause=true。暂停seek目标0.5/2.5，组合同步后播放到目标+0.3，暂停并等待Rendering后单次RTB；actualPosition均0.8/2.8，样本红0000FFFF/蓝FF0000FF。seek-return与paused-seek-rendered两段已亲读，不把seek返回耗时字段误记成异步完成时间。首轮EOS：x64 3945.4095ms；x86首轮EOS计数通过reopenedState ended2及事件上下文核，未在本文补造首轮时间。

额外连续段：seek回零/组合同步后仅一次Play，中途不seek/Pause，0.8/2.8两次采样paused=false。x64 before/after均0.8和2.8，capture5201.7912/7222.3289ms；x86同样0.8和2.8，capture4872.6943/6869.8327ms。第二EOS x64 8276.0733ms、x86 7965.1078ms。两侧三轮同player立即reopen-Close-Open后收到Opened；reopenedState opened4/ended2、cycle2；最终Close HRESULT0，Window IsDisposed=true，cleanupDiagnostics=[]。

### 原始帧独立字节复核
仅为此必要验证扩展既有 `Tests/CurrentSourceAuditR15`（Program.cs及csproj）`frames-r22`模式，输入限定两侧candidate-raw/*.pbgra32，未加载Native/WPF，未改业务oracle/expected/阈值。专用publish一次成功，完整11行工具日志已读；专用sandbox一次，返回passed=true/count8。结果 `frame-audit-readback/Evidence/frames-r22.json` 87行已全文回读。不是截图目测或仅相信result.sample。

八份帧均32x16对应2048字节/512像素、单一颜色。两架构暂停/连续的0.5帧全部字节组0000FFFF，SHA `1D6F3E7946101E19E458A63004805269FA94526B51BB8DA2FACDDF08F32A1579`；2.5帧全部FF0000FF，SHA `243565B2512B60233EF3EC38459C55D387445F31A42D9D9FE98BAF793A1EED00`。辅助没有复制或加载原版gfx，不改变已有快照/业务包身份。辅助发布/运行摘录已存 `frame-audit-publish-receipt.txt`，工具未返回task/PID/退出码字段，不捏造父子监督细节。

## 分账与完成边界
前任务账（用户交接）：publish11成功，sandbox5（冻结清单1、恢复双架构2、正常视频双架构2），生产修改0，仅CurrentSourceAuditR15辅助修改。原件与本轮独立回读分开；不能把本轮辅助1次加进前任务11，不能称11次原始工具输出全部保留全文。

本轮账：独立build0、run_tests0、Native publish0、辅助publish1成功、sandbox1；仅既有审计辅助两文件及文档/摘录收据修改。新执行结果已全文读回并落文，无新业务重跑，无已新执行未读结果。未执行终端/脚本、未原版gfx加载/复制fallback；未分派执行子任务。

r22可关闭的是“本次快照发布候选的四业务原件独立回读与正常帧字节确认”，不关闭以下门：
- full仍缺两份可信原始reference JSON，Open。
- 桌面原nativeControl失败，Open，停止捕获派生；本次RTB红蓝不是桌面扫描输出证明。
- 内部异常Flush失败、晚drain/永不回调与r21条件性清理重试责任缺口仍Open；公开三失败不是内部故障注入。
- 源端pending/URL同步创建硬取消、任意竞态、最终COMrefs0未验证。
- 未认证完整供应链或可重现；不把r20托管测试身份、r21旧Native或本轮只读辅助身份混绑到r22。

下一步按真实未关门选独立限定任务；不再派发r22四业务“未运行/未回读”，不为比较SHA重发所有Native。若要求完整发布命令逐字认证，需要读取/解析既存binlog或补齐原始工具收据，不能由本文反推补写。
