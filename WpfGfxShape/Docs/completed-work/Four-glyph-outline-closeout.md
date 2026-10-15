# MilGlyphRun_GetGlyphOutline：strict raw 保留，裁决待定

最终身份及原始证据见[集成档](Four-exports-integration-closeout.md)。最新测试报告GlyphOutline30项中raw22一致/8不一致（test1–4、6–9）；定义几何字节独立诊断30/30一致。原始GeometryBytes、GeometrySha256及179个差异字节记录全部保留，不替换golden、不过滤strict raw。

依据：原生 `core/resources/GlyphRunGeometrySink.cpp` Initialize使用WPFAlloc后写Flags/FigureCount/Bounds/Size，不写ForcePacking；BeginFigure/AddGenericPoly的结构成员及wgx_core_types.h布局是独立诊断authority。几何头offset44、figure+36、segment+12各4字节为ForcePacking/布局占位，不能给未初始化字节赋原生保证。诊断只排除这些字节看几何定义字段，不改变原始ReferenceComparison.Equal；HResult、Size、Fill、字体/glyph身份仍应分别验收，不能把字节诊断30/30称完整raw Green。

待父级/验收authority裁决：定义字段与未定义padding是否允许独立语义验收；在获得明确批准前，raw差分仍失败，outline完整Closed不成立。30例含em24/0/-1/NaN、5个glyph、sideways交替；不外推全部字体、OOM、并发或全ABI。
