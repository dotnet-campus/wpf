# Glyph方法族当前映射与验收边界

证据：../../Docs/handoffs/completed-work/S-20261012-GLYPH-PIXEL-CLOSEOUT.md。

- core/glyph/glyphruncore.cpp、core/resources/glyphrunslave.cpp、Generated/wgx_commands.h → Core/GeneratedGlyphRunResource.cs：76字节命令、glyph数组、真实IDWriteFont/CreateFontFace、替换/失败/最终释放。
- glyphrunslave DirectWrite analysis/alpha realization、软件glyph painter/gamma → Core/GeneratedGlyphAlpha.cs：真实分析及texture、中心子像素灰度、gamma、整数source-over与float覆盖。
- GlyphRunDrawing/drawingcontext DrawGlyphRun/bounds → Core/GeneratedDrawingInstructions.cs、GeneratedVisualRenderer.cs、GeneratedImageContentBounds.cs及现有软件渲染链：两条完整surface实际消费者，clip/opacity/translation/brush/fontface/锁失败重试/HDR。
- UCE transport引用移交 → Core/SameThreadChannel.cs：自动handle接收者先创建，修复路由消费后传输再次释放font的悬空引用；真实自动handle2项通过。
- 实际引用存活关闭：glyph handle释放2项、通道/连接关闭4项；真实传输Dispose回调重入沿用UceChannelTests两个实例，无虚构glyph回调。

范围完成：generated软件离屏系统字体消费者方法族。文件族仍Partial；复杂脚本、侧向bidi组合、ClearType显示目标、动画缓存、所有brush/一般仿射未纳入完成范围。

最终实际定向发现32/通过32/失败0/跳过0；RunID 3ba97665-f1ef-48b9-aa6a-9ad4e73b49f1。
全量1230/通过1229/既有失败1/跳过0；RunID f4b49add-af5a-422d-bc5c-c989de5de142。
发布2 SHA256 38D2FA963A7666A995617ADB000737172FF9F433A48FE2709AFFB23C0B4050A0。
报告wpf-calibration-1531ed30aecb4ca2b799ff6de5866194.json：float投影57/450、Pbgra32 0/180，严格校准失败保留。

本视图同步已核实方法映射和证据，不晋升损坏机器JSONL；schema-version.json仍RepairRequiredPartial。阶段4 Active，不能替换原库。下一生产大块D3DImage真实interop。本轮最终全量后仅写文档。
