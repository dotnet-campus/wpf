# AJ / AM Bitmap 布局修复及集成失败分类

## 范围与结论

AM 已完成一个确定的紧邻夹具根因闭环，未完成全部生产自产路径。AJ 既有生产改动为 NativeBitmapBrushLayout、BitmapRenderTargetExports.Drawing.cs、NativeImmediateSolidRealization.cs；AM 只修改 `Tests/WpfGfxShape.ComAcceptance/BitmapRenderTargetTests.BitmapRealizer.cs` 的架构字段偏移和说明。不修改项目、入口、独立测试、skip、golden、阈值、fallback、曲线覆盖、圆角、通知、上传或捕获。

全程使用专用读写/发布能力及 Windows 沙箱执行编译后的正式 C# 测试程序，无终端、shell或脚本。没有清理其他人的目录。AM 独有 ignored 证据根为 `WpfGfxShape/artifacts/am-bitmap-closure-20260721-a/`（下文路径均相对于工作区）。本报告先登记交接事实，再完成修复，最后更新落盘并回读；不是全仓验收通过声明。

## AJ 原始事实登记

原始根：`WpfGfxShape/artifacts/aj-bitmap-layout-20260721/`，未覆盖。

- `pre-readback-x86/TestResults/pre-x86.trx`：4项，2通过2失败；交接为官方2通过、候选2失败。修后独立双架构各4/4为交接事实，并由AM最新运行再次验证。
- `post-readback-x86/TestResults/post-x86.trx`：191项，160通过31失败。失败全属 `WhenPrivateBitmapBrushIsRealizedThenMappingClipAndOpacityReachPixels`，原始消息在DrawRealizedPath阶段返回-2147467263，非缺reference。
- `post-readback-x64/TestResults/post-x64.trx`：191项全通过。
- AJ x86 actual SHA256：`609893C352E789BD672217DE62F6DD4C984470D6E6FADC730462F0833E4A4673`；x64身份 SHA256：`6514FF2F76B36237BE1EA46C968C39DB8B9ECAEEB03E5A454CB49C9C4C518BD9`。独立actual自带模块路径、架构和SHA；各 `Results/aot-identity.json` 的Arguments被source-clip官方子角色覆写，不能拿它冒称父全族进程身份。
- `producer-readback-x86`：交接GetState/Ensure崩溃，未发现顶层完成TRX；不是0失败。
- `producer-readback-x64/TestResults/producer-x64.trx`：128项，103通过25失败。现已分为：image-mask锁源族16项present HRESULT失败；DrawingImageVisualMask三项缺 `ReferenceData/original-wpf.json`；RotatedDrawing六项present HRESULT失败。不是25项全部缺reference。

## 已闭环根因：兄弟夹具固定x64偏移

官方声明 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/api_brush.h:245-249,419-445` 明确 CMILBrushBitmap 多继承、sampling space、matrix、CParallelogram、两个BOOL、texture、wrap、border、opacity顺序。独立测试 `IndependentBitmapRealizerAbiTests` 通过独立Sequential声明及真实官方行为验证 CMILBrush 次级基址的偏移。

旧兄弟夹具无条件写 matrix80、points160、useClip200、texture208等x64位置，在x86上构造了无效brush。AM最小修改为以下CMILBrush-relative架构字段表，仍保留原240字节清零缓冲及全部原始断言：

| 字段 | x86 | x64 |
|---|---:|---:|
| sampling space |44|76|
| matrix |48|80|
| clip points |120|160|
| use clip |156|200|
| texture |164|208|
| wrap |168|216|
| border |172|220|
| opacity |188|236|

未改变生产以适配坏夹具，未猜测未知兄弟布局。本根因的Red是AJ原始160/191，Green为AM新正式包191/191；31项全部消失，证明它们属于夹具旧偏移而非本次新增生产bitmap缺陷。此结论不覆盖自产resource realizer。

## 最新源码双架构Release正式包

专用发布均成功：生产NativeAOT win-x86/win-x64；ComAcceptance两架构 `ComAcceptanceSandbox=true`、`SelfContained=true`，显式对应新AotDllPath；另外仅现有正式托管Tests项目两架构自包含发布，EnableMSTestRunner=true。未创建第三项目。生产和包输出分别置于AM根的 `x86/`、`x64/`；托管置于 `managed-x86/`、`managed-x64/`。

| 路径 | 实际结果 | 原始证据 |
|---|---|---|
| 原生bitmap/realizer族 x86 |191通过/191，0失败0未执行|`readback-x86/TestResults/am-family-x86.trx`|
| 原生bitmap/realizer族 x64 |191通过/191，0失败0未执行|`readback-x64/TestResults/am-family-x64.trx`|
| 独立官方/候选bitmap |两架构各4/4，包含平移/非平移|上述TRX及各Results/independent-bitmap-realizer-*.json|
| 托管Tests x64全量 |4249通过/4249，0失败0未执行|`managed-readback-x64/TestResults/am-managed-x64.trx`|
| 托管Tests x86全量尝试 |输出停在[+3566/x0/?0]，无完成TRX，不能报全通过|`managed-readback-x86/`、`execution-observations.txt`|
| 自产image-mask x86 |GetState/Ensure未处理NullReferenceException，未完成|`producer-x86/`、`execution-observations.txt`|
| 自产image-mask x64 |119项，103通过16失败，0skip|`producer-x64/TestResults/am-producer-x64.trx`|

正式包manifest `aot.sha256`与独立actual核对相等：

- x86 candidate：`C422361F16B9870A0C8B3B2F4B6544E004CFAD6C55F131975D23B28B248D7A12`。
- x64 candidate：`90FA6BCB10C53E9E42221C5BA1524C55936401165EB12F5106127E05B63ACBE2`。
- 同轮官方x86：`5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`。
- 同轮官方x64：`AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`。

官方 FileVersion `10,0,1326,50504`，ProductVersion `10.0.13-servicing.26504.104`，commit `4099f83977f4d29604a9aeb971fb0a7bc1d5f699`；实际运行.NET10.0.13。候选内嵌ProductVersion不是dirty源码身份凭证，本次身份依赖新发布路径、包SHA与actual一致，不拼接历史SHA/full结论。

独立actual：两架构candidate每个case均16次HRESULT=0，Ensure/Brush/Effects各16，ContextMatches=true。非平移BGRA `[0,0,255,255,0,0,255,255]`；平移 `[0,0,0,0,0,0,255,255]`，零阈值断言未改。x86阶段日志 `bitmap-stages-True-0ea36e7289674affa5f09de54fdfe1f2.log` 已回读至DrawInfinitePath.15.after=0、GetTargetBitmap.after、Release.after。

有限source-clip两架构均通过现有纯C#官方子角色当场生成reference，各两次官方1/1成功，位于AM `readback-*/Results/source-clip-reference/<guid>/ReferenceData/`；未写tracked ReferenceData。

## 自产崩溃阶段与未完成路径

最新x86自产身份文件 `producer-x86/Results/aot-identity.json` 指向任务 `daffd5da736347d795b433d5dc90a044`、对应新candidate SHA，参数是image-mask族而不是191项族。沙箱原始输出完整可见栈只有：

- `System.NullReferenceException`
- `NativeResourceBrushRealization.GetState(Instance*) + 0x3`
- `NativeResourceBrushRealization.Ensure(Instance*, UInt32, UInt32, IntPtr, IntPtr, IntPtr) + 0x32`

这证明生产Ensure→GetState阶段崩溃；没有完整原生调用栈、case前置阶段或数值退出码，工具未提供数值退出码，本轮没有补充崩溃分阶段探针，不能声称完整崩溃阶段闭环。`execution-observations.txt`落盘保留该输出和执行边界。

源码观察：NativeResourceBrushRealization成员仍为默认unmanaged调用约定，而紧邻NativeImmediateSolidRealization已使用Thiscall；这是下一根因调查线索，不是已证明结论，本轮未作大范围调用约定替换。

尚未完成项明确保留：

1. x86 NativeResourceBrushRealization.GetState/Ensure崩溃的精确调用约定/槽位/this证据及生产修复和回归。
2. x64 image-mask锁源族16项真实present HRESULT失败（新跑119项中103通过）；它们不依赖缺reference，不能隐藏或归为环境。
3. AJ RotatedDrawing六项（scalingMode1/2/3 × tiled false/true）present HRESULT失败，本轮未重跑、未修。
4. AJ DrawingImageVisualMask三项(17,0)/(17,1)/(17,2)缺original-wpf.json，尚未通过现有官方完整生成流程补齐并复验；未用缺文件结果替代真实生产缺陷判断。
5. 托管Tests x86未得到完成TRX，原因未查明；不把进度计数3566当完成分母。
6. 未跑全仓原生全量、未闭环其他未知兄弟风险。191项族已完成，不等于自产128项或全量Green。

## 最终状态

本轮交付一个最小夹具根因闭环及双架构正式自包含实跑证据。生产自产问题仍未完成；没有修改skip/golden/阈值/fallback，没有用历史SHA或缺reference粉饰通过。报告以本次落盘回读为准。
