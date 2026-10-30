# 持续媒体独立内容 oracle 只读审计：Completed / Closeout Completed

> 当前边界：用户已撤销生产原版wpfgfx媒体委托，旧模式证据不能认证独立媒体。测试official oracle仍独立保留；生产工厂 E_NOTIMPL/null，独立媒体 Open / NotImplemented，完整替换 Open。见[删除报告](replacement-remove-original-media-delegation.md)。

## 裁决

`NativeRun.cs:106–121` 新增 VerifyContinuousMedia 将 WriteMediaAsset 中的输入 RGB 整数直接用于每个 RTB 输出像素的 exact expected，**跨层合同不成立（没有已建立的保证）**。BI_RGB 是输入未压缩格式，不是 MediaPlayer → 系统媒体服务 → VideoDrawing → RenderTargetBitmap 的 bit-exact 承诺。应修新增入口的独立 oracle，不能将 FFEB1011 固化为 expected，不能添加按本次结果拟合的 tolerance，也不能放宽 official/candidate 的 threshold=0 全字节比较。

这是确定的测试合同缺陷，不是确定的颜色差异根因：没有证明本次差异一定是正确转换，也没有证明官方颜色处理错误；实际协商的媒体子类型、矩阵、范围、转换节点未记录，根因仍 Unknown。不得由首像素反推 BT.601/709 或断言某公式必然命中。

本阶段只读源码、既有报告/业务 JSON、微软公开资料并更新文档；源码/项目/测试新增修改0，normalbuild/publish/Native/沙箱/测试/发现/终端/脚本/再委派全部0。旧 r1 正式 Failed 原样保留，当前15A545媒体 NotCovered，完整替换 Open。

## 亲读及证据边界

已读最新 continuous-media-default-x86-r1 的裁决、实施、official四帧/事件、身份、最终账及停止合同；real-media-resource-contract-review 的真实链、样本、可执行门与预算；current-execution-rules 全27行及当前恢复/工作项/plan状态顶部。源码亲读 NativeRun 新入口及比较 helper、MediaScenes 的 RenderMedia/WaitMedia/CopyMedia/CompareMedia/完整 WriteMediaAsset，以及 SystemMediaModule.Acquire、SystemVideoSample.Acquire。既有 WPF 颜色参考亲读 S-20261010-010-TARGET-COMPATIBLE-INTERMEDIATE，并核官方 swrast.cpp:1210–1255 的 sRGB 源格式选择。

原件亲读本 task 的 original/result.json 身份头、404–507 的事件/四帧/actualNativeModules/终态；已列原件目录确认2 AVI和4 pbgra32保留。业务根为 `artifacts/continuous-media-default-x86-r1/readback/Results/pcore/media/default/b9641ead43744cceaa8d1d08675c2384/Results/original/`。

**限制：现有只读工具不提供二进制十六进制/结构解析；本阶段没有将 AVI 当文本读，没有执行解析程序，也没有亲自逐字节解包既有 AVI。以下 AVI 头和 payload 是完整生成器的逐字段静态核算，不冒称原件二进制实测；既有输入 hash Unknown 仍保留。** result.json 的 sample/hash 是角色既有记录，非本次重新计算或全图扫描。此边界不影响“公开 API 未提供跨层 exact 保证”的裁决，但不能据此认证原件没有损坏/替换。

## 输入容器与字节合同（由实际生成代码核算）

- RIFF form type `AVI `；`LIST hdrl` 含 avih 与两条 strl，`LIST movi` 交错视频 `00db` 与音频 `01wb`。代码不写 idx1。容器本身不等于压缩编码。
- avih：100000微秒/帧，40帧，两流，宽2高1；视频 strh 类型 `vids`、handler `DIB `，scale1/rate10、length40、suggested buffer8。4秒、10fps。
- 视频 strf 是40字节 BITMAPINFOHEADER：biSize40、biWidth2、biHeight+1、biPlanes1、biBitCount24、**biCompression0=BI_RGB**、biSizeImage8，剩余16字节为0。无 BITMAPV4/V5 色域/profile，未显式描述完整 primaries/transfer/matrix/nominal-range。
- 每帧两个相同 BGR24 像素6字节，后跟2个0 padding；最小行stride `((2*24+31)&~31)>>3=8`，1行共8字节。正高度为 bottom-up，但仅一行且同色，不能用于验证翻转/通道以外的复杂空间映射。
- asset0 帧0–19（[0,2)秒）红：`00 00 FF 00 00 FF 00 00`；帧20–39（[2,4)秒）蓝：`FF 00 00 FF 00 00 00 00`。
- asset1 前20帧绿：`00 FF 00 00 FF 00 00 00`；后20帧黄：`00 FF FF 00 FF FF 00 00`。源整数的高位FF没有作为alpha写入视频，不能把padding误当alpha。
- 音频 strf 为 PCM/tag1、单声道、8000Hz、16000bytes/s、blockAlign2、16bit；每个01wb为800个short/1600bytes，440Hz正弦。音轨存在不证明音频播放内容或设备输出。
- CopyMedia 明确为32×16、96dpi、Pbgra32，输出stride128、2048bytes/帧，四帧8192bytes。2×1 → 32×16 是绘制缩放，不是输入行padding复制。result通用input的32×32不能作为此媒体尺寸。

## 分层保证与未保证项

1. **压缩/存储层**：BI_RGB与上述00db payload支持“源未经过有损视频压缩”；不能拿 JPEG/H.264 损耗解释这个输入。也不能由无压缩推出播放链不协商其它surface格式或不进行颜色处理。
2. **媒体层**：MediaPlayer公开合同提供打开、播放、位置、事件和视频尺寸；没有把源BGR24字节作为公开输出接口，也没有保证每个源通道在最终RTB保持0/255。既有源码/报告显示官方通过系统WindowsMediaPlayer服务；不能把 Media Foundation 的说明当成本次具体采用 MF/特定MFT 的实证。
3. **颜色层**：微软 Extended Color Information 将 primaries、transfer、matrix、chroma sampling/siting、nominal range 分开说明，允许实际转换近似及合并步骤。RGB/YUV往返、范围量化、矩阵、色域不是同一件事。这里只说明不应推定 identity，不说明这些操作在本次都发生过。
4. **Pbgra32层**：微软定义为8bit/channel sRGB、BGRA且RGB预乘alpha。A=255时预乘本身不应令纯红变成EB/10/11；不能用“预乘”笼统解释本次差异。目标格式定义不提供上游RGB identity保证；源没有alpha，正确全覆盖不透明视频才支持A=255的局部断言。
5. **绘制/缩放层**：VideoDrawing.Rect指定视频绘制矩形；RTB光栅化visual，CopyPixels复制目标像素而非原AVI。均色输入的内部区域应保持均色语义，但公开文档没有给出所有边缘/所有renderer的逐像素uniform值保证。既有WPF参考还说明sRGB与scRGB采样、目标兼容intermediate和rounding必须按层区分，不能用线性float公式替代8bit采样合同；它不证明本次媒体颜色转换实现。

## 新独立 oracle 规格：source语义 + 同次official exact，预注册而非拟合

只作用于新增无参default入口，保留旧DataRow三模式及其默认helper路径。继续 official exit成功与独立负控全部通过后才启动candidate，candidate通过后才独立comparator；缺证或失败即停，不转skip/Green。

### A. 固定输入与证据绑定

保留两个素材、生成器、0.5/2.5采样、原15秒等待、32×16输出及原请求不变。新增入口的父验证绑定本次唯一root/role的result、asset/request/file四元组，恰好四项且无重复，status/stage/exit成功，检查文件长度2048与记录hash对应、尺寸来自媒体字段/CopyMedia而非通用input。身份来自actualLoaded而非requested/frozenCandidate。未来执行应保存并核输入AVI身份；不得据本次原件缺hash伪造历史hash。

### B. 预注册四个平台颜色语义（不是四个空间色块）

按生成器的先后顺序固定，不从official读回生成谓词：

|asset/request|源语义|每个受检像素的关系|
|---|---|---|
|0/0.5|红平台|R>G 且 R>B|
|0/2.5|蓝平台|B>R 且 B>G|
|1/0.5|绿平台|G>R 且 G>B|
|1/2.5|黄平台|R>B 且 G>B；不要求R=G或R/G谁较大|

全部要求A=255、RGB至少一通道非零，使用严格关系而非距FFFF0000的容差；不设EB附近范围、不引入饱和度/亮度拟合阈值。空间预注册为完整32×16矩形输出，长度覆盖全图；颜色/不透明谓词至少覆盖固定内区 `x=8..23,y=4..11` 的每一个像素（左右/上下均有分布），不是只取首像素或用平均值掩盖局部空洞。内区在运行前由几何边界确定，不由失败位置选取；全图空/透明/同色旧帧不得通过。全图所有字节仍交给exact comparator，**内区以外未独立保证覆盖正确**，不声称该素材验证复杂空间结构。

适用性：这些谓词是普通颜色保持播放下从极端源色得到的、与本次具体数值无关的弱内容负控；标准单调通道曲线和共同正比例/偏移通常保持这种次序，但任意色域变换/效果并不数学保证保持所有通道次序。微软没有承诺对所有媒体pipeline都保持它们。故这是本素材/同版官方有效控制的**适用条件**，不是新增跨平台通用色彩标准。official若违反，应停为control/oracle适用性未建立，保存证据再审，不自动指控候选，也不修改谓词追绿。

不采用全图逐像素必须相等于一个均色值的附加exact oracle：边缘覆盖/过滤未有独立保证，且不需要该假设才能否决空帧、错误顺序和整张旧平台。可报告全图关系/alpha统计，但不能把未验证的边缘exact合同伪装为微软保证。

### C. 时间、事件及陈旧内容负控

- 每asset的实际Position有限且在既有 `[requested,requested+1)` 窗口：0.5–1.5、2.5–3.5，均远离2秒切换；后值大于前值。不得要求恰等0.5/2.5或本次四个观测值。父记录Position在Pause后采集，与WaitMedia中CopyMedia的检查不是原子时间戳；若父记录越界停为控制失败，不延长窗口。
- 每asset两帧全字节不同；四个平台关系不同，四帧可再要求两两不同。结合颜色顺序拒绝“Position推进但一直显示旧红/绿”、全黑、透明、文件错序；仅hash不同不代表正确，单独Position也不保证surface的精确PTS。
- 要求主player Opened2/Ended2、两段Open→两次平台采样→Ended，非预期MediaFailed0；HasVideo/HasAudio真且NaturalVideoWidth2/Height1。沿用RenderMedia的实时等待与控制流程，父检查事件/帧记录齐备。原JSON事件与帧分表，不虚称已有跨表精确时间戳。
- 保留Pause/VideoDrawing换引用后的全帧SequenceEqual检查、parallel-close-first/second各1、missing-asset的expected-failed-close1与正常角色收尾。预期失败不以精确环境HRESULT作内容标准；Ended证明播放到结尾事件，不证明所有帧呈现、设备音频、线程全退出。
- 本负控能排除常见空/同色陈旧样本，不能排除伪造四张正确平台的恶意实现或精确到每个PTS的旧帧；源每个平台20张同色，本质上无帧编号信息。不得改素材增加编号来偷换本门输入。

### D. 独立比较边界

两角色各自满足A/B/C后，继续原CompareMedia：四文件各2048bytes，official与candidate逐字节SequenceEqual，threshold0，共8192bytes；无颜色归一化、mask、内区裁剪、舍入或tolerance。official在同次独立进程提供具体系统媒体输出基准；source序列语义提供反空/反旧/反错序负控，两者不能相互替代。若两边都“合法但不同”仍差分失败并停查，不能用语义相同豁免。共享系统decoder的共同颜色缺陷不能由差分排除，这是边界，不是宣称媒体颜色学完全正确。

## task 9add3442fe144cb6aa38c0c68398b02d 已取得哪些认证

original角色实际X86/.NET10.0.13，actualNativeModules核官方SHA5EBD18E0…；stage completed、OriginalOnly、exit0。四帧记录请求/实际为0.5/0.658666、2.5/2.535233、0.5/0.5307796、2.5/2.5294644，均在原窗口。首2像素sample依次1110EBFF、EB1011FF、11EA11FF、10EBEAFF，alphaFF且符合上述源顺序关系；四hash各异。这是**已有sample级语义支持**，不是本阶段全图/固定内区新oracle已运行通过。

原件事件Opened2、Ended2（均4秒）、预期Failed-close1、两个parallel-close各1；无非预期failed记录。结合成功走完RenderMedia，可认证官方真实系统媒体打开/seek与有内容且变化的RTB采样、换引用保留帧、自然结束事件和有限Close/回调路径，不因父随后断言失败全部否认。反之不能由角色自报Green认证完整独立颜色oracle、8192bytes差分、候选消费、所有callback静默/worker退出、COM ref0或长期收敛。mediaClosed仍HasVideo/HasAudio真；Volume0/HasAudio仅说明识别音轨和静音设置，**不认证实际音频内容/设备输出**。

r1正式体仍Failed；official1 exit0、candidate0/comparator0；当前15A545没有新媒体通过证据。首帧FFFF0000≠FFEB1011保留原错，不追认历史门Passed。

## 最小下一实施门（仅建议，本阶段不执行）

另授权只修 NativeRun.cs 新入口的 VerifyContinuousMedia 独立oracle及其必要父证据检查；不改生产、MediaScenes/WaitMedia/素材/输入/旧matrix/CompareMedia阈值、工程/Host/SDK/依赖。不新增运行探针，不自动x64/full。冻结本报告语义谓词与证据范围后实施，若当前JSON不能支撑更强原子时序，不扩大本门改底层采样，明确保留限制。

x86 Release，复用现15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620，Native0；normalbuild≤1、COMpublish≤1、沙箱≤1、正式无参体≤1；official→candidate→comparator各≤1、任何前置失败即停。正常publish若隐含编译必须另计且先说明，不能称no-build；不为补日志/探测追加构建。沿用监督120秒/沙箱180秒与原条件15秒，不按结果延长。不执行第二轮追绿、不更换codec/素材/expected、不扩大阈值。

这是纠正“输入存储精度自动穿透系统媒体链”这个未保证契约，而不是掩盖产品问题：仍保留独立源内容负控、真实事件/Position/不同帧、官方先停、当前身份与完整零阈值差分；候选任何字节差异仍红。原失败和账永久保留，无法解释的官方语义失败仍停止，产品未知项不关闭。

## 公开依据与阅读限制

- [BITMAPINFOHEADER](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapinfoheader)：BI_RGB未压缩、正高度方向、DWORD行stride；支持输入格式裁决，不支持端到端exact。
- [Extended Color Information](https://learn.microsoft.com/en-us/windows/win32/medfound/extended-color-information)：颜色含义/矩阵/范围/传递函数与转换近似；查阅搜索返回正文，不将示例认作本次实际协商。
- [Recommended 8-Bit YUV Formats](https://learn.microsoft.com/en-us/windows/win32/medfound/recommended-8-bit-yuv-formats-for-video-rendering)：round/clip/名义范围与近似公式，不能由本sample选择某公式作为expected。
- [PixelFormats.Pbgra32](https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.pixelformats.pbgra32)：sRGB、BGRA、预乘alpha，查阅搜索返回正文。
- [VideoDrawing](https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.videodrawing)及[Rect](https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.videodrawing.rect)：媒体绘入指定矩形，搜索结果有定义；直接全文抓取VideoDrawing/RTB及播放示例超时，未据此声称读到额外精度保证。
- [MediaPlayer.Position](https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.mediaplayer.position)：亲读正文，当前媒体位置，不是采样surface精确PTS保证。

审计/文档/收尾Completed；实施与所有新执行NotStarted。未保证的端到端exact不可宣称已建立；使用source序列语义与同次official基准的有限边界。显示Unknown、媒体全闭/音频输出/全worker/趋势/HW/loss/full及完整替换仍Open。历史normalbuild1/publish1（隐含编译1）/sandbox1/正式体1 Failed/official1/candidate0/comparator0/Native0原账不变。
