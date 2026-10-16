# Glyph outline 本轮执行报告

## 状态

未完成像素一致性目标。已实际修复测试的非法 DrawGeometry record 长度，原版 outline 路径现在能够 CloseBatch、Commit、Present，不再在本轮运行中 AV。严格整面像素断言保留，当前三组 outline 用例仍失败，不得作为通过验收。

## 已核对的协议与证据

读取 av-stages/Results/glyph-outline-stages.txt 和 av-no-glyph-command/Results/glyph-outline-stages.txt：均停在 close batch。重新执行 reproduce 时到 batch closed 后中断，因此旧日志不能精确证明 AV 一定发生在 CloseBatch 内部。

源码 Generated/RenderData.cs 中 MILCMD_DRAW_GEOMETRY 有三个 uint handle 和一个 QuadWordPad0，payload 为16字节；record header 为8字节。原测试记录写20、发送32字节，漏掉尾部padding。现按 CompositionTests.ActiveRender.cs 的稳定编码改成记录24、发送36字节。

资源枚举与命令核对仓库原版生成头：PathGeometry=73 / 0x7d；SolidColorBrush=75 / 0x7e；DrawingGroup=91 / 0x8b；Visual=39；RenderData=43 / 0x18；DrawGeometry=0x46。四阶段对原版及候选各执行一次，8项均实际通过（full TRX 2415行起）。阶段为单独PathGeometry空路径、单独SolidColorBrush、DrawingGroup经RenderData绑定Visual、RenderData合法DrawGeometry空handle记录；不把空handle阶段称为非空几何像素验证。

原版 apifunc.cpp 的 GetGlyphOutline 接受IDWriteFontFace且消耗一份引用；现有AddRef保留。返回的是连续MilPathGeometry，不是MILCMD_PATHGEOMETRY命令。该块可作为命令20字节header后的Figures数据。两者本轮均返回544字节、fill=1、FigureCount=2。ForcePacking位于header最后4字节，原版实际值不稳定，候选为0；分别保存outline-original.bin和outline-candidate.bin，未要求垃圾字节相等。

## 非空白 oracle

新增expected非空断言。此前补齐record后出现的3项通过其实是透明面相等，不能算有效结果。原版wire MilColorF为RGBA，而候选内部类型为ARGB；原outline测试把原版alpha写成0。当前使用不透明白色排除通道歧义，并对Visual设置(2,20)使基线坐标轮廓位于32x24目标内部。

严格像素对比仍失败：首差byte offset=1180，原版255，候选251。普通、锁失败重试、owner释放三组同样失败。未加容差、未skip、未捕获AV，未使用系统实现生成候选输出。覆盖率/扫描转换差异尚未定位修复，颜色wire差异亦未修复。

## 发布及实际测试

已实际执行生产Release win-x64 NativeAOT publish，随后发布自包含ComAcceptance，最终包在WpfGfxShape/artifacts/com-acceptance-package，含aot.sha256。未保留任何本轮生产代码逻辑修改；生产中原有WIP一起重新发布，但本轮没有独立证明72-byte相关用例的具体覆盖。

包SHA256、目标Results/aot-identity.json、full Results/aot-identity.json均已实际回读，完全相同：

7C62FB28EB058246C50A32F963F1F1EB5535AD46A902B6F82403B8B7251ABB1B

最新目标：white-targeted/Results/Administrator_EN-DEV-01_2026-10-15_02_06_37.7822963.trx；41项，38通过、3失败、0跳过。过滤FullyQualifiedName~Glyph。

最新full：final-full/Results/Administrator_EN-DEV-01_2026-10-15_02_07_55.9128027.trx；1440项，1405通过、35失败、0跳过。包含相同3项outline失败及32项其他硬件/桌面呈现相关失败；原生GPU control也记录桌面像素0，不能据此宣称其余32项都是生产回归或都是环境问题。

目标task=65d9bd3fd3714518aab7ae81279bd0a1；full task=527aac8b67af442f88ed15688f2ad33b。

四份Docs未修改。未新增原生代码、Host、假ABI或探针。当前新增测试仍以本地packet编码为主，尚未提炼Shared helper，因此也未完全满足复用helper要求。
