# x86 布局与内容修复包（阻塞，未完成）

## 裁决

完成部分可静态证明的夹具布局修正；未取得最后源码的双架构 Release 实跑通过证据，不能签署本包完成或完整替换完成。生产代码未修改，golden、阈值、skip 未修改。未修改 DesktopDuplication、捕获公共代码、交接总文档或并行 HW 报告。没有删除本地历史证据，没有扩大清理，也未新增原生探针、脚本或第三项目。

## 根因及修改

核对原生 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/api_rendercontext.h` 的 Release CContextState 字段顺序，以及生产 `BitmapRenderTargetExports.Drawing.cs` 的 NativeContextPrefix：三个前置指针后接固定宽度矩阵/状态，因此 x86/x64 的 Clip 分别为364/376，RenderState为384/400，WorldToDevice为388/408。x86 夹具写入376/400/408，实际并未设置预期 clip、状态指针及变换，可能表现为空裁剪或错误状态；不能把此前零像素直接判为生产 FLOAT 算法错误。

新增测试侧独立 `InternalContextLayout.cs`，不引用候选 ABI 类型。修改：
- `BitmapRenderTargetTests.Float.cs`：两个绘制夹具使用实际指针宽度偏移，移除仅允许8字节指针的断言，保留内容预期。
- `BitmapRenderTargetTests.InternalClip.cs`：修正 clip 各坐标、状态指针及变换寻址。
- `BitmapRenderTargetTests.InternalLayers.cs`：修正零透明度层绘制 context。
- `WpfReference/FloatReadbackTests.cs`：官方 FLOAT DrawBitmap 夹具同样修正，未改变独立官方调用或预期。
- 新增 `InternalContextLayoutTests.cs`，检查当前进程布局三元组；这是辅助布局回归，尚未执行，不替代官方行为比较。

Realizer/BitmapRealizer 仍有固定布局和 C++ 成员回调约定风险，本包未冒险把偏移修正当完整 ABI 修复。`BitmapRenderTargetTests.cs` 等兄弟夹具仍有固定偏移，未关闭。FLOAT 零像素是否还有生产根因、source18/affine3 mask 的1字节差异均未关闭，不能仅凭本次静态修正宣称解决。

## 已实际检查的首包证据

根：`artifacts/sandbox-readback/replacement-package1-x86-full/Results/`。
- 回读 `aot-identity.json`：X86，pointer4，候选 SHA256 `3CC69D4F32E5411F81A2A50F6BF1CD23876E6F152524C44C52CCA6B94EF7220C`，实际路径 `C:/CodingAgentSandbox/Tasks/8d94314905f54b8889889fed9888a31c/Native/wpfgfx_cor3.dll`。这只是首包原始身份，不是本次最后修改后的运行身份。
- 回读 `wpf-calibration-dfcacf3f059c4e0a82431010e644edab.json`：Compared450，候选同上；报告 OriginalDllSha256=`0CC79CC937B17827344B8652556A4CFD774CFF64B7FD2709EF5284C480894047`，ReferenceFileSha256=`D1B5D0E89B44A9B392EA7A620B4E3292A921D9C22A914E87D1C18C05146F0B2B`。所读部分明确是 CrossFormatDifferences，包含 source17/affine3 的22/21、32/31、103/102等。报告自身声明跨格式诊断不是 FLOAT golden，不能把该集合自动作为同格式生产缺陷。
- 未取得首包官方角色实际模块版本/路径回读；上述报告字段不补齐该缺口，也不借历史身份补齐。

## 实际发布与沙箱尝试

先检查已有双架构发布目录，再使用现有 x86 Release 发布目录，不重发生产。
1. x86 self-contained/10.0.9/ComAcceptanceSandbox 发布：CS8104，refint 写入磁盘空间不足。
2. 最小化重试（no-restore、ProduceReferenceAssembly=false、DebugType=None、DebugSymbols=false）：编译和 Publish 输出成功，但 PackageSelfContainedComAcceptance 复制 WindowsBase.resources.dll 到 com-acceptance-package 时 MSB3027/MSB3021，磁盘空间不足；整个发布退出1，不能称发布成功。
3. 用该现有 win-x86 发布目录调用 Windows 沙箱，定向 InternalDrawUsesFloatTarget、FloatSpanStretchesHdrGradient、InternalBitmapClipHasNoTargetArea、NativeZeroOpacityLayerDraws，要求 TRX/Results。回收失败，WinRemoteShell退出 -1073740791，IOException磁盘空间不足。回读本地 `artifacts/sandbox-readback/replacement-x86-layout-content-targeted/Results` 仅见 Deploy 目录，无可读最终TRX或identity，不能记通过、不能推断实际执行总数。
4. 官方 FLOAT 夹具修正后再试 x86 最小发布：退出1；之后又新增辅助布局回归，最后源码无成功发布/执行。
5. 最后源码 x64 Release no-restore 发布：NETSDK1047，当前 assets 没有 win-x64 target。带还原重试：NuGet.targets(198,5) 写 ed0ce8e6-d610-4ce6-891f-96cfde88edd0.tmp 时磁盘空间不足，退出1。

## 最后回读与阻塞

最后回读了新增布局定义、布局回归及 FLOAT 夹具实际文件；新运行 Results 无最终 TRX/identity 可读。没有本次最后源码的新 SHA、官方实际模块身份、原始内容或通过数字可提交；生产未改不生成新生产身份。不能把首包候选SHA绑定最后测试改动或把历史x64通过数充当本次复验。

硬阻塞为工作区磁盘空间不足，既影响编译/还原也影响沙箱证据回收。未采取删除本地历史证据的办法。需要先恢复足够空间或提供可回收证据存储，再完成：最后源码 x86/x64 Release 发布、定向布局/FLOAT/clip/layer实跑、官方实际身份/TRX/原始内容回读，以及Realizer成员ABI和1字节差异最小化。当前提交是未完成的局部夹具修正及阻塞记录，不是稳定回归已通过的交付。
