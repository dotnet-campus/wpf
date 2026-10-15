# S-20261009-220：软件 bitmap immediate 创建与共同消费者验收

## 交付范围

承接上轮已留在工作区、尚未全量验收的 bitmap immediate 创建者和共同消费接线。本轮完成 alpha 合同核对、共同消费者的几何裁剪与 AlphaScale 修复，以及 bitmap fill/stroke、ImageMask、多层失败恢复的 AOT 联合验收。保持纯 C#，不修改原 WPF 源码。

这是软件 immediate 窄合同闭环，不是完整 CBrushResourceRealizer、MSVC 对象布局或 private DrawPath 全方法族关闭；批准的大模块仍未全部完成。

## 原生依据

- `core/resources/BrushRealizer.cpp:36-90`：realizer 效果中的 alpha scale 合同。
- `core/sw/swlib/renderingbuilder.cpp:85-230`：AlphaMask 获取位图引用、按映射创建 mask span，并对输入像素就地施加遮罩；PBGRA32 使用 `MaskAlpha_32bppPARGB_32bppPARGB`。
- `core/sw/swlib/bilinearspan.cpp:4514-4540`：已有 alpha 的输入走带 alpha 的 mask 运算。
- `core/sw/swlib/swsurfrt.cpp:1480-1575`：层恢复的几何与常量 alpha fixup 时序；本轮没有据此宣称完整 native layer stack 已翻译。

以上路径均相对于 `src/Microsoft.DotNet.Wpf/src/WpfGfx/`。

## 生产实现与所有权

- 工作区已有 `NativeImmediateSolidRealization.CreateBitmap`：软件 immediate 持有映射为 identity、Border wrap、opacity=1 的 bitmap brush；创建时保留 source，最终 owner Release 释放 source 和分配内存。brush 是借用对象，不是独立拥有引用的完整 MSVC 对象。
- 工作区已有 ImageBrush 中间位图 → MIL source QI → bitmap immediate → `DrawSoftwareRealization` 接线，与 private FillInternalShape 使用同一 `ConsumeSoftwareRealization`。generated 路径没有调用 private DrawPath 槽本身，不能将共同消费者误称完整 private ABI 已闭合。
- 本轮在共同消费者合并 drawing-context GeometryMask 与 fill/stroke coverage，solid/bitmap 均不再丢弃既有几何约束。
- bitmap opacity 与 drawing-context AlphaScale 相乘，不再覆盖调用者的 alpha 状态。效果、aliased clip、compositing 继续随绘制上下文传递。
- 创建/消费失败仍直接回传首个 HRESULT，finally 释放 immediate owner、QI source、bitmap lock 和中间位图；GeneratedVisualRenderer 在绘制首错时 AbortLayers，并逆序释放帧资源。

## 测试合同与结果

开始先发布，再测试。未指定 csproj 的首次测试误选原 WPF solution，因 TraceWpp.exe 缺失而失败；随后始终使用唯一验收 csproj，不把该构建失败当成 AOT 行为失败。

初始 AOT 全量：380 通过、1 失败、0 跳过，共 381。遗留用例使用同一个半透明 ImageBrush 作填充与遮罩，原预期 `0x80800000`，实际 `0x40400000`。原生 mask 合同及测试源像素表明 alpha 应再次相乘；修正错误测试预期，不修改生产 alpha 运算迎合旧断言。既有同参数测试已覆盖此合同，消除重复后新增 bitmap stroke 三层 opacity/mask 失败恢复用例，并保留上轮 tiled fill 回归。

中间发布的定向验证出现一次新增测试预期错误：scopedGeometry 根据 translucentGeometry 参数重写了源像素，实际为不透明红。改为显式半透明、多层 bitmap stroke 用例后重新发布验收。

最终发布 #3：

- 定向筛选：`FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~BitmapRenderTargetTests|FullyQualifiedName~CompositionTests`，280/280，202 ms，0 失败/跳过。
- 全量：382/382，362 ms，0 失败/跳过。
- 测试项目：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`。
- 全量 TRX：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，run id `fb6b4f81-b5b7-4890-a7fa-7bfef125d54e`。
- 最终 SHA256：`7FEA984DB864E948C3683DD5D46D5DF3AC4E91140BD865A758DA3A2829E456C4`，从最终全量 TRX 的实际加载产物日志取得。
- 定向/中间 TRX 被标准固定文件名覆盖，未另行保存其 run id；不虚构历史产物 hash。

## 本轮逐次发布

三次完整命令均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，目标项目 `WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj`，配置 Release，RID win-x64。

共同产物路径：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

1. #1 开始前发布成功，日志 11 行。对应初始 380/381；未提取该发布的 SHA256/TRX run id，产物随后被覆盖。
2. #2 bitmap GeometryMask/AlphaScale 修复后发布成功，日志 60 行；包含 CA1416 平台分析警告，未记录为零警告。定向 261 通过、1 失败，共 262（筛选未包括全部 CompositionTests），失败为新增 scoped 测试预期错误。未提取 SHA256/TRX run id，随后被覆盖。
3. #3 将 GeometryMask 合并移到 solid/bitmap 共同消费处后发布成功，日志 60 行；仍有 CA1416 警告。最终定向与全量结果及 SHA256/TRX 如上。

测试重编译另有既存 MSTEST0037/MSTEST0044 警告，未扩大到本轮范围外修复。

## 未关闭边界与后续模块

完整 resource realizer 的创建/Ensure/FreeRealizationResources、真实 meta-intermediate fixup、完整 QI/MSVC 多重继承布局、非圆 dash/marker、完整层栈和真实重入删除替换销毁尚未关闭。本轮全量通过不证明这些能力已实现；不能替换原 wpfgfx_cor3.dll，阶段 4 仍 Active。

下一轮合并推进 resource/immediate realization 与 fill/stroke/Compose 生命周期：按原生创建者、所有权和 FreeRealizationResources 时序实现生产资源 realization，接入共同消费者；同时闭合实际失败和重入释放路径、AOT 发布验收与文档。不得仅新增槽测试、参数测试或重复复验作为独立交付。
