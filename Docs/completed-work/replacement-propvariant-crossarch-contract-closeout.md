# PROPVARIANT 跨架构契约与 factory 正式收尾

## 本轮历史更正：EvidenceRecovered / X86MfPassed / OverallOpen
下方EvidenceUnavailable为上轮审计方法错误，原文保留，不再作为当前结论。独立执行核验子智能体已直接Read指定根md文件，未当同名目录；嵌套同名md与嵌套正式report精准Read返回文件不存在，但有限列嵌套artifacts/propvariant-crossarch确有原件，不能称所有artifacts不存在。factory任务ea4ca67a6fe5463385a292b254d85375 TRX38/监督40/三业务93,95,95全文3/3/0 child0，唯一NativeF4D4/noofficial/sourceLinkfalse、CloseShutdownRelease0/refs0/disposals1；前代理准确前读范围Unknown，本次范围另记。MF任务62165d941d3e450baab5dfe5f4ad2d4c TRX26/监督40/resource76/source-reader1006/duration100全文亲读，1/1/0 child0，40帧PTS/RGB/EOS及40释放/reader0/MFShutdown0/CoUninitialize1完整。test148F001BF0A763BB7FA4442204265A0C4CA920E1A1D356646716ABDA33C89455/MVID24c6bee8-e7a8-4596-af2f-573942848ea4/.NET10.0.13 X86；duration别名程序集SHA29C0/MVID2f0391d5分层。MF业务source-linked非NativeAOT；组合桥未调用独立IMFSample Get/SetDuration，仍未证，不以duration/replay替代。pure23仍历史结果未新核。x64NotRun。
当前详细原件范围、十项FQN参数/适用性、发布参数、实际差分与cleanup边界见 [ReferenceData恢复报告](replacement-reference-ten-recovery-closeout.md)。十项实际8执行8Passed/2BlockedUntrustedGlyph，DirectPbgra450/0，跨format57差保留。此次normal0/tests0/COM1/sandbox1/Native0/production0/full0，不重跑既有factory/MFx86。生产Video24仅x86overallocated未证ABI错。旧91/音频endpoint0/AVI16samples缺口/AVsync/强cancel/HWloss/resourcesUnknown/fullOpen保留，完整替换OverallOpen。

## 终态：EvidenceUnavailable / CloseoutBlocked / OverallOpen
主代理亲自只读取证及修改文档，无终端、脚本、再委派。此次源码/build/tests/publish/沙箱均 0。未把交接信息伪装为本次亲读，未新增执行补证。本文已落盘，但指定任务的证据认证尚无法完成。

## 当前可见工作区与原件缺口
工作根由目录工具确认：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape`。
- 用户提及 `.coding-agent/propvariant-crossarch-receipts/` 目录不存在；实际只有 `.coding-agent/propvariant-crossarch-receipts.md`，已亲读全文 L1–9。该旧文件 L3 仍写“当前全部0”，L9 仍为计划，不能当最终执行收据。
- 按名称搜索 `toolreceipt`、`62165`、`artifacts` 均无匹配；按内容搜索 MF 完整任务名仅命中新建本文，factory 任务名前缀在 `.coding-agent` 中无命中。根和 WpfGfxShape 子目录列表均无 artifacts。没有从 tool receipt 获得回收路径，禁止猜测或创建伪原件。
- 因此 stdout 指定 TestResults/TRX、MF 完整业务 JSON、supervision、identity、callHR、40frame RGB 内容、PTS、EOS、readerShutdown/COM/MF uninit、sample ABI Get/Set/duration、finalClose 均没有可亲读原件。其 counts、完整 SHA/MVID、退出码及清理结论在本次认证中均 Unknown；不能以开头 Passed 代全。

## 交接结论（非本次原件认证）
- factory `taskea4ca67a6fe5463385a292b254d85375`：用户报告实际 3 passed、child 0；new test SHA 前缀 `148F`、MVID 前缀 `24c6bee8`，actual Native 前缀 `F4D4`，requested=actual、no official；Close/Shutdown/releases、player/proxy/factory 0、descriptor 1。不补全截断身份；当前未找到前代理准确读取范围，不虚构范围。
- x86 MF `actualtask62165d941d3e450baab5dfe5f4ad2d4c`：交接要求回读，但缺 tool receipt 和业务原件，不能正式判 Passed 或 Failed；失败归因 Unknown。不得重跑。
- x64 未 publish/run，保持 NotRun。旧 full x64 timeout 的 Unknown 是另一历史记录，二者不混合。

## 即时账（来源严格分层）
用户交接：normal build 1，3910 warnings/0 errors；pure 23 passed，implicit Debug build 1；COM publish 2；sandbox 3；Native 0；production 修改 0；新 full 0。COM/sandbox 预算已耗尽，不增用。额外首失败次数、逐次命令/退出码、工具 numeric exit 是否提供和日志截断范围，当前无对应 receipt，均 Unknown，不能用旧轮数据顶替。此次新增执行全部 0。

## 契约与架构硬编码
官方源已核 x86 native 16/x64 24，见旧收据 L6。生产 Video offset 8/固定 24 仍存在，但仅能说明 x86 overallocated，未证明生产 ABI 错误，不改 production/Native。测试真实布局字段、移除硬 pointer8 和纯测 23 pass 为交接结论，不能据此宣布全库无架构硬编码。Audio 的真实自然布局及两架构系统认证未在本次补证。
source-linked 系统 MF 即使 Passed，最多证明托管生产源码与真实 WinAPI32 契约；旧 fixture Native 仅 init，不冒充新增 Native MF 导出。architecture query 与 actual-module 字段必须以真实原件认证，现为 Unknown。
模块必须精确识别 `wpfgfx_cor3.dll`；修 managed-host 误匹配不能弱化唯一独立 Native 模块、SHA 和 no-official 要求。

## 历史与未闭合门
旧 factory residual 3 failed/关闭0完整保留；旧 full x86 1843/1752/91 不相减，x64 timeout 分母 Unknown。原 audio 强失败和 AVI 少16 samples 失败保留；video-only 40 frames 不能替代 audio/AVsync/cancel/HWloss/resources/fullOpen。ReferenceData 十门 NotRun，受信 5EBD 原版 Tile header-only 不等十场景适用。

## 下一具体事项（本轮不执行）
先由已有任务的持有方恢复原 tool receipt 与回收文件到当前工作区；只读 receipt 给出的路径，再从 stdout 精确定位 TRX，逐段读到业务和清理尾部，补真实终态，不重跑。
下一业务验证固定为受信原版 ReferenceData 十场景：按现有十门逐 case 核对原版身份、架构、内容和 oracle 适用性；5EBD Tile header-only 不适用的 case 须受信原版该场景数据，禁止 candidate 自 oracle。此处不授权任何执行。x64 hung 如转入调查必须有界取得自有 officialWindow 线程现场，不能凭 timeout 判 deadlock。
