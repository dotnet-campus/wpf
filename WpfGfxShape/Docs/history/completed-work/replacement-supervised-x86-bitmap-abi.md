# AI：x86 BitmapRealizer 独立夹具有效性与缺陷交接

## AL 最终只读回读结论

**独立纯C#夹具有效：官方x86合法成功出图；被测归档候选x86透明，缺陷已发现，生产修复未验收。** 本节替代初稿“尚未证明官方合法成功”。AL只读代码/既有产物并写文档，未改代码、构建发布运行或使用终端/shell/直接间接脚本；主代理仅监督。

实际产物根是项目内 `artifacts/supervised-x86-bitmap-abi/readback/`，不是省略readback的根下final目录。下列原始文件已找到，未以路径误差冒充缺失。历史D候选仅作归档差异证据，不代表当前源码或AJ修复后候选。

## 最终计数（AL亲核）

|包|TRX（相对readback根）|执行/通过/失败|
|---|---|---|
|x86 Bitmap|`final-x86/ResultsBitmap/Administrator_EN-DEV-01_2026-10-19_11_25_48.6835667.trx:84`|4/2/2，退出2|
|x64 Bitmap|`final-x64/ResultsBitmap/Administrator_EN-DEV-01_2026-10-19_11_26_14.8295239.trx:62`|4/4/0|
|x86兄弟回归|`sibling-x86/ResultsSibling/Administrator_EN-DEV-01_2026-10-19_11_27_17.3110073.trx:40`|2/2/0|
|x64兄弟回归|`sibling-x64/ResultsSibling/Administrator_EN-DEV-01_2026-10-19_11_27_28.4725831.trx:40`|2/2/0|

上述Counters均error/timeout/aborted/notExecuted=0。x86两通过是官方False/False、False/True；两失败是候选True/False、True/True，不是夹具整体失败或官方失败。兄弟计数不扩大Bitmap合同，也不是full。

## 自然布局及官方行为证明（AL亲读源码与8份业务JSON）

`Tests/WpfGfxShape.ComAcceptance/IndependentBitmapRealizerAbiTests.cs:23–61` 独立声明Sequential的MilObject、ResourceCache、次级BrushTable和BitmapBrush，不把候选类型当唯一预期；`:143–160` 使用Marshal.OffsetOf减BrushTable偏移，得到相对CMILBrush子对象的Matrix/Texture位置。C++成员槽用Thiscall，COM调用为Stdcall。不是任意猜固定偏移或只做sizeof自洽；官方实际颜色和平移消费提供行为佐证。不声明直接观测ESP/寄存器。

|架构|Matrix/Texture相对子对象偏移|官方未平移BGRA两像素|官方平移BGRA两像素|候选|
|---|---|---|---|---|
|x86|48/164|`0000FFFF 0000FFFF`|`00000000 0000FFFF`|两输入均`00000000 00000000`|
|x64|80/208|`0000FFFF 0000FFFF`|`00000000 0000FFFF`|两输入均与官方完全一致|

每份JSON均Results为16个0，Ensured/Brushes/Effects各16、ContextMatches=true。x86不是调用失败：官方确实消费纹理并产生非空红色，平移改变像素；候选HRESULT/回调成功但像素全透明，不能以S_OK作为功能通过。初始化384字节缓冲用于让旧候选的错误x64前缀读取落在已初始化存储内，不修改正确x86布局，也不为旧读取填充伪纹理来掩盖缺陷。

八份原始业务文件均位于 `final-<arch>/Results/independent-bitmap-realizer-<Candidate>-<id>.json`：

|架构/角色|未平移id|平移id|
|---|---|---|
|x86官方False|21f3f05ec87e496fb33b8b777d267b68|9ce8a98a7a6f41e3ab90a15c54dedca3|
|x86候选True|e72cc41a1cb4467b84afad5634a446d8|be7a73297ecd4cbbb08c293396a7ac9d|
|x64官方False|a0745cc2ddd340e88d6d0368c15d718b|09353e6bb7084db8a16d7fbbedb951f0|
|x64候选True|16b13a06ef8940dcadbff47a8ceffbd8|417da7e583c34ceb8e9f4fcb92f27993|

## 身份归属与交接

实际Runtime .NET10.0.13，官方FileVersion `10,0,1326,50504`，ProductVersion `10.0.13-servicing.26504.104`，commit `4099f83977f4d29604a9aeb971fb0a7bc1d5f699`；候选ProductVersion `1.0.0+b4de881638949d7fdbb653f9650fb21fd07bc685`。

|角色|现场SHA256（由业务JSON亲读，未重算）|
|---|---|
|官方x86|`5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`|
|候选x86|`DD10C795FE66535B8D8079781D3DE301BF4DCA81F1A1A9A9227515DCFDAF1EAB`|
|官方x64|`AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`|
|候选x64|`FDF0A6D91EF121ABD399F2B902738FB688629784EED9D0305307D5C9E7774FBE`|

x86 `Results/aot-identity.json`另亲核实际Native加载路径、架构X86/指针4与候选SHA。此包是D归档，不把它称为当前生产复现，更不把它绑定AG/AD或AJ新源码。归档身份只用于证据归属，不能变成推进修复/集成全量的前置阻塞。

AJ正在处理Bitmap固定布局，当前报告 `replacement-supervised-bitmap-layout-fix.md` 尚无新验收结果；下一由生产所有者对当前候选保存身份、复现并修复producer/consumer同类路径，以本独立夹具和兄弟回归验证双架构。AL未核定AJ最终根因/修复成功，未重审所有官方原生类继承源码或所有释放路径，不将报告交接冒充亲核；当前亲核仅限上述独立声明、最终TRX、八份JSON与x86加载identity。

AH圆角生产尚未验收，交接报告保留软件RTB官方36/候选40差异；AK正在修，不猜结果。真实恢复、S Capture WAIT_TIMEOUT/resource0、持续媒体、长期资源及最终full缺口不因夹具有效关闭。最后源码双项目×双架构Release全量尚未跑，不是无需跑；本轮集成稳定后应准备全量，不能无限只做局部Green。
