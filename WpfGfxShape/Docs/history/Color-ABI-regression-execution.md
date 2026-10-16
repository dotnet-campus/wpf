# 颜色 ABI 全局回归：文档代理独立接入页

## 结论与范围

本轮为测试协议修复及实际全局回归，不是总计划完成。生产代码没有修改，未重新发布生产 NativeAOT。四份主 Docs 均未修改。已有生产包及沙箱实际加载身份为 `77394561E16E13DEC82DD194DC5C0090D4A64A5477FEA83D7879DE2369D1D33D`。仅回收 Results，未回收执行器、运行时或生产 DLL。

## 两个真实失败与独立证明

修改前实际执行 GeneratedBrushResourceTests + GeneratedValueResourceTests：17 pass / 2 fail / 19，0 skip。失败分别为 WhenValuePacketsAreWrittenThenCommandIdsHandlesAndValuesMatchNativeLayout（旧 expected A/R=1/.25，实际 R/G=.25/.5）和 WhenBrushPacketsAreWrittenThenIdsFieldsAndGradientPayloadMatchNativeLayout（旧梯度首 float expected Alpha=1，实际 Red=.5）。

权威 `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/Generated/wgx_misc.cs:196–203` 为 Pack=1 的 r/g/b/a。生产 MilColorF 保留内部构造参数 Alpha/Red/Green/Blue，但显式内存偏移 Red0/Green4/Blue8/Alpha12、Size16。原两测试现在验证全部 RGBA 四通道，不是降低预期。新增 MilColorWireLayoutTests 实测 sizeof=16、Marshal.SizeOf=16、四 backing field 的 Marshal.OffsetOf=0/4/8/12；独立固定 IEEE754 小端字节为 `00 00 80 3E 00 00 00 3F 00 00 40 3F 00 00 80 3F`，不由生产结构计算 expected。

同类写包检查：ColorResource、SolidColorBrush、Linear/RadialGradient payload、Ambient/DirectionalLight、Diffuse（Color 和 Ambient）、Specular/EmissiveMaterial、TargetSetClearColor，11 个固定协议偏移字节用例通过。也审阅 DropShadowEffect 的 MilColorF 嵌套布局和读包、3D 颜色结构、即时实色与 BitmapRenderTarget 的 float 指针 RGBA 边界、hardware Clear 的成员访问及 intermediate buffer cast；未发现需要修改生产的 ARGB wire 旁路。ARGB packed uint 像素转换不等于 MilColorF 的 wire 顺序，不改动该转换。

## 最后源码构建与实跑结果

| 实跑范围 | 结果 | 说明 |
|---|---:|---|
| WpfGfxShape.slnx Release 源码构建 | 成功 | 编译生产与普通测试/ABI 项目；不重发布生产 |
| ComAcceptance Release 源码构建 | 成功 | 与已有生产包回归分开记录 |
| 新增 ABI 独立证明 | 13/13 pass | sizeof/offsetof、固定字节、11 siblings |
| 最后修复相关普通测试 | 32/32 pass | Brush + Value + MilColorWireLayout，0 skip |
| 全普通 WpfGfxShape.Tests | 4224 pass / 1 fail / 4225 | 0 skip；真实完整执行，不以小组结果替代 |
| GenProto 全量 | 24 pass / 4 fail / 28 | 0 skip；真实完整执行 |
| 同基线 related COM | 765/765 pass | 0 skip，geometry/glyph/通知/async/effects 等原过滤范围 |
| 额外宽 related COM | 1246 pass / 2 fail / 1248 | 两个 hardware cache geometry present 用例，保留失败 |
| COM 全量 | 1415 pass / 32 fail / 1447 | 0 skip，与提供的 32 present 基线相同，不随机追绿 |

全普通测试唯一失败：SystemVideoSampleTests.WhenRealSystemMediaHasNoFrameThenSoftwareSampleLeaseCanBeClosed，校准 JSON 指向 `C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\10.0.12\wpfgfx_cor3.dll`，本机加载抛 DllNotFoundException。未改外部校准输入，未 skip。另尝试测试执行器自包含发布与沙箱执行；首次无 exe，第二次无 Results 回收，不能计为有效沙箱普通测试结果，以上普通测试数字来自本机完整 dotnet test。

GenProto 四失败：ConsumerAssertionTests.WhenRegeneratedProtocolDeclarationsAreComparedThenFrozenCommonConsumerMembersAndOffsetsMatch（GenProtoEvidence 目录当时不存在）；EvidenceArchiveTests.WhenEvidenceIsArchivedThenMachineStatesAndBinaryIdentityAreReadBack（path=null）；IndependentManagedCoreTests.WhenFrozenManagedCoreIsMeasuredThenFixedWidthFieldsMatchBothNativeTargets（path=null）；Stage4ClosureTests.WhenLedgerIsValidatedThenAllReferencesResolveAndCountsAreRecorded（6 个 Satisfied evidence path/hash mismatch）。外部证据未伪造，未改 ledger、基准或生产来隐藏失败。closure report 在其既有 GenProtoEvidence 输出目录，以上为本轮实跑错误摘要。

## 原始 Results 与身份

- `com-full/Results/Administrator_EN-DEV-01_2026-10-15_02_37_20.4701392.trx`；task `b9e4be771379477f9ee35be64a92223a`。
- `com-full/Results/aot-identity.json`：实际加载 SHA 与上述相同；全量含 glyph outline 三组 Passed、红蓝两组 Passed 及 glyph command 两组 Passed（7/7），不以空图或放宽像素比较替代。
- `com-related-765/Results/Administrator_EN-DEV-01_2026-10-15_02_39_47.1987772.trx`；task `1f905cb906714eba9754fbb2b3db3d10`。
- `com-related/Results/Administrator_EN-DEV-01_2026-10-15_02_35_56.4530309.trx`；task `d614b97fb3c94d74a693226e0558b117`。
- 原样保留 32 present 的原生 control/桌面窗口证据：control staging 正确而桌面 readback 为零、conditionSatisfied=false，不据此断言全部仅是环境或全部为生产错误。

外部 input 与真实 present 条件仍开放。当前已完成本轮颜色测试修复与构建/全量执行实证，但全局不是全绿，也不声明总完成。

## 后续实际执行：Docs 与 GenProto 证据恢复

以上24 pass /4 fail为颜色回归当时原始结果，不覆写。本次已实际同步四份主交接Docs及独立glyph/color ABI归档，旧unit27/29、7C62…35fail仅保留为历史。Glyph normal/retry/ownerRelease非空整面3/3、coverage半向上、DrawGeometry padding、RGBA wire有限闭合；严格原版未初始化padding raw及通用OOM/异常Open保留，Missing2/Extra0不冒充新SHA strict实跑。

最终GenProto源码后Release构建成功，随后Debug全量28 executed /28 passed /0 failed /0 skip，退出码0。原始 `../../../Tests/WpfGfxShape.GenProto.Tests/TestResults/lindexi_DESKTOP-51A5UGG_2026-10-15_10_51_03_net10.0.trx`，RunID `b93b5895-7477-4c0c-8add-88bdb46010bc`；已回读头部、归档stdout、Counters。未执行COM或生产publish。

新完整归档 `../../GenProtoEvidence/delivery-6a03b29edb064c22a50b7b1985affa62/report.json`，SHA256 `1274062D5CB9C18ED1644F26D269B5193B75E9478AB519FCC818657621D85107`；测量程序集SHA256 `D695CE5E9B82C7376A22773EFC873D040A8CB8F5F2804C58AFFFAAE53F1EBB64`，.NET10.0.13/X64。source report/hash与嵌入报告、Ledger snapshots均实际生成，生产SHA773945…不变。

根因证据：旧closure-fc375…仍保留6 mismatch，指向当前缺失delivery-3e43…及其source树；并非已证内容篡改，也不能证明是移动或root错。当前root正确，独立生成恢复可测映射；不造旧原物、不改旧hash。移除归档测试硬编码ordinary4212/0与虚假publishSucceeded声明；固定名历史COM TRX实际254项，改记historicalComRun而非full。closure新增sourceReportHash严格校验和具体ID/路径诊断，未降低required。详情、确切缺失历史文件及最终证据见[独立恢复记录](../../../Docs/investigations/genproto-evidence-recovery.md)。

28/28仅有限验证，历史authority/ARM64真实输入、完整physical闭包继续Blocked，stage4Closed=false；七阶段、四外部input、DEC-0004进程驻留决议不变。
